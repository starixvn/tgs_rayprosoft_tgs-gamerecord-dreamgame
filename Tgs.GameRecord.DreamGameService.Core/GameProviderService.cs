using Microsoft.Extensions.Caching.Memory;
using Tgs.GameProviders.Domains;
using Tgs.GameProviders.QueryServer.Client;

namespace Tgs.GameRecord.DreamGameService.Core;

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
                _ = _memoryCache.Set($"{GameProviderType.DreamGame}", gameProviderId);
            }
            return gameProviderId;
        }
        return gameProviderId;
    }
}
