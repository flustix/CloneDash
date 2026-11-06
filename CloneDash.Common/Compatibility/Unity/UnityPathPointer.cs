using System.Text.Json.Serialization;

namespace CloneDash.Compatibility.Unity;

public readonly struct UnityPathPointer : IEquatable<UnityPathPointer>
{
	[JsonPropertyName("m_FileID")]
	public long FileID { get; init; }

	[JsonPropertyName("m_PathID")]
	public long PathID { get; init; }

	public bool Equals(UnityPathPointer other) => FileID == other.FileID && PathID == other.PathID;
	public override bool Equals(object? obj) => obj is UnityPathPointer other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(FileID, PathID);
}