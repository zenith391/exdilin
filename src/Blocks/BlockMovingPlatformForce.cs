using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockMovingPlatformForce : BlockAbstractMovingPlatform
{
	public BlockMovingPlatformForce(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMovingPlatformForce>("MovingPlatformForce.MoveTo", null, (Block b) => ((BlockMovingPlatformForce)b).MoveTo, new Type[2]
		{
			typeof(int),
			typeof(float)
		});
		PredicateRegistry.Add<BlockMovingPlatformForce>("MovingPlatformForce.MoveTowards", null, (Block b) => ((BlockMovingPlatformForce)b).MoveTowards, new Type[2]
		{
			typeof(int),
			typeof(float)
		});
		PredicateRegistry.Add<BlockMovingPlatformForce>("MovingPlatformForce.StepTowards", null, (Block b) => ((BlockMovingPlatformForce)b).StepTowards, new Type[3]
		{
			typeof(int),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockMovingPlatformForce>("MovingPlatformForce.AtPosition", (Block b) => ((BlockMovingPlatformForce)b).AtPosition, null, new Type[1] { typeof(int) });
	}

	public TileResultCode StepTowards(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (chunkRigidBody != null && enabled)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			float floatArg = Util.GetFloatArg(args, 1, 5f);
			float num = Mathf.Max(0.001f, Util.GetFloatArg(args, 2, 5f) * eInfo.floatArg);
			float num2 = floatArg / num;
			float magnitude = (positions[1] - positions[0]).magnitude;
			float num3 = num * Blocksworld.fixedDeltaTime;
			if (intArg == 0)
			{
				num3 *= -1f;
			}
			positionOffset = Mathf.Clamp(positionOffset + num3, 0f, magnitude);
			maxSpeed = Mathf.Max(maxSpeed, Mathf.Abs(num));
			if (!(eInfo.timer < num2))
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public TileResultCode MoveTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (chunkRigidBody != null && enabled)
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
			maxSpeed = Mathf.Max(maxSpeed, Mathf.Abs(num));
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
		if (enabled)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			float magnitude = (positions[1] - positions[0]).magnitude;
			float num = ((intArg != 0) ? (magnitude - positionOffset) : positionOffset);
			float magnitude2 = (positions[intArg] - goT.position).magnitude;
			if (!(num >= 0.01f) && !(magnitude2 >= 0.25f))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.True;
	}

	public TileResultCode MoveTowards(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (chunkRigidBody != null && enabled)
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
			maxSpeed = Mathf.Max(maxSpeed, Mathf.Abs(num));
		}
		return TileResultCode.True;
	}
}
