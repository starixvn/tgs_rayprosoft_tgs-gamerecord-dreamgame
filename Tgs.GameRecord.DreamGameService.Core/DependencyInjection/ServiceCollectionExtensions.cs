using System.Reflection;
using Tgs.GameCommand.Client;
using Tgs.GameInterface.DreamGamingService.Client;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRounds.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordCore(
        this IServiceCollection services,
        Action<RedisOptions, IServiceProvider> redisDBconfigure,
        Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
        Action<MongoSettingOptions, IServiceProvider> mongoSettingOptions,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameRoundsClientOptions, IServiceProvider> gameRoundsClientOptions,
        Action<GameInterfaceDreamGamingClientOptions, IServiceProvider> gameInterfaceDreamGamingClientOptions)
        => services
            .AddOptions<RedisOptions>()
            .Configure(redisDBconfigure)
            .Services
            .AddOptions<GameRecordInfoOptions>()
            .Configure(gameRecordInfoOptions)
            .Services
            .AddOptions<MongoSettingOptions>()
            .Configure(mongoSettingOptions)
            .Services
            .RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddMemoryCache()
            .AddSingleton<RedisConnectionManager>()
            .AddGameCommandClient(gameCommandClientOptions)
            .AddGameRoundsClient(gameRoundsClientOptions)
            .AddGameInterfaceDreamGamingServiceClient(gameInterfaceDreamGamingClientOptions)
            .AddTransient<ISystemClock, SystemClock>()
            .AddTransient<IScheduleDispatch>(sp => ActivatorUtilities.CreateInstance<ScheduleDispatch>(sp, sp.GetRequiredService<RedisConnectionManager>().Database))
            .AddTransient<IUtensil>(sp => ActivatorUtilities.CreateInstance<UtensilRepository>(sp, TimeSpan.FromHours(1)))
            .AddTransient<ITransferUtensil>(sp => ActivatorUtilities.CreateInstance<TransferUtensilRepository>(sp, TimeSpan.FromHours(1)));
}
