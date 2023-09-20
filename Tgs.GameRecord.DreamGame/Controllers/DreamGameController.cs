using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.DreamGame.Core.ApplicationServices;
using Tgs.GameRecord.DreamGame.Models;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGame.Controllers;

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
