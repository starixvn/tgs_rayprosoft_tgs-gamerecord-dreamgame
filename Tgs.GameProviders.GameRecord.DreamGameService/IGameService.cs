namespace Tgs.GameProviders.GameRecord.DreamGameService
{
    public interface IGameService
    {
        ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId);
    }
}
