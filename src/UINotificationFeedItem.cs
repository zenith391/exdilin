using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200044C RID: 1100
public class UINotificationFeedItem : MonoBehaviour
{
	// Token: 0x06002EEC RID: 12012 RVA: 0x0014D2A5 File Offset: 0x0014B6A5
	public void SetNotificationText(string notificationStr)
	{
		this.mainText.text = notificationStr;
		UnityEngine.Object.Destroy(base.gameObject, this.lifetime);
	}

	// Token: 0x04002747 RID: 10055
	public Text mainText;

	// Token: 0x04002748 RID: 10056
	public float lifetime = 3f;
}
