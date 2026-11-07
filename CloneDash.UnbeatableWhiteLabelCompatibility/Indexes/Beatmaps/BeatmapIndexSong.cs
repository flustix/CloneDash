using AssetStudio;
using CloneDash.Common.Songs;
using CloneDash.Compatibility.Unity;
using CloneDash.Unbeatable.Songs;
using System.Text.Json.Serialization;

namespace CloneDash.Unbeatable.Indexes.Beatmaps;

public class BeatmapIndexSong
{
	[JsonPropertyName("name")]
	public string Name { get; init; } = string.Empty;
	
	[JsonPropertyName("coverArt")]
	public UnityPathPointer CoverArtPath { get; init; }

	[JsonPropertyName("beatmaps")]
	public BeatmapIndexChart[] Charts { get; init; } = [];

	[JsonIgnore]
	public SerializedFile ResourcesFile { get; internal set; } = null!;

	[JsonIgnore]
	public ISong Song => field ??= new UnbeatableSong(this);
}