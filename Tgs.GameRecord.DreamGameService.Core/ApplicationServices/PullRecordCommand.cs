namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

public record PullRecordCommand(
    Uri ApiUrl,
    string ApiKey,
    string AgentName);
