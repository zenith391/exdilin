using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000D5 RID: 213
	public class BlockStatue : Block
	{
		// Token: 0x06000FC8 RID: 4040 RVA: 0x00069B15 File Offset: 0x00067F15
		public BlockStatue(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000FC9 RID: 4041 RVA: 0x00069B20 File Offset: 0x00067F20
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			if (permanent && meshIndex == 1 && base.GetTexture(1) == "Plain" && Block.skinPaints.Contains(paint) && base.GetDefaultTexture(1) != "Plain")
			{
				this.TextureTo("Clothing Underwear", Vector3.forward, permanent, 1, false);
			}
			return base.PaintTo(paint, permanent, meshIndex);
		}

		// Token: 0x06000FCA RID: 4042 RVA: 0x00069B94 File Offset: 0x00067F94
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
