using CloneDash.Common.Songs;
using CloneDash.Unbeatable.Songs;
using System.Text.Json.Serialization;

namespace CloneDash.Unbeatable.Indexes.Beatmaps;

public class BeatmapIndexSong
{
	[JsonPropertyName("name")]
	public string Name { get; init; } = string.Empty;

	[JsonPropertyName("beatmaps")]
	public BeatmapIndexChart[] Charts { get; init; } = [];

	[JsonIgnore]
	public ISong Song => field ??= new UnbeatableSong(this);
}