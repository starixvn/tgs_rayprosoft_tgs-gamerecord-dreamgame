using Microsoft.Extensions.Options;
using Tgs.GameRecord.DreamGameService.Core.WorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.DispatchServices;

internal class TransferScheduleDispatchService : ICommandService<TransferScheduleDispatchCommand>
{
    private readonly ICommandService<TransferGameHistoryWorkerCommand> _commandService;
    private readonly GameRecordInfoOptions _gameRecordInfoOptions;
    private readonly IScheduleDispatch _scheduleDispatch;

    public TransferScheduleDispatchService(
        IOptions<GameRecordInfoOptions> options,
        ICommandService<TransferGameHistoryWorkerCommand> commandService,
        IScheduleDispatch scheduleDispatch)
    {
        _gameRecordInfoOptions = options.Value;
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _scheduleDispatch = scheduleDispatch ?? throw new ArgumentNullException(nameof(scheduleDispatch));
    }

    public async ValueTask ExecuteAsync(TransferScheduleDispatchCommand command, CancellationToken cancellationToken = default)
    {
        var key = $"gp:gamerecord:{_gameRecordInfoOptions.GameProviderCodeTransfer}:{command.AgentName}".ToLower();
        if (!await _scheduleDispatch.ExistAsync(key: key).ConfigureAwait(false))
        {
            if (await _scheduleDispatch.LockingAsync(key: key, command: command).ConfigureAwait(false))
            {
                try
                {
                    var workerCommand = new TransferGameHistoryWorkerCommand(
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
