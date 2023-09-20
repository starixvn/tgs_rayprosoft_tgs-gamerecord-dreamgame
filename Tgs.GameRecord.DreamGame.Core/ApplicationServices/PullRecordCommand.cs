namespace Tgs.GameRecord.DreamGame.Core.ApplicationServices;

public record PullRecordCommand(
    Uri ApiUrl,
    string ApiKey,
    string AgentName);
