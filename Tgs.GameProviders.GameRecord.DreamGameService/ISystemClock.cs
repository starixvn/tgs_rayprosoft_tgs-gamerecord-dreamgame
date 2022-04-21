namespace Tgs.GameProviders.GameRecord.DreamGameService
{
    public interface ISystemClock
    {
        DateTimeOffset CurrentTime { get; }
    }
}
