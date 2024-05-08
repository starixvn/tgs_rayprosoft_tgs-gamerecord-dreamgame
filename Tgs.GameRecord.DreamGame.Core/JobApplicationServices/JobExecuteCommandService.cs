using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameRecord.DreamGame.Common;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.DreamGame.Core.JobApplicationServices;

internal class JobExecuteCommandService(
	ILogger<JobExecuteCommandService> logger,
	IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
	public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
	{
		var parameters = new DreamGameGameRecordMessage
		{
			ApiUrl = command.ApiUrl,
			ApiKey = command.ApiKey,
			AgentName = command.AgentName,
		};

		logger.LogInformation("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

		return messageSender.PublishAsync(
			subject: DreamGameNatsConst.SchedulePublish,
			data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters)),
			cancellationToken: cancellationToken);
	}
}
