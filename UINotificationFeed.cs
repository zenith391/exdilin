using System;
using UnityEngine;

// Token: 0x0200044B RID: 1099
public class UINotificationFeed : MonoBehaviour
{
	// Token: 0x06002EE9 RID: 12009 RVA: 0x0014D219 File Offset: 0x0014B619
	private void OnEnable()
	{
		this.itemTemplate.gameObject.SetActive(false);
		this.parentTranform = (RectTransform)base.transform;
	}

	// Token: 0x06002EEA RID: 12010 RVA: 0x0014D240 File Offset: 0x0014B640
	public void ShowNotification(string notificationStr)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.itemTemplate.gameObject);
		UINotificationFeedItem component = gameObject.GetComponent<UINotificationFeedItem>();
		component.SetNotificationText(notificationStr);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(this.parentTranform, false);
		rectTransform.SetAsFirstSibling();
		gameObject.SetActive(true);
	}

	// Token: 0x04002745 RID: 10053
	public UINotificationFeedItem itemTemplate;

	// Token: 0x04002746 RID: 10054
	private RectTransform parentTranform;
}
