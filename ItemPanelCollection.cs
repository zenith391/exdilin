using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200042A RID: 1066
public class ItemPanelCollection : UISceneElement, ItemPanelDelegate
{
	// Token: 0x06002DEF RID: 11759 RVA: 0x001472D4 File Offset: 0x001456D4
	public override void OnEnable()
	{
		this.contentCanvas = base.GetComponent<CanvasGroup>();
		if (this.contentCanvas != null)
		{
			this.contentCanvas.alpha = 0f;
			this.contentCanvas.interactable = false;
		}
		this.scrollable &= (this.scrollRect != null);
		if (this.scrollable)
		{
			LayoutElement layoutElement = null;
			if (this.itemPanelPrefab != null)
			{
				layoutElement = this.itemPanelPrefab.GetComponent<LayoutElement>();
			}
			else if (this.panelContentsPrefab != null)
			{
				layoutElement = this.panelContentsPrefab.GetComponent<LayoutElement>();
			}
			if (layoutElement != null)
			{
				this.scrollItemHeight = layoutElement.preferredHeight;
			}
			VerticalLayoutGroup component = this.contentRoot.GetComponent<VerticalLayoutGroup>();
			if (component != null)
			{
				this.scrollItemSpacing = component.spacing;
			}
			this.scrollItemCount = 10 + (int)((this.scrollRect.viewport.sizeDelta.y + 4f * this.edgeOffsetForScrollUpdate) / (this.scrollItemHeight + this.scrollItemSpacing));
			if (this.scrollTopSpace == null)
			{
				GameObject gameObject = new GameObject("ScrollTop", new Type[]
				{
					typeof(RectTransform)
				});
				this.scrollTop = (RectTransform)gameObject.transform;
				this.scrollTopSpace = gameObject.AddComponent<LayoutElement>();
				this.scrollTopSpace.preferredHeight = 0f;
				this.scrollTop.SetParent(this.contentRoot, false);
				this.scrollTop.SetAsFirstSibling();
			}
			if (this.scrollBottomSpace == null)
			{
				GameObject gameObject2 = new GameObject("ScrollBottom", new Type[]
				{
					typeof(RectTransform)
				});
				this.scrollBottom = (RectTransform)gameObject2.transform;
				this.scrollBottomSpace = gameObject2.AddComponent<LayoutElement>();
				this.scrollBottomSpace.preferredHeight = 0f;
				this.scrollBottom.SetParent(this.contentRoot, false);
				this.scrollBottom.SetAsLastSibling();
			}
		}
		base.OnEnable();
	}

	// Token: 0x06002DF0 RID: 11760 RVA: 0x001474F4 File Offset: 0x001458F4
	protected override void LoadContentFromDataSource()
	{
		this.Init();
		this.Clear();
		if (this.scrollable)
		{
			this.displayStart = 0;
			this.displayEnd = this.scrollItemCount;
		}
		this.dataSource.AddListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		if (this.forceReloadData)
		{
			this.dataSource.ClearData();
		}
		this.dataSource.LoadIfNeeded();
	}

	// Token: 0x06002DF1 RID: 11761 RVA: 0x00147563 File Offset: 0x00145963
	private void OnDataSourceReloaded(List<string> modifiedKeys)
	{
		if (this.loadedItems == null)
		{
			this.Clear();
			this.loadedItems = new Dictionary<string, GameObject>();
		}
		this.Layout();
	}

	// Token: 0x06002DF2 RID: 11762 RVA: 0x00147588 File Offset: 0x00145988
	private void Layout()
	{
		List<string> keys = this.dataSource.Keys;
		if (this.scrollable)
		{
			this.displayStart = Mathf.Max(0, this.displayStart);
			this.displayEnd = Mathf.Min(keys.Count - 1, this.displayStart + this.scrollItemCount);
			this.scrollTopSpace.preferredHeight = (float)this.displayStart * this.scrollItemHeight + (float)(this.displayStart - 1) * this.scrollItemSpacing;
			this.scrollBottomSpace.preferredHeight = (float)(keys.Count - this.displayEnd) * this.scrollItemHeight + (float)(keys.Count - this.displayEnd - 1) * this.scrollItemSpacing;
		}
		else
		{
			this.displayStart = 0;
			this.displayEnd = keys.Count - 1;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string item in this.loadedItems.Keys)
		{
			if (!keys.Contains(item))
			{
				hashSet.Add(item);
			}
		}
		int num = 0;
		if (this.scrollTop != null)
		{
			this.scrollTop.SetSiblingIndex(num);
			num++;
		}
		for (int i = 0; i < keys.Count; i++)
		{
			string text = keys[i];
			if (i < this.displayStart || i > this.displayEnd || hashSet.Contains(text))
			{
				GameObject obj = null;
				if (this.loadedItems.TryGetValue(text, out obj))
				{
					UnityEngine.Object.Destroy(obj);
					this.loadedItems.Remove(text);
				}
			}
			if (i >= this.displayStart && i <= this.displayEnd)
			{
				GameObject gameObject = null;
				if (!this.loadedItems.TryGetValue(text, out gameObject))
				{
					gameObject = this.CreateNewItemWithID(text);
					if (gameObject != null)
					{
						this.loadedItems.Add(text, gameObject);
						RectTransform rectTransform = (RectTransform)gameObject.transform;
						rectTransform.SetParent(this.contentRoot, false);
					}
				}
				((RectTransform)gameObject.transform).SetSiblingIndex(num);
				num++;
			}
		}
		if (this.scrollBottom != null)
		{
			this.scrollBottom.SetSiblingIndex(num);
		}
		bool flag = this.loadedItems.Count == 0;
		if (this.hideIfEmpty)
		{
			base.gameObject.SetActive(!flag);
		}
		if (this.noContentsView != null)
		{
			this.noContentsView.SetActive(flag);
		}
		if (this.contentCanvas != null && base.gameObject.activeSelf)
		{
			this.contentCanvas.alpha = 1f;
			this.contentCanvas.interactable = true;
		}
	}

	// Token: 0x06002DF3 RID: 11763 RVA: 0x00147890 File Offset: 0x00145C90
	private GameObject CreateNewItemWithID(string itemID)
	{
		if (this.dataSource.PanelOverrides != null && this.dataSource.PanelOverrides.ContainsKey(itemID))
		{
			int num = this.dataSource.PanelOverrides[itemID];
			if (num < this.panelContentsOverrides.Length)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.panelContentsOverrides[num].gameObject);
				gameObject.SetActive(true);
				UIPanelContents component = gameObject.GetComponent<UIPanelContents>();
				component.SetupPanel(this.dataSource, this.imageManager, itemID);
				return gameObject;
			}
		}
		if (this.itemPanelPrefab != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.itemPanelPrefab.gameObject);
			gameObject2.SetActive(true);
			ItemPanel component2 = gameObject2.GetComponent<ItemPanel>();
			component2.itemDelegate = this;
			component2.panelContents.SetupPanel(this.dataSource, this.imageManager, itemID);
			component2.expandedPanelContents.SetupPanel(this.dataSource, this.imageManager, itemID);
			component2.dataType = this.dataSource.dataType;
			component2.dataSubtype = this.dataSource.dataSubtype;
			return gameObject2;
		}
		if (this.panelContentsPrefab != null)
		{
			GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(this.panelContentsPrefab.gameObject);
			gameObject3.SetActive(true);
			UIPanelContents component3 = gameObject3.GetComponent<UIPanelContents>();
			component3.SetupPanel(this.dataSource, this.imageManager, itemID);
			return gameObject3;
		}
		return null;
	}

	// Token: 0x06002DF4 RID: 11764 RVA: 0x001479F8 File Offset: 0x00145DF8
	public override GameObject CloneItemWithID(string itemID)
	{
		GameObject gameObject = base.CloneItemWithID(itemID);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		GameObject gameObject2 = null;
		if (this.loadedItems.TryGetValue(itemID, out gameObject2))
		{
			RectTransform rectTransform2 = (RectTransform)gameObject2.transform;
			Rect rect = rectTransform2.rect;
			rectTransform.position = rectTransform2.position;
			rectTransform.sizeDelta = rect.size;
		}
		return gameObject;
	}

	// Token: 0x06002DF5 RID: 11765 RVA: 0x00147A5C File Offset: 0x00145E5C
	public override bool SelectItemWithID(string itemID)
	{
		foreach (KeyValuePair<string, GameObject> keyValuePair in this.loadedItems)
		{
			if (keyValuePair.Key == itemID)
			{
				GameObject value = keyValuePair.Value;
				ItemPanel component = value.GetComponent<ItemPanel>();
				if (component != null)
				{
					component.DoSelect();
				}
				else
				{
					UIScrollControl componentInParent = base.GetComponentInParent<UIScrollControl>();
					if (componentInParent != null)
					{
						componentInParent.SnapToTransform((RectTransform)value.transform, 0.8f);
					}
				}
				return true;
			}
		}
		return false;
	}

	// Token: 0x06002DF6 RID: 11766 RVA: 0x00147B24 File Offset: 0x00145F24
	public void ItemSelected(ItemPanel itemPanel, bool selectedByClick)
	{
		foreach (GameObject gameObject in this.loadedItems.Values)
		{
			ItemPanel component = gameObject.GetComponent<ItemPanel>();
			if (!(component == null))
			{
				component.SetDetailMode(true);
				if (component == itemPanel)
				{
					bool flag = !selectedByClick;
					UIScrollControl componentInParent = base.GetComponentInParent<UIScrollControl>();
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
	}

	// Token: 0x06002DF7 RID: 11767 RVA: 0x00147C14 File Offset: 0x00146014
	public override void UnloadContent()
	{
		if (this.dataSource != null)
		{
			this.dataSource.RemoveListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		}
		this.Clear();
	}

	// Token: 0x06002DF8 RID: 11768 RVA: 0x00147C40 File Offset: 0x00146040
	private void Clear()
	{
		if (this.itemPanelPrefab != null && this.itemPanelPrefab.gameObject.scene.name != null)
		{
			this.itemPanelPrefab.gameObject.SetActive(false);
		}
		if (this.panelContentsPrefab != null && this.panelContentsPrefab.gameObject.scene.name != null)
		{
			this.panelContentsPrefab.gameObject.SetActive(false);
		}
		int childCount = this.contentRoot.childCount;
		for (int i = childCount - 1; i >= 0; i--)
		{
			GameObject gameObject = this.contentRoot.GetChild(i).gameObject;
			ItemPanel component = gameObject.GetComponent<ItemPanel>();
			if (component != null && component != this.itemPanelPrefab)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			UIPanelContents component2 = gameObject.GetComponent<UIPanelContents>();
			if (component2 != null && component2 != this.panelContentsPrefab)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		if (this.loadedItems != null)
		{
			this.loadedItems.Clear();
		}
		if (this.contentCanvas != null)
		{
			this.contentCanvas.alpha = 0f;
			this.contentCanvas.interactable = false;
		}
	}

	// Token: 0x06002DF9 RID: 11769 RVA: 0x00147D9F File Offset: 0x0014619F
	private void OnDestroy()
	{
		if (this.dataSource != null)
		{
			this.dataSource.RemoveListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		}
	}

	// Token: 0x06002DFA RID: 11770 RVA: 0x00147DC4 File Offset: 0x001461C4
	public new void Update()
	{
		base.Update();
		if (!this.scrollable)
		{
			return;
		}
		if (this.loadedItems == null || this.loadedItems.Count == 0)
		{
			return;
		}
		if (this.dataSource == null || !this.dataSource.IsDataLoaded())
		{
			return;
		}
		if (this.dataSource.Keys.Count <= this.displayStart)
		{
			return;
		}
		if (Time.time - this.lastDisplayRangeChangeTime < 0.1f)
		{
			return;
		}
		string key = this.dataSource.Keys[this.displayStart];
		string key2 = this.dataSource.Keys[this.displayEnd];
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		if (this.displayStart > 0 && this.loadedItems.TryGetValue(key, out gameObject2))
		{
			float num = this.scrollRect.viewport.position.y - gameObject2.transform.position.y;
			if (num > -this.edgeOffsetForScrollUpdate)
			{
				int num2 = 6 + (int)((num + this.edgeOffsetForScrollUpdate) / (this.scrollItemHeight + this.scrollItemSpacing));
				num2 = Mathf.Max(2, num2);
				this.displayStart -= num2;
				if (this.displayStart < 0)
				{
					this.displayStart = 0;
				}
				this.Layout();
				Canvas.ForceUpdateCanvases();
				return;
			}
		}
		if (this.displayEnd < this.dataSource.Keys.Count && this.displayEnd > 0 && this.scrollRect.viewport.sizeDelta.y > 10f && this.loadedItems.TryGetValue(key2, out gameObject))
		{
			float num3 = this.scrollRect.viewport.position.y - gameObject.transform.position.y;
			if (num3 < this.scrollRect.viewport.sizeDelta.y + this.edgeOffsetForScrollUpdate)
			{
				int num4 = 6 + (int)((this.scrollRect.viewport.sizeDelta.y + this.edgeOffsetForScrollUpdate - num3) / (this.scrollItemHeight + this.scrollItemSpacing));
				num4 = Mathf.Max(2, num4);
				this.displayStart += num4;
				if (this.displayStart + this.scrollItemCount >= this.dataSource.Keys.Count)
				{
					this.displayStart = this.dataSource.Keys.Count - 1 - this.scrollItemCount;
				}
				this.lastDisplayRangeChangeTime = Time.time;
				this.Layout();
				Canvas.ForceUpdateCanvases();
				return;
			}
		}
	}

	// Token: 0x04002660 RID: 9824
	public RectTransform contentRoot;

	// Token: 0x04002661 RID: 9825
	public ItemPanel itemPanelPrefab;

	// Token: 0x04002662 RID: 9826
	public UIPanelContents panelContentsPrefab;

	// Token: 0x04002663 RID: 9827
	public GameObject noContentsView;

	// Token: 0x04002664 RID: 9828
	public UIPanelContents[] panelContentsOverrides;

	// Token: 0x04002665 RID: 9829
	public bool scrollable;

	// Token: 0x04002666 RID: 9830
	public ScrollRect scrollRect;

	// Token: 0x04002667 RID: 9831
	private int scrollItemCount = 36;

	// Token: 0x04002668 RID: 9832
	private float scrollItemHeight = 60f;

	// Token: 0x04002669 RID: 9833
	private float scrollItemSpacing;

	// Token: 0x0400266A RID: 9834
	private int displayStart;

	// Token: 0x0400266B RID: 9835
	private int displayEnd;

	// Token: 0x0400266C RID: 9836
	private RectTransform scrollTop;

	// Token: 0x0400266D RID: 9837
	private RectTransform scrollBottom;

	// Token: 0x0400266E RID: 9838
	private LayoutElement scrollTopSpace;

	// Token: 0x0400266F RID: 9839
	private LayoutElement scrollBottomSpace;

	// Token: 0x04002670 RID: 9840
	private float edgeOffsetForScrollUpdate = 500f;

	// Token: 0x04002671 RID: 9841
	private float lastDisplayRangeChangeTime;

	// Token: 0x04002672 RID: 9842
	public bool hideIfEmpty;

	// Token: 0x04002673 RID: 9843
	private Dictionary<string, GameObject> loadedItems;

	// Token: 0x04002674 RID: 9844
	private CanvasGroup contentCanvas;
}
