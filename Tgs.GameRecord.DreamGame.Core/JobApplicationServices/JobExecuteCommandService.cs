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
		var message = new DreamGameGameRecordMessage
		{
			ApiUrl = command.ApiUrl,
			ApiKey = command.ApiKey,
			AgentName = command.AgentName,
		};

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = DreamGameNatsConst.SchedulePublish,
				Data = new
				{
					message.ApiUrl,
					message.ApiKey,
					message.AgentName,
				},
				Message = "This message publishing handler."
			}));

		return messageSender.PublishAsync(
			subject: DreamGameNatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(message),
			cancellationToken: cancellationToken);
	}
}
