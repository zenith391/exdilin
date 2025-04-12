using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020003FF RID: 1023
public class ImageLoader
{
	// Token: 0x06002CCC RID: 11468 RVA: 0x00140E3F File Offset: 0x0013F23F
	public ImageLoader(string path)
	{
		this.path = path;
		this.tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
		this.loadState = ImageLoader.LoadState.NotLoaded;
		this.references = new HashSet<int>();
	}

	// Token: 0x14000020 RID: 32
	// (add) Token: 0x06002CCD RID: 11469 RVA: 0x00140E70 File Offset: 0x0013F270
	// (remove) Token: 0x06002CCE RID: 11470 RVA: 0x00140EA8 File Offset: 0x0013F2A8
	public event ImageLoaderLoadedImageEventHandler onImageLoaded;

	// Token: 0x06002CCF RID: 11471 RVA: 0x00140EDE File Offset: 0x0013F2DE
	public void AddListener(ImageLoaderLoadedImageEventHandler listener)
	{
		this.onImageLoaded -= listener;
		this.onImageLoaded += listener;
	}

	// Token: 0x06002CD0 RID: 11472 RVA: 0x00140EEE File Offset: 0x0013F2EE
	public void RemoveListener(ImageLoaderLoadedImageEventHandler listener)
	{
		this.onImageLoaded -= listener;
	}

	// Token: 0x06002CD1 RID: 11473 RVA: 0x00140EF7 File Offset: 0x0013F2F7
	public void ClearListeners()
	{
		this.onImageLoaded = null;
	}

	// Token: 0x06002CD2 RID: 11474 RVA: 0x00140F00 File Offset: 0x0013F300
	public void Reset()
	{
		this.loadState = ImageLoader.LoadState.NotLoaded;
	}

	// Token: 0x06002CD3 RID: 11475 RVA: 0x00140F09 File Offset: 0x0013F309
	public void ClearImageData()
	{
		this.loadState = ImageLoader.LoadState.Cancelled;
		if (this.tex != null)
		{
			UnityEngine.Object.Destroy(this.tex);
		}
		this.tex = null;
	}

	// Token: 0x06002CD4 RID: 11476 RVA: 0x00140F35 File Offset: 0x0013F335
	public void OnImageLoaded()
	{
		if (this.onImageLoaded != null)
		{
			this.onImageLoaded(this.path, this.tex, this.loadState);
		}
	}

	// Token: 0x04002562 RID: 9570
	public string path;

	// Token: 0x04002563 RID: 9571
	public ImageLoader.LoadState loadState;

	// Token: 0x04002565 RID: 9573
	public Texture2D tex;

	// Token: 0x04002566 RID: 9574
	public HashSet<int> references;

	// Token: 0x02000400 RID: 1024
	public enum LoadState
	{
		// Token: 0x04002568 RID: 9576
		NotLoaded,
		// Token: 0x04002569 RID: 9577
		Loading,
		// Token: 0x0400256A RID: 9578
		Loaded,
		// Token: 0x0400256B RID: 9579
		Failed,
		// Token: 0x0400256C RID: 9580
		Cancelled
	}
}
