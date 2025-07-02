using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanelPagedList : UISceneElement, ItemPanelListDelegate, DetailPanelDelegate
{
	public ItemPanel panelPrefab;

	public DetailPanel detailPanelPrefab;

	public GameObject noDataPrefab;

	public RectTransform contentParent;

	public CanvasGroup noContentsGroup;

	public CanvasGroup titleGroup;

	public CanvasGroup nextButtonGroup;

	public CanvasGroup prevButtonGroup;

	public bool hideIfEmpty;

	private CanvasGroup contentGroup;

	[HideInInspector]
	public ItemPanelList itemList;

	public AnimationCurve scrollAnimCurve;

	public float scrollAnimDuration = 0.5f;

	public ItemPanelPagedListDelegate pagedListDelegate;

	private Text title;

	private int itemsPerPage = 4;

	private float panelWidth = 200f;

	private float itemSpacing;

	private int pageStartIndex;

	private DetailPanel detailPanel;

	private float scrollPosition;

	private bool updatePosition;

	private float scrollAnimStartPos;

	private float scrollAnimEndPos;

	private float scrollAnimTime = -1f;

	private bool initComplete;

	private bool itemsLoaded;

	private void Awake()
	{
		contentGroup = contentParent.GetComponent<CanvasGroup>();
		ShowContent(show: false);
		ShowNoContentsView(show: false);
		ShowNextButton(show: false);
		ShowPrevButton(show: false);
	}

	public override void Init()
	{
		if (!initComplete)
		{
			initComplete = true;
			itemList = contentParent.GetComponentInChildren<ItemPanelList>();
			HorizontalLayoutGroup componentInChildren = itemList.GetComponentInChildren<HorizontalLayoutGroup>();
			itemSpacing = componentInChildren.spacing;
			updatePosition = false;
			LayoutElement component = panelPrefab.GetComponent<LayoutElement>();
			panelWidth = component.preferredWidth;
			if (detailPanelPrefab != null)
			{
				GameObject gameObject = Object.Instantiate(detailPanelPrefab.gameObject);
				RectTransform parent = (RectTransform)base.transform.parent;
				RectTransform rectTransform = (RectTransform)gameObject.transform;
				rectTransform.SetParent(parent, worldPositionStays: false);
				rectTransform.SetSiblingIndex(base.transform.GetSiblingIndex() + 1);
				detailPanel = gameObject.GetComponent<DetailPanel>();
			}
			if (detailPanel != null)
			{
				itemList.detailView = detailPanel;
				detailPanel.detailPanelDelegate = this;
				detailPanel.itemPanelList = itemList;
				detailPanel.Init();
			}
			ShowNoContentsView(show: false);
		}
	}

	protected override void LoadContentFromDataSource()
	{
		if (!itemsLoaded)
		{
			imageManager = imageManager;
			Init();
			Clear();
			dataSource.AddListener(OnDataSourceReloaded);
			if (forceReloadData)
			{
				dataSource.ClearData();
			}
			dataSource.LoadIfNeeded();
		}
	}

	public override void ClearSelection()
	{
		if (itemList != null)
		{
			itemList.ClearSelection();
		}
		detailPanel.Hide(immediate: false);
	}

	public override void UnloadContent()
	{
		Clear();
		detailPanel.Hide(immediate: true);
		dataSource.RemoveListener(OnDataSourceReloaded);
	}

	private void OnDataSourceReloaded(List<string> modifiedKeys)
	{
		Layout();
	}

	private void Layout()
	{
		if (!dataSource.IsDataLoaded())
		{
			return;
		}
		if (itemsLoaded)
		{
			bool flag = false;
			HashSet<string> hashSet = new HashSet<string>();
			foreach (string itemId in itemList.itemIds)
			{
				if (!dataSource.Keys.Contains(itemId))
				{
					hashSet.Add(itemId);
				}
			}
			foreach (string item in hashSet)
			{
				if (itemList.selectedItem != null && item == itemList.selectedItem.itemId)
				{
					itemList.ClearSelection();
					HideDetailPanel(immediate: true);
				}
				itemList.RemoveItemWithId(item);
				flag = true;
			}
			HashSet<string> hashSet2 = new HashSet<string>();
			foreach (string key in dataSource.Keys)
			{
				if (!itemList.itemIds.Contains(key))
				{
					int index = dataSource.Keys.IndexOf(key);
					itemList.InsertItemWithId(index, key, dataSource, imageManager);
					flag = true;
				}
			}
			if (flag)
			{
				itemList.OrganizeChildObjects();
				DoScroll();
			}
			if (itemList.items.Count == 0 && detailPanel != null && detailPanel.IsShowing())
			{
				detailPanel.Hide(immediate: false);
			}
			if (itemList.itemIds.Count == 0)
			{
				if (hideIfEmpty)
				{
					base.gameObject.SetActive(value: false);
					if (detailPanel != null)
					{
						detailPanel.gameObject.SetActive(value: false);
					}
				}
				else
				{
					ShowNoContentsView(show: true);
				}
			}
			else
			{
				ShowNoContentsView(show: false);
			}
			ShowContent(show: true);
		}
		else
		{
			List<string> keys = dataSource.Keys;
			ItemPanel selectedItem = itemList.selectedItem;
			if (selectedItem == null || !keys.Contains(selectedItem.itemId))
			{
				itemList.ClearSelection();
				HideDetailPanel(immediate: true);
			}
			itemList.SetPanelPrefab(panelPrefab);
			itemList.SetNoDataPrefab(noDataPrefab);
			itemList.listDelegate = this;
			RectTransform rectTransform = (RectTransform)base.transform;
			float x = rectTransform.sizeDelta.x;
			itemsPerPage = Mathf.FloorToInt(x / ItemWidth());
			float num = (float)itemsPerPage * (panelWidth + itemSpacing);
			itemList.LoadContent(keys, itemsPerPage + 2, dataSource, imageManager);
			pageStartIndex = 0;
			SetScrollPosition(0f);
			DoScroll();
			itemsLoaded = true;
			Animator component = GetComponent<Animator>();
			if (component != null)
			{
				component.SetTrigger("Show");
			}
			ShowNoContentsView(keys.Count == 0);
			ShowContent(show: true);
		}
	}

	private void ShowNoContentsView(bool show)
	{
		if (titleGroup != null)
		{
			titleGroup.interactable = !show;
			titleGroup.blocksRaycasts = !show;
		}
		ShowGroup(noContentsGroup, show);
	}

	private void ShowContent(bool show)
	{
		ShowGroup(contentGroup, show);
	}

	private void ShowNextButton(bool show)
	{
		ShowGroup(nextButtonGroup, show);
	}

	private void ShowPrevButton(bool show)
	{
		ShowGroup(prevButtonGroup, show);
	}

	private void Clear()
	{
		if (itemList != null)
		{
			itemList.Clear();
		}
		itemsLoaded = false;
	}

	public void HideDetailPanel(bool immediate)
	{
		if (detailPanel != null)
		{
			detailPanel.Hide(immediate);
		}
	}

	public void SetTitleText(string text)
	{
		title.text = text;
	}

	private bool NextPageAvailable()
	{
		return pageStartIndex < itemList.items.Count - itemsPerPage;
	}

	private bool PrevPageAvailable()
	{
		return pageStartIndex >= itemsPerPage;
	}

	public void ButtonTapped_NextPage()
	{
		if (NextPageAvailable())
		{
			pageStartIndex += itemsPerPage;
			DoScroll();
		}
	}

	public void ButtonTapped_PreviousPage()
	{
		if (PrevPageAvailable())
		{
			pageStartIndex -= itemsPerPage;
			DoScroll();
		}
	}

	private void DoScroll()
	{
		int num = pageStartIndex + itemsPerPage;
		for (int i = pageStartIndex; i <= num + 1; i++)
		{
			itemList.SetupPanelForItemAtIndex(i);
		}
		for (int j = 0; j < itemList.items.Count; j++)
		{
			bool enable = j >= pageStartIndex && j < num;
			itemList.EnableItemAtIndex(j, enable);
		}
		scrollAnimStartPos = scrollPosition;
		scrollAnimEndPos = (0f - (float)pageStartIndex) * ItemWidth();
		scrollAnimTime = scrollAnimDuration;
		ShowPrevButton(PrevPageAvailable());
		ShowNextButton(NextPageAvailable());
		if (itemList.items.Count > pageStartIndex)
		{
			defaultSelectable = itemList.items[pageStartIndex].GetComponent<Selectable>();
		}
		else
		{
			defaultSelectable = null;
		}
	}

	public void SetScrollPosition(float pos)
	{
		scrollPosition = pos;
		updatePosition = true;
	}

	public override bool SelectItemWithID(string itemID)
	{
		List<GameObject> items = itemList.items;
		for (int i = 0; i < items.Count; i++)
		{
			ItemPanel component = items[i].GetComponent<ItemPanel>();
			if (component != null && component.itemId == itemID)
			{
				SelectItemAtIndex(i);
				return true;
			}
		}
		return false;
	}

	public override GameObject CloneItemWithID(string itemID)
	{
		GameObject gameObject = base.CloneItemWithID(itemID);
		List<GameObject> items = itemList.items;
		for (int i = 0; i < items.Count; i++)
		{
			ItemPanel component = items[i].GetComponent<ItemPanel>();
			if (component != null && component.itemId == itemID)
			{
				RectTransform rectTransform = (RectTransform)gameObject.transform;
				RectTransform rectTransform2 = ((!(detailPanel != null) || !detailPanel.IsShowing() || !(detailPanel.cloneItemPosition != null)) ? ((RectTransform)items[i].transform) : detailPanel.cloneItemPosition);
				Rect rect = rectTransform2.rect;
				rectTransform.position = rectTransform2.position;
				rectTransform.sizeDelta = rect.size;
				return gameObject;
			}
		}
		return null;
	}

	public void SelectItemAtIndex(int index)
	{
		if (pageStartIndex + itemsPerPage < index)
		{
			pageStartIndex = index;
			DoScroll();
			scrollAnimTime = 0f;
			scrollPosition = scrollAnimEndPos;
			float num = ContentWidth();
			if (num > 1f)
			{
				float x = num / 2f + scrollPosition;
				contentParent.anchoredPosition = new Vector2(x, contentParent.anchoredPosition.y);
				updatePosition = false;
			}
		}
		itemList.SelectItemAtIndex(index);
	}

	public new void Update()
	{
		base.Update();
		if (!itemsLoaded)
		{
			return;
		}
		if (scrollAnimTime > 0f)
		{
			float t = scrollAnimCurve.Evaluate((scrollAnimDuration - scrollAnimTime) / scrollAnimDuration);
			scrollPosition = Mathf.Lerp(scrollAnimStartPos, scrollAnimEndPos, t);
			scrollAnimTime -= Time.deltaTime;
			if (scrollAnimTime <= 0f)
			{
				scrollPosition = scrollAnimEndPos;
				scrollAnimTime = -1f;
			}
			updatePosition = true;
		}
		if (updatePosition)
		{
			float num = ContentWidth();
			if (num > 1f)
			{
				float x = num / 2f + scrollPosition;
				contentParent.anchoredPosition = new Vector2(x, contentParent.anchoredPosition.y);
				updatePosition = false;
			}
		}
	}

	private float ContentWidth()
	{
		return (float)(itemList.items.Count + itemList.noDataPanels.Count) * (panelWidth + itemSpacing);
	}

	private float PageWidth()
	{
		return (float)itemsPerPage * (panelWidth + itemSpacing);
	}

	private float ItemWidth()
	{
		return panelWidth + itemSpacing;
	}

	public void ItemInListSelected(ItemPanelList list, int index, bool selectedByClick)
	{
		if (pagedListDelegate != null)
		{
			pagedListDelegate.ItemInPagedListSelected(this, index, selectedByClick);
		}
		if (pageStartIndex >= index)
		{
			pageStartIndex = index;
		}
		else if (pageStartIndex + itemsPerPage <= index)
		{
			pageStartIndex = index - itemsPerPage + 1;
		}
		DoScroll();
	}

	public Selectable FindDownSelectable(ItemPanelList list, int index)
	{
		if (nextElement == null)
		{
			return null;
		}
		if (!(nextElement is ItemPanelPagedList))
		{
			return nextElement.defaultSelectable;
		}
		ItemPanelPagedList itemPanelPagedList = nextElement as ItemPanelPagedList;
		if (itemPanelPagedList.itemList == null)
		{
			return null;
		}
		int num = index - pageStartIndex;
		int num2 = Mathf.Min(itemPanelPagedList.itemList.items.Count - 1, itemPanelPagedList.pageStartIndex + num);
		if (num2 >= 0)
		{
			return itemPanelPagedList.itemList.items[num2].GetComponent<Selectable>();
		}
		return null;
	}

	public Selectable FindUpSelectable(ItemPanelList list, int index)
	{
		if (previousElement == null)
		{
			return null;
		}
		if (!(previousElement is ItemPanelPagedList))
		{
			return previousElement.defaultSelectable;
		}
		ItemPanelPagedList itemPanelPagedList = previousElement as ItemPanelPagedList;
		if (itemPanelPagedList.itemList == null)
		{
			return null;
		}
		int num = index - pageStartIndex;
		int num2 = Mathf.Min(itemPanelPagedList.itemList.items.Count - 1, itemPanelPagedList.pageStartIndex + num);
		if (num2 >= 0)
		{
			return itemPanelPagedList.itemList.items[num2].GetComponent<Selectable>();
		}
		return null;
	}

	public void DetailPanelClosed(DetailPanel detailPanel)
	{
		itemList.ClearSelection();
	}

	public override void UnloadEditorExampleContent()
	{
		if (detailPanel != null)
		{
			Object.DestroyImmediate(detailPanel.gameObject);
		}
		int childCount = itemList.transform.childCount;
		for (int num = childCount - 1; num >= 0; num--)
		{
			GameObject gameObject = itemList.transform.GetChild(num).gameObject;
			if (gameObject.GetComponent<ItemPanel>() != null)
			{
				Object.DestroyImmediate(gameObject);
			}
		}
	}

	public void OnDestroy()
	{
		if (dataSource != null)
		{
			dataSource.RemoveListener(OnDataSourceReloaded);
		}
	}
}
