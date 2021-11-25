using System;
using UnityEngine;

// Token: 0x020002CC RID: 716
using Exdilin;
public class TileIconResourceHandle : TileIconHandle
{
	private Texture2D assetTex;
	// Token: 0x060020C6 RID: 8390 RVA: 0x000F007C File Offset: 0x000EE47C
	public override void SetFilePath(string path)
	{
		this.filePath = path;
	}

	// Token: 0x060020C7 RID: 8391 RVA: 0x000F0085 File Offset: 0x000EE485
	public override string GetFilePath()
	{
		return this.filePath;
	}

	// Token: 0x060020C8 RID: 8392 RVA: 0x000F008D File Offset: 0x000EE48D
	public override bool CheckFileExists()
	{
		return !string.IsNullOrEmpty(this.filePath);
	}

	// Token: 0x060020C9 RID: 8393 RVA: 0x000F00A2 File Offset: 0x000EE4A2
	public override void StartLoad()
	{
		if (!string.IsNullOrEmpty(this.filePath))
		{
			string sanitized = filePath.Replace('\\', '/');
			if (AssetsManager.HasObject(sanitized)) {
				assetTex = AssetsManager.GetTexture(sanitized);
				this.loadState = TileIconLoadState.Loaded;
				return;
			}
			this.resourceLoader = Resources.LoadAsync<Texture2D>(this.filePath);
			this.loadState = TileIconLoadState.Loading;
		}
		else
		{
			BWLog.Error("Starting tile load but resource path is null or empty");
			this.CancelLoad();
		}
	}

	// Token: 0x060020CA RID: 8394 RVA: 0x000F00E1 File Offset: 0x000EE4E1
	public override bool UpdateLoad()
	{
		if (this.resourceLoader.isDone)
		{
			this.loadState = TileIconLoadState.Loaded;
			return true;
		}
		return false;
	}

	// Token: 0x060020CB RID: 8395 RVA: 0x000F00FD File Offset: 0x000EE4FD
	public override void CancelLoad()
	{
		if (this.resourceLoader != null)
		{
			this.resourceLoader = null;
		}
		this.filePath = string.Empty;
		this.loadState = TileIconLoadState.Unloaded;
	}

	public Texture2D Downscale(Texture2D src, int dstWidth, int dstHeight) {
		float wFactor = src.width / (float) dstWidth;
		float hFactor = src.height / (float) dstHeight;
		Texture2D dst = new Texture2D(dstWidth, dstHeight, src.format, false);
		for (int x = 0; x < dstWidth; x++) {
			for (int y = 0; y < dstHeight; y++) {
				int sx = Mathf.RoundToInt(x * wFactor);
				int sy = Mathf.RoundToInt(y * hFactor);
				dst.SetPixel(x, y, src.GetPixel(sx, sy));
			}
		}
		dst.Apply();
		return dst;
	}

	public Texture2D ToFormat(Texture2D src, TextureFormat dst) {
		if (src.format == dst) {
			return src;
		} else {
			Texture2D tex2D = new Texture2D(src.width, src.height, dst, false);
			tex2D.SetPixels32(src.GetPixels32());
			tex2D.Apply();
			return tex2D;
		}
	}

	// Token: 0x060020CC RID: 8396 RVA: 0x000F0124 File Offset: 0x000EE524
	public override void ApplyTexture(Texture2D tex)
	{
		if (assetTex != null) {
			if (assetTex.width > 128 && assetTex.height > 128) { // maximum size: 64x64
				assetTex = Downscale(assetTex, 128, 128);
			}
			assetTex = ToFormat(assetTex, tex.format);
			tex.Resize(assetTex.width, assetTex.height);
			tex.LoadRawTextureData(assetTex.GetRawTextureData());
			tex.Apply();
			this.loadState = TileIconLoadState.Applied;
			assetTex = null;
			return;
		}
		if (this.resourceLoader.asset == null)
		{
			BWLog.Error("No asset at path " + this.filePath);
			this.CancelLoad();
			return;
		}
		Texture2D texture2D = (Texture2D) resourceLoader.asset;
		int width = texture2D.width;
		int height = texture2D.height;
		tex.Resize(width, height);
		tex.LoadRawTextureData(texture2D.GetRawTextureData());
		tex.Apply();
		this.loadState = TileIconLoadState.Applied;
		Resources.UnloadAsset(this.resourceLoader.asset);
		this.resourceLoader = null;
	}

	// Token: 0x04001BD4 RID: 7124
	private ResourceRequest resourceLoader;
}
