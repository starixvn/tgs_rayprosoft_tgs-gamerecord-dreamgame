using System.Net.Mime;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.GameRecord.DreamGameService;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.MessageQueue.MongoDB.Configuration;
using MongoMessageQueue = MongoDB.Messaging.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
            fluentOptions.Host = nodeIp;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
            fluentOptions.UseMessagePack();
        });
    });

// Add services to the container.

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers(options =>
    {
        options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
        options.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddGameRecordCore((options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        configuration.GetSection("GameRecordInfo").Bind(options);
    }, (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        configuration.GetSection("DGTFGameRecordInfo").Bind(options);
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        configuration.GetSection("MongoSetting").Bind(options);
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        configuration.GetSection("DGTFMongoSetting").Bind(options);
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersServiceBaseUrl");
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameCommandClientBaseUrl");
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameRoundsServiceBaseUrl");
    });

var mongoSetting = builder.Configuration.GetSection("MongoSetting").Get<MongoSettingOptions>();
var natsSetting = builder.Configuration.GetSection("NatsSetting").Get<NatsSettingOptions>();

var dgtfMongoSetting = builder.Configuration.GetSection("DGTMongoSetting").Get<DGTFMongoSettingOptions>();
var dgtfNatsSetting = builder.Configuration.GetSection("DGTFNatsSetting").Get<DGTFNatsSettingOptions>();

builder.Services
    .AddMessageQueue()
    .AddMongoMessageQueue(config =>
    {
        _ = config
        .DefineMessageQueue(
            mongoSetting.ManagerName,
            sp => new MongoMessageQueueBuilderOptions
            {
                MessageQueue = MongoMessageQueue.Default,
                ConnectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("MongoMessaging")!
            });

        _ = config
        .DefineMessageQueue(
            dgtfMongoSetting.ManagerName,
            sp => new MongoMessageQueueBuilderOptions
            {
                MessageQueue = MongoMessageQueue.Default,
                ConnectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("MongoMessaging")!
            });
    })
    .AddNatsMessageQueue(natsConfig =>
    {
        _ = natsConfig
        .ConfigQueueOptions((options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            options.Url = configuration.GetConnectionString("Nats")!;
        })
        .AddHandler<ScheduleMessageHandler>(natsSetting.Subject, natsSetting.Group)
        .AddHandler<DGTFScheduleMessageHandler>(dgtfNatsSetting.Subject, dgtfNatsSetting.Group);
    })
    .ClearAllExchanges()
    .AddMongoGlobPatternExchange(mongoSetting.GlobPattern, mongoSetting.ManagerName, mongoSetting.QueueName)
    .AddMongoGlobPatternExchange(dgtfMongoSetting.GlobPattern, dgtfMongoSetting.ManagerName, dgtfMongoSetting.QueueName);

builder.Services
     .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
        sp => new ConfigureNamedOptions<OtlpExporterOptions>(
            Microsoft.Extensions.Options.Options.DefaultName,
            exporterOptions =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var nodeIp = config.GetValue<string>("NODE_IP");

                if (!string.IsNullOrWhiteSpace(nodeIp))
                    exporterOptions.Endpoint = new Uri($"http://{nodeIp}:4317");

                config.GetSection("Opentelemetry:Tracing").Bind(exporterOptions);
            }))
    .AddOpenTelemetryTracing(traceBuilder => traceBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddProfileViewExporter()
        .AddOtlpExporter())
    .AddProfileViewer()
    .Configure<AspNetCoreInstrumentationOptions>(options =>
    {
        options.Filter = httpContext =>
        {
            return !httpContext.Request.Path.HasValue
                || !httpContext.Request.Path.StartsWithSegments("/swagger")
                && !httpContext.Request.Path.StartsWithSegments("/healthz")
                && !httpContext.Request.Path.StartsWithSegments("/profiler/")
                && !httpContext.Request.Path.StartsWithSegments("/hangfire")
                && !httpContext.Request.Path.StartsWithSegments("/metrics")
                   && !new[] { "html", "ico", "png", "jpg" }.Any(
                       ext => httpContext.Request.Path.Value.EndsWith($".{ext}"));
        };
    });

builder.Services
    .AddHealthChecks();

var app = builder.Build();

_ = app.UseSwagger();
_ = app.UseSwaggerUI();

_ = app.UseAuthorization();

_ = app.MapControllers();

app.MapProfileViewer();

app.MapHealthChecks("/healthz");

_ = app.UseGameRecordRegister();

app.Run();
