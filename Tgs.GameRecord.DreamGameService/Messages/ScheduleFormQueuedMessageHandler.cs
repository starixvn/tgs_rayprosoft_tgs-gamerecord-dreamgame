using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRounds.Schedule.Common.Protos;
using Tgs.MessageQueue;

namespace Tgs.GameRecord.DreamGameService.Messages;

internal class ScheduleFormQueuedMessageHandler : IMessageHandler
{
    private readonly IScheduleFormDispatcher _dispatcher;
    private readonly ILogger<ScheduleFormQueuedMessageHandler> _logger;

    public ScheduleFormQueuedMessageHandler(
        ILogger<ScheduleFormQueuedMessageHandler> logger,
        IScheduleFormDispatcher dispatcher)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = GameRoundsScheduleFormRequest.Parser.ParseFrom(data.Span)
        ?? throw new NullReferenceException("The request data resolves to a null value");

        _logger.LogInformation("LogOn:{logOn} Information Request:{request}", DateTime.Now.ToString("o"), request);

        return _dispatcher.ExecuteAsync(
            gameProviderCode: request.GameProviderCode,
            id: request.Id,
            cancellationToken: cancellationToken);
    }
}
