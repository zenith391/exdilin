using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockIdol : Block
{
	public BlockIdol(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		if (permanent && meshIndex == 1 && GetTexture(1) == "Plain" && Block.skinPaints.Contains(paint) && GetDefaultTexture(1) != "Plain")
		{
			TextureTo("Clothing Underwear", Vector3.forward, permanent, 1);
		}
		return base.PaintTo(paint, permanent, meshIndex);
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (permanent && meshIndex == 1 && texture == "Plain" && Block.skinPaints.Contains(GetPaint(1)) && GetDefaultTexture(1) != "Plain")
		{
			texture = "Clothing Underwear";
		}
		return base.TextureTo(texture, normal, permanent, meshIndex, force);
	}
}
