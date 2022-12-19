using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.DreamGameService.Core;

public interface IPlayerService
{
    ValueTask<PlayerModel?> GetAsync(
        string playerCode,
        CancellationToken cancellationToken = default);
}
