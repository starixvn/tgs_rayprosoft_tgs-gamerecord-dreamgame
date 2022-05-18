namespace Tgs.GameRecord.DreamGameService
{
    public interface IGameProviderService
    {
        public ValueTask<Guid?> GameProviderGetGameProviderIdAsync();
    }
}
