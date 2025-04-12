using System;
using UnityEngine;

// Token: 0x020002CB RID: 715
public class TileIconHandle
{
	// Token: 0x060020BE RID: 8382 RVA: 0x000EFF23 File Offset: 0x000EE323
	public virtual void SetFilePath(string path)
	{
		this.filePath = path;
	}

	// Token: 0x060020BF RID: 8383 RVA: 0x000EFF2C File Offset: 0x000EE32C
	public virtual string GetFilePath()
	{
		return this.filePath;
	}

	// Token: 0x060020C0 RID: 8384 RVA: 0x000EFF34 File Offset: 0x000EE334
	public virtual bool CheckFileExists()
	{
		return !string.IsNullOrEmpty(this.filePath) && BWFilesystem.FileExists(this.filePath);
	}

	// Token: 0x060020C1 RID: 8385 RVA: 0x000EFF60 File Offset: 0x000EE360
	public virtual void StartLoad()
	{
		if (!string.IsNullOrEmpty(this.filePath))
		{
			this.fileLoader = new WWW(this.filePath);
			this.loadState = TileIconLoadState.Loading;
		}
		else
		{
			BWLog.Error("Starting tile load but file path is null or empty");
			this.CancelLoad();
		}
	}

	// Token: 0x060020C2 RID: 8386 RVA: 0x000EFFA0 File Offset: 0x000EE3A0
	public virtual bool UpdateLoad()
	{
		bool isDone = this.fileLoader.isDone;
		if (!string.IsNullOrEmpty(this.fileLoader.error))
		{
			BWLog.Error("Icon Load Error: " + this.fileLoader.error);
			this.CancelLoad();
			return false;
		}
		if (isDone)
		{
			this.loadState = TileIconLoadState.Loaded;
			return true;
		}
		return false;
	}

	// Token: 0x060020C3 RID: 8387 RVA: 0x000F0000 File Offset: 0x000EE400
	public virtual void CancelLoad()
	{
		if (this.fileLoader != null)
		{
			this.fileLoader = null;
		}
		this.filePath = string.Empty;
		this.loadState = TileIconLoadState.Unloaded;
	}

	// Token: 0x060020C4 RID: 8388 RVA: 0x000F0028 File Offset: 0x000EE428
	public virtual void ApplyTexture(Texture2D tex)
	{
		this.fileLoader.LoadImageIntoTexture(tex);
		if (!string.IsNullOrEmpty(this.fileLoader.error))
		{
			BWLog.Error(this.fileLoader.error);
		}
		this.loadState = TileIconLoadState.Applied;
		this.fileLoader = null;
	}

	// Token: 0x04001BD1 RID: 7121
	public TileIconLoadState loadState;

	// Token: 0x04001BD2 RID: 7122
	protected string filePath = string.Empty;

	// Token: 0x04001BD3 RID: 7123
	private WWW fileLoader;
}
