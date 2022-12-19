using Microsoft.Extensions.Caching.Memory;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.DreamGameService.Core;

internal class PlayerService : IPlayerService
{
    private readonly IGameProvidersQueryServerClient _gameProvidersQueryServerClient;
    private readonly IMemoryCache _memoryCache;

    public PlayerService(
        IMemoryCache memoryCache,
        IGameProvidersQueryServerClient gameProvidersQueryServerClient)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _gameProvidersQueryServerClient = gameProvidersQueryServerClient ?? throw new ArgumentNullException(nameof(gameProvidersQueryServerClient));
    }

    public async ValueTask<PlayerModel?> GetAsync(Guid gameProviderId, string playerCode)
    {
        if (!_memoryCache.TryGetValue($"{gameProviderId}:{playerCode}", out PlayerModel? playerInfo))
        {
            var playerViewModel = await _gameProvidersQueryServerClient.PlayerGetAsync(playerCode).ConfigureAwait(false);
            var gameProviderConfigurationId = default(Guid?);
            if (playerViewModel != null)
            {
                gameProviderConfigurationId = await _gameProvidersQueryServerClient.BrandGetGameProviderConfigurationIdAsync(playerViewModel.BrandId, gameProviderId).ConfigureAwait(false);
            }
            if (playerViewModel != null && gameProviderConfigurationId.HasValue)
            {
                _ = _memoryCache.Set($"{gameProviderId}:{playerCode}", playerInfo);
            }
        }
        return playerInfo;
    }
}
