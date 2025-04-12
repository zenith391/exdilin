using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020002ED RID: 749
public class Dialog_ImageData : Dialog_Generic
{
	// Token: 0x06002215 RID: 8725 RVA: 0x000FEF28 File Offset: 0x000FD328
	public void LoadImageFromData(byte[] imageData)
	{
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGB24, false);
		texture2D.LoadImage(imageData);
		this.image.texture = texture2D;
	}

	// Token: 0x04001D20 RID: 7456
	public RawImage image;
}
