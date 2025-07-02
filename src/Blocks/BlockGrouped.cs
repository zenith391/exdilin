using System.Collections.Generic;
using Gestures;
using UnityEngine;

namespace Blocks;

public class BlockGrouped : Block
{
	public BlockGroup group;

	private static Dictionary<ScriptRowExecutionInfo, List<ScriptRowExecutionInfo>> scriptInfos = new Dictionary<ScriptRowExecutionInfo, List<ScriptRowExecutionInfo>>();

	private static Dictionary<string, string> vfxTranslations = new Dictionary<string, string> { { "Sparkle", "Sparkle Group" } };

	public BlockGrouped(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public override void SetBlockGroup(BlockGroup group)
	{
		base.SetBlockGroup(group);
		if (group is TankTreadsBlockGroup)
		{
			this.group = group;
		}
	}

	public virtual bool BlockUsesDefaultPaintsAndTextures()
	{
		return true;
	}

	public virtual bool GroupHasIndividualSripting()
	{
		return false;
	}

	public virtual bool GroupRotateMainBlockOnPlacement()
	{
		return true;
	}

	public bool IsMainBlockInGroup()
	{
		if (group != null)
		{
			return group.GetBlocks()[0] == this;
		}
		return false;
	}

	public Block GetMainBlockInGroup()
	{
		if (group == null)
		{
			return this;
		}
		return group.GetBlocks()[0];
	}

	public override void Destroy()
	{
		if (IsMainBlockInGroup())
		{
			foreach (ScriptRowExecutionInfo item in new List<ScriptRowExecutionInfo>(scriptInfos.Keys))
			{
				if (item.block == this)
				{
					scriptInfos.Remove(item);
				}
			}
		}
		base.Destroy();
	}

	public override TileResultCode IsWithinWater(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.IsWithinWater(eInfo, args);
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (BlockWater.BlockWithinWater(block))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public override TileResultCode WithinTaggedWater(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.WithinTaggedWater(eInfo, args);
		}
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (BlockWater.BlockWithinTaggedWater(block, stringArg))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public override TileResultCode StopScriptsBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.StopScriptsBlock(eInfo, args);
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (Block.vanishingOrAppearingBlocks.Contains(block))
			{
				return TileResultCode.Delayed;
			}
			BWSceneManager.RemoveScriptBlock(block);
		}
		return TileResultCode.True;
	}

	public override TileResultCode Mute(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.Mute(eInfo, args);
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			block.Mute();
		}
		Blocksworld.AddFixedUpdateUniqueCommand(Block.unmuteCommand);
		return TileResultCode.True;
	}

	public override TileResultCode PlayVfxDurational(string vfxName, float lengthMult, float timer, string colorName)
	{
		if (vfxTranslations.TryGetValue(vfxName, out var value))
		{
			vfxName = value;
		}
		return base.PlayVfxDurational(vfxName, lengthMult, timer, colorName);
	}

	public override TileResultCode IsBumping(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.IsBumping(eInfo, args);
		}
		string target = (string)args[0];
		Block[] blocks = group.GetBlocks();
		foreach (Block b in blocks)
		{
			if (CollisionManager.IsBumpingBlock(b, target))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public override TileResultCode IsBumpingTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.IsBumpingTag(eInfo, args);
		}
		string tag = (string)args[0];
		Block[] blocks = group.GetBlocks();
		foreach (Block b in blocks)
		{
			if (CollisionManager.BlockIsBumpingTag(b, tag))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public override TileResultCode IsHitByProjectile(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.IsHitByProjectile(eInfo, args);
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (BlockAbstractLaser.IsHitByProjectile(block))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public override TileResultCode IsHitByTaggedProjectile(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.IsHitByTaggedProjectile(eInfo, args);
		}
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (BlockAbstractLaser.IsHitByTaggedProjectile(block, stringArg))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public override TileResultCode IsHitByPulseOrBeam(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.IsHitByPulseOrBeam(eInfo, args);
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (BlockAbstractLaser.IsHitByPulseOrBeam(block))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public override TileResultCode IsHitByTaggedPulseOrBeam(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.IsHitByTaggedPulseOrBeam(eInfo, args);
		}
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (BlockAbstractLaser.IsHitByTaggedPulseOrBeam(block, stringArg))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public override TileResultCode PullLockBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.PullLockBlock(eInfo, args);
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block b in blocks)
		{
			PullObjectGesture.PullLock(b);
		}
		return TileResultCode.True;
	}

	public override TileResultCode Unfreeze(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.Unfreeze(eInfo, args);
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			block.Unfreeze();
		}
		return TileResultCode.True;
	}

	public override TileResultCode Freeze(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.Freeze(eInfo, args);
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			block.Freeze(informModelBlocks: true);
		}
		return TileResultCode.True;
	}

	public override TileResultCode RegisterTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.RegisterTag(eInfo, args);
		}
		string tagName = (string)args[0];
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			TagManager.RegisterBlockTag(block, tagName);
		}
		return TileResultCode.True;
	}

	public override TileResultCode AppearBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.AppearBlock(eInfo, args);
		}
		bool animate = CanAnimateScale() && Util.GetIntBooleanArg(args, 0, defaultValue: false);
		TileResultCode result = TileResultCode.True;
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			TileResultCode tileResultCode = block.AppearBlock(animate, eInfo.timer);
			if (tileResultCode == TileResultCode.Delayed)
			{
				result = tileResultCode;
			}
		}
		return result;
	}

	public override TileResultCode VanishBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GroupHasIndividualSripting())
		{
			return base.VanishBlock(eInfo, args);
		}
		bool animate = CanAnimateScale() && Util.GetIntBooleanArg(args, 0, defaultValue: false);
		TileResultCode result = TileResultCode.True;
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			TileResultCode tileResultCode = block.VanishBlock(animate, eInfo.timer);
			if (tileResultCode == TileResultCode.Delayed)
			{
				result = tileResultCode;
			}
		}
		return result;
	}

	public override void TBoxSnap()
	{
		if (group == null)
		{
			return;
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			Vector3 position = block.GetPosition();
			Vector3 scale = block.goT.rotation * block.Scale();
			List<Vector3> list = new List<Vector3>();
			if (!TBox.OkScalePositionCombination(scale, position, list))
			{
				Vector3 pos = new Vector3(Mathf.Round(position.x * 2f) * 0.5f, Mathf.Round(position.y * 2f) * 0.5f, Mathf.Round(position.z * 2f) * 0.5f);
				if (!TBox.OkScalePositionCombination(scale, pos) && list.Count > 0)
				{
					pos = list[0];
				}
				block.MoveTo(pos);
			}
		}
	}
}
