using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.DreamGamingService.Client;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.WorkerServices;

internal class GameHistoryWorkerService : ICommandService<GameHistoryWorkerCommand>
{
    private readonly IGameInterfaceDreamGamingClient _client;
    private readonly ILogger<GameHistoryWorkerService> _logger;
    private readonly ICommandService<GamePullRecordSettledCommand> _settledService;

    public GameHistoryWorkerService(
        IGameInterfaceDreamGamingClient client,
        ILogger<GameHistoryWorkerService> logger,
        ICommandService<GamePullRecordSettledCommand> settledService)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settledService = settledService ?? throw new ArgumentNullException(nameof(settledService));
    }

    public async ValueTask ExecuteAsync(GameHistoryWorkerCommand command, CancellationToken cancellationToken = default)
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

            _logger.LogInformation("LogOn:{logOn} | Action:GameHistorySettledAsync | Response:{response}",
                DateTime.UtcNow.ToString("O"),
                JsonSerializer.Serialize(gameHistorySettleds));

            await _settledService
                .ExecuteAsync(
                    command: new GamePullRecordSettledCommand(
                        Records: gameHistorySettleds),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LogOn:{logOn} | Action:GameHistorySettledAsync | Message:DreamGaming Transfer處理結算過程中發生例外", DateTime.UtcNow.ToString("O"));
        }
    }
}