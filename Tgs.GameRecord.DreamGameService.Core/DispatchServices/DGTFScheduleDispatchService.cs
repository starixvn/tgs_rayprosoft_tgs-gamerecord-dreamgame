using Microsoft.Extensions.Options;
using Tgs.GameRecord.DreamGameService.Core.WorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.DispatchServices;

internal class DGTFScheduleDispatchService : ICommandService<DGTFScheduleDispatchCommand>
{
    private readonly ICommandService<GameHistoryWorkerCommand> _commandService;
    private readonly DGTFGameRecordInfoOptions _gameRecordInfoOptions;
    private readonly double _intervalMinute;
    private readonly IScheduleDispatch _scheduleDispatch;

    public DGTFScheduleDispatchService(
        IOptions<DGTFGameRecordInfoOptions> options,
        ICommandService<GameHistoryWorkerCommand> commandService,
        IScheduleDispatch scheduleDispatch)
    {
        _gameRecordInfoOptions = options.Value;
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _scheduleDispatch = scheduleDispatch ?? throw new ArgumentNullException(nameof(scheduleDispatch));
        _intervalMinute = (_gameRecordInfoOptions.IntervalMinutes > _gameRecordInfoOptions.IntervalMinutesMax)
                        ? _gameRecordInfoOptions.IntervalMinutesMax
                        : (_gameRecordInfoOptions.IntervalMinutes < _gameRecordInfoOptions.IntervalMinutesMinimum)
                        ? _gameRecordInfoOptions.IntervalMinutesMinimum
                        : _gameRecordInfoOptions.IntervalMinutes;
    }

    public async ValueTask ExecuteAsync(DGTFScheduleDispatchCommand command, CancellationToken cancellationToken = default)
    {
        var key = $"gp:gamerecord:{_gameRecordInfoOptions.GameProviderCode}:{command.AgentName}".ToLower();
        if (!await _scheduleDispatch.ExistAsync(key: key).ConfigureAwait(false))
        {
            if (await _scheduleDispatch.LockingAsync(key: key, command: command).ConfigureAwait(false))
            {
                try
                {
                    var endTime = DateTimeOffset.ParseExact(DateTimeOffset.UtcNow.ToString(@"yyyy-MM-dd\THH:mm:00K"), @"yyyy-MM-dd\THH:mm:ssK", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);

                    var startTime = endTime.AddMinutes(_intervalMinute * -1);

                    var workerCommand = new GameHistoryWorkerCommand(
                         ApiUrl: command.ApiUrl,
                         ApiKey: command.ApiKey,
                         AgentName: command.AgentName);

                    await _commandService
                            .ExecuteAsync(
                                command: workerCommand,
                                cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    await _scheduleDispatch.UnlockAsync(key: key).ConfigureAwait(false);
                }
            }
        }
    }
}