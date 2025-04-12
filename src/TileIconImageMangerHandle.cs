using System;
using UnityEngine;

// Token: 0x020002CD RID: 717
public class TileIconImageMangerHandle : TileIconHandle
{
	// Token: 0x060020CD RID: 8397 RVA: 0x000F01BB File Offset: 0x000EE5BB
	public TileIconImageMangerHandle(int refID)
	{
		this.referenceID = refID;
	}

	// Token: 0x060020CE RID: 8398 RVA: 0x000F01CA File Offset: 0x000EE5CA
	public override bool CheckFileExists()
	{
		return !string.IsNullOrEmpty(this.filePath);
	}

	// Token: 0x060020CF RID: 8399 RVA: 0x000F01E0 File Offset: 0x000EE5E0
	public override void StartLoad()
	{
		if (string.IsNullOrEmpty(this.filePath))
		{
			BWLog.Error("Starting tile load but resource path is null or empty");
			this.CancelLoad();
			return;
		}
		this.loadState = TileIconLoadState.Loading;
		ImageManager.Instance.AddListener(this.filePath, new ImageLoaderLoadedImageEventHandler(this.ImageMangerLoadedImage));
		ImageManager.Instance.LoadImage(this.filePath, this.referenceID);
	}

	// Token: 0x060020D0 RID: 8400 RVA: 0x000F0247 File Offset: 0x000EE647
	public override bool UpdateLoad()
	{
		return this.loadState == TileIconLoadState.Loaded;
	}

	// Token: 0x060020D1 RID: 8401 RVA: 0x000F0252 File Offset: 0x000EE652
	public override void CancelLoad()
	{
		if (ImageManager.Instance != null)
		{
			ImageManager.Instance.RemoveListener(this.filePath, new ImageLoaderLoadedImageEventHandler(this.ImageMangerLoadedImage));
		}
		this.filePath = string.Empty;
		this.loadState = TileIconLoadState.Unloaded;
	}

	// Token: 0x060020D2 RID: 8402 RVA: 0x000F0294 File Offset: 0x000EE694
	public override void ApplyTexture(Texture2D tex)
	{
		if (this.imageManagerTexture == null)
		{
			BWLog.Error("imageManager didn't load image " + this.filePath);
			this.CancelLoad();
			return;
		}
		int width = this.imageManagerTexture.width;
		int height = this.imageManagerTexture.height;
		tex.Resize(width, height);
		tex.SetPixels(this.imageManagerTexture.GetPixels());
		tex.Apply();
		this.loadState = TileIconLoadState.Applied;
	}

	// Token: 0x060020D3 RID: 8403 RVA: 0x000F030D File Offset: 0x000EE70D
	private void ImageMangerLoadedImage(string path, Texture2D imageTex, ImageLoader.LoadState imageManagerLoadState)
	{
		if (imageManagerLoadState == ImageLoader.LoadState.Loaded)
		{
			this.imageManagerTexture = imageTex;
			this.loadState = TileIconLoadState.Loaded;
		}
		else
		{
			this.CancelLoad();
		}
	}

	// Token: 0x04001BD5 RID: 7125
	private int referenceID;

	// Token: 0x04001BD6 RID: 7126
	private bool loaded;

	// Token: 0x04001BD7 RID: 7127
	private Texture2D imageManagerTexture;
}
