using System.Collections.Generic;
using Blocks;

public class TextureAndPaintBlockRegistry
{
	private static Dictionary<string, HashSet<Block>> textureBlocks = new Dictionary<string, HashSet<Block>>();

	private static Dictionary<string, HashSet<Block>> paintBlocks = new Dictionary<string, HashSet<Block>>();

	private static HashSet<Block> emptySet = new HashSet<Block>();

	public static void Clear()
	{
		textureBlocks.Clear();
		paintBlocks.Clear();
	}

	private static void Add(Dictionary<string, HashSet<Block>> map, Block b, string newKey)
	{
		if (!map.TryGetValue(newKey, out var value))
		{
			value = (map[newKey] = new HashSet<Block>());
		}
		value.Add(b);
	}

	private static void Remove(Dictionary<string, HashSet<Block>> map, Block b, string oldKey)
	{
		if (oldKey != null && map.TryGetValue(oldKey, out var value))
		{
			value.Remove(b);
		}
	}

	private static void Update(Dictionary<string, HashSet<Block>> map, Block b, string newKey, string oldKey)
	{
		Remove(map, b, oldKey);
		Add(map, b, newKey);
	}

	public static void BlockTextureChanged(Block b, string texture, string oldTexture)
	{
		Update(textureBlocks, b, texture, oldTexture);
	}

	public static void BlockTextureRemoved(Block b, string oldTexture)
	{
		Remove(textureBlocks, b, oldTexture);
	}

	public static void BlockPaintChanged(Block b, string paint, string oldPaint)
	{
		Update(paintBlocks, b, paint, oldPaint);
	}

	public static void BlockPaintRemoved(Block b, string oldPaint)
	{
		Remove(paintBlocks, b, oldPaint);
	}

	public static HashSet<Block> GetBlocksWithTexture(string texture)
	{
		if (textureBlocks.TryGetValue(texture, out var value))
		{
			return value;
		}
		return emptySet;
	}

	public static HashSet<Block> GetBlocksWithPaint(string paint)
	{
		if (paintBlocks.TryGetValue(paint, out var value))
		{
			return value;
		}
		return emptySet;
	}
}
