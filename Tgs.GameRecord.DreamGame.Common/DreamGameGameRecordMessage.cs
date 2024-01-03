using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.DreamGame.Common;

public record DreamGameGameRecordMessage
{
    [Required]
    [DataType(DataType.Url)]
    public Uri ApiUrl { get; init; } = default!;
    [Required]
    public string ApiKey { get; init; } = default!;
    [Required]
    public string AgentName { get; init; } = default!;
}
