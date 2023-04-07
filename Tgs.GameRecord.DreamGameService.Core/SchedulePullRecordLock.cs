using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Tgs.GameRecord.DreamGameService.Core;

internal class SchedulePullRecordLock : ISchedulePullRecordLock
{
    private readonly IDatabase _database;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public SchedulePullRecordLock(
        IOptions<GameRecordInfoOptions> options,
        IDatabase database)
    {
        _gameRecordInfo = options.Value;
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public Task<bool> LockingAsync(string agentName)
        => _database.StringSetAsync(key: $"{_gameRecordInfo.GameProviderCode.ToLower()}:schedulepullrecord:{agentName}", value: agentName, expiry: TimeSpan.FromMinutes(_gameRecordInfo.LockMinute), when: When.NotExists);

    public Task<bool> UnlockAsync(string agentName)
        => _database.KeyDeleteAsync(key: $"{_gameRecordInfo.GameProviderCode.ToLower()}:schedulepullrecord:{agentName}");
}
