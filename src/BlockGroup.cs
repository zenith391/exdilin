using System.Collections.Generic;
using Blocks;

public abstract class BlockGroup
{
	protected const string TYPE_KEY = "type";

	protected const string INDICES_KEY = "indices";

	protected const string CHUNK_TYPE = "chunk";

	protected const string MODEL_TYPE = "model";

	protected const string WITHIN_TERRAIN_TYPE = "within-terrain";

	public const string TANK_TREADS_TYPE = "tank-treads";

	public const string LOCKED_MODEL_TYPE = "locked-model";

	public const string TELEPORT_VOLUME_TYPE = "teleport-volume";

	protected const string TEMPLATE_TYPE = "template";

	public static int nextGroupId = 0;

	public static HashSet<int> usedGroupIds = new HashSet<int>();

	public int groupId = -1;

	protected string groupType = string.Empty;

	protected Block[] blocks;

	protected BlockGroup(List<Block> blocks, string groupType)
	{
		this.blocks = blocks.ToArray();
		this.groupType = groupType;
		groupId = GetNextGroupId();
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			BlockGroups.SetGroup(block.tiles, groupId, groupType, i == 0);
		}
	}

	public static int GetNextGroupId()
	{
		int num = nextGroupId;
		usedGroupIds.Add(num);
		nextGroupId++;
		return num;
	}

	public Block[] GetBlocks()
	{
		return blocks;
	}

	public List<Block> GetBlockList()
	{
		return new List<Block>(blocks);
	}

	public virtual void Initialize()
	{
	}

	public static BlockGroup Create(List<Block> blocks, string type)
	{
		return type switch
		{
			"tank-treads" => new TankTreadsBlockGroup(blocks), 
			"locked-model" => new LockedModelBlockGroup(blocks), 
			"teleport-volume" => new TeleportVolumeBlockGroup(blocks), 
			_ => new TemplateBlockGroup(blocks), 
		};
	}

	public static BlockGroup Read(List<Block> blocks, string type)
	{
		return type switch
		{
			"tank-treads" => new TankTreadsBlockGroup(blocks), 
			"locked-model" => new LockedModelBlockGroup(blocks), 
			"teleport-volume" => new TeleportVolumeBlockGroup(blocks), 
			_ => null, 
		};
	}

	public string GetGroupType()
	{
		return groupType;
	}
}
