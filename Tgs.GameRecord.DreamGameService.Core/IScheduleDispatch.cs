using Tgs.GameRecord.DreamGameService.Core.DispatchServices;

namespace Tgs.GameRecord.DreamGameService.Core;

public interface IScheduleDispatch
{
    public Task<bool> ExistAsync(string key);

    public Task<bool> LockingAsync(string key, ScheduleDispatchCommand command);

    public Task UnlockAsync(string key);
}
