namespace Tgs.GameRecord.DreamGameService;

public record NatsSettingOptions
{
    public string Group { get; init; } = default!;

    public string Subject { get; set; } = default!;
}
