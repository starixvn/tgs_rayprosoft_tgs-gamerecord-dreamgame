using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.DreamGame.Common;

public record DreamGameGameRecordMessage
{
	[Required]
	[DataType(DataType.Url)]
	[DefaultValue(typeof(Uri), "http://localhost/")]
	public Uri ApiUrl { get; init; } = default!;

	[Required]
	[DefaultValue(typeof(string), "974e8e9a072e4ad59a9e3f43a06ff9ae")]
	public string ApiKey { get; init; } = default!;

	[Required]
	[DefaultValue(typeof(string), "DGTE0105CO")]
	public string AgentName { get; init; } = default!;
}
