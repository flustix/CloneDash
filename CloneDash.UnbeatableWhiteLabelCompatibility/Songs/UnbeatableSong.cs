using AssetStudio;
using CloneDash.Charts;
using CloneDash.Common.Compatibility;
using CloneDash.Common.Songs;
using CloneDash.Compatibility.Unity;
using CloneDash.Unbeatable.Indexes.Beatmaps;
using Nucleus;
using Nucleus.Common.Audio;
using Nucleus.Common.Graphics;

namespace CloneDash.Unbeatable.Songs;

public class UnbeatableSong(BeatmapIndexSong index) : ISong
{
	private readonly Lazy<ITexture?> _coverTexture = new(() => {
		AssetStudio.Object? obj = index.ResourcesFile.GetObjectByPath(index.CoverArtPath);
		if (obj is not Sprite spr) return null;

		Texture2D? tex = spr.m_RD.GetTexture();
		return tex is null ? null : UnityAssetUtils.ConvertTexture(EngineCore.Level, tex);
	});

	public SongMetadata FetchMetadata() => new() { Name = GetName(), Author = "D-Cell Games" };

	public string GetName() {
		if (index.Charts.First().GetChart(index) is not UnbeatableChart chart)
			return null!;
		
		return chart?.Beatmap.metadata.title ?? index.Name;
	}

	public string GetID() => index.Name.ToLowerInvariant();
	public ICloneDashModProvider GetProvider() => ChartMod.Instance.GetProviderByID(UnbeatableCompatibility.ID)!;

	public SongCoverInfo GetCoverTexture() => new() { Texture = _coverTexture.Value };

	public IAudioClip? GetDemoAudio() {
		if (index.Charts.First().GetChart(index) is not UnbeatableChart chart)
			return null!;

		string name = Path.GetFileNameWithoutExtension(chart.Beatmap.general.audioFilename);
		// should be loading from BgmClips but i have no idea how to associate the filenames
		IAudioClip? clip = UnbeatableCompatibility.MasterClips.GetValueOrDefault(name);
		return clip!;
	}

	public IReadOnlyList<ISongChart> GetCharts() => [.. index.Charts.Select(x => x.GetChart(index))];

	public bool IsAsynchronouslyLoading() => false;

	public void WaitForAsynchronousLoad(OnAsynchronousLoadingCompleteFn callback) =>
		throw new NotImplementedException();
}