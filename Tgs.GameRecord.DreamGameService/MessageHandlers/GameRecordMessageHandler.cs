using System.Text.Json;
using MongoDB.Bson;
using Tgs.GameRounds.Common.MessageModels;
using Tgs.MessageQueue;

namespace Tgs.GameRecord.DreamGameService.MessageHandlers
{
    public class GameRecordMessageHandler : IGameRecordMessageHandler
    {
        private readonly ILogger<GameRecordMessageHandler> _logger;
        private readonly IMessageSender _messageSender;

        public GameRecordMessageHandler(
            ILogger<GameRecordMessageHandler> logger,
            IMessageSender messageSender)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        }

        public ValueTask SendGameRoundAsync(GameRecordMessage data)
        {
            _logger.Log(LogLevel.Information,
                new EventId(1000, nameof(GameRecordMessageHandler)),
                new
                {
                    MessageQueue = $"tgs.gp.gamerecord.database.round.dg",
                    Data = data,
                    SendDateTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(+8)).ToString("O")
                },
                null,
                (state, ex) => JsonSerializer.Serialize(state));
            return _messageSender.PublishAsync($"tgs.gp.gamerecord.database.round.dg", data.ToBson());
        }
    }
}
