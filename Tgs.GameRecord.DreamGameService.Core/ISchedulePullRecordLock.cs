namespace Tgs.GameRecord.DreamGameService.Core;

public interface ISchedulePullRecordLock
{
    public Task<bool> LockingAsync(string agentName);

    public Task<bool> UnlockAsync(string agentName);
}
