using System.Reflection;
using Tgs.GameRecord.DreamGame.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameRecordCore(this IServiceCollection services)
		=> services
		.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
		.AddSingleton<IPullRecordDispatcher, PullRecordDispatcher>();
}
