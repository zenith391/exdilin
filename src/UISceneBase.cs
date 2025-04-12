using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000440 RID: 1088
public class UISceneBase : MonoBehaviour, ItemPanelPagedListDelegate, IMenuInputHandler
{
	// Token: 0x06002E92 RID: 11922 RVA: 0x0014B4D0 File Offset: 0x001498D0
	public void Start()
	{
		this.canvas = base.GetComponentInChildren<Canvas>();
		this.canvas.pixelPerfect = false;
		this.canvasGroup = this.canvas.GetComponent<CanvasGroup>();
		this.sceneBackground = base.GetComponentInChildren<UISceneBackground>();
		if (this.canvasGroup == null)
		{
			this.canvasGroup = this.canvas.gameObject.AddComponent<CanvasGroup>();
		}
		if (MainUIController.active && MainUIController.Instance.autoLoadStartScene)
		{
			return;
		}
		if (this.sceneAnimator == null)
		{
			this.sceneAnimator = base.GetComponent<Animator>();
		}
		if (this.scrollRect == null)
		{
			this.scrollRect = base.GetComponentInChildren<ScrollRect>();
		}
	}

	// Token: 0x06002E93 RID: 11923 RVA: 0x0014B590 File Offset: 0x00149990
	public void OnSceneLoad(MainUIController controller, UISceneInfo newSceneInfo)
	{
		if (this.tempObjects != null)
		{
			UnityEngine.Object.Destroy(this.tempObjects);
		}
		if (this.sceneAnimator == null)
		{
			this.sceneAnimator = base.GetComponent<Animator>();
		}
		this.uiController = controller;
		this.sharedUIResources = this.uiController.sharedUIResources;
		this.dataManager = this.sharedUIResources.dataManager;
		this.imageManager = this.sharedUIResources.imageManager;
		this.SceneDidLoad(newSceneInfo);
		MenuInputHandler.Clear();
		MenuInputHandler.RequestControl(this);
		BWPendingPayouts.AddPendingPayoutsListener(new PendingPayoutsEventListener(this.OnPendingPayoutsRecieved));
		BWPendingPayouts.LoadCurrentUserPendingPayouts();
	}

	// Token: 0x06002E94 RID: 11924 RVA: 0x0014B638 File Offset: 0x00149A38
	public virtual void OnReturnToScene()
	{
		MenuInputHandler.Clear();
		MenuInputHandler.RequestControl(this);
	}

	// Token: 0x06002E95 RID: 11925 RVA: 0x0014B648 File Offset: 0x00149A48
	public virtual void SceneDidLoad(UISceneInfo newSceneInfo)
	{
		this.sceneInfo = newSceneInfo;
		if (this.allSceneElements == null)
		{
			this.allSceneElements = base.GetComponentsInChildren<UISceneElement>();
		}
		this.allSceneInfoDisplays = base.GetComponentsInChildren<UISceneInfoDisplay>();
		for (int i = 0; i < this.allSceneElements.Length; i++)
		{
			if (i > 0)
			{
				this.allSceneElements[i].previousElement = this.allSceneElements[i - 1];
			}
			if (i < this.allSceneElements.Length - 1)
			{
				this.allSceneElements[i].nextElement = this.allSceneElements[i + 1];
			}
		}
		if (this.uiController != null)
		{
			this.uiController.menuBar.settingsMenuButton.interactable = true;
		}
		if (this.scrollRect != null)
		{
			UIScrollControl component = this.scrollRect.gameObject.GetComponent<UIScrollControl>();
			if (component != null)
			{
				component.Cancel();
			}
			this.scrollRect.verticalNormalizedPosition = 1f;
		}
		base.StopAllCoroutines();
		base.StartCoroutine(this.LoadData());
	}

	// Token: 0x06002E96 RID: 11926 RVA: 0x0014B75C File Offset: 0x00149B5C
	private IEnumerator LoadData()
	{
		this.displayReady = false;
		foreach (UISceneInfoDisplay uisceneInfoDisplay in this.allSceneInfoDisplays)
		{
			uisceneInfoDisplay.Setup(this.sceneInfo);
		}
		if (!string.IsNullOrEmpty(this.sceneInfo.dataType) && !string.IsNullOrEmpty(this.sceneInfo.dataSubtype))
		{
			UIDataSource dataSource = this.dataManager.GetDataSource(this.sceneInfo.dataType, this.sceneInfo.dataSubtype);
			dataSource.Refresh();
			foreach (UIPanelContents uipanelContents in base.GetComponentsInChildren<UIPanelContents>())
			{
				if (uipanelContents.autoLoadDataSourceFromScene)
				{
					uipanelContents.SetupPanel(dataSource, this.imageManager, this.sceneInfo.dataSubtype);
				}
			}
		}
		foreach (UISceneElement element in this.allSceneElements)
		{
			element.Init();
			if (element.getDataTypeFromSceneParameters && !string.IsNullOrEmpty(this.sceneInfo.dataType))
			{
				element.dataType = this.sceneInfo.dataType;
			}
			if (element.getDataSubtypeFromSceneParameters && !string.IsNullOrEmpty(this.sceneInfo.dataSubtype))
			{
				element.dataSubtype = this.sceneInfo.dataSubtype;
			}
			if (element.gameObject != null && !element.getIDFromParentPanel)
			{
				element.LoadContent(this.dataManager, this.imageManager);
			}
			yield return null;
		}
		bool allReady = false;
		while (!allReady)
		{
			bool check = true;
			foreach (UISceneElement uisceneElement in this.allSceneElements)
			{
				if (!uisceneElement.ContentLoaded())
				{
					check = false;
					break;
				}
			}
			allReady = check;
			yield return null;
		}
		this.allLists = base.GetComponentsInChildren<ItemPanelPagedList>();
		foreach (ItemPanelPagedList itemPanelPagedList in this.allLists)
		{
			itemPanelPagedList.pagedListDelegate = this;
		}
		this.allSceneElements = base.GetComponentsInChildren<UISceneElement>();
		if (this.scrollRect != null)
		{
			this.scrollRect.verticalNormalizedPosition = 1f;
		}
		if (this.sceneBackground != null)
		{
			this.sceneBackground.Select();
		}
		if (this.sceneInfo.parameters != null && this.sceneInfo.parameters.ContainsKey("LoadAction") && this.sceneInfo.parameters.ContainsKey("LoadActionID"))
		{
			this.uiController.HandleMessage(this.sceneInfo.parameters["LoadAction"], this.sceneInfo.parameters["LoadActionID"], null, null);
		}
		this.displayReady = true;
		BWStandalone.Instance.OnUISceneLoad();
		yield break;
	}

	// Token: 0x06002E97 RID: 11927 RVA: 0x0014B778 File Offset: 0x00149B78
	public virtual Vector3 GetShoppingCartPosition()
	{
		UIShoppingCartContentIndicator componentInChildren = base.GetComponentInChildren<UIShoppingCartContentIndicator>();
		if (componentInChildren != null)
		{
			return componentInChildren.transform.position;
		}
		return new Vector3(1850f, 1100f);
	}

	// Token: 0x06002E98 RID: 11928 RVA: 0x0014B7B4 File Offset: 0x00149BB4
	public virtual Selectable GetDefaultSelectable()
	{
		Selectable result = null;
		foreach (UISceneElement uisceneElement in this.allSceneElements)
		{
			if (uisceneElement.defaultSelectable != null)
			{
				result = uisceneElement.defaultSelectable;
				break;
			}
		}
		return result;
	}

	// Token: 0x06002E99 RID: 11929 RVA: 0x0014B800 File Offset: 0x00149C00
	public virtual void ClearSelection()
	{
		if (this.sceneBackground != null)
		{
			this.sceneBackground.Select();
		}
		else
		{
			this.SceneBackgroundSelected();
		}
	}

	// Token: 0x06002E9A RID: 11930 RVA: 0x0014B82C File Offset: 0x00149C2C
	public void ClearContent()
	{
		if (this.scrollRect != null)
		{
			UIScrollControl component = this.scrollRect.gameObject.GetComponent<UIScrollControl>();
			if (component != null)
			{
				component.Cancel();
			}
			this.scrollRect.verticalNormalizedPosition = 1f;
		}
		this.allSceneElements = base.GetComponentsInChildren<UISceneElement>();
		HashSet<UISceneElement> hashSet = new HashSet<UISceneElement>();
		List<UISceneElement> list = new List<UISceneElement>(this.allSceneElements);
		for (int i = list.Count - 1; i >= 0; i--)
		{
			UISceneElement uisceneElement = list[i];
			if (uisceneElement.deleteOnSceneRefresh)
			{
				list.RemoveAt(i);
				hashSet.Add(uisceneElement);
			}
			else
			{
				uisceneElement.StopAllCoroutines();
				uisceneElement.UnloadContent();
			}
		}
		this.allSceneElements = list.ToArray();
		foreach (UISceneElement uisceneElement2 in hashSet)
		{
			UnityEngine.Object.Destroy(uisceneElement2.gameObject);
		}
	}

	// Token: 0x06002E9B RID: 11931 RVA: 0x0014B94C File Offset: 0x00149D4C
	public void RefreshContent()
	{
		foreach (UISceneElement uisceneElement in this.allSceneElements)
		{
			uisceneElement.RefreshContent();
		}
	}

	// Token: 0x06002E9C RID: 11932 RVA: 0x0014B97E File Offset: 0x00149D7E
	public virtual void Show()
	{
		this.canvasGroup.alpha = 1f;
		this.canvasGroup.interactable = true;
		this.canvasGroup.blocksRaycasts = true;
		Canvas.ForceUpdateCanvases();
	}

	// Token: 0x06002E9D RID: 11933 RVA: 0x0014B9AD File Offset: 0x00149DAD
	public virtual void HideAll()
	{
		this.canvasGroup.alpha = 0f;
		this.canvasGroup.interactable = false;
		this.canvasGroup.blocksRaycasts = false;
	}

	// Token: 0x06002E9E RID: 11934 RVA: 0x0014B9D7 File Offset: 0x00149DD7
	private void OnDestroy()
	{
		BWPendingPayouts.RemovePendingPayoutsListener(new PendingPayoutsEventListener(this.OnPendingPayoutsRecieved));
	}

	// Token: 0x06002E9F RID: 11935 RVA: 0x0014B9EC File Offset: 0x00149DEC
	public virtual void HandleSearchRequest(string searchStr)
	{
		if (string.IsNullOrEmpty(searchStr))
		{
			return;
		}
		UISceneInfo uisceneInfo = new UISceneInfo();
		uisceneInfo.path = "PlayMenu/WorldBrowser";
		uisceneInfo.title = "World Search Results: \"" + searchStr + "\"";
		uisceneInfo.dataType = "WorldSearch";
		uisceneInfo.dataSubtype = searchStr;
		MainUIController.Instance.LoadUIScene(uisceneInfo, false, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
	}

	// Token: 0x06002EA0 RID: 11936 RVA: 0x0014BA4C File Offset: 0x00149E4C
	public virtual void SceneBackgroundSelected()
	{
		if (this.allSceneElements == null)
		{
			return;
		}
		foreach (UISceneElement uisceneElement in this.allSceneElements)
		{
			uisceneElement.ClearSelection();
		}
	}

	// Token: 0x06002EA1 RID: 11937 RVA: 0x0014BA8A File Offset: 0x00149E8A
	public bool IsDisplayReady()
	{
		return this.displayReady;
	}

	// Token: 0x06002EA2 RID: 11938 RVA: 0x0014BA92 File Offset: 0x00149E92
	public virtual void SceneWillUnload()
	{
		BWPendingPayouts.RemovePendingPayoutsListener(new PendingPayoutsEventListener(this.OnPendingPayoutsRecieved));
		this.DoSceneHide();
	}

	// Token: 0x06002EA3 RID: 11939 RVA: 0x0014BAAB File Offset: 0x00149EAB
	public void DoSceneReveal()
	{
		if (this.sceneAnimator != null)
		{
			this.sceneAnimator.SetTrigger("Reveal");
		}
	}

	// Token: 0x06002EA4 RID: 11940 RVA: 0x0014BACE File Offset: 0x00149ECE
	protected void DoSceneHide()
	{
		if (this.sceneAnimator != null)
		{
			this.sceneAnimatorHideDelay = 5;
			this.sceneAnimator.SetTrigger("Hide");
		}
	}

	// Token: 0x06002EA5 RID: 11941 RVA: 0x0014BAF8 File Offset: 0x00149EF8
	public bool SceneAnimationInProgress()
	{
		if (this.sceneAnimator == null)
		{
			return false;
		}
		if (this.sceneAnimatorHideDelay > 0)
		{
			this.sceneAnimatorHideDelay--;
			return true;
		}
		return this.sceneAnimator.IsInTransition(0) || this.sceneAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.99f;
	}

	// Token: 0x06002EA6 RID: 11942 RVA: 0x0014BB64 File Offset: 0x00149F64
	public void SelectItemInSceneElements(string itemID, string dataType, string dataSubtype)
	{
		foreach (UISceneElement uisceneElement in this.allSceneElements)
		{
			if ((string.IsNullOrEmpty(dataType) || !(dataType != uisceneElement.dataType)) && (string.IsNullOrEmpty(dataSubtype) || !(dataSubtype != uisceneElement.dataSubtype)))
			{
				if (uisceneElement.SelectItemWithID(itemID))
				{
					return;
				}
			}
		}
		if (string.IsNullOrEmpty(dataType))
		{
			return;
		}
		foreach (UISceneElement uisceneElement2 in this.allSceneElements)
		{
			if (uisceneElement2.SelectItemWithID(itemID))
			{
				break;
			}
		}
	}

	// Token: 0x06002EA7 RID: 11943 RVA: 0x0014BC20 File Offset: 0x0014A020
	public GameObject CloneItemInSceneElements(string itemID, string dataType, string dataSubtype)
	{
		foreach (UISceneElement uisceneElement in this.allSceneElements)
		{
			if ((string.IsNullOrEmpty(dataType) || dataType == uisceneElement.dataType) && (string.IsNullOrEmpty(dataSubtype) || dataSubtype == uisceneElement.dataSubtype))
			{
				return uisceneElement.CloneItemWithID(itemID);
			}
		}
		return null;
	}

	// Token: 0x06002EA8 RID: 11944 RVA: 0x0014BC90 File Offset: 0x0014A090
	public void TriggerDetailPanelAnimation(string detailPanelID, string animKey, string animTrigger)
	{
		foreach (ItemPanelPagedList itemPanelPagedList in this.allLists)
		{
			DetailPanel detailView = itemPanelPagedList.itemList.detailView;
			if (detailView != null && detailView.contentId == detailPanelID)
			{
				detailView.TriggerAnimation(animKey, animTrigger);
			}
		}
	}

	// Token: 0x06002EA9 RID: 11945 RVA: 0x0014BCF0 File Offset: 0x0014A0F0
	public void ResetDetailPanelAnimation(string detailPanelID)
	{
		foreach (ItemPanelPagedList itemPanelPagedList in this.allLists)
		{
			DetailPanel detailView = itemPanelPagedList.itemList.detailView;
			if (detailView != null && detailView.contentId == detailPanelID)
			{
				detailView.ResetAnimation();
			}
		}
	}

	// Token: 0x06002EAA RID: 11946 RVA: 0x0014BD4C File Offset: 0x0014A14C
	public void ItemInPagedListSelected(ItemPanelPagedList pagedList, int index, bool selectedByClick)
	{
		bool flag = false;
		foreach (ItemPanelPagedList itemPanelPagedList in this.allLists)
		{
			if (itemPanelPagedList != pagedList)
			{
				flag |= (itemPanelPagedList.itemList.selectedItem != null);
				itemPanelPagedList.itemList.ClearSelection();
				bool immediate = !selectedByClick;
				itemPanelPagedList.HideDetailPanel(immediate);
			}
		}
	}

	// Token: 0x06002EAB RID: 11947 RVA: 0x0014BDB3 File Offset: 0x0014A1B3
	public virtual void HandleMenuInputEvents()
	{
		if (MappedInput.InputDown(MappableInput.MENU_CANCEL))
		{
			MainUIController.Instance.NavigateBack();
		}
	}

	// Token: 0x06002EAC RID: 11948 RVA: 0x0014BDCB File Offset: 0x0014A1CB
	private void OnPendingPayoutsRecieved(List<BWPendingPayout> payouts)
	{
		BWStandalone.Overlays.ShowPopupPendingPayouts();
	}

	// Token: 0x06002EAD RID: 11949 RVA: 0x0014BDD8 File Offset: 0x0014A1D8
	public void LoadExampleContent(int exampleContentSize)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("SharedUIResources"));
		this.sharedUIResources = gameObject.GetComponent<SharedUIResources>();
		this.sharedUIResources.dataManager.InitWithExampleData(exampleContentSize);
		this.LayoutExampleContent(exampleContentSize);
		UnityEngine.Object.DestroyImmediate(gameObject);
		this.sharedUIResources = null;
	}

	// Token: 0x06002EAE RID: 11950 RVA: 0x0014BE28 File Offset: 0x0014A228
	protected virtual void LayoutExampleContent(int exampleContentSize)
	{
		this.ClearExampleContent();
		foreach (UISceneElement uisceneElement in base.GetComponentsInChildren<UISceneElement>())
		{
			uisceneElement.Init();
			if (uisceneElement.getDataTypeFromSceneParameters)
			{
				uisceneElement.dataType = this.sceneInfo.dataType;
				uisceneElement.dataSubtype = this.sceneInfo.dataSubtype;
			}
			uisceneElement.LoadContent(this.sharedUIResources.dataManager, this.sharedUIResources.imageManager);
		}
	}

	// Token: 0x06002EAF RID: 11951 RVA: 0x0014BEAC File Offset: 0x0014A2AC
	public virtual void ClearExampleContent()
	{
		foreach (UISceneElement uisceneElement in base.GetComponentsInChildren<UISceneElement>())
		{
			uisceneElement.Init();
			uisceneElement.UnloadEditorExampleContent();
		}
	}

	// Token: 0x04002713 RID: 10003
	public UISceneInfo sceneInfo;

	// Token: 0x04002714 RID: 10004
	public Animator sceneAnimator;

	// Token: 0x04002715 RID: 10005
	public ScrollRect scrollRect;

	// Token: 0x04002716 RID: 10006
	public GameObject tempObjects;

	// Token: 0x04002717 RID: 10007
	public GameObject sceneTransitionPrefab;

	// Token: 0x04002718 RID: 10008
	protected MainUIController uiController;

	// Token: 0x04002719 RID: 10009
	protected UIDataManager dataManager;

	// Token: 0x0400271A RID: 10010
	protected ImageManager imageManager;

	// Token: 0x0400271B RID: 10011
	protected SharedUIResources sharedUIResources;

	// Token: 0x0400271C RID: 10012
	protected UISceneElement[] allSceneElements;

	// Token: 0x0400271D RID: 10013
	protected UISceneInfoDisplay[] allSceneInfoDisplays;

	// Token: 0x0400271E RID: 10014
	protected ItemPanelPagedList[] allLists;

	// Token: 0x0400271F RID: 10015
	protected Canvas canvas;

	// Token: 0x04002720 RID: 10016
	protected CanvasGroup canvasGroup;

	// Token: 0x04002721 RID: 10017
	private int sceneAnimatorHideDelay;

	// Token: 0x04002722 RID: 10018
	private bool displayReady;

	// Token: 0x04002723 RID: 10019
	private UISceneBackground sceneBackground;
}
