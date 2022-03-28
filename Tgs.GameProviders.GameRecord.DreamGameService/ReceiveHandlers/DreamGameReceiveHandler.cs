using System.Text;
using System.Text.Json;
using Tgs.GameProviders.GameRecord.DreamGameService.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using TGS.GameProviders.Domains;

namespace Tgs.GameProviders.GameRecord.DreamGameService.ReceiveHandlers
{
    internal class DreamGameReceiveHandler : IMessageHandler
    {
        private readonly ICommandService<GameRecordCommand> _commandService;
        private readonly ILogger<DreamGameReceiveHandler> _logger;

        public DreamGameReceiveHandler(
            ILogger<DreamGameReceiveHandler> logger,
            ICommandService<GameRecordCommand> commandService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        }

        public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            var json = Encoding.UTF8.GetString(data.Span);
            var command = JsonSerializer.Deserialize<DreamGameCommand>(json);
            _logger.Log(LogLevel.Information,
                new EventId((int)GameProviderUnique.DreamGame, nameof(DreamGameReceiveHandler)),
                new
                {
                    GameProviderCode = GameProviderType.TCG,
                    GameRecord = command!.GameProviderCode,
                    SendDateTime = command!.SendDateTime.ToOffset(TimeSpan.FromHours(+8)).ToString("O"),
                    ReceiveTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(+8)).ToString("O")
                },
                null,
                (state, ex) => JsonSerializer.Serialize(state));
            return _commandService.ExecuteAsync(new(), cancellationToken);
        }
    }
}
