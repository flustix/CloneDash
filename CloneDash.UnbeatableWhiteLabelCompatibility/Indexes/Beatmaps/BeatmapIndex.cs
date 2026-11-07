using System.Text.Json.Serialization;

namespace CloneDash.Unbeatable.Indexes.Beatmaps;

public class BeatmapIndex
{
	[JsonPropertyName("songs")]
	public BeatmapIndexSong[] Songs { get; init; } = [];
}