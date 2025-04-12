using System;

namespace Blocks
{
	// Token: 0x02000071 RID: 113
	public class WaterSplashInfo
	{
		// Token: 0x0600091D RID: 2333 RVA: 0x0003F781 File Offset: 0x0003DB81
		public WaterSplashInfo(Block block)
		{
			this.block = block;
		}

		// Token: 0x0600091E RID: 2334 RVA: 0x0003F790 File Offset: 0x0003DB90
		public void Update()
		{
			this.counter++;
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x0003F7A0 File Offset: 0x0003DBA0
		public void EnterWater()
		{
			this.counter = 0;
			this.forceSum = 0f;
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x0003F7B4 File Offset: 0x0003DBB4
		public void LeaveWater()
		{
		}

		// Token: 0x0400072A RID: 1834
		public Block block;

		// Token: 0x0400072B RID: 1835
		public float forceSum;

		// Token: 0x0400072C RID: 1836
		public int counter;
	}
}
