namespace Tgs.GameRecord.DreamGameService.Core;

public interface ISystemClock
{
    DateTimeOffset CurrentTime { get; }
}
