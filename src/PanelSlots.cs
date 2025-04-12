using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;
using Exdilin;

// Token: 0x02000260 RID: 608
public static class PanelSlots
{
	// Token: 0x06001C60 RID: 7264 RVA: 0x000C7A1C File Offset: 0x000C5E1C
	public static void LoadPanelSlotsFromResources()
	{
		PanelSlots._allTilesInTabsAsIDs = new List<List<int>>[10];
		string text = Resources.Load<TextAsset>("panel_slots_json").text;
		List<JObject> arrayValue = JSONDecoder.Decode(text).ArrayValue;
        foreach (JObject jobject in arrayValue)
        {
            List<List<int>> list = new List<List<int>>();
            foreach (JObject jobject2 in jobject["sections"].ArrayValue)
            {
                List<int> list2 = new List<int>();
                foreach (JObject jobject3 in jobject2["block_items"].ArrayValue)
                {
                    if (BlockItem.Exists(jobject3.StringValue))
                    {
                        BlockItem blockItem = BlockItem.FindByInternalIdentifier(jobject3.StringValue);
                        if (blockItem != null)
                        {
                            list2.Add(blockItem.Id);
                        }
                    }
                }
                list.Add(list2);
            }
            string stringValue = jobject["internal_identifier"].StringValue;
            int panelIndexForTab = TabBar.GetPanelIndexForTab(stringValue);
            PanelSlots._allTilesInTabsAsIDs[panelIndexForTab] = list;
        }
        foreach (BlockItemEntry entry in BlockItemsRegistry.GetItemEntries())
        {
            List<List<int>> list = PanelSlots._allTilesInTabsAsIDs[TabBar.GetPanelIndexForTab(entry.buildPaneTab)];
            list[entry.buildPaneSubTab].Add(entry.item.Id);
        }
    }

	// Token: 0x06001C61 RID: 7265 RVA: 0x000C7BCC File Offset: 0x000C5FCC
	public static List<List<int>> GetTilesInTabAsIDs(TabBarTabId tab)
	{
		int panelIndexForTab = TabBar.GetPanelIndexForTab(tab);
		return PanelSlots._allTilesInTabsAsIDs[panelIndexForTab];
	}

	// Token: 0x06001C62 RID: 7266 RVA: 0x000C7BE8 File Offset: 0x000C5FE8
	public static int GetTabIndexForGafSmart(GAF gaf)
	{
		GAF gaf2 = null;
		Predicate predicate = gaf.Predicate;
		if (predicate == Block.predicateCreate || predicate == Block.predicatePlaySoundDurational)
		{
			gaf2 = gaf.Clone();
		}
		else if (predicate == Block.predicatePaintTo)
		{
			gaf2 = new GAF(Block.predicatePaintTo, new object[]
			{
				gaf.Args[0]
			});
		}
		else if (predicate == Block.predicateTextureTo)
		{
			gaf2 = new GAF(Block.predicateTextureTo, new object[]
			{
				gaf.Args[0],
				Vector3.zero
			});
		}
		else if (predicate == BlockMaster.predicatePaintSkyTo)
		{
			gaf2 = new GAF(Block.predicatePaintTo, new object[]
			{
				(string)gaf.Args[0]
			});
		}
		else if (predicate == BlockMaster.predicateSetEnvEffect)
		{
			string text = BlockSky.EnvEffectToTexture((string)gaf.Args[0]);
			gaf2 = new GAF(Block.predicateTextureTo, new object[]
			{
				text,
				Vector3.zero
			});
		}
		else if (predicate == Block.predicateCreateModel)
		{
			return 1;
		}
		int result;
		if (gaf2 == null)
		{
			result = 8;
		}
		else
		{
			result = PanelSlots.GetTabIndexForGaf(gaf2);
		}
		return result;
	}

	// Token: 0x06001C63 RID: 7267 RVA: 0x000C7D20 File Offset: 0x000C6120
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
		for (int i = 0; i < PanelSlots._allTilesInTabsAsIDs.Length; i++)
		{
			List<List<int>> list = PanelSlots._allTilesInTabsAsIDs[i];
			if (list != null)
			{
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
		}
		return 0;
	}

	// Token: 0x06001C64 RID: 7268 RVA: 0x000C7E2C File Offset: 0x000C622C
	public static bool GetBuildPanelInfo(BlockItem blockItem, out int tabIndex, out int positionInTab)
	{
		int id = blockItem.Id;
		for (int i = 0; i < PanelSlots._allTilesInTabsAsIDs.Length; i++)
		{
			List<List<int>> list = PanelSlots._allTilesInTabsAsIDs[i];
			int num = 0;
			if (list != null)
			{
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
		}
		tabIndex = 0;
		positionInTab = 0;
		return false;
	}

	// Token: 0x04001742 RID: 5954
	private static List<List<int>>[] _allTilesInTabsAsIDs;
}
