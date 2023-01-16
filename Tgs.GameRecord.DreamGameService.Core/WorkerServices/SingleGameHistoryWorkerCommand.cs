namespace Tgs.GameRecord.DreamGameService.Core.WorkerServices;

public record SingleGameHistoryWorkerCommand(
    Uri ApiUrl,
    string ApiKey,
    string AgentName);
