using UnityEngine;
using UnityEngine.UI;

public class UISceneElement : MonoBehaviour
{
	public string dataType;

	public string dataSubtype;

	public bool getDataTypeFromSceneParameters;

	public bool getDataSubtypeFromSceneParameters;

	public bool getIDFromParentPanel;

	public bool forceReloadData;

	public bool autoLoadOnStart = true;

	public bool delayLoadUntilOnScreen;

	public bool unloadWhenOffScreen;

	public Selectable defaultSelectable;

	public UIPanelContents clonePanelPrefab;

	public UISceneElement previousElement;

	public UISceneElement nextElement;

	[HideInInspector]
	public bool deleteOnSceneRefresh;

	protected UIDataSource dataSource;

	protected UIDataManager dataManager;

	protected ImageManager imageManager;

	private Vector3[] worldSpaceCorners;

	private RectTransform rectTransform;

	private bool noData;

	private bool onScreen;

	private bool loadingContent;

	public virtual void OnEnable()
	{
		if (MainUIController.active && autoLoadOnStart)
		{
			Init();
			LoadContent(MainUIController.Instance.dataManager, MainUIController.Instance.imageManager);
		}
		rectTransform = (RectTransform)base.transform;
		worldSpaceCorners = new Vector3[4];
		for (int i = 0; i < 4; i++)
		{
			worldSpaceCorners[i] = Vector3.zero;
		}
	}

	public void Update()
	{
		if (dataSource == null || (!delayLoadUntilOnScreen && !unloadWhenOffScreen))
		{
			return;
		}
		float num = (float)Screen.height * 0.7f;
		Rect rect = new Rect(0f, 0f - num, Screen.width, (float)Screen.height + 2f * num);
		rectTransform.GetWorldCorners(worldSpaceCorners);
		onScreen = false;
		for (int i = 0; i < 4; i++)
		{
			if (rect.Contains(worldSpaceCorners[i]))
			{
				onScreen = true;
				break;
			}
		}
		if (onScreen && delayLoadUntilOnScreen && !loadingContent)
		{
			loadingContent = true;
			LoadContentFromDataSource();
		}
		else if (!onScreen && unloadWhenOffScreen && loadingContent)
		{
			loadingContent = false;
			UnloadContent();
		}
	}

	public virtual void Init()
	{
	}

	public void LoadContent(UIDataManager dataManager, ImageManager imageManager)
	{
		this.dataManager = dataManager;
		this.imageManager = imageManager;
		dataSource = dataManager.GetDataSource(dataType, dataSubtype);
		if (dataSource != null)
		{
			UIPanelContents[] componentsInChildren = GetComponentsInChildren<UIPanelContents>();
			foreach (UIPanelContents uIPanelContents in componentsInChildren)
			{
				if (!(uIPanelContents == null) && uIPanelContents.loadFromDataSourceInfo)
				{
					dataSource.LoadIfNeeded();
					uIPanelContents.Init();
					uIPanelContents.SetupPanel(dataSource, imageManager);
				}
			}
			if (!delayLoadUntilOnScreen)
			{
				LoadContentFromDataSource();
			}
		}
		else
		{
			noData = true;
		}
	}

	public void RefreshContent()
	{
		if (dataSource != null)
		{
			dataSource.ClearData();
			dataSource.Refresh();
		}
	}

	protected virtual void LoadContentFromDataSource()
	{
	}

	public virtual void ClearSelection()
	{
	}

	public virtual void UnloadContent()
	{
	}

	public virtual bool SelectItemWithID(string itemID)
	{
		return false;
	}

	public virtual GameObject CloneItemWithID(string itemID)
	{
		if (clonePanelPrefab == null)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(clonePanelPrefab.gameObject);
		UIPanelContents component = gameObject.GetComponent<UIPanelContents>();
		component.SetupPanel(dataSource, imageManager, itemID);
		return gameObject;
	}

	public virtual void UnloadEditorExampleContent()
	{
	}

	public virtual bool ContentLoaded()
	{
		if (!delayLoadUntilOnScreen && !noData)
		{
			if (dataSource != null)
			{
				if (!dataSource.IsDataLoaded())
				{
					return dataSource.DataLoadFailed();
				}
				return true;
			}
			return false;
		}
		return true;
	}

	protected void ShowGroup(CanvasGroup canvasGroup, bool show)
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = ((!show) ? 0f : 1f);
			canvasGroup.interactable = show;
			canvasGroup.blocksRaycasts = show;
		}
	}
}
