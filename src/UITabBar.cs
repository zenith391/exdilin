using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITabBar : MonoBehaviour
{
	[Serializable]
	public class TabInfo
	{
		public TabBarTabId id;

		public GameObject buttonObj;
	}

	public RectTransform backgroundTransform;

	public RectTransform selectionIndicator;

	public RectTransform tabLayoutTransform;

	public TabInfo[] tabs;

	public Text noDataMessage;

	public bool tabButtonsAutoLayout;

	private float _defaultWidth;

	private float _defaultY;

	private TabBar tabDelegate;

	private List<Image> _buttonImages;

	private string _noDataMessage;

	private Dictionary<TabBarTabId, string> noDataMessageText = new Dictionary<TabBarTabId, string>
	{
		{
			TabBarTabId.Blocks,
			"You have no blocks."
		},
		{
			TabBarTabId.Models,
			"Save your models here."
		},
		{
			TabBarTabId.Props,
			"You have no props."
		},
		{
			TabBarTabId.Colors,
			"You have no colors."
		},
		{
			TabBarTabId.Textures,
			"You have no textures."
		},
		{
			TabBarTabId.Blocksters,
			"You have no Blocksters."
		},
		{
			TabBarTabId.Gear,
			"You have no gear."
		},
		{
			TabBarTabId.ActionBlocks,
			"You have no action blocks."
		},
		{
			TabBarTabId.Actions,
			"Select a block in the world to view inputs and actions."
		},
		{
			TabBarTabId.Sounds,
			"Select a block in the world to view sound effects."
		}
	};

	public void Init()
	{
		_defaultWidth = backgroundTransform.sizeDelta.x;
		_defaultY = backgroundTransform.anchoredPosition.y;
		_buttonImages = new List<Image>();
		for (int i = 0; i < tabs.Length; i++)
		{
			Button componentInChildren = tabs[i].buttonObj.GetComponentInChildren<Button>(includeInactive: true);
			UITabBarButtonDownHandler uITabBarButtonDownHandler = componentInChildren.gameObject.AddComponent<UITabBarButtonDownHandler>();
			uITabBarButtonDownHandler.tabId = tabs[i].id;
			_buttonImages.Add(tabs[i].buttonObj.transform.GetChild(0).GetComponent<Image>());
		}
		MoveSelectionIndicatorToTab(TabBarTabId.Blocks);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		Layout();
		if (tabDelegate != null)
		{
			MoveSelectionIndicatorToTab(tabDelegate.SelectedTab);
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public bool Hit(Vector3 pos)
	{
		return Util.RectTransformContains(backgroundTransform, pos);
	}

	public string GetNoDataMessageStr()
	{
		return _noDataMessage;
	}

	public float GetPixelWidth()
	{
		return NormalizedScreen.pixelScale * _defaultWidth;
	}

	public void AssignDelegate(TabBar tabBar)
	{
		tabDelegate = tabBar;
		MoveSelectionIndicatorToTab(tabDelegate.SelectedTab);
	}

	public void DidSelectTabBarPanel(TabBarTabId tabId)
	{
		if (tabDelegate.SelectedTab == tabId)
		{
			ScrollToNextSection();
		}
		else
		{
			SwitchToTab(tabId);
		}
	}

	public void SwitchToTab(TabBarTabId tabId)
	{
		_noDataMessage = ((!noDataMessageText.ContainsKey(tabId)) ? string.Empty : noDataMessageText[tabId]);
		tabDelegate.SetSelectedTab((int)tabId);
		MoveSelectionIndicatorToTab(tabId);
		TileIconManager.Instance.ClearNewLoadLimit();
	}

	private void ScrollToNextSection()
	{
		tabDelegate.ScrollToNextSection();
	}

	public void MoveSelectionIndicatorToTab(TabBarTabId tabId)
	{
		GameObject tabObject = GetTabObject(tabId);
		RectTransform rectTransform = (RectTransform)tabObject.transform;
		selectionIndicator.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
	}

	public Vector3 GetTabBarPosition(TabBarTabId tabId)
	{
		GameObject gameObject = GetTabObject(tabId);
		if (gameObject == null)
		{
			BWLog.Info("Couldn't find UI element for tab " + tabId.ToString() + " using first tab");
			gameObject = tabs[0].buttonObj;
		}
		return Util.CenterOfRectTransform(gameObject.transform);
	}

	public float GetTabHeight()
	{
		RectTransform rectTransform = (RectTransform)tabs[0].buttonObj.transform;
		return rectTransform.sizeDelta.y;
	}

	private GameObject GetTabObject(TabBarTabId tabId)
	{
		GameObject result = null;
		for (int i = 0; i < tabs.Length; i++)
		{
			if (tabs[i].id == tabId)
			{
				result = tabs[i].buttonObj;
				break;
			}
		}
		return result;
	}

	public void SelectTabAtScreenPos(Vector3 pos)
	{
		for (int i = 0; i < 10; i++)
		{
			TabBarTabId tabId = (TabBarTabId)i;
			GameObject tabObject = GetTabObject(tabId);
			if (!(tabObject == null))
			{
				RectTransform t = (RectTransform)tabObject.transform;
				if (Util.RectTransformContains(t, pos))
				{
					DidSelectTabBarPanel(tabId);
					break;
				}
			}
		}
	}

	public void Layout()
	{
		float pixelWidth = GetPixelWidth();
		float num = _defaultY * NormalizedScreen.pixelScale;
		Vector2 sizeDelta = new Vector2(pixelWidth, (float)Screen.height + num * 2f);
		Vector2 anchoredPosition = new Vector2(0f, num);
		backgroundTransform.anchoredPosition = anchoredPosition;
		tabLayoutTransform.anchoredPosition = anchoredPosition;
		backgroundTransform.sizeDelta = sizeDelta;
		tabLayoutTransform.sizeDelta = sizeDelta;
		Canvas.ForceUpdateCanvases();
		selectionIndicator.sizeDelta = new Vector2(pixelWidth, GetTabHeight());
		if (tabDelegate != null)
		{
			MoveSelectionIndicatorToTab(tabDelegate.SelectedTab);
		}
		if (!tabButtonsAutoLayout)
		{
			for (int i = 0; i < _buttonImages.Count; i++)
			{
				float a = Mathf.Min(1f, GetTabHeight() / _buttonImages[i].preferredHeight);
				float pixelScale = NormalizedScreen.pixelScale;
				float num2 = Mathf.Min(a, pixelScale);
				_buttonImages[i].transform.localScale = Vector3.one * num2;
				_buttonImages[i].rectTransform.sizeDelta = new Vector2(_buttonImages[i].preferredWidth, _buttonImages[i].preferredHeight);
			}
		}
	}
}
