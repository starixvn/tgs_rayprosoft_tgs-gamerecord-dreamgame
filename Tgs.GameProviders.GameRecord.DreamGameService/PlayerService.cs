using Microsoft.Extensions.Caching.Memory;
using Tgs.GameProviders.QueryServer.Client;

namespace Tgs.GameProviders.GameRecord.DreamGameService
{
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

        public async ValueTask<PlayerInfo?> GetAsync(Guid gameProviderId, string playerCode)
        {
            if (!_memoryCache.TryGetValue($"{gameProviderId}:{playerCode}", out PlayerInfo playerInfo))
            {
                var playerViewModel = await _gameProvidersQueryServerClient.PlayerGetAsync(playerCode).ConfigureAwait(false);
                var gameProviderConfigurationId = default(Guid?);
                if (playerViewModel != null)
                {
                    gameProviderConfigurationId = await _gameProvidersQueryServerClient.BrandGetGameProviderConfigurationIdAsync(playerViewModel.BrandId, gameProviderId).ConfigureAwait(false);
                }
                if (playerViewModel != null && gameProviderConfigurationId.HasValue)
                {
                    playerInfo = new PlayerInfo(
                        playerViewModel.BrandId,
                        playerViewModel.PlayerCode,
                        playerViewModel.DefaultWalletCurrencyCode,
                        playerViewModel.DefaultWalletId,
                        gameProviderConfigurationId.Value
                        );
                    _ = _memoryCache.Set($"{gameProviderId}:{playerCode}", playerInfo, TimeSpan.FromHours(1));
                }
            }
            return playerInfo;
        }
    }
}
