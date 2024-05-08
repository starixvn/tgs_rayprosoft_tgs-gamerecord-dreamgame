using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using Tgs.GameInterface.DreamGamingService.Client;
using Tgs.GameInterface.DreamGamingService.Protos;
using Tgs.GameRecord.DreamGame.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.DreamGame.Core.UnitTest.ApplicationServices;

public class PullRecordCommandServiceUnitTest
{
	[Fact]
	public async Task No_Data()
	{
		var fakeLogger = NullLogger<PullRecordCommandService>.Instance;
		var fakePullRecordLock = Substitute.For<IPullRecordLock>();
		var fakeClient = Substitute.For<IGameInterfaceDreamGamingClient>();
		var fakeService = Substitute.For<ICommandService<HandleRecordCommand>>();

		var command = new PullRecordCommand(
			ApiUrl: new("http://localhost/"),
			ApiKey: "974e8e9a072e4ad59a9e3f43a06ff9ae",
			AgentName: "DGTE0105CO");

		_ = fakePullRecordLock.LockingAsync(
			agentName: Arg.Is(command.AgentName))
			.Returns(true);

		_ = fakeClient
			.GameHistorySettledAsync(
				apiUrl: Arg.Is(command.ApiUrl),
				apiKey: Arg.Is(command.ApiKey),
				agentName: Arg.Is(command.AgentName),
				cancellationToken: Arg.Any<CancellationToken>())
			.Returns(Enumerable.Empty<GameHistorySettled>().ToAsyncEnumerable());

		var service = new PullRecordCommandService(
			fakeLogger,
			fakePullRecordLock,
			fakeClient,
			fakeService);

		await service.ExecuteAsync(command: command);

		_ = await fakeClient
			.Received(0)
			.GameHistoryMarkAsync(
			apiUrl: Arg.Is(command.ApiUrl),
			apiKey: Arg.Is(command.ApiKey),
			agentName: Arg.Is(command.AgentName),
			externalRoundIds: Arg.Any<List<string>>(),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakePullRecordLock
			.Received(1)
			.UnlockedAsync(
			agentName: Arg.Is(command.AgentName));
	}

	[Fact]
	public async Task Normal_Process()
	{
		var fakeLogger = NullLogger<PullRecordCommandService>.Instance;
		var fakePullRecordLock = Substitute.For<IPullRecordLock>();
		var fakeClient = Substitute.For<IGameInterfaceDreamGamingClient>();
		var fakeService = Substitute.For<ICommandService<HandleRecordCommand>>();

		var command = new PullRecordCommand(
			ApiUrl: new("http://localhost/"),
			ApiKey: "974e8e9a072e4ad59a9e3f43a06ff9ae",
			AgentName: "DGTE0105CO");

		_ = fakePullRecordLock.LockingAsync(
			agentName: Arg.Is(command.AgentName))
			.Returns(true);

		var gameHistorySettleds = new GameHistorySettled[]
		{
			new() {
				PlayerCode = "test",
				ExternalRoundId = "10893708376",
				ExternalGameId = "bacc",
				IsVoid = false,
				IsClosed = true,
				ExternalCreatedOn = new DateTimeOffset(DateTime.Parse("2022-04-21 18:30:39"), TimeSpan.FromHours(+8)).ToTimestamp(),
				ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2022-04-21 18:31:06"), TimeSpan.FromHours(+8)).ToTimestamp(),
				Bet = 20m,
				ValidBet = 20m,
				Settle = 0.0m,
				Turnover = 20.00m,
				PlayerWinLoss = -20.00m,
				AdditionalData = string.Empty
			}
		};
		_ = fakeClient
			.GameHistorySettledAsync(
				apiUrl: Arg.Is(command.ApiUrl),
				apiKey: Arg.Is(command.ApiKey),
				agentName: Arg.Is(command.AgentName),
				cancellationToken: Arg.Any<CancellationToken>())
			.Returns(gameHistorySettleds.ToAsyncEnumerable());

		var service = new PullRecordCommandService(
			fakeLogger,
			fakePullRecordLock,
			fakeClient,
			fakeService);

		await service.ExecuteAsync(command: command);

		_ = await fakeClient
			.Received(1)
			.GameHistoryMarkAsync(
			apiUrl: Arg.Is(command.ApiUrl),
			apiKey: Arg.Is(command.ApiKey),
			agentName: Arg.Is(command.AgentName),
			externalRoundIds: Arg.Any<List<string>>(),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakePullRecordLock
			.Received(1)
			.UnlockedAsync(
			agentName: Arg.Is(command.AgentName));
	}

	[Fact]
	public async Task Schedule_Handle_Record_Exception()
	{
		var fakeLogger = NullLogger<PullRecordCommandService>.Instance;
		var fakePullRecordLock = Substitute.For<IPullRecordLock>();
		var fakeClient = Substitute.For<IGameInterfaceDreamGamingClient>();
		var fakeService = Substitute.For<ICommandService<HandleRecordCommand>>();

		var command = new PullRecordCommand(
			ApiUrl: new("http://localhost/"),
			ApiKey: "974e8e9a072e4ad59a9e3f43a06ff9ae",
			AgentName: "DGTE0105CO");

		_ = fakePullRecordLock.LockingAsync(
			agentName: Arg.Is(command.AgentName))
			.Returns(true);

		var gameHistorySettleds = new GameHistorySettled[]
		{
			new() {
				PlayerCode = "test",
				ExternalRoundId = "10893708376",
				ExternalGameId = "bacc",
				IsVoid = false,
				IsClosed = true,
				ExternalCreatedOn = new DateTimeOffset(DateTime.Parse("2022-04-21 18:30:39"), TimeSpan.FromHours(+8)).ToTimestamp(),
				ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2022-04-21 18:31:06"), TimeSpan.FromHours(+8)).ToTimestamp(),
				Bet = 20m,
				ValidBet = 20m,
				Settle = 0.0m,
				Turnover = 20.00m,
				PlayerWinLoss = -20.00m,
				AdditionalData = string.Empty
			}
		};
		_ = fakeClient
			.GameHistorySettledAsync(
				apiUrl: Arg.Is(command.ApiUrl),
				apiKey: Arg.Is(command.ApiKey),
				agentName: Arg.Is(command.AgentName),
				cancellationToken: Arg.Any<CancellationToken>())
			.Returns(gameHistorySettleds.ToAsyncEnumerable());

		_ = fakeService
			.ExecuteAsync(
				command: Arg.Any<HandleRecordCommand>(),
				cancellationToken: Arg.Any<CancellationToken>())
			.Throws<Exception>();

		var service = new PullRecordCommandService(
			fakeLogger,
			fakePullRecordLock,
			fakeClient,
			fakeService);

		await service.ExecuteAsync(command: command);

		_ = await fakeClient
			.Received(0)
			.GameHistoryMarkAsync(
			apiUrl: Arg.Is(command.ApiUrl),
			apiKey: Arg.Is(command.ApiKey),
			agentName: Arg.Is(command.AgentName),
			externalRoundIds: Arg.Any<List<string>>(),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakePullRecordLock
			.Received(1)
			.UnlockedAsync(
			agentName: Arg.Is(command.AgentName));
	}
}
