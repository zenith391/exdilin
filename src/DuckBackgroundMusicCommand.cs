using System;
using UnityEngine;

// Token: 0x0200013B RID: 315
public class DuckBackgroundMusicCommand : Command
{
	// Token: 0x06001445 RID: 5189 RVA: 0x0008E735 File Offset: 0x0008CB35
	public void DuckMusicVolume(float duration)
	{
		this.unDuckTime = Mathf.Max(Time.fixedTime + duration, this.unDuckTime);
		this.targetVolumeMultiplier = 0f;
	}

	// Token: 0x06001446 RID: 5190 RVA: 0x0008E75C File Offset: 0x0008CB5C
	public override void Execute()
	{
		if (this.unDuckTime < 0f)
		{
			this.done = true;
			return;
		}
		float fixedTime = Time.fixedTime;
		if (fixedTime > this.unDuckTime)
		{
			this.targetVolumeMultiplier = 1f;
		}
		this.currentVolumeMultiplier = (1f - this.alpha) * this.currentVolumeMultiplier + this.alpha * this.targetVolumeMultiplier;
		if (Mathf.Abs(this.currentVolumeMultiplier - this.targetVolumeMultiplier) < 0.01f)
		{
			this.currentVolumeMultiplier = this.targetVolumeMultiplier;
			if (fixedTime > this.unDuckTime)
			{
				this.done = true;
			}
		}
		this.SetMultiplier(this.currentVolumeMultiplier);
	}

	// Token: 0x06001447 RID: 5191 RVA: 0x0008E80C File Offset: 0x0008CC0C
	public override void Removed()
	{
		this.SetMultiplier(1f);
	}

	// Token: 0x06001448 RID: 5192 RVA: 0x0008E819 File Offset: 0x0008CC19
	private void SetMultiplier(float m)
	{
		Blocksworld.SetBackgroundMusicVolumeMultiplier(m);
	}

	// Token: 0x04000FFF RID: 4095
	private float currentVolumeMultiplier = 1f;

	// Token: 0x04001000 RID: 4096
	private float targetVolumeMultiplier = 1f;

	// Token: 0x04001001 RID: 4097
	private float unDuckTime = -1f;

	// Token: 0x04001002 RID: 4098
	private readonly float alpha = 0.2f;
}
