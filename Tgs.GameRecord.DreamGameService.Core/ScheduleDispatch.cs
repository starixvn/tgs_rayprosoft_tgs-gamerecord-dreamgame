using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tgs.GameRecord.DreamGameService.Core.DispatchServices;

namespace Tgs.GameRecord.DreamGameService.Core;

internal class ScheduleDispatch : IScheduleDispatch
{
    private readonly IDatabase _database;
    private readonly TimeSpan _expiry;
    private readonly DGTFGameRecordInfoOptions _gameRecordInfoOptions;

    public ScheduleDispatch(
        IOptions<DGTFGameRecordInfoOptions> options,
        IDatabase database)
    {
        _gameRecordInfoOptions = options.Value;
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _expiry = TimeSpan.FromMinutes(_gameRecordInfoOptions.LockMinute);
    }

    public Task<bool> ExistAsync(string key)
        => _database.KeyExistsAsync(key: key);

    public Task<bool> LockingAsync(string key, ScheduleDispatchCommand command)
        => _database.StringSetAsync(key: key, value: JsonSerializer.Serialize(command), expiry: _expiry, when: When.NotExists);

    public async Task UnlockAsync(string key)
    {
        _ = await _database
                .KeyDeleteAsync(
                    key: key,
                    flags: CommandFlags.FireAndForget)
                .ConfigureAwait(false);
    }
}
