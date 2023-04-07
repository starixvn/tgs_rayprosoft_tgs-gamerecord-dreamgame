using Microsoft.Extensions.Logging;
using Tgs.GameInterface.DreamGamingService.Client;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

internal class SchedulePullRecordCommandService : ICommandService<SchedulePullRecordCommand>
{
    private readonly IGameInterfaceDreamGamingClient _client;
    private readonly ILogger<SchedulePullRecordCommandService> _logger;
    private readonly ICommandService<ScheduleHandleRecordCommand> _service;

    public SchedulePullRecordCommandService(
        ILogger<SchedulePullRecordCommandService> logger,
        IGameInterfaceDreamGamingClient client,
        ICommandService<ScheduleHandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default)
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

        foreach (var gameRecord in gameRecords)
        {
            try
            {
                await _service
                    .ExecuteAsync(
                        command: new(gameRecord),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "遊戲紀錄處理發生例外.({gameRecord})", gameRecord);
            }
        }
    }
}
