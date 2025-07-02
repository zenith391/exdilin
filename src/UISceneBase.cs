using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISceneBase : MonoBehaviour, ItemPanelPagedListDelegate, IMenuInputHandler
{
	public UISceneInfo sceneInfo;

	public Animator sceneAnimator;

	public ScrollRect scrollRect;

	public GameObject tempObjects;

	public GameObject sceneTransitionPrefab;

	protected MainUIController uiController;

	protected UIDataManager dataManager;

	protected ImageManager imageManager;

	protected SharedUIResources sharedUIResources;

	protected UISceneElement[] allSceneElements;

	protected UISceneInfoDisplay[] allSceneInfoDisplays;

	protected ItemPanelPagedList[] allLists;

	protected Canvas canvas;

	protected CanvasGroup canvasGroup;

	private int sceneAnimatorHideDelay;

	private bool displayReady;

	private UISceneBackground sceneBackground;

	public void Start()
	{
		canvas = GetComponentInChildren<Canvas>();
		canvas.pixelPerfect = false;
		canvasGroup = canvas.GetComponent<CanvasGroup>();
		sceneBackground = GetComponentInChildren<UISceneBackground>();
		if (canvasGroup == null)
		{
			canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
		}
		if (!MainUIController.active || !MainUIController.Instance.autoLoadStartScene)
		{
			if (sceneAnimator == null)
			{
				sceneAnimator = GetComponent<Animator>();
			}
			if (scrollRect == null)
			{
				scrollRect = GetComponentInChildren<ScrollRect>();
			}
		}
	}

	public void OnSceneLoad(MainUIController controller, UISceneInfo newSceneInfo)
	{
		if (tempObjects != null)
		{
			Object.Destroy(tempObjects);
		}
		if (sceneAnimator == null)
		{
			sceneAnimator = GetComponent<Animator>();
		}
		uiController = controller;
		sharedUIResources = uiController.sharedUIResources;
		dataManager = sharedUIResources.dataManager;
		imageManager = sharedUIResources.imageManager;
		SceneDidLoad(newSceneInfo);
		MenuInputHandler.Clear();
		MenuInputHandler.RequestControl(this);
		BWPendingPayouts.AddPendingPayoutsListener(OnPendingPayoutsRecieved);
		BWPendingPayouts.LoadCurrentUserPendingPayouts();
	}

	public virtual void OnReturnToScene()
	{
		MenuInputHandler.Clear();
		MenuInputHandler.RequestControl(this);
	}

	public virtual void SceneDidLoad(UISceneInfo newSceneInfo)
	{
		sceneInfo = newSceneInfo;
		if (allSceneElements == null)
		{
			allSceneElements = GetComponentsInChildren<UISceneElement>();
		}
		allSceneInfoDisplays = GetComponentsInChildren<UISceneInfoDisplay>();
		for (int i = 0; i < allSceneElements.Length; i++)
		{
			if (i > 0)
			{
				allSceneElements[i].previousElement = allSceneElements[i - 1];
			}
			if (i < allSceneElements.Length - 1)
			{
				allSceneElements[i].nextElement = allSceneElements[i + 1];
			}
		}
		if (uiController != null)
		{
			uiController.menuBar.settingsMenuButton.interactable = true;
		}
		if (scrollRect != null)
		{
			UIScrollControl component = scrollRect.gameObject.GetComponent<UIScrollControl>();
			if (component != null)
			{
				component.Cancel();
			}
			scrollRect.verticalNormalizedPosition = 1f;
		}
		StopAllCoroutines();
		StartCoroutine(LoadData());
	}

	private IEnumerator LoadData()
	{
		displayReady = false;
		UISceneInfoDisplay[] array = allSceneInfoDisplays;
		foreach (UISceneInfoDisplay uISceneInfoDisplay in array)
		{
			uISceneInfoDisplay.Setup(sceneInfo);
		}
		if (!string.IsNullOrEmpty(sceneInfo.dataType) && !string.IsNullOrEmpty(sceneInfo.dataSubtype))
		{
			UIDataSource dataSource = dataManager.GetDataSource(sceneInfo.dataType, sceneInfo.dataSubtype);
			dataSource.Refresh();
			UIPanelContents[] componentsInChildren = GetComponentsInChildren<UIPanelContents>();
			foreach (UIPanelContents uIPanelContents in componentsInChildren)
			{
				if (uIPanelContents.autoLoadDataSourceFromScene)
				{
					uIPanelContents.SetupPanel(dataSource, imageManager, sceneInfo.dataSubtype);
				}
			}
		}
		UISceneElement[] array2 = allSceneElements;
		foreach (UISceneElement uISceneElement in array2)
		{
			uISceneElement.Init();
			if (uISceneElement.getDataTypeFromSceneParameters && !string.IsNullOrEmpty(sceneInfo.dataType))
			{
				uISceneElement.dataType = sceneInfo.dataType;
			}
			if (uISceneElement.getDataSubtypeFromSceneParameters && !string.IsNullOrEmpty(sceneInfo.dataSubtype))
			{
				uISceneElement.dataSubtype = sceneInfo.dataSubtype;
			}
			if (uISceneElement.gameObject != null && !uISceneElement.getIDFromParentPanel)
			{
				uISceneElement.LoadContent(dataManager, imageManager);
			}
			yield return null;
		}
		bool allReady = false;
		while (!allReady)
		{
			bool flag = true;
			UISceneElement[] array3 = allSceneElements;
			foreach (UISceneElement uISceneElement2 in array3)
			{
				if (!uISceneElement2.ContentLoaded())
				{
					flag = false;
					break;
				}
			}
			allReady = flag;
			yield return null;
		}
		allLists = GetComponentsInChildren<ItemPanelPagedList>();
		ItemPanelPagedList[] array4 = allLists;
		foreach (ItemPanelPagedList itemPanelPagedList in array4)
		{
			itemPanelPagedList.pagedListDelegate = this;
		}
		allSceneElements = GetComponentsInChildren<UISceneElement>();
		if (scrollRect != null)
		{
			scrollRect.verticalNormalizedPosition = 1f;
		}
		if (sceneBackground != null)
		{
			sceneBackground.Select();
		}
		if (sceneInfo.parameters != null && sceneInfo.parameters.ContainsKey("LoadAction") && sceneInfo.parameters.ContainsKey("LoadActionID"))
		{
			uiController.HandleMessage(sceneInfo.parameters["LoadAction"], sceneInfo.parameters["LoadActionID"], null, null);
		}
		displayReady = true;
		BWStandalone.Instance.OnUISceneLoad();
	}

	public virtual Vector3 GetShoppingCartPosition()
	{
		UIShoppingCartContentIndicator componentInChildren = GetComponentInChildren<UIShoppingCartContentIndicator>();
		if (componentInChildren != null)
		{
			return componentInChildren.transform.position;
		}
		return new Vector3(1850f, 1100f);
	}

	public virtual Selectable GetDefaultSelectable()
	{
		Selectable result = null;
		UISceneElement[] array = allSceneElements;
		foreach (UISceneElement uISceneElement in array)
		{
			if (uISceneElement.defaultSelectable != null)
			{
				result = uISceneElement.defaultSelectable;
				break;
			}
		}
		return result;
	}

	public virtual void ClearSelection()
	{
		if (sceneBackground != null)
		{
			sceneBackground.Select();
		}
		else
		{
			SceneBackgroundSelected();
		}
	}

	public void ClearContent()
	{
		if (scrollRect != null)
		{
			UIScrollControl component = scrollRect.gameObject.GetComponent<UIScrollControl>();
			if (component != null)
			{
				component.Cancel();
			}
			scrollRect.verticalNormalizedPosition = 1f;
		}
		allSceneElements = GetComponentsInChildren<UISceneElement>();
		HashSet<UISceneElement> hashSet = new HashSet<UISceneElement>();
		List<UISceneElement> list = new List<UISceneElement>(allSceneElements);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			UISceneElement uISceneElement = list[num];
			if (uISceneElement.deleteOnSceneRefresh)
			{
				list.RemoveAt(num);
				hashSet.Add(uISceneElement);
			}
			else
			{
				uISceneElement.StopAllCoroutines();
				uISceneElement.UnloadContent();
			}
		}
		allSceneElements = list.ToArray();
		foreach (UISceneElement item in hashSet)
		{
			Object.Destroy(item.gameObject);
		}
	}

	public void RefreshContent()
	{
		UISceneElement[] array = allSceneElements;
		foreach (UISceneElement uISceneElement in array)
		{
			uISceneElement.RefreshContent();
		}
	}

	public virtual void Show()
	{
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		Canvas.ForceUpdateCanvases();
	}

	public virtual void HideAll()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}

	private void OnDestroy()
	{
		BWPendingPayouts.RemovePendingPayoutsListener(OnPendingPayoutsRecieved);
	}

	public virtual void HandleSearchRequest(string searchStr)
	{
		if (!string.IsNullOrEmpty(searchStr))
		{
			UISceneInfo uISceneInfo = new UISceneInfo();
			uISceneInfo.path = "PlayMenu/WorldBrowser";
			uISceneInfo.title = "World Search Results: \"" + searchStr + "\"";
			uISceneInfo.dataType = "WorldSearch";
			uISceneInfo.dataSubtype = searchStr;
			MainUIController.Instance.LoadUIScene(uISceneInfo, back: false, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
		}
	}

	public virtual void SceneBackgroundSelected()
	{
		if (allSceneElements != null)
		{
			UISceneElement[] array = allSceneElements;
			foreach (UISceneElement uISceneElement in array)
			{
				uISceneElement.ClearSelection();
			}
		}
	}

	public bool IsDisplayReady()
	{
		return displayReady;
	}

	public virtual void SceneWillUnload()
	{
		BWPendingPayouts.RemovePendingPayoutsListener(OnPendingPayoutsRecieved);
		DoSceneHide();
	}

	public void DoSceneReveal()
	{
		if (sceneAnimator != null)
		{
			sceneAnimator.SetTrigger("Reveal");
		}
	}

	protected void DoSceneHide()
	{
		if (sceneAnimator != null)
		{
			sceneAnimatorHideDelay = 5;
			sceneAnimator.SetTrigger("Hide");
		}
	}

	public bool SceneAnimationInProgress()
	{
		if (sceneAnimator == null)
		{
			return false;
		}
		if (sceneAnimatorHideDelay > 0)
		{
			sceneAnimatorHideDelay--;
			return true;
		}
		if (!sceneAnimator.IsInTransition(0))
		{
			return sceneAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.99f;
		}
		return true;
	}

	public void SelectItemInSceneElements(string itemID, string dataType, string dataSubtype)
	{
		UISceneElement[] array = allSceneElements;
		foreach (UISceneElement uISceneElement in array)
		{
			if ((string.IsNullOrEmpty(dataType) || !(dataType != uISceneElement.dataType)) && (string.IsNullOrEmpty(dataSubtype) || !(dataSubtype != uISceneElement.dataSubtype)) && uISceneElement.SelectItemWithID(itemID))
			{
				return;
			}
		}
		if (string.IsNullOrEmpty(dataType))
		{
			return;
		}
		UISceneElement[] array2 = allSceneElements;
		foreach (UISceneElement uISceneElement2 in array2)
		{
			if (uISceneElement2.SelectItemWithID(itemID))
			{
				break;
			}
		}
	}

	public GameObject CloneItemInSceneElements(string itemID, string dataType, string dataSubtype)
	{
		UISceneElement[] array = allSceneElements;
		foreach (UISceneElement uISceneElement in array)
		{
			if ((string.IsNullOrEmpty(dataType) || dataType == uISceneElement.dataType) && (string.IsNullOrEmpty(dataSubtype) || dataSubtype == uISceneElement.dataSubtype))
			{
				return uISceneElement.CloneItemWithID(itemID);
			}
		}
		return null;
	}

	public void TriggerDetailPanelAnimation(string detailPanelID, string animKey, string animTrigger)
	{
		ItemPanelPagedList[] array = allLists;
		foreach (ItemPanelPagedList itemPanelPagedList in array)
		{
			DetailPanel detailView = itemPanelPagedList.itemList.detailView;
			if (detailView != null && detailView.contentId == detailPanelID)
			{
				detailView.TriggerAnimation(animKey, animTrigger);
			}
		}
	}

	public void ResetDetailPanelAnimation(string detailPanelID)
	{
		ItemPanelPagedList[] array = allLists;
		foreach (ItemPanelPagedList itemPanelPagedList in array)
		{
			DetailPanel detailView = itemPanelPagedList.itemList.detailView;
			if (detailView != null && detailView.contentId == detailPanelID)
			{
				detailView.ResetAnimation();
			}
		}
	}

	public void ItemInPagedListSelected(ItemPanelPagedList pagedList, int index, bool selectedByClick)
	{
		bool flag = false;
		ItemPanelPagedList[] array = allLists;
		foreach (ItemPanelPagedList itemPanelPagedList in array)
		{
			if (itemPanelPagedList != pagedList)
			{
				flag |= itemPanelPagedList.itemList.selectedItem != null;
				itemPanelPagedList.itemList.ClearSelection();
				bool immediate = !selectedByClick;
				itemPanelPagedList.HideDetailPanel(immediate);
			}
		}
	}

	public virtual void HandleMenuInputEvents()
	{
		if (MappedInput.InputDown(MappableInput.MENU_CANCEL))
		{
			MainUIController.Instance.NavigateBack();
		}
	}

	private void OnPendingPayoutsRecieved(List<BWPendingPayout> payouts)
	{
		BWStandalone.Overlays.ShowPopupPendingPayouts();
	}

	public void LoadExampleContent(int exampleContentSize)
	{
		GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>("SharedUIResources"));
		sharedUIResources = gameObject.GetComponent<SharedUIResources>();
		sharedUIResources.dataManager.InitWithExampleData(exampleContentSize);
		LayoutExampleContent(exampleContentSize);
		Object.DestroyImmediate(gameObject);
		sharedUIResources = null;
	}

	protected virtual void LayoutExampleContent(int exampleContentSize)
	{
		ClearExampleContent();
		UISceneElement[] componentsInChildren = GetComponentsInChildren<UISceneElement>();
		foreach (UISceneElement uISceneElement in componentsInChildren)
		{
			uISceneElement.Init();
			if (uISceneElement.getDataTypeFromSceneParameters)
			{
				uISceneElement.dataType = sceneInfo.dataType;
				uISceneElement.dataSubtype = sceneInfo.dataSubtype;
			}
			uISceneElement.LoadContent(sharedUIResources.dataManager, sharedUIResources.imageManager);
		}
	}

	public virtual void ClearExampleContent()
	{
		UISceneElement[] componentsInChildren = GetComponentsInChildren<UISceneElement>();
		foreach (UISceneElement uISceneElement in componentsInChildren)
		{
			uISceneElement.Init();
			uISceneElement.UnloadEditorExampleContent();
		}
	}
}
