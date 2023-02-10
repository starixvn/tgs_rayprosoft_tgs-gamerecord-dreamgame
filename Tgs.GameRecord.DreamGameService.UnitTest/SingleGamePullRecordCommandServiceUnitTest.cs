using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameCommand.Common;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.GameRounds.Client;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.ViewModels;
using Tgs.MessageQueue;

namespace Tgs.GameRecord.DreamGameServiceUnitTest;

[TestClass]
public class SingleGamePullRecordCommandServiceUnitTest
{
    [TestMethod]
    public async Task Normal_Process()
    {
        var fakeOptions = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "DG",
            GameProviderConfig = "DGConfiguration",
            GameProviderCodeTransfer = "DGTF",
            GameProviderConfigTransfer = "DGTFConfiguration",
            GameExternalId = "dg"
        });
        var fakeMongoSettingOptions = Options.Create(new MongoSettingOptions
        {
            ManagerName = "gp.gamerecord.database",
            QueueName = "tgs.gp.gamerecord.database.round.dg",
            GlobPattern = "tgs.gp.gamerecord.database.round.dg"
        });
        var fakeLogger = NullLogger<SingleGamePullRecordCommandService>.Instance;
        var fakeGameRoundsClient = Substitute.For<IGameRoundsClient>();
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeUtensil = Substitute.For<IUtensil>();

        var command = new SingleGamePullRecordCommand(
            Records: new GameHistorySettled[] {
            new GameHistorySettled()
            {
                PlayerCode=  "test",
                ExternalRoundId="10893708376",
                IsClosed= true,
                ExternalCreatedOn=new DateTimeOffset(DateTime.Parse("2022-04-21 18:30:39"), TimeSpan.FromHours(+8)).ToTimestamp(),
                ExternalClosedOn= new DateTimeOffset(DateTime.Parse("2022-04-21 18:31:06"), TimeSpan.FromHours(+8)).ToTimestamp(),
                Bet=20m,
                ValidBet=20m,
                PlayerWinLoss=  -20.00m,
                Turnover= 20.00m,
                Settle=0.0m,
                AdditionalData=string.Empty
            }});

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
                SettleAmount: 0m,
                Commission: 0m,
                Turnover: 0m,
                CompanyWinLoss: 0m,
                TieBetAdjustment: 0m,
                ClientStatus: GameRoundClientStatus.Open,
                ReopenTurnoverAdjustment: null,
                ReopenedOn: null,
                EffectiveBettingPercentage: null,
                AdditionalData: string.Empty)
        };

        _ = fakeGameRoundsClient
            .GetGameRoundAsync(
                gameProviderId: Arg.Is(gameProviderId),
                externalRoundId: Arg.Is("10893708376"),
                gameId: Arg.Is(gameId),
                walletId: Arg.Is(player.DefaultWalletId))
            .Returns(gameRoundDataResponse);

        var service = new SingleGamePullRecordCommandService(
            fakeOptions,
            fakeMongoSettingOptions,
            fakeLogger,
            fakeGameRoundsClient,
            fakeMessageSender,
            fakeUtensil);

        await service.ExecuteAsync(command).ConfigureAwait(false);

        await fakeMessageSender.Received(1)
             .PublishAsync(
                subject: Arg.Is(fakeMongoSettingOptions.Value.GlobPattern),
                data: Arg.Any<ReadOnlyMemory<byte>>(),
                cancellationToken: Arg.Any<CancellationToken>())
             .ConfigureAwait(false);
    }
}
