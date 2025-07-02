using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractPlatform : BlockAbstractHover
{
	protected Vector3 targetPosition = Vector3.zero;

	protected Vector3 rightDirection = Vector3.right;

	protected Vector3 forwardDirection = Vector3.forward;

	protected Vector3 upDirection = Vector3.up;

	private float rightMassDistribution = 1f;

	private float forwardMassDistribution = 1f;

	private float upMassDistribution = 1f;

	protected float massMultiplier = 1f;

	protected float prevMassMultiplier = 1f;

	protected float tensorMultiplier = 1f;

	protected float prevTensorMultiplier = 1f;

	protected Dictionary<Chunk, float> origChunkMasses = new Dictionary<Chunk, float>();

	protected Dictionary<Chunk, Vector3> origChunkTensors = new Dictionary<Chunk, Vector3>();

	private const float MAX_VEL_ERROR = 20f;

	private AntigravityMetaData metaData;

	protected bool enabled = true;

	protected bool controlsVelocity;

	public BlockAbstractPlatform(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public override void OnCreate()
	{
		base.OnCreate();
		metaData = go.GetComponent<AntigravityMetaData>();
		if (metaData != null)
		{
			rotation = Quaternion.Euler(metaData.orientation);
		}
		else
		{
			BWLog.Info("Could not find antigravity meta data component in " + BlockType());
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		enabled = true;
		origChunkMasses.Clear();
		origChunkTensors.Clear();
	}

	public override void Play()
	{
		base.Play();
		enabled = true;
	}

	public override void Play2()
	{
		base.Play2();
		forwardDirection = goT.forward;
		upDirection = goT.up;
		rightDirection = goT.right;
		chunkRigidBody = chunk.rb;
		if (chunkRigidBody != null)
		{
			forwardMassDistribution = CalculateMassDistribution(chunk, forwardDirection);
			rightMassDistribution = CalculateMassDistribution(chunk, rightDirection);
			upMassDistribution = CalculateMassDistribution(chunk, upDirection);
			targetPosition = GetPlatformPosition();
		}
		else
		{
			rightMassDistribution = 1f;
			forwardMassDistribution = 1f;
			upMassDistribution = 1f;
		}
		if (enabled)
		{
			UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			foreach (Block item in list)
			{
				if (item != this && item is BlockAbstractPlatform)
				{
					BlockAbstractPlatform blockAbstractPlatform = (BlockAbstractPlatform)item;
					blockAbstractPlatform.enabled = false;
				}
			}
		}
		massMultiplier = 1f;
		prevMassMultiplier = 1f;
		tensorMultiplier = 1f;
		prevTensorMultiplier = 1f;
		origChunkMasses = new Dictionary<Chunk, float>();
		origChunkTensors = new Dictionary<Chunk, Vector3>();
		UpdateConnectedCache();
		HashSet<Chunk> hashSet = Block.connectedChunks[this];
		foreach (Chunk item2 in hashSet)
		{
			GameObject gameObject = item2.go;
			if (gameObject != null && gameObject.GetComponent<Rigidbody>() != null)
			{
				origChunkMasses[chunk] = gameObject.GetComponent<Rigidbody>().mass;
				origChunkTensors[chunk] = gameObject.GetComponent<Rigidbody>().inertiaTensor;
			}
		}
	}

	protected virtual Vector3 GetPlatformPosition()
	{
		return goT.position;
	}

	private void ApplyModelGravityForce()
	{
		float gravityMultiplier = -1f;
		foreach (Rigidbody allRigidbody in allRigidbodies)
		{
			if (!(allRigidbody == null))
			{
				AddGravityForce(allRigidbody, gravityMultiplier, allRigidbody.mass);
				if (varyingMassBlocksModel.Count > 0 && varyingMassBlocksModel.TryGetValue(allRigidbody, out var value))
				{
					float varyingMassOffset = GetVaryingMassOffset(value);
					AddGravityForce(allRigidbody, gravityMultiplier, varyingMassOffset);
				}
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!enabled)
		{
			return;
		}
		if (!controlsVelocity)
		{
			ApplyModelGravityForce();
			ApplyChunkPositionForces();
			AlignPlatform();
		}
		if (prevMassMultiplier != massMultiplier)
		{
			foreach (KeyValuePair<Chunk, float> origChunkMass in origChunkMasses)
			{
				Chunk key = origChunkMass.Key;
				GameObject gameObject = key.go;
				if (gameObject != null && gameObject.GetComponent<Rigidbody>() != null)
				{
					gameObject.GetComponent<Rigidbody>().mass = origChunkMass.Value * massMultiplier;
				}
			}
		}
		if (prevTensorMultiplier != tensorMultiplier)
		{
			foreach (KeyValuePair<Chunk, Vector3> origChunkTensor in origChunkTensors)
			{
				Chunk key2 = origChunkTensor.Key;
				GameObject gameObject2 = key2.go;
				if (gameObject2 != null && gameObject2.GetComponent<Rigidbody>() != null)
				{
					try
					{
						gameObject2.GetComponent<Rigidbody>().inertiaTensor = origChunkTensor.Value * tensorMultiplier;
					}
					catch
					{
						BWLog.Info("Unable to set inertia tensor, possibly due to the use of rigidbody constraints in the world.");
					}
				}
			}
		}
		prevMassMultiplier = massMultiplier;
		prevTensorMultiplier = tensorMultiplier;
		tensorMultiplier = 1f;
		massMultiplier = 1f;
	}

	protected virtual Vector3 GetTargetPositionOffset()
	{
		return Vector3.zero;
	}

	private void ApplyChunkPositionForces()
	{
		if (!(chunkRigidBody != null))
		{
			return;
		}
		Vector3 vector = targetPosition + GetTargetPositionOffset();
		Vector3 vector2 = vector - GetPlatformPosition();
		float magnitude = vector2.magnitude;
		if ((double)magnitude > 1E-05)
		{
			Vector3 vector3 = vector2 * 20f;
			Vector3 velocity = chunkRigidBody.velocity;
			Vector3 vector4 = vector3 - velocity;
			if (vector4.sqrMagnitude > 400f)
			{
				vector4 = vector4.normalized * 20f;
			}
			float mass = chunkRigidBody.mass;
			float num = 20f;
			Vector3 force = vector4 * mass * num;
			chunkRigidBody.AddForce(force);
		}
	}

	protected virtual Quaternion GetRotationOffset()
	{
		return Quaternion.identity;
	}

	protected virtual void AlignPlatform()
	{
		Quaternion quaternion = GetRotationOffset() * rotation;
		Align(upDirection, quaternion * Vector3.up);
		Align(forwardDirection, quaternion * Vector3.forward);
	}

	private float GetTorqueScaler(Vector3 normalizedTorque)
	{
		return Mathf.Abs(Vector3.Dot(normalizedTorque, forwardMassDistribution * goT.forward)) + Mathf.Abs(Vector3.Dot(normalizedTorque, rightMassDistribution * goT.right)) + Mathf.Abs(Vector3.Dot(normalizedTorque, upMassDistribution * goT.up));
	}

	protected virtual void Align(Vector3 target, Vector3 localUp)
	{
		if (chunkRigidBody != null)
		{
			Vector3 vector = goT.TransformDirection(localUp);
			float a = Vector3.Angle(vector, target);
			Vector3 vector2 = Vector3.Cross(vector, target);
			vector2 = ((!(vector2.sqrMagnitude > 0.001f)) ? goT.forward : vector2.normalized);
			float torqueScaler = GetTorqueScaler(vector2);
			float num = 0.7f;
			Vector3 torque = num * Mathf.Min(a, 90f) * torqueScaler * vector2;
			Vector3 angularVelocity = chunkRigidBody.angularVelocity;
			angularVelocity = Util.ProjectOntoPlane(angularVelocity, target.normalized);
			float magnitude = angularVelocity.magnitude;
			if (magnitude > 0.001f)
			{
				Vector3 normalizedTorque = angularVelocity / magnitude;
				float torqueScaler2 = GetTorqueScaler(normalizedTorque);
				float num2 = 1f;
				float num3 = magnitude * num2 * torqueScaler2;
				torque += (0f - num3) * angularVelocity;
			}
			chunkRigidBody.AddTorque(torque);
		}
	}
}
