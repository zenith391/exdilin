using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Blocks;

public class CharacterWalkState : CharacterBaseState
{
	public enum WalkDirection
	{
		Forward,
		Backward,
		StrafeLeft,
		StrafeRight,
		TurnLeft,
		TurnRight,
		FastTurnLeft,
		FastTurnRight,
		NumDirections
	}

	public class WalkData
	{
		public List<string> animations;

		public List<float> blendSpeeds;

		public List<float> animationRate;

		public List<float> velocityChange;

		public List<float> fpcTilt;
	}

	public List<WalkData> allWalkData;

	public float baseRunVel = 100f;

	protected bool isInterrupting;

	public CharacterWalkState()
	{
		allWalkData = new List<WalkData>();
		for (int i = 0; i < 8; i++)
		{
			allWalkData.Add(new WalkData());
		}
	}

	private static bool IsStrafeDir(WalkDirection dir)
	{
		if (dir != WalkDirection.StrafeLeft)
		{
			return dir == WalkDirection.StrafeRight;
		}
		return true;
	}

	private static bool IsTurnDir(WalkDirection direction)
	{
		if (direction != WalkDirection.FastTurnLeft && direction != WalkDirection.FastTurnRight && direction != WalkDirection.TurnLeft)
		{
			return direction == WalkDirection.TurnRight;
		}
		return true;
	}

	private static bool IsFastTurnDir(WalkDirection dir)
	{
		if (dir != WalkDirection.FastTurnLeft)
		{
			return dir == WalkDirection.FastTurnRight;
		}
		return true;
	}

	public void AddDirection(JObject state)
	{
		List<string> animations = ((!state.ContainsKey("animations")) ? null : new List<string>(state["animations"].StringValue.Split(',')));
		WalkDirection index = (state.ContainsKey("direction") ? ((WalkDirection)Enum.Parse(typeof(WalkDirection), state["direction"].StringValue)) : WalkDirection.Forward);
		WalkData walkData = allWalkData[(int)index];
		walkData.animations = animations;
		walkData.blendSpeeds = new List<float>();
		if (state.ContainsKey("blends"))
		{
			List<JObject> arrayValue = state["blends"].ArrayValue;
			for (int i = 0; i < arrayValue.Count; i++)
			{
				walkData.blendSpeeds.Add(arrayValue[i].FloatValue);
			}
		}
		while (walkData.blendSpeeds.Count < 3)
		{
			walkData.blendSpeeds.Add(0.1f);
		}
		walkData.animationRate = new List<float>();
		if (state.ContainsKey("animationRate"))
		{
			List<JObject> arrayValue2 = state["animationRate"].ArrayValue;
			for (int j = 0; j < arrayValue2.Count; j++)
			{
				walkData.animationRate.Add(arrayValue2[j].FloatValue);
			}
		}
		while (walkData.animationRate.Count < 3)
		{
			walkData.animationRate.Add(1f);
		}
		walkData.velocityChange = new List<float>();
		if (state.ContainsKey("velocityChange"))
		{
			List<JObject> arrayValue3 = state["velocityChange"].ArrayValue;
			for (int k = 0; k < arrayValue3.Count; k++)
			{
				walkData.velocityChange.Add(arrayValue3[k].FloatValue);
			}
		}
		while (walkData.velocityChange.Count < 3)
		{
			walkData.velocityChange.Add(10000f);
		}
		if (state.ContainsKey("fpcTilt"))
		{
			JObject jObject = state["fpcTilt"];
			walkData.fpcTilt = new List<float>();
			for (int l = 0; l < jObject.Count; l++)
			{
				walkData.fpcTilt.Add(jObject[l].FloatValue);
			}
			while (walkData.fpcTilt.Count < 3)
			{
				walkData.fpcTilt.Add(0f);
			}
		}
	}

	protected void BlendAnimationRate(CharacterStateHandler parent, WalkData walkState, float speed, float minSpeed, float maxSpeed)
	{
		if (parent.currentDirection >= WalkDirection.TurnLeft)
		{
			parent.desiredAnimSpeed = 1f;
			return;
		}
		float num = (speed - minSpeed) / (maxSpeed - minSpeed);
		parent.desiredAnimSpeed = (1f - num) * walkState.animationRate[2 * parent.currentVelocityRange] + num * walkState.animationRate[2 * parent.currentVelocityRange + 1];
	}

	protected void UpdateWalkGeneral(CharacterStateHandler parent, WalkDirection desiredDir)
	{
		int num = (int)desiredDir;
		if (num >= 8)
		{
			num = 0;
		}
		if (allWalkData[num].animations == null || allWalkData[num].animations.Count == 0)
		{
			num = 0;
		}
		WalkData walkData = allWalkData[num];
		int currentVelocityRange = parent.currentVelocityRange;
		bool flag = IsTurnDir(parent.currentDirection);
		bool flag2 = IsStrafeDir(parent.currentDirection);
		if (desiredDir != parent.currentDirection)
		{
			parent.currentVelocityRange = 0;
		}
		float num2 = 0f;
		switch (parent.currentVelocityRange)
		{
		case 0:
			if (parent.currentSpeed > walkData.velocityChange[0])
			{
				num2 = parent.currentSpeed - walkData.velocityChange[0];
				parent.currentVelocityRange = 1;
				parent.desiredAnimSpeed = walkData.animationRate[2 * parent.currentVelocityRange];
			}
			else
			{
				BlendAnimationRate(parent, walkData, parent.currentSpeed, 0f, walkData.velocityChange[0]);
			}
			break;
		case 1:
			if (parent.currentSpeed > walkData.velocityChange[2])
			{
				num2 = parent.currentSpeed - walkData.velocityChange[2];
				parent.currentVelocityRange = 2;
				parent.desiredAnimSpeed = walkData.animationRate[2 * parent.currentVelocityRange];
			}
			else if (parent.currentSpeed < walkData.velocityChange[1])
			{
				parent.currentVelocityRange = 0;
				parent.desiredAnimSpeed = walkData.animationRate[2 * parent.currentVelocityRange + 1];
			}
			else
			{
				BlendAnimationRate(parent, walkData, parent.currentSpeed, walkData.velocityChange[1], walkData.velocityChange[2]);
			}
			break;
		case 2:
			if (parent.currentSpeed < walkData.velocityChange[3])
			{
				parent.currentVelocityRange = 1;
				parent.desiredAnimSpeed = walkData.animationRate[2 * parent.currentVelocityRange + 1];
			}
			break;
		default:
			if (parent.currentSpeed > walkData.velocityChange[2])
			{
				parent.currentVelocityRange = 2;
			}
			else if (parent.currentSpeed > walkData.velocityChange[0])
			{
				parent.currentVelocityRange = 1;
			}
			else
			{
				parent.currentVelocityRange = 0;
			}
			break;
		}
		bool flag3 = IsTurnDir(desiredDir);
		bool flag4 = IsStrafeDir(desiredDir);
		bool flag5 = (flag3 && !flag) || (flag && !flag3);
		bool flag6 = (flag4 && !flag2) || (flag2 && !flag4);
		bool flag7 = num2 > 0.08f || flag6 || flag5;
		bool flag8 = !flag7 && parent.targetController.IsInTransition(0);
		if ((parent.currentVelocityRange != currentVelocityRange || desiredDir != parent.currentDirection) && !flag8)
		{
			parent.currentDirection = desiredDir;
			Blocksworld.blocksworldCamera.fpcTilt = ((walkData.fpcTilt == null) ? 0f : walkData.fpcTilt[parent.currentVelocityRange]);
			if (isInterrupting)
			{
				parent.stateBlend = 0f;
			}
			else if (flag7)
			{
				parent.stateBlend = 0.05f;
			}
			else
			{
				parent.stateBlend = walkData.blendSpeeds[parent.currentVelocityRange];
			}
			parent.speedForceModifier = ((!flag5) ? 1f : 0.05f);
			if (parent.currentVelocityRange < walkData.animations.Count)
			{
				if (flag7)
				{
					parent.PlayAnimation(walkData.animations[parent.currentVelocityRange], interrupt: true);
				}
				else if (flag || flag3 || flag2 || flag4)
				{
					parent.PlayAnimation(walkData.animations[parent.currentVelocityRange]);
				}
				else
				{
					parent.ShiftAnimation(walkData.animations[parent.currentVelocityRange]);
				}
			}
		}
		else
		{
			parent.currentVelocityRange = currentVelocityRange;
		}
	}

	protected void UpdateEnvironmentalWalk(CharacterStateHandler parent)
	{
		WalkDirection currentDirection = parent.currentDirection;
		if (Mathf.Abs(parent.currentVelocity.x) > Mathf.Abs(parent.currentVelocity.z))
		{
			if (parent.currentVelocity.x > 0f)
			{
				currentDirection = WalkDirection.StrafeRight;
				parent.currentSpeed = parent.offset.x;
			}
			else
			{
				currentDirection = WalkDirection.StrafeLeft;
				parent.currentSpeed = 0f - parent.offset.x;
			}
		}
		else if (parent.currentVelocity.z > 0f)
		{
			currentDirection = WalkDirection.Forward;
			parent.currentSpeed = parent.offset.z;
		}
		else
		{
			currentDirection = WalkDirection.Backward;
			parent.currentSpeed = 0f - parent.offset.z;
		}
		UpdateWalkGeneral(parent, currentDirection);
	}

	protected void UpdateDeliberateWalk(CharacterStateHandler parent)
	{
		WalkDirection walkDirection = parent.currentDirection;
		if (parent.turnPower != 0f && IsTurnDir(walkDirection))
		{
			UpdateWalkGeneral(parent, walkDirection);
			return;
		}
		if (parent.walkStrafe)
		{
			walkDirection = ((parent.desiredGoto.sqrMagnitude < 0.01f) ? WalkDirection.NumDirections : ((Mathf.Abs(parent.desiredGoto.z) >= 1.2f * Mathf.Abs(parent.desiredGoto.x)) ? ((parent.desiredGoto.z < 0f) ? WalkDirection.Backward : WalkDirection.Forward) : ((!(parent.desiredGoto.x < 0f)) ? WalkDirection.StrafeRight : WalkDirection.StrafeLeft)));
		}
		else if (parent.turnPower != 0f && Mathf.Abs(parent.desiredGoto.z) < 0.05f)
		{
			walkDirection = ((!(parent.turnPower > 0f)) ? WalkDirection.FastTurnLeft : WalkDirection.FastTurnRight);
		}
		else if (parent.desiredGoto.magnitude >= 0.001f)
		{
			bool flag = parent.desiredGoto.x > 0f;
			float num = Mathf.Abs(Vector3.Angle(Vector3.forward, parent.desiredGoto));
			walkDirection = WalkDirection.Forward;
			if (num > 120f)
			{
				walkDirection = ((!flag) ? ((!parent.walkStrafe) ? WalkDirection.FastTurnLeft : WalkDirection.StrafeLeft) : ((!parent.walkStrafe) ? WalkDirection.FastTurnRight : WalkDirection.StrafeRight));
			}
			else if ((parent.currentDirection != WalkDirection.NumDirections) ? (num > 90f) : (num > 20f))
			{
				if (flag && walkDirection != WalkDirection.FastTurnRight)
				{
					walkDirection = ((!parent.walkStrafe) ? WalkDirection.TurnRight : WalkDirection.StrafeRight);
				}
				else if (walkDirection != WalkDirection.FastTurnLeft)
				{
					walkDirection = ((!parent.walkStrafe) ? WalkDirection.TurnLeft : WalkDirection.StrafeLeft);
				}
			}
		}
		if (walkDirection == WalkDirection.NumDirections || allWalkData[(int)walkDirection].animations == null || allWalkData[(int)walkDirection].animations.Count == 0)
		{
			walkDirection = WalkDirection.Forward;
		}
		parent.currentSpeed = parent.offset.magnitude;
		UpdateWalkGeneral(parent, walkDirection);
	}

	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		isInterrupting = interrupt;
		parent.currentVelocityRange = -1;
		parent.currentSpeed = 0f;
		parent.currentDirection = WalkDirection.NumDirections;
		if (parent.desiresMove)
		{
			parent.deliberateWalk = true;
			if (parent.targetObject.walkController.previousVicinityMode == WalkControllerAnimated.VicinityMode.AvoidTag && parent.desiredGoto.magnitude < 0.1f)
			{
				parent.deliberateWalk = false;
			}
		}
		if (parent.deliberateWalk)
		{
			UpdateDeliberateWalk(parent);
		}
		else
		{
			UpdateEnvironmentalWalk(parent);
		}
		isInterrupting = false;
	}

	public override void Exit(CharacterStateHandler parent)
	{
		base.Exit(parent);
	}

	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		if (parent.deliberateWalk)
		{
			if (parent.walkStrafe)
			{
				parent.speedForceModifier = 1f;
			}
			else if (parent.currentDirection < WalkDirection.TurnLeft)
			{
				parent.speedForceModifier = Mathf.Lerp(parent.speedForceModifier, Vector3.Dot(Vector3.forward, parent.desiredGoto.normalized), 0.3f);
			}
			else if (IsFastTurnDir(parent.currentDirection) && parent.targetObject.walkController.IsFPC())
			{
				parent.speedForceModifier = 0.8f;
			}
			else
			{
				parent.speedForceModifier = 0.01f;
			}
			UpdateDeliberateWalk(parent);
		}
		else
		{
			parent.speedForceModifier = Mathf.Lerp(parent.speedForceModifier, 1f, 0.1f);
			UpdateEnvironmentalWalk(parent);
		}
		if (parent.deliberateWalk || parent.currentSpeed >= 0.0075f)
		{
			if (parent.deliberateWalk && !parent.desiresMove)
			{
				return parent.desiredGoto.magnitude >= 1.25f;
			}
			return true;
		}
		return false;
	}

	public override Vector3 GetOffset(CharacterStateHandler parent)
	{
		return Vector3.zero;
	}

	public override void OnScreenDebug(CharacterStateHandler parent)
	{
		int num = (int)parent.currentDirection;
		if (num >= 8)
		{
			num = 0;
		}
		ActionDebug.AddMessage("Walk", string.Concat(string.Empty, num, " ", parent.currentDirection, " ", parent.currentSpeed, " range ", parent.currentVelocityRange, " turn ", parent.turnPower));
	}
}
