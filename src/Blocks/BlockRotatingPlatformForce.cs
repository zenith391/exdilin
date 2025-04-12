using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000C8 RID: 200
	public class BlockRotatingPlatformForce : BlockAbstractRotatingPlatform
	{
		// Token: 0x06000F36 RID: 3894 RVA: 0x00066566 File Offset: 0x00064966
		public BlockRotatingPlatformForce(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000F37 RID: 3895 RVA: 0x00066570 File Offset: 0x00064970
		public new static void Register()
		{
			PredicateRegistry.Add<BlockRotatingPlatformForce>("RotatingPlatformForce.IncreaseAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractRotatingPlatform)b).IncreaseAngleDurational), new Type[]
			{
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockRotatingPlatformForce>("RotatingPlatformForce.ReturnAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractRotatingPlatform)b).ReturnAngleDurational), new Type[]
			{
				typeof(float),
				typeof(float)
			}, null, null);
		}
	}
}
