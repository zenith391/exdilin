using Exdilin;
using UnityEngine;

public class TileIconResourceHandle : TileIconHandle
{
	private Texture2D assetTex;

	private ResourceRequest resourceLoader;

	public override void SetFilePath(string path)
	{
		filePath = path;
	}

	public override string GetFilePath()
	{
		return filePath;
	}

	public override bool CheckFileExists()
	{
		return !string.IsNullOrEmpty(filePath);
	}

	public override void StartLoad()
	{
		if (!string.IsNullOrEmpty(filePath))
		{
			string fullId = filePath.Replace('\\', '/');
			if (AssetsManager.HasObject(fullId))
			{
				assetTex = AssetsManager.GetTexture(fullId);
				loadState = TileIconLoadState.Loaded;
			}
			else
			{
				resourceLoader = Resources.LoadAsync<Texture2D>(filePath);
				loadState = TileIconLoadState.Loading;
			}
		}
		else
		{
			BWLog.Error("Starting tile load but resource path is null or empty");
			CancelLoad();
		}
	}

	public override bool UpdateLoad()
	{
		if (resourceLoader.isDone)
		{
			loadState = TileIconLoadState.Loaded;
			return true;
		}
		return false;
	}

	public override void CancelLoad()
	{
		if (resourceLoader != null)
		{
			resourceLoader = null;
		}
		filePath = string.Empty;
		loadState = TileIconLoadState.Unloaded;
	}

	public Texture2D Downscale(Texture2D src, int dstWidth, int dstHeight)
	{
		float num = (float)src.width / (float)dstWidth;
		float num2 = (float)src.height / (float)dstHeight;
		Texture2D texture2D = new Texture2D(dstWidth, dstHeight, src.format, mipmap: false);
		for (int i = 0; i < dstWidth; i++)
		{
			for (int j = 0; j < dstHeight; j++)
			{
				int x = Mathf.RoundToInt((float)i * num);
				int y = Mathf.RoundToInt((float)j * num2);
				texture2D.SetPixel(i, j, src.GetPixel(x, y));
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public Texture2D ToFormat(Texture2D src, TextureFormat dst)
	{
		if (src.format == dst)
		{
			return src;
		}
		Texture2D texture2D = new Texture2D(src.width, src.height, dst, mipmap: false);
		texture2D.SetPixels32(src.GetPixels32());
		texture2D.Apply();
		return texture2D;
	}

	public override void ApplyTexture(Texture2D tex)
	{
		if (assetTex != null)
		{
			if (assetTex.width > 128 && assetTex.height > 128)
			{
				assetTex = Downscale(assetTex, 128, 128);
			}
			assetTex = ToFormat(assetTex, tex.format);
			tex.Resize(assetTex.width, assetTex.height);
			tex.LoadRawTextureData(assetTex.GetRawTextureData());
			tex.Apply();
			loadState = TileIconLoadState.Applied;
			assetTex = null;
		}
		else if (resourceLoader.asset == null)
		{
			BWLog.Error("No asset at path " + filePath);
			CancelLoad();
		}
		else
		{
			Texture2D texture2D = (Texture2D)resourceLoader.asset;
			int width = texture2D.width;
			int height = texture2D.height;
			tex.Resize(width, height);
			tex.LoadRawTextureData(texture2D.GetRawTextureData());
			tex.Apply();
			loadState = TileIconLoadState.Applied;
			Resources.UnloadAsset(resourceLoader.asset);
			resourceLoader = null;
		}
	}
}
