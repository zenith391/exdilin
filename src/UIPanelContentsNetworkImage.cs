using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000411 RID: 1041
[RequireComponent(typeof(RawImage))]
public class UIPanelContentsNetworkImage : UIPanelElement
{
	// Token: 0x06002D56 RID: 11606 RVA: 0x00143B6B File Offset: 0x00141F6B
	public override void Init(UIPanelContents parentPanel)
	{
		this.image = base.GetComponent<RawImage>();
	}

	// Token: 0x06002D57 RID: 11607 RVA: 0x00143B7C File Offset: 0x00141F7C
	public override void Fill(Dictionary<string, string> data)
	{
		this.imageUrl = string.Empty;
		if (data.TryGetValue(this.imageUrlKey, out this.imageUrl) && !string.IsNullOrEmpty(this.imageUrl))
		{
			this.imageManager.AddListener(this.imageUrl, new ImageLoaderLoadedImageEventHandler(this.ImageLoaded));
			this.imageManager.LoadImage(this.imageUrl, base.gameObject.GetInstanceID());
			this.clear = false;
		}
		else
		{
			BWLog.Info("No data for key " + this.imageUrlKey);
			this.image.texture = this.missingImagePlaceholder;
			this.image.enabled = true;
		}
	}

	// Token: 0x06002D58 RID: 11608 RVA: 0x00143C34 File Offset: 0x00142034
	public override void Fill(string dataValue)
	{
		this.imageUrl = dataValue;
		if (!string.IsNullOrEmpty(this.imageUrl))
		{
			this.imageManager.AddListener(this.imageUrl, new ImageLoaderLoadedImageEventHandler(this.ImageLoaded));
			this.imageManager.LoadImage(this.imageUrl, base.gameObject.GetInstanceID());
			this.clear = false;
		}
	}

	// Token: 0x06002D59 RID: 11609 RVA: 0x00143C98 File Offset: 0x00142098
	public override void Clear()
	{
		if (this.image != null)
		{
			this.image.texture = null;
			this.image.enabled = false;
		}
		this.clear = true;
		if (!string.IsNullOrEmpty(this.imageUrl))
		{
			this.imageManager.ReleaseReference(this.imageUrl, base.gameObject.GetInstanceID());
			this.imageManager.RemoveListener(this.imageUrl, new ImageLoaderLoadedImageEventHandler(this.ImageLoaded));
		}
		this.imageUrl = string.Empty;
	}

	// Token: 0x06002D5A RID: 11610 RVA: 0x00143D29 File Offset: 0x00142129
	public void OnDestroy()
	{
		this.Clear();
	}

	// Token: 0x06002D5B RID: 11611 RVA: 0x00143D34 File Offset: 0x00142134
	private void ImageLoaded(string path, Texture2D imageTex, ImageLoader.LoadState loadState)
	{
		if (loadState == ImageLoader.LoadState.Loaded)
		{
			this.image.texture = imageTex;
			this.image.enabled = true;
			this.FixAspectRatio();
		}
		else
		{
			this.image.texture = this.missingImagePlaceholder;
			this.image.enabled = true;
			this.FixAspectRatio();
		}
	}

	// Token: 0x06002D5C RID: 11612 RVA: 0x00143D90 File Offset: 0x00142190
	private void FixAspectRatio()
	{
		if (!this.adjustAspectRatioFitter)
		{
			return;
		}
		if (this.image.texture == null)
		{
			return;
		}
		AspectRatioFitter component = base.GetComponent<AspectRatioFitter>();
		if (component == null)
		{
			return;
		}
		component.aspectRatio = (float)this.image.texture.width / (float)this.image.texture.height;
	}

	// Token: 0x040025E3 RID: 9699
	public string imageUrlKey;

	// Token: 0x040025E4 RID: 9700
	public Texture2D missingImagePlaceholder;

	// Token: 0x040025E5 RID: 9701
	private string imageUrl;

	// Token: 0x040025E6 RID: 9702
	private RawImage image;

	// Token: 0x040025E7 RID: 9703
	public bool adjustAspectRatioFitter = true;

	// Token: 0x040025E8 RID: 9704
	private bool clear;
}
