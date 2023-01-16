namespace Tgs.GameRecord.DreamGameService.Core;

public record DGTFGameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameExternalId { get; set; } = default!;
    public double IntervalMinutes { get; set; } = 5d;
    public double IntervalMinutesMax { get; set; } = 1440;
    public double IntervalMinutesMinimum { get; set; } = 2;
    public double LockMinute { get; set; } = 4d;
}
