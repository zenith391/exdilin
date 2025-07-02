public interface IAnalytics
{
	void SendAnalyticsEvent(string eventName);

	void SendAnalyticsEvent(string eventName, string extra);
}
