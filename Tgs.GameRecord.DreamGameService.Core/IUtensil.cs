using Tgs.GameCommand.Common;

namespace Tgs.GameRecord.DreamGameService.Core;

public interface IUtensil
{
    ValueTask<Guid?> BrandGetGameProviderConfigurationIdAsync(
        Guid brandId,
        Guid gameProviderId,
        CancellationToken cancellation = default);

    ValueTask<int> GetGameIdAsync(
        Guid gameProviderId,
        string gameExternalId,
        CancellationToken cancellation = default);

    ValueTask<Guid> GetGameProviderIdAsync(
        CancellationToken cancellation = default);

    ValueTask<PlayerInfo?> GetPlayerAsync(
        string playerCode,
        CancellationToken cancellation = default);
}
