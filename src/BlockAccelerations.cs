using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class BlockAccelerations
{
	public static bool enabled = false;

	private static HashSet<Block> relevantBlocks = new HashSet<Block>();

	private static Dictionary<List<Block>, Vector3> modelAccelerations = new Dictionary<List<Block>, Vector3>();

	public static void AddModel(List<Block> model)
	{
		relevantBlocks.UnionWith(model);
	}

	public static void Play()
	{
		Clear();
	}

	public static void Stop()
	{
		Clear();
	}

	private static void Clear()
	{
		modelAccelerations.Clear();
		relevantBlocks.Clear();
	}

	public static Vector3 GetModelAcceleration(List<Block> model)
	{
		modelAccelerations.TryGetValue(model, out var value);
		return value;
	}

	public static void BlockAccelerates(Block b, Vector3 a)
	{
		if (relevantBlocks.Count > 0 && relevantBlocks.Contains(b))
		{
			List<Block> key = Block.connectedCache[b];
			if (!modelAccelerations.TryGetValue(key, out var value))
			{
				value = a;
			}
			else
			{
				value += a;
			}
			modelAccelerations[key] = value;
		}
	}

	public static void ResetFrame()
	{
		if (relevantBlocks.Count > 0)
		{
			modelAccelerations.Clear();
		}
	}
}
