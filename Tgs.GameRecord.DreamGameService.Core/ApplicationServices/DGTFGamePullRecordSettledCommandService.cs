using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.MessageModels;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

internal class DGTFGamePullRecordSettledCommandService : ICommandService<DGTFGamePullRecordSettledCommand>
{
    private readonly DGTFGameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<DGTFGamePullRecordSettledCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly DGTFMongoSettingOptions _mongoSetting;
    private readonly IUtensil _utensil;

    public DGTFGamePullRecordSettledCommandService(
        ILogger<DGTFGamePullRecordSettledCommandService> logger,
        IOptions<DGTFGameRecordInfoOptions> gameRecordInfoOptions,
        IOptions<DGTFMongoSettingOptions> mongoSettingOptions,
        IMessageSender messageSender,
        IUtensil utensil)
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _mongoSetting = mongoSettingOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask ExecuteAsync(DGTFGamePullRecordSettledCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetDGTFGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        foreach (var record in command.Records)
        {
            bool success = true;
            var playerModel = await _utensil
                .GetDGTFPlayerAsync(
                    playerCode: record.PlayerCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (playerModel == null)
            {
                success = false;
            }

            Guid? gameProviderConfigurationId = default;
            if (playerModel != null)
            {
                gameProviderConfigurationId = await _utensil
                    .GetDGTFBrandGameProviderConfigurationIdAsync(
                        brandId: playerModel.BrandId,
                        gameProviderId: gameProviderId,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                if (!gameProviderConfigurationId.HasValue)
                {
                    success = false;
                }
            }

            var gameId = await _utensil
                .GetDGTFGameIdAsync(
                    gameProviderId: gameProviderId,
                    gameExternalId: record.ExternalGameId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var clientStatus = GetGameRoundClientStatus(record.IsClosed, record.PlayerWinLoss);
            var status = record.IsClosed ? GameRoundStatus.Closed : GameRoundStatus.Void;

            var message = new GameRecordMessage(
                ExternalRoundId: record.ExternalRoundId,
                GameProviderId: gameProviderId,
                GameProviderCode: _gameRecordInfo.GameProviderCode,
                GameId: gameId,
                ExternalGameId: record.ExternalGameId,
                PlayerCode: (playerModel == null) ? string.Empty : playerModel.PlayerCode,
                CurrencyCode: (playerModel == null) ? string.Empty : playerModel.DefaultWalletCurrencyCode,
                WalletId: (playerModel == null) ? Guid.Empty : playerModel.DefaultWalletId,
                BrandId: (playerModel == null) ? Guid.Empty : playerModel.BrandId,
                GameProviderConfigurationId: gameProviderConfigurationId ?? Guid.Empty,
                Status: status,
                ClientStatus: clientStatus,
                ExternalCreatedOn: record.ExternalCreatedOn.ToDateTimeOffset(),
                ExternalClosedOn: record.ExternalClosedOn.ToDateTimeOffset(),
                BetAmount: record.Bet.ToDecimal().GetValueOrDefault(0),
                ValidBetAmount: record.ValidBet.ToDecimal().GetValueOrDefault(0),
                SettleAmount: record.Settle,
                Commission: 0,
                Turnover: record.Turnover,
                PlayerWinLoss: record.PlayerWinLoss,
                AdditionalData: record.AdditionalData,
                Success: success);

            _logger.LogInformation("LogOn:{logOn}  Subject:{subject} MessageRequest:{message}",
                DateTime.Now.ToString("o"),
                _mongoSetting.GlobPattern,
                message);

            await _messageSender
                .PublishAsync(
                    subject: _mongoSetting.GlobPattern,
                    data: message.ToBson(),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private GameRoundClientStatus GetGameRoundClientStatus(bool IsClosed, decimal PlayerWinLoss)
    {
        if (IsClosed is false)
            return GameRoundClientStatus.Void;

        return PlayerWinLoss switch
        {
            > 0 => GameRoundClientStatus.Win,
            < 0 => GameRoundClientStatus.Loss,
            _ => GameRoundClientStatus.Draw
        };
    }
}