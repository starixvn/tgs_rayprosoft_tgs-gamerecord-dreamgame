namespace Tgs.GameRecord.DreamGameService
{
    public interface ISystemClock
    {
        DateTimeOffset CurrentTime { get; }
    }
}
