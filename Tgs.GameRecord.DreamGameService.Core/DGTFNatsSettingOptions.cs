namespace Tgs.GameRecord.DreamGameService.Core;

public record DGTFNatsSettingOptions
{
    public string Group { get; set; } = default!;
    public string Subject { get; set; } = default!;
}
