using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000B7 RID: 183
	public class BlockMovingPlatformForce : BlockAbstractMovingPlatform
	{
		// Token: 0x06000E3A RID: 3642 RVA: 0x000606C3 File Offset: 0x0005EAC3
		public BlockMovingPlatformForce(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000E3B RID: 3643 RVA: 0x000606CC File Offset: 0x0005EACC
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMovingPlatformForce>("MovingPlatformForce.MoveTo", null, (Block b) => new PredicateActionDelegate(((BlockMovingPlatformForce)b).MoveTo), new Type[]
			{
				typeof(int),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMovingPlatformForce>("MovingPlatformForce.MoveTowards", null, (Block b) => new PredicateActionDelegate(((BlockMovingPlatformForce)b).MoveTowards), new Type[]
			{
				typeof(int),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMovingPlatformForce>("MovingPlatformForce.StepTowards", null, (Block b) => new PredicateActionDelegate(((BlockMovingPlatformForce)b).StepTowards), new Type[]
			{
				typeof(int),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMovingPlatformForce>("MovingPlatformForce.AtPosition", (Block b) => new PredicateSensorDelegate(((BlockMovingPlatformForce)b).AtPosition), null, new Type[]
			{
				typeof(int)
			}, null, null);
		}

		// Token: 0x06000E3C RID: 3644 RVA: 0x00060808 File Offset: 0x0005EC08
		public TileResultCode StepTowards(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.chunkRigidBody != null && this.enabled)
			{
				int intArg = Util.GetIntArg(args, 0, 0);
				float floatArg = Util.GetFloatArg(args, 1, 5f);
				float num = Mathf.Max(0.001f, Util.GetFloatArg(args, 2, 5f) * eInfo.floatArg);
				float num2 = floatArg / num;
				float magnitude = (this.positions[1] - this.positions[0]).magnitude;
				float num3 = num * Blocksworld.fixedDeltaTime;
				if (intArg == 0)
				{
					num3 *= -1f;
				}
				this.positionOffset = Mathf.Clamp(this.positionOffset + num3, 0f, magnitude);
				this.maxSpeed = Mathf.Max(this.maxSpeed, Mathf.Abs(num));
				return (eInfo.timer < num2) ? TileResultCode.Delayed : TileResultCode.True;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000E3D RID: 3645 RVA: 0x000608FC File Offset: 0x0005ECFC
		public TileResultCode MoveTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.chunkRigidBody != null && this.enabled)
			{
				int intArg = Util.GetIntArg(args, 0, 0);
				float num = Mathf.Max(0.001f, Util.GetFloatArg(args, 1, 5f) * eInfo.floatArg);
				float magnitude = (this.positions[1] - this.positions[0]).magnitude;
				float num2 = (intArg != 0) ? (magnitude - this.positionOffset) : this.positionOffset;
				float num3 = num2 / num;
				float num4 = num * Blocksworld.fixedDeltaTime;
				if (intArg == 0)
				{
					num4 *= -1f;
				}
				this.positionOffset = Mathf.Clamp(this.positionOffset + num4, 0f, magnitude);
				this.maxSpeed = Mathf.Max(this.maxSpeed, Mathf.Abs(num));
				return (num3 <= 0f) ? TileResultCode.True : TileResultCode.Delayed;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000E3E RID: 3646 RVA: 0x000609FC File Offset: 0x0005EDFC
		public TileResultCode AtPosition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled)
			{
				int intArg = Util.GetIntArg(args, 0, 0);
				float magnitude = (this.positions[1] - this.positions[0]).magnitude;
				float num = (intArg != 0) ? (magnitude - this.positionOffset) : this.positionOffset;
				float magnitude2 = (this.positions[intArg] - this.goT.position).magnitude;
				return (num >= 0.01f || magnitude2 >= 0.25f) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000E3F RID: 3647 RVA: 0x00060AB4 File Offset: 0x0005EEB4
		public TileResultCode MoveTowards(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.chunkRigidBody != null && this.enabled)
			{
				int intArg = Util.GetIntArg(args, 0, 0);
				float num = Mathf.Max(0.001f, Util.GetFloatArg(args, 1, 5f) * eInfo.floatArg);
				float magnitude = (this.positions[1] - this.positions[0]).magnitude;
				float num2 = num * Blocksworld.fixedDeltaTime;
				if (intArg == 0)
				{
					num2 *= -1f;
				}
				this.positionOffset = Mathf.Clamp(this.positionOffset + num2, 0f, magnitude);
				this.maxSpeed = Mathf.Max(this.maxSpeed, Mathf.Abs(num));
			}
			return TileResultCode.True;
		}
	}
}
