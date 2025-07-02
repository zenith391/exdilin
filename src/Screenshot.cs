using System.IO;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
	private void Start()
	{
		BWLog.Info("Press F2 to save screenshot");
	}

	private void Update()
	{
		if (MappedInput.InputDown(MappableInput.SCREENSHOT))
		{
			SaveScreenshot();
		}
	}

	private void SaveScreenshot()
	{
		BWLog.Info("Generating screenshot...");
		int num = 4096;
		int num2 = 3072;
		RenderTexture renderTexture = new RenderTexture(num, num2, 24);
		Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, mipmap: false);
		Camera.main.targetTexture = renderTexture;
		Camera.main.Render();
		Camera.main.targetTexture = null;
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, num, num2), 0, 0);
		RenderTexture.active = null;
		byte[] bytes = texture2D.EncodeToPNG();
		Object.Destroy(texture2D);
		Object.Destroy(renderTexture);
		string text = Application.dataPath + "/../Planet.png";
		File.WriteAllBytes(text, bytes);
		BWLog.Info("Wrote: " + text);
	}
}
