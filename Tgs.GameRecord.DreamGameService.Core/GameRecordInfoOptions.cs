namespace Tgs.GameRecord.DreamGameService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;

    public string GameExternalId { get; set; } = default!;
}
