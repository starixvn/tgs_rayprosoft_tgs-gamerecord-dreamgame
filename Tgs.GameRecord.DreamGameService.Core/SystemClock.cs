namespace Tgs.GameRecord.DreamGameService.Core;

internal class SystemClock : ISystemClock
{
    public DateTimeOffset CurrentTime => DateTimeOffset.UtcNow;
}
