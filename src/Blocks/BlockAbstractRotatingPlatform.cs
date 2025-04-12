using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000069 RID: 105
	public class BlockAbstractRotatingPlatform : BlockAbstractPlatform
	{
		// Token: 0x06000887 RID: 2183 RVA: 0x0003B70E File Offset: 0x00039B0E
		public BlockAbstractRotatingPlatform(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x0003B718 File Offset: 0x00039B18
		public override void Play2()
		{
			base.Play2();
			this.targetAngle = 0f;
			this.prevTargetAngle = 0f;
			this.targetSteps = 0f;
			this.prevTargetSteps = 0f;
			this.spinFree = false;
			this.lastErrorAngle = 0f;
			this.colliding = false;
			this.diffTargetSteps = 0f;
		}

		// Token: 0x06000889 RID: 2185 RVA: 0x0003B77C File Offset: 0x00039B7C
		protected override Quaternion GetRotationOffset()
		{
			return Quaternion.Euler(new Vector3(0f, 0f, this.targetAngle));
		}

		// Token: 0x0600088A RID: 2186 RVA: 0x0003B7A5 File Offset: 0x00039BA5
		protected int GetSteps(float angle)
		{
			return Mathf.RoundToInt(angle * 100000f);
		}

		// Token: 0x0600088B RID: 2187 RVA: 0x0003B7B4 File Offset: 0x00039BB4
		public TileResultCode AtAngle(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				float floatArg = Util.GetFloatArg(args, 0, 0f);
				int steps = this.GetSteps(floatArg);
				int steps2 = this.GetSteps(1f);
				int steps3 = this.GetSteps(360f);
				float num = Mathf.Min(new float[]
				{
					Mathf.Abs(this.diffTargetSteps),
					Mathf.Abs(this.diffTargetSteps + (float)steps3),
					Mathf.Abs(this.diffTargetSteps - (float)steps3)
				});
				float num2 = num / 100000f;
				float num3 = Mathf.Min(new float[]
				{
					Mathf.Abs((float)steps - this.targetSteps),
					Mathf.Abs((float)(steps + steps3) - this.targetSteps),
					Mathf.Abs((float)(steps + 2 * steps3) - this.targetSteps),
					Mathf.Abs((float)(steps - steps3) - this.targetSteps),
					Mathf.Abs((float)(steps - 2 * steps3) - this.targetSteps)
				});
				if (num3 < Mathf.Max(0.5f * num, (float)steps2) && Mathf.Abs(this.lastErrorAngle) <= Mathf.Max(5f * num2, 10f))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x0600088C RID: 2188 RVA: 0x0003B8F4 File Offset: 0x00039CF4
		public TileResultCode IncreaseAngle(float velocity, float duration, float timer)
		{
			bool flag = timer + Blocksworld.fixedDeltaTime >= duration;
			float num = Blocksworld.fixedDeltaTime;
			if (duration > 0f && timer + Blocksworld.fixedDeltaTime > duration)
			{
				num = Mathf.Max(num - (timer + Blocksworld.fixedDeltaTime - duration), 0f);
			}
			float angle = velocity * num;
			if (this.colliding)
			{
				angle = 0f;
			}
			this.targetSteps += (float)this.GetSteps(angle);
			if (this.targetSteps > (float)this.GetSteps(180f))
			{
				this.targetSteps -= (float)this.GetSteps(360f);
			}
			else if (this.targetSteps < (float)this.GetSteps(-180f))
			{
				this.targetSteps += (float)this.GetSteps(360f);
			}
			this.targetAngle = this.targetSteps / 100000f;
			this.spinFree = false;
			return (!flag) ? TileResultCode.Delayed : TileResultCode.True;
		}

		// Token: 0x0600088D RID: 2189 RVA: 0x0003B9F8 File Offset: 0x00039DF8
		public TileResultCode IncreaseAngleDurational(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				float floatArg = Util.GetFloatArg(args, 0, 45f);
				float num = Mathf.Max(Util.GetFloatArg(args, 1, 0.25f), 0.05f);
				float velocity = floatArg / num;
				return this.IncreaseAngle(velocity, num, eInfo.timer);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600088E RID: 2190 RVA: 0x0003BA54 File Offset: 0x00039E54
		public TileResultCode IncreaseAngleNonDurational(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				float floatArg = Util.GetFloatArg(args, 0, 45f);
				return this.IncreaseAngle(floatArg, 0f, eInfo.timer);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600088F RID: 2191 RVA: 0x0003BA98 File Offset: 0x00039E98
		public TileResultCode ReturnAngle(float speed, float duration, float timer)
		{
			float num = speed * Blocksworld.fixedDeltaTime;
			if (Mathf.Abs(this.targetAngle) <= num)
			{
				this.targetAngle = 0f;
			}
			else
			{
				int num2 = 1;
				if (this.targetAngle > 0f)
				{
					num2 = -1;
				}
				this.targetAngle += (float)num2 * num;
			}
			this.targetSteps = (float)this.GetSteps(this.targetAngle);
			this.spinFree = false;
			return (timer + Blocksworld.fixedDeltaTime < duration) ? TileResultCode.Delayed : TileResultCode.True;
		}

		// Token: 0x06000890 RID: 2192 RVA: 0x0003BB24 File Offset: 0x00039F24
		public TileResultCode ReturnAngleDurational(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				float num = Mathf.Abs(Util.GetFloatArg(args, 0, 45f));
				float num2 = Mathf.Max(Util.GetFloatArg(args, 1, 0f), 0.05f);
				float speed = num / num2;
				return this.ReturnAngle(speed, num2, eInfo.timer);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x0003BB84 File Offset: 0x00039F84
		public TileResultCode ReturnAngleNonDurational(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				float speed = Mathf.Abs(Util.GetFloatArg(args, 0, 45f));
				return this.ReturnAngle(speed, 0f, eInfo.timer);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000892 RID: 2194 RVA: 0x0003BBCD File Offset: 0x00039FCD
		public TileResultCode FreeSpin(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				this.massMultiplier = Util.GetFloatArg(args, 0, 1f);
				this.tensorMultiplier = this.massMultiplier;
				this.spinFree = true;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000893 RID: 2195 RVA: 0x0003BC0B File Offset: 0x0003A00B
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.diffTargetSteps = this.targetSteps - this.prevTargetSteps;
			this.prevTargetAngle = this.targetAngle;
			this.prevTargetSteps = this.targetSteps;
		}

		// Token: 0x0400068D RID: 1677
		protected float targetAngle;

		// Token: 0x0400068E RID: 1678
		protected float prevTargetAngle;

		// Token: 0x0400068F RID: 1679
		protected float targetSteps;

		// Token: 0x04000690 RID: 1680
		protected float prevTargetSteps;

		// Token: 0x04000691 RID: 1681
		protected float diffTargetSteps;

		// Token: 0x04000692 RID: 1682
		protected float lastErrorAngle;

		// Token: 0x04000693 RID: 1683
		protected bool spinFree;

		// Token: 0x04000694 RID: 1684
		protected bool colliding;

		// Token: 0x04000695 RID: 1685
		private const float STEPS_PER_ANGLE = 100000f;
	}
}
