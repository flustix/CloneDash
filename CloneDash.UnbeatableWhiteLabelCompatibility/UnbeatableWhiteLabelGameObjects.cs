using AssetStudio;

namespace CloneDash.Unbeatable;

public static partial class UnbeatableCompatibility
{
	public static GameObject? FindRootGameObject(string fileName, string name) {
		if (Assets is null)
			return null;
		
		foreach (var file in Assets.AssetsFileList) {
			if (file.fileName != fileName)
				continue;

			foreach (var obj in file.Objects)
				if (obj is GameObject gameObject && gameObject.m_Name == name && !gameObject.m_Transform.m_Father.TryGet(out _))
					return gameObject;
		}

		return null;
	}
}
