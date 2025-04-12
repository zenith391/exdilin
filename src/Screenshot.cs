using System;
using System.IO;
using UnityEngine;

// Token: 0x02000223 RID: 547
public class Screenshot : MonoBehaviour
{
	// Token: 0x06001A93 RID: 6803 RVA: 0x000C2E89 File Offset: 0x000C1289
	private void Start()
	{
		BWLog.Info("Press F2 to save screenshot");
	}

	// Token: 0x06001A94 RID: 6804 RVA: 0x000C2E95 File Offset: 0x000C1295
	private void Update()
	{
		if (MappedInput.InputDown(MappableInput.SCREENSHOT))
		{
			this.SaveScreenshot();
		}
	}

	// Token: 0x06001A95 RID: 6805 RVA: 0x000C2EAC File Offset: 0x000C12AC
	private void SaveScreenshot()
	{
		BWLog.Info("Generating screenshot...");
		int num = 4096;
		int num2 = 3072;
		RenderTexture renderTexture = new RenderTexture(num, num2, 24);
		Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, false);
		Camera.main.targetTexture = renderTexture;
		Camera.main.Render();
		Camera.main.targetTexture = null;
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, (float)num, (float)num2), 0, 0);
		RenderTexture.active = null;
		byte[] bytes = texture2D.EncodeToPNG();
		UnityEngine.Object.Destroy(texture2D);
		UnityEngine.Object.Destroy(renderTexture);
		string text = Application.dataPath + "/../Planet.png";
		File.WriteAllBytes(text, bytes);
		BWLog.Info("Wrote: " + text);
	}
}
