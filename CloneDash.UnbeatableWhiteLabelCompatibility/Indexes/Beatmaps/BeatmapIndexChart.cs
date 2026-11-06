using CloneDash.Common.Songs;
using CloneDash.Compatibility.Unity;
using CloneDash.Unbeatable.Songs;
using System.Text.Json.Serialization;

namespace CloneDash.Unbeatable.Indexes.Beatmaps;

public class BeatmapIndexChart
{
	[JsonPropertyName("textAsset")]
	public UnityPathPointer Path { get; init; }

	[JsonPropertyName("difficulty")]
	public string Difficulty { get; init; } = string.Empty;

	[JsonIgnore]
	private ISongChart? _chart;

	public ISongChart GetChart(BeatmapIndexSong song) => _chart ??= new UnbeatableChart(song, this);
}