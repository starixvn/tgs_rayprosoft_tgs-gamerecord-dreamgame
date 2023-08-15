using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

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
        var status = command.GameRecord.IsClosed || command.GameRecord.IsVoid ? GameRecordStatus.Closed : GameRecordStatus.Open;
        var clientStatus = GameRecordClientStatus.Open;
        var externalCreatedOn = command.GameRecord.ExternalCreatedOn.ToDateTimeOffset();
        var externalClosedOn = (DateTimeOffset?)null;
        var bet = command.GameRecord.Bet.ToDecimal().GetValueOrDefault(0);
        var validBet = command.GameRecord.ValidBet.ToDecimal().GetValueOrDefault(0);
        var settle = command.GameRecord.Settle.ToDecimal().GetValueOrDefault(0);
        var commission = 0.0m;
        var turnover = command.GameRecord.Turnover.ToDecimal().GetValueOrDefault(0);
        var winLoss = command.GameRecord.PlayerWinLoss.ToDecimal().GetValueOrDefault(0);
        if (command.GameRecord.IsVoid)
        {
            clientStatus = GameRecordClientStatus.Void;
        }
        else
        {
            if (command.GameRecord.IsClosed)
            {
                externalClosedOn = command.GameRecord.ExternalClosedOn.ToDateTimeOffset();
                clientStatus = command.GameRecord.PlayerWinLoss.ToDecimal().GetValueOrDefault(0) switch
                {
                    > 0 => GameRecordClientStatus.Win,
                    < 0 => GameRecordClientStatus.Loss,
                    _ => GameRecordClientStatus.Draw
                };
            }
        }

        return _client
            .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            externalRoundId: command.GameRecord.ExternalRoundId,
            externalGameId: command.GameRecord.ExternalGameId,
            playerCode: command.GameRecord.PlayerCode,
            status: status,
            clientStatus: clientStatus,
            externalCreatedOn: externalCreatedOn,
            externalClosedOn: externalClosedOn,
            bet: bet,
            validBet: validBet,
            settle: settle,
            commission: commission,
            turnover: turnover,
            playerWinLoss: winLoss,
            additionalData: command.GameRecord.AdditionalData,
            cancellationToken: cancellationToken);
    }
}
