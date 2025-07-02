using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockDriveAssist : BlockAbstractAntiGravity
{
	private float assistApplications;

	private float appliedAssistApplications;

	private float cmOffset = 1f;

	private float dynamicFriction = 0.75f;

	private float staticFriction = 0.75f;

	private float alignScaler = 1f;

	private Dictionary<Block, CapsuleCollider> newColliders = new Dictionary<Block, CapsuleCollider>();

	private Dictionary<Block, MeshCollider> oldColliders = new Dictionary<Block, MeshCollider>();

	public BlockDriveAssist(List<List<Tile>> tiles)
		: base(tiles)
	{
		playLoop = false;
		informAboutVaryingGravity = false;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.IncreaseModelGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.IncreaseChunkGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.AlignInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.PositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.TurnTowardsTagChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).TurnTowardsTagChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.Assist", null, (Block b) => ((BlockDriveAssist)b).Assist, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.AlignAlongDPadChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignAlongDPadChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
	}

	public override void Play()
	{
		base.Play();
		assistApplications = 0f;
		appliedAssistApplications = 0f;
		cmOffset = Mathf.Abs(Vector3.Dot(Util.ComputeBounds(chunk.blocks).extents, go.transform.up));
		newColliders.Clear();
		oldColliders.Clear();
		DriveAssistMetaData component = go.GetComponent<DriveAssistMetaData>();
		if (component != null)
		{
			cmOffset = component.centerMassOffsetMultplier;
			dynamicFriction = component.dynamicFriction;
			staticFriction = component.staticFriction;
		}
		else
		{
			BWLog.Info("Could not find drive assist meta data component");
		}
		ComputeAlignScaler();
	}

	private void ComputeAlignScaler()
	{
		Chunk chunk = base.chunk;
		Rigidbody component = chunk.go.GetComponent<Rigidbody>();
		float num = ((!(component != null) || component.isKinematic) ? 1f : component.mass);
		UpdateConnectedCache();
		float num2 = 0f;
		foreach (Chunk item in Block.connectedChunks[this])
		{
			if (item != chunk)
			{
				component = item.go.GetComponent<Rigidbody>();
				if (component != null && !component.isKinematic)
				{
					num2 += component.mass;
				}
			}
		}
		alignScaler = 1f;
		if (num2 > 0.25f && num > 0.25f)
		{
			alignScaler = (num + num2) / num;
		}
	}

	protected void ReplaceWheelColliders()
	{
		newColliders.Clear();
		oldColliders.Clear();
		UpdateConnectedCache();
		List<Block> list = Block.connectedCache[this];
		foreach (Block item in list)
		{
			if (item is BlockAbstractWheel)
			{
				Vector3 vector = item.Scale();
				if (Mathf.Abs(vector.y - vector.z) < 0.01f && vector.y / vector.x < 2.1f)
				{
					MeshCollider component = item.go.GetComponent<MeshCollider>();
					component.enabled = false;
					CapsuleCollider capsuleCollider = item.go.AddComponent<CapsuleCollider>();
					float num = vector.y * 0.5f;
					float height = vector.x + num;
					capsuleCollider.height = height;
					capsuleCollider.radius = num;
					capsuleCollider.direction = 0;
					capsuleCollider.material = new PhysicMaterial
					{
						dynamicFriction = dynamicFriction,
						staticFriction = staticFriction,
						frictionCombine = PhysicMaterialCombine.Average
					};
					newColliders[item] = capsuleCollider;
					oldColliders[item] = component;
				}
			}
		}
	}

	protected void RestoreWheelColliders()
	{
		foreach (Block key in newColliders.Keys)
		{
			UnityEngine.Object.Destroy(newColliders[key]);
			oldColliders[key].enabled = true;
		}
		newColliders.Clear();
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		RestoreWheelColliders();
	}

	private void OffsetRigidbodies(Vector3 localOffset)
	{
		Vector3 vector = go.transform.TransformDirection(localOffset);
		Rigidbody component = chunk.go.GetComponent<Rigidbody>();
		if (component != null && !component.isKinematic)
		{
			Vector3 centerOfMass = component.centerOfMass;
			component.centerOfMass = centerOfMass + vector;
		}
	}

	public TileResultCode Assist(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
		assistApplications += num;
		return TileResultCode.True;
	}

	public override void FixedUpdate()
	{
		alignInFieldChunkApplications *= alignScaler;
		base.FixedUpdate();
		if (broken || didFix)
		{
			return;
		}
		Vector3 vector = -Vector3.up * cmOffset;
		if (assistApplications != appliedAssistApplications)
		{
			float num = assistApplications - appliedAssistApplications;
			OffsetRigidbodies(num * vector);
			if (appliedAssistApplications == 0f)
			{
				ReplaceWheelColliders();
			}
			if (assistApplications == 0f)
			{
				RestoreWheelColliders();
			}
			appliedAssistApplications = assistApplications;
		}
		assistApplications = 0f;
	}

	protected void VisualizeCM()
	{
		Rigidbody component = chunk.go.GetComponent<Rigidbody>();
		if (component != null && !component.isKinematic)
		{
			Vector3 worldCenterOfMass = component.worldCenterOfMass;
			Debug.DrawLine(worldCenterOfMass, worldCenterOfMass + Vector3.up * 3f);
		}
	}
}
