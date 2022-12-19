using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.DreamGameService.Core;

public interface IGameService
{
    ValueTask<GameModel?> GetGameAsync(Guid gameProviderId, string gameExternalId);

    ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId);
}
