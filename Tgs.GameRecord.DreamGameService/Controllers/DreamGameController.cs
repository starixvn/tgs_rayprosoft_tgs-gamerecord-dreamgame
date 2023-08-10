using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.GameRecord.DreamGameService.Models;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Controllers;

[Route("[controller]")]
[ApiController]
public class DreamGameController : ControllerBase
{
    [HttpPost]
    public ValueTask GameRecordAsync(
       [FromServices] ICommandService<PullRecordCommand> commandService,
       [FromBody] SchedulePullRecordViewModel source)
        => commandService
        .ExecuteAsync(
            command: new PullRecordCommand(
                    source.ApiUrl,
                    source.ApiKey,
                    source.AgentName),
            cancellationToken: HttpContext.RequestAborted);
}
