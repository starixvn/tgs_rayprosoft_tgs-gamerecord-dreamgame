using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameProviders.Client;
using Tgs.GameRounds.Client;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.MessageModels;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

internal class GameRecordCommandService : ICommandService<GameRecordCommand>
{
    private readonly IDreamGameClient _dreamGameClient;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IGameRoundsClient _gameRoundsClient;
    private readonly ILogger<GameRecordCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly ISystemClock _systemClock;
    private readonly IUtensil _utensil;

    public GameRecordCommandService(
        IOptions<GameRecordInfoOptions> options,
        ILogger<GameRecordCommandService> logger,
        IDreamGameClient dreamGameClient,
        IGameRoundsClient gameRoundsClient,
        IMessageSender messageSender,
        ISystemClock systemClock,
        IUtensil utensil)
    {
        _gameRecordInfo = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dreamGameClient = dreamGameClient ?? throw new ArgumentNullException(nameof(dreamGameClient));
        _gameRoundsClient = gameRoundsClient ?? throw new ArgumentNullException(nameof(gameRoundsClient));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask ExecuteAsync(GameRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellation: cancellationToken)
            .ConfigureAwait(false);

        var gameId = await _utensil
            .GetGameIdAsync(
                gameProviderId: gameProviderId,
                gameExternalId: _gameRecordInfo.GameExternalId,
                cancellation: cancellationToken)
            .ConfigureAwait(false);

        var gameRecordResponse = await _dreamGameClient.GameRecordListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        if (gameRecordResponse == null || !gameRecordResponse.Success)
        {
            return;
        }

        var markGameRecords = new List<string>();
        var gameRecords = gameRecordResponse.Datas?
            .Where(x => !x.ExternalClosedOn.HasValue || x.ExternalClosedOn.Value.UtcDateTime < _systemClock.CurrentTime.UtcDateTime.AddMinutes(-3))
            ?? Enumerable.Empty<GameRecordModle>();

        _logger.LogInformation("Information Received:{gameRecords}", gameRecords);

        foreach (var data in gameRecords)
        {
            var player = await _utensil
                .GetPlayerAsync(
                    playerCode: data.PlayerCode,
                    cancellation: cancellationToken)
                .ConfigureAwait(false);

            if (player is null)
            {
                continue;
            }

            var roundModel = await _gameRoundsClient
                .GetGameRoundAsync(
                    gameProviderId: gameProviderId,
                    externalRoundId: data.ExternalRoundId,
                    gameId: gameId,
                    walletId: player.DefaultWalletId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (!roundModel.Success)
            {
                _logger.LogError("porder does not exist({ExternalRoundId})", data.ExternalRoundId);
                continue;
            }

            if (roundModel.Data == null)
            {
                var tempGameId = await _utensil
                    .GetGameIdAsync(
                        gameProviderId: gameProviderId,
                        gameExternalId: data.ExternalGameId,
                        cancellation: cancellationToken)
                    .ConfigureAwait(false);

                roundModel = await _gameRoundsClient
                        .GetGameRoundAsync(
                            gameProviderId: gameProviderId,
                            externalRoundId: data.ExternalRoundId,
                            gameId: tempGameId,
                            walletId: player.DefaultWalletId,
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                if (roundModel.Data == null)
                {
                    continue;
                }
            }

            if (data.Status == GameRecordRoundStatus.Closed)
            {
                var messager = new GameRecordMessage(
                    ExternalRoundId: roundModel.Data.ExternalRoundId,
                    GameProviderId: roundModel.Data.GameProviderId,
                    GameProviderCode: data.GameProviderCode,
                    GameId: roundModel.Data.GameId,
                    ExternalGameId: _gameRecordInfo.GameExternalId,
                    PlayerCode: roundModel.Data.PlayerCode,
                    CurrencyCode: (player == null) ? string.Empty : player.DefaultWalletCurrencyCode,
                    WalletId: (player == null) ? Guid.Empty : player.DefaultWalletId,
                    BrandId: (player == null) ? Guid.Empty : player.BrandId,
                    GameProviderConfigurationId: roundModel.Data.GameProviderConfigurationId,
                    Status: (GameRoundStatus)data.Status,
                    ClientStatus: (GameRoundClientStatus)data.ClientStatus,
                    ExternalCreatedOn: data.ExternalCreatedOn!.Value,
                    ExternalClosedOn: data.ExternalClosedOn,
                    BetAmount: data.BetAmount.GetValueOrDefault(0),
                    ValidBetAmount: data.ValidBetAmount.GetValueOrDefault(0),
                    SettleAmount: data.SettleAmount.GetValueOrDefault(0),
                    Commission: data.Commission.GetValueOrDefault(0),
                    Turnover: data.Turnover.GetValueOrDefault(0),
                    PlayerWinLoss: data.PlayerWinLoss.GetValueOrDefault(0),
                    AdditionalData: data.AdditionalData,
                    Success: true);
                await _messageSender
                    .PublishAsync(
                        subject: $"tgs.gp.gamerecord.database.round.dg",
                        data: messager.ToBson(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            markGameRecords.Add(data.ExternalRoundId);
        }
        if (markGameRecords.Any())
        {
            await _dreamGameClient.MarkGameRecordListAsync(markGameRecords, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
