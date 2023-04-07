using System.Text.Json;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

internal class ScheduleHandleRecordCommandService : ICommandService<ScheduleHandleRecordCommand>
{
    private readonly IUtensil _utensil;
    private readonly IGameRoundsWorkClient _workClient;

    public ScheduleHandleRecordCommandService(
        IUtensil utensil,
        IGameRoundsWorkClient workClient)
    {
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _workClient = workClient ?? throw new ArgumentNullException(nameof(workClient));
    }

    public async ValueTask ExecuteAsync(ScheduleHandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken)
            .ConfigureAwait(false);
        var gameProviderCode = _utensil.GetGameProviderCode();

        bool success = true;

        var player = await _utensil
            .GetPlayerAsync(
                playerCode: command.GameRecord.PlayerCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var gameId = await _utensil
            .GetGameIdAsync(
                gameProviderId: gameProviderId,
                gameExternalId: command.GameRecord.ExternalGameId,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (player == null || !gameId.HasValue)
        {
            success = false;
        }

        Guid? gameProviderConfigurationId = default;
        if (player != null)
        {
            gameProviderConfigurationId = await _utensil
                .GetBrandGameProviderConfigurationIdAsync(
                    brandId: player.BrandId,
                    gameProviderId: gameProviderId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (!gameProviderConfigurationId.HasValue)
            {
                success = false;
            }
        }

        if (success)
        {
            var status = command.GameRecord.IsClosed || command.GameRecord.IsVoid ? RoundStatus.Closed : RoundStatus.Open;
            var clientStatus = RoundClientStatus.Open;
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
                clientStatus = RoundClientStatus.Void;
            }
            else
            {
                if (command.GameRecord.IsClosed)
                {
                    externalClosedOn = command.GameRecord.ExternalClosedOn.ToDateTimeOffset();
                    clientStatus = command.GameRecord.PlayerWinLoss.ToDecimal().GetValueOrDefault(0) switch
                    {
                        > 0 => RoundClientStatus.Win,
                        < 0 => RoundClientStatus.Loss,
                        _ => RoundClientStatus.Draw
                    };
                }
            }

            await _workClient
            .RoundBidding(
                gameProviderId: gameProviderId,
                gameProviderCode: gameProviderCode,
                externalRoundId: command.GameRecord.ExternalRoundId,
                gameId: gameId!.Value,
                externalGameId: command.GameRecord.ExternalGameId,
                playerCode: player!.PlayerCode,
                walletId: player!.DefaultWalletId,
                currencyCode: player!.DefaultWalletCurrencyCode,
                brandId: player!.BrandId,
                gameProviderConfigurationId: gameProviderConfigurationId!.Value,
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
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
        else
        {
            await _workClient
            .RoundMissing(
                gameProviderId: gameProviderId,
                gameProviderCode: gameProviderCode,
                externalRoundId: command.GameRecord.ExternalRoundId,
                gameId: gameId ?? -1,
                externalGameId: command.GameRecord.ExternalGameId,
                playerCode: player?.PlayerCode ?? string.Empty,
                data: JsonSerializer.Serialize(command.GameRecord),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
    }
}
