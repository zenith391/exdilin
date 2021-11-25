using System;
using System.Collections.Generic;
using System.Linq;
using Blocks;
using UnityEngine;

// Token: 0x020002D5 RID: 725
public class TreasureHandler
{
	// Token: 0x0600211B RID: 8475 RVA: 0x000F2654 File Offset: 0x000F0A54
	public static bool IsPartOfTreasureModel(Block block)
	{
		return TreasureHandler.GetTreasureState(block) != null;
	}

	// Token: 0x0600211C RID: 8476 RVA: 0x000F2664 File Offset: 0x000F0A64
	public static bool IsPartOfPickedUpTreasureModel(Block block)
	{
		TreasureHandler.TreasureState treasureState = TreasureHandler.GetTreasureState(block);
		return treasureState != null && treasureState.mode == TreasureHandler.TreasureMode.PickedUp;
	}

	// Token: 0x0600211D RID: 8477 RVA: 0x000F268C File Offset: 0x000F0A8C
	private static TreasureHandler.TreasureState GetTreasureState(Block block)
	{
		TreasureHandler.TreasureState result;
		if (TreasureHandler.treasureStates.TryGetValue(block.GetInstanceId(), out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x0600211E RID: 8478 RVA: 0x000F26B4 File Offset: 0x000F0AB4
	public static void RemoveTreasureModel(Block block)
	{
		TreasureHandler.TreasureState treasureState = TreasureHandler.GetTreasureState(block);
		if (treasureState != null)
		{
			treasureState.transform.DetachChildren();
			UnityEngine.Object.Destroy(treasureState.transform.gameObject);
			TreasureHandler.treasureBlocks.Remove(treasureState.treasureBlock.GetInstanceId());
			TreasureHandler.uniqueTreasureStates.Remove(treasureState);
			foreach (Block block2 in treasureState.model)
			{
				TreasureHandler.treasureStates.Remove(block2.GetInstanceId());
				TreasureHandler.triggerStayScripts.Remove(block2.go.GetComponent<ForwardTriggerStay>());
				block2.isTreasure = false;
			}
			TreasureHandler.forceUpdateObjectCounters = true;
		}
	}

	// Token: 0x0600211F RID: 8479 RVA: 0x000F2788 File Offset: 0x000F0B88
	public static bool IsHiddenTreasureModel(Block block)
	{
		TreasureHandler.TreasureState treasureState = TreasureHandler.GetTreasureState(block);
		return treasureState != null && treasureState.mode == TreasureHandler.TreasureMode.Hidden;
	}

	// Token: 0x06002120 RID: 8480 RVA: 0x000F27B0 File Offset: 0x000F0BB0
	public static bool IsPickingUpOrRespawning(Block block)
	{
		TreasureHandler.TreasureState treasureState = TreasureHandler.GetTreasureState(block);
		return treasureState != null && (treasureState.mode == TreasureHandler.TreasureMode.PickingUp || treasureState.mode == TreasureHandler.TreasureMode.Respawning);
	}

	// Token: 0x06002121 RID: 8481 RVA: 0x000F27E4 File Offset: 0x000F0BE4
	public static float GetTreasureModelScale(Block block)
	{
		TreasureHandler.TreasureState treasureState = TreasureHandler.GetTreasureState(block);
		if (treasureState != null)
		{
			return Util.MinComponent(treasureState.transform.localScale);
		}
		return 1f;
	}

	// Token: 0x06002122 RID: 8482 RVA: 0x000F2814 File Offset: 0x000F0C14
	public static void Respawn(Block block)
	{
		TreasureHandler.TreasureState treasureState = TreasureHandler.GetTreasureState(block);
		if (treasureState != null)
		{
			treasureState.respawnExecuted = true;
		}
	}

	// Token: 0x06002123 RID: 8483 RVA: 0x000F2838 File Offset: 0x000F0C38
	public static bool IsPickedUpThisFrame(Block block, string tag = null)
	{
		TreasureHandler.TreasureState treasureState = TreasureHandler.GetTreasureState(block);
		return treasureState != null && (treasureState.respawnedDuringPickingUp || (treasureState.mode == TreasureHandler.TreasureMode.PickingUp && (treasureState.previousMode == TreasureHandler.TreasureMode.Respawning || treasureState.previousMode == TreasureHandler.TreasureMode.Active) && (tag == null || treasureState.pickingUpTags.Contains(tag))));
	}

	// Token: 0x06002124 RID: 8484 RVA: 0x000F28A0 File Offset: 0x000F0CA0
	public static void ForceCollect(Block block, string tag = null)
	{
		TreasureHandler.TreasureState treasureState = TreasureHandler.GetTreasureState(block);
		if (treasureState != null && treasureState.mode == TreasureHandler.TreasureMode.Active)
		{
			TreasureHandler.Pickup(treasureState);
			treasureState.mode = TreasureHandler.TreasureMode.ForcePickingUp;
			if (tag != null)
			{
				treasureState.pickingUpTags.Add(tag);
			}
		}
	}

	// Token: 0x06002125 RID: 8485 RVA: 0x000F28E8 File Offset: 0x000F0CE8
	public static void SetAsTreasureBlockIcon(Block block, int counterIndex = 0)
	{
		BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(counterIndex);
		if (objectCounter != null)
		{
			objectCounter.SetCustomIconGAF(block.GetIconGaf());
		}
	}

	// Token: 0x06002126 RID: 8486 RVA: 0x000F2910 File Offset: 0x000F0D10
	public static void SetAsTreasureTextureIcon(Block block, int counterIndex = 0)
	{
		BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(counterIndex);
		if (objectCounter != null)
		{
			objectCounter.SetCustomIconGAF(new GAF(Block.predicateTextureTo, new object[]
			{
				block.GetTexture(0),
				Vector3.zero
			}));
		}
	}

	// Token: 0x06002127 RID: 8487 RVA: 0x000F2958 File Offset: 0x000F0D58
	public static BlockObjectCounterUI GetObjectCounter(int counterIndex)
	{
		BlockObjectCounterUI result;
		if (TreasureHandler.objectCounters.TryGetValue(counterIndex, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06002128 RID: 8488 RVA: 0x000F297C File Offset: 0x000F0D7C
	public static void BlockFrozen(Block block)
	{
		int instanceId = block.GetInstanceId();
		TreasureHandler.TreasureState treasureState;
		if (TreasureHandler.treasureStates.TryGetValue(instanceId, out treasureState))
		{
			treasureState.animate = false;
		}
	}

	// Token: 0x06002129 RID: 8489 RVA: 0x000F29AC File Offset: 0x000F0DAC
	public static void BlockUnfrozen(Block block)
	{
		int instanceId = block.GetInstanceId();
		TreasureHandler.TreasureState treasureState;
		if (TreasureHandler.treasureStates.TryGetValue(instanceId, out treasureState))
		{
			treasureState.animate = true;
		}
	}

	// Token: 0x0600212A RID: 8490 RVA: 0x000F29DC File Offset: 0x000F0DDC
	private static HashSet<Predicate> GetTreasurePreds()
	{
		if (TreasureHandler.treasurePreds == null)
		{
			TreasureHandler.treasurePreds = new HashSet<Predicate>
			{
				Block.predicateIsTreasure,
				Block.predicateIsTreasureForTag,
				Block.predicateIsPickup,
				Block.predicateIsPickupForTag,
				Block.predicateLevitate
			};
		}
		return TreasureHandler.treasurePreds;
	}

	// Token: 0x0600212B RID: 8491 RVA: 0x000F2A40 File Offset: 0x000F0E40
	public static void AddTreasureModel(Block block, string tag = null, bool updateTheObjectCounter = true, int counterIndex = 0)
	{
		int instanceId = block.GetInstanceId();
		if (TreasureHandler.treasureBlocks.Contains(instanceId))
		{
			return;
		}
		Collider component = block.go.GetComponent<Collider>();
		TreasureHandler.treasureBlocks.Add(instanceId);
		bool flag = TreasureHandler.updateObjectCounters[counterIndex] || updateTheObjectCounter;
		TreasureHandler.updateObjectCounters[counterIndex] = flag;
		if (component != null)
		{
			bool flag2 = false;
			TreasureHandler.TreasureState treasureState = TreasureHandler.GetTreasureState(block);
			if (treasureState != null && (treasureState.objectCounterIndex != counterIndex || treasureState.updateObjectCounter != updateTheObjectCounter || treasureState.treasureBlock != block))
			{
				treasureState.treasureBlock = block;
				treasureState.treasureBlockId = instanceId;
				treasureState.updateObjectCounter = updateTheObjectCounter;
				treasureState.objectCounterIndex = counterIndex;
				if (treasureState.mode == TreasureHandler.TreasureMode.Hidden)
				{
					treasureState.mode = TreasureHandler.TreasureMode.Active;
					treasureState.SetVisible(true);
					treasureState.SetShadowsVisible(true);
				}
				TreasureHandler.forceUpdateObjectCounters = true;
			}
			if (treasureState == null)
			{
				List<Block> list = Block.connectedCache[block];
				treasureState = new TreasureHandler.TreasureState();
				treasureState.animate = !block.didFix;
				Block block2 = null;
				foreach (Block block3 in list)
				{
					if (block3.didFix)
					{
						treasureState.animate = false;
					}
					if (block3 is ITreasureAnimationDriver)
					{
						ITreasureAnimationDriver treasureAnimationDriver = (ITreasureAnimationDriver)block3;
						if (treasureAnimationDriver.TreasureAnimationActivated())
						{
							treasureState.animationDriver = treasureAnimationDriver;
							block2 = block3;
						}
					}
				}
				treasureState.treasureBlockId = instanceId;
				treasureState.treasureBlock = block;
				treasureState.updateObjectCounter = updateTheObjectCounter;
				treasureState.objectCounterIndex = counterIndex;
				TreasureHandler.uniqueTreasureStates.Add(treasureState);
				GameObject go = block.go;
				GameObject gameObject = new GameObject("Treasure " + go.name);
				HashSet<Chunk> hashSet = Block.connectedChunks[block];
				Vector3 vector = Vector3.zero;
				foreach (Chunk chunk in hashSet)
				{
					Chunk chunk2 = chunk;
					if (chunk.blocks.Count == 0 || chunk.go == null)
					{
						chunk2 = block.chunk;
					}
					chunk2.blocks[0].Freeze(false);
					Transform transform = chunk2.go.transform;
					vector += transform.position / (float)hashSet.Count;
				}
				if (block2 != null)
				{
					vector = block2.goT.position;
				}
				treasureState.transform = gameObject.transform;
				treasureState.transform.position = vector;
				foreach (Chunk chunk3 in hashSet)
				{
					Chunk chunk4 = chunk3;
					if (chunk3.blocks.Count == 0 || chunk3.go == null)
					{
						chunk4 = block.chunk;
					}
					Transform transform2 = chunk4.go.transform;
					transform2.parent = treasureState.transform;
				}
				treasureState.origPos = vector;
				foreach (Block block4 in list)
				{
					List<Collider> colliders = block4.GetColliders();
					if (colliders.Count > 0)
					{
						colliders.ForEach(delegate(Collider bc)
						{
							bc.isTrigger = true;
						});
						if (colliders.Count == 1 && colliders[0] is MeshCollider)
						{
							MeshCollider mc = (MeshCollider)colliders[0];
							block4.ReplaceMeshCollider(mc);
						}
						ForwardTriggerStay item = block4.go.AddComponent<ForwardTriggerStay>();
						TreasureHandler.triggerStayScripts.Add(item);
					}
				}
				if (treasureState.animate)
				{
					Vector3 size = Util.ComputeBounds(list).size;
					Vector3[] array = new Vector3[]
					{
						Vector3.down,
						Vector3.forward,
						Vector3.back,
						Vector3.right,
						Vector3.left,
						Vector3.up
					};
					foreach (Vector3 vector2 in array)
					{
						RaycastHit[] array3 = Physics.RaycastAll(vector, vector2, Mathf.Abs(Vector3.Dot(vector2, size)));
						Util.SmartSort(array3, vector);
						foreach (RaycastHit raycastHit in array3)
						{
							Block block5 = BWSceneManager.FindBlock(raycastHit.collider.gameObject, true);
							if (block5 != null && !block5.isTerrain && !block5.ContainsTileWithAnyPredicateInPlayMode(TreasureHandler.GetTreasurePreds()) && !list.Contains(block5))
							{
								Transform goT = block5.goT;
								treasureState.localOrigPos = goT.worldToLocalMatrix.MultiplyPoint(vector);
								treasureState.attachedTransform = goT;
								treasureState.attachedModelBlockIds = new HashSet<int>(from bl in Block.connectedCache[block5]
								select bl.GetInstanceId());
								break;
							}
						}
						if (treasureState.attachedTransform != null)
						{
							break;
						}
					}
				}
				flag2 = true;
			}
			if (!string.IsNullOrEmpty(tag))
			{
				treasureState.tags.Add(tag);
			}
			if (flag2)
			{
				treasureState.model = new HashSet<Block>(Block.connectedCache[block]);
				foreach (Block block6 in treasureState.model)
				{
					Collider component2 = block6.go.GetComponent<Collider>();
					if (component2 != null)
					{
						TreasureHandler.treasureStates[block6.GetInstanceId()] = treasureState;
					}
					block6.isTreasure = true;
					block6.BecameTreasure();
				}
			}
		}
	}

	// Token: 0x0600212C RID: 8492 RVA: 0x000F3110 File Offset: 0x000F1510
	public static void ResetState()
	{
		foreach (TreasureHandler.TreasureState treasureState in TreasureHandler.uniqueTreasureStates)
		{
			treasureState.tags.Clear();
			treasureState.respawnExecuted = false;
			treasureState.respawnedDuringPickingUp = false;
		}
	}

	// Token: 0x0600212D RID: 8493 RVA: 0x000F3180 File Offset: 0x000F1580
	public static void FixedUpdate()
	{
		int num = 0;
		int num2 = 0;
		TreasureHandler.objectCounterIndices.Clear();
		foreach (TreasureHandler.TreasureState treasureState in TreasureHandler.uniqueTreasureStates)
		{
			treasureState.previousMode = treasureState.mode;
			TreasureHandler.objectCounterIndices.Add(treasureState.objectCounterIndex);
			switch (treasureState.mode)
			{
			case TreasureHandler.TreasureMode.Active:
				treasureState.Spin();
				if (treasureState.updateObjectCounter)
				{
					num++;
				}
				if (!TreasureHandler.treasureBlocks.Contains(treasureState.treasureBlockId))
				{
					treasureState.mode = TreasureHandler.TreasureMode.Hidden;
					treasureState.SetVisible(false);
					treasureState.SetShadowsVisible(false);
					treasureState.respawnedDuringPickingUp = false;
				}
				if (!treasureState.updateObjectCounter)
				{
					treasureState.pickingUpNowCounter = 0;
				}
				break;
			case TreasureHandler.TreasureMode.ForcePickingUp:
				treasureState.mode = TreasureHandler.TreasureMode.PickingUp;
				treasureState.previousMode = TreasureHandler.TreasureMode.Active;
				break;
			case TreasureHandler.TreasureMode.PickingUp:
				if (treasureState.respawnExecuted && treasureState.pickingUpAnimationStep <= 0)
				{
					treasureState.mode = TreasureHandler.TreasureMode.Active;
					treasureState.respawnedDuringPickingUp = true;
					treasureState.Spin();
					treasureState.EnableColliders(true);
					if (treasureState.updateObjectCounter)
					{
						TileResultCode tileResultCode = treasureState.treasureBlock.PlayVfxDurational("Sparkle Model", 0.25f, (float)treasureState.pickingUpNowCounter * Blocksworld.fixedDeltaTime, "White");
						if (tileResultCode == TileResultCode.True)
						{
							treasureState.pickingUpNowCounter = 0;
						}
						else
						{
							treasureState.pickingUpNowCounter++;
						}
					}
					else
					{
						treasureState.pickingUpNowCounter = 0;
					}
				}
				else
				{
					treasureState.pickingUpNowCounter = 0;
					treasureState.PickingUp();
					if (treasureState.respawnExecuted)
					{
						treasureState.respawnExecuted = false;
						treasureState.respawnWhenPickedUp = true;
					}
				}
				if (treasureState.updateObjectCounter)
				{
					num++;
					num2++;
				}
				break;
			case TreasureHandler.TreasureMode.Respawning:
				treasureState.Respawning();
				if (treasureState.updateObjectCounter)
				{
					num++;
					num2++;
				}
				break;
			case TreasureHandler.TreasureMode.PickedUp:
				if (treasureState.updateObjectCounter)
				{
					num++;
					num2++;
				}
				if (treasureState.respawnExecuted || treasureState.respawnWhenPickedUp)
				{
					if (!treasureState.updateObjectCounter || !WinLoseManager.winning)
					{
						treasureState.shouldInactivate = false;
						TreasureHandler.Respawn(treasureState);
					}
					treasureState.respawnWhenPickedUp = false;
					treasureState.respawnExecuted = false;
				}
				if (treasureState.shouldInactivate && !Sound.durationalSoundBlockIDs.Contains(treasureState.treasureBlock.GetInstanceId()))
				{
					treasureState.transform.gameObject.SetActive(false);
					treasureState.shouldInactivate = false;
				}
				break;
			case TreasureHandler.TreasureMode.Hidden:
				if (TreasureHandler.treasureBlocks.Contains(treasureState.treasureBlockId))
				{
					treasureState.mode = TreasureHandler.TreasureMode.Active;
					treasureState.SetVisible(true);
					treasureState.SetShadowsVisible(true);
				}
				TreasureHandler.forceUpdateObjectCounters = true;
				break;
			}
		}
		foreach (int num3 in TreasureHandler.objectCounterIndices)
		{
			BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(num3);
			if (objectCounter != null && (TreasureHandler.updateObjectCounters[num3] || TreasureHandler.forceUpdateObjectCounters))
			{
				objectCounter.UpdateTreasureState(num, num2);
			}
		}
		TreasureHandler.forceUpdateObjectCounters = false;
		Array.Clear(TreasureHandler.updateObjectCounters, 0, TreasureHandler.updateObjectCounters.Length);
		TreasureHandler.treasureBlocks.Clear();
	}

	// Token: 0x0600212E RID: 8494 RVA: 0x000F3510 File Offset: 0x000F1910
	public static void ForwardTriggerStay(GameObject stayObject, Collider collider)
	{
		Collider component = stayObject.GetComponent<Collider>();
		if (component == null)
		{
			return;
		}
		int instanceID = stayObject.GetInstanceID();
		if (TreasureHandler.treasureStates.ContainsKey(instanceID))
		{
			TreasureHandler.TreasureState treasureState = TreasureHandler.treasureStates[instanceID];
			if (treasureState.mode == TreasureHandler.TreasureMode.Active)
			{
				GameObject gameObject = collider.gameObject;
				bool flag = false;
				Block block = BWSceneManager.FindBlock(gameObject, true);
				if (block == null || block.broken)
				{
					return;
				}
				if (treasureState.attachedModelBlockIds != null)
				{
					int instanceId = block.GetInstanceId();
					if (treasureState.attachedModelBlockIds.Contains(instanceId))
					{
						return;
					}
				}
				if (!block.isTerrain && !(block is BlockPosition) && !treasureState.model.Contains(block))
				{
					if (treasureState.tags.Count == 0)
					{
						flag = true;
						List<string> blockTags = TagManager.GetBlockTags(block);
						if (blockTags != null)
						{
							treasureState.pickingUpTags.UnionWith(blockTags);
						}
					}
					else
					{
						if (block == null)
						{
							return;
						}
						List<string> blockTags2 = TagManager.GetBlockTags(block);
						if (blockTags2 != null)
						{
							foreach (string item in blockTags2)
							{
								if (treasureState.tags.Contains(item))
								{
									flag = true;
									treasureState.pickingUpTags.Add(item);
								}
							}
						}
					}
				}
				if (flag)
				{
					TreasureHandler.Pickup(treasureState);
				}
			}
		}
	}

	// Token: 0x0600212F RID: 8495 RVA: 0x000F369C File Offset: 0x000F1A9C
	private static void Pickup(TreasureHandler.TreasureState state)
	{
		if (state.mode != TreasureHandler.TreasureMode.PickingUp)
		{
			if (state.treasureBlock.BlockType() == "Coin")
			{
				Blocksworld.worldSessionCoinsCollected++;
			}
			state.mode = TreasureHandler.TreasureMode.PickingUp;
		}
		if (!state.respawnExecuted)
		{
			state.EnableColliders(false);
		}
		state.pickedUpTime = Time.fixedTime;
	}

	// Token: 0x06002130 RID: 8496 RVA: 0x000F36FF File Offset: 0x000F1AFF
	private static void Respawn(TreasureHandler.TreasureState state)
	{
		state.mode = TreasureHandler.TreasureMode.Respawning;
		state.pickingUpTags.Clear();
		state.SetVisible(true);
	}

	// Token: 0x06002131 RID: 8497 RVA: 0x000F371A File Offset: 0x000F1B1A
	public static void RegisterObjectCounter(BlockObjectCounterUI counter)
	{
		TreasureHandler.objectCounters[counter.index] = counter;
	}

	// Token: 0x06002132 RID: 8498 RVA: 0x000F372D File Offset: 0x000F1B2D
	public static void Stop()
	{
		TreasureHandler.Clear();
	}

	// Token: 0x06002133 RID: 8499 RVA: 0x000F3734 File Offset: 0x000F1B34
	public static void Clear()
	{
		foreach (TreasureHandler.TreasureState treasureState in TreasureHandler.uniqueTreasureStates)
		{
			treasureState.transform.DetachChildren();
			UnityEngine.Object.Destroy(treasureState.transform.gameObject);
		}
		TreasureHandler.treasureStates.Clear();
		TreasureHandler.uniqueTreasureStates.Clear();
		foreach (ForwardTriggerStay obj in TreasureHandler.triggerStayScripts)
		{
			UnityEngine.Object.Destroy(obj);
		}
		TreasureHandler.triggerStayScripts.Clear();
		TreasureHandler.treasureBlocks.Clear();
		TreasureHandler.objectCounters.Clear();
	}

	// Token: 0x04001C0F RID: 7183
	private static Dictionary<int, BlockObjectCounterUI> objectCounters = new Dictionary<int, BlockObjectCounterUI>();

	// Token: 0x04001C10 RID: 7184
	public const int MAX_OBJECT_COUNTERS = 2;

	// Token: 0x04001C11 RID: 7185
	private static bool[] updateObjectCounters = new bool[2];

	// Token: 0x04001C12 RID: 7186
	private static Dictionary<int, TreasureHandler.TreasureState> treasureStates = new Dictionary<int, TreasureHandler.TreasureState>();

	// Token: 0x04001C13 RID: 7187
	private static HashSet<TreasureHandler.TreasureState> uniqueTreasureStates = new HashSet<TreasureHandler.TreasureState>();

	// Token: 0x04001C14 RID: 7188
	private static List<ForwardTriggerStay> triggerStayScripts = new List<ForwardTriggerStay>();

	// Token: 0x04001C15 RID: 7189
	private static HashSet<int> treasureBlocks = new HashSet<int>();

	// Token: 0x04001C16 RID: 7190
	private static bool forceUpdateObjectCounters = false;

	// Token: 0x04001C17 RID: 7191
	private const float ANIMATION_TIME = 0.3f;

	// Token: 0x04001C18 RID: 7192
	private const float incTime = 0.0600000024f;

	// Token: 0x04001C19 RID: 7193
	private const float ANIMATION_MAX_SCALE = 1.5f;

	// Token: 0x04001C1A RID: 7194
	private static AnimationCurve pickupAnimationCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 1f, 0f, 0f),
		new Keyframe(0.0600000024f, 1.5f, 0f, 0f),
		new Keyframe(0.3f, 0.02f, 0f, 0f)
	});

	// Token: 0x04001C1B RID: 7195
	private static HashSet<Predicate> treasurePreds = null;

	// Token: 0x04001C1C RID: 7196
	private static HashSet<int> objectCounterIndices = new HashSet<int>();

	// Token: 0x020002D6 RID: 726
	public enum TreasureMode
	{
		// Token: 0x04001C20 RID: 7200
		Active,
		// Token: 0x04001C21 RID: 7201
		ForcePickingUp,
		// Token: 0x04001C22 RID: 7202
		PickingUp,
		// Token: 0x04001C23 RID: 7203
		Respawning,
		// Token: 0x04001C24 RID: 7204
		PickedUp,
		// Token: 0x04001C25 RID: 7205
		Hidden
	}

	// Token: 0x020002D7 RID: 727
	public class TreasureState
	{
		// Token: 0x06002138 RID: 8504 RVA: 0x000F393C File Offset: 0x000F1D3C
		private Vector3 GetCurrentReferencePosition()
		{
			return (!(this.attachedTransform == null)) ? this.attachedTransform.TransformPoint(this.localOrigPos) : this.origPos;
		}

		// Token: 0x06002139 RID: 8505 RVA: 0x000F3978 File Offset: 0x000F1D78
		private Vector3 GetPositionOffset()
		{
			Vector3 result = Vector3.zero;
			if (this.animationDriver != null)
			{
				result = this.animationDriver.GetTreasurePositionOffset(this);
			}
			else
			{
				result = Vector3.up * (0.5f - 0.5f * Mathf.Cos(0.05f * (float)this.activeAnimationStep));
			}
			return result;
		}

		// Token: 0x0600213A RID: 8506 RVA: 0x000F39D4 File Offset: 0x000F1DD4
		private void Animate()
		{
			Vector3 axis = new Vector3(0.2f, 0.9f, 0f);
			Quaternion rotation;
			if (this.animationDriver != null)
			{
				rotation = this.animationDriver.GetTreasureRotation(this);
			}
			else
			{
				rotation = Quaternion.AngleAxis(1f * (float)this.activeAnimationStep, axis);
			}
			this.transform.rotation = rotation;
			Vector3 position = this.GetCurrentReferencePosition() + this.GetPositionOffset();
			this.transform.position = position;
		}

		// Token: 0x0600213B RID: 8507 RVA: 0x000F3A52 File Offset: 0x000F1E52
		public void Spin()
		{
			if (this.animate)
			{
				this.Animate();
			}
			this.activeAnimationStep++;
		}

		// Token: 0x0600213C RID: 8508 RVA: 0x000F3A74 File Offset: 0x000F1E74
		public void PickingUp()
		{
			float num = Blocksworld.fixedDeltaTime * (float)this.pickingUpAnimationStep;
			if (this.updateObjectCounter)
			{
				this.treasureBlock.PlayVfxDurational("Sparkle Model", 0.25f, num, "White");
			}
			float d = TreasureHandler.pickupAnimationCurve.Evaluate(num);
			this.transform.localScale = Vector3.one * d;
			this.transform.position = this.GetCurrentReferencePosition() + this.GetPositionOffset();
			if (this.pickingUpAnimationStep == 1)
			{
				this.SetShadowsVisible(false);
			}
			this.pickingUpAnimationStep++;
			if (num >= 0.3f)
			{
				this.mode = TreasureHandler.TreasureMode.PickedUp;
				this.SetVisible(false);
				this.transform.localScale = Vector3.one * 1E-07f;
			}
			else if (this.animationDriver != null)
			{
				this.Animate();
			}
		}

		// Token: 0x0600213D RID: 8509 RVA: 0x000F3B60 File Offset: 0x000F1F60
		public void EnableColliders(bool e)
		{
			Collider[] componentsInChildren = this.treasureBlock.go.GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				collider.enabled = e;
			}
		}

		// Token: 0x0600213E RID: 8510 RVA: 0x000F3BA0 File Offset: 0x000F1FA0
		public void Respawning()
		{
			float num = Blocksworld.fixedDeltaTime * (float)this.pickingUpAnimationStep;
			float d = TreasureHandler.pickupAnimationCurve.Evaluate(num);
			this.transform.localScale = Vector3.one * d;
			this.transform.position = this.GetCurrentReferencePosition() + this.GetPositionOffset();
			this.pickingUpAnimationStep--;
			if (num <= 0f)
			{
				this.treasureBlock.Appeared();
				this.mode = TreasureHandler.TreasureMode.Active;
				this.SetShadowsVisible(true);
				this.EnableColliders(true);
			}
			if (this.animationDriver != null)
			{
				this.Animate();
			}
		}

		// Token: 0x0600213F RID: 8511 RVA: 0x000F3C44 File Offset: 0x000F2044
		public void SetVisible(bool v)
		{
			if (v || !Sound.durationalSoundBlockIDs.Contains(this.treasureBlock.GetInstanceId()))
			{
				this.transform.gameObject.SetActive(v);
				this.shouldInactivate = false;
			}
			else
			{
				this.shouldInactivate = true;
			}
			foreach (Block block in this.model)
			{
				block.vanished = !v;
				if (v)
				{
					block.go.SetActive(true);
				}
				else
				{
					block.Vanished();
				}
			}
		}

		// Token: 0x06002140 RID: 8512 RVA: 0x000F3D04 File Offset: 0x000F2104
		public void SetShadowsVisible(bool v)
		{
			foreach (Block block in this.model)
			{
				GameObject goShadow = block.goShadow;
				if (goShadow != null)
				{
					goShadow.GetComponent<Renderer>().enabled = v;
				}
			}
		}

		// Token: 0x04001C26 RID: 7206
		public TreasureHandler.TreasureMode mode;

		// Token: 0x04001C27 RID: 7207
		public TreasureHandler.TreasureMode previousMode;

		// Token: 0x04001C28 RID: 7208
		public HashSet<string> pickingUpTags = new HashSet<string>();

		// Token: 0x04001C29 RID: 7209
		public bool respawnExecuted;

		// Token: 0x04001C2A RID: 7210
		public bool respawnWhenPickedUp;

		// Token: 0x04001C2B RID: 7211
		public bool respawnedDuringPickingUp;

		// Token: 0x04001C2C RID: 7212
		public HashSet<Block> model;

		// Token: 0x04001C2D RID: 7213
		public int treasureBlockId;

		// Token: 0x04001C2E RID: 7214
		public Block treasureBlock;

		// Token: 0x04001C2F RID: 7215
		public int activeAnimationStep;

		// Token: 0x04001C30 RID: 7216
		public int pickingUpAnimationStep;

		// Token: 0x04001C31 RID: 7217
		public HashSet<string> tags = new HashSet<string>();

		// Token: 0x04001C32 RID: 7218
		public Transform transform;

		// Token: 0x04001C33 RID: 7219
		public Transform attachedTransform;

		// Token: 0x04001C34 RID: 7220
		public HashSet<int> attachedModelBlockIds;

		// Token: 0x04001C35 RID: 7221
		public Vector3 origPos;

		// Token: 0x04001C36 RID: 7222
		public Vector3 localOrigPos;

		// Token: 0x04001C37 RID: 7223
		public bool updateObjectCounter = true;

		// Token: 0x04001C38 RID: 7224
		public int objectCounterIndex;

		// Token: 0x04001C39 RID: 7225
		public bool animate = true;

		// Token: 0x04001C3A RID: 7226
		public ITreasureAnimationDriver animationDriver;

		// Token: 0x04001C3B RID: 7227
		public float pickedUpTime;

		// Token: 0x04001C3C RID: 7228
		public bool shouldInactivate;

		// Token: 0x04001C3D RID: 7229
		public int pickingUpNowCounter;
	}
}
