public class AnalyticsInterface : IAnalytics
{
	public virtual void SendAnalyticsEvent(string eventName)
	{
	}

	public virtual void SendAnalyticsEvent(string eventName, string extra)
	{
	}
}
