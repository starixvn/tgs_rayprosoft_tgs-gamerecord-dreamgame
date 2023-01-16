namespace Tgs.GameRecord.DreamGameService.Core;

public record DGTFMongoSettingOptions
{
    public string ManagerName { get; set; } = default!;
    public string QueueName { get; set; } = default!;
    public string GlobPattern { get; set; } = default!;
}
