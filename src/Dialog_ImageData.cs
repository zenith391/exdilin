using UnityEngine;
using UnityEngine.UI;

public class Dialog_ImageData : Dialog_Generic
{
	public RawImage image;

	public void LoadImageFromData(byte[] imageData)
	{
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGB24, mipmap: false);
		texture2D.LoadImage(imageData);
		image.texture = texture2D;
	}
}
