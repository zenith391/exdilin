using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockRotatingPlatform : BlockAbstractRotatingPlatform, ITreasureAnimationDriver
{
	private bool movedCm;

	private Vector3 chunkCM = Vector3.zero;

	private float errorSum;

	private Vector3 origRight = Vector3.right;

	private Vector3 origPos = Vector3.zero;

	public BlockRotatingPlatform(List<List<Tile>> tiles)
		: base(tiles)
	{
		controlsVelocity = true;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.AtAngle", (Block b) => ((BlockRotatingPlatform)b).AtAngle, null, new Type[1] { typeof(float) }, new string[1] { "Angle" });
		PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.IncreaseAngle", null, (Block b) => ((BlockRotatingPlatform)b).IncreaseAngleDurational, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Angles", "Duration" });
		PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.IncreaseAngleNonDurational", null, (Block b) => ((BlockRotatingPlatform)b).IncreaseAngleNonDurational, new Type[1] { typeof(float) }, new string[1] { "Velocity" });
		PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.ReturnAngle", null, (Block b) => ((BlockRotatingPlatform)b).ReturnAngleDurational, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Angles", "Duration" });
		PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.ReturnAngleNonDurational", null, (Block b) => ((BlockRotatingPlatform)b).ReturnAngleNonDurational, new Type[1] { typeof(float) }, new string[1] { "Speed" });
		PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.FreeSpin", null, (Block b) => ((BlockRotatingPlatform)b).FreeSpin, new Type[1] { typeof(float) }, new string[1] { "Mass Multiplier" });
		Block.AddSimpleDefaultTiles(new GAF("RotatingPlatform.IncreaseAngleNonDurational", 45f), "Rotating Platform");
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		AlignPlatform();
		spinFree = false;
	}

	private RigidbodyConstraints GetConstraint(Vector3 v)
	{
		if (Mathf.Abs(Vector3.Dot(v, Vector3.up)) > 0.01f)
		{
			return RigidbodyConstraints.FreezeRotationY;
		}
		if (Mathf.Abs(Vector3.Dot(v, Vector3.right)) > 0.01f)
		{
			return RigidbodyConstraints.FreezeRotationX;
		}
		return RigidbodyConstraints.FreezeRotationZ;
	}

	private void SetConstraints()
	{
		if (chunkRigidBody != null && !broken)
		{
			goT.localPosition += Vector3.right * 0.05f;
			RigidbodyConstraints constraints = RigidbodyConstraints.FreezePosition | GetConstraint(upDirection) | GetConstraint(forwardDirection);
			chunkRigidBody.constraints = constraints;
		}
	}

	public override void Play2()
	{
		base.Play2();
		movedCm = false;
		if (!modelBlock.ContainsTileWithPredicate(Block.predicateFreeze) || ContainsTileWithPredicate(Block.predicateFreeze))
		{
			SetConstraints();
		}
		errorSum = 0f;
		origRight = goT.right;
		origPos = goT.position;
	}

	protected void MoveCM()
	{
		Vector3 worldCenterOfMass = chunkRigidBody.worldCenterOfMass;
		Vector3 position = goT.position;
		Vector3 vector = position - worldCenterOfMass;
		Vector3 centerOfMass = chunkRigidBody.centerOfMass + vector;
		chunkRigidBody.centerOfMass = centerOfMass;
		chunkRigidBody.inertiaTensorRotation = chunkRigidBody.rotation;
		chunkCM = centerOfMass;
		movedCm = true;
	}

	protected override void AlignPlatform()
	{
		if (!enabled || broken)
		{
			return;
		}
		colliding = false;
		Vector3 up = goT.up;
		if (chunkRigidBody != null && !chunkRigidBody.isKinematic)
		{
			if (!movedCm)
			{
				MoveCM();
			}
			if (spinFree)
			{
				targetAngle = 0f - Util.AngleBetween(up, upDirection, goT.right);
				targetSteps = GetSteps(targetAngle);
				lastErrorAngle = 0f;
			}
			else
			{
				Quaternion quaternion = Quaternion.AngleAxis(targetAngle, origRight);
				Vector3 vector = quaternion * upDirection;
				float value = Vector3.Angle(up, vector);
				float num = Mathf.Clamp(value, -20f, 20f);
				Vector3 vector2 = Vector3.Cross(up, vector);
				vector2 = ((!(vector2.sqrMagnitude > 1E-07f)) ? Vector3.zero : vector2.normalized);
				float num2 = 0.25f;
				float num3 = 0.002f;
				float num4 = Mathf.Abs(num);
				float num5 = errorSum;
				if (num4 < 1f)
				{
					errorSum *= num4;
				}
				else if (num4 < 3f)
				{
					errorSum = Mathf.Clamp(errorSum + num, -100f, 100f);
				}
				else
				{
					errorSum *= 3f / num4;
				}
				Vector3 vector3 = vector2 * (num2 * num + num3 * errorSum);
				float a = Mathf.Abs(targetAngle - prevTargetAngle);
				float num6 = Mathf.Max(a, 10f);
				if (num4 > num6 && CollisionManager.bumpedObject.Overlaps(chunk.blocks))
				{
					chunkRigidBody.angularVelocity = vector3 * 0.1f;
					targetAngle = prevTargetAngle;
					targetSteps = prevTargetSteps;
					errorSum = num5;
					colliding = true;
				}
				else
				{
					chunkRigidBody.angularVelocity = vector3;
				}
				lastErrorAngle = num;
			}
		}
		prevTargetAngle = targetAngle;
		prevTargetSteps = targetSteps;
	}

	public Vector3 GetTreasurePositionOffset(TreasureHandler.TreasureState state)
	{
		Vector3 position = state.transform.position;
		Vector3 vector = goT.position - position;
		return -vector;
	}

	public Quaternion GetTreasureRotation(TreasureHandler.TreasureState state)
	{
		return Quaternion.AngleAxis(targetAngle, origRight);
	}

	public bool TreasureAnimationActivated()
	{
		return enabled;
	}

	protected override void Vanishing(float scale)
	{
		base.Vanishing(scale);
		SetChunkPose(scale);
	}

	protected override void Appearing(float scale)
	{
		base.Appearing(scale);
		SetChunkPose(scale);
	}

	protected void SetChunkPose(float scale = 1f)
	{
		if (enabled && !didFix && !isTreasure && !broken)
		{
			Quaternion quaternion = Quaternion.AngleAxis(targetAngle, origRight);
			Transform transform = chunk.go.transform;
			transform.rotation = quaternion;
			Vector3 vector = origPos - goT.position;
			transform.position += vector;
		}
	}

	public override void Appeared()
	{
		base.Appeared();
		if (chunkRigidBody != null)
		{
			chunkRigidBody.centerOfMass = chunkCM;
		}
		SetChunkPose();
	}

	protected override void ModelBlockAppeared()
	{
		SetChunkPose();
		if (chunkRigidBody != null)
		{
			chunkRigidBody.centerOfMass = chunkCM;
		}
	}

	protected override void ModelBlockAppearing(float scale)
	{
		SetChunkPose(scale);
	}

	protected override void ModelBlockVanishing(float scale)
	{
		SetChunkPose(scale);
	}

	public override void ChunkInModelFrozen()
	{
		base.ChunkInModelFrozen();
		enabled = false;
	}

	public override void ChunkInModelUnfrozen()
	{
		base.ChunkInModelUnfrozen();
		foreach (Block item in Block.connectedCache[this])
		{
			if (item.didFix)
			{
				return;
			}
		}
		enabled = true;
	}
}
