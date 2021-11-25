using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x02000040 RID: 64
public class BlockData : ObjectData
{
	// Token: 0x06000222 RID: 546 RVA: 0x0000C798 File Offset: 0x0000AB98
	public static BlockData GetBlockData(Block b)
	{
		BlockData blockData;
		if (!BlockData.datas.TryGetValue(b, out blockData))
		{
			blockData = new BlockData();
			BlockData.datas[b] = blockData;
		}
		return blockData;
	}

	// Token: 0x06000223 RID: 547 RVA: 0x0000C7CA File Offset: 0x0000ABCA
	public static void Clear()
	{
		BlockData.datas.Clear();
	}

	// Token: 0x04000207 RID: 519
	private static Dictionary<Block, BlockData> datas = new Dictionary<Block, BlockData>();
}
