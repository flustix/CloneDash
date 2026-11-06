using CloneDash.Compatibility.Unity;
using System.Text.Json.Serialization;

namespace CloneDash.Unbeatable.Indexes.Beatmaps;

public class BeatmapIndex
{
	[JsonPropertyName("coverArt")]
	public UnityPathPointer CoverArt { get; init; }
	
	[JsonPropertyName("songs")]
	public BeatmapIndexSong[] Songs { get; init; } = [];
}