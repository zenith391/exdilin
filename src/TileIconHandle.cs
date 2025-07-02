using UnityEngine;

public class TileIconHandle
{
	public TileIconLoadState loadState;

	protected string filePath = string.Empty;

	private WWW fileLoader;

	public virtual void SetFilePath(string path)
	{
		filePath = path;
	}

	public virtual string GetFilePath()
	{
		return filePath;
	}

	public virtual bool CheckFileExists()
	{
		if (!string.IsNullOrEmpty(filePath))
		{
			return BWFilesystem.FileExists(filePath);
		}
		return false;
	}

	public virtual void StartLoad()
	{
		if (!string.IsNullOrEmpty(filePath))
		{
			fileLoader = new WWW(filePath);
			loadState = TileIconLoadState.Loading;
		}
		else
		{
			BWLog.Error("Starting tile load but file path is null or empty");
			CancelLoad();
		}
	}

	public virtual bool UpdateLoad()
	{
		bool isDone = fileLoader.isDone;
		if (!string.IsNullOrEmpty(fileLoader.error))
		{
			BWLog.Error("Icon Load Error: " + fileLoader.error);
			CancelLoad();
			return false;
		}
		if (isDone)
		{
			loadState = TileIconLoadState.Loaded;
			return true;
		}
		return false;
	}

	public virtual void CancelLoad()
	{
		if (fileLoader != null)
		{
			fileLoader = null;
		}
		filePath = string.Empty;
		loadState = TileIconLoadState.Unloaded;
	}

	public virtual void ApplyTexture(Texture2D tex)
	{
		fileLoader.LoadImageIntoTexture(tex);
		if (!string.IsNullOrEmpty(fileLoader.error))
		{
			BWLog.Error(fileLoader.error);
		}
		loadState = TileIconLoadState.Applied;
		fileLoader = null;
	}
}
