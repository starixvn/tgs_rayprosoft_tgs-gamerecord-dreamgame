using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;

namespace Tgs.GameRecord.DreamGameService.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
    [Fact]
    public async Task Normal_Process()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "DGTF",
            LockMinute = 10
        });
        var fakeClient = Substitute.For<IGameRecordHandleClient>();

        var command = new HandleRecordCommand(
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

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service
            .ExecuteAsync(
            command: command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            externalRoundId: Arg.Is(command.GameRecord.ExternalRoundId),
            externalGameId: Arg.Is(command.GameRecord.ExternalGameId),
            playerCode: Arg.Is(command.GameRecord.PlayerCode),
            status: Arg.Is(GameRecordStatus.Closed),
            clientStatus: Arg.Is(GameRecordClientStatus.Loss),
            externalCreatedOn: Arg.Is(command.GameRecord.ExternalCreatedOn.ToDateTimeOffset()),
            externalClosedOn: Arg.Is(command.GameRecord.ExternalClosedOn.ToDateTimeOffset()),
            bet: Arg.Is(command.GameRecord.Bet.ToDecimal().GetValueOrDefault(0)),
            validBet: Arg.Is(command.GameRecord.ValidBet.ToDecimal().GetValueOrDefault(0)),
            settle: Arg.Is(command.GameRecord.Settle.ToDecimal().GetValueOrDefault(0)),
            commission: Arg.Is(0.0m),
            turnover: Arg.Is(command.GameRecord.Turnover.ToDecimal().GetValueOrDefault(0)),
            playerWinLoss: Arg.Is(command.GameRecord.PlayerWinLoss.ToDecimal().GetValueOrDefault(0)),
            additionalData: Arg.Is(command.GameRecord.AdditionalData),
            cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}
