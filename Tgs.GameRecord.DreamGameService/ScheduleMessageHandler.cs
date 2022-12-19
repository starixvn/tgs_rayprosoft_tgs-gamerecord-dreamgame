using System.Text;
using System.Text.Json;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService;

internal class ScheduleMessageHandler : IMessageHandler
{
    private readonly ICommandService<GameRecordCommand> _commandService;
    private readonly ILogger<ScheduleMessageHandler> _logger;

    public ScheduleMessageHandler(
        ILogger<ScheduleMessageHandler> logger,
        ICommandService<GameRecordCommand> commandService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var messageModel = JsonSerializer.Deserialize<ScheduleMessageModel>(json);

        if (messageModel == null)
        {
            _logger.LogError("Information Received:{LogInfo}", messageModel);
            return ValueTask.CompletedTask;
        }

        _logger.LogInformation("Information Received:{LogInfo}", messageModel);

        return _commandService.ExecuteAsync(new(), cancellationToken);
    }
}
