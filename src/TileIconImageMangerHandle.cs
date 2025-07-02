using UnityEngine;

public class TileIconImageMangerHandle : TileIconHandle
{
	private int referenceID;

	private bool loaded;

	private Texture2D imageManagerTexture;

	public TileIconImageMangerHandle(int refID)
	{
		referenceID = refID;
	}

	public override bool CheckFileExists()
	{
		return !string.IsNullOrEmpty(filePath);
	}

	public override void StartLoad()
	{
		if (string.IsNullOrEmpty(filePath))
		{
			BWLog.Error("Starting tile load but resource path is null or empty");
			CancelLoad();
		}
		else
		{
			loadState = TileIconLoadState.Loading;
			ImageManager.Instance.AddListener(filePath, ImageMangerLoadedImage);
			ImageManager.Instance.LoadImage(filePath, referenceID);
		}
	}

	public override bool UpdateLoad()
	{
		return loadState == TileIconLoadState.Loaded;
	}

	public override void CancelLoad()
	{
		if (ImageManager.Instance != null)
		{
			ImageManager.Instance.RemoveListener(filePath, ImageMangerLoadedImage);
		}
		filePath = string.Empty;
		loadState = TileIconLoadState.Unloaded;
	}

	public override void ApplyTexture(Texture2D tex)
	{
		if (imageManagerTexture == null)
		{
			BWLog.Error("imageManager didn't load image " + filePath);
			CancelLoad();
			return;
		}
		int width = imageManagerTexture.width;
		int height = imageManagerTexture.height;
		tex.Resize(width, height);
		tex.SetPixels(imageManagerTexture.GetPixels());
		tex.Apply();
		loadState = TileIconLoadState.Applied;
	}

	private void ImageMangerLoadedImage(string path, Texture2D imageTex, ImageLoader.LoadState imageManagerLoadState)
	{
		if (imageManagerLoadState == ImageLoader.LoadState.Loaded)
		{
			imageManagerTexture = imageTex;
			loadState = TileIconLoadState.Loaded;
		}
		else
		{
			CancelLoad();
		}
	}
}
