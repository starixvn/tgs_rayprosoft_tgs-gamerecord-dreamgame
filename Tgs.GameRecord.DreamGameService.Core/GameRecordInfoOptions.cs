namespace Tgs.GameRecord.DreamGameService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;

    public string GameProviderConfig { get; set; } = default!;

    public string GameProviderCodeTransfer { get; set; } = default!;

    public string GameProviderConfigTransfer { get; set; } = default!;

    public string GameExternalId { get; set; } = default!;

    public double LockMinute { get; set; } = 4d;
}
