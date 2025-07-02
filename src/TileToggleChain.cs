using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

public class TileToggleChain
{
	public static Dictionary<int, List<int>> tileToggleChains = null;

	public static HashSet<Predicate> allPredicates = new HashSet<Predicate>();

	public static HashSet<int> GetAllNonPrimitiveBlockIds()
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (KeyValuePair<int, List<int>> tileToggleChain in tileToggleChains)
		{
			List<int> value = tileToggleChain.Value;
			for (int i = 1; i < value.Count; i++)
			{
				hashSet.Add(value[i]);
			}
		}
		return hashSet;
	}

	public static HashSet<int> GetAllPrimitiveBlockIds()
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (KeyValuePair<int, List<int>> tileToggleChain in tileToggleChains)
		{
			List<int> value = tileToggleChain.Value;
			if (value.Count > 0)
			{
				hashSet.Add(value[0]);
			}
		}
		return hashSet;
	}

	private static List<int> GetChainFor(int blockId)
	{
		if (!tileToggleChains.ContainsKey(blockId))
		{
			return null;
		}
		return tileToggleChains[blockId];
	}

	public static GAF GetPrimitiveGafFor(GAF gaf)
	{
		int primitiveBlockItemIdFor = GetPrimitiveBlockItemIdFor(gaf.BlockItemId);
		if (primitiveBlockItemIdFor == 0)
		{
			return null;
		}
		BlockItem blockItem = BlockItem.FindByID(primitiveBlockItemIdFor);
		return new GAF(blockItem);
	}

	public static int GetPrimitiveBlockItemIdFor(int blockId)
	{
		List<int> chainFor = GetChainFor(blockId);
		if (chainFor != null && chainFor.Count > 0)
		{
			return chainFor[0];
		}
		return 0;
	}

	public static int GetUnlockedCount(int blockId)
	{
		List<int> chainFor = GetChainFor(blockId);
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

	public static int GetLastUnlockedBlockItemIdFor(int blockId)
	{
		List<int> chainFor = GetChainFor(blockId);
		if (chainFor != null && chainFor.Count > 0)
		{
			for (int num = chainFor.Count - 1; num >= 0; num--)
			{
				int num2 = chainFor[num];
				if (Blocksworld.buildPanel.IsBlockItemInInventory(num2))
				{
					return num2;
				}
			}
		}
		return 0;
	}

	public static void AddChain(params GAF[] chain)
	{
		AddChain(new List<GAF>(chain));
	}

	public static void AddChain(List<GAF> chain)
	{
		List<int> list = new List<int>();
		foreach (GAF item in chain)
		{
			allPredicates.Add(item.Predicate);
			item.Predicate.canHaveOverlay = true;
			int blockItemId = item.BlockItemId;
			if (blockItemId <= 0)
			{
				Debug.Log("No blockItem id for gaf " + item);
			}
			else
			{
				list.Add(blockItemId);
			}
		}
		foreach (int item2 in list)
		{
			tileToggleChains[item2] = list;
		}
	}

	public static void LoadTileToggleChains()
	{
		if (tileToggleChains != null)
		{
			return;
		}
		tileToggleChains = new Dictionary<int, List<int>>();
		TextAsset textAsset = (TextAsset)Resources.Load("TileToggleChains");
		if (textAsset != null)
		{
			JObject jObject = JSONDecoder.Decode(textAsset.text);
			JObject jObject2 = jObject["tile-toggle-chains"];
			foreach (JObject item3 in jObject2.ArrayValue)
			{
				List<int> list = new List<int>();
				foreach (JObject item4 in item3.ArrayValue)
				{
					GAF gAF = GAF.FromJSON(item4, nullOnFailure: true);
					if (gAF != null)
					{
						int blockItemId = gAF.BlockItemId;
						if (blockItemId > 0)
						{
							list.Add(blockItemId);
							tileToggleChains[blockItemId] = list;
							allPredicates.Add(gAF.Predicate);
							gAF.Predicate.canHaveOverlay = true;
						}
					}
				}
			}
		}
		else
		{
			BWLog.Info("Could not find TileToggleChains.txt");
		}
		GAF item = new GAF(Block.predicateSendCustomSignal, "*", 1f);
		GAF item2 = new GAF(Block.predicateSendCustomSignalModel, "*", 1f);
		AddChain(new List<GAF> { item, item2 });
		BlockSteeringWheel.AddVehicleDefintionToggleChain();
	}

	public static bool InSameChain(int blockItemId1, int blockItemId2)
	{
		int primitiveBlockItemIdFor = GetPrimitiveBlockItemIdFor(blockItemId1);
		int primitiveBlockItemIdFor2 = GetPrimitiveBlockItemIdFor(blockItemId2);
		if (primitiveBlockItemIdFor != 0 && primitiveBlockItemIdFor2 != 0)
		{
			return primitiveBlockItemIdFor.Equals(primitiveBlockItemIdFor2);
		}
		return false;
	}

	public static bool IsToggleGaf(GAF gaf)
	{
		LoadTileToggleChains();
		return GetChainFor(gaf.BlockItemId) != null;
	}

	public static GAF GetNextUnlocked(GAF gaf)
	{
		LoadTileToggleChains();
		int blockItemId = gaf.BlockItemId;
		List<int> chainFor = GetChainFor(blockItemId);
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
				GAF gAF = new GAF(blockItem);
				if (flag)
				{
					gAF.Args[0] = Util.GetStringArgSafe(gaf.Args, 0, "*");
				}
				return gAF;
			}
		}
		return null;
	}

	public static bool HasMoreThanOneUnlocked(GAF gaf)
	{
		LoadTileToggleChains();
		int blockItemId = gaf.BlockItemId;
		List<int> chainFor = GetChainFor(blockItemId);
		if (chainFor != null && chainFor.Count > 0)
		{
			int index = (chainFor.IndexOf(blockItemId) + 1) % chainFor.Count;
			return chainFor[index] != blockItemId;
		}
		return false;
	}
}
