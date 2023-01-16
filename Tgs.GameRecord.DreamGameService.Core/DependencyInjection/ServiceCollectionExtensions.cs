using System.Reflection;
using Tgs.GameCommand.Client;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRounds.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordCore(
        this IServiceCollection services,
        Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
        Action<DGTFGameRecordInfoOptions, IServiceProvider> dgtfGameRecordInfoOptions,
        Action<MongoSettingOptions, IServiceProvider> mongoSettingOptions,
        Action<DGTFMongoSettingOptions, IServiceProvider> dgtfMongoSettingOptions,
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameRoundsClientOptions, IServiceProvider> gameRoundsClientOptions
        )
        => services
            .AddOptions<GameRecordInfoOptions>()
            .Configure(gameRecordInfoOptions)
            .Services
            .AddOptions<DGTFGameRecordInfoOptions>()
            .Configure(dgtfGameRecordInfoOptions)
            .Services
            .AddOptions<MongoSettingOptions>()
            .Configure(mongoSettingOptions)
            .Services
            .AddOptions<DGTFMongoSettingOptions>()
            .Configure(dgtfMongoSettingOptions)
            .Services.RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddMemoryCache()
            .AddGameProvidersClient(gameProvidersClientOptions)
            .AddGameCommandClient(gameCommandClientOptions)
            .AddGameRoundsClient(gameRoundsClientOptions)
            .AddTransient<ISystemClock, SystemClock>()
            .AddTransient<IUtensil>(sp => ActivatorUtilities.CreateInstance<UtensilRepository>(sp, TimeSpan.FromHours(1)));
}
