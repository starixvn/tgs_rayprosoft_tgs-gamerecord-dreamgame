namespace Tgs.GameRecord.DreamGameService.Core.WorkerServices;

public record TransferGameHistoryWorkerCommand(
    Uri ApiUrl,
    string ApiKey,
    string AgentName);
