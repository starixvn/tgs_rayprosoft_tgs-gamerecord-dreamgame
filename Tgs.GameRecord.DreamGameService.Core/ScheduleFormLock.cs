using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Tgs.GameRecord.DreamGameService.Core;

internal class ScheduleFormLock : IScheduleFormLock
{
    private readonly IDatabase _database;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public ScheduleFormLock(
        IOptions<GameRecordInfoOptions> options,
        IDatabase database)
    {
        _gameRecordInfo = options.Value;
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public Task<bool> LockingAsync(long id)
        => _database.StringSetAsync(key: $"{_gameRecordInfo.GameProviderCode.ToLower()}:scheduleform:{id}", value: id, expiry: TimeSpan.FromMinutes(1), when: When.NotExists);

    public Task<bool> UnlockAsync(long id)
        => _database.KeyDeleteAsync(key: $"{_gameRecordInfo.GameProviderCode.ToLower()}:scheduleform:{id}");
}
