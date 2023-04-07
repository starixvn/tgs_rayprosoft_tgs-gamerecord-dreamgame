using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.GameRounds.Schedule.Client;
using Tgs.GameRounds.Schedule.Common.Protos;
using Tgs.GameRounds.Schedule.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core;

internal class ScheduleFormDispatcher : IScheduleFormDispatcher
{
    private readonly IGameRoundsScheduleClient _clinet;
    private readonly ILogger<ScheduleFormDispatcher> _logger;
    private readonly IScheduleFormLock _scheduleFormLock;
    private readonly ISchedulePullRecordLock _schedulePullRecordLock;
    private readonly ICommandService<SchedulePullRecordCommand> _scheduleService;

    public ScheduleFormDispatcher(
        ILogger<ScheduleFormDispatcher> logger,
        IGameRoundsScheduleClient clinet,
        IScheduleFormLock scheduleFormLock,
        ISchedulePullRecordLock schedulePullRecordLock,
        ICommandService<SchedulePullRecordCommand> scheduleService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _clinet = clinet ?? throw new ArgumentNullException(nameof(clinet));
        _scheduleFormLock = scheduleFormLock ?? throw new ArgumentNullException(nameof(scheduleFormLock));
        _schedulePullRecordLock = schedulePullRecordLock ?? throw new ArgumentNullException(nameof(schedulePullRecordLock));
        _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
    }

    public async ValueTask ExecuteAsync(
        string gameProviderCode,
        long id,
        CancellationToken cancellationToken = default)
    {
        var scheduleForm = await _clinet
            .ScheduleFormGetAsync(
                gameProviderCode: gameProviderCode,
                id: id,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new Exception($"The game provider ({gameProviderCode}) cannot find the schedule form. ({id})");

        if (scheduleForm.State != ScheduleFormState.Queued)
        {
            _logger.LogWarning("This game provider ({gameProviderCode}) schedule form status is not queued.({id})", gameProviderCode, id);
            return;
        }

        if (!await _scheduleFormLock
            .LockingAsync(
                id: id)
            .ConfigureAwait(false))
        {
            _logger.LogWarning("LogOn:{logOn} Message:This game provider ({gameProviderCode}) schedule form has been locked. ({id})",
                DateTime.Now.ToString("o"),
                gameProviderCode,
                id);
            return;
        }

        try
        {
            await _clinet
                .ScheduleFormProcessingAsync(
                    gameProviderCode: gameProviderCode,
                    id: id,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var parameter = JsonSerializer.Deserialize<DreamgameParameter>(scheduleForm.Parameters)
                ?? throw new Exception($"This game provider ({gameProviderCode}) scheduling form parameter is null. ({id})");

            if (await _schedulePullRecordLock
                .LockingAsync(
                    agentName: parameter.AgentName)
                .ConfigureAwait(false))
            {
                try
                {
                    var command = new SchedulePullRecordCommand(
                        ApiUrl: new Uri(parameter.ApiUrl),
                        ApiKey: parameter.ApiKey,
                        AgentName: parameter.AgentName);

                    _logger.LogInformation("LogOn:{logOn} execute the command:{command}", DateTime.Now.ToString("o"), command);

                    await _scheduleService
                        .ExecuteAsync(
                            command: command,
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occurred in the game provider ({gameProviderCode}) schedule form. ({id})", gameProviderCode, id);
                    throw;
                }
                finally
                {
                    _ = await _schedulePullRecordLock
                         .UnlockAsync(
                             agentName: parameter.AgentName)
                         .ConfigureAwait(false);
                }
            }

            await _clinet
                .ScheduleFormCompleteAsync(
                    gameProviderCode: gameProviderCode,
                    id: id,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred in the game provider ({gameProviderCode}) schedule form. ({id})", gameProviderCode, id);

            await _clinet
                .ScheduleFormFaileAsync(
                    gameProviderCode: gameProviderCode,
                    id: id,
                    status: ex.Message,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _ = await _scheduleFormLock
                 .UnlockAsync(
                     id: id)
                 .ConfigureAwait(false);
        }
    }
}
