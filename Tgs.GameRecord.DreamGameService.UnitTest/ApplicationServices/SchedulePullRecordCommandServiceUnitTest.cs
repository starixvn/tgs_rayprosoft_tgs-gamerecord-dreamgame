using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Tgs.GameInterface.DreamGamingService.Client;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.UnitTest.ApplicationServices;

public class SchedulePullRecordCommandServiceUnitTest
{
    [Fact]
    public async Task Normal_Process()
    {
        var fakeLogger = NullLogger<SchedulePullRecordCommandService>.Instance;
        var fakeClient = Substitute.For<IGameInterfaceDreamGamingClient>();
        var fakeService = Substitute.For<ICommandService<ScheduleHandleRecordCommand>>();

        var command = new SchedulePullRecordCommand(
            ApiUrl: new("http://localhost/"),
            ApiKey: "974e8e9a072e4ad59a9e3f43a06ff9ae",
            AgentName: "DGTE0105CO");

        var service = new SchedulePullRecordCommandService(
            fakeLogger,
            fakeClient,
            fakeService);

        await service
            .ExecuteAsync(
                command: command)
            .ConfigureAwait(false);

        _ = await fakeClient
            .Received(1)
            .GameHistorySettledAsync(
                apiUrl: Arg.Is(command.ApiUrl),
                apiKey: Arg.Is(command.ApiKey),
                agentName: Arg.Is(command.AgentName),
                cancellationToken: Arg.Any<CancellationToken>())
            .ToArrayAsync(
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}
