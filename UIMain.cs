using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x0200030B RID: 779
public class UIMain : MonoBehaviour, IMenuInputHandler
{
	// Token: 0x17000166 RID: 358
	// (get) Token: 0x060022FE RID: 8958 RVA: 0x001040D7 File Offset: 0x001024D7
	public UIControls Controls
	{
		get
		{
			return this._controls;
		}
	}

	// Token: 0x17000167 RID: 359
	// (get) Token: 0x060022FF RID: 8959 RVA: 0x001040DF File Offset: 0x001024DF
	public UIDialog Dialog
	{
		get
		{
			return this._dialog;
		}
	}

	// Token: 0x17000168 RID: 360
	// (get) Token: 0x06002300 RID: 8960 RVA: 0x001040E7 File Offset: 0x001024E7
	public UIOverlays Overlay
	{
		get
		{
			return this._overlay;
		}
	}

	// Token: 0x17000169 RID: 361
	// (get) Token: 0x06002301 RID: 8961 RVA: 0x001040EF File Offset: 0x001024EF
	public UITitleBar TitleBar
	{
		get
		{
			return this._titleBar;
		}
	}

	// Token: 0x1700016A RID: 362
	// (get) Token: 0x06002302 RID: 8962 RVA: 0x001040F7 File Offset: 0x001024F7
	public UISidePanel SidePanel
	{
		get
		{
			return this._sidePanel;
		}
	}

	// Token: 0x1700016B RID: 363
	// (get) Token: 0x06002303 RID: 8963 RVA: 0x001040FF File Offset: 0x001024FF
	public UITabBar TabBar
	{
		get
		{
			return this._sidePanel.tabBarUI;
		}
	}

	// Token: 0x1700016C RID: 364
	// (get) Token: 0x06002304 RID: 8964 RVA: 0x0010410C File Offset: 0x0010250C
	public UIQuickSelect QuickSelect
	{
		get
		{
			return this._sidePanel.quickSelect;
		}
	}

	// Token: 0x1700016D RID: 365
	// (get) Token: 0x06002305 RID: 8965 RVA: 0x00104119 File Offset: 0x00102519
	public UISpeechBubbleController SpeechBubble
	{
		get
		{
			return this._speechBubbleController;
		}
	}

	// Token: 0x1700016E RID: 366
	// (get) Token: 0x06002306 RID: 8966 RVA: 0x00104121 File Offset: 0x00102521
	public UILeaderboardController Leaderboard
	{
		get
		{
			return this._leaderboardController;
		}
	}

	// Token: 0x1700016F RID: 367
	// (get) Token: 0x06002307 RID: 8967 RVA: 0x00104129 File Offset: 0x00102529
	public UITapedeck Tapedeck
	{
		get
		{
			return this._tapedeck;
		}
	}

	// Token: 0x17000170 RID: 368
	// (get) Token: 0x06002308 RID: 8968 RVA: 0x00104131 File Offset: 0x00102531
	public UIProfileSelection ProfileSelection
	{
		get
		{
			return this._profileSelection;
		}
	}

	// Token: 0x06002309 RID: 8969 RVA: 0x0010413C File Offset: 0x0010253C
	public static UIMain CreateUI()
	{
		string path = BW.Options.UIPrefabPath();
		UIMain uimain = UnityEngine.Object.Instantiate<UIMain>(Resources.Load<UIMain>(path));
		uimain.Init();
		if (BWStandalone.Instance != null)
		{
			EventSystem componentInChildren = uimain.GetComponentInChildren<EventSystem>();
			componentInChildren.gameObject.SetActive(false);
		}
		return uimain;
	}

	// Token: 0x0600230A RID: 8970 RVA: 0x0010418C File Offset: 0x0010258C
	public void Init()
	{
		this._controls = UnityEngine.Object.Instantiate<UIControls>(this.controlsPrefab);
		this._controls.Init();
		this._dialog = UnityEngine.Object.Instantiate<UIDialog>(this.dialogsPrefab);
		this._dialog.Init();
		this._overlay = UnityEngine.Object.Instantiate<UIOverlays>(this.overlaysPrefab);
		this._overlay.Init();
		this._titleBar = UnityEngine.Object.Instantiate<UITitleBar>(this.titleBarPrefab);
		this._titleBar.Init();
		this._sidePanel = UnityEngine.Object.Instantiate<UISidePanel>(this.sidePanelPrefab);
		this._sidePanel.Init();
		this._speechBubbleController = UnityEngine.Object.Instantiate<UISpeechBubbleController>(this.speechBubbleControllerPrefab);
		this._speechBubbleController.Init();
		this._leaderboardController = UnityEngine.Object.Instantiate<UILeaderboardController>(this.leaderboardControllerPrefab);
		this._leaderboardController.Init();
		this._tapedeck = UnityEngine.Object.Instantiate<UITapedeck>(this.tapedeckPrefab);
		this._tapedeck.Init();
		RectTransform rectTransform = (RectTransform)this._tapedeck.transform;
		rectTransform.SetParent(this.topLeftButtonParent, false);
		rectTransform.SetAsFirstSibling();
		this._mainCanvas = base.GetComponent<Canvas>();
		this._controlsCanvas = this._controls.GetComponent<Canvas>();
		this._titleBarCanvas = this._titleBar.GetComponent<Canvas>();
		this._overlayCanvas = this._overlay.GetComponent<Canvas>();
		this.buyModelButton.Init(false);
		this.buyModelButton.clickAction = delegate()
		{
			WorldSession.current.ConfirmModelPurchase();
		};
		this.buyModelButton.Hide();
		this.addModelToCartButton.Init(false);
		this.addModelToCartButton.clickAction = delegate()
		{
			WorldSession.current.AddModelToCart();
		};
		this.addModelToCartButton.Hide();
		this.buildModelButton.Init(false);
		this.buildModelButton.clickAction = delegate()
		{
			this.buildModelButton.Hide();
			WorldSession.current.TriggerModelTutorial();
		};
		this.buildModelButton.Hide();
		this.undoBackgroundImage = this.undoButton.GetComponent<Image>();
		this.undoButtonImage = this.undoButton.transform.GetChild(0).GetComponent<Image>();
		this.redoBackgroundImage = this.redoButton.GetComponent<Image>();
		this.redoButtonImage = this.redoButton.transform.GetChild(0).GetComponent<Image>();
		this.likeButtonImage = this.likeButton.GetComponent<Image>();
		this._canvasScaler = base.GetComponent<CanvasScaler>();
		Button component = this.cheatButton.GetComponent<Button>();
		component.onClick.RemoveAllListeners();
		component.onClick.AddListener(new UnityAction(this.ButtonPressed_Cheat));
		this.HideCheatButton();
		this.Layout();
		ViewportWatchdog.AddListener(new ViewportWatchdog.ViewportSizeChangedAction(this.ViewportSizeDidChange));
		this.potentialBlockers = new List<GameObject>
		{
			this.undoButton,
			this.redoButton,
			this.likeButton
		};
		this._tapedeck.GetUIObjects(this.potentialBlockers);
		if (this._controls.mouseAndFingerControlEnabled)
		{
			this._controls.GetUIObjects(this.potentialBlockers);
		}
		this._leaderboardController.GetUIObjects(this.potentialBlockers);
	}

	// Token: 0x0600230B RID: 8971 RVA: 0x001044BE File Offset: 0x001028BE
	public void SetAllCanvasVisible(bool visible)
	{
		this.SetMainCanvasVisible(visible);
		this.SetControlsCanvasVisible(visible);
		this.SetTitleBarCanvasVisible(visible);
		this.SetOverlayCanvasVisible(visible);
	}

	// Token: 0x0600230C RID: 8972 RVA: 0x001044DC File Offset: 0x001028DC
	public void SetMainCanvasVisible(bool visible)
	{
		this._mainCanvas.enabled = visible;
	}

	// Token: 0x0600230D RID: 8973 RVA: 0x001044EA File Offset: 0x001028EA
	public void SetControlsCanvasVisible(bool visible)
	{
		this._controlsCanvas.enabled = visible;
	}

	// Token: 0x0600230E RID: 8974 RVA: 0x001044F8 File Offset: 0x001028F8
	public void SetTitleBarCanvasVisible(bool visible)
	{
		this._titleBarCanvas.enabled = visible;
	}

	// Token: 0x0600230F RID: 8975 RVA: 0x00104506 File Offset: 0x00102906
	public void SetOverlayCanvasVisible(bool visible)
	{
		this._overlayCanvas.enabled = visible;
	}

	// Token: 0x06002310 RID: 8976 RVA: 0x00104514 File Offset: 0x00102914
	public void SetTapedeckVisible(bool visible)
	{
		this._tapedeck.gameObject.SetActive(visible);
	}

	// Token: 0x06002311 RID: 8977 RVA: 0x00104528 File Offset: 0x00102928
	public void HideAll()
	{
		this.Dialog.CloseActiveDialog();
		this.buyModelButton.Hide();
		this.addModelToCartButton.Hide();
		this.buildModelButton.Hide();
		this.SidePanel.HideSaveModelButton();
		this.SidePanel.HideCopyModelButton();
		this.Overlay.HidePurchasedBanner();
		this.Overlay.HideOnScreenMessage();
		this.HideTitleBar();
		this.HideControls();
		this.HideCheatButton();
		this.HideLike();
		this.SetVRCameraToggleButtonActive(false);
		this.SpeechBubble.ClearAll();
		if (this._profileSelection != null)
		{
			UnityEngine.Object.Destroy(this._profileSelection.gameObject);
		}
		this._profileSelection = null;
	}

	// Token: 0x06002312 RID: 8978 RVA: 0x001045DF File Offset: 0x001029DF
	public void HideControls()
	{
		this._controls.Hide();
	}

	// Token: 0x06002313 RID: 8979 RVA: 0x001045EC File Offset: 0x001029EC
	public void ShowTitleBar()
	{
		this._titleBar.Show();
	}

	// Token: 0x06002314 RID: 8980 RVA: 0x001045F9 File Offset: 0x001029F9
	public void HideTitleBar()
	{
		this._titleBar.Hide();
	}

	// Token: 0x06002315 RID: 8981 RVA: 0x00104606 File Offset: 0x00102A06
	public void UpdateSpeechBubbles()
	{
		this._speechBubbleController.UpdateSpeechBubbles();
	}

	// Token: 0x06002316 RID: 8982 RVA: 0x00104613 File Offset: 0x00102A13
	public void UpdateTextWindows()
	{
		this._speechBubbleController.UpdateTextWindows();
	}

	// Token: 0x06002317 RID: 8983 RVA: 0x00104620 File Offset: 0x00102A20
	public void ShowBuyModelButton(int price)
	{
		this.buyModelButton.SetText(price.ToString());
		this.buyModelButton.Show();
		this.addModelToCartButton.SetText(price.ToString());
		this.addModelToCartButton.Show();
	}

	// Token: 0x06002318 RID: 8984 RVA: 0x00104673 File Offset: 0x00102A73
	public void HideBuyModelButton()
	{
		this.buyModelButton.Hide();
		this.addModelToCartButton.Hide();
	}

	// Token: 0x06002319 RID: 8985 RVA: 0x0010468B File Offset: 0x00102A8B
	public void ShowBuildModelButton()
	{
	}

	// Token: 0x0600231A RID: 8986 RVA: 0x0010468D File Offset: 0x00102A8D
	public void HideBuildModelButton()
	{
		this.buildModelButton.Hide();
	}

	// Token: 0x0600231B RID: 8987 RVA: 0x0010469A File Offset: 0x00102A9A
	public void ShowCheatButton()
	{
		this.cheatButton.SetActive(true);
	}

	// Token: 0x0600231C RID: 8988 RVA: 0x001046A8 File Offset: 0x00102AA8
	public void HideCheatButton()
	{
		this.cheatButton.SetActive(false);
	}

	// Token: 0x0600231D RID: 8989 RVA: 0x001046B6 File Offset: 0x00102AB6
	public void MakeCheatButtonInvisible()
	{
		this.cheatButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
	}

	// Token: 0x0600231E RID: 8990 RVA: 0x001046E4 File Offset: 0x00102AE4
	public Transform GetTransformForButton(TILE_BUTTON button)
	{
		Transform result = null;
		switch (button)
		{
		case TILE_BUTTON.TOOLS:
			result = this._tapedeck.toolsButtonObj.transform;
			break;
		case TILE_BUTTON.PLAY:
			result = this._tapedeck.playButtonObj.transform;
			break;
		case TILE_BUTTON.PAUSE:
			result = this._tapedeck.pauseButtonObj.transform;
			break;
		case TILE_BUTTON.STOP:
			result = this._tapedeck.stopButtonObj.transform;
			break;
		case TILE_BUTTON.RESTART:
			result = this._tapedeck.rewindButtonObj.transform;
			break;
		case TILE_BUTTON.CAPTURE:
			result = this._tapedeck.captureButtonObj.transform;
			break;
		case TILE_BUTTON.OPTIONS:
			result = this._tapedeck.menuButtonObj.transform;
			break;
		case TILE_BUTTON.EXIT:
			result = this._tapedeck.exitButtonObj.transform;
			break;
		}
		return result;
	}

	// Token: 0x0600231F RID: 8991 RVA: 0x001047D9 File Offset: 0x00102BD9
	public void ShowUndo()
	{
		if (this.undoButton != null)
		{
			this.undoButton.SetActive(true);
		}
	}

	// Token: 0x06002320 RID: 8992 RVA: 0x001047F8 File Offset: 0x00102BF8
	public void ShowRedo()
	{
		if (this.redoButton != null)
		{
			this.redoButton.SetActive(true);
		}
	}

	// Token: 0x06002321 RID: 8993 RVA: 0x00104817 File Offset: 0x00102C17
	public void HideUndo()
	{
		if (this.undoButton != null)
		{
			this.undoButton.SetActive(false);
		}
	}

	// Token: 0x06002322 RID: 8994 RVA: 0x00104836 File Offset: 0x00102C36
	public void HideRedo()
	{
		if (this.redoButton != null)
		{
			this.redoButton.SetActive(false);
		}
	}

	// Token: 0x06002323 RID: 8995 RVA: 0x00104858 File Offset: 0x00102C58
	public void EnableUndo(bool status)
	{
		float a = (!status) ? 0.5f : 1f;
		Color color = new Color(1f, 1f, 1f, a);
		this.undoBackgroundImage.color = color;
		this.undoButtonImage.color = color;
	}

	// Token: 0x06002324 RID: 8996 RVA: 0x001048AC File Offset: 0x00102CAC
	public void EnableRedo(bool status)
	{
		float a = (!status) ? 0.5f : 1f;
		Color color = new Color(1f, 1f, 1f, a);
		this.redoBackgroundImage.color = color;
		this.redoButtonImage.color = color;
	}

	// Token: 0x06002325 RID: 8997 RVA: 0x001048FE File Offset: 0x00102CFE
	public void ShowLike()
	{
		this.likeButton.SetActive(true);
	}

	// Token: 0x06002326 RID: 8998 RVA: 0x0010490C File Offset: 0x00102D0C
	public void SetLikedStatus(bool isLiked)
	{
		this.likeButtonImage.sprite = ((!isLiked) ? this.spriteLikeOff : this.spriteLikeOn);
	}

	// Token: 0x06002327 RID: 8999 RVA: 0x00104930 File Offset: 0x00102D30
	public void HideLike()
	{
		this.likeButton.SetActive(false);
	}

	// Token: 0x06002328 RID: 9000 RVA: 0x0010493E File Offset: 0x00102D3E
	public void SetVRCameraToggleButtonActive(bool active)
	{
		this.vrCameraButton.SetActive(active);
	}

	// Token: 0x06002329 RID: 9001 RVA: 0x0010494C File Offset: 0x00102D4C
	public void SetRadarUIActive(bool active)
	{
		this.radarUI.SetActive(active);
	}

	// Token: 0x0600232A RID: 9002 RVA: 0x0010495C File Offset: 0x00102D5C
	public void SetRadarUICenterTagActive(string centerTag)
	{
		int num = -1;
		if (!string.IsNullOrEmpty(centerTag))
		{
			num = Array.FindIndex<string>(Tile.shortTagNames, (string w) => w == centerTag);
		}
		for (int i = 0; i < this.radarUICenterTags.Length; i++)
		{
			this.radarUICenterTags[i].SetActive(i == num);
		}
	}

	// Token: 0x0600232B RID: 9003 RVA: 0x001049CC File Offset: 0x00102DCC
	public Rect GetRadarUIRect()
	{
		RectTransform rt = (RectTransform)this.radarUI.transform.GetChild(0);
		return Util.GetWorldRectForRectTransform(rt);
	}

	// Token: 0x0600232C RID: 9004 RVA: 0x001049F6 File Offset: 0x00102DF6
	public void ShowOptionsScreen(string title, string username, string description)
	{
		if (this._optionsScreen == null)
		{
			this._optionsScreen = UnityEngine.Object.Instantiate<UIOptionsScreen>(this.optionsScreenPrefab);
		}
		this.SetMainCanvasVisible(false);
		this.SetControlsCanvasVisible(false);
		this._optionsScreen.Init(title, username, description);
	}

	// Token: 0x0600232D RID: 9005 RVA: 0x00104A36 File Offset: 0x00102E36
	public void HideOptionsScreen()
	{
		if (this._optionsScreen == null)
		{
			return;
		}
		UnityEngine.Object.Destroy(this._optionsScreen.gameObject);
		this.SetMainCanvasVisible(true);
		this.SetControlsCanvasVisible(true);
		this._tapedeck.RefreshRecordButtonState();
	}

	// Token: 0x0600232E RID: 9006 RVA: 0x00104A73 File Offset: 0x00102E73
	public bool IsOptionsScreenVisible()
	{
		return this._optionsScreen != null;
	}

	// Token: 0x0600232F RID: 9007 RVA: 0x00104A81 File Offset: 0x00102E81
	public void ShowWorldLoadingScreen()
	{
		if (this._worldLoadingScreen != null)
		{
			return;
		}
		this._worldLoadingScreen = UnityEngine.Object.Instantiate<UILoadingScreen>(this.loadingScreenPrefab);
	}

	// Token: 0x06002330 RID: 9008 RVA: 0x00104AA6 File Offset: 0x00102EA6
	public void HideWorldLoadingScreen()
	{
		if (this._worldLoadingScreen == null)
		{
			return;
		}
		UnityEngine.Object.Destroy(this._worldLoadingScreen.gameObject);
	}

	// Token: 0x06002331 RID: 9009 RVA: 0x00104ACC File Offset: 0x00102ECC
	public void ShowProfileSelectionScreen()
	{
		if (this._profileSelection != null)
		{
			this._profileSelection.gameObject.SetActive(true);
		}
		else
		{
			this._profileSelection = UnityEngine.Object.Instantiate<UIProfileSelection>(this.profileSelectionPrefab);
		}
		this._profileSelection.Init();
		Blocksworld.lockInput = true;
		this.HideUndo();
		this.HideRedo();
		this._tapedeck.ShowProfileSelect(false);
		this._tapedeck.ShowCapture(false);
		this._sidePanel.Hide();
	}

	// Token: 0x06002332 RID: 9010 RVA: 0x00104B51 File Offset: 0x00102F51
	public void HideProfileSelectionScreen()
	{
		if (this._profileSelection == null)
		{
			return;
		}
		this._profileSelection.gameObject.SetActive(false);
		Blocksworld.lockInput = false;
		this._sidePanel.Show();
		WorldUILayout.currentLayout.Apply();
	}

	// Token: 0x06002333 RID: 9011 RVA: 0x00104B91 File Offset: 0x00102F91
	public UISpeechBubble ShowTextWindow(Block block, string text, Vector2 pos, float width, string buttons)
	{
		return this.SpeechBubble.ShowTextWindow(block, text, pos, width, buttons);
	}

	// Token: 0x06002334 RID: 9012 RVA: 0x00104BA5 File Offset: 0x00102FA5
	public UISpeechBubble GetBlockTextWindow(Block block)
	{
		return this.SpeechBubble.GetBlockTextWindow(block);
	}

	// Token: 0x06002335 RID: 9013 RVA: 0x00104BB3 File Offset: 0x00102FB3
	public void UpdateOnScreenLog(string textStr)
	{
		if (this._onScreenLog == null)
		{
			this._onScreenLog = UnityEngine.Object.Instantiate<UIDebug>(this.OnScreenLogPrefab);
		}
		this._onScreenLog.SetText(textStr);
	}

	// Token: 0x06002336 RID: 9014 RVA: 0x00104BE4 File Offset: 0x00102FE4
	public bool IsBlocking(Vector3 screenPoint)
	{
		if (this._sidePanel.HitCopyModelButton(screenPoint) || this._sidePanel.HitSaveModelButton(screenPoint))
		{
			return true;
		}
		Vector2 normalizedPoint = NormalizedScreen.scale * NormalizedScreen.scale * screenPoint;
		for (int i = 0; i < this.potentialBlockers.Count; i++)
		{
			if (this.IsBlocking(this.potentialBlockers[i], normalizedPoint))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06002337 RID: 9015 RVA: 0x00104C63 File Offset: 0x00103063
	private bool IsBlocking(GameObject g, Vector2 normalizedPoint)
	{
		return g.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)g.transform, normalizedPoint, Blocksworld.guiCamera);
	}

	// Token: 0x06002338 RID: 9016 RVA: 0x00104C88 File Offset: 0x00103088
	public void GetProtectedRects(List<Rect> rects)
	{
		rects.Add(Util.GetWorldRectForRectTransform(this.topLeftButtonParent));
		this._controls.GetProtectedRects(rects);
	}

	// Token: 0x06002339 RID: 9017 RVA: 0x00104CA7 File Offset: 0x001030A7
	public void ButtonPressed_Undo()
	{
		Blocksworld.bw.ButtonUndoTapped();
	}

	// Token: 0x0600233A RID: 9018 RVA: 0x00104CB3 File Offset: 0x001030B3
	public void ButtonPressed_Redo()
	{
		Blocksworld.bw.ButtonRedoTapped();
	}

	// Token: 0x0600233B RID: 9019 RVA: 0x00104CBF File Offset: 0x001030BF
	public void ButtonPressed_LikeToggle()
	{
		Blocksworld.bw.ButtonLikeToggleTapped();
	}

	// Token: 0x0600233C RID: 9020 RVA: 0x00104CCB File Offset: 0x001030CB
	public void ButtonPressed_VRCameraToggle()
	{
		Blocksworld.bw.ButtonVRCameraTapped();
	}

	// Token: 0x0600233D RID: 9021 RVA: 0x00104CD7 File Offset: 0x001030D7
	public void ButtonPressed_Cheat()
	{
		Tutorial.CheatCreateBlock();
	}

	// Token: 0x0600233E RID: 9022 RVA: 0x00104CDE File Offset: 0x001030DE
	private void ViewportSizeDidChange()
	{
		this.Layout();
	}

	// Token: 0x0600233F RID: 9023 RVA: 0x00104CE6 File Offset: 0x001030E6
	private void Layout()
	{
		this._canvasScaler.scaleFactor = NormalizedScreen.pixelScale;
		this._controls.Layout();
		this.SidePanel.Layout();
	}

	// Token: 0x06002340 RID: 9024 RVA: 0x00104D10 File Offset: 0x00103110
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
				Blocksworld.Select(null, false, true);
			}
			else if (Blocksworld.CurrentState == State.Build || Blocksworld.CurrentState == State.Play)
			{
				Blocksworld.bw.ButtonExitWorldTapped();
			}
		}
	}

	// Token: 0x04001E20 RID: 7712
	public RectTransform topLeftButtonParent;

	// Token: 0x04001E21 RID: 7713
	public GameObject undoButton;

	// Token: 0x04001E22 RID: 7714
	public GameObject redoButton;

	// Token: 0x04001E23 RID: 7715
	public GameObject likeButton;

	// Token: 0x04001E24 RID: 7716
	public GameObject vrCameraButton;

	// Token: 0x04001E25 RID: 7717
	public GameObject cheatButton;

	// Token: 0x04001E26 RID: 7718
	public GameObject radarUI;

	// Token: 0x04001E27 RID: 7719
	public GameObject[] radarUICenterTags;

	// Token: 0x04001E28 RID: 7720
	public UIControls controlsPrefab;

	// Token: 0x04001E29 RID: 7721
	public UIDialog dialogsPrefab;

	// Token: 0x04001E2A RID: 7722
	public UIOverlays overlaysPrefab;

	// Token: 0x04001E2B RID: 7723
	public UITitleBar titleBarPrefab;

	// Token: 0x04001E2C RID: 7724
	public UISidePanel sidePanelPrefab;

	// Token: 0x04001E2D RID: 7725
	public UISpeechBubbleController speechBubbleControllerPrefab;

	// Token: 0x04001E2E RID: 7726
	public UILeaderboardController leaderboardControllerPrefab;

	// Token: 0x04001E2F RID: 7727
	public UIOptionsScreen optionsScreenPrefab;

	// Token: 0x04001E30 RID: 7728
	public UILoadingScreen loadingScreenPrefab;

	// Token: 0x04001E31 RID: 7729
	public UITapedeck tapedeckPrefab;

	// Token: 0x04001E32 RID: 7730
	public UIDebug OnScreenLogPrefab;

	// Token: 0x04001E33 RID: 7731
	public UIProfileSelection profileSelectionPrefab;

	// Token: 0x04001E34 RID: 7732
	public UIButton buyModelButton;

	// Token: 0x04001E35 RID: 7733
	public UIButton addModelToCartButton;

	// Token: 0x04001E36 RID: 7734
	public UIButton buildModelButton;

	// Token: 0x04001E37 RID: 7735
	public Sprite spriteHome;

	// Token: 0x04001E38 RID: 7736
	public Sprite spritePlay;

	// Token: 0x04001E39 RID: 7737
	public Sprite spriteRestart;

	// Token: 0x04001E3A RID: 7738
	public Sprite spriteStop;

	// Token: 0x04001E3B RID: 7739
	public Sprite spritePause;

	// Token: 0x04001E3C RID: 7740
	public Sprite spriteOptions;

	// Token: 0x04001E3D RID: 7741
	public Sprite spriteCapture;

	// Token: 0x04001E3E RID: 7742
	public Sprite spriteLikeOn;

	// Token: 0x04001E3F RID: 7743
	public Sprite spriteLikeOff;

	// Token: 0x04001E40 RID: 7744
	private UIControls _controls;

	// Token: 0x04001E41 RID: 7745
	private UIDialog _dialog;

	// Token: 0x04001E42 RID: 7746
	private UIOverlays _overlay;

	// Token: 0x04001E43 RID: 7747
	private UITitleBar _titleBar;

	// Token: 0x04001E44 RID: 7748
	private UISidePanel _sidePanel;

	// Token: 0x04001E45 RID: 7749
	private UISpeechBubbleController _speechBubbleController;

	// Token: 0x04001E46 RID: 7750
	private UILeaderboardController _leaderboardController;

	// Token: 0x04001E47 RID: 7751
	private UITapedeck _tapedeck;

	// Token: 0x04001E48 RID: 7752
	private UIProfileSelection _profileSelection;

	// Token: 0x04001E49 RID: 7753
	private UIOptionsScreen _optionsScreen;

	// Token: 0x04001E4A RID: 7754
	private UILoadingScreen _worldLoadingScreen;

	// Token: 0x04001E4B RID: 7755
	private UIDebug _onScreenLog;

	// Token: 0x04001E4C RID: 7756
	private RectTransform screenSpaceParent;

	// Token: 0x04001E4D RID: 7757
	private Canvas _mainCanvas;

	// Token: 0x04001E4E RID: 7758
	private Canvas _controlsCanvas;

	// Token: 0x04001E4F RID: 7759
	private Canvas _titleBarCanvas;

	// Token: 0x04001E50 RID: 7760
	private Canvas _overlayCanvas;

	// Token: 0x04001E51 RID: 7761
	private CanvasScaler _canvasScaler;

	// Token: 0x04001E52 RID: 7762
	private Image undoBackgroundImage;

	// Token: 0x04001E53 RID: 7763
	private Image redoBackgroundImage;

	// Token: 0x04001E54 RID: 7764
	private Image undoButtonImage;

	// Token: 0x04001E55 RID: 7765
	private Image redoButtonImage;

	// Token: 0x04001E56 RID: 7766
	private Image likeButtonImage;

	// Token: 0x04001E57 RID: 7767
	private Dictionary<TILE_BUTTON, Sprite> spriteLookup;

	// Token: 0x04001E58 RID: 7768
	private TILE_BUTTON[] currentMainButtons;

	// Token: 0x04001E59 RID: 7769
	private List<GameObject> potentialBlockers = new List<GameObject>();
}
