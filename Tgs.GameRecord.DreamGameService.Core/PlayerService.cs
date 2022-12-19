using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.DreamGameService.Core;

internal class PlayerService : IPlayerService
{
    private readonly IGameProvidersQueryServerClient _gameProvidersQueryServerClient;
    private readonly ILogger<PlayerService> _logger;
    private readonly IMemoryCache _memoryCache;

    public PlayerService(
        ILogger<PlayerService> logger,
        IMemoryCache memoryCache,
        IGameProvidersQueryServerClient gameProvidersQueryServerClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _gameProvidersQueryServerClient = gameProvidersQueryServerClient ?? throw new ArgumentNullException(nameof(gameProvidersQueryServerClient));
    }

    public async ValueTask<PlayerModel?> GetAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
    {
        var key = $"game:dg:player:get:{playerCode}";
        if (!_memoryCache.TryGetValue(key, out PlayerModel? player))
        {
            player = await _gameProvidersQueryServerClient
               .PlayerGetAsync(
                   playerCode: playerCode,
                   cancellationToken: cancellationToken)
               .ConfigureAwait(false);

            if (player is null)
            {
                _logger.LogError("player does not exist({playerCode})", playerCode);
                return null;
            }

            _ = _memoryCache.Set(key, player);

        }
        return player;
    }
}
