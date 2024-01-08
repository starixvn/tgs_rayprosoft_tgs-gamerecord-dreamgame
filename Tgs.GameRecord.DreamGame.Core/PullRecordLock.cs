using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Tgs.GameRecord.DreamGame.Core;

internal class PullRecordLock(
    IOptions<GameRecordInfoOptions> options,
    IDatabase database) : IPullRecordLock
{
    private readonly GameRecordInfoOptions _options = options.Value;

    public Task<bool> LockingAsync(string agentName)
        => database.StringSetAsync(key: $"gamerecord-dgtf-pull-locking:{agentName}", value: agentName, expiry: TimeSpan.FromMinutes(_options.LockMinute), when: When.NotExists);

    public Task<bool> UnlockedAsync(string agentName)
        => database.KeyDeleteAsync(key: $"gamerecord-dgtf-pull-locking:{agentName}", flags: CommandFlags.FireAndForget);
}
