using System.Collections.Generic;
using Blocks;
using Exdilin;
using SimpleJSON;
using UnityEngine;

public static class PanelSlots
{
	private static List<List<int>>[] _allTilesInTabsAsIDs;

	public static void LoadPanelSlotsFromResources()
	{
		_allTilesInTabsAsIDs = new List<List<int>>[10];
		string text = Resources.Load<TextAsset>("panel_slots_json").text;
		List<JObject> arrayValue = JSONDecoder.Decode(text).ArrayValue;
		foreach (JObject item in arrayValue)
		{
			List<List<int>> list = new List<List<int>>();
			foreach (JObject item2 in item["sections"].ArrayValue)
			{
				List<int> list2 = new List<int>();
				foreach (JObject item3 in item2["block_items"].ArrayValue)
				{
					if (BlockItem.Exists(item3.StringValue))
					{
						BlockItem blockItem = BlockItem.FindByInternalIdentifier(item3.StringValue);
						if (blockItem != null)
						{
							list2.Add(blockItem.Id);
						}
					}
				}
				list.Add(list2);
			}
			string stringValue = item["internal_identifier"].StringValue;
			int panelIndexForTab = TabBar.GetPanelIndexForTab(stringValue);
			_allTilesInTabsAsIDs[panelIndexForTab] = list;
		}
		BlockItemEntry[] itemEntries = BlockItemsRegistry.GetItemEntries();
		foreach (BlockItemEntry blockItemEntry in itemEntries)
		{
			List<List<int>> list3 = _allTilesInTabsAsIDs[TabBar.GetPanelIndexForTab(blockItemEntry.buildPaneTab)];
			list3[blockItemEntry.buildPaneSubTab].Add(blockItemEntry.item.Id);
		}
	}

	public static List<List<int>> GetTilesInTabAsIDs(TabBarTabId tab)
	{
		int panelIndexForTab = TabBar.GetPanelIndexForTab(tab);
		return _allTilesInTabsAsIDs[panelIndexForTab];
	}

	public static int GetTabIndexForGafSmart(GAF gaf)
	{
		GAF gAF = null;
		Predicate predicate = gaf.Predicate;
		if (predicate == Block.predicateCreate || predicate == Block.predicatePlaySoundDurational)
		{
			gAF = gaf.Clone();
		}
		else if (predicate == Block.predicatePaintTo)
		{
			gAF = new GAF(Block.predicatePaintTo, gaf.Args[0]);
		}
		else if (predicate == Block.predicateTextureTo)
		{
			gAF = new GAF(Block.predicateTextureTo, gaf.Args[0], Vector3.zero);
		}
		else if (predicate == BlockMaster.predicatePaintSkyTo)
		{
			gAF = new GAF(Block.predicatePaintTo, (string)gaf.Args[0]);
		}
		else if (predicate == BlockMaster.predicateSetEnvEffect)
		{
			string text = BlockSky.EnvEffectToTexture((string)gaf.Args[0]);
			gAF = new GAF(Block.predicateTextureTo, text, Vector3.zero);
		}
		else if (predicate == Block.predicateCreateModel)
		{
			return 1;
		}
		if (gAF == null)
		{
			return 8;
		}
		return GetTabIndexForGaf(gAF);
	}

	public static int GetTabIndexForGaf(GAF gaf)
	{
		BlockItem blockItem = BlockItem.FindByGafPredicateNameAndArguments(gaf.Predicate.Name, gaf.Args);
		if (blockItem == null)
		{
			TabBarTabId result = TabBarTabId.Blocks;
			if (gaf.Predicate == Block.predicateCreate)
			{
				result = TabBarTabId.Blocks;
			}
			else if (gaf.Predicate == Block.predicateTextureTo)
			{
				result = TabBarTabId.Textures;
			}
			else if (gaf.Predicate == Block.predicatePaintTo)
			{
				result = TabBarTabId.Colors;
			}
			else if (gaf.Predicate == Block.predicatePlaySoundDurational)
			{
				result = TabBarTabId.Sounds;
			}
			return (int)result;
		}
		int id = blockItem.Id;
		for (int i = 0; i < _allTilesInTabsAsIDs.Length; i++)
		{
			List<List<int>> list = _allTilesInTabsAsIDs[i];
			if (list == null)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				List<int> list2 = list[j];
				for (int k = 0; k < list2.Count; k++)
				{
					if (list2[k] == id)
					{
						return i;
					}
				}
			}
		}
		return 0;
	}

	public static bool GetBuildPanelInfo(BlockItem blockItem, out int tabIndex, out int positionInTab)
	{
		int id = blockItem.Id;
		for (int i = 0; i < _allTilesInTabsAsIDs.Length; i++)
		{
			List<List<int>> list = _allTilesInTabsAsIDs[i];
			int num = 0;
			if (list == null)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				List<int> list2 = list[j];
				for (int k = 0; k < list2.Count; k++)
				{
					if (list2[k] == id)
					{
						tabIndex = i;
						positionInTab = num;
						return true;
					}
					num++;
				}
			}
		}
		tabIndex = 0;
		positionInTab = 0;
		return false;
	}
}
