using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000321 RID: 801
public class UITile : MonoBehaviour
{
	// Token: 0x0600244E RID: 9294 RVA: 0x00109F2C File Offset: 0x0010832C
	public UITile CloneWithGAF(GAF gaf)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(base.gameObject);
		UITile component = gameObject.GetComponent<UITile>();
		component.iconLoader = TileIconManager.CreateTileIconHandle();
		component.SetGAF(gaf);
		return component;
	}

	// Token: 0x0600244F RID: 9295 RVA: 0x00109F60 File Offset: 0x00108360
	public UITile CloneWithTexture(Texture2D tex)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(base.gameObject);
		UITile component = gameObject.GetComponent<UITile>();
		component.iconLoader = TileIconManager.CreateTileIconHandle();
		component.foreground.texture = tex;
		component.background.enabled = false;
		return component;
	}

	// Token: 0x06002450 RID: 9296 RVA: 0x00109FA4 File Offset: 0x001083A4
	public void Show()
	{
		if (this._gaf != null)
		{
			this.SetGAF(this._gaf);
		}
		base.gameObject.SetActive(true);
		this.quantityBadge.gameObject.SetActive(false);
		this.quantityText.Hide();
	}

	// Token: 0x06002451 RID: 9297 RVA: 0x00109FF0 File Offset: 0x001083F0
	public void Hide()
	{
		base.gameObject.SetActive(false);
		if (this._gaf != null)
		{
			this.foreground.texture = null;
		}
	}

	// Token: 0x06002452 RID: 9298 RVA: 0x0010A015 File Offset: 0x00108415
	public void OnDestroy()
	{
		if (this.iconLoader != null)
		{
			this.iconLoader.CancelLoad();
		}
		if (this.iconTexture != null && !this._usingPreloadedIcon)
		{
			UnityEngine.Object.Destroy(this.iconTexture);
		}
	}

	// Token: 0x06002453 RID: 9299 RVA: 0x0010A054 File Offset: 0x00108454
	public void ShowQuantity(int quantity)
	{
		string text = quantity.ToString();
		int length = text.Length;
		Vector2 sizeDelta = this.quantityBadge.rectTransform.sizeDelta;
		this.quantityBadge.rectTransform.sizeDelta = new Vector2((float)(length * 50), sizeDelta.y);
		this.quantityBadge.gameObject.SetActive(true);
		this.quantityBadge.SetVerticesDirty();
		this.quantityText.Set(text);
		this.quantityText.Show();
	}

	// Token: 0x06002454 RID: 9300 RVA: 0x0010A0DB File Offset: 0x001084DB
	public void HideQuantity()
	{
		this.quantityBadge.gameObject.SetActive(false);
		this.quantityText.Hide();
	}

	// Token: 0x06002455 RID: 9301 RVA: 0x0010A0FC File Offset: 0x001084FC
	public void SetIcon(string iconName)
	{
		if (this.iconLoader == null)
		{
			this.iconLoader = TileIconManager.CreateTileIconHandle();
		}
		if (TileIconManager.Instance.preloadedIcons.ContainsKey(iconName))
		{
			this.iconTexture = TileIconManager.Instance.preloadedIcons[iconName];
			this.ApplyTexture();
			this.iconLoader.loadState = TileIconLoadState.Applied;
			this.background.enabled = false;
			this._iconName = iconName;
			this._usingPreloadedIcon = true;
			return;
		}
		this._usingPreloadedIcon = false;
		TileIconInfo tileInfoForIcon = TileIconManager.Instance.GetTileInfoForIcon(iconName);
		if (tileInfoForIcon == null)
		{
			Debug.Log("No info for icon " + iconName);
			return;
		}
		bool flag = TileIconManager.Instance.RequestIconLoad(tileInfoForIcon.filePath, this.iconLoader);
		if (flag)
		{
			if (this.iconTexture == null)
			{
				this.iconTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			}
			this._iconName = iconName;
			this.foreground.uvRect = new Rect(0f, 0f, 1f, 1f);
			this.foreground.enabled = false;
			this.background.enabled = false;
		}
	}

	// Token: 0x06002456 RID: 9302 RVA: 0x0010A221 File Offset: 0x00108621
	public string IconName()
	{
		return this._iconName;
	}

	// Token: 0x06002457 RID: 9303 RVA: 0x0010A22C File Offset: 0x0010862C
	public void SetGAF(GAF gaf)
	{
		if (gaf == null)
		{
			BWLog.Error(" gaf is null");
			return;
		}
		if (this.iconLoader == null)
		{
			this.iconLoader = TileIconManager.CreateTileIconHandle();
		}
		if (gaf.Equals(this._gaf) && this.iconLoader.loadState != TileIconLoadState.Unloaded)
		{
			return;
		}
		this._gaf = gaf;
		TileIconInfo tileInfo = TileIconManager.Instance.GetTileInfo(gaf);
		if (tileInfo == null)
		{
			Debug.Log("No info for gaf " + gaf);
			return;
		}
		bool flag = TileIconManager.Instance.RequestIconLoad(tileInfo.filePath, this.iconLoader);
		if (flag)
		{
			if (this.iconTexture == null)
			{
				this.iconTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			}
			this.foreground.uvRect = new Rect(0f, 0f, 1f, 1f);
			this.foreground.enabled = false;
			if (tileInfo.clearBackground)
			{
				this.background.enabled = false;
			}
			else
			{
				this.background.texture = TileIconManager.Instance.GetBackgroundTexture(gaf);
				Color[] colors = Blocksworld.GetColors(tileInfo.backgroundColorName);
				this.background.enabled = (this.background.texture != null);
				this.background.color = colors[0];
			}
			BlockItem blockItem = BlockItem.FindByGafPredicateNameAndArguments(gaf.Predicate.Name, gaf.Args);
			if (blockItem != null && blockItem.IsRare)
			{
				Material material;
				if (Blocksworld.rarityBorderMaterialsEnabled.TryGetValue(blockItem.Rarity, out material))
				{
					this.rariryBorder.texture = material.mainTexture;
					this.rariryBorder.enabled = true;
				}
				else
				{
					this.rariryBorder.enabled = false;
				}
			}
			else
			{
				this.rariryBorder.enabled = false;
			}
		}
		else
		{
			BWLog.Info("SetGAF called for '" + gaf + "' but no icon found.");
			this.iconLoader.CancelLoad();
			this.foreground.texture = null;
			this.foreground.enabled = false;
			this.background.enabled = false;
		}
	}

	// Token: 0x06002458 RID: 9304 RVA: 0x0010A454 File Offset: 0x00108854
	public void SetTexture(Texture2D tex)
	{
		this.foreground.uvRect = new Rect(0f, 0f, 1f, 1f);
		this.foreground.texture = tex;
		this.foreground.enabled = true;
		this.foreground.rectTransform.sizeDelta = this.background.rectTransform.sizeDelta;
		this.background.enabled = false;
	}

	// Token: 0x06002459 RID: 9305 RVA: 0x0010A4CC File Offset: 0x001088CC
	public void SetEnabled(bool enabled)
	{
		if (enabled)
		{
			this.foreground.color = new Color(this.foreground.color.r, this.foreground.color.g, this.foreground.color.b, 1f);
			this.background.color = new Color(this.background.color.r, this.background.color.g, this.background.color.b, 1f);
		}
		else
		{
			this.foreground.color = new Color(this.foreground.color.r, this.foreground.color.g, this.foreground.color.b, 0.5f);
			this.background.color = new Color(this.background.color.r, this.background.color.g, this.background.color.b, 0.5f);
		}
	}

	// Token: 0x0600245A RID: 9306 RVA: 0x0010A624 File Offset: 0x00108A24
	public void SetImageScale(float scale)
	{
		this.foreground.rectTransform.localScale = new Vector3(scale, scale, 1f);
	}

	// Token: 0x0600245B RID: 9307 RVA: 0x0010A642 File Offset: 0x00108A42
	private void Update()
	{
		if (this.iconLoader != null && this.iconLoader.loadState == TileIconLoadState.Loaded)
		{
			this.iconLoader.ApplyTexture(this.iconTexture);
			this.ApplyTexture();
		}
	}

	// Token: 0x0600245C RID: 9308 RVA: 0x0010A678 File Offset: 0x00108A78
	private void ApplyTexture()
	{
		this.foreground.texture = this.iconTexture;
		float x = 100f * ((float)this.iconTexture.width / 67f);
		float y = 100f * ((float)this.iconTexture.height / 67f);
		Vector2 a = new Vector2(x, y);
		this.foreground.rectTransform.sizeDelta = a / TileIconManager.iconScaleFactor;
		this.rariryBorder.rectTransform.sizeDelta = this.background.rectTransform.sizeDelta;
		this.foreground.enabled = true;
	}

	// Token: 0x04001F47 RID: 8007
	public RawImage background;

	// Token: 0x04001F48 RID: 8008
	public RawImage foreground;

	// Token: 0x04001F49 RID: 8009
	public RawImage rariryBorder;

	// Token: 0x04001F4A RID: 8010
	public Image quantityBadge;

	// Token: 0x04001F4B RID: 8011
	public UIEditableText quantityText;

	// Token: 0x04001F4C RID: 8012
	private TileIconHandle iconLoader;

	// Token: 0x04001F4D RID: 8013
	private Texture2D iconTexture;

	// Token: 0x04001F4E RID: 8014
	private GAF _gaf;

	// Token: 0x04001F4F RID: 8015
	private bool _usingPreloadedIcon;

	// Token: 0x04001F50 RID: 8016
	private string _iconName;
}
