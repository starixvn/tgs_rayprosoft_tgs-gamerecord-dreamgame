namespace Tgs.GameRecord.DreamGame.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public string GameProviderHangfire { get; init; } = default!;

    public double LockMinute { get; set; } = 5d;
}
