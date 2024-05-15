using System.Text;
using System.Text.Json;
using Tgs.GameRecord.DreamGame.Common;
using Tgs.GameRecord.DreamGame.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.DreamGame.Messages;

internal class ManualMessageHandler(
   ILogger<ManualMessageHandler> logger,
   ICommandService<PullRecordCommand> service) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<DreamGameGameRecordMessage>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{message}",
			DateTime.Now.ToString("o"),
			subject,
			message);

		var command = new PullRecordCommand(
			ApiUrl: message.ApiUrl,
			ApiKey: message.ApiKey,
			AgentName: message.AgentName);

		return service.ExecuteAsync(
			command: command,
			cancellationToken: cancellationToken);
	}
}
