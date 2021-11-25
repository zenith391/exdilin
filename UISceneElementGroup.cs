using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000430 RID: 1072
public class UISceneElementGroup : UISceneElement
{
	// Token: 0x06002E37 RID: 11831 RVA: 0x0014943C File Offset: 0x0014783C
	protected override void LoadContentFromDataSource()
	{
		if (!this.layoutComplete)
		{
			this.Layout();
		}
	}

	// Token: 0x06002E38 RID: 11832 RVA: 0x00149450 File Offset: 0x00147850
	private void Awake()
	{
		if (this.childElementPrefab.gameObject.scene.name != null)
		{
			this.childElementPrefab.gameObject.SetActive(false);
		}
		if (this.noContentsView != null)
		{
			this.noContentsView.SetActive(false);
		}
	}

	// Token: 0x06002E39 RID: 11833 RVA: 0x001494A8 File Offset: 0x001478A8
	private void Layout()
	{
		if (!this.dataSource.IsDataLoaded())
		{
			return;
		}
		base.StartCoroutine(this.LayoutCoroutine());
	}

	// Token: 0x06002E3A RID: 11834 RVA: 0x001494C8 File Offset: 0x001478C8
	private IEnumerator LayoutCoroutine()
	{
		this.childContentLoaded = false;
		yield return null;
		this.childElements = new List<UISceneElement>();
		int insertAtIndex = base.transform.GetSiblingIndex();
		for (int j = 0; j < this.dataSource.Keys.Count; j++)
		{
			string text = this.dataSource.Keys[j];
			string text2 = this.childDataType;
			string text3 = this.childDataSubtype;
			if (this.overwriteChildDataType)
			{
				text2 = text;
			}
			if (this.overwriteChildDataSubtype)
			{
				text3 = text;
			}
			UISceneElement uisceneElement = UnityEngine.Object.Instantiate<UISceneElement>(this.childElementPrefab);
			RectTransform rectTransform = (RectTransform)uisceneElement.transform;
			uisceneElement.gameObject.SetActive(true);
			GameObject gameObject = uisceneElement.gameObject;
			gameObject.name = gameObject.name + text2 + "_" + text3;
			rectTransform.SetParent(this.contentParent, false);
			if (insertAtIndex >= 0 && insertAtIndex < this.contentParent.childCount)
			{
				rectTransform.SetSiblingIndex(insertAtIndex);
				insertAtIndex++;
			}
			uisceneElement.dataType = text2;
			uisceneElement.dataSubtype = text3;
			this.childElements.Add(uisceneElement);
		}
		UISceneElement lastChildElement = null;
		for (int i = 0; i < this.childElements.Count; i++)
		{
			string key = this.dataSource.Keys[i];
			UISceneElement childElement = this.childElements[i];
			childElement.Init();
			UIPanelContents mainPanel = childElement.GetComponent<UIPanelContents>();
			if (mainPanel != null)
			{
				mainPanel.SetupPanel(this.dataSource, this.imageManager, key);
			}
			childElement.LoadContent(this.dataManager, this.imageManager);
			if (i == 0 && this.previousElement != null)
			{
				this.previousElement.nextElement = childElement;
				childElement.previousElement = this.previousElement;
			}
			if (i > 0)
			{
				childElement.previousElement = lastChildElement;
				lastChildElement.nextElement = childElement;
			}
			childElement.deleteOnSceneRefresh = true;
			lastChildElement = childElement;
			yield return null;
		}
		if (lastChildElement != null)
		{
			lastChildElement.nextElement = this.nextElement;
		}
		this.layoutComplete = true;
		while (!this.childContentLoaded)
		{
			this.childContentLoaded = this.childElements.TrueForAll((UISceneElement e) => e.ContentLoaded());
		}
		bool empty = this.childElements.TrueForAll((UISceneElement e) => !e.gameObject.activeSelf);
		if (empty && this.noContentsView != null)
		{
			this.noContentsView.SetActive(true);
		}
		yield break;
	}

	// Token: 0x06002E3B RID: 11835 RVA: 0x001494E3 File Offset: 0x001478E3
	public override bool ContentLoaded()
	{
		return base.ContentLoaded() && this.layoutComplete;
	}

	// Token: 0x06002E3C RID: 11836 RVA: 0x001494FC File Offset: 0x001478FC
	public override void ClearSelection()
	{
		foreach (UISceneElement uisceneElement in this.childElements)
		{
			uisceneElement.ClearSelection();
		}
	}

	// Token: 0x06002E3D RID: 11837 RVA: 0x00149558 File Offset: 0x00147958
	public override void UnloadContent()
	{
		this.layoutComplete = false;
		this.childContentLoaded = false;
		this.childElements.Clear();
		if (this.noContentsView != null)
		{
			this.noContentsView.SetActive(false);
		}
	}

	// Token: 0x040026B2 RID: 9906
	public UISceneElement childElementPrefab;

	// Token: 0x040026B3 RID: 9907
	public RectTransform contentParent;

	// Token: 0x040026B4 RID: 9908
	public GameObject noContentsView;

	// Token: 0x040026B5 RID: 9909
	public int insertAtIndexStart = -1;

	// Token: 0x040026B6 RID: 9910
	public string childDataType = string.Empty;

	// Token: 0x040026B7 RID: 9911
	public string childDataSubtype = string.Empty;

	// Token: 0x040026B8 RID: 9912
	public bool overwriteChildDataType;

	// Token: 0x040026B9 RID: 9913
	public bool overwriteChildDataSubtype;

	// Token: 0x040026BA RID: 9914
	private List<UISceneElement> childElements;

	// Token: 0x040026BB RID: 9915
	private bool layoutComplete;

	// Token: 0x040026BC RID: 9916
	private bool childContentLoaded;
}
