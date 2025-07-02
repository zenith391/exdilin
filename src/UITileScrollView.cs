using System.Collections.Generic;
using UnityEngine;

public class UITileScrollView : MonoBehaviour
{
	public UITile tileTemplate;

	public RectTransform tileParentTransform;

	private List<UITile> tiles;

	public void Init()
	{
		tileTemplate.Hide();
		tiles = new List<UITile>();
	}

	public void AddTileFromGAF(GAF gaf)
	{
		UITile uITile = tileTemplate.CloneWithGAF(gaf);
		float imageScale = 1f / NormalizedScreen.scale;
		uITile.SetImageScale(imageScale);
		AddTile(uITile);
	}

	public void AddTilesFromGAF(GAF gaf, int quantity)
	{
		UITile uITile = tileTemplate.CloneWithGAF(gaf);
		float imageScale = 1f / NormalizedScreen.scale;
		uITile.SetImageScale(imageScale);
		uITile.ShowQuantity(quantity);
		AddTile(uITile);
	}

	public void AddTileFromTexture(Texture2D texture)
	{
		UITile tile = tileTemplate.CloneWithTexture(texture);
		AddTile(tile);
	}

	public void AddTile(UITile tile)
	{
		RectTransform component = tile.gameObject.GetComponent<RectTransform>();
		component.SetParent(tileParentTransform, worldPositionStays: false);
		float imageScale = 1f / NormalizedScreen.scale;
		tile.SetImageScale(imageScale);
		tile.Show();
		tiles.Add(tile);
	}

	public void SetTiles(ICollection<GAF> gafs)
	{
		foreach (GAF gaf in gafs)
		{
			AddTileFromGAF(gaf);
		}
	}

	public void SetTilesAndQuantities(Dictionary<GAF, int> gafCountDict)
	{
		foreach (KeyValuePair<GAF, int> item in gafCountDict)
		{
			GAF key = item.Key;
			int value = item.Value;
			UITile uITile = tileTemplate.CloneWithGAF(key);
			RectTransform component = uITile.gameObject.GetComponent<RectTransform>();
			component.SetParent(tileParentTransform, worldPositionStays: false);
			float imageScale = 1f / NormalizedScreen.scale;
			uITile.SetImageScale(imageScale);
			uITile.Show();
			uITile.ShowQuantity(value);
			tiles.Add(uITile);
		}
	}

	public void ClearTiles()
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			tiles[i].Hide();
			Object.Destroy(tiles[i].gameObject);
		}
		tiles.Clear();
	}
}
