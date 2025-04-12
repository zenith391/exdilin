using System;
using UnityEngine;

// Token: 0x0200025E RID: 606
internal class OnScreenLogItem
{
	// Token: 0x06001B5E RID: 7006 RVA: 0x000C6D11 File Offset: 0x000C5111
	public OnScreenLogItem(string text, float duration)
	{
		this.text = text;
		this.duration = duration;
		this.startTime = Time.time;
	}

	// Token: 0x04001737 RID: 5943
	public float startTime;

	// Token: 0x04001738 RID: 5944
	public float duration = 5f;

	// Token: 0x04001739 RID: 5945
	public string text = string.Empty;
}
