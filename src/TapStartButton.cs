using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TapStartButton : Selectable, IPointerDownHandler, IEventSystemHandler
{
	public UnityAction tapAction;

	public new void OnPointerDown(PointerEventData eventData)
	{
		if (base.interactable && tapAction != null)
		{
			tapAction();
		}
	}
}
