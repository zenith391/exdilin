using System;

// Token: 0x0200024F RID: 591
public interface IAnalytics
{
	// Token: 0x06001B18 RID: 6936
	void SendAnalyticsEvent(string eventName);

	// Token: 0x06001B19 RID: 6937
	void SendAnalyticsEvent(string eventName, string extra);
}
