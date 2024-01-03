using Tgs.GameRecord.DreamGame.Core;
namespace Tgs.GameRecord.DreamGame;

public record RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
}
