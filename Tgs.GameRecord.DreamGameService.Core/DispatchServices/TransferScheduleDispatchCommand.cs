namespace Tgs.GameRecord.DreamGameService.Core.DispatchServices;

public record TransferScheduleDispatchCommand(
    Uri ApiUrl,
    string ApiKey,
    string AgentName);
