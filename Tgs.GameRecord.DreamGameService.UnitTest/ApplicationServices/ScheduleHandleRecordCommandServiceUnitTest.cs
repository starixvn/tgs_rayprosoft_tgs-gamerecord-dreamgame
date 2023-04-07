using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using NSubstitute;
using Tgs.GameCommand.Common;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;

namespace Tgs.GameRecord.DreamGameService.UnitTest.ApplicationServices;

public class ScheduleHandleRecordCommandServiceUnitTest
{
    [Fact]
    public async Task Normal_Process()
    {
        var fakeUtensil = Substitute.For<IUtensil>();
        var fakeGameRoundsWorkClient = Substitute.For<IGameRoundsWorkClient>();

        var command = new ScheduleHandleRecordCommand(
            GameRecord: new GameHistorySettled
            {
                PlayerCode = "test",
                ExternalRoundId = "10893708376",
                ExternalGameId = "bacc",
                IsVoid = false,
                IsClosed = true,
                ExternalCreatedOn = new DateTimeOffset(DateTime.Parse("2022-04-21 18:30:39"), TimeSpan.FromHours(+8)).ToTimestamp(),
                ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2022-04-21 18:31:06"), TimeSpan.FromHours(+8)).ToTimestamp(),
                Bet = 20m,
                ValidBet = 20m,
                Settle = 0.0m,
                Turnover = 20.00m,
                PlayerWinLoss = -20.00m,
                AdditionalData = string.Empty
            });

        var gameProviderId = Guid.NewGuid();
        _ = fakeUtensil
            .GetGameProviderIdAsync(
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var gameProviderCode = "DGTF";
        _ = fakeUtensil
            .GetGameProviderCode()
            .Returns(gameProviderCode);

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

        var gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
                Arg.Is(gameProviderId),
                Arg.Is(command.GameRecord.ExternalGameId),
                Arg.Any<CancellationToken>())
            .Returns(gameId);

        var gameProviderConfigurationId = Guid.NewGuid();
        _ = fakeUtensil
            .GetBrandGameProviderConfigurationIdAsync(
                Arg.Is(player.BrandId),
                Arg.Is(gameProviderId),
                Arg.Any<CancellationToken>())
            .Returns(gameProviderConfigurationId);

        var service = new ScheduleHandleRecordCommandService(
            fakeUtensil,
            fakeGameRoundsWorkClient);

        await service
            .ExecuteAsync(
                command: command)
            .ConfigureAwait(false);

        var bet = command.GameRecord.Bet.ToDecimal().GetValueOrDefault(0);
        var validBet = command.GameRecord.ValidBet.ToDecimal().GetValueOrDefault(0);
        var settle = command.GameRecord.Settle.ToDecimal().GetValueOrDefault(0);
        var commission = 0.0m;
        var turnover = command.GameRecord.Turnover.ToDecimal().GetValueOrDefault(0);
        var winLoss = command.GameRecord.PlayerWinLoss.ToDecimal().GetValueOrDefault(0);

        await fakeGameRoundsWorkClient
            .Received(1)
            .RoundBidding(
                gameProviderId: Arg.Is(gameProviderId),
                gameProviderCode: Arg.Is(gameProviderCode),
                externalRoundId: Arg.Is(command.GameRecord.ExternalRoundId),
                gameId: Arg.Is(gameId),
                externalGameId: Arg.Is(command.GameRecord.ExternalGameId),
                playerCode: Arg.Is(player!.PlayerCode),
                walletId: Arg.Is(player!.DefaultWalletId),
                currencyCode: Arg.Is(player!.DefaultWalletCurrencyCode),
                brandId: Arg.Is(player!.BrandId),
                gameProviderConfigurationId: Arg.Is(gameProviderConfigurationId),
                status: Arg.Is(RoundStatus.Closed),
                clientStatus: Arg.Is(RoundClientStatus.Loss),
                externalCreatedOn: Arg.Is(command.GameRecord.ExternalCreatedOn.ToDateTimeOffset()),
                externalClosedOn: Arg.Is(command.GameRecord.ExternalClosedOn.ToDateTimeOffset()),
                bet: Arg.Is(bet),
                validBet: Arg.Is(validBet),
                settle: Arg.Is(settle),
                commission: Arg.Is(commission),
                turnover: Arg.Is(turnover),
                playerWinLoss: Arg.Is(winLoss),
                additionalData: Arg.Is(command.GameRecord.AdditionalData),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}
