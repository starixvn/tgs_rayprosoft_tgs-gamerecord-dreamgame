using Microsoft.Extensions.Caching.Memory;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.DreamGameService;

namespace Tgs.GameRecord.DreamGameService
{
    internal class GameProviderService : IGameProviderService
    {
        private readonly IGameProvidersQueryServerClient _gameProvidersQueryServerClient;
        private readonly IMemoryCache _memoryCache;

        public GameProviderService(
            IGameProvidersQueryServerClient gameProvidersQueryServerClient,
           IMemoryCache memoryCache)
        {
            _gameProvidersQueryServerClient = gameProvidersQueryServerClient;
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async ValueTask<Guid?> GameProviderGetGameProviderIdAsync()
        {
            if (!_memoryCache.TryGetValue($"{GameProviderType.DreamGame}", out Guid? gameProviderId))
            {
                gameProviderId = await _gameProvidersQueryServerClient.GameProviderGetGameProviderIdAsync(GameProviderType.DreamGame).ConfigureAwait(false);

                if (gameProviderId.HasValue)
                {
                    _ = _memoryCache.Set($"{GameProviderType.DreamGame}", gameProviderId, TimeSpan.FromHours(1));
                }
                return gameProviderId;
            }
            return gameProviderId;
        }
    }
}
