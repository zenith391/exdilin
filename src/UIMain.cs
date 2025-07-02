using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMain : MonoBehaviour, IMenuInputHandler
{
	public RectTransform topLeftButtonParent;

	public GameObject undoButton;

	public GameObject redoButton;

	public GameObject likeButton;

	public GameObject vrCameraButton;

	public GameObject cheatButton;

	public GameObject radarUI;

	public GameObject[] radarUICenterTags;

	public UIControls controlsPrefab;

	public UIDialog dialogsPrefab;

	public UIOverlays overlaysPrefab;

	public UITitleBar titleBarPrefab;

	public UISidePanel sidePanelPrefab;

	public UISpeechBubbleController speechBubbleControllerPrefab;

	public UILeaderboardController leaderboardControllerPrefab;

	public UIOptionsScreen optionsScreenPrefab;

	public UILoadingScreen loadingScreenPrefab;

	public UITapedeck tapedeckPrefab;

	public UIDebug OnScreenLogPrefab;

	public UIProfileSelection profileSelectionPrefab;

	public UIButton buyModelButton;

	public UIButton addModelToCartButton;

	public UIButton buildModelButton;

	public Sprite spriteHome;

	public Sprite spritePlay;

	public Sprite spriteRestart;

	public Sprite spriteStop;

	public Sprite spritePause;

	public Sprite spriteOptions;

	public Sprite spriteCapture;

	public Sprite spriteLikeOn;

	public Sprite spriteLikeOff;

	private UIControls _controls;

	private UIDialog _dialog;

	private UIOverlays _overlay;

	private UITitleBar _titleBar;

	private UISidePanel _sidePanel;

	private UISpeechBubbleController _speechBubbleController;

	private UILeaderboardController _leaderboardController;

	private UITapedeck _tapedeck;

	private UIProfileSelection _profileSelection;

	private UIOptionsScreen _optionsScreen;

	private UILoadingScreen _worldLoadingScreen;

	private UIDebug _onScreenLog;

	private RectTransform screenSpaceParent;

	private Canvas _mainCanvas;

	private Canvas _controlsCanvas;

	private Canvas _titleBarCanvas;

	private Canvas _overlayCanvas;

	private CanvasScaler _canvasScaler;

	private Image undoBackgroundImage;

	private Image redoBackgroundImage;

	private Image undoButtonImage;

	private Image redoButtonImage;

	private Image likeButtonImage;

	private Dictionary<TILE_BUTTON, Sprite> spriteLookup;

	private TILE_BUTTON[] currentMainButtons;

	private List<GameObject> potentialBlockers = new List<GameObject>();

	public UIControls Controls => _controls;

	public UIDialog Dialog => _dialog;

	public UIOverlays Overlay => _overlay;

	public UITitleBar TitleBar => _titleBar;

	public UISidePanel SidePanel => _sidePanel;

	public UITabBar TabBar => _sidePanel.tabBarUI;

	public UIQuickSelect QuickSelect => _sidePanel.quickSelect;

	public UISpeechBubbleController SpeechBubble => _speechBubbleController;

	public UILeaderboardController Leaderboard => _leaderboardController;

	public UITapedeck Tapedeck => _tapedeck;

	public UIProfileSelection ProfileSelection => _profileSelection;

	public static UIMain CreateUI()
	{
		string path = BW.Options.UIPrefabPath();
		UIMain uIMain = UnityEngine.Object.Instantiate(Resources.Load<UIMain>(path));
		uIMain.Init();
		if (BWStandalone.Instance != null)
		{
			EventSystem componentInChildren = uIMain.GetComponentInChildren<EventSystem>();
			componentInChildren.gameObject.SetActive(value: false);
		}
		return uIMain;
	}

	public void Init()
	{
		_controls = UnityEngine.Object.Instantiate(controlsPrefab);
		_controls.Init();
		_dialog = UnityEngine.Object.Instantiate(dialogsPrefab);
		_dialog.Init();
		_overlay = UnityEngine.Object.Instantiate(overlaysPrefab);
		_overlay.Init();
		_titleBar = UnityEngine.Object.Instantiate(titleBarPrefab);
		_titleBar.Init();
		_sidePanel = UnityEngine.Object.Instantiate(sidePanelPrefab);
		_sidePanel.Init();
		_speechBubbleController = UnityEngine.Object.Instantiate(speechBubbleControllerPrefab);
		_speechBubbleController.Init();
		_leaderboardController = UnityEngine.Object.Instantiate(leaderboardControllerPrefab);
		_leaderboardController.Init();
		_tapedeck = UnityEngine.Object.Instantiate(tapedeckPrefab);
		_tapedeck.Init();
		RectTransform rectTransform = (RectTransform)_tapedeck.transform;
		rectTransform.SetParent(topLeftButtonParent, worldPositionStays: false);
		rectTransform.SetAsFirstSibling();
		_mainCanvas = GetComponent<Canvas>();
		_controlsCanvas = _controls.GetComponent<Canvas>();
		_titleBarCanvas = _titleBar.GetComponent<Canvas>();
		_overlayCanvas = _overlay.GetComponent<Canvas>();
		buyModelButton.Init();
		buyModelButton.clickAction = delegate
		{
			WorldSession.current.ConfirmModelPurchase();
		};
		buyModelButton.Hide();
		addModelToCartButton.Init();
		addModelToCartButton.clickAction = delegate
		{
			WorldSession.current.AddModelToCart();
		};
		addModelToCartButton.Hide();
		buildModelButton.Init();
		buildModelButton.clickAction = delegate
		{
			buildModelButton.Hide();
			WorldSession.current.TriggerModelTutorial();
		};
		buildModelButton.Hide();
		undoBackgroundImage = undoButton.GetComponent<Image>();
		undoButtonImage = undoButton.transform.GetChild(0).GetComponent<Image>();
		redoBackgroundImage = redoButton.GetComponent<Image>();
		redoButtonImage = redoButton.transform.GetChild(0).GetComponent<Image>();
		likeButtonImage = likeButton.GetComponent<Image>();
		_canvasScaler = GetComponent<CanvasScaler>();
		Button component = cheatButton.GetComponent<Button>();
		component.onClick.RemoveAllListeners();
		component.onClick.AddListener(ButtonPressed_Cheat);
		HideCheatButton();
		Layout();
		ViewportWatchdog.AddListener(ViewportSizeDidChange);
		potentialBlockers = new List<GameObject> { undoButton, redoButton, likeButton };
		_tapedeck.GetUIObjects(potentialBlockers);
		if (_controls.mouseAndFingerControlEnabled)
		{
			_controls.GetUIObjects(potentialBlockers);
		}
		_leaderboardController.GetUIObjects(potentialBlockers);
	}

	public void SetAllCanvasVisible(bool visible)
	{
		SetMainCanvasVisible(visible);
		SetControlsCanvasVisible(visible);
		SetTitleBarCanvasVisible(visible);
		SetOverlayCanvasVisible(visible);
	}

	public void SetMainCanvasVisible(bool visible)
	{
		_mainCanvas.enabled = visible;
	}

	public void SetControlsCanvasVisible(bool visible)
	{
		_controlsCanvas.enabled = visible;
	}

	public void SetTitleBarCanvasVisible(bool visible)
	{
		_titleBarCanvas.enabled = visible;
	}

	public void SetOverlayCanvasVisible(bool visible)
	{
		_overlayCanvas.enabled = visible;
	}

	public void SetTapedeckVisible(bool visible)
	{
		_tapedeck.gameObject.SetActive(visible);
	}

	public void HideAll()
	{
		Dialog.CloseActiveDialog();
		buyModelButton.Hide();
		addModelToCartButton.Hide();
		buildModelButton.Hide();
		SidePanel.HideSaveModelButton();
		SidePanel.HideCopyModelButton();
		Overlay.HidePurchasedBanner();
		Overlay.HideOnScreenMessage();
		HideTitleBar();
		HideControls();
		HideCheatButton();
		HideLike();
		SetVRCameraToggleButtonActive(active: false);
		SpeechBubble.ClearAll();
		if (_profileSelection != null)
		{
			UnityEngine.Object.Destroy(_profileSelection.gameObject);
		}
		_profileSelection = null;
	}

	public void HideControls()
	{
		_controls.Hide();
	}

	public void ShowTitleBar()
	{
		_titleBar.Show();
	}

	public void HideTitleBar()
	{
		_titleBar.Hide();
	}

	public void UpdateSpeechBubbles()
	{
		_speechBubbleController.UpdateSpeechBubbles();
	}

	public void UpdateTextWindows()
	{
		_speechBubbleController.UpdateTextWindows();
	}

	public void ShowBuyModelButton(int price)
	{
		buyModelButton.SetText(price.ToString());
		buyModelButton.Show();
		addModelToCartButton.SetText(price.ToString());
		addModelToCartButton.Show();
	}

	public void HideBuyModelButton()
	{
		buyModelButton.Hide();
		addModelToCartButton.Hide();
	}

	public void ShowBuildModelButton()
	{
	}

	public void HideBuildModelButton()
	{
		buildModelButton.Hide();
	}

	public void ShowCheatButton()
	{
		cheatButton.SetActive(value: true);
	}

	public void HideCheatButton()
	{
		cheatButton.SetActive(value: false);
	}

	public void MakeCheatButtonInvisible()
	{
		cheatButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
	}

	public Transform GetTransformForButton(TILE_BUTTON button)
	{
		Transform result = null;
		switch (button)
		{
		case TILE_BUTTON.TOOLS:
			result = _tapedeck.toolsButtonObj.transform;
			break;
		case TILE_BUTTON.PLAY:
			result = _tapedeck.playButtonObj.transform;
			break;
		case TILE_BUTTON.PAUSE:
			result = _tapedeck.pauseButtonObj.transform;
			break;
		case TILE_BUTTON.STOP:
			result = _tapedeck.stopButtonObj.transform;
			break;
		case TILE_BUTTON.RESTART:
			result = _tapedeck.rewindButtonObj.transform;
			break;
		case TILE_BUTTON.CAPTURE:
			result = _tapedeck.captureButtonObj.transform;
			break;
		case TILE_BUTTON.OPTIONS:
			result = _tapedeck.menuButtonObj.transform;
			break;
		case TILE_BUTTON.EXIT:
			result = _tapedeck.exitButtonObj.transform;
			break;
		}
		return result;
	}

	public void ShowUndo()
	{
		if (undoButton != null)
		{
			undoButton.SetActive(value: true);
		}
	}

	public void ShowRedo()
	{
		if (redoButton != null)
		{
			redoButton.SetActive(value: true);
		}
	}

	public void HideUndo()
	{
		if (undoButton != null)
		{
			undoButton.SetActive(value: false);
		}
	}

	public void HideRedo()
	{
		if (redoButton != null)
		{
			redoButton.SetActive(value: false);
		}
	}

	public void EnableUndo(bool status)
	{
		float a = ((!status) ? 0.5f : 1f);
		Color color = new Color(1f, 1f, 1f, a);
		undoBackgroundImage.color = color;
		undoButtonImage.color = color;
	}

	public void EnableRedo(bool status)
	{
		float a = ((!status) ? 0.5f : 1f);
		Color color = new Color(1f, 1f, 1f, a);
		redoBackgroundImage.color = color;
		redoButtonImage.color = color;
	}

	public void ShowLike()
	{
		likeButton.SetActive(value: true);
	}

	public void SetLikedStatus(bool isLiked)
	{
		likeButtonImage.sprite = ((!isLiked) ? spriteLikeOff : spriteLikeOn);
	}

	public void HideLike()
	{
		likeButton.SetActive(value: false);
	}

	public void SetVRCameraToggleButtonActive(bool active)
	{
		vrCameraButton.SetActive(active);
	}

	public void SetRadarUIActive(bool active)
	{
		radarUI.SetActive(active);
	}

	public void SetRadarUICenterTagActive(string centerTag)
	{
		int num = -1;
		if (!string.IsNullOrEmpty(centerTag))
		{
			num = Array.FindIndex(Tile.shortTagNames, (string w) => w == centerTag);
		}
		for (int num2 = 0; num2 < radarUICenterTags.Length; num2++)
		{
			radarUICenterTags[num2].SetActive(num2 == num);
		}
	}

	public Rect GetRadarUIRect()
	{
		RectTransform rt = (RectTransform)radarUI.transform.GetChild(0);
		return Util.GetWorldRectForRectTransform(rt);
	}

	public void ShowOptionsScreen(string title, string username, string description)
	{
		if (_optionsScreen == null)
		{
			_optionsScreen = UnityEngine.Object.Instantiate(optionsScreenPrefab);
		}
		SetMainCanvasVisible(visible: false);
		SetControlsCanvasVisible(visible: false);
		_optionsScreen.Init(title, username, description);
	}

	public void HideOptionsScreen()
	{
		if (!(_optionsScreen == null))
		{
			UnityEngine.Object.Destroy(_optionsScreen.gameObject);
			SetMainCanvasVisible(visible: true);
			SetControlsCanvasVisible(visible: true);
			_tapedeck.RefreshRecordButtonState();
		}
	}

	public bool IsOptionsScreenVisible()
	{
		return _optionsScreen != null;
	}

	public void ShowWorldLoadingScreen()
	{
		if (!(_worldLoadingScreen != null))
		{
			_worldLoadingScreen = UnityEngine.Object.Instantiate(loadingScreenPrefab);
		}
	}

	public void HideWorldLoadingScreen()
	{
		if (!(_worldLoadingScreen == null))
		{
			UnityEngine.Object.Destroy(_worldLoadingScreen.gameObject);
		}
	}

	public void ShowProfileSelectionScreen()
	{
		if (_profileSelection != null)
		{
			_profileSelection.gameObject.SetActive(value: true);
		}
		else
		{
			_profileSelection = UnityEngine.Object.Instantiate(profileSelectionPrefab);
		}
		_profileSelection.Init();
		Blocksworld.lockInput = true;
		HideUndo();
		HideRedo();
		_tapedeck.ShowProfileSelect(show: false);
		_tapedeck.ShowCapture(show: false);
		_sidePanel.Hide();
	}

	public void HideProfileSelectionScreen()
	{
		if (!(_profileSelection == null))
		{
			_profileSelection.gameObject.SetActive(value: false);
			Blocksworld.lockInput = false;
			_sidePanel.Show();
			WorldUILayout.currentLayout.Apply();
		}
	}

	public UISpeechBubble ShowTextWindow(Block block, string text, Vector2 pos, float width, string buttons)
	{
		return SpeechBubble.ShowTextWindow(block, text, pos, width, buttons);
	}

	public UISpeechBubble GetBlockTextWindow(Block block)
	{
		return SpeechBubble.GetBlockTextWindow(block);
	}

	public void UpdateOnScreenLog(string textStr)
	{
		if (_onScreenLog == null)
		{
			_onScreenLog = UnityEngine.Object.Instantiate(OnScreenLogPrefab);
		}
		_onScreenLog.SetText(textStr);
	}

	public bool IsBlocking(Vector3 screenPoint)
	{
		if (_sidePanel.HitCopyModelButton(screenPoint) || _sidePanel.HitSaveModelButton(screenPoint))
		{
			return true;
		}
		Vector2 normalizedPoint = NormalizedScreen.scale * NormalizedScreen.scale * screenPoint;
		for (int i = 0; i < potentialBlockers.Count; i++)
		{
			if (IsBlocking(potentialBlockers[i], normalizedPoint))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsBlocking(GameObject g, Vector2 normalizedPoint)
	{
		if (g.activeInHierarchy)
		{
			return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)g.transform, normalizedPoint, Blocksworld.guiCamera);
		}
		return false;
	}

	public void GetProtectedRects(List<Rect> rects)
	{
		rects.Add(Util.GetWorldRectForRectTransform(topLeftButtonParent));
		_controls.GetProtectedRects(rects);
	}

	public void ButtonPressed_Undo()
	{
		Blocksworld.bw.ButtonUndoTapped();
	}

	public void ButtonPressed_Redo()
	{
		Blocksworld.bw.ButtonRedoTapped();
	}

	public void ButtonPressed_LikeToggle()
	{
		Blocksworld.bw.ButtonLikeToggleTapped();
	}

	public void ButtonPressed_VRCameraToggle()
	{
		Blocksworld.bw.ButtonVRCameraTapped();
	}

	public void ButtonPressed_Cheat()
	{
		Tutorial.CheatCreateBlock();
	}

	private void ViewportSizeDidChange()
	{
		Layout();
	}

	private void Layout()
	{
		_canvasScaler.scaleFactor = NormalizedScreen.pixelScale;
		_controls.Layout();
		SidePanel.Layout();
	}

	public void HandleMenuInputEvents()
	{
		if (MappedInput.InputDown(MappableInput.MENU_CANCEL))
		{
			bool flag = Blocksworld.selectedBlock != null || Blocksworld.selectedBunch != null;
			if (CharacterEditor.Instance.InEditMode())
			{
				CharacterEditor.Instance.Exit();
			}
			else if (flag)
			{
				Blocksworld.Select(null);
			}
			else if (Blocksworld.CurrentState == State.Build || Blocksworld.CurrentState == State.Play)
			{
				Blocksworld.bw.ButtonExitWorldTapped();
			}
		}
	}
}
