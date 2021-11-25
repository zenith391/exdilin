using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x0200040D RID: 1037
public class UIMenuBarButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	// Token: 0x06002D3C RID: 11580 RVA: 0x0014317C File Offset: 0x0014157C
	private void OnEnable()
	{
		animator = GetComponent<Animator>();
	}

	// Token: 0x06002D3D RID: 11581 RVA: 0x0014318A File Offset: 0x0014158A
	public void OnPointerClick(PointerEventData eventData)
	{
		Select();
	}

	// Token: 0x06002D3E RID: 11582 RVA: 0x00143192 File Offset: 0x00141592
	public void Select()
	{
		animator.SetTrigger(selectAnimatorTrigger);
		if (deselectSiblings)
		{
			DeselectSiblings();
		}
	}

	// Token: 0x06002D3F RID: 11583 RVA: 0x001431B6 File Offset: 0x001415B6
	public void Deselect()
	{
		animator.SetTrigger(deselectAnimatorTrigger);
	}

	// Token: 0x06002D40 RID: 11584 RVA: 0x001431CC File Offset: 0x001415CC
	private void DeselectSiblings()
	{
		MenuBar menuBar = GetComponentInParent<MenuBar>();
		menuBar.DeselectCustomButton();
		if (menuBar != null)
		{
			UIMenuBarButton[] componentsInChildren = menuBar.GetComponentsInChildren<UIMenuBarButton>();
			foreach (UIMenuBarButton uimenuBarButton in componentsInChildren)
			{
				if (uimenuBarButton != this)
				{
					uimenuBarButton.Deselect();
				}
			}
		}
	}

	// Token: 0x040025CE RID: 9678
	public MenuBarButtonEnum menuBarButton;

	// Token: 0x040025CF RID: 9679
	public bool deselectSiblings = true;

	// Token: 0x040025D0 RID: 9680
	public string selectAnimatorTrigger = "Select";

	// Token: 0x040025D1 RID: 9681
	public string deselectAnimatorTrigger = "Deselect";

	// Token: 0x040025D2 RID: 9682
	private Animator animator;
}
