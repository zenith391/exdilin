using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x02000461 RID: 1121
public class UITab : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	// Token: 0x06002F68 RID: 12136 RVA: 0x0014F536 File Offset: 0x0014D936
	private void OnEnable()
	{
		this.animator = base.GetComponent<Animator>();
		this.tabGroup = base.GetComponentInParent<UITabGroup>();
		if (this.tabGroup == null)
		{
			BWLog.Error("UITab outside of UITabGroup");
			base.enabled = false;
		}
	}

	// Token: 0x06002F69 RID: 12137 RVA: 0x0014F572 File Offset: 0x0014D972
	public void OnPointerClick(PointerEventData eventData)
	{
		if (this.selectOnClick)
		{
			this.tabGroup.SelectTab(this);
		}
	}

	// Token: 0x06002F6A RID: 12138 RVA: 0x0014F58B File Offset: 0x0014D98B
	public void Select(bool select)
	{
		if (select)
		{
			this.Select();
		}
		else
		{
			this.Deselect();
		}
	}

	// Token: 0x06002F6B RID: 12139 RVA: 0x0014F5A4 File Offset: 0x0014D9A4
	public void Select()
	{
		this.animator.SetTrigger(this.selectAnimatorTrigger);
	}

	// Token: 0x06002F6C RID: 12140 RVA: 0x0014F5B7 File Offset: 0x0014D9B7
	public void Deselect()
	{
		this.animator.SetTrigger(this.deselectAnimatorTrigger);
	}

	// Token: 0x040027C1 RID: 10177
	public new string tag;

	// Token: 0x040027C2 RID: 10178
	public bool selectOnClick;

	// Token: 0x040027C3 RID: 10179
	public string selectAnimatorTrigger = "Select";

	// Token: 0x040027C4 RID: 10180
	public string deselectAnimatorTrigger = "Deselect";

	// Token: 0x040027C5 RID: 10181
	private Animator animator;

	// Token: 0x040027C6 RID: 10182
	private UITabGroup tabGroup;
}
