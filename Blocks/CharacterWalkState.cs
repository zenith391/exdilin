using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020002A2 RID: 674
	public class CharacterWalkState : CharacterBaseState
	{
		// Token: 0x06001F4A RID: 8010 RVA: 0x000E19F4 File Offset: 0x000DFDF4
		public CharacterWalkState()
		{
			this.allWalkData = new List<CharacterWalkState.WalkData>();
			for (int i = 0; i < 8; i++)
			{
				this.allWalkData.Add(new CharacterWalkState.WalkData());
			}
		}

		// Token: 0x06001F4B RID: 8011 RVA: 0x000E1A3F File Offset: 0x000DFE3F
		private static bool IsStrafeDir(CharacterWalkState.WalkDirection dir)
		{
			return dir == CharacterWalkState.WalkDirection.StrafeLeft || dir == CharacterWalkState.WalkDirection.StrafeRight;
		}

		// Token: 0x06001F4C RID: 8012 RVA: 0x000E1A4F File Offset: 0x000DFE4F
		private static bool IsTurnDir(CharacterWalkState.WalkDirection direction)
		{
			return direction == CharacterWalkState.WalkDirection.FastTurnLeft || direction == CharacterWalkState.WalkDirection.FastTurnRight || direction == CharacterWalkState.WalkDirection.TurnLeft || direction == CharacterWalkState.WalkDirection.TurnRight;
		}

		// Token: 0x06001F4D RID: 8013 RVA: 0x000E1A6D File Offset: 0x000DFE6D
		private static bool IsFastTurnDir(CharacterWalkState.WalkDirection dir)
		{
			return dir == CharacterWalkState.WalkDirection.FastTurnLeft || dir == CharacterWalkState.WalkDirection.FastTurnRight;
		}

		// Token: 0x06001F4E RID: 8014 RVA: 0x000E1A80 File Offset: 0x000DFE80
		public void AddDirection(JObject state)
		{
			List<string> animations = (!state.ContainsKey("animations")) ? null : new List<string>(state["animations"].StringValue.Split(new char[]
			{
				','
			}));
			CharacterWalkState.WalkDirection index = (!state.ContainsKey("direction")) ? CharacterWalkState.WalkDirection.Forward : ((CharacterWalkState.WalkDirection)Enum.Parse(typeof(CharacterWalkState.WalkDirection), state["direction"].StringValue));
			CharacterWalkState.WalkData walkData = this.allWalkData[(int)index];
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
				JObject jobject = state["fpcTilt"];
				walkData.fpcTilt = new List<float>();
				for (int l = 0; l < jobject.Count; l++)
				{
					walkData.fpcTilt.Add(jobject[l].FloatValue);
				}
				while (walkData.fpcTilt.Count < 3)
				{
					walkData.fpcTilt.Add(0f);
				}
			}
		}

		// Token: 0x06001F4F RID: 8015 RVA: 0x000E1D30 File Offset: 0x000E0130
		protected void BlendAnimationRate(CharacterStateHandler parent, CharacterWalkState.WalkData walkState, float speed, float minSpeed, float maxSpeed)
		{
			if (parent.currentDirection >= CharacterWalkState.WalkDirection.TurnLeft)
			{
				parent.desiredAnimSpeed = 1f;
				return;
			}
			float num = (speed - minSpeed) / (maxSpeed - minSpeed);
			parent.desiredAnimSpeed = (1f - num) * walkState.animationRate[2 * parent.currentVelocityRange] + num * walkState.animationRate[2 * parent.currentVelocityRange + 1];
		}

		// Token: 0x06001F50 RID: 8016 RVA: 0x000E1D9C File Offset: 0x000E019C
		protected void UpdateWalkGeneral(CharacterStateHandler parent, CharacterWalkState.WalkDirection desiredDir)
		{
			int num = (int)desiredDir;
			if (num >= 8)
			{
				num = 0;
			}
			if (this.allWalkData[num].animations == null || this.allWalkData[num].animations.Count == 0)
			{
				num = 0;
			}
			CharacterWalkState.WalkData walkData = this.allWalkData[num];
			int currentVelocityRange = parent.currentVelocityRange;
			bool flag = CharacterWalkState.IsTurnDir(parent.currentDirection);
			bool flag2 = CharacterWalkState.IsStrafeDir(parent.currentDirection);
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
					this.BlendAnimationRate(parent, walkData, parent.currentSpeed, 0f, walkData.velocityChange[0]);
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
					this.BlendAnimationRate(parent, walkData, parent.currentSpeed, walkData.velocityChange[1], walkData.velocityChange[2]);
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
			bool flag3 = CharacterWalkState.IsTurnDir(desiredDir);
			bool flag4 = CharacterWalkState.IsStrafeDir(desiredDir);
			bool flag5 = (flag3 && !flag) || (flag && !flag3);
			bool flag6 = (flag4 && !flag2) || (flag2 && !flag4);
			bool flag7 = num2 > 0.08f || flag6 || flag5;
			bool flag8 = !flag7 && parent.targetController.IsInTransition(0);
			if ((parent.currentVelocityRange != currentVelocityRange || desiredDir != parent.currentDirection) && !flag8)
			{
				parent.currentDirection = desiredDir;
				Blocksworld.blocksworldCamera.fpcTilt = ((walkData.fpcTilt == null) ? 0f : walkData.fpcTilt[parent.currentVelocityRange]);
				if (this.isInterrupting)
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
						parent.PlayAnimation(walkData.animations[parent.currentVelocityRange], true);
					}
					else if (flag || flag3 || flag2 || flag4)
					{
						parent.PlayAnimation(walkData.animations[parent.currentVelocityRange], false);
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

		// Token: 0x06001F51 RID: 8017 RVA: 0x000E21E4 File Offset: 0x000E05E4
		protected void UpdateEnvironmentalWalk(CharacterStateHandler parent)
		{
			CharacterWalkState.WalkDirection desiredDir = parent.currentDirection;
			bool flag = Mathf.Abs(parent.currentVelocity.x) > Mathf.Abs(parent.currentVelocity.z);
			if (flag)
			{
				if (parent.currentVelocity.x > 0f)
				{
					desiredDir = CharacterWalkState.WalkDirection.StrafeRight;
					parent.currentSpeed = parent.offset.x;
				}
				else
				{
					desiredDir = CharacterWalkState.WalkDirection.StrafeLeft;
					parent.currentSpeed = -parent.offset.x;
				}
			}
			else if (parent.currentVelocity.z > 0f)
			{
				desiredDir = CharacterWalkState.WalkDirection.Forward;
				parent.currentSpeed = parent.offset.z;
			}
			else
			{
				desiredDir = CharacterWalkState.WalkDirection.Backward;
				parent.currentSpeed = -parent.offset.z;
			}
			this.UpdateWalkGeneral(parent, desiredDir);
		}

		// Token: 0x06001F52 RID: 8018 RVA: 0x000E22B0 File Offset: 0x000E06B0
		protected void UpdateDeliberateWalk(CharacterStateHandler parent)
		{
			CharacterWalkState.WalkDirection walkDirection = parent.currentDirection;
			if (parent.turnPower != 0f && CharacterWalkState.IsTurnDir(walkDirection))
			{
				this.UpdateWalkGeneral(parent, walkDirection);
				return;
			}
			if (parent.walkStrafe)
			{
				if (parent.desiredGoto.sqrMagnitude < 0.01f)
				{
					walkDirection = CharacterWalkState.WalkDirection.NumDirections;
				}
				else if (Mathf.Abs(parent.desiredGoto.z) >= 1.2f * Mathf.Abs(parent.desiredGoto.x))
				{
					if (parent.desiredGoto.z < 0f)
					{
						walkDirection = CharacterWalkState.WalkDirection.Backward;
					}
					else
					{
						walkDirection = CharacterWalkState.WalkDirection.Forward;
					}
				}
				else if (parent.desiredGoto.x < 0f)
				{
					walkDirection = CharacterWalkState.WalkDirection.StrafeLeft;
				}
				else
				{
					walkDirection = CharacterWalkState.WalkDirection.StrafeRight;
				}
			}
			else if (parent.turnPower != 0f && Mathf.Abs(parent.desiredGoto.z) < 0.05f)
			{
				if (parent.turnPower > 0f)
				{
					walkDirection = CharacterWalkState.WalkDirection.FastTurnRight;
				}
				else
				{
					walkDirection = CharacterWalkState.WalkDirection.FastTurnLeft;
				}
			}
			else if (parent.desiredGoto.magnitude >= 0.001f)
			{
				bool flag = parent.desiredGoto.x > 0f;
				float num = Mathf.Abs(Vector3.Angle(Vector3.forward, parent.desiredGoto));
				walkDirection = CharacterWalkState.WalkDirection.Forward;
				if (num > 120f)
				{
					if (flag)
					{
						walkDirection = ((!parent.walkStrafe) ? CharacterWalkState.WalkDirection.FastTurnRight : CharacterWalkState.WalkDirection.StrafeRight);
					}
					else
					{
						walkDirection = ((!parent.walkStrafe) ? CharacterWalkState.WalkDirection.FastTurnLeft : CharacterWalkState.WalkDirection.StrafeLeft);
					}
				}
				else if ((parent.currentDirection != CharacterWalkState.WalkDirection.NumDirections) ? (num > 90f) : (num > 20f))
				{
					if (flag && walkDirection != CharacterWalkState.WalkDirection.FastTurnRight)
					{
						walkDirection = ((!parent.walkStrafe) ? CharacterWalkState.WalkDirection.TurnRight : CharacterWalkState.WalkDirection.StrafeRight);
					}
					else if (walkDirection != CharacterWalkState.WalkDirection.FastTurnLeft)
					{
						walkDirection = ((!parent.walkStrafe) ? CharacterWalkState.WalkDirection.TurnLeft : CharacterWalkState.WalkDirection.StrafeLeft);
					}
				}
			}
			if (walkDirection == CharacterWalkState.WalkDirection.NumDirections || this.allWalkData[(int)walkDirection].animations == null || this.allWalkData[(int)walkDirection].animations.Count == 0)
			{
				walkDirection = CharacterWalkState.WalkDirection.Forward;
			}
			parent.currentSpeed = parent.offset.magnitude;
			this.UpdateWalkGeneral(parent, walkDirection);
		}

		// Token: 0x06001F53 RID: 8019 RVA: 0x000E2508 File Offset: 0x000E0908
		public override void Enter(CharacterStateHandler parent, bool interrupt)
		{
			base.Enter(parent, interrupt);
			this.isInterrupting = interrupt;
			parent.currentVelocityRange = -1;
			parent.currentSpeed = 0f;
			parent.currentDirection = CharacterWalkState.WalkDirection.NumDirections;
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
				this.UpdateDeliberateWalk(parent);
			}
			else
			{
				this.UpdateEnvironmentalWalk(parent);
			}
			this.isInterrupting = false;
		}

		// Token: 0x06001F54 RID: 8020 RVA: 0x000E25A6 File Offset: 0x000E09A6
		public override void Exit(CharacterStateHandler parent)
		{
			base.Exit(parent);
		}

		// Token: 0x06001F55 RID: 8021 RVA: 0x000E25B0 File Offset: 0x000E09B0
		public override bool Update(CharacterStateHandler parent)
		{
			base.Update(parent);
			if (parent.deliberateWalk)
			{
				if (parent.walkStrafe)
				{
					parent.speedForceModifier = 1f;
				}
				else if (parent.currentDirection < CharacterWalkState.WalkDirection.TurnLeft)
				{
					parent.speedForceModifier = Mathf.Lerp(parent.speedForceModifier, Vector3.Dot(Vector3.forward, parent.desiredGoto.normalized), 0.3f);
				}
				else if (CharacterWalkState.IsFastTurnDir(parent.currentDirection) && parent.targetObject.walkController.IsFPC())
				{
					parent.speedForceModifier = 0.8f;
				}
				else
				{
					parent.speedForceModifier = 0.01f;
				}
				this.UpdateDeliberateWalk(parent);
			}
			else
			{
				parent.speedForceModifier = Mathf.Lerp(parent.speedForceModifier, 1f, 0.1f);
				this.UpdateEnvironmentalWalk(parent);
			}
			return (parent.deliberateWalk || parent.currentSpeed >= 0.0075f) && (!parent.deliberateWalk || parent.desiresMove || parent.desiredGoto.magnitude >= 1.25f);
		}

		// Token: 0x06001F56 RID: 8022 RVA: 0x000E26E0 File Offset: 0x000E0AE0
		public override Vector3 GetOffset(CharacterStateHandler parent)
		{
			return Vector3.zero;
		}

		// Token: 0x06001F57 RID: 8023 RVA: 0x000E26E8 File Offset: 0x000E0AE8
		public override void OnScreenDebug(CharacterStateHandler parent)
		{
			int num = (int)parent.currentDirection;
			if (num >= 8)
			{
				num = 0;
			}
			ActionDebug.AddMessage("Walk", string.Concat(new object[]
			{
				string.Empty,
				num,
				" ",
				parent.currentDirection,
				" ",
				parent.currentSpeed,
				" range ",
				parent.currentVelocityRange,
				" turn ",
				parent.turnPower
			}), false);
		}

		// Token: 0x0400197A RID: 6522
		public List<CharacterWalkState.WalkData> allWalkData;

		// Token: 0x0400197B RID: 6523
		public float baseRunVel = 100f;

		// Token: 0x0400197C RID: 6524
		protected bool isInterrupting;

		// Token: 0x020002A3 RID: 675
		public enum WalkDirection
		{
			// Token: 0x0400197E RID: 6526
			Forward,
			// Token: 0x0400197F RID: 6527
			Backward,
			// Token: 0x04001980 RID: 6528
			StrafeLeft,
			// Token: 0x04001981 RID: 6529
			StrafeRight,
			// Token: 0x04001982 RID: 6530
			TurnLeft,
			// Token: 0x04001983 RID: 6531
			TurnRight,
			// Token: 0x04001984 RID: 6532
			FastTurnLeft,
			// Token: 0x04001985 RID: 6533
			FastTurnRight,
			// Token: 0x04001986 RID: 6534
			NumDirections
		}

		// Token: 0x020002A4 RID: 676
		public class WalkData
		{
			// Token: 0x04001987 RID: 6535
			public List<string> animations;

			// Token: 0x04001988 RID: 6536
			public List<float> blendSpeeds;

			// Token: 0x04001989 RID: 6537
			public List<float> animationRate;

			// Token: 0x0400198A RID: 6538
			public List<float> velocityChange;

			// Token: 0x0400198B RID: 6539
			public List<float> fpcTilt;
		}
	}
}
