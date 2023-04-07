using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.DreamGameService.Models;

public class SchedulePullRecordViewModel
{
    [Required]
    public string AgentName { get; init; } = default!;

    [Required]
    public string ApiKey { get; init; } = default!;

    [Required]
    public Uri ApiUrl { get; init; } = default!;
}
