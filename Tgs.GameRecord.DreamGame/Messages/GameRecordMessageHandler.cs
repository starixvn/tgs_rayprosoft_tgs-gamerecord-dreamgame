using System.Text;
using System.Text.Json;
using Tgs.GameRecord.DreamGame.Common;
using Tgs.GameRecord.DreamGame.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.DreamGame.Messages;

internal class GameRecordMessageHandler(
   ILogger<GameRecordMessageHandler> logger,
   ICommandService<PullRecordCommand> service) : IMessageHandler
{
    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<DreamGameGameRecordMessage>(json)
            ?? throw new NullReferenceException("The request data resolves to a null value");

        logger.LogInformation("LogOn:{logOn} Information Request:{message}", DateTime.Now.ToString("o"), message);

        var command = new PullRecordCommand(
            ApiUrl: message.ApiUrl,
            ApiKey: message.ApiKey,
            AgentName: message.AgentName);

        return service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}
