using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockSliceInverse : BlockProceduralCollider
{
	public BlockSliceInverse(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	protected override float Evaluate(float x)
	{
		float f = Mathf.Clamp(1f - x * x, 0f, 1f);
		return 1f - Mathf.Sqrt(f);
	}
}
