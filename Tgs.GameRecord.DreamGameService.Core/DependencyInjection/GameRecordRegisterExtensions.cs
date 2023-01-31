using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.GameRecord.DreamGameService.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class GameRecordRegisterExtensions
{
    public static async Task<WebApplication> UseSingleGameRecordRegister(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var gameRecordInfo = app.Configuration.GetSection("GameRecordInfo").Get<GameRecordInfoOptions>();
            var utensil = scope.ServiceProvider.GetRequiredService<IUtensil>();
            var gameProviderId = await utensil.GetGameProviderIdAsync().ConfigureAwait(false);

            _ = await utensil
                .GetGameIdAsync(
                    gameProviderId,
                    gameRecordInfo.GameExternalId)
                .ConfigureAwait(false);
        }

        return app;
    }

    public static async Task<WebApplication> UseTransferGameRecordRegister(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var gameRecordInfo = app.Configuration.GetSection("GameRecordInfo").Get<GameRecordInfoOptions>();
            var utensil = scope.ServiceProvider.GetRequiredService<ITransferUtensil>();
            var gameProviderId = await utensil.GetGameProviderIdAsync().ConfigureAwait(false);
            var clinet = scope.ServiceProvider.GetRequiredService<IGameCommandClient>();

            var games = await clinet
                .GameQueryAsync(gameRecordInfo.GameProviderCodeTransfer)
                .ToArrayAsync()
                .ConfigureAwait(false)
                ?? Enumerable.Empty<GameInfo>();

            foreach (var game in games)
            {
                _ = await utensil
                        .GetGameIdAsync(
                            gameProviderId,
                            game.GameExternalId)
                        .ConfigureAwait(false);
            }
        }

        return app;
    }
}
