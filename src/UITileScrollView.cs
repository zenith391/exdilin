using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000322 RID: 802
public class UITileScrollView : MonoBehaviour
{
	// Token: 0x0600245E RID: 9310 RVA: 0x0010A720 File Offset: 0x00108B20
	public void Init()
	{
		this.tileTemplate.Hide();
		this.tiles = new List<UITile>();
	}

	// Token: 0x0600245F RID: 9311 RVA: 0x0010A738 File Offset: 0x00108B38
	public void AddTileFromGAF(GAF gaf)
	{
		UITile uitile = this.tileTemplate.CloneWithGAF(gaf);
		float imageScale = 1f / NormalizedScreen.scale;
		uitile.SetImageScale(imageScale);
		this.AddTile(uitile);
	}

	// Token: 0x06002460 RID: 9312 RVA: 0x0010A76C File Offset: 0x00108B6C
	public void AddTilesFromGAF(GAF gaf, int quantity)
	{
		UITile uitile = this.tileTemplate.CloneWithGAF(gaf);
		float imageScale = 1f / NormalizedScreen.scale;
		uitile.SetImageScale(imageScale);
		uitile.ShowQuantity(quantity);
		this.AddTile(uitile);
	}

	// Token: 0x06002461 RID: 9313 RVA: 0x0010A7A8 File Offset: 0x00108BA8
	public void AddTileFromTexture(Texture2D texture)
	{
		UITile tile = this.tileTemplate.CloneWithTexture(texture);
		this.AddTile(tile);
	}

	// Token: 0x06002462 RID: 9314 RVA: 0x0010A7CC File Offset: 0x00108BCC
	public void AddTile(UITile tile)
	{
		RectTransform component = tile.gameObject.GetComponent<RectTransform>();
		component.SetParent(this.tileParentTransform, false);
		float imageScale = 1f / NormalizedScreen.scale;
		tile.SetImageScale(imageScale);
		tile.Show();
		this.tiles.Add(tile);
	}

	// Token: 0x06002463 RID: 9315 RVA: 0x0010A818 File Offset: 0x00108C18
	public void SetTiles(ICollection<GAF> gafs)
	{
		foreach (GAF gaf in gafs)
		{
			this.AddTileFromGAF(gaf);
		}
	}

	// Token: 0x06002464 RID: 9316 RVA: 0x0010A86C File Offset: 0x00108C6C
	public void SetTilesAndQuantities(Dictionary<GAF, int> gafCountDict)
	{
		foreach (KeyValuePair<GAF, int> keyValuePair in gafCountDict)
		{
			GAF key = keyValuePair.Key;
			int value = keyValuePair.Value;
			UITile uitile = this.tileTemplate.CloneWithGAF(key);
			RectTransform component = uitile.gameObject.GetComponent<RectTransform>();
			component.SetParent(this.tileParentTransform, false);
			float imageScale = 1f / NormalizedScreen.scale;
			uitile.SetImageScale(imageScale);
			uitile.Show();
			uitile.ShowQuantity(value);
			this.tiles.Add(uitile);
		}
	}

	// Token: 0x06002465 RID: 9317 RVA: 0x0010A928 File Offset: 0x00108D28
	public void ClearTiles()
	{
		for (int i = 0; i < this.tiles.Count; i++)
		{
			this.tiles[i].Hide();
			UnityEngine.Object.Destroy(this.tiles[i].gameObject);
		}
		this.tiles.Clear();
	}

	// Token: 0x04001F51 RID: 8017
	public UITile tileTemplate;

	// Token: 0x04001F52 RID: 8018
	public RectTransform tileParentTransform;

	// Token: 0x04001F53 RID: 8019
	private List<UITile> tiles;
}
