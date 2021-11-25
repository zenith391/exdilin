using System;
using UnityEngine;

// Token: 0x02000147 RID: 327
public class UnlockInputCommand : Command
{
	// Token: 0x06001476 RID: 5238 RVA: 0x0008FFF2 File Offset: 0x0008E3F2
	public void SetUnlockTime(float unlockTime)
	{
		this.unlockTime = Mathf.Max(unlockTime, this.unlockTime);
	}

	// Token: 0x06001477 RID: 5239 RVA: 0x00090006 File Offset: 0x0008E406
	public override void Execute()
	{
		if (Blocksworld.lockInput && Time.fixedTime > this.unlockTime)
		{
			Blocksworld.lockInput = false;
		}
	}

	// Token: 0x06001478 RID: 5240 RVA: 0x00090028 File Offset: 0x0008E428
	public override void Removed()
	{
		Blocksworld.lockInput = false;
	}

	// Token: 0x04001035 RID: 4149
	private float unlockTime;
}
