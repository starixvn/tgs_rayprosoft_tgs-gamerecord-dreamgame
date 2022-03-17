using System.Reflection;
using Tgs.GameProviders.GameRecord.DreamGameService;
using Tgs.GameProviders.GameRecord.DreamGameService.MessageHandlers;
using Tgs.GameProviders.QueryServer.Client;
using TGS.GameProviders.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollecitonExtensions
    {
        public static IServiceCollection AddGameRecord(
            this IServiceCollection services,
            Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
            Action<GameProvidersQueryServerClientOptions, IServiceProvider> gameProvidersQueryServerClientOptions
            )
            => services
                .RegisterAllServiceUtilityTypes(
                    Assembly.GetExecutingAssembly(),
                    ServiceLifetime.Transient)
                .AddGameProvidersClient(gameProvidersClientOptions)
                .AddGameProvidersQueryServerClient(gameProvidersQueryServerClientOptions)
                .AddTransient<IGameRecordMessageHandler, GameRecordMessageHandler>();
    }
}
