using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using Tgs.GameRecord.DreamGameService.ApplicationServices;

namespace Tgs.GameRecord.DreamGameService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DreamGameController : ControllerBase
    {
        [HttpPost]
        public ValueTask GameRecordAsync(
           [FromServices] ICommandService<GameRecordCommand> commandService)

        => commandService.ExecuteAsync(new());
    }
}
