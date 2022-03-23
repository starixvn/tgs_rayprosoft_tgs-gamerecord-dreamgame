namespace Tgs.GameProviders.GameRecord.DreamGameService
{
    public interface IGameRecordMessageHandler
    {
        ValueTask SendGameRoundAsync(GameRoundData data);
        ValueTask SendGameRoundDetailAsync(GameRoundDetail data);
    }
}
