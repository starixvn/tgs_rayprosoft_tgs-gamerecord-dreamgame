using System.Text.Json.Serialization;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Tgs.GameProviders.GameRecord.DreamGameService.ReceiveHandlers;
using Tgs.MessageQueue.MongoDB.Configuration;
using MongoMessageQueue = MongoDB.Messaging.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((_, builder) =>
{
    builder.AddJsonFile("/config/appsettings.json", true);
});

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
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddMessageQueue()
    .AddRabbitMessageQueue(config =>
    {
        config
            .ConfigQueueOptions((options, sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                configuration.GetSection("RabbitMQ").Bind(options);

                options.OnSetupQueueAndExchange = channel =>
                {
                    channel.QueueDeclare(
                        queue: "gp.gamerecord.dg",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    channel.QueueBind(
                        "gp.gamerecord.dg",
                        "amq.direct",
                        $"tgs.gp.gamerecord.dg",
                        new Dictionary<string, object>());
                };
            })
            .AddHandler<DreamGameReceiveHandler>("gp.gamerecord.dg", false);
    })
    .AddNatsMessageQueue(natsConfig =>
    {
        natsConfig
        .ConfigQueueOptions((options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            options.Url = configuration.GetConnectionString("NatsMQ");
        });
    })
    .AddMongoMessageQueue(config =>
    {
        config
        .DefineMessageQueue(
            "gp.gamerecord.database",
            sp => new MongoMessageQueueBuilderOptions
            {
                MessageQueue = MongoMessageQueue.Default,
                ConnectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("MongoDBMessagingConnectionString")
            })
        .DeclareQueue("tgs.gp.gamerecord.database.round.dg");
    })
    .ClearAllExchanges()
    .AddMongoGlobPatternExchange("tgs.gp.gamerecord.database.round.dg", "gp.gamerecord.database", "tgs.gp.gamerecord.database.round.dg")
    .AddNatsGlobPatternExchange("*");

builder.Services
    .AddGameRecord((options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersBaseUrl");
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersQueryServerBaseUrl");
    });

builder.Services
    .AddOptions<OtlpExporterOptions>()
    .Configure<IConfiguration>((options, conifg) =>
    {
        var nodeIp = conifg.GetValue<string>("NODE_IP");

        if (!string.IsNullOrWhiteSpace(nodeIp))
            options.Endpoint = new Uri($"http://{nodeIp}:4317");

        conifg.GetSection("Opentelemetry:Tracing").Bind(options);
    });

builder.Services.AddOpenTelemetryTracing(builder =>
{
    builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddNpgsql()
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddProfileViewExporter()
        .AddOtlpExporter();
});

builder.Services.Configure<AspNetCoreInstrumentationOptions>(options =>
{
    options.Filter = httpContext =>
    {
        return !httpContext.Request.Path.HasValue
            || !httpContext.Request.Path.StartsWithSegments("/swagger")
            && !httpContext.Request.Path.StartsWithSegments("/healthz")
            && !httpContext.Request.Path.StartsWithSegments("/profiler/")
            && !new[] { "html", "ico", "png", "jpg", "js" }.Any(
                ext => httpContext.Request.Path.Value.EndsWith($".{ext}"));
    };
});

builder.Services.AddProfileViewer();

builder.Services
    .AddInterceptionService()
    .AddServiceUtiltiyInterceptors();

builder.Services
    .AddHealthChecks()
    .ForwardToPrometheus();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.MapMetrics();

app.MapProfileViewer();

app.Run();