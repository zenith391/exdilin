using System.Collections.Generic;
using UnityEngine;

public class ImageLoader
{
	public enum LoadState
	{
		NotLoaded,
		Loading,
		Loaded,
		Failed,
		Cancelled
	}

	public string path;

	public LoadState loadState;

	public Texture2D tex;

	public HashSet<int> references;

	public event ImageLoaderLoadedImageEventHandler onImageLoaded;

	public ImageLoader(string path)
	{
		this.path = path;
		tex = new Texture2D(4, 4, TextureFormat.RGBA32, mipmap: false);
		loadState = LoadState.NotLoaded;
		references = new HashSet<int>();
	}

	public void AddListener(ImageLoaderLoadedImageEventHandler listener)
	{
		onImageLoaded -= listener;
		onImageLoaded += listener;
	}

	public void RemoveListener(ImageLoaderLoadedImageEventHandler listener)
	{
		onImageLoaded -= listener;
	}

	public void ClearListeners()
	{
		this.onImageLoaded = null;
	}

	public void Reset()
	{
		loadState = LoadState.NotLoaded;
	}

	public void ClearImageData()
	{
		loadState = LoadState.Cancelled;
		if (tex != null)
		{
			Object.Destroy(tex);
		}
		tex = null;
	}

	public void OnImageLoaded()
	{
		if (this.onImageLoaded != null)
		{
			this.onImageLoaded(path, tex, loadState);
		}
	}
}
