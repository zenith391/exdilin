using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class Chunk
{
	public static int uid;

	public List<Block> blocks;

	public GameObject go;

	public Rigidbody rb;

	private float chunkMass;

	private float totalBlockMass;

	private Vector3 chunkCenterOfGravity;

	private bool unappliedMassChanges;

	public BlockAbstractLegs normalCharacter;

	public BlockWalkable mobileCharacter;

	private float dragMultiplier = 1f;

	private float angularDragMultiplier = 1f;

	private const float MINIMUM_CHUNK_MASS = 0.1f;

	public const float DEFAULT_DRAG = 0.2f;

	public const float DEFAULT_ANGULAR_DRAG = 2f;

	public float approxVolume = 1f;

	public float approxSizeMaxComponent = 1f;

	public Vector3 initialInertiaTensor = Vector3.one;

	public Chunk(List<Block> blocks, bool forceRigidbody = false)
		: this(blocks, Quaternion.identity, forceRigidbody)
	{
	}

	public Chunk(List<Block> blocks, Quaternion rotation, bool forceRigidbody = false)
	{
		this.blocks = blocks;
		if (blocks[0].BlockType() == "Sky")
		{
			go = new GameObject("Sky");
		}
		else if (blocks[0].isTerrain)
		{
			go = new GameObject("Ground");
		}
		else
		{
			go = new GameObject("Chunk " + uid++);
		}
		normalCharacter = null;
		mobileCharacter = null;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block is BlockAbstractLegs blockAbstractLegs && normalCharacter == null && !blockAbstractLegs.unmoving)
			{
				normalCharacter = blockAbstractLegs;
			}
			if (block is BlockWalkable blockWalkable && mobileCharacter == null && !blockWalkable.unmoving)
			{
				mobileCharacter = blockWalkable;
			}
			block.chunk = this;
			block.modelBlock = blocks[0];
		}
		Vector3 vector = Util.ComputeCenter(blocks, usePlayModeCenter: true);
		if (IsValidVector3(vector))
		{
			go.transform.position = vector;
		}
		else
		{
			BWLog.Warning($"Invalid center position calculated for chunk: {vector}, using Vector3.zero");
			go.transform.position = Vector3.zero;
		}
		for (int j = 0; j < blocks.Count; j++)
		{
			Block block2 = blocks[j];
			if (block2.HasPreferredChunkRotation())
			{
				rotation = block2.GetPreferredChunkRotation();
			}
		}
		if (IsValidQuaternion(rotation))
		{
			go.transform.rotation = rotation;
		}
		else
		{
			BWLog.Warning($"Invalid rotation for chunk: {rotation}, using Quaternion.identity");
			go.transform.rotation = Quaternion.identity;
		}
		chunkMass = 0f;
		for (int k = 0; k < blocks.Count; k++)
		{
			Block block3 = blocks[k];
			float mass = block3.GetMass();
			if (!float.IsNaN(mass) && !float.IsInfinity(mass))
			{
				chunkMass += mass;
			}
			Collider component = block3.go.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = false;
			}
			block3.goT.parent = go.transform;
			if (component != null)
			{
				component.enabled = true;
			}
		}
		totalBlockMass = chunkMass;
		if (forceRigidbody || !IsDefaultFixed(blocks))
		{
			go.AddComponent<ForwardEvents>();
			AddRigidbody();
		}
		CalculateApproxVolume();
	}

	public bool HasCharacter()
	{
		if (mobileCharacter == null)
		{
			return normalCharacter != null;
		}
		return true;
	}

	private RigidbodyConstraints GetChunkRigidbodyConstraints()
	{
		int num = 0;
		for (int i = 0; i < blocks.Count; i++)
		{
			num |= blocks[i].GetRigidbodyConstraintsMask();
		}
		return (RigidbodyConstraints)num;
	}

	public void UpdateRBConstraints()
	{
		if (rb != null)
		{
			rb.constraints = GetChunkRigidbodyConstraints();
		}
	}

	public Rigidbody AddRigidbody()
	{
		if (null != rb)
		{
			BWLog.Warning("Trying to add rigidbody to chunk with rigidbody");
			return null;
		}
		rb = go.AddComponent<Rigidbody>();
		if (Blocksworld.interpolateRigidBodies)
		{
			rb.interpolation = RigidbodyInterpolation.Interpolate;
		}
		rb.mass = Mathf.Max(0.1f, chunkMass);
		if (HasCharacter())
		{
			rb.centerOfMass -= 0.5f * rb.transform.up;
		}
		rb.drag = 0.2f;
		rb.angularDrag = 2f;
		initialInertiaTensor = rb.inertiaTensor;
		rb.constraints = GetChunkRigidbodyConstraints();
		return rb;
	}

	public void RemoveRigidbody()
	{
		if (null == rb)
		{
			BWLog.Warning("Trying to remove rigidbody from chunk with no rigidbody");
			return;
		}
		Object.Destroy(rb);
		rb = null;
	}

	public void CalculateApproxVolume()
	{
		Bounds bounds = default(Bounds);
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block.GetMass() != 0f)
			{
				Vector3 position = block.goT.position;
				Collider component = block.go.GetComponent<Collider>();
				Bounds bounds2 = new Bounds(position, Vector3.one);
				if (component != null)
				{
					bounds2 = component.bounds;
				}
				if (i == 0)
				{
					bounds = bounds2;
				}
				else
				{
					bounds.Encapsulate(bounds2);
				}
			}
		}
		Vector3 size = bounds.size;
		if (Util.MinComponent(size) > 0.1f)
		{
			approxVolume = size.x * size.y * size.z;
			approxSizeMaxComponent = Util.MaxComponent(size);
		}
	}

	public float CalculateVolumeWithSizes(float limit = float.MaxValue)
	{
		float num = 0f;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block.GetMass() != 0f)
			{
				Vector3 size = block.size;
				num += size.x * size.y * size.z;
				if (num > limit)
				{
					return limit;
				}
			}
		}
		return num;
	}

	public void RemoveBlock(Block block)
	{
		blocks.Remove(block);
		UpdateCenterOfMass();
	}

	public void AddBlock(Block block)
	{
		blocks.Add(block);
		block.chunk = this;
		block.goT.parent = go.transform;
		block.modelBlock = blocks[0];
		UpdateCenterOfMass();
	}

	private bool UsesBumpSensor(List<Block> blocks)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			for (int j = 0; j < block.tiles.Count; j++)
			{
				List<Tile> list = block.tiles[j];
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k].gaf.Predicate == Block.predicateBump)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool IsDefaultFixed(List<Block> blocks)
	{
		if (blocks[0].isTerrain)
		{
			return true;
		}
		bool flag = false;
		for (int i = 0; i < blocks.Count; i++)
		{
			flag |= blocks[i].didFix;
		}
		return flag;
	}

	public bool IsFrozen()
	{
		return IsDefaultFixed(blocks);
	}

	public void Destroy(bool delayed = false)
	{
		Blocksworld.blocksworldCamera.Unfollow(this);
		foreach (Block block in blocks)
		{
			if (block.go != null)
			{
				Util.UnparentTransformSafely(block.goT);
			}
		}
		if (go != null)
		{
			if (delayed)
			{
				Blocksworld.AddFixedUpdateCommand(new DelayedDetachChildrenAndDestroyCommand(go));
			}
			else
			{
				go.transform.DetachChildren();
				Object.Destroy(go);
			}
			blocks.Clear();
		}
		rb = null;
	}

	public override string ToString()
	{
		string text = "Chunk '" + go.name + "' [";
		for (int i = 0; i < blocks.Count; i++)
		{
			text += blocks[i].go.name;
			if (i < blocks.Count - 1)
			{
				text += ",";
			}
		}
		return text + "]";
	}

	public Vector3 GetPosition()
	{
		return go.transform.position;
	}

	public float GetMass()
	{
		return chunkMass;
	}

	public float GetTotalBlockMass()
	{
		return totalBlockMass;
	}

	public bool IsMoving()
	{
		float num = 2f;
		if (rb != null)
		{
			if (!(rb.velocity.sqrMagnitude > num))
			{
				return rb.angularVelocity.sqrMagnitude > num;
			}
			return true;
		}
		return false;
	}

	public float GetDragMultiplier()
	{
		return dragMultiplier;
	}

	public float GetAngularDragMultiplier()
	{
		return angularDragMultiplier;
	}

	public void SetAngularDragMultiplier(float ad)
	{
		angularDragMultiplier = ad;
	}

	public void DrawDebugLines()
	{
		if (rb != null)
		{
			Vector3 worldCenterOfMass = rb.worldCenterOfMass;
			Debug.DrawLine(worldCenterOfMass, worldCenterOfMass + go.transform.up * rb.mass, Color.red);
			Debug.DrawLine(worldCenterOfMass + 0.1f * go.transform.right, worldCenterOfMass + 0.1f * go.transform.right + go.transform.up * GetMass(), Color.green);
			Debug.DrawLine(worldCenterOfMass, worldCenterOfMass + go.transform.rotation * rb.inertiaTensor, Color.blue);
		}
	}

	public void UpdateCenterOfMass(bool immediate = true)
	{
		if (null == rb)
		{
			return;
		}
		mobileCharacter = null;
		normalCharacter = null;
		int num = 0;
		foreach (Block block in blocks)
		{
			if (block is BlockWalkable blockWalkable && mobileCharacter == null && !blockWalkable.unmoving && !blockWalkable.broken)
			{
				mobileCharacter = blockWalkable;
			}
			if (block is BlockAbstractLegs blockAbstractLegs)
			{
				if (!blockAbstractLegs.unmoving && !blockAbstractLegs.broken && normalCharacter == null)
				{
					normalCharacter = blockAbstractLegs;
				}
				num += blockAbstractLegs.legPairCount;
			}
			if (block is BlockAbstractMotor)
			{
				return;
			}
		}
		if (mobileCharacter != null)
		{
			float mass = mobileCharacter.GetMass();
			if (!float.IsNaN(mass) && !float.IsInfinity(mass))
			{
				chunkMass = mass;
				chunkMass = Mathf.Max(0.1f, chunkMass - (float)num);
				totalBlockMass = chunkMass;
			}
			Collider component = mobileCharacter.go.GetComponent<Collider>();
			Transform transform = mobileCharacter.go.transform;
			Vector3 center = component.bounds.center;
			Vector3 vector = transform.up * 0.5f;
			if (IsValidVector3(center) && IsValidVector3(vector))
			{
				Vector3 position = center - vector;
				Vector3 vector2 = go.transform.InverseTransformPoint(position);
				if (IsValidVector3(vector2))
				{
					chunkCenterOfGravity = vector2;
				}
				else
				{
					BWLog.Warning("Invalid center of gravity calculated for mobile character, using Vector3.zero");
					chunkCenterOfGravity = Vector3.zero;
				}
			}
			else
			{
				BWLog.Warning("Invalid bounds or transform data for mobile character");
				chunkCenterOfGravity = Vector3.zero;
			}
			unappliedMassChanges = true;
			if (immediate)
			{
				ApplyCenterOfMassChanges();
			}
			return;
		}
		_ = chunkMass;
		chunkMass = 0f;
		Vector3 zero = Vector3.zero;
		foreach (Block block2 in blocks)
		{
			float mass2 = block2.GetMass();
			if (!(mass2 > 0f) || float.IsNaN(mass2) || float.IsInfinity(mass2))
			{
				continue;
			}
			chunkMass += mass2;
			Vector3 vector3 = block2.goT.position;
			Collider component2 = block2.go.GetComponent<Collider>();
			if (component2 != null)
			{
				Vector3 center2 = component2.bounds.center;
				if (IsValidVector3(center2))
				{
					vector3 = center2;
				}
			}
			Vector3 position2 = rb.transform.position;
			if (IsValidVector3(vector3) && IsValidVector3(position2))
			{
				Vector3 vector4 = (vector3 - position2) * mass2;
				if (IsValidVector3(vector4))
				{
					zero += vector4;
				}
			}
		}
		totalBlockMass = chunkMass;
		if (chunkMass > 0f)
		{
			zero /= chunkMass;
			chunkMass = Mathf.Max(0.1f, chunkMass - (float)num);
		}
		if (HasCharacter())
		{
			Vector3 vector5 = 0.5f * rb.transform.up;
			if (IsValidVector3(vector5))
			{
				zero -= vector5;
			}
		}
		if (IsValidVector3(zero))
		{
			chunkCenterOfGravity = zero;
		}
		else
		{
			BWLog.Warning("Invalid center of gravity calculated, using Vector3.zero");
			chunkCenterOfGravity = Vector3.zero;
		}
		unappliedMassChanges = true;
		if (immediate)
		{
			ApplyCenterOfMassChanges();
		}
	}

	public void ApplyCenterOfMassChanges()
	{
		if (!unappliedMassChanges)
		{
			return;
		}
		if (rb != null)
		{
			if (IsValidVector3(chunkCenterOfGravity))
			{
				if ((chunkCenterOfGravity - rb.centerOfMass).sqrMagnitude > Mathf.Epsilon)
				{
					rb.centerOfMass = chunkCenterOfGravity;
				}
			}
			else
			{
				BWLog.Warning("Invalid chunkCenterOfGravity, not applying to rigidbody");
			}
			if (!float.IsNaN(chunkMass) && !float.IsInfinity(chunkMass))
			{
				if (Mathf.Abs(chunkMass - rb.mass) > Mathf.Epsilon)
				{
					rb.mass = Mathf.Max(0.1f, chunkMass);
				}
			}
			else
			{
				BWLog.Warning("Invalid chunkMass, setting to minimum mass");
				rb.mass = 0.1f;
			}
			initialInertiaTensor = rb.inertiaTensor;
		}
		foreach (Block block in blocks)
		{
			if (block is BlockAbstractLegs { unmoving: false } blockAbstractLegs)
			{
				blockAbstractLegs.UpdateSpring();
			}
		}
		unappliedMassChanges = false;
	}

	public void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			blocks[i].ChunksAndJointsModified(oldToNew, oldToNewChunks, newToOldChunks);
		}
	}

	public void ForceMass(float minimumMass)
	{
		if (chunkMass < minimumMass)
		{
			chunkMass = minimumMass;
			if (rb != null)
			{
				rb.mass = Mathf.Max(0.1f, chunkMass);
			}
		}
	}

	public void StartPull()
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			if (blocks[i] is BlockAnimatedCharacter blockAnimatedCharacter)
			{
				blockAnimatedCharacter.stateHandler.StartPull();
			}
		}
	}

	public void StopPull()
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			if (blocks[i] is BlockAnimatedCharacter blockAnimatedCharacter)
			{
				blockAnimatedCharacter.stateHandler.StopPull();
			}
		}
	}

	static Chunk()
	{
		uid = 1;
	}

	private static bool IsValidVector3(Vector3 vector)
	{
		if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) && !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y))
		{
			return !float.IsInfinity(vector.z);
		}
		return false;
	}

	private static bool IsValidQuaternion(Quaternion quat)
	{
		if (!float.IsNaN(quat.x) && !float.IsNaN(quat.y) && !float.IsNaN(quat.z) && !float.IsNaN(quat.w) && !float.IsInfinity(quat.x) && !float.IsInfinity(quat.y) && !float.IsInfinity(quat.z))
		{
			return !float.IsInfinity(quat.w);
		}
		return false;
	}
}
