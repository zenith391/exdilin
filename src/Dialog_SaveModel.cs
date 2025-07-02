using UnityEngine;
using UnityEngine.UI;

public class Dialog_SaveModel : Dialog_TextInput
{
	public RawImage modelImage;

	public void SetModelImage(Texture2D image)
	{
		modelImage.texture = image;
	}
}
