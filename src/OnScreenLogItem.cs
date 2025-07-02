using UnityEngine;

internal class OnScreenLogItem
{
	public float startTime;

	public float duration = 5f;

	public string text = string.Empty;

	public OnScreenLogItem(string text, float duration)
	{
		this.text = text;
		this.duration = duration;
		startTime = Time.time;
	}
}
