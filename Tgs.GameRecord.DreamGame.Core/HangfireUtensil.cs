using Tgs.GameRecord.DreamGame.Core.JobApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGame.Core;

internal class HangfireUtensil : IHangfireUtensil
{
    private readonly ICommandService<JobCheckCommand> _jobCheckService;
    private readonly ICommandService<JobExecuteCommand> _jobExecuteService;
    private readonly ICommandService<JobInitCommand> _jobInitservice;

    public HangfireUtensil(
        ICommandService<JobCheckCommand> jobCheckService,
        ICommandService<JobExecuteCommand> jobExecuteService,
        ICommandService<JobInitCommand> jobInitservice
        )
    {
        _jobCheckService = jobCheckService ?? throw new ArgumentNullException(nameof(jobCheckService));
        _jobExecuteService = jobExecuteService ?? throw new ArgumentNullException(nameof(jobExecuteService));
        _jobInitservice = jobInitservice ?? throw new ArgumentNullException(nameof(jobInitservice));
    }

    public Task JobCheckAsync()
        => _jobCheckService.ExecuteAsync(new()).AsTask();

    public Task JobExecuteAsync(JobExecuteCommand source)
        => _jobExecuteService.ExecuteAsync(source).AsTask();

    public Task JobInitAsync()
        => _jobInitservice.ExecuteAsync(new()).AsTask();
}
