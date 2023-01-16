namespace Tgs.GameRecord.DreamGameService.Core;

public record NatsSettingOptions
{
    public string Group { get; set; } = default!;
    public string SingleSubject { get; set; } = default!;
    public string TransferSubject { get; set; } = default!;
}
