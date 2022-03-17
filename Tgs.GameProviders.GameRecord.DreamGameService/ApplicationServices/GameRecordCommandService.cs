using Tgs.GameProviders.QueryServer.Client;
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

        public GameRecordCommandService(IDreamGameClient dreamGameClient,
            IGameProvidersQueryServerClient gameProvidersQueryServerClient,
            IGameRecordMessageHandler gameRecordMessageHandler)
        {
            _dreamGameClient = dreamGameClient ?? throw new ArgumentNullException(nameof(dreamGameClient));
            _gameProvidersQueryServerClient = gameProvidersQueryServerClient ?? throw new ArgumentNullException(nameof(gameProvidersQueryServerClient));
            _gameRecordMessageHandler = gameRecordMessageHandler ?? throw new ArgumentNullException(nameof(gameRecordMessageHandler));
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

                await _gameRecordMessageHandler.SendGameRoundDetailAsync(new(roundModel.RoundId, data.AdditionalData)).ConfigureAwait(false);
                markGameRecords.Add(data.ExternalRoundId);
            }

            await _dreamGameClient.MarkGameRecordListAsync(markGameRecords).ConfigureAwait(false);
        }
    }
}
