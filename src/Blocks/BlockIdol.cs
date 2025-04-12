using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000096 RID: 150
	public class BlockIdol : Block
	{
		// Token: 0x06000C30 RID: 3120 RVA: 0x00056B0C File Offset: 0x00054F0C
		public BlockIdol(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C31 RID: 3121 RVA: 0x00056B18 File Offset: 0x00054F18
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			if (permanent && meshIndex == 1 && base.GetTexture(1) == "Plain" && Block.skinPaints.Contains(paint) && base.GetDefaultTexture(1) != "Plain")
			{
				this.TextureTo("Clothing Underwear", Vector3.forward, permanent, 1, false);
			}
			return base.PaintTo(paint, permanent, meshIndex);
		}

		// Token: 0x06000C32 RID: 3122 RVA: 0x00056B8C File Offset: 0x00054F8C
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (permanent && meshIndex == 1 && texture == "Plain" && Block.skinPaints.Contains(this.GetPaint(1)) && base.GetDefaultTexture(1) != "Plain")
			{
				texture = "Clothing Underwear";
			}
			return base.TextureTo(texture, normal, permanent, meshIndex, force);
		}
	}
}
