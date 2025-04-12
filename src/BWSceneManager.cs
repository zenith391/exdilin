using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200027C RID: 636
public static class BWSceneManager
{
	// Token: 0x06001DA6 RID: 7590 RVA: 0x000D3E7A File Offset: 0x000D227A
	public static void RegisterRuntimeBlockSubstitution(Block block, BlockSubstitution substitution)
	{
		BWSceneManager._runtimeBlockSubstitutions[block] = substitution;
	}

	// Token: 0x06001DA7 RID: 7591 RVA: 0x000D3E88 File Offset: 0x000D2288
	public static void InitPlay()
	{
		foreach (BlockSubstitution blockSubstitution in BWSceneManager._runtimeBlockSubstitutions.Values)
		{
			blockSubstitution.ApplySubstitute();
		}
		BWSceneManager.ResetPlayBlocksAndPredicates();
	}

	// Token: 0x06001DA8 RID: 7592 RVA: 0x000D3EEC File Offset: 0x000D22EC
	public static void ResetPlayPredicates()
	{
		BWSceneManager.allBlockPlayPredicates.Clear();
		for (int i = 0; i < BWSceneManager.blocks.Count; i++)
		{
			Block block = BWSceneManager.blocks[i];
			HashSet<Predicate> playPredicates = block.GetPlayPredicates();
			BWSceneManager.allBlockPlayPredicates[block] = playPredicates;
		}
	}

	// Token: 0x06001DA9 RID: 7593 RVA: 0x000D3F40 File Offset: 0x000D2340
	public static void ResetPlayBlocksAndPredicates()
	{
		BWSceneManager.playBlocksRemoved.Clear();
		BWSceneManager.playBlocks.Clear();
		BWSceneManager.playBlocks.AddRange(BWSceneManager.blocks);
		BWSceneManager.allPlayPredicates.Clear();
		BWSceneManager.allBlockPlayPredicates.Clear();
		BWSceneManager.resetFrameBlocks.Clear();
		for (int i = 0; i < BWSceneManager.playBlocks.Count; i++)
		{
			Block block = BWSceneManager.playBlocks[i];
			if (block.GetType() != typeof(Block))
			{
				BWSceneManager.resetFrameBlocks.Add(block);
			}
			HashSet<Predicate> playPredicates = block.GetPlayPredicates();
			BWSceneManager.allBlockPlayPredicates[block] = playPredicates;
			BWSceneManager.allPlayPredicates.UnionWith(playPredicates);
		}
	}

	// Token: 0x06001DAA RID: 7594 RVA: 0x000D3FF4 File Offset: 0x000D23F4
	public static void ResetFrame()
	{
		int count = BWSceneManager.resetFrameBlocks.Count;
		for (int i = 0; i < count; i++)
		{
			BWSceneManager.resetFrameBlocks[i].ResetFrame();
		}
	}

	// Token: 0x06001DAB RID: 7595 RVA: 0x000D4030 File Offset: 0x000D2430
	public static void BeforePlay()
	{
		for (int i = 0; i < BWSceneManager.blocks.Count; i++)
		{
			Block block = BWSceneManager.blocks[i];
			block.CheckContainsPlayModeTiles();
			if (block.containsPlayModeTiles)
			{
				block.CreateFlattenTiles();
			}
			block.BeforePlay();
		}
	}

	// Token: 0x06001DAC RID: 7596 RVA: 0x000D4084 File Offset: 0x000D2484
	public static void ExecutePlay()
	{
		for (int i = 0; i < BWSceneManager.blocks.Count; i++)
		{
			Block block = BWSceneManager.blocks[i];
			block.Play();
			if (block.containsPlayModeTiles)
			{
				BWSceneManager.scriptBlocks.Add(block);
			}
		}
		BWSceneManager.scriptBlocksArr = BWSceneManager.scriptBlocks.ToArray();
		BWSceneManager.playBlocksArr = BWSceneManager.playBlocks.ToArray();
	}

	// Token: 0x06001DAD RID: 7597 RVA: 0x000D40F4 File Offset: 0x000D24F4
	public static void ExecutePlay2()
	{
		for (int i = 0; i < BWSceneManager.blocks.Count; i++)
		{
			Block block = BWSceneManager.blocks[i];
			block.Play2();
		}
	}

	// Token: 0x06001DAE RID: 7598 RVA: 0x000D4130 File Offset: 0x000D2530
	public static void ExecutePlayLegs()
	{
		for (int i = 0; i < BWSceneManager.blocks.Count; i++)
		{
			Block block = BWSceneManager.blocks[i];
			if (block is BlockAbstractLegs)
			{
				BlockAbstractLegs blockAbstractLegs = (BlockAbstractLegs)block;
				blockAbstractLegs.PlayLegs1();
			}
		}
		for (int j = 0; j < BWSceneManager.blocks.Count; j++)
		{
			Block block2 = BWSceneManager.blocks[j];
			if (block2 is BlockAbstractLegs)
			{
				BlockAbstractLegs blockAbstractLegs2 = (BlockAbstractLegs)block2;
				blockAbstractLegs2.PlayLegs2();
			}
		}
	}

	// Token: 0x06001DAF RID: 7599 RVA: 0x000D41C0 File Offset: 0x000D25C0
	public static void StopPlay()
	{
		foreach (BlockSubstitution blockSubstitution in BWSceneManager._runtimeBlockSubstitutions.Values)
		{
			blockSubstitution.RestoreOriginal();
		}
		BWSceneManager.playBlocks.Clear();
		BWSceneManager.playBlocksArr = null;
		BWSceneManager.resetFrameBlocks.Clear();
		BWSceneManager.scriptBlocks.Clear();
		BWSceneManager.scriptBlocksArr = null;
		BWSceneManager.allPlayPredicates.Clear();
		BWSceneManager.allBlockPlayPredicates.Clear();
	}

	// Token: 0x06001DB0 RID: 7600 RVA: 0x000D4260 File Offset: 0x000D2660
	public static void ClearScriptBlocks()
	{
		BWSceneManager.scriptBlocks.Clear();
		BWSceneManager.scriptBlocksArr = null;
	}

	// Token: 0x06001DB1 RID: 7601 RVA: 0x000D4274 File Offset: 0x000D2674
	public static void MakeFixedAndSpawnpointBeforeFirstFrame()
	{
		for (int i = 0; i < BWSceneManager.blocks.Count; i++)
		{
			BWSceneManager.blocks[i].MakeFixedAndSpawnpointBeforeFirstFrame();
		}
	}

	// Token: 0x06001DB2 RID: 7602 RVA: 0x000D42AC File Offset: 0x000D26AC
	public static void ExecutePlayBlocksUpdate()
	{
		int num = BWSceneManager.playBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			BWSceneManager.playBlocksArr[i].Update();
		}
	}

	// Token: 0x06001DB3 RID: 7603 RVA: 0x000D42E0 File Offset: 0x000D26E0
	public static void ExecuteAllBlocksUpdate()
	{
		int count = BWSceneManager.blocks.Count;
		for (int i = 0; i < count; i++)
		{
			Block block = BWSceneManager.blocks[i];
			block.Update();
		}
	}

	// Token: 0x06001DB4 RID: 7604 RVA: 0x000D431C File Offset: 0x000D271C
	public static void ExecuteStop(bool resetBlocks)
	{
		string text = null;
		for (int i = 0; i < BWSceneManager.blocks.Count; i++)
		{
			Block block = BWSceneManager.blocks[i];
			try
			{
				block.Stop(resetBlocks);
			}
			catch (Exception ex)
			{
				if (text == null)
				{
					text = ex.Message;
				}
			}
		}
		if (resetBlocks) {
			foreach (Block b in tempBlocks) {
				RemoveBlock(b);
				//RemovePlayBlock(b);
				UnityEngine.Object.Destroy(b.go);
			}
			tempBlocks.Clear();
		}
		if (text != null)
		{
			BWLog.Error("Got an exception calling Stop on blocks: '" + text + "'");
		}
	}

	// Token: 0x06001DB5 RID: 7605 RVA: 0x000D439C File Offset: 0x000D279C
	private static void PausePlayBlockSimple(Block b)
	{
		b.Pause();
		GameObject go = b.go;
		if (go != null)
		{
			Transform parent = go.transform.parent;
			if (parent != null)
			{
				GameObject gameObject = parent.gameObject;
				if (gameObject.GetComponent<Rigidbody>() != null && !gameObject.GetComponent<Rigidbody>().isKinematic)
				{
					BWSceneManager.pausedVelocity.Add(gameObject.GetComponent<Rigidbody>().velocity);
					BWSceneManager.pausedAngularVelocity.Add(gameObject.GetComponent<Rigidbody>().angularVelocity);
					BWSceneManager.pausedNonKinematic.Add(gameObject);
					gameObject.GetComponent<Rigidbody>().isKinematic = true;
				}
			}
		}
	}

	// Token: 0x06001DB6 RID: 7606 RVA: 0x000D4444 File Offset: 0x000D2844
	public static void PausePlayBlock(Block b)
	{
		BWSceneManager.PausePlayBlockSimple(b);
		BlockAnimatedCharacter blockAnimatedCharacter = b as BlockAnimatedCharacter;
		if (blockAnimatedCharacter != null)
		{
			BlockAnimatedCharacter.stateControllers[blockAnimatedCharacter].Freeze();
		}
	}

	// Token: 0x06001DB7 RID: 7607 RVA: 0x000D4474 File Offset: 0x000D2874
	public static void PauseBlocks()
	{
		BWSceneManager.pausedNonKinematic.Clear();
		BWSceneManager.pausedVelocity.Clear();
		BWSceneManager.pausedAngularVelocity.Clear();
		for (int i = 0; i < BWSceneManager.playBlocksArr.Length; i++)
		{
			BWSceneManager.PausePlayBlockSimple(BWSceneManager.playBlocksArr[i]);
		}
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> keyValuePair in BlockAnimatedCharacter.stateControllers)
		{
			keyValuePair.Value.Freeze();
		}
	}

	// Token: 0x06001DB8 RID: 7608 RVA: 0x000D4518 File Offset: 0x000D2918
	public static void UnPausePlayBlock(Block b)
	{
		BlockAnimatedCharacter blockAnimatedCharacter = b as BlockAnimatedCharacter;
		if (blockAnimatedCharacter != null)
		{
			BlockAnimatedCharacter.stateControllers[blockAnimatedCharacter].Unfreeze();
		}
		if (b.go != null && b.go.transform.parent != null)
		{
			GameObject gameObject = b.go.transform.parent.gameObject;
			for (int i = 0; i < BWSceneManager.pausedNonKinematic.Count; i++)
			{
				if (gameObject == BWSceneManager.pausedNonKinematic[i])
				{
					Rigidbody component = gameObject.GetComponent<Rigidbody>();
					if (component != null)
					{
						component.isKinematic = false;
						component.velocity = BWSceneManager.pausedVelocity[i];
						component.angularVelocity = BWSceneManager.pausedAngularVelocity[i];
					}
					BWSceneManager.pausedNonKinematic.RemoveAt(i);
					BWSceneManager.pausedVelocity.RemoveAt(i);
					BWSceneManager.pausedAngularVelocity.RemoveAt(i);
					break;
				}
			}
		}
		b.Resume();
	}

	// Token: 0x06001DB9 RID: 7609 RVA: 0x000D4620 File Offset: 0x000D2A20
	public static void UnpauseBlocks()
	{
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> keyValuePair in BlockAnimatedCharacter.stateControllers)
		{
			keyValuePair.Value.Unfreeze();
		}
		for (int i = 0; i < BWSceneManager.pausedNonKinematic.Count; i++)
		{
			GameObject gameObject = BWSceneManager.pausedNonKinematic[i];
			if (gameObject != null)
			{
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.isKinematic = false;
					component.velocity = BWSceneManager.pausedVelocity[i];
					component.angularVelocity = BWSceneManager.pausedAngularVelocity[i];
				}
			}
		}
		BWSceneManager.pausedNonKinematic.Clear();
		BWSceneManager.pausedVelocity.Clear();
		BWSceneManager.pausedAngularVelocity.Clear();
		for (int j = 0; j < BWSceneManager.playBlocksArr.Length; j++)
		{
			BWSceneManager.playBlocksArr[j].Resume();
		}
	}

	// Token: 0x06001DBA RID: 7610 RVA: 0x000D473C File Offset: 0x000D2B3C
	public static void ResumeScene()
	{
		for (int i = 0; i < BWSceneManager.pausedNonKinematic.Count; i++)
		{
			GameObject gameObject = BWSceneManager.pausedNonKinematic[i];
			gameObject.GetComponent<Rigidbody>().isKinematic = false;
			gameObject.GetComponent<Rigidbody>().velocity = BWSceneManager.pausedVelocity[i];
			gameObject.GetComponent<Rigidbody>().angularVelocity = BWSceneManager.pausedAngularVelocity[i];
		}
		for (int j = 0; j < BWSceneManager.blocks.Count; j++)
		{
			BWSceneManager.blocks[j].Resume();
		}
	}

	// Token: 0x06001DBB RID: 7611 RVA: 0x000D47D4 File Offset: 0x000D2BD4
	private static List<Block> RemoveAllCopy(List<Block> list, HashSet<Block> toRemove, Action<Block> action = null)
	{
		List<Block> list2 = new List<Block>();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (!toRemove.Contains(block))
			{
				list2.Add(block);
			}
			else if (action != null)
			{
				action(block);
			}
		}
		return list2;
	}

	// Token: 0x06001DBC RID: 7612 RVA: 0x000D482C File Offset: 0x000D2C2C
	public static void ModifyPlayAndScriptBlocks()
	{
		bool flag = false;
		bool flag2 = false;
		if (BWSceneManager.playBlocksToRemove.Count > 0)
		{
			foreach (Block block in BWSceneManager.playBlocksToRemove)
			{
				for (int i = 0; i < BWSceneManager.playBlocks.Count; i++)
				{
					BWSceneManager.playBlocks[i].RemovedPlayBlock(block);
				}
				BWSceneManager.playBlocksRemoved.Add(block);
				Blocksworld.UI.Controls.ScriptBlockRemoved(block);
				Block.connectedCache.Remove(block);
				Block.connectedChunks.Remove(block);
				Chunk chunk = block.chunk;
				chunk.RemoveBlock(block);
				if (chunk.blocks.Count == 0)
				{
					Blocksworld.blocksworldCamera.Unfollow(chunk);
					Blocksworld.chunks.Remove(chunk);
					chunk.Destroy(false);
				}
				if (block.go != null)
				{
					block.go.SetActive(false);
				}
				if (block.goShadow != null)
				{
					block.goShadow.GetComponent<Renderer>().enabled = false;
				}
			}
			BWSceneManager.resetFrameBlocks = BWSceneManager.RemoveAllCopy(BWSceneManager.resetFrameBlocks, BWSceneManager.playBlocksToRemove, null);
			BWSceneManager.playBlocks = BWSceneManager.RemoveAllCopy(BWSceneManager.playBlocks, BWSceneManager.playBlocksToRemove, null);
			BWSceneManager.scriptBlocks = BWSceneManager.RemoveAllCopy(BWSceneManager.scriptBlocks, BWSceneManager.playBlocksToRemove, null);
			BWSceneManager.playBlocksToRemove.Clear();
			flag2 = true;
			flag = true;
		}
		if (BWSceneManager.scriptBlocksToRemove.Count > 0)
		{
			BWSceneManager.scriptBlocks = BWSceneManager.RemoveAllCopy(BWSceneManager.scriptBlocks, BWSceneManager.scriptBlocksToRemove, delegate(Block b)
			{
				Blocksworld.UI.Controls.ScriptBlockRemoved(b);
				if (b is BlockAnimatedCharacter)
				{
					BlockAnimatedCharacter blockAnimatedCharacter = b as BlockAnimatedCharacter;
					blockAnimatedCharacter.stateHandler.InterruptState(CharacterState.Idle, false);
				}
			});
			BWSceneManager.scriptBlocksToRemove.Clear();
			flag = true;
		}
		if (BWSceneManager.scriptBlocksToAdd.Count > 0)
		{
			BWSceneManager.scriptBlocks.AddRange(BWSceneManager.scriptBlocksToAdd);
			BWSceneManager.scriptBlocksToAdd.Clear();
			flag = true;
		}
		if (flag)
		{
			BWSceneManager.scriptBlocksArr = BWSceneManager.scriptBlocks.ToArray();
		}
		if (flag2)
		{
			BWSceneManager.playBlocksArr = BWSceneManager.playBlocks.ToArray();
		}
	}

	// Token: 0x06001DBD RID: 7613 RVA: 0x000D4A78 File Offset: 0x000D2E78
	public static void RunFirstFrameActions()
	{
		int num = BWSceneManager.scriptBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			BWSceneManager.scriptBlocksArr[i].RunFirstFrameActions();
		}
	}

	// Token: 0x06001DBE RID: 7614 RVA: 0x000D4AAC File Offset: 0x000D2EAC
	public static void RunConditions()
	{
		int num = BWSceneManager.scriptBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			BWSceneManager.scriptBlocksArr[i].RunConditions();
		}
	}

	// Token: 0x06001DBF RID: 7615 RVA: 0x000D4AE0 File Offset: 0x000D2EE0
	public static void RunActions()
	{
		int num = BWSceneManager.scriptBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			BWSceneManager.scriptBlocksArr[i].RunActions();
		}
	}

	// Token: 0x06001DC0 RID: 7616 RVA: 0x000D4B14 File Offset: 0x000D2F14
	public static void RunFixedUpdate()
	{
		int num = BWSceneManager.playBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			BWSceneManager.playBlocksArr[i].FixedUpdate();
		}
	}

	// Token: 0x06001DC1 RID: 7617 RVA: 0x000D4B48 File Offset: 0x000D2F48
	public static void OnHudMesh()
	{
		int num = BWSceneManager.scriptBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			BWSceneManager.scriptBlocksArr[i].OnHudMesh();
		}
	}

	// Token: 0x06001DC2 RID: 7618 RVA: 0x000D4B7C File Offset: 0x000D2F7C
	public static void RemoveBlockInstanceIDs(GameObject go)
	{
		BWSceneManager.blockMap.Remove(go.GetInstanceID());
		IEnumerator enumerator = go.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				BWSceneManager.childGoBlockMap.Remove(transform.gameObject.GetInstanceID());
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
	}

	// Token: 0x06001DC3 RID: 7619 RVA: 0x000D4C04 File Offset: 0x000D3004
	public static void AddChildBlockInstanceID(GameObject go, Block b)
	{
		BWSceneManager.childGoBlockMap[go.GetInstanceID()] = b;
	}

	// Token: 0x06001DC4 RID: 7620 RVA: 0x000D4C17 File Offset: 0x000D3017
	public static void RemoveChildBlockInstanceID(GameObject go)
	{
		BWSceneManager.childGoBlockMap.Remove(go.GetInstanceID());
	}

	// Token: 0x06001DC5 RID: 7621 RVA: 0x000D4C2A File Offset: 0x000D302A
	public static bool ContainsPlayBlockPredicate(Block b)
	{
		return BWSceneManager.allBlockPlayPredicates.ContainsKey(b);
	}

	// Token: 0x06001DC6 RID: 7622 RVA: 0x000D4C37 File Offset: 0x000D3037
	public static HashSet<Predicate> PlayBlockPredicates(Block b)
	{
		return BWSceneManager.allBlockPlayPredicates[b];
	}

	// Token: 0x06001DC7 RID: 7623 RVA: 0x000D4C44 File Offset: 0x000D3044
	public static void ApplyRuntimeSubstution(Block b)
	{
		if (BWSceneManager._runtimeBlockSubstitutions.ContainsKey(b))
		{
			BWSceneManager._runtimeBlockSubstitutions[b].ApplySubstitute();
		}
	}

	// Token: 0x06001DC8 RID: 7624 RVA: 0x000D4C66 File Offset: 0x000D3066
	public static void BlockDestroyed(Block b)
	{
		if (BWSceneManager._runtimeBlockSubstitutions.ContainsKey(b))
		{
			BWSceneManager._runtimeBlockSubstitutions.Remove(b);
		}
	}

	// Token: 0x06001DC9 RID: 7625 RVA: 0x000D4C84 File Offset: 0x000D3084
	public static void ClearBlocks()
	{
		BWSceneManager.blocks.Clear();
		BWSceneManager.blockMap.Clear();
		BWSceneManager.childGoBlockMap.Clear();
		BWSceneManager.pausedNonKinematic.Clear();
		BWSceneManager.pausedAngularVelocity.Clear();
		BWSceneManager.pausedVelocity.Clear();
		BWSceneManager.playBlocks.Clear();
		BWSceneManager.playBlocksArr = null;
		BWSceneManager.scriptBlocks.Clear();
		BWSceneManager.scriptBlocksArr = null;
		BWSceneManager.playBlocksToRemove.Clear();
		BWSceneManager.playBlocksRemoved.Clear();
		BWSceneManager.scriptBlocksToAdd.Clear();
		BWSceneManager.scriptBlocksToRemove.Clear();
	}

	// Token: 0x06001DCA RID: 7626 RVA: 0x000D4D15 File Offset: 0x000D3115
	public static void Cleanup()
	{
		BWSceneManager._runtimeBlockSubstitutions.Clear();
	}

	// Token: 0x06001DCB RID: 7627 RVA: 0x000D4D21 File Offset: 0x000D3121
	public static void RemovePlayBlock(Block b)
	{
		BWSceneManager.playBlocksToRemove.Add(b);
	}

	// Token: 0x06001DCC RID: 7628 RVA: 0x000D4D2F File Offset: 0x000D312F
	public static void RemoveScriptBlock(Block b)
	{
		BWSceneManager.scriptBlocksToRemove.Add(b);
	}

	// Token: 0x06001DCD RID: 7629 RVA: 0x000D4D3D File Offset: 0x000D313D
	public static void AddScriptBlock(Block b)
	{
		BWSceneManager.scriptBlocksToAdd.Add(b);
	}

	public static void AddTempBlock(Block b) {
		AddBlock(b);
		tempBlocks.Add(b);
	}

	// Token: 0x06001DCE RID: 7630 RVA: 0x000D4D4C File Offset: 0x000D314C
	public static void AddBlock(Block b)
	{
		BWSceneManager.blocks.Add(b);
		BWSceneManager.blockMap[b.go.GetInstanceID()] = b;
		IEnumerator enumerator = b.goT.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				BWSceneManager.childGoBlockMap[transform.gameObject.GetInstanceID()] = b;
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
		if (b.HasDynamicalLight() && !Blocksworld.dynamicalLightChangers.Contains(b))
		{
			Blocksworld.dynamicalLightChangers.Add(b);
		}
		Block.ClearConnectedCache();
	}

	// Token: 0x06001DCF RID: 7631 RVA: 0x000D4E0C File Offset: 0x000D320C
	public static void AddBlockMap(GameObject go, Block b)
	{
		BWSceneManager.blockMap[go.GetInstanceID()] = b;
	}

	// Token: 0x06001DD0 RID: 7632 RVA: 0x000D4E1F File Offset: 0x000D321F
	public static void RemoveBlockMap(GameObject go)
	{
		BWSceneManager.blockMap.Remove(go.GetInstanceID());
	}

	// Token: 0x06001DD1 RID: 7633 RVA: 0x000D4E34 File Offset: 0x000D3234
	public static void RemoveBlock(Block b)
	{
		if (b == null)
		{
			return;
		}
		ConnectednessGraph.Remove(b);
		BWSceneManager.blocks.Remove(b);
		if (b.go != null)
		{
			b.RemoveBlockMaps();
			Blocksworld.dynamicalLightChangers.Remove(b);
			if (b is BlockTerrain)
			{
				BWSceneManager.RemoveTerrainBlock((BlockTerrain)b);
			}
			else if (b is BlockWaterCube)
			{
				BlockAbstractWater.waterCubes.Remove((BlockWaterCube)b);
			}
			Blocksworld.editorSelectionLocked.Remove(b);
		}
		Blocksworld.blockNames.Remove(b);
		BlockGrouped blockGrouped = b as BlockGrouped;
		if (blockGrouped != null && blockGrouped.group != null)
		{
			bool flag = false;
			foreach (Block block in blockGrouped.group.GetBlocks())
			{
				if (block != b && block != null && block.go != null && BWSceneManager.blockMap.ContainsKey(block.GetInstanceId()))
				{
					BWSceneManager.RemoveBlock(block);
					flag = true;
				}
			}
			if (flag)
			{
				BlockGroups.RemoveGroup(blockGrouped.group);
			}
		}
	}

	// Token: 0x06001DD2 RID: 7634 RVA: 0x000D4F5C File Offset: 0x000D335C
	public static List<Block> AllBlocks()
	{
		return BWSceneManager.blocks;
	}

	// Token: 0x06001DD3 RID: 7635 RVA: 0x000D4F63 File Offset: 0x000D3363
	public static int IndexOfBlock(Block b)
	{
		return BWSceneManager.blocks.IndexOf(b);
	}

	// Token: 0x06001DD4 RID: 7636 RVA: 0x000D4F70 File Offset: 0x000D3370
	public static void AddTerrainBlock(BlockTerrain terrain)
	{
		BWSceneManager.terrainBlocks.Add(terrain);
	}

	// Token: 0x06001DD5 RID: 7637 RVA: 0x000D4F7D File Offset: 0x000D337D
	public static void ClearTerrainBlocks()
	{
		BWSceneManager.terrainBlocks.Clear();
	}

	// Token: 0x06001DD6 RID: 7638 RVA: 0x000D4F89 File Offset: 0x000D3389
	public static void RemoveTerrainBlock(BlockTerrain terrain)
	{
		BWSceneManager.terrainBlocks.Remove(terrain);
	}

	// Token: 0x06001DD7 RID: 7639 RVA: 0x000D4F97 File Offset: 0x000D3397
	public static List<BlockTerrain> AllTerrainBlocks()
	{
		return BWSceneManager.terrainBlocks;
	}

	// Token: 0x06001DD8 RID: 7640 RVA: 0x000D4FA0 File Offset: 0x000D33A0
	public static List<Block> NonTerrainBlocks()
	{
		List<Block> list = new List<Block>();
		foreach (Block block in BWSceneManager.blocks)
		{
			if (!block.isTerrain)
			{
				list.Add(block);
			}
		}
		return list;
	}

	// Token: 0x06001DD9 RID: 7641 RVA: 0x000D5010 File Offset: 0x000D3410
	public static Dictionary<int, Block> BlockInstanceIds()
	{
		return BWSceneManager.blockMap;
	}

	// Token: 0x06001DDA RID: 7642 RVA: 0x000D5017 File Offset: 0x000D3417
	public static int BlockCount()
	{
		return BWSceneManager.blocks.Count;
	}

	// Token: 0x06001DDB RID: 7643 RVA: 0x000D5024 File Offset: 0x000D3424
	public static List<Block> BlocksAtPosition(Vector3 pos)
	{
		List<Block> list = new List<Block>();
		foreach (Block block in BWSceneManager.blocks)
		{
			if (block.goT.position == pos)
			{
				list.Add(block);
			}
		}
		return list;
	}

	// Token: 0x06001DDC RID: 7644 RVA: 0x000D509C File Offset: 0x000D349C
	public static Block FindBlock(GameObject go, bool checkChildGos = false)
	{
		int instanceID = go.GetInstanceID();
		Block result;
		if (BWSceneManager.blockMap.TryGetValue(instanceID, out result))
		{
			return result;
		}
		if (checkChildGos && BWSceneManager.childGoBlockMap.TryGetValue(instanceID, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06001DDD RID: 7645 RVA: 0x000D50E0 File Offset: 0x000D34E0
	public static Block FindBlockOfType(string type)
	{
		for (int i = 0; i < BWSceneManager.blocks.Count; i++)
		{
			Block block = BWSceneManager.blocks[i];
			if (block.BlockType() == type)
			{
				return block;
			}
		}
		return null;
	}

	// Token: 0x06001DDE RID: 7646 RVA: 0x000D5128 File Offset: 0x000D3528
	public static Tile FindTileInBlocks(GAF gaf)
	{
		for (int i = 0; i < BWSceneManager.blocks.Count; i++)
		{
			Block block = BWSceneManager.blocks[i];
			foreach (List<Tile> list in block.tiles)
			{
				foreach (Tile tile in list)
				{
					if (tile.gaf.Equals(gaf))
					{
						return tile;
					}
				}
			}
		}
		return null;
	}

	// Token: 0x04001828 RID: 6184
	private static Dictionary<Block, BlockSubstitution> _runtimeBlockSubstitutions = new Dictionary<Block, BlockSubstitution>();

	// Token: 0x04001829 RID: 6185
	private static HashSet<Predicate> allPlayPredicates = new HashSet<Predicate>();

	// Token: 0x0400182A RID: 6186
	private static Dictionary<Block, HashSet<Predicate>> allBlockPlayPredicates = new Dictionary<Block, HashSet<Predicate>>();

	// Token: 0x0400182B RID: 6187
	private static List<Block> blocks = new List<Block>();

	// Token: 0x0400182C RID: 6188
	private static List<BlockTerrain> terrainBlocks = new List<BlockTerrain>();

	// Token: 0x0400182D RID: 6189
	public static Dictionary<int, Block> blockMap = new Dictionary<int, Block>();

	// Token: 0x0400182E RID: 6190
	public static Dictionary<int, Block> childGoBlockMap = new Dictionary<int, Block>();

	// Token: 0x0400182F RID: 6191
	private static List<Block> playBlocks = new List<Block>();

	// Token: 0x04001830 RID: 6192
	private static Block[] playBlocksArr;

	// Token: 0x04001831 RID: 6193
	private static HashSet<Block> playBlocksToRemove = new HashSet<Block>();

	// Token: 0x04001832 RID: 6194
	public static HashSet<Block> playBlocksRemoved = new HashSet<Block>();

	// Token: 0x04001833 RID: 6195
	private static List<Block> resetFrameBlocks = new List<Block>();

	// Token: 0x04001834 RID: 6196
	private static List<Block> scriptBlocks = new List<Block>();

	private static List<Block> tempBlocks = new List<Block>();

	// Token: 0x04001835 RID: 6197
	private static Block[] scriptBlocksArr;

	// Token: 0x04001836 RID: 6198
	private static HashSet<Block> scriptBlocksToRemove = new HashSet<Block>();

	// Token: 0x04001837 RID: 6199
	private static List<Block> scriptBlocksToAdd = new List<Block>();

	// Token: 0x04001838 RID: 6200
	public static List<GameObject> pausedNonKinematic = new List<GameObject>();

	// Token: 0x04001839 RID: 6201
	public static List<Vector3> pausedVelocity = new List<Vector3>();

	// Token: 0x0400183A RID: 6202
	public static List<Vector3> pausedAngularVelocity = new List<Vector3>();
}
