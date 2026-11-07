using AssetStudio;
using CloneDash.Common.Compatibility;
using CloneDash.Common.Data;
using CloneDash.Common.Gamemodes;
using CloneDash.Common.Songs;
using CloneDash.Compatibility.UnbeatableWhiteLabel;
using CloneDash.Compatibility.Unity;
using CloneDash.Unbeatable.Indexes.Beatmaps;
using CloneDash.Unbeatable.Internal;
using Nucleus.Common.Audio;
using System.Text;
using Color = Nucleus.Common.Types.Color;

namespace CloneDash.Unbeatable.Songs;

public class UnbeatableChart(BeatmapIndexSong song, BeatmapIndexChart chart) : ISongChart
{
	public Beatmap Beatmap => _beatmap.Value;

	private readonly Lazy<Beatmap> _beatmap = new(() => {
		Beatmap beatmap = new();
		AssetStudio.Object? obj = song.ResourcesFile.GetObjectByPath(chart.Path);

		if (obj is not TextAsset text)
			return beatmap;

		BeatmapParserEngine parser = new();
		parser.ReadBeatmap(Encoding.UTF8.GetString(text.m_Script), ref beatmap);
		return beatmap;
	});

	public ISong GetSong() => song.Song;

	public IAudioClip GetAudioTrack() {
		string name = Path.GetFileNameWithoutExtension(Beatmap.general.audioFilename);
		IAudioClip? clip = UnbeatableCompatibility.MasterClips.GetValueOrDefault(name);
		return clip!;
	}

	public ChartTiming GetTiming() {
		List<TempoChange> tempoChanges = [];
		double beat = 0, lastTime = 0, lastBPM = 0;

		foreach (var timingPoint in Beatmap.timingPoints) {
			if (!timingPoint.uninherited || timingPoint.beatLength <= 0)
				continue;

			double time = timingPoint.time / 1000d;
			double bpm = 60000d / timingPoint.beatLength;
			if (lastBPM > 0)
				beat += (time - lastTime) * lastBPM / 240;
			else if (time > 0)
				tempoChanges.Add(new TempoChange(0, (int)Math.Round(-time * bpm / 240), bpm));

			lastTime = time;
			lastBPM = bpm;
			tempoChanges.Add(new TempoChange(time, (int)Math.Round(beat), lastBPM));
		}

		return new ChartTiming(tempoChanges, []);
	}

	public IGamemodeDescriptor GetGamemode() =>
		ModStores.Get<GamemodeMod>().GetDescriptorByUID(UnbeatableWhiteLabelGamemode.UUID)!;

	public object GetGamemodeData() => Beatmap;

	public SongChartMetadata FetchMetadata() => new() {
		DifficultyName = chart.Difficulty,
		ChartAuthors = "D-Cell Games",
		Color = new Color(115, 55, 55),
		Difficulty = "5",
		GamemodeName = "UNBEATABLE"
	};

	public int GetRatingNumber() => 5; // TODO
}