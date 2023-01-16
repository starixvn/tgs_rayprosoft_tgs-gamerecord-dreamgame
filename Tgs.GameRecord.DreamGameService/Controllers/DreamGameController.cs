using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.DreamGameService.Core.DispatchServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Controllers;

[Route("[controller]")]
[ApiController]
public class DreamGameController : ControllerBase
{
    [HttpPost]
    public ValueTask GameRecordAsync(
       [FromServices] ICommandService<SingleScheduleDispatchCommand> commandService)

    => commandService.ExecuteAsync(new());
}
