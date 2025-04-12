using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200004D RID: 77
	public class BlockAbstractAntiGravityWing : BlockAbstractAntiGravity
	{
		// Token: 0x0600065F RID: 1631 RVA: 0x0002D556 File Offset: 0x0002B956
		public BlockAbstractAntiGravityWing(List<List<Tile>> tiles) : base(tiles)
		{
			this.playLoop = false;
			this.informAboutVaryingGravity = false;
		}
	}
}
