using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanelList : MonoBehaviour, ItemPanelDelegate
{
	public ItemPanel panelPrefab;

	public GameObject noDataPrefab;

	public List<string> itemIds;

	public RectTransform rootTransform;

	public DetailPanel detailView;

	public ItemPanelListDelegate listDelegate;

	public bool moveOtherChildrenToEnd;

	private UIDataSource _dataSource;

	private ImageManager _imageManager;

	public List<GameObject> noDataPanels;

	private List<GameObject> otherChildObjects;

	private Dictionary<string, ItemPanel> itemPanelsById;

	private int loadedItemIds;

	private bool contentLoaded;

	public ItemPanel selectedItem { get; private set; }

	public List<GameObject> items { get; private set; }

	private void Awake()
	{
		Init();
	}

	public void Init()
	{
		if (rootTransform == null)
		{
			rootTransform = (RectTransform)base.transform;
		}
		if (detailView != null)
		{
			detailView.Init();
		}
		selectedItem = null;
		Clear();
	}

	public void Clear()
	{
		items = new List<GameObject>();
		itemPanelsById = new Dictionary<string, ItemPanel>();
		loadedItemIds = 0;
		if (noDataPanels != null)
		{
			foreach (GameObject noDataPanel in noDataPanels)
			{
				Object.Destroy(noDataPanel);
			}
		}
		noDataPanels = new List<GameObject>();
		otherChildObjects = new List<GameObject>();
		int childCount = rootTransform.childCount;
		for (int num = childCount - 1; num >= 0; num--)
		{
			Transform child = rootTransform.GetChild(num);
			ItemPanel component = child.gameObject.GetComponent<ItemPanel>();
			if (component != null)
			{
				if (component.preserveDuringClear)
				{
					component.itemDelegate = this;
					items.Add(child.gameObject);
				}
				else
				{
					Object.Destroy(child.gameObject);
				}
			}
			else
			{
				otherChildObjects.Add(child.gameObject);
				items.Add(child.gameObject);
			}
		}
	}

	public void SetPanelPrefab(ItemPanel prefab)
	{
		panelPrefab = prefab;
	}

	public void SetNoDataPrefab(GameObject prefab)
	{
		noDataPrefab = prefab;
	}

	public void LoadContent(List<string> ids, int preload, UIDataSource dataSource, ImageManager imageManager)
	{
		_dataSource = dataSource;
		_imageManager = imageManager;
		if (rootTransform == null)
		{
			rootTransform = (RectTransform)base.transform;
		}
		Clear();
		itemIds = new List<string>(ids);
		for (int i = 0; i < Mathf.Min(preload, itemIds.Count); i++)
		{
			ItemPanel itemPanel = Expand();
			if (itemPanel == null)
			{
				break;
			}
			itemPanel.panelContents.SetupPanel(dataSource, imageManager, itemPanel.itemId);
			itemPanel.expandedPanelContents.SetupPanel(dataSource, imageManager, itemPanel.itemId);
			itemPanel.dataType = dataSource.dataType;
			itemPanel.dataSubtype = dataSource.dataSubtype;
		}
		OrganizeChildObjects();
		UpdateNoDataPanels();
		contentLoaded = true;
	}

	private void UpdateNoDataPanels()
	{
		int expectedSize = _dataSource.expectedSize;
		if (expectedSize == 0 || noDataPrefab == null)
		{
			return;
		}
		int num = 0;
		if (loadedItemIds >= itemIds.Count)
		{
			num = Mathf.Max(0, expectedSize - itemIds.Count);
		}
		if (num < noDataPanels.Count)
		{
			for (int num2 = noDataPanels.Count - 1; num2 >= num; num2--)
			{
				GameObject obj = noDataPanels[num2];
				Object.Destroy(obj);
				noDataPanels.RemoveAt(num2);
			}
		}
		else if (num > noDataPanels.Count)
		{
			for (int i = noDataPanels.Count; i <= num; i++)
			{
				GameObject gameObject = Object.Instantiate(noDataPrefab);
				noDataPanels.Add(gameObject);
				RectTransform rectTransform = (RectTransform)gameObject.transform;
				rectTransform.SetParent(rootTransform, worldPositionStays: false);
			}
		}
		foreach (GameObject noDataPanel in noDataPanels)
		{
			noDataPanel.transform.SetAsLastSibling();
		}
	}

	public void OrganizeChildObjects()
	{
		for (int i = 0; i < items.Count; i++)
		{
			items[i].transform.SetAsLastSibling();
		}
		foreach (GameObject otherChildObject in otherChildObjects)
		{
			if (moveOtherChildrenToEnd)
			{
				otherChildObject.transform.SetAsLastSibling();
			}
			else
			{
				otherChildObject.transform.SetAsFirstSibling();
			}
		}
	}

	public void RemoveItemWithId(string id)
	{
		ItemPanel value = null;
		if (itemPanelsById.TryGetValue(id, out value))
		{
			itemIds.Remove(id);
			items.Remove(value.gameObject);
			itemPanelsById.Remove(id);
			Object.Destroy(value.gameObject);
		}
	}

	public void InsertItemWithId(int index, string id, UIDataSource dataSource, ImageManager imageManager)
	{
		itemIds.Insert(index, id);
		if (index > loadedItemIds)
		{
			return;
		}
		int num = 0;
		int index2 = items.Count;
		for (int i = 0; i < items.Count; i++)
		{
			if (num == index)
			{
				index2 = num;
				break;
			}
			if (items[i].GetComponent<ItemPanel>() != null)
			{
				num++;
			}
		}
		GameObject gameObject = Object.Instantiate(panelPrefab.gameObject);
		ItemPanel component = gameObject.GetComponent<ItemPanel>();
		component.itemId = id;
		component.itemDelegate = this;
		items.Insert(index2, gameObject);
		itemPanelsById.Add(id, component);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(rootTransform, worldPositionStays: false);
		component.panelContents.SetupPanel(dataSource, imageManager, id);
		component.expandedPanelContents.SetupPanel(dataSource, imageManager, id);
		component.dataType = dataSource.dataType;
		component.dataSubtype = dataSource.dataSubtype;
		loadedItemIds++;
	}

	private ItemPanel Expand()
	{
		if (loadedItemIds >= itemIds.Count)
		{
			return null;
		}
		string text = itemIds[loadedItemIds];
		if (itemPanelsById.ContainsKey(text))
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(panelPrefab.gameObject);
		ItemPanel component = gameObject.GetComponent<ItemPanel>();
		component.itemId = text;
		component.itemDelegate = this;
		items.Add(gameObject);
		itemPanelsById.Add(text, component);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(rootTransform, worldPositionStays: false);
		loadedItemIds++;
		return component;
	}

	public void SetupPanelForItemAtIndex(int index)
	{
		for (int i = items.Count; i <= index; i++)
		{
			Expand();
		}
		OrganizeChildObjects();
		UpdateNoDataPanels();
		if (index < items.Count)
		{
			ItemPanel component = items[index].GetComponent<ItemPanel>();
			if (component != null)
			{
				component.panelContents.SetupPanel(_dataSource, _imageManager, component.itemId);
				component.expandedPanelContents.SetupPanel(_dataSource, _imageManager, component.itemId);
				component.dataType = _dataSource.dataType;
				component.dataSubtype = _dataSource.dataSubtype;
			}
		}
	}

	public GameObject CreateItemPanelWithID(string itemID)
	{
		GameObject gameObject = Object.Instantiate(panelPrefab.gameObject);
		ItemPanel component = gameObject.GetComponent<ItemPanel>();
		component.itemId = itemID;
		component.panelContents.SetupPanel(_dataSource, _imageManager, component.itemId);
		component.expandedPanelContents.SetupPanel(_dataSource, _imageManager, component.itemId);
		component.dataType = _dataSource.dataType;
		component.dataSubtype = _dataSource.dataSubtype;
		return gameObject;
	}

	private void UpdateNavigationLinksForItemAtIndex(int index)
	{
		Selectable component = items[index].GetComponent<Selectable>();
		if (!(component == null))
		{
			Navigation navigation = component.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			if (listDelegate != null)
			{
				navigation.selectOnUp = listDelegate.FindUpSelectable(this, index);
				navigation.selectOnDown = listDelegate.FindDownSelectable(this, index);
			}
			if (index > 0)
			{
				Selectable component2 = items[index - 1].GetComponent<Selectable>();
				navigation.selectOnLeft = component2;
			}
			if (index < items.Count - 1)
			{
				Selectable component3 = items[index + 1].GetComponent<Selectable>();
				navigation.selectOnRight = component3;
			}
			component.navigation = navigation;
		}
	}

	public void EnableItemAtIndex(int index, bool enable)
	{
		if (index < items.Count)
		{
			ItemPanel component = items[index].GetComponent<ItemPanel>();
			if (component != null)
			{
				component.SetDisabled(!enable);
			}
		}
	}

	public void SelectItemAtIndex(int index)
	{
		if (index < items.Count)
		{
			ItemPanel component = items[index].GetComponent<ItemPanel>();
			if (component != null)
			{
				component.DoSelect();
			}
		}
	}

	public bool IsContentLoaded()
	{
		return contentLoaded;
	}

	public float ContentWidth()
	{
		return rootTransform.sizeDelta.x;
	}

	public void ClearSelection()
	{
		if (items == null)
		{
			return;
		}
		foreach (GameObject item in items)
		{
			if (!(item == null))
			{
				ItemPanel component = item.GetComponent<ItemPanel>();
				if (component != null)
				{
					component.SetDetailMode(state: false);
					component.Deselect();
				}
			}
		}
		selectedItem = null;
	}

	public void ItemSelected(ItemPanel itemPanel, bool selectedByClick)
	{
		if (itemPanel == selectedItem)
		{
			if (detailView != null)
			{
				bool immediate = !selectedByClick;
				detailView.Show(immediate);
			}
			return;
		}
		selectedItem = itemPanel;
		for (int i = 0; i < items.Count; i++)
		{
			ItemPanel component = items[i].GetComponent<ItemPanel>();
			if (component == null)
			{
				continue;
			}
			component.SetDetailMode(state: true);
			if (component == itemPanel)
			{
				UpdateNavigationLinksForItemAtIndex(i);
				if (listDelegate != null)
				{
					listDelegate.ItemInListSelected(this, i, selectedByClick);
				}
				if (!(detailView != null))
				{
					continue;
				}
				detailView.SetOverrideContents(itemPanel.overrideDetailPanelContents);
				detailView.LoadContentForID(component.itemId, _dataSource, _imageManager);
				detailView.LinkToItemPanel(component);
				bool flag = !selectedByClick;
				detailView.Show(flag);
				UIScrollControl componentInParent = GetComponentInParent<UIScrollControl>();
				if (componentInParent != null)
				{
					if (flag)
					{
						componentInParent.SnapToTransform(rootTransform, 0.8f);
					}
					else
					{
						componentInParent.ScrollToTransform(rootTransform, 0.8f, 0.85f, 0.1f);
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
