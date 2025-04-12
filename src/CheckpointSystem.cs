using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000111 RID: 273
public class CheckpointSystem
{
	// Token: 0x0600133C RID: 4924 RVA: 0x000844A5 File Offset: 0x000828A5
	public static void Clear()
	{
		CheckpointSystem.checkpointBlocks.Clear();
		CheckpointSystem.activeCheckpoints.Clear();
		CheckpointSystem.spawnPoints.Clear();
		CheckpointSystem.activatedCheckpoints.Clear();
	}

	// Token: 0x0600133D RID: 4925 RVA: 0x000844CF File Offset: 0x000828CF
	public static void SetSpawnPoint(Block block, int playerIndex)
	{
		CheckpointSystem.spawnPoints[playerIndex] = block;
	}

	// Token: 0x0600133E RID: 4926 RVA: 0x000844E0 File Offset: 0x000828E0
	public static void ClearActivatedCheckpoints(int playerIndex)
	{
		HashSet<Block> hashSet;
		if (CheckpointSystem.activatedCheckpoints.TryGetValue(playerIndex, out hashSet))
		{
			hashSet.Clear();
		}
		CheckpointSystem.activeCheckpoints.Remove(playerIndex);
	}

	// Token: 0x0600133F RID: 4927 RVA: 0x00084514 File Offset: 0x00082914
	public static void SetActiveCheckPoint(Block block, int playerIndex, bool allowOld = true)
	{
		if (allowOld)
		{
			CheckpointSystem.activeCheckpoints[playerIndex] = block;
		}
		else
		{
			HashSet<Block> hashSet;
			if (CheckpointSystem.activatedCheckpoints.ContainsKey(playerIndex))
			{
				hashSet = CheckpointSystem.activatedCheckpoints[playerIndex];
			}
			else
			{
				hashSet = new HashSet<Block>();
				CheckpointSystem.activatedCheckpoints[playerIndex] = hashSet;
			}
			if (!hashSet.Contains(block))
			{
				hashSet.Add(block);
				CheckpointSystem.activeCheckpoints[playerIndex] = block;
			}
		}
	}

	// Token: 0x06001340 RID: 4928 RVA: 0x0008458C File Offset: 0x0008298C
	private static void TeleportBlockToBlock(Block toTeleport, Block target)
	{
		Vector3 targetPos = target.goT.position;
		targetPos = toTeleport.GetSafeTeleportToPosition(targetPos);
		toTeleport.TeleportTo(targetPos, false, false, false);
	}

	// Token: 0x06001341 RID: 4929 RVA: 0x000845B8 File Offset: 0x000829B8
	public static void Spawn(Block toSpawn, int playerIndex)
	{
		Block target;
		if (CheckpointSystem.activeCheckpoints.TryGetValue(playerIndex, out target))
		{
			CheckpointSystem.TeleportBlockToBlock(toSpawn, target);
		}
		else if (CheckpointSystem.spawnPoints.TryGetValue(playerIndex, out target))
		{
			CheckpointSystem.TeleportBlockToBlock(toSpawn, target);
		}
		else
		{
			BWLog.Info("No spawnpoint and no checkpoints found for player index " + playerIndex);
		}
	}

	// Token: 0x04000F27 RID: 3879
	private static Dictionary<int, List<Block>> checkpointBlocks = new Dictionary<int, List<Block>>();

	// Token: 0x04000F28 RID: 3880
	private static Dictionary<int, Block> activeCheckpoints = new Dictionary<int, Block>();

	// Token: 0x04000F29 RID: 3881
	private static Dictionary<int, Block> spawnPoints = new Dictionary<int, Block>();

	// Token: 0x04000F2A RID: 3882
	private static Dictionary<int, HashSet<Block>> activatedCheckpoints = new Dictionary<int, HashSet<Block>>();
}
