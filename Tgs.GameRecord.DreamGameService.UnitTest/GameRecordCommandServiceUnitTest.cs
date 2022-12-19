using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.QueryServer.Models;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.GameRounds.Client;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.ViewModels;

namespace Tgs.GameRecord.DreamGameServiceUnitTest;

[TestClass]
public class GameRecordCommandServiceUnitTest
{
    [TestMethod]
    public async Task More_Than_3_Minutes_Settlement_Day_To_Be_Processed()
    {
        var fakeDreamGameClient = Substitute.For<IDreamGameClient>();
        var fakeGameProvider = Substitute.For<IGameProviderService>();
        var fakeGameRoundsClient = Substitute.For<IGameRoundsClient>();
        var fakeGameRecordMessageHandler = Substitute.For<IGameRecordMessageHandler>();
        var fakeGame = Substitute.For<IGameService>();
        var fakePlayer = Substitute.For<IPlayerService>();
        var fakeSystemClock = Substitute.For<ISystemClock>();

        var gameProviderId = Guid.NewGuid();
        _ = fakeGameProvider.GameProviderGetGameProviderIdAsync().Returns(gameProviderId);

        var game = new GameModel(
            GameId: 1,
            GameProviderId: gameProviderId,
            GameProviderCode: "DG",
            GameExternalId: "dg",
            Status: GameStatus.Active,
            Description: null,
            EndpointPath: null,
            RuleUrl: null,
            GameType: GameType.LiveDealer,
            PlatformType: GamePlatformType.DesktopAndMobile,
            SubGame: true,
            AvailableToBrand: false,
            HasTurnoverOnTieBet: false,
            Html5: true,
            AvailableCurrencies: null,
            GameCode: "DG",
            Name: "dg",
            Localization: null,
            SubGameProviderCode: "DG");

        _ = fakeGame.GetGameAsync(gameProviderId, "dg").Returns(game);

        var gameRecords = new GameRecordModle[] {
            new GameRecordModle()
            {
                GameProviderCode ="DG",
                GameProviderId = gameProviderId,
                PlayerCode ="test",
                Status= GameRecordRoundStatus.Closed,
                ClientStatus = GameRecordRoundClientStatus.Loss,
                ExternalRoundId ="10893708376",
                ExternalGameId ="dg",
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

        _ = fakeGameRoundsClient.GetGameRoundAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<Guid>()).Returns(gameRoundDataResponse);

        _ = fakePlayer.GetAsync(Arg.Any<Guid>(), Arg.Any<string>()).Returns(new PlayerModel(
            Guid.NewGuid(),
            "TGS",
            Guid.NewGuid(),
            "TGS",
            Guid.NewGuid(),
            "GPI",
            "test",
            "test",
            Guid.NewGuid(),
            "CNY",
            PlayerStatus.Enabled));

        var service = new GameRecordCommandService(
             fakeDreamGameClient,
             fakeGameProvider,
             fakeGameRecordMessageHandler,
             fakeGameRoundsClient,
             fakeGame,
             fakePlayer,
             fakeSystemClock);

        await service.ExecuteAsync(new GameRecordCommand()).ConfigureAwait(false);

        await fakeDreamGameClient.Received(1)
             .MarkGameRecordListAsync(Arg.Any<List<string>>())
             .ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Settlement_Date_Less_Than_3_Minutes_Will_Not_Be_Processed()
    {
        var fakeDreamGameClient = Substitute.For<IDreamGameClient>();
        var fakeGameProvider = Substitute.For<IGameProviderService>();
        var fakeGameRoundsClient = Substitute.For<IGameRoundsClient>();
        var fakeGameRecordMessageHandler = Substitute.For<IGameRecordMessageHandler>();
        var fakeGame = Substitute.For<IGameService>();
        var fakePlayer = Substitute.For<IPlayerService>();
        var fakeSystemClock = Substitute.For<ISystemClock>();

        var gameProviderId = Guid.NewGuid();
        _ = fakeGameProvider.GameProviderGetGameProviderIdAsync().Returns(gameProviderId);

        var game = new GameModel(
            GameId: 1,
            GameProviderId: gameProviderId,
            GameProviderCode: "DG",
            GameExternalId: "dg",
            Status: GameStatus.Active,
            Description: null,
            EndpointPath: null,
            RuleUrl: null,
            GameType: GameType.LiveDealer,
            PlatformType: GamePlatformType.DesktopAndMobile,
            SubGame: true,
            AvailableToBrand: false,
            HasTurnoverOnTieBet: false,
            Html5: true,
            AvailableCurrencies: null,
            GameCode: "DG",
            Name: "dg",
            Localization: null,
            SubGameProviderCode: "DG");

        _ = fakeGame.GetGameAsync(gameProviderId, "dg").Returns(game);

        var gameRecords = new GameRecordModle[] {
            new GameRecordModle()
            {
                GameProviderCode ="DG",
                GameProviderId = gameProviderId,
                PlayerCode ="test",
                Status= GameRecordRoundStatus.Closed,
                ClientStatus = GameRecordRoundClientStatus.Loss,
                ExternalRoundId ="10893708376",
                ExternalGameId ="dg",
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
        var service = new GameRecordCommandService(
             fakeDreamGameClient,
             fakeGameProvider,
             fakeGameRecordMessageHandler,
             fakeGameRoundsClient,
             fakeGame,
             fakePlayer,
             fakeSystemClock);

        await service.ExecuteAsync(new GameRecordCommand()).ConfigureAwait(false);

        await fakeDreamGameClient.Received(0)
             .MarkGameRecordListAsync(Array.Empty<string>())
             .ConfigureAwait(false);
    }
}
