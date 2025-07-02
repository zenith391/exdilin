using System;
using System.Collections.Generic;
using System.Linq;
using Blocks;
using UnityEngine;

public class TreasureHandler
{
	public enum TreasureMode
	{
		Active,
		ForcePickingUp,
		PickingUp,
		Respawning,
		PickedUp,
		Hidden
	}

	public class TreasureState
	{
		public TreasureMode mode;

		public TreasureMode previousMode;

		public HashSet<string> pickingUpTags = new HashSet<string>();

		public bool respawnExecuted;

		public bool respawnWhenPickedUp;

		public bool respawnedDuringPickingUp;

		public HashSet<Block> model;

		public int treasureBlockId;

		public Block treasureBlock;

		public int activeAnimationStep;

		public int pickingUpAnimationStep;

		public HashSet<string> tags = new HashSet<string>();

		public Transform transform;

		public Transform attachedTransform;

		public HashSet<int> attachedModelBlockIds;

		public Vector3 origPos;

		public Vector3 localOrigPos;

		public bool updateObjectCounter = true;

		public int objectCounterIndex;

		public bool animate = true;

		public ITreasureAnimationDriver animationDriver;

		public float pickedUpTime;

		public bool shouldInactivate;

		public int pickingUpNowCounter;

		private Vector3 GetCurrentReferencePosition()
		{
			if (attachedTransform == null)
			{
				return origPos;
			}
			return attachedTransform.TransformPoint(localOrigPos);
		}

		private Vector3 GetPositionOffset()
		{
			Vector3 zero = Vector3.zero;
			if (animationDriver != null)
			{
				return animationDriver.GetTreasurePositionOffset(this);
			}
			return Vector3.up * (0.5f - 0.5f * Mathf.Cos(0.05f * (float)activeAnimationStep));
		}

		private void Animate()
		{
			Vector3 axis = new Vector3(0.2f, 0.9f, 0f);
			Quaternion rotation = ((animationDriver == null) ? Quaternion.AngleAxis(1f * (float)activeAnimationStep, axis) : animationDriver.GetTreasureRotation(this));
			transform.rotation = rotation;
			Vector3 position = GetCurrentReferencePosition() + GetPositionOffset();
			transform.position = position;
		}

		public void Spin()
		{
			if (animate)
			{
				Animate();
			}
			activeAnimationStep++;
		}

		public void PickingUp()
		{
			float num = Blocksworld.fixedDeltaTime * (float)pickingUpAnimationStep;
			if (updateObjectCounter)
			{
				treasureBlock.PlayVfxDurational("Sparkle Model", 0.25f, num, "White");
			}
			float num2 = pickupAnimationCurve.Evaluate(num);
			transform.localScale = Vector3.one * num2;
			transform.position = GetCurrentReferencePosition() + GetPositionOffset();
			if (pickingUpAnimationStep == 1)
			{
				SetShadowsVisible(v: false);
			}
			pickingUpAnimationStep++;
			if (num >= 0.3f)
			{
				mode = TreasureMode.PickedUp;
				SetVisible(v: false);
				transform.localScale = Vector3.one * 1E-07f;
			}
			else if (animationDriver != null)
			{
				Animate();
			}
		}

		public void EnableColliders(bool e)
		{
			Collider[] componentsInChildren = treasureBlock.go.GetComponentsInChildren<Collider>();
			Collider[] array = componentsInChildren;
			foreach (Collider collider in array)
			{
				collider.enabled = e;
			}
		}

		public void Respawning()
		{
			float num = Blocksworld.fixedDeltaTime * (float)pickingUpAnimationStep;
			float num2 = pickupAnimationCurve.Evaluate(num);
			transform.localScale = Vector3.one * num2;
			transform.position = GetCurrentReferencePosition() + GetPositionOffset();
			pickingUpAnimationStep--;
			if (num <= 0f)
			{
				treasureBlock.Appeared();
				mode = TreasureMode.Active;
				SetShadowsVisible(v: true);
				EnableColliders(e: true);
			}
			if (animationDriver != null)
			{
				Animate();
			}
		}

		public void SetVisible(bool v)
		{
			if (v || !Sound.durationalSoundBlockIDs.Contains(treasureBlock.GetInstanceId()))
			{
				transform.gameObject.SetActive(v);
				shouldInactivate = false;
			}
			else
			{
				shouldInactivate = true;
			}
			foreach (Block item in model)
			{
				item.vanished = !v;
				if (v)
				{
					item.go.SetActive(value: true);
				}
				else
				{
					item.Vanished();
				}
			}
		}

		public void SetShadowsVisible(bool v)
		{
			foreach (Block item in model)
			{
				GameObject goShadow = item.goShadow;
				if (goShadow != null)
				{
					goShadow.GetComponent<Renderer>().enabled = v;
				}
			}
		}
	}

	private static Dictionary<int, BlockObjectCounterUI> objectCounters = new Dictionary<int, BlockObjectCounterUI>();

	public const int MAX_OBJECT_COUNTERS = 2;

	private static bool[] updateObjectCounters = new bool[2];

	private static Dictionary<int, TreasureState> treasureStates = new Dictionary<int, TreasureState>();

	private static HashSet<TreasureState> uniqueTreasureStates = new HashSet<TreasureState>();

	private static List<ForwardTriggerStay> triggerStayScripts = new List<ForwardTriggerStay>();

	private static HashSet<int> treasureBlocks = new HashSet<int>();

	private static bool forceUpdateObjectCounters = false;

	private const float ANIMATION_TIME = 0.3f;

	private const float incTime = 0.060000002f;

	private const float ANIMATION_MAX_SCALE = 1.5f;

	private static AnimationCurve pickupAnimationCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(0.060000002f, 1.5f, 0f, 0f), new Keyframe(0.3f, 0.02f, 0f, 0f));

	private static HashSet<Predicate> treasurePreds = null;

	private static HashSet<int> objectCounterIndices = new HashSet<int>();

	public static bool IsPartOfTreasureModel(Block block)
	{
		return GetTreasureState(block) != null;
	}

	public static bool IsPartOfPickedUpTreasureModel(Block block)
	{
		TreasureState treasureState = GetTreasureState(block);
		if (treasureState != null)
		{
			return treasureState.mode == TreasureMode.PickedUp;
		}
		return false;
	}

	private static TreasureState GetTreasureState(Block block)
	{
		if (treasureStates.TryGetValue(block.GetInstanceId(), out var value))
		{
			return value;
		}
		return null;
	}

	public static void RemoveTreasureModel(Block block)
	{
		TreasureState treasureState = GetTreasureState(block);
		if (treasureState == null)
		{
			return;
		}
		treasureState.transform.DetachChildren();
		UnityEngine.Object.Destroy(treasureState.transform.gameObject);
		treasureBlocks.Remove(treasureState.treasureBlock.GetInstanceId());
		uniqueTreasureStates.Remove(treasureState);
		foreach (Block item in treasureState.model)
		{
			treasureStates.Remove(item.GetInstanceId());
			triggerStayScripts.Remove(item.go.GetComponent<ForwardTriggerStay>());
			item.isTreasure = false;
		}
		forceUpdateObjectCounters = true;
	}

	public static bool IsHiddenTreasureModel(Block block)
	{
		TreasureState treasureState = GetTreasureState(block);
		if (treasureState != null)
		{
			return treasureState.mode == TreasureMode.Hidden;
		}
		return false;
	}

	public static bool IsPickingUpOrRespawning(Block block)
	{
		TreasureState treasureState = GetTreasureState(block);
		if (treasureState != null)
		{
			if (treasureState.mode != TreasureMode.PickingUp)
			{
				return treasureState.mode == TreasureMode.Respawning;
			}
			return true;
		}
		return false;
	}

	public static float GetTreasureModelScale(Block block)
	{
		TreasureState treasureState = GetTreasureState(block);
		if (treasureState != null)
		{
			return Util.MinComponent(treasureState.transform.localScale);
		}
		return 1f;
	}

	public static void Respawn(Block block)
	{
		TreasureState treasureState = GetTreasureState(block);
		if (treasureState != null)
		{
			treasureState.respawnExecuted = true;
		}
	}

	public static bool IsPickedUpThisFrame(Block block, string tag = null)
	{
		TreasureState treasureState = GetTreasureState(block);
		if (treasureState != null)
		{
			if (!treasureState.respawnedDuringPickingUp)
			{
				if (treasureState.mode == TreasureMode.PickingUp && (treasureState.previousMode == TreasureMode.Respawning || treasureState.previousMode == TreasureMode.Active))
				{
					if (tag != null)
					{
						return treasureState.pickingUpTags.Contains(tag);
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static void ForceCollect(Block block, string tag = null)
	{
		TreasureState treasureState = GetTreasureState(block);
		if (treasureState != null && treasureState.mode == TreasureMode.Active)
		{
			Pickup(treasureState);
			treasureState.mode = TreasureMode.ForcePickingUp;
			if (tag != null)
			{
				treasureState.pickingUpTags.Add(tag);
			}
		}
	}

	public static void SetAsTreasureBlockIcon(Block block, int counterIndex = 0)
	{
		GetObjectCounter(counterIndex)?.SetCustomIconGAF(block.GetIconGaf());
	}

	public static void SetAsTreasureTextureIcon(Block block, int counterIndex = 0)
	{
		GetObjectCounter(counterIndex)?.SetCustomIconGAF(new GAF(Block.predicateTextureTo, block.GetTexture(), Vector3.zero));
	}

	public static BlockObjectCounterUI GetObjectCounter(int counterIndex)
	{
		if (objectCounters.TryGetValue(counterIndex, out var value))
		{
			return value;
		}
		return null;
	}

	public static void BlockFrozen(Block block)
	{
		int instanceId = block.GetInstanceId();
		if (treasureStates.TryGetValue(instanceId, out var value))
		{
			value.animate = false;
		}
	}

	public static void BlockUnfrozen(Block block)
	{
		int instanceId = block.GetInstanceId();
		if (treasureStates.TryGetValue(instanceId, out var value))
		{
			value.animate = true;
		}
	}

	private static HashSet<Predicate> GetTreasurePreds()
	{
		if (treasurePreds == null)
		{
			treasurePreds = new HashSet<Predicate>
			{
				Block.predicateIsTreasure,
				Block.predicateIsTreasureForTag,
				Block.predicateIsPickup,
				Block.predicateIsPickupForTag,
				Block.predicateLevitate
			};
		}
		return treasurePreds;
	}

	public static void AddTreasureModel(Block block, string tag = null, bool updateTheObjectCounter = true, int counterIndex = 0)
	{
		int instanceId = block.GetInstanceId();
		if (treasureBlocks.Contains(instanceId))
		{
			return;
		}
		Collider component = block.go.GetComponent<Collider>();
		treasureBlocks.Add(instanceId);
		bool flag = updateObjectCounters[counterIndex] || updateTheObjectCounter;
		updateObjectCounters[counterIndex] = flag;
		if (!(component != null))
		{
			return;
		}
		bool flag2 = false;
		TreasureState treasureState = GetTreasureState(block);
		if (treasureState != null && (treasureState.objectCounterIndex != counterIndex || treasureState.updateObjectCounter != updateTheObjectCounter || treasureState.treasureBlock != block))
		{
			treasureState.treasureBlock = block;
			treasureState.treasureBlockId = instanceId;
			treasureState.updateObjectCounter = updateTheObjectCounter;
			treasureState.objectCounterIndex = counterIndex;
			if (treasureState.mode == TreasureMode.Hidden)
			{
				treasureState.mode = TreasureMode.Active;
				treasureState.SetVisible(v: true);
				treasureState.SetShadowsVisible(v: true);
			}
			forceUpdateObjectCounters = true;
		}
		if (treasureState == null)
		{
			List<Block> list = Block.connectedCache[block];
			treasureState = new TreasureState();
			treasureState.animate = !block.didFix;
			Block block2 = null;
			foreach (Block item2 in list)
			{
				if (item2.didFix)
				{
					treasureState.animate = false;
				}
				if (item2 is ITreasureAnimationDriver)
				{
					ITreasureAnimationDriver treasureAnimationDriver = (ITreasureAnimationDriver)item2;
					if (treasureAnimationDriver.TreasureAnimationActivated())
					{
						treasureState.animationDriver = treasureAnimationDriver;
						block2 = item2;
					}
				}
			}
			treasureState.treasureBlockId = instanceId;
			treasureState.treasureBlock = block;
			treasureState.updateObjectCounter = updateTheObjectCounter;
			treasureState.objectCounterIndex = counterIndex;
			uniqueTreasureStates.Add(treasureState);
			GameObject go = block.go;
			GameObject gameObject = new GameObject("Treasure " + go.name);
			HashSet<Chunk> hashSet = Block.connectedChunks[block];
			Vector3 vector = Vector3.zero;
			foreach (Chunk item3 in hashSet)
			{
				Chunk chunk = item3;
				if (item3.blocks.Count == 0 || item3.go == null)
				{
					chunk = block.chunk;
				}
				chunk.blocks[0].Freeze(informModelBlocks: false);
				Transform transform = chunk.go.transform;
				vector += transform.position / hashSet.Count;
			}
			if (block2 != null)
			{
				vector = block2.goT.position;
			}
			treasureState.transform = gameObject.transform;
			treasureState.transform.position = vector;
			foreach (Chunk item4 in hashSet)
			{
				Chunk chunk2 = item4;
				if (item4.blocks.Count == 0 || item4.go == null)
				{
					chunk2 = block.chunk;
				}
				Transform transform2 = chunk2.go.transform;
				transform2.parent = treasureState.transform;
			}
			treasureState.origPos = vector;
			foreach (Block item5 in list)
			{
				List<Collider> colliders = item5.GetColliders();
				if (colliders.Count > 0)
				{
					colliders.ForEach(delegate(Collider bc)
					{
						bc.isTrigger = true;
					});
					if (colliders.Count == 1 && colliders[0] is MeshCollider)
					{
						MeshCollider mc = (MeshCollider)colliders[0];
						item5.ReplaceMeshCollider(mc);
					}
					ForwardTriggerStay item = item5.go.AddComponent<ForwardTriggerStay>();
					triggerStayScripts.Add(item);
				}
			}
			if (treasureState.animate)
			{
				Vector3 size = Util.ComputeBounds(list).size;
				Vector3[] array = new Vector3[6]
				{
					Vector3.down,
					Vector3.forward,
					Vector3.back,
					Vector3.right,
					Vector3.left,
					Vector3.up
				};
				Vector3[] array2 = array;
				foreach (Vector3 vector2 in array2)
				{
					RaycastHit[] array3 = Physics.RaycastAll(vector, vector2, Mathf.Abs(Vector3.Dot(vector2, size)));
					Util.SmartSort(array3, vector);
					RaycastHit[] array4 = array3;
					foreach (RaycastHit raycastHit in array4)
					{
						Block block3 = BWSceneManager.FindBlock(raycastHit.collider.gameObject, checkChildGos: true);
						if (block3 != null && !block3.isTerrain && !block3.ContainsTileWithAnyPredicateInPlayMode(GetTreasurePreds()) && !list.Contains(block3))
						{
							Transform goT = block3.goT;
							treasureState.localOrigPos = goT.worldToLocalMatrix.MultiplyPoint(vector);
							treasureState.attachedTransform = goT;
							treasureState.attachedModelBlockIds = new HashSet<int>(Block.connectedCache[block3].Select((Block bl) => bl.GetInstanceId()));
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
		if (!flag2)
		{
			return;
		}
		treasureState.model = new HashSet<Block>(Block.connectedCache[block]);
		foreach (Block item6 in treasureState.model)
		{
			Collider component2 = item6.go.GetComponent<Collider>();
			if (component2 != null)
			{
				treasureStates[item6.GetInstanceId()] = treasureState;
			}
			item6.isTreasure = true;
			item6.BecameTreasure();
		}
	}

	public static void ResetState()
	{
		foreach (TreasureState uniqueTreasureState in uniqueTreasureStates)
		{
			uniqueTreasureState.tags.Clear();
			uniqueTreasureState.respawnExecuted = false;
			uniqueTreasureState.respawnedDuringPickingUp = false;
		}
	}

	public static void FixedUpdate()
	{
		int num = 0;
		int num2 = 0;
		objectCounterIndices.Clear();
		foreach (TreasureState uniqueTreasureState in uniqueTreasureStates)
		{
			uniqueTreasureState.previousMode = uniqueTreasureState.mode;
			objectCounterIndices.Add(uniqueTreasureState.objectCounterIndex);
			switch (uniqueTreasureState.mode)
			{
			case TreasureMode.Active:
				uniqueTreasureState.Spin();
				if (uniqueTreasureState.updateObjectCounter)
				{
					num++;
				}
				if (!treasureBlocks.Contains(uniqueTreasureState.treasureBlockId))
				{
					uniqueTreasureState.mode = TreasureMode.Hidden;
					uniqueTreasureState.SetVisible(v: false);
					uniqueTreasureState.SetShadowsVisible(v: false);
					uniqueTreasureState.respawnedDuringPickingUp = false;
				}
				if (!uniqueTreasureState.updateObjectCounter)
				{
					uniqueTreasureState.pickingUpNowCounter = 0;
				}
				break;
			case TreasureMode.ForcePickingUp:
				uniqueTreasureState.mode = TreasureMode.PickingUp;
				uniqueTreasureState.previousMode = TreasureMode.Active;
				break;
			case TreasureMode.PickingUp:
				if (uniqueTreasureState.respawnExecuted && uniqueTreasureState.pickingUpAnimationStep <= 0)
				{
					uniqueTreasureState.mode = TreasureMode.Active;
					uniqueTreasureState.respawnedDuringPickingUp = true;
					uniqueTreasureState.Spin();
					uniqueTreasureState.EnableColliders(e: true);
					if (uniqueTreasureState.updateObjectCounter)
					{
						TileResultCode tileResultCode = uniqueTreasureState.treasureBlock.PlayVfxDurational("Sparkle Model", 0.25f, (float)uniqueTreasureState.pickingUpNowCounter * Blocksworld.fixedDeltaTime, "White");
						if (tileResultCode == TileResultCode.True)
						{
							uniqueTreasureState.pickingUpNowCounter = 0;
						}
						else
						{
							uniqueTreasureState.pickingUpNowCounter++;
						}
					}
					else
					{
						uniqueTreasureState.pickingUpNowCounter = 0;
					}
				}
				else
				{
					uniqueTreasureState.pickingUpNowCounter = 0;
					uniqueTreasureState.PickingUp();
					if (uniqueTreasureState.respawnExecuted)
					{
						uniqueTreasureState.respawnExecuted = false;
						uniqueTreasureState.respawnWhenPickedUp = true;
					}
				}
				if (uniqueTreasureState.updateObjectCounter)
				{
					num++;
					num2++;
				}
				break;
			case TreasureMode.Respawning:
				uniqueTreasureState.Respawning();
				if (uniqueTreasureState.updateObjectCounter)
				{
					num++;
					num2++;
				}
				break;
			case TreasureMode.PickedUp:
				if (uniqueTreasureState.updateObjectCounter)
				{
					num++;
					num2++;
				}
				if (uniqueTreasureState.respawnExecuted || uniqueTreasureState.respawnWhenPickedUp)
				{
					if (!uniqueTreasureState.updateObjectCounter || !WinLoseManager.winning)
					{
						uniqueTreasureState.shouldInactivate = false;
						Respawn(uniqueTreasureState);
					}
					uniqueTreasureState.respawnWhenPickedUp = false;
					uniqueTreasureState.respawnExecuted = false;
				}
				if (uniqueTreasureState.shouldInactivate && !Sound.durationalSoundBlockIDs.Contains(uniqueTreasureState.treasureBlock.GetInstanceId()))
				{
					uniqueTreasureState.transform.gameObject.SetActive(value: false);
					uniqueTreasureState.shouldInactivate = false;
				}
				break;
			case TreasureMode.Hidden:
				if (treasureBlocks.Contains(uniqueTreasureState.treasureBlockId))
				{
					uniqueTreasureState.mode = TreasureMode.Active;
					uniqueTreasureState.SetVisible(v: true);
					uniqueTreasureState.SetShadowsVisible(v: true);
				}
				forceUpdateObjectCounters = true;
				break;
			}
		}
		foreach (int objectCounterIndex in objectCounterIndices)
		{
			BlockObjectCounterUI objectCounter = GetObjectCounter(objectCounterIndex);
			if (objectCounter != null && (updateObjectCounters[objectCounterIndex] || forceUpdateObjectCounters))
			{
				objectCounter.UpdateTreasureState(num, num2);
			}
		}
		forceUpdateObjectCounters = false;
		Array.Clear(updateObjectCounters, 0, updateObjectCounters.Length);
		treasureBlocks.Clear();
	}

	public static void ForwardTriggerStay(GameObject stayObject, Collider collider)
	{
		Collider component = stayObject.GetComponent<Collider>();
		if (component == null)
		{
			return;
		}
		int instanceID = stayObject.GetInstanceID();
		if (!treasureStates.ContainsKey(instanceID))
		{
			return;
		}
		TreasureState treasureState = treasureStates[instanceID];
		if (treasureState.mode != TreasureMode.Active)
		{
			return;
		}
		GameObject gameObject = collider.gameObject;
		bool flag = false;
		Block block = BWSceneManager.FindBlock(gameObject, checkChildGos: true);
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
			Pickup(treasureState);
		}
	}

	private static void Pickup(TreasureState state)
	{
		if (state.mode != TreasureMode.PickingUp)
		{
			if (state.treasureBlock.BlockType() == "Coin")
			{
				Blocksworld.worldSessionCoinsCollected++;
			}
			state.mode = TreasureMode.PickingUp;
		}
		if (!state.respawnExecuted)
		{
			state.EnableColliders(e: false);
		}
		state.pickedUpTime = Time.fixedTime;
	}

	private static void Respawn(TreasureState state)
	{
		state.mode = TreasureMode.Respawning;
		state.pickingUpTags.Clear();
		state.SetVisible(v: true);
	}

	public static void RegisterObjectCounter(BlockObjectCounterUI counter)
	{
		objectCounters[counter.index] = counter;
	}

	public static void Stop()
	{
		Clear();
	}

	public static void Clear()
	{
		foreach (TreasureState uniqueTreasureState in uniqueTreasureStates)
		{
			uniqueTreasureState.transform.DetachChildren();
			UnityEngine.Object.Destroy(uniqueTreasureState.transform.gameObject);
		}
		treasureStates.Clear();
		uniqueTreasureStates.Clear();
		foreach (ForwardTriggerStay triggerStayScript in triggerStayScripts)
		{
			UnityEngine.Object.Destroy(triggerStayScript);
		}
		triggerStayScripts.Clear();
		treasureBlocks.Clear();
		objectCounters.Clear();
	}
}
