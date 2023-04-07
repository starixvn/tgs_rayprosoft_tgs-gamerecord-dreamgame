using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameRecord.DreamGameService.Core;
using Tgs.GameRecord.DreamGameService.Core.ApplicationServices;
using Tgs.GameRounds.Schedule.Client;
using Tgs.GameRounds.Schedule.Common.Protos;
using Tgs.GameRounds.Schedule.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGameService.UnitTest;

public class ScheduleFormDispatcherUnitTest
{
    [Fact]
    public async Task Normal_Process()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeGameRoundsScheduleClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakeSchedulePullRecordLock = Substitute.For<ISchedulePullRecordLock>();
        var fakeService = Substitute.For<ICommandService<SchedulePullRecordCommand>>();

        var gameProviderCode = "DGTF";
        var id = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var parameter = new DreamgameParameter
        {
            ApiUrl = "http://localhost/",
            ApiKey = "974e8e9a072e4ad59a9e3f43a06ff9ae",
            AgentName = "DGTE0105CO"
        };
        var scheduleForm = new ScheduleFormReply
        {
            Id = id,
            GameProviderCode = gameProviderCode,
            GameProviderId = Guid.NewGuid(),
            Name = "DGTE0105CO",
            Source = ScheduleFormSource.Schedule,
            Subject = "gp.gamerecord.scheduleform.dg.queued",
            State = ScheduleFormState.Queued,
            Status = $"{ScheduleFormState.Queued}...",
            Parameters = JsonSerializer.Serialize(parameter)
        };
        _ = fakeGameRoundsScheduleClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleForm);

        _ = fakeScheduleFormLock
            .LockingAsync(
                id: Arg.Is(id))
            .Returns(true);

        _ = fakeSchedulePullRecordLock
            .LockingAsync(
                agentName: Arg.Is(parameter.AgentName))
            .Returns(true);

        var service = new ScheduleFormDispatcher(
            fakeLogger,
            fakeGameRoundsScheduleClient,
            fakeScheduleFormLock,
            fakeSchedulePullRecordLock,
            fakeService);

        await service
            .ExecuteAsync(
                gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        await fakeGameRoundsScheduleClient
            .Received(1)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeService
            .Received(1)
            .ExecuteAsync(
                command: Arg.Is(new SchedulePullRecordCommand(
                    ApiUrl: new Uri(parameter.ApiUrl),
                    ApiKey: parameter.ApiKey,
                    AgentName: parameter.AgentName)),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeGameRoundsScheduleClient
            .Received(1)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeSchedulePullRecordLock
            .Received(1)
            .UnlockAsync(
               agentName: Arg.Is(parameter.AgentName))
           .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(1)
            .UnlockAsync(
               id: Arg.Is(id))
           .ConfigureAwait(false);
    }

    [Fact]
    public async Task Schedule_Form_Is_Lock()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeGameRoundsScheduleClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakeSchedulePullRecordLock = Substitute.For<ISchedulePullRecordLock>();
        var fakeService = Substitute.For<ICommandService<SchedulePullRecordCommand>>();

        var gameProviderCode = "DGTF";
        var id = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var parameter = new DreamgameParameter
        {
            ApiUrl = "http://localhost/",
            ApiKey = "974e8e9a072e4ad59a9e3f43a06ff9ae",
            AgentName = "DGTE0105CO"
        };
        var scheduleForm = new ScheduleFormReply
        {
            Id = id,
            GameProviderCode = gameProviderCode,
            GameProviderId = Guid.NewGuid(),
            Name = "DGTE0105CO",
            Source = ScheduleFormSource.Schedule,
            Subject = "gp.gamerecord.scheduleform.dg.queued",
            State = ScheduleFormState.Queued,
            Status = $"{ScheduleFormState.Queued}...",
            Parameters = JsonSerializer.Serialize(parameter)
        };
        _ = fakeGameRoundsScheduleClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleForm);

        _ = fakeScheduleFormLock
            .LockingAsync(
                id: Arg.Is(id))
            .Returns(false);

        var service = new ScheduleFormDispatcher(
            fakeLogger,
            fakeGameRoundsScheduleClient,
            fakeScheduleFormLock,
            fakeSchedulePullRecordLock,
            fakeService);

        await service
            .ExecuteAsync(
                gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        _ = await fakeSchedulePullRecordLock
            .Received(0)
            .LockingAsync(
                agentName: Arg.Is(parameter.AgentName))
            .ConfigureAwait(false);

        await fakeGameRoundsScheduleClient
            .Received(0)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeService
            .Received(0)
            .ExecuteAsync(
                command: Arg.Is(new SchedulePullRecordCommand(
                    ApiUrl: new Uri(parameter.ApiUrl),
                    ApiKey: parameter.ApiKey,
                    AgentName: parameter.AgentName)),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeGameRoundsScheduleClient
            .Received(0)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeSchedulePullRecordLock
            .Received(0)
            .UnlockAsync(
               agentName: Arg.Is(parameter.AgentName))
           .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(0)
            .UnlockAsync(
               id: Arg.Is(id))
           .ConfigureAwait(false);
    }

    [Fact]
    public async Task Schedule_Form_State_Not_Queued()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeGameRoundsScheduleClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakeSchedulePullRecordLock = Substitute.For<ISchedulePullRecordLock>();
        var fakeService = Substitute.For<ICommandService<SchedulePullRecordCommand>>();

        var gameProviderCode = "DGTF";
        var id = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var parameter = new DreamgameParameter
        {
            ApiUrl = "http://localhost/",
            ApiKey = "974e8e9a072e4ad59a9e3f43a06ff9ae",
            AgentName = "DGTE0105CO"
        };
        var scheduleForm = new ScheduleFormReply
        {
            Id = id,
            GameProviderCode = gameProviderCode,
            GameProviderId = Guid.NewGuid(),
            Name = "DGTE0105CO",
            Source = ScheduleFormSource.Schedule,
            Subject = "gp.gamerecord.scheduleform.dg.queued",
            State = ScheduleFormState.Processing,
            Status = $"{ScheduleFormState.Processing}...",
            Parameters = JsonSerializer.Serialize(parameter)
        };
        _ = fakeGameRoundsScheduleClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleForm);

        var service = new ScheduleFormDispatcher(
            fakeLogger,
            fakeGameRoundsScheduleClient,
            fakeScheduleFormLock,
            fakeSchedulePullRecordLock,
            fakeService);

        await service
            .ExecuteAsync(
                gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(0)
            .LockingAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);

        _ = await fakeSchedulePullRecordLock
            .Received(0)
            .LockingAsync(
                agentName: Arg.Is(parameter.AgentName))
            .ConfigureAwait(false);

        await fakeGameRoundsScheduleClient
            .Received(0)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeService
            .Received(0)
            .ExecuteAsync(
                command: Arg.Is(new SchedulePullRecordCommand(
                    ApiUrl: new Uri(parameter.ApiUrl),
                    ApiKey: parameter.ApiKey,
                    AgentName: parameter.AgentName)),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeGameRoundsScheduleClient
            .Received(0)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeSchedulePullRecordLock
            .Received(0)
            .UnlockAsync(
               agentName: Arg.Is(parameter.AgentName))
           .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(0)
            .UnlockAsync(
               id: Arg.Is(id))
           .ConfigureAwait(false);
    }

    [Fact]
    public async Task Schedule_Pull_Record_Lock()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeGameRoundsScheduleClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakeSchedulePullRecordLock = Substitute.For<ISchedulePullRecordLock>();
        var fakeService = Substitute.For<ICommandService<SchedulePullRecordCommand>>();

        var gameProviderCode = "DGTF";
        var id = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var parameter = new DreamgameParameter
        {
            ApiUrl = "http://localhost/",
            ApiKey = "974e8e9a072e4ad59a9e3f43a06ff9ae",
            AgentName = "DGTE0105CO"
        };
        var scheduleForm = new ScheduleFormReply
        {
            Id = id,
            GameProviderCode = gameProviderCode,
            GameProviderId = Guid.NewGuid(),
            Name = "DGTE0105CO",
            Source = ScheduleFormSource.Schedule,
            Subject = "gp.gamerecord.scheduleform.dg.queued",
            State = ScheduleFormState.Queued,
            Status = $"{ScheduleFormState.Queued}...",
            Parameters = JsonSerializer.Serialize(parameter)
        };
        _ = fakeGameRoundsScheduleClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleForm);

        _ = fakeScheduleFormLock
            .LockingAsync(
                id: Arg.Is(id))
            .Returns(true);

        _ = fakeSchedulePullRecordLock
            .LockingAsync(
                agentName: Arg.Is(parameter.AgentName))
            .Returns(false);

        var service = new ScheduleFormDispatcher(
            fakeLogger,
            fakeGameRoundsScheduleClient,
            fakeScheduleFormLock,
            fakeSchedulePullRecordLock,
            fakeService);

        await service
            .ExecuteAsync(
                gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        await fakeGameRoundsScheduleClient
            .Received(1)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeService
            .Received(0)
            .ExecuteAsync(
                command: Arg.Is(new SchedulePullRecordCommand(
                    ApiUrl: new Uri(parameter.ApiUrl),
                    ApiKey: parameter.ApiKey,
                    AgentName: parameter.AgentName)),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeGameRoundsScheduleClient
            .Received(1)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeSchedulePullRecordLock
            .Received(0)
            .UnlockAsync(
               agentName: Arg.Is(parameter.AgentName))
           .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(1)
            .UnlockAsync(
               id: Arg.Is(id))
           .ConfigureAwait(false);
    }
}
