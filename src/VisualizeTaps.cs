using System;
using UnityEngine;

// Token: 0x0200024A RID: 586
public static class VisualizeTaps
{
	// Token: 0x06001B05 RID: 6917 RVA: 0x000C5EC4 File Offset: 0x000C42C4
	public static void On()
	{
		if (VisualizeTaps.masterObject != null)
		{
			return;
		}
		if (VisualizeTaps.tapTexture == null)
		{
			string path = "GUI/SimFingerActivePointer";
			VisualizeTaps.tapTexture = (Resources.Load(path) as Texture2D);
		}
		if (VisualizeTaps.tapTexture == null)
		{
			BWLog.Warning("Failed to load tap Texture");
			return;
		}
		VisualizeTaps.masterObject = new GameObject("VisualizeTaps");
		VisualizeTapsMB visualizeTapsMB = VisualizeTaps.masterObject.AddComponent<VisualizeTapsMB>();
		visualizeTapsMB.Setup(VisualizeTaps.masterObject, VisualizeTaps.tapTexture);
	}

	// Token: 0x06001B06 RID: 6918 RVA: 0x000C5F4D File Offset: 0x000C434D
	public static void Off()
	{
		UnityEngine.Object.Destroy(VisualizeTaps.masterObject);
		VisualizeTaps.masterObject = null;
	}

	// Token: 0x04001707 RID: 5895
	private static GameObject masterObject;

	// Token: 0x04001708 RID: 5896
	private static Texture2D tapTexture;
}
