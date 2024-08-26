using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.DreamGame.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.DreamGame.Controllers;

[Route("[controller]")]
[ApiController]
public class DreamGameController : ControllerBase
{
	[HttpPost]
	public ValueTask GameRecordAsync(
		[FromServices] IMessageSender messageSender,
		DreamGameGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: DreamGameNatsConst.ManualPublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);
}
