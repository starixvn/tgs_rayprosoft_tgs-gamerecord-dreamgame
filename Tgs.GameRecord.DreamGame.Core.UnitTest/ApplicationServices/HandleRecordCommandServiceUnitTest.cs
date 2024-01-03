using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.GameRecord.DreamGame.Core.ApplicationServices;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;

namespace Tgs.GameRecord.DreamGame.Core.UnitTest.ApplicationServices;

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
                AdditionalData = "{\"id\": 18333201338, \"ip\": \"110.54.213.118\", \"ext\": \"240103B031130\", \"gameId\": 1, \"playId\": 37, \"result\": \"{\\\"result\\\":\\\"1,1,9\\\",\\\"poker\\\":{\\\"banker\\\":\\\"22-37-0\\\",\\\"player\\\":\\\"49-44-0\\\"}}\", \"roadid\": 0, \"shoeId\": 22, \"betTime\": \"2024-01-03 15:16:50\", \"calTime\": \"2024-01-03 15:17:12\", \"lobbyId\": 1, \"tableId\": 10103, \"gameType\": 1, \"memberId\": 96442319, \"parentId\": 23958, \"pluginid\": 0, \"userName\": \"4UIXLN1TGO25\", \"betDetail\": \"{\\\"player\\\":400.0}\", \"betPoints\": 400.0, \"winOrLoss\": 0.0, \"betPointsz\": 0.0, \"currencyId\": 7, \"deviceType\": 5, \"availableBet\": 400.0, \"isRevocation\": 1, \"balanceBefore\": 400.5}"
            });

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service.ExecuteAsync(command: command);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            playerCode: Arg.Is(command.GameRecord.PlayerCode),
            wagerState: Arg.Is(WagerStates.Lose),
            wagerType: Arg.Is(WagerTypes.Normal),
            freeGameState: Arg.Is(FreeGameStates.None),
            gambleState: Arg.Is(GambleStates.None),
            cashoutState: Arg.Is(CashoutStates.None),
            resettleState: Arg.Is(ResettleStates.None),
            externalRoundId: Arg.Is(command.GameRecord.ExternalRoundId),
            externalWagerId: Arg.Is(command.GameRecord.ExternalRoundId),
            externalGameId: Arg.Is(command.GameRecord.ExternalGameId),
            externalCreatedOn: Arg.Is(command.GameRecord.ExternalCreatedOn.ToDateTimeOffset()),
            externalClosedOn: Arg.Is(command.GameRecord.ExternalClosedOn.ToDateTimeOffset()),
            externalPayoffOn: Arg.Is(command.GameRecord.ExternalClosedOn.ToDateTimeOffset()),
            externalResettleOn: Arg.Is(DateTimeOffset.UnixEpoch),
            bet: Arg.Is(command.GameRecord.Bet.ToDecimal().GetValueOrDefault(0)),
            validBet: Arg.Is(command.GameRecord.ValidBet.ToDecimal().GetValueOrDefault(0)),
            settle: Arg.Is(command.GameRecord.Settle.ToDecimal().GetValueOrDefault(0)),
            commission: Arg.Is(0.0m),
            turnover: Arg.Is(command.GameRecord.Turnover.ToDecimal().GetValueOrDefault(0)),
            winLoss: Arg.Is(command.GameRecord.PlayerWinLoss.ToDecimal().GetValueOrDefault(0)),
            additionalData: Arg.Any<JsonNode>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
