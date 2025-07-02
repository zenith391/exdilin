using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class CollisionManager
{
	private class CollisionEnterInfo
	{
		public GameObject go1;

		public GameObject go2;

		public Collision collision;

		public CollisionEnterInfo(GameObject go1, GameObject go2, Collision collision)
		{
			this.go1 = go1;
			this.go2 = go2;
			this.collision = collision;
		}

		public bool Same(CollisionEnterInfo cei)
		{
			if (cei.go1 == go1 && cei.go2 == go2)
			{
				return cei.collision == collision;
			}
			return false;
		}
	}

	public enum GhostBlockMode
	{
		NotGhost,
		Propagate,
		NoPropagate,
		TestUnGhost
	}

	public class GhostBlockInfo
	{
		public Block block;

		public bool didPropagate;

		public GhostBlockMode mode;

		public int counter;

		public int unGhostCounter;

		public string oldTexture;
	}

	private const int BUMP_COOLDOWN_STEPS = 3;

	private static bool collisionChanged = true;

	private static HashSet<long> collisionEnterKeys = new HashSet<long>();

	private static HashSet<int> ignoreTriggerIds = new HashSet<int>();

	private static int collisionEnterInfosCount = 0;

	private static List<CollisionEnterInfo> collisionEnterInfos = new List<CollisionEnterInfo>();

	private static List<CollisionEnterInfo> prevCollisionEnterInfos = new List<CollisionEnterInfo>();

	public static Dictionary<int, HashSet<string>> tagBumpBlocks = new Dictionary<int, HashSet<string>>();

	public static HashSet<Block> bumpedObject = new HashSet<Block>();

	public static HashSet<Block> modelBumpedObject = new HashSet<Block>();

	public static HashSet<Block> bumpedGround = new HashSet<Block>();

	public static HashSet<Block> modelBumpedGround = new HashSet<Block>();

	public static HashSet<Block> blocksOnGround = new HashSet<Block>();

	public static Dictionary<Block, HashSet<int>> particleCollideObject = new Dictionary<Block, HashSet<int>>();

	public static Dictionary<Block, HashSet<int>> modelParticleCollideObject = new Dictionary<Block, HashSet<int>>();

	public static Dictionary<Block, Block> bumpedBy = new Dictionary<Block, Block>();

	public static HashSet<GameObject> bumping = new HashSet<GameObject>();

	public static Dictionary<string, HashSet<GameObject>> triggering = new Dictionary<string, HashSet<GameObject>>();

	private static Dictionary<int, GhostBlockInfo> ghostBlocks = new Dictionary<int, GhostBlockInfo>();

	private static List<Vector4> wakeUpPosRads = new List<Vector4>();

	private const int unGhostInterval = 150;

	private static List<GhostBlockInfo> ghostsToRemove = new List<GhostBlockInfo>();

	public static GhostBlockInfo GetGhostBlockInfo(Block b)
	{
		if (ghostBlocks.TryGetValue(b.GetInstanceId(), out var value))
		{
			return value;
		}
		return null;
	}

	public static GhostBlockInfo SetGhostBlockMode(Block b, GhostBlockMode mode)
	{
		GhostBlockInfo ghostBlockInfo = GetGhostBlockInfo(b);
		if (ghostBlockInfo != null)
		{
			ghostBlockInfo.mode = mode;
		}
		switch (mode)
		{
		case GhostBlockMode.NotGhost:
			SetBlockLayer(b, Layer.Default);
			ghostBlocks.Remove(b.GetInstanceId());
			ghostBlockInfo = null;
			break;
		case GhostBlockMode.Propagate:
			if (ghostBlockInfo == null)
			{
				ghostBlockInfo = new GhostBlockInfo
				{
					block = b,
					mode = mode,
					didPropagate = false,
					counter = Random.Range(1, 10),
					oldTexture = b.GetTexture()
				};
				ghostBlocks[b.GetInstanceId()] = ghostBlockInfo;
			}
			SetBlockLayer(b, Layer.ChunkedBlock);
			break;
		case GhostBlockMode.NoPropagate:
			if (ghostBlockInfo == null)
			{
				ghostBlockInfo = new GhostBlockInfo
				{
					block = b,
					mode = mode,
					didPropagate = false,
					counter = Random.Range(1, 10),
					oldTexture = b.GetTexture()
				};
				ghostBlocks[b.GetInstanceId()] = ghostBlockInfo;
			}
			SetBlockLayer(b, Layer.ChunkedBlock);
			break;
		case GhostBlockMode.TestUnGhost:
			SetBlockLayer(b, Layer.Default);
			if (ghostBlockInfo == null)
			{
				BWLog.Info("Ghosted block should not be in mode " + mode.ToString() + " and not have a ghost info");
			}
			break;
		}
		return ghostBlockInfo;
	}

	public static GhostBlockMode GetGhostBlockMode(Block b)
	{
		if (ghostBlocks.TryGetValue(b.GetInstanceId(), out var value))
		{
			return value.mode;
		}
		return GhostBlockMode.NotGhost;
	}

	public static void SetBlockLayer(Block b, Layer layer)
	{
		b.go.SetLayer(layer, recursive: true);
		if (!(b is BlockAnimatedCharacter blockAnimatedCharacter))
		{
			return;
		}
		foreach (GameObject allBodyPartObject in blockAnimatedCharacter.GetAllBodyPartObjects())
		{
			if (allBodyPartObject.transform.parent == null)
			{
				allBodyPartObject.SetLayer(layer);
			}
		}
	}

	public static void AddIgnoreTriggerGO(GameObject go)
	{
		ignoreTriggerIds.Add(go.GetInstanceID());
	}

	public static void Play()
	{
		List<Block> list = BWSceneManager.AllBlocks();
		HashSet<Block> hashSet = new HashSet<Block>();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (!(block is BlockGrouped blockGrouped))
			{
				block.tagBumpEnabled = block.ContainsTagBump();
				if (block.tagBumpEnabled)
				{
					tagBumpBlocks[block.GetInstanceId()] = new HashSet<string>();
					hashSet.Add(block);
				}
			}
			else
			{
				if (!blockGrouped.IsMainBlockInGroup())
				{
					continue;
				}
				bool flag = block.ContainsTagBump();
				Block[] blocks = blockGrouped.group.GetBlocks();
				foreach (Block block2 in blocks)
				{
					block2.tagBumpEnabled = flag;
					if (flag)
					{
						tagBumpBlocks[block2.GetInstanceId()] = new HashSet<string>();
						hashSet.Add(block2);
					}
				}
			}
		}
		foreach (Block item in hashSet)
		{
			List<Block> list2;
			if (item.ContainsTileWithPredicateInPlayMode(Block.predicateTaggedBumpModel))
			{
				item.UpdateConnectedCache();
				list2 = Block.connectedCache[item];
			}
			else
			{
				if (!item.ContainsTileWithPredicateInPlayMode(Block.predicateTaggedBumpChunk))
				{
					continue;
				}
				list2 = item.chunk.blocks;
			}
			foreach (Block item2 in list2)
			{
				item2.tagBumpEnabled = true;
				tagBumpBlocks[item2.GetInstanceId()] = new HashSet<string>();
			}
		}
	}

	public static void Stop()
	{
		ResetState(force: true);
		ignoreTriggerIds.Clear();
		bumping.Clear();
		bumpedObject.Clear();
		modelBumpedObject.Clear();
		bumpedBy.Clear();
		bumpedGround.Clear();
		modelBumpedGround.Clear();
		particleCollideObject.Clear();
		modelParticleCollideObject.Clear();
		blocksOnGround.Clear();
		triggering.Clear();
		foreach (int item in new List<int>(ghostBlocks.Keys))
		{
			SetBlockLayer(ghostBlocks[item].block, Layer.Default);
		}
		ghostBlocks.Clear();
		ghostsToRemove.Clear();
	}

	private static void DebugGhostInfo(GhostBlockInfo info)
	{
		Vector3 position = info.block.go.transform.position;
		Color color = Color.white;
		switch (info.mode)
		{
		case GhostBlockMode.NotGhost:
			color = Color.green;
			break;
		case GhostBlockMode.Propagate:
			color = Color.red;
			break;
		case GhostBlockMode.NoPropagate:
			color = Color.white;
			break;
		case GhostBlockMode.TestUnGhost:
			color = Color.blue;
			break;
		}
		Debug.DrawLine(position, position + Vector3.up * 5f, color);
	}

	public static void FixedUpdate()
	{
		ExecuteWakeUps();
		bumping.Clear();
		if (ghostBlocks.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<int, GhostBlockInfo> ghostBlock in ghostBlocks)
		{
			GhostBlockInfo value = ghostBlock.Value;
			if (value.mode == GhostBlockMode.NoPropagate)
			{
				value.counter++;
				if (value.counter % 150 == 0)
				{
					SetGhostBlockMode(value.block, GhostBlockMode.TestUnGhost);
				}
			}
			else if (value.mode == GhostBlockMode.TestUnGhost)
			{
				value.unGhostCounter++;
				if (value.unGhostCounter > 5)
				{
					ghostsToRemove.Add(ghostBlock.Value);
				}
			}
		}
		if (ghostsToRemove.Count <= 0)
		{
			return;
		}
		foreach (GhostBlockInfo item in ghostsToRemove)
		{
			SetGhostBlockMode(item.block, GhostBlockMode.NotGhost);
		}
		ghostsToRemove.Clear();
	}

	public static void ResetState(bool force = false)
	{
		collisionChanged = collisionChanged || prevCollisionEnterInfos.Count > collisionEnterInfosCount;
		prevCollisionEnterInfos.Clear();
		collisionEnterKeys.Clear();
		particleCollideObject.Clear();
		modelParticleCollideObject.Clear();
		if (collisionChanged)
		{
			bumpedObject.Clear();
			modelBumpedObject.Clear();
			bumpedBy.Clear();
			bumpedGround.Clear();
			modelBumpedGround.Clear();
			foreach (KeyValuePair<int, HashSet<string>> tagBumpBlock in tagBumpBlocks)
			{
				tagBumpBlock.Value.Clear();
			}
			if (!force)
			{
				ExecuteForwardCollisionEnterInfos();
			}
		}
		if (!force)
		{
			for (int i = 0; i < collisionEnterInfosCount; i++)
			{
				prevCollisionEnterInfos.Add(collisionEnterInfos[i]);
			}
		}
		collisionEnterInfosCount = 0;
		if (force)
		{
			collisionEnterInfos.Clear();
			wakeUpPosRads.Clear();
		}
		collisionChanged = false;
	}

	private static void SetBumpedGround(Block b)
	{
		if (b != null)
		{
			bumpedGround.Add(b);
			modelBumpedGround.Add(b.modelBlock);
			blocksOnGround.Add(b);
		}
	}

	public static void WakeUpObjectsRestingOn(Block b)
	{
		if (Physics.gravity.sqrMagnitude > 0f)
		{
			Vector4 item = b.goT.position - Physics.gravity.normalized * 0.25f;
			item.w = b.GetBounds().extents.magnitude + 0.25f;
			wakeUpPosRads.Add(item);
		}
	}

	private static void ExecuteWakeUps()
	{
		int count = wakeUpPosRads.Count;
		if (count == 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			Vector4 vector = wakeUpPosRads[i];
			float w = vector.w;
			Vector3 position = vector;
			Collider[] array = Physics.OverlapSphere(position, w);
			for (int j = 0; j < array.Length; j++)
			{
				Rigidbody componentInParent = array[j].gameObject.GetComponentInParent<Rigidbody>();
				if (componentInParent != null)
				{
					componentInParent.WakeUp();
				}
			}
		}
		wakeUpPosRads.Clear();
	}

	public static void ExecuteForwardCollisionEnterInfos()
	{
		for (int i = 0; i < collisionEnterInfosCount; i++)
		{
			CollisionEnterInfo collisionEnterInfo = collisionEnterInfos[i];
			GameObject go = collisionEnterInfo.go1;
			GameObject go2 = collisionEnterInfo.go2;
			Collision collision = collisionEnterInfo.collision;
			Block block = BWSceneManager.FindBlock(go, checkChildGos: true);
			Block block2 = BWSceneManager.FindBlock(go2, checkChildGos: true);
			bool flag = block?.isTerrain ?? false;
			bool flag2 = block2?.isTerrain ?? false;
			if (flag && !flag2)
			{
				SetBumpedGround(block2);
				bumpedObject.Add(block);
				modelBumpedObject.Add(block.modelBlock);
			}
			else if (flag2 && !flag)
			{
				SetBumpedGround(block);
				bumpedObject.Add(block2);
				modelBumpedObject.Add(block2.modelBlock);
			}
			else if (flag && flag2)
			{
				SetBumpedGround(block);
				SetBumpedGround(block2);
			}
			else if (block != null)
			{
				bumpedObject.Add(block);
				modelBumpedObject.Add(block.modelBlock);
				bumpedBy[block] = ((block2 != null) ? block2 : block);
			}
			UpdateTaggedBumps(block, block2);
			UpdateTaggedBumps(block2, block);
			if (collision != null && Sound.sfxEnabled)
			{
				Sound.CollisionSFX(block, block2, go, go2, collision);
			}
			if (ghostBlocks.Count > 0 && !flag && !flag2 && block != null && block2 != null)
			{
				UpdateGhostBlock(block, block2);
				UpdateGhostBlock(block2, block);
			}
		}
	}

	private static void UpdateGhostBlock(Block b1, Block b2)
	{
		GhostBlockInfo ghostBlockInfo = GetGhostBlockInfo(b1);
		if (ghostBlockInfo == null)
		{
			return;
		}
		if (ghostBlockInfo.mode == GhostBlockMode.Propagate)
		{
			ghostBlockInfo.didPropagate = true;
			GhostBlockInfo ghostBlockInfo2 = GetGhostBlockInfo(b2);
			if (ghostBlockInfo2 == null || ghostBlockInfo2.mode != GhostBlockMode.Propagate)
			{
				SetGhostBlockMode(b2, GhostBlockMode.NoPropagate);
			}
		}
		else if (ghostBlockInfo.mode == GhostBlockMode.TestUnGhost)
		{
			GhostBlockInfo ghostBlockInfo3 = GetGhostBlockInfo(b2);
			if (ghostBlockInfo3 != null)
			{
				ghostBlockInfo.unGhostCounter = 1;
				SetGhostBlockMode(b1, GhostBlockMode.NoPropagate);
			}
		}
	}

	public static void ForwardCollisionEnter(GameObject go1, GameObject go2, Collision collision)
	{
		if (Blocksworld.CurrentState != State.Play || Blocksworld.playFixedUpdateCounter <= 3)
		{
			return;
		}
		long item = go1.GetInstanceID() + (long)go2.GetInstanceID() * 100000000L;
		if (collisionEnterKeys.Contains(item))
		{
			return;
		}
		collisionEnterKeys.Add(item);
		CollisionEnterInfo collisionEnterInfo;
		if (collisionEnterInfosCount < collisionEnterInfos.Count)
		{
			collisionEnterInfo = collisionEnterInfos[collisionEnterInfosCount];
			collisionEnterInfo.go1 = go1;
			collisionEnterInfo.go2 = go2;
			collisionEnterInfo.collision = collision;
		}
		else
		{
			collisionEnterInfo = new CollisionEnterInfo(go1, go2, collision);
			collisionEnterInfos.Add(collisionEnterInfo);
		}
		collisionEnterInfosCount++;
		if (collisionChanged)
		{
			return;
		}
		if (collisionEnterInfosCount > prevCollisionEnterInfos.Count)
		{
			collisionChanged = true;
			return;
		}
		int index = collisionEnterInfosCount - 1;
		CollisionEnterInfo cei = prevCollisionEnterInfos[index];
		if (!collisionEnterInfo.Same(cei))
		{
			collisionChanged = true;
		}
	}

	private static void UpdateTaggedBumps(Block b1, Block b2)
	{
		if (b1 != null && b1.tagBumpEnabled && b2 != null && !b2.isTerrain)
		{
			int instanceID = b1.go.GetInstanceID();
			List<string> blockTags = TagManager.GetBlockTags(b2);
			for (int i = 0; i < blockTags.Count; i++)
			{
				string item = blockTags[i];
				tagBumpBlocks[instanceID].Add(item);
			}
		}
	}

	public static void ForwardCollisionStay(GameObject go, Collision collision)
	{
		if (Blocksworld.CurrentState == State.Play && Blocksworld.playFixedUpdateCounter > 3)
		{
			bumping.Add(go);
		}
	}

	public static void ForwardTriggerEnter(GameObject enterObject, Collider collider)
	{
		if (!ignoreTriggerIds.Contains(enterObject.GetInstanceID()))
		{
			string name = collider.name;
			if (!triggering.TryGetValue(name, out var value))
			{
				value = new HashSet<GameObject>();
				triggering[name] = value;
			}
			value.Add(enterObject);
		}
	}

	public static void ForwardTriggerExit(GameObject exitObject, Collider collider)
	{
		if (!ignoreTriggerIds.Contains(exitObject.GetInstanceID()))
		{
			string name = collider.name;
			if (triggering.TryGetValue(name, out var value))
			{
				value.Remove(exitObject);
			}
		}
	}

	public static void ForwardParticleCollision(Block hitBlock, int ourType)
	{
		if (Blocksworld.CurrentState != State.Play)
		{
			return;
		}
		int instanceID = hitBlock.go.GetInstanceID();
		if (ignoreTriggerIds.Contains(instanceID) || hitBlock == null)
		{
			return;
		}
		if (particleCollideObject.ContainsKey(hitBlock))
		{
			if (!particleCollideObject[hitBlock].Contains(ourType))
			{
				particleCollideObject[hitBlock].Add(ourType);
			}
		}
		else
		{
			particleCollideObject.Add(hitBlock, new HashSet<int> { ourType });
		}
		if (modelParticleCollideObject.ContainsKey(hitBlock.modelBlock))
		{
			if (!modelParticleCollideObject[hitBlock.modelBlock].Contains(ourType))
			{
				modelParticleCollideObject[hitBlock.modelBlock].Add(ourType);
			}
		}
		else
		{
			modelParticleCollideObject.Add(hitBlock.modelBlock, new HashSet<int> { ourType });
		}
	}

	public static bool IsImpactingBlock(Block b)
	{
		if (!bumpedObject.Contains(b))
		{
			return bumpedGround.Contains(b);
		}
		return true;
	}

	public static bool IsImpactingModelBlock(Block modelBlock)
	{
		if (!modelBumpedObject.Contains(modelBlock))
		{
			return modelBumpedGround.Contains(modelBlock);
		}
		return true;
	}

	public static bool IsParticleImpactingBlock(Block b, int particleType)
	{
		if (particleCollideObject.ContainsKey(b))
		{
			return particleCollideObject[b].Contains(particleType);
		}
		return false;
	}

	public static bool IsParticleImpactingModelBlock(Block modelBlock, int particleType)
	{
		if (modelParticleCollideObject.ContainsKey(modelBlock))
		{
			return modelParticleCollideObject[modelBlock].Contains(particleType);
		}
		return false;
	}

	public static bool IsBumpingBlock(Block b, string target)
	{
		return target switch
		{
			"object" => bumpedObject.Contains(b), 
			"ground" => bumpedGround.Contains(b), 
			"notGround" => !bumpedGround.Contains(b), 
			_ => false, 
		};
	}

	public static bool IsTriggeringBlock(Block b)
	{
		if (triggering.TryGetValue(b.colliderName, out var value))
		{
			return value.Count > 0;
		}
		return false;
	}

	public static HashSet<GameObject> GetTriggeringBlocks(Block triggerBlock)
	{
		if (triggering.TryGetValue(triggerBlock.colliderName, out var value))
		{
			return value;
		}
		return null;
	}

	public static bool BlockIsBumpingTag(Block b, string tag)
	{
		int instanceID = b.go.GetInstanceID();
		HashSet<string> hashSet = tagBumpBlocks[instanceID];
		return hashSet.Contains(tag);
	}

	public static bool IsBumpingAny(List<Block> blocks, string target, Block bumpBlock)
	{
		if (!IsImpactingModelBlock(bumpBlock.modelBlock))
		{
			return target == "notGround";
		}
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if ((block == bumpBlock || block.CanTriggerBlockListSensor()) && IsBumpingBlock(block, target))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsImpactingAny(List<Block> model, Block bumpBlock)
	{
		if (!IsImpactingModelBlock(bumpBlock.modelBlock))
		{
			return false;
		}
		for (int i = 0; i < model.Count; i++)
		{
			Block block = model[i];
			if (block == bumpBlock || block.CanTriggerBlockListSensor())
			{
				if (bumpedGround.Contains(block))
				{
					return true;
				}
				if (bumpedObject.Contains(block))
				{
					Block block2 = bumpedBy[block];
					return block2.modelBlock != bumpBlock.modelBlock;
				}
			}
		}
		return false;
	}

	public static bool IsParticleCollidingAny(List<Block> model, Block triggerBlock, int particleType)
	{
		if (!IsParticleImpactingModelBlock(triggerBlock.modelBlock, particleType))
		{
			return false;
		}
		for (int i = 0; i < model.Count; i++)
		{
			Block block = model[i];
			if ((block == triggerBlock || block.CanTriggerBlockListSensor()) && particleCollideObject.ContainsKey(block) && particleCollideObject[block].Contains(particleType))
			{
				return true;
			}
		}
		return false;
	}
}
