using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000405 RID: 1029
public class ItemPanelList : MonoBehaviour, ItemPanelDelegate
{
	// Token: 0x17000244 RID: 580
	// (get) Token: 0x06002CF8 RID: 11512 RVA: 0x00141841 File Offset: 0x0013FC41
	// (set) Token: 0x06002CF9 RID: 11513 RVA: 0x00141849 File Offset: 0x0013FC49
	public ItemPanel selectedItem { get; private set; }

	// Token: 0x17000245 RID: 581
	// (get) Token: 0x06002CFA RID: 11514 RVA: 0x00141852 File Offset: 0x0013FC52
	// (set) Token: 0x06002CFB RID: 11515 RVA: 0x0014185A File Offset: 0x0013FC5A
	public List<GameObject> items { get; private set; }

	// Token: 0x06002CFC RID: 11516 RVA: 0x00141863 File Offset: 0x0013FC63
	private void Awake()
	{
		this.Init();
	}

	// Token: 0x06002CFD RID: 11517 RVA: 0x0014186C File Offset: 0x0013FC6C
	public void Init()
	{
		if (this.rootTransform == null)
		{
			this.rootTransform = (RectTransform)base.transform;
		}
		if (this.detailView != null)
		{
			this.detailView.Init();
		}
		this.selectedItem = null;
		this.Clear();
	}

	// Token: 0x06002CFE RID: 11518 RVA: 0x001418C4 File Offset: 0x0013FCC4
	public void Clear()
	{
		this.items = new List<GameObject>();
		this.itemPanelsById = new Dictionary<string, ItemPanel>();
		this.loadedItemIds = 0;
		if (this.noDataPanels != null)
		{
			foreach (GameObject obj in this.noDataPanels)
			{
				UnityEngine.Object.Destroy(obj);
			}
		}
		this.noDataPanels = new List<GameObject>();
		this.otherChildObjects = new List<GameObject>();
		int childCount = this.rootTransform.childCount;
		for (int i = childCount - 1; i >= 0; i--)
		{
			Transform child = this.rootTransform.GetChild(i);
			ItemPanel component = child.gameObject.GetComponent<ItemPanel>();
			if (component != null)
			{
				if (component.preserveDuringClear)
				{
					component.itemDelegate = this;
					this.items.Add(child.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(child.gameObject);
				}
			}
			else
			{
				this.otherChildObjects.Add(child.gameObject);
				this.items.Add(child.gameObject);
			}
		}
	}

	// Token: 0x06002CFF RID: 11519 RVA: 0x00141A08 File Offset: 0x0013FE08
	public void SetPanelPrefab(ItemPanel prefab)
	{
		this.panelPrefab = prefab;
	}

	// Token: 0x06002D00 RID: 11520 RVA: 0x00141A11 File Offset: 0x0013FE11
	public void SetNoDataPrefab(GameObject prefab)
	{
		this.noDataPrefab = prefab;
	}

	// Token: 0x06002D01 RID: 11521 RVA: 0x00141A1C File Offset: 0x0013FE1C
	public void LoadContent(List<string> ids, int preload, UIDataSource dataSource, ImageManager imageManager)
	{
		this._dataSource = dataSource;
		this._imageManager = imageManager;
		if (this.rootTransform == null)
		{
			this.rootTransform = (RectTransform)base.transform;
		}
		this.Clear();
		this.itemIds = new List<string>(ids);
		for (int i = 0; i < Mathf.Min(preload, this.itemIds.Count); i++)
		{
			ItemPanel itemPanel = this.Expand();
			if (itemPanel == null)
			{
				break;
			}
			itemPanel.panelContents.SetupPanel(dataSource, imageManager, itemPanel.itemId);
			itemPanel.expandedPanelContents.SetupPanel(dataSource, imageManager, itemPanel.itemId);
			itemPanel.dataType = dataSource.dataType;
			itemPanel.dataSubtype = dataSource.dataSubtype;
		}
		this.OrganizeChildObjects();
		this.UpdateNoDataPanels();
		this.contentLoaded = true;
	}

	// Token: 0x06002D02 RID: 11522 RVA: 0x00141AFC File Offset: 0x0013FEFC
	private void UpdateNoDataPanels()
	{
		int expectedSize = this._dataSource.expectedSize;
		if (expectedSize == 0)
		{
			return;
		}
		if (this.noDataPrefab == null)
		{
			return;
		}
		int num = 0;
		if (this.loadedItemIds >= this.itemIds.Count)
		{
			num = Mathf.Max(0, expectedSize - this.itemIds.Count);
		}
		if (num < this.noDataPanels.Count)
		{
			for (int i = this.noDataPanels.Count - 1; i >= num; i--)
			{
				GameObject obj = this.noDataPanels[i];
				UnityEngine.Object.Destroy(obj);
				this.noDataPanels.RemoveAt(i);
			}
		}
		else if (num > this.noDataPanels.Count)
		{
			for (int j = this.noDataPanels.Count; j <= num; j++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.noDataPrefab);
				this.noDataPanels.Add(gameObject);
				RectTransform rectTransform = (RectTransform)gameObject.transform;
				rectTransform.SetParent(this.rootTransform, false);
			}
		}
		foreach (GameObject gameObject2 in this.noDataPanels)
		{
			gameObject2.transform.SetAsLastSibling();
		}
	}

	// Token: 0x06002D03 RID: 11523 RVA: 0x00141C6C File Offset: 0x0014006C
	public void OrganizeChildObjects()
	{
		for (int i = 0; i < this.items.Count; i++)
		{
			this.items[i].transform.SetAsLastSibling();
		}
		foreach (GameObject gameObject in this.otherChildObjects)
		{
			if (this.moveOtherChildrenToEnd)
			{
				gameObject.transform.SetAsLastSibling();
			}
			else
			{
				gameObject.transform.SetAsFirstSibling();
			}
		}
	}

	// Token: 0x06002D04 RID: 11524 RVA: 0x00141D1C File Offset: 0x0014011C
	public void RemoveItemWithId(string id)
	{
		ItemPanel itemPanel = null;
		if (this.itemPanelsById.TryGetValue(id, out itemPanel))
		{
			this.itemIds.Remove(id);
			this.items.Remove(itemPanel.gameObject);
			this.itemPanelsById.Remove(id);
			UnityEngine.Object.Destroy(itemPanel.gameObject);
		}
	}

	// Token: 0x06002D05 RID: 11525 RVA: 0x00141D78 File Offset: 0x00140178
	public void InsertItemWithId(int index, string id, UIDataSource dataSource, ImageManager imageManager)
	{
		this.itemIds.Insert(index, id);
		if (index <= this.loadedItemIds)
		{
			int num = 0;
			int index2 = this.items.Count;
			for (int i = 0; i < this.items.Count; i++)
			{
				if (num == index)
				{
					index2 = num;
					break;
				}
				if (this.items[i].GetComponent<ItemPanel>() != null)
				{
					num++;
				}
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.panelPrefab.gameObject);
			ItemPanel component = gameObject.GetComponent<ItemPanel>();
			component.itemId = id;
			component.itemDelegate = this;
			this.items.Insert(index2, gameObject);
			this.itemPanelsById.Add(id, component);
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			rectTransform.SetParent(this.rootTransform, false);
			component.panelContents.SetupPanel(dataSource, imageManager, id);
			component.expandedPanelContents.SetupPanel(dataSource, imageManager, id);
			component.dataType = dataSource.dataType;
			component.dataSubtype = dataSource.dataSubtype;
			this.loadedItemIds++;
		}
	}

	// Token: 0x06002D06 RID: 11526 RVA: 0x00141EA0 File Offset: 0x001402A0
	private ItemPanel Expand()
	{
		if (this.loadedItemIds >= this.itemIds.Count)
		{
			return null;
		}
		string text = this.itemIds[this.loadedItemIds];
		if (this.itemPanelsById.ContainsKey(text))
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.panelPrefab.gameObject);
		ItemPanel component = gameObject.GetComponent<ItemPanel>();
		component.itemId = text;
		component.itemDelegate = this;
		this.items.Add(gameObject);
		this.itemPanelsById.Add(text, component);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(this.rootTransform, false);
		this.loadedItemIds++;
		return component;
	}

	// Token: 0x06002D07 RID: 11527 RVA: 0x00141F54 File Offset: 0x00140354
	public void SetupPanelForItemAtIndex(int index)
	{
		for (int i = this.items.Count; i <= index; i++)
		{
			this.Expand();
		}
		this.OrganizeChildObjects();
		this.UpdateNoDataPanels();
		if (index >= this.items.Count)
		{
			return;
		}
		ItemPanel component = this.items[index].GetComponent<ItemPanel>();
		if (component != null)
		{
			component.panelContents.SetupPanel(this._dataSource, this._imageManager, component.itemId);
			component.expandedPanelContents.SetupPanel(this._dataSource, this._imageManager, component.itemId);
			component.dataType = this._dataSource.dataType;
			component.dataSubtype = this._dataSource.dataSubtype;
		}
	}

	// Token: 0x06002D08 RID: 11528 RVA: 0x0014201C File Offset: 0x0014041C
	public GameObject CreateItemPanelWithID(string itemID)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.panelPrefab.gameObject);
		ItemPanel component = gameObject.GetComponent<ItemPanel>();
		component.itemId = itemID;
		component.panelContents.SetupPanel(this._dataSource, this._imageManager, component.itemId);
		component.expandedPanelContents.SetupPanel(this._dataSource, this._imageManager, component.itemId);
		component.dataType = this._dataSource.dataType;
		component.dataSubtype = this._dataSource.dataSubtype;
		return gameObject;
	}

	// Token: 0x06002D09 RID: 11529 RVA: 0x001420A8 File Offset: 0x001404A8
	private void UpdateNavigationLinksForItemAtIndex(int index)
	{
		Selectable component = this.items[index].GetComponent<Selectable>();
		if (component == null)
		{
			return;
		}
		Navigation navigation = component.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		if (this.listDelegate != null)
		{
			navigation.selectOnUp = this.listDelegate.FindUpSelectable(this, index);
			navigation.selectOnDown = this.listDelegate.FindDownSelectable(this, index);
		}
		if (index > 0)
		{
			Selectable component2 = this.items[index - 1].GetComponent<Selectable>();
			navigation.selectOnLeft = component2;
		}
		if (index < this.items.Count - 1)
		{
			Selectable component3 = this.items[index + 1].GetComponent<Selectable>();
			navigation.selectOnRight = component3;
		}
		component.navigation = navigation;
	}

	// Token: 0x06002D0A RID: 11530 RVA: 0x00142170 File Offset: 0x00140570
	public void EnableItemAtIndex(int index, bool enable)
	{
		if (index >= this.items.Count)
		{
			return;
		}
		ItemPanel component = this.items[index].GetComponent<ItemPanel>();
		if (component != null)
		{
			component.SetDisabled(!enable);
		}
	}

	// Token: 0x06002D0B RID: 11531 RVA: 0x001421B8 File Offset: 0x001405B8
	public void SelectItemAtIndex(int index)
	{
		if (index >= this.items.Count)
		{
			return;
		}
		ItemPanel component = this.items[index].GetComponent<ItemPanel>();
		if (component != null)
		{
			component.DoSelect();
		}
	}

	// Token: 0x06002D0C RID: 11532 RVA: 0x001421FB File Offset: 0x001405FB
	public bool IsContentLoaded()
	{
		return this.contentLoaded;
	}

	// Token: 0x06002D0D RID: 11533 RVA: 0x00142204 File Offset: 0x00140604
	public float ContentWidth()
	{
		return this.rootTransform.sizeDelta.x;
	}

	// Token: 0x06002D0E RID: 11534 RVA: 0x00142224 File Offset: 0x00140624
	public void ClearSelection()
	{
		if (this.items == null)
		{
			return;
		}
		foreach (GameObject gameObject in this.items)
		{
			if (!(gameObject == null))
			{
				ItemPanel component = gameObject.GetComponent<ItemPanel>();
				if (component != null)
				{
					component.SetDetailMode(false);
					component.Deselect();
				}
			}
		}
		this.selectedItem = null;
	}

	// Token: 0x06002D0F RID: 11535 RVA: 0x001422C0 File Offset: 0x001406C0
	public void ItemSelected(ItemPanel itemPanel, bool selectedByClick)
	{
		if (itemPanel == this.selectedItem)
		{
			if (this.detailView != null)
			{
				bool immediate = !selectedByClick;
				this.detailView.Show(immediate);
			}
			return;
		}
		this.selectedItem = itemPanel;
		for (int i = 0; i < this.items.Count; i++)
		{
			ItemPanel component = this.items[i].GetComponent<ItemPanel>();
			if (!(component == null))
			{
				component.SetDetailMode(true);
				if (component == itemPanel)
				{
					this.UpdateNavigationLinksForItemAtIndex(i);
					if (this.listDelegate != null)
					{
						this.listDelegate.ItemInListSelected(this, i, selectedByClick);
					}
					if (this.detailView != null)
					{
						this.detailView.SetOverrideContents(itemPanel.overrideDetailPanelContents);
						this.detailView.LoadContentForID(component.itemId, this._dataSource, this._imageManager);
						this.detailView.LinkToItemPanel(component);
						bool flag = !selectedByClick;
						this.detailView.Show(flag);
						UIScrollControl componentInParent = base.GetComponentInParent<UIScrollControl>();
						if (componentInParent != null)
						{
							if (flag)
							{
								componentInParent.SnapToTransform(this.rootTransform, 0.8f);
							}
							else
							{
								componentInParent.ScrollToTransform(this.rootTransform, 0.8f, 0.85f, 0.1f);
							}
						}
					}
				}
				else
				{
					component.Deselect();
				}
			}
		}
	}

	// Token: 0x04002585 RID: 9605
	public ItemPanel panelPrefab;

	// Token: 0x04002586 RID: 9606
	public GameObject noDataPrefab;

	// Token: 0x04002587 RID: 9607
	public List<string> itemIds;

	// Token: 0x04002588 RID: 9608
	public RectTransform rootTransform;

	// Token: 0x04002589 RID: 9609
	public DetailPanel detailView;

	// Token: 0x0400258B RID: 9611
	public ItemPanelListDelegate listDelegate;

	// Token: 0x0400258C RID: 9612
	public bool moveOtherChildrenToEnd;

	// Token: 0x0400258D RID: 9613
	private UIDataSource _dataSource;

	// Token: 0x0400258E RID: 9614
	private ImageManager _imageManager;

	// Token: 0x04002590 RID: 9616
	public List<GameObject> noDataPanels;

	// Token: 0x04002591 RID: 9617
	private List<GameObject> otherChildObjects;

	// Token: 0x04002592 RID: 9618
	private Dictionary<string, ItemPanel> itemPanelsById;

	// Token: 0x04002593 RID: 9619
	private int loadedItemIds;

	// Token: 0x04002594 RID: 9620
	private bool contentLoaded;
}
