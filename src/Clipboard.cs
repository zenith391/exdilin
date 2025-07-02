using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Blocks;
using SimpleJSON;
using UnityEngine;

public class Clipboard
{
	public delegate void ClipboardEventHandler();

	private class TilesScarcityInfo
	{
		public int remaining = -1;

		public Dictionary<GAF, int> missing = new Dictionary<GAF, int>();

		public List<GAF> uniques = new List<GAF>();
	}

	public List<List<List<Tile>>> modelCopyPasteBuffer = new List<List<List<Tile>>>();

	public List<List<List<Tile>>> modelDuplicateBuffer = new List<List<List<Tile>>>();

	public List<List<List<Tile>>> scriptCopyPasteBuffer = new List<List<List<Tile>>>();

	public List<List<List<Tile>>> modelTrashedBuffer = new List<List<List<Tile>>>();

	public Texture2D copiedModelIcon;

	public bool copiedModelIconUpToDate;

	private GAF paintGAF;

	private GAF textureGAF;

	private const string defaultPaintName = "Yellow";

	private const string defaultTextureName = "Plain";

	private bool _autoPaintMode;

	private bool _autoTextureMode;

	private static Dictionary<ModelData, TilesScarcityInfo> cachedModelScarcityInfos = new Dictionary<ModelData, TilesScarcityInfo>();

	private static Dictionary<List<List<List<Tile>>>, TilesScarcityInfo> cachedScriptScarcityInfos = new Dictionary<List<List<List<Tile>>>, TilesScarcityInfo>();

	private TilesScarcityInfo modelCopyPasteScarcityInfo;

	private TilesScarcityInfo trashedModelScarcityInfo;

	public bool autoPaintMode
	{
		get
		{
			return _autoPaintMode;
		}
		set
		{
			_autoPaintMode = value;
			Blocksworld.UI.QuickSelect.SetAutoPaint(_autoPaintMode);
		}
	}

	public bool autoTextureMode
	{
		get
		{
			return _autoTextureMode;
		}
		set
		{
			_autoTextureMode = value;
			Blocksworld.UI.QuickSelect.SetAutoTexture(_autoTextureMode);
		}
	}

	internal string activePaint
	{
		get
		{
			if (paintGAF == null)
			{
				return "Yellow";
			}
			return (string)paintGAF.Args[0];
		}
	}

	internal string activeTexture
	{
		get
		{
			if (textureGAF == null)
			{
				return "Plain";
			}
			return (string)textureGAF.Args[0];
		}
	}

	public event ClipboardEventHandler OnSetPaintColor;

	public event ClipboardEventHandler OnSetTexture;

	public event ClipboardEventHandler OnSetModel;

	public event ClipboardEventHandler OnSetScript;

	public Clipboard()
	{
		Scarcity.OnInventoryChange += InventoryDirty;
		Blocksworld.UI.QuickSelect.ConnectToClipboard(this);
	}

	private void InventoryDirty()
	{
		cachedModelScarcityInfos.Clear();
		modelCopyPasteScarcityInfo = null;
		SetDefaultBuffersIfNecessary();
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		if (quickSelect != null)
		{
			quickSelect.UpdateModelIcon();
			quickSelect.UpdateScriptIcon();
		}
	}

	public void SetPaintColor(string paint, bool save = true)
	{
		paintGAF = new GAF("Block.PaintTo", paint);
		if (this.OnSetPaintColor != null)
		{
			this.OnSetPaintColor();
		}
		if (save)
		{
			Save();
		}
	}

	public void SetTexture(string texture, bool save = true)
	{
		textureGAF = new GAF("Block.TextureTo", texture, Vector3.zero);
		if (this.OnSetTexture != null)
		{
			this.OnSetTexture();
		}
		if (save)
		{
			Save();
		}
	}

	public void SetModelToSelection()
	{
		Blocksworld.bw.CopySelectionToBuffer(modelCopyPasteBuffer);
		modelCopyPasteScarcityInfo = TilesScarcityInfoForBlockList(modelCopyPasteBuffer);
		if (this.OnSetModel != null)
		{
			this.OnSetModel();
		}
		copiedModelIconUpToDate = false;
		Save();
	}

	public void SetTrashedModelToSelection()
	{
		Blocksworld.bw.CopySelectionToBuffer(modelTrashedBuffer);
		trashedModelScarcityInfo = TilesScarcityInfoForBlockList(modelTrashedBuffer);
	}

	public void GenerateModelIcon(Action<Texture2D> completion)
	{
		ScreenshotUtils.GenerateModelIconTexture(modelCopyPasteBuffer, 100 * (int)NormalizedScreen.scale, withOutline: true, perspectiveCamera: false, delegate(Texture2D tex)
		{
			copiedModelIconUpToDate = true;
			copiedModelIcon = tex;
			if (completion != null)
			{
				completion(tex);
			}
		});
	}

	private TilesScarcityInfo GetTrashedModelToSelection()
	{
		return trashedModelScarcityInfo;
	}

	public void SetScriptToSelection()
	{
		cachedScriptScarcityInfos.Remove(scriptCopyPasteBuffer);
		Blocksworld.bw.CopySelectedScriptToBuffer(scriptCopyPasteBuffer);
		if (this.OnSetScript != null)
		{
			this.OnSetScript();
		}
		Save();
	}

	public GAF GetLastPaintedColorGAF()
	{
		if (paintGAF == null)
		{
			SetPaintColor("Yellow");
		}
		else if (!TileIconManager.Instance.GAFHasIcon(paintGAF))
		{
			BWLog.Error("Paint GAF " + paintGAF?.ToString() + " is saved on clipboard but can't be found in tile atlas");
			SetPaintColor("Yellow");
		}
		return paintGAF;
	}

	public GAF GetLastTextureGAF()
	{
		if (textureGAF == null)
		{
			SetTexture("Plain");
		}
		else if (!TileIconManager.Instance.GAFHasIcon(textureGAF))
		{
			BWLog.Error("Texture GAF " + textureGAF?.ToString() + " is saved on clipboard but can't be found in tile atlas");
			SetTexture("Plain");
		}
		return textureGAF;
	}

	public bool HasEnoughScarcityForModel(Dictionary<GAF, int> missing = null, List<GAF> uniquesInModel = null)
	{
		if (!BW.Options.useScarcity())
		{
			return true;
		}
		if (modelCopyPasteScarcityInfo == null)
		{
			modelCopyPasteScarcityInfo = TilesScarcityInfoForBlockList(modelCopyPasteBuffer);
		}
		GetMissingAndUniqueBlocksFromScarcityInfo(modelCopyPasteScarcityInfo, missing, uniquesInModel);
		int remaining = modelCopyPasteScarcityInfo.remaining;
		if (remaining != -1)
		{
			return remaining > 0;
		}
		return true;
	}

	public bool HasEnoughScarcityForScript(Dictionary<GAF, int> missing = null, List<GAF> uniquesInScript = null)
	{
		if (!BW.Options.useScarcity())
		{
			return true;
		}
		int num = AvailableScriptCount(scriptCopyPasteBuffer, missing, uniquesInScript);
		if (num != -1)
		{
			return num > 0;
		}
		return true;
	}

	public int AvailableScriptCount(List<List<List<Tile>>> scripts, Dictionary<GAF, int> missing = null, List<GAF> uniquesInScript = null)
	{
		if (!BW.Options.useScarcity())
		{
			return -1;
		}
		if (Scarcity.inventory == null)
		{
			return -1;
		}
		if (cachedScriptScarcityInfos.TryGetValue(scripts, out var value))
		{
			if (missing != null && value.missing != null)
			{
				foreach (KeyValuePair<GAF, int> item in value.missing)
				{
					missing[item.Key] = item.Value;
				}
			}
			if (uniquesInScript != null && value.uniques != null)
			{
				uniquesInScript.AddRange(value.uniques);
			}
			return value.remaining;
		}
		value = new TilesScarcityInfo();
		cachedScriptScarcityInfos[scripts] = value;
		int num = -1;
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		foreach (List<List<Tile>> script in scripts)
		{
			foreach (List<Tile> item2 in script)
			{
				foreach (Tile item3 in item2)
				{
					GAF gaf = item3.gaf;
					if (gaf.Predicate == Block.predicatePlaySoundDurational)
					{
						if (dictionary.ContainsKey(gaf))
						{
							dictionary[gaf]++;
						}
						else
						{
							dictionary.Add(gaf, 1);
						}
					}
				}
			}
		}
		foreach (GAF key in dictionary.Keys)
		{
			int inventoryCount = Scarcity.GetInventoryCount(key, zeroIfNotExist: true);
			int num2 = dictionary[key];
			if (inventoryCount < 0)
			{
				continue;
			}
			int num3 = inventoryCount / num2;
			if (inventoryCount < num2)
			{
				int value2 = num2 - inventoryCount;
				if (missing != null)
				{
					missing[key] = value2;
				}
				value.missing[key] = value2;
			}
			num = ((num != -1) ? Mathf.Min(num, num3) : num3);
		}
		value.remaining = num;
		return num;
	}

	public static void CalculateGafRelevance(List<List<Tile>> info, Dictionary<GAF, int> selectionGAFs)
	{
		Tile tile = info[0].Find((Tile t) => t.gaf.Predicate == Block.predicateCreate);
		string blockType = "Cube";
		if (tile != null)
		{
			blockType = (string)tile.gaf.Args[0];
		}
		for (int num = 0; num < info.Count; num++)
		{
			for (int num2 = 0; num2 < info[num].Count; num2++)
			{
				Tile tile2 = info[num][num2];
				if (Scarcity.IsRelevantGAF(blockType, tile2.gaf, num == 0))
				{
					GAF normalizedGaf = Scarcity.GetNormalizedGaf(tile2.gaf);
					if (selectionGAFs.ContainsKey(normalizedGaf))
					{
						selectionGAFs[normalizedGaf]++;
					}
					else
					{
						selectionGAFs.Add(normalizedGaf, 1);
					}
				}
			}
		}
	}

	public int AvailableCopyPasteBufferCount()
	{
		if (!BW.Options.useScarcity())
		{
			return -1;
		}
		if (Scarcity.inventory == null)
		{
			return -1;
		}
		if (modelCopyPasteBuffer == null)
		{
			return 0;
		}
		if (modelCopyPasteScarcityInfo == null)
		{
			modelCopyPasteScarcityInfo = TilesScarcityInfoForBlockList(modelCopyPasteBuffer);
		}
		return modelCopyPasteScarcityInfo.remaining;
	}

	public int AvailablityCountForBlockList(List<List<List<Tile>>> blockList, Dictionary<GAF, int> missing = null, List<GAF> uniquesInModel = null)
	{
		TilesScarcityInfo tilesScarcityInfo = TilesScarcityInfoForBlockList(blockList);
		GetMissingAndUniqueBlocksFromScarcityInfo(tilesScarcityInfo, missing, uniquesInModel);
		return tilesScarcityInfo.remaining;
	}

	private void GetMissingAndUniqueBlocksFromScarcityInfo(TilesScarcityInfo info, Dictionary<GAF, int> missing, List<GAF> uniquesInModel)
	{
		if (missing != null && info.missing != null)
		{
			foreach (KeyValuePair<GAF, int> item in info.missing)
			{
				missing[item.Key] = item.Value;
			}
		}
		if (uniquesInModel != null && info.uniques != null)
		{
			uniquesInModel.AddRange(info.uniques);
		}
	}

	private TilesScarcityInfo TilesScarcityInfoForBlockList(List<List<List<Tile>>> blockList, Dictionary<GAF, int> missing = null, List<GAF> uniquesInModel = null)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		BlockGroups.GetHomogenousGroupBlockCounts(blockList, dictionary);
		TilesScarcityInfo tilesScarcityInfo = new TilesScarcityInfo();
		Dictionary<GAF, int> dictionary2 = new Dictionary<GAF, int>();
		int num = -1;
		Dictionary<string, HashSet<string>> uniqueBlockMap = Blocksworld.GetUniqueBlockMap();
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < blockList.Count; i++)
		{
			List<Tile> list = blockList[i][0];
			Tile tile = list.Find((Tile t) => t.gaf.Predicate == Block.predicateCreate);
			if (tile != null)
			{
				string text = (string)tile.gaf.Args[0];
				if (uniqueBlockMap.ContainsKey(text))
				{
					hashSet.Add(text);
				}
			}
			CalculateGafRelevance(blockList[i], dictionary2);
		}
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			GAF key = new GAF(Block.predicateCreate, item2.Key);
			int value = (dictionary2[key] = (dictionary2.TryGetValue(key, out value) ? (value + item2.Value) : item2.Value));
		}
		foreach (GAF key2 in dictionary2.Keys)
		{
			if (BlockItem.FindByGafPredicateNameAndArguments(key2.Predicate.Name, key2.Args) == null)
			{
				continue;
			}
			int num3 = dictionary2[key2];
			GAF normalizedGaf = Scarcity.GetNormalizedGaf(key2);
			int inventoryCount = Scarcity.GetInventoryCount(normalizedGaf, zeroIfNotExist: true);
			if (inventoryCount < 0)
			{
				continue;
			}
			int num4 = inventoryCount / num3;
			if (inventoryCount < num3)
			{
				int value2 = num3 - inventoryCount;
				if (missing != null)
				{
					missing[normalizedGaf] = value2;
				}
				tilesScarcityInfo.missing[normalizedGaf] = value2;
			}
			num = ((num != -1) ? Mathf.Min(num, num4) : num4);
		}
		if (hashSet.Count > 0)
		{
			foreach (Block item3 in BWSceneManager.AllBlocks())
			{
				string text2 = item3.BlockType();
				if (hashSet.Contains(text2))
				{
					num = 0;
					GAF item = new GAF(Block.predicateCreate, text2);
					uniquesInModel?.Add(item);
					tilesScarcityInfo.uniques.Add(item);
				}
			}
			if (num != 0)
			{
				num = 1;
			}
		}
		tilesScarcityInfo.remaining = num;
		return tilesScarcityInfo;
	}

	private TilesScarcityInfo TilesScarcityInfoForModel(ModelData model, Dictionary<GAF, int> missing = null, List<GAF> uniquesInModel = null)
	{
		TilesScarcityInfo tilesScarcityInfo = new TilesScarcityInfo();
		int num = -1;
		foreach (GAF key in model.gafUsage.Keys)
		{
			int num2 = model.gafUsage[key];
			GAF normalizedGaf = Scarcity.GetNormalizedGaf(key);
			if (!Scarcity.GetExistsInInventory(normalizedGaf))
			{
				num = 0;
				int num3 = 1;
				if (tilesScarcityInfo.missing.ContainsKey(normalizedGaf))
				{
					num3 += tilesScarcityInfo.missing[normalizedGaf];
				}
				tilesScarcityInfo.missing[normalizedGaf] = num3;
				if (missing != null)
				{
					missing[normalizedGaf] = num3;
				}
				continue;
			}
			int inventoryCount = Scarcity.GetInventoryCount(normalizedGaf, zeroIfNotExist: true);
			if (inventoryCount < 0)
			{
				continue;
			}
			int num4 = inventoryCount / num2;
			if (inventoryCount < num2)
			{
				int value = num2 - inventoryCount;
				if (missing != null)
				{
					missing[normalizedGaf] = value;
				}
				tilesScarcityInfo.missing[normalizedGaf] = value;
			}
			num = ((num != -1) ? Mathf.Min(num, num4) : num4);
		}
		if (model.uniqueBlockNames.Count > 0)
		{
			foreach (Block item2 in BWSceneManager.AllBlocks())
			{
				string text = item2.BlockType();
				if (model.uniqueBlockNames.Contains(text))
				{
					num = 0;
					GAF item = new GAF(Block.predicateCreate, text);
					uniquesInModel?.Add(item);
					tilesScarcityInfo.uniques.Add(item);
				}
			}
			if (num != 0)
			{
				num = 1;
			}
		}
		tilesScarcityInfo.remaining = num;
		return tilesScarcityInfo;
	}

	public int AvailableModelCount(ModelData model, Dictionary<GAF, int> missing = null, List<GAF> uniquesInModel = null, bool useCache = true)
	{
		if (Scarcity.inventory == null)
		{
			return -1;
		}
		if (useCache && cachedModelScarcityInfos.TryGetValue(model, out var value))
		{
			GetMissingAndUniqueBlocksFromScarcityInfo(value, missing, uniquesInModel);
			return value.remaining;
		}
		value = TilesScarcityInfoForModel(model, missing, uniquesInModel);
		cachedModelScarcityInfos[model] = value;
		return value.remaining;
	}

	public void Save()
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jSONStreamEncoder = new JSONStreamEncoder(writer);
		jSONStreamEncoder.BeginObject();
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.WriteKey("paint");
		jSONStreamEncoder.WriteString(activePaint);
		jSONStreamEncoder.WriteKey("auto-paint");
		jSONStreamEncoder.WriteBool(autoPaintMode);
		jSONStreamEncoder.WriteKey("texture");
		jSONStreamEncoder.WriteString(activeTexture);
		jSONStreamEncoder.WriteKey("auto-texture");
		jSONStreamEncoder.WriteBool(autoTextureMode);
		jSONStreamEncoder.WriteKey("model");
		ModelUtils.WriteJSONForModel(jSONStreamEncoder, modelCopyPasteBuffer);
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.WriteKey("script");
		ModelUtils.WriteJSONForModel(jSONStreamEncoder, scriptCopyPasteBuffer);
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.EndObject();
		WorldSession.SaveClipboard(stringBuilder.ToString());
	}

	public void Load()
	{
		SetDefaultBuffersIfNecessary();
		WorldSession.LoadClipboard();
	}

	public static void LoadCallback(string clipboardString)
	{
		if (!string.IsNullOrEmpty(clipboardString))
		{
			Blocksworld.clipboard.LoadFromString(clipboardString);
		}
	}

	public void Reset()
	{
		if (BW.isUnityEditor)
		{
			paintGAF = null;
			textureGAF = null;
			PlayerPrefs.DeleteKey("tempClipboard");
			scriptCopyPasteBuffer.Clear();
			modelCopyPasteBuffer.Clear();
			SetDefaultBuffersIfNecessary();
			Save();
			Scarcity.UpdateInventory();
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			if (quickSelect != null)
			{
				quickSelect.UpdateIcons();
			}
		}
	}

	private void SetDefaultBuffersIfNecessary()
	{
		if (scriptCopyPasteBuffer.Count == 0 || scriptCopyPasteBuffer[0].Count == 0)
		{
			cachedScriptScarcityInfos.Remove(scriptCopyPasteBuffer);
			scriptCopyPasteBuffer = new List<List<List<Tile>>>
			{
				new List<List<Tile>>
				{
					new List<Tile>
					{
						new Tile(new GAF(Block.predicateTapModel)),
						new Tile(new GAF(Block.predicateThen)),
						new Tile(new GAF("Block.Speak", "Hello!"))
					},
					new List<Tile>
					{
						new Tile(new GAF(Block.predicateThen))
					}
				}
			};
		}
		if (modelCopyPasteBuffer != null && (modelCopyPasteBuffer.Count == 0 || modelCopyPasteBuffer[0].Count == 0) && Blocksworld.buildPanel.IsGAFInBuildPanel(new GAF(Block.predicateCreate, "Cube")))
		{
			modelCopyPasteScarcityInfo = null;
			modelCopyPasteBuffer = new List<List<List<Tile>>>
			{
				new List<List<Tile>>
				{
					new List<Tile>
					{
						new Tile(new GAF(Block.predicateStop)),
						new Tile(new GAF(Block.predicateCreate, "Cube")),
						new Tile(new GAF(Block.predicateMoveTo, Vector3.zero)),
						new Tile(new GAF(Block.predicateRotateTo, Vector3.zero)),
						new Tile(new GAF(Block.predicatePaintTo, "Yellow")),
						new Tile(new GAF(Block.predicateTextureTo, "Plain", Vector3.up)),
						new Tile(new GAF(Block.predicateScaleTo, Vector3.one))
					},
					new List<Tile>
					{
						new Tile(new GAF(Block.predicateThen))
					}
				}
			};
		}
	}

	private bool LoadFromString(string s)
	{
		bool flag = false;
		try
		{
			JObject jobj = JSONDecoder.Decode(s);
			LoadFromJSON(jobj);
			return true;
		}
		catch (Exception ex)
		{
			BWLog.Warning("Unable to decode JSON for " + s);
			BWLog.Warning(ex.GetType().Name + " exception message: " + ex.Message + " stacktrace: " + ex.StackTrace);
			return false;
		}
	}

	private void LoadFromJSON(JObject jobj)
	{
		JObject jObject = jobj["paint"];
		if (jObject != null)
		{
			SetPaintColor((string)jObject, save: false);
		}
		JObject jObject2 = jobj["auto-paint"];
		if (jObject2 != null)
		{
			autoPaintMode = (bool)jObject2;
		}
		JObject jObject3 = jobj["texture"];
		if (jObject3 != null)
		{
			SetTexture((string)jObject3, save: false);
		}
		JObject jObject4 = jobj["auto-texture"];
		if (jObject4 != null)
		{
			autoTextureMode = (bool)jObject4;
		}
		modelCopyPasteBuffer = ModelUtils.ParseModelJSON(jobj["model"]);
		scriptCopyPasteBuffer = ModelUtils.ParseModelJSON(jobj["script"]);
		SetDefaultBuffersIfNecessary();
	}
}
