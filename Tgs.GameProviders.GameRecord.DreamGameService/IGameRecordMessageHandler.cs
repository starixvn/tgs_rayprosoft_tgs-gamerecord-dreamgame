namespace Tgs.GameProviders.GameRecord.DreamGameService
{
    public interface IGameRecordMessageHandler
    {
        ValueTask SendGameRoundDetailAsync(GameRoundDetail data);
    }
}
