using System.Collections;
using System.Collections.Generic;
using Exdilin.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainUIController : MonoBehaviour
{
	private static MainUIController _instance;

	public string startScene = "PlayMenu";

	public MenuBar menuBar;

	public Camera menuCam;

	public GameObject sceneTransitionPrefabSlideLeft;

	public GameObject sceneTransitionPrefabSlideRight;

	public GameObject sceneTransitionPrefabFade;

	public SharedUIResources sharedUIResources;

	public UINavDebug navDebug;

	[HideInInspector]
	public UIDataManager dataManager;

	[HideInInspector]
	public ImageManager imageManager;

	public bool autoLoadStartScene = true;

	private bool isLoadingScene;

	private bool isUnloadingScene;

	private UISceneInfo loadedSceneInfo;

	private Stack<UISceneInfo> history;

	private Stack<UISceneInfo> future;

	private Canvas mainCanvas;

	public static MainUIController Instance => _instance;

	public static bool active
	{
		get
		{
			if (_instance != null)
			{
				return _instance.isActiveAndEnabled;
			}
			return false;
		}
	}

	public UISceneBase loadedSceneController { get; private set; }

	public bool hasLoadedScene { get; private set; }

	public static string LoadedSceneInfoPath
	{
		get
		{
			if (Instance.loadedSceneInfo != null)
			{
				return Instance.loadedSceneInfo.path;
			}
			return string.Empty;
		}
	}

	private void Awake()
	{
		if (!(_instance != null))
		{
			_instance = this;
			if (sharedUIResources == null)
			{
				sharedUIResources = Resources.Load<SharedUIResources>("SharedUIResources");
			}
			dataManager = sharedUIResources.dataManager;
			imageManager = sharedUIResources.imageManager;
		}
	}

	private IEnumerator Start()
	{
		while (BWStandalone.Instance == null || BWStandalone.Overlays == null)
		{
			yield return null;
		}
		mainCanvas = GetComponentInChildren<Canvas>();
		if (menuBar != null)
		{
			menuBar.Init(dataManager, imageManager);
		}
		if (autoLoadStartScene)
		{
			history = new Stack<UISceneInfo>();
			future = new Stack<UISceneInfo>();
			LoadUIScene(new UISceneInfo
			{
				path = startScene
			}, back: false, SceneTransitionStyle.None, SceneTransitionStyle.None);
		}
	}

	private void Update()
	{
		MappedInput.Update();
	}

	public void HideAll()
	{
		menuCam.gameObject.SetActive(value: false);
		mainCanvas.enabled = false;
		mainCanvas.gameObject.SetActive(value: false);
		if (hasLoadedScene)
		{
			loadedSceneController.HideAll();
		}
		imageManager.ClearListeners();
		base.enabled = false;
	}

	public void Show()
	{
		mainCanvas.gameObject.SetActive(value: true);
		mainCanvas.enabled = true;
		menuCam.gameObject.SetActive(value: true);
		loadedSceneController.Show();
		base.enabled = true;
	}

	public void SceneBackgroundSelected()
	{
		if (loadedSceneController != null)
		{
			loadedSceneController.SceneBackgroundSelected();
		}
	}

	public bool IsLoadingScene()
	{
		return isLoadingScene;
	}

	public void LoadUIScene(string scenePath)
	{
		LoadUIScene(new UISceneInfo
		{
			path = scenePath
		});
	}

	public void LoadUIScene(UISceneInfo sceneInfo)
	{
		LoadUIScene(sceneInfo, back: false, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
	}

	public void LoadUIScene(UISceneInfo sceneInfo, bool back, SceneTransitionStyle transitionStyleOut, SceneTransitionStyle transitionStyleIn)
	{
		if (isLoadingScene || isUnloadingScene || BWStandalone.Instance.addingItemToCart)
		{
			BWLog.Info("Navigation busy...");
			return;
		}
		BWLog.Info("Navigating to scene: " + sceneInfo.path + " title:" + sceneInfo.title + " dataType: " + sceneInfo.dataType + " subtype:" + sceneInfo.dataSubtype);
		if (hasLoadedScene && sceneInfo.path == loadedSceneInfo.path)
		{
			RefreshLoadedPage(sceneInfo);
			return;
		}
		if (back)
		{
			future.Push(loadedSceneInfo);
			menuBar.ShowForwardButton();
		}
		else if (hasLoadedScene)
		{
			history.Push(loadedSceneInfo);
			menuBar.ShowBackButton();
		}
		else
		{
			dataManager.Init();
			menuBar.HideBackButton();
			menuBar.HideForwardButton();
		}
		if (hasLoadedScene && loadedSceneController != null)
		{
			loadedSceneController.SceneWillUnload();
		}
		StartCoroutine(DoSceneTransition(sceneInfo, transitionStyleOut, transitionStyleIn));
	}

	public void RefreshLoadedPage()
	{
		RefreshLoadedPage(loadedSceneInfo);
	}

	private void RefreshLoadedPage(UISceneInfo sceneInfo)
	{
		loadedSceneInfo = sceneInfo;
		loadedSceneController.StopAllCoroutines();
		loadedSceneController.ClearContent();
		loadedSceneController.OnSceneLoad(this, loadedSceneInfo);
	}

	public void NavigateBack()
	{
		if (history.Count != 0)
		{
			UISceneInfo sceneInfo = history.Pop();
			if (history.Count == 0)
			{
				menuBar.HideBackButton();
			}
			LoadUIScene(sceneInfo, back: true, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
		}
	}

	public void NavigateForward()
	{
		if (future.Count != 0)
		{
			UISceneInfo sceneInfo = future.Pop();
			if (future.Count == 0)
			{
				menuBar.HideForwardButton();
			}
			LoadUIScene(sceneInfo, back: false, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
		}
	}

	public void Search(string searchStr)
	{
		if (base.isActiveAndEnabled && loadedSceneController != null && WorldSession.current == null)
		{
			loadedSceneController.HandleSearchRequest(searchStr);
		}
	}

	public bool HandleMessage(string messageStr, string senderId, string senderDataType, string senderDataSubtype)
	{
		if (BWStandalone.Instance != null)
		{
			return BWStandalone.Instance.HandleMenuUIMessage(messageStr, senderId, senderDataType, senderDataSubtype);
		}
		return true;
	}

	private IEnumerator DoSceneTransition(UISceneInfo toSceneInfo, SceneTransitionStyle transitionStyleOut, SceneTransitionStyle transitionStyleIn)
	{
		isLoadingScene = true;
		BWStandalone.Overlays.SetUIBusy(busy: true);
		menuBar.SetInteractable(interactable: false);
		yield return null;
		string fullScenePath = FullScenePath(toSceneInfo.path);
		AsyncOperation sceneLoader = SceneManager.LoadSceneAsync(fullScenePath, LoadSceneMode.Additive);
		while (!sceneLoader.isDone)
		{
			yield return null;
		}
		Scene sceneByPath = SceneManager.GetSceneByPath(fullScenePath);
		UISceneBase uISceneBase = loadedSceneController;
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		GameObject[] rootGameObjects = sceneByPath.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			UISceneBase component = gameObject.GetComponent<UISceneBase>();
			if (component != null)
			{
				loadedSceneController = component;
				dataManager.ClearListeners();
				imageManager.ClearListeners();
				break;
			}
			UISceneTempObjects component2 = gameObject.GetComponent<UISceneTempObjects>();
			if (component2 != null)
			{
				hashSet.Add(component2.gameObject);
			}
		}
		foreach (GameObject item in hashSet)
		{
			Object.Destroy(item);
		}
		Animator fromSceneTransitionAnimator = null;
		if (transitionStyleOut != SceneTransitionStyle.None && hasLoadedScene && uISceneBase != null)
		{
			GameObject sceneTransitionObject = GetSceneTransitionObject(transitionStyleOut);
			fromSceneTransitionAnimator = sceneTransitionObject.GetComponentInChildren<Animator>();
			RectTransform parent = (RectTransform)fromSceneTransitionAnimator.transform;
			RectTransform rectTransform = (RectTransform)uISceneBase.transform;
			rectTransform.SetParent(parent, worldPositionStays: true);
		}
		else if (uISceneBase != null)
		{
			uISceneBase.GetComponent<Canvas>().enabled = false;
		}
		SceneManager.SetActiveScene(sceneByPath);
		Show();
		BWStandalone.Overlays.ShowLoadingOverlay(show: false);
		RectTransform loadedSceneRootTransform = (RectTransform)loadedSceneController.transform;
		if (transitionStyleIn != SceneTransitionStyle.None)
		{
			GameObject toSceneTransitionObject = GetSceneTransitionObject(transitionStyleIn);
			Animator toSceneTransitionAnimator = toSceneTransitionObject.GetComponentInChildren<Animator>();
			RectTransform parent2 = (RectTransform)toSceneTransitionAnimator.transform;
			loadedSceneRootTransform.SetParent(parent2, worldPositionStays: true);
			toSceneTransitionAnimator.SetTrigger("SetHidden");
			yield return null;
			if (fromSceneTransitionAnimator != null)
			{
				fromSceneTransitionAnimator.SetTrigger("Hide");
			}
			toSceneTransitionAnimator.SetTrigger("Show");
			bool transitionAnimComplete = false;
			loadedSceneController.OnSceneLoad(this, toSceneInfo);
			loadedSceneController.GetComponent<Canvas>().enabled = true;
			loadedSceneController.DoSceneReveal();
			while (!transitionAnimComplete)
			{
				transitionAnimComplete = toSceneTransitionAnimator.GetCurrentAnimatorStateInfo(0).IsName("Showing");
				yield return null;
			}
			loadedSceneRootTransform.SetParent(null);
			Object.Destroy(toSceneTransitionObject);
		}
		else
		{
			loadedSceneController.OnSceneLoad(this, toSceneInfo);
			loadedSceneController.GetComponent<Canvas>().enabled = true;
			loadedSceneController.DoSceneReveal();
		}
		while (!loadedSceneController.IsDisplayReady())
		{
			yield return null;
		}
		if (hasLoadedScene)
		{
			isUnloadingScene = true;
			AsyncOperation sceneUnloader = SceneManager.UnloadSceneAsync(FullScenePath(loadedSceneInfo.path));
			while (!sceneUnloader.isDone)
			{
				yield return null;
			}
			isUnloadingScene = false;
		}
		menuBar.OnSceneLoad();
		while (!loadedSceneController.IsDisplayReady())
		{
			yield return null;
		}
		menuBar.SetInteractable(interactable: true);
		loadedSceneInfo = new UISceneInfo();
		loadedSceneInfo.path = toSceneInfo.path;
		loadedSceneInfo.title = toSceneInfo.title;
		loadedSceneInfo.userImageUrl = toSceneInfo.userImageUrl;
		loadedSceneInfo.dataType = toSceneInfo.dataType;
		loadedSceneInfo.dataSubtype = toSceneInfo.dataSubtype;
		isLoadingScene = false;
		hasLoadedScene = true;
		BWStandalone.Overlays.SetUIBusy(busy: false);
		loadedSceneController.GetComponent<Canvas>();
		UIHelper.CreateLabelGameObject(loadedSceneController, 200f, 600f, "Hello World");
	}

	private GameObject GetSceneTransitionObject(SceneTransitionStyle style)
	{
		return style switch
		{
			SceneTransitionStyle.Fade => Object.Instantiate(sceneTransitionPrefabFade), 
			SceneTransitionStyle.SlideLeft => Object.Instantiate(sceneTransitionPrefabSlideLeft), 
			SceneTransitionStyle.SlideRight => Object.Instantiate(sceneTransitionPrefabSlideRight), 
			_ => null, 
		};
	}

	private int SceneDepth(string scenePath)
	{
		return scenePath.Split('/').Length;
	}

	private string FullScenePath(string shortPath)
	{
		return "Assets/Universal/Scenes/MenuScenes/Root/" + shortPath + ".unity";
	}
}
