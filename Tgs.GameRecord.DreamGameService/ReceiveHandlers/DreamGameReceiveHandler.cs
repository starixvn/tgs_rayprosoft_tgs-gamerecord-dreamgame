using System.Text;
using System.Text.Json;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.DreamGameService.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.ReceiveHandlers
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

            var logInfo = new
            {
                GameProviderCode = GameProviderType.BigGaming,
                Data = command,
                ReceiveTime = DateTimeOffset.Now.ToString("O")
            };
            if (command == null)
            {
                _logger.LogError("Information Received:{LogInfo}", logInfo);
                return ValueTask.CompletedTask;
            }
            _logger.LogInformation("Information Received:{LogInfo}", logInfo);

            return _commandService.ExecuteAsync(new(), cancellationToken);
        }
    }
}
