using Microsoft.Extensions.Logging;
using Tgs.GameRounds.Schedule.Client;
using Tgs.GameRounds.Schedule.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGame.Core.JobApplicationServices;

internal class JobExecuteCommandService : ICommandService<JobExecuteCommand>
{
    private readonly IGameRoundsScheduleClient _client;
    private readonly ILogger<JobExecuteCommandService> _logger;

    public JobExecuteCommandService(
        ILogger<JobExecuteCommandService> logger,
        IGameRoundsScheduleClient client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            command.GameProviderCode,
            command.ApiUrl,
            command.ApiKey,
            command.AgentName,
        };

        _logger.LogDebug("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

        return _client
            .Dreamgame_ScheduleFormQueuedAsync(
            gameProviderCode: command.GameProviderCode,
            name: command.Name,
            subject: "gp.gamerecord.scheduleform.dg.queued",
            source: ScheduleFormSource.Schedule,
            apiUrl: parameters.ApiUrl,
            apiKey: parameters.ApiKey,
            agentName: parameters.AgentName,
            cancellationToken: cancellationToken);
    }
}
