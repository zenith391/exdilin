using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020003FD RID: 1021
public class DetailPanel : MonoBehaviour, PanelContentsDelegate, IMenuInputHandler
{
	// Token: 0x17000243 RID: 579
	// (get) Token: 0x06002CB6 RID: 11446 RVA: 0x001406AB File Offset: 0x0013EAAB
	// (set) Token: 0x06002CB7 RID: 11447 RVA: 0x001406B3 File Offset: 0x0013EAB3
	public string contentId { get; private set; }

	// Token: 0x06002CB8 RID: 11448 RVA: 0x001406BC File Offset: 0x0013EABC
	public void Init()
	{
		base.gameObject.SetActive(true);
		this.rootTransform = (RectTransform)base.transform;
		this.animator = base.GetComponent<Animator>();
		this.panelContents.Init();
		this.panelContents.panelContentsDelegate = this;
	}

	// Token: 0x06002CB9 RID: 11449 RVA: 0x0014070C File Offset: 0x0013EB0C
	public void LoadContentForID(string id, UIDataSource dataSource, ImageManager imageManager)
	{
		this.dataSource = dataSource;
		this.contentId = id;
		if (this.overrideContents != null)
		{
			this.overrideContents.SetupPanel(dataSource, imageManager, this.contentId);
		}
		else
		{
			this.panelContents.SetupPanel(dataSource, imageManager, this.contentId);
		}
	}

	// Token: 0x06002CBA RID: 11450 RVA: 0x00140763 File Offset: 0x0013EB63
	public void LinkToItemPanel(ItemPanel itemPanel)
	{
		this.itemPanel = itemPanel;
	}

	// Token: 0x06002CBB RID: 11451 RVA: 0x0014076C File Offset: 0x0013EB6C
	public void Show(bool immediate)
	{
		if (this.showing)
		{
			return;
		}
		this.showing = true;
		string trigger = (!immediate) ? "Show" : "ShowImmediate";
		this.animator.SetTrigger(trigger);
		this.ResetAnimation();
		this.panelContents.Show();
		DetailPanel.activeDetailPanel = this;
		MenuInputHandler.RequestControl(this);
		if (this.tabSelectionCycle != null && this.tabSelectionCycle.Count > 0)
		{
			this.currentTabSelection = this.tabSelectionCycle[0];
			this.SetupSelectionCycle();
		}
	}

	// Token: 0x06002CBC RID: 11452 RVA: 0x00140800 File Offset: 0x0013EC00
	public void Hide(bool immediate)
	{
		if (!this.showing)
		{
			return;
		}
		this.dataSource.BackupData(this.contentId);
		this.showing = false;
		MenuInputHandler.Release(this);
		string trigger = (!immediate) ? "Hide" : "HideImmediate";
		this.animator.SetTrigger(trigger);
		this.ResetAnimation();
		if (this.overrideContents != null)
		{
			this.overrideContents.Hide();
		}
		if (this.panelContents != null)
		{
			this.panelContents.Hide();
		}
		if (this.detailPanelDelegate != null)
		{
			this.detailPanelDelegate.DetailPanelClosed(this);
		}
		UIScrollControl componentInParent = base.GetComponentInParent<UIScrollControl>();
		if (componentInParent != null)
		{
			if (!immediate && this == DetailPanel.activeDetailPanel && this.itemPanel != null)
			{
				componentInParent.ScrollToTransform(this.rootTransform, 0.2f, 0.75f, 0.1f);
			}
			else
			{
				componentInParent.ClampScroll();
			}
		}
		if (DetailPanel.activeDetailPanel == this)
		{
			DetailPanel.activeDetailPanel = null;
		}
	}

	// Token: 0x06002CBD RID: 11453 RVA: 0x00140924 File Offset: 0x0013ED24
	public bool IsShowing()
	{
		return this.showing;
	}

	// Token: 0x06002CBE RID: 11454 RVA: 0x0014092C File Offset: 0x0013ED2C
	private void OnDestroy()
	{
		MenuInputHandler.Release(this);
		if (this == DetailPanel.activeDetailPanel)
		{
			DetailPanel.activeDetailPanel = null;
		}
	}

	// Token: 0x06002CBF RID: 11455 RVA: 0x0014094C File Offset: 0x0013ED4C
	public void TriggerAnimation(string animKey, string animTrigger)
	{
		foreach (UIPanelElementAnimator uipanelElementAnimator in base.GetComponentsInChildren<UIPanelElementAnimator>())
		{
			if (uipanelElementAnimator.animKey == animKey)
			{
				uipanelElementAnimator.TriggerAnimation(animTrigger);
			}
		}
	}

	// Token: 0x06002CC0 RID: 11456 RVA: 0x00140990 File Offset: 0x0013ED90
	public void ResetAnimation()
	{
		foreach (UIPanelElementAnimator uipanelElementAnimator in base.GetComponentsInChildren<UIPanelElementAnimator>())
		{
			uipanelElementAnimator.Reset();
		}
	}

	// Token: 0x06002CC1 RID: 11457 RVA: 0x001409C4 File Offset: 0x0013EDC4
	public void SetOverrideContents(UIPanelContents contentsPrefab)
	{
		if (this.overrideContents != null)
		{
			UnityEngine.Object.Destroy(this.overrideContents.gameObject);
		}
		if (contentsPrefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(contentsPrefab.gameObject);
			this.overrideContents = gameObject.GetComponent<UIPanelContents>();
			((RectTransform)this.overrideContents.transform).SetParent(this.rootTransform, false);
			this.panelContents.gameObject.SetActive(false);
			gameObject.SetActive(true);
			this.overrideContents.enabled = true;
			this.overrideContents.panelContentsDelegate = this;
		}
		else
		{
			this.overrideContents = null;
			this.panelContents.gameObject.SetActive(true);
		}
	}

	// Token: 0x06002CC2 RID: 11458 RVA: 0x00140A80 File Offset: 0x0013EE80
	private void SetupSelectionCycle()
	{
		Selectable selectable = null;
		Selectable selectable2 = null;
		if (this.itemPanel != null)
		{
			selectable = this.itemPanel;
			selectable2 = this.itemPanel.navigation.selectOnDown;
		}
		int count = this.tabSelectionCycle.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(this.tabSelectionCycle[i] == null))
			{
				int index = ((i - 1) % count + count) % count;
				int index2 = (i + 1) % count;
				Navigation navigation = default(Navigation);
				navigation.mode = Navigation.Mode.Explicit;
				if (selectable2 != null)
				{
					navigation.selectOnDown = selectable2;
				}
				if (selectable != null)
				{
					navigation.selectOnUp = selectable;
				}
				navigation.selectOnLeft = this.tabSelectionCycle[index];
				navigation.selectOnRight = this.tabSelectionCycle[index2];
				this.tabSelectionCycle[i].navigation = navigation;
			}
		}
	}

	// Token: 0x06002CC3 RID: 11459 RVA: 0x00140B84 File Offset: 0x0013EF84
	public void HandleMenuInputEvents()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject == null)
		{
			return;
		}
		if ((!(this.itemPanel != null) || !(this.itemPanel.gameObject == currentSelectedGameObject)) && !currentSelectedGameObject.transform.IsChildOf(base.transform))
		{
			return;
		}
		foreach (Selectable selectable in this.tabSelectionCycle)
		{
			if (selectable.gameObject == EventSystem.current.currentSelectedGameObject && this.currentTabSelection != selectable)
			{
				PointerEventData eventData = new PointerEventData(EventSystem.current);
				ExecuteEvents.Execute<IPointerExitHandler>(this.currentTabSelection.gameObject, eventData, ExecuteEvents.pointerExitHandler);
				this.currentTabSelection = selectable;
				Debug.Log("Switching tab selection to selected object: " + this.currentTabSelection.gameObject);
			}
		}
		bool flag = false;
		bool flag2 = false;
		if (MappedInput.InputDown(MappableInput.MENU_CANCEL))
		{
			MainUIController.Instance.loadedSceneController.ClearSelection();
		}
		else
		{
			if (this.currentTabSelection == null)
			{
				return;
			}
			if (MappedInput.InputDown(MappableInput.MENU_SUBMIT))
			{
				ExecuteEvents.Execute<ISubmitHandler>(this.currentTabSelection.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
			}
			else if (MappedInput.InputDown(MappableInput.MENU_PAGE_LEFT))
			{
				flag2 = true;
			}
			else
			{
				if (!MappedInput.InputDown(MappableInput.MENU_PAGE_RIGHT))
				{
					return;
				}
				flag = true;
			}
		}
		Selectable selectable2 = null;
		if (flag)
		{
			selectable2 = this.currentTabSelection.navigation.selectOnRight;
		}
		else if (flag2)
		{
			selectable2 = this.currentTabSelection.navigation.selectOnLeft;
		}
		if (selectable2 != null)
		{
			PointerEventData eventData2 = new PointerEventData(EventSystem.current);
			ExecuteEvents.Execute<IPointerExitHandler>(this.currentTabSelection.gameObject, eventData2, ExecuteEvents.pointerExitHandler);
			if (this.currentTabSelection is InputField)
			{
				(this.currentTabSelection as InputField).DeactivateInputField();
			}
			if (this.currentTabSelection.gameObject == EventSystem.current.currentSelectedGameObject)
			{
				selectable2.Select();
			}
			this.currentTabSelection = selectable2;
			ExecuteEvents.Execute<IPointerEnterHandler>(this.currentTabSelection.gameObject, eventData2, ExecuteEvents.pointerEnterHandler);
			if (this.currentTabSelection is InputField)
			{
				(this.currentTabSelection as InputField).ActivateInputField();
			}
		}
	}

	// Token: 0x06002CC4 RID: 11460 RVA: 0x00140E30 File Offset: 0x0013F230
	public void OnLayoutComplete(UIPanelContents panelContents)
	{
	}

	// Token: 0x06002CC5 RID: 11461 RVA: 0x00140E32 File Offset: 0x0013F232
	public void OnCloseButtonPressed(UIPanelContents panelContents)
	{
		this.Hide(false);
	}

	// Token: 0x06002CC6 RID: 11462 RVA: 0x00140E3B File Offset: 0x0013F23B
	public void OnButtonPressed(UIPanelContents panelContents, string message)
	{
	}

	// Token: 0x04002553 RID: 9555
	[HideInInspector]
	public static DetailPanel activeDetailPanel;

	// Token: 0x04002554 RID: 9556
	public UIPanelContents panelContents;

	// Token: 0x04002555 RID: 9557
	public List<Selectable> tabSelectionCycle;

	// Token: 0x04002556 RID: 9558
	public DetailPanelDelegate detailPanelDelegate;

	// Token: 0x04002557 RID: 9559
	public RectTransform cloneItemPosition;

	// Token: 0x04002558 RID: 9560
	[HideInInspector]
	public ItemPanelList itemPanelList;

	// Token: 0x04002559 RID: 9561
	private RectTransform rootTransform;

	// Token: 0x0400255A RID: 9562
	private Animator animator;

	// Token: 0x0400255B RID: 9563
	private GameObject contentObject;

	// Token: 0x0400255D RID: 9565
	private UIPanelContents overrideContents;

	// Token: 0x0400255E RID: 9566
	private bool showing;

	// Token: 0x0400255F RID: 9567
	private ItemPanel itemPanel;

	// Token: 0x04002560 RID: 9568
	private UIDataSource dataSource;

	// Token: 0x04002561 RID: 9569
	private Selectable currentTabSelection;
}
