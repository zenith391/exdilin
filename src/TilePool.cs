using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002D2 RID: 722
public class TilePool
{
	// Token: 0x06002105 RID: 8453 RVA: 0x000F1D14 File Offset: 0x000F0114
	public TilePool(string name, int capacity, TilePool.TileImageSource imageSource)
	{
		this.poolTransform = new GameObject(name + " TilePool").transform;
		this.available = new List<TileObject>(capacity);
		this.inUse = new List<TileObject>();
		this.imageSource = imageSource;
		for (int i = 0; i < capacity; i++)
		{
			this.GrowPool();
		}
	}

	// Token: 0x06002106 RID: 8454 RVA: 0x000F1D78 File Offset: 0x000F0178
	public TileObject GetTileObject(GAF gaf, bool enabled, bool asOverlay = false)
	{
		if (this.available.Count == 0)
		{
			BWLog.Warning("Tile pool is empty, growing");
			this.GrowPool();
		}
		TileObject tileObject = this.available[0];
		this.available.RemoveAt(0);
		tileObject.isOverlay = asOverlay;
		tileObject.Setup(gaf, enabled);
		this.inUse.Add(tileObject);
		tileObject.poolIndex = this.inUse.Count - 1;
		return tileObject;
	}

	// Token: 0x06002107 RID: 8455 RVA: 0x000F1DF0 File Offset: 0x000F01F0
	public TileObject GetTileObjectForIcon(string iconName, bool enabled)
	{
		if (this.available.Count == 0)
		{
			BWLog.Warning("Tile pool is empty, growing");
			this.GrowPool();
		}
		TileObject tileObject = this.available[0];
		this.available.RemoveAt(0);
		tileObject.SetupForIcon(iconName, enabled);
		this.inUse.Add(tileObject);
		tileObject.poolIndex = this.inUse.Count - 1;
		return tileObject;
	}

	// Token: 0x06002108 RID: 8456 RVA: 0x000F1E60 File Offset: 0x000F0260
	public void RecycleTileObject(TileObject tileObject)
	{
		int num;
		if (tileObject.poolIndex >= 0 && tileObject.poolIndex < this.inUse.Count && this.inUse[tileObject.poolIndex] == tileObject)
		{
			num = tileObject.poolIndex;
		}
		else
		{
			BWLog.Warning(string.Concat(new object[]
			{
				"Tile pool index not set correctly: ",
				tileObject.poolIndex,
				" in use: ",
				this.inUse.Count
			}));
			num = this.inUse.IndexOf(tileObject);
		}
		if (num < 0)
		{
			BWLog.Error("Recycling tile object not in pool");
		}
		else
		{
			for (int i = num + 1; i < this.inUse.Count; i++)
			{
				this.inUse[i].poolIndex--;
			}
			this.inUse.RemoveAt(num);
		}
		tileObject.Cleanup();
		tileObject.Hide();
		tileObject.transform.parent = this.poolTransform;
		this.available.Add(tileObject);
	}

	// Token: 0x06002109 RID: 8457 RVA: 0x000F1F88 File Offset: 0x000F0388
	private void GrowPool()
	{
		TileObject tileObject = TileObject.CreateTemplate(this.imageSource);
		tileObject.transform.parent = this.poolTransform;
		tileObject.obtainedFromPool = this;
		this.available.Add(tileObject);
	}

	// Token: 0x04001C05 RID: 7173
	private List<TileObject> available;

	// Token: 0x04001C06 RID: 7174
	private List<TileObject> inUse;

	// Token: 0x04001C07 RID: 7175
	private Transform poolTransform;

	// Token: 0x04001C08 RID: 7176
	internal TilePool.TileImageSource imageSource;

	// Token: 0x020002D3 RID: 723
	public enum TileImageSource
	{
		// Token: 0x04001C0A RID: 7178
		StreamingAssets,
		// Token: 0x04001C0B RID: 7179
		Resources,
		// Token: 0x04001C0C RID: 7180
		StandaloneImageManger
	}
}
