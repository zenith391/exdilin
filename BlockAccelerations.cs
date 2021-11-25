using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200003E RID: 62
public class BlockAccelerations
{
	// Token: 0x0600020F RID: 527 RVA: 0x0000C5A7 File Offset: 0x0000A9A7
	public static void AddModel(List<Block> model)
	{
		BlockAccelerations.relevantBlocks.UnionWith(model);
	}

	// Token: 0x06000210 RID: 528 RVA: 0x0000C5B4 File Offset: 0x0000A9B4
	public static void Play()
	{
		BlockAccelerations.Clear();
	}

	// Token: 0x06000211 RID: 529 RVA: 0x0000C5BB File Offset: 0x0000A9BB
	public static void Stop()
	{
		BlockAccelerations.Clear();
	}

	// Token: 0x06000212 RID: 530 RVA: 0x0000C5C2 File Offset: 0x0000A9C2
	private static void Clear()
	{
		BlockAccelerations.modelAccelerations.Clear();
		BlockAccelerations.relevantBlocks.Clear();
	}

	// Token: 0x06000213 RID: 531 RVA: 0x0000C5D8 File Offset: 0x0000A9D8
	public static Vector3 GetModelAcceleration(List<Block> model)
	{
		Vector3 result;
		BlockAccelerations.modelAccelerations.TryGetValue(model, out result);
		return result;
	}

	// Token: 0x06000214 RID: 532 RVA: 0x0000C5F4 File Offset: 0x0000A9F4
	public static void BlockAccelerates(Block b, Vector3 a)
	{
		if (BlockAccelerations.relevantBlocks.Count > 0 && BlockAccelerations.relevantBlocks.Contains(b))
		{
			List<Block> key = Block.connectedCache[b];
			Vector3 vector;
			if (!BlockAccelerations.modelAccelerations.TryGetValue(key, out vector))
			{
				vector = a;
			}
			else
			{
				vector += a;
			}
			BlockAccelerations.modelAccelerations[key] = vector;
		}
	}

	// Token: 0x06000215 RID: 533 RVA: 0x0000C65A File Offset: 0x0000AA5A
	public static void ResetFrame()
	{
		if (BlockAccelerations.relevantBlocks.Count > 0)
		{
			BlockAccelerations.modelAccelerations.Clear();
		}
	}

	// Token: 0x04000201 RID: 513
	public static bool enabled = false;

	// Token: 0x04000202 RID: 514
	private static HashSet<Block> relevantBlocks = new HashSet<Block>();

	// Token: 0x04000203 RID: 515
	private static Dictionary<List<Block>, Vector3> modelAccelerations = new Dictionary<List<Block>, Vector3>();
}
