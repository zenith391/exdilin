using System;
using UnityEngine.UI;

// Token: 0x02000404 RID: 1028
public interface ItemPanelListDelegate
{
	// Token: 0x06002CF4 RID: 11508
	void ItemInListSelected(ItemPanelList list, int index, bool selectedByClick);

	// Token: 0x06002CF5 RID: 11509
	Selectable FindUpSelectable(ItemPanelList list, int index);

	// Token: 0x06002CF6 RID: 11510
	Selectable FindDownSelectable(ItemPanelList list, int index);
}
