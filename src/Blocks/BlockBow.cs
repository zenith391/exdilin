using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockBow : BlockAbstractBow
{
	private const float _minTimeBetweenShots = 0.33f;

	private readonly Vector3 _ammoPositionOffset = new Vector3(-0.555f, 0.018f, -0.332f);

	private readonly string _ammoPrefabName = "Blocks/Prefab SAM Arrow";

	private readonly int[] _ammoSubmeshColorMapping = new int[3] { 0, 1, 2 };

	private readonly string _blockedSFX = "Arrow Blocked";

	private readonly string _hitSFX = "Arrow Hit";

	private readonly string _loadSFX = "Arrow Nock";

	private readonly string _shootSFX = "Arrow Loose";

	public BlockBow(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	protected override float GetMinTimeBetweenShots()
	{
		return 0.33f;
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
		PredicateRegistry.Add<BlockBow>("Bow.Loosed", (Block b) => b.IsFiredAsWeapon);
		PredicateRegistry.Add<BlockBow>("Bow.Nock", (Block b) => ((BlockAbstractBow)b).IsLoaded, (Block b) => ((BlockAbstractBow)b).Load);
		PredicateRegistry.Add<BlockBow>("Bow.Lose", null, (Block b) => ((BlockAbstractBow)b).Shoot, new Type[1] { typeof(float) }, new string[1] { "Force" });
	}
}
