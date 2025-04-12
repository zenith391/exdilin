using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000297 RID: 663
	public class CharacterIdleState : CharacterBaseState
	{
		// Token: 0x06001ED0 RID: 7888 RVA: 0x000DC890 File Offset: 0x000DAC90
		public override void Enter(CharacterStateHandler parent, bool interrupt)
		{
			base.Enter(parent, interrupt);
			if (this.animations.Count == 0)
			{
				BWLog.Error("Attempting to start animation, but none found in list!");
				return;
			}
			parent.sideAnim = -1;
			if (parent.targetObject.goT.up.y > 0.8f)
			{
				parent.animationHash = parent.PlayAnimation(this.animations[0], interrupt);
				parent.timeInAnim = (float)UnityEngine.Random.Range(10, 25);
			}
			else
			{
				this.EnterSideAnim(parent);
			}
		}

		// Token: 0x06001ED1 RID: 7889 RVA: 0x000DC920 File Offset: 0x000DAD20
		protected void EnterSideAnim(CharacterStateHandler parent)
		{
			string animName = string.Empty;
			int num = parent.sideAnim;
			parent.stateBlend = 0.1f;
			if (parent.targetObject.goT.up.y < -0.5f)
			{
				num = 0;
				animName = ((!(this.topIdle != string.Empty)) ? this.animations[0] : this.topIdle);
			}
			else if (parent.targetObject.goT.forward.y < -0.5f)
			{
				num = 1;
				animName = ((!(this.frontIdle != string.Empty)) ? this.animations[0] : this.frontIdle);
			}
			else if (parent.targetObject.goT.forward.y > 0.5f)
			{
				num = 2;
				animName = ((!(this.backIdle != string.Empty)) ? this.animations[0] : this.backIdle);
			}
			else if (parent.targetObject.goT.right.y > 0.5f)
			{
				num = 3;
				animName = ((!(this.leftIdle != string.Empty)) ? this.animations[0] : this.leftIdle);
			}
			else
			{
				if (parent.targetObject.goT.right.y >= -0.5f)
				{
					return;
				}
				num = 4;
				animName = ((!(this.rightIdle != string.Empty)) ? this.animations[0] : this.rightIdle);
			}
			if (num != parent.sideAnim)
			{
				parent.stateTime = 0f;
				parent.timeInAnim = 3f;
				parent.lastForward = parent.targetObject.goT.forward;
				parent.lastRight = parent.targetObject.goT.right;
				parent.lastUp = parent.targetObject.goT.up;
				parent.sideAnim = num;
				parent.PlayAnimation(animName, false);
			}
		}

		// Token: 0x06001ED2 RID: 7890 RVA: 0x000DCB6B File Offset: 0x000DAF6B
		public bool IsOnSide(CharacterStateHandler parent)
		{
			return parent.sideAnim != -1;
		}

		// Token: 0x06001ED3 RID: 7891 RVA: 0x000DCB79 File Offset: 0x000DAF79
		public override Vector3 GetOffset(CharacterStateHandler parent)
		{
			if (!this.IsOnSide(parent) || this.sideOffsets == null)
			{
				return base.GetOffset(parent);
			}
			return this.sideOffsets[parent.sideAnim];
		}

		// Token: 0x06001ED4 RID: 7892 RVA: 0x000DCBAB File Offset: 0x000DAFAB
		public override void Exit(CharacterStateHandler parent)
		{
			base.Exit(parent);
		}

		// Token: 0x06001ED5 RID: 7893 RVA: 0x000DCBB4 File Offset: 0x000DAFB4
		public override bool Update(CharacterStateHandler parent)
		{
			base.Update(parent);
			if (parent.targetController.IsInTransition(0) || parent.currentState == CharacterState.EditSitting)
			{
				return true;
			}
			bool flag = false;
			if (parent.sideAnim != -1)
			{
				parent.lastSideAnim = parent.sideAnim;
				if (!parent.IsPulling() && !parent.WasPulling())
				{
					Quaternion sideAnimRotation = Quaternion.identity;
					Vector3 vector = parent.lastForward;
					vector.y = 0f;
					vector = vector.normalized;
					Vector3 vector2 = parent.lastUp;
					vector2.y = 0f;
					vector2 = vector2.normalized;
					Vector3 vector3 = parent.lastForward;
					switch (parent.sideAnim)
					{
					case 0:
						vector3 = parent.lastForward;
						break;
					case 1:
						vector3 = parent.lastUp;
						break;
					case 2:
						vector3 = -parent.lastUp;
						break;
					case 3:
						vector3 = parent.lastForward;
						break;
					case 4:
						vector3 = parent.lastForward;
						break;
					}
					vector3.y = 0f;
					sideAnimRotation = Quaternion.LookRotation(vector3.normalized, Vector3.up);
					parent.SetSideAnimRotation(sideAnimRotation);
				}
				if (parent.stateTime > parent.timeInAnim)
				{
					parent.InterruptQueue(CharacterState.GetUp);
					return false;
				}
			}
			else if (parent.timeInAnim > 0f)
			{
				flag = (parent.stateTime > parent.timeInAnim);
				flag &= (parent.upperBody.GetState() == UpperBodyState.BaseLayer && parent.upperBody.stateTime > parent.timeInAnim);
			}
			else
			{
				flag = (!parent.targetController.IsInTransition(0) && parent.animationHash != parent.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash);
			}
			if (flag && Blocksworld.blocksworldCamera.firstPersonBlock != parent.targetObject)
			{
				parent.sideAnim = -1;
				int num = 0;
				if (parent.timeInAnim > 0f)
				{
					num = UnityEngine.Random.Range(0, this.animations.Count);
				}
				parent.stateBlend = 0.1f;
				string animName = this.animations[num];
				parent.animationHash = parent.PlayAnimation(animName, false);
				if (num == 0)
				{
					parent.timeInAnim = (float)UnityEngine.Random.Range(10, 25);
					parent.stateTime = 0f;
				}
				else
				{
					parent.timeInAnim = -1f;
				}
			}
			return true;
		}

		// Token: 0x040018D1 RID: 6353
		public List<string> animations;

		// Token: 0x040018D2 RID: 6354
		public string leftIdle;

		// Token: 0x040018D3 RID: 6355
		public string rightIdle;

		// Token: 0x040018D4 RID: 6356
		public string frontIdle;

		// Token: 0x040018D5 RID: 6357
		public string backIdle;

		// Token: 0x040018D6 RID: 6358
		public string topIdle;

		// Token: 0x040018D7 RID: 6359
		public List<Vector3> sideOffsets;
	}
}
