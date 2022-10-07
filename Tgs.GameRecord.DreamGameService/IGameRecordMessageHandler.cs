using Tgs.GameRounds.Common.MessageModels;

namespace Tgs.GameRecord.DreamGameService
{
    public interface IGameRecordMessageHandler
    {
        ValueTask SendGameRoundAsync(GameRecordMessage data);
    }
}
