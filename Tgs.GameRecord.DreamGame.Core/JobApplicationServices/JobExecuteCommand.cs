namespace Tgs.GameRecord.DreamGame.Core.JobApplicationServices;

public record JobExecuteCommand(
    string GameProviderCode,
    string Name,
    Uri ApiUrl,
    string ApiKey,
    string AgentName);
