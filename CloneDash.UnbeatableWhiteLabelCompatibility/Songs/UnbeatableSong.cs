using CloneDash.Charts;
using CloneDash.Common.Compatibility;
using CloneDash.Common.Songs;
using CloneDash.Unbeatable.Indexes.Beatmaps;
using Nucleus.Common.Audio;

namespace CloneDash.Unbeatable.Songs;

public class UnbeatableSong(BeatmapIndexSong index) : ISong
{
	public SongMetadata FetchMetadata() => new() { Name = index.Name, Author = "D-Cell Games" };

	public string GetName() => index.Name;
	public string GetID() => index.Name.ToLowerInvariant();
	public ICloneDashModProvider GetProvider() => ChartMod.Instance.GetProviderByID(UnbeatableCompatibility.ID)!;

	public SongCoverInfo GetCoverTexture() => new();
	public IAudioClip? GetDemoAudio() => null;

	public IReadOnlyList<ISongChart> GetCharts() => [.. index.Charts.Select(x => x.GetChart(index))];

	public bool IsAsynchronouslyLoading() => false;

	public void WaitForAsynchronousLoad(OnAsynchronousLoadingCompleteFn callback) =>
		throw new NotImplementedException();
}