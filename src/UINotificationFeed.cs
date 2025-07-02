using UnityEngine;

public class UINotificationFeed : MonoBehaviour
{
	public UINotificationFeedItem itemTemplate;

	private RectTransform parentTranform;

	private void OnEnable()
	{
		itemTemplate.gameObject.SetActive(value: false);
		parentTranform = (RectTransform)base.transform;
	}

	public void ShowNotification(string notificationStr)
	{
		GameObject gameObject = Object.Instantiate(itemTemplate.gameObject);
		UINotificationFeedItem component = gameObject.GetComponent<UINotificationFeedItem>();
		component.SetNotificationText(notificationStr);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(parentTranform, worldPositionStays: false);
		rectTransform.SetAsFirstSibling();
		gameObject.SetActive(value: true);
	}
}
