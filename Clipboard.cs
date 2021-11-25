using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x02000113 RID: 275
public class Clipboard
{
	// Token: 0x06001362 RID: 4962 RVA: 0x000855AC File Offset: 0x000839AC
	public Clipboard()
	{
		Scarcity.OnInventoryChange += this.InventoryDirty;
		Blocksworld.UI.QuickSelect.ConnectToClipboard(this);
	}

	// Token: 0x14000007 RID: 7
	// (add) Token: 0x06001363 RID: 4963 RVA: 0x0008560C File Offset: 0x00083A0C
	// (remove) Token: 0x06001364 RID: 4964 RVA: 0x00085644 File Offset: 0x00083A44
	public event Clipboard.ClipboardEventHandler OnSetPaintColor;

	// Token: 0x14000008 RID: 8
	// (add) Token: 0x06001365 RID: 4965 RVA: 0x0008567C File Offset: 0x00083A7C
	// (remove) Token: 0x06001366 RID: 4966 RVA: 0x000856B4 File Offset: 0x00083AB4
	public event Clipboard.ClipboardEventHandler OnSetTexture;

	// Token: 0x14000009 RID: 9
	// (add) Token: 0x06001367 RID: 4967 RVA: 0x000856EC File Offset: 0x00083AEC
	// (remove) Token: 0x06001368 RID: 4968 RVA: 0x00085724 File Offset: 0x00083B24
	public event Clipboard.ClipboardEventHandler OnSetModel;

	// Token: 0x1400000A RID: 10
	// (add) Token: 0x06001369 RID: 4969 RVA: 0x0008575C File Offset: 0x00083B5C
	// (remove) Token: 0x0600136A RID: 4970 RVA: 0x00085794 File Offset: 0x00083B94
	public event Clipboard.ClipboardEventHandler OnSetScript;

	// Token: 0x17000052 RID: 82
	// (get) Token: 0x0600136B RID: 4971 RVA: 0x000857CA File Offset: 0x00083BCA
	// (set) Token: 0x0600136C RID: 4972 RVA: 0x000857D2 File Offset: 0x00083BD2
	public bool autoPaintMode
	{
		get
		{
			return this._autoPaintMode;
		}
		set
		{
			this._autoPaintMode = value;
			Blocksworld.UI.QuickSelect.SetAutoPaint(this._autoPaintMode);
		}
	}

	// Token: 0x17000053 RID: 83
	// (get) Token: 0x0600136D RID: 4973 RVA: 0x000857F0 File Offset: 0x00083BF0
	// (set) Token: 0x0600136E RID: 4974 RVA: 0x000857F8 File Offset: 0x00083BF8
	public bool autoTextureMode
	{
		get
		{
			return this._autoTextureMode;
		}
		set
		{
			this._autoTextureMode = value;
			Blocksworld.UI.QuickSelect.SetAutoTexture(this._autoTextureMode);
		}
	}

	// Token: 0x17000054 RID: 84
	// (get) Token: 0x0600136F RID: 4975 RVA: 0x00085816 File Offset: 0x00083C16
	internal string activePaint
	{
		get
		{
			return (this.paintGAF != null) ? ((string)this.paintGAF.Args[0]) : "Yellow";
		}
	}

	// Token: 0x17000055 RID: 85
	// (get) Token: 0x06001370 RID: 4976 RVA: 0x0008583F File Offset: 0x00083C3F
	internal string activeTexture
	{
		get
		{
			return (this.textureGAF != null) ? ((string)this.textureGAF.Args[0]) : "Plain";
		}
	}

	// Token: 0x06001371 RID: 4977 RVA: 0x00085868 File Offset: 0x00083C68
	private void InventoryDirty()
	{
		Clipboard.cachedModelScarcityInfos.Clear();
		this.modelCopyPasteScarcityInfo = null;
		this.SetDefaultBuffersIfNecessary();
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		if (quickSelect != null)
		{
			quickSelect.UpdateModelIcon();
			quickSelect.UpdateScriptIcon();
		}
	}

	// Token: 0x06001372 RID: 4978 RVA: 0x000858AF File Offset: 0x00083CAF
	public void SetPaintColor(string paint, bool save = true)
	{
		this.paintGAF = new GAF("Block.PaintTo", new object[]
		{
			paint
		});
		if (this.OnSetPaintColor != null)
		{
			this.OnSetPaintColor();
		}
		if (save)
		{
			this.Save();
		}
	}

	// Token: 0x06001373 RID: 4979 RVA: 0x000858F0 File Offset: 0x00083CF0
	public void SetTexture(string texture, bool save = true)
	{
		this.textureGAF = new GAF("Block.TextureTo", new object[]
		{
			texture,
			Vector3.zero
		});
		if (this.OnSetTexture != null)
		{
			this.OnSetTexture();
		}
		if (save)
		{
			this.Save();
		}
	}

	// Token: 0x06001374 RID: 4980 RVA: 0x00085948 File Offset: 0x00083D48
	public void SetModelToSelection()
	{
		Blocksworld.bw.CopySelectionToBuffer(this.modelCopyPasteBuffer);
		this.modelCopyPasteScarcityInfo = this.TilesScarcityInfoForBlockList(this.modelCopyPasteBuffer, null, null);
		if (this.OnSetModel != null)
		{
			this.OnSetModel();
		}
		this.copiedModelIconUpToDate = false;
		this.Save();
	}

	// Token: 0x06001375 RID: 4981 RVA: 0x0008599C File Offset: 0x00083D9C
	public void SetTrashedModelToSelection()
	{
		Blocksworld.bw.CopySelectionToBuffer(this.modelTrashedBuffer);
		this.trashedModelScarcityInfo = this.TilesScarcityInfoForBlockList(this.modelTrashedBuffer, null, null);
	}

	// Token: 0x06001376 RID: 4982 RVA: 0x000859C4 File Offset: 0x00083DC4
	public void GenerateModelIcon(Action<Texture2D> completion)
	{
		ScreenshotUtils.GenerateModelIconTexture(this.modelCopyPasteBuffer, 100 * (int)NormalizedScreen.scale, true, false, delegate(Texture2D tex)
		{
			this.copiedModelIconUpToDate = true;
			this.copiedModelIcon = tex;
			if (completion != null)
			{
				completion(tex);
			}
		});
	}

	// Token: 0x06001377 RID: 4983 RVA: 0x00085A07 File Offset: 0x00083E07
	private Clipboard.TilesScarcityInfo GetTrashedModelToSelection()
	{
		return this.trashedModelScarcityInfo;
	}

	// Token: 0x06001378 RID: 4984 RVA: 0x00085A0F File Offset: 0x00083E0F
	public void SetScriptToSelection()
	{
		Clipboard.cachedScriptScarcityInfos.Remove(this.scriptCopyPasteBuffer);
		Blocksworld.bw.CopySelectedScriptToBuffer(this.scriptCopyPasteBuffer);
		if (this.OnSetScript != null)
		{
			this.OnSetScript();
		}
		this.Save();
	}

	// Token: 0x06001379 RID: 4985 RVA: 0x00085A50 File Offset: 0x00083E50
	public GAF GetLastPaintedColorGAF()
	{
		if (this.paintGAF == null)
		{
			this.SetPaintColor("Yellow", true);
		}
		else if (!TileIconManager.Instance.GAFHasIcon(this.paintGAF))
		{
			BWLog.Error("Paint GAF " + this.paintGAF + " is saved on clipboard but can't be found in tile atlas");
			this.SetPaintColor("Yellow", true);
		}
		return this.paintGAF;
	}

	// Token: 0x0600137A RID: 4986 RVA: 0x00085ABC File Offset: 0x00083EBC
	public GAF GetLastTextureGAF()
	{
		if (this.textureGAF == null)
		{
			this.SetTexture("Plain", true);
		}
		else if (!TileIconManager.Instance.GAFHasIcon(this.textureGAF))
		{
			BWLog.Error("Texture GAF " + this.textureGAF + " is saved on clipboard but can't be found in tile atlas");
			this.SetTexture("Plain", true);
		}
		return this.textureGAF;
	}

	// Token: 0x0600137B RID: 4987 RVA: 0x00085B28 File Offset: 0x00083F28
	public bool HasEnoughScarcityForModel(Dictionary<GAF, int> missing = null, List<GAF> uniquesInModel = null)
	{
		if (!BW.Options.useScarcity())
		{
			return true;
		}
		if (this.modelCopyPasteScarcityInfo == null)
		{
			this.modelCopyPasteScarcityInfo = this.TilesScarcityInfoForBlockList(this.modelCopyPasteBuffer, null, null);
		}
		this.GetMissingAndUniqueBlocksFromScarcityInfo(this.modelCopyPasteScarcityInfo, missing, uniquesInModel);
		int remaining = this.modelCopyPasteScarcityInfo.remaining;
		return remaining == -1 || remaining > 0;
	}

	// Token: 0x0600137C RID: 4988 RVA: 0x00085B90 File Offset: 0x00083F90
	public bool HasEnoughScarcityForScript(Dictionary<GAF, int> missing = null, List<GAF> uniquesInScript = null)
	{
		if (!BW.Options.useScarcity())
		{
			return true;
		}
		int num = this.AvailableScriptCount(this.scriptCopyPasteBuffer, missing, uniquesInScript);
		return num == -1 || num > 0;
	}

	// Token: 0x0600137D RID: 4989 RVA: 0x00085BCC File Offset: 0x00083FCC
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
		Clipboard.TilesScarcityInfo tilesScarcityInfo;
		if (Clipboard.cachedScriptScarcityInfos.TryGetValue(scripts, out tilesScarcityInfo))
		{
			if (missing != null && tilesScarcityInfo.missing != null)
			{
				foreach (KeyValuePair<GAF, int> keyValuePair in tilesScarcityInfo.missing)
				{
					missing[keyValuePair.Key] = keyValuePair.Value;
				}
			}
			if (uniquesInScript != null && tilesScarcityInfo.uniques != null)
			{
				uniquesInScript.AddRange(tilesScarcityInfo.uniques);
			}
			return tilesScarcityInfo.remaining;
		}
		tilesScarcityInfo = new Clipboard.TilesScarcityInfo();
		Clipboard.cachedScriptScarcityInfos[scripts] = tilesScarcityInfo;
		int num = -1;
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		foreach (List<List<Tile>> list in scripts)
		{
			foreach (List<Tile> list2 in list)
			{
				foreach (Tile tile in list2)
				{
					GAF gaf = tile.gaf;
					if (gaf.Predicate == Block.predicatePlaySoundDurational)
					{
						if (dictionary.ContainsKey(gaf))
						{
							Dictionary<GAF, int> dictionary2;
							GAF key;
							(dictionary2 = dictionary)[key = gaf] = dictionary2[key] + 1;
						}
						else
						{
							dictionary.Add(gaf, 1);
						}
					}
				}
			}
		}
		foreach (GAF gaf2 in dictionary.Keys)
		{
			int inventoryCount = Scarcity.GetInventoryCount(gaf2, true);
			int num2 = dictionary[gaf2];
			if (inventoryCount >= 0)
			{
				int num3 = inventoryCount / num2;
				if (inventoryCount < num2)
				{
					int value = num2 - inventoryCount;
					if (missing != null)
					{
						missing[gaf2] = value;
					}
					tilesScarcityInfo.missing[gaf2] = value;
				}
				if (num == -1)
				{
					num = num3;
				}
				else
				{
					num = Mathf.Min(num, num3);
				}
			}
		}
		tilesScarcityInfo.remaining = num;
		return num;
	}

	// Token: 0x0600137E RID: 4990 RVA: 0x00085E84 File Offset: 0x00084284
	public static void CalculateGafRelevance(List<List<Tile>> info, Dictionary<GAF, int> selectionGAFs)
	{
		Tile tile = info[0].Find((Tile t) => t.gaf.Predicate == Block.predicateCreate);
		string blockType = "Cube";
		if (tile != null)
		{
			blockType = (string)tile.gaf.Args[0];
		}
		for (int i = 0; i < info.Count; i++)
		{
			for (int j = 0; j < info[i].Count; j++)
			{
				Tile tile2 = info[i][j];
				bool flag = Scarcity.IsRelevantGAF(blockType, tile2.gaf, i == 0);
				if (flag)
				{
					GAF normalizedGaf = Scarcity.GetNormalizedGaf(tile2.gaf, false);
					if (selectionGAFs.ContainsKey(normalizedGaf))
					{
						GAF key;
						selectionGAFs[key = normalizedGaf] = selectionGAFs[key] + 1;
					}
					else
					{
						selectionGAFs.Add(normalizedGaf, 1);
					}
				}
			}
		}
	}

	// Token: 0x0600137F RID: 4991 RVA: 0x00085F7C File Offset: 0x0008437C
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
		if (this.modelCopyPasteBuffer == null)
		{
			return 0;
		}
		if (this.modelCopyPasteScarcityInfo == null)
		{
			this.modelCopyPasteScarcityInfo = this.TilesScarcityInfoForBlockList(this.modelCopyPasteBuffer, null, null);
		}
		return this.modelCopyPasteScarcityInfo.remaining;
	}

	// Token: 0x06001380 RID: 4992 RVA: 0x00085FE0 File Offset: 0x000843E0
	public int AvailablityCountForBlockList(List<List<List<Tile>>> blockList, Dictionary<GAF, int> missing = null, List<GAF> uniquesInModel = null)
	{
		Clipboard.TilesScarcityInfo tilesScarcityInfo = this.TilesScarcityInfoForBlockList(blockList, null, null);
		this.GetMissingAndUniqueBlocksFromScarcityInfo(tilesScarcityInfo, missing, uniquesInModel);
		return tilesScarcityInfo.remaining;
	}

	// Token: 0x06001381 RID: 4993 RVA: 0x00086008 File Offset: 0x00084408
	private void GetMissingAndUniqueBlocksFromScarcityInfo(Clipboard.TilesScarcityInfo info, Dictionary<GAF, int> missing, List<GAF> uniquesInModel)
	{
		if (missing != null && info.missing != null)
		{
			foreach (KeyValuePair<GAF, int> keyValuePair in info.missing)
			{
				missing[keyValuePair.Key] = keyValuePair.Value;
			}
		}
		if (uniquesInModel != null && info.uniques != null)
		{
			uniquesInModel.AddRange(info.uniques);
		}
	}

	// Token: 0x06001382 RID: 4994 RVA: 0x000860A0 File Offset: 0x000844A0
	private Clipboard.TilesScarcityInfo TilesScarcityInfoForBlockList(List<List<List<Tile>>> blockList, Dictionary<GAF, int> missing = null, List<GAF> uniquesInModel = null)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		BlockGroups.GetHomogenousGroupBlockCounts(blockList, dictionary);
		Clipboard.TilesScarcityInfo tilesScarcityInfo = new Clipboard.TilesScarcityInfo();
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
			Clipboard.CalculateGafRelevance(blockList[i], dictionary2);
		}
		foreach (KeyValuePair<string, int> keyValuePair in dictionary)
		{
			GAF key = new GAF(Block.predicateCreate, new object[]
			{
				keyValuePair.Key
			});
			int num2;
			if (!dictionary2.TryGetValue(key, out num2))
			{
				num2 = keyValuePair.Value;
			}
			else
			{
				num2 += keyValuePair.Value;
			}
			dictionary2[key] = num2;
		}
		foreach (GAF gaf in dictionary2.Keys)
		{
			if (BlockItem.FindByGafPredicateNameAndArguments(gaf.Predicate.Name, gaf.Args) != null)
			{
				int num3 = dictionary2[gaf];
				GAF normalizedGaf = Scarcity.GetNormalizedGaf(gaf, false);
				int inventoryCount = Scarcity.GetInventoryCount(normalizedGaf, true);
				if (inventoryCount >= 0)
				{
					int num4 = inventoryCount / num3;
					if (inventoryCount < num3)
					{
						int value = num3 - inventoryCount;
						if (missing != null)
						{
							missing[normalizedGaf] = value;
						}
						tilesScarcityInfo.missing[normalizedGaf] = value;
					}
					if (num == -1)
					{
						num = num4;
					}
					else
					{
						num = Mathf.Min(num, num4);
					}
				}
			}
		}
		if (hashSet.Count > 0)
		{
			foreach (Block block in BWSceneManager.AllBlocks())
			{
				string text2 = block.BlockType();
				if (hashSet.Contains(text2))
				{
					num = 0;
					GAF item = new GAF(Block.predicateCreate, new object[]
					{
						text2
					});
					if (uniquesInModel != null)
					{
						uniquesInModel.Add(item);
					}
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

	// Token: 0x06001383 RID: 4995 RVA: 0x0008638C File Offset: 0x0008478C
	private Clipboard.TilesScarcityInfo TilesScarcityInfoForModel(ModelData model, Dictionary<GAF, int> missing = null, List<GAF> uniquesInModel = null)
	{
		Clipboard.TilesScarcityInfo tilesScarcityInfo = new Clipboard.TilesScarcityInfo();
		int num = -1;
		foreach (GAF gaf in model.gafUsage.Keys)
		{
			int num2 = model.gafUsage[gaf];
			GAF normalizedGaf = Scarcity.GetNormalizedGaf(gaf, false);
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
			}
			else
			{
				int inventoryCount = Scarcity.GetInventoryCount(normalizedGaf, true);
				if (inventoryCount >= 0)
				{
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
					if (num == -1)
					{
						num = num4;
					}
					else
					{
						num = Mathf.Min(num, num4);
					}
				}
			}
		}
		if (model.uniqueBlockNames.Count > 0)
		{
			foreach (Block block in BWSceneManager.AllBlocks())
			{
				string text = block.BlockType();
				if (model.uniqueBlockNames.Contains(text))
				{
					num = 0;
					GAF item = new GAF(Block.predicateCreate, new object[]
					{
						text
					});
					if (uniquesInModel != null)
					{
						uniquesInModel.Add(item);
					}
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

	// Token: 0x06001384 RID: 4996 RVA: 0x00086574 File Offset: 0x00084974
	public int AvailableModelCount(ModelData model, Dictionary<GAF, int> missing = null, List<GAF> uniquesInModel = null, bool useCache = true)
	{
		if (Scarcity.inventory == null)
		{
			return -1;
		}
		Clipboard.TilesScarcityInfo tilesScarcityInfo;
		if (useCache && Clipboard.cachedModelScarcityInfos.TryGetValue(model, out tilesScarcityInfo))
		{
			this.GetMissingAndUniqueBlocksFromScarcityInfo(tilesScarcityInfo, missing, uniquesInModel);
			return tilesScarcityInfo.remaining;
		}
		tilesScarcityInfo = this.TilesScarcityInfoForModel(model, missing, uniquesInModel);
		Clipboard.cachedModelScarcityInfos[model] = tilesScarcityInfo;
		return tilesScarcityInfo.remaining;
	}

	// Token: 0x06001385 RID: 4997 RVA: 0x000865D4 File Offset: 0x000849D4
	public void Save()
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jsonstreamEncoder = new JSONStreamEncoder(writer, 20);
		jsonstreamEncoder.BeginObject();
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.WriteKey("paint");
		jsonstreamEncoder.WriteString(this.activePaint);
		jsonstreamEncoder.WriteKey("auto-paint");
		jsonstreamEncoder.WriteBool(this.autoPaintMode);
		jsonstreamEncoder.WriteKey("texture");
		jsonstreamEncoder.WriteString(this.activeTexture);
		jsonstreamEncoder.WriteKey("auto-texture");
		jsonstreamEncoder.WriteBool(this.autoTextureMode);
		jsonstreamEncoder.WriteKey("model");
		ModelUtils.WriteJSONForModel(jsonstreamEncoder, this.modelCopyPasteBuffer);
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.WriteKey("script");
		ModelUtils.WriteJSONForModel(jsonstreamEncoder, this.scriptCopyPasteBuffer);
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.EndObject();
		WorldSession.SaveClipboard(stringBuilder.ToString());
	}

	// Token: 0x06001386 RID: 4998 RVA: 0x000866B4 File Offset: 0x00084AB4
	public void Load()
	{
		this.SetDefaultBuffersIfNecessary();
		WorldSession.LoadClipboard();
	}

	// Token: 0x06001387 RID: 4999 RVA: 0x000866C1 File Offset: 0x00084AC1
	public static void LoadCallback(string clipboardString)
	{
		if (!string.IsNullOrEmpty(clipboardString))
		{
			Blocksworld.clipboard.LoadFromString(clipboardString);
		}
	}

	// Token: 0x06001388 RID: 5000 RVA: 0x000866DC File Offset: 0x00084ADC
	public void Reset()
	{
		if (BW.isUnityEditor)
		{
			this.paintGAF = null;
			this.textureGAF = null;
			PlayerPrefs.DeleteKey("tempClipboard");
			this.scriptCopyPasteBuffer.Clear();
			this.modelCopyPasteBuffer.Clear();
			this.SetDefaultBuffersIfNecessary();
			this.Save();
			Scarcity.UpdateInventory(true, null);
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			if (quickSelect != null)
			{
				quickSelect.UpdateIcons();
			}
		}
	}

	// Token: 0x06001389 RID: 5001 RVA: 0x00086754 File Offset: 0x00084B54
	private void SetDefaultBuffersIfNecessary()
	{
		if (this.scriptCopyPasteBuffer.Count == 0 || this.scriptCopyPasteBuffer[0].Count == 0)
		{
			Clipboard.cachedScriptScarcityInfos.Remove(this.scriptCopyPasteBuffer);
			this.scriptCopyPasteBuffer = new List<List<List<Tile>>>
			{
				new List<List<Tile>>
				{
					new List<Tile>
					{
						new Tile(new GAF(Block.predicateTapModel, new object[0])),
						new Tile(new GAF(Block.predicateThen, new object[0])),
						new Tile(new GAF("Block.Speak", new object[]
						{
							"Hello!"
						}))
					},
					new List<Tile>
					{
						new Tile(new GAF(Block.predicateThen, new object[0]))
					}
				}
			};
		}
		if (this.modelCopyPasteBuffer != null && (this.modelCopyPasteBuffer.Count == 0 || this.modelCopyPasteBuffer[0].Count == 0) && Blocksworld.buildPanel.IsGAFInBuildPanel(new GAF(Block.predicateCreate, new object[]
		{
			"Cube"
		})))
		{
			this.modelCopyPasteScarcityInfo = null;
			this.modelCopyPasteBuffer = new List<List<List<Tile>>>
			{
				new List<List<Tile>>
				{
					new List<Tile>
					{
						new Tile(new GAF(Block.predicateStop, new object[0])),
						new Tile(new GAF(Block.predicateCreate, new object[]
						{
							"Cube"
						})),
						new Tile(new GAF(Block.predicateMoveTo, new object[]
						{
							Vector3.zero
						})),
						new Tile(new GAF(Block.predicateRotateTo, new object[]
						{
							Vector3.zero
						})),
						new Tile(new GAF(Block.predicatePaintTo, new object[]
						{
							"Yellow"
						})),
						new Tile(new GAF(Block.predicateTextureTo, new object[]
						{
							"Plain",
							Vector3.up
						})),
						new Tile(new GAF(Block.predicateScaleTo, new object[]
						{
							Vector3.one
						}))
					},
					new List<Tile>
					{
						new Tile(new GAF(Block.predicateThen, new object[0]))
					}
				}
			};
		}
	}

	// Token: 0x0600138A RID: 5002 RVA: 0x000869F8 File Offset: 0x00084DF8
	private bool LoadFromString(string s)
	{
		bool result = false;
		try
		{
			JObject jobj = JSONDecoder.Decode(s);
			this.LoadFromJSON(jobj);
			return true;
		}
		catch (Exception ex)
		{
			BWLog.Warning("Unable to decode JSON for " + s);
			BWLog.Warning(string.Concat(new string[]
			{
				ex.GetType().Name,
				" exception message: ",
				ex.Message,
				" stacktrace: ",
				ex.StackTrace
			}));
			result = false;
		}
		return result;
	}

	// Token: 0x0600138B RID: 5003 RVA: 0x00086A88 File Offset: 0x00084E88
	private void LoadFromJSON(JObject jobj)
	{
		JObject jobject = jobj["paint"];
		if (jobject != null)
		{
			this.SetPaintColor((string)jobject, false);
		}
		JObject jobject2 = jobj["auto-paint"];
		if (jobject2 != null)
		{
			this.autoPaintMode = (bool)jobject2;
		}
		JObject jobject3 = jobj["texture"];
		if (jobject3 != null)
		{
			this.SetTexture((string)jobject3, false);
		}
		JObject jobject4 = jobj["auto-texture"];
		if (jobject4 != null)
		{
			this.autoTextureMode = (bool)jobject4;
		}
		this.modelCopyPasteBuffer = ModelUtils.ParseModelJSON(jobj["model"]);
		this.scriptCopyPasteBuffer = ModelUtils.ParseModelJSON(jobj["script"]);
		this.SetDefaultBuffersIfNecessary();
	}

	// Token: 0x04000F3D RID: 3901
	public List<List<List<Tile>>> modelCopyPasteBuffer = new List<List<List<Tile>>>();

	// Token: 0x04000F3E RID: 3902
	public List<List<List<Tile>>> modelDuplicateBuffer = new List<List<List<Tile>>>();

	// Token: 0x04000F3F RID: 3903
	public List<List<List<Tile>>> scriptCopyPasteBuffer = new List<List<List<Tile>>>();

	// Token: 0x04000F40 RID: 3904
	public List<List<List<Tile>>> modelTrashedBuffer = new List<List<List<Tile>>>();

	// Token: 0x04000F41 RID: 3905
	public Texture2D copiedModelIcon;

	// Token: 0x04000F42 RID: 3906
	public bool copiedModelIconUpToDate;

	// Token: 0x04000F47 RID: 3911
	private GAF paintGAF;

	// Token: 0x04000F48 RID: 3912
	private GAF textureGAF;

	// Token: 0x04000F49 RID: 3913
	private const string defaultPaintName = "Yellow";

	// Token: 0x04000F4A RID: 3914
	private const string defaultTextureName = "Plain";

	// Token: 0x04000F4B RID: 3915
	private bool _autoPaintMode;

	// Token: 0x04000F4C RID: 3916
	private bool _autoTextureMode;

	// Token: 0x04000F4D RID: 3917
	private static Dictionary<ModelData, Clipboard.TilesScarcityInfo> cachedModelScarcityInfos = new Dictionary<ModelData, Clipboard.TilesScarcityInfo>();

	// Token: 0x04000F4E RID: 3918
	private static Dictionary<List<List<List<Tile>>>, Clipboard.TilesScarcityInfo> cachedScriptScarcityInfos = new Dictionary<List<List<List<Tile>>>, Clipboard.TilesScarcityInfo>();

	// Token: 0x04000F4F RID: 3919
	private Clipboard.TilesScarcityInfo modelCopyPasteScarcityInfo;

	// Token: 0x04000F50 RID: 3920
	private Clipboard.TilesScarcityInfo trashedModelScarcityInfo;

	// Token: 0x02000114 RID: 276
	// (Invoke) Token: 0x06001390 RID: 5008
	public delegate void ClipboardEventHandler();

	// Token: 0x02000115 RID: 277
	private class TilesScarcityInfo
	{
		// Token: 0x04000F53 RID: 3923
		public int remaining = -1;

		// Token: 0x04000F54 RID: 3924
		public Dictionary<GAF, int> missing = new Dictionary<GAF, int>();

		// Token: 0x04000F55 RID: 3925
		public List<GAF> uniques = new List<GAF>();
	}
}
