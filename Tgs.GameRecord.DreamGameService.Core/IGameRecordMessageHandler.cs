using Tgs.GameRounds.Common.MessageModels;

namespace Tgs.GameRecord.DreamGameService.Core;

public interface IGameRecordMessageHandler
{
    ValueTask SendGameRoundAsync(GameRecordMessage data);
}
