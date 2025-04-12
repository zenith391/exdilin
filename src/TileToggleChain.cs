using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x020002D4 RID: 724
public class TileToggleChain
{
	// Token: 0x0600210B RID: 8459 RVA: 0x000F1FD0 File Offset: 0x000F03D0
	public static HashSet<int> GetAllNonPrimitiveBlockIds()
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (KeyValuePair<int, List<int>> keyValuePair in TileToggleChain.tileToggleChains)
		{
			List<int> value = keyValuePair.Value;
			for (int i = 1; i < value.Count; i++)
			{
				hashSet.Add(value[i]);
			}
		}
		return hashSet;
	}

	// Token: 0x0600210C RID: 8460 RVA: 0x000F2060 File Offset: 0x000F0460
	public static HashSet<int> GetAllPrimitiveBlockIds()
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (KeyValuePair<int, List<int>> keyValuePair in TileToggleChain.tileToggleChains)
		{
			List<int> value = keyValuePair.Value;
			if (value.Count > 0)
			{
				hashSet.Add(value[0]);
			}
		}
		return hashSet;
	}

	// Token: 0x0600210D RID: 8461 RVA: 0x000F20E0 File Offset: 0x000F04E0
	private static List<int> GetChainFor(int blockId)
	{
		if (!TileToggleChain.tileToggleChains.ContainsKey(blockId))
		{
			return null;
		}
		return TileToggleChain.tileToggleChains[blockId];
	}

	// Token: 0x0600210E RID: 8462 RVA: 0x000F2100 File Offset: 0x000F0500
	public static GAF GetPrimitiveGafFor(GAF gaf)
	{
		int primitiveBlockItemIdFor = TileToggleChain.GetPrimitiveBlockItemIdFor(gaf.BlockItemId);
		if (primitiveBlockItemIdFor == 0)
		{
			return null;
		}
		BlockItem blockItem = BlockItem.FindByID(primitiveBlockItemIdFor);
		return new GAF(blockItem);
	}

	// Token: 0x0600210F RID: 8463 RVA: 0x000F2130 File Offset: 0x000F0530
	public static int GetPrimitiveBlockItemIdFor(int blockId)
	{
		List<int> chainFor = TileToggleChain.GetChainFor(blockId);
		if (chainFor != null && chainFor.Count > 0)
		{
			return chainFor[0];
		}
		return 0;
	}

	// Token: 0x06002110 RID: 8464 RVA: 0x000F2160 File Offset: 0x000F0560
	public static int GetUnlockedCount(int blockId)
	{
		List<int> chainFor = TileToggleChain.GetChainFor(blockId);
		int num = 0;
		if (chainFor != null)
		{
			for (int i = 0; i < chainFor.Count; i++)
			{
				int blockItemId = chainFor[i];
				if (Blocksworld.buildPanel.IsBlockItemInInventory(blockItemId))
				{
					num++;
				}
			}
		}
		return num;
	}

	// Token: 0x06002111 RID: 8465 RVA: 0x000F21B0 File Offset: 0x000F05B0
	public static int GetLastUnlockedBlockItemIdFor(int blockId)
	{
		List<int> chainFor = TileToggleChain.GetChainFor(blockId);
		if (chainFor != null && chainFor.Count > 0)
		{
			for (int i = chainFor.Count - 1; i >= 0; i--)
			{
				int num = chainFor[i];
				if (Blocksworld.buildPanel.IsBlockItemInInventory(num))
				{
					return num;
				}
			}
		}
		return 0;
	}

	// Token: 0x06002112 RID: 8466 RVA: 0x000F220A File Offset: 0x000F060A
	public static void AddChain(params GAF[] chain)
	{
		TileToggleChain.AddChain(new List<GAF>(chain));
	}

	// Token: 0x06002113 RID: 8467 RVA: 0x000F2218 File Offset: 0x000F0618
	public static void AddChain(List<GAF> chain)
	{
		List<int> list = new List<int>();
		foreach (GAF gaf in chain)
		{
			TileToggleChain.allPredicates.Add(gaf.Predicate);
			gaf.Predicate.canHaveOverlay = true;
			int blockItemId = gaf.BlockItemId;
			if (blockItemId <= 0)
			{
				Debug.Log("No blockItem id for gaf " + gaf);
			}
			else
			{
				list.Add(blockItemId);
			}
		}
		foreach (int key in list)
		{
			TileToggleChain.tileToggleChains[key] = list;
		}
	}

	// Token: 0x06002114 RID: 8468 RVA: 0x000F2304 File Offset: 0x000F0704
	public static void LoadTileToggleChains()
	{
		if (TileToggleChain.tileToggleChains == null)
		{
			TileToggleChain.tileToggleChains = new Dictionary<int, List<int>>();
			TextAsset textAsset = (TextAsset)Resources.Load("TileToggleChains");
			if (textAsset != null)
			{
				JObject jobject = JSONDecoder.Decode(textAsset.text);
				JObject jobject2 = jobject["tile-toggle-chains"];
				foreach (JObject jobject3 in jobject2.ArrayValue)
				{
					List<int> list = new List<int>();
					foreach (JObject obj in jobject3.ArrayValue)
					{
						GAF gaf = GAF.FromJSON(obj, true, true);
						if (gaf != null)
						{
							int blockItemId = gaf.BlockItemId;
							if (blockItemId > 0)
							{
								list.Add(blockItemId);
								TileToggleChain.tileToggleChains[blockItemId] = list;
								TileToggleChain.allPredicates.Add(gaf.Predicate);
								gaf.Predicate.canHaveOverlay = true;
							}
						}
					}
				}
			}
			else
			{
				BWLog.Info("Could not find TileToggleChains.txt");
			}
			GAF item = new GAF(Block.predicateSendCustomSignal, new object[]
			{
				"*",
				1f
			});
			GAF item2 = new GAF(Block.predicateSendCustomSignalModel, new object[]
			{
				"*",
				1f
			});
			TileToggleChain.AddChain(new List<GAF>
			{
				item,
				item2
			});
			BlockSteeringWheel.AddVehicleDefintionToggleChain();
		}
	}

	// Token: 0x06002115 RID: 8469 RVA: 0x000F24CC File Offset: 0x000F08CC
	public static bool InSameChain(int blockItemId1, int blockItemId2)
	{
		int primitiveBlockItemIdFor = TileToggleChain.GetPrimitiveBlockItemIdFor(blockItemId1);
		int primitiveBlockItemIdFor2 = TileToggleChain.GetPrimitiveBlockItemIdFor(blockItemId2);
		return primitiveBlockItemIdFor != 0 && primitiveBlockItemIdFor2 != 0 && primitiveBlockItemIdFor.Equals(primitiveBlockItemIdFor2);
	}

	// Token: 0x06002116 RID: 8470 RVA: 0x000F24FE File Offset: 0x000F08FE
	public static bool IsToggleGaf(GAF gaf)
	{
		TileToggleChain.LoadTileToggleChains();
		return TileToggleChain.GetChainFor(gaf.BlockItemId) != null;
	}

	// Token: 0x06002117 RID: 8471 RVA: 0x000F2518 File Offset: 0x000F0918
	public static GAF GetNextUnlocked(GAF gaf)
	{
		TileToggleChain.LoadTileToggleChains();
		int blockItemId = gaf.BlockItemId;
		List<int> chainFor = TileToggleChain.GetChainFor(blockItemId);
		bool flag = gaf.Predicate == Block.predicateSendCustomSignal || gaf.Predicate == Block.predicateSendCustomSignalModel;
		if (chainFor != null && chainFor.Count > 0)
		{
			int num = (chainFor.IndexOf(blockItemId) + 1) % chainFor.Count;
			int num2 = num;
			if (num2 < num + chainFor.Count - 1)
			{
				int index = num2 % chainFor.Count;
				int id = chainFor[index];
				BlockItem blockItem = BlockItem.FindByID(id);
				GAF gaf2 = new GAF(blockItem);
				if (flag)
				{
					gaf2.Args[0] = Util.GetStringArgSafe(gaf.Args, 0, "*");
				}
				return gaf2;
			}
		}
		return null;
	}

	// Token: 0x06002118 RID: 8472 RVA: 0x000F25E8 File Offset: 0x000F09E8
	public static bool HasMoreThanOneUnlocked(GAF gaf)
	{
		TileToggleChain.LoadTileToggleChains();
		int blockItemId = gaf.BlockItemId;
		List<int> chainFor = TileToggleChain.GetChainFor(blockItemId);
		if (chainFor != null && chainFor.Count > 0)
		{
			int index = (chainFor.IndexOf(blockItemId) + 1) % chainFor.Count;
			return chainFor[index] != blockItemId;
		}
		return false;
	}

	// Token: 0x04001C0D RID: 7181
	public static Dictionary<int, List<int>> tileToggleChains = null;

	// Token: 0x04001C0E RID: 7182
	public static HashSet<Predicate> allPredicates = new HashSet<Predicate>();
}
