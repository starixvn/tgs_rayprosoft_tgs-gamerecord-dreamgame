using Tgs.GameInterface.DreamGamingService.Protos;

namespace Tgs.GameRecord.DreamGameService.Core.ApplicationServices;

public record DGTFGamePullRecordSettledCommand(IEnumerable<GameHistorySettled> Records);