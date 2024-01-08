namespace Tgs.GameRecord.DreamGame.Core;

public interface IPullRecordLock
{
    public Task<bool> LockingAsync(string agentName);

    public Task<bool> UnlockedAsync(string agentName);
}
