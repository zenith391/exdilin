using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x02000403 RID: 1027
[RequireComponent(typeof(Animator))]
public class ItemPanel : Selectable, PanelContentsDelegate
{
	// Token: 0x06002CE3 RID: 11491 RVA: 0x0014155D File Offset: 0x0013F95D
	public new void Awake()
	{
		this.Init();
		this.mainCanvasGroup = base.GetComponent<CanvasGroup>();
	}

	// Token: 0x06002CE4 RID: 11492 RVA: 0x00141574 File Offset: 0x0013F974
	public void Init()
	{
		this._animator = base.GetComponent<Animator>();
		this._rootTransform = (RectTransform)base.transform;
		this._noContentsImage = base.GetComponent<Image>();
		this.panelContents.Init();
		this.expandedPanelContents.Init();
		this.panelContents.panelContentsDelegate = this;
		this.expandedPanelContents.panelContentsDelegate = this;
		if (!this.hideTillDataLoad && this._animator != null)
		{
			this._animator.SetBool("DataLoaded", true);
		}
	}

	// Token: 0x06002CE5 RID: 11493 RVA: 0x00141605 File Offset: 0x0013FA05
	public void SetDisabled(bool disabled)
	{
		if (this._animator != null)
		{
			this._animator.SetBool("Disabled", disabled);
		}
	}

	// Token: 0x06002CE6 RID: 11494 RVA: 0x00141629 File Offset: 0x0013FA29
	public void DoSelect()
	{
		this.selectedByClick = false;
		this.Select();
	}

	// Token: 0x06002CE7 RID: 11495 RVA: 0x00141638 File Offset: 0x0013FA38
	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (this.mainCanvasGroup != null)
		{
			this.mainCanvasGroup.interactable = false;
			this.mainCanvasGroup.blocksRaycasts = false;
		}
		this.SetDisabled(false);
		this._animator.SetBool("Selected", true);
		if (this.itemDelegate != null)
		{
			this.itemDelegate.ItemSelected(this, this.selectedByClick);
		}
		this.selectedByClick = false;
		if (!string.IsNullOrEmpty(this.onSelectMessage))
		{
			BWStandalone.Instance.HandleMenuUIMessage(this.onSelectMessage, this.itemId, this.dataType, this.dataSubtype);
		}
	}

	// Token: 0x06002CE8 RID: 11496 RVA: 0x001416E4 File Offset: 0x0013FAE4
	public bool IsSelected()
	{
		return EventSystem.current.currentSelectedGameObject == base.gameObject;
	}

	// Token: 0x06002CE9 RID: 11497 RVA: 0x001416FB File Offset: 0x0013FAFB
	public void LoadExpandedContents()
	{
	}

	// Token: 0x06002CEA RID: 11498 RVA: 0x001416FD File Offset: 0x0013FAFD
	public void ShowExpandedContents()
	{
		this.expandedPanelContents.Show();
	}

	// Token: 0x06002CEB RID: 11499 RVA: 0x0014170A File Offset: 0x0013FB0A
	public void HideExpandedContents()
	{
		this.expandedPanelContents.Hide();
	}

	// Token: 0x06002CEC RID: 11500 RVA: 0x00141717 File Offset: 0x0013FB17
	public void Deselect()
	{
		if (this.mainCanvasGroup != null)
		{
			this.mainCanvasGroup.interactable = true;
			this.mainCanvasGroup.blocksRaycasts = true;
		}
		this._animator.SetBool("Selected", false);
	}

	// Token: 0x06002CED RID: 11501 RVA: 0x00141753 File Offset: 0x0013FB53
	public void SetDetailMode(bool state)
	{
		this._animator.SetBool("DetailMode", state);
	}

	// Token: 0x06002CEE RID: 11502 RVA: 0x00141766 File Offset: 0x0013FB66
	public override void OnPointerEnter(PointerEventData eventData)
	{
		this.selectedByClick = true;
		this._animator.SetBool(this.hoverParameterName, true);
	}

	// Token: 0x06002CEF RID: 11503 RVA: 0x00141781 File Offset: 0x0013FB81
	public override void OnPointerExit(PointerEventData eventData)
	{
		this.selectedByClick = false;
		this._animator.SetBool(this.hoverParameterName, false);
	}

	// Token: 0x06002CF0 RID: 11504 RVA: 0x0014179C File Offset: 0x0013FB9C
	public override void OnPointerDown(PointerEventData eventData)
	{
		this._animator.SetBool(this.pressParameterName, true);
	}

	// Token: 0x06002CF1 RID: 11505 RVA: 0x001417B0 File Offset: 0x0013FBB0
	public override void OnPointerUp(PointerEventData eventData)
	{
		this._animator.SetBool(this.pressParameterName, false);
	}

	// Token: 0x06002CF2 RID: 11506 RVA: 0x001417C4 File Offset: 0x0013FBC4
	public void OnLayoutComplete(UIPanelContents panelContents)
	{
		if (panelContents == this.panelContents && this._animator != null)
		{
			this._animator.SetBool("DataLoaded", true);
		}
		if (panelContents == this.expandedPanelContents && this._animator != null)
		{
			this._animator.SetBool("ExpandedDataLoaded", true);
		}
	}

	// Token: 0x06002CF3 RID: 11507 RVA: 0x00141837 File Offset: 0x0013FC37
	public void OnCloseButtonPressed(UIPanelContents panelContents)
	{
	}

	// Token: 0x04002572 RID: 9586
	public string hoverParameterName = "Hover";

	// Token: 0x04002573 RID: 9587
	public string pressParameterName = "Press";

	// Token: 0x04002574 RID: 9588
	public UIPanelContents panelContents;

	// Token: 0x04002575 RID: 9589
	public UIPanelContents expandedPanelContents;

	// Token: 0x04002576 RID: 9590
	public RectTransform contentParent;

	// Token: 0x04002577 RID: 9591
	public RectTransform expandedContentParent;

	// Token: 0x04002578 RID: 9592
	public bool preserveDuringClear;

	// Token: 0x04002579 RID: 9593
	public bool hideTillDataLoad;

	// Token: 0x0400257A RID: 9594
	public UIPanelContents overrideDetailPanelContents;

	// Token: 0x0400257B RID: 9595
	public string onSelectMessage;

	// Token: 0x0400257C RID: 9596
	public ItemPanelDelegate itemDelegate;

	// Token: 0x0400257D RID: 9597
	private bool selectedByClick;

	// Token: 0x0400257E RID: 9598
	[HideInInspector]
	public string itemId;

	// Token: 0x0400257F RID: 9599
	[HideInInspector]
	public string dataType;

	// Token: 0x04002580 RID: 9600
	[HideInInspector]
	public string dataSubtype;

	// Token: 0x04002581 RID: 9601
	private Animator _animator;

	// Token: 0x04002582 RID: 9602
	private RectTransform _rootTransform;

	// Token: 0x04002583 RID: 9603
	private Image _noContentsImage;

	// Token: 0x04002584 RID: 9604
	private CanvasGroup mainCanvasGroup;
}
