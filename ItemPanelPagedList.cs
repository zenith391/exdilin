using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200042C RID: 1068
public class ItemPanelPagedList : UISceneElement, ItemPanelListDelegate, DetailPanelDelegate
{
	// Token: 0x06002DFD RID: 11773 RVA: 0x001480BC File Offset: 0x001464BC
	private void Awake()
	{
		this.contentGroup = this.contentParent.GetComponent<CanvasGroup>();
		this.ShowContent(false);
		this.ShowNoContentsView(false);
		this.ShowNextButton(false);
		this.ShowPrevButton(false);
	}

	// Token: 0x06002DFE RID: 11774 RVA: 0x001480EC File Offset: 0x001464EC
	public override void Init()
	{
		if (this.initComplete)
		{
			return;
		}
		this.initComplete = true;
		this.itemList = this.contentParent.GetComponentInChildren<ItemPanelList>();
		HorizontalLayoutGroup componentInChildren = this.itemList.GetComponentInChildren<HorizontalLayoutGroup>();
		this.itemSpacing = componentInChildren.spacing;
		this.updatePosition = false;
		LayoutElement component = this.panelPrefab.GetComponent<LayoutElement>();
		this.panelWidth = component.preferredWidth;
		if (this.detailPanelPrefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.detailPanelPrefab.gameObject);
			RectTransform parent = (RectTransform)base.transform.parent;
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			rectTransform.SetParent(parent, false);
			rectTransform.SetSiblingIndex(base.transform.GetSiblingIndex() + 1);
			this.detailPanel = gameObject.GetComponent<DetailPanel>();
		}
		if (this.detailPanel != null)
		{
			this.itemList.detailView = this.detailPanel;
			this.detailPanel.detailPanelDelegate = this;
			this.detailPanel.itemPanelList = this.itemList;
			this.detailPanel.Init();
		}
		this.ShowNoContentsView(false);
	}

	// Token: 0x06002DFF RID: 11775 RVA: 0x00148210 File Offset: 0x00146610
	protected override void LoadContentFromDataSource()
	{
		if (this.itemsLoaded)
		{
			return;
		}
		this.imageManager = this.imageManager;
		this.Init();
		this.Clear();
		this.dataSource.AddListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		if (this.forceReloadData)
		{
			this.dataSource.ClearData();
		}
		this.dataSource.LoadIfNeeded();
	}

	// Token: 0x06002E00 RID: 11776 RVA: 0x00148279 File Offset: 0x00146679
	public override void ClearSelection()
	{
		if (this.itemList != null)
		{
			this.itemList.ClearSelection();
		}
		this.detailPanel.Hide(false);
	}

	// Token: 0x06002E01 RID: 11777 RVA: 0x001482A3 File Offset: 0x001466A3
	public override void UnloadContent()
	{
		this.Clear();
		this.detailPanel.Hide(true);
		this.dataSource.RemoveListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
	}

	// Token: 0x06002E02 RID: 11778 RVA: 0x001482CE File Offset: 0x001466CE
	private void OnDataSourceReloaded(List<string> modifiedKeys)
	{
		this.Layout();
	}

	// Token: 0x06002E03 RID: 11779 RVA: 0x001482D8 File Offset: 0x001466D8
	private void Layout()
	{
		if (!this.dataSource.IsDataLoaded())
		{
			return;
		}
		if (this.itemsLoaded)
		{
			bool flag = false;
			HashSet<string> hashSet = new HashSet<string>();
			foreach (string item in this.itemList.itemIds)
			{
				if (!this.dataSource.Keys.Contains(item))
				{
					hashSet.Add(item);
				}
			}
			foreach (string text in hashSet)
			{
				if (this.itemList.selectedItem != null && text == this.itemList.selectedItem.itemId)
				{
					this.itemList.ClearSelection();
					this.HideDetailPanel(true);
				}
				this.itemList.RemoveItemWithId(text);
				flag = true;
			}
			HashSet<string> hashSet2 = new HashSet<string>();
			foreach (string text2 in this.dataSource.Keys)
			{
				if (!this.itemList.itemIds.Contains(text2))
				{
					int index = this.dataSource.Keys.IndexOf(text2);
					this.itemList.InsertItemWithId(index, text2, this.dataSource, this.imageManager);
					flag = true;
				}
			}
			if (flag)
			{
				this.itemList.OrganizeChildObjects();
				this.DoScroll();
			}
			if (this.itemList.items.Count == 0 && this.detailPanel != null && this.detailPanel.IsShowing())
			{
				this.detailPanel.Hide(false);
			}
			bool flag2 = this.itemList.itemIds.Count == 0;
			if (flag2)
			{
				if (this.hideIfEmpty)
				{
					base.gameObject.SetActive(false);
					if (this.detailPanel != null)
					{
						this.detailPanel.gameObject.SetActive(false);
					}
				}
				else
				{
					this.ShowNoContentsView(true);
				}
			}
			else
			{
				this.ShowNoContentsView(false);
			}
			this.ShowContent(true);
			return;
		}
		List<string> keys = this.dataSource.Keys;
		ItemPanel selectedItem = this.itemList.selectedItem;
		if (selectedItem == null || !keys.Contains(selectedItem.itemId))
		{
			this.itemList.ClearSelection();
			this.HideDetailPanel(true);
		}
		this.itemList.SetPanelPrefab(this.panelPrefab);
		this.itemList.SetNoDataPrefab(this.noDataPrefab);
		this.itemList.listDelegate = this;
		RectTransform rectTransform = (RectTransform)base.transform;
		float x = rectTransform.sizeDelta.x;
		this.itemsPerPage = Mathf.FloorToInt(x / this.ItemWidth());
		float num = (float)this.itemsPerPage * (this.panelWidth + this.itemSpacing);
		this.itemList.LoadContent(keys, this.itemsPerPage + 2, this.dataSource, this.imageManager);
		this.pageStartIndex = 0;
		this.SetScrollPosition(0f);
		this.DoScroll();
		this.itemsLoaded = true;
		Animator component = base.GetComponent<Animator>();
		if (component != null)
		{
			component.SetTrigger("Show");
		}
		this.ShowNoContentsView(keys.Count == 0);
		this.ShowContent(true);
	}

	// Token: 0x06002E04 RID: 11780 RVA: 0x001486AC File Offset: 0x00146AAC
	private void ShowNoContentsView(bool show)
	{
		if (this.titleGroup != null)
		{
			this.titleGroup.interactable = !show;
			this.titleGroup.blocksRaycasts = !show;
		}
		base.ShowGroup(this.noContentsGroup, show);
	}

	// Token: 0x06002E05 RID: 11781 RVA: 0x001486EA File Offset: 0x00146AEA
	private void ShowContent(bool show)
	{
		base.ShowGroup(this.contentGroup, show);
	}

	// Token: 0x06002E06 RID: 11782 RVA: 0x001486F9 File Offset: 0x00146AF9
	private void ShowNextButton(bool show)
	{
		base.ShowGroup(this.nextButtonGroup, show);
	}

	// Token: 0x06002E07 RID: 11783 RVA: 0x00148708 File Offset: 0x00146B08
	private void ShowPrevButton(bool show)
	{
		base.ShowGroup(this.prevButtonGroup, show);
	}

	// Token: 0x06002E08 RID: 11784 RVA: 0x00148717 File Offset: 0x00146B17
	private void Clear()
	{
		if (this.itemList != null)
		{
			this.itemList.Clear();
		}
		this.itemsLoaded = false;
	}

	// Token: 0x06002E09 RID: 11785 RVA: 0x0014873C File Offset: 0x00146B3C
	public void HideDetailPanel(bool immediate)
	{
		if (this.detailPanel != null)
		{
			this.detailPanel.Hide(immediate);
		}
	}

	// Token: 0x06002E0A RID: 11786 RVA: 0x0014875B File Offset: 0x00146B5B
	public void SetTitleText(string text)
	{
		this.title.text = text;
	}

	// Token: 0x06002E0B RID: 11787 RVA: 0x00148769 File Offset: 0x00146B69
	private bool NextPageAvailable()
	{
		return this.pageStartIndex < this.itemList.items.Count - this.itemsPerPage;
	}

	// Token: 0x06002E0C RID: 11788 RVA: 0x0014878A File Offset: 0x00146B8A
	private bool PrevPageAvailable()
	{
		return this.pageStartIndex >= this.itemsPerPage;
	}

	// Token: 0x06002E0D RID: 11789 RVA: 0x0014879D File Offset: 0x00146B9D
	public void ButtonTapped_NextPage()
	{
		if (this.NextPageAvailable())
		{
			this.pageStartIndex += this.itemsPerPage;
			this.DoScroll();
		}
	}

	// Token: 0x06002E0E RID: 11790 RVA: 0x001487C3 File Offset: 0x00146BC3
	public void ButtonTapped_PreviousPage()
	{
		if (this.PrevPageAvailable())
		{
			this.pageStartIndex -= this.itemsPerPage;
			this.DoScroll();
		}
	}

	// Token: 0x06002E0F RID: 11791 RVA: 0x001487EC File Offset: 0x00146BEC
	private void DoScroll()
	{
		int num = this.pageStartIndex + this.itemsPerPage;
		for (int i = this.pageStartIndex; i <= num + 1; i++)
		{
			this.itemList.SetupPanelForItemAtIndex(i);
		}
		for (int j = 0; j < this.itemList.items.Count; j++)
		{
			bool enable = j >= this.pageStartIndex && j < num;
			this.itemList.EnableItemAtIndex(j, enable);
		}
		this.scrollAnimStartPos = this.scrollPosition;
		this.scrollAnimEndPos = (float)(-(float)this.pageStartIndex) * this.ItemWidth();
		this.scrollAnimTime = this.scrollAnimDuration;
		this.ShowPrevButton(this.PrevPageAvailable());
		this.ShowNextButton(this.NextPageAvailable());
		if (this.itemList.items.Count > this.pageStartIndex)
		{
			this.defaultSelectable = this.itemList.items[this.pageStartIndex].GetComponent<Selectable>();
		}
		else
		{
			this.defaultSelectable = null;
		}
	}

	// Token: 0x06002E10 RID: 11792 RVA: 0x001488FB File Offset: 0x00146CFB
	public void SetScrollPosition(float pos)
	{
		this.scrollPosition = pos;
		this.updatePosition = true;
	}

	// Token: 0x06002E11 RID: 11793 RVA: 0x0014890C File Offset: 0x00146D0C
	public override bool SelectItemWithID(string itemID)
	{
		List<GameObject> items = this.itemList.items;
		for (int i = 0; i < items.Count; i++)
		{
			ItemPanel component = items[i].GetComponent<ItemPanel>();
			if (component != null && component.itemId == itemID)
			{
				this.SelectItemAtIndex(i);
				return true;
			}
		}
		return false;
	}

	// Token: 0x06002E12 RID: 11794 RVA: 0x00148970 File Offset: 0x00146D70
	public override GameObject CloneItemWithID(string itemID)
	{
		GameObject gameObject = base.CloneItemWithID(itemID);
		List<GameObject> items = this.itemList.items;
		for (int i = 0; i < items.Count; i++)
		{
			ItemPanel component = items[i].GetComponent<ItemPanel>();
			if (component != null && component.itemId == itemID)
			{
				RectTransform rectTransform = (RectTransform)gameObject.transform;
				RectTransform rectTransform2;
				if (this.detailPanel != null && this.detailPanel.IsShowing() && this.detailPanel.cloneItemPosition != null)
				{
					rectTransform2 = this.detailPanel.cloneItemPosition;
				}
				else
				{
					rectTransform2 = (RectTransform)items[i].transform;
				}
				Rect rect = rectTransform2.rect;
				rectTransform.position = rectTransform2.position;
				rectTransform.sizeDelta = rect.size;
				return gameObject;
			}
		}
		return null;
	}

	// Token: 0x06002E13 RID: 11795 RVA: 0x00148A68 File Offset: 0x00146E68
	public void SelectItemAtIndex(int index)
	{
		if (this.pageStartIndex + this.itemsPerPage < index)
		{
			this.pageStartIndex = index;
			this.DoScroll();
			this.scrollAnimTime = 0f;
			this.scrollPosition = this.scrollAnimEndPos;
			float num = this.ContentWidth();
			if (num > 1f)
			{
				float x = num / 2f + this.scrollPosition;
				this.contentParent.anchoredPosition = new Vector2(x, this.contentParent.anchoredPosition.y);
				this.updatePosition = false;
			}
		}
		this.itemList.SelectItemAtIndex(index);
	}

	// Token: 0x06002E14 RID: 11796 RVA: 0x00148B04 File Offset: 0x00146F04
	public new void Update()
	{
		base.Update();
		if (!this.itemsLoaded)
		{
			return;
		}
		if (this.scrollAnimTime > 0f)
		{
			float t = this.scrollAnimCurve.Evaluate((this.scrollAnimDuration - this.scrollAnimTime) / this.scrollAnimDuration);
			this.scrollPosition = Mathf.Lerp(this.scrollAnimStartPos, this.scrollAnimEndPos, t);
			this.scrollAnimTime -= Time.deltaTime;
			if (this.scrollAnimTime <= 0f)
			{
				this.scrollPosition = this.scrollAnimEndPos;
				this.scrollAnimTime = -1f;
			}
			this.updatePosition = true;
		}
		if (this.updatePosition)
		{
			float num = this.ContentWidth();
			if (num > 1f)
			{
				float x = num / 2f + this.scrollPosition;
				this.contentParent.anchoredPosition = new Vector2(x, this.contentParent.anchoredPosition.y);
				this.updatePosition = false;
			}
		}
	}

	// Token: 0x06002E15 RID: 11797 RVA: 0x00148C02 File Offset: 0x00147002
	private float ContentWidth()
	{
		return (float)(this.itemList.items.Count + this.itemList.noDataPanels.Count) * (this.panelWidth + this.itemSpacing);
	}

	// Token: 0x06002E16 RID: 11798 RVA: 0x00148C34 File Offset: 0x00147034
	private float PageWidth()
	{
		return (float)this.itemsPerPage * (this.panelWidth + this.itemSpacing);
	}

	// Token: 0x06002E17 RID: 11799 RVA: 0x00148C4B File Offset: 0x0014704B
	private float ItemWidth()
	{
		return this.panelWidth + this.itemSpacing;
	}

	// Token: 0x06002E18 RID: 11800 RVA: 0x00148C5C File Offset: 0x0014705C
	public void ItemInListSelected(ItemPanelList list, int index, bool selectedByClick)
	{
		if (this.pagedListDelegate != null)
		{
			this.pagedListDelegate.ItemInPagedListSelected(this, index, selectedByClick);
		}
		if (this.pageStartIndex >= index)
		{
			this.pageStartIndex = index;
		}
		else if (this.pageStartIndex + this.itemsPerPage <= index)
		{
			this.pageStartIndex = index - this.itemsPerPage + 1;
		}
		this.DoScroll();
	}

	// Token: 0x06002E19 RID: 11801 RVA: 0x00148CC4 File Offset: 0x001470C4
	public Selectable FindDownSelectable(ItemPanelList list, int index)
	{
		if (this.nextElement == null)
		{
			return null;
		}
		if (!(this.nextElement is ItemPanelPagedList))
		{
			return this.nextElement.defaultSelectable;
		}
		ItemPanelPagedList itemPanelPagedList = this.nextElement as ItemPanelPagedList;
		if (itemPanelPagedList.itemList == null)
		{
			return null;
		}
		int num = index - this.pageStartIndex;
		int num2 = Mathf.Min(itemPanelPagedList.itemList.items.Count - 1, itemPanelPagedList.pageStartIndex + num);
		if (num2 >= 0)
		{
			return itemPanelPagedList.itemList.items[num2].GetComponent<Selectable>();
		}
		return null;
	}

	// Token: 0x06002E1A RID: 11802 RVA: 0x00148D6C File Offset: 0x0014716C
	public Selectable FindUpSelectable(ItemPanelList list, int index)
	{
		if (this.previousElement == null)
		{
			return null;
		}
		if (!(this.previousElement is ItemPanelPagedList))
		{
			return this.previousElement.defaultSelectable;
		}
		ItemPanelPagedList itemPanelPagedList = this.previousElement as ItemPanelPagedList;
		if (itemPanelPagedList.itemList == null)
		{
			return null;
		}
		int num = index - this.pageStartIndex;
		int num2 = Mathf.Min(itemPanelPagedList.itemList.items.Count - 1, itemPanelPagedList.pageStartIndex + num);
		if (num2 >= 0)
		{
			return itemPanelPagedList.itemList.items[num2].GetComponent<Selectable>();
		}
		return null;
	}

	// Token: 0x06002E1B RID: 11803 RVA: 0x00148E14 File Offset: 0x00147214
	public void DetailPanelClosed(DetailPanel detailPanel)
	{
		this.itemList.ClearSelection();
	}

	// Token: 0x06002E1C RID: 11804 RVA: 0x00148E24 File Offset: 0x00147224
	public override void UnloadEditorExampleContent()
	{
		if (this.detailPanel != null)
		{
			UnityEngine.Object.DestroyImmediate(this.detailPanel.gameObject);
		}
		int childCount = this.itemList.transform.childCount;
		for (int i = childCount - 1; i >= 0; i--)
		{
			GameObject gameObject = this.itemList.transform.GetChild(i).gameObject;
			if (gameObject.GetComponent<ItemPanel>() != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}
	}

	// Token: 0x06002E1D RID: 11805 RVA: 0x00148EA5 File Offset: 0x001472A5
	public void OnDestroy()
	{
		if (this.dataSource != null)
		{
			this.dataSource.RemoveListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		}
	}

	// Token: 0x04002675 RID: 9845
	public ItemPanel panelPrefab;

	// Token: 0x04002676 RID: 9846
	public DetailPanel detailPanelPrefab;

	// Token: 0x04002677 RID: 9847
	public GameObject noDataPrefab;

	// Token: 0x04002678 RID: 9848
	public RectTransform contentParent;

	// Token: 0x04002679 RID: 9849
	public CanvasGroup noContentsGroup;

	// Token: 0x0400267A RID: 9850
	public CanvasGroup titleGroup;

	// Token: 0x0400267B RID: 9851
	public CanvasGroup nextButtonGroup;

	// Token: 0x0400267C RID: 9852
	public CanvasGroup prevButtonGroup;

	// Token: 0x0400267D RID: 9853
	public bool hideIfEmpty;

	// Token: 0x0400267E RID: 9854
	private CanvasGroup contentGroup;

	// Token: 0x0400267F RID: 9855
	[HideInInspector]
	public ItemPanelList itemList;

	// Token: 0x04002680 RID: 9856
	public AnimationCurve scrollAnimCurve;

	// Token: 0x04002681 RID: 9857
	public float scrollAnimDuration = 0.5f;

	// Token: 0x04002682 RID: 9858
	public ItemPanelPagedListDelegate pagedListDelegate;

	// Token: 0x04002683 RID: 9859
	private Text title;

	// Token: 0x04002684 RID: 9860
	private int itemsPerPage = 4;

	// Token: 0x04002685 RID: 9861
	private float panelWidth = 200f;

	// Token: 0x04002686 RID: 9862
	private float itemSpacing;

	// Token: 0x04002687 RID: 9863
	private int pageStartIndex;

	// Token: 0x04002688 RID: 9864
	private DetailPanel detailPanel;

	// Token: 0x04002689 RID: 9865
	private float scrollPosition;

	// Token: 0x0400268A RID: 9866
	private bool updatePosition;

	// Token: 0x0400268B RID: 9867
	private float scrollAnimStartPos;

	// Token: 0x0400268C RID: 9868
	private float scrollAnimEndPos;

	// Token: 0x0400268D RID: 9869
	private float scrollAnimTime = -1f;

	// Token: 0x0400268E RID: 9870
	private bool initComplete;

	// Token: 0x0400268F RID: 9871
	private bool itemsLoaded;
}
