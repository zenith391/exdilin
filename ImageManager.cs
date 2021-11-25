using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x02000401 RID: 1025
public class ImageManager : MonoBehaviour
{
	// Token: 0x06002CD6 RID: 11478 RVA: 0x00140F88 File Offset: 0x0013F388
	private void Awake()
	{
		if (ImageManager.Instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		ImageManager.Instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	// Token: 0x06002CD7 RID: 11479 RVA: 0x00140FB7 File Offset: 0x0013F3B7
	private void Start()
	{
		base.StartCoroutine(this.ImageCleanup());
	}

	// Token: 0x06002CD8 RID: 11480 RVA: 0x00140FC8 File Offset: 0x0013F3C8
	public void LoadImage(string path, int referenceID)
	{
		ImageLoader imageLoader;
		if (!this.loaders.TryGetValue(path, out imageLoader))
		{
			imageLoader = new ImageLoader(path);
			this.loaders[path] = imageLoader;
		}
		imageLoader.references.Add(referenceID);
		if (imageLoader.loadState == ImageLoader.LoadState.NotLoaded)
		{
			if (this.useExampleData && Application.isEditor && !Application.isPlaying)
			{
				Debug.Log("Editor script, load immediate");
				this.LoadImageImmediate(imageLoader);
			}
			else
			{
				base.StartCoroutine(this.LoadImageCoroutine(imageLoader));
			}
		}
		else if (imageLoader.loadState == ImageLoader.LoadState.Loaded || imageLoader.loadState == ImageLoader.LoadState.Failed)
		{
			imageLoader.OnImageLoaded();
		}
	}

	// Token: 0x06002CD9 RID: 11481 RVA: 0x0014107C File Offset: 0x0013F47C
	public void SetUnloaded(string path)
	{
		ImageLoader imageLoader;
		if (this.loaders.TryGetValue(path, out imageLoader))
		{
			imageLoader.Reset();
		}
	}

	// Token: 0x06002CDA RID: 11482 RVA: 0x001410A4 File Offset: 0x0013F4A4
	private IEnumerator LoadImageCoroutine(ImageLoader loader)
	{
		loader.loadState = ImageLoader.LoadState.Loading;
		string path = loader.path;
		if (this.useExampleData)
		{
			path = BWFilesystem.FileProtocolPrefixStr + path;
		}
		WWW www = new WWW(path);
		while (!www.isDone)
		{
			yield return null;
		}
		if (loader.loadState == ImageLoader.LoadState.Cancelled)
		{
			yield break;
		}
		if (string.IsNullOrEmpty(www.error))
		{
			www.LoadImageIntoTexture(loader.tex);
			loader.loadState = ImageLoader.LoadState.Loaded;
		}
		else
		{
			BWLog.Info("Failed to load image from " + path + " error: " + www.error);
			loader.loadState = ImageLoader.LoadState.Failed;
		}
		loader.OnImageLoaded();
		yield break;
	}

	// Token: 0x06002CDB RID: 11483 RVA: 0x001410C8 File Offset: 0x0013F4C8
	private void LoadImageImmediate(ImageLoader loader)
	{
		string path = loader.path;
		if (File.Exists(path))
		{
			byte[] data = File.ReadAllBytes(path);
			loader.tex.LoadImage(data);
			loader.OnImageLoaded();
		}
	}

	// Token: 0x06002CDC RID: 11484 RVA: 0x00141104 File Offset: 0x0013F504
	public void ReleaseReference(string path, int referenceID)
	{
		ImageLoader imageLoader;
		if (this.loaders.TryGetValue(path, out imageLoader))
		{
			imageLoader.references.Remove(referenceID);
			if (imageLoader.references.Count == 0)
			{
				this.unusedImagePaths.Enqueue(path);
			}
		}
	}

	// Token: 0x06002CDD RID: 11485 RVA: 0x00141150 File Offset: 0x0013F550
	public void AddListener(string path, ImageLoaderLoadedImageEventHandler listener)
	{
		ImageLoader imageLoader;
		if (!this.loaders.TryGetValue(path, out imageLoader))
		{
			imageLoader = new ImageLoader(path);
			this.loaders[path] = imageLoader;
		}
		imageLoader.AddListener(listener);
	}

	// Token: 0x06002CDE RID: 11486 RVA: 0x0014118C File Offset: 0x0013F58C
	public void RemoveListener(string path, ImageLoaderLoadedImageEventHandler listener)
	{
		ImageLoader imageLoader;
		if (this.loaders.TryGetValue(path, out imageLoader))
		{
			imageLoader.RemoveListener(listener);
		}
	}

	// Token: 0x06002CDF RID: 11487 RVA: 0x001411B4 File Offset: 0x0013F5B4
	public void ClearListeners()
	{
		foreach (ImageLoader imageLoader in this.loaders.Values)
		{
			imageLoader.ClearListeners();
		}
	}

	// Token: 0x06002CE0 RID: 11488 RVA: 0x00141214 File Offset: 0x0013F614
	private IEnumerator ImageCleanup()
	{
		for (;;)
		{
			int loadedCount = this.loaders.Count;
			int unusedCount = this.unusedImagePaths.Count;
			while (loadedCount > this.maxLoadedImages && unusedCount > 0)
			{
				string purgeImagePath = this.unusedImagePaths.Dequeue();
				ImageLoader loader;
				if (this.loaders.TryGetValue(purgeImagePath, out loader) && loader.references.Count == 0)
				{
					loader.ClearImageData();
					this.loaders.Remove(purgeImagePath);
				}
				yield return null;
				loadedCount = this.loaders.Count;
				unusedCount = this.unusedImagePaths.Count;
			}
			yield return null;
		}
		yield break;
	}

	// Token: 0x0400256D RID: 9581
	public static ImageManager Instance;

	// Token: 0x0400256E RID: 9582
	private Dictionary<string, ImageLoader> loaders = new Dictionary<string, ImageLoader>();

	// Token: 0x0400256F RID: 9583
	private Queue<string> unusedImagePaths = new Queue<string>();

	// Token: 0x04002570 RID: 9584
	private int maxLoadedImages = 512;

	// Token: 0x04002571 RID: 9585
	public bool useExampleData;
}
