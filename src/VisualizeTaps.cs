using UnityEngine;

public static class VisualizeTaps
{
	private static GameObject masterObject;

	private static Texture2D tapTexture;

	public static void On()
	{
		if (!(masterObject != null))
		{
			if (tapTexture == null)
			{
				string path = "GUI/SimFingerActivePointer";
				tapTexture = Resources.Load(path) as Texture2D;
			}
			if (tapTexture == null)
			{
				BWLog.Warning("Failed to load tap Texture");
				return;
			}
			masterObject = new GameObject("VisualizeTaps");
			VisualizeTapsMB visualizeTapsMB = masterObject.AddComponent<VisualizeTapsMB>();
			visualizeTapsMB.Setup(masterObject, tapTexture);
		}
	}

	public static void Off()
	{
		Object.Destroy(masterObject);
		masterObject = null;
	}
}
