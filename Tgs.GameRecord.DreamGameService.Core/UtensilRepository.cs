using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;

namespace Tgs.GameRecord.DreamGameService.Core;

internal class UtensilRepository : IUtensil
{
    private readonly TimeSpan _absoluteExpirationRelativeToNow;
    private readonly IGameCommandClient _client;
    private readonly DGTFGameRecordInfoOptions _dgtfGameRecordInfo;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IMemoryCache _memoryCache;

    public UtensilRepository(
        IOptions<GameRecordInfoOptions> options,
        IOptions<DGTFGameRecordInfoOptions> dgtfOptions,
        IGameCommandClient client,
        IMemoryCache memoryCache,
        TimeSpan absoluteExpirationRelativeToNow)
    {
        _gameRecordInfo = options.Value;
        _dgtfGameRecordInfo = dgtfOptions.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _absoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
    }

    public async ValueTask<Guid?> GetBrandGameProviderConfigurationIdAsync(
        Guid brandId,
        Guid gameProviderId,
        CancellationToken cancellationToken = default)
    {
        var key = $"gamerecord:{_gameRecordInfo.GameProviderCode}:{brandId}:{gameProviderId}";

        if (!_memoryCache.TryGetValue(key, out Guid? gameProviderConfigurationId))
        {
            gameProviderConfigurationId = await _client
                .BrandGetGameProviderConfigurationIdAsync(
                    brandId: brandId,
                    gameProviderId: gameProviderId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (gameProviderConfigurationId.HasValue)
            {
                return _memoryCache
                    .Set(
                        key: key,
                        value: gameProviderConfigurationId,
                        absoluteExpirationRelativeToNow: _absoluteExpirationRelativeToNow);
            }
        }
        return gameProviderConfigurationId;
    }

    public async ValueTask<Guid?> GetDGTFBrandGameProviderConfigurationIdAsync(
        Guid brandId,
        Guid gameProviderId,
        CancellationToken cancellationToken = default)
    {
        var key = $"gamerecord:{_dgtfGameRecordInfo.GameProviderCode}:{brandId}:{gameProviderId}";

        if (!_memoryCache.TryGetValue(key, out Guid? gameProviderConfigurationId))
        {
            gameProviderConfigurationId = await _client
                .BrandGetGameProviderConfigurationIdAsync(
                    brandId: brandId,
                    gameProviderId: gameProviderId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (gameProviderConfigurationId.HasValue)
            {
                return _memoryCache
                    .Set(
                        key: key,
                        value: gameProviderConfigurationId,
                        absoluteExpirationRelativeToNow: _absoluteExpirationRelativeToNow);
            }
        }
        return gameProviderConfigurationId;
    }

    public async ValueTask<int> GetDGTFGameIdAsync(
        Guid gameProviderId,
        string gameExternalId,
        CancellationToken cancellationToken = default)
    {
        var key = $"gamerecord:{_dgtfGameRecordInfo.GameProviderCode}:{gameProviderId}:{gameExternalId}";
        if (!_memoryCache.TryGetValue(key, out int? gameId))
        {
            gameId = await _client
                .GameGetIdAsync(
                    gameProviderId: gameProviderId,
                    gameExternalId: gameExternalId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"This game does not exist.({gameExternalId})");

            if (gameId.HasValue)
            {
                return _memoryCache
                    .Set(
                        key: key,
                        value: gameId.Value);
            }
        }
        return gameId ?? throw new Exception($"This game does not exist.({gameExternalId})");
    }

    public async ValueTask<Guid> GetDGTFGameProviderIdAsync(
        CancellationToken cancellationToken = default)
    {
        var key = $"gamerecord:{_dgtfGameRecordInfo.GameProviderCode}";
        if (!_memoryCache.TryGetValue(key, out Guid gameProviderId))
        {
            gameProviderId = await _client
                .GameProviderGetGameProviderIdAsync(
                    gameProviderCode: _dgtfGameRecordInfo.GameProviderCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"The identifier for this game provider does not exist.({_dgtfGameRecordInfo.GameProviderCode})");

            return _memoryCache.Set(key, gameProviderId);
        }
        return gameProviderId;
    }

    public async ValueTask<PlayerInfo?> GetDGTFPlayerAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
    {
        var key = $"gamerecord:{_dgtfGameRecordInfo.GameProviderCode}:{playerCode}";
        if (!_memoryCache.TryGetValue(key, out PlayerInfo? playerModel))
        {
            playerModel = await _client
                .PlayerGetAsync(
                    playerCode: playerCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (playerModel is not null)
            {
                return _memoryCache
                    .Set(
                        key: key,
                        value: playerModel,
                        absoluteExpirationRelativeToNow: _absoluteExpirationRelativeToNow);
            }
        }
        return playerModel;
    }

    public async ValueTask<int> GetGameIdAsync(
        Guid gameProviderId,
        string gameExternalId,
        CancellationToken cancellationToken = default)
    {
        var key = $"gamerecord:{_gameRecordInfo.GameProviderCode}:{gameProviderId}:{gameExternalId}";
        if (!_memoryCache.TryGetValue(key, out int? gameId))
        {
            gameId = await _client
                .GameGetIdAsync(
                    gameProviderId: gameProviderId,
                    gameExternalId: gameExternalId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"This game does not exist.({gameExternalId})");

            if (gameId.HasValue)
            {
                return _memoryCache
                    .Set(
                        key: key,
                        value: gameId.Value);
            }
        }
        return gameId ?? throw new Exception($"This game does not exist.({gameExternalId})");
    }

    public async ValueTask<Guid> GetGameProviderIdAsync(
        CancellationToken cancellationToken = default)
    {
        var key = $"gamerecord:{_gameRecordInfo.GameProviderCode}";
        if (!_memoryCache.TryGetValue(key, out Guid gameProviderId))
        {
            gameProviderId = await _client
                .GameProviderGetGameProviderIdAsync(
                    gameProviderCode: _gameRecordInfo.GameProviderCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"The identifier for this game provider does not exist.({_gameRecordInfo.GameProviderCode})");

            return _memoryCache.Set(key, gameProviderId);
        }
        return gameProviderId;
    }

    public async ValueTask<PlayerInfo?> GetPlayerAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
    {
        var key = $"gamerecord:{_gameRecordInfo.GameProviderCode}:{playerCode}";
        if (!_memoryCache.TryGetValue(key, out PlayerInfo? playerModel))
        {
            playerModel = await _client
                .PlayerGetAsync(
                    playerCode: playerCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (playerModel is not null)
            {
                return _memoryCache
                    .Set(
                        key: key,
                        value: playerModel,
                        absoluteExpirationRelativeToNow: _absoluteExpirationRelativeToNow);
            }
        }
        return playerModel;
    }
}
