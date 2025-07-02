using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DetailPanel : MonoBehaviour, PanelContentsDelegate, IMenuInputHandler
{
	[HideInInspector]
	public static DetailPanel activeDetailPanel;

	public UIPanelContents panelContents;

	public List<Selectable> tabSelectionCycle;

	public DetailPanelDelegate detailPanelDelegate;

	public RectTransform cloneItemPosition;

	[HideInInspector]
	public ItemPanelList itemPanelList;

	private RectTransform rootTransform;

	private Animator animator;

	private GameObject contentObject;

	private UIPanelContents overrideContents;

	private bool showing;

	private ItemPanel itemPanel;

	private UIDataSource dataSource;

	private Selectable currentTabSelection;

	public string contentId { get; private set; }

	public void Init()
	{
		base.gameObject.SetActive(value: true);
		rootTransform = (RectTransform)base.transform;
		animator = GetComponent<Animator>();
		panelContents.Init();
		panelContents.panelContentsDelegate = this;
	}

	public void LoadContentForID(string id, UIDataSource dataSource, ImageManager imageManager)
	{
		this.dataSource = dataSource;
		contentId = id;
		if (overrideContents != null)
		{
			overrideContents.SetupPanel(dataSource, imageManager, contentId);
		}
		else
		{
			panelContents.SetupPanel(dataSource, imageManager, contentId);
		}
	}

	public void LinkToItemPanel(ItemPanel itemPanel)
	{
		this.itemPanel = itemPanel;
	}

	public void Show(bool immediate)
	{
		if (!showing)
		{
			showing = true;
			string trigger = ((!immediate) ? "Show" : "ShowImmediate");
			animator.SetTrigger(trigger);
			ResetAnimation();
			panelContents.Show();
			activeDetailPanel = this;
			MenuInputHandler.RequestControl(this);
			if (tabSelectionCycle != null && tabSelectionCycle.Count > 0)
			{
				currentTabSelection = tabSelectionCycle[0];
				SetupSelectionCycle();
			}
		}
	}

	public void Hide(bool immediate)
	{
		if (!showing)
		{
			return;
		}
		dataSource.BackupData(contentId);
		showing = false;
		MenuInputHandler.Release(this);
		string trigger = ((!immediate) ? "Hide" : "HideImmediate");
		animator.SetTrigger(trigger);
		ResetAnimation();
		if (overrideContents != null)
		{
			overrideContents.Hide();
		}
		if (panelContents != null)
		{
			panelContents.Hide();
		}
		if (detailPanelDelegate != null)
		{
			detailPanelDelegate.DetailPanelClosed(this);
		}
		UIScrollControl componentInParent = GetComponentInParent<UIScrollControl>();
		if (componentInParent != null)
		{
			if (!immediate && this == activeDetailPanel && itemPanel != null)
			{
				componentInParent.ScrollToTransform(rootTransform, 0.2f, 0.75f, 0.1f);
			}
			else
			{
				componentInParent.ClampScroll();
			}
		}
		if (activeDetailPanel == this)
		{
			activeDetailPanel = null;
		}
	}

	public bool IsShowing()
	{
		return showing;
	}

	private void OnDestroy()
	{
		MenuInputHandler.Release(this);
		if (this == activeDetailPanel)
		{
			activeDetailPanel = null;
		}
	}

	public void TriggerAnimation(string animKey, string animTrigger)
	{
		UIPanelElementAnimator[] componentsInChildren = GetComponentsInChildren<UIPanelElementAnimator>();
		foreach (UIPanelElementAnimator uIPanelElementAnimator in componentsInChildren)
		{
			if (uIPanelElementAnimator.animKey == animKey)
			{
				uIPanelElementAnimator.TriggerAnimation(animTrigger);
			}
		}
	}

	public void ResetAnimation()
	{
		UIPanelElementAnimator[] componentsInChildren = GetComponentsInChildren<UIPanelElementAnimator>();
		foreach (UIPanelElementAnimator uIPanelElementAnimator in componentsInChildren)
		{
			uIPanelElementAnimator.Reset();
		}
	}

	public void SetOverrideContents(UIPanelContents contentsPrefab)
	{
		if (overrideContents != null)
		{
			Object.Destroy(overrideContents.gameObject);
		}
		if (contentsPrefab != null)
		{
			GameObject gameObject = Object.Instantiate(contentsPrefab.gameObject);
			overrideContents = gameObject.GetComponent<UIPanelContents>();
			((RectTransform)overrideContents.transform).SetParent(rootTransform, worldPositionStays: false);
			panelContents.gameObject.SetActive(value: false);
			gameObject.SetActive(value: true);
			overrideContents.enabled = true;
			overrideContents.panelContentsDelegate = this;
		}
		else
		{
			overrideContents = null;
			panelContents.gameObject.SetActive(value: true);
		}
	}

	private void SetupSelectionCycle()
	{
		Selectable selectable = null;
		Selectable selectable2 = null;
		if (itemPanel != null)
		{
			selectable = itemPanel;
			selectable2 = itemPanel.navigation.selectOnDown;
		}
		int count = tabSelectionCycle.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(tabSelectionCycle[i] == null))
			{
				int index = ((i - 1) % count + count) % count;
				int index2 = (i + 1) % count;
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit
				};
				if (selectable2 != null)
				{
					navigation.selectOnDown = selectable2;
				}
				if (selectable != null)
				{
					navigation.selectOnUp = selectable;
				}
				navigation.selectOnLeft = tabSelectionCycle[index];
				navigation.selectOnRight = tabSelectionCycle[index2];
				tabSelectionCycle[i].navigation = navigation;
			}
		}
	}

	public void HandleMenuInputEvents()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject == null || ((!(itemPanel != null) || !(itemPanel.gameObject == currentSelectedGameObject)) && !currentSelectedGameObject.transform.IsChildOf(base.transform)))
		{
			return;
		}
		foreach (Selectable item in tabSelectionCycle)
		{
			if (item.gameObject == EventSystem.current.currentSelectedGameObject && currentTabSelection != item)
			{
				PointerEventData eventData = new PointerEventData(EventSystem.current);
				ExecuteEvents.Execute(currentTabSelection.gameObject, eventData, ExecuteEvents.pointerExitHandler);
				currentTabSelection = item;
				Debug.Log("Switching tab selection to selected object: " + currentTabSelection.gameObject);
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
			if (currentTabSelection == null)
			{
				return;
			}
			if (MappedInput.InputDown(MappableInput.MENU_SUBMIT))
			{
				ExecuteEvents.Execute(currentTabSelection.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
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
		Selectable selectable = null;
		if (flag)
		{
			selectable = currentTabSelection.navigation.selectOnRight;
		}
		else if (flag2)
		{
			selectable = currentTabSelection.navigation.selectOnLeft;
		}
		if (selectable != null)
		{
			PointerEventData eventData2 = new PointerEventData(EventSystem.current);
			ExecuteEvents.Execute(currentTabSelection.gameObject, eventData2, ExecuteEvents.pointerExitHandler);
			if (currentTabSelection is InputField)
			{
				(currentTabSelection as InputField).DeactivateInputField();
			}
			if (currentTabSelection.gameObject == EventSystem.current.currentSelectedGameObject)
			{
				selectable.Select();
			}
			currentTabSelection = selectable;
			ExecuteEvents.Execute(currentTabSelection.gameObject, eventData2, ExecuteEvents.pointerEnterHandler);
			if (currentTabSelection is InputField)
			{
				(currentTabSelection as InputField).ActivateInputField();
			}
		}
	}

	public void OnLayoutComplete(UIPanelContents panelContents)
	{
	}

	public void OnCloseButtonPressed(UIPanelContents panelContents)
	{
		Hide(immediate: false);
	}

	public void OnButtonPressed(UIPanelContents panelContents, string message)
	{
	}
}
