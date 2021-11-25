using System;
using System.Collections.Generic;
using Gestures;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000092 RID: 146
	public class BlockGrouped : Block
	{
		// Token: 0x06000C01 RID: 3073 RVA: 0x00055D3E File Offset: 0x0005413E
		public BlockGrouped(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C02 RID: 3074 RVA: 0x00055D47 File Offset: 0x00054147
		public override void SetBlockGroup(BlockGroup group)
		{
			base.SetBlockGroup(group);
			if (group is TankTreadsBlockGroup)
			{
				this.group = group;
			}
		}

		// Token: 0x06000C03 RID: 3075 RVA: 0x00055D62 File Offset: 0x00054162
		public virtual bool BlockUsesDefaultPaintsAndTextures()
		{
			return true;
		}

		// Token: 0x06000C04 RID: 3076 RVA: 0x00055D65 File Offset: 0x00054165
		public virtual bool GroupHasIndividualSripting()
		{
			return false;
		}

		// Token: 0x06000C05 RID: 3077 RVA: 0x00055D68 File Offset: 0x00054168
		public virtual bool GroupRotateMainBlockOnPlacement()
		{
			return true;
		}

		// Token: 0x06000C06 RID: 3078 RVA: 0x00055D6B File Offset: 0x0005416B
		public bool IsMainBlockInGroup()
		{
			return this.group != null && this.group.GetBlocks()[0] == this;
		}

		// Token: 0x06000C07 RID: 3079 RVA: 0x00055D8B File Offset: 0x0005418B
		public Block GetMainBlockInGroup()
		{
			return (this.group != null) ? this.group.GetBlocks()[0] : this;
		}

		// Token: 0x06000C08 RID: 3080 RVA: 0x00055DAC File Offset: 0x000541AC
		public override void Destroy()
		{
			if (this.IsMainBlockInGroup())
			{
				foreach (ScriptRowExecutionInfo scriptRowExecutionInfo in new List<ScriptRowExecutionInfo>(BlockGrouped.scriptInfos.Keys))
				{
					if (scriptRowExecutionInfo.block == this)
					{
						BlockGrouped.scriptInfos.Remove(scriptRowExecutionInfo);
					}
				}
			}
			base.Destroy();
		}

		// Token: 0x06000C09 RID: 3081 RVA: 0x00055E34 File Offset: 0x00054234
		public override TileResultCode IsWithinWater(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.IsWithinWater(eInfo, args);
			}
			foreach (Block block in this.group.GetBlocks())
			{
				if (BlockWater.BlockWithinWater(block, false))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x06000C0A RID: 3082 RVA: 0x00055E88 File Offset: 0x00054288
		public override TileResultCode WithinTaggedWater(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.WithinTaggedWater(eInfo, args);
			}
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			foreach (Block block in this.group.GetBlocks())
			{
				if (BlockWater.BlockWithinTaggedWater(block, stringArg))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x06000C0B RID: 3083 RVA: 0x00055EEC File Offset: 0x000542EC
		public override TileResultCode StopScriptsBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.StopScriptsBlock(eInfo, args);
			}
			foreach (Block block in this.group.GetBlocks())
			{
				if (Block.vanishingOrAppearingBlocks.Contains(block))
				{
					return TileResultCode.Delayed;
				}
				BWSceneManager.RemoveScriptBlock(block);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000C0C RID: 3084 RVA: 0x00055F4C File Offset: 0x0005434C
		public override TileResultCode Mute(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.Mute(eInfo, args);
			}
			foreach (Block block in this.group.GetBlocks())
			{
				block.Mute();
			}
			Blocksworld.AddFixedUpdateUniqueCommand(Block.unmuteCommand, true);
			return TileResultCode.True;
		}

		// Token: 0x06000C0D RID: 3085 RVA: 0x00055FA4 File Offset: 0x000543A4
		public override TileResultCode PlayVfxDurational(string vfxName, float lengthMult, float timer, string colorName)
		{
			string text;
			if (BlockGrouped.vfxTranslations.TryGetValue(vfxName, out text))
			{
				vfxName = text;
			}
			return base.PlayVfxDurational(vfxName, lengthMult, timer, colorName);
		}

		// Token: 0x06000C0E RID: 3086 RVA: 0x00055FD4 File Offset: 0x000543D4
		public override TileResultCode IsBumping(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.IsBumping(eInfo, args);
			}
			string target = (string)args[0];
			foreach (Block b in this.group.GetBlocks())
			{
				if (CollisionManager.IsBumpingBlock(b, target))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x06000C0F RID: 3087 RVA: 0x00056034 File Offset: 0x00054434
		public override TileResultCode IsBumpingTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.IsBumpingTag(eInfo, args);
			}
			string tag = (string)args[0];
			foreach (Block b in this.group.GetBlocks())
			{
				if (CollisionManager.BlockIsBumpingTag(b, tag))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x06000C10 RID: 3088 RVA: 0x00056094 File Offset: 0x00054494
		public override TileResultCode IsHitByProjectile(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.IsHitByProjectile(eInfo, args);
			}
			foreach (Block block in this.group.GetBlocks())
			{
				if (BlockAbstractLaser.IsHitByProjectile(block))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x06000C11 RID: 3089 RVA: 0x000560E8 File Offset: 0x000544E8
		public override TileResultCode IsHitByTaggedProjectile(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.IsHitByTaggedProjectile(eInfo, args);
			}
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			foreach (Block block in this.group.GetBlocks())
			{
				if (BlockAbstractLaser.IsHitByTaggedProjectile(block, stringArg))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x06000C12 RID: 3090 RVA: 0x0005614C File Offset: 0x0005454C
		public override TileResultCode IsHitByPulseOrBeam(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.IsHitByPulseOrBeam(eInfo, args);
			}
			foreach (Block block in this.group.GetBlocks())
			{
				if (BlockAbstractLaser.IsHitByPulseOrBeam(block))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x06000C13 RID: 3091 RVA: 0x000561A0 File Offset: 0x000545A0
		public override TileResultCode IsHitByTaggedPulseOrBeam(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.IsHitByTaggedPulseOrBeam(eInfo, args);
			}
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			foreach (Block block in this.group.GetBlocks())
			{
				if (BlockAbstractLaser.IsHitByTaggedPulseOrBeam(block, stringArg))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x06000C14 RID: 3092 RVA: 0x00056204 File Offset: 0x00054604
		public override TileResultCode PullLockBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.PullLockBlock(eInfo, args);
			}
			foreach (Block b in this.group.GetBlocks())
			{
				PullObjectGesture.PullLock(b);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000C15 RID: 3093 RVA: 0x00056250 File Offset: 0x00054650
		public override TileResultCode Unfreeze(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.Unfreeze(eInfo, args);
			}
			foreach (Block block in this.group.GetBlocks())
			{
				block.Unfreeze();
			}
			return TileResultCode.True;
		}

		// Token: 0x06000C16 RID: 3094 RVA: 0x0005629C File Offset: 0x0005469C
		public override TileResultCode Freeze(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.Freeze(eInfo, args);
			}
			foreach (Block block in this.group.GetBlocks())
			{
				block.Freeze(true);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000C17 RID: 3095 RVA: 0x000562EC File Offset: 0x000546EC
		public override TileResultCode RegisterTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.RegisterTag(eInfo, args);
			}
			string tagName = (string)args[0];
			foreach (Block block in this.group.GetBlocks())
			{
				TagManager.RegisterBlockTag(block, tagName);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000C18 RID: 3096 RVA: 0x00056344 File Offset: 0x00054744
		public override TileResultCode AppearBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.AppearBlock(eInfo, args);
			}
			bool animate = this.CanAnimateScale() && Util.GetIntBooleanArg(args, 0, false);
			TileResultCode result = TileResultCode.True;
			foreach (Block block in this.group.GetBlocks())
			{
				TileResultCode tileResultCode = block.AppearBlock(animate, eInfo.timer);
				if (tileResultCode == TileResultCode.Delayed)
				{
					result = tileResultCode;
				}
			}
			return result;
		}

		// Token: 0x06000C19 RID: 3097 RVA: 0x000563C4 File Offset: 0x000547C4
		public override TileResultCode VanishBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.GroupHasIndividualSripting())
			{
				return base.VanishBlock(eInfo, args);
			}
			bool animate = this.CanAnimateScale() && Util.GetIntBooleanArg(args, 0, false);
			TileResultCode result = TileResultCode.True;
			foreach (Block block in this.group.GetBlocks())
			{
				TileResultCode tileResultCode = block.VanishBlock(animate, eInfo.timer);
				if (tileResultCode == TileResultCode.Delayed)
				{
					result = tileResultCode;
				}
			}
			return result;
		}

		// Token: 0x06000C1A RID: 3098 RVA: 0x00056444 File Offset: 0x00054844
		public override void TBoxSnap()
		{
			if (this.group != null)
			{
				foreach (Block block in this.group.GetBlocks())
				{
					Vector3 position = block.GetPosition();
					Vector3 scale = block.goT.rotation * block.Scale();
					List<Vector3> list = new List<Vector3>();
					if (!TBox.OkScalePositionCombination(scale, position, list))
					{
						Vector3 pos = new Vector3(Mathf.Round(position.x * 2f) * 0.5f, Mathf.Round(position.y * 2f) * 0.5f, Mathf.Round(position.z * 2f) * 0.5f);
						if (!TBox.OkScalePositionCombination(scale, pos, null) && list.Count > 0)
						{
							pos = list[0];
						}
						block.MoveTo(pos);
					}
				}
			}
		}

		// Token: 0x04000993 RID: 2451
		public BlockGroup group;

		// Token: 0x04000994 RID: 2452
		private static Dictionary<ScriptRowExecutionInfo, List<ScriptRowExecutionInfo>> scriptInfos = new Dictionary<ScriptRowExecutionInfo, List<ScriptRowExecutionInfo>>();

		// Token: 0x04000995 RID: 2453
		private static Dictionary<string, string> vfxTranslations = new Dictionary<string, string>
		{
			{
				"Sparkle",
				"Sparkle Group"
			}
		};
	}
}
