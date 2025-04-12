using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020003FB RID: 1019
public class Deselectable : Selectable
{
	// Token: 0x06002CB0 RID: 11440 RVA: 0x0014065A File Offset: 0x0013EA5A
	public override void OnPointerEnter(PointerEventData eventData)
	{
		this.pointerOver = true;
	}

	// Token: 0x06002CB1 RID: 11441 RVA: 0x00140663 File Offset: 0x0013EA63
	public override void OnPointerExit(PointerEventData eventData)
	{
		this.pointerOver = false;
	}

	// Token: 0x06002CB2 RID: 11442 RVA: 0x0014066C File Offset: 0x0013EA6C
	public void Deselect()
	{
		if (!this.pointerOver && this.animator != null)
		{
			this.animator.SetTrigger(this.animatorDeselectTrigger);
		}
	}

	// Token: 0x06002CB3 RID: 11443 RVA: 0x0014069B File Offset: 0x0013EA9B
	public override void OnDeselect(BaseEventData eventData)
	{
		this.Deselect();
	}

	// Token: 0x04002550 RID: 9552
	public new Animator animator;

	// Token: 0x04002551 RID: 9553
	public string animatorDeselectTrigger = "Deselected";

	// Token: 0x04002552 RID: 9554
	private bool pointerOver;
}
