using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class CheckpointSystem
{
	private static Dictionary<int, List<Block>> checkpointBlocks = new Dictionary<int, List<Block>>();

	private static Dictionary<int, Block> activeCheckpoints = new Dictionary<int, Block>();

	private static Dictionary<int, Block> spawnPoints = new Dictionary<int, Block>();

	private static Dictionary<int, HashSet<Block>> activatedCheckpoints = new Dictionary<int, HashSet<Block>>();

	public static void Clear()
	{
		checkpointBlocks.Clear();
		activeCheckpoints.Clear();
		spawnPoints.Clear();
		activatedCheckpoints.Clear();
	}

	public static void SetSpawnPoint(Block block, int playerIndex)
	{
		spawnPoints[playerIndex] = block;
	}

	public static void ClearActivatedCheckpoints(int playerIndex)
	{
		if (activatedCheckpoints.TryGetValue(playerIndex, out var value))
		{
			value.Clear();
		}
		activeCheckpoints.Remove(playerIndex);
	}

	public static void SetActiveCheckPoint(Block block, int playerIndex, bool allowOld = true)
	{
		if (allowOld)
		{
			activeCheckpoints[playerIndex] = block;
			return;
		}
		HashSet<Block> hashSet;
		if (activatedCheckpoints.ContainsKey(playerIndex))
		{
			hashSet = activatedCheckpoints[playerIndex];
		}
		else
		{
			hashSet = new HashSet<Block>();
			activatedCheckpoints[playerIndex] = hashSet;
		}
		if (!hashSet.Contains(block))
		{
			hashSet.Add(block);
			activeCheckpoints[playerIndex] = block;
		}
	}

	private static void TeleportBlockToBlock(Block toTeleport, Block target)
	{
		Vector3 position = target.goT.position;
		position = toTeleport.GetSafeTeleportToPosition(position);
		toTeleport.TeleportTo(position);
	}

	public static void Spawn(Block toSpawn, int playerIndex)
	{
		if (activeCheckpoints.TryGetValue(playerIndex, out var value))
		{
			TeleportBlockToBlock(toSpawn, value);
		}
		else if (spawnPoints.TryGetValue(playerIndex, out value))
		{
			TeleportBlockToBlock(toSpawn, value);
		}
		else
		{
			BWLog.Info("No spawnpoint and no checkpoints found for player index " + playerIndex);
		}
	}
}
