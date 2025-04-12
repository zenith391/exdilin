using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000298 RID: 664
	public class CharacterJumpState : CharacterBaseState
	{
		// Token: 0x06001ED7 RID: 7895 RVA: 0x000DCE40 File Offset: 0x000DB240
		public bool CanAddJumpForce(CharacterStateHandler parent)
		{
			CharacterJumpState.JumpState currentJumpState = parent.currentJumpState;
			return (currentJumpState == CharacterJumpState.JumpState.Enter || currentJumpState == CharacterJumpState.JumpState.Up) && !parent.hasDoubleJumped;
		}

		// Token: 0x06001ED8 RID: 7896 RVA: 0x000DCE71 File Offset: 0x000DB271
		public bool HeadingUp(CharacterStateHandler parent)
		{
			return parent.currentJumpState < CharacterJumpState.JumpState.Peak;
		}

		// Token: 0x06001ED9 RID: 7897 RVA: 0x000DCE7C File Offset: 0x000DB27C
		public override void Enter(CharacterStateHandler parent, bool interrupt)
		{
			base.Enter(parent, interrupt);
			int index = 0;
			parent.currentJumpState = CharacterJumpState.JumpState.Enter;
			parent.desiredAnimSpeed = 1f;
			if (this.blendSpeeds != null)
			{
				parent.stateBlend = this.blendSpeeds[index];
			}
			if (parent.desiredJumpForce < 0f)
			{
				index = 2;
				parent.currentJumpState = CharacterJumpState.JumpState.Down;
				if (parent.targetObject.NearGround(0.1f) > 0f)
				{
					parent.stateBlend = 0.3f;
					index = 3;
					parent.currentJumpState = CharacterJumpState.JumpState.Land;
				}
				else
				{
					parent.stateBlend = 0.7f;
				}
			}
			parent.PlayAnimation(this.animations[index], interrupt);
			parent.hasDoubleJumped = false;
			parent.allowDouble = false;
		}

		// Token: 0x06001EDA RID: 7898 RVA: 0x000DCF3B File Offset: 0x000DB33B
		public override void Exit(CharacterStateHandler parent)
		{
			base.Exit(parent);
		}

		// Token: 0x06001EDB RID: 7899 RVA: 0x000DCF44 File Offset: 0x000DB344
		public void DoubleJump(CharacterStateHandler parent)
		{
			if (!this.CanAddJumpForce(parent) || parent.hasDoubleJumped || !parent.allowDouble)
			{
				return;
			}
			parent.hasDoubleJumped = true;
			parent.allowDouble = false;
			parent.PlayAnimation(this.animations[6], false);
			parent.targetObject.walkController.AddJumpForce(15f * parent.desiredJumpForce);
		}

		// Token: 0x06001EDC RID: 7900 RVA: 0x000DCFB2 File Offset: 0x000DB3B2
		public void Bounce(CharacterStateHandler parent)
		{
			parent.PlayAnimation(this.animations[6], false);
		}

		// Token: 0x06001EDD RID: 7901 RVA: 0x000DCFC8 File Offset: 0x000DB3C8
		public override bool Update(CharacterStateHandler parent)
		{
			base.Update(parent);
			parent.allowDouble = false;
			float y = parent.rb.velocity.y;
			if (parent.currentJumpState < CharacterJumpState.JumpState.Peak && parent.targetObject.goT.up.y < 0.999f)
			{
				Transform goT = parent.targetObject.goT;
				Vector3 vector = goT.forward;
				if (goT.up.y < 0.5f)
				{
					if (goT.forward.y > 0f)
					{
						vector = -goT.up;
					}
					else
					{
						vector = goT.up;
					}
				}
				vector.y = 0f;
				Quaternion b = Quaternion.LookRotation(vector.normalized, Vector3.up);
				parent.targetObject.goT.rotation = Quaternion.Slerp(parent.targetObject.goT.rotation, b, 0.2f);
			}
			int shortNameHash = parent.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash;
			switch (parent.currentJumpState)
			{
			case CharacterJumpState.JumpState.Enter:
				if (parent.animationHash != shortNameHash)
				{
					parent.animationHash = shortNameHash;
					if (this.blendSpeeds != null)
					{
						parent.stateBlend = this.blendSpeeds[1];
					}
					parent.currentJumpState = CharacterJumpState.JumpState.Up;
					parent.targetObject.walkController.AddJumpForce(15f * parent.desiredJumpForce);
				}
				break;
			case CharacterJumpState.JumpState.Up:
				if (y < 0.001f)
				{
					bool hasDoubleJumped = parent.hasDoubleJumped;
					if (hasDoubleJumped)
					{
						if (this.blendSpeeds != null)
						{
							parent.stateBlend = this.blendSpeeds[2];
						}
						parent.PlayAnimation(this.animations[1], false);
						parent.currentJumpState = CharacterJumpState.JumpState.Peak;
					}
					else
					{
						parent.currentJumpState = CharacterJumpState.JumpState.Down;
					}
				}
				break;
			case CharacterJumpState.JumpState.Peak:
				if (parent.animationHash != shortNameHash)
				{
					if (this.blendSpeeds != null)
					{
						parent.stateBlend = this.blendSpeeds[3];
					}
					parent.animationHash = shortNameHash;
					parent.currentJumpState = CharacterJumpState.JumpState.Down;
				}
				break;
			case CharacterJumpState.JumpState.Down:
				if (parent.targetObject.IsOnGround())
				{
					if (this.blendSpeeds != null)
					{
						parent.stateBlend = this.blendSpeeds[4];
					}
					float a = (parent.rb.velocity.y <= 0f) ? parent.rb.velocity.magnitude : 0f;
					parent.maxDownSpeedDuringJump = Mathf.Max(a, parent.maxDownSpeedDuringJump);
					parent.stateTime = 0f;
					parent.blendStart = 0f;
					float bounciness = 0f;
					if (parent.targetObject.CanBounce(out bounciness))
					{
						parent.Bounce(bounciness);
					}
					else if (parent.desiresMove)
					{
						if (this.blendSpeeds != null)
						{
							parent.stateBlend = this.blendSpeeds[5];
						}
						parent.currentJumpState = CharacterJumpState.JumpState.LandRunning;
						parent.PlayAnimation((parent.currentVelocity.z <= 9f) ? this.animations[4] : this.animations[5], false);
					}
					else
					{
						parent.PlayAnimation(this.animations[3], false);
						parent.currentJumpState = CharacterJumpState.JumpState.Land;
					}
					if (parent.currentJumpState == CharacterJumpState.JumpState.Land || parent.currentJumpState == CharacterJumpState.JumpState.LandRunning)
					{
						bool flag = parent.desiredGoto.sqrMagnitude > 0.1f;
						Vector3 downSlopeDirection = parent.targetObject.walkController.GetDownSlopeDirection();
						Vector3 forward = parent.targetObject.goT.forward;
						float num;
						float num2;
						if (flag)
						{
							num = 0.3f;
							num2 = 0.25f;
						}
						else
						{
							num = 0.1f;
							num2 = 0.45f;
						}
						if (downSlopeDirection.sqrMagnitude < Mathf.Epsilon)
						{
							num += num2;
							num2 = 0f;
						}
						Vector3 a2 = num * forward;
						Vector3 b2 = num2 * downSlopeDirection;
						parent.targetObject.walkController.slideVelocity = parent.maxDownSpeedDuringJump * (a2 + b2);
					}
					parent.desiredAnimSpeed = 1f;
				}
				break;
			case CharacterJumpState.JumpState.Land:
				if (parent.animationHash != shortNameHash && !parent.isTransitioning && parent.stateTime > 0.1f)
				{
					return false;
				}
				if (parent.desiresMove && !parent.isTransitioning && parent.stateTime > 0.1f)
				{
					return false;
				}
				break;
			case CharacterJumpState.JumpState.LandRunning:
				if (parent.animationHash != shortNameHash && !parent.isTransitioning && parent.stateTime > 0.05f)
				{
					return false;
				}
				break;
			}
			return true;
		}

		// Token: 0x06001EDE RID: 7902 RVA: 0x000DD4BA File Offset: 0x000DB8BA
		public override void OnScreenDebug(CharacterStateHandler parent)
		{
			ActionDebug.AddMessage("Jump State", string.Empty + parent.currentJumpState, false);
		}

		// Token: 0x040018D8 RID: 6360
		public List<string> animations;

		// Token: 0x040018D9 RID: 6361
		public List<float> blendSpeeds;

		// Token: 0x02000299 RID: 665
		public enum JumpState
		{
			// Token: 0x040018DB RID: 6363
			Enter,
			// Token: 0x040018DC RID: 6364
			Up,
			// Token: 0x040018DD RID: 6365
			Peak,
			// Token: 0x040018DE RID: 6366
			Down,
			// Token: 0x040018DF RID: 6367
			Land,
			// Token: 0x040018E0 RID: 6368
			LandRunning
		}
	}
}
