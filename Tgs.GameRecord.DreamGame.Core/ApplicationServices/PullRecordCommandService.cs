using Microsoft.Extensions.Logging;
using Tgs.GameInterface.DreamGamingService.Client;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGame.Core.ApplicationServices;

internal class PullRecordCommandService(
	ILogger<PullRecordCommandService> logger,
	IPullRecordLock pullRecordLock,
	IGameInterfaceDreamGamingClient client,
	ICommandService<HandleRecordCommand> service) : ICommandService<PullRecordCommand>
{
	public async ValueTask ExecuteAsync(PullRecordCommand command, CancellationToken cancellationToken = default)
	{
		if (!await pullRecordLock.LockingAsync(
			agentName: command.AgentName)
			.ConfigureAwait(false))
		{
			logger.LogWarning("LogOn:{logOn} Message:This agent is locked. ({agentName})",
				DateTime.Now.ToString("o"),
				command.AgentName);
			return;
		}

		try
		{
			var gameRecords = await client.GameHistorySettledAsync(
			apiUrl: command.ApiUrl,
			apiKey: command.ApiKey,
			agentName: command.AgentName,
			cancellationToken: cancellationToken)
			.ToArrayAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? Enumerable.Empty<GameHistorySettled>();

			logger.LogInformation("LogOn:{logOn} | Action:GameHistorySettledAsync | Response:{response}",
				DateTime.UtcNow.ToString("O"),
				gameRecords);

			if (!gameRecords.Any())
			{
				return;
			}

			var marks = new List<string>();

			foreach (var gameRecord in gameRecords)
			{
				try
				{
					await service
						.ExecuteAsync(
							command: new(gameRecord),
							cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					marks.Add(gameRecord.ExternalRoundId);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message: game record exception.({gameRecord})", DateTime.Now.ToString("o"), gameRecord);
				}
			}

			if (marks.Count != 0)
			{
				_ = await client
					.GameHistoryMarkAsync(
						apiUrl: command.ApiUrl,
						apiKey: command.ApiKey,
						agentName: command.AgentName,
						externalRoundIds: marks,
						cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "LogOn:{logOn} Message: An exception occurs.", DateTime.Now.ToString("o"));
		}
		finally
		{
			_ = await pullRecordLock.UnlockedAsync(
				agentName: command.AgentName)
				.ConfigureAwait(false);
		}
	}
}
