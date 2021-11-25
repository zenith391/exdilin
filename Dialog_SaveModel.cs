using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020002EF RID: 751
public class Dialog_SaveModel : Dialog_TextInput
{
	// Token: 0x0600221E RID: 8734 RVA: 0x000FF2D9 File Offset: 0x000FD6D9
	public void SetModelImage(Texture2D image)
	{
		this.modelImage.texture = image;
	}

	// Token: 0x04001D24 RID: 7460
	public RawImage modelImage;
}
