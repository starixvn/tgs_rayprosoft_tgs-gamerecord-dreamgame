namespace Tgs.GameRecord.DreamGameService.Core.WorkerServices;

public record GameHistoryWorkerCommand(
    Uri ApiUrl,
    string ApiKey,
    string AgentName);