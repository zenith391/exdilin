using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Blocks;
using Gestures;
using SimpleJSON;
using UnityEngine;

public class Scarcity
{
	public delegate void ScarcityEventHandler();

	public static Dictionary<GAF, int> puzzleInventory;

	public static Dictionary<GAF, int> puzzleGAFUsage;

	public static Dictionary<GAF, int> puzzleInventoryAfterStepByStep;

	public static Dictionary<GAF, int> puzzleGAFUsageAfterStepByStep;

	public static Dictionary<GAF, int> inventory = null;

	public static Dictionary<GAF, float> inventoryScales = new Dictionary<GAF, float>();

	public static Dictionary<GAF, int> globalInventory = null;

	public static Dictionary<GAF, int> worldGAFUsage = null;

	public static HashSet<GAF> autoRemoveHighlights = new HashSet<GAF>();

	public static Dictionary<string, string> textureOriginals = new Dictionary<string, string>();

	private static List<HudMeshLabel> scarcityLabels = new List<HudMeshLabel>();

	private static HudMeshLabel quickSelectPaintScarcityLabel;

	private static HudMeshLabel quickSelectTextureScarcityLabel;

	private static HudMeshLabel quickSelectModelScarcityLabel;

	private static HudMeshLabel quickSelectScriptScarcityLabel;

	public static Dictionary<string, BlockScarcityInfo> scarcityInfos;

	private static HashSet<Predicate> notCountInStepByStepPredicates = null;

	private static string[] normalizedPostfixes = new string[4] { "_Terrain", "_Sky", "_Water", "_Block" };

	public static event ScarcityEventHandler OnInventoryChange;

	public static void ReadScarcityInfo()
	{
		scarcityInfos = new Dictionary<string, BlockScarcityInfo>();
		TextAsset textAsset = Resources.Load("BlockInfoList") as TextAsset;
		if (textAsset == null)
		{
			BWLog.Error("Unable to load BlockInfoList");
		}
		JObject jObject = JSONDecoder.Decode(textAsset.text);
		List<JObject> arrayValue = jObject["block-infos"].ArrayValue;
		foreach (JObject item in arrayValue)
		{
			BlockScarcityInfo blockScarcityInfo = BlockScarcityInfo.Read(item);
			scarcityInfos[blockScarcityInfo.blockName] = blockScarcityInfo;
		}
	}

	private static bool TryGetBlockMeshScarcityInfo(string blockType, int meshIndex, out BlockMeshScarcityInfo info)
	{
		if (scarcityInfos.TryGetValue(blockType, out var value) && meshIndex < value.meshInfos.Count)
		{
			info = value.meshInfos[meshIndex];
			return true;
		}
		info = null;
		return false;
	}

	public static string DefaultTexture(string blockType, int meshIndex)
	{
		if (TryGetBlockMeshScarcityInfo(blockType, meshIndex, out var info))
		{
			return info.defaultTexture;
		}
		return "Plain";
	}

	public static string DefaultPaint(string blockType, int meshIndex)
	{
		if (TryGetBlockMeshScarcityInfo(blockType, meshIndex, out var info))
		{
			return info.defaultPaint;
		}
		return "Yellow";
	}

	public static string[] DefaultPaints(string blockType)
	{
		List<string> list = new List<string>();
		if (scarcityInfos.TryGetValue(blockType, out var value))
		{
			foreach (BlockMeshScarcityInfo meshInfo in value.meshInfos)
			{
				list.Add(meshInfo.defaultPaint);
			}
		}
		if (list.Count == 0)
		{
			list.Add("Yellow");
		}
		return list.ToArray();
	}

	public static string[] DefaultTextures(string blockType)
	{
		List<string> list = new List<string>();
		if (scarcityInfos.TryGetValue(blockType, out var value))
		{
			foreach (BlockMeshScarcityInfo meshInfo in value.meshInfos)
			{
				list.Add(meshInfo.defaultTexture);
			}
		}
		if (list.Count == 0)
		{
			list.Add("Plain");
		}
		return list.ToArray();
	}

	public static bool FreeTexture(string blockType, int meshIndex, string texture)
	{
		if (blockType != null && texture != null && TryGetBlockMeshScarcityInfo(blockType, meshIndex, out var info))
		{
			return info.freeTextures.Contains(texture);
		}
		return false;
	}

	public static bool FreePaint(string blockType, int meshIndex, string paint)
	{
		if (blockType != null && paint != null && TryGetBlockMeshScarcityInfo(blockType, meshIndex, out var info))
		{
			return info.freePaints.Contains(paint);
		}
		return false;
	}

	public static HashSet<string> FreePaints(string blockType, int meshIndex)
	{
		if (TryGetBlockMeshScarcityInfo(blockType, meshIndex, out var info))
		{
			return info.freePaints;
		}
		return new HashSet<string> { DefaultPaint(blockType, meshIndex) };
	}

	public static bool FreeSfx(string blockType, string sfx)
	{
		if (blockType != null && sfx != null && scarcityInfos.TryGetValue(blockType, out var value))
		{
			return value.freeSfxs.Contains(sfx);
		}
		return false;
	}

	public static HashSet<string> GetShapeCategories(string blockType)
	{
		if (scarcityInfos.TryGetValue(blockType, out var value))
		{
			return value.shapeCategories;
		}
		return new HashSet<string>();
	}

	public static void SetTextureOriginal(string rewritten, string original)
	{
		textureOriginals[rewritten] = original;
	}

	private static HashSet<Predicate> GetNotCountInStepByStepPredicates()
	{
		if (notCountInStepByStepPredicates == null)
		{
			notCountInStepByStepPredicates = new HashSet<Predicate>
			{
				Block.predicateThen,
				Block.predicateLocked,
				Block.predicateUnlocked,
				Block.predicateTutorialCreateBlockHint,
				Block.predicateTutorialRemoveBlockHint
			};
		}
		return notCountInStepByStepPredicates;
	}

	public static void GetScriptColorAndTextureUse(List<List<List<Tile>>> tiles, Dictionary<GAF, int> result)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			List<List<Tile>> list = tiles[i];
			string blockType = null;
			HashSet<GAF> hashSet = new HashSet<GAF>();
			List<Tile> list2 = list[0];
			for (int j = 0; j < list2.Count; j++)
			{
				Tile tile = list2[j];
				GAF gaf = tile.gaf;
				Predicate predicate = gaf.Predicate;
				object[] args = gaf.Args;
				if (predicate == Block.predicateCreate)
				{
					blockType = (string)args[0];
					break;
				}
			}
			for (int k = 1; k < list.Count; k++)
			{
				List<Tile> list3 = list[k];
				for (int l = 0; l < list3.Count; l++)
				{
					Tile tile2 = list3[l];
					GAF gaf2 = tile2.gaf;
					Predicate predicate2 = gaf2.Predicate;
					if (predicate2 == Block.predicateHideNextTile)
					{
						l++;
						continue;
					}
					object[] args2 = gaf2.Args;
					if (predicate2 == Block.predicatePaintTo)
					{
						string stringArg = Util.GetStringArg(args2, 0, "Yellow");
						int intArg = Util.GetIntArg(args2, 1, 0);
						if (!FreePaint(blockType, intArg, stringArg))
						{
							GAF normalizedGaf = GetNormalizedGaf(gaf2);
							hashSet.Add(normalizedGaf);
						}
					}
					else if (predicate2 == Block.predicateTextureTo)
					{
						string stringArg2 = Util.GetStringArg(args2, 0, "Plain");
						int intArg2 = Util.GetIntArg(args2, 2, 0);
						if (!FreeTexture(blockType, intArg2, stringArg2))
						{
							GAF normalizedGaf2 = GetNormalizedGaf(gaf2);
							hashSet.Add(normalizedGaf2);
						}
					}
				}
			}
			foreach (GAF item in hashSet)
			{
				int value = 0;
				result.TryGetValue(item, out value);
				result[item] = value + 1;
			}
		}
	}

	public static Dictionary<GAF, int> GetNormalizedInventoryUse(List<List<List<Tile>>> tiles, WorldType worldType, bool includeLocked = false)
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		GetInventoryUse(tiles, worldType, dictionary, includeLocked);
		Dictionary<GAF, int> dictionary2 = new Dictionary<GAF, int>();
		foreach (KeyValuePair<GAF, int> item in dictionary)
		{
			GAF normalizedGaf = GetNormalizedGaf(item.Key);
			if (BlockItem.FindByGafPredicateNameAndArguments(normalizedGaf.Predicate.Name, normalizedGaf.Args) != null)
			{
				int value = item.Value;
				int value2 = 0;
				dictionary2.TryGetValue(normalizedGaf, out value2);
				dictionary2[normalizedGaf] = value + value2;
			}
		}
		GetScriptColorAndTextureUse(tiles, dictionary2);
		return dictionary2;
	}

	public static void GetInventoryUse(List<List<List<Tile>>> tiles, WorldType worldType, Dictionary<GAF, int> result, bool includeLocked = false)
	{
		int[] array = new int[tiles.Count];
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		for (int i = 0; i < tiles.Count; i++)
		{
			List<List<Tile>> tiles2 = tiles[i];
			Tile tile = BlockGroups.FindGroupTile(tiles2, "tank-treads");
			if (tile != null)
			{
				int key = BlockGroups.GroupId(tile);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = 0;
				}
				dictionary[key] = value + 1;
				if (BlockGroups.IsMainGroupBlock(tile))
				{
					dictionary2[key] = i;
				}
			}
			Tile tile2 = BlockGroups.FindGroupTile(tiles2, "teleport-volume");
			if (tile2 != null)
			{
				int key2 = BlockGroups.GroupId(tile2);
				if (!dictionary.TryGetValue(key2, out var value2))
				{
					value2 = 0;
				}
				dictionary[key2] = value2 + 1;
				if (BlockGroups.IsMainGroupBlock(tile2))
				{
					dictionary2[key2] = i;
				}
			}
		}
		foreach (KeyValuePair<int, int> item in dictionary2)
		{
			int key3 = item.Key;
			int value3 = item.Value;
			array[value3] = dictionary[key3];
		}
		for (int j = 0; j < tiles.Count; j++)
		{
			GetInventoryUse(tiles[j], worldType, array[j], result, includeLocked);
		}
	}

	public static void GetInventoryUse(List<List<Tile>> tiles, WorldType worldType, int groupSize, Dictionary<GAF, int> result, bool includeLocked = false)
	{
		if (tiles.Count > 1 && tiles[1].Count > 1 && tiles[1][1].IsLocked() && !includeLocked)
		{
			return;
		}
		bool flag = worldType != WorldType.ForComplexityCalculation;
		for (int i = 0; i < tiles.Count; i++)
		{
			bool flag2 = i == 0;
			List<Tile> list = tiles[i];
			bool flag3 = false;
			for (int j = 0; j < list.Count; j++)
			{
				Tile tile = list[j];
				if (tile.gaf.Predicate == Block.predicateHideTileRow)
				{
					flag3 = true;
					break;
				}
			}
			if (flag3)
			{
				continue;
			}
			string text = null;
			for (int k = 0; k < list.Count; k++)
			{
				Tile tile2 = list[k];
				GAF gaf = tile2.gaf;
				Predicate predicate = gaf.Predicate;
				if (predicate == Block.predicateHideNextTile)
				{
					k++;
					continue;
				}
				object[] args = gaf.Args;
				if (predicate == Block.predicateCreate)
				{
					text = (string)args[0];
					if (!ProfileBlocksterUtils.IsProfileBlockType(text))
					{
						if (groupSize == 0)
						{
							UpdateWorldGAFUsage(gaf, 1, result);
						}
						else
						{
							UpdateWorldGAFUsage(new GAF(Block.predicateCreate, (string)args[0] + " x" + groupSize), 1, result);
						}
					}
				}
				else if (predicate == BlockAnimatedCharacter.predicateReplaceLimb)
				{
					string text2 = (string)args[0];
					if (text2.EndsWith(" Left"))
					{
						text2 = text2.Remove(text2.Length - 5, 5);
					}
					else if (text2.EndsWith(" Right"))
					{
						text2 = text2.Remove(text2.Length - 6, 6);
					}
					UpdateWorldGAFUsage(new GAF(BlockAnimatedCharacter.predicateReplaceLimb, text2), 1, result);
				}
				if (worldType == WorldType.StepByStepTutorial)
				{
					if (GetNotCountInStepByStepPredicates().Contains(predicate))
					{
						continue;
					}
					EditableTileParameter editableParameter = gaf.Predicate.EditableParameter;
					GAF primitiveGafFor = TileToggleChain.GetPrimitiveGafFor(gaf);
					if (primitiveGafFor != null)
					{
						UpdateWorldGAFUsage(primitiveGafFor, 1, result);
					}
					else if (editableParameter != null)
					{
						GAF gAF = gaf.Clone();
						gAF.Args[editableParameter.parameterIndex] = null;
						Tile tile3 = Blocksworld.buildPanel.FindTileMatching(gAF);
						if (tile3 != null)
						{
							UpdateWorldGAFUsage(tile3.gaf, 1, result);
							continue;
						}
						GAF gAF2 = gaf.Clone();
						GAF gaf2 = new GAF(gAF2.Predicate, gAF2.Predicate.ExtendArguments(gAF2.Args, overwrite: true));
						UpdateWorldGAFUsage(gaf2, 1, result);
					}
					else if (predicate == BlockMaster.predicateSetEnvEffect)
					{
						string text3 = BlockSky.EnvEffectToTexture(Util.GetStringArg(args, 0, "Clear"));
						UpdateWorldGAFUsage(new GAF(Block.predicateTextureTo, text3, Vector3.zero), 1, result);
					}
					else if (predicate == BlockMaster.predicatePaintSkyTo)
					{
						UpdateWorldGAFUsage(new GAF(Block.predicatePaintTo, Util.GetStringArg(args, 0, "Yellow")), 1, result);
					}
					else if (predicate == Block.predicateTutorialPaintExistingBlock)
					{
						UpdateWorldGAFUsage(new GAF(Block.predicatePaintTo, Util.GetStringArg(args, 1, "Yellow")), 1, result);
					}
					else if (predicate == Block.predicateTutorialTextureExistingBlock)
					{
						UpdateWorldGAFUsage(new GAF(Block.predicateTextureTo, Util.GetStringArg(args, 1, "Plain"), Vector3.zero), 1, result);
					}
					else if (predicate == Block.predicateTextureTo)
					{
						UpdateWorldGAFUsage(new GAF(Block.predicateTextureTo, Util.GetStringArg(args, 0, "Plain"), Vector3.zero), 1, result);
					}
					else if (predicate == Block.predicatePaintTo)
					{
						UpdateWorldGAFUsage(new GAF(Block.predicatePaintTo, Util.GetStringArg(args, 0, "Yellow")), 1, result);
					}
					else if (!flag2)
					{
						UpdateWorldGAFUsage(gaf, 1, result);
					}
					continue;
				}
				if (flag2 && flag)
				{
					if (predicate == Block.predicatePaintTo)
					{
						string stringArg = Util.GetStringArg(args, 0, "Yellow");
						int intArg = Util.GetIntArg(args, 1, 0);
						if (flag2 && !FreePaint(text, intArg, stringArg))
						{
							UpdateWorldGAFUsage(gaf, 1, result);
						}
					}
					else if (predicate == Block.predicateTextureTo)
					{
						string stringArg2 = Util.GetStringArg(args, 0, "Plain");
						int intArg2 = Util.GetIntArg(args, 2, 0);
						if (flag2 && !FreeTexture(text, intArg2, stringArg2))
						{
							UpdateWorldGAFUsage(gaf, 1, result);
						}
					}
				}
				if (predicate == Block.predicatePlaySoundDurational)
				{
					string stringArg3 = Util.GetStringArg(args, 0, string.Empty);
					if (!FreeSfx(text, stringArg3))
					{
						UpdateWorldGAFUsage(gaf, 1, result);
					}
				}
			}
		}
	}

	public static void StepInventoryScales()
	{
		if (!ScarcityBadgesShowing() || GetScarcityHighlightGafs() == null)
		{
			return;
		}
		List<GAF> list = new List<GAF>(inventoryScales.Keys);
		foreach (GAF item in list)
		{
			float num = inventoryScales[item];
			num *= 0.93f;
			if (num < 1f)
			{
				num = 1f;
			}
			inventoryScales[item] = num;
		}
		History.RemoveHighlightsIfNecessary();
	}

	private static HashSet<GAF> GetScarcityHighlightGafs()
	{
		HashSet<GAF> result = null;
		result = CreateTileDragGesture.GetScarcityHighlightGafs(result);
		result = TileDragGesture.GetScarcityHighlightGafs(result);
		result = CharacterEditGearGesture.GetScarcityHighlightGafs(result);
		result = ReplaceBodyPartTileDragGesture.GetScarcityHighlightGafs(result);
		result = RewardVisualization.GetScarcityHighlightGafs(result);
		result = History.GetScarcityHighlightGafs(result);
		return TBoxGesture.GetScarcityHighlightGafs(result);
	}

	private static bool ScarcityBadgesShowing()
	{
		if (inventory == null || Blocksworld.CurrentState == State.Play)
		{
			return RewardVisualization.rewardAnimationRunning;
		}
		return true;
	}

	public static void PaintScarcityBadges()
	{
		if (!ScarcityBadgesShowing() || inventory == null)
		{
			return;
		}
		HudMeshStyle inventoryStyle = HudMeshOnGUI.dataSource.inventoryStyle;
		HashSet<GAF> scarcityHighlightGafs = GetScarcityHighlightGafs();
		BuildPanel buildPanel = Blocksworld.buildPanel;
		int labelIndex = 0;
		TabBarTabId selectedTab = buildPanel.GetTabBar().SelectedTab;
		List<Tile> tilesInTab = buildPanel.GetTilesInTab(selectedTab);
		if (tilesInTab == null)
		{
			return;
		}
		for (int i = 0; i < tilesInTab.Count; i++)
		{
			Tile tile = tilesInTab[i];
			if (tile.IsShowing())
			{
				GAF gaf = tile.gaf;
				PaintBadge(tile, gaf, scarcityHighlightGafs, ref labelIndex, quickSelectUIVisible: true, inventoryStyle);
				if (Blocksworld.clipboard.autoTextureMode && buildPanel.IsBlockTabSelected())
				{
					Blocksworld.UI.QuickSelect.ShowTextureScarcity();
				}
			}
		}
		if (Blocksworld.modelCollection != null && buildPanel.IsModelTabSelected())
		{
			List<Tile> tilesInTab2 = buildPanel.GetTilesInTab(TabBarTabId.Models);
			for (int j = 0; j < tilesInTab2.Count; j++)
			{
				PaintBadge(tilesInTab2[j], tilesInTab2[j].gaf, scarcityHighlightGafs, ref labelIndex, quickSelectUIVisible: true, inventoryStyle);
			}
		}
	}

	private static bool PaintBadge(Tile tile, GAF gaf, HashSet<GAF> highlightGafs, ref int labelIndex, bool quickSelectUIVisible, HudMeshStyle inventoryStyle)
	{
		if (!tile.IsShowing())
		{
			return false;
		}
		int inventoryCount = -1;
		float scale = 1f;
		bool flag = ShouldDrawBadge(gaf, highlightGafs, out inventoryCount, out scale);
		if (!flag)
		{
			return false;
		}
		if (tile.IsCreateModel())
		{
			int intArg = Util.GetIntArg(gaf.Args, 0, -1);
			if (intArg >= 0)
			{
				ModelData model = Blocksworld.modelCollection.models[intArg];
				inventoryCount = Blocksworld.clipboard.AvailableModelCount(model);
			}
		}
		if (inventoryCount > 0 && !tile.IsEnabled())
		{
			tile.Enable(enabled: true);
		}
		else if (inventoryCount == 0 && tile.IsEnabled())
		{
			tile.Enable(enabled: false);
		}
		string text = inventoryCount.ToString();
		Vector2 centerPos = new Vector2(tile.tileObject.GetPosition().x + 70f * NormalizedScreen.pixelScale, (float)NormalizedScreen.height - tile.tileObject.GetPosition().y - 70f * NormalizedScreen.pixelScale);
		Rect rect = RectForLabel(text, centerPos, scale);
		if (quickSelectUIVisible)
		{
			flag = centerPos.y < (float)NormalizedScreen.height - Blocksworld.UI.QuickSelect.GetHeight() - rect.height;
		}
		if (flag)
		{
			HudMeshOnGUI.Label(scarcityLabels, labelIndex, rect, text, inventoryStyle);
			if (scarcityLabels.Count > labelIndex && scarcityLabels[labelIndex] != null)
			{
				scarcityLabels[labelIndex].transform.localScale = new Vector3(scale, scale, 1f);
			}
			labelIndex++;
			return true;
		}
		return false;
	}

	private static bool ShouldDrawBadge(GAF gaf, HashSet<GAF> highlightGafs, out int inventoryCount, out float scale, bool debug = false)
	{
		inventoryCount = -1;
		scale = 1f;
		if (gaf == null)
		{
			return false;
		}
		if (inventory != null && inventory.ContainsKey(gaf))
		{
			inventoryCount = inventory[gaf];
			if (inventoryCount < 0)
			{
				return false;
			}
		}
		else if (gaf.Predicate != Block.predicateCreateModel)
		{
			return false;
		}
		if (highlightGafs == null || !gaf.MatchesAny(highlightGafs))
		{
			return false;
		}
		if (!inventoryScales.TryGetValue(gaf, out scale))
		{
			scale = 1f;
		}
		return true;
	}

	public static Rect RectForLabel(string labelText, Vector2 centerPos, float scale)
	{
		Vector2 vector = new Vector2(32f, 32f) * NormalizedScreen.pixelScale;
		float num = 5f * NormalizedScreen.scale;
		float scale2 = NormalizedScreen.scale;
		Vector2 vector2 = vector * scale2;
		if (labelText.Length > 1)
		{
			vector2 += new Vector2(num * (float)labelText.Length - 1f, 0f) * scale2;
		}
		Vector2 vector3 = (vector2 * scale - vector2) / (2f * scale2);
		Vector2 vector4 = new Vector2((centerPos.x - vector.x / 2f - vector3.x) * scale2, (centerPos.y - vector.y / 2f - vector3.y) * scale2);
		return new Rect(vector4.x, vector4.y, vector2.x, vector2.y);
	}

	public static string GetNormalizedTexture(string texture)
	{
		if (textureOriginals.ContainsKey(texture))
		{
			return textureOriginals[texture];
		}
		for (int i = 0; i < normalizedPostfixes.Length; i++)
		{
			int num = texture.IndexOf(normalizedPostfixes[i]);
			if (num >= 0)
			{
				texture = texture.Substring(0, num);
				break;
			}
		}
		return texture;
	}

	public static GAF GetNormalizedGaf(GAF gaf, bool keepPaintAndTextureIndices = false)
	{
		string name = gaf.Predicate.Name;
		object[] args = gaf.Args;
		switch (name)
		{
		case "Block.TextureTo":
		{
			string texture = (string)args[0];
			if (keepPaintAndTextureIndices)
			{
				return new GAF(name, GetNormalizedTexture(texture), Vector3.zero, Util.GetIntArg(gaf.Args, 2, 0));
			}
			return new GAF(name, GetNormalizedTexture(texture), Vector3.zero);
		}
		case "Block.PaintTo":
		{
			string text3 = (string)args[0];
			if (keepPaintAndTextureIndices)
			{
				return new GAF(name, text3, Util.GetIntArg(gaf.Args, 1, 0));
			}
			return new GAF(name, text3);
		}
		case "Block.Create":
		{
			string text2 = (string)args[0];
			switch (text2)
			{
			case "Anim Character Avatar":
				return new GAF(name, "Character Avatar");
			default:
				return new GAF(name, text2);
			case "Volume":
				break;
			}
			break;
		}
		case "AnimCharacter.ReplaceBodyPart":
		{
			string text = (string)args[0];
			if (text.EndsWith(" Left"))
			{
				text = text.Remove(text.Length - 5, 5);
			}
			else if (text.EndsWith(" Right"))
			{
				text = text.Remove(text.Length - 6, 6);
			}
			return new GAF(name, text);
		}
		case "Block.SendCustomSignal":
		case "Block.SendCustomSignalModel":
			return new GAF(name, "*", 1f);
		case "BlockVariable.Int":
		case "Variable.CustomInt":
			return new GAF(name, "*", 0);
		}
		return gaf;
	}

	public static bool IsFreeTexture(string texture, int meshIndex, Block b)
	{
		switch (texture)
		{
		case "Plain":
			return true;
		case "Clothing Underwear":
			if (b is BlockCharacter)
			{
				return meshIndex == 1;
			}
			return false;
		default:
			if (b.BlockType() == "Sky UV")
			{
				switch (texture)
				{
				case "Cloudy Sky":
				case "Space Sky":
					return true;
				}
			}
			return false;
		}
	}

	public static void CalculateBlockGafUsage(Block b, Dictionary<GAF, int> result, bool verbose = false, bool forStepByStepTutorial = false)
	{
		List<Tile> list = b.tiles[0];
		string blockType = b.BlockType();
		BlockGrouped blockGrouped = b as BlockGrouped;
		if ((blockGrouped != null && !blockGrouped.IsMainBlockInGroup()) || (b.IsLocked() && (!b.isTerrain || b.SelectableTerrain())) || (Blocksworld.editorSelectionLocked.Contains(b) && !b.IsProfileCharacter()))
		{
			return;
		}
		HashSet<Predicate> hashSet = null;
		if (forStepByStepTutorial)
		{
			hashSet = new HashSet<Predicate>
			{
				Block.predicateThen,
				Block.predicateTutorialPaintExistingBlock,
				Block.predicateTutorialTextureExistingBlock,
				Block.predicateLocked,
				PredicateRegistry.ByName("Block.TutorialCreateBlockHint")
			};
		}
		string text = string.Empty;
		if (b is BlockAnimatedCharacter)
		{
			(b as BlockAnimatedCharacter).CalculateBodyPartsGAFUsage(result);
		}
		for (int i = 1; i < b.tiles.Count; i++)
		{
			List<Tile> list2 = b.tiles[i];
			bool flag = false;
			for (int j = 0; j < list2.Count; j++)
			{
				Tile tile = list2[j];
				GAF gaf = tile.gaf;
				if (gaf.Predicate == Block.predicateHideTileRow)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			bool flag2 = false;
			for (int k = 0; k < list2.Count; k++)
			{
				Tile tile2 = list2[k];
				if (flag2)
				{
					flag2 = false;
					continue;
				}
				GAF gaf2 = tile2.gaf;
				if (gaf2.Predicate == Block.predicatePlaySoundDurational)
				{
					object[] args = gaf2.Args;
					GAF gAF = new GAF(gaf2.Predicate.Name, args);
					if (!Block.IsDefaultSfx(blockType, (string)args[0]))
					{
						UpdateWorldGAFUsage(gAF, 1, result);
						if (verbose)
						{
							string text2 = text;
							text = string.Concat(text2, b.BlockType(), ": ", gAF, "\n");
						}
					}
				}
				if (forStepByStepTutorial)
				{
					if (gaf2.Predicate == Block.predicateHideNextTile)
					{
						flag2 = true;
					}
					else if (!hashSet.Contains(gaf2.Predicate))
					{
						UpdateWorldGAFUsage(GetNormalizedGaf(gaf2), 1, result);
					}
				}
			}
		}
		bool[] array = new bool[b.subMeshGameObjects.Count + 1];
		for (int l = 0; l < list.Count; l++)
		{
			Tile tile3 = list[l];
			GAF gaf3 = tile3.gaf;
			Predicate predicate = gaf3.Predicate;
			object[] args2 = gaf3.Args;
			if (predicate == Block.predicateTextureTo || predicate == BlockSky.predicateEnvEffect)
			{
				bool flag3 = predicate == Block.predicateTextureTo;
				string text3 = (string)args2[0];
				string texture = ((!flag3) ? BlockSky.EnvEffectToTexture(text3) : text3);
				int num = (flag3 ? ((args2.Length > 2) ? ((int)args2[2]) : 0) : 0);
				if (num < array.Length && !array[num] && !IsFreeTexture(texture, num, b) && !b.IsDefaultTexture(texture, num))
				{
					GAF gAF2 = new GAF(Block.predicateTextureTo, GetNormalizedTexture(texture), Vector3.zero);
					UpdateWorldGAFUsage(gAF2, 1, result);
					if (verbose)
					{
						string text4 = text;
						text = string.Concat(text4, b.BlockType(), ": ", gAF2, "\n");
					}
					array[num] = true;
				}
			}
			else if (predicate == Block.predicatePaintTo)
			{
				string text5 = (string)args2[0];
				if (!b.IsDefaultPaint(gaf3))
				{
					GAF gAF3 = new GAF(predicate, text5);
					UpdateWorldGAFUsage(gAF3, 1, result);
					if (verbose)
					{
						string text6 = text;
						text = string.Concat(text6, b.BlockType(), ": ", gAF3, "\n");
					}
				}
			}
			else
			{
				if (predicate != Block.predicateCreate)
				{
					continue;
				}
				gaf3 = GetNormalizedGaf(gaf3);
				string text7 = (string)args2[0];
				if (ProfileBlocksterUtils.IsProfileBlockType(text7) || (text7 != null && text7 == "Volume"))
				{
					continue;
				}
				if (blockGrouped != null)
				{
					gaf3 = blockGrouped.GetIconGaf();
				}
				if (Blocksworld.buildPanel.IsGAFInBuildPanel(gaf3) || Blocksworld.GetUniqueBlockMap().ContainsKey(text7))
				{
					UpdateWorldGAFUsage(gaf3, 1, result);
					if (verbose)
					{
						string text8 = text;
						text = string.Concat(text8, b.BlockType(), ": ", gaf3, "\n");
					}
				}
			}
		}
		if (verbose && text.Length > 0)
		{
			BWLog.Info(text);
		}
	}

	public static Dictionary<GAF, int> CalculateWorldGafUsage(bool verbose = false, bool forStepByStepTutorials = false)
	{
		List<Block> list = BWSceneManager.AllBlocks();
		Dictionary<GAF, int> result = new Dictionary<GAF, int>();
		for (int i = 0; i < list.Count; i++)
		{
			CalculateBlockGafUsage(list[i], result, verbose, forStepByStepTutorials);
		}
		return result;
	}

	public static bool IsRelevantGAF(string blockType, GAF gaf, bool firstRow)
	{
		object[] args = gaf.Args;
		string text = ((args.Length == 0 || !(args[0] is string)) ? string.Empty : ((string)args[0]));
		int meshIndex = 0;
		if (gaf.Predicate == Block.predicatePaintTo)
		{
			meshIndex = Util.GetIntArg(args, 1, 0);
		}
		else if (gaf.Predicate == Block.predicateTextureTo)
		{
			meshIndex = Util.GetIntArg(args, 2, 0);
		}
		bool flag = false;
		if (firstRow)
		{
			flag |= gaf.Predicate == Block.predicateTextureTo && !FreeTexture(blockType, meshIndex, text);
			flag |= gaf.Predicate == Block.predicatePaintTo && !FreePaint(blockType, meshIndex, text);
			flag |= gaf.Predicate == Block.predicateCreate && text != "Volume" && !ProfileBlocksterUtils.IsProfileBlockType(text) && Blocksworld.buildPanel.IsGAFInBuildPanel(gaf);
		}
		flag |= gaf.Predicate == Block.predicatePlaySoundDurational && !FreeSfx(blockType, text);
		return flag | (gaf.Predicate == BlockAnimatedCharacter.predicateReplaceLimb);
	}

	public static void CompareIncrementalAndAbsoluteGafUsages()
	{
		CompareGafUsages(CalculateWorldGafUsage(), worldGAFUsage, "Absolute", "Incremental");
	}

	public static Dictionary<GAF, int> CompareGafUsages(Dictionary<GAF, int> d1, Dictionary<GAF, int> d2, string n1 = "d1", string n2 = "d2")
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		foreach (GAF key in d1.Keys)
		{
			int num = d1[key];
			if (d2.ContainsKey(key))
			{
				int num2 = d2[key];
				if (num != num2)
				{
					UpdateWorldGAFUsage(key, num - num2, dictionary, clampZero: false);
				}
			}
			else
			{
				UpdateWorldGAFUsage(key, num, dictionary, clampZero: false);
			}
		}
		foreach (GAF key2 in d2.Keys)
		{
			if (!d1.ContainsKey(key2))
			{
				int num3 = d2[key2];
				UpdateWorldGAFUsage(key2, -num3, dictionary, clampZero: false);
			}
		}
		return dictionary;
	}

	public static void UpdateScarcityBadges(HashSet<GAF> highlights, Dictionary<GAF, int> oldGafUsage, Dictionary<GAF, int> startInventory)
	{
		if (startInventory == null)
		{
			return;
		}
		Dictionary<GAF, int> d = CalculateWorldGafUsage();
		Dictionary<GAF, int> dictionary = CompareGafUsages(oldGafUsage, d);
		foreach (GAF key in dictionary.Keys)
		{
			if (dictionary[key] != 0)
			{
				highlights.Add(key);
			}
		}
		foreach (GAF highlight in highlights)
		{
			if (startInventory.ContainsKey(highlight))
			{
				inventoryScales[highlight] = 1.5f;
			}
		}
	}

	public static bool GetExistsInInventory(GAF gaf)
	{
		if (inventory != null)
		{
			return inventory.ContainsKey(gaf);
		}
		return false;
	}

	public static int GetInventoryCount(GAF gaf, bool zeroIfNotExist = false)
	{
		if (inventory != null && inventory.ContainsKey(gaf))
		{
			return inventory[gaf];
		}
		if (zeroIfNotExist)
		{
			return 0;
		}
		return -1;
	}

	public static Dictionary<GAF, int> GetInventoryCopy()
	{
		if (inventory != null)
		{
			Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
			{
				foreach (GAF key in inventory.Keys)
				{
					dictionary.Add(key, inventory[key]);
				}
				return dictionary;
			}
		}
		return null;
	}

	public static void UpdateWorldGAFUsage(GAF gaf, int change, Dictionary<GAF, int> usage = null, bool clampZero = true)
	{
		if (usage == null)
		{
			usage = worldGAFUsage;
		}
		if (!usage.ContainsKey(gaf))
		{
			usage[gaf] = 0;
		}
		usage[gaf] = ((!clampZero) ? (usage[gaf] + change) : Mathf.Max(0, usage[gaf] + change));
	}

	public static string SaveWorldGAFUsage(bool verbose = false, bool insertLines = false)
	{
		worldGAFUsage = CalculateWorldGafUsage(verbose);
		return GetInventoryJSON(worldGAFUsage, verbose, insertLines);
	}

	public static string GetInventoryJSON(Dictionary<GAF, int> inv, bool verbose = false, bool insertLines = false)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jSONStreamEncoder = new JSONStreamEncoder(writer);
		jSONStreamEncoder.BeginArray();
		foreach (KeyValuePair<GAF, int> item in inv)
		{
			jSONStreamEncoder.BeginArray();
			item.Key.ToJSONCompact(jSONStreamEncoder);
			jSONStreamEncoder.WriteNumber(item.Value);
			jSONStreamEncoder.EndArray();
			if (insertLines)
			{
				jSONStreamEncoder.InsertNewline();
			}
		}
		jSONStreamEncoder.EndArray();
		return stringBuilder.ToString();
	}

	public static string GetBlockIDInventoryString(Dictionary<GAF, int> inv, HashSet<Predicate> infinitePredicates = null)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		bool flag = true;
		foreach (KeyValuePair<GAF, int> item in inv)
		{
			GAF normalizedGaf = GetNormalizedGaf(item.Key);
			BlockItem blockItem = BlockItem.FindByGafPredicateNameAndArguments(normalizedGaf.Predicate.Name, normalizedGaf.Args);
			if (blockItem == null)
			{
				BWLog.Info("Couldn't find id for block: " + normalizedGaf.ToString());
				continue;
			}
			int id = blockItem.Id;
			if (!flag)
			{
				stringBuilder.Append('|');
			}
			int value = item.Value;
			int num = ((infinitePredicates != null && infinitePredicates.Contains(normalizedGaf.Predicate)) ? 1 : 0);
			stringBuilder.Append(id.ToString());
			stringBuilder.Append(':');
			if (num == 0)
			{
				stringBuilder.Append(value.ToString());
			}
			else
			{
				stringBuilder.Append(';');
				stringBuilder.Append(num.ToString());
			}
			flag = false;
		}
		return stringBuilder.ToString();
	}

	public static Dictionary<GAF, int> ReadBlockIDInventoryString(string blockIDInventoryString)
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		int c = 0;
		StringBuilder sb = new StringBuilder(32768);
		List<char> separators = new List<char> { ':', ';', '|' };
		bool done = false;
		char currentSeparator = '|';
		Action action = delegate
		{
			sb.Length = 0;
			for (; c < blockIDInventoryString.Length && !separators.Contains(blockIDInventoryString[c]); c++)
			{
				sb.Append(blockIDInventoryString[c]);
			}
			done = c >= blockIDInventoryString.Length;
			if (!done)
			{
				currentSeparator = blockIDInventoryString[c];
				c++;
			}
		};
		while (!done)
		{
			int result = 0;
			int result2 = 0;
			action();
			int.TryParse(sb.ToString(), out var result3);
			action();
			int.TryParse(sb.ToString(), out result);
			if (currentSeparator != '|')
			{
				action();
				int.TryParse(sb.ToString(), out result2);
			}
			if (BlockItem.Exists(result3))
			{
				BlockItem blockItem = BlockItem.FindByID(result3);
				if (blockItem == null)
				{
					BWLog.Error("Couldn't find block with id: " + result3);
					continue;
				}
				GAF key = new GAF(blockItem);
				int value = ((result2 <= 0) ? result : (-1));
				dictionary[key] = value;
			}
		}
		return dictionary;
	}

	public static void ResetInventory()
	{
		if (globalInventory != null)
		{
			inventory = new Dictionary<GAF, int>(globalInventory);
		}
		else
		{
			inventory = null;
		}
		if (Blocksworld.modelCollection != null)
		{
			Blocksworld.modelCollection.RefreshScarcity();
		}
		if (Blocksworld.buildPanel != null)
		{
			Blocksworld.UpdateTiles();
			Blocksworld.buildPanel.UpdateGestureRecognizer(Blocksworld.recognizer);
		}
		if (Scarcity.OnInventoryChange != null)
		{
			Scarcity.OnInventoryChange();
		}
	}

	public static void RefreshScarcity(BlocksInventory blocksInventory, bool addImplicitGafs)
	{
		globalInventory = ParseScarcityData(blocksInventory);
		if (addImplicitGafs)
		{
			HashSet<Type> allRegisteredTypes = PredicateRegistry.GetAllRegisteredTypes();
			foreach (Type item in allRegisteredTypes)
			{
				List<GAF> implicitlyUnlockedGAFs = GetImplicitlyUnlockedGAFs(item);
				foreach (GAF item2 in implicitlyUnlockedGAFs)
				{
					if (!globalInventory.ContainsKey(item2))
					{
						globalInventory[item2] = -1;
					}
				}
			}
		}
		ResetInventory();
		UpdateInventory();
	}

	public static List<GAF> GetImplicitlyUnlockedGAFs(Type type)
	{
		List<GAF> list = new List<GAF>();
		if (type == typeof(Block))
		{
			return list;
		}
		List<Predicate> list2 = PredicateRegistry.ForType(type, includeBaseTypes: false);
		foreach (Predicate item in list2)
		{
			List<BlockItem> list3 = BlockItem.FindByGafPredicateName(item.Name);
			foreach (BlockItem item2 in list3)
			{
				list.Add(new GAF(item2));
			}
		}
		return list;
	}

	public static Dictionary<GAF, int> ParseScarcityData(JObject scarcityData)
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		foreach (JObject item in scarcityData.ArrayValue)
		{
			GAF gAF = GAF.FromJSON(item[0], nullOnFailure: true);
			if (gAF == null)
			{
				BWLog.Warning("Could not find GAF " + gAF?.ToString() + " when refreshing scarcity");
				continue;
			}
			JObject jObject = item[1];
			if (jObject.StringValue == "infinity")
			{
				dictionary[gAF] = -1;
			}
			else
			{
				dictionary[gAF] = Mathf.Max(0, (int)jObject);
			}
		}
		return dictionary;
	}

	public static Dictionary<GAF, int> ParseScarcityData(BlocksInventory blocksInventory)
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		foreach (int blockItemId in blocksInventory.BlockItemIds)
		{
			if (BlockItem.Exists(blockItemId))
			{
				BlockItem blockItem = BlockItem.FindByID(blockItemId);
				if (blockItem != null)
				{
					GAF key = new GAF(blockItem);
					dictionary[key] = blocksInventory.CountOrMinusOneIfInfinityOf(blockItemId);
				}
			}
		}
		return dictionary;
	}

	public static void PrintInventory(string name, Dictionary<GAF, int> inv)
	{
		BWLog.Info(name);
		foreach (GAF key in inv.Keys)
		{
			BWLog.Info(string.Concat("  ", key, ": ", inv[key]));
		}
	}

	public static void UpdateInventory(bool updateTiles = true, Dictionary<GAF, int> changes = null)
	{
		if (worldGAFUsage == null || inventory == null || (Tutorial.state != TutorialState.None && Tutorial.mode == TutorialMode.StepByStep))
		{
			return;
		}
		Dictionary<GAF, int> d = CalculateWorldGafUsage();
		Dictionary<GAF, int> dictionary = CompareGafUsages(worldGAFUsage, d);
		bool flag = false;
		foreach (GAF key in dictionary.Keys)
		{
			int num = dictionary[key];
			GAF normalizedGaf = GetNormalizedGaf(key);
			if (inventory.ContainsKey(normalizedGaf) && inventory[normalizedGaf] != -1)
			{
				inventory[normalizedGaf] += num;
				if (inventory[normalizedGaf] < 0)
				{
					inventory[normalizedGaf] = 0;
				}
				if (changes != null)
				{
					changes[normalizedGaf] = num;
				}
			}
			if (num != 0)
			{
				flag = true;
			}
		}
		worldGAFUsage = d;
		if (Blocksworld.modelCollection != null)
		{
			Blocksworld.modelCollection.RefreshScarcity();
		}
		if (updateTiles && flag)
		{
			Blocksworld.UpdateTiles();
		}
		if (Scarcity.OnInventoryChange != null)
		{
			Scarcity.OnInventoryChange();
		}
	}

	public static HashSet<string> GetUniqueBlockNames(Dictionary<GAF, int> gafUsageDict)
	{
		Dictionary<string, HashSet<string>> uniqueBlockMap = Blocksworld.GetUniqueBlockMap();
		HashSet<string> hashSet = new HashSet<string>();
		if (gafUsageDict == null || gafUsageDict.Count == 0)
		{
			return hashSet;
		}
		foreach (GAF key in gafUsageDict.Keys)
		{
			if (key.Predicate.ArgTypes.Length != 0 && key.Predicate.ArgTypes[0] == typeof(string))
			{
				string text = (string)key.Args[0];
				if (uniqueBlockMap.ContainsKey(text))
				{
					hashSet.Add(text);
				}
			}
		}
		return hashSet;
	}
}
