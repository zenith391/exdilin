using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockVolumeBlock : BlockPosition
{
	private bool _destroyBlocksTouching;

	private bool _destroyChunkTouching;

	public BlockVolumeBlock(List<List<Tile>> tiles)
		: base(tiles, isVisible: true)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockVolumeBlock>("Volume.DestroyTouchingBlock", null, (Block b) => ((BlockVolumeBlock)b).DestroyTouchingBlock);
		PredicateRegistry.Add<BlockVolumeBlock>("Volume.DestroyTouchingChunk", null, (Block b) => ((BlockVolumeBlock)b).DestroyTouchingChunk);
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			Block.ThenTile(),
			new Tile(new GAF("Block.Fixed"))
		});
		list.Add(Block.EmptyTileRow());
		List<List<Tile>> value = list;
		Block.defaultExtraTiles["Volume Block"] = value;
		Block.defaultExtraTiles["Volume Block No Glue"] = value;
		Block.defaultExtraTiles["Volume Block Slab"] = value;
		Block.defaultExtraTiles["Volume Block Slab No Glue"] = value;
	}

	public override void Play()
	{
		base.Play();
		_destroyBlocksTouching = false;
		_destroyChunkTouching = false;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		_destroyBlocksTouching = false;
		_destroyChunkTouching = false;
	}

	public TileResultCode DestroyTouchingBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		_destroyBlocksTouching = true;
		return TileResultCode.True;
	}

	public TileResultCode DestroyTouchingChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		_destroyChunkTouching = true;
		return TileResultCode.True;
	}

	private bool IsBlockForRemoval(Block b)
	{
		bool result = false;
		if (b != null)
		{
			result = ((!(b is BlockGrouped blockGrouped)) ? (!Invincibility.IsInvincible(b)) : (!Invincibility.IsInvincible(blockGrouped.GetMainBlockInGroup())));
		}
		return result;
	}

	private void RemovePlayBlockFromScene(Block b)
	{
		if (b is BlockMissile blockMissile)
		{
			blockMissile.RemoveMissile();
			return;
		}
		Blocksworld.AddFixedUpdateCommand(new SplitChunkCommand(b));
		BWSceneManager.RemovePlayBlock(b);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_destroyBlocksTouching || _destroyChunkTouching)
		{
			HashSet<GameObject> triggeringBlocks = CollisionManager.GetTriggeringBlocks(this);
			if (triggeringBlocks != null)
			{
				Collider component = go.GetComponent<Collider>();
				Bounds bounds = component.bounds;
				HashSet<GameObject> hashSet = new HashSet<GameObject>();
				foreach (GameObject item in triggeringBlocks)
				{
					if (item == null)
					{
						continue;
					}
					if (_destroyChunkTouching)
					{
						Vector3 worldCenterOfMass = item.GetComponent<Rigidbody>().worldCenterOfMass;
						if (!bounds.Contains(worldCenterOfMass))
						{
							continue;
						}
						foreach (object item2 in item.transform)
						{
							Transform transform = (Transform)item2;
							Block b = BWSceneManager.FindBlock(transform.gameObject);
							if (IsBlockForRemoval(b))
							{
								RemovePlayBlockFromScene(b);
								hashSet.Add(item);
							}
						}
						continue;
					}
					foreach (object item3 in item.transform)
					{
						Transform transform2 = (Transform)item3;
						Block b2 = BWSceneManager.FindBlock(transform2.gameObject);
						if (IsBlockForRemoval(b2) && bounds.Intersects(transform2.gameObject.GetComponent<Collider>().bounds))
						{
							RemovePlayBlockFromScene(b2);
							hashSet.Add(item);
						}
					}
				}
				foreach (GameObject item4 in hashSet)
				{
					triggeringBlocks.Remove(item4);
				}
			}
		}
		_destroyBlocksTouching = false;
		_destroyChunkTouching = false;
	}
}
