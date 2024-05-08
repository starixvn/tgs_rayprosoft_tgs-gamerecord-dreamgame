using Tgs.GameInterface.DreamGamingService.Client;
using Tgs.GameRecord.DreamGame.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegisterExtensions
{
	public static async Task<WebApplication> UseHangfireRegisterAsync(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			var sp = scope.ServiceProvider;
			var hangfireSchedule = scope.ServiceProvider.GetRequiredService<IHangfireUtensil>();
			await hangfireSchedule.JobInitAsync().ConfigureAwait(false);
		}

		return app;
	}

	public static async Task<WebApplication> UseRegister(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			var client = scope.ServiceProvider.GetRequiredService<IGameInterfaceDreamGamingClient>();

			_ = await client
				.CheckingServiceAsync(
				machineName: Environment.MachineName)
				.ConfigureAwait(false);
		}

		return app;
	}
}
