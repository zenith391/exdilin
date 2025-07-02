using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageManager : MonoBehaviour
{
	public static ImageManager Instance;

	private Dictionary<string, ImageLoader> loaders = new Dictionary<string, ImageLoader>();

	private Queue<string> unusedImagePaths = new Queue<string>();

	private int maxLoadedImages = 512;

	public bool useExampleData;

	private void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		StartCoroutine(ImageCleanup());
	}

	public void LoadImage(string path, int referenceID)
	{
		if (!loaders.TryGetValue(path, out var value))
		{
			value = new ImageLoader(path);
			loaders[path] = value;
		}
		value.references.Add(referenceID);
		if (value.loadState == ImageLoader.LoadState.NotLoaded)
		{
			if (useExampleData && Application.isEditor && !Application.isPlaying)
			{
				Debug.Log("Editor script, load immediate");
				LoadImageImmediate(value);
			}
			else
			{
				StartCoroutine(LoadImageCoroutine(value));
			}
		}
		else if (value.loadState == ImageLoader.LoadState.Loaded || value.loadState == ImageLoader.LoadState.Failed)
		{
			value.OnImageLoaded();
		}
	}

	public void SetUnloaded(string path)
	{
		if (loaders.TryGetValue(path, out var value))
		{
			value.Reset();
		}
	}

	private IEnumerator LoadImageCoroutine(ImageLoader loader)
	{
		loader.loadState = ImageLoader.LoadState.Loading;
		string path = loader.path;
		if (useExampleData)
		{
			path = BWFilesystem.FileProtocolPrefixStr + path;
		}
		WWW www = new WWW(path);
		while (!www.isDone)
		{
			yield return null;
		}
		if (loader.loadState != ImageLoader.LoadState.Cancelled)
		{
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
		}
	}

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

	public void ReleaseReference(string path, int referenceID)
	{
		if (loaders.TryGetValue(path, out var value))
		{
			value.references.Remove(referenceID);
			if (value.references.Count == 0)
			{
				unusedImagePaths.Enqueue(path);
			}
		}
	}

	public void AddListener(string path, ImageLoaderLoadedImageEventHandler listener)
	{
		if (!loaders.TryGetValue(path, out var value))
		{
			value = new ImageLoader(path);
			loaders[path] = value;
		}
		value.AddListener(listener);
	}

	public void RemoveListener(string path, ImageLoaderLoadedImageEventHandler listener)
	{
		if (loaders.TryGetValue(path, out var value))
		{
			value.RemoveListener(listener);
		}
	}

	public void ClearListeners()
	{
		foreach (ImageLoader value in loaders.Values)
		{
			value.ClearListeners();
		}
	}

	private IEnumerator ImageCleanup()
	{
		while (true)
		{
			int count = loaders.Count;
			int count2 = unusedImagePaths.Count;
			while (count > maxLoadedImages && count2 > 0)
			{
				string key = unusedImagePaths.Dequeue();
				if (loaders.TryGetValue(key, out var value) && value.references.Count == 0)
				{
					value.ClearImageData();
					loaders.Remove(key);
				}
				yield return null;
				count = loaders.Count;
				count2 = unusedImagePaths.Count;
			}
			yield return null;
		}
	}
}
