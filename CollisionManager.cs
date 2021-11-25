using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000116 RID: 278
public class CollisionManager
{
	// Token: 0x06001395 RID: 5013 RVA: 0x00086BE8 File Offset: 0x00084FE8
	public static CollisionManager.GhostBlockInfo GetGhostBlockInfo(Block b)
	{
		CollisionManager.GhostBlockInfo result;
		if (CollisionManager.ghostBlocks.TryGetValue(b.GetInstanceId(), out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06001396 RID: 5014 RVA: 0x00086C10 File Offset: 0x00085010
	public static CollisionManager.GhostBlockInfo SetGhostBlockMode(Block b, CollisionManager.GhostBlockMode mode)
	{
		CollisionManager.GhostBlockInfo ghostBlockInfo = CollisionManager.GetGhostBlockInfo(b);
		if (ghostBlockInfo != null)
		{
			ghostBlockInfo.mode = mode;
		}
		switch (mode)
		{
		case CollisionManager.GhostBlockMode.NotGhost:
			CollisionManager.SetBlockLayer(b, Layer.Default);
			CollisionManager.ghostBlocks.Remove(b.GetInstanceId());
			if (ghostBlockInfo != null)
			{
			}
			ghostBlockInfo = null;
			break;
		case CollisionManager.GhostBlockMode.Propagate:
			if (ghostBlockInfo == null)
			{
				ghostBlockInfo = new CollisionManager.GhostBlockInfo
				{
					block = b,
					mode = mode,
					didPropagate = false,
					counter = UnityEngine.Random.Range(1, 10),
					oldTexture = b.GetTexture(0)
				};
				CollisionManager.ghostBlocks[b.GetInstanceId()] = ghostBlockInfo;
			}
			CollisionManager.SetBlockLayer(b, Layer.ChunkedBlock);
			break;
		case CollisionManager.GhostBlockMode.NoPropagate:
			if (ghostBlockInfo == null)
			{
				ghostBlockInfo = new CollisionManager.GhostBlockInfo
				{
					block = b,
					mode = mode,
					didPropagate = false,
					counter = UnityEngine.Random.Range(1, 10),
					oldTexture = b.GetTexture(0)
				};
				CollisionManager.ghostBlocks[b.GetInstanceId()] = ghostBlockInfo;
			}
			CollisionManager.SetBlockLayer(b, Layer.ChunkedBlock);
			break;
		case CollisionManager.GhostBlockMode.TestUnGhost:
			CollisionManager.SetBlockLayer(b, Layer.Default);
			if (ghostBlockInfo == null)
			{
				BWLog.Info("Ghosted block should not be in mode " + mode + " and not have a ghost info");
			}
			break;
		}
		return ghostBlockInfo;
	}

	// Token: 0x06001397 RID: 5015 RVA: 0x00086D58 File Offset: 0x00085158
	public static CollisionManager.GhostBlockMode GetGhostBlockMode(Block b)
	{
		CollisionManager.GhostBlockInfo ghostBlockInfo;
		if (CollisionManager.ghostBlocks.TryGetValue(b.GetInstanceId(), out ghostBlockInfo))
		{
			return ghostBlockInfo.mode;
		}
		return CollisionManager.GhostBlockMode.NotGhost;
	}

	// Token: 0x06001398 RID: 5016 RVA: 0x00086D84 File Offset: 0x00085184
	public static void SetBlockLayer(Block b, Layer layer)
	{
		b.go.SetLayer(layer, true);
		BlockAnimatedCharacter blockAnimatedCharacter = b as BlockAnimatedCharacter;
		if (blockAnimatedCharacter != null)
		{
			foreach (GameObject gameObject in blockAnimatedCharacter.GetAllBodyPartObjects())
			{
				if (gameObject.transform.parent == null)
				{
					gameObject.SetLayer(layer, false);
				}
			}
		}
	}

	// Token: 0x06001399 RID: 5017 RVA: 0x00086E14 File Offset: 0x00085214
	public static void AddIgnoreTriggerGO(GameObject go)
	{
		CollisionManager.ignoreTriggerIds.Add(go.GetInstanceID());
	}

	// Token: 0x0600139A RID: 5018 RVA: 0x00086E28 File Offset: 0x00085228
	public static void Play()
	{
		List<Block> list = BWSceneManager.AllBlocks();
		HashSet<Block> hashSet = new HashSet<Block>();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			BlockGrouped blockGrouped = block as BlockGrouped;
			if (blockGrouped == null)
			{
				block.tagBumpEnabled = block.ContainsTagBump();
				if (block.tagBumpEnabled)
				{
					CollisionManager.tagBumpBlocks[block.GetInstanceId()] = new HashSet<string>();
					hashSet.Add(block);
				}
			}
			else if (blockGrouped.IsMainBlockInGroup())
			{
				bool flag = block.ContainsTagBump();
				foreach (Block block2 in blockGrouped.group.GetBlocks())
				{
					block2.tagBumpEnabled = flag;
					if (flag)
					{
						CollisionManager.tagBumpBlocks[block2.GetInstanceId()] = new HashSet<string>();
						hashSet.Add(block2);
					}
				}
			}
		}
		foreach (Block block3 in hashSet)
		{
			List<Block> list2;
			if (block3.ContainsTileWithPredicateInPlayMode(Block.predicateTaggedBumpModel))
			{
				block3.UpdateConnectedCache();
				list2 = Block.connectedCache[block3];
			}
			else
			{
				if (!block3.ContainsTileWithPredicateInPlayMode(Block.predicateTaggedBumpChunk))
				{
					continue;
				}
				list2 = block3.chunk.blocks;
			}
			foreach (Block block4 in list2)
			{
				block4.tagBumpEnabled = true;
				CollisionManager.tagBumpBlocks[block4.GetInstanceId()] = new HashSet<string>();
			}
		}
	}

	// Token: 0x0600139B RID: 5019 RVA: 0x00087018 File Offset: 0x00085418
	public static void Stop()
	{
		CollisionManager.ResetState(true);
		CollisionManager.ignoreTriggerIds.Clear();
		CollisionManager.bumping.Clear();
		CollisionManager.bumpedObject.Clear();
		CollisionManager.modelBumpedObject.Clear();
		CollisionManager.bumpedBy.Clear();
		CollisionManager.bumpedGround.Clear();
		CollisionManager.modelBumpedGround.Clear();
		CollisionManager.particleCollideObject.Clear();
		CollisionManager.modelParticleCollideObject.Clear();
		CollisionManager.blocksOnGround.Clear();
		CollisionManager.triggering.Clear();
		foreach (int key in new List<int>(CollisionManager.ghostBlocks.Keys))
		{
			CollisionManager.SetBlockLayer(CollisionManager.ghostBlocks[key].block, Layer.Default);
		}
		CollisionManager.ghostBlocks.Clear();
		CollisionManager.ghostsToRemove.Clear();
	}

	// Token: 0x0600139C RID: 5020 RVA: 0x00087114 File Offset: 0x00085514
	private static void DebugGhostInfo(CollisionManager.GhostBlockInfo info)
	{
		Vector3 position = info.block.go.transform.position;
		Color color = Color.white;
		switch (info.mode)
		{
		case CollisionManager.GhostBlockMode.NotGhost:
			color = Color.green;
			break;
		case CollisionManager.GhostBlockMode.Propagate:
			color = Color.red;
			break;
		case CollisionManager.GhostBlockMode.NoPropagate:
			color = Color.white;
			break;
		case CollisionManager.GhostBlockMode.TestUnGhost:
			color = Color.blue;
			break;
		}
		Debug.DrawLine(position, position + Vector3.up * 5f, color);
	}

	// Token: 0x0600139D RID: 5021 RVA: 0x000871A8 File Offset: 0x000855A8
	public static void FixedUpdate()
	{
		CollisionManager.ExecuteWakeUps();
		CollisionManager.bumping.Clear();
		if (CollisionManager.ghostBlocks.Count > 0)
		{
			foreach (KeyValuePair<int, CollisionManager.GhostBlockInfo> keyValuePair in CollisionManager.ghostBlocks)
			{
				CollisionManager.GhostBlockInfo value = keyValuePair.Value;
				if (value.mode == CollisionManager.GhostBlockMode.NoPropagate)
				{
					value.counter++;
					if (value.counter % 150 == 0)
					{
						CollisionManager.SetGhostBlockMode(value.block, CollisionManager.GhostBlockMode.TestUnGhost);
					}
				}
				else if (value.mode == CollisionManager.GhostBlockMode.TestUnGhost)
				{
					value.unGhostCounter++;
					if (value.unGhostCounter > 5)
					{
						CollisionManager.ghostsToRemove.Add(keyValuePair.Value);
					}
				}
			}
			if (CollisionManager.ghostsToRemove.Count > 0)
			{
				foreach (CollisionManager.GhostBlockInfo ghostBlockInfo in CollisionManager.ghostsToRemove)
				{
					CollisionManager.SetGhostBlockMode(ghostBlockInfo.block, CollisionManager.GhostBlockMode.NotGhost);
				}
				CollisionManager.ghostsToRemove.Clear();
			}
		}
	}

	// Token: 0x0600139E RID: 5022 RVA: 0x00087304 File Offset: 0x00085704
	public static void ResetState(bool force = false)
	{
		CollisionManager.collisionChanged = (CollisionManager.collisionChanged || CollisionManager.prevCollisionEnterInfos.Count > CollisionManager.collisionEnterInfosCount);
		CollisionManager.prevCollisionEnterInfos.Clear();
		CollisionManager.collisionEnterKeys.Clear();
		CollisionManager.particleCollideObject.Clear();
		CollisionManager.modelParticleCollideObject.Clear();
		if (CollisionManager.collisionChanged)
		{
			CollisionManager.bumpedObject.Clear();
			CollisionManager.modelBumpedObject.Clear();
			CollisionManager.bumpedBy.Clear();
			CollisionManager.bumpedGround.Clear();
			CollisionManager.modelBumpedGround.Clear();
			foreach (KeyValuePair<int, HashSet<string>> keyValuePair in CollisionManager.tagBumpBlocks)
			{
				keyValuePair.Value.Clear();
			}
			if (!force)
			{
				CollisionManager.ExecuteForwardCollisionEnterInfos();
			}
		}
		if (!force)
		{
			for (int i = 0; i < CollisionManager.collisionEnterInfosCount; i++)
			{
				CollisionManager.prevCollisionEnterInfos.Add(CollisionManager.collisionEnterInfos[i]);
			}
		}
		CollisionManager.collisionEnterInfosCount = 0;
		if (force)
		{
			CollisionManager.collisionEnterInfos.Clear();
			CollisionManager.wakeUpPosRads.Clear();
		}
		CollisionManager.collisionChanged = false;
	}

	// Token: 0x0600139F RID: 5023 RVA: 0x00087450 File Offset: 0x00085850
	private static void SetBumpedGround(Block b)
	{
		if (b == null)
		{
			return;
		}
		CollisionManager.bumpedGround.Add(b);
		CollisionManager.modelBumpedGround.Add(b.modelBlock);
		CollisionManager.blocksOnGround.Add(b);
	}

	// Token: 0x060013A0 RID: 5024 RVA: 0x00087484 File Offset: 0x00085884
	public static void WakeUpObjectsRestingOn(Block b)
	{
		if (Physics.gravity.sqrMagnitude > 0f)
		{
			Vector4 item = b.goT.position - Physics.gravity.normalized * 0.25f;
			item.w = b.GetBounds().extents.magnitude + 0.25f;
			CollisionManager.wakeUpPosRads.Add(item);
		}
	}

	// Token: 0x060013A1 RID: 5025 RVA: 0x00087504 File Offset: 0x00085904
	private static void ExecuteWakeUps()
	{
		int count = CollisionManager.wakeUpPosRads.Count;
		if (count == 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			Vector4 v = CollisionManager.wakeUpPosRads[i];
			float w = v.w;
			Vector3 position = v;
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
		CollisionManager.wakeUpPosRads.Clear();
	}

	// Token: 0x060013A2 RID: 5026 RVA: 0x000875A4 File Offset: 0x000859A4
	public static void ExecuteForwardCollisionEnterInfos()
	{
		for (int i = 0; i < CollisionManager.collisionEnterInfosCount; i++)
		{
			CollisionManager.CollisionEnterInfo collisionEnterInfo = CollisionManager.collisionEnterInfos[i];
			GameObject go = collisionEnterInfo.go1;
			GameObject go2 = collisionEnterInfo.go2;
			Collision collision = collisionEnterInfo.collision;
			Block block = BWSceneManager.FindBlock(go, true);
			Block block2 = BWSceneManager.FindBlock(go2, true);
			bool flag = block != null && block.isTerrain;
			bool flag2 = block2 != null && block2.isTerrain;
			if (flag && !flag2)
			{
				CollisionManager.SetBumpedGround(block2);
				CollisionManager.bumpedObject.Add(block);
				CollisionManager.modelBumpedObject.Add(block.modelBlock);
			}
			else if (flag2 && !flag)
			{
				CollisionManager.SetBumpedGround(block);
				CollisionManager.bumpedObject.Add(block2);
				CollisionManager.modelBumpedObject.Add(block2.modelBlock);
			}
			else if (flag && flag2)
			{
				CollisionManager.SetBumpedGround(block);
				CollisionManager.SetBumpedGround(block2);
			}
			else if (block != null)
			{
				CollisionManager.bumpedObject.Add(block);
				CollisionManager.modelBumpedObject.Add(block.modelBlock);
				CollisionManager.bumpedBy[block] = ((block2 != null) ? block2 : block);
			}
			CollisionManager.UpdateTaggedBumps(block, block2);
			CollisionManager.UpdateTaggedBumps(block2, block);
			if (collision != null && Sound.sfxEnabled)
			{
				Sound.CollisionSFX(block, block2, go, go2, collision);
			}
			if (CollisionManager.ghostBlocks.Count > 0 && !flag && !flag2 && block != null && block2 != null)
			{
				CollisionManager.UpdateGhostBlock(block, block2);
				CollisionManager.UpdateGhostBlock(block2, block);
			}
		}
	}

	// Token: 0x060013A3 RID: 5027 RVA: 0x00087764 File Offset: 0x00085B64
	private static void UpdateGhostBlock(Block b1, Block b2)
	{
		CollisionManager.GhostBlockInfo ghostBlockInfo = CollisionManager.GetGhostBlockInfo(b1);
		if (ghostBlockInfo == null)
		{
			return;
		}
		if (ghostBlockInfo.mode == CollisionManager.GhostBlockMode.Propagate)
		{
			ghostBlockInfo.didPropagate = true;
			CollisionManager.GhostBlockInfo ghostBlockInfo2 = CollisionManager.GetGhostBlockInfo(b2);
			if (ghostBlockInfo2 == null || ghostBlockInfo2.mode != CollisionManager.GhostBlockMode.Propagate)
			{
				CollisionManager.SetGhostBlockMode(b2, CollisionManager.GhostBlockMode.NoPropagate);
			}
		}
		else if (ghostBlockInfo.mode == CollisionManager.GhostBlockMode.TestUnGhost)
		{
			CollisionManager.GhostBlockInfo ghostBlockInfo3 = CollisionManager.GetGhostBlockInfo(b2);
			if (ghostBlockInfo3 != null)
			{
				ghostBlockInfo.unGhostCounter = 1;
				CollisionManager.SetGhostBlockMode(b1, CollisionManager.GhostBlockMode.NoPropagate);
			}
		}
	}

	// Token: 0x060013A4 RID: 5028 RVA: 0x000877E0 File Offset: 0x00085BE0
	public static void ForwardCollisionEnter(GameObject go1, GameObject go2, Collision collision)
	{
		if (Blocksworld.CurrentState == State.Play && Blocksworld.playFixedUpdateCounter > 3)
		{
			long item = (long)go1.GetInstanceID() + (long)go2.GetInstanceID() * 100000000L;
			if (CollisionManager.collisionEnterKeys.Contains(item))
			{
				return;
			}
			CollisionManager.collisionEnterKeys.Add(item);
			CollisionManager.CollisionEnterInfo collisionEnterInfo;
			if (CollisionManager.collisionEnterInfosCount < CollisionManager.collisionEnterInfos.Count)
			{
				collisionEnterInfo = CollisionManager.collisionEnterInfos[CollisionManager.collisionEnterInfosCount];
				collisionEnterInfo.go1 = go1;
				collisionEnterInfo.go2 = go2;
				collisionEnterInfo.collision = collision;
			}
			else
			{
				collisionEnterInfo = new CollisionManager.CollisionEnterInfo(go1, go2, collision);
				CollisionManager.collisionEnterInfos.Add(collisionEnterInfo);
			}
			CollisionManager.collisionEnterInfosCount++;
			if (!CollisionManager.collisionChanged)
			{
				if (CollisionManager.collisionEnterInfosCount > CollisionManager.prevCollisionEnterInfos.Count)
				{
					CollisionManager.collisionChanged = true;
				}
				else
				{
					int index = CollisionManager.collisionEnterInfosCount - 1;
					CollisionManager.CollisionEnterInfo cei = CollisionManager.prevCollisionEnterInfos[index];
					if (!collisionEnterInfo.Same(cei))
					{
						CollisionManager.collisionChanged = true;
					}
				}
			}
		}
	}

	// Token: 0x060013A5 RID: 5029 RVA: 0x000878E4 File Offset: 0x00085CE4
	private static void UpdateTaggedBumps(Block b1, Block b2)
	{
		if (b1 != null && b1.tagBumpEnabled && b2 != null && !b2.isTerrain)
		{
			int instanceID = b1.go.GetInstanceID();
			List<string> blockTags = TagManager.GetBlockTags(b2);
			for (int i = 0; i < blockTags.Count; i++)
			{
				string item = blockTags[i];
				CollisionManager.tagBumpBlocks[instanceID].Add(item);
			}
		}
	}

	// Token: 0x060013A6 RID: 5030 RVA: 0x00087957 File Offset: 0x00085D57
	public static void ForwardCollisionStay(GameObject go, Collision collision)
	{
		if (Blocksworld.CurrentState == State.Play && Blocksworld.playFixedUpdateCounter > 3)
		{
			CollisionManager.bumping.Add(go);
		}
	}

	// Token: 0x060013A7 RID: 5031 RVA: 0x0008797C File Offset: 0x00085D7C
	public static void ForwardTriggerEnter(GameObject enterObject, Collider collider)
	{
		if (CollisionManager.ignoreTriggerIds.Contains(enterObject.GetInstanceID()))
		{
			return;
		}
		string name = collider.name;
		HashSet<GameObject> hashSet;
		if (!CollisionManager.triggering.TryGetValue(name, out hashSet))
		{
			hashSet = new HashSet<GameObject>();
			CollisionManager.triggering[name] = hashSet;
		}
		hashSet.Add(enterObject);
	}

	// Token: 0x060013A8 RID: 5032 RVA: 0x000879D4 File Offset: 0x00085DD4
	public static void ForwardTriggerExit(GameObject exitObject, Collider collider)
	{
		if (CollisionManager.ignoreTriggerIds.Contains(exitObject.GetInstanceID()))
		{
			return;
		}
		string name = collider.name;
		HashSet<GameObject> hashSet;
		if (CollisionManager.triggering.TryGetValue(name, out hashSet))
		{
			hashSet.Remove(exitObject);
		}
	}

	// Token: 0x060013A9 RID: 5033 RVA: 0x00087A18 File Offset: 0x00085E18
	public static void ForwardParticleCollision(Block hitBlock, int ourType)
	{
		if (Blocksworld.CurrentState == State.Play)
		{
			int instanceID = hitBlock.go.GetInstanceID();
			if (CollisionManager.ignoreTriggerIds.Contains(instanceID))
			{
				return;
			}
			if (hitBlock != null)
			{
				if (CollisionManager.particleCollideObject.ContainsKey(hitBlock))
				{
					if (!CollisionManager.particleCollideObject[hitBlock].Contains(ourType))
					{
						CollisionManager.particleCollideObject[hitBlock].Add(ourType);
					}
				}
				else
				{
					CollisionManager.particleCollideObject.Add(hitBlock, new HashSet<int>
					{
						ourType
					});
				}
				if (CollisionManager.modelParticleCollideObject.ContainsKey(hitBlock.modelBlock))
				{
					if (!CollisionManager.modelParticleCollideObject[hitBlock.modelBlock].Contains(ourType))
					{
						CollisionManager.modelParticleCollideObject[hitBlock.modelBlock].Add(ourType);
					}
				}
				else
				{
					CollisionManager.modelParticleCollideObject.Add(hitBlock.modelBlock, new HashSet<int>
					{
						ourType
					});
				}
			}
		}
	}

	// Token: 0x060013AA RID: 5034 RVA: 0x00087B15 File Offset: 0x00085F15
	public static bool IsImpactingBlock(Block b)
	{
		return CollisionManager.bumpedObject.Contains(b) || CollisionManager.bumpedGround.Contains(b);
	}

	// Token: 0x060013AB RID: 5035 RVA: 0x00087B35 File Offset: 0x00085F35
	public static bool IsImpactingModelBlock(Block modelBlock)
	{
		return CollisionManager.modelBumpedObject.Contains(modelBlock) || CollisionManager.modelBumpedGround.Contains(modelBlock);
	}

	// Token: 0x060013AC RID: 5036 RVA: 0x00087B55 File Offset: 0x00085F55
	public static bool IsParticleImpactingBlock(Block b, int particleType)
	{
		return CollisionManager.particleCollideObject.ContainsKey(b) && CollisionManager.particleCollideObject[b].Contains(particleType);
	}

	// Token: 0x060013AD RID: 5037 RVA: 0x00087B7A File Offset: 0x00085F7A
	public static bool IsParticleImpactingModelBlock(Block modelBlock, int particleType)
	{
		return CollisionManager.modelParticleCollideObject.ContainsKey(modelBlock) && CollisionManager.modelParticleCollideObject[modelBlock].Contains(particleType);
	}

	// Token: 0x060013AE RID: 5038 RVA: 0x00087BA0 File Offset: 0x00085FA0
	public static bool IsBumpingBlock(Block b, string target)
	{
		if (target == "object")
		{
			return CollisionManager.bumpedObject.Contains(b);
		}
		if (target == "ground")
		{
			return CollisionManager.bumpedGround.Contains(b);
		}
		return target == "notGround" && !CollisionManager.bumpedGround.Contains(b);
	}

	// Token: 0x060013AF RID: 5039 RVA: 0x00087C08 File Offset: 0x00086008
	public static bool IsTriggeringBlock(Block b)
	{
		HashSet<GameObject> hashSet;
		return CollisionManager.triggering.TryGetValue(b.colliderName, out hashSet) && hashSet.Count > 0;
	}

	// Token: 0x060013B0 RID: 5040 RVA: 0x00087C3C File Offset: 0x0008603C
	public static HashSet<GameObject> GetTriggeringBlocks(Block triggerBlock)
	{
		HashSet<GameObject> result;
		if (CollisionManager.triggering.TryGetValue(triggerBlock.colliderName, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x060013B1 RID: 5041 RVA: 0x00087C64 File Offset: 0x00086064
	public static bool BlockIsBumpingTag(Block b, string tag)
	{
		int instanceID = b.go.GetInstanceID();
		HashSet<string> hashSet = CollisionManager.tagBumpBlocks[instanceID];
		return hashSet.Contains(tag);
	}

	// Token: 0x060013B2 RID: 5042 RVA: 0x00087C90 File Offset: 0x00086090
	public static bool IsBumpingAny(List<Block> blocks, string target, Block bumpBlock)
	{
		if (!CollisionManager.IsImpactingModelBlock(bumpBlock.modelBlock))
		{
			return target == "notGround";
		}
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block == bumpBlock || block.CanTriggerBlockListSensor())
			{
				if (CollisionManager.IsBumpingBlock(block, target))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060013B3 RID: 5043 RVA: 0x00087D00 File Offset: 0x00086100
	public static bool IsImpactingAny(List<Block> model, Block bumpBlock)
	{
		if (!CollisionManager.IsImpactingModelBlock(bumpBlock.modelBlock))
		{
			return false;
		}
		for (int i = 0; i < model.Count; i++)
		{
			Block block = model[i];
			if (block == bumpBlock || block.CanTriggerBlockListSensor())
			{
				if (CollisionManager.bumpedGround.Contains(block))
				{
					return true;
				}
				if (CollisionManager.bumpedObject.Contains(block))
				{
					Block block2 = CollisionManager.bumpedBy[block];
					return block2.modelBlock != bumpBlock.modelBlock;
				}
			}
		}
		return false;
	}

	// Token: 0x060013B4 RID: 5044 RVA: 0x00087D98 File Offset: 0x00086198
	public static bool IsParticleCollidingAny(List<Block> model, Block triggerBlock, int particleType)
	{
		if (!CollisionManager.IsParticleImpactingModelBlock(triggerBlock.modelBlock, particleType))
		{
			return false;
		}
		for (int i = 0; i < model.Count; i++)
		{
			Block block = model[i];
			if (block == triggerBlock || block.CanTriggerBlockListSensor())
			{
				if (CollisionManager.particleCollideObject.ContainsKey(block) && CollisionManager.particleCollideObject[block].Contains(particleType))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x04000F56 RID: 3926
	private const int BUMP_COOLDOWN_STEPS = 3;

	// Token: 0x04000F57 RID: 3927
	private static bool collisionChanged = true;

	// Token: 0x04000F58 RID: 3928
	private static HashSet<long> collisionEnterKeys = new HashSet<long>();

	// Token: 0x04000F59 RID: 3929
	private static HashSet<int> ignoreTriggerIds = new HashSet<int>();

	// Token: 0x04000F5A RID: 3930
	private static int collisionEnterInfosCount = 0;

	// Token: 0x04000F5B RID: 3931
	private static List<CollisionManager.CollisionEnterInfo> collisionEnterInfos = new List<CollisionManager.CollisionEnterInfo>();

	// Token: 0x04000F5C RID: 3932
	private static List<CollisionManager.CollisionEnterInfo> prevCollisionEnterInfos = new List<CollisionManager.CollisionEnterInfo>();

	// Token: 0x04000F5D RID: 3933
	public static Dictionary<int, HashSet<string>> tagBumpBlocks = new Dictionary<int, HashSet<string>>();

	// Token: 0x04000F5E RID: 3934
	public static HashSet<Block> bumpedObject = new HashSet<Block>();

	// Token: 0x04000F5F RID: 3935
	public static HashSet<Block> modelBumpedObject = new HashSet<Block>();

	// Token: 0x04000F60 RID: 3936
	public static HashSet<Block> bumpedGround = new HashSet<Block>();

	// Token: 0x04000F61 RID: 3937
	public static HashSet<Block> modelBumpedGround = new HashSet<Block>();

	// Token: 0x04000F62 RID: 3938
	public static HashSet<Block> blocksOnGround = new HashSet<Block>();

	// Token: 0x04000F63 RID: 3939
	public static Dictionary<Block, HashSet<int>> particleCollideObject = new Dictionary<Block, HashSet<int>>();

	// Token: 0x04000F64 RID: 3940
	public static Dictionary<Block, HashSet<int>> modelParticleCollideObject = new Dictionary<Block, HashSet<int>>();

	// Token: 0x04000F65 RID: 3941
	public static Dictionary<Block, Block> bumpedBy = new Dictionary<Block, Block>();

	// Token: 0x04000F66 RID: 3942
	public static HashSet<GameObject> bumping = new HashSet<GameObject>();

	// Token: 0x04000F67 RID: 3943
	public static Dictionary<string, HashSet<GameObject>> triggering = new Dictionary<string, HashSet<GameObject>>();

	// Token: 0x04000F68 RID: 3944
	private static Dictionary<int, CollisionManager.GhostBlockInfo> ghostBlocks = new Dictionary<int, CollisionManager.GhostBlockInfo>();

	// Token: 0x04000F69 RID: 3945
	private static List<Vector4> wakeUpPosRads = new List<Vector4>();

	// Token: 0x04000F6A RID: 3946
	private const int unGhostInterval = 150;

	// Token: 0x04000F6B RID: 3947
	private static List<CollisionManager.GhostBlockInfo> ghostsToRemove = new List<CollisionManager.GhostBlockInfo>();

	// Token: 0x02000117 RID: 279
	private class CollisionEnterInfo
	{
		// Token: 0x060013B6 RID: 5046 RVA: 0x00087EE5 File Offset: 0x000862E5
		public CollisionEnterInfo(GameObject go1, GameObject go2, Collision collision)
		{
			this.go1 = go1;
			this.go2 = go2;
			this.collision = collision;
		}

		// Token: 0x060013B7 RID: 5047 RVA: 0x00087F02 File Offset: 0x00086302
		public bool Same(CollisionManager.CollisionEnterInfo cei)
		{
			return cei.go1 == this.go1 && cei.go2 == this.go2 && cei.collision == this.collision;
		}

		// Token: 0x04000F6C RID: 3948
		public GameObject go1;

		// Token: 0x04000F6D RID: 3949
		public GameObject go2;

		// Token: 0x04000F6E RID: 3950
		public Collision collision;
	}

	// Token: 0x02000118 RID: 280
	public enum GhostBlockMode
	{
		// Token: 0x04000F70 RID: 3952
		NotGhost,
		// Token: 0x04000F71 RID: 3953
		Propagate,
		// Token: 0x04000F72 RID: 3954
		NoPropagate,
		// Token: 0x04000F73 RID: 3955
		TestUnGhost
	}

	// Token: 0x02000119 RID: 281
	public class GhostBlockInfo
	{
		// Token: 0x04000F74 RID: 3956
		public Block block;

		// Token: 0x04000F75 RID: 3957
		public bool didPropagate;

		// Token: 0x04000F76 RID: 3958
		public CollisionManager.GhostBlockMode mode;

		// Token: 0x04000F77 RID: 3959
		public int counter;

		// Token: 0x04000F78 RID: 3960
		public int unGhostCounter;

		// Token: 0x04000F79 RID: 3961
		public string oldTexture;
	}
}
