using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x0200031C RID: 796
public class UITabBarButtonDownHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	// Token: 0x0600240E RID: 9230 RVA: 0x00108F5A File Offset: 0x0010735A
	public void OnPointerDown(PointerEventData eventData)
	{
		Blocksworld.UI.TabBar.DidSelectTabBarPanel(this.tabId);
	}

	// Token: 0x04001F13 RID: 7955
	internal TabBarTabId tabId;
}
