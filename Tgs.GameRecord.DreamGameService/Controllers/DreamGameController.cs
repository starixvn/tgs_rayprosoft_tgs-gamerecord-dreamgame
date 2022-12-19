using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Controllers;

[Route("[controller]")]
[ApiController]
public class DreamGameController : ControllerBase
{
    [HttpPost]
    public ValueTask GameRecordAsync(
       [FromServices] ICommandService<GameRecordCommand> commandService)

    => commandService.ExecuteAsync(new());
}
