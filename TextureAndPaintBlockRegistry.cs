using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x020002C5 RID: 709
public class TextureAndPaintBlockRegistry
{
	// Token: 0x06002065 RID: 8293 RVA: 0x000EDF5D File Offset: 0x000EC35D
	public static void Clear()
	{
		TextureAndPaintBlockRegistry.textureBlocks.Clear();
		TextureAndPaintBlockRegistry.paintBlocks.Clear();
	}

	// Token: 0x06002066 RID: 8294 RVA: 0x000EDF74 File Offset: 0x000EC374
	private static void Add(Dictionary<string, HashSet<Block>> map, Block b, string newKey)
	{
		HashSet<Block> hashSet;
		if (!map.TryGetValue(newKey, out hashSet))
		{
			hashSet = new HashSet<Block>();
			map[newKey] = hashSet;
		}
		hashSet.Add(b);
	}

	// Token: 0x06002067 RID: 8295 RVA: 0x000EDFA8 File Offset: 0x000EC3A8
	private static void Remove(Dictionary<string, HashSet<Block>> map, Block b, string oldKey)
	{
		HashSet<Block> hashSet;
		if (oldKey != null && map.TryGetValue(oldKey, out hashSet))
		{
			hashSet.Remove(b);
		}
	}

	// Token: 0x06002068 RID: 8296 RVA: 0x000EDFD1 File Offset: 0x000EC3D1
	private static void Update(Dictionary<string, HashSet<Block>> map, Block b, string newKey, string oldKey)
	{
		TextureAndPaintBlockRegistry.Remove(map, b, oldKey);
		TextureAndPaintBlockRegistry.Add(map, b, newKey);
	}

	// Token: 0x06002069 RID: 8297 RVA: 0x000EDFE3 File Offset: 0x000EC3E3
	public static void BlockTextureChanged(Block b, string texture, string oldTexture)
	{
		TextureAndPaintBlockRegistry.Update(TextureAndPaintBlockRegistry.textureBlocks, b, texture, oldTexture);
	}

	// Token: 0x0600206A RID: 8298 RVA: 0x000EDFF2 File Offset: 0x000EC3F2
	public static void BlockTextureRemoved(Block b, string oldTexture)
	{
		TextureAndPaintBlockRegistry.Remove(TextureAndPaintBlockRegistry.textureBlocks, b, oldTexture);
	}

	// Token: 0x0600206B RID: 8299 RVA: 0x000EE000 File Offset: 0x000EC400
	public static void BlockPaintChanged(Block b, string paint, string oldPaint)
	{
		TextureAndPaintBlockRegistry.Update(TextureAndPaintBlockRegistry.paintBlocks, b, paint, oldPaint);
	}

	// Token: 0x0600206C RID: 8300 RVA: 0x000EE00F File Offset: 0x000EC40F
	public static void BlockPaintRemoved(Block b, string oldPaint)
	{
		TextureAndPaintBlockRegistry.Remove(TextureAndPaintBlockRegistry.paintBlocks, b, oldPaint);
	}

	// Token: 0x0600206D RID: 8301 RVA: 0x000EE020 File Offset: 0x000EC420
	public static HashSet<Block> GetBlocksWithTexture(string texture)
	{
		HashSet<Block> result;
		if (TextureAndPaintBlockRegistry.textureBlocks.TryGetValue(texture, out result))
		{
			return result;
		}
		return TextureAndPaintBlockRegistry.emptySet;
	}

	// Token: 0x0600206E RID: 8302 RVA: 0x000EE048 File Offset: 0x000EC448
	public static HashSet<Block> GetBlocksWithPaint(string paint)
	{
		HashSet<Block> result;
		if (TextureAndPaintBlockRegistry.paintBlocks.TryGetValue(paint, out result))
		{
			return result;
		}
		return TextureAndPaintBlockRegistry.emptySet;
	}

	// Token: 0x04001BAC RID: 7084
	private static Dictionary<string, HashSet<Block>> textureBlocks = new Dictionary<string, HashSet<Block>>();

	// Token: 0x04001BAD RID: 7085
	private static Dictionary<string, HashSet<Block>> paintBlocks = new Dictionary<string, HashSet<Block>>();

	// Token: 0x04001BAE RID: 7086
	private static HashSet<Block> emptySet = new HashSet<Block>();
}
