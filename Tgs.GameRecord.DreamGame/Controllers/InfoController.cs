using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.DreamGame.Core;

namespace Tgs.GameRecord.DreamGame.Controllers;

[Route("[controller]")]
[ApiController]
public class InfoController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public InfoController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpGet]
    public async Task<object> GetAsync([FromServices] IWebHostEnvironment _hostingEnvironment)
    {
        var hostName = Dns.GetHostName();
        var hostEntry = await Dns.GetHostEntryAsync(hostName);
        var hostIp = Array.Find(hostEntry.AddressList,
            x => x.AddressFamily == AddressFamily.InterNetwork);
        var gameRecordInfo = _configuration.GetSection("GameRecordInfo").Get<GameRecordInfoOptions>();

        return new
        {
            Environment.MachineName,
            HostName = hostName,
            HostIp = hostIp?.ToString() ?? string.Empty,
            Environment = _hostingEnvironment.EnvironmentName,
            OsVersion = $"{Environment.OSVersion}",
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
            ProcessCount = Environment.ProcessorCount,
            gameRecordInfo,
        };
    }
}
