using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.GameRecord.DreamGameService.Core;

namespace Tgs.GameRecord.DreamGameService.UnitTest;

public class UtensilRepositoryUnitTest
{
    [Fact]
    public async Task Get_Brand_GameProvider_Configuration_Id()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "DGTF",
            LockMinute = 10
        });
        var fakeClient = Substitute.For<IGameCommandClient>();
        var fakeMemoryCache = Substitute.For<IMemoryCache>();
        var absoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

        var brandId = Guid.NewGuid();
        var gameProviderId = Guid.NewGuid();

        var key = $"gamerecord:{gameRecordInfo.Value.GameProviderCode}:{brandId}:{gameProviderId}";

        _ = fakeMemoryCache
            .TryGetValue(
                key: Arg.Is(key),
                value: out _)
             .Returns(false);

        var gameProviderConfigurationId = Guid.NewGuid();
        _ = fakeClient
            .BrandGetGameProviderConfigurationIdAsync(
                brandId: Arg.Is(brandId),
                gameProviderId: Arg.Is(gameProviderId),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderConfigurationId);

        var repository = new UtensilRepository(
            gameRecordInfo,
            fakeClient,
            fakeMemoryCache,
            absoluteExpirationRelativeToNow);

        var actual = await repository
            .GetBrandGameProviderConfigurationIdAsync(
                brandId,
                gameProviderId)
            .ConfigureAwait(false);

        Assert.NotNull(actual);
        Assert.True(actual.HasValue);
        Assert.Equal(gameProviderConfigurationId, actual.Value);

        _ = fakeMemoryCache
            .Received(1)
            .Set(
                key: key,
                gameProviderConfigurationId);
    }

    [Fact]
    public async Task Get_Game_Id()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "DGTF",
            LockMinute = 10
        });
        var fakeClient = Substitute.For<IGameCommandClient>();
        var fakeMemoryCache = Substitute.For<IMemoryCache>();
        var absoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

        var gameProviderId = Guid.NewGuid();
        var gameExternalId = "1";

        var key = $"gamerecord:{gameRecordInfo.Value.GameProviderCode}:{gameProviderId}:{gameExternalId}";

        _ = fakeMemoryCache
            .TryGetValue(
                key: Arg.Is(key),
                value: out _)
             .Returns(false);

        var gameId = 1;
        _ = fakeClient
            .GameGetIdAsync(
                gameProviderId: Arg.Is(gameProviderId),
                gameExternalId: Arg.Is(gameExternalId),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameId);

        var repository = new UtensilRepository(
            gameRecordInfo,
            fakeClient,
            fakeMemoryCache,
            absoluteExpirationRelativeToNow);

        var actual = await repository
            .GetGameIdAsync(
                gameProviderId,
                gameExternalId)
            .ConfigureAwait(false);

        Assert.NotNull(actual);
        Assert.Equal(gameId, actual);

        _ = fakeMemoryCache
            .Received(1)
            .Set(
                key: key,
                gameId);
    }

    [Fact]
    public async Task Get_GameProvider_Id()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "DGTF",
            LockMinute = 10
        });
        var fakeClient = Substitute.For<IGameCommandClient>();
        var fakeMemoryCache = Substitute.For<IMemoryCache>();
        var absoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

        var key = $"gamerecord:{gameRecordInfo.Value.GameProviderCode}";

        _ = fakeMemoryCache
            .TryGetValue(
                key: Arg.Is(key),
                value: out _)
             .Returns(false);

        var gameProviderId = Guid.NewGuid();
        _ = fakeClient
            .GameProviderGetGameProviderIdAsync(
                gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var repository = new UtensilRepository(
            gameRecordInfo,
            fakeClient,
            fakeMemoryCache,
            absoluteExpirationRelativeToNow);

        var actual = await repository
            .GetGameProviderIdAsync()
            .ConfigureAwait(false);

        Assert.Equal(gameProviderId, actual);

        _ = fakeMemoryCache
            .Received(1)
            .Set(
                key: key,
                gameProviderId);
    }

    [Fact]
    public async Task Get_GameProvider_Id_Exception()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "DGTF",
            LockMinute = 10
        });

        var fakeClient = Substitute.For<IGameCommandClient>();
        var fakeMemoryCache = Substitute.For<IMemoryCache>();
        var absoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

        var key = $"{gameRecordInfo.Value.GameProviderCode}";

        _ = fakeMemoryCache
            .TryGetValue(
                key: Arg.Is(key),
                value: out _)
             .Returns(false);

        _ = fakeClient
            .GameProviderGetGameProviderIdAsync(
                gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<Guid?>(null));

        var repository = new UtensilRepository(
            gameRecordInfo,
            fakeClient,
            fakeMemoryCache,
            absoluteExpirationRelativeToNow);

        _ = await Assert.ThrowsAsync<Exception>(() => repository
             .GetGameProviderIdAsync()
             .AsTask())
             .ConfigureAwait(false);
    }

    [Fact]
    public async Task Get_Player()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "DGTF",
            LockMinute = 10
        });

        var fakeClient = Substitute.For<IGameCommandClient>();
        var fakeMemoryCache = Substitute.For<IMemoryCache>();
        var absoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

        var playerCode = "test";

        var key = $"gamerecord:{gameRecordInfo.Value.GameProviderCode}:{playerCode}";

        _ = fakeMemoryCache
            .TryGetValue(
                key: Arg.Is(key),
                value: out _)
             .Returns(false);

        var player = new PlayerInfo(
            ResellerId: Guid.NewGuid(),
            ResellerCode: "TGS",
            LicenseeId: Guid.NewGuid(),
            LicenseeCode: "TGS",
            BrandId: Guid.NewGuid(),
            BrandCode: "GPI",
            PlayerCode: playerCode,
            Username: playerCode,
            DefaultWalletId: Guid.NewGuid(),
            DefaultWalletCurrencyCode: "CNY");
        _ = fakeClient
            .PlayerGetAsync(
                playerCode: Arg.Is(playerCode),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(player);

        var repository = new UtensilRepository(
            gameRecordInfo,
            fakeClient,
            fakeMemoryCache,
            absoluteExpirationRelativeToNow);

        var actual = await repository
            .GetPlayerAsync(
                playerCode: playerCode)
            .ConfigureAwait(false);

        Assert.NotNull(actual);
        Assert.Equal(playerCode, player.PlayerCode);

        _ = fakeMemoryCache
            .Received(1)
            .Set(
                key: key,
                player);
    }
}
