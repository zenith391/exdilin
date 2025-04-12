using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Blocks;
using Gestures;
using SimpleJSON;
using UnityEngine;

// Token: 0x02000278 RID: 632
public class Scarcity
{
	// Token: 0x06001D69 RID: 7529 RVA: 0x000D1058 File Offset: 0x000CF458
	public static void ReadScarcityInfo()
	{
		Scarcity.scarcityInfos = new Dictionary<string, BlockScarcityInfo>();
		TextAsset textAsset = Resources.Load("BlockInfoList") as TextAsset;
		if (textAsset == null)
		{
			BWLog.Error("Unable to load BlockInfoList");
		}
		JObject jobject = JSONDecoder.Decode(textAsset.text);
		List<JObject> arrayValue = jobject["block-infos"].ArrayValue;
		foreach (JObject jObj in arrayValue)
		{
			BlockScarcityInfo blockScarcityInfo = BlockScarcityInfo.Read(jObj);
			Scarcity.scarcityInfos[blockScarcityInfo.blockName] = blockScarcityInfo;
		}
	}

	// Token: 0x06001D6A RID: 7530 RVA: 0x000D1114 File Offset: 0x000CF514
	private static bool TryGetBlockMeshScarcityInfo(string blockType, int meshIndex, out BlockMeshScarcityInfo info)
	{
		BlockScarcityInfo blockScarcityInfo;
		if (Scarcity.scarcityInfos.TryGetValue(blockType, out blockScarcityInfo) && meshIndex < blockScarcityInfo.meshInfos.Count)
		{
			info = blockScarcityInfo.meshInfos[meshIndex];
			return true;
		}
		info = null;
		return false;
	}

	// Token: 0x06001D6B RID: 7531 RVA: 0x000D1158 File Offset: 0x000CF558
	public static string DefaultTexture(string blockType, int meshIndex)
	{
		BlockMeshScarcityInfo blockMeshScarcityInfo;
		if (Scarcity.TryGetBlockMeshScarcityInfo(blockType, meshIndex, out blockMeshScarcityInfo))
		{
			return blockMeshScarcityInfo.defaultTexture;
		}
		return "Plain";
	}

	// Token: 0x06001D6C RID: 7532 RVA: 0x000D1180 File Offset: 0x000CF580
	public static string DefaultPaint(string blockType, int meshIndex)
	{
		BlockMeshScarcityInfo blockMeshScarcityInfo;
		if (Scarcity.TryGetBlockMeshScarcityInfo(blockType, meshIndex, out blockMeshScarcityInfo))
		{
			return blockMeshScarcityInfo.defaultPaint;
		}
		return "Yellow";
	}

	// Token: 0x06001D6D RID: 7533 RVA: 0x000D11A8 File Offset: 0x000CF5A8
	public static string[] DefaultPaints(string blockType)
	{
		List<string> list = new List<string>();
		BlockScarcityInfo blockScarcityInfo;
		if (Scarcity.scarcityInfos.TryGetValue(blockType, out blockScarcityInfo))
		{
			foreach (BlockMeshScarcityInfo blockMeshScarcityInfo in blockScarcityInfo.meshInfos)
			{
				list.Add(blockMeshScarcityInfo.defaultPaint);
			}
		}
		if (list.Count == 0)
		{
			list.Add("Yellow");
		}
		return list.ToArray();
	}

	// Token: 0x06001D6E RID: 7534 RVA: 0x000D1240 File Offset: 0x000CF640
	public static string[] DefaultTextures(string blockType)
	{
		List<string> list = new List<string>();
		BlockScarcityInfo blockScarcityInfo;
		if (Scarcity.scarcityInfos.TryGetValue(blockType, out blockScarcityInfo))
		{
			foreach (BlockMeshScarcityInfo blockMeshScarcityInfo in blockScarcityInfo.meshInfos)
			{
				list.Add(blockMeshScarcityInfo.defaultTexture);
			}
		}
		if (list.Count == 0)
		{
			list.Add("Plain");
		}
		return list.ToArray();
	}

	// Token: 0x06001D6F RID: 7535 RVA: 0x000D12D8 File Offset: 0x000CF6D8
	public static bool FreeTexture(string blockType, int meshIndex, string texture)
	{
		BlockMeshScarcityInfo blockMeshScarcityInfo;
		return blockType != null && texture != null && Scarcity.TryGetBlockMeshScarcityInfo(blockType, meshIndex, out blockMeshScarcityInfo) && blockMeshScarcityInfo.freeTextures.Contains(texture);
	}

	// Token: 0x06001D70 RID: 7536 RVA: 0x000D1310 File Offset: 0x000CF710
	public static bool FreePaint(string blockType, int meshIndex, string paint)
	{
		BlockMeshScarcityInfo blockMeshScarcityInfo;
		return blockType != null && paint != null && Scarcity.TryGetBlockMeshScarcityInfo(blockType, meshIndex, out blockMeshScarcityInfo) && blockMeshScarcityInfo.freePaints.Contains(paint);
	}

	// Token: 0x06001D71 RID: 7537 RVA: 0x000D1348 File Offset: 0x000CF748
	public static HashSet<string> FreePaints(string blockType, int meshIndex)
	{
		BlockMeshScarcityInfo blockMeshScarcityInfo;
		if (Scarcity.TryGetBlockMeshScarcityInfo(blockType, meshIndex, out blockMeshScarcityInfo))
		{
			return blockMeshScarcityInfo.freePaints;
		}
		return new HashSet<string>
		{
			Scarcity.DefaultPaint(blockType, meshIndex)
		};
	}

	// Token: 0x06001D72 RID: 7538 RVA: 0x000D1380 File Offset: 0x000CF780
	public static bool FreeSfx(string blockType, string sfx)
	{
		BlockScarcityInfo blockScarcityInfo;
		return blockType != null && sfx != null && Scarcity.scarcityInfos.TryGetValue(blockType, out blockScarcityInfo) && blockScarcityInfo.freeSfxs.Contains(sfx);
	}

	// Token: 0x06001D73 RID: 7539 RVA: 0x000D13BC File Offset: 0x000CF7BC
	public static HashSet<string> GetShapeCategories(string blockType)
	{
		BlockScarcityInfo blockScarcityInfo;
		if (Scarcity.scarcityInfos.TryGetValue(blockType, out blockScarcityInfo))
		{
			return blockScarcityInfo.shapeCategories;
		}
		return new HashSet<string>();
	}

	// Token: 0x06001D74 RID: 7540 RVA: 0x000D13E7 File Offset: 0x000CF7E7
	public static void SetTextureOriginal(string rewritten, string original)
	{
		Scarcity.textureOriginals[rewritten] = original;
	}

	// Token: 0x06001D75 RID: 7541 RVA: 0x000D13F8 File Offset: 0x000CF7F8
	private static HashSet<Predicate> GetNotCountInStepByStepPredicates()
	{
		if (Scarcity.notCountInStepByStepPredicates == null)
		{
			Scarcity.notCountInStepByStepPredicates = new HashSet<Predicate>
			{
				Block.predicateThen,
				Block.predicateLocked,
				Block.predicateUnlocked,
				Block.predicateTutorialCreateBlockHint,
				Block.predicateTutorialRemoveBlockHint
			};
		}
		return Scarcity.notCountInStepByStepPredicates;
	}

	// Token: 0x06001D76 RID: 7542 RVA: 0x000D145C File Offset: 0x000CF85C
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
					}
					else
					{
						object[] args2 = gaf2.Args;
						if (predicate2 == Block.predicatePaintTo)
						{
							string stringArg = Util.GetStringArg(args2, 0, "Yellow");
							int intArg = Util.GetIntArg(args2, 1, 0);
							if (!Scarcity.FreePaint(blockType, intArg, stringArg))
							{
								GAF normalizedGaf = Scarcity.GetNormalizedGaf(gaf2, false);
								hashSet.Add(normalizedGaf);
							}
						}
						else if (predicate2 == Block.predicateTextureTo)
						{
							string stringArg2 = Util.GetStringArg(args2, 0, "Plain");
							int intArg2 = Util.GetIntArg(args2, 2, 0);
							if (!Scarcity.FreeTexture(blockType, intArg2, stringArg2))
							{
								GAF normalizedGaf2 = Scarcity.GetNormalizedGaf(gaf2, false);
								hashSet.Add(normalizedGaf2);
							}
						}
					}
				}
			}
			foreach (GAF key in hashSet)
			{
				int num = 0;
				result.TryGetValue(key, out num);
				result[key] = num + 1;
			}
		}
	}

	// Token: 0x06001D77 RID: 7543 RVA: 0x000D1668 File Offset: 0x000CFA68
	public static Dictionary<GAF, int> GetNormalizedInventoryUse(List<List<List<Tile>>> tiles, WorldType worldType, bool includeLocked = false)
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		Scarcity.GetInventoryUse(tiles, worldType, dictionary, includeLocked);
		Dictionary<GAF, int> dictionary2 = new Dictionary<GAF, int>();
		foreach (KeyValuePair<GAF, int> keyValuePair in dictionary)
		{
			GAF normalizedGaf = Scarcity.GetNormalizedGaf(keyValuePair.Key, false);
			if (BlockItem.FindByGafPredicateNameAndArguments(normalizedGaf.Predicate.Name, normalizedGaf.Args) != null)
			{
				int value = keyValuePair.Value;
				int num = 0;
				dictionary2.TryGetValue(normalizedGaf, out num);
				dictionary2[normalizedGaf] = value + num;
			}
		}
		Scarcity.GetScriptColorAndTextureUse(tiles, dictionary2);
		return dictionary2;
	}

	// Token: 0x06001D78 RID: 7544 RVA: 0x000D1730 File Offset: 0x000CFB30
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
				int num;
				if (!dictionary.TryGetValue(key, out num))
				{
					num = 0;
				}
				dictionary[key] = num + 1;
				if (BlockGroups.IsMainGroupBlock(tile))
				{
					dictionary2[key] = i;
				}
			}
			Tile tile2 = BlockGroups.FindGroupTile(tiles2, "teleport-volume");
			if (tile2 != null)
			{
				int key2 = BlockGroups.GroupId(tile2);
				int num2;
				if (!dictionary.TryGetValue(key2, out num2))
				{
					num2 = 0;
				}
				dictionary[key2] = num2 + 1;
				if (BlockGroups.IsMainGroupBlock(tile2))
				{
					dictionary2[key2] = i;
				}
			}
		}
		foreach (KeyValuePair<int, int> keyValuePair in dictionary2)
		{
			int key3 = keyValuePair.Key;
			int value = keyValuePair.Value;
			array[value] = dictionary[key3];
		}
		for (int j = 0; j < tiles.Count; j++)
		{
			Scarcity.GetInventoryUse(tiles[j], worldType, array[j], result, includeLocked);
		}
	}

	// Token: 0x06001D79 RID: 7545 RVA: 0x000D18AC File Offset: 0x000CFCAC
	public static void GetInventoryUse(List<List<Tile>> tiles, WorldType worldType, int groupSize, Dictionary<GAF, int> result, bool includeLocked = false)
	{
		bool flag = tiles.Count > 1 && tiles[1].Count > 1 && tiles[1][1].IsLocked();
		if (flag && !includeLocked)
		{
			return;
		}
		bool flag2 = worldType != WorldType.ForComplexityCalculation;
		for (int i = 0; i < tiles.Count; i++)
		{
			bool flag3 = i == 0;
			List<Tile> list = tiles[i];
			bool flag4 = false;
			for (int j = 0; j < list.Count; j++)
			{
				Tile tile = list[j];
				if (tile.gaf.Predicate == Block.predicateHideTileRow)
				{
					flag4 = true;
					break;
				}
			}
			if (!flag4)
			{
				string text = null;
				for (int k = 0; k < list.Count; k++)
				{
					Tile tile2 = list[k];
					GAF gaf = tile2.gaf;
					Predicate predicate = gaf.Predicate;
					if (predicate == Block.predicateHideNextTile)
					{
						k++;
					}
					else
					{
						object[] args = gaf.Args;
						if (predicate == Block.predicateCreate)
						{
							text = (string)args[0];
							if (!ProfileBlocksterUtils.IsProfileBlockType(text))
							{
								if (groupSize == 0)
								{
									Scarcity.UpdateWorldGAFUsage(gaf, 1, result, true);
								}
								else
								{
									Scarcity.UpdateWorldGAFUsage(new GAF(Block.predicateCreate, new object[]
									{
										(string)args[0] + " x" + groupSize
									}), 1, result, true);
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
							Scarcity.UpdateWorldGAFUsage(new GAF(BlockAnimatedCharacter.predicateReplaceLimb, new object[]
							{
								text2
							}), 1, result, true);
						}
						if (worldType == WorldType.StepByStepTutorial)
						{
							if (!Scarcity.GetNotCountInStepByStepPredicates().Contains(predicate))
							{
								EditableTileParameter editableParameter = gaf.Predicate.EditableParameter;
								GAF primitiveGafFor = TileToggleChain.GetPrimitiveGafFor(gaf);
								if (primitiveGafFor != null)
								{
									Scarcity.UpdateWorldGAFUsage(primitiveGafFor, 1, result, true);
								}
								else if (editableParameter != null)
								{
									GAF gaf2 = gaf.Clone();
									gaf2.Args[editableParameter.parameterIndex] = null;
									Tile tile3 = Blocksworld.buildPanel.FindTileMatching(gaf2);
									if (tile3 != null)
									{
										Scarcity.UpdateWorldGAFUsage(tile3.gaf, 1, result, true);
									}
									else
									{
										GAF gaf3 = gaf.Clone();
										GAF gaf4 = new GAF(gaf3.Predicate, gaf3.Predicate.ExtendArguments(gaf3.Args, true));
										Scarcity.UpdateWorldGAFUsage(gaf4, 1, result, true);
									}
								}
								else if (predicate == BlockMaster.predicateSetEnvEffect)
								{
									string text3 = BlockSky.EnvEffectToTexture(Util.GetStringArg(args, 0, "Clear"));
									Scarcity.UpdateWorldGAFUsage(new GAF(Block.predicateTextureTo, new object[]
									{
										text3,
										Vector3.zero
									}), 1, result, true);
								}
								else if (predicate == BlockMaster.predicatePaintSkyTo)
								{
									Scarcity.UpdateWorldGAFUsage(new GAF(Block.predicatePaintTo, new object[]
									{
										Util.GetStringArg(args, 0, "Yellow")
									}), 1, result, true);
								}
								else if (predicate == Block.predicateTutorialPaintExistingBlock)
								{
									Scarcity.UpdateWorldGAFUsage(new GAF(Block.predicatePaintTo, new object[]
									{
										Util.GetStringArg(args, 1, "Yellow")
									}), 1, result, true);
								}
								else if (predicate == Block.predicateTutorialTextureExistingBlock)
								{
									Scarcity.UpdateWorldGAFUsage(new GAF(Block.predicateTextureTo, new object[]
									{
										Util.GetStringArg(args, 1, "Plain"),
										Vector3.zero
									}), 1, result, true);
								}
								else if (predicate == Block.predicateTextureTo)
								{
									Scarcity.UpdateWorldGAFUsage(new GAF(Block.predicateTextureTo, new object[]
									{
										Util.GetStringArg(args, 0, "Plain"),
										Vector3.zero
									}), 1, result, true);
								}
								else if (predicate == Block.predicatePaintTo)
								{
									Scarcity.UpdateWorldGAFUsage(new GAF(Block.predicatePaintTo, new object[]
									{
										Util.GetStringArg(args, 0, "Yellow")
									}), 1, result, true);
								}
								else if (!flag3)
								{
									Scarcity.UpdateWorldGAFUsage(gaf, 1, result, true);
								}
							}
						}
						else
						{
							if (flag3 && flag2)
							{
								if (predicate == Block.predicatePaintTo)
								{
									string stringArg = Util.GetStringArg(args, 0, "Yellow");
									int intArg = Util.GetIntArg(args, 1, 0);
									if (flag3 && !Scarcity.FreePaint(text, intArg, stringArg))
									{
										Scarcity.UpdateWorldGAFUsage(gaf, 1, result, true);
									}
								}
								else if (predicate == Block.predicateTextureTo)
								{
									string stringArg2 = Util.GetStringArg(args, 0, "Plain");
									int intArg2 = Util.GetIntArg(args, 2, 0);
									if (flag3 && !Scarcity.FreeTexture(text, intArg2, stringArg2))
									{
										Scarcity.UpdateWorldGAFUsage(gaf, 1, result, true);
									}
								}
							}
							if (predicate == Block.predicatePlaySoundDurational)
							{
								string stringArg3 = Util.GetStringArg(args, 0, string.Empty);
								if (!Scarcity.FreeSfx(text, stringArg3))
								{
									Scarcity.UpdateWorldGAFUsage(gaf, 1, result, true);
								}
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06001D7A RID: 7546 RVA: 0x000D1E04 File Offset: 0x000D0204
	public static void StepInventoryScales()
	{
		if (Scarcity.ScarcityBadgesShowing() && Scarcity.GetScarcityHighlightGafs() != null)
		{
			List<GAF> list = new List<GAF>(Scarcity.inventoryScales.Keys);
			foreach (GAF key in list)
			{
				float num = Scarcity.inventoryScales[key];
				num *= 0.93f;
				if (num < 1f)
				{
					num = 1f;
				}
				Scarcity.inventoryScales[key] = num;
			}
			History.RemoveHighlightsIfNecessary();
		}
	}

	// Token: 0x06001D7B RID: 7547 RVA: 0x000D1EB0 File Offset: 0x000D02B0
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

	// Token: 0x06001D7C RID: 7548 RVA: 0x000D1EF1 File Offset: 0x000D02F1
	private static bool ScarcityBadgesShowing()
	{
		return (Scarcity.inventory != null && Blocksworld.CurrentState != State.Play) || RewardVisualization.rewardAnimationRunning;
	}

	// Token: 0x06001D7D RID: 7549 RVA: 0x000D1F10 File Offset: 0x000D0310
	public static void PaintScarcityBadges()
	{
		if (Scarcity.ScarcityBadgesShowing() && Scarcity.inventory != null)
		{
			HudMeshStyle inventoryStyle = HudMeshOnGUI.dataSource.inventoryStyle;
			HashSet<GAF> scarcityHighlightGafs = Scarcity.GetScarcityHighlightGafs();
			BuildPanel buildPanel = Blocksworld.buildPanel;
			int num = 0;
			TabBarTabId selectedTab = buildPanel.GetTabBar().SelectedTab;
			List<Tile> tilesInTab = buildPanel.GetTilesInTab(selectedTab);
			if (tilesInTab != null)
			{
				for (int i = 0; i < tilesInTab.Count; i++)
				{
					Tile tile = tilesInTab[i];
					if (tile.IsShowing())
					{
						GAF gaf = tile.gaf;
						Scarcity.PaintBadge(tile, gaf, scarcityHighlightGafs, ref num, true, inventoryStyle);
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
						Scarcity.PaintBadge(tilesInTab2[j], tilesInTab2[j].gaf, scarcityHighlightGafs, ref num, true, inventoryStyle);
					}
				}
			}
		}
	}

	// Token: 0x06001D7E RID: 7550 RVA: 0x000D2040 File Offset: 0x000D0440
	private static bool PaintBadge(Tile tile, GAF gaf, HashSet<GAF> highlightGafs, ref int labelIndex, bool quickSelectUIVisible, HudMeshStyle inventoryStyle)
	{
		if (!tile.IsShowing())
		{
			return false;
		}
		int num = -1;
		float num2 = 1f;
		bool flag = Scarcity.ShouldDrawBadge(gaf, highlightGafs, out num, out num2, false);
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
				num = Blocksworld.clipboard.AvailableModelCount(model, null, null, true);
			}
		}
		if (num > 0 && !tile.IsEnabled())
		{
			tile.Enable(true);
		}
		else if (num == 0 && tile.IsEnabled())
		{
			tile.Enable(false);
		}
		string text = num.ToString();
		Vector2 centerPos = new Vector2(tile.tileObject.GetPosition().x + 70f * NormalizedScreen.pixelScale, (float)NormalizedScreen.height - tile.tileObject.GetPosition().y - 70f * NormalizedScreen.pixelScale);
		Rect rect = Scarcity.RectForLabel(text, centerPos, num2);
		if (quickSelectUIVisible)
		{
			flag = (centerPos.y < (float)NormalizedScreen.height - Blocksworld.UI.QuickSelect.GetHeight() - rect.height);
		}
		if (flag)
		{
			HudMeshOnGUI.Label(Scarcity.scarcityLabels, labelIndex, rect, text, inventoryStyle);
			if (Scarcity.scarcityLabels.Count > labelIndex && Scarcity.scarcityLabels[labelIndex] != null)
			{
				Scarcity.scarcityLabels[labelIndex].transform.localScale = new Vector3(num2, num2, 1f);
			}
			labelIndex++;
			return true;
		}
		return false;
	}

	// Token: 0x06001D7F RID: 7551 RVA: 0x000D21F4 File Offset: 0x000D05F4
	private static bool ShouldDrawBadge(GAF gaf, HashSet<GAF> highlightGafs, out int inventoryCount, out float scale, bool debug = false)
	{
		inventoryCount = -1;
		scale = 1f;
		if (gaf == null)
		{
			return false;
		}
		if (Scarcity.inventory != null && Scarcity.inventory.ContainsKey(gaf))
		{
			inventoryCount = Scarcity.inventory[gaf];
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
		if (!Scarcity.inventoryScales.TryGetValue(gaf, out scale))
		{
			scale = 1f;
		}
		return true;
	}

	// Token: 0x06001D80 RID: 7552 RVA: 0x000D2288 File Offset: 0x000D0688
	public static Rect RectForLabel(string labelText, Vector2 centerPos, float scale)
	{
		Vector2 a = new Vector2(32f, 32f) * NormalizedScreen.pixelScale;
		float num = 5f * NormalizedScreen.scale;
		float scale2 = NormalizedScreen.scale;
		Vector2 vector = a * scale2;
		if (labelText.Length > 1)
		{
			vector += new Vector2(num * (float)labelText.Length - 1f, 0f) * scale2;
		}
		Vector2 vector2 = (vector * scale - vector) / (2f * scale2);
		Vector2 vector3 = new Vector2((centerPos.x - a.x / 2f - vector2.x) * scale2, (centerPos.y - a.y / 2f - vector2.y) * scale2);
		Rect result = new Rect(vector3.x, vector3.y, vector.x, vector.y);
		return result;
	}

	// Token: 0x06001D81 RID: 7553 RVA: 0x000D2380 File Offset: 0x000D0780
	public static string GetNormalizedTexture(string texture)
	{
		if (Scarcity.textureOriginals.ContainsKey(texture))
		{
			return Scarcity.textureOriginals[texture];
		}
		for (int i = 0; i < Scarcity.normalizedPostfixes.Length; i++)
		{
			int num = texture.IndexOf(Scarcity.normalizedPostfixes[i]);
			if (num >= 0)
			{
				texture = texture.Substring(0, num);
				break;
			}
		}
		return texture;
	}

	// Token: 0x06001D82 RID: 7554 RVA: 0x000D23E8 File Offset: 0x000D07E8
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
				return new GAF(name, new object[]
				{
					Scarcity.GetNormalizedTexture(texture),
					Vector3.zero,
					Util.GetIntArg(gaf.Args, 2, 0)
				});
			}
			return new GAF(name, new object[]
			{
				Scarcity.GetNormalizedTexture(texture),
				Vector3.zero
			});
		}
		case "Block.PaintTo":
		{
			string text = (string)args[0];
			if (keepPaintAndTextureIndices)
			{
				return new GAF(name, new object[]
				{
					text,
					Util.GetIntArg(gaf.Args, 1, 0)
				});
			}
			return new GAF(name, new object[]
			{
				text
			});
		}
		case "Block.Create":
		{
			string text2 = (string)args[0];
			if (text2 != null)
			{
				if (text2 == "Volume")
				{
					break;
				}
				if (text2 == "Anim Character Avatar")
				{
					return new GAF(name, new object[]
					{
						"Character Avatar"
					});
				}
			}
			return new GAF(name, new object[]
			{
				text2
			});
		}
		case "AnimCharacter.ReplaceBodyPart":
		{
			string text3 = (string)args[0];
			if (text3.EndsWith(" Left"))
			{
				text3 = text3.Remove(text3.Length - 5, 5);
			}
			else if (text3.EndsWith(" Right"))
			{
				text3 = text3.Remove(text3.Length - 6, 6);
			}
			return new GAF(name, new object[]
			{
				text3
			});
		}
		case "Block.SendCustomSignal":
		case "Block.SendCustomSignalModel":
			return new GAF(name, new object[]
			{
				"*",
				1f
			});
		case "BlockVariable.Int":
		case "Variable.CustomInt":
			return new GAF(name, new object[]
			{
				"*",
				0
			});
		}
		return gaf;
	}

	// Token: 0x06001D83 RID: 7555 RVA: 0x000D2684 File Offset: 0x000D0A84
	public static bool IsFreeTexture(string texture, int meshIndex, Block b)
	{
		if (texture != null)
		{
			if (texture == "Plain")
			{
				return true;
			}
			if (texture == "Clothing Underwear")
			{
				return b is BlockCharacter && meshIndex == 1;
			}
		}
		if (b.BlockType() == "Sky UV" && texture != null)
		{
			if (texture == "Cloudy Sky" || texture == "Space Sky")
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001D84 RID: 7556 RVA: 0x000D2714 File Offset: 0x000D0B14
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
				PredicateRegistry.ByName("Block.TutorialCreateBlockHint", true)
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
			if (!flag)
			{
				bool flag2 = false;
				for (int k = 0; k < list2.Count; k++)
				{
					Tile tile2 = list2[k];
					if (flag2)
					{
						flag2 = false;
					}
					else
					{
						GAF gaf2 = tile2.gaf;
						if (gaf2.Predicate == Block.predicatePlaySoundDurational)
						{
							object[] args = gaf2.Args;
							GAF gaf3 = new GAF(gaf2.Predicate.Name, args);
							if (!Block.IsDefaultSfx(blockType, (string)args[0]))
							{
								Scarcity.UpdateWorldGAFUsage(gaf3, 1, result, true);
								if (verbose)
								{
									string text2 = text;
									text = string.Concat(new object[]
									{
										text2,
										b.BlockType(),
										": ",
										gaf3,
										"\n"
									});
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
								Scarcity.UpdateWorldGAFUsage(Scarcity.GetNormalizedGaf(gaf2, false), 1, result, true);
							}
						}
					}
				}
			}
		}
		bool[] array = new bool[b.subMeshGameObjects.Count + 1];
		for (int l = 0; l < list.Count; l++)
		{
			Tile tile3 = list[l];
			GAF gaf4 = tile3.gaf;
			Predicate predicate = gaf4.Predicate;
			object[] args2 = gaf4.Args;
			if (predicate == Block.predicateTextureTo || predicate == BlockSky.predicateEnvEffect)
			{
				bool flag3 = predicate == Block.predicateTextureTo;
				string text3 = (string)args2[0];
				string texture = (!flag3) ? BlockSky.EnvEffectToTexture(text3) : text3;
				int num = (!flag3) ? 0 : ((args2.Length <= 2) ? 0 : ((int)args2[2]));
				if (num < array.Length && !array[num] && !Scarcity.IsFreeTexture(texture, num, b) && !b.IsDefaultTexture(texture, num))
				{
					GAF gaf5 = new GAF(Block.predicateTextureTo, new object[]
					{
						Scarcity.GetNormalizedTexture(texture),
						Vector3.zero
					});
					Scarcity.UpdateWorldGAFUsage(gaf5, 1, result, true);
					if (verbose)
					{
						string text2 = text;
						text = string.Concat(new object[]
						{
							text2,
							b.BlockType(),
							": ",
							gaf5,
							"\n"
						});
					}
					array[num] = true;
				}
			}
			else if (predicate == Block.predicatePaintTo)
			{
				string text4 = (string)args2[0];
				if (!b.IsDefaultPaint(gaf4))
				{
					GAF gaf6 = new GAF(predicate, new object[]
					{
						text4
					});
					Scarcity.UpdateWorldGAFUsage(gaf6, 1, result, true);
					if (verbose)
					{
						string text2 = text;
						text = string.Concat(new object[]
						{
							text2,
							b.BlockType(),
							": ",
							gaf6,
							"\n"
						});
					}
				}
			}
			else if (predicate == Block.predicateCreate)
			{
				gaf4 = Scarcity.GetNormalizedGaf(gaf4, false);
				string text5 = (string)args2[0];
				if (!ProfileBlocksterUtils.IsProfileBlockType(text5))
				{
					if (text5 != null)
					{
						if (text5 == "Volume")
						{
							goto IL_51E;
						}
					}
					if (blockGrouped != null)
					{
						gaf4 = blockGrouped.GetIconGaf();
					}
					if (Blocksworld.buildPanel.IsGAFInBuildPanel(gaf4) || Blocksworld.GetUniqueBlockMap().ContainsKey(text5))
					{
						Scarcity.UpdateWorldGAFUsage(gaf4, 1, result, true);
						if (verbose)
						{
							string text2 = text;
							text = string.Concat(new object[]
							{
								text2,
								b.BlockType(),
								": ",
								gaf4,
								"\n"
							});
						}
					}
				}
			}
			IL_51E:;
		}
		if (verbose && text.Length > 0)
		{
			BWLog.Info(text);
		}
	}

	// Token: 0x06001D85 RID: 7557 RVA: 0x000D2C6C File Offset: 0x000D106C
	public static Dictionary<GAF, int> CalculateWorldGafUsage(bool verbose = false, bool forStepByStepTutorials = false)
	{
		List<Block> list = BWSceneManager.AllBlocks();
		Dictionary<GAF, int> result = new Dictionary<GAF, int>();
		for (int i = 0; i < list.Count; i++)
		{
			Scarcity.CalculateBlockGafUsage(list[i], result, verbose, forStepByStepTutorials);
		}
		return result;
	}

	// Token: 0x06001D86 RID: 7558 RVA: 0x000D2CAC File Offset: 0x000D10AC
	public static bool IsRelevantGAF(string blockType, GAF gaf, bool firstRow)
	{
		object[] args = gaf.Args;
		string text = (args.Length <= 0 || !(args[0] is string)) ? string.Empty : ((string)args[0]);
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
			flag |= (gaf.Predicate == Block.predicateTextureTo && !Scarcity.FreeTexture(blockType, meshIndex, text));
			flag |= (gaf.Predicate == Block.predicatePaintTo && !Scarcity.FreePaint(blockType, meshIndex, text));
			flag |= (gaf.Predicate == Block.predicateCreate && text != "Volume" && !ProfileBlocksterUtils.IsProfileBlockType(text) && Blocksworld.buildPanel.IsGAFInBuildPanel(gaf));
		}
		flag |= (gaf.Predicate == Block.predicatePlaySoundDurational && !Scarcity.FreeSfx(blockType, text));
		return flag | gaf.Predicate == BlockAnimatedCharacter.predicateReplaceLimb;
	}

	// Token: 0x06001D87 RID: 7559 RVA: 0x000D2DD9 File Offset: 0x000D11D9
	public static void CompareIncrementalAndAbsoluteGafUsages()
	{
		Scarcity.CompareGafUsages(Scarcity.CalculateWorldGafUsage(false, false), Scarcity.worldGAFUsage, "Absolute", "Incremental");
	}

	// Token: 0x06001D88 RID: 7560 RVA: 0x000D2DF8 File Offset: 0x000D11F8
	public static Dictionary<GAF, int> CompareGafUsages(Dictionary<GAF, int> d1, Dictionary<GAF, int> d2, string n1 = "d1", string n2 = "d2")
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		foreach (GAF gaf in d1.Keys)
		{
			int num = d1[gaf];
			if (d2.ContainsKey(gaf))
			{
				int num2 = d2[gaf];
				if (num != num2)
				{
					Scarcity.UpdateWorldGAFUsage(gaf, num - num2, dictionary, false);
				}
			}
			else
			{
				Scarcity.UpdateWorldGAFUsage(gaf, num, dictionary, false);
			}
		}
		foreach (GAF gaf2 in d2.Keys)
		{
			if (!d1.ContainsKey(gaf2))
			{
				int num3 = d2[gaf2];
				Scarcity.UpdateWorldGAFUsage(gaf2, -num3, dictionary, false);
			}
		}
		return dictionary;
	}

	// Token: 0x06001D89 RID: 7561 RVA: 0x000D2EFC File Offset: 0x000D12FC
	public static void UpdateScarcityBadges(HashSet<GAF> highlights, Dictionary<GAF, int> oldGafUsage, Dictionary<GAF, int> startInventory)
	{
		if (startInventory != null)
		{
			Dictionary<GAF, int> d = Scarcity.CalculateWorldGafUsage(false, false);
			Dictionary<GAF, int> dictionary = Scarcity.CompareGafUsages(oldGafUsage, d, "d1", "d2");
			foreach (GAF gaf in dictionary.Keys)
			{
				int num = dictionary[gaf];
				if (num != 0)
				{
					highlights.Add(gaf);
				}
			}
			foreach (GAF key in highlights)
			{
				if (startInventory.ContainsKey(key))
				{
					Scarcity.inventoryScales[key] = 1.5f;
				}
			}
		}
	}

	// Token: 0x06001D8A RID: 7562 RVA: 0x000D2FE8 File Offset: 0x000D13E8
	public static bool GetExistsInInventory(GAF gaf)
	{
		return Scarcity.inventory != null && Scarcity.inventory.ContainsKey(gaf);
	}

	// Token: 0x06001D8B RID: 7563 RVA: 0x000D3002 File Offset: 0x000D1402
	public static int GetInventoryCount(GAF gaf, bool zeroIfNotExist = false)
	{
		if (Scarcity.inventory != null && Scarcity.inventory.ContainsKey(gaf))
		{
			return Scarcity.inventory[gaf];
		}
		return (!zeroIfNotExist) ? -1 : 0;
	}

	// Token: 0x06001D8C RID: 7564 RVA: 0x000D3038 File Offset: 0x000D1438
	public static Dictionary<GAF, int> GetInventoryCopy()
	{
		if (Scarcity.inventory != null)
		{
			Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
			foreach (GAF key in Scarcity.inventory.Keys)
			{
				dictionary.Add(key, Scarcity.inventory[key]);
			}
			return dictionary;
		}
		return null;
	}

	// Token: 0x06001D8D RID: 7565 RVA: 0x000D30B8 File Offset: 0x000D14B8
	public static void UpdateWorldGAFUsage(GAF gaf, int change, Dictionary<GAF, int> usage = null, bool clampZero = true)
	{
		if (usage == null)
		{
			usage = Scarcity.worldGAFUsage;
		}
		if (!usage.ContainsKey(gaf))
		{
			usage[gaf] = 0;
		}
		usage[gaf] = ((!clampZero) ? (usage[gaf] + change) : Mathf.Max(0, usage[gaf] + change));
	}

	// Token: 0x06001D8E RID: 7566 RVA: 0x000D3110 File Offset: 0x000D1510
	public static string SaveWorldGAFUsage(bool verbose = false, bool insertLines = false)
	{
		Scarcity.worldGAFUsage = Scarcity.CalculateWorldGafUsage(verbose, false);
		return Scarcity.GetInventoryJSON(Scarcity.worldGAFUsage, verbose, insertLines);
	}

	// Token: 0x06001D8F RID: 7567 RVA: 0x000D312C File Offset: 0x000D152C
	public static string GetInventoryJSON(Dictionary<GAF, int> inv, bool verbose = false, bool insertLines = false)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jsonstreamEncoder = new JSONStreamEncoder(writer, 20);
		jsonstreamEncoder.BeginArray();
		foreach (KeyValuePair<GAF, int> keyValuePair in inv)
		{
			jsonstreamEncoder.BeginArray();
			keyValuePair.Key.ToJSONCompact(jsonstreamEncoder);
			jsonstreamEncoder.WriteNumber((long)keyValuePair.Value);
			jsonstreamEncoder.EndArray();
			if (insertLines)
			{
				jsonstreamEncoder.InsertNewline();
			}
		}
		jsonstreamEncoder.EndArray();
		return stringBuilder.ToString();
	}

	// Token: 0x06001D90 RID: 7568 RVA: 0x000D31E4 File Offset: 0x000D15E4
	public static string GetBlockIDInventoryString(Dictionary<GAF, int> inv, HashSet<Predicate> infinitePredicates = null)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		bool flag = true;
		foreach (KeyValuePair<GAF, int> keyValuePair in inv)
		{
			GAF normalizedGaf = Scarcity.GetNormalizedGaf(keyValuePair.Key, false);
			BlockItem blockItem = BlockItem.FindByGafPredicateNameAndArguments(normalizedGaf.Predicate.Name, normalizedGaf.Args);
			if (blockItem == null)
			{
				BWLog.Info("Couldn't find id for block: " + normalizedGaf.ToString());
			}
			else
			{
				int id = blockItem.Id;
				if (!flag)
				{
					stringBuilder.Append('|');
				}
				int value = keyValuePair.Value;
				int num = (infinitePredicates == null || !infinitePredicates.Contains(normalizedGaf.Predicate)) ? 0 : 1;
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
		}
		return stringBuilder.ToString();
	}

	// Token: 0x06001D91 RID: 7569 RVA: 0x000D333C File Offset: 0x000D173C
	public static Dictionary<GAF, int> ReadBlockIDInventoryString(string blockIDInventoryString)
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		int c = 0;
		StringBuilder sb = new StringBuilder(32768);
		List<char> separators = new List<char>
		{
			':',
			';',
			'|'
		};
		bool done = false;
		char currentSeparator = '|';
		Action action = delegate()
		{
			sb.Length = 0;
			while (c < blockIDInventoryString.Length && !separators.Contains(blockIDInventoryString[c]))
			{
				sb.Append(blockIDInventoryString[c]);
				c++;
			}
			done = (c >= blockIDInventoryString.Length);
			if (!done)
			{
				currentSeparator = blockIDInventoryString[c];
				c++;
			}
		};
		while (!done)
		{
			int num = 0;
			int num2 = 0;
			action();
			int num3;
			int.TryParse(sb.ToString(), out num3);
			action();
			int.TryParse(sb.ToString(), out num);
			if (currentSeparator != '|')
			{
				action();
				int.TryParse(sb.ToString(), out num2);
			}
			if (BlockItem.Exists(num3))
			{
				BlockItem blockItem = BlockItem.FindByID(num3);
				if (blockItem == null)
				{
					BWLog.Error("Couldn't find block with id: " + num3);
				}
				else
				{
					GAF key = new GAF(blockItem);
					int value = (num2 <= 0) ? num : -1;
					dictionary[key] = value;
				}
			}
		}
		return dictionary;
	}

	// Token: 0x06001D92 RID: 7570 RVA: 0x000D3480 File Offset: 0x000D1880
	public static void ResetInventory()
	{
		if (Scarcity.globalInventory != null)
		{
			Scarcity.inventory = new Dictionary<GAF, int>(Scarcity.globalInventory);
		}
		else
		{
			Scarcity.inventory = null;
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

	// Token: 0x06001D93 RID: 7571 RVA: 0x000D34F8 File Offset: 0x000D18F8
	public static void RefreshScarcity(BlocksInventory blocksInventory, bool addImplicitGafs)
	{
		Scarcity.globalInventory = Scarcity.ParseScarcityData(blocksInventory);
		if (addImplicitGafs)
		{
			HashSet<Type> allRegisteredTypes = PredicateRegistry.GetAllRegisteredTypes();
			foreach (Type type in allRegisteredTypes)
			{
				List<GAF> implicitlyUnlockedGAFs = Scarcity.GetImplicitlyUnlockedGAFs(type);
				foreach (GAF key in implicitlyUnlockedGAFs)
				{
					if (!Scarcity.globalInventory.ContainsKey(key))
					{
						Scarcity.globalInventory[key] = -1;
					}
				}
			}
		}
		Scarcity.ResetInventory();
		Scarcity.UpdateInventory(true, null);
	}

	// Token: 0x06001D94 RID: 7572 RVA: 0x000D35D4 File Offset: 0x000D19D4
	public static List<GAF> GetImplicitlyUnlockedGAFs(Type type)
	{
		List<GAF> list = new List<GAF>();
		if (type == typeof(Block))
		{
			return list;
		}
		List<Predicate> list2 = PredicateRegistry.ForType(type, false);
		foreach (Predicate predicate in list2)
		{
			List<BlockItem> list3 = BlockItem.FindByGafPredicateName(predicate.Name);
			foreach (BlockItem blockItem in list3)
			{
				list.Add(new GAF(blockItem));
			}
		}
		return list;
	}

	// Token: 0x06001D95 RID: 7573 RVA: 0x000D36A4 File Offset: 0x000D1AA4
	public static Dictionary<GAF, int> ParseScarcityData(JObject scarcityData)
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		foreach (JObject jobject in scarcityData.ArrayValue)
		{
			GAF gaf = GAF.FromJSON(jobject[0], true, true);
			if (gaf == null)
			{
				BWLog.Warning("Could not find GAF " + gaf + " when refreshing scarcity");
			}
			else
			{
				JObject jobject2 = jobject[1];
				if (jobject2.StringValue == "infinity")
				{
					dictionary[gaf] = -1;
				}
				else
				{
					dictionary[gaf] = Mathf.Max(0, (int)jobject2);
				}
			}
		}
		return dictionary;
	}

	// Token: 0x06001D96 RID: 7574 RVA: 0x000D3770 File Offset: 0x000D1B70
	public static Dictionary<GAF, int> ParseScarcityData(BlocksInventory blocksInventory)
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		foreach (int num in blocksInventory.BlockItemIds)
		{
			if (BlockItem.Exists(num))
			{
				BlockItem blockItem = BlockItem.FindByID(num);
				if (blockItem != null)
				{
					GAF key = new GAF(blockItem);
					dictionary[key] = blocksInventory.CountOrMinusOneIfInfinityOf(num);
				}
			}
		}
		return dictionary;
	}

	// Token: 0x06001D97 RID: 7575 RVA: 0x000D3800 File Offset: 0x000D1C00
	public static void PrintInventory(string name, Dictionary<GAF, int> inv)
	{
		BWLog.Info(name);
		foreach (GAF gaf in inv.Keys)
		{
			BWLog.Info(string.Concat(new object[]
			{
				"  ",
				gaf,
				": ",
				inv[gaf]
			}));
		}
	}

	// Token: 0x06001D98 RID: 7576 RVA: 0x000D3890 File Offset: 0x000D1C90
	public static void UpdateInventory(bool updateTiles = true, Dictionary<GAF, int> changes = null)
	{
		if (Scarcity.worldGAFUsage != null && Scarcity.inventory != null && (Tutorial.state == TutorialState.None || Tutorial.mode != TutorialMode.StepByStep))
		{
			Dictionary<GAF, int> d = Scarcity.CalculateWorldGafUsage(false, false);
			Dictionary<GAF, int> dictionary = Scarcity.CompareGafUsages(Scarcity.worldGAFUsage, d, "d1", "d2");
			bool flag = false;
			foreach (GAF gaf in dictionary.Keys)
			{
				int num = dictionary[gaf];
				GAF normalizedGaf = Scarcity.GetNormalizedGaf(gaf, false);
				if (Scarcity.inventory.ContainsKey(normalizedGaf) && Scarcity.inventory[normalizedGaf] != -1)
				{
					Dictionary<GAF, int> dictionary2;
					GAF key;
					(dictionary2 = Scarcity.inventory)[key = normalizedGaf] = dictionary2[key] + num;
					if (Scarcity.inventory[normalizedGaf] < 0)
					{
						Scarcity.inventory[normalizedGaf] = 0;
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
			Scarcity.worldGAFUsage = d;
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
	}

	// Token: 0x06001D99 RID: 7577 RVA: 0x000D39FC File Offset: 0x000D1DFC
	public static HashSet<string> GetUniqueBlockNames(Dictionary<GAF, int> gafUsageDict)
	{
		Dictionary<string, HashSet<string>> uniqueBlockMap = Blocksworld.GetUniqueBlockMap();
		HashSet<string> hashSet = new HashSet<string>();
		if (gafUsageDict == null || gafUsageDict.Count == 0)
		{
			return hashSet;
		}
		foreach (GAF gaf in gafUsageDict.Keys)
		{
			if (gaf.Predicate.ArgTypes.Length != 0 && gaf.Predicate.ArgTypes[0] == typeof(string))
			{
				string text = (string)gaf.Args[0];
				if (uniqueBlockMap.ContainsKey(text))
				{
					hashSet.Add(text);
				}
			}
		}
		return hashSet;
	}

	// Token: 0x14000010 RID: 16
	// (add) Token: 0x06001D9A RID: 7578 RVA: 0x000D3ACC File Offset: 0x000D1ECC
	// (remove) Token: 0x06001D9B RID: 7579 RVA: 0x000D3B00 File Offset: 0x000D1F00
	public static event Scarcity.ScarcityEventHandler OnInventoryChange;

	// Token: 0x0400180C RID: 6156
	public static Dictionary<GAF, int> puzzleInventory;

	// Token: 0x0400180D RID: 6157
	public static Dictionary<GAF, int> puzzleGAFUsage;

	// Token: 0x0400180E RID: 6158
	public static Dictionary<GAF, int> puzzleInventoryAfterStepByStep;

	// Token: 0x0400180F RID: 6159
	public static Dictionary<GAF, int> puzzleGAFUsageAfterStepByStep;

	// Token: 0x04001810 RID: 6160
	public static Dictionary<GAF, int> inventory = null;

	// Token: 0x04001811 RID: 6161
	public static Dictionary<GAF, float> inventoryScales = new Dictionary<GAF, float>();

	// Token: 0x04001812 RID: 6162
	public static Dictionary<GAF, int> globalInventory = null;

	// Token: 0x04001813 RID: 6163
	public static Dictionary<GAF, int> worldGAFUsage = null;

	// Token: 0x04001814 RID: 6164
	public static HashSet<GAF> autoRemoveHighlights = new HashSet<GAF>();

	// Token: 0x04001815 RID: 6165
	public static Dictionary<string, string> textureOriginals = new Dictionary<string, string>();

	// Token: 0x04001816 RID: 6166
	private static List<HudMeshLabel> scarcityLabels = new List<HudMeshLabel>();

	// Token: 0x04001817 RID: 6167
	private static HudMeshLabel quickSelectPaintScarcityLabel;

	// Token: 0x04001818 RID: 6168
	private static HudMeshLabel quickSelectTextureScarcityLabel;

	// Token: 0x04001819 RID: 6169
	private static HudMeshLabel quickSelectModelScarcityLabel;

	// Token: 0x0400181A RID: 6170
	private static HudMeshLabel quickSelectScriptScarcityLabel;

	// Token: 0x0400181B RID: 6171
	public static Dictionary<string, BlockScarcityInfo> scarcityInfos;

	// Token: 0x0400181C RID: 6172
	private static HashSet<Predicate> notCountInStepByStepPredicates = null;

	// Token: 0x0400181D RID: 6173
	private static string[] normalizedPostfixes = new string[]
	{
		"_Terrain",
		"_Sky",
		"_Water",
		"_Block"
	};

	// Token: 0x02000279 RID: 633
	// (Invoke) Token: 0x06001D9E RID: 7582
	public delegate void ScarcityEventHandler();
}
