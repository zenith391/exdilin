using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public static class BWSceneManager
{
	private static Dictionary<Block, BlockSubstitution> _runtimeBlockSubstitutions;

	private static HashSet<Predicate> allPlayPredicates;

	private static Dictionary<Block, HashSet<Predicate>> allBlockPlayPredicates;

	private static List<Block> blocks;

	private static List<BlockTerrain> terrainBlocks;

	public static Dictionary<int, Block> blockMap;

	public static Dictionary<int, Block> childGoBlockMap;

	private static List<Block> playBlocks;

	private static Block[] playBlocksArr;

	private static HashSet<Block> playBlocksToRemove;

	public static HashSet<Block> playBlocksRemoved;

	private static List<Block> resetFrameBlocks;

	private static List<Block> scriptBlocks;

	private static List<Block> tempBlocks;

	private static Block[] scriptBlocksArr;

	private static HashSet<Block> scriptBlocksToRemove;

	private static List<Block> scriptBlocksToAdd;

	public static List<GameObject> pausedNonKinematic;

	public static List<Vector3> pausedVelocity;

	public static List<Vector3> pausedAngularVelocity;

	public static void RegisterRuntimeBlockSubstitution(Block block, BlockSubstitution substitution)
	{
		_runtimeBlockSubstitutions[block] = substitution;
	}

	public static void InitPlay()
	{
		foreach (BlockSubstitution value in _runtimeBlockSubstitutions.Values)
		{
			value.ApplySubstitute();
		}
		ResetPlayBlocksAndPredicates();
	}

	public static void ResetPlayPredicates()
	{
		allBlockPlayPredicates.Clear();
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			HashSet<Predicate> playPredicates = block.GetPlayPredicates();
			allBlockPlayPredicates[block] = playPredicates;
		}
	}

	public static void ResetPlayBlocksAndPredicates()
	{
		playBlocksRemoved.Clear();
		playBlocks.Clear();
		playBlocks.AddRange(blocks);
		allPlayPredicates.Clear();
		allBlockPlayPredicates.Clear();
		resetFrameBlocks.Clear();
		for (int i = 0; i < playBlocks.Count; i++)
		{
			Block block = playBlocks[i];
			if (block.GetType() != typeof(Block))
			{
				resetFrameBlocks.Add(block);
			}
			HashSet<Predicate> playPredicates = block.GetPlayPredicates();
			allBlockPlayPredicates[block] = playPredicates;
			allPlayPredicates.UnionWith(playPredicates);
		}
	}

	public static void ResetFrame()
	{
		int count = resetFrameBlocks.Count;
		for (int i = 0; i < count; i++)
		{
			resetFrameBlocks[i].ResetFrame();
		}
	}

	public static void BeforePlay()
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			block.CheckContainsPlayModeTiles();
			if (block.containsPlayModeTiles)
			{
				block.CreateFlattenTiles();
			}
			block.BeforePlay();
		}
	}

	public static void ExecutePlay()
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			block.Play();
			if (block.containsPlayModeTiles)
			{
				scriptBlocks.Add(block);
			}
		}
		scriptBlocksArr = scriptBlocks.ToArray();
		playBlocksArr = playBlocks.ToArray();
	}

	public static void ExecutePlay2()
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			blocks[i].Play2();
		}
	}

	public static void ExecutePlayLegs()
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block is BlockAbstractLegs)
			{
				((BlockAbstractLegs)block).PlayLegs1();
			}
		}
		for (int j = 0; j < blocks.Count; j++)
		{
			Block block2 = blocks[j];
			if (block2 is BlockAbstractLegs)
			{
				((BlockAbstractLegs)block2).PlayLegs2();
			}
		}
	}

	public static void StopPlay()
	{
		foreach (BlockSubstitution value in _runtimeBlockSubstitutions.Values)
		{
			value.RestoreOriginal();
		}
		playBlocks.Clear();
		playBlocksArr = null;
		resetFrameBlocks.Clear();
		scriptBlocks.Clear();
		scriptBlocksArr = null;
		allPlayPredicates.Clear();
		allBlockPlayPredicates.Clear();
	}

	public static void ClearScriptBlocks()
	{
		scriptBlocks.Clear();
		scriptBlocksArr = null;
	}

	public static void MakeFixedAndSpawnpointBeforeFirstFrame()
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			blocks[i].MakeFixedAndSpawnpointBeforeFirstFrame();
		}
	}

	public static void ExecutePlayBlocksUpdate()
	{
		int num = playBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			playBlocksArr[i].Update();
		}
	}

	public static void ExecuteAllBlocksUpdate()
	{
		int count = blocks.Count;
		for (int i = 0; i < count; i++)
		{
			blocks[i].Update();
		}
	}

	public static void ExecuteStop(bool resetBlocks)
	{
		string text = null;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
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
		if (resetBlocks)
		{
			foreach (Block tempBlock in tempBlocks)
			{
				RemoveBlock(tempBlock);
				UnityEngine.Object.Destroy(tempBlock.go);
			}
			tempBlocks.Clear();
		}
		if (text != null)
		{
			BWLog.Error("Got an exception calling Stop on blocks: '" + text + "'");
		}
	}

	private static void PausePlayBlockSimple(Block b)
	{
		b.Pause();
		GameObject go = b.go;
		if (!(go != null))
		{
			return;
		}
		Transform parent = go.transform.parent;
		if (parent != null)
		{
			GameObject gameObject = parent.gameObject;
			if (gameObject.GetComponent<Rigidbody>() != null && !gameObject.GetComponent<Rigidbody>().isKinematic)
			{
				pausedVelocity.Add(gameObject.GetComponent<Rigidbody>().velocity);
				pausedAngularVelocity.Add(gameObject.GetComponent<Rigidbody>().angularVelocity);
				pausedNonKinematic.Add(gameObject);
				gameObject.GetComponent<Rigidbody>().isKinematic = true;
			}
		}
	}

	public static void PausePlayBlock(Block b)
	{
		PausePlayBlockSimple(b);
		if (b is BlockAnimatedCharacter key)
		{
			BlockAnimatedCharacter.stateControllers[key].Freeze();
		}
	}

	public static void PauseBlocks()
	{
		pausedNonKinematic.Clear();
		pausedVelocity.Clear();
		pausedAngularVelocity.Clear();
		for (int i = 0; i < playBlocksArr.Length; i++)
		{
			PausePlayBlockSimple(playBlocksArr[i]);
		}
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> stateController in BlockAnimatedCharacter.stateControllers)
		{
			stateController.Value.Freeze();
		}
	}

	public static void UnPausePlayBlock(Block b)
	{
		if (b is BlockAnimatedCharacter key)
		{
			BlockAnimatedCharacter.stateControllers[key].Unfreeze();
		}
		if (b.go != null && b.go.transform.parent != null)
		{
			GameObject gameObject = b.go.transform.parent.gameObject;
			for (int i = 0; i < pausedNonKinematic.Count; i++)
			{
				if (gameObject == pausedNonKinematic[i])
				{
					Rigidbody component = gameObject.GetComponent<Rigidbody>();
					if (component != null)
					{
						component.isKinematic = false;
						component.velocity = pausedVelocity[i];
						component.angularVelocity = pausedAngularVelocity[i];
					}
					pausedNonKinematic.RemoveAt(i);
					pausedVelocity.RemoveAt(i);
					pausedAngularVelocity.RemoveAt(i);
					break;
				}
			}
		}
		b.Resume();
	}

	public static void UnpauseBlocks()
	{
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> stateController in BlockAnimatedCharacter.stateControllers)
		{
			stateController.Value.Unfreeze();
		}
		for (int i = 0; i < pausedNonKinematic.Count; i++)
		{
			GameObject gameObject = pausedNonKinematic[i];
			if (gameObject != null)
			{
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.isKinematic = false;
					component.velocity = pausedVelocity[i];
					component.angularVelocity = pausedAngularVelocity[i];
				}
			}
		}
		pausedNonKinematic.Clear();
		pausedVelocity.Clear();
		pausedAngularVelocity.Clear();
		for (int j = 0; j < playBlocksArr.Length; j++)
		{
			playBlocksArr[j].Resume();
		}
	}

	public static void ResumeScene()
	{
		for (int i = 0; i < pausedNonKinematic.Count; i++)
		{
			GameObject gameObject = pausedNonKinematic[i];
			gameObject.GetComponent<Rigidbody>().isKinematic = false;
			gameObject.GetComponent<Rigidbody>().velocity = pausedVelocity[i];
			gameObject.GetComponent<Rigidbody>().angularVelocity = pausedAngularVelocity[i];
		}
		for (int j = 0; j < blocks.Count; j++)
		{
			blocks[j].Resume();
		}
	}

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
			else
			{
				action?.Invoke(block);
			}
		}
		return list2;
	}

	public static void ModifyPlayAndScriptBlocks()
	{
		bool flag = false;
		bool flag2 = false;
		if (playBlocksToRemove.Count > 0)
		{
			foreach (Block item in playBlocksToRemove)
			{
				for (int i = 0; i < playBlocks.Count; i++)
				{
					playBlocks[i].RemovedPlayBlock(item);
				}
				playBlocksRemoved.Add(item);
				Blocksworld.UI.Controls.ScriptBlockRemoved(item);
				Block.connectedCache.Remove(item);
				Block.connectedChunks.Remove(item);
				Chunk chunk = item.chunk;
				chunk.RemoveBlock(item);
				if (chunk.blocks.Count == 0)
				{
					Blocksworld.blocksworldCamera.Unfollow(chunk);
					Blocksworld.chunks.Remove(chunk);
					chunk.Destroy();
				}
				if (item.go != null)
				{
					item.go.SetActive(value: false);
				}
				if (item.goShadow != null)
				{
					item.goShadow.GetComponent<Renderer>().enabled = false;
				}
			}
			resetFrameBlocks = RemoveAllCopy(resetFrameBlocks, playBlocksToRemove);
			playBlocks = RemoveAllCopy(playBlocks, playBlocksToRemove);
			scriptBlocks = RemoveAllCopy(scriptBlocks, playBlocksToRemove);
			playBlocksToRemove.Clear();
			flag2 = true;
			flag = true;
		}
		if (scriptBlocksToRemove.Count > 0)
		{
			scriptBlocks = RemoveAllCopy(scriptBlocks, scriptBlocksToRemove, delegate(Block b)
			{
				Blocksworld.UI.Controls.ScriptBlockRemoved(b);
				if (b is BlockAnimatedCharacter)
				{
					(b as BlockAnimatedCharacter).stateHandler.InterruptState(CharacterState.Idle, noBlend: false);
				}
			});
			scriptBlocksToRemove.Clear();
			flag = true;
		}
		if (scriptBlocksToAdd.Count > 0)
		{
			scriptBlocks.AddRange(scriptBlocksToAdd);
			scriptBlocksToAdd.Clear();
			flag = true;
		}
		if (flag)
		{
			scriptBlocksArr = scriptBlocks.ToArray();
		}
		if (flag2)
		{
			playBlocksArr = playBlocks.ToArray();
		}
	}

	public static void RunFirstFrameActions()
	{
		int num = scriptBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			scriptBlocksArr[i].RunFirstFrameActions();
		}
	}

	public static void RunConditions()
	{
		int num = scriptBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			scriptBlocksArr[i].RunConditions();
		}
	}

	public static void RunActions()
	{
		int num = scriptBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			scriptBlocksArr[i].RunActions();
		}
	}

	public static void RunFixedUpdate()
	{
		int num = playBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			playBlocksArr[i].FixedUpdate();
		}
	}

	public static void OnHudMesh()
	{
		int num = scriptBlocksArr.Length;
		for (int i = 0; i < num; i++)
		{
			scriptBlocksArr[i].OnHudMesh();
		}
	}

	public static void RemoveBlockInstanceIDs(GameObject go)
	{
		blockMap.Remove(go.GetInstanceID());
		foreach (Transform item in go.transform)
		{
			childGoBlockMap.Remove(item.gameObject.GetInstanceID());
		}
	}

	public static void AddChildBlockInstanceID(GameObject go, Block b)
	{
		childGoBlockMap[go.GetInstanceID()] = b;
	}

	public static void RemoveChildBlockInstanceID(GameObject go)
	{
		childGoBlockMap.Remove(go.GetInstanceID());
	}

	public static bool ContainsPlayBlockPredicate(Block b)
	{
		return allBlockPlayPredicates.ContainsKey(b);
	}

	public static HashSet<Predicate> PlayBlockPredicates(Block b)
	{
		return allBlockPlayPredicates[b];
	}

	public static void ApplyRuntimeSubstution(Block b)
	{
		if (_runtimeBlockSubstitutions.ContainsKey(b))
		{
			_runtimeBlockSubstitutions[b].ApplySubstitute();
		}
	}

	public static void BlockDestroyed(Block b)
	{
		if (_runtimeBlockSubstitutions.ContainsKey(b))
		{
			_runtimeBlockSubstitutions.Remove(b);
		}
	}

	public static void ClearBlocks()
	{
		blocks.Clear();
		blockMap.Clear();
		childGoBlockMap.Clear();
		pausedNonKinematic.Clear();
		pausedAngularVelocity.Clear();
		pausedVelocity.Clear();
		playBlocks.Clear();
		playBlocksArr = null;
		scriptBlocks.Clear();
		scriptBlocksArr = null;
		playBlocksToRemove.Clear();
		playBlocksRemoved.Clear();
		scriptBlocksToAdd.Clear();
		scriptBlocksToRemove.Clear();
	}

	public static void Cleanup()
	{
		_runtimeBlockSubstitutions.Clear();
	}

	public static void RemovePlayBlock(Block b)
	{
		playBlocksToRemove.Add(b);
	}

	public static void RemoveScriptBlock(Block b)
	{
		scriptBlocksToRemove.Add(b);
	}

	public static void AddScriptBlock(Block b)
	{
		scriptBlocksToAdd.Add(b);
	}

	public static void AddTempBlock(Block b)
	{
		AddBlock(b);
		tempBlocks.Add(b);
	}

	public static void AddBlock(Block b)
	{
		blocks.Add(b);
		blockMap[b.go.GetInstanceID()] = b;
		foreach (Transform item in b.goT)
		{
			childGoBlockMap[item.gameObject.GetInstanceID()] = b;
		}
		if (b.HasDynamicalLight() && !Blocksworld.dynamicalLightChangers.Contains(b))
		{
			Blocksworld.dynamicalLightChangers.Add(b);
		}
		Block.ClearConnectedCache();
	}

	public static void AddBlockMap(GameObject go, Block b)
	{
		blockMap[go.GetInstanceID()] = b;
	}

	public static void RemoveBlockMap(GameObject go)
	{
		blockMap.Remove(go.GetInstanceID());
	}

	public static void RemoveBlock(Block b)
	{
		if (b == null)
		{
			return;
		}
		ConnectednessGraph.Remove(b);
		blocks.Remove(b);
		if (b.go != null)
		{
			b.RemoveBlockMaps();
			Blocksworld.dynamicalLightChangers.Remove(b);
			if (b is BlockTerrain)
			{
				RemoveTerrainBlock((BlockTerrain)b);
			}
			else if (b is BlockWaterCube)
			{
				BlockAbstractWater.waterCubes.Remove((BlockWaterCube)b);
			}
			Blocksworld.editorSelectionLocked.Remove(b);
		}
		Blocksworld.blockNames.Remove(b);
		if (!(b is BlockGrouped { group: not null } blockGrouped))
		{
			return;
		}
		bool flag = false;
		Block[] array = blockGrouped.group.GetBlocks();
		foreach (Block block in array)
		{
			if (block != b && block != null && block.go != null && blockMap.ContainsKey(block.GetInstanceId()))
			{
				RemoveBlock(block);
				flag = true;
			}
		}
		if (flag)
		{
			BlockGroups.RemoveGroup(blockGrouped.group);
		}
	}

	public static List<Block> AllBlocks()
	{
		return blocks;
	}

	public static int IndexOfBlock(Block b)
	{
		return blocks.IndexOf(b);
	}

	public static void AddTerrainBlock(BlockTerrain terrain)
	{
		terrainBlocks.Add(terrain);
	}

	public static void ClearTerrainBlocks()
	{
		terrainBlocks.Clear();
	}

	public static void RemoveTerrainBlock(BlockTerrain terrain)
	{
		terrainBlocks.Remove(terrain);
	}

	public static List<BlockTerrain> AllTerrainBlocks()
	{
		return terrainBlocks;
	}

	public static List<Block> NonTerrainBlocks()
	{
		List<Block> list = new List<Block>();
		foreach (Block block in blocks)
		{
			if (!block.isTerrain)
			{
				list.Add(block);
			}
		}
		return list;
	}

	public static Dictionary<int, Block> BlockInstanceIds()
	{
		return blockMap;
	}

	public static int BlockCount()
	{
		return blocks.Count;
	}

	public static List<Block> BlocksAtPosition(Vector3 pos)
	{
		List<Block> list = new List<Block>();
		foreach (Block block in blocks)
		{
			if (block.goT.position == pos)
			{
				list.Add(block);
			}
		}
		return list;
	}

	public static Block FindBlock(GameObject go, bool checkChildGos = false)
	{
		int instanceID = go.GetInstanceID();
		if (blockMap.TryGetValue(instanceID, out var value))
		{
			return value;
		}
		if (checkChildGos && childGoBlockMap.TryGetValue(instanceID, out value))
		{
			return value;
		}
		return null;
	}

	public static Block FindBlockOfType(string type)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block.BlockType() == type)
			{
				return block;
			}
		}
		return null;
	}

	public static Tile FindTileInBlocks(GAF gaf)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			foreach (List<Tile> tile in blocks[i].tiles)
			{
				foreach (Tile item in tile)
				{
					if (item.gaf.Equals(gaf))
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	static BWSceneManager()
	{
		_runtimeBlockSubstitutions = new Dictionary<Block, BlockSubstitution>();
		allPlayPredicates = new HashSet<Predicate>();
		allBlockPlayPredicates = new Dictionary<Block, HashSet<Predicate>>();
		blocks = new List<Block>();
		terrainBlocks = new List<BlockTerrain>();
		blockMap = new Dictionary<int, Block>();
		childGoBlockMap = new Dictionary<int, Block>();
		playBlocks = new List<Block>();
		playBlocksToRemove = new HashSet<Block>();
		playBlocksRemoved = new HashSet<Block>();
		resetFrameBlocks = new List<Block>();
		scriptBlocks = new List<Block>();
		tempBlocks = new List<Block>();
		scriptBlocksToRemove = new HashSet<Block>();
		scriptBlocksToAdd = new List<Block>();
		pausedNonKinematic = new List<GameObject>();
		pausedVelocity = new List<Vector3>();
		pausedAngularVelocity = new List<Vector3>();
	}
}
