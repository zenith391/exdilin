using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020002F6 RID: 758
public class TapStartButton : Selectable, IPointerDownHandler, IEventSystemHandler
{
	// Token: 0x06002242 RID: 8770 RVA: 0x000FF76C File Offset: 0x000FDB6C
	public new void OnPointerDown(PointerEventData eventData)
	{
		if (base.interactable && this.tapAction != null)
		{
			this.tapAction();
		}
	}

	// Token: 0x04001D39 RID: 7481
	public UnityAction tapAction;
}
