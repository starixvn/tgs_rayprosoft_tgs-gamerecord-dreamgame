namespace Tgs.GameRecord.DreamGame.Core;

public record RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
}
