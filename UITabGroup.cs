using System;
using UnityEngine;

// Token: 0x02000462 RID: 1122
public class UITabGroup : MonoBehaviour
{
	// Token: 0x06002F6E RID: 12142 RVA: 0x0014F5D4 File Offset: 0x0014D9D4
	public void SelectTab(UITab tabToSelect)
	{
		foreach (UITab uitab in this.Tabs)
		{
			uitab.Select(uitab == tabToSelect);
		}
	}

	// Token: 0x06002F6F RID: 12143 RVA: 0x0014F610 File Offset: 0x0014DA10
	public void SelectTabWithTag(string tag)
	{
		foreach (UITab uitab in this.Tabs)
		{
			uitab.Select(uitab.tag == tag);
		}
	}

	// Token: 0x1700024F RID: 591
	// (get) Token: 0x06002F70 RID: 12144 RVA: 0x0014F64E File Offset: 0x0014DA4E
	private UITab[] Tabs
	{
		get
		{
			return base.GetComponentsInChildren<UITab>();
		}
	}
}
