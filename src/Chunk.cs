using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000112 RID: 274
public class Chunk
{
	// Token: 0x06001343 RID: 4931 RVA: 0x00084640 File Offset: 0x00082A40
	public Chunk(List<Block> blocks, bool forceRigidbody = false) : this(blocks, Quaternion.identity, forceRigidbody)
	{
	}

	// Token: 0x06001344 RID: 4932 RVA: 0x00084650 File Offset: 0x00082A50
	public Chunk(List<Block> blocks, Quaternion rotation, bool forceRigidbody = false)
	{
		this.blocks = blocks;
		if (blocks[0].BlockType() == "Sky")
		{
			this.go = new GameObject("Sky");
		}
		else if (blocks[0].isTerrain)
		{
			this.go = new GameObject("Ground");
		}
		else
		{
			this.go = new GameObject("Chunk " + Chunk.uid++);
		}
		this.normalCharacter = null;
		this.mobileCharacter = null;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			BlockAbstractLegs blockAbstractLegs = block as BlockAbstractLegs;
			if (blockAbstractLegs != null && this.normalCharacter == null && !blockAbstractLegs.unmoving)
			{
				this.normalCharacter = blockAbstractLegs;
			}
			BlockWalkable blockWalkable = block as BlockWalkable;
			if (blockWalkable != null && this.mobileCharacter == null && !blockWalkable.unmoving)
			{
				this.mobileCharacter = blockWalkable;
			}
			block.chunk = this;
			block.modelBlock = blocks[0];
		}
		this.go.transform.position = Util.ComputeCenter(blocks, true);
		for (int j = 0; j < blocks.Count; j++)
		{
			Block block2 = blocks[j];
			if (block2.HasPreferredChunkRotation())
			{
				rotation = block2.GetPreferredChunkRotation();
			}
		}
		this.go.transform.rotation = rotation;
		this.chunkMass = 0f;
		for (int k = 0; k < blocks.Count; k++)
		{
			Block block3 = blocks[k];
			this.chunkMass += block3.GetMass();
			Collider component = block3.go.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = false;
			}
			block3.goT.parent = this.go.transform;
			if (component != null)
			{
				component.enabled = true;
			}
		}
		this.totalBlockMass = this.chunkMass;
		if (forceRigidbody || !this.IsDefaultFixed(blocks))
		{
			this.go.AddComponent<ForwardEvents>();
			this.AddRigidbody();
		}
		this.CalculateApproxVolume();
	}

	// Token: 0x06001345 RID: 4933 RVA: 0x000848DF File Offset: 0x00082CDF
	public bool HasCharacter()
	{
		return this.mobileCharacter != null || this.normalCharacter != null;
	}

	// Token: 0x06001346 RID: 4934 RVA: 0x000848FC File Offset: 0x00082CFC
	private RigidbodyConstraints GetChunkRigidbodyConstraints()
	{
		int num = 0;
		for (int i = 0; i < this.blocks.Count; i++)
		{
			num |= this.blocks[i].GetRigidbodyConstraintsMask();
		}
		return (RigidbodyConstraints)num;
	}

	// Token: 0x06001347 RID: 4935 RVA: 0x0008493C File Offset: 0x00082D3C
	public void UpdateRBConstraints()
	{
		if (this.rb != null)
		{
			this.rb.constraints = this.GetChunkRigidbodyConstraints();
		}
	}

	// Token: 0x06001348 RID: 4936 RVA: 0x00084960 File Offset: 0x00082D60
	public Rigidbody AddRigidbody()
	{
		if (null != this.rb)
		{
			BWLog.Warning("Trying to add rigidbody to chunk with rigidbody");
			return null;
		}
		this.rb = this.go.AddComponent<Rigidbody>();
		if (Blocksworld.interpolateRigidBodies)
		{
			this.rb.interpolation = RigidbodyInterpolation.Interpolate;
		}
		this.rb.mass = Mathf.Max(0.1f, this.chunkMass);
		if (this.HasCharacter())
		{
			this.rb.centerOfMass -= 0.5f * this.rb.transform.up;
		}
		this.rb.drag = 0.2f;
		this.rb.angularDrag = 2f;
		this.initialInertiaTensor = this.rb.inertiaTensor;
		this.rb.constraints = this.GetChunkRigidbodyConstraints();
		return this.rb;
	}

	// Token: 0x06001349 RID: 4937 RVA: 0x00084A4F File Offset: 0x00082E4F
	public void RemoveRigidbody()
	{
		if (null == this.rb)
		{
			BWLog.Warning("Trying to remove rigidbody from chunk with no rigidbody");
			return;
		}
		UnityEngine.Object.Destroy(this.rb);
		this.rb = null;
	}

	// Token: 0x0600134A RID: 4938 RVA: 0x00084A80 File Offset: 0x00082E80
	public void CalculateApproxVolume()
	{
		Bounds bounds = default(Bounds);
		for (int i = 0; i < this.blocks.Count; i++)
		{
			Block block = this.blocks[i];
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
			this.approxVolume = size.x * size.y * size.z;
			this.approxSizeMaxComponent = Util.MaxComponent(size);
		}
	}

	// Token: 0x0600134B RID: 4939 RVA: 0x00084B6C File Offset: 0x00082F6C
	public float CalculateVolumeWithSizes(float limit = 3.40282347E+38f)
	{
		float num = 0f;
		for (int i = 0; i < this.blocks.Count; i++)
		{
			Block block = this.blocks[i];
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

	// Token: 0x0600134C RID: 4940 RVA: 0x00084BE8 File Offset: 0x00082FE8
	public void RemoveBlock(Block block)
	{
		this.blocks.Remove(block);
		this.UpdateCenterOfMass(true);
	}

	// Token: 0x0600134D RID: 4941 RVA: 0x00084C00 File Offset: 0x00083000
	public void AddBlock(Block block)
	{
		this.blocks.Add(block);
		block.chunk = this;
		block.goT.parent = this.go.transform;
		block.modelBlock = this.blocks[0];
		this.UpdateCenterOfMass(true);
	}

	// Token: 0x0600134E RID: 4942 RVA: 0x00084C50 File Offset: 0x00083050
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
					Tile tile = list[k];
					if (tile.gaf.Predicate == Block.predicateBump)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	// Token: 0x0600134F RID: 4943 RVA: 0x00084CE4 File Offset: 0x000830E4
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

	// Token: 0x06001350 RID: 4944 RVA: 0x00084D2D File Offset: 0x0008312D
	public bool IsFrozen()
	{
		return this.IsDefaultFixed(this.blocks);
	}

	// Token: 0x06001351 RID: 4945 RVA: 0x00084D3C File Offset: 0x0008313C
	public void Destroy(bool delayed = false)
	{
		Blocksworld.blocksworldCamera.Unfollow(this);
		foreach (Block block in this.blocks)
		{
			if (block.go != null)
			{
				Util.UnparentTransformSafely(block.goT);
			}
		}
		if (this.go != null)
		{
			if (delayed)
			{
				Blocksworld.AddFixedUpdateCommand(new DelayedDetachChildrenAndDestroyCommand(this.go));
			}
			else
			{
				this.go.transform.DetachChildren();
				UnityEngine.Object.Destroy(this.go);
			}
			this.blocks.Clear();
		}
		this.rb = null;
	}

	// Token: 0x06001352 RID: 4946 RVA: 0x00084E14 File Offset: 0x00083214
	public override string ToString()
	{
		string str = "Chunk '" + this.go.name + "' [";
		for (int i = 0; i < this.blocks.Count; i++)
		{
			str += this.blocks[i].go.name;
			if (i < this.blocks.Count - 1)
			{
				str += ",";
			}
		}
		return str + "]";
	}

	// Token: 0x06001353 RID: 4947 RVA: 0x00084E9F File Offset: 0x0008329F
	public Vector3 GetPosition()
	{
		return this.go.transform.position;
	}

	// Token: 0x06001354 RID: 4948 RVA: 0x00084EB1 File Offset: 0x000832B1
	public float GetMass()
	{
		return this.chunkMass;
	}

	// Token: 0x06001355 RID: 4949 RVA: 0x00084EB9 File Offset: 0x000832B9
	public float GetTotalBlockMass()
	{
		return this.totalBlockMass;
	}

	// Token: 0x06001356 RID: 4950 RVA: 0x00084EC4 File Offset: 0x000832C4
	public bool IsMoving()
	{
		float num = 2f;
		return this.rb != null && (this.rb.velocity.sqrMagnitude > num || this.rb.angularVelocity.sqrMagnitude > num);
	}

	// Token: 0x06001357 RID: 4951 RVA: 0x00084F1D File Offset: 0x0008331D
	public float GetDragMultiplier()
	{
		return this.dragMultiplier;
	}

	// Token: 0x06001358 RID: 4952 RVA: 0x00084F25 File Offset: 0x00083325
	public float GetAngularDragMultiplier()
	{
		return this.angularDragMultiplier;
	}

	// Token: 0x06001359 RID: 4953 RVA: 0x00084F2D File Offset: 0x0008332D
	public void SetAngularDragMultiplier(float ad)
	{
		this.angularDragMultiplier = ad;
	}

	// Token: 0x0600135A RID: 4954 RVA: 0x00084F38 File Offset: 0x00083338
	public void DrawDebugLines()
	{
		if (this.rb != null)
		{
			Vector3 worldCenterOfMass = this.rb.worldCenterOfMass;
			Debug.DrawLine(worldCenterOfMass, worldCenterOfMass + this.go.transform.up * this.rb.mass, Color.red);
			Debug.DrawLine(worldCenterOfMass + 0.1f * this.go.transform.right, worldCenterOfMass + 0.1f * this.go.transform.right + this.go.transform.up * this.GetMass(), Color.green);
			Debug.DrawLine(worldCenterOfMass, worldCenterOfMass + this.go.transform.rotation * this.rb.inertiaTensor, Color.blue);
		}
	}

	// Token: 0x0600135B RID: 4955 RVA: 0x00085030 File Offset: 0x00083430
	public void UpdateCenterOfMass(bool immediate = true)
	{
		if (null == this.rb)
		{
			return;
		}
		this.mobileCharacter = null;
		this.normalCharacter = null;
		int num = 0;
		foreach (Block block in this.blocks)
		{
			BlockWalkable blockWalkable = block as BlockWalkable;
			if (blockWalkable != null && this.mobileCharacter == null && !blockWalkable.unmoving && !blockWalkable.broken)
			{
				this.mobileCharacter = blockWalkable;
			}
			BlockAbstractLegs blockAbstractLegs = block as BlockAbstractLegs;
			if (blockAbstractLegs != null)
			{
				if (!blockAbstractLegs.unmoving && !blockAbstractLegs.broken && this.normalCharacter == null)
				{
					this.normalCharacter = blockAbstractLegs;
				}
				num += blockAbstractLegs.legPairCount;
			}
			if (block is BlockAbstractMotor)
			{
				return;
			}
		}
		if (this.mobileCharacter != null)
		{
			this.chunkMass = this.mobileCharacter.GetMass();
			this.chunkMass = Mathf.Max(0.1f, this.chunkMass - (float)num);
			this.totalBlockMass = this.chunkMass;
			Collider component = this.mobileCharacter.go.GetComponent<Collider>();
			Transform transform = this.mobileCharacter.go.transform;
			Vector3 position = component.bounds.center - transform.up * 0.5f;
			Vector3 vector = this.go.transform.InverseTransformPoint(position);
			this.chunkCenterOfGravity = vector;
			this.unappliedMassChanges = true;
			if (immediate)
			{
				this.ApplyCenterOfMassChanges();
			}
			return;
		}
		float num2 = this.chunkMass;
		this.chunkMass = 0f;
		Vector3 a = Vector3.zero;
		foreach (Block block2 in this.blocks)
		{
			float mass = block2.GetMass();
			if (mass > 0f)
			{
				this.chunkMass += mass;
				Vector3 a2 = block2.goT.position;
				Collider component2 = block2.go.GetComponent<Collider>();
				if (component2 != null)
				{
					a2 = component2.bounds.center;
				}
				a += (a2 - this.rb.transform.position) * mass;
			}
		}
		this.totalBlockMass = this.chunkMass;
		if (this.chunkMass > 0f)
		{
			a /= this.chunkMass;
			this.chunkMass = Mathf.Max(0.1f, this.chunkMass - (float)num);
		}
		if (this.HasCharacter())
		{
			a -= 0.5f * this.rb.transform.up;
		}
		this.chunkCenterOfGravity = a;
		this.unappliedMassChanges = true;
		if (immediate)
		{
			this.ApplyCenterOfMassChanges();
		}
	}

	// Token: 0x0600135C RID: 4956 RVA: 0x00085360 File Offset: 0x00083760
	public void ApplyCenterOfMassChanges()
	{
		if (!this.unappliedMassChanges)
		{
			return;
		}
		if (this.rb != null)
		{
			if ((this.chunkCenterOfGravity - this.rb.centerOfMass).sqrMagnitude > Mathf.Epsilon)
			{
				this.rb.centerOfMass = this.chunkCenterOfGravity;
			}
			if (Mathf.Abs(this.chunkMass - this.rb.mass) > Mathf.Epsilon)
			{
				this.rb.mass = Mathf.Max(0.1f, this.chunkMass);
			}
			this.initialInertiaTensor = this.rb.inertiaTensor;
		}
		foreach (Block block in this.blocks)
		{
			BlockAbstractLegs blockAbstractLegs = block as BlockAbstractLegs;
			if (blockAbstractLegs != null && !blockAbstractLegs.unmoving)
			{
				blockAbstractLegs.UpdateSpring();
			}
		}
		this.unappliedMassChanges = false;
	}

	// Token: 0x0600135D RID: 4957 RVA: 0x00085480 File Offset: 0x00083880
	public void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
	{
		for (int i = 0; i < this.blocks.Count; i++)
		{
			this.blocks[i].ChunksAndJointsModified(oldToNew, oldToNewChunks, newToOldChunks);
		}
	}

	// Token: 0x0600135E RID: 4958 RVA: 0x000854C0 File Offset: 0x000838C0
	public void ForceMass(float minimumMass)
	{
		if (this.chunkMass < minimumMass)
		{
			this.chunkMass = minimumMass;
			if (this.rb != null)
			{
				this.rb.mass = Mathf.Max(0.1f, this.chunkMass);
			}
		}
	}

	// Token: 0x0600135F RID: 4959 RVA: 0x0008550C File Offset: 0x0008390C
	public void StartPull()
	{
		for (int i = 0; i < this.blocks.Count; i++)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = this.blocks[i] as BlockAnimatedCharacter;
			if (blockAnimatedCharacter != null)
			{
				blockAnimatedCharacter.stateHandler.StartPull();
			}
		}
	}

	// Token: 0x06001360 RID: 4960 RVA: 0x00085558 File Offset: 0x00083958
	public void StopPull()
	{
		for (int i = 0; i < this.blocks.Count; i++)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = this.blocks[i] as BlockAnimatedCharacter;
			if (blockAnimatedCharacter != null)
			{
				blockAnimatedCharacter.stateHandler.StopPull();
			}
		}
	}

	// Token: 0x04000F2B RID: 3883
	public static int uid = 1;

	// Token: 0x04000F2C RID: 3884
	public List<Block> blocks;

	// Token: 0x04000F2D RID: 3885
	public GameObject go;

	// Token: 0x04000F2E RID: 3886
	public Rigidbody rb;

	// Token: 0x04000F2F RID: 3887
	private float chunkMass;

	// Token: 0x04000F30 RID: 3888
	private float totalBlockMass;

	// Token: 0x04000F31 RID: 3889
	private Vector3 chunkCenterOfGravity;

	// Token: 0x04000F32 RID: 3890
	private bool unappliedMassChanges;

	// Token: 0x04000F33 RID: 3891
	public BlockAbstractLegs normalCharacter;

	// Token: 0x04000F34 RID: 3892
	public BlockWalkable mobileCharacter;

	// Token: 0x04000F35 RID: 3893
	private float dragMultiplier = 1f;

	// Token: 0x04000F36 RID: 3894
	private float angularDragMultiplier = 1f;

	// Token: 0x04000F37 RID: 3895
	private const float MINIMUM_CHUNK_MASS = 0.1f;

	// Token: 0x04000F38 RID: 3896
	public const float DEFAULT_DRAG = 0.2f;

	// Token: 0x04000F39 RID: 3897
	public const float DEFAULT_ANGULAR_DRAG = 2f;

	// Token: 0x04000F3A RID: 3898
	public float approxVolume = 1f;

	// Token: 0x04000F3B RID: 3899
	public float approxSizeMaxComponent = 1f;

	// Token: 0x04000F3C RID: 3900
	public Vector3 initialInertiaTensor = Vector3.one;
}
