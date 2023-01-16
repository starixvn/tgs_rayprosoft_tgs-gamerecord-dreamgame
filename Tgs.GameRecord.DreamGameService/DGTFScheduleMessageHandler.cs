using System.Text;
using System.Text.Json;
using Tgs.GameRecord.DreamGameService.Core.DispatchServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService;

internal class DGTFScheduleMessageHandler : IMessageHandler
{
    private readonly ILogger<DGTFScheduleMessageHandler> _logger;
    private readonly ICommandService<DGTFScheduleDispatchCommand> _service;

    public DGTFScheduleMessageHandler(
        ILogger<DGTFScheduleMessageHandler> logger,
        ICommandService<DGTFScheduleDispatchCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<DGTFScheduleMessageModel>(json);

        if (message == null)
        {
            _logger.LogError("Information Received:{message}", message);
            return ValueTask.CompletedTask;
        }

        _logger.LogInformation("Information Received:{message}", message);

        var command = new DGTFScheduleDispatchCommand(
            ApiUrl: message.ApiUrl,
            ApiKey: message.ApiKey,
            AgentName: message.AgentName);

        return _service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}
