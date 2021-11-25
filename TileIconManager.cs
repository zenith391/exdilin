using System;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;

// Token: 0x020002CE RID: 718
public class TileIconManager
{
	// Token: 0x060020D4 RID: 8404 RVA: 0x000F0330 File Offset: 0x000EE730
	public TileIconManager()
	{
		this.waitingToLoad = new List<TileIconHandle>();
		this.waitingForModelIconDownload = new List<TileIconHandle>();
		this.loading = new List<TileIconHandle>();
		this.preloadedIcons = new Dictionary<string, Texture2D>();
		this.InitMaterials();
		this.InitLabels();
		this.PreloadIcons();
	}

	// Token: 0x17000157 RID: 343
	// (get) Token: 0x060020D5 RID: 8405 RVA: 0x000F0388 File Offset: 0x000EE788
	public static TileIconManager Instance
	{
		get
		{
			return TileIconManager._instance;
		}
	}

	// Token: 0x060020D6 RID: 8406 RVA: 0x000F038F File Offset: 0x000EE78F
	public static void Init()
	{
		TileIconManager._instance = new TileIconManager();
	}

	// Token: 0x060020D7 RID: 8407 RVA: 0x000F039B File Offset: 0x000EE79B
	public static TileIconHandle CreateTileIconHandle()
	{
		return new TileIconResourceHandle();
	}

	// Token: 0x17000158 RID: 344
	// (get) Token: 0x060020D8 RID: 8408 RVA: 0x000F03A2 File Offset: 0x000EE7A2
	public static float iconScaleFactor
	{
		get
		{
			return 2f;
		}
	}

	// Token: 0x060020D9 RID: 8409 RVA: 0x000F03AC File Offset: 0x000EE7AC
	private void InitMaterials()
	{
		string str = (!Blocksworld.hd) ? "SD" : "HD";
		this.iconBackgroundMaterial = Resources.Load<Material>("TileBackgrounds/TileBackgroundEnabled" + str);
		this.iconBackgroundMaterialDisabled = Resources.Load<Material>("TileBackgrounds/TileBackgroundDisabled" + str);
		this.iconBackgroundWithLabelMaterial = Resources.Load<Material>("TileBackgrounds/TileBackgroundLabelEnabled" + str);
		this.iconBackgroundWithLabelMaterialDisabled = Resources.Load<Material>("TileBackgrounds/TileBackgroundLabelDisabled" + str);
	}

	// Token: 0x060020DA RID: 8410 RVA: 0x000F042C File Offset: 0x000EE82C
	private void InitLabels()
	{
		bool flag = Blocksworld.hd;
		flag |= (BWStandalone.Instance != null);
		string path = (!flag) ? "TileLabelFontBold SD" : "TileLabelFontBold HD";
		Font font = Resources.Load<Font>(path);
		this.labelAtlas = new TileLabelAtlas(font, flag);
	}

	// Token: 0x060020DB RID: 8411 RVA: 0x000F0478 File Offset: 0x000EE878
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
		foreach (string text in list)
		{
			TileIconInfo tileInfoForIcon = this.GetTileInfoForIcon(text);
			string filePath = tileInfoForIcon.filePath;
			Texture2D texture2D = Resources.Load<Texture2D>(filePath);
			if (texture2D == null)
			{
				BWLog.Error("Failed to preload icon from resources: " + filePath);
			}
			else
			{
				this.preloadedIcons[text] = texture2D;
			}
		}
	}

	// Token: 0x060020DC RID: 8412 RVA: 0x000F05B8 File Offset: 0x000EE9B8
	public bool IsPreloadedIcon(Texture2D iconTex)
	{
		return this.preloadedIcons.ContainsValue(iconTex);
	}

	// Token: 0x060020DD RID: 8413 RVA: 0x000F05C8 File Offset: 0x000EE9C8
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
			return this.GetTileInfo(blockItem);
		}
		return null;
	}

	// Token: 0x060020DE RID: 8414 RVA: 0x000F05FC File Offset: 0x000EE9FC
	public string GetButtonInputVariantKeyImagePath(GAF gaf)
	{
		if (!gaf.IsButtonInput())
		{
			return null;
		}
		string stringArg = Util.GetStringArg(gaf.Args, 0, string.Empty);
		UIInputControl.ControlVariant controlVariant;
		if (UIInputControl.controlVariantFromString.TryGetValue(stringArg, out controlVariant))
		{
			string text = null;
			switch (controlVariant)
			{
			case UIInputControl.ControlVariant.Action:
				text = "Misc/Key_Action";
				goto IL_154;
			case UIInputControl.ControlVariant.Attack:
				text = "Misc/Key_Attack";
				goto IL_154;
			case UIInputControl.ControlVariant.Explode:
				text = "Misc/Key_Explode";
				goto IL_154;
			case UIInputControl.ControlVariant.Help:
				text = "Misc/Key_Help";
				goto IL_154;
			case UIInputControl.ControlVariant.Jump:
				text = "Misc/Key_Jump";
				goto IL_154;
			case UIInputControl.ControlVariant.Laser:
				text = "Misc/Key_Laser";
				goto IL_154;
			case UIInputControl.ControlVariant.Missile:
				text = "Misc/Key_Missile";
				goto IL_154;
			case UIInputControl.ControlVariant.Mode:
				text = "Misc/Key_Mode";
				goto IL_154;
			case UIInputControl.ControlVariant.Speak:
				text = "Misc/Key_Speak";
				goto IL_154;
			case UIInputControl.ControlVariant.Speed:
				text = "Misc/Key_Speed";
				goto IL_154;
			}
			UIInputControl.ControlType controlType;
			if (UIInputControl.controlTypeFromString.TryGetValue(stringArg, out controlType))
			{
				switch (controlType)
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
			IL_154:
			if (!string.IsNullOrEmpty(text))
			{
				return TileIconManager.GetPathToIcon(text);
			}
		}
		return null;
	}

	// Token: 0x060020DF RID: 8415 RVA: 0x000F0770 File Offset: 0x000EEB70
	public TileIconInfo GetTileInfo(BlockItem blockItem)
	{
		TileIconInfo tileIconInfo = new TileIconInfo();
		string text = blockItem.IconName;
		if (string.IsNullOrEmpty(text))
		{
			text = "Yellow/Warning_Triangle";
		}
		string pathToIcon = TileIconManager.GetPathToIcon(text);
		tileIconInfo.filePath = pathToIcon;
		tileIconInfo.hasIcon = !string.IsNullOrEmpty(pathToIcon);
		tileIconInfo.label = blockItem.Label;
		tileIconInfo.backgroundColorName = blockItem.IconBackgroundColor;
		tileIconInfo.clearBackground = string.IsNullOrEmpty(tileIconInfo.backgroundColorName);
		return tileIconInfo;
	}

	// Token: 0x060020E0 RID: 8416 RVA: 0x000F07E4 File Offset: 0x000EEBE4
	public TileIconInfo GetTileInfoForIcon(string iconName)
	{
		TileIconInfo tileIconInfo = new TileIconInfo();
		if (string.IsNullOrEmpty(iconName))
		{
			iconName = "Yellow/Warning_Triangle";
		}
		string pathToIcon = TileIconManager.GetPathToIcon(iconName);
		tileIconInfo.filePath = pathToIcon;
		tileIconInfo.filePath = pathToIcon;
		tileIconInfo.hasIcon = !string.IsNullOrEmpty(pathToIcon);
		tileIconInfo.label = string.Empty;
		tileIconInfo.backgroundColorName = string.Empty;
		tileIconInfo.clearBackground = true;
		return tileIconInfo;
	}

	// Token: 0x060020E1 RID: 8417 RVA: 0x000F084C File Offset: 0x000EEC4C
	public string GetPathToIcon(GAF gaf)
	{
		TileIconInfo tileInfo = this.GetTileInfo(gaf);
		if (tileInfo != null)
		{
			return tileInfo.filePath;
		}
		return null;
	}

	// Token: 0x060020E2 RID: 8418 RVA: 0x000F0870 File Offset: 0x000EEC70
	public bool GAFHasIcon(GAF gaf)
	{
		TileIconInfo tileInfo = this.GetTileInfo(gaf);
		return tileInfo != null && !string.IsNullOrEmpty(tileInfo.filePath);
	}

	// Token: 0x060020E3 RID: 8419 RVA: 0x000F089C File Offset: 0x000EEC9C
	public string GetLabelStr(GAF gaf)
	{
		TileIconInfo tileInfo = this.GetTileInfo(gaf);
		if (tileInfo != null)
		{
			return tileInfo.label;
		}
		return null;
	}

	// Token: 0x060020E4 RID: 8420 RVA: 0x000F08BF File Offset: 0x000EECBF
	public Material GetBackgroundMaterial(GAF gaf)
	{
		if (gaf.HasBuildPanelLabel || gaf.HasDynamicLabel())
		{
			return this.iconBackgroundWithLabelMaterial;
		}
		return this.iconBackgroundMaterial;
	}

	// Token: 0x060020E5 RID: 8421 RVA: 0x000F08E4 File Offset: 0x000EECE4
	public Material GetBackgroundMaterialDisabled(GAF gaf)
	{
		if (gaf.HasBuildPanelLabel || gaf.HasDynamicLabel())
		{
			return this.iconBackgroundWithLabelMaterialDisabled;
		}
		return this.iconBackgroundMaterialDisabled;
	}

	// Token: 0x060020E6 RID: 8422 RVA: 0x000F090C File Offset: 0x000EED0C
	public Texture2D GetBackgroundTexture(GAF gaf)
	{
		Material backgroundMaterial = this.GetBackgroundMaterial(gaf);
		if (backgroundMaterial != null)
		{
			return backgroundMaterial.mainTexture as Texture2D;
		}
		return null;
	}

	// Token: 0x060020E7 RID: 8423 RVA: 0x000F093A File Offset: 0x000EED3A
	public bool RequestIconLoad(string path, TileIconHandle handle)
	{
		if (!string.IsNullOrEmpty(path))
		{
			handle.SetFilePath(path);
			handle.loadState = TileIconLoadState.WaitingToLoad;
			this.waitingToLoad.Add(handle);
			return true;
		}
		return false;
	}

	// Token: 0x060020E8 RID: 8424 RVA: 0x000F0964 File Offset: 0x000EED64
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
			this.waitingToLoad.Add(handle);
			return true;
		}
		if (!string.IsNullOrEmpty(modelId) && !string.IsNullOrEmpty(modelType))
		{
			WorldSession.platformDelegate.RequestMissingModelIcon(modelType, modelId);
			handle.loadState = TileIconLoadState.WaitingForModelIconDownload;
			this.waitingForModelIconDownload.Add(handle);
			return true;
		}
		return false;
	}

	// Token: 0x060020E9 RID: 8425 RVA: 0x000F09E3 File Offset: 0x000EEDE3
	public void SetNewLoadLimit(int limit)
	{
		this._newLoadLimit = limit;
	}

	// Token: 0x060020EA RID: 8426 RVA: 0x000F09EC File Offset: 0x000EEDEC
	public void ClearNewLoadLimit()
	{
		this._newLoadLimit = -1;
	}

	// Token: 0x060020EB RID: 8427 RVA: 0x000F09F8 File Offset: 0x000EEDF8
	public void Update()
	{
		int count = this.loading.Count;
		for (int i = count - 1; i >= 0; i--)
		{
			TileIconHandle tileIconHandle = this.loading[i];
			if (tileIconHandle.loadState == TileIconLoadState.Loading)
			{
				tileIconHandle.UpdateLoad();
			}
			else if (tileIconHandle.loadState != TileIconLoadState.Loaded)
			{
				this.loading.RemoveAt(i);
			}
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num < this.waitingToLoad.Count && num2 < 128 && num3 < 8)
		{
			num2++;
			TileIconHandle tileIconHandle2 = this.waitingToLoad[num];
			if (tileIconHandle2.loadState != TileIconLoadState.WaitingToLoad)
			{
				this.waitingToLoad.RemoveAt(num);
			}
			else if (this.loading.Count < 16 && this._newLoadLimit != 0)
			{
				tileIconHandle2.StartLoad();
				if (this._newLoadLimit > 0)
				{
					this._newLoadLimit--;
				}
				num3++;
				this.loading.Add(tileIconHandle2);
				this.waitingToLoad.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	// Token: 0x060020EC RID: 8428 RVA: 0x000F0B2C File Offset: 0x000EEF2C
	public void CheckModelIcons()
	{
		for (int i = 0; i < this.waitingForModelIconDownload.Count; i++)
		{
			TileIconHandle tileIconHandle = this.waitingForModelIconDownload[i];
			if (tileIconHandle.loadState == TileIconLoadState.WaitingForModelIconDownload)
			{
				if (tileIconHandle.CheckFileExists())
				{
					tileIconHandle.loadState = TileIconLoadState.WaitingToLoad;
					this.waitingToLoad.Add(tileIconHandle);
				}
			}
		}
		this.waitingForModelIconDownload.RemoveAll((TileIconHandle handle) => handle.loadState != TileIconLoadState.WaitingForModelIconDownload);
	}

	// Token: 0x060020ED RID: 8429 RVA: 0x000F0BBC File Offset: 0x000EEFBC
	public void CancelAllFileLoads()
	{
		this.waitingToLoad.Clear();
		this.waitingForModelIconDownload.Clear();
		for (int i = 0; i < this.loading.Count; i++)
		{
			this.loading[i].CancelLoad();
		}
		this.loading.Clear();
	}

	// Token: 0x17000159 RID: 345
	// (get) Token: 0x060020EE RID: 8430 RVA: 0x000F0C18 File Offset: 0x000EF018
	private static string StreamingBasePath
	{
		get
		{
			string empty = string.Empty;
			string path = "HD";
			return Path.Combine(empty, Path.Combine("Icons", path));
		}
	}

	// Token: 0x060020EF RID: 8431 RVA: 0x000F0C44 File Offset: 0x000EF044
	public static string GetPathToIcon(string iconName)
	{
		string streamingBasePath = TileIconManager.StreamingBasePath;
		string str = "HD";
		string empty = string.Empty;
		return Path.Combine(TileIconManager.StreamingBasePath, iconName + "_" + str + empty);
	}

	// Token: 0x060020F0 RID: 8432 RVA: 0x000F0C7C File Offset: 0x000EF07C
	public static string IconDataForGAFString(string gafJsonStr)
	{
		JObject obj = JSONDecoder.Decode(gafJsonStr);
		GAF gaf = GAF.FromJSON(obj, false, true);
		if (gaf == null)
		{
			BWLog.Error("Failed to decode gaf json: " + gafJsonStr);
			return string.Empty;
		}
		Dictionary<string, object> dictionary = TileIconManager.IconDataDictForGAF(gaf);
		if (dictionary.ContainsKey("error"))
		{
			BWLog.Info("Not adding atlas info: " + dictionary["error"]);
		}
		return JSONEncoder.Encode(dictionary);
	}

	// Token: 0x060020F1 RID: 8433 RVA: 0x000F0CEC File Offset: 0x000EF0EC
	public static string IconDataForGAFArray(string gafArrayJsonStr)
	{
		JObject jobject = JSONDecoder.Decode(gafArrayJsonStr);
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (JObject obj in jobject.ArrayValue)
		{
			GAF gaf = GAF.FromJSON(obj, false, true);
			Dictionary<string, object> dictionary = TileIconManager.IconDataDictForGAF(gaf);
			if (dictionary.ContainsKey("error"))
			{
				BWLog.Info("Not adding atlas info: " + dictionary["error"]);
			}
			list.Add(dictionary);
		}
		return JSONEncoder.Encode(list);
	}

	// Token: 0x060020F2 RID: 8434 RVA: 0x000F0D9C File Offset: 0x000EF19C
	private static Dictionary<string, object> IconDataDictForGAF(GAF gaf)
	{
		if (gaf == null)
		{
			string value = "gaf is null ";
			return new Dictionary<string, object>
			{
				{
					"error",
					value
				}
			};
		}
		TileIconInfo tileInfo = TileIconManager.Instance.GetTileInfo(gaf);
		if (tileInfo == null)
		{
			string value2 = "no data for gaf";
			return new Dictionary<string, object>
			{
				{
					"error",
					value2
				}
			};
		}
		Color[] colors = Blocksworld.GetColors(tileInfo.backgroundColorName);
		return new Dictionary<string, object>
		{
			{
				"colors",
				new float[][]
				{
					new float[]
					{
						colors[0].r,
						colors[0].g,
						colors[0].b,
						colors[0].a
					},
					new float[]
					{
						colors[1].r,
						colors[1].g,
						colors[1].b,
						colors[1].a
					}
				}
			},
			{
				"iconFilePath",
				tileInfo.filePath
			},
			{
				"clearBackground",
				(!tileInfo.clearBackground) ? 0f : 1f
			}
		};
	}

	// Token: 0x04001BD8 RID: 7128
	private static TileIconManager _instance;

	// Token: 0x04001BD9 RID: 7129
	private const int streamCount = 16;

	// Token: 0x04001BDA RID: 7130
	private int _newLoadLimit = -1;

	// Token: 0x04001BDB RID: 7131
	private List<TileIconHandle> waitingToLoad;

	// Token: 0x04001BDC RID: 7132
	private List<TileIconHandle> waitingForModelIconDownload;

	// Token: 0x04001BDD RID: 7133
	private List<TileIconHandle> loading;

	// Token: 0x04001BDE RID: 7134
	public Dictionary<string, Texture2D> preloadedIcons;

	// Token: 0x04001BDF RID: 7135
	private Material iconBackgroundMaterial;

	// Token: 0x04001BE0 RID: 7136
	private Material iconBackgroundMaterialDisabled;

	// Token: 0x04001BE1 RID: 7137
	private Material iconBackgroundWithLabelMaterial;

	// Token: 0x04001BE2 RID: 7138
	private Material iconBackgroundWithLabelMaterialDisabled;

	// Token: 0x04001BE3 RID: 7139
	public TileLabelAtlas labelAtlas;
}
