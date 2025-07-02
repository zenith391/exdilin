using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class ItemPanel : Selectable, PanelContentsDelegate
{
	public string hoverParameterName = "Hover";

	public string pressParameterName = "Press";

	public UIPanelContents panelContents;

	public UIPanelContents expandedPanelContents;

	public RectTransform contentParent;

	public RectTransform expandedContentParent;

	public bool preserveDuringClear;

	public bool hideTillDataLoad;

	public UIPanelContents overrideDetailPanelContents;

	public string onSelectMessage;

	public ItemPanelDelegate itemDelegate;

	private bool selectedByClick;

	[HideInInspector]
	public string itemId;

	[HideInInspector]
	public string dataType;

	[HideInInspector]
	public string dataSubtype;

	private Animator _animator;

	private RectTransform _rootTransform;

	private Image _noContentsImage;

	private CanvasGroup mainCanvasGroup;

	public new void Awake()
	{
		Init();
		mainCanvasGroup = GetComponent<CanvasGroup>();
	}

	public void Init()
	{
		_animator = GetComponent<Animator>();
		_rootTransform = (RectTransform)base.transform;
		_noContentsImage = GetComponent<Image>();
		panelContents.Init();
		expandedPanelContents.Init();
		panelContents.panelContentsDelegate = this;
		expandedPanelContents.panelContentsDelegate = this;
		if (!hideTillDataLoad && _animator != null)
		{
			_animator.SetBool("DataLoaded", value: true);
		}
	}

	public void SetDisabled(bool disabled)
	{
		if (_animator != null)
		{
			_animator.SetBool("Disabled", disabled);
		}
	}

	public void DoSelect()
	{
		selectedByClick = false;
		Select();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (mainCanvasGroup != null)
		{
			mainCanvasGroup.interactable = false;
			mainCanvasGroup.blocksRaycasts = false;
		}
		SetDisabled(disabled: false);
		_animator.SetBool("Selected", value: true);
		if (itemDelegate != null)
		{
			itemDelegate.ItemSelected(this, selectedByClick);
		}
		selectedByClick = false;
		if (!string.IsNullOrEmpty(onSelectMessage))
		{
			BWStandalone.Instance.HandleMenuUIMessage(onSelectMessage, itemId, dataType, dataSubtype);
		}
	}

	public bool IsSelected()
	{
		return EventSystem.current.currentSelectedGameObject == base.gameObject;
	}

	public void LoadExpandedContents()
	{
	}

	public void ShowExpandedContents()
	{
		expandedPanelContents.Show();
	}

	public void HideExpandedContents()
	{
		expandedPanelContents.Hide();
	}

	public void Deselect()
	{
		if (mainCanvasGroup != null)
		{
			mainCanvasGroup.interactable = true;
			mainCanvasGroup.blocksRaycasts = true;
		}
		_animator.SetBool("Selected", value: false);
	}

	public void SetDetailMode(bool state)
	{
		_animator.SetBool("DetailMode", state);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		selectedByClick = true;
		_animator.SetBool(hoverParameterName, value: true);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		selectedByClick = false;
		_animator.SetBool(hoverParameterName, value: false);
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		_animator.SetBool(pressParameterName, value: true);
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		_animator.SetBool(pressParameterName, value: false);
	}

	public void OnLayoutComplete(UIPanelContents panelContents)
	{
		if (panelContents == this.panelContents && _animator != null)
		{
			_animator.SetBool("DataLoaded", value: true);
		}
		if (panelContents == expandedPanelContents && _animator != null)
		{
			_animator.SetBool("ExpandedDataLoaded", value: true);
		}
	}

	public void OnCloseButtonPressed(UIPanelContents panelContents)
	{
	}
}
