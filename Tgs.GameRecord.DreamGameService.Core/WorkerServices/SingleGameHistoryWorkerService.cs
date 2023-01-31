using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.DreamGamingService.Client;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.WorkerServices;

internal class SingleGameHistoryWorkerService : ICommandService<SingleGameHistoryWorkerCommand>
{
    private readonly IGameInterfaceDreamGamingClient _client;
    private readonly ILogger<SingleGameHistoryWorkerService> _logger;
    private readonly ICommandService<SingleGamePullRecordCommand> _service;
    private readonly ISystemClock _systemClock;

    public SingleGameHistoryWorkerService(
        ILogger<SingleGameHistoryWorkerService> logger,
        IGameInterfaceDreamGamingClient client,
        ICommandService<SingleGamePullRecordCommand> service,
        ISystemClock systemClock)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
    }

    public async ValueTask ExecuteAsync(SingleGameHistoryWorkerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var gameHistorySettleds = await _client
                    .GameHistorySettledAsync(
                        apiUrl: command.ApiUrl,
                        apiKey: command.ApiKey,
                        agentName: command.AgentName,
                        cancellationToken: cancellationToken)
                    .ToArrayAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            _logger.LogInformation("LogOn:{logOn} | Response:{response}",
                DateTime.UtcNow.ToString("O"),
                JsonSerializer.Serialize(gameHistorySettleds));

            var gameRecords = gameHistorySettleds
                .Where(x => x.ExternalClosedOn.ToDateTimeOffset().UtcDateTime < _systemClock.CurrentTime.UtcDateTime.AddMinutes(-3))
                ?? Enumerable.Empty<GameHistorySettled>();

            if (!gameRecords.Any())
            {
                return;
            }

            _logger.LogInformation("LogOn:{logOn} | Response:{response}",
                DateTime.UtcNow.ToString("O"),
                JsonSerializer.Serialize(gameHistorySettleds));

            var settledCommand = new SingleGamePullRecordCommand(gameRecords);

            await _service
                .ExecuteAsync(
                    command: settledCommand,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            /*
            var externalRoundIds = gameHistorySettleds.Select(x => x.ExternalRoundId).ToArray();

            if (externalRoundIds.Any())
            {
                _ = await _client
                    .GameHistoryMarkAsync(
                        apiUrl: command.ApiUrl,
                        apiKey: command.ApiKey,
                        agentName: command.AgentName,
                        externalRoundIds: externalRoundIds,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LogOn:{logOn} | Message:DreamGaming Single處理結算過程中發生例外", DateTime.UtcNow.ToString("O"));
        }
    }
}
