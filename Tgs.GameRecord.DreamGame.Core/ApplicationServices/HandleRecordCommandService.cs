using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.DreamGame.Core.ApplicationServices;

internal class HandleRecordCommandService(
	ILogger<HandleRecordCommandService> logger,
	IOptions<GameRecordInfoOptions> options,
	IMessageSender messageSender) : ICommandService<HandleRecordCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

	public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
	{
		var betState = Handle.Common.Protos.BetState.Running;
		var externalCreatedOn = command.GameRecord.ExternalCreatedOn.ToDateTimeOffset();
		var externalClosedOn = DateTimeOffset.UnixEpoch;
		var bet = command.GameRecord.Bet.ToDecimal().GetValueOrDefault(0);
		var validBet = command.GameRecord.ValidBet.ToDecimal().GetValueOrDefault(0);
		var settle = 0.0m;
		var commission = 0.0m;
		var turnover = 0.0m;
		var winLoss = 0.0m;
		if (command.GameRecord.IsVoid || command.GameRecord.IsClosed)
		{
			betState = command.GameRecord.IsVoid
				? Handle.Common.Protos.BetState.Void
				: command.GameRecord.PlayerWinLoss.ToDecimal().GetValueOrDefault(0) switch
				{
					> 0 => Handle.Common.Protos.BetState.Win,
					< 0 => Handle.Common.Protos.BetState.Loss,
					_ => Handle.Common.Protos.BetState.Draw
				};
			externalClosedOn = command.GameRecord.ExternalClosedOn.ToDateTimeOffset();
			settle = command.GameRecord.Settle.ToDecimal().GetValueOrDefault(0);
			turnover = command.GameRecord.Turnover.ToDecimal().GetValueOrDefault(0);
			winLoss = command.GameRecord.PlayerWinLoss.ToDecimal().GetValueOrDefault(0);
		}

		var request = new Handle.Common.Protos.GameRecordHandleRequest
		{
			GameProviderCode = _gameRecordInfo.GameProviderCode,
			PlayerCode = command.GameRecord.PlayerCode,
			ExternalRoundId = command.GameRecord.ExternalRoundId,
			ExternalBetId = command.GameRecord.ExternalRoundId,
			ExternalGameId = command.GameRecord.ExternalGameId,
			ExternalCreatedOn = externalCreatedOn.ToTimestamp(),
			ExternalClosedOn = externalClosedOn.ToTimestamp(),
			ExternalPayoffOn = externalClosedOn.ToTimestamp(),
			ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
			BetState = betState,
			BetType = Handle.Common.Protos.BetType.Normal,
			FreeGameState = Handle.Common.Protos.FreeGameState.None,
			GambleState = Handle.Common.Protos.GambleState.None,
			CashoutState = Handle.Common.Protos.CashoutState.None,
			ResettleState = Handle.Common.Protos.ResettleState.None,
			JackpotState = Handle.Common.Protos.JackpotState.None,
			Bet = bet,
			ValidBet = validBet,
			Settle = settle,
			Commission = commission,
			Turnover = turnover,
			WinLoss = winLoss,
			Jackpot = 0.0M,
			AdditionalData = command.GameRecord.AdditionalData
		};

		await messageSender.PublishAsync(
			subject: Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest,
			data: request,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This publication requests information.({request})",
			DateTime.Now.ToString("o"),
			Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest,
			new
			{
				request.GameProviderCode,
				request.PlayerCode,
				request.ExternalRoundId,
				request.ExternalBetId,
				request.ExternalGameId,
				ExternalCreatedOn = request.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = request.ExternalClosedOn.ToDateTimeOffset(),
				request.BetState
			});
	}
}
