using System;
using Blocks;
using UnityEngine;

// Token: 0x020002B2 RID: 690
public class SteeringWheelDebugger : MonoBehaviour
{
	// Token: 0x06001FD3 RID: 8147 RVA: 0x000E5164 File Offset: 0x000E3564
	public void Update()
	{
		if (this.lastAwdBalanceFront != this.awdBalanceFront || this.lastAwdBalanceRear != this.awdBalanceRear || this.applyChanges)
		{
			if (this.lastAwdBalanceFront != this.awdBalanceFront || this.applyChanges)
			{
				BlockSteeringWheel.awdBalanceFront = this.awdBalanceFront;
			}
			if (this.lastAwdBalanceRear != this.awdBalanceRear || this.applyChanges)
			{
				BlockSteeringWheel.awdBalanceRear = this.awdBalanceRear;
			}
			if (this.applyChanges)
			{
				this.applyChanges = false;
				Blocksworld.UI.Overlay.ShowTimedOnScreenMessage("Steering wheel changes applied.", 3f);
			}
			this.lastAwdBalanceFront = this.awdBalanceFront;
			this.lastAwdBalanceRear = this.awdBalanceRear;
		}
	}

	// Token: 0x040019FE RID: 6654
	public float awdBalanceFront = 0.4f;

	// Token: 0x040019FF RID: 6655
	public float awdBalanceRear = 0.6f;

	// Token: 0x04001A00 RID: 6656
	public bool applyChanges;

	// Token: 0x04001A01 RID: 6657
	private float lastAwdBalanceFront = 0.4f;

	// Token: 0x04001A02 RID: 6658
	private float lastAwdBalanceRear = 0.6f;
}
