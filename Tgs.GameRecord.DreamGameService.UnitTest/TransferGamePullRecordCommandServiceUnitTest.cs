using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameCommand.Common;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.MessageQueue;

namespace Tgs.GameRecord.DreamGameServiceUnitTest;

[TestClass]
public class TransferGamePullRecordCommandServiceUnitTest
{
    [TestMethod]
    public async Task Normal_Process()
    {
        var fakeOptions = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "DG",
            GameProviderConfig = "DGConfiguration",
            GameProviderCodeTransfer = "DGTF",
            GameProviderConfigTransfer = "DGTFConfiguration",
            GameExternalId = "dg"
        });
        var fakeMongoSettingOptions = Options.Create(new MongoSettingOptions
        {
            ManagerName = "gp.gamerecord.database",
            QueueName = "tgs.gp.gamerecord.database.round.dg",
            GlobPattern = "tgs.gp.gamerecord.database.round.dg"
        });
        var fakeLogger = NullLogger<TransferGamePullRecordCommandService>.Instance;
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeUtensil = Substitute.For<ITransferUtensil>();

        var command = new TransferGamePullRecordCommand(
            Records: new GameHistorySettled[] {
            new GameHistorySettled()
            {
                PlayerCode=  "test",
                ExternalRoundId="10893708376",
                IsClosed= true,
                ExternalCreatedOn=new DateTimeOffset(DateTime.Parse("2022-04-21 18:30:39"), TimeSpan.FromHours(+8)).ToTimestamp(),
                ExternalClosedOn= new DateTimeOffset(DateTime.Parse("2022-04-21 18:31:06"), TimeSpan.FromHours(+8)).ToTimestamp(),
                Bet=20m,
                ValidBet=20m,
                PlayerWinLoss=  -20.00m,
                Turnover= 20.00m,
                Settle=0.0m,
                AdditionalData=string.Empty
            }});

        var gameProviderId = Guid.NewGuid();
        _ = fakeUtensil
            .GetGameProviderIdAsync(
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var player = new PlayerInfo(
                   ResellerId: Guid.NewGuid(),
                   ResellerCode: "TGS",
                   LicenseeId: Guid.NewGuid(),
                   LicenseeCode: "TGS",
                   BrandId: Guid.NewGuid(),
                   BrandCode: "GPI",
                   PlayerCode: "test",
                   Username: "test",
                   DefaultWalletId: Guid.NewGuid(),
                   DefaultWalletCurrencyCode: "CNY");
        _ = fakeUtensil
            .GetPlayerAsync(
                playerCode: Arg.Is("test"),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(player);

        var gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
                Arg.Is(gameProviderId),
                Arg.Is(command.Records.Single().ExternalGameId),
                Arg.Any<CancellationToken>())
            .Returns(gameId);

        var service = new TransferGamePullRecordCommandService(
            fakeLogger,
            fakeOptions,
            fakeMongoSettingOptions,
            fakeMessageSender,
            fakeUtensil);

        await service.ExecuteAsync(command).ConfigureAwait(false);

        await fakeMessageSender.Received(1)
             .PublishAsync(
                subject: Arg.Is(fakeMongoSettingOptions.Value.GlobPattern),
                data: Arg.Any<ReadOnlyMemory<byte>>(),
                cancellationToken: Arg.Any<CancellationToken>())
             .ConfigureAwait(false);
    }
}
