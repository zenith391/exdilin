using System.Runtime.CompilerServices;
using UnityEngine;

public class WorldUILayout
{
	public static WorldUILayout currentLayout;

	private WorldUILayoutParameters _params;

	private bool hideAll;

	private static UIMain uiMain;

	private const float titleBarButtonOffset = 24f;

	private static WorldUILayoutParameters hiddenParams = new WorldUILayoutParameters
	{
		includeTapedeck = false,
		includeRecord = false
	};

	public static WorldUILayout WorldUILayoutHidden = new WorldUILayout(hiddenParams);

	[CompilerGenerated]
	private static ViewportWatchdog.ViewportSizeChangedAction f__mg_cache0;

	public WorldUILayout(WorldUILayoutParameters p)
	{
		_params = p;
	}

	public WorldUILayout(TILE_BUTTON button)
	{
		_params = new WorldUILayoutParameters(button);
	}

	public WorldUILayout(TILE_BUTTON button1, TILE_BUTTON button2)
	{
		_params = new WorldUILayoutParameters(button1, button2);
	}

	public WorldUILayout(TILE_BUTTON button1, TILE_BUTTON button2, TILE_BUTTON button3)
	{
		_params = new WorldUILayoutParameters(button1, button2, button3);
	}

	public WorldUILayout(TILE_BUTTON button1, TILE_BUTTON button2, TILE_BUTTON button3, TILE_BUTTON button4)
	{
		_params = new WorldUILayoutParameters(button1, button2, button3, button4);
	}

	public WorldUILayout(TILE_BUTTON[] buttons)
	{
		_params = new WorldUILayoutParameters(buttons);
	}

	public static void Init()
	{
		uiMain = Blocksworld.UI;
		ViewportWatchdog.AddListener(OnViewportSizeChange);
	}

	private static void OnViewportSizeChange()
	{
		int width = NormalizedScreen.width;
		int height = NormalizedScreen.height;
		Blocksworld.guiCamera.transform.position = new Vector3(width / 2, height / 2, -1f);
		Blocksworld.guiCamera.orthographicSize = height / 2;
		if (currentLayout != null)
		{
			currentLayout.Apply();
		}
	}

	public void Apply()
	{
		currentLayout = this;
		Vector3 vector = ((!_params.includeTitleBar) ? Vector3.zero : new Vector3(0f, -24f, 0f));
		Vector3 vector2 = ((!Blocksworld.buildPanel.isPanelVisible) ? Vector3.zero : new Vector3(0f - Blocksworld.buildPanel.width - TabBar.pixelWidth, 0f, 0f));
		if (_params.includeTapedeck)
		{
			uiMain.SetTapedeckVisible(visible: true);
			uiMain.Tapedeck.HideAllButtons();
			for (int i = 0; i < _params.mainButtons.Length; i++)
			{
				switch (_params.mainButtons[i])
				{
				case TILE_BUTTON.TOOLS:
					uiMain.Tapedeck.ShowTools(show: true);
					break;
				case TILE_BUTTON.PLAY:
					uiMain.Tapedeck.ShowPlay(show: true);
					break;
				case TILE_BUTTON.PAUSE:
					uiMain.Tapedeck.ShowPause(show: true);
					break;
				case TILE_BUTTON.STOP:
					uiMain.Tapedeck.ShowStop(show: true);
					break;
				case TILE_BUTTON.RESTART:
					uiMain.Tapedeck.ShowRewind(show: true);
					break;
				case TILE_BUTTON.CAPTURE_SETUP:
					uiMain.Tapedeck.ShowCaptureSetup(show: true);
					break;
				case TILE_BUTTON.CAPTURE:
					uiMain.Tapedeck.ShowCapture(show: true);
					break;
				case TILE_BUTTON.OPTIONS:
					uiMain.Tapedeck.ShowMenu(show: true);
					break;
				case TILE_BUTTON.PROFILE_SELECT:
					uiMain.Tapedeck.ShowProfileSelect(show: true);
					break;
				case TILE_BUTTON.EXIT:
					uiMain.Tapedeck.ShowExit(show: true);
					break;
				}
			}
			bool flag = WorldSession.platformDelegate.ScreenRecordingAvailable();
			uiMain.Tapedeck.SetScreenRecordingEnabled(flag);
			if (flag)
			{
				bool includeRecord = _params.includeRecord;
				uiMain.Tapedeck.ShowRecord(includeRecord);
			}
		}
		else
		{
			uiMain.SetTapedeckVisible(visible: false);
		}
		if (_params.includeTitleBar)
		{
			Blocksworld.UI.TitleBar.SetTitleText(_params.titleBarText);
			Blocksworld.UI.TitleBar.SetSubtext(_params.titleBarSubtitle);
			if (_params.titleBarHasCoinBalance)
			{
				Blocksworld.UI.TitleBar.ShowCoinBalance();
				Blocksworld.UI.TitleBar.SetCoinBalance(_params.titleBarCoinBalance);
			}
			else
			{
				Blocksworld.UI.TitleBar.HideCoinBalance();
			}
			Blocksworld.UI.ShowTitleBar();
		}
		else
		{
			Blocksworld.UI.HideTitleBar();
		}
		Blocksworld.UI.SetVRCameraToggleButtonActive(_params.includeVRCameraToggle);
		if (_params.includePurchasedBanner)
		{
			Blocksworld.UI.Overlay.ShowPurchasedBanner();
		}
		else
		{
			Blocksworld.UI.Overlay.HidePurchasedBanner();
		}
		if (_params.includeBuildModelButton)
		{
			Blocksworld.UI.ShowBuildModelButton();
		}
		else
		{
			Blocksworld.UI.HideBuildModelButton();
		}
		if (_params.includeBuyModelButton)
		{
			Blocksworld.UI.ShowBuyModelButton(_params.buyModelPrice);
		}
		else
		{
			Blocksworld.UI.HideBuyModelButton();
		}
		RefreshLikeUnlikeButtons(vector + vector2);
		RefreshUndoRedoButtons();
	}

	public void RefreshLikeUnlikeButtons(Vector3 offset)
	{
		if (_params.includeLikeUnlike)
		{
			uiMain.ShowLike();
			bool isLiked = WorldSession.current.IsLiked;
			uiMain.SetLikedStatus(isLiked);
		}
		else
		{
			uiMain.HideLike();
		}
	}

	public void RefreshUndoRedoButtons()
	{
		if (_params.includeUndoRedo)
		{
			bool status = History.CanUndo();
			bool status2 = History.CanRedo();
			uiMain.ShowUndo();
			uiMain.EnableUndo(status);
			uiMain.ShowRedo();
			uiMain.EnableRedo(status2);
		}
		else
		{
			uiMain.HideUndo();
			uiMain.HideRedo();
		}
	}

	public static void HideAll()
	{
		WorldUILayoutHidden.Apply();
		Blocksworld.UI.HideAll();
	}
}
