using System.Text.Json;
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
			logger.LogError("{logError}",
				JsonSerializer.Serialize(new
				{
					LogOn = DateTime.Now.ToString("u"),
					Message = "This game provider configuration schedule has not been started, initializing is required."
				}));
			return service
				.ExecuteAsync(
				command: new(),
				cancellationToken: cancellationToken);
		}

		logger.LogWarning("{logWarning}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Message = $"This game provider configuration schedule has been started ({connection.GetRecurringJobs().Count})."
			}));
		return ValueTask.CompletedTask;
	}
}
