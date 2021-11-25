using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200031D RID: 797
public class UITabBar : MonoBehaviour
{
	// Token: 0x06002410 RID: 9232 RVA: 0x00109010 File Offset: 0x00107410
	public void Init()
	{
		this._defaultWidth = this.backgroundTransform.sizeDelta.x;
		this._defaultY = this.backgroundTransform.anchoredPosition.y;
		this._buttonImages = new List<Image>();
		for (int i = 0; i < this.tabs.Length; i++)
		{
			Button componentInChildren = this.tabs[i].buttonObj.GetComponentInChildren<Button>(true);
			UITabBarButtonDownHandler uitabBarButtonDownHandler = componentInChildren.gameObject.AddComponent<UITabBarButtonDownHandler>();
			uitabBarButtonDownHandler.tabId = this.tabs[i].id;
			this._buttonImages.Add(this.tabs[i].buttonObj.transform.GetChild(0).GetComponent<Image>());
		}
		this.MoveSelectionIndicatorToTab(TabBarTabId.Blocks);
	}

	// Token: 0x06002411 RID: 9233 RVA: 0x001090D7 File Offset: 0x001074D7
	public void Show()
	{
		base.gameObject.SetActive(true);
		this.Layout();
		if (this.tabDelegate != null)
		{
			this.MoveSelectionIndicatorToTab(this.tabDelegate.SelectedTab);
		}
	}

	// Token: 0x06002412 RID: 9234 RVA: 0x00109107 File Offset: 0x00107507
	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x06002413 RID: 9235 RVA: 0x00109115 File Offset: 0x00107515
	public bool Hit(Vector3 pos)
	{
		return Util.RectTransformContains(this.backgroundTransform, pos);
	}

	// Token: 0x06002414 RID: 9236 RVA: 0x00109123 File Offset: 0x00107523
	public string GetNoDataMessageStr()
	{
		return this._noDataMessage;
	}

	// Token: 0x06002415 RID: 9237 RVA: 0x0010912B File Offset: 0x0010752B
	public float GetPixelWidth()
	{
		return NormalizedScreen.pixelScale * this._defaultWidth;
	}

	// Token: 0x06002416 RID: 9238 RVA: 0x00109139 File Offset: 0x00107539
	public void AssignDelegate(TabBar tabBar)
	{
		this.tabDelegate = tabBar;
		this.MoveSelectionIndicatorToTab(this.tabDelegate.SelectedTab);
	}

	// Token: 0x06002417 RID: 9239 RVA: 0x00109153 File Offset: 0x00107553
	public void DidSelectTabBarPanel(TabBarTabId tabId)
	{
		if (this.tabDelegate.SelectedTab == tabId)
		{
			this.ScrollToNextSection();
		}
		else
		{
			this.SwitchToTab(tabId);
		}
	}

	// Token: 0x06002418 RID: 9240 RVA: 0x00109178 File Offset: 0x00107578
	public void SwitchToTab(TabBarTabId tabId)
	{
		this._noDataMessage = ((!this.noDataMessageText.ContainsKey(tabId)) ? string.Empty : this.noDataMessageText[tabId]);
		this.tabDelegate.SetSelectedTab((int)tabId, true);
		this.MoveSelectionIndicatorToTab(tabId);
		TileIconManager.Instance.ClearNewLoadLimit();
	}

	// Token: 0x06002419 RID: 9241 RVA: 0x001091D0 File Offset: 0x001075D0
	private void ScrollToNextSection()
	{
		this.tabDelegate.ScrollToNextSection(true);
	}

	// Token: 0x0600241A RID: 9242 RVA: 0x001091E0 File Offset: 0x001075E0
	public void MoveSelectionIndicatorToTab(TabBarTabId tabId)
	{
		GameObject tabObject = this.GetTabObject(tabId);
		RectTransform rectTransform = (RectTransform)tabObject.transform;
		this.selectionIndicator.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
	}

	// Token: 0x0600241B RID: 9243 RVA: 0x00109224 File Offset: 0x00107624
	public Vector3 GetTabBarPosition(TabBarTabId tabId)
	{
		GameObject gameObject = this.GetTabObject(tabId);
		if (gameObject == null)
		{
			BWLog.Info("Couldn't find UI element for tab " + tabId + " using first tab");
			gameObject = this.tabs[0].buttonObj;
		}
		return Util.CenterOfRectTransform(gameObject.transform);
	}

	// Token: 0x0600241C RID: 9244 RVA: 0x00109278 File Offset: 0x00107678
	public float GetTabHeight()
	{
		RectTransform rectTransform = (RectTransform)this.tabs[0].buttonObj.transform;
		return rectTransform.sizeDelta.y;
	}

	// Token: 0x0600241D RID: 9245 RVA: 0x001092AC File Offset: 0x001076AC
	private GameObject GetTabObject(TabBarTabId tabId)
	{
		GameObject result = null;
		for (int i = 0; i < this.tabs.Length; i++)
		{
			if (this.tabs[i].id == tabId)
			{
				result = this.tabs[i].buttonObj;
				break;
			}
		}
		return result;
	}

	// Token: 0x0600241E RID: 9246 RVA: 0x001092FC File Offset: 0x001076FC
	public void SelectTabAtScreenPos(Vector3 pos)
	{
		for (int i = 0; i < 10; i++)
		{
			TabBarTabId tabId = (TabBarTabId)i;
			GameObject tabObject = this.GetTabObject(tabId);
			if (!(tabObject == null))
			{
				RectTransform t = (RectTransform)tabObject.transform;
				if (Util.RectTransformContains(t, pos))
				{
					this.DidSelectTabBarPanel(tabId);
					return;
				}
			}
		}
	}

	// Token: 0x0600241F RID: 9247 RVA: 0x00109358 File Offset: 0x00107758
	public void Layout()
	{
		float pixelWidth = this.GetPixelWidth();
		float num = this._defaultY * NormalizedScreen.pixelScale;
		Vector2 sizeDelta = new Vector2(pixelWidth, (float)Screen.height + num * 2f);
		Vector2 anchoredPosition = new Vector2(0f, num);
		this.backgroundTransform.anchoredPosition = anchoredPosition;
		this.tabLayoutTransform.anchoredPosition = anchoredPosition;
		this.backgroundTransform.sizeDelta = sizeDelta;
		this.tabLayoutTransform.sizeDelta = sizeDelta;
		Canvas.ForceUpdateCanvases();
		this.selectionIndicator.sizeDelta = new Vector2(pixelWidth, this.GetTabHeight());
		if (this.tabDelegate != null)
		{
			this.MoveSelectionIndicatorToTab(this.tabDelegate.SelectedTab);
		}
		if (this.tabButtonsAutoLayout)
		{
			return;
		}
		for (int i = 0; i < this._buttonImages.Count; i++)
		{
			float a = Mathf.Min(1f, this.GetTabHeight() / this._buttonImages[i].preferredHeight);
			float pixelScale = NormalizedScreen.pixelScale;
			float d = Mathf.Min(a, pixelScale);
			this._buttonImages[i].transform.localScale = Vector3.one * d;
			this._buttonImages[i].rectTransform.sizeDelta = new Vector2(this._buttonImages[i].preferredWidth, this._buttonImages[i].preferredHeight);
		}
	}

	// Token: 0x04001F14 RID: 7956
	public RectTransform backgroundTransform;

	// Token: 0x04001F15 RID: 7957
	public RectTransform selectionIndicator;

	// Token: 0x04001F16 RID: 7958
	public RectTransform tabLayoutTransform;

	// Token: 0x04001F17 RID: 7959
	public UITabBar.TabInfo[] tabs;

	// Token: 0x04001F18 RID: 7960
	public Text noDataMessage;

	// Token: 0x04001F19 RID: 7961
	public bool tabButtonsAutoLayout;

	// Token: 0x04001F1A RID: 7962
	private float _defaultWidth;

	// Token: 0x04001F1B RID: 7963
	private float _defaultY;

	// Token: 0x04001F1C RID: 7964
	private TabBar tabDelegate;

	// Token: 0x04001F1D RID: 7965
	private List<Image> _buttonImages;

	// Token: 0x04001F1E RID: 7966
	private string _noDataMessage;

	// Token: 0x04001F1F RID: 7967
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

	// Token: 0x0200031E RID: 798
	[Serializable]
	public class TabInfo
	{
		// Token: 0x04001F20 RID: 7968
		public TabBarTabId id;

		// Token: 0x04001F21 RID: 7969
		public GameObject buttonObj;
	}
}
