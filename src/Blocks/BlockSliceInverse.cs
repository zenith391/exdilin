using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000CE RID: 206
	public class BlockSliceInverse : BlockProceduralCollider
	{
		// Token: 0x06000F7C RID: 3964 RVA: 0x0006826D File Offset: 0x0006666D
		public BlockSliceInverse(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000F7D RID: 3965 RVA: 0x00068278 File Offset: 0x00066678
		protected override float Evaluate(float x)
		{
			float f = Mathf.Clamp(1f - x * x, 0f, 1f);
			return 1f - Mathf.Sqrt(f);
		}
	}
}
