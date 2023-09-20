using Microsoft.Extensions.Logging;
using Tgs.GameInterface.DreamGamingService.Client;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGame.Core.ApplicationServices;

internal class PullRecordCommandService : ICommandService<PullRecordCommand>
{
    private readonly IGameInterfaceDreamGamingClient _client;
    private readonly ILogger<PullRecordCommandService> _logger;
    private readonly ICommandService<HandleRecordCommand> _service;

    public PullRecordCommandService(
        ILogger<PullRecordCommandService> logger,
        IGameInterfaceDreamGamingClient client,
        ICommandService<HandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(PullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameRecords = await _client
                .GameHistorySettledAsync(
                    apiUrl: command.ApiUrl,
                    apiKey: command.ApiKey,
                    agentName: command.AgentName,
                    cancellationToken: cancellationToken)
                .ToArrayAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? Enumerable.Empty<GameHistorySettled>();

        _logger.LogInformation("LogOn:{logOn} | Action:GameHistorySettledAsync | Response:{response}",
            DateTime.UtcNow.ToString("O"),
            gameRecords);

        if (!gameRecords.Any())
        {
            return;
        }

        var marks = new List<string>();

        foreach (var gameRecord in gameRecords)
        {
            try
            {
                await _service
                    .ExecuteAsync(
                        command: new(gameRecord),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                marks.Add(gameRecord.ExternalRoundId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LogOn:{logOn} Message: game record exception.({gameRecord})", DateTime.Now.ToString("o"), gameRecord);
            }
        }

        if (marks.Any())
        {
            _ = await _client
                .GameHistoryMarkAsync(
                    apiUrl: command.ApiUrl,
                    apiKey: command.ApiKey,
                    agentName: command.AgentName,
                    externalRoundIds: marks,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
