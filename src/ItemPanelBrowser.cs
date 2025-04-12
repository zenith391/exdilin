using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000429 RID: 1065
public class ItemPanelBrowser : UISceneElement, ItemPanelListDelegate, DetailPanelDelegate
{
	// Token: 0x06002DD5 RID: 11733 RVA: 0x00146458 File Offset: 0x00144858
	public void Awake()
	{
		this.canvasGroup = base.GetComponent<CanvasGroup>();
		this.Hide();
		if (this.nextPageButtons != null)
		{
			foreach (Button button in this.nextPageButtons)
			{
				button.onClick.RemoveListener(new UnityAction(this.NextPageButtonPressed));
				button.onClick.AddListener(new UnityAction(this.NextPageButtonPressed));
			}
		}
		if (this.prevPageButtons != null)
		{
			foreach (Button button2 in this.prevPageButtons)
			{
				button2.onClick.RemoveListener(new UnityAction(this.PrevPageButtonPressed));
				button2.onClick.AddListener(new UnityAction(this.PrevPageButtonPressed));
			}
		}
	}

	// Token: 0x06002DD6 RID: 11734 RVA: 0x00146578 File Offset: 0x00144978
	public override void Init()
	{
		this.browserContentParent = (RectTransform)base.transform;
	}

	// Token: 0x06002DD7 RID: 11735 RVA: 0x0014658B File Offset: 0x0014498B
	public void OnDestroy()
	{
		if (this.dataSource != null)
		{
			this.dataSource.RemoveListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		}
	}

	// Token: 0x06002DD8 RID: 11736 RVA: 0x001465B0 File Offset: 0x001449B0
	protected override void LoadContentFromDataSource()
	{
		this.Init();
		this.Clear();
		this.dataSource.AddListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		if (this.forceReloadData)
		{
			this.dataSource.ClearData();
		}
		this.dataSource.LoadIfNeeded();
	}

	// Token: 0x06002DD9 RID: 11737 RVA: 0x00146601 File Offset: 0x00144A01
	private void OnDataSourceReloaded(List<string> modifiedKeys)
	{
		this.Layout();
	}

	// Token: 0x06002DDA RID: 11738 RVA: 0x0014660C File Offset: 0x00144A0C
	private void Layout()
	{
		if (!this.dataSource.IsDataLoaded())
		{
			return;
		}
		int num = (this.currentPage - 1) * this.itemsPerPage;
		int count = Mathf.Min(this.itemsPerPage, this.dataSource.Keys.Count - num);
		List<string> range = this.dataSource.Keys.GetRange(num, count);
		if (this.itemsLoaded)
		{
			bool flag = false;
			if (range.Count == this.loadedItemIds.Count)
			{
				foreach (string item in range)
				{
					if (!this.loadedItemIds.Contains(item))
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
			this.ClearSelection();
			this.Clear();
		}
		Canvas componentInParent = base.GetComponentInParent<Canvas>();
		RectTransform rectTransform = (RectTransform)componentInParent.transform;
		float x = rectTransform.sizeDelta.x;
		float x2 = this.browserContentParent.offsetMin.x;
		float preferredWidth = this.browserRowPrefab.panelPrefab.GetComponent<LayoutElement>().preferredWidth;
		HorizontalLayoutGroup component = this.browserRowPrefab.rootTransform.GetComponent<HorizontalLayoutGroup>();
		float spacing = component.spacing;
		float leftPadding = (float)component.padding.left;
		float rightPadding = (float)component.padding.right;
		int num2 = Mathf.FloorToInt((x - x2 * 2f) / (preferredWidth + spacing));
		num2 = Mathf.Max(num2, 4);
		float fullRowWidth = this.WidthForNItems(num2, preferredWidth, spacing, leftPadding, rightPadding);
		this.rows = new List<ItemPanelList>();
		for (int i = 0; i < range.Count; i += num2)
		{
			int num3 = Mathf.Min(i + num2, range.Count);
			List<string> range2 = range.GetRange(i, num3 - i);
			float rowWidth = this.WidthForNItems(range2.Count, preferredWidth, spacing, leftPadding, rightPadding);
			this.rows.Add(this.CreateRow(range2, fullRowWidth, rowWidth, this.dataSource, this.imageManager));
		}
		if (range.Count > 0)
		{
			this.Show();
		}
		else
		{
			this.ShowNoContentsGroup(true);
		}
		if (this.rows.Count > 0 && this.rows[0].items.Count > 0)
		{
			this.defaultSelectable = this.rows[0].items[0].GetComponent<Selectable>();
		}
		else
		{
			this.defaultSelectable = null;
		}
		this.UpdatePageControls();
		this.loadedItemIds = new HashSet<string>(range);
		this.itemsLoaded = true;
	}

	// Token: 0x06002DDB RID: 11739 RVA: 0x001468DC File Offset: 0x00144CDC
	private float WidthForNItems(int n, float panelWidth, float panelSpacing, float leftPadding, float rightPadding)
	{
		return panelWidth * (float)n + panelSpacing * (float)(n - 1) + leftPadding + rightPadding;
	}

	// Token: 0x06002DDC RID: 11740 RVA: 0x001468EF File Offset: 0x00144CEF
	private void Hide()
	{
		if (this.canvasGroup != null)
		{
			this.canvasGroup.alpha = 0f;
			this.canvasGroup.interactable = false;
			this.canvasGroup.blocksRaycasts = false;
		}
	}

	// Token: 0x06002DDD RID: 11741 RVA: 0x0014692A File Offset: 0x00144D2A
	private void Show()
	{
		if (this.canvasGroup != null)
		{
			this.canvasGroup.alpha = 1f;
			this.canvasGroup.interactable = true;
			this.canvasGroup.blocksRaycasts = true;
		}
	}

	// Token: 0x06002DDE RID: 11742 RVA: 0x00146968 File Offset: 0x00144D68
	public override bool SelectItemWithID(string itemID)
	{
		if (this.rows == null)
		{
			return false;
		}
		foreach (ItemPanelList itemPanelList in this.rows)
		{
			foreach (GameObject gameObject in itemPanelList.items)
			{
				ItemPanel component = gameObject.GetComponent<ItemPanel>();
				if (component != null && component.itemId == itemID)
				{
					component.DoSelect();
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06002DDF RID: 11743 RVA: 0x00146A48 File Offset: 0x00144E48
	public override GameObject CloneItemWithID(string itemID)
	{
		GameObject gameObject = base.CloneItemWithID(itemID);
		foreach (ItemPanelList itemPanelList in this.rows)
		{
			foreach (GameObject gameObject2 in itemPanelList.items)
			{
				ItemPanel component = gameObject2.GetComponent<ItemPanel>();
				if (component != null && component.itemId == itemID)
				{
					RectTransform rectTransform = (RectTransform)gameObject.transform;
					RectTransform rectTransform2;
					if (itemPanelList.detailView != null && itemPanelList.detailView.IsShowing() && itemPanelList.detailView.cloneItemPosition != null)
					{
						rectTransform2 = itemPanelList.detailView.cloneItemPosition;
					}
					else
					{
						rectTransform2 = (RectTransform)gameObject2.transform;
					}
					Rect rect = rectTransform2.rect;
					rectTransform.position = rectTransform2.position;
					rectTransform.sizeDelta = rect.size;
					return gameObject;
				}
			}
		}
		return null;
	}

	// Token: 0x06002DE0 RID: 11744 RVA: 0x00146BC0 File Offset: 0x00144FC0
	public override void UnloadContent()
	{
		this.dataSource.RemoveListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		this.Clear();
	}

	// Token: 0x06002DE1 RID: 11745 RVA: 0x00146BE0 File Offset: 0x00144FE0
	private void Clear()
	{
		this.browserContentParent = (RectTransform)base.transform;
		if (this.rows != null)
		{
			foreach (ItemPanelList itemPanelList in this.rows)
			{
				if (itemPanelList.detailView != null)
				{
					itemPanelList.detailView.Hide(true);
				}
				UnityEngine.Object.Destroy(itemPanelList.gameObject);
			}
		}
		this.itemsLoaded = false;
		this.loadedItemIds = new HashSet<string>();
		this.rows = new List<ItemPanelList>();
		this.ShowNoContentsGroup(false);
	}

	// Token: 0x06002DE2 RID: 11746 RVA: 0x00146CA0 File Offset: 0x001450A0
	private ItemPanelList CreateRow(List<string> ids, float fullRowWidth, float rowWidth, UIDataSource dataSource, ImageManager imageManager)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.browserRowPrefab.gameObject);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(this.browserContentParent, false);
		ItemPanelList component = gameObject.GetComponent<ItemPanelList>();
		component.LoadContent(ids, ids.Count, dataSource, imageManager);
		if (this.detailPanelPrefab != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.detailPanelPrefab.gameObject);
			RectTransform rectTransform2 = (RectTransform)gameObject2.transform;
			DetailPanel component2 = gameObject2.GetComponent<DetailPanel>();
			component2.Init();
			component2.detailPanelDelegate = this;
			rectTransform2.SetParent(this.browserContentParent, false);
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

	// Token: 0x06002DE3 RID: 11747 RVA: 0x00146DC8 File Offset: 0x001451C8
	private void ShowNoContentsGroup(bool show)
	{
		if (this.noContentsGroup == null)
		{
			return;
		}
		this.noContentsGroup.alpha = ((!show) ? 0f : 1f);
		this.noContentsGroup.interactable = show;
		this.noContentsGroup.blocksRaycasts = show;
	}

	// Token: 0x06002DE4 RID: 11748 RVA: 0x00146E20 File Offset: 0x00145220
	private void UpdatePageControls()
	{
		if (this.nextPageButtons != null)
		{
			foreach (Button button in this.nextPageButtons)
			{
				button.interactable = !this.OnLastPage();
			}
		}
		if (this.prevPageButtons != null)
		{
			foreach (Button button2 in this.prevPageButtons)
			{
				button2.interactable = (this.currentPage > 1);
			}
		}
		if (this.pageIndicator != null)
		{
			this.pageIndicator.text = this.currentPage.ToString();
		}
	}

	// Token: 0x06002DE5 RID: 11749 RVA: 0x00146F1C File Offset: 0x0014531C
	public void NextPageButtonPressed()
	{
		if (this.OnLastPage())
		{
			return;
		}
		this.currentPage++;
		if (this.dataSource.CanExpand() && this.dataSource.PagesLoaded() <= this.currentPage)
		{
			this.dataSource.Expand();
		}
		this.Clear();
		this.Layout();
		this.UpdatePageControls();
	}

	// Token: 0x06002DE6 RID: 11750 RVA: 0x00146F86 File Offset: 0x00145386
	public void PrevPageButtonPressed()
	{
		if (this.currentPage <= 1)
		{
			return;
		}
		this.currentPage--;
		this.Clear();
		this.Layout();
		this.UpdatePageControls();
	}

	// Token: 0x06002DE7 RID: 11751 RVA: 0x00146FB5 File Offset: 0x001453B5
	private bool OnLastPage()
	{
		return !this.dataSource.CanExpand() && this.currentPage - 1 >= this.dataSource.Keys.Count / this.itemsPerPage;
	}

	// Token: 0x06002DE8 RID: 11752 RVA: 0x00146FF0 File Offset: 0x001453F0
	public void ItemInListSelected(ItemPanelList list, int index, bool selectedByClick)
	{
		foreach (ItemPanelList itemPanelList in this.rows)
		{
			if (itemPanelList != list)
			{
				itemPanelList.ClearSelection();
				if (itemPanelList.detailView != null)
				{
					itemPanelList.detailView.Hide(!selectedByClick);
				}
			}
		}
	}

	// Token: 0x06002DE9 RID: 11753 RVA: 0x00147078 File Offset: 0x00145478
	public Selectable FindDownSelectable(ItemPanelList list, int index)
	{
		int num = this.rows.IndexOf(list);
		if (num >= 0 && num < this.rows.Count - 1)
		{
			ItemPanelList itemPanelList = this.rows[num + 1];
			int num2 = Mathf.Min(index, itemPanelList.items.Count - 1);
			if (num2 >= 0)
			{
				return itemPanelList.items[num2].GetComponent<Selectable>();
			}
		}
		return null;
	}

	// Token: 0x06002DEA RID: 11754 RVA: 0x001470EC File Offset: 0x001454EC
	public Selectable FindUpSelectable(ItemPanelList list, int index)
	{
		int num = this.rows.IndexOf(list);
		if (num >= 1)
		{
			ItemPanelList itemPanelList = this.rows[num - 1];
			int num2 = Mathf.Min(index, itemPanelList.items.Count - 1);
			if (num2 >= 0)
			{
				return itemPanelList.items[num2].GetComponent<Selectable>();
			}
		}
		return null;
	}

	// Token: 0x06002DEB RID: 11755 RVA: 0x0014714C File Offset: 0x0014554C
	public void DetailPanelClosed(DetailPanel detailPanel)
	{
		foreach (ItemPanelList itemPanelList in this.rows)
		{
			if (itemPanelList == detailPanel.itemPanelList)
			{
				itemPanelList.ClearSelection();
			}
		}
	}

	// Token: 0x06002DEC RID: 11756 RVA: 0x001471B8 File Offset: 0x001455B8
	public override void ClearSelection()
	{
		if (this.rows == null)
		{
			return;
		}
		foreach (ItemPanelList itemPanelList in this.rows)
		{
			itemPanelList.ClearSelection();
			if (itemPanelList.detailView != null)
			{
				itemPanelList.detailView.Hide(false);
			}
		}
	}

	// Token: 0x06002DED RID: 11757 RVA: 0x0014723C File Offset: 0x0014563C
	public override void UnloadEditorExampleContent()
	{
		this.browserContentParent = (RectTransform)base.transform;
		int childCount = this.browserContentParent.transform.childCount;
		for (int i = childCount - 1; i >= 0; i--)
		{
			UnityEngine.Object.DestroyImmediate(this.browserContentParent.GetChild(i).gameObject);
		}
		if (this.rows != null)
		{
			this.rows.Clear();
		}
	}

	// Token: 0x04002652 RID: 9810
	public ItemPanelList browserRowPrefab;

	// Token: 0x04002653 RID: 9811
	public DetailPanel detailPanelPrefab;

	// Token: 0x04002654 RID: 9812
	public CanvasGroup noContentsGroup;

	// Token: 0x04002655 RID: 9813
	public List<Button> nextPageButtons;

	// Token: 0x04002656 RID: 9814
	public List<Button> prevPageButtons;

	// Token: 0x04002657 RID: 9815
	public Text pageIndicator;

	// Token: 0x04002658 RID: 9816
	private RectTransform browserContentParent;

	// Token: 0x04002659 RID: 9817
	private int currentPage = 1;

	// Token: 0x0400265A RID: 9818
	private int totalPageCount;

	// Token: 0x0400265B RID: 9819
	public int itemsPerPage = 24;

	// Token: 0x0400265C RID: 9820
	private bool itemsLoaded;

	// Token: 0x0400265D RID: 9821
	private HashSet<string> loadedItemIds;

	// Token: 0x0400265E RID: 9822
	private CanvasGroup canvasGroup;

	// Token: 0x0400265F RID: 9823
	private List<ItemPanelList> rows;
}
