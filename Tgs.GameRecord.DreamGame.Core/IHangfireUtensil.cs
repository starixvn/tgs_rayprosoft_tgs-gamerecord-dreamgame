using Tgs.GameRecord.DreamGame.Core.JobApplicationServices;

namespace Tgs.GameRecord.DreamGame.Core;

public interface IHangfireUtensil
{
	public Task JobCheckAsync();

	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}
