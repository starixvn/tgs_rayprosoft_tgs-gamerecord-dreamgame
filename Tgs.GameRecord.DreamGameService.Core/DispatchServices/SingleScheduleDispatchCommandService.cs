using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;
using Tgs.GameRecord.DreamGameService.Core.WorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.Core.DispatchServices;

internal class SingleScheduleDispatchCommandService : ICommandService<SingleScheduleDispatchCommand>
{
    private readonly IGameCommandClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ICommandService<SingleGameHistoryWorkerCommand> _service;

    public SingleScheduleDispatchCommandService(
      IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
      IGameCommandClient client,
      ICommandService<SingleGameHistoryWorkerCommand> service)
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(SingleScheduleDispatchCommand command, CancellationToken cancellationToken = default)
    {
        var settingConfigurationJson = JsonNode.Parse(await _client.SettingGetConfigurationStringAsync(_gameRecordInfo.GameProviderConfig).ConfigureAwait(false)
            ?? throw new Exception($"This game product setting does not exist.({_gameRecordInfo.GameProviderConfig})"));

        if (settingConfigurationJson == null)
        {
            throw new Exception($"This game product setting does not exist.({_gameRecordInfo.GameProviderConfig})");
        }

        var apiUrl = new Uri(settingConfigurationJson["ApiUrl"]?.ToString()
            ?? throw new Exception($"Required parameters(ApiUrl)"));
        var apiKey = settingConfigurationJson["ApiKey"]?.ToString()
            ?? throw new Exception($"Required parameters(ApiKey)");
        var agentName = settingConfigurationJson["AgentName"]?.ToString()
            ?? throw new Exception($"Required parameters(AgentName)");

        var workerCommand = new SingleGameHistoryWorkerCommand(
            ApiUrl: apiUrl,
            ApiKey: apiKey,
            AgentName: agentName);

        await _service
             .ExecuteAsync(
                 command: workerCommand,
                 cancellationToken: cancellationToken)
             .ConfigureAwait(false);
    }
}
