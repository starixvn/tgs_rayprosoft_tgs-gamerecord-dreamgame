using System.Reflection;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRounds.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordCore(
        this IServiceCollection services,
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameProvidersQueryServerClientOptions, IServiceProvider> gameProvidersQueryServerClientOptions,
        Action<GameRoundsClientOptions, IServiceProvider> gameRoundsClientOptions
        )
        => services
            .RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddGameProvidersClient(gameProvidersClientOptions)
            .AddGameProvidersQueryServerClient(gameProvidersQueryServerClientOptions)
            .AddGameRoundsClient(gameRoundsClientOptions)
            .AddTransient<IGameRecordMessageHandler, GameRecordMessageHandler>()
            .AddTransient<IGameProviderService, GameProviderService>()
            .AddTransient<IGameService, GameService>()
            .AddTransient<IPlayerService, PlayerService>()
            .AddTransient<ISystemClock, SystemClock>();
}
