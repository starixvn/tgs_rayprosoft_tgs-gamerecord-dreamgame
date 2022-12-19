namespace Tgs.GameRecord.DreamGameService.Core;

public interface IGameProviderService
{
    public ValueTask<Guid?> GameProviderGetGameProviderIdAsync();
}
