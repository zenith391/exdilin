using UnityEngine;
using UnityEngine.EventSystems;

public class UITabBarButtonDownHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	internal TabBarTabId tabId;

	public void OnPointerDown(PointerEventData eventData)
	{
		Blocksworld.UI.TabBar.DidSelectTabBarPanel(tabId);
	}
}
