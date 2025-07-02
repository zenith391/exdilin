using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISceneElementGroup : UISceneElement
{
	public UISceneElement childElementPrefab;

	public RectTransform contentParent;

	public GameObject noContentsView;

	public int insertAtIndexStart = -1;

	public string childDataType = string.Empty;

	public string childDataSubtype = string.Empty;

	public bool overwriteChildDataType;

	public bool overwriteChildDataSubtype;

	private List<UISceneElement> childElements;

	private bool layoutComplete;

	private bool childContentLoaded;

	protected override void LoadContentFromDataSource()
	{
		if (!layoutComplete)
		{
			Layout();
		}
	}

	private void Awake()
	{
		if (childElementPrefab.gameObject.scene.name != null)
		{
			childElementPrefab.gameObject.SetActive(value: false);
		}
		if (noContentsView != null)
		{
			noContentsView.SetActive(value: false);
		}
	}

	private void Layout()
	{
		if (dataSource.IsDataLoaded())
		{
			StartCoroutine(LayoutCoroutine());
		}
	}

	private IEnumerator LayoutCoroutine()
	{
		childContentLoaded = false;
		yield return null;
		childElements = new List<UISceneElement>();
		int num = base.transform.GetSiblingIndex();
		for (int i = 0; i < dataSource.Keys.Count; i++)
		{
			string text = dataSource.Keys[i];
			string text2 = childDataType;
			string text3 = childDataSubtype;
			if (overwriteChildDataType)
			{
				text2 = text;
			}
			if (overwriteChildDataSubtype)
			{
				text3 = text;
			}
			UISceneElement uISceneElement = Object.Instantiate(childElementPrefab);
			RectTransform rectTransform = (RectTransform)uISceneElement.transform;
			uISceneElement.gameObject.SetActive(value: true);
			GameObject gameObject = uISceneElement.gameObject;
			gameObject.name = gameObject.name + text2 + "_" + text3;
			rectTransform.SetParent(contentParent, worldPositionStays: false);
			if (num >= 0 && num < contentParent.childCount)
			{
				rectTransform.SetSiblingIndex(num);
				num++;
			}
			uISceneElement.dataType = text2;
			uISceneElement.dataSubtype = text3;
			childElements.Add(uISceneElement);
		}
		UISceneElement lastChildElement = null;
		for (int j = 0; j < childElements.Count; j++)
		{
			string itemId = dataSource.Keys[j];
			UISceneElement uISceneElement2 = childElements[j];
			uISceneElement2.Init();
			UIPanelContents component = uISceneElement2.GetComponent<UIPanelContents>();
			if (component != null)
			{
				component.SetupPanel(dataSource, imageManager, itemId);
			}
			uISceneElement2.LoadContent(dataManager, imageManager);
			if (j == 0 && previousElement != null)
			{
				previousElement.nextElement = uISceneElement2;
				uISceneElement2.previousElement = previousElement;
			}
			if (j > 0)
			{
				uISceneElement2.previousElement = lastChildElement;
				lastChildElement.nextElement = uISceneElement2;
			}
			uISceneElement2.deleteOnSceneRefresh = true;
			lastChildElement = uISceneElement2;
			yield return null;
		}
		if (lastChildElement != null)
		{
			lastChildElement.nextElement = nextElement;
		}
		layoutComplete = true;
		while (!childContentLoaded)
		{
			childContentLoaded = childElements.TrueForAll((UISceneElement e) => e.ContentLoaded());
		}
		if (childElements.TrueForAll((UISceneElement e) => !e.gameObject.activeSelf) && noContentsView != null)
		{
			noContentsView.SetActive(value: true);
		}
	}

	public override bool ContentLoaded()
	{
		if (base.ContentLoaded())
		{
			return layoutComplete;
		}
		return false;
	}

	public override void ClearSelection()
	{
		foreach (UISceneElement childElement in childElements)
		{
			childElement.ClearSelection();
		}
	}

	public override void UnloadContent()
	{
		layoutComplete = false;
		childContentLoaded = false;
		childElements.Clear();
		if (noContentsView != null)
		{
			noContentsView.SetActive(value: false);
		}
	}
}
