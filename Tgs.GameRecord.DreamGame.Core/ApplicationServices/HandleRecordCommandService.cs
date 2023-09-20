using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGame.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly IGameRecordHandleClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public HandleRecordCommandService(
        IOptions<GameRecordInfoOptions> options,
        IGameRecordHandleClient client)
    {
        _gameRecordInfo = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var wagerState = WagerStates.Running;
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
            wagerState = command.GameRecord.IsVoid
                ? WagerStates.Void
                : command.GameRecord.PlayerWinLoss.ToDecimal().GetValueOrDefault(0) switch
                {
                    > 0 => WagerStates.Win,
                    < 0 => WagerStates.Lose,
                    _ => WagerStates.Draw
                };
            externalClosedOn = command.GameRecord.ExternalClosedOn.ToDateTimeOffset();
            settle = command.GameRecord.Settle.ToDecimal().GetValueOrDefault(0);
            turnover = command.GameRecord.Turnover.ToDecimal().GetValueOrDefault(0);
            winLoss = command.GameRecord.PlayerWinLoss.ToDecimal().GetValueOrDefault(0);
        }

        return _client
            .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            playerCode: command.GameRecord.PlayerCode,
            wagerState: wagerState,
            wagerType: WagerTypes.Normal,
            freeGameState: FreeGameStates.None,
            gambleState: GambleStates.None,
            cashoutState: CashoutStates.None,
            resettleState: ResettleStates.None,
            externalRoundId: command.GameRecord.ExternalRoundId,
            externalWagerId: command.GameRecord.ExternalRoundId,
            externalGameId: command.GameRecord.ExternalGameId,
            externalCreatedOn: externalCreatedOn,
            externalClosedOn: externalClosedOn,
            externalPayoffOn: externalClosedOn,
            externalResettleOn: DateTimeOffset.UnixEpoch,
            bet: bet,
            validBet: validBet,
            settle: settle,
            commission: commission,
            turnover: turnover,
            winLoss: winLoss,
            additionalData: command.GameRecord.AdditionalData,
            cancellationToken: cancellationToken);
    }
}
