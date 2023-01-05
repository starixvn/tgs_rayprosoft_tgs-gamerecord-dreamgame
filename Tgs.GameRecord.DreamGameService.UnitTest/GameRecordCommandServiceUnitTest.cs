using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameCommand.Common;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.GameRounds.Client;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.ViewModels;
using Tgs.MessageQueue;

namespace Tgs.GameRecord.DreamGameServiceUnitTest;

[TestClass]
public class GameRecordCommandServiceUnitTest
{
    [TestMethod]
    public async Task More_Than_3_Minutes_Settlement_Day_To_Be_Processed()
    {
        var fakeOptions = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "DG",
            GameExternalId = "dg",
        });
        var fakeMongoSettingOptions = Options.Create(new MongoSettingOptions
        {
            ManagerName = "gp.gamerecord.database",
            QueueName = "tgs.gp.gamerecord.database.round.dg",
            GlobPattern = "tgs.gp.gamerecord.database.round.dg"
        });
        var fakeLogger = NullLogger<GameRecordCommandService>.Instance;
        var fakeDreamGameClient = Substitute.For<IDreamGameClient>();
        var fakeGameRoundsClient = Substitute.For<IGameRoundsClient>();
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakeUtensil = Substitute.For<IUtensil>();

        var gameProviderId = Guid.NewGuid();
        _ = fakeUtensil
            .GetGameProviderIdAsync(
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
                Arg.Is(gameProviderId),
                Arg.Is(fakeOptions.Value.GameExternalId),
                Arg.Any<CancellationToken>())
            .Returns(gameId);

        var gameRecords = new GameRecordModle[] {
            new GameRecordModle()
            {
                GameProviderCode =fakeOptions.Value.GameProviderCode,
                GameProviderId = gameProviderId,
                PlayerCode ="test",
                Status= GameRecordRoundStatus.Closed,
                ClientStatus = GameRecordRoundClientStatus.Loss,
                ExternalRoundId ="10893708376",
                ExternalGameId =fakeOptions.Value.GameExternalId,
                ExternalCreatedOn=new DateTimeOffset(DateTime.Parse("2022-04-21 18:30:39"), TimeSpan.FromHours(+8)),
                ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2022-04-21 18:31:06"), TimeSpan.FromHours(+8)),
                BetAmount =20.00m,
                ValidBetAmount = 20.00m,
                PlayerWinLoss =-20.00m,
                Commission =0m,
                Turnover = 20.00m,
                SettleAmount = 0.00m,
                TieBetAdjustment =0m,
                AdditionalData =string.Empty}};

        var gameRecordResponse = new GameRecordResponse()
        {
            Success = true,
            Datas = gameRecords
        };
        _ = fakeDreamGameClient.GameRecordListAsync().Returns(gameRecordResponse);

        _ = fakeSystemClock.CurrentTime.Returns(new DateTimeOffset(DateTime.Parse("2022-04-21 18:35:00"), TimeSpan.FromHours(+8)));

        var gameRoundDataResponse = new GameRoundDataResponse()
        {
            Success = true,
            HttpStatusCode = System.Net.HttpStatusCode.OK,
            Data = new GameRoundViewModel(
                RoundId: Guid.NewGuid(),
                ExternalRoundId: "10893708376",
                GameProviderId: gameProviderId,
                GameProviderCode: "DG",
                PlayerCode: "test",
                GameId: 1,
                ExternalGameId: "dg",
                CurrencyCode: "CNY",
                Status: GameRoundStatus.Open,
                GameProviderConfigurationId: Guid.NewGuid(),
                WalletId: Guid.NewGuid(),
                CreatedOn: DateTimeOffset.UtcNow,
                ClosedOn: null,
                UpdatedOn: DateTimeOffset.UtcNow,
                BrandId: Guid.NewGuid(),
                ExternalCreatedOn: DateTimeOffset.UtcNow,
                ExternalClosedOn: null,
                BetAmount: 20m,
                ValidBetAmount: 20m,
                SettleAmount: null,
                Commission: null,
                Turnover: null,
                CompanyWinLoss: null,
                TieBetAdjustment: null,
                ClientStatus: GameRoundClientStatus.Open,
                ReopenTurnoverAdjustment: null,
                ReopenedOn: null,
                EffectiveBettingPercentage: null,
                AdditionalData: string.Empty)
        };

        var player = new PlayerInfo(
            ResellerId: Guid.NewGuid(),
            ResellerCode: "TGS",
            LicenseeId: Guid.NewGuid(),
            LicenseeCode: "TGS",
            BrandId: Guid.NewGuid(),
            BrandCode: "GPI",
            PlayerCode: "test",
            Username: "test",
            DefaultWalletId: Guid.NewGuid(),
            DefaultWalletCurrencyCode: "CNY");
        _ = fakeUtensil
            .GetPlayerAsync(
                playerCode: Arg.Is("test"),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(player);

        _ = fakeGameRoundsClient
            .GetGameRoundAsync(
                gameProviderId: Arg.Is(gameProviderId),
                externalRoundId: Arg.Is("10893708376"),
                gameId: Arg.Is(gameId),
                walletId: Arg.Is(player.DefaultWalletId))
            .Returns(gameRoundDataResponse);

        var service = new GameRecordCommandService(
            fakeOptions,
            fakeMongoSettingOptions,
            fakeLogger,
            fakeDreamGameClient,
            fakeGameRoundsClient,
            fakeMessageSender,
            fakeSystemClock,
            fakeUtensil);

        await service.ExecuteAsync(new GameRecordCommand()).ConfigureAwait(false);

        await fakeMessageSender.Received(1)
             .PublishAsync(
                subject: Arg.Is(fakeMongoSettingOptions.Value.GlobPattern),
                data: Arg.Any<ReadOnlyMemory<byte>>(),
                cancellationToken: Arg.Any<CancellationToken>())
             .ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Settlement_Date_Less_Than_3_Minutes_Will_Not_Be_Processed()
    {
        var fakeOptions = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "DG",
            GameExternalId = "dg",
        });
        var fakeMongoSettingOptions = Options.Create(new MongoSettingOptions
        {
            ManagerName = "gp.gamerecord.database",
            QueueName = "tgs.gp.gamerecord.database.round.dg",
            GlobPattern = "tgs.gp.gamerecord.database.round.dg"
        });
        var fakeLogger = NullLogger<GameRecordCommandService>.Instance;
        var fakeDreamGameClient = Substitute.For<IDreamGameClient>();
        var fakeGameRoundsClient = Substitute.For<IGameRoundsClient>();
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakeUtensil = Substitute.For<IUtensil>();

        var gameProviderId = Guid.NewGuid();
        _ = fakeUtensil
            .GetGameProviderIdAsync(
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
                Arg.Is(gameProviderId),
                Arg.Is(fakeOptions.Value.GameExternalId),
                Arg.Any<CancellationToken>())
            .Returns(gameId);

        var gameRecords = new GameRecordModle[] {
            new GameRecordModle()
            {
                GameProviderCode =fakeOptions.Value.GameProviderCode,
                GameProviderId = gameProviderId,
                PlayerCode ="test",
                Status= GameRecordRoundStatus.Closed,
                ClientStatus = GameRecordRoundClientStatus.Loss,
                ExternalRoundId ="10893708376",
                ExternalGameId =fakeOptions.Value.GameExternalId,
                ExternalCreatedOn=new DateTimeOffset(DateTime.Parse("2022-04-21 18:30:39"), TimeSpan.FromHours(+8)),
                ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2022-04-21 18:31:06"), TimeSpan.FromHours(+8)),
                BetAmount =20.00m,
                ValidBetAmount = 20.00m,
                PlayerWinLoss =-20.00m,
                Commission =0m,
                Turnover = 20.00m,
                SettleAmount = 0.00m,
                TieBetAdjustment =0m,
                AdditionalData =string.Empty}};

        var gameRecordResponse = new GameRecordResponse()
        {
            Success = true,
            Datas = gameRecords
        };
        _ = fakeDreamGameClient.GameRecordListAsync().Returns(gameRecordResponse);

        _ = fakeSystemClock.CurrentTime.Returns(new DateTimeOffset(DateTime.Parse("2022-04-21 18:33:00"), TimeSpan.FromHours(+8)));

        var gameRoundDataResponse = new GameRoundDataResponse()
        {
            Success = true,
            HttpStatusCode = System.Net.HttpStatusCode.OK,
            Data = new GameRoundViewModel(
                RoundId: Guid.NewGuid(),
                ExternalRoundId: "10893708376",
                GameProviderId: gameProviderId,
                GameProviderCode: "DG",
                PlayerCode: "test",
                GameId: 1,
                ExternalGameId: "dg",
                CurrencyCode: "CNY",
                Status: GameRoundStatus.Open,
                GameProviderConfigurationId: Guid.NewGuid(),
                WalletId: Guid.NewGuid(),
                CreatedOn: DateTimeOffset.UtcNow,
                ClosedOn: null,
                UpdatedOn: DateTimeOffset.UtcNow,
                BrandId: Guid.NewGuid(),
                ExternalCreatedOn: DateTimeOffset.UtcNow,
                ExternalClosedOn: null,
                BetAmount: 20m,
                ValidBetAmount: 20m,
                SettleAmount: null,
                Commission: null,
                Turnover: null,
                CompanyWinLoss: null,
                TieBetAdjustment: null,
                ClientStatus: GameRoundClientStatus.Open,
                ReopenTurnoverAdjustment: null,
                ReopenedOn: null,
                EffectiveBettingPercentage: null,
                AdditionalData: string.Empty)
        };

        var player = new PlayerInfo(
            ResellerId: Guid.NewGuid(),
            ResellerCode: "TGS",
            LicenseeId: Guid.NewGuid(),
            LicenseeCode: "TGS",
            BrandId: Guid.NewGuid(),
            BrandCode: "GPI",
            PlayerCode: "test",
            Username: "test",
            DefaultWalletId: Guid.NewGuid(),
            DefaultWalletCurrencyCode: "CNY");
        _ = fakeUtensil
            .GetPlayerAsync(
                playerCode: Arg.Is("test"),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(player);

        _ = fakeGameRoundsClient
            .GetGameRoundAsync(
                gameProviderId: Arg.Is(gameProviderId),
                externalRoundId: Arg.Is("10893708376"),
                gameId: Arg.Is(gameId),
                walletId: Arg.Is(player.DefaultWalletId))
            .Returns(gameRoundDataResponse);

        var service = new GameRecordCommandService(
            fakeOptions,
            fakeMongoSettingOptions,
            fakeLogger,
            fakeDreamGameClient,
            fakeGameRoundsClient,
            fakeMessageSender,
            fakeSystemClock,
            fakeUtensil);

        await service.ExecuteAsync(new GameRecordCommand()).ConfigureAwait(false);

        await fakeMessageSender.Received(0)
             .PublishAsync(
                subject: Arg.Is(fakeMongoSettingOptions.Value.GlobPattern),
                data: Arg.Any<ReadOnlyMemory<byte>>(),
                cancellationToken: Arg.Any<CancellationToken>())
             .ConfigureAwait(false);
    }
}
