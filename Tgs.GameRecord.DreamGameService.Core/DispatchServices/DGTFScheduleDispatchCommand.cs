namespace Tgs.GameRecord.DreamGameService.Core.DispatchServices;

public record DGTFScheduleDispatchCommand(
    Uri ApiUrl,
    string ApiKey,
    string AgentName);
