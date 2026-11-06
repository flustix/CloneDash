using AssetStudio;
using CloneDash.Charts;
using CloneDash.Common.Compatibility;
using CloneDash.Common.Compatibility.Valve;
using CloneDash.Common.Songs;
using CloneDash.Compatibility.Unity;
using CloneDash.Unbeatable.Indexes.Beatmaps;
using Fmod5Sharp;
using Fmod5Sharp.FmodTypes;
using Nucleus;
using Nucleus.Commands;
using Nucleus.Common.Audio;
using Nucleus.Core;
using Nucleus.Util;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CloneDash.Unbeatable;

[MarkForStaticConstruction]
public static partial class UnbeatableCompatibility
{
	#region ConVar

	public static readonly ConVar CustomGamePath = new("ub_path_override", string.Empty, FCvar.Saved, "");

	public static readonly ConVar UnbeatableEnabled = new("ub_enabled", "0", FCvar.Saved,
		"Enables/disables UNBEATABLE compatibility. Requires a restart to take effect.");

	#endregion

	public const string ID = "unbeatable";

	public static BeatmapIndex BeatmapIndex = null!;

	internal static AssetsManager? Assets;
	internal static AssemblyLoader? Assemblies;
	private static string? _installDirectory;

	internal static readonly Dictionary<UnityPathPointer, AssetLookupEntry> AssetPointerTable = [];
	internal static readonly Dictionary<long, AssetLookupEntry> PathIDLUT = [];
	internal static readonly Dictionary<ulong, AssetLookupEntry> NameLUT = [];

	static readonly Dictionary<string, IAudioClip> BgmClips = [];
	static readonly Dictionary<string, IAudioClip> MasterClips = [];
	static readonly Dictionary<string, IAudioClip> SfxClips = [];

	public static AppStatus LightInitialize() {
		string path = new(CustomGamePath.GetString());

		if (string.IsNullOrWhiteSpace(path)) {
			if (!SteamApps.WhereIsAppInstalled(UNBEATABLE_WHITELABEL_APPID).IsOK(out string? dir, out AppStatus err)) {
				return err;
			}

			path = dir!;
		}

		_installDirectory = path;
		string data = Path.Combine(_installDirectory, "UNBEATABLE_Data");

		if (!Path.Exists(_installDirectory))
			return AppStatus.CriticalPathNotFound;

		Assets = new AssetsManager();
		Assets.LoadFolder(data);

		Assemblies = new AssemblyLoader();
		Assemblies.Load(path);
		Assemblies.Load(Path.Combine(data, "Managed"));

		return AppStatus.OK;
	}

	const uint UNBEATABLE_WHITELABEL_APPID = 1290490;

	internal class AssetLookupEntry
	{
		public required SerializedFile SerializedFile;
		public required AssetStudio.Object Object;
	}

	public static AppStatus InitializeCompatibilityLayer() {
		AppStatus result;
		if ((result = LightInitialize()) != AppStatus.OK) {
			return result; // unbeatable is optional <- it's actually not
		}

		if (Assets is null)
			return AppStatus.AppNotInstalled;

		// Build the LUT
		foreach (var serializedFile in Assets.AssetsFileList) {
			foreach (var obj in serializedFile.Objects) {
				AssetLookupEntry entry = new() { SerializedFile = serializedFile, Object = obj };
				PathIDLUT[obj.m_PathID] = entry;
				NameLUT[obj.GetUnityName().Hash(false)] = entry;

				// TODO: this is completely wrong and only here to get charts to load
				int id = serializedFile.fileName == "resources.assets" ? 0 : 1;
				
				UnityPathPointer pointer = new() { FileID = id, PathID = obj.m_PathID };
				AssetPointerTable[pointer] = entry;
			}
		}

		/*var bgm = LoadFsbMagically(System.IO.File.ReadAllBytes(System.IO.Path.Combine(InstallDir, "UNBEATABLE_Data",
			"StreamingAssets", "BGM.bank")));
		var master = LoadFsbMagically(System.IO.File.ReadAllBytes(System.IO.Path.Combine(InstallDir, "UNBEATABLE_Data",
			"StreamingAssets", "Master.bank")));
		var sfx = LoadFsbMagically(System.IO.File.ReadAllBytes(System.IO.Path.Combine(InstallDir, "UNBEATABLE_Data",
			"StreamingAssets", "SFX.bank")));
		BuildNucleusClips(Bgm, BgmClips);
		BuildNucleusClips(Master, MasterClips);
		BuildNucleusClips(Sfx, SfxClips);*/

		MonoBehaviour indexBehaviour = GetObjectByName<MonoBehaviour>("BeatmapIndex")!;
		OrderedDictionary indexDictionary = indexBehaviour.ToType(indexBehaviour.ConvertToTypeTree(Assemblies));
		string indexJson = JsonSerializer.Serialize(indexDictionary);
		BeatmapIndex = JsonSerializer.Deserialize<BeatmapIndex>(indexJson)!;

		/*object[] songs = (object[])data["songs"]!;
		BeatmapIndex.Songs = new BeatmapIndexSong[songs.Length];
		int i = 0;
		foreach (var songObj in songs) {
			OrderedDictionary songDict = (OrderedDictionary)songObj;
			string songName = (string)songDict["name"]!;
			if (songName == null || Songs.TryGetValue(songName, out var musicName))
				continue;

			Songs[songName] = new SongMusicInfo();

			BeatmapIndexSong song = BeatmapIndex.Songs[i] = new BeatmapIndexSong();
			song.Name = songName;
			BeatmapIndex.SongDict[song.Name] = song;
			song.StageScene = (string)songDict["stageScene"]!;

			object[] beatmaps = (object[])songDict["beatmaps"]!;
			song.Beatmaps = new BeatmapInfo[beatmaps.Length];

			if (!string.IsNullOrWhiteSpace(musicName.SongPreview))
				BgmClips.TryGetValue(musicName.SongPreview, out song.PreviewClip);

			int j = 0;
			foreach (var beatmapObj in beatmaps) {
				OrderedDictionary beatmapDict = (OrderedDictionary)beatmapObj;
				BeatmapInfo beatmap = song.Beatmaps[j] = new BeatmapInfo(song);
				beatmap.Difficulty = (string)beatmapDict["difficulty"]!;

				OrderedDictionary textAssetPtrRaw = (OrderedDictionary)beatmapDict["textAsset"]!;
				beatmap.TextAsset = new PPtr<TextAsset>((int)textAssetPtrRaw["m_FileID"]!,
					(long)textAssetPtrRaw["m_PathID"]!, mb_beatmapIndex.assetsFile);
				j++;

				if (!string.IsNullOrWhiteSpace(musicName.SongAudio))
					MasterClips.TryGetValue(musicName.SongAudio, out beatmap.AudioClip!);
			}

			i++;
		}*/

		// BeatmapIndex.Songs = BeatmapIndex.Songs.Where(x => x != null).ToArray();
		return AppStatus.OK;
	}

	private static FmodSoundBank[] LoadFsbMagically(byte[] bank) {
		ReadOnlySpan<byte> fsb5Magic = "FSB5"u8;
		ReadOnlySpan<byte> currentSpan = bank;

		List<FmodSoundBank> soundBanks = [];

		while (true) {
			int index = currentSpan.IndexOf(fsb5Magic);

			if (index == -1) {
				break;
			}

			ReadOnlySpan<byte> fsbSpan = currentSpan[index..];
			soundBanks.Add(FsbLoader.LoadFsbFromByteArray(fsbSpan.ToArray()));
			currentSpan = currentSpan[(index + fsb5Magic.Length)..];
		}

		return soundBanks.ToArray();
	}

	private static void BuildNucleusClips(FmodSoundBank[] banks, Dictionary<string, IAudioClip> clips) {
		for (int h = 0; h < banks.Length; h++) {
			FmodSoundBank bank = banks[h];
			HashSet<string> uniqueStrs = [];
			for (int i = 0; i < bank.Samples.Count; i++) {
				var sample = bank.Samples[i];
				int add = 1;
				string sampleName = string.IsNullOrWhiteSpace(sample.Name) ? $"sample_{i}" : sample.Name;
				if (uniqueStrs.Contains(sampleName)) {
					string pickAnother;
					while (uniqueStrs.Contains(pickAnother = $"{sampleName}_{add}"))
						add++;
					sampleName = pickAnother;
				}

				uniqueStrs.Add(sampleName);

				// 3. Rebuild the data into standard format (.wav, .ogg, etc.)
				if (sample.RebuildAsStandardFileFormat(out var dataBytes, out var fileExtension)) {
					using var into = new MemoryStream(dataBytes!);
					clips[sampleName] = audiosystem.CreateStreamAudioClip(into, $"UWLAsset:{sampleName}")!;
				}
			}
		}
	}

	public static T? GetObjectByName<T>(ReadOnlySpan<char> name) where T : AssetStudio.Object {
		if (!NameLUT.TryGetValue(name.Hash(false), out AssetLookupEntry? entry))
			return null;
		return entry.Object is T t ? t : null;
	}

	public static IAudioClip? GetMasterClip(string name) => MasterClips.TryGetValue(name, out var clip) ? clip : null;

	public static T? GetObjectByPathID<T>(long pathID) where T : AssetStudio.Object {
		if (!PathIDLUT.TryGetValue(pathID, out AssetLookupEntry? entry))
			return null;
		return entry.Object is T t ? t : null;
	}

	public static bool IsEnabled() => UnbeatableEnabled.GetBool();
}

public class UnbeatableWhiteLabelChartSource : BaseContiguousSongSource
{
	public UnbeatableWhiteLabelChartSource(BaseContiguousChartSongFilter? filter = null,
		ISongSourceState? parent = null) : base(
		[.. UnbeatableCompatibility.BeatmapIndex.Songs.Select(x => x.Song)],
		filter, parent) {
	}

	public override ISongSourceState ProduceNewSource(IChartSongFilter filter) {
		if (filter is not BaseContiguousChartSongFilter contigFilter)
			throw new InvalidCastException("Invalid contigFilter");
		return new UnbeatableWhiteLabelChartSource(contigFilter, this.GetRootSource());
	}
}

[MarkForStaticConstruction]
public class UnbeatableWhiteLabelChartProvider() : ModProvider<ISong>("UNBEATABLE", "unbeatable"), IChartSongProvider
{
	public static readonly ConVar ubwl_lastsong =
		new ConVar(nameof(ubwl_lastsong), "", FCvar.Saved, "The last selected song ID");

	public static readonly ConVar ubwl_lastfilter =
		new ConVar(nameof(ubwl_lastfilter), "", FCvar.Saved, "The last selected song filter");

	public IChartSongFilter? GetSavedFilter() => ubwl_lastfilter.GetString().IsEmpty
		? null
		: JSON.Deserialize<BaseContiguousChartSongFilter>(new(ubwl_lastfilter.GetString()));

	public ISong? GetSavedSong() => ChartMod.Instance.GetDescriptorByUID(ubwl_lastsong.GetString());

	public void UpdateSavedFilter(IChartSongFilter? filter) =>
		ubwl_lastfilter.SetValue(filter == null ? "" : JSON.Serialize((BaseContiguousChartSongFilter)filter));

	public void UpdateSavedSong(ISong? selectedSong) =>
		ubwl_lastsong.SetValue(selectedSong == null ? "" : selectedSong.GetUUID());

	public ISong? FindSongByName(ReadOnlySpan<char> name) {
		name = name.SliceNullTerminatedString();
		foreach (var song in UnbeatableCompatibility.BeatmapIndex.Songs) {
			if (name.Equals(song.Name, StringComparison.InvariantCultureIgnoreCase))
				return song.Song;
		}

		return null;
	}

	public override IEnumerable<string> GetAvailableIDs() {
		foreach (var song in UnbeatableCompatibility.BeatmapIndex.Songs)
			yield return song.Song.GetID();
	}

	public override ISong? FindByID(ReadOnlySpan<char> id) {
		foreach (var song in UnbeatableCompatibility.BeatmapIndex.Songs)
			if (id.Equals(song.Song.GetID(), StringComparison.Ordinal))
				return song.Song;
		return null;
	}

	public ISongSourceState NewState() => new UnbeatableWhiteLabelChartSource();

	public override int GetSortIndex() => 40000;

	public IChartSongProvider.NavigationButtonInstructions GetNavigationButtonInstructions() => new() {
		Name = "Play UNBEATABLE charts",
		Description = "Play a chart from UNBEATABLE",
		Hue = 348,
		Icon = "icons/play.png"
	};

	public override bool IsEnabled() => UnbeatableCompatibility.UnbeatableEnabled.GetBool();
}