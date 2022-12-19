using Microsoft.Extensions.Caching.Memory;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.DreamGameService.Core;

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

    public async ValueTask<GameModel?> GetGameAsync(Guid gameProviderId, string gameExternalId)
    {
        if (!_memoryCache.TryGetValue($"{gameProviderId}:{gameExternalId}", out GameModel? game))
        {
            game = await _gameProvidersQueryServerClient.GameGetAsync(gameProviderId, gameExternalId).ConfigureAwait(false);

            if (game != null)
            {
                _ = _memoryCache.Set($"{gameProviderId}:{gameExternalId}", game);
            }
        }
        return game;
    }
}
