using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockCrossbow : BlockAbstractBow
{
	private const float _minTimeBetweenShots = 0.15f;

	private readonly Vector3 _ammoPositionOffset = new Vector3(-0.793f, 0.196f, -0.3343f);

	private readonly string _ammoPrefabName = "Blocks/Prefab RAR Cross Bow Bolt";

	private readonly int[] _ammoSubmeshColorMapping = new int[3] { 1, 0, 2 };

	private readonly string _blockedSFX = "Arrow Blocked";

	private readonly string _hitSFX = "Arrow Hit";

	private readonly string _loadSFX = "Arrow Nock";

	private readonly string _shootSFX = "Arrow Loose";

	public BlockCrossbow(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	protected override float GetMinTimeBetweenShots()
	{
		return 0.15f;
	}

	protected override Vector3 GetAmmoPositionOffset()
	{
		return _ammoPositionOffset;
	}

	protected override string GetAmmoPrefabName()
	{
		return _ammoPrefabName;
	}

	protected override int GetAmmoSubmeshColorMapping(int i)
	{
		return _ammoSubmeshColorMapping[i];
	}

	protected override string GetSFXForBlocked()
	{
		return _blockedSFX;
	}

	protected override string GetSFXForHit()
	{
		return _hitSFX;
	}

	protected override string GetSFXForLoad()
	{
		return _loadSFX;
	}

	protected override string GetSFXForShoot()
	{
		return _shootSFX;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockCrossbow>("Crossbow.Triggered", (Block b) => b.IsFiredAsWeapon);
		Predicate pred = PredicateRegistry.Add<BlockCrossbow>("Crossbow.Load", (Block b) => ((BlockAbstractBow)b).IsLoaded, (Block b) => ((BlockAbstractBow)b).Load);
		PredicateRegistry.Add<BlockCrossbow>("Crossbow.Shoot", null, (Block b) => ((BlockAbstractBow)b).Shoot, new Type[1] { typeof(float) }, new string[1] { "Force" });
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateFirstFrame),
			Block.ThenTile(),
			new Tile(pred)
		});
		list.Add(Block.EmptyTileRow());
		List<List<Tile>> value = list;
		Block.defaultExtraTiles["RAR Cross Bow"] = value;
	}
}
