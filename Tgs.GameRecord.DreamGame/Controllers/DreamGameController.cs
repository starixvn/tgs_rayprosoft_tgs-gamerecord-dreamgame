using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.DreamGame.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.DreamGame.Controllers;

[Route("[controller]")]
[ApiController]
public class DreamGameController : ControllerBase
{
	/// <summary>
	/// Games the record manual asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("manual")]
	public ValueTask GameRecordManualAsync(
		[FromServices] IMessageSender messageSender,
		DreamGameGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: DreamGameNatsConst.ManualPublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record schedule asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("schedule")]
	public ValueTask GameRecordScheduleAsync(
		[FromServices] IMessageSender messageSender,
		DreamGameGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: DreamGameNatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);
}
