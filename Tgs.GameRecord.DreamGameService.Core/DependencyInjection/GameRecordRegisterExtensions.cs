using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Tgs.GameRecord.DreamGameService.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class GameRecordRegisterExtensions
{
    public static async Task<WebApplication> UseGameRecordRegister(this WebApplication app)
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
}
