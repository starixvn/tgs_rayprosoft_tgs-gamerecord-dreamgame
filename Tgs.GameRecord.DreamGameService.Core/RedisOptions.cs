namespace Tgs.GameRecord.DreamGameService.Core;

public record RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
}
