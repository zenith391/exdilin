using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockMovingPlatform : BlockAbstractMovingPlatform, ITreasureAnimationDriver
{
	private Vector3 origPosition;

	private const float STEPS_PER_UNIT = 100000f;

	private bool hasFrozenPosition;

	public BlockMovingPlatform(List<List<Tile>> tiles)
		: base(tiles)
	{
		controlsVelocity = true;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMovingPlatform>("MovingPlatform.MoveTo", null, (Block b) => ((BlockMovingPlatform)b).MoveTo, new Type[2]
		{
			typeof(int),
			typeof(float)
		}, new string[2] { "0: A, 1: B", "Speed" });
		PredicateRegistry.Add<BlockMovingPlatform>("MovingPlatform.MoveTowards", null, (Block b) => ((BlockMovingPlatform)b).MoveTowards, new Type[2]
		{
			typeof(int),
			typeof(float)
		}, new string[2] { "0: A, 1: B", "Speed" });
		PredicateRegistry.Add<BlockMovingPlatform>("MovingPlatform.StepTowards", null, (Block b) => ((BlockMovingPlatform)b).StepTowards, new Type[3]
		{
			typeof(int),
			typeof(float),
			typeof(float)
		}, new string[3] { "0: A, 1: B", "Units", "Speed" });
		PredicateRegistry.Add<BlockMovingPlatform>("MovingPlatform.AtPosition", (Block b) => ((BlockMovingPlatform)b).AtPosition, null, new Type[1] { typeof(int) }, new string[1] { "0: A, 1: B" });
		PredicateRegistry.Add<BlockMovingPlatform>("MovingPlatform.FreeSlide", null, (Block b) => ((BlockMovingPlatform)b).FreeSlide, new Type[1] { typeof(float) }, new string[1] { "Mass multiplier" });
		List<GAF> list = new List<GAF>();
		list.Add(new GAF("MovingPlatform.MoveTo", 1, 2f));
		list.Add(new GAF("MovingPlatform.MoveTo", 0, 2f));
		Block.AddSimpleDefaultTiles(list, "Moving Platform");
	}

	public TileResultCode FreeSlide(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			slideFree = true;
			massMultiplier = Util.GetFloatArg(args, 0, 1f);
		}
		return TileResultCode.True;
	}

	protected int GetSteps(float positionInc)
	{
		return Mathf.RoundToInt(positionInc * 100000f);
	}

	public Vector3 GetPositionOffset()
	{
		Vector3 vector = positions[1] - positions[0];
		if (vector.sqrMagnitude > 0.1f)
		{
			Vector3 normalized = vector.normalized;
			return normalized * positionOffset;
		}
		return Vector3.zero;
	}

	public TileResultCode StepTowards(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			float floatArg = Util.GetFloatArg(args, 1, 2f);
			float num = Mathf.Max(0.001f, Util.GetFloatArg(args, 2, 2f) * eInfo.floatArg);
			float num2 = floatArg / num;
			float magnitude = (positions[1] - positions[0]).magnitude;
			float num3 = 0f;
			if (num2 > 0f && eInfo.timer + Blocksworld.fixedDeltaTime > num2)
			{
				num3 = eInfo.timer + Blocksworld.fixedDeltaTime - num2;
			}
			float num4 = num * Mathf.Max(Blocksworld.fixedDeltaTime - num3, 0f);
			if (intArg == 0)
			{
				num4 *= -1f;
			}
			targetSteps = Mathf.Clamp(targetSteps + GetSteps(num4), 0, GetSteps(magnitude));
			positionOffset = (float)targetSteps / 100000f;
			slideFree = false;
			if (!(eInfo.timer + Blocksworld.fixedDeltaTime < num2))
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public TileResultCode MoveTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			float num = Mathf.Max(0.001f, Util.GetFloatArg(args, 1, 5f) * eInfo.floatArg);
			float magnitude = (positions[1] - positions[0]).magnitude;
			float num2 = ((intArg != 0) ? (magnitude - positionOffset) : positionOffset);
			float num3 = num2 / num;
			float num4 = num * Blocksworld.fixedDeltaTime;
			if (intArg == 0)
			{
				num4 *= -1f;
			}
			positionOffset = Mathf.Clamp(positionOffset + num4, 0f, magnitude);
			targetSteps = GetSteps(positionOffset);
			slideFree = false;
			if (!(num3 <= 0f))
			{
				return TileResultCode.Delayed;
			}
			return TileResultCode.True;
		}
		return TileResultCode.True;
	}

	public TileResultCode AtPosition(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			float magnitude = (positions[1] - positions[0]).magnitude;
			int num = ((intArg != 0) ? GetSteps(magnitude) : 0);
			slideFree = false;
			int value = num - targetSteps;
			if (!((float)Mathf.Abs(value) >= 10000f))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.True;
	}

	public TileResultCode MoveTowards(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			float num = Mathf.Max(0.001f, Util.GetFloatArg(args, 1, 5f) * eInfo.floatArg);
			float magnitude = (positions[1] - positions[0]).magnitude;
			float num2 = num * Blocksworld.fixedDeltaTime;
			if (intArg == 0)
			{
				num2 *= -1f;
			}
			positionOffset = Mathf.Clamp(positionOffset + num2, 0f, magnitude);
			targetSteps = GetSteps(positionOffset);
			maxSpeed = Mathf.Max(maxSpeed, Mathf.Abs(num));
			slideFree = false;
		}
		return TileResultCode.True;
	}

	private void ControlPlatform()
	{
		Vector3 vector = positions[1] - positions[0];
		Vector3 normalized = vector.normalized;
		Vector3 vector2 = goT.position - origPosition;
		Vector3 vector3 = vector2 + positions[0];
		Vector3 vector4 = vector3;
		bool flag = true;
		if (slideFree)
		{
			float num = Vector3.Dot(normalized, vector2);
			if (num < 0f)
			{
				vector4 = positions[0];
			}
			else if (num > vector.magnitude)
			{
				vector4 = positions[1];
			}
			else
			{
				flag = false;
			}
			positionOffset = Mathf.Clamp(num, 0f, vector.magnitude);
			targetSteps = GetSteps(positionOffset);
		}
		else
		{
			vector4 = positions[0] + normalized * positionOffset;
		}
		if (flag)
		{
			Vector3 vector5 = vector4 - vector3;
			float num2 = Mathf.Abs(positionOffset - lastPositionOffset);
			if (chunkRigidBody != null && !chunkRigidBody.isKinematic)
			{
				float num3 = Mathf.Max(num2 * 10f, 0.5f);
				if (vector5.sqrMagnitude > num3 * num3)
				{
					if (CollisionManager.bumpedObject.Overlaps(chunk.blocks))
					{
						Vector3 velocity = vector5;
						if (velocity.sqrMagnitude > 1f)
						{
							velocity.Normalize();
						}
						chunkRigidBody.velocity = velocity;
					}
					else
					{
						chunkRigidBody.velocity = vector5 * 10f;
					}
				}
				else
				{
					chunkRigidBody.velocity = vector5 * 10f;
				}
			}
		}
		lastTargetSteps = targetSteps;
		lastPositionOffset = positionOffset;
	}

	public override void Play2()
	{
		base.Play2();
		if (enabled)
		{
			hasFrozenPosition = false;
			UpdateFreezePosition(freezePosition: true);
			origPosition = goT.position;
		}
	}

	private void UpdateFreezePosition(bool freezePosition)
	{
		if (freezePosition == hasFrozenPosition)
		{
			return;
		}
		RigidbodyConstraints rigidbodyConstraints = RigidbodyConstraints.FreezeRotation;
		if (freezePosition)
		{
			Vector3 vector = positions[1] - positions[0];
			if (Mathf.Abs(vector.x) < 0.01f)
			{
				rigidbodyConstraints |= RigidbodyConstraints.FreezePositionX;
			}
			if (Mathf.Abs(vector.y) < 0.01f)
			{
				rigidbodyConstraints |= RigidbodyConstraints.FreezePositionY;
			}
			if (Mathf.Abs(vector.z) < 0.01f)
			{
				rigidbodyConstraints |= RigidbodyConstraints.FreezePositionZ;
			}
		}
		chunkRigidBody.constraints = rigidbodyConstraints;
		hasFrozenPosition = freezePosition;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (enabled && !broken)
		{
			if (didSlideFree != slideFree)
			{
				UpdateFreezePosition(slideFree);
			}
			ControlPlatform();
			didSlideFree = slideFree;
			slideFree = false;
		}
	}

	public Vector3 GetTreasurePositionOffset(TreasureHandler.TreasureState state)
	{
		return GetPositionOffset();
	}

	public Quaternion GetTreasureRotation(TreasureHandler.TreasureState state)
	{
		return Quaternion.identity;
	}

	public bool TreasureAnimationActivated()
	{
		return enabled;
	}

	protected override void Vanishing(float scale)
	{
		SetChildrenLocalScale(scale);
		UpdateFreezePosition(didSlideFree);
		SetChunkPosition(scale);
	}

	protected override void Appearing(float scale)
	{
		SetChildrenLocalScale(scale);
		UpdateFreezePosition(didSlideFree);
		SetChunkPosition(scale);
	}

	protected void SetChunkPosition(float scale = 1f)
	{
		if (enabled && !didFix && !isTreasure && !broken)
		{
			Vector3 vector = positions[1] - positions[0];
			if (!(vector.sqrMagnitude < 0.01f))
			{
				Vector3 vector2 = goT.position - origPosition;
				Vector3 vector3 = vector2 + positions[0];
				Vector3 normalized = vector.normalized;
				Vector3 vector4 = positions[0] + normalized * positionOffset;
				Vector3 vector5 = vector4 - vector3;
				Transform transform = chunk.go.transform;
				Vector3 position = transform.position + vector5;
				transform.position = position;
			}
		}
	}

	public override void Appeared()
	{
		base.Appeared();
		SetChildrenLocalScale(1f);
		SetChunkPosition();
		UpdateFreezePosition(freezePosition: true);
	}

	protected override void ModelBlockAppearing(float scale)
	{
		SetChunkPosition(scale);
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		SetChildrenLocalScale(1f);
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
