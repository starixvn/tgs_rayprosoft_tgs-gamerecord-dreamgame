namespace Tgs.GameRecord.DreamGameService.ReceiveHandlers
{
    public record DreamGameCommand(string GameProviderCode, DateTimeOffset SendDateTime);
}
