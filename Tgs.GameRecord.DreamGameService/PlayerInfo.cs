namespace Tgs.GameRecord.DreamGameService
{
    public record PlayerInfo(Guid BrandId, string PlayerCode, string DefaultWalletCurrencyCode, Guid DefaultWalletId, Guid GameProviderConfigurationId);
}
