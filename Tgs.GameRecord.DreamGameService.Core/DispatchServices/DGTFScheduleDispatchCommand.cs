namespace Tgs.GameRecord.DreamGameService.Core.DispatchServices;

public record ScheduleDispatchCommand(
    Uri ApiUrl,
    string ApiKey,
    string AgentName);
