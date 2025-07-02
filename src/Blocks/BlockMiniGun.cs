using System.Collections.Generic;

namespace Blocks;

public class BlockMiniGun : BlockAbstractLaser
{
	public BlockMiniGun(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMiniGun>("MiniGun.Fire", (Block b) => ((BlockAbstractLaser)b).IsFiringProjectile, (Block b) => ((BlockAbstractLaser)b).FireProjectile);
		PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => b.IsFiredAsWeapon);
		Block.AddSimpleDefaultTiles(new GAF("MiniGun.Fire"), "Minigun");
	}

	public override bool CanFireLaser()
	{
		return false;
	}

	public override bool CanFireProjectiles()
	{
		return true;
	}
}
