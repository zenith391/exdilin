using UnityEngine.UI;

public interface ItemPanelListDelegate
{
	void ItemInListSelected(ItemPanelList list, int index, bool selectedByClick);

	Selectable FindUpSelectable(ItemPanelList list, int index);

	Selectable FindDownSelectable(ItemPanelList list, int index);
}
