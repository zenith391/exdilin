using UnityEngine;
using UnityEngine.UI;

public class UITile : MonoBehaviour
{
	public RawImage background;

	public RawImage foreground;

	public RawImage rariryBorder;

	public Image quantityBadge;

	public UIEditableText quantityText;

	private TileIconHandle iconLoader;

	private Texture2D iconTexture;

	private GAF _gaf;

	private bool _usingPreloadedIcon;

	private string _iconName;

	public UITile CloneWithGAF(GAF gaf)
	{
		GameObject gameObject = Object.Instantiate(base.gameObject);
		UITile component = gameObject.GetComponent<UITile>();
		component.iconLoader = TileIconManager.CreateTileIconHandle();
		component.SetGAF(gaf);
		return component;
	}

	public UITile CloneWithTexture(Texture2D tex)
	{
		GameObject gameObject = Object.Instantiate(base.gameObject);
		UITile component = gameObject.GetComponent<UITile>();
		component.iconLoader = TileIconManager.CreateTileIconHandle();
		component.foreground.texture = tex;
		component.background.enabled = false;
		return component;
	}

	public void Show()
	{
		if (_gaf != null)
		{
			SetGAF(_gaf);
		}
		base.gameObject.SetActive(value: true);
		quantityBadge.gameObject.SetActive(value: false);
		quantityText.Hide();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		if (_gaf != null)
		{
			foreground.texture = null;
		}
	}

	public void OnDestroy()
	{
		if (iconLoader != null)
		{
			iconLoader.CancelLoad();
		}
		if (iconTexture != null && !_usingPreloadedIcon)
		{
			Object.Destroy(iconTexture);
		}
	}

	public void ShowQuantity(int quantity)
	{
		string text = quantity.ToString();
		int length = text.Length;
		Vector2 sizeDelta = quantityBadge.rectTransform.sizeDelta;
		quantityBadge.rectTransform.sizeDelta = new Vector2(length * 50, sizeDelta.y);
		quantityBadge.gameObject.SetActive(value: true);
		quantityBadge.SetVerticesDirty();
		quantityText.Set(text);
		quantityText.Show();
	}

	public void HideQuantity()
	{
		quantityBadge.gameObject.SetActive(value: false);
		quantityText.Hide();
	}

	public void SetIcon(string iconName)
	{
		if (iconLoader == null)
		{
			iconLoader = TileIconManager.CreateTileIconHandle();
		}
		if (TileIconManager.Instance.preloadedIcons.ContainsKey(iconName))
		{
			iconTexture = TileIconManager.Instance.preloadedIcons[iconName];
			ApplyTexture();
			iconLoader.loadState = TileIconLoadState.Applied;
			background.enabled = false;
			_iconName = iconName;
			_usingPreloadedIcon = true;
			return;
		}
		_usingPreloadedIcon = false;
		TileIconInfo tileInfoForIcon = TileIconManager.Instance.GetTileInfoForIcon(iconName);
		if (tileInfoForIcon == null)
		{
			Debug.Log("No info for icon " + iconName);
		}
		else if (TileIconManager.Instance.RequestIconLoad(tileInfoForIcon.filePath, iconLoader))
		{
			if (iconTexture == null)
			{
				iconTexture = new Texture2D(1, 1, TextureFormat.RGBA32, mipmap: false);
			}
			_iconName = iconName;
			foreground.uvRect = new Rect(0f, 0f, 1f, 1f);
			foreground.enabled = false;
			background.enabled = false;
		}
	}

	public string IconName()
	{
		return _iconName;
	}

	public void SetGAF(GAF gaf)
	{
		if (gaf == null)
		{
			BWLog.Error(" gaf is null");
			return;
		}
		if (iconLoader == null)
		{
			iconLoader = TileIconManager.CreateTileIconHandle();
		}
		if (gaf.Equals(_gaf) && iconLoader.loadState != TileIconLoadState.Unloaded)
		{
			return;
		}
		_gaf = gaf;
		TileIconInfo tileInfo = TileIconManager.Instance.GetTileInfo(gaf);
		if (tileInfo == null)
		{
			Debug.Log("No info for gaf " + gaf);
		}
		else if (TileIconManager.Instance.RequestIconLoad(tileInfo.filePath, iconLoader))
		{
			if (iconTexture == null)
			{
				iconTexture = new Texture2D(1, 1, TextureFormat.RGBA32, mipmap: false);
			}
			foreground.uvRect = new Rect(0f, 0f, 1f, 1f);
			foreground.enabled = false;
			if (tileInfo.clearBackground)
			{
				background.enabled = false;
			}
			else
			{
				background.texture = TileIconManager.Instance.GetBackgroundTexture(gaf);
				Color[] colors = Blocksworld.GetColors(tileInfo.backgroundColorName);
				background.enabled = background.texture != null;
				background.color = colors[0];
			}
			BlockItem blockItem = BlockItem.FindByGafPredicateNameAndArguments(gaf.Predicate.Name, gaf.Args);
			if (blockItem != null && blockItem.IsRare)
			{
				if (Blocksworld.rarityBorderMaterialsEnabled.TryGetValue(blockItem.Rarity, out var value))
				{
					rariryBorder.texture = value.mainTexture;
					rariryBorder.enabled = true;
				}
				else
				{
					rariryBorder.enabled = false;
				}
			}
			else
			{
				rariryBorder.enabled = false;
			}
		}
		else
		{
			BWLog.Info("SetGAF called for '" + gaf?.ToString() + "' but no icon found.");
			iconLoader.CancelLoad();
			foreground.texture = null;
			foreground.enabled = false;
			background.enabled = false;
		}
	}

	public void SetTexture(Texture2D tex)
	{
		foreground.uvRect = new Rect(0f, 0f, 1f, 1f);
		foreground.texture = tex;
		foreground.enabled = true;
		foreground.rectTransform.sizeDelta = background.rectTransform.sizeDelta;
		background.enabled = false;
	}

	public void SetEnabled(bool enabled)
	{
		if (enabled)
		{
			foreground.color = new Color(foreground.color.r, foreground.color.g, foreground.color.b, 1f);
			background.color = new Color(background.color.r, background.color.g, background.color.b, 1f);
		}
		else
		{
			foreground.color = new Color(foreground.color.r, foreground.color.g, foreground.color.b, 0.5f);
			background.color = new Color(background.color.r, background.color.g, background.color.b, 0.5f);
		}
	}

	public void SetImageScale(float scale)
	{
		foreground.rectTransform.localScale = new Vector3(scale, scale, 1f);
	}

	private void Update()
	{
		if (iconLoader != null && iconLoader.loadState == TileIconLoadState.Loaded)
		{
			iconLoader.ApplyTexture(iconTexture);
			ApplyTexture();
		}
	}

	private void ApplyTexture()
	{
		foreground.texture = iconTexture;
		float x = 100f * ((float)iconTexture.width / 67f);
		float y = 100f * ((float)iconTexture.height / 67f);
		Vector2 vector = new Vector2(x, y);
		foreground.rectTransform.sizeDelta = vector / TileIconManager.iconScaleFactor;
		rariryBorder.rectTransform.sizeDelta = background.rectTransform.sizeDelta;
		foreground.enabled = true;
	}
}
