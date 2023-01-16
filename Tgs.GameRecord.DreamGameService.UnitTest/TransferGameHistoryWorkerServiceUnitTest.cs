using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.DreamGamingService.Client;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.GameRecord.DreamGameService.Core.WorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameServiceUnitTest;

[TestClass]
public class TransferGameHistoryWorkerServiceUnitTest
{
    [TestMethod]
    public async Task Normal_Process()
    {
        var fakeLogger = NullLogger<TransferGameHistoryWorkerService>.Instance;
        var fakeClient = Substitute.For<IGameInterfaceDreamGamingClient>();
        var fakeService = Substitute.For<ICommandService<TransferGamePullRecordCommand>>();

        var command = new TransferGameHistoryWorkerCommand(
            ApiUrl: new("https://api.dg0.co"),
            ApiKey: "974e8e9a072e4ad59a9e3f43a06ff9ae",
            AgentName: "DGTE0105CO");

        var gameRecords = new GameHistorySettled[] {
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
                AdditionalData=string.Empty }};

        _ = fakeClient
            .GameHistorySettledAsync(
                Arg.Is(command.ApiUrl),
                Arg.Is(command.ApiKey),
                Arg.Is(command.AgentName),
                Arg.Any<CancellationToken>()
            ).Returns(gameRecords.ToAsyncEnumerable());

        var service = new TransferGameHistoryWorkerService(
            fakeLogger,
            fakeClient,
            fakeService);

        await service.ExecuteAsync(command).ConfigureAwait(false);

        await fakeService
            .Received(1)
            .ExecuteAsync(
                Arg.Any<TransferGamePullRecordCommand>(),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeClient
            .Received(1)
            .GameHistoryMarkAsync(
                apiUrl: Arg.Is(command.ApiUrl),
                apiKey: Arg.Is(command.ApiKey),
                agentName: Arg.Is(command.AgentName),
                externalRoundIds: Arg.Any<string[]>(),
               cancellationToken: Arg.Any<CancellationToken>())
             .ConfigureAwait(false);
    }
}
