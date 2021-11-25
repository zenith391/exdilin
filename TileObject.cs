using System;
using UnityEngine;

// Token: 0x02000230 RID: 560
public class TileObject : MonoBehaviour
{
	// Token: 0x06001AA3 RID: 6819 RVA: 0x000C3036 File Offset: 0x000C1436
	public bool IsIconLoaded()
	{
		return this.iconLoaded;
	}

	// Token: 0x06001AA4 RID: 6820 RVA: 0x000C3040 File Offset: 0x000C1440
	public void MoveTo(float x, float y)
	{
		float z = this.mainGameObject.transform.position.z;
		this.MoveTo(new Vector3(x, y, z));
	}

	// Token: 0x06001AA5 RID: 6821 RVA: 0x000C3074 File Offset: 0x000C1474
	public void MoveTo(Vector3 pos)
	{
		this.mainGameObject.transform.position = pos;
	}

	// Token: 0x06001AA6 RID: 6822 RVA: 0x000C3087 File Offset: 0x000C1487
	public void LocalMoveTo(Vector3 pos)
	{
		this.mainGameObject.transform.localPosition = pos;
	}

	// Token: 0x06001AA7 RID: 6823 RVA: 0x000C309C File Offset: 0x000C149C
	public void SetupForIcon(string iconName, bool enabled)
	{
		this._iconName = iconName;
		if (TileIconManager.Instance.preloadedIcons.ContainsKey(iconName))
		{
			this.mainTileTexture = TileIconManager.Instance.preloadedIcons[iconName];
			this.mainTileRenderer.enabled = true;
			this.mainTileMaterial.mainTexture = this.mainTileTexture;
			this.RefreshMainTileAlpha();
			this.RefreshMainTileScale();
			this.hasIcon = true;
			this.iconLoaded = true;
			this.iconLoader.loadState = TileIconLoadState.Applied;
		}
		else
		{
			TileIconInfo tileInfoForIcon = TileIconManager.Instance.GetTileInfoForIcon(iconName);
			this.hasIcon = TileIconManager.Instance.RequestIconLoad(tileInfoForIcon.filePath, this.iconLoader);
			if (this.hasIcon)
			{
				this.iconLoaded = false;
				this.mainTileMaterial.mainTexture = null;
			}
			this.mainTileRenderer.enabled = false;
		}
		this.labelRenderer.enabled = false;
		this.backgroundRenderer.enabled = false;
		this.rarityBorderRenderer.enabled = false;
		this.SetScale(Vector3.one * NormalizedScreen.pixelScale);
		this.isEnabled = enabled;
		this.RefreshMainTileAlpha();
	}

	// Token: 0x06001AA8 RID: 6824 RVA: 0x000C31BC File Offset: 0x000C15BC
	public void Setup(GAF gaf, bool enabled)
	{
		this._gaf = gaf;
		this._blockItem = BlockItem.FindByGafPredicateNameAndArguments(gaf.Predicate.Name, gaf.Args);
		bool flag = gaf.IsCreateModel();
		bool flag2 = gaf.IsButtonInput();
		TileIconInfo tileIconInfo;
		if (flag)
		{
			tileIconInfo = new TileIconInfo();
			tileIconInfo.filePath = Blocksworld.modelCollection.GetPathToIcon(gaf);
			tileIconInfo.clearBackground = true;
            tileIconInfo.label = Blocksworld.modelCollection.GetModelLabel(gaf);
		}
		else
		{
			tileIconInfo = TileIconManager.Instance.GetTileInfo(gaf);
            if (tileIconInfo != null && (tileIconInfo.label == null || tileIconInfo.label.Length == 0))
            {
				// ADDED BY EXDILIN: EMPTY LABEL TILE RENAME
                string str = gaf.Predicate.Name;
				if (str != "Sky.SkyBoxTo" && str != "AnimCharacter.ReplaceBodyPart") {
					tileIconInfo.label = str.Substring(str.IndexOf('.') + 1);
				}
            }
        }
		if (tileIconInfo == null)
		{
			BWLog.Info(string.Concat(new object[]
			{
				"NULL tile info, disabling renderer for tile: ",
				gaf,
				"\nBlock item: ",
				this._blockItem,
				"\nMost likely it is unable to find the corresponding block item for this GAF because the declaration of argument types in the Register function does not match the types in use in the code!"
			}));
			this.mainTileRenderer.enabled = false;
			this.labelRenderer.enabled = false;
			this.backgroundRenderer.enabled = false;
			return;
		}
		if (flag2)
		{
			if (this.isOverlay)
			{
				string buttonInputVariantKeyImagePath = TileIconManager.Instance.GetButtonInputVariantKeyImagePath(gaf);
				if (!string.IsNullOrEmpty(buttonInputVariantKeyImagePath))
				{
					tileIconInfo.filePath = buttonInputVariantKeyImagePath;
				}
			}
			else if (this.childTileObject != null)
			{
				this.childTileObject.Setup(gaf, enabled);
			}
			else
			{
				this.childTileObject = this.obtainedFromPool.GetTileObject(gaf, this.isEnabled, true);
				this.childTileObject.transform.SetParent(this.mainGameObject.transform, false);
				this.childTileObject.transform.localScale = Vector3.one;
			}
		}
		if (flag && Blocksworld.modelCollection.IsSourceLocked(gaf))
		{
			if (this.isOverlay)
			{
				tileIconInfo.filePath = TileIconManager.GetPathToIcon("Misc/Locked_Model_Icon_Overlay");
			}
			else if (this.childTileObject != null)
			{
				this.childTileObject.Setup(gaf, enabled);
			}
			else
			{
				this.childTileObject = Blocksworld.tilePool.GetTileObject(gaf, this.isEnabled, true);
				this.childTileObject.transform.SetParent(this.mainGameObject.transform, false);
				this.childTileObject.transform.localScale = Vector3.one;
			}
		}
		bool flag3 = false;
		if (this._gaf.Predicate.Name == "BlockWorldJumper.Jump")
		{
			string text = (string)gaf.Args[0];
			if (!string.IsNullOrEmpty(text))
			{
				WorldInfo worldWithId = WorldSession.current.availableTeleportWorlds.GetWorldWithId(text);
				if (worldWithId != null)
				{
					flag3 = true;
					this.LinkToWorldThumbnail(worldWithId);
				}
			}
		}
		if (flag)
		{
			string modelID = Blocksworld.modelCollection.GetModelID(gaf);
			string modelType = Blocksworld.modelCollection.GetModelType(gaf);
			this.hasIcon = TileIconManager.Instance.RequestModelIconLoad(tileIconInfo.filePath, modelID, modelType, this.iconLoader);
		}
		else if (!flag3)
		{
			this.hasIcon = TileIconManager.Instance.RequestIconLoad(tileIconInfo.filePath, this.iconLoader);
		}
		if (!this.hasIcon)
		{
			this.mainTileRenderer.enabled = false;
		}
		else if (!flag3)
		{
			this.iconLoaded = false;
			this.mainTileMaterial.mainTexture = null;
			this.mainTileRenderer.enabled = false;
		}
		this.hasLabel = ((this._gaf.HasBuildPanelLabel || this._gaf.HasDynamicLabel()) && !string.IsNullOrEmpty(tileIconInfo.label) && !this.isOverlay);
		if (this.hasLabel)
		{
			TileLabelAtlas labelAtlas = TileIconManager.Instance.labelAtlas;
			string text2 = tileIconInfo.label;
			if (this._gaf.HasDynamicLabel() && !flag)
			{
				text2 = this._gaf.GetDynamicLabel();
			}
			if (!labelAtlas.Contains(text2))
			{
				labelAtlas.AddNewLabel(text2);
			}
			Vector2[] labelUVs = labelAtlas.GetLabelUVs(text2);
			if (labelUVs != null && labelUVs.Length >= 4)
			{
				string str = "UVs ";
				for (int i = 0; i < 4; i++)
				{
					str = str + labelUVs[i].ToString() + ((i >= 3) ? string.Empty : ", ");
				}
				this.enabledLabelMaterial = labelAtlas.GetMaterial(text2);
				this.disabledLabelMaterial = labelAtlas.GetMaterialDisabled(text2);
				if (this.enabledLabelMaterial != null && this.disabledLabelMaterial != null)
				{
					this.labelMeshFilter.sharedMesh.uv = labelUVs;
					this.labelRenderer.sharedMaterial = ((!enabled) ? this.disabledLabelMaterial : this.enabledLabelMaterial);
					this.labelRenderer.enabled = true;
				}
				else
				{
					BWLog.Info("No label materials for label " + text2);
					this.labelRenderer.enabled = false;
				}
			}
			else
			{
				BWLog.Info("No label uvs for : " + this._gaf);
				this.labelRenderer.enabled = false;
			}
		}
		else
		{
			this.labelRenderer.enabled = false;
		}
		if (tileIconInfo.clearBackground || this.isOverlay)
		{
			this.backgroundRenderer.enabled = false;
		}
		else
		{
			this.enabledBackgroundMaterial = TileIconManager.Instance.GetBackgroundMaterial(this._gaf);
			this.disabledBackgroundMaterial = TileIconManager.Instance.GetBackgroundMaterialDisabled(this._gaf);
			this.backgroundRenderer.sharedMaterial = ((!enabled) ? this.disabledBackgroundMaterial : this.enabledBackgroundMaterial);
			Color[] colors = Blocksworld.GetColors(tileIconInfo.backgroundColorName);
			this.backgroundMeshFilter.sharedMesh.colors = new Color[]
			{
				colors[0],
				colors[0],
				colors[1],
				colors[1]
			};
			this.backgroundRenderer.enabled = true;
		}
		if (this._blockItem != null && this._blockItem.HasRarityBorder)
		{
			this.rarityBorderRenderer.sharedMaterial = ((!enabled) ? Blocksworld.rarityBorderMaterialsDisabled[this._blockItem.Rarity] : Blocksworld.rarityBorderMaterialsEnabled[this._blockItem.Rarity]);
			this.rarityBorderRenderer.enabled = true;
		}
		else
		{
			this.rarityBorderRenderer.enabled = false;
		}
		this.SetScale(Vector3.one * NormalizedScreen.pixelScale);
		this.isEnabled = enabled;
		this.RefreshMainTileAlpha();
	}

	// Token: 0x06001AA9 RID: 6825 RVA: 0x000C3874 File Offset: 0x000C1C74
	private void LinkToWorldThumbnail(WorldInfo worldInfo)
	{
		this._thumbnailWorldInfo = worldInfo;
		worldInfo.ThumbnailLoaded += this.WorldInfoLoadedScreenshot;
		Texture2D worldThumbnailForTiles = worldInfo.GetWorldThumbnailForTiles();
		this.backgroundRenderer.enabled = false;
		this.labelRenderer.enabled = false;
		this.rarityBorderRenderer.enabled = false;
		this.hasIcon = true;
		if (worldThumbnailForTiles == null)
		{
			worldInfo.LoadThumbnail();
			this.iconLoaded = false;
			this.mainTileRenderer.enabled = false;
		}
		else
		{
			this.iconLoaded = true;
			this.mainTileTexture = worldThumbnailForTiles;
			this.mainTileRenderer.enabled = true;
			this.mainTileMaterial.mainTexture = this.mainTileTexture;
			this.RefreshMainTileAlpha();
			this.RefreshMainTileScale();
			this.iconLoader.loadState = TileIconLoadState.Applied;
		}
	}

	// Token: 0x06001AAA RID: 6826 RVA: 0x000C393C File Offset: 0x000C1D3C
	private void WorldInfoLoadedScreenshot(object sender, EventArgs e)
	{
		if (sender != this._thumbnailWorldInfo)
		{
			return;
		}
		Texture2D worldThumbnailForTiles = this._thumbnailWorldInfo.GetWorldThumbnailForTiles();
		if (worldThumbnailForTiles == null)
		{
			BWLog.Error("Failed to load thumbnail");
			return;
		}
		this.hasIcon = true;
		this.iconLoaded = true;
		this.mainTileTexture = worldThumbnailForTiles;
		this.mainTileRenderer.enabled = true;
		this.mainTileMaterial.mainTexture = this.mainTileTexture;
		this.RefreshMainTileAlpha();
		this.RefreshMainTileScale();
		this.iconLoader.loadState = TileIconLoadState.Applied;
	}

	// Token: 0x06001AAB RID: 6827 RVA: 0x000C39C3 File Offset: 0x000C1DC3
	public bool IsSetupForGAF(GAF gaf)
	{
		return gaf.Equals(this._gaf) && this.hasIcon;
	}

	// Token: 0x06001AAC RID: 6828 RVA: 0x000C39E6 File Offset: 0x000C1DE6
	public bool IsActive()
	{
		return this.mainGameObject.activeSelf;
	}

	// Token: 0x06001AAD RID: 6829 RVA: 0x000C39F3 File Offset: 0x000C1DF3
	public string IconName()
	{
		return this._iconName;
	}

	// Token: 0x06001AAE RID: 6830 RVA: 0x000C39FB File Offset: 0x000C1DFB
	public void Show(bool show)
	{
		if (show)
		{
			this.Show();
		}
		else
		{
			this.Hide();
		}
	}

	// Token: 0x06001AAF RID: 6831 RVA: 0x000C3A14 File Offset: 0x000C1E14
	public void Show()
	{
		this.mainGameObject.SetActive(true);
		this.mainTileRenderer.enabled = (this.hasIcon && this.iconLoaded);
		this.RefreshMainTileAlpha();
		if (this.childTileObject != null)
		{
			this.childTileObject.Show();
		}
	}

	// Token: 0x06001AB0 RID: 6832 RVA: 0x000C3A6E File Offset: 0x000C1E6E
	public void Hide()
	{
		this.mainGameObject.SetActive(false);
		if (this.childTileObject != null)
		{
			this.childTileObject.Hide();
		}
	}

	// Token: 0x06001AB1 RID: 6833 RVA: 0x000C3A98 File Offset: 0x000C1E98
	public bool IsShowing()
	{
		return this.IsActive() && this.mainTileRenderer.enabled;
	}

	// Token: 0x06001AB2 RID: 6834 RVA: 0x000C3AB3 File Offset: 0x000C1EB3
	public bool IsEnabled()
	{
		return this.isEnabled;
	}

	// Token: 0x06001AB3 RID: 6835 RVA: 0x000C3ABB File Offset: 0x000C1EBB
	public void Enable(bool enable)
	{
		if (enable)
		{
			this.Enable();
		}
		else
		{
			this.Disable();
		}
	}

	// Token: 0x06001AB4 RID: 6836 RVA: 0x000C3AD4 File Offset: 0x000C1ED4
	public void Enable()
	{
		if (this.childTileObject != null)
		{
			this.childTileObject.Enable();
		}
		this.isEnabled = true;
		if (this.hasIcon && this.iconLoaded)
		{
			this.mainTileRenderer.enabled = true;
		}
		this.RefreshMainTileAlpha();
		this.RefreshMainTileScale();
		if (this.backgroundRenderer.enabled)
		{
			this.backgroundRenderer.sharedMaterial = this.enabledBackgroundMaterial;
		}
		if (this.labelRenderer.enabled)
		{
			this.labelRenderer.sharedMaterial = this.enabledLabelMaterial;
		}
		if (this._blockItem != null && this._blockItem.HasRarityBorder)
		{
			this.rarityBorderRenderer.sharedMaterial = Blocksworld.rarityBorderMaterialsEnabled[this._blockItem.Rarity];
			this.rarityBorderRenderer.enabled = true;
		}
		else
		{
			this.rarityBorderRenderer.enabled = false;
		}
	}

	// Token: 0x06001AB5 RID: 6837 RVA: 0x000C3BCC File Offset: 0x000C1FCC
	public void Disable()
	{
		if (this.childTileObject != null)
		{
			this.childTileObject.Disable();
		}
		this.isEnabled = false;
		if (this.hasIcon && this.iconLoaded)
		{
			this.mainTileRenderer.enabled = true;
		}
		this.RefreshMainTileAlpha();
		this.RefreshMainTileScale();
		this.backgroundRenderer.sharedMaterial = this.disabledBackgroundMaterial;
		if (this.labelRenderer.enabled)
		{
			this.labelRenderer.sharedMaterial = this.disabledLabelMaterial;
		}
		if (this._blockItem != null && this._blockItem.HasRarityBorder)
		{
			this.rarityBorderRenderer.sharedMaterial = Blocksworld.rarityBorderMaterialsDisabled[this._blockItem.Rarity];
			this.rarityBorderRenderer.enabled = true;
		}
		else
		{
			this.rarityBorderRenderer.enabled = false;
		}
	}

	// Token: 0x06001AB6 RID: 6838 RVA: 0x000C3CB4 File Offset: 0x000C20B4
	public void SetGhosted(bool ghosted)
	{
		if (ghosted)
		{
			bool flag = this.isEnabled;
			this.Disable();
			this.isEnabled = flag;
		}
		else if (this.isEnabled)
		{
			this.Enable();
		}
		else
		{
			this.Disable();
		}
	}

	// Token: 0x06001AB7 RID: 6839 RVA: 0x000C3CFC File Offset: 0x000C20FC
	private void RefreshMainTileAlpha()
	{
		float value = (!this.isEnabled) ? 0.3f : 1f;
		this.mainTileMaterial.SetFloat("_Alpha", value);
	}

	// Token: 0x06001AB8 RID: 6840 RVA: 0x000C3D38 File Offset: 0x000C2138
	private void RefreshMainTileScale()
	{
		if (this.isOverlay)
		{
			return;
		}
		if (this.hasIcon && this.iconLoaded)
		{
			float num = (float)this.mainTileTexture.width;
			float num2 = (float)this.mainTileTexture.height;
			float num3 = NormalizedScreen.scale * TileIconManager.iconScaleFactor;
			float x = 2f + Mathf.Ceil(31.5f - num / (2f * num3));
			float y = 2f + Mathf.Ceil(31.5f - num2 / (2f * num3));
			float num4 = num / num3;
			float num5 = num2 / num3;
			if (this.hasLabel && this._thumbnailWorldInfo == null)
			{
				y = Mathf.Ceil((TileObject.sizeTile.x + 14f) / 2f - num2 / (2f * num3));
			}
			this.iconTransform.localPosition = new Vector3(x, y, this.iconTransform.localPosition.z);
			this.iconTransform.localScale = new Vector3(num4 / TileObject.sizeTile.x, num5 / TileObject.sizeTile.y, 1f);
		}
		else
		{
			this.iconTransform.localPosition = new Vector3(0f, 0f, -0.1f);
			this.iconTransform.localScale = Vector3.one;
		}
	}

	// Token: 0x06001AB9 RID: 6841 RVA: 0x000C3E95 File Offset: 0x000C2295
	public void SetPosition(Vector3 position)
	{
		this.mainGameObject.transform.position = position;
	}

	// Token: 0x06001ABA RID: 6842 RVA: 0x000C3EA8 File Offset: 0x000C22A8
	public Vector3 GetPosition()
	{
		return this.mainGameObject.transform.position;
	}

	// Token: 0x06001ABB RID: 6843 RVA: 0x000C3EBC File Offset: 0x000C22BC
	public Vector3 GetCenterPosition()
	{
		Vector3 position = this.mainGameObject.transform.position;
		Vector3 vector = this.GetScale();
		Vector3 b = new Vector3(vector.x / 2f, vector.y / 2f, 0f);
		return position + b;
	}

	// Token: 0x06001ABC RID: 6844 RVA: 0x000C3F13 File Offset: 0x000C2313
	public void SetLocalPosition(Vector3 position)
	{
		this.mainGameObject.transform.localPosition = position;
	}

	// Token: 0x06001ABD RID: 6845 RVA: 0x000C3F26 File Offset: 0x000C2326
	public Vector3 GetLocalPosition()
	{
		return this.mainGameObject.transform.localPosition;
	}

	// Token: 0x06001ABE RID: 6846 RVA: 0x000C3F38 File Offset: 0x000C2338
	public void SetScale(float scale)
	{
		this.SetScale(scale * Vector3.one);
	}

	// Token: 0x06001ABF RID: 6847 RVA: 0x000C3F4B File Offset: 0x000C234B
	public void SetScale(Vector3 scale)
	{
		this.mainGameObject.transform.localScale = scale;
	}

	// Token: 0x06001AC0 RID: 6848 RVA: 0x000C3F5E File Offset: 0x000C235E
	public Vector2 GetScale()
	{
		return Vector3.Scale(this.mainGameObject.transform.localScale, TileObject.sizeTile);
	}

	// Token: 0x06001AC1 RID: 6849 RVA: 0x000C3F84 File Offset: 0x000C2384
	public GameObject GetGameObject()
	{
		return this.mainGameObject;
	}

	// Token: 0x06001AC2 RID: 6850 RVA: 0x000C3F8C File Offset: 0x000C238C
	public void SetParent(Transform p)
	{
		this.mainGameObject.transform.parent = p;
	}

	// Token: 0x06001AC3 RID: 6851 RVA: 0x000C3FA0 File Offset: 0x000C23A0
	public Vector3 CalculateTileOffset()
	{
		Bounds bounds = default(Bounds);
		foreach (Vector3 point in this.backgroundMeshFilter.sharedMesh.vertices)
		{
			bounds.Encapsulate(point);
		}
		return bounds.center;
	}

	// Token: 0x06001AC4 RID: 6852 RVA: 0x000C3FF8 File Offset: 0x000C23F8
	public void Cleanup()
	{
		this.Enable();
		if (this.iconLoader != null)
		{
			this.iconLoader.CancelLoad();
		}
		if (!TileIconManager.Instance.IsPreloadedIcon(this.mainTileTexture) && this.imageSource != TilePool.TileImageSource.StandaloneImageManger && this._thumbnailWorldInfo == null)
		{
			this.mainTileTexture.Resize(1, 1);
		}
		if (this._thumbnailWorldInfo != null)
		{
			this._thumbnailWorldInfo.ThumbnailLoaded -= this.WorldInfoLoadedScreenshot;
			this._thumbnailWorldInfo = null;
		}
		this.poolIndex = -1;
		this.iconTransform.localPosition = new Vector3(0f, 0f, -0.1f);
		this.iconTransform.localScale = Vector3.one;
		base.transform.position = Vector3.zero;
		base.transform.localScale = Vector3.one;
		this.mainTileRenderer.enabled = false;
		this.mainTileMaterial.mainTexture = null;
		this.backgroundRenderer.enabled = false;
		this.rarityBorderRenderer.enabled = false;
		this.childTileObject = null;
		this.isOverlay = false;
		this.hasIcon = false;
		this.iconLoaded = false;
		this.hasLabel = false;
		this._blockItem = null;
		this._gaf = null;
	}

	// Token: 0x06001AC5 RID: 6853 RVA: 0x000C413C File Offset: 0x000C253C
	public void ReturnToPool()
	{
		if (this.childTileObject != null)
		{
			this.childTileObject.ReturnToPool();
			this.childTileObject = null;
		}
		if (this.obtainedFromPool != null)
		{
			this.obtainedFromPool.RecycleTileObject(this);
		}
		else
		{
			BWLog.Error("Trying to return tile to pool but tile was not created from pool");
		}
	}

	// Token: 0x06001AC6 RID: 6854 RVA: 0x000C4192 File Offset: 0x000C2592
	public GameObject GetIconGameObject()
	{
		return this.iconTransform.gameObject;
	}

	// Token: 0x06001AC7 RID: 6855 RVA: 0x000C41A0 File Offset: 0x000C25A0
	public void OverrideBackgroundColor(Color color)
	{
		this.backgroundMeshFilter.sharedMesh.colors = new Color[]
		{
			color,
			color,
			color,
			color
		};
	}

	// Token: 0x06001AC8 RID: 6856 RVA: 0x000C41F8 File Offset: 0x000C25F8
	public void OverrideForegroundColor(Color color)
	{
		this.iconMeshFilter.sharedMesh.colors = new Color[]
		{
			color,
			color,
			color,
			color
		};
	}

	// Token: 0x06001AC9 RID: 6857 RVA: 0x000C4250 File Offset: 0x000C2650
	private void Update()
	{
		if (this.iconLoader != null && this.iconLoader.loadState == TileIconLoadState.Loaded)
		{
			this.iconLoaded = true;
			this.iconLoader.ApplyTexture(this.mainTileTexture);
			this.mainTileMaterial.mainTexture = this.mainTileTexture;
			this.RefreshMainTileAlpha();
			this.RefreshMainTileScale();
			this.mainTileRenderer.enabled = true;
		}
	}

	// Token: 0x06001ACA RID: 6858 RVA: 0x000C42BA File Offset: 0x000C26BA
	private void OnDisable()
	{
		if (this.iconLoader != null)
		{
			this.iconLoader.CancelLoad();
		}
	}

	// Token: 0x06001ACB RID: 6859 RVA: 0x000C42D4 File Offset: 0x000C26D4
	private void OnEnable()
	{
		if (this.hasIcon && !this.iconLoaded)
		{
			if (this._gaf != null)
			{
				this.Setup(this._gaf, this.isEnabled);
			}
			else if (this._iconName != null)
			{
				this.SetupForIcon(this._iconName, this.isEnabled);
			}
		}
	}

	// Token: 0x06001ACC RID: 6860 RVA: 0x000C4338 File Offset: 0x000C2738
	public bool Hit(Vector3 v, bool allowDisabledTiles = false)
	{
		Vector3 position = this.GetPosition();
		float num = -7f * NormalizedScreen.pixelScale;
		Vector2 scale = this.GetScale();
		return (allowDisabledTiles || this.isEnabled) && v.x >= position.x - num && v.x <= position.x + scale.x + num && v.y >= position.y - num && v.y <= position.y + scale.y + num;
	}

	// Token: 0x06001ACD RID: 6861 RVA: 0x000C43DC File Offset: 0x000C27DC
	public bool Hit40(Vector3 v)
	{
		float num = -15f * NormalizedScreen.pixelScale;
		Vector2 scale = this.GetScale();
		return this.isEnabled && v.x >= this.GetPosition().x - num && v.x <= this.GetPosition().x + scale.x + num && v.y >= this.GetPosition().y - num && v.y <= this.GetPosition().y + scale.y + num;
	}

	// Token: 0x06001ACE RID: 6862 RVA: 0x000C448C File Offset: 0x000C288C
	public bool HitExtended(Vector3 v, float extendXMin, float extendXMax, float extendYMin, float extendYMax, bool allowDisabledTiles = false)
	{
		float num = -7f * NormalizedScreen.pixelScale;
		Vector2 scale = this.GetScale();
		return (allowDisabledTiles || this.isEnabled) && v.x + extendXMin >= this.GetPosition().x - num && v.x - extendXMax <= this.GetPosition().x + scale.x + num && v.y + extendYMin >= this.GetPosition().y - num && v.y - extendYMax <= this.GetPosition().y + scale.y + num;
	}

	// Token: 0x06001ACF RID: 6863 RVA: 0x000C4550 File Offset: 0x000C2950
	public static TileObject CreateTemplate(TilePool.TileImageSource imageSource)
	{
		GameObject gameObject = new GameObject("Tile");
		GameObject gameObject2 = new GameObject("TileIcon");
		GameObject gameObject3 = new GameObject("RarityBorder");
		GameObject gameObject4 = new GameObject("Label");
		TileObject tileObject = gameObject.AddComponent<TileObject>();
		tileObject.imageSource = imageSource;
		tileObject.mainGameObject = gameObject;
		tileObject.iconTransform = gameObject2.transform;
		if (TileObject.sharedBorderMesh == null)
		{
			TileObject.sharedBorderMesh = TileObject.CreateQuadMesh(TileObject.sizeBorder);
		}
		tileObject.tileIconMesh = TileObject.CreateQuadMesh(TileObject.sizeTile);
		tileObject.tileBackgroundMesh = TileObject.CreateQuadMesh(TileObject.sizeTile);
		tileObject.labelMesh = TileObject.CreateQuadMesh(TileObject.sizeLabel);
		TileObject.AddQuadMeshOnGameObject(gameObject2, tileObject.tileIconMesh);
		TileObject.AddQuadMeshOnGameObject(gameObject, tileObject.tileBackgroundMesh);
		TileObject.AddQuadMeshOnGameObject(gameObject4, tileObject.labelMesh);
		TileObject.AddQuadMeshOnGameObject(gameObject3, TileObject.sharedBorderMesh);
		tileObject.iconMeshFilter = gameObject2.GetComponent<MeshFilter>();
		tileObject.backgroundMeshFilter = gameObject.GetComponent<MeshFilter>();
		tileObject.labelMeshFilter = gameObject4.GetComponent<MeshFilter>();
		tileObject.mainTileRenderer = gameObject2.GetComponent<MeshRenderer>();
		tileObject.backgroundRenderer = gameObject.GetComponent<MeshRenderer>();
		tileObject.rarityBorderRenderer = gameObject3.GetComponent<MeshRenderer>();
		tileObject.labelRenderer = gameObject4.GetComponent<MeshRenderer>();
		MeshUtils.DisableUnusedProperties(tileObject.mainTileRenderer);
		MeshUtils.DisableUnusedProperties(tileObject.backgroundRenderer);
		MeshUtils.DisableUnusedProperties(tileObject.rarityBorderRenderer);
		MeshUtils.DisableUnusedProperties(tileObject.labelRenderer);
		if (TileObject.iconBaseMaterial == null)
		{
			TileObject.iconBaseMaterial = new Material(Resources.Load<Material>("GUI/TileObjectForeground"));
		}
		tileObject.mainTileMaterial = new Material(TileObject.iconBaseMaterial);
		tileObject.mainTileTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		tileObject.mainTileMaterial.mainTexture = tileObject.mainTileTexture;
		tileObject.mainTileRenderer.sharedMaterial = tileObject.mainTileMaterial;
		tileObject.iconTransform.parent = gameObject.transform;
		gameObject3.transform.parent = gameObject.transform;
		gameObject4.transform.parent = gameObject.transform;
		tileObject.iconTransform.localPosition = new Vector3(0f, 0f, -0.1f);
		gameObject3.transform.localPosition = new Vector3((TileObject.sizeTile.x - TileObject.sizeBorder.x) / 2f, (TileObject.sizeTile.y - TileObject.sizeBorder.y) / 2f, -0.2f);
		gameObject4.transform.localPosition = new Vector3((TileObject.sizeTile.x - TileObject.sizeLabel.x) / 2f, 1f, -0.15f);
		if (imageSource == TilePool.TileImageSource.Resources)
		{
			tileObject.iconLoader = new TileIconResourceHandle();
		}
		else if (imageSource == TilePool.TileImageSource.StandaloneImageManger)
		{
			tileObject.iconLoader = new TileIconImageMangerHandle(tileObject.GetInstanceID());
		}
		else
		{
			tileObject.iconLoader = new TileIconHandle();
		}
		tileObject.Hide();
		return tileObject;
	}

	// Token: 0x06001AD0 RID: 6864 RVA: 0x000C4848 File Offset: 0x000C2C48
	private static Mesh CreateQuadMesh(Vector2 size)
	{
		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		int[] triangles = new int[6];
		vertices = new Vector3[]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(size.x, 0f, 0f),
			new Vector3(0f, size.y, 0f),
			new Vector3(size.x, size.y, 0f)
		};
		uv = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		triangles = new int[]
		{
			0,
			2,
			1,
			1,
			2,
			3
		};
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		return mesh;
	}

	// Token: 0x06001AD1 RID: 6865 RVA: 0x000C499C File Offset: 0x000C2D9C
	private static void AddQuadMeshOnGameObject(GameObject obj, Mesh mesh)
	{
		obj.layer = LayerMask.NameToLayer("GUI");
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		MeshUtils.AddBWDefaultMeshRenderer(obj);
		meshFilter.mesh = mesh;
	}

	// Token: 0x04001659 RID: 5721
	public int poolIndex;

	// Token: 0x0400165A RID: 5722
	public TilePool obtainedFromPool;

	// Token: 0x0400165B RID: 5723
	private GameObject mainGameObject;

	// Token: 0x0400165C RID: 5724
	private GameObject tileBackground;

	// Token: 0x0400165D RID: 5725
	private GameObject labelObject;

	// Token: 0x0400165E RID: 5726
	private GameObject rarityBorderObject;

	// Token: 0x0400165F RID: 5727
	private MeshRenderer mainTileRenderer;

	// Token: 0x04001660 RID: 5728
	private MeshRenderer backgroundRenderer;

	// Token: 0x04001661 RID: 5729
	private MeshRenderer rarityBorderRenderer;

	// Token: 0x04001662 RID: 5730
	private MeshRenderer labelRenderer;

	// Token: 0x04001663 RID: 5731
	private Material mainTileMaterial;

	// Token: 0x04001664 RID: 5732
	private Material backgroundMaterial;

	// Token: 0x04001665 RID: 5733
	private Texture2D mainTileTexture;

	// Token: 0x04001666 RID: 5734
	private TileIconHandle iconLoader;

	// Token: 0x04001667 RID: 5735
	private Transform iconTransform;

	// Token: 0x04001668 RID: 5736
	private GAF _gaf;

	// Token: 0x04001669 RID: 5737
	private BlockItem _blockItem;

	// Token: 0x0400166A RID: 5738
	private string _iconName;

	// Token: 0x0400166B RID: 5739
	private WorldInfo _thumbnailWorldInfo;

	// Token: 0x0400166C RID: 5740
	private Mesh tileIconMesh;

	// Token: 0x0400166D RID: 5741
	private Mesh tileBackgroundMesh;

	// Token: 0x0400166E RID: 5742
	private Mesh labelMesh;

	// Token: 0x0400166F RID: 5743
	private MeshFilter iconMeshFilter;

	// Token: 0x04001670 RID: 5744
	private MeshFilter backgroundMeshFilter;

	// Token: 0x04001671 RID: 5745
	private MeshFilter labelMeshFilter;

	// Token: 0x04001672 RID: 5746
	private Material enabledBackgroundMaterial;

	// Token: 0x04001673 RID: 5747
	private Material disabledBackgroundMaterial;

	// Token: 0x04001674 RID: 5748
	private Material enabledLabelMaterial;

	// Token: 0x04001675 RID: 5749
	private Material disabledLabelMaterial;

	// Token: 0x04001676 RID: 5750
	private TilePool.TileImageSource imageSource;

	// Token: 0x04001677 RID: 5751
	private bool isEnabled = true;

	// Token: 0x04001678 RID: 5752
	private bool hasIcon;

	// Token: 0x04001679 RID: 5753
	private bool iconLoaded;

	// Token: 0x0400167A RID: 5754
	private bool hasLabel;

	// Token: 0x0400167B RID: 5755
	public TileObject childTileObject;

	// Token: 0x0400167C RID: 5756
	public bool isOverlay;

	// Token: 0x0400167D RID: 5757
	public static Vector2 sizeTile = new Vector2(67f, 67f);

	// Token: 0x0400167E RID: 5758
	private static Vector2 sizeLabel = new Vector2(63f, 14f);

	// Token: 0x0400167F RID: 5759
	private static Vector2 sizeBorder = new Vector2(64f, 64f);

	// Token: 0x04001680 RID: 5760
	private static Material iconBaseMaterial;

	// Token: 0x04001681 RID: 5761
	private static Mesh sharedBorderMesh;
}
