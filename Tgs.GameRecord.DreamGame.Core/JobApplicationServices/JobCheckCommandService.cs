using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGame.Core.JobApplicationServices;

internal class JobCheckCommandService(
	ILogger<JobCheckCommandService> logger,
	ICommandService<JobInitCommand> service) : ICommandService<JobCheckCommand>
{
	public ValueTask ExecuteAsync(JobCheckCommand command, CancellationToken cancellationToken = default)
	{
		using var connection = JobStorage.Current.GetConnection();
		if (connection.GetRecurringJobs().Count == 0)
		{
			logger.LogError("LogOn:{logOn} Message:檢查遊戲廠商排程未啟動，進行重置。", DateTime.Now.ToString("o"));
			return service
				.ExecuteAsync(
				command: new(),
				cancellationToken: cancellationToken);
		}

		logger.LogWarning("LogOn:{logOn} Message:檢查遊戲廠商排程有啟動，數量({count})。", DateTime.Now.ToString("o"), connection.GetRecurringJobs().Count);
		return ValueTask.CompletedTask;
	}
}
