namespace Tgs.GameRecord.DreamGame.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public string HangfirePrefix { get; set; } = "gamerecord-dg-hangfire:";
    public string HangfireLocking { get; set; } = "gamerecord-dg-locking:";
    public double LockMinute { get; set; } = 5d;
}
