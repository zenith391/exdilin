using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000433 RID: 1075
[RequireComponent(typeof(RawImage))]
public class UISceneUserImageDisplay : UISceneInfoDisplay
{
	// Token: 0x06002E44 RID: 11844 RVA: 0x00149A71 File Offset: 0x00147E71
	public void OnEnable()
	{
		this.image = base.GetComponent<RawImage>();
		this.image.enabled = false;
	}

	// Token: 0x06002E45 RID: 11845 RVA: 0x00149A8C File Offset: 0x00147E8C
	public void OnDisable()
	{
		if (!MainUIController.active)
		{
			return;
		}
		if (!string.IsNullOrEmpty(this.imageUrl))
		{
			MainUIController.Instance.imageManager.RemoveListener(this.imageUrl, new ImageLoaderLoadedImageEventHandler(this.ImageLoaded));
			MainUIController.Instance.imageManager.ReleaseReference(this.imageUrl, base.gameObject.GetInstanceID());
		}
	}

	// Token: 0x06002E46 RID: 11846 RVA: 0x00149AF8 File Offset: 0x00147EF8
	public override void Setup(UISceneInfo sceneInfo)
	{
		if (!MainUIController.active)
		{
			return;
		}
		this.imageUrl = sceneInfo.userImageUrl;
		if (!string.IsNullOrEmpty(this.imageUrl))
		{
			MainUIController.Instance.imageManager.AddListener(this.imageUrl, new ImageLoaderLoadedImageEventHandler(this.ImageLoaded));
			MainUIController.Instance.imageManager.LoadImage(this.imageUrl, base.gameObject.GetInstanceID());
		}
	}

	// Token: 0x06002E47 RID: 11847 RVA: 0x00149B70 File Offset: 0x00147F70
	private void ImageLoaded(string path, Texture2D imageTex, ImageLoader.LoadState loadState)
	{
		if (loadState == ImageLoader.LoadState.Loaded)
		{
			this.image.texture = imageTex;
			this.image.enabled = true;
		}
		else
		{
			this.image.texture = this.missingImagePlaceholder;
			this.image.enabled = true;
		}
	}

	// Token: 0x040026BE RID: 9918
	public Texture2D missingImagePlaceholder;

	// Token: 0x040026BF RID: 9919
	private string imageUrl;

	// Token: 0x040026C0 RID: 9920
	private RawImage image;
}
