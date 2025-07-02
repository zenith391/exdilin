using System.Collections.Generic;
using UnityEngine;

public class TilePool
{
	public enum TileImageSource
	{
		StreamingAssets,
		Resources,
		StandaloneImageManger
	}

	private List<TileObject> available;

	private List<TileObject> inUse;

	private Transform poolTransform;

	internal TileImageSource imageSource;

	public TilePool(string name, int capacity, TileImageSource imageSource)
	{
		poolTransform = new GameObject(name + " TilePool").transform;
		available = new List<TileObject>(capacity);
		inUse = new List<TileObject>();
		this.imageSource = imageSource;
		for (int i = 0; i < capacity; i++)
		{
			GrowPool();
		}
	}

	public TileObject GetTileObject(GAF gaf, bool enabled, bool asOverlay = false)
	{
		if (available.Count == 0)
		{
			BWLog.Warning("Tile pool is empty, growing");
			GrowPool();
		}
		TileObject tileObject = available[0];
		available.RemoveAt(0);
		tileObject.isOverlay = asOverlay;
		tileObject.Setup(gaf, enabled);
		inUse.Add(tileObject);
		tileObject.poolIndex = inUse.Count - 1;
		return tileObject;
	}

	public TileObject GetTileObjectForIcon(string iconName, bool enabled)
	{
		if (available.Count == 0)
		{
			BWLog.Warning("Tile pool is empty, growing");
			GrowPool();
		}
		TileObject tileObject = available[0];
		available.RemoveAt(0);
		tileObject.SetupForIcon(iconName, enabled);
		inUse.Add(tileObject);
		tileObject.poolIndex = inUse.Count - 1;
		return tileObject;
	}

	public void RecycleTileObject(TileObject tileObject)
	{
		int num;
		if (tileObject.poolIndex >= 0 && tileObject.poolIndex < inUse.Count && inUse[tileObject.poolIndex] == tileObject)
		{
			num = tileObject.poolIndex;
		}
		else
		{
			BWLog.Warning("Tile pool index not set correctly: " + tileObject.poolIndex + " in use: " + inUse.Count);
			num = inUse.IndexOf(tileObject);
		}
		if (num < 0)
		{
			BWLog.Error("Recycling tile object not in pool");
		}
		else
		{
			for (int i = num + 1; i < inUse.Count; i++)
			{
				inUse[i].poolIndex--;
			}
			inUse.RemoveAt(num);
		}
		tileObject.Cleanup();
		tileObject.Hide();
		tileObject.transform.parent = poolTransform;
		available.Add(tileObject);
	}

	private void GrowPool()
	{
		TileObject tileObject = TileObject.CreateTemplate(imageSource);
		tileObject.transform.parent = poolTransform;
		tileObject.obtainedFromPool = this;
		available.Add(tileObject);
	}
}
