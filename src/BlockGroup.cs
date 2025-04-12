using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x02000044 RID: 68
public abstract class BlockGroup
{
	// Token: 0x06000244 RID: 580 RVA: 0x0000D398 File Offset: 0x0000B798
	protected BlockGroup(List<Block> blocks, string groupType)
	{
		this.blocks = blocks.ToArray();
		this.groupType = groupType;
		this.groupId = BlockGroup.GetNextGroupId();
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			BlockGroups.SetGroup(block.tiles, this.groupId, groupType, i == 0);
		}
	}

	// Token: 0x06000245 RID: 581 RVA: 0x0000D410 File Offset: 0x0000B810
	public static int GetNextGroupId()
	{
		int num = BlockGroup.nextGroupId;
		BlockGroup.usedGroupIds.Add(num);
		BlockGroup.nextGroupId++;
		return num;
	}

	// Token: 0x06000246 RID: 582 RVA: 0x0000D43C File Offset: 0x0000B83C
	public Block[] GetBlocks()
	{
		return this.blocks;
	}

	// Token: 0x06000247 RID: 583 RVA: 0x0000D444 File Offset: 0x0000B844
	public List<Block> GetBlockList()
	{
		return new List<Block>(this.blocks);
	}

	// Token: 0x06000248 RID: 584 RVA: 0x0000D451 File Offset: 0x0000B851
	public virtual void Initialize()
	{
	}

	// Token: 0x06000249 RID: 585 RVA: 0x0000D454 File Offset: 0x0000B854
	public static BlockGroup Create(List<Block> blocks, string type)
	{
		if (type != null)
		{
			if (type == "tank-treads")
			{
				return new TankTreadsBlockGroup(blocks);
			}
			if (type == "locked-model")
			{
				return new LockedModelBlockGroup(blocks);
			}
			if (type == "teleport-volume")
			{
				return new TeleportVolumeBlockGroup(blocks);
			}
		}
		return new TemplateBlockGroup(blocks);
	}

	// Token: 0x0600024A RID: 586 RVA: 0x0000D4B8 File Offset: 0x0000B8B8
	public static BlockGroup Read(List<Block> blocks, string type)
	{
		if (type != null)
		{
			if (type == "tank-treads")
			{
				return new TankTreadsBlockGroup(blocks);
			}
			if (type == "locked-model")
			{
				return new LockedModelBlockGroup(blocks);
			}
			if (type == "teleport-volume")
			{
				return new TeleportVolumeBlockGroup(blocks);
			}
		}
		return null;
	}

	// Token: 0x0600024B RID: 587 RVA: 0x0000D516 File Offset: 0x0000B916
	public string GetGroupType()
	{
		return this.groupType;
	}

	// Token: 0x04000211 RID: 529
	protected const string TYPE_KEY = "type";

	// Token: 0x04000212 RID: 530
	protected const string INDICES_KEY = "indices";

	// Token: 0x04000213 RID: 531
	protected const string CHUNK_TYPE = "chunk";

	// Token: 0x04000214 RID: 532
	protected const string MODEL_TYPE = "model";

	// Token: 0x04000215 RID: 533
	protected const string WITHIN_TERRAIN_TYPE = "within-terrain";

	// Token: 0x04000216 RID: 534
	public const string TANK_TREADS_TYPE = "tank-treads";

	// Token: 0x04000217 RID: 535
	public const string LOCKED_MODEL_TYPE = "locked-model";

	// Token: 0x04000218 RID: 536
	public const string TELEPORT_VOLUME_TYPE = "teleport-volume";

	// Token: 0x04000219 RID: 537
	protected const string TEMPLATE_TYPE = "template";

	// Token: 0x0400021A RID: 538
	public static int nextGroupId = 0;

	// Token: 0x0400021B RID: 539
	public static HashSet<int> usedGroupIds = new HashSet<int>();

	// Token: 0x0400021C RID: 540
	public int groupId = -1;

	// Token: 0x0400021D RID: 541
	protected string groupType = string.Empty;

	// Token: 0x0400021E RID: 542
	protected Block[] blocks;
}
