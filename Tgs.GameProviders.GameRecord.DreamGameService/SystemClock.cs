namespace Tgs.GameProviders.GameRecord.DreamGameService
{
    internal class SystemClock : ISystemClock
    {
        public DateTimeOffset CurrentTime => DateTimeOffset.UtcNow;
    }
}
