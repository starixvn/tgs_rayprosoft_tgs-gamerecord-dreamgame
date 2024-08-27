using Tgs.GameRecord.DreamGame.Core.ApplicationServices;

namespace Tgs.GameRecord.DreamGame.Core;

public interface IPullRecordDispatcher
{
	ValueTask PushCommandAsync(PullRecordCommand command, CancellationToken cancellationToken = default);
}