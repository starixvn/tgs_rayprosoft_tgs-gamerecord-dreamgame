using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameProviders.QueryServer.Models;
using Tgs.ServiceUtility;
using TGS.GameProviders.Client;
using TGS.GameProviders.Domains;

namespace Tgs.GameProviders.GameRecord.DreamGameService.ApplicationServices
{
    public class GameRecordCommandService : ICommandService<GameRecordCommand>
    {
        private readonly IDreamGameClient _dreamGameClient;
        private readonly IGameProvidersQueryServerClient _gameProvidersQueryServerClient;
        private readonly IGameRecordMessageHandler _gameRecordMessageHandler;
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;

        public GameRecordCommandService(IDreamGameClient dreamGameClient,
            IGameProvidersQueryServerClient gameProvidersQueryServerClient,
            IGameRecordMessageHandler gameRecordMessageHandler,
            IGameService gameService,
            IPlayerService playerService)
        {
            _dreamGameClient = dreamGameClient ?? throw new ArgumentNullException(nameof(dreamGameClient));
            _gameProvidersQueryServerClient = gameProvidersQueryServerClient ?? throw new ArgumentNullException(nameof(gameProvidersQueryServerClient));
            _gameRecordMessageHandler = gameRecordMessageHandler ?? throw new ArgumentNullException(nameof(gameRecordMessageHandler));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _playerService = playerService;
        }

        public async ValueTask ExecuteAsync(GameRecordCommand command, CancellationToken cancellationToken = default)
        {
            var gameProviderId = await _gameProvidersQueryServerClient.GameProviderGetGameProviderIdAsync(GameProviderType.DreamGame).ConfigureAwait(false);
            if (!gameProviderId.HasValue)
            {
                return;
            }

            var game = await _gameProvidersQueryServerClient.GameGetAsync(gameProviderId.Value, "dg").ConfigureAwait(false);
            if (game == null)
            {
                return;
            }

            var gameRecordResponse = await _dreamGameClient.GameRecordListAsync().ConfigureAwait(false);
            if (gameRecordResponse == null || !gameRecordResponse.Success || gameRecordResponse.Datas == null)
            {
                return;
            }

            var markGameRecords = new List<string>();
            foreach (var data in gameRecordResponse.Datas)
            {
                var roundModel = await _gameProvidersQueryServerClient.RoundGetAsync(gameProviderId.Value, data.ExternalRoundId, game.GameId, data.PlayerCode).ConfigureAwait(false);
                if (roundModel == null)
                {
                    var tempGame = await _gameProvidersQueryServerClient.GameGetAsync(gameProviderId.Value, data.ExternalGameId).ConfigureAwait(false);
                    if (tempGame == null)
                    {
                        continue;
                    }
                    roundModel = await _gameProvidersQueryServerClient.RoundGetAsync(gameProviderId.Value, data.ExternalRoundId, tempGame.GameId, data.PlayerCode).ConfigureAwait(false);
                    if (roundModel == null)
                    {
                        continue;
                    }
                }
                var playerInfo = await _playerService.GetAsync(gameProviderId.Value, data.PlayerCode).ConfigureAwait(false);
                if (playerInfo == null)
                {
                    continue;
                }
                var gameId = await _gameService.GetGameIdAsync(gameProviderId.Value, data.ExternalRoundId).ConfigureAwait(false);
                if (!gameId.HasValue)
                {
                    continue;
                }
                if (roundModel.Status == RoundStatus.Open)
                {
                    await _gameRecordMessageHandler.SendGameRoundAsync(new GameRoundData()
                    {
                        GameProviderCode = data.GameProviderCode,
                        GameProviderId = data.GameProviderId,
                        ExternalRoundId = data.ExternalRoundId,
                        PlayerCode = data.PlayerCode,
                        GameId = gameId ?? -1,
                        ExternalGameId = data.ExternalGameId,
                        CurrencyCode = (playerInfo == null) ? string.Empty : playerInfo.DefaultWalletCurrencyCode,
                        Status = (RoundStatus)data.Status,
                        GameProviderConfigurationId = (playerInfo == null) ? Guid.Empty : playerInfo.GameProviderConfigurationId,
                        WalletId = (playerInfo == null) ? Guid.Empty : playerInfo.DefaultWalletId,
                        CreatedOn = roundModel.CreatedOn,
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
                        ClientStatus = (RoundClientStatus)data.ClientStatus,
                        AdditionalData = data.AdditionalData,
                        Success = true
                    }).ConfigureAwait(false);
                }

                markGameRecords.Add(data.ExternalRoundId);
            }

            await _dreamGameClient.MarkGameRecordListAsync(markGameRecords).ConfigureAwait(false);
        }
    }
}
