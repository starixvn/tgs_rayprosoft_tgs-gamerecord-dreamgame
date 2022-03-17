using Microsoft.Extensions.Caching.Memory;
using Tgs.GameProviders.QueryServer.Client;

namespace Tgs.GameProviders.GameRecord.DreamGameService
{
    public class GameService : IGameService
    {

        private readonly IGameProvidersQueryServerClient _gameProvidersQueryServerClient;
        private readonly IMemoryCache _memoryCache;

        public GameService(
            IGameProvidersQueryServerClient gameProvidersQueryServerClient,
           IMemoryCache memoryCache)
        {
            _gameProvidersQueryServerClient = gameProvidersQueryServerClient;
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }
        public async ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId)
        {
            if (!_memoryCache.TryGetValue($"{gameProviderId}:{gameExternalId}", out int? gameId))
            {
                var game = await _gameProvidersQueryServerClient.GameGetAsync(gameProviderId, gameExternalId).ConfigureAwait(false);

                if (game != null)
                {

                    _memoryCache.Set($"{gameProviderId}:{gameExternalId}", game.GameId, TimeSpan.FromHours(1));
                }
            }
            return gameId;
        }
    }
}
