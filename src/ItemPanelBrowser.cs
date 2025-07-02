using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanelBrowser : UISceneElement, ItemPanelListDelegate, DetailPanelDelegate
{
	public ItemPanelList browserRowPrefab;

	public DetailPanel detailPanelPrefab;

	public CanvasGroup noContentsGroup;

	public List<Button> nextPageButtons;

	public List<Button> prevPageButtons;

	public Text pageIndicator;

	private RectTransform browserContentParent;

	private int currentPage = 1;

	private int totalPageCount;

	public int itemsPerPage = 24;

	private bool itemsLoaded;

	private HashSet<string> loadedItemIds;

	private CanvasGroup canvasGroup;

	private List<ItemPanelList> rows;

	public void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		Hide();
		if (nextPageButtons != null)
		{
			foreach (Button nextPageButton in nextPageButtons)
			{
				nextPageButton.onClick.RemoveListener(NextPageButtonPressed);
				nextPageButton.onClick.AddListener(NextPageButtonPressed);
			}
		}
		if (prevPageButtons == null)
		{
			return;
		}
		foreach (Button prevPageButton in prevPageButtons)
		{
			prevPageButton.onClick.RemoveListener(PrevPageButtonPressed);
			prevPageButton.onClick.AddListener(PrevPageButtonPressed);
		}
	}

	public override void Init()
	{
		browserContentParent = (RectTransform)base.transform;
	}

	public void OnDestroy()
	{
		if (dataSource != null)
		{
			dataSource.RemoveListener(OnDataSourceReloaded);
		}
	}

	protected override void LoadContentFromDataSource()
	{
		Init();
		Clear();
		dataSource.AddListener(OnDataSourceReloaded);
		if (forceReloadData)
		{
			dataSource.ClearData();
		}
		dataSource.LoadIfNeeded();
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
		int num = (currentPage - 1) * itemsPerPage;
		int count = Mathf.Min(itemsPerPage, dataSource.Keys.Count - num);
		List<string> range = dataSource.Keys.GetRange(num, count);
		if (itemsLoaded)
		{
			bool flag = false;
			if (range.Count == loadedItemIds.Count)
			{
				foreach (string item in range)
				{
					if (!loadedItemIds.Contains(item))
					{
						flag = true;
						break;
					}
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			ClearSelection();
			Clear();
		}
		Canvas componentInParent = GetComponentInParent<Canvas>();
		RectTransform rectTransform = (RectTransform)componentInParent.transform;
		float x = rectTransform.sizeDelta.x;
		float x2 = browserContentParent.offsetMin.x;
		float preferredWidth = browserRowPrefab.panelPrefab.GetComponent<LayoutElement>().preferredWidth;
		HorizontalLayoutGroup component = browserRowPrefab.rootTransform.GetComponent<HorizontalLayoutGroup>();
		float spacing = component.spacing;
		float leftPadding = component.padding.left;
		float rightPadding = component.padding.right;
		int a = Mathf.FloorToInt((x - x2 * 2f) / (preferredWidth + spacing));
		a = Mathf.Max(a, 4);
		float fullRowWidth = WidthForNItems(a, preferredWidth, spacing, leftPadding, rightPadding);
		rows = new List<ItemPanelList>();
		for (int i = 0; i < range.Count; i += a)
		{
			int num2 = Mathf.Min(i + a, range.Count);
			List<string> range2 = range.GetRange(i, num2 - i);
			float rowWidth = WidthForNItems(range2.Count, preferredWidth, spacing, leftPadding, rightPadding);
			rows.Add(CreateRow(range2, fullRowWidth, rowWidth, dataSource, imageManager));
		}
		if (range.Count > 0)
		{
			Show();
		}
		else
		{
			ShowNoContentsGroup(show: true);
		}
		if (rows.Count > 0 && rows[0].items.Count > 0)
		{
			defaultSelectable = rows[0].items[0].GetComponent<Selectable>();
		}
		else
		{
			defaultSelectable = null;
		}
		UpdatePageControls();
		loadedItemIds = new HashSet<string>(range);
		itemsLoaded = true;
	}

	private float WidthForNItems(int n, float panelWidth, float panelSpacing, float leftPadding, float rightPadding)
	{
		return panelWidth * (float)n + panelSpacing * (float)(n - 1) + leftPadding + rightPadding;
	}

	private void Hide()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
	}

	private void Show()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		}
	}

	public override bool SelectItemWithID(string itemID)
	{
		if (rows == null)
		{
			return false;
		}
		foreach (ItemPanelList row in rows)
		{
			foreach (GameObject item in row.items)
			{
				ItemPanel component = item.GetComponent<ItemPanel>();
				if (component != null && component.itemId == itemID)
				{
					component.DoSelect();
					return true;
				}
			}
		}
		return false;
	}

	public override GameObject CloneItemWithID(string itemID)
	{
		GameObject gameObject = base.CloneItemWithID(itemID);
		foreach (ItemPanelList row in rows)
		{
			foreach (GameObject item in row.items)
			{
				ItemPanel component = item.GetComponent<ItemPanel>();
				if (component != null && component.itemId == itemID)
				{
					RectTransform rectTransform = (RectTransform)gameObject.transform;
					RectTransform rectTransform2 = ((!(row.detailView != null) || !row.detailView.IsShowing() || !(row.detailView.cloneItemPosition != null)) ? ((RectTransform)item.transform) : row.detailView.cloneItemPosition);
					Rect rect = rectTransform2.rect;
					rectTransform.position = rectTransform2.position;
					rectTransform.sizeDelta = rect.size;
					return gameObject;
				}
			}
		}
		return null;
	}

	public override void UnloadContent()
	{
		dataSource.RemoveListener(OnDataSourceReloaded);
		Clear();
	}

	private void Clear()
	{
		browserContentParent = (RectTransform)base.transform;
		if (rows != null)
		{
			foreach (ItemPanelList row in rows)
			{
				if (row.detailView != null)
				{
					row.detailView.Hide(immediate: true);
				}
				Object.Destroy(row.gameObject);
			}
		}
		itemsLoaded = false;
		loadedItemIds = new HashSet<string>();
		rows = new List<ItemPanelList>();
		ShowNoContentsGroup(show: false);
	}

	private ItemPanelList CreateRow(List<string> ids, float fullRowWidth, float rowWidth, UIDataSource dataSource, ImageManager imageManager)
	{
		GameObject gameObject = Object.Instantiate(browserRowPrefab.gameObject);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(browserContentParent, worldPositionStays: false);
		ItemPanelList component = gameObject.GetComponent<ItemPanelList>();
		component.LoadContent(ids, ids.Count, dataSource, imageManager);
		if (detailPanelPrefab != null)
		{
			GameObject gameObject2 = Object.Instantiate(detailPanelPrefab.gameObject);
			RectTransform rectTransform2 = (RectTransform)gameObject2.transform;
			DetailPanel component2 = gameObject2.GetComponent<DetailPanel>();
			component2.Init();
			component2.detailPanelDelegate = this;
			rectTransform2.SetParent(browserContentParent, worldPositionStays: false);
			component2.itemPanelList = component;
			component.detailView = component2;
		}
		for (int i = 0; i < ids.Count; i++)
		{
			component.SetupPanelForItemAtIndex(i);
		}
		float num = (fullRowWidth - rowWidth) / 2f;
		float x = component.rootTransform.anchoredPosition.x - num;
		component.rootTransform.anchoredPosition = new Vector2(x, component.rootTransform.anchoredPosition.y);
		component.listDelegate = this;
		return component;
	}

	private void ShowNoContentsGroup(bool show)
	{
		if (!(noContentsGroup == null))
		{
			noContentsGroup.alpha = ((!show) ? 0f : 1f);
			noContentsGroup.interactable = show;
			noContentsGroup.blocksRaycasts = show;
		}
	}

	private void UpdatePageControls()
	{
		if (nextPageButtons != null)
		{
			foreach (Button nextPageButton in nextPageButtons)
			{
				nextPageButton.interactable = !OnLastPage();
			}
		}
		if (prevPageButtons != null)
		{
			foreach (Button prevPageButton in prevPageButtons)
			{
				prevPageButton.interactable = currentPage > 1;
			}
		}
		if (pageIndicator != null)
		{
			pageIndicator.text = currentPage.ToString();
		}
	}

	public void NextPageButtonPressed()
	{
		if (!OnLastPage())
		{
			currentPage++;
			if (dataSource.CanExpand() && dataSource.PagesLoaded() <= currentPage)
			{
				dataSource.Expand();
			}
			Clear();
			Layout();
			UpdatePageControls();
		}
	}

	public void PrevPageButtonPressed()
	{
		if (currentPage > 1)
		{
			currentPage--;
			Clear();
			Layout();
			UpdatePageControls();
		}
	}

	private bool OnLastPage()
	{
		if (!dataSource.CanExpand())
		{
			return currentPage - 1 >= dataSource.Keys.Count / itemsPerPage;
		}
		return false;
	}

	public void ItemInListSelected(ItemPanelList list, int index, bool selectedByClick)
	{
		foreach (ItemPanelList row in rows)
		{
			if (row != list)
			{
				row.ClearSelection();
				if (row.detailView != null)
				{
					row.detailView.Hide(!selectedByClick);
				}
			}
		}
	}

	public Selectable FindDownSelectable(ItemPanelList list, int index)
	{
		int num = rows.IndexOf(list);
		if (num >= 0 && num < rows.Count - 1)
		{
			ItemPanelList itemPanelList = rows[num + 1];
			int num2 = Mathf.Min(index, itemPanelList.items.Count - 1);
			if (num2 >= 0)
			{
				return itemPanelList.items[num2].GetComponent<Selectable>();
			}
		}
		return null;
	}

	public Selectable FindUpSelectable(ItemPanelList list, int index)
	{
		int num = rows.IndexOf(list);
		if (num >= 1)
		{
			ItemPanelList itemPanelList = rows[num - 1];
			int num2 = Mathf.Min(index, itemPanelList.items.Count - 1);
			if (num2 >= 0)
			{
				return itemPanelList.items[num2].GetComponent<Selectable>();
			}
		}
		return null;
	}

	public void DetailPanelClosed(DetailPanel detailPanel)
	{
		foreach (ItemPanelList row in rows)
		{
			if (row == detailPanel.itemPanelList)
			{
				row.ClearSelection();
			}
		}
	}

	public override void ClearSelection()
	{
		if (rows == null)
		{
			return;
		}
		foreach (ItemPanelList row in rows)
		{
			row.ClearSelection();
			if (row.detailView != null)
			{
				row.detailView.Hide(immediate: false);
			}
		}
	}

	public override void UnloadEditorExampleContent()
	{
		browserContentParent = (RectTransform)base.transform;
		int childCount = browserContentParent.transform.childCount;
		for (int num = childCount - 1; num >= 0; num--)
		{
			Object.DestroyImmediate(browserContentParent.GetChild(num).gameObject);
		}
		if (rows != null)
		{
			rows.Clear();
		}
	}
}
