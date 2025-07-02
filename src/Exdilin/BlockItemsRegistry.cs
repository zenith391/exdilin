using System.Collections.Generic;

namespace Exdilin;

public static class BlockItemsRegistry
{
	private static List<BlockItemEntry> entries = new List<BlockItemEntry>();

	private static Dictionary<string, BlockEntry> blockEntries = new Dictionary<string, BlockEntry>();

	public static BlockItemEntry[] GetItemEntries()
	{
		return entries.ToArray();
	}

	public static Dictionary<string, BlockEntry> GetBlockEntries()
	{
		return blockEntries;
	}

	public static BlockEntry GetBlockEntry(string id)
	{
		BlockEntry value = null;
		blockEntries.TryGetValue(id, out value);
		return value;
	}

	public static void AddBlockItem(BlockItemEntry entry)
	{
		entries.Add(entry);
	}

	public static void AddBlock(BlockEntry entry)
	{
		Blocksworld.existingBlockNames.Add(entry.id);
		if (entry.originator == null)
		{
			entry.originator = Mod.ExecutionMod;
		}
		blockEntries.Add(entry.id, entry);
	}
}
