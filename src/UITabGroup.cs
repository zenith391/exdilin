using UnityEngine;

public class UITabGroup : MonoBehaviour
{
	private UITab[] Tabs => GetComponentsInChildren<UITab>();

	public void SelectTab(UITab tabToSelect)
	{
		UITab[] tabs = Tabs;
		foreach (UITab uITab in tabs)
		{
			uITab.Select(uITab == tabToSelect);
		}
	}

	public void SelectTabWithTag(string tag)
	{
		UITab[] tabs = Tabs;
		foreach (UITab uITab in tabs)
		{
			uITab.Select(uITab.tag == tag);
		}
	}
}
