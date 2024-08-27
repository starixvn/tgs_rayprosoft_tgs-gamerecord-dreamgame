using Tgs.GameRecord.DreamGame.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegisterExtensions
{
	public static async Task<WebApplication> UseHangfireRegisterAsync(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			var sp = scope.ServiceProvider;
			var hangfireSchedule = scope.ServiceProvider.GetRequiredService<IHangfireUtensilRepository>();
			await hangfireSchedule.JobInitAsync().ConfigureAwait(false);
		}

		return app;
	}
}
