namespace Tgs.GameRecord.DreamGameService
{
    internal class SystemClock : ISystemClock
    {
        public DateTimeOffset CurrentTime => DateTimeOffset.UtcNow;
    }
}
