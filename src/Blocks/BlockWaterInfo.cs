using System;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000EE RID: 238
	public class BlockWaterInfo
	{
		// Token: 0x060011B2 RID: 4530 RVA: 0x000791D0 File Offset: 0x000775D0
		public BlockWaterInfo(Block b)
		{
			this.block = b;
			this.mass = b.GetMass();
			this.scale = b.Scale();
			this.counterOffset = UnityEngine.Random.Range(0, 10000);
			this.maxExtent = b.CalculateMaxExtent();
		}

		// Token: 0x04000DF2 RID: 3570
		public Block block;

		// Token: 0x04000DF3 RID: 3571
		public float mass;

		// Token: 0x04000DF4 RID: 3572
		public Vector3 scale;

		// Token: 0x04000DF5 RID: 3573
		public int interval;

		// Token: 0x04000DF6 RID: 3574
		public int checkCount;

		// Token: 0x04000DF7 RID: 3575
		public int counterOffset;

		// Token: 0x04000DF8 RID: 3576
		public bool isSimulating = true;

		// Token: 0x04000DF9 RID: 3577
		public bool hasWaterSensor;

		// Token: 0x04000DFA RID: 3578
		public float maxExtent = 1f;
	}
}
