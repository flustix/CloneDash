using CloneDash.Characters;
using CloneDash.Common.Compatibility;
using CloneDash.Common.Gamemodes;
using CloneDash.Common.Gamemodes.MuseDash.V1;
using CloneDash.Compatibility.Unity;
using Nucleus;
using Nucleus.Common.Graphics;

namespace CloneDash.Compatibility.UnbeatableWhiteLabel;

public class UnbeatableWhiteLabelCharacterProvider() : ModProvider<ICharacterDescriptor>("UNBEATABLE", "unbeatable"), ICharacterProvider
{
	public const string BEAT = "beat";

	public override IEnumerable<string> GetAvailableIDs() {
		if (Unbeatable.UnbeatableCompatibility.IsEnabled())
			yield return BEAT;
	}

	public override ICharacterDescriptor? FindByID(ReadOnlySpan<char> id) {
		if (!Unbeatable.UnbeatableCompatibility.IsEnabled() || !id.Equals(BEAT, StringComparison.Ordinal))
			return null;
		return new UnbeatableWhiteLabelCharacterDescriptor(this);
	}

	public override bool IsEnabled() => Unbeatable.UnbeatableCompatibility.IsEnabled();
}

public class UnbeatableWhiteLabelCharacterDescriptor(ICloneDashModProvider provider) : ModDescriptor, ICharacterDescriptor
{
	const string THUMBNAIL = "beatattack1";

	public override string GetID() => UnbeatableWhiteLabelCharacterProvider.BEAT;
	public override string GetName() => "Beat";
	public override ICloneDashModProvider GetProvider() => provider;

	public ReadOnlySpan<char> GetAuthor() => "D-CELL GAMES";
	public ReadOnlySpan<char> GetCharacterName() => "Beat";
	public ReadOnlySpan<char> GetCosplayName() => "";
	public ReadOnlySpan<char> GetDescription() => "Beat, with Quaver backing her up.";
	public ReadOnlySpan<char> GetPerk() => "";

	public ITexture? GetThumbnailTexture() {
		var texture = Unbeatable.UnbeatableCompatibility.GetObjectByName<AssetStudio.Texture2D>(THUMBNAIL)
			?? Unbeatable.UnbeatableCompatibility.GetObjectByName<AssetStudio.Sprite>(THUMBNAIL)?.m_RD.GetTexture();
		return texture == null ? null : UnityAssetUtils.ConvertTexture(EngineCore.Level, texture);
	}

	public void ApplyQuirks(ref MuseDash1GameplayQuirks quirks) { }

	public ICharacterMainMenuInstance CreateMainMenu() => new UnbeatableWhiteLabelCharacterMainMenuInstance(this);
	public ICharacterVictoryInstance CreateVictory() => new UnbeatableWhiteLabelCharacterVictoryInstance(this);
	public ICharacterFailureInstance CreateFailure() => new UnbeatableWhiteLabelCharacterFailureInstance(this);

	public IGamemodeDescriptor GetGamemode() => ModStores.Get<GamemodeMod>().GetDescriptorByUID(UnbeatableWhiteLabelGamemode.UUID)!;
	public Type GetRuntimeType() => typeof(UnbeatableWhiteLabelBeatModel);
}
