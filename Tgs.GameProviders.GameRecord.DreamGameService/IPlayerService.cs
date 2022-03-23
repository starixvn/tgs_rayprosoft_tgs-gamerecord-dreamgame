namespace Tgs.GameProviders.GameRecord.DreamGameService
{
    public interface IPlayerService
    {
        ValueTask<PlayerInfo?> GetAsync(Guid gameProviderId, string playerCode);
    }
}
