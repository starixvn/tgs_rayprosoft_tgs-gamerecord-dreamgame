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
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameRoundsClientOptions, IServiceProvider> gameRoundsClientOptions
        )
        => services
            .AddOptions<GameRecordInfoOptions>()
            .Configure(gameRecordInfoOptions)
            .Services
            .RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddGameProvidersClient(gameProvidersClientOptions)
            .AddGameCommandClient(gameCommandClientOptions)
            .AddGameRoundsClient(gameRoundsClientOptions)
            .AddTransient<ISystemClock, SystemClock>()
            .AddTransient<IUtensil, UtensilRepository>()
            .Decorate<IUtensil>((innerRepository, sp) => ActivatorUtilities.CreateInstance<UtensilMemoryCacheRepository>(sp, innerRepository, TimeSpan.FromHours(1)));
}
