using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanelCollection : UISceneElement, ItemPanelDelegate
{
	public RectTransform contentRoot;

	public ItemPanel itemPanelPrefab;

	public UIPanelContents panelContentsPrefab;

	public GameObject noContentsView;

	public UIPanelContents[] panelContentsOverrides;

	public bool scrollable;

	public ScrollRect scrollRect;

	private int scrollItemCount = 36;

	private float scrollItemHeight = 60f;

	private float scrollItemSpacing;

	private int displayStart;

	private int displayEnd;

	private RectTransform scrollTop;

	private RectTransform scrollBottom;

	private LayoutElement scrollTopSpace;

	private LayoutElement scrollBottomSpace;

	private float edgeOffsetForScrollUpdate = 500f;

	private float lastDisplayRangeChangeTime;

	public bool hideIfEmpty;

	private Dictionary<string, GameObject> loadedItems;

	private CanvasGroup contentCanvas;

	public override void OnEnable()
	{
		contentCanvas = GetComponent<CanvasGroup>();
		if (contentCanvas != null)
		{
			contentCanvas.alpha = 0f;
			contentCanvas.interactable = false;
		}
		scrollable &= scrollRect != null;
		if (scrollable)
		{
			LayoutElement layoutElement = null;
			if (itemPanelPrefab != null)
			{
				layoutElement = itemPanelPrefab.GetComponent<LayoutElement>();
			}
			else if (panelContentsPrefab != null)
			{
				layoutElement = panelContentsPrefab.GetComponent<LayoutElement>();
			}
			if (layoutElement != null)
			{
				scrollItemHeight = layoutElement.preferredHeight;
			}
			VerticalLayoutGroup component = contentRoot.GetComponent<VerticalLayoutGroup>();
			if (component != null)
			{
				scrollItemSpacing = component.spacing;
			}
			scrollItemCount = 10 + (int)((scrollRect.viewport.sizeDelta.y + 4f * edgeOffsetForScrollUpdate) / (scrollItemHeight + scrollItemSpacing));
			if (scrollTopSpace == null)
			{
				GameObject gameObject = new GameObject("ScrollTop", typeof(RectTransform));
				scrollTop = (RectTransform)gameObject.transform;
				scrollTopSpace = gameObject.AddComponent<LayoutElement>();
				scrollTopSpace.preferredHeight = 0f;
				scrollTop.SetParent(contentRoot, worldPositionStays: false);
				scrollTop.SetAsFirstSibling();
			}
			if (scrollBottomSpace == null)
			{
				GameObject gameObject2 = new GameObject("ScrollBottom", typeof(RectTransform));
				scrollBottom = (RectTransform)gameObject2.transform;
				scrollBottomSpace = gameObject2.AddComponent<LayoutElement>();
				scrollBottomSpace.preferredHeight = 0f;
				scrollBottom.SetParent(contentRoot, worldPositionStays: false);
				scrollBottom.SetAsLastSibling();
			}
		}
		base.OnEnable();
	}

	protected override void LoadContentFromDataSource()
	{
		Init();
		Clear();
		if (scrollable)
		{
			displayStart = 0;
			displayEnd = scrollItemCount;
		}
		dataSource.AddListener(OnDataSourceReloaded);
		if (forceReloadData)
		{
			dataSource.ClearData();
		}
		dataSource.LoadIfNeeded();
	}

	private void OnDataSourceReloaded(List<string> modifiedKeys)
	{
		if (loadedItems == null)
		{
			Clear();
			loadedItems = new Dictionary<string, GameObject>();
		}
		Layout();
	}

	private void Layout()
	{
		List<string> keys = dataSource.Keys;
		if (scrollable)
		{
			displayStart = Mathf.Max(0, displayStart);
			displayEnd = Mathf.Min(keys.Count - 1, displayStart + scrollItemCount);
			scrollTopSpace.preferredHeight = (float)displayStart * scrollItemHeight + (float)(displayStart - 1) * scrollItemSpacing;
			scrollBottomSpace.preferredHeight = (float)(keys.Count - displayEnd) * scrollItemHeight + (float)(keys.Count - displayEnd - 1) * scrollItemSpacing;
		}
		else
		{
			displayStart = 0;
			displayEnd = keys.Count - 1;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string key in loadedItems.Keys)
		{
			if (!keys.Contains(key))
			{
				hashSet.Add(key);
			}
		}
		int num = 0;
		if (scrollTop != null)
		{
			scrollTop.SetSiblingIndex(num);
			num++;
		}
		for (int i = 0; i < keys.Count; i++)
		{
			string text = keys[i];
			if (i < displayStart || i > displayEnd || hashSet.Contains(text))
			{
				GameObject value = null;
				if (loadedItems.TryGetValue(text, out value))
				{
					Object.Destroy(value);
					loadedItems.Remove(text);
				}
			}
			if (i < displayStart || i > displayEnd)
			{
				continue;
			}
			GameObject value2 = null;
			if (!loadedItems.TryGetValue(text, out value2))
			{
				value2 = CreateNewItemWithID(text);
				if (value2 != null)
				{
					loadedItems.Add(text, value2);
					RectTransform rectTransform = (RectTransform)value2.transform;
					rectTransform.SetParent(contentRoot, worldPositionStays: false);
				}
			}
			((RectTransform)value2.transform).SetSiblingIndex(num);
			num++;
		}
		if (scrollBottom != null)
		{
			scrollBottom.SetSiblingIndex(num);
		}
		bool flag = loadedItems.Count == 0;
		if (hideIfEmpty)
		{
			base.gameObject.SetActive(!flag);
		}
		if (noContentsView != null)
		{
			noContentsView.SetActive(flag);
		}
		if (contentCanvas != null && base.gameObject.activeSelf)
		{
			contentCanvas.alpha = 1f;
			contentCanvas.interactable = true;
		}
	}

	private GameObject CreateNewItemWithID(string itemID)
	{
		if (dataSource.PanelOverrides != null && dataSource.PanelOverrides.ContainsKey(itemID))
		{
			int num = dataSource.PanelOverrides[itemID];
			if (num < panelContentsOverrides.Length)
			{
				GameObject gameObject = Object.Instantiate(panelContentsOverrides[num].gameObject);
				gameObject.SetActive(value: true);
				UIPanelContents component = gameObject.GetComponent<UIPanelContents>();
				component.SetupPanel(dataSource, imageManager, itemID);
				return gameObject;
			}
		}
		if (itemPanelPrefab != null)
		{
			GameObject gameObject2 = Object.Instantiate(itemPanelPrefab.gameObject);
			gameObject2.SetActive(value: true);
			ItemPanel component2 = gameObject2.GetComponent<ItemPanel>();
			component2.itemDelegate = this;
			component2.panelContents.SetupPanel(dataSource, imageManager, itemID);
			component2.expandedPanelContents.SetupPanel(dataSource, imageManager, itemID);
			component2.dataType = dataSource.dataType;
			component2.dataSubtype = dataSource.dataSubtype;
			return gameObject2;
		}
		if (panelContentsPrefab != null)
		{
			GameObject gameObject3 = Object.Instantiate(panelContentsPrefab.gameObject);
			gameObject3.SetActive(value: true);
			UIPanelContents component3 = gameObject3.GetComponent<UIPanelContents>();
			component3.SetupPanel(dataSource, imageManager, itemID);
			return gameObject3;
		}
		return null;
	}

	public override GameObject CloneItemWithID(string itemID)
	{
		GameObject gameObject = base.CloneItemWithID(itemID);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		GameObject value = null;
		if (loadedItems.TryGetValue(itemID, out value))
		{
			RectTransform rectTransform2 = (RectTransform)value.transform;
			Rect rect = rectTransform2.rect;
			rectTransform.position = rectTransform2.position;
			rectTransform.sizeDelta = rect.size;
		}
		return gameObject;
	}

	public override bool SelectItemWithID(string itemID)
	{
		foreach (KeyValuePair<string, GameObject> loadedItem in loadedItems)
		{
			if (!(loadedItem.Key == itemID))
			{
				continue;
			}
			GameObject value = loadedItem.Value;
			ItemPanel component = value.GetComponent<ItemPanel>();
			if (component != null)
			{
				component.DoSelect();
			}
			else
			{
				UIScrollControl componentInParent = GetComponentInParent<UIScrollControl>();
				if (componentInParent != null)
				{
					componentInParent.SnapToTransform((RectTransform)value.transform, 0.8f);
				}
			}
			return true;
		}
		return false;
	}

	public void ItemSelected(ItemPanel itemPanel, bool selectedByClick)
	{
		foreach (GameObject value in loadedItems.Values)
		{
			ItemPanel component = value.GetComponent<ItemPanel>();
			if (component == null)
			{
				continue;
			}
			component.SetDetailMode(state: true);
			if (component == itemPanel)
			{
				bool flag = !selectedByClick;
				UIScrollControl componentInParent = GetComponentInParent<UIScrollControl>();
				if (componentInParent != null)
				{
					if (flag)
					{
						componentInParent.SnapToTransform((RectTransform)component.transform, 0.8f);
					}
					else
					{
						componentInParent.ScrollToTransform((RectTransform)component.transform, 0.8f, 0.85f, 0.1f);
					}
				}
			}
			else
			{
				component.Deselect();
			}
		}
	}

	public override void UnloadContent()
	{
		if (dataSource != null)
		{
			dataSource.RemoveListener(OnDataSourceReloaded);
		}
		Clear();
	}

	private void Clear()
	{
		if (itemPanelPrefab != null && itemPanelPrefab.gameObject.scene.name != null)
		{
			itemPanelPrefab.gameObject.SetActive(value: false);
		}
		if (panelContentsPrefab != null && panelContentsPrefab.gameObject.scene.name != null)
		{
			panelContentsPrefab.gameObject.SetActive(value: false);
		}
		int childCount = contentRoot.childCount;
		for (int num = childCount - 1; num >= 0; num--)
		{
			GameObject gameObject = contentRoot.GetChild(num).gameObject;
			ItemPanel component = gameObject.GetComponent<ItemPanel>();
			if (component != null && component != itemPanelPrefab)
			{
				Object.Destroy(gameObject);
			}
			UIPanelContents component2 = gameObject.GetComponent<UIPanelContents>();
			if (component2 != null && component2 != panelContentsPrefab)
			{
				Object.Destroy(gameObject);
			}
		}
		if (loadedItems != null)
		{
			loadedItems.Clear();
		}
		if (contentCanvas != null)
		{
			contentCanvas.alpha = 0f;
			contentCanvas.interactable = false;
		}
	}

	private void OnDestroy()
	{
		if (dataSource != null)
		{
			dataSource.RemoveListener(OnDataSourceReloaded);
		}
	}

	public new void Update()
	{
		base.Update();
		if (!scrollable || loadedItems == null || loadedItems.Count == 0 || dataSource == null || !dataSource.IsDataLoaded() || dataSource.Keys.Count <= displayStart || Time.time - lastDisplayRangeChangeTime < 0.1f)
		{
			return;
		}
		string key = dataSource.Keys[displayStart];
		string key2 = dataSource.Keys[displayEnd];
		GameObject value = null;
		GameObject value2 = null;
		if (displayStart > 0 && loadedItems.TryGetValue(key, out value2))
		{
			float num = scrollRect.viewport.position.y - value2.transform.position.y;
			if (num > 0f - edgeOffsetForScrollUpdate)
			{
				int b = 6 + (int)((num + edgeOffsetForScrollUpdate) / (scrollItemHeight + scrollItemSpacing));
				b = Mathf.Max(2, b);
				displayStart -= b;
				if (displayStart < 0)
				{
					displayStart = 0;
				}
				Layout();
				Canvas.ForceUpdateCanvases();
				return;
			}
		}
		if (displayEnd >= dataSource.Keys.Count || displayEnd <= 0 || !(scrollRect.viewport.sizeDelta.y > 10f) || !loadedItems.TryGetValue(key2, out value))
		{
			return;
		}
		float num2 = scrollRect.viewport.position.y - value.transform.position.y;
		if (num2 < scrollRect.viewport.sizeDelta.y + edgeOffsetForScrollUpdate)
		{
			int b2 = 6 + (int)((scrollRect.viewport.sizeDelta.y + edgeOffsetForScrollUpdate - num2) / (scrollItemHeight + scrollItemSpacing));
			b2 = Mathf.Max(2, b2);
			displayStart += b2;
			if (displayStart + scrollItemCount >= dataSource.Keys.Count)
			{
				displayStart = dataSource.Keys.Count - 1 - scrollItemCount;
			}
			lastDisplayRangeChangeTime = Time.time;
			Layout();
			Canvas.ForceUpdateCanvases();
		}
	}
}
