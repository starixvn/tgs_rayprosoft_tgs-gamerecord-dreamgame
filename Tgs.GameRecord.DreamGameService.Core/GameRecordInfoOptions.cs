namespace Tgs.GameRecord.DreamGameService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;

    public double LockMinute { get; set; } = 5d;
}
