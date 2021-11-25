using System;

namespace Blocks
{
	// Token: 0x020000AD RID: 173
	public interface IMissileLauncher
	{
		// Token: 0x06000DC5 RID: 3525
		IMissile GetLaunchedMissile();

		// Token: 0x06000DC6 RID: 3526
		bool CanLaunch();

		// Token: 0x06000DC7 RID: 3527
		void LaunchMissile(float burstMultiplier);

		// Token: 0x06000DC8 RID: 3528
		bool CanReload();

		// Token: 0x06000DC9 RID: 3529
		bool IsLoaded();

		// Token: 0x06000DCA RID: 3530
		void Reload();

		// Token: 0x06000DCB RID: 3531
		bool MissileGone();

		// Token: 0x06000DCC RID: 3532
		bool HasLabel(int label);

		// Token: 0x06000DCD RID: 3533
		void AddControllerTargetTag(string tagName, float lockDelay);

		// Token: 0x06000DCE RID: 3534
		void SetGlobalBurstTime(float burstTime);
	}
}
