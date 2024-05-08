namespace Tgs.GameRecord.DreamGame.Core;

public record GameRecordInfoOptions
{
	public string GameProviderCode { get; set; } = default!;
	public string GameProviderConfiguration { get; set; } = default!;
	public string HangfirePrefix { get; set; } = "gamerecord-dgtf-hangfire:";
	public string HangfireLocking { get; set; } = "gamerecord-dgtf-locking:";
	public double LockMinute { get; set; } = 5d;
}
