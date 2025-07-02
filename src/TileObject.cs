using System;
using UnityEngine;

public class TileObject : MonoBehaviour
{
	public int poolIndex;

	public TilePool obtainedFromPool;

	private GameObject mainGameObject;

	private GameObject tileBackground;

	private GameObject labelObject;

	private GameObject rarityBorderObject;

	private MeshRenderer mainTileRenderer;

	private MeshRenderer backgroundRenderer;

	private MeshRenderer rarityBorderRenderer;

	private MeshRenderer labelRenderer;

	private Material mainTileMaterial;

	private Material backgroundMaterial;

	private Texture2D mainTileTexture;

	private TileIconHandle iconLoader;

	private Transform iconTransform;

	private GAF _gaf;

	private BlockItem _blockItem;

	private string _iconName;

	private WorldInfo _thumbnailWorldInfo;

	private Mesh tileIconMesh;

	private Mesh tileBackgroundMesh;

	private Mesh labelMesh;

	private MeshFilter iconMeshFilter;

	private MeshFilter backgroundMeshFilter;

	private MeshFilter labelMeshFilter;

	private Material enabledBackgroundMaterial;

	private Material disabledBackgroundMaterial;

	private Material enabledLabelMaterial;

	private Material disabledLabelMaterial;

	private TilePool.TileImageSource imageSource;

	private bool isEnabled = true;

	private bool hasIcon;

	private bool iconLoaded;

	private bool hasLabel;

	public TileObject childTileObject;

	public bool isOverlay;

	public static Vector2 sizeTile = new Vector2(67f, 67f);

	private static Vector2 sizeLabel = new Vector2(63f, 14f);

	private static Vector2 sizeBorder = new Vector2(64f, 64f);

	private static Material iconBaseMaterial;

	private static Mesh sharedBorderMesh;

	public bool IsIconLoaded()
	{
		return iconLoaded;
	}

	public void MoveTo(float x, float y)
	{
		float z = mainGameObject.transform.position.z;
		MoveTo(new Vector3(x, y, z));
	}

	public void MoveTo(Vector3 pos)
	{
		mainGameObject.transform.position = pos;
	}

	public void LocalMoveTo(Vector3 pos)
	{
		mainGameObject.transform.localPosition = pos;
	}

	public void SetupForIcon(string iconName, bool enabled)
	{
		_iconName = iconName;
		if (TileIconManager.Instance.preloadedIcons.ContainsKey(iconName))
		{
			mainTileTexture = TileIconManager.Instance.preloadedIcons[iconName];
			mainTileRenderer.enabled = true;
			mainTileMaterial.mainTexture = mainTileTexture;
			RefreshMainTileAlpha();
			RefreshMainTileScale();
			hasIcon = true;
			iconLoaded = true;
			iconLoader.loadState = TileIconLoadState.Applied;
		}
		else
		{
			TileIconInfo tileInfoForIcon = TileIconManager.Instance.GetTileInfoForIcon(iconName);
			hasIcon = TileIconManager.Instance.RequestIconLoad(tileInfoForIcon.filePath, iconLoader);
			if (hasIcon)
			{
				iconLoaded = false;
				mainTileMaterial.mainTexture = null;
			}
			mainTileRenderer.enabled = false;
		}
		labelRenderer.enabled = false;
		backgroundRenderer.enabled = false;
		rarityBorderRenderer.enabled = false;
		SetScale(Vector3.one * NormalizedScreen.pixelScale);
		isEnabled = enabled;
		RefreshMainTileAlpha();
	}

	public void Setup(GAF gaf, bool enabled)
	{
		_gaf = gaf;
		_blockItem = BlockItem.FindByGafPredicateNameAndArguments(gaf.Predicate.Name, gaf.Args);
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
				string text = gaf.Predicate.Name;
				if (text != "Sky.SkyBoxTo" && text != "AnimCharacter.ReplaceBodyPart")
				{
					tileIconInfo.label = text.Substring(text.IndexOf('.') + 1);
				}
			}
		}
		if (tileIconInfo == null)
		{
			BWLog.Info(string.Concat("NULL tile info, disabling renderer for tile: ", gaf, "\nBlock item: ", _blockItem, "\nMost likely it is unable to find the corresponding block item for this GAF because the declaration of argument types in the Register function does not match the types in use in the code!"));
			mainTileRenderer.enabled = false;
			labelRenderer.enabled = false;
			backgroundRenderer.enabled = false;
			return;
		}
		if (flag2)
		{
			if (isOverlay)
			{
				string buttonInputVariantKeyImagePath = TileIconManager.Instance.GetButtonInputVariantKeyImagePath(gaf);
				if (!string.IsNullOrEmpty(buttonInputVariantKeyImagePath))
				{
					tileIconInfo.filePath = buttonInputVariantKeyImagePath;
				}
			}
			else if (childTileObject != null)
			{
				childTileObject.Setup(gaf, enabled);
			}
			else
			{
				childTileObject = obtainedFromPool.GetTileObject(gaf, isEnabled, asOverlay: true);
				childTileObject.transform.SetParent(mainGameObject.transform, worldPositionStays: false);
				childTileObject.transform.localScale = Vector3.one;
			}
		}
		if (flag && Blocksworld.modelCollection.IsSourceLocked(gaf))
		{
			if (isOverlay)
			{
				tileIconInfo.filePath = TileIconManager.GetPathToIcon("Misc/Locked_Model_Icon_Overlay");
			}
			else if (childTileObject != null)
			{
				childTileObject.Setup(gaf, enabled);
			}
			else
			{
				childTileObject = Blocksworld.tilePool.GetTileObject(gaf, isEnabled, asOverlay: true);
				childTileObject.transform.SetParent(mainGameObject.transform, worldPositionStays: false);
				childTileObject.transform.localScale = Vector3.one;
			}
		}
		bool flag3 = false;
		if (_gaf.Predicate.Name == "BlockWorldJumper.Jump")
		{
			string text2 = (string)gaf.Args[0];
			if (!string.IsNullOrEmpty(text2))
			{
				WorldInfo worldWithId = WorldSession.current.availableTeleportWorlds.GetWorldWithId(text2);
				if (worldWithId != null)
				{
					flag3 = true;
					LinkToWorldThumbnail(worldWithId);
				}
			}
		}
		if (flag)
		{
			string modelID = Blocksworld.modelCollection.GetModelID(gaf);
			string modelType = Blocksworld.modelCollection.GetModelType(gaf);
			hasIcon = TileIconManager.Instance.RequestModelIconLoad(tileIconInfo.filePath, modelID, modelType, iconLoader);
		}
		else if (!flag3)
		{
			hasIcon = TileIconManager.Instance.RequestIconLoad(tileIconInfo.filePath, iconLoader);
		}
		if (!hasIcon)
		{
			mainTileRenderer.enabled = false;
		}
		else if (!flag3)
		{
			iconLoaded = false;
			mainTileMaterial.mainTexture = null;
			mainTileRenderer.enabled = false;
		}
		hasLabel = (_gaf.HasBuildPanelLabel || _gaf.HasDynamicLabel()) && !string.IsNullOrEmpty(tileIconInfo.label) && !isOverlay;
		if (hasLabel)
		{
			TileLabelAtlas labelAtlas = TileIconManager.Instance.labelAtlas;
			string text3 = tileIconInfo.label;
			if (_gaf.HasDynamicLabel() && !flag)
			{
				text3 = _gaf.GetDynamicLabel();
			}
			if (!labelAtlas.Contains(text3))
			{
				labelAtlas.AddNewLabel(text3);
			}
			Vector2[] labelUVs = labelAtlas.GetLabelUVs(text3);
			if (labelUVs != null && labelUVs.Length >= 4)
			{
				string text4 = "UVs ";
				for (int i = 0; i < 4; i++)
				{
					text4 = text4 + labelUVs[i].ToString() + ((i >= 3) ? string.Empty : ", ");
				}
				enabledLabelMaterial = labelAtlas.GetMaterial(text3);
				disabledLabelMaterial = labelAtlas.GetMaterialDisabled(text3);
				if (enabledLabelMaterial != null && disabledLabelMaterial != null)
				{
					labelMeshFilter.sharedMesh.uv = labelUVs;
					labelRenderer.sharedMaterial = ((!enabled) ? disabledLabelMaterial : enabledLabelMaterial);
					labelRenderer.enabled = true;
				}
				else
				{
					BWLog.Info("No label materials for label " + text3);
					labelRenderer.enabled = false;
				}
			}
			else
			{
				BWLog.Info("No label uvs for : " + _gaf);
				labelRenderer.enabled = false;
			}
		}
		else
		{
			labelRenderer.enabled = false;
		}
		if (tileIconInfo.clearBackground || isOverlay)
		{
			backgroundRenderer.enabled = false;
		}
		else
		{
			enabledBackgroundMaterial = TileIconManager.Instance.GetBackgroundMaterial(_gaf);
			disabledBackgroundMaterial = TileIconManager.Instance.GetBackgroundMaterialDisabled(_gaf);
			backgroundRenderer.sharedMaterial = ((!enabled) ? disabledBackgroundMaterial : enabledBackgroundMaterial);
			Color[] colors = Blocksworld.GetColors(tileIconInfo.backgroundColorName);
			backgroundMeshFilter.sharedMesh.colors = new Color[4]
			{
				colors[0],
				colors[0],
				colors[1],
				colors[1]
			};
			backgroundRenderer.enabled = true;
		}
		if (_blockItem != null && _blockItem.HasRarityBorder)
		{
			rarityBorderRenderer.sharedMaterial = ((!enabled) ? Blocksworld.rarityBorderMaterialsDisabled[_blockItem.Rarity] : Blocksworld.rarityBorderMaterialsEnabled[_blockItem.Rarity]);
			rarityBorderRenderer.enabled = true;
		}
		else
		{
			rarityBorderRenderer.enabled = false;
		}
		SetScale(Vector3.one * NormalizedScreen.pixelScale);
		isEnabled = enabled;
		RefreshMainTileAlpha();
	}

	private void LinkToWorldThumbnail(WorldInfo worldInfo)
	{
		_thumbnailWorldInfo = worldInfo;
		worldInfo.ThumbnailLoaded += WorldInfoLoadedScreenshot;
		Texture2D worldThumbnailForTiles = worldInfo.GetWorldThumbnailForTiles();
		backgroundRenderer.enabled = false;
		labelRenderer.enabled = false;
		rarityBorderRenderer.enabled = false;
		hasIcon = true;
		if (worldThumbnailForTiles == null)
		{
			worldInfo.LoadThumbnail();
			iconLoaded = false;
			mainTileRenderer.enabled = false;
			return;
		}
		iconLoaded = true;
		mainTileTexture = worldThumbnailForTiles;
		mainTileRenderer.enabled = true;
		mainTileMaterial.mainTexture = mainTileTexture;
		RefreshMainTileAlpha();
		RefreshMainTileScale();
		iconLoader.loadState = TileIconLoadState.Applied;
	}

	private void WorldInfoLoadedScreenshot(object sender, EventArgs e)
	{
		if (sender == _thumbnailWorldInfo)
		{
			Texture2D worldThumbnailForTiles = _thumbnailWorldInfo.GetWorldThumbnailForTiles();
			if (worldThumbnailForTiles == null)
			{
				BWLog.Error("Failed to load thumbnail");
				return;
			}
			hasIcon = true;
			iconLoaded = true;
			mainTileTexture = worldThumbnailForTiles;
			mainTileRenderer.enabled = true;
			mainTileMaterial.mainTexture = mainTileTexture;
			RefreshMainTileAlpha();
			RefreshMainTileScale();
			iconLoader.loadState = TileIconLoadState.Applied;
		}
	}

	public bool IsSetupForGAF(GAF gaf)
	{
		if (gaf.Equals(_gaf))
		{
			return hasIcon;
		}
		return false;
	}

	public bool IsActive()
	{
		return mainGameObject.activeSelf;
	}

	public string IconName()
	{
		return _iconName;
	}

	public void Show(bool show)
	{
		if (show)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Show()
	{
		mainGameObject.SetActive(value: true);
		mainTileRenderer.enabled = hasIcon && iconLoaded;
		RefreshMainTileAlpha();
		if (childTileObject != null)
		{
			childTileObject.Show();
		}
	}

	public void Hide()
	{
		mainGameObject.SetActive(value: false);
		if (childTileObject != null)
		{
			childTileObject.Hide();
		}
	}

	public bool IsShowing()
	{
		if (IsActive())
		{
			return mainTileRenderer.enabled;
		}
		return false;
	}

	public bool IsEnabled()
	{
		return isEnabled;
	}

	public void Enable(bool enable)
	{
		if (enable)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	public void Enable()
	{
		if (childTileObject != null)
		{
			childTileObject.Enable();
		}
		isEnabled = true;
		if (hasIcon && iconLoaded)
		{
			mainTileRenderer.enabled = true;
		}
		RefreshMainTileAlpha();
		RefreshMainTileScale();
		if (backgroundRenderer.enabled)
		{
			backgroundRenderer.sharedMaterial = enabledBackgroundMaterial;
		}
		if (labelRenderer.enabled)
		{
			labelRenderer.sharedMaterial = enabledLabelMaterial;
		}
		if (_blockItem != null && _blockItem.HasRarityBorder)
		{
			rarityBorderRenderer.sharedMaterial = Blocksworld.rarityBorderMaterialsEnabled[_blockItem.Rarity];
			rarityBorderRenderer.enabled = true;
		}
		else
		{
			rarityBorderRenderer.enabled = false;
		}
	}

	public void Disable()
	{
		if (childTileObject != null)
		{
			childTileObject.Disable();
		}
		isEnabled = false;
		if (hasIcon && iconLoaded)
		{
			mainTileRenderer.enabled = true;
		}
		RefreshMainTileAlpha();
		RefreshMainTileScale();
		backgroundRenderer.sharedMaterial = disabledBackgroundMaterial;
		if (labelRenderer.enabled)
		{
			labelRenderer.sharedMaterial = disabledLabelMaterial;
		}
		if (_blockItem != null && _blockItem.HasRarityBorder)
		{
			rarityBorderRenderer.sharedMaterial = Blocksworld.rarityBorderMaterialsDisabled[_blockItem.Rarity];
			rarityBorderRenderer.enabled = true;
		}
		else
		{
			rarityBorderRenderer.enabled = false;
		}
	}

	public void SetGhosted(bool ghosted)
	{
		if (ghosted)
		{
			bool flag = isEnabled;
			Disable();
			isEnabled = flag;
		}
		else if (isEnabled)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	private void RefreshMainTileAlpha()
	{
		float value = ((!isEnabled) ? 0.3f : 1f);
		mainTileMaterial.SetFloat("_Alpha", value);
	}

	private void RefreshMainTileScale()
	{
		if (isOverlay)
		{
			return;
		}
		if (hasIcon && iconLoaded)
		{
			float num = mainTileTexture.width;
			float num2 = mainTileTexture.height;
			float num3 = NormalizedScreen.scale * TileIconManager.iconScaleFactor;
			float x = 2f + Mathf.Ceil(31.5f - num / (2f * num3));
			float y = 2f + Mathf.Ceil(31.5f - num2 / (2f * num3));
			float num4 = num / num3;
			float num5 = num2 / num3;
			if (hasLabel && _thumbnailWorldInfo == null)
			{
				y = Mathf.Ceil((sizeTile.x + 14f) / 2f - num2 / (2f * num3));
			}
			iconTransform.localPosition = new Vector3(x, y, iconTransform.localPosition.z);
			iconTransform.localScale = new Vector3(num4 / sizeTile.x, num5 / sizeTile.y, 1f);
		}
		else
		{
			iconTransform.localPosition = new Vector3(0f, 0f, -0.1f);
			iconTransform.localScale = Vector3.one;
		}
	}

	public void SetPosition(Vector3 position)
	{
		mainGameObject.transform.position = position;
	}

	public Vector3 GetPosition()
	{
		return mainGameObject.transform.position;
	}

	public Vector3 GetCenterPosition()
	{
		Vector3 position = mainGameObject.transform.position;
		Vector3 vector = GetScale();
		Vector3 vector2 = new Vector3(vector.x / 2f, vector.y / 2f, 0f);
		return position + vector2;
	}

	public void SetLocalPosition(Vector3 position)
	{
		mainGameObject.transform.localPosition = position;
	}

	public Vector3 GetLocalPosition()
	{
		return mainGameObject.transform.localPosition;
	}

	public void SetScale(float scale)
	{
		SetScale(scale * Vector3.one);
	}

	public void SetScale(Vector3 scale)
	{
		mainGameObject.transform.localScale = scale;
	}

	public Vector2 GetScale()
	{
		return Vector3.Scale(mainGameObject.transform.localScale, sizeTile);
	}

	public GameObject GetGameObject()
	{
		return mainGameObject;
	}

	public void SetParent(Transform p)
	{
		mainGameObject.transform.parent = p;
	}

	public Vector3 CalculateTileOffset()
	{
		Bounds bounds = default(Bounds);
		Vector3[] vertices = backgroundMeshFilter.sharedMesh.vertices;
		foreach (Vector3 point in vertices)
		{
			bounds.Encapsulate(point);
		}
		return bounds.center;
	}

	public void Cleanup()
	{
		Enable();
		if (iconLoader != null)
		{
			iconLoader.CancelLoad();
		}
		if (!TileIconManager.Instance.IsPreloadedIcon(mainTileTexture) && imageSource != TilePool.TileImageSource.StandaloneImageManger && _thumbnailWorldInfo == null)
		{
			mainTileTexture.Resize(1, 1);
		}
		if (_thumbnailWorldInfo != null)
		{
			_thumbnailWorldInfo.ThumbnailLoaded -= WorldInfoLoadedScreenshot;
			_thumbnailWorldInfo = null;
		}
		poolIndex = -1;
		iconTransform.localPosition = new Vector3(0f, 0f, -0.1f);
		iconTransform.localScale = Vector3.one;
		base.transform.position = Vector3.zero;
		base.transform.localScale = Vector3.one;
		mainTileRenderer.enabled = false;
		mainTileMaterial.mainTexture = null;
		backgroundRenderer.enabled = false;
		rarityBorderRenderer.enabled = false;
		childTileObject = null;
		isOverlay = false;
		hasIcon = false;
		iconLoaded = false;
		hasLabel = false;
		_blockItem = null;
		_gaf = null;
	}

	public void ReturnToPool()
	{
		if (childTileObject != null)
		{
			childTileObject.ReturnToPool();
			childTileObject = null;
		}
		if (obtainedFromPool != null)
		{
			obtainedFromPool.RecycleTileObject(this);
		}
		else
		{
			BWLog.Error("Trying to return tile to pool but tile was not created from pool");
		}
	}

	public GameObject GetIconGameObject()
	{
		return iconTransform.gameObject;
	}

	public void OverrideBackgroundColor(Color color)
	{
		backgroundMeshFilter.sharedMesh.colors = new Color[4] { color, color, color, color };
	}

	public void OverrideForegroundColor(Color color)
	{
		iconMeshFilter.sharedMesh.colors = new Color[4] { color, color, color, color };
	}

	private void Update()
	{
		if (iconLoader != null && iconLoader.loadState == TileIconLoadState.Loaded)
		{
			iconLoaded = true;
			iconLoader.ApplyTexture(mainTileTexture);
			mainTileMaterial.mainTexture = mainTileTexture;
			RefreshMainTileAlpha();
			RefreshMainTileScale();
			mainTileRenderer.enabled = true;
		}
	}

	private void OnDisable()
	{
		if (iconLoader != null)
		{
			iconLoader.CancelLoad();
		}
	}

	private void OnEnable()
	{
		if (hasIcon && !iconLoaded)
		{
			if (_gaf != null)
			{
				Setup(_gaf, isEnabled);
			}
			else if (_iconName != null)
			{
				SetupForIcon(_iconName, isEnabled);
			}
		}
	}

	public bool Hit(Vector3 v, bool allowDisabledTiles = false)
	{
		Vector3 position = GetPosition();
		float num = -7f * NormalizedScreen.pixelScale;
		Vector2 scale = GetScale();
		if ((allowDisabledTiles || isEnabled) && v.x >= position.x - num && v.x <= position.x + scale.x + num && v.y >= position.y - num)
		{
			return v.y <= position.y + scale.y + num;
		}
		return false;
	}

	public bool Hit40(Vector3 v)
	{
		float num = -15f * NormalizedScreen.pixelScale;
		Vector2 scale = GetScale();
		if (isEnabled && v.x >= GetPosition().x - num && v.x <= GetPosition().x + scale.x + num && v.y >= GetPosition().y - num)
		{
			return v.y <= GetPosition().y + scale.y + num;
		}
		return false;
	}

	public bool HitExtended(Vector3 v, float extendXMin, float extendXMax, float extendYMin, float extendYMax, bool allowDisabledTiles = false)
	{
		float num = -7f * NormalizedScreen.pixelScale;
		Vector2 scale = GetScale();
		if ((allowDisabledTiles || isEnabled) && v.x + extendXMin >= GetPosition().x - num && v.x - extendXMax <= GetPosition().x + scale.x + num && v.y + extendYMin >= GetPosition().y - num)
		{
			return v.y - extendYMax <= GetPosition().y + scale.y + num;
		}
		return false;
	}

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
		if (sharedBorderMesh == null)
		{
			sharedBorderMesh = CreateQuadMesh(sizeBorder);
		}
		tileObject.tileIconMesh = CreateQuadMesh(sizeTile);
		tileObject.tileBackgroundMesh = CreateQuadMesh(sizeTile);
		tileObject.labelMesh = CreateQuadMesh(sizeLabel);
		AddQuadMeshOnGameObject(gameObject2, tileObject.tileIconMesh);
		AddQuadMeshOnGameObject(gameObject, tileObject.tileBackgroundMesh);
		AddQuadMeshOnGameObject(gameObject4, tileObject.labelMesh);
		AddQuadMeshOnGameObject(gameObject3, sharedBorderMesh);
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
		if (iconBaseMaterial == null)
		{
			iconBaseMaterial = new Material(Resources.Load<Material>("GUI/TileObjectForeground"));
		}
		tileObject.mainTileMaterial = new Material(iconBaseMaterial);
		tileObject.mainTileTexture = new Texture2D(1, 1, TextureFormat.RGBA32, mipmap: false);
		tileObject.mainTileMaterial.mainTexture = tileObject.mainTileTexture;
		tileObject.mainTileRenderer.sharedMaterial = tileObject.mainTileMaterial;
		tileObject.iconTransform.parent = gameObject.transform;
		gameObject3.transform.parent = gameObject.transform;
		gameObject4.transform.parent = gameObject.transform;
		tileObject.iconTransform.localPosition = new Vector3(0f, 0f, -0.1f);
		gameObject3.transform.localPosition = new Vector3((sizeTile.x - sizeBorder.x) / 2f, (sizeTile.y - sizeBorder.y) / 2f, -0.2f);
		gameObject4.transform.localPosition = new Vector3((sizeTile.x - sizeLabel.x) / 2f, 1f, -0.15f);
		switch (imageSource)
		{
		case TilePool.TileImageSource.Resources:
			tileObject.iconLoader = new TileIconResourceHandle();
			break;
		case TilePool.TileImageSource.StandaloneImageManger:
			tileObject.iconLoader = new TileIconImageMangerHandle(tileObject.GetInstanceID());
			break;
		default:
			tileObject.iconLoader = new TileIconHandle();
			break;
		}
		tileObject.Hide();
		return tileObject;
	}

	private static Mesh CreateQuadMesh(Vector2 size)
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		int[] array3 = new int[6];
		array = new Vector3[4]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(size.x, 0f, 0f),
			new Vector3(0f, size.y, 0f),
			new Vector3(size.x, size.y, 0f)
		};
		array2 = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		array3 = new int[6] { 0, 2, 1, 1, 2, 3 };
		mesh.vertices = array;
		mesh.triangles = array3;
		mesh.uv = array2;
		return mesh;
	}

	private static void AddQuadMeshOnGameObject(GameObject obj, Mesh mesh)
	{
		obj.layer = LayerMask.NameToLayer("GUI");
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		MeshUtils.AddBWDefaultMeshRenderer(obj);
		meshFilter.mesh = mesh;
	}
}
