using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.GameRecord.DreamGame.Core.ApplicationServices;
using Tgs.GameRecord.Handle.Client;

namespace Tgs.GameRecord.DreamGame.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Normal_Process()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var fakeGameRecordInfoOptions = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "DGTF",
			GameProviderConfiguration = "DGTFConfiguration",
			HangfirePrefix = "gamerecord-dgtf-hangfire:",
			HangfireLocking = "gamerecord-dgtf-locking:",
			LockMinute = 10
		});
		var fakePolicySettingsOptions = Options.Create(new PolicySettingsOptions
		{
			RetryCount = 1,
			RetrySeconds = 1,
		});
		var fakeGameRecordHandleClient = Substitute.For<IGameRecordHandleClient>();

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

		var sut = new HandleRecordCommandService(
			faceLogger,
			fakeGameRecordInfoOptions,
			fakePolicySettingsOptions,
			fakeGameRecordHandleClient);

		await sut.ExecuteAsync(command: command);

		_ = fakeGameRecordHandleClient
			.Received(1)
			.HandleAsync(
			source: Arg.Is(new Handle.Common.ViewModels.GameRecordHandleViewModel
			{
				GameProviderCode = fakeGameRecordInfoOptions.Value.GameProviderCode,
				PlayerCode = command.GameRecord.PlayerCode,
				ExternalRoundId = command.GameRecord.ExternalRoundId,
				ExternalBetId = command.GameRecord.ExternalRoundId,
				ExternalGameId = command.GameRecord.ExternalGameId,
				ExternalCreatedOn = command.GameRecord.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = command.GameRecord.ExternalClosedOn.ToDateTimeOffset(),
				ExternalPayoffOn = command.GameRecord.ExternalClosedOn.ToDateTimeOffset(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch,
				BetState = Handle.Common.Protos.BetState.Loss,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 20.0m,
				ValidBet = 20.0m,
				Settle = 0.0M,
				Commission = 0.0m,
				Turnover = 20.00m,
				WinLoss = -20.00m,
				Jackpot = 0.0M,
				AdditionalData = command.GameRecord.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
