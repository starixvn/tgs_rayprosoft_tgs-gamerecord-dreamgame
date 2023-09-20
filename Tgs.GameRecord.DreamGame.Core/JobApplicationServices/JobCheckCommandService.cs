using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGame.Core.JobApplicationServices;

internal class JobCheckCommandService : ICommandService<JobCheckCommand>
{
    private readonly ILogger<JobCheckCommandService> _logger;
    private readonly ICommandService<JobInitCommand> _service;

    public JobCheckCommandService(
        ILogger<JobCheckCommandService> logger,
        ICommandService<JobInitCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask ExecuteAsync(JobCheckCommand command, CancellationToken cancellationToken = default)
    {
        using var connection = JobStorage.Current.GetConnection();
        if (!connection.GetRecurringJobs().Any())
        {
            _logger.LogError("LogOn:{logOn} Message:檢查遊戲廠商排程未啟動，進行重置。", DateTime.Now.ToString("o"));
            return _service
                .ExecuteAsync(
                command: new(),
                cancellationToken: cancellationToken);
        }
        _logger.LogWarning("LogOn:{logOn} Message:檢查遊戲廠商排程有啟動，數量({count})。", DateTime.Now.ToString("o"), connection.GetRecurringJobs().Count);
        return ValueTask.CompletedTask;
    }
}
