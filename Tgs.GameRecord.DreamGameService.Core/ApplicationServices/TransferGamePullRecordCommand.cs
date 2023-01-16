using Tgs.GameInterface.DreamGamingService.Protos;

namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

public record TransferGamePullRecordCommand(IEnumerable<GameHistorySettled> Records);
