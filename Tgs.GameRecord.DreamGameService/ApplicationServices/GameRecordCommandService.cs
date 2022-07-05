using Tgs.GameProviders.Client;
using Tgs.GameRounds.Client;
using Tgs.GameRounds.Common;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.ApplicationServices
{
    public class GameRecordCommandService : ICommandService<GameRecordCommand>
    {
        private readonly IDreamGameClient _dreamGameClient;
        private readonly IGameProviderService _gameProviderService;
        private readonly IGameRecordMessageHandler _gameRecordMessageHandler;
        private readonly IGameRoundsClient _gameRoundsClient;
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;
        private readonly ISystemClock _systemClock;

        public GameRecordCommandService(
            IDreamGameClient dreamGameClient,
            IGameProviderService gameProviderService,
            IGameRecordMessageHandler gameRecordMessageHandler,
            IGameRoundsClient gameRoundsClient,
            IGameService gameService,
            IPlayerService playerService,
            ISystemClock systemClock)
        {
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

            foreach (var data in gameRecords)
            {
                var playerInfo = await _playerService.GetAsync(gameProviderId.Value, data.PlayerCode).ConfigureAwait(false);
                if (playerInfo == null)
                {
                    continue;
                }

                var roundModel = await _gameRoundsClient.GetGameRoundAsync(gameProviderId.Value, data.ExternalRoundId, game.GameId, playerInfo.DefaultWalletId).ConfigureAwait(false);

                if (!roundModel.Success)
                {
                    continue;
                }

                if (roundModel.Data == null)
                {
                    var tempGame = await _gameService.GetGameAsync(gameProviderId.Value, data.ExternalGameId).ConfigureAwait(false);
                    if (tempGame == null)
                    {
                        continue;
                    }
                    roundModel = await _gameRoundsClient.GetGameRoundAsync(gameProviderId.Value, data.ExternalRoundId, tempGame.GameId, playerInfo.DefaultWalletId).ConfigureAwait(false);
                    if (roundModel.Data == null)
                    {
                        continue;
                    }
                }

                if (roundModel.Data.Status == GameRoundStatus.Open || data.Status == GameRecordRoundStatus.Closed)
                {
                    await _gameRecordMessageHandler.SendGameRoundAsync(new GameRoundData()
                    {
                        GameProviderCode = data.GameProviderCode,
                        GameProviderId = roundModel.Data.GameProviderId,
                        ExternalRoundId = roundModel.Data.ExternalRoundId,
                        PlayerCode = roundModel.Data.PlayerCode,
                        GameId = roundModel.Data.GameId,
                        ExternalGameId = game.GameExternalId,
                        CurrencyCode = (playerInfo == null) ? string.Empty : playerInfo.DefaultWalletCurrencyCode,
                        Status = (GameRoundStatus)data.Status,
                        GameProviderConfigurationId = roundModel.Data.GameProviderConfigurationId,
                        WalletId = (playerInfo == null) ? Guid.Empty : playerInfo.DefaultWalletId,
                        CreatedOn = roundModel.Data.CreatedOn,
                        ClosedOn = (data.Status == GameRecordRoundStatus.Closed) ? DateTimeOffset.UtcNow : null,
                        UpdatedOn = DateTimeOffset.UtcNow,
                        BrandId = (playerInfo == null) ? Guid.Empty : playerInfo.BrandId,
                        ExternalCreatedOn = data.ExternalCreatedOn,
                        ExternalClosedOn = data.ExternalClosedOn,
                        BetAmount = data.BetAmount,
                        ValidBetAmount = data.ValidBetAmount,
                        PlayerWinLoss = data.PlayerWinLoss,
                        Commission = data.Commission,
                        Turnover = data.Turnover,
                        SettleAmount = data.SettleAmount,
                        TieBetAdjustment = null,
                        ClientStatus = (GameRoundClientStatus)data.ClientStatus,
                        AdditionalData = data.AdditionalData,
                        Success = true
                    }).ConfigureAwait(false);
                }

                markGameRecords.Add(data.ExternalRoundId);
            }
            if (markGameRecords.Any())
            {
                await _dreamGameClient.MarkGameRecordListAsync(markGameRecords, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
