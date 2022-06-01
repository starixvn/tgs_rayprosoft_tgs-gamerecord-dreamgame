using Tgs.GameRounds.Common;

namespace Tgs.GameRecord.DreamGameService
{
    public record GameRoundData
    {
        public string ExternalRoundId { get; init; } = default!;
        public Guid GameProviderId { get; init; }
        public string GameProviderCode { get; set; } = default!;
        public string PlayerCode { get; init; } = default!;
        public int GameId { get; init; }
        public string ExternalGameId { get; init; } = default!;
        public string CurrencyCode { get; init; } = default!;
        public GameRoundStatus Status { get; init; }
        public Guid GameProviderConfigurationId { get; init; }
        public Guid WalletId { get; init; }
        public DateTimeOffset CreatedOn { get; init; }
        public DateTimeOffset? ClosedOn { get; init; }
        public DateTimeOffset UpdatedOn { get; init; }
        public DateTimeOffset? ExternalCreatedOn { get; init; }
        public DateTimeOffset? ExternalClosedOn { get; init; }
        public Guid BrandId { get; init; }
        public decimal? BetAmount { get; init; }
        public decimal? ValidBetAmount { get; init; }
        public decimal? SettleAmount { get; init; }
        public decimal? Commission { get; init; }
        public decimal? Turnover { get; init; }
        public decimal? PlayerWinLoss { get; init; }
        public decimal? TieBetAdjustment { get; init; }
        public string AdditionalData { get; init; } = default!;
        public GameRoundClientStatus ClientStatus { get; init; }
        public bool Success { get; init; }
    }
}
