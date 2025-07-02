using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractRotatingPlatform : BlockAbstractPlatform
{
	protected float targetAngle;

	protected float prevTargetAngle;

	protected float targetSteps;

	protected float prevTargetSteps;

	protected float diffTargetSteps;

	protected float lastErrorAngle;

	protected bool spinFree;

	protected bool colliding;

	private const float STEPS_PER_ANGLE = 100000f;

	public BlockAbstractRotatingPlatform(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public override void Play2()
	{
		base.Play2();
		targetAngle = 0f;
		prevTargetAngle = 0f;
		targetSteps = 0f;
		prevTargetSteps = 0f;
		spinFree = false;
		lastErrorAngle = 0f;
		colliding = false;
		diffTargetSteps = 0f;
	}

	protected override Quaternion GetRotationOffset()
	{
		return Quaternion.Euler(new Vector3(0f, 0f, targetAngle));
	}

	protected int GetSteps(float angle)
	{
		return Mathf.RoundToInt(angle * 100000f);
	}

	public TileResultCode AtAngle(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0f);
			int steps = GetSteps(floatArg);
			int steps2 = GetSteps(1f);
			int steps3 = GetSteps(360f);
			float num = Mathf.Min(Mathf.Abs(diffTargetSteps), Mathf.Abs(diffTargetSteps + (float)steps3), Mathf.Abs(diffTargetSteps - (float)steps3));
			float num2 = num / 100000f;
			float num3 = Mathf.Min(Mathf.Abs((float)steps - targetSteps), Mathf.Abs((float)(steps + steps3) - targetSteps), Mathf.Abs((float)(steps + 2 * steps3) - targetSteps), Mathf.Abs((float)(steps - steps3) - targetSteps), Mathf.Abs((float)(steps - 2 * steps3) - targetSteps));
			if (num3 < Mathf.Max(0.5f * num, steps2) && Mathf.Abs(lastErrorAngle) <= Mathf.Max(5f * num2, 10f))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public TileResultCode IncreaseAngle(float velocity, float duration, float timer)
	{
		bool flag = timer + Blocksworld.fixedDeltaTime >= duration;
		float num = Blocksworld.fixedDeltaTime;
		if (duration > 0f && timer + Blocksworld.fixedDeltaTime > duration)
		{
			num = Mathf.Max(num - (timer + Blocksworld.fixedDeltaTime - duration), 0f);
		}
		float angle = velocity * num;
		if (colliding)
		{
			angle = 0f;
		}
		targetSteps += GetSteps(angle);
		if (targetSteps > (float)GetSteps(180f))
		{
			targetSteps -= GetSteps(360f);
		}
		else if (targetSteps < (float)GetSteps(-180f))
		{
			targetSteps += GetSteps(360f);
		}
		targetAngle = targetSteps / 100000f;
		spinFree = false;
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode IncreaseAngleDurational(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			float floatArg = Util.GetFloatArg(args, 0, 45f);
			float num = Mathf.Max(Util.GetFloatArg(args, 1, 0.25f), 0.05f);
			float velocity = floatArg / num;
			return IncreaseAngle(velocity, num, eInfo.timer);
		}
		return TileResultCode.True;
	}

	public TileResultCode IncreaseAngleNonDurational(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			float floatArg = Util.GetFloatArg(args, 0, 45f);
			return IncreaseAngle(floatArg, 0f, eInfo.timer);
		}
		return TileResultCode.True;
	}

	public TileResultCode ReturnAngle(float speed, float duration, float timer)
	{
		float num = speed * Blocksworld.fixedDeltaTime;
		if (Mathf.Abs(targetAngle) <= num)
		{
			targetAngle = 0f;
		}
		else
		{
			int num2 = 1;
			if (targetAngle > 0f)
			{
				num2 = -1;
			}
			targetAngle += (float)num2 * num;
		}
		targetSteps = GetSteps(targetAngle);
		spinFree = false;
		if (!(timer + Blocksworld.fixedDeltaTime < duration))
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode ReturnAngleDurational(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			float num = Mathf.Abs(Util.GetFloatArg(args, 0, 45f));
			float num2 = Mathf.Max(Util.GetFloatArg(args, 1, 0f), 0.05f);
			float speed = num / num2;
			return ReturnAngle(speed, num2, eInfo.timer);
		}
		return TileResultCode.True;
	}

	public TileResultCode ReturnAngleNonDurational(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			float speed = Mathf.Abs(Util.GetFloatArg(args, 0, 45f));
			return ReturnAngle(speed, 0f, eInfo.timer);
		}
		return TileResultCode.True;
	}

	public TileResultCode FreeSpin(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (enabled && !broken)
		{
			massMultiplier = Util.GetFloatArg(args, 0, 1f);
			tensorMultiplier = massMultiplier;
			spinFree = true;
		}
		return TileResultCode.True;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		diffTargetSteps = targetSteps - prevTargetSteps;
		prevTargetAngle = targetAngle;
		prevTargetSteps = targetSteps;
	}
}
