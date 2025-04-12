using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x0200043C RID: 1084
public class MainUIController : MonoBehaviour
{
	// Token: 0x17000249 RID: 585
	// (get) Token: 0x06002E63 RID: 11875 RVA: 0x0014A53B File Offset: 0x0014893B
	public static MainUIController Instance
	{
		get
		{
			return MainUIController._instance;
		}
	}

	// Token: 0x1700024A RID: 586
	// (get) Token: 0x06002E64 RID: 11876 RVA: 0x0014A542 File Offset: 0x00148942
	public static bool active
	{
		get
		{
			return MainUIController._instance != null && MainUIController._instance.isActiveAndEnabled;
		}
	}

	// Token: 0x06002E65 RID: 11877 RVA: 0x0014A564 File Offset: 0x00148964
	private void Awake()
	{
		if (MainUIController._instance != null)
		{
			return;
		}
		MainUIController._instance = this;
		if (this.sharedUIResources == null)
		{
			this.sharedUIResources = Resources.Load<SharedUIResources>("SharedUIResources");
		}
		this.dataManager = this.sharedUIResources.dataManager;
		this.imageManager = this.sharedUIResources.imageManager;
	}

	// Token: 0x1700024B RID: 587
	// (get) Token: 0x06002E66 RID: 11878 RVA: 0x0014A5CB File Offset: 0x001489CB
	// (set) Token: 0x06002E67 RID: 11879 RVA: 0x0014A5D3 File Offset: 0x001489D3
	public UISceneBase loadedSceneController { get; private set; }

	// Token: 0x1700024C RID: 588
	// (get) Token: 0x06002E68 RID: 11880 RVA: 0x0014A5DC File Offset: 0x001489DC
	// (set) Token: 0x06002E69 RID: 11881 RVA: 0x0014A5E4 File Offset: 0x001489E4
	public bool hasLoadedScene { get; private set; }

	// Token: 0x1700024D RID: 589
	// (get) Token: 0x06002E6A RID: 11882 RVA: 0x0014A5ED File Offset: 0x001489ED
	public static string LoadedSceneInfoPath
	{
		get
		{
			if (MainUIController.Instance.loadedSceneInfo != null)
			{
				return MainUIController.Instance.loadedSceneInfo.path;
			}
			return string.Empty;
		}
	}

	// Token: 0x06002E6B RID: 11883 RVA: 0x0014A614 File Offset: 0x00148A14
	private IEnumerator Start()
	{
		while (BWStandalone.Instance == null || BWStandalone.Overlays == null)
		{
			yield return null;
		}
		this.mainCanvas = base.GetComponentInChildren<Canvas>();
		if (this.menuBar != null)
		{
			this.menuBar.Init(this.dataManager, this.imageManager);
		}
		if (this.autoLoadStartScene)
		{
			this.history = new Stack<UISceneInfo>();
			this.future = new Stack<UISceneInfo>();
			this.LoadUIScene(new UISceneInfo
			{
				path = this.startScene
			}, false, SceneTransitionStyle.None, SceneTransitionStyle.None);
		}
		yield break;
	}

	// Token: 0x06002E6C RID: 11884 RVA: 0x0014A62F File Offset: 0x00148A2F
	private void Update()
	{
		MappedInput.Update();
	}

	// Token: 0x06002E6D RID: 11885 RVA: 0x0014A638 File Offset: 0x00148A38
	public void HideAll()
	{
		this.menuCam.gameObject.SetActive(false);
		this.mainCanvas.enabled = false;
		this.mainCanvas.gameObject.SetActive(false);
		if (this.hasLoadedScene)
		{
			this.loadedSceneController.HideAll();
		}
		this.imageManager.ClearListeners();
		base.enabled = false;
	}

	// Token: 0x06002E6E RID: 11886 RVA: 0x0014A69C File Offset: 0x00148A9C
	public void Show()
	{
		this.mainCanvas.gameObject.SetActive(true);
		this.mainCanvas.enabled = true;
		this.menuCam.gameObject.SetActive(true);
		this.loadedSceneController.Show();
		base.enabled = true;
	}

	// Token: 0x06002E6F RID: 11887 RVA: 0x0014A6E9 File Offset: 0x00148AE9
	public void SceneBackgroundSelected()
	{
		if (this.loadedSceneController != null)
		{
			this.loadedSceneController.SceneBackgroundSelected();
		}
	}

	// Token: 0x06002E70 RID: 11888 RVA: 0x0014A707 File Offset: 0x00148B07
	public bool IsLoadingScene()
	{
		return this.isLoadingScene;
	}

	// Token: 0x06002E71 RID: 11889 RVA: 0x0014A710 File Offset: 0x00148B10
	public void LoadUIScene(string scenePath)
	{
		this.LoadUIScene(new UISceneInfo
		{
			path = scenePath
		});
	}

	// Token: 0x06002E72 RID: 11890 RVA: 0x0014A731 File Offset: 0x00148B31
	public void LoadUIScene(UISceneInfo sceneInfo)
	{
		this.LoadUIScene(sceneInfo, false, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
	}

	// Token: 0x06002E73 RID: 11891 RVA: 0x0014A740 File Offset: 0x00148B40
	public void LoadUIScene(UISceneInfo sceneInfo, bool back, SceneTransitionStyle transitionStyleOut, SceneTransitionStyle transitionStyleIn)
	{
		if (this.isLoadingScene || this.isUnloadingScene || BWStandalone.Instance.addingItemToCart)
		{
			BWLog.Info("Navigation busy...");
			return;
		}
		BWLog.Info(string.Concat(new string[]
		{
			"Navigating to scene: ",
			sceneInfo.path,
			" title:",
			sceneInfo.title,
			" dataType: ",
			sceneInfo.dataType,
			" subtype:",
			sceneInfo.dataSubtype
		}));
		if (this.hasLoadedScene && sceneInfo.path == this.loadedSceneInfo.path)
		{
			this.RefreshLoadedPage(sceneInfo);
			return;
		}
		if (back)
		{
			this.future.Push(this.loadedSceneInfo);
			this.menuBar.ShowForwardButton();
		}
		else if (this.hasLoadedScene)
		{
			this.history.Push(this.loadedSceneInfo);
			this.menuBar.ShowBackButton();
		}
		else
		{
			this.dataManager.Init();
			this.menuBar.HideBackButton();
			this.menuBar.HideForwardButton();
		}
		if (this.hasLoadedScene && this.loadedSceneController != null)
		{
			this.loadedSceneController.SceneWillUnload();
		}
		base.StartCoroutine(this.DoSceneTransition(sceneInfo, transitionStyleOut, transitionStyleIn));
	}

	// Token: 0x06002E74 RID: 11892 RVA: 0x0014A8AB File Offset: 0x00148CAB
	public void RefreshLoadedPage()
	{
		this.RefreshLoadedPage(this.loadedSceneInfo);
	}

	// Token: 0x06002E75 RID: 11893 RVA: 0x0014A8B9 File Offset: 0x00148CB9
	private void RefreshLoadedPage(UISceneInfo sceneInfo)
	{
		this.loadedSceneInfo = sceneInfo;
		this.loadedSceneController.StopAllCoroutines();
		this.loadedSceneController.ClearContent();
		this.loadedSceneController.OnSceneLoad(this, this.loadedSceneInfo);
	}

	// Token: 0x06002E76 RID: 11894 RVA: 0x0014A8EC File Offset: 0x00148CEC
	public void NavigateBack()
	{
		if (this.history.Count == 0)
		{
			return;
		}
		UISceneInfo sceneInfo = this.history.Pop();
		if (this.history.Count == 0)
		{
			this.menuBar.HideBackButton();
		}
		this.LoadUIScene(sceneInfo, true, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
	}

	// Token: 0x06002E77 RID: 11895 RVA: 0x0014A93C File Offset: 0x00148D3C
	public void NavigateForward()
	{
		if (this.future.Count == 0)
		{
			return;
		}
		UISceneInfo sceneInfo = this.future.Pop();
		if (this.future.Count == 0)
		{
			this.menuBar.HideForwardButton();
		}
		this.LoadUIScene(sceneInfo, false, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
	}

	// Token: 0x06002E78 RID: 11896 RVA: 0x0014A98B File Offset: 0x00148D8B
	public void Search(string searchStr)
	{
		if (base.isActiveAndEnabled && this.loadedSceneController != null && WorldSession.current == null)
		{
			this.loadedSceneController.HandleSearchRequest(searchStr);
		}
	}

	// Token: 0x06002E79 RID: 11897 RVA: 0x0014A9BF File Offset: 0x00148DBF
	public bool HandleMessage(string messageStr, string senderId, string senderDataType, string senderDataSubtype)
	{
		return !(BWStandalone.Instance != null) || BWStandalone.Instance.HandleMenuUIMessage(messageStr, senderId, senderDataType, senderDataSubtype);
	}

	// Token: 0x06002E7A RID: 11898 RVA: 0x0014A9E4 File Offset: 0x00148DE4
	private IEnumerator DoSceneTransition(UISceneInfo toSceneInfo, SceneTransitionStyle transitionStyleOut, SceneTransitionStyle transitionStyleIn)
	{
		this.isLoadingScene = true;
		BWStandalone.Overlays.SetUIBusy(true);
		this.menuBar.SetInteractable(false);
		yield return null;
		string fullScenePath = this.FullScenePath(toSceneInfo.path);
		AsyncOperation sceneLoader = SceneManager.LoadSceneAsync(fullScenePath, LoadSceneMode.Additive);
		while (!sceneLoader.isDone)
		{
			yield return null;
		}
		Scene newScene = SceneManager.GetSceneByPath(fullScenePath);
		UISceneBase fromSceneController = this.loadedSceneController;
		HashSet<GameObject> toDestroy = new HashSet<GameObject>();
		foreach (GameObject gameObject in newScene.GetRootGameObjects())
		{
			UISceneBase uiScene = gameObject.GetComponent<UISceneBase>();
			if (uiScene != null)
			{
				this.loadedSceneController = uiScene;
				this.dataManager.ClearListeners();
				this.imageManager.ClearListeners();
				break;
			}
			UISceneTempObjects component = gameObject.GetComponent<UISceneTempObjects>();
			if (component != null)
			{
				toDestroy.Add(component.gameObject);
			}
		}
		foreach (GameObject obj in toDestroy)
		{
			UnityEngine.Object.Destroy(obj);
		}
		Animator fromSceneTransitionAnimator = null;
		Animator toSceneTransitionAnimator = null;
		if (transitionStyleOut != SceneTransitionStyle.None && this.hasLoadedScene && fromSceneController != null)
		{
			GameObject sceneTransitionObject = this.GetSceneTransitionObject(transitionStyleOut);
			fromSceneTransitionAnimator = sceneTransitionObject.GetComponentInChildren<Animator>();
			RectTransform parent = (RectTransform)fromSceneTransitionAnimator.transform;
			RectTransform rectTransform = (RectTransform)fromSceneController.transform;
			rectTransform.SetParent(parent, true);
		}
		else if (fromSceneController != null)
		{
			fromSceneController.GetComponent<Canvas>().enabled = false;
		}
		SceneManager.SetActiveScene(newScene);
		this.Show();
		BWStandalone.Overlays.ShowLoadingOverlay(false);
		RectTransform loadedSceneRootTransform = (RectTransform)this.loadedSceneController.transform;
		if (transitionStyleIn != SceneTransitionStyle.None)
		{
			GameObject toSceneTransitionObject = this.GetSceneTransitionObject(transitionStyleIn);
			toSceneTransitionAnimator = toSceneTransitionObject.GetComponentInChildren<Animator>();
			RectTransform toSceneTransformParent = (RectTransform)toSceneTransitionAnimator.transform;
			loadedSceneRootTransform.SetParent(toSceneTransformParent, true);
			toSceneTransitionAnimator.SetTrigger("SetHidden");
			yield return null;
			if (fromSceneTransitionAnimator != null)
			{
				fromSceneTransitionAnimator.SetTrigger("Hide");
			}
			toSceneTransitionAnimator.SetTrigger("Show");
			bool transitionAnimComplete = false;
			this.loadedSceneController.OnSceneLoad(this, toSceneInfo);
			this.loadedSceneController.GetComponent<Canvas>().enabled = true;
			this.loadedSceneController.DoSceneReveal();
			while (!transitionAnimComplete)
			{
				transitionAnimComplete = toSceneTransitionAnimator.GetCurrentAnimatorStateInfo(0).IsName("Showing");
				yield return null;
			}
			loadedSceneRootTransform.SetParent(null);
			UnityEngine.Object.Destroy(toSceneTransitionObject);
		}
		else
		{
			this.loadedSceneController.OnSceneLoad(this, toSceneInfo);
			this.loadedSceneController.GetComponent<Canvas>().enabled = true;
			this.loadedSceneController.DoSceneReveal();
		}
		while (!this.loadedSceneController.IsDisplayReady())
		{
			yield return null;
		}
		if (this.hasLoadedScene)
		{
			this.isUnloadingScene = true;
			AsyncOperation sceneUnloader = SceneManager.UnloadSceneAsync(this.FullScenePath(this.loadedSceneInfo.path));
			while (!sceneUnloader.isDone)
			{
				yield return null;
			}
			this.isUnloadingScene = false;
		}
		this.menuBar.OnSceneLoad();
		while (!this.loadedSceneController.IsDisplayReady())
		{
			yield return null;
		}
		this.menuBar.SetInteractable(true);
		this.loadedSceneInfo = new UISceneInfo();
		this.loadedSceneInfo.path = toSceneInfo.path;
		this.loadedSceneInfo.title = toSceneInfo.title;
		this.loadedSceneInfo.userImageUrl = toSceneInfo.userImageUrl;
		this.loadedSceneInfo.dataType = toSceneInfo.dataType;
		this.loadedSceneInfo.dataSubtype = toSceneInfo.dataSubtype;
		this.isLoadingScene = false;
		this.hasLoadedScene = true;
		BWStandalone.Overlays.SetUIBusy(false);
		Canvas canvas = loadedSceneController.GetComponent<Canvas>();
		Exdilin.UI.UIHelper.CreateLabelGameObject(loadedSceneController, 200, 600, "Hello World");
		yield break;
	}

	// Token: 0x06002E7B RID: 11899 RVA: 0x0014AA14 File Offset: 0x00148E14
	private GameObject GetSceneTransitionObject(SceneTransitionStyle style)
	{
		if (style == SceneTransitionStyle.Fade)
		{
			return UnityEngine.Object.Instantiate<GameObject>(this.sceneTransitionPrefabFade);
		}
		if (style == SceneTransitionStyle.SlideLeft)
		{
			return UnityEngine.Object.Instantiate<GameObject>(this.sceneTransitionPrefabSlideLeft);
		}
		if (style != SceneTransitionStyle.SlideRight)
		{
			return null;
		}
		return UnityEngine.Object.Instantiate<GameObject>(this.sceneTransitionPrefabSlideRight);
	}

	// Token: 0x06002E7C RID: 11900 RVA: 0x0014AA60 File Offset: 0x00148E60
	private int SceneDepth(string scenePath)
	{
		return scenePath.Split(new char[]
		{
			'/'
		}).Length;
	}

	// Token: 0x06002E7D RID: 11901 RVA: 0x0014AA78 File Offset: 0x00148E78
	private string FullScenePath(string shortPath)
	{
		return "Assets/Universal/Scenes/MenuScenes/Root/" + shortPath + ".unity";
	}

	// Token: 0x040026ED RID: 9965
	private static MainUIController _instance;

	// Token: 0x040026EE RID: 9966
	public string startScene = "PlayMenu";

	// Token: 0x040026EF RID: 9967
	public MenuBar menuBar;

	// Token: 0x040026F0 RID: 9968
	public Camera menuCam;

	// Token: 0x040026F1 RID: 9969
	public GameObject sceneTransitionPrefabSlideLeft;

	// Token: 0x040026F2 RID: 9970
	public GameObject sceneTransitionPrefabSlideRight;

	// Token: 0x040026F3 RID: 9971
	public GameObject sceneTransitionPrefabFade;

	// Token: 0x040026F4 RID: 9972
	public SharedUIResources sharedUIResources;

	// Token: 0x040026F5 RID: 9973
	public UINavDebug navDebug;

	// Token: 0x040026F6 RID: 9974
	[HideInInspector]
	public UIDataManager dataManager;

	// Token: 0x040026F7 RID: 9975
	[HideInInspector]
	public ImageManager imageManager;

	// Token: 0x040026F8 RID: 9976
	public bool autoLoadStartScene = true;

	// Token: 0x040026FA RID: 9978
	private bool isLoadingScene;

	// Token: 0x040026FB RID: 9979
	private bool isUnloadingScene;

	// Token: 0x040026FD RID: 9981
	private UISceneInfo loadedSceneInfo;

	// Token: 0x040026FE RID: 9982
	private Stack<UISceneInfo> history;

	// Token: 0x040026FF RID: 9983
	private Stack<UISceneInfo> future;

	// Token: 0x04002700 RID: 9984
	private Canvas mainCanvas;
}
