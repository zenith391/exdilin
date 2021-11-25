using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000AE RID: 174
	public interface IMissile
	{
		// Token: 0x06000DCF RID: 3535
		float GetLifetime();

		// Token: 0x06000DD0 RID: 3536
		void SetLifetime(float newLifetime);

		// Token: 0x06000DD1 RID: 3537
		HashSet<int> GetLabels();

		// Token: 0x06000DD2 RID: 3538
		void FixedUpdate();

		// Token: 0x06000DD3 RID: 3539
		void Update();

		// Token: 0x06000DD4 RID: 3540
		bool IsBroken();

		// Token: 0x06000DD5 RID: 3541
		void Break();

		// Token: 0x06000DD6 RID: 3542
		bool CanExplode();

		// Token: 0x06000DD7 RID: 3543
		void Explode(float radius);

		// Token: 0x06000DD8 RID: 3544
		bool HasExploded();

		// Token: 0x06000DD9 RID: 3545
		bool HasExpired();

		// Token: 0x06000DDA RID: 3546
		void SetExpired();

		// Token: 0x06000DDB RID: 3547
		void Destroy();

		// Token: 0x06000DDC RID: 3548
		void Deactivate();

		// Token: 0x06000DDD RID: 3549
		bool IsBursting();

		// Token: 0x06000DDE RID: 3550
		float GetInFlightTime();
	}
}
