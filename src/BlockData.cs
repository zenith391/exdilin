using System.Collections.Generic;
using Blocks;

public class BlockData : ObjectData
{
	private static Dictionary<Block, BlockData> datas = new Dictionary<Block, BlockData>();

	public static BlockData GetBlockData(Block b)
	{
		if (!datas.TryGetValue(b, out var value))
		{
			value = new BlockData();
			datas[b] = value;
		}
		return value;
	}

	public static void Clear()
	{
		datas.Clear();
	}
}
