using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;

public class TileIconManager
{
	private static TileIconManager _instance;

	private const int streamCount = 16;

	private int _newLoadLimit = -1;

	private List<TileIconHandle> waitingToLoad;

	private List<TileIconHandle> waitingForModelIconDownload;

	private List<TileIconHandle> loading;

	public Dictionary<string, Texture2D> preloadedIcons;

	private Material iconBackgroundMaterial;

	private Material iconBackgroundMaterialDisabled;

	private Material iconBackgroundWithLabelMaterial;

	private Material iconBackgroundWithLabelMaterialDisabled;

	public TileLabelAtlas labelAtlas;

	public static TileIconManager Instance => _instance;

	public static float iconScaleFactor => 2f;

	private static string StreamingBasePath
	{
		get
		{
			string empty = string.Empty;
			string path = "HD";
			return Path.Combine(empty, Path.Combine("Icons", path));
		}
	}

	public TileIconManager()
	{
		waitingToLoad = new List<TileIconHandle>();
		waitingForModelIconDownload = new List<TileIconHandle>();
		loading = new List<TileIconHandle>();
		preloadedIcons = new Dictionary<string, Texture2D>();
		InitMaterials();
		InitLabels();
		PreloadIcons();
	}

	public static void Init()
	{
		_instance = new TileIconManager();
	}

	public static TileIconHandle CreateTileIconHandle()
	{
		return new TileIconResourceHandle();
	}

	private void InitMaterials()
	{
		string text = ((!Blocksworld.hd) ? "SD" : "HD");
		iconBackgroundMaterial = Resources.Load<Material>("TileBackgrounds/TileBackgroundEnabled" + text);
		iconBackgroundMaterialDisabled = Resources.Load<Material>("TileBackgrounds/TileBackgroundDisabled" + text);
		iconBackgroundWithLabelMaterial = Resources.Load<Material>("TileBackgrounds/TileBackgroundLabelEnabled" + text);
		iconBackgroundWithLabelMaterialDisabled = Resources.Load<Material>("TileBackgrounds/TileBackgroundLabelDisabled" + text);
	}

	private void InitLabels()
	{
		bool hd = Blocksworld.hd;
		hd |= BWStandalone.Instance != null;
		string path = ((!hd) ? "TileLabelFontBold SD" : "TileLabelFontBold HD");
		Font font = Resources.Load<Font>(path);
		labelAtlas = new TileLabelAtlas(font, hd);
	}

	private void PreloadIcons()
	{
		List<string> list = new List<string>
		{
			TBox.moveIconName,
			TBox.moveUpIconName,
			TBox.rotateIconName,
			TBox.scaleIconName,
			TBox.scaleUpIconName,
			TBox.lockedModelIconName,
			TBox.characterEditOnIconName,
			TBox.characterEditOffIconName,
			UIQuickSelect.scriptButtonIconName,
			UIQuickSelect.modelButtonIconName,
			"Buttons/Clear_Script",
			"Buttons/Copy_Script",
			"Buttons/Paste_Script",
			"Misc/Locked_Model_Icon_Overlay"
		};
		foreach (string item in list)
		{
			TileIconInfo tileInfoForIcon = GetTileInfoForIcon(item);
			string filePath = tileInfoForIcon.filePath;
			Texture2D texture2D = Resources.Load<Texture2D>(filePath);
			if (texture2D == null)
			{
				BWLog.Error("Failed to preload icon from resources: " + filePath);
			}
			else
			{
				preloadedIcons[item] = texture2D;
			}
		}
	}

	public bool IsPreloadedIcon(Texture2D iconTex)
	{
		return preloadedIcons.ContainsValue(iconTex);
	}

	public TileIconInfo GetTileInfo(GAF gaf)
	{
		int blockItemId = gaf.BlockItemId;
		if (blockItemId <= 0)
		{
			return null;
		}
		BlockItem blockItem = BlockItem.FindByID(blockItemId);
		if (blockItem != null)
		{
			return GetTileInfo(blockItem);
		}
		return null;
	}

	public string GetButtonInputVariantKeyImagePath(GAF gaf)
	{
		if (!gaf.IsButtonInput())
		{
			return null;
		}
		string stringArg = Util.GetStringArg(gaf.Args, 0, string.Empty);
		if (UIInputControl.controlVariantFromString.TryGetValue(stringArg, out var value))
		{
			string text = null;
			switch (value)
			{
			case UIInputControl.ControlVariant.Action:
				text = "Misc/Key_Action";
				break;
			case UIInputControl.ControlVariant.Attack:
				text = "Misc/Key_Attack";
				break;
			case UIInputControl.ControlVariant.Explode:
				text = "Misc/Key_Explode";
				break;
			case UIInputControl.ControlVariant.Help:
				text = "Misc/Key_Help";
				break;
			case UIInputControl.ControlVariant.Jump:
				text = "Misc/Key_Jump";
				break;
			case UIInputControl.ControlVariant.Laser:
				text = "Misc/Key_Laser";
				break;
			case UIInputControl.ControlVariant.Missile:
				text = "Misc/Key_Missile";
				break;
			case UIInputControl.ControlVariant.Mode:
				text = "Misc/Key_Mode";
				break;
			case UIInputControl.ControlVariant.Speak:
				text = "Misc/Key_Speak";
				break;
			case UIInputControl.ControlVariant.Speed:
				text = "Misc/Key_Speed";
				break;
			default:
			{
				if (UIInputControl.controlTypeFromString.TryGetValue(stringArg, out var value2))
				{
					switch (value2)
					{
					case UIInputControl.ControlType.Left:
						text = "Misc/Key_Left";
						break;
					case UIInputControl.ControlType.Right:
						text = "Misc/Key_Right";
						break;
					case UIInputControl.ControlType.Up:
						text = "Misc/Key_Up";
						break;
					case UIInputControl.ControlType.Down:
						text = "Misc/Key_Down";
						break;
					case UIInputControl.ControlType.L:
						text = "Misc/Key_L";
						break;
					case UIInputControl.ControlType.R:
						text = "Misc/Key_R";
						break;
					}
				}
				break;
			}
			}
			if (!string.IsNullOrEmpty(text))
			{
				return GetPathToIcon(text);
			}
		}
		return null;
	}

	public TileIconInfo GetTileInfo(BlockItem blockItem)
	{
		TileIconInfo tileIconInfo = new TileIconInfo();
		string text = blockItem.IconName;
		if (string.IsNullOrEmpty(text))
		{
			text = "Yellow/Warning_Triangle";
		}
		tileIconInfo.hasIcon = !string.IsNullOrEmpty(tileIconInfo.filePath = GetPathToIcon(text));
		tileIconInfo.label = blockItem.Label;
		tileIconInfo.backgroundColorName = blockItem.IconBackgroundColor;
		tileIconInfo.clearBackground = string.IsNullOrEmpty(tileIconInfo.backgroundColorName);
		return tileIconInfo;
	}

	public TileIconInfo GetTileInfoForIcon(string iconName)
	{
		TileIconInfo tileIconInfo = new TileIconInfo();
		if (string.IsNullOrEmpty(iconName))
		{
			iconName = "Yellow/Warning_Triangle";
		}
		tileIconInfo.hasIcon = !string.IsNullOrEmpty(tileIconInfo.filePath = (tileIconInfo.filePath = GetPathToIcon(iconName)));
		tileIconInfo.label = string.Empty;
		tileIconInfo.backgroundColorName = string.Empty;
		tileIconInfo.clearBackground = true;
		return tileIconInfo;
	}

	public string GetPathToIcon(GAF gaf)
	{
		return GetTileInfo(gaf)?.filePath;
	}

	public bool GAFHasIcon(GAF gaf)
	{
		TileIconInfo tileInfo = GetTileInfo(gaf);
		if (tileInfo != null)
		{
			return !string.IsNullOrEmpty(tileInfo.filePath);
		}
		return false;
	}

	public string GetLabelStr(GAF gaf)
	{
		return GetTileInfo(gaf)?.label;
	}

	public Material GetBackgroundMaterial(GAF gaf)
	{
		if (gaf.HasBuildPanelLabel || gaf.HasDynamicLabel())
		{
			return iconBackgroundWithLabelMaterial;
		}
		return iconBackgroundMaterial;
	}

	public Material GetBackgroundMaterialDisabled(GAF gaf)
	{
		if (gaf.HasBuildPanelLabel || gaf.HasDynamicLabel())
		{
			return iconBackgroundWithLabelMaterialDisabled;
		}
		return iconBackgroundMaterialDisabled;
	}

	public Texture2D GetBackgroundTexture(GAF gaf)
	{
		Material backgroundMaterial = GetBackgroundMaterial(gaf);
		if (backgroundMaterial != null)
		{
			return backgroundMaterial.mainTexture as Texture2D;
		}
		return null;
	}

	public bool RequestIconLoad(string path, TileIconHandle handle)
	{
		if (!string.IsNullOrEmpty(path))
		{
			handle.SetFilePath(path);
			handle.loadState = TileIconLoadState.WaitingToLoad;
			waitingToLoad.Add(handle);
			return true;
		}
		return false;
	}

	public bool RequestModelIconLoad(string path, string modelId, string modelType, TileIconHandle handle)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		handle.SetFilePath(path);
		if (handle.CheckFileExists())
		{
			handle.loadState = TileIconLoadState.WaitingToLoad;
			waitingToLoad.Add(handle);
			return true;
		}
		if (!string.IsNullOrEmpty(modelId) && !string.IsNullOrEmpty(modelType))
		{
			WorldSession.platformDelegate.RequestMissingModelIcon(modelType, modelId);
			handle.loadState = TileIconLoadState.WaitingForModelIconDownload;
			waitingForModelIconDownload.Add(handle);
			return true;
		}
		return false;
	}

	public void SetNewLoadLimit(int limit)
	{
		_newLoadLimit = limit;
	}

	public void ClearNewLoadLimit()
	{
		_newLoadLimit = -1;
	}

	public void Update()
	{
		int count = loading.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			TileIconHandle tileIconHandle = loading[num];
			if (tileIconHandle.loadState == TileIconLoadState.Loading)
			{
				tileIconHandle.UpdateLoad();
			}
			else if (tileIconHandle.loadState != TileIconLoadState.Loaded)
			{
				loading.RemoveAt(num);
			}
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num2 < waitingToLoad.Count && num3 < 128 && num4 < 8)
		{
			num3++;
			TileIconHandle tileIconHandle2 = waitingToLoad[num2];
			if (tileIconHandle2.loadState != TileIconLoadState.WaitingToLoad)
			{
				waitingToLoad.RemoveAt(num2);
			}
			else if (loading.Count < 16 && _newLoadLimit != 0)
			{
				tileIconHandle2.StartLoad();
				if (_newLoadLimit > 0)
				{
					_newLoadLimit--;
				}
				num4++;
				loading.Add(tileIconHandle2);
				waitingToLoad.RemoveAt(num2);
			}
			else
			{
				num2++;
			}
		}
	}

	public void CheckModelIcons()
	{
		for (int i = 0; i < waitingForModelIconDownload.Count; i++)
		{
			TileIconHandle tileIconHandle = waitingForModelIconDownload[i];
			if (tileIconHandle.loadState == TileIconLoadState.WaitingForModelIconDownload && tileIconHandle.CheckFileExists())
			{
				tileIconHandle.loadState = TileIconLoadState.WaitingToLoad;
				waitingToLoad.Add(tileIconHandle);
			}
		}
		waitingForModelIconDownload.RemoveAll((TileIconHandle handle) => handle.loadState != TileIconLoadState.WaitingForModelIconDownload);
	}

	public void CancelAllFileLoads()
	{
		waitingToLoad.Clear();
		waitingForModelIconDownload.Clear();
		for (int i = 0; i < loading.Count; i++)
		{
			loading[i].CancelLoad();
		}
		loading.Clear();
	}

	public static string GetPathToIcon(string iconName)
	{
		string streamingBasePath = StreamingBasePath;
		string text = "HD";
		string empty = string.Empty;
		return Path.Combine(StreamingBasePath, iconName + "_" + text + empty);
	}

	public static string IconDataForGAFString(string gafJsonStr)
	{
		JObject obj = JSONDecoder.Decode(gafJsonStr);
		GAF gAF = GAF.FromJSON(obj);
		if (gAF == null)
		{
			BWLog.Error("Failed to decode gaf json: " + gafJsonStr);
			return string.Empty;
		}
		Dictionary<string, object> dictionary = IconDataDictForGAF(gAF);
		if (dictionary.ContainsKey("error"))
		{
			BWLog.Info("Not adding atlas info: " + dictionary["error"]);
		}
		return JSONEncoder.Encode(dictionary);
	}

	public static string IconDataForGAFArray(string gafArrayJsonStr)
	{
		JObject jObject = JSONDecoder.Decode(gafArrayJsonStr);
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (JObject item in jObject.ArrayValue)
		{
			GAF gaf = GAF.FromJSON(item);
			Dictionary<string, object> dictionary = IconDataDictForGAF(gaf);
			if (dictionary.ContainsKey("error"))
			{
				BWLog.Info("Not adding atlas info: " + dictionary["error"]);
			}
			list.Add(dictionary);
		}
		return JSONEncoder.Encode(list);
	}

	private static Dictionary<string, object> IconDataDictForGAF(GAF gaf)
	{
		if (gaf == null)
		{
			string value = "gaf is null ";
			return new Dictionary<string, object> { { "error", value } };
		}
		TileIconInfo tileInfo = Instance.GetTileInfo(gaf);
		if (tileInfo == null)
		{
			string value2 = "no data for gaf";
			return new Dictionary<string, object> { { "error", value2 } };
		}
		Color[] colors = Blocksworld.GetColors(tileInfo.backgroundColorName);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("colors", new float[2][]
		{
			new float[4]
			{
				colors[0].r,
				colors[0].g,
				colors[0].b,
				colors[0].a
			},
			new float[4]
			{
				colors[1].r,
				colors[1].g,
				colors[1].b,
				colors[1].a
			}
		});
		dictionary.Add("iconFilePath", tileInfo.filePath);
		dictionary.Add("clearBackground", (!tileInfo.clearBackground) ? 0f : 1f);
		return dictionary;
	}
}
