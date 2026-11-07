using AssetStudio;

namespace CloneDash.Compatibility.Unity;

public static class AssetExtensions
{
	extension(SerializedFile file)
	{
		public AssetStudio.Object? GetObjectByPath(UnityPathPointer pointer) {
			switch (pointer.FileID) {
				case 0:
					return file.ObjectsDic.GetValueOrDefault(pointer.PathID, null);
			}
			
			return null;
		}
	}
}