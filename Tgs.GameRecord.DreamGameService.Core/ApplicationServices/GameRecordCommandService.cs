using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Client;
using Tgs.GameRounds.Client;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.MessageModels;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

internal class GameRecordCommandService : ICommandService<GameRecordCommand>
{
    private readonly IDreamGameClient _dreamGameClient;
    private readonly IGameProviderService _gameProviderService;
    private readonly IGameRecordMessageHandler _gameRecordMessageHandler;
    private readonly IGameRoundsClient _gameRoundsClient;
    private readonly IGameService _gameService;
    private readonly ILogger<GameRecordCommandService> _logger;
    private readonly IPlayerService _playerService;
    private readonly ISystemClock _systemClock;

    public GameRecordCommandService(
        ILogger<GameRecordCommandService> logger,
        IDreamGameClient dreamGameClient,
        IGameProviderService gameProviderService,
        IGameRecordMessageHandler gameRecordMessageHandler,
        IGameRoundsClient gameRoundsClient,
        IGameService gameService,
        IPlayerService playerService,
        ISystemClock systemClock)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dreamGameClient = dreamGameClient ?? throw new ArgumentNullException(nameof(dreamGameClient));
        _gameProviderService = gameProviderService ?? throw new ArgumentNullException(nameof(gameProviderService));
        _gameRecordMessageHandler = gameRecordMessageHandler ?? throw new ArgumentNullException(nameof(gameRecordMessageHandler));
        _gameRoundsClient = gameRoundsClient ?? throw new ArgumentNullException(nameof(gameRoundsClient));
        _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
    }

    public async ValueTask ExecuteAsync(GameRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _gameProviderService.GameProviderGetGameProviderIdAsync().ConfigureAwait(false);
        if (!gameProviderId.HasValue)
        {
            return;
        }

        var game = await _gameService.GetGameAsync(gameProviderId.Value, "dg").ConfigureAwait(false);
        if (game == null)
        {
            return;
        }

        var gameRecordResponse = await _dreamGameClient.GameRecordListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        if (gameRecordResponse == null || !gameRecordResponse.Success || gameRecordResponse.Datas == null)
        {
            return;
        }

        var markGameRecords = new List<string>();
        var gameRecords = gameRecordResponse.Datas.Where(x => !x.ExternalClosedOn.HasValue || x.ExternalClosedOn.Value.UtcDateTime < _systemClock.CurrentTime.UtcDateTime.AddMinutes(-3));

        _logger.LogInformation("Information Received:{gameRecords}", gameRecords);

        foreach (var data in gameRecords)
        {
            var player = await _playerService
                .GetAsync(
                    playerCode: data.PlayerCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (player is null)
            {
                continue;
            }

            var roundModel = await _gameRoundsClient
                .GetGameRoundAsync(
                    gameProviderId.Value,
                    data.ExternalRoundId,
                    game.GameId,
                    player.DefaultWalletId,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!roundModel.Success)
            {
                _logger.LogError("porder does not exist({ExternalRoundId})", data.ExternalRoundId);
                continue;
            }

            if (roundModel.Data == null)
            {
                var tempGame = await _gameService.GetGameAsync(gameProviderId.Value, data.ExternalGameId).ConfigureAwait(false);
                if (tempGame == null)
                {
                    continue;
                }
                roundModel = await _gameRoundsClient
                        .GetGameRoundAsync(
                            gameProviderId.Value,
                            data.ExternalRoundId,
                            tempGame.GameId,
                            player.DefaultWalletId,
                            cancellationToken).ConfigureAwait(false);
                if (roundModel.Data == null)
                {
                    continue;
                }
            }

            if (roundModel.Data.Status == GameRoundStatus.Open || data.Status == GameRecordRoundStatus.Closed)
            {
                var messager = new GameRecordMessage(
                    ExternalRoundId: roundModel.Data.ExternalRoundId,
                    GameProviderId: roundModel.Data.GameProviderId,
                    GameProviderCode: data.GameProviderCode,
                    GameId: roundModel.Data.GameId,
                    ExternalGameId: game.GameExternalId,
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

                await _gameRecordMessageHandler.SendGameRoundAsync(messager).ConfigureAwait(false);
            }

            markGameRecords.Add(data.ExternalRoundId);
        }
        if (markGameRecords.Any())
        {
            await _dreamGameClient.MarkGameRecordListAsync(markGameRecords, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
