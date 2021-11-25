using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200042F RID: 1071
public class UISceneElement : MonoBehaviour
{
	// Token: 0x06002E29 RID: 11817 RVA: 0x001460EC File Offset: 0x001444EC
	public virtual void OnEnable()
	{
		if (MainUIController.active && this.autoLoadOnStart)
		{
			this.Init();
			this.LoadContent(MainUIController.Instance.dataManager, MainUIController.Instance.imageManager);
		}
		this.rectTransform = (RectTransform)base.transform;
		this.worldSpaceCorners = new Vector3[4];
		for (int i = 0; i < 4; i++)
		{
			this.worldSpaceCorners[i] = Vector3.zero;
		}
	}

	// Token: 0x06002E2A RID: 11818 RVA: 0x00146174 File Offset: 0x00144574
	public void Update()
	{
		if (this.dataSource == null)
		{
			return;
		}
		if (this.delayLoadUntilOnScreen || this.unloadWhenOffScreen)
		{
			float num = (float)Screen.height * 0.7f;
			Rect rect = new Rect(0f, -num, (float)Screen.width, (float)Screen.height + 2f * num);
			this.rectTransform.GetWorldCorners(this.worldSpaceCorners);
			this.onScreen = false;
			for (int i = 0; i < 4; i++)
			{
				if (rect.Contains(this.worldSpaceCorners[i]))
				{
					this.onScreen = true;
					break;
				}
			}
			if (this.onScreen && this.delayLoadUntilOnScreen && !this.loadingContent)
			{
				this.loadingContent = true;
				this.LoadContentFromDataSource();
			}
			else if (!this.onScreen && this.unloadWhenOffScreen && this.loadingContent)
			{
				this.loadingContent = false;
				this.UnloadContent();
			}
		}
	}

	// Token: 0x06002E2B RID: 11819 RVA: 0x00146286 File Offset: 0x00144686
	public virtual void Init()
	{
	}

	// Token: 0x06002E2C RID: 11820 RVA: 0x00146288 File Offset: 0x00144688
	public void LoadContent(UIDataManager dataManager, ImageManager imageManager)
	{
		this.dataManager = dataManager;
		this.imageManager = imageManager;
		this.dataSource = dataManager.GetDataSource(this.dataType, this.dataSubtype);
		if (this.dataSource != null)
		{
			foreach (UIPanelContents uipanelContents in base.GetComponentsInChildren<UIPanelContents>())
			{
				if (!(uipanelContents == null))
				{
					if (uipanelContents.loadFromDataSourceInfo)
					{
						this.dataSource.LoadIfNeeded();
						uipanelContents.Init();
						uipanelContents.SetupPanel(this.dataSource, imageManager);
					}
				}
			}
			if (!this.delayLoadUntilOnScreen)
			{
				this.LoadContentFromDataSource();
			}
		}
		else
		{
			this.noData = true;
		}
	}

	// Token: 0x06002E2D RID: 11821 RVA: 0x0014633C File Offset: 0x0014473C
	public void RefreshContent()
	{
		if (this.dataSource != null)
		{
			this.dataSource.ClearData();
			this.dataSource.Refresh();
		}
	}

	// Token: 0x06002E2E RID: 11822 RVA: 0x0014635F File Offset: 0x0014475F
	protected virtual void LoadContentFromDataSource()
	{
	}

	// Token: 0x06002E2F RID: 11823 RVA: 0x00146361 File Offset: 0x00144761
	public virtual void ClearSelection()
	{
	}

	// Token: 0x06002E30 RID: 11824 RVA: 0x00146363 File Offset: 0x00144763
	public virtual void UnloadContent()
	{
	}

	// Token: 0x06002E31 RID: 11825 RVA: 0x00146365 File Offset: 0x00144765
	public virtual bool SelectItemWithID(string itemID)
	{
		return false;
	}

	// Token: 0x06002E32 RID: 11826 RVA: 0x00146368 File Offset: 0x00144768
	public virtual GameObject CloneItemWithID(string itemID)
	{
		if (this.clonePanelPrefab == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.clonePanelPrefab.gameObject);
		UIPanelContents component = gameObject.GetComponent<UIPanelContents>();
		component.SetupPanel(this.dataSource, this.imageManager, itemID);
		return gameObject;
	}

	// Token: 0x06002E33 RID: 11827 RVA: 0x001463B4 File Offset: 0x001447B4
	public virtual void UnloadEditorExampleContent()
	{
	}

	// Token: 0x06002E34 RID: 11828 RVA: 0x001463B8 File Offset: 0x001447B8
	public virtual bool ContentLoaded()
	{
		return this.delayLoadUntilOnScreen || this.noData || (this.dataSource != null && (this.dataSource.IsDataLoaded() || this.dataSource.DataLoadFailed()));
	}

	// Token: 0x06002E35 RID: 11829 RVA: 0x0014640A File Offset: 0x0014480A
	protected void ShowGroup(CanvasGroup canvasGroup, bool show)
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = ((!show) ? 0f : 1f);
			canvasGroup.interactable = show;
			canvasGroup.blocksRaycasts = show;
		}
	}

	// Token: 0x0400269C RID: 9884
	public string dataType;

	// Token: 0x0400269D RID: 9885
	public string dataSubtype;

	// Token: 0x0400269E RID: 9886
	public bool getDataTypeFromSceneParameters;

	// Token: 0x0400269F RID: 9887
	public bool getDataSubtypeFromSceneParameters;

	// Token: 0x040026A0 RID: 9888
	public bool getIDFromParentPanel;

	// Token: 0x040026A1 RID: 9889
	public bool forceReloadData;

	// Token: 0x040026A2 RID: 9890
	public bool autoLoadOnStart = true;

	// Token: 0x040026A3 RID: 9891
	public bool delayLoadUntilOnScreen;

	// Token: 0x040026A4 RID: 9892
	public bool unloadWhenOffScreen;

	// Token: 0x040026A5 RID: 9893
	public Selectable defaultSelectable;

	// Token: 0x040026A6 RID: 9894
	public UIPanelContents clonePanelPrefab;

	// Token: 0x040026A7 RID: 9895
	public UISceneElement previousElement;

	// Token: 0x040026A8 RID: 9896
	public UISceneElement nextElement;

	// Token: 0x040026A9 RID: 9897
	[HideInInspector]
	public bool deleteOnSceneRefresh;

	// Token: 0x040026AA RID: 9898
	protected UIDataSource dataSource;

	// Token: 0x040026AB RID: 9899
	protected UIDataManager dataManager;

	// Token: 0x040026AC RID: 9900
	protected ImageManager imageManager;

	// Token: 0x040026AD RID: 9901
	private Vector3[] worldSpaceCorners;

	// Token: 0x040026AE RID: 9902
	private RectTransform rectTransform;

	// Token: 0x040026AF RID: 9903
	private bool noData;

	// Token: 0x040026B0 RID: 9904
	private bool onScreen;

	// Token: 0x040026B1 RID: 9905
	private bool loadingContent;
}
