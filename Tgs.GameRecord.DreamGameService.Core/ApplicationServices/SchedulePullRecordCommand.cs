namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

public record SchedulePullRecordCommand(
    Uri ApiUrl,
    string ApiKey,
    string AgentName);
