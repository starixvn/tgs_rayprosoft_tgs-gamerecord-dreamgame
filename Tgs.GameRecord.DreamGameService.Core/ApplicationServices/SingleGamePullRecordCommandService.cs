using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameRounds.Client;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.MessageModels;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

internal class SingleGamePullRecordCommandService : ICommandService<SingleGamePullRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IGameRoundsClient _gameRoundsClient;
    private readonly ILogger<SingleGamePullRecordCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly MongoSettingOptions _mongoSetting;
    private readonly IUtensil _utensil;

    public SingleGamePullRecordCommandService(
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IOptions<MongoSettingOptions> mongoSettingOptions,
        ILogger<SingleGamePullRecordCommandService> logger,
        IGameRoundsClient gameRoundsClient,
        IMessageSender messageSender,
        IUtensil utensil)
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _mongoSetting = mongoSettingOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRoundsClient = gameRoundsClient ?? throw new ArgumentNullException(nameof(gameRoundsClient));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask ExecuteAsync(SingleGamePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var gameId = await _utensil
            .GetGameIdAsync(
                gameProviderId: gameProviderId,
                gameExternalId: _gameRecordInfo.GameExternalId,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        foreach (var record in command.Records)
        {
            var player = await _utensil
                .GetPlayerAsync(
                    playerCode: record.PlayerCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (player is null)
            {
                continue;
            }

            var roundModel = await _gameRoundsClient
                .GetGameRoundAsync(
                    gameProviderId: gameProviderId,
                    externalRoundId: record.ExternalRoundId,
                    gameId: gameId,
                    walletId: player.DefaultWalletId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (!roundModel.Success)
            {
                _logger.LogError("porder does not exist({ExternalRoundId})", record.ExternalRoundId);
                continue;
            }

            if (roundModel.Data == null)
            {
                var tempGameId = await _utensil
                    .GetGameIdAsync(
                        gameProviderId: gameProviderId,
                        gameExternalId: record.ExternalGameId,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                roundModel = await _gameRoundsClient
                        .GetGameRoundAsync(
                            gameProviderId: gameProviderId,
                            externalRoundId: record.ExternalRoundId,
                            gameId: tempGameId,
                            walletId: player.DefaultWalletId,
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                if (roundModel.Data == null)
                {
                    continue;
                }
            }

            var externalClosedOn = (DateTimeOffset?)null;
            var clientStatus = GameRoundClientStatus.Open;
            if (record.IsVoid)
            {
                clientStatus = GameRoundClientStatus.Void;
            }
            else
            {

                if (record.IsClosed)
                {
                    externalClosedOn = record.ExternalClosedOn.ToDateTimeOffset();
                    clientStatus = record.PlayerWinLoss.ToDecimal().GetValueOrDefault(0) switch
                    {
                        > 0 => GameRoundClientStatus.Win,
                        < 0 => GameRoundClientStatus.Loss,
                        _ => GameRoundClientStatus.Draw
                    };
                }
            }

            var status = record.IsClosed || record.IsVoid ? GameRoundStatus.Closed : GameRoundStatus.Open;

            var message = new GameRecordMessage(
               ExternalRoundId: record.ExternalRoundId,
               GameProviderId: gameProviderId,
               GameProviderCode: _gameRecordInfo.GameProviderCode,
               GameId: roundModel.Data.GameId,
               ExternalGameId: _gameRecordInfo.GameExternalId,
               PlayerCode: roundModel.Data.PlayerCode,
               CurrencyCode: roundModel.Data.CurrencyCode,
               WalletId: roundModel.Data.WalletId,
               BrandId: roundModel.Data.BrandId,
               GameProviderConfigurationId: roundModel.Data.GameProviderConfigurationId,
               Status: status,
               ClientStatus: clientStatus,
               ExternalCreatedOn: record.ExternalCreatedOn.ToDateTimeOffset(),
               ExternalClosedOn: externalClosedOn,
               BetAmount: record.Bet.ToDecimal().GetValueOrDefault(0),
               ValidBetAmount: record.ValidBet.ToDecimal().GetValueOrDefault(0),
               SettleAmount: record.Settle.ToDecimal().GetValueOrDefault(0),
               Commission: 0,
               Turnover: record.Turnover.ToDecimal().GetValueOrDefault(0),
               PlayerWinLoss: record.PlayerWinLoss.ToDecimal().GetValueOrDefault(0),
               AdditionalData: record.AdditionalData,
               Success: true);

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
}
