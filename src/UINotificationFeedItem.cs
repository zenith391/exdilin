using UnityEngine;
using UnityEngine.UI;

public class UINotificationFeedItem : MonoBehaviour
{
	public Text mainText;

	public float lifetime = 3f;

	public void SetNotificationText(string notificationStr)
	{
		mainText.text = notificationStr;
		Object.Destroy(base.gameObject, lifetime);
	}
}
