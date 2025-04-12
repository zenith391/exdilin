using System;

// Token: 0x02000250 RID: 592
public class AnalyticsInterface : IAnalytics
{
	// Token: 0x06001B1B RID: 6939 RVA: 0x000C663D File Offset: 0x000C4A3D
	public virtual void SendAnalyticsEvent(string eventName)
	{
	}

	// Token: 0x06001B1C RID: 6940 RVA: 0x000C663F File Offset: 0x000C4A3F
	public virtual void SendAnalyticsEvent(string eventName, string extra)
	{
	}
}
