using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UISceneUserImageDisplay : UISceneInfoDisplay
{
	public Texture2D missingImagePlaceholder;

	private string imageUrl;

	private RawImage image;

	public void OnEnable()
	{
		image = GetComponent<RawImage>();
		image.enabled = false;
	}

	public void OnDisable()
	{
		if (MainUIController.active && !string.IsNullOrEmpty(imageUrl))
		{
			MainUIController.Instance.imageManager.RemoveListener(imageUrl, ImageLoaded);
			MainUIController.Instance.imageManager.ReleaseReference(imageUrl, base.gameObject.GetInstanceID());
		}
	}

	public override void Setup(UISceneInfo sceneInfo)
	{
		if (MainUIController.active)
		{
			imageUrl = sceneInfo.userImageUrl;
			if (!string.IsNullOrEmpty(imageUrl))
			{
				MainUIController.Instance.imageManager.AddListener(imageUrl, ImageLoaded);
				MainUIController.Instance.imageManager.LoadImage(imageUrl, base.gameObject.GetInstanceID());
			}
		}
	}

	private void ImageLoaded(string path, Texture2D imageTex, ImageLoader.LoadState loadState)
	{
		if (loadState == ImageLoader.LoadState.Loaded)
		{
			image.texture = imageTex;
			image.enabled = true;
		}
		else
		{
			image.texture = missingImagePlaceholder;
			image.enabled = true;
		}
	}
}
