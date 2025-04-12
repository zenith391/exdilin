using System;
using System.Runtime.CompilerServices;
using UnityEngine;

// Token: 0x0200032B RID: 811
public class WorldUILayout
{
	// Token: 0x0600249D RID: 9373 RVA: 0x0010B5C8 File Offset: 0x001099C8
	public WorldUILayout(WorldUILayoutParameters p)
	{
		this._params = p;
	}

	// Token: 0x0600249E RID: 9374 RVA: 0x0010B5D7 File Offset: 0x001099D7
	public WorldUILayout(TILE_BUTTON button)
	{
		this._params = new WorldUILayoutParameters(button);
	}

	// Token: 0x0600249F RID: 9375 RVA: 0x0010B5EB File Offset: 0x001099EB
	public WorldUILayout(TILE_BUTTON button1, TILE_BUTTON button2)
	{
		this._params = new WorldUILayoutParameters(button1, button2);
	}

	// Token: 0x060024A0 RID: 9376 RVA: 0x0010B600 File Offset: 0x00109A00
	public WorldUILayout(TILE_BUTTON button1, TILE_BUTTON button2, TILE_BUTTON button3)
	{
		this._params = new WorldUILayoutParameters(button1, button2, button3);
	}

	// Token: 0x060024A1 RID: 9377 RVA: 0x0010B616 File Offset: 0x00109A16
	public WorldUILayout(TILE_BUTTON button1, TILE_BUTTON button2, TILE_BUTTON button3, TILE_BUTTON button4)
	{
		this._params = new WorldUILayoutParameters(button1, button2, button3, button4);
	}

	// Token: 0x060024A2 RID: 9378 RVA: 0x0010B62E File Offset: 0x00109A2E
	public WorldUILayout(TILE_BUTTON[] buttons)
	{
		this._params = new WorldUILayoutParameters(buttons);
	}

	// Token: 0x060024A3 RID: 9379 RVA: 0x0010B642 File Offset: 0x00109A42
	public static void Init()
	{
		WorldUILayout.uiMain = Blocksworld.UI;
		if (WorldUILayout.f__mg_cache0 == null)
		{
			WorldUILayout.f__mg_cache0 = new ViewportWatchdog.ViewportSizeChangedAction(WorldUILayout.OnViewportSizeChange);
		}
		ViewportWatchdog.AddListener(WorldUILayout.f__mg_cache0);
	}

	// Token: 0x060024A4 RID: 9380 RVA: 0x0010B670 File Offset: 0x00109A70
	private static void OnViewportSizeChange()
	{
		int width = NormalizedScreen.width;
		int height = NormalizedScreen.height;
		Blocksworld.guiCamera.transform.position = new Vector3((float)(width / 2), (float)(height / 2), -1f);
		Blocksworld.guiCamera.orthographicSize = (float)(height / 2);
		if (WorldUILayout.currentLayout != null)
		{
			WorldUILayout.currentLayout.Apply();
		}
	}

	// Token: 0x060024A5 RID: 9381 RVA: 0x0010B6CC File Offset: 0x00109ACC
	public void Apply()
	{
		WorldUILayout.currentLayout = this;
		Vector3 a = (!this._params.includeTitleBar) ? Vector3.zero : new Vector3(0f, -24f, 0f);
		Vector3 b = (!Blocksworld.buildPanel.isPanelVisible) ? Vector3.zero : new Vector3(-Blocksworld.buildPanel.width - TabBar.pixelWidth, 0f, 0f);
		if (this._params.includeTapedeck)
		{
			WorldUILayout.uiMain.SetTapedeckVisible(true);
			WorldUILayout.uiMain.Tapedeck.HideAllButtons();
			for (int i = 0; i < this._params.mainButtons.Length; i++)
			{
				switch (this._params.mainButtons[i])
				{
				case TILE_BUTTON.TOOLS:
					WorldUILayout.uiMain.Tapedeck.ShowTools(true);
					break;
				case TILE_BUTTON.PLAY:
					WorldUILayout.uiMain.Tapedeck.ShowPlay(true);
					break;
				case TILE_BUTTON.PAUSE:
					WorldUILayout.uiMain.Tapedeck.ShowPause(true);
					break;
				case TILE_BUTTON.STOP:
					WorldUILayout.uiMain.Tapedeck.ShowStop(true);
					break;
				case TILE_BUTTON.RESTART:
					WorldUILayout.uiMain.Tapedeck.ShowRewind(true);
					break;
				case TILE_BUTTON.CAPTURE_SETUP:
					WorldUILayout.uiMain.Tapedeck.ShowCaptureSetup(true);
					break;
				case TILE_BUTTON.CAPTURE:
					WorldUILayout.uiMain.Tapedeck.ShowCapture(true);
					break;
				case TILE_BUTTON.OPTIONS:
					WorldUILayout.uiMain.Tapedeck.ShowMenu(true);
					break;
				case TILE_BUTTON.PROFILE_SELECT:
					WorldUILayout.uiMain.Tapedeck.ShowProfileSelect(true);
					break;
				case TILE_BUTTON.EXIT:
					WorldUILayout.uiMain.Tapedeck.ShowExit(true);
					break;
				}
			}
			bool flag = WorldSession.platformDelegate.ScreenRecordingAvailable();
			WorldUILayout.uiMain.Tapedeck.SetScreenRecordingEnabled(flag);
			if (flag)
			{
				bool includeRecord = this._params.includeRecord;
				WorldUILayout.uiMain.Tapedeck.ShowRecord(includeRecord);
			}
		}
		else
		{
			WorldUILayout.uiMain.SetTapedeckVisible(false);
		}
		if (this._params.includeTitleBar)
		{
			Blocksworld.UI.TitleBar.SetTitleText(this._params.titleBarText);
			Blocksworld.UI.TitleBar.SetSubtext(this._params.titleBarSubtitle);
			if (this._params.titleBarHasCoinBalance)
			{
				Blocksworld.UI.TitleBar.ShowCoinBalance();
				Blocksworld.UI.TitleBar.SetCoinBalance(this._params.titleBarCoinBalance);
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
		Blocksworld.UI.SetVRCameraToggleButtonActive(this._params.includeVRCameraToggle);
		if (this._params.includePurchasedBanner)
		{
			Blocksworld.UI.Overlay.ShowPurchasedBanner();
		}
		else
		{
			Blocksworld.UI.Overlay.HidePurchasedBanner();
		}
		if (this._params.includeBuildModelButton)
		{
			Blocksworld.UI.ShowBuildModelButton();
		}
		else
		{
			Blocksworld.UI.HideBuildModelButton();
		}
		if (this._params.includeBuyModelButton)
		{
			Blocksworld.UI.ShowBuyModelButton(this._params.buyModelPrice);
		}
		else
		{
			Blocksworld.UI.HideBuyModelButton();
		}
		this.RefreshLikeUnlikeButtons(a + b);
		this.RefreshUndoRedoButtons();
	}

	// Token: 0x060024A6 RID: 9382 RVA: 0x0010BA5C File Offset: 0x00109E5C
	public void RefreshLikeUnlikeButtons(Vector3 offset)
	{
		if (this._params.includeLikeUnlike)
		{
			WorldUILayout.uiMain.ShowLike();
			bool isLiked = WorldSession.current.IsLiked;
			WorldUILayout.uiMain.SetLikedStatus(isLiked);
		}
		else
		{
			WorldUILayout.uiMain.HideLike();
		}
	}

	// Token: 0x060024A7 RID: 9383 RVA: 0x0010BAA8 File Offset: 0x00109EA8
	public void RefreshUndoRedoButtons()
	{
		if (this._params.includeUndoRedo)
		{
			bool status = History.CanUndo();
			bool status2 = History.CanRedo();
			WorldUILayout.uiMain.ShowUndo();
			WorldUILayout.uiMain.EnableUndo(status);
			WorldUILayout.uiMain.ShowRedo();
			WorldUILayout.uiMain.EnableRedo(status2);
		}
		else
		{
			WorldUILayout.uiMain.HideUndo();
			WorldUILayout.uiMain.HideRedo();
		}
	}

	// Token: 0x060024A8 RID: 9384 RVA: 0x0010BB14 File Offset: 0x00109F14
	public static void HideAll()
	{
		WorldUILayout.WorldUILayoutHidden.Apply();
		Blocksworld.UI.HideAll();
	}

	// Token: 0x04001F8D RID: 8077
	public static WorldUILayout currentLayout;

	// Token: 0x04001F8E RID: 8078
	private WorldUILayoutParameters _params;

	// Token: 0x04001F8F RID: 8079
	private bool hideAll;

	// Token: 0x04001F90 RID: 8080
	private static UIMain uiMain;

	// Token: 0x04001F91 RID: 8081
	private const float titleBarButtonOffset = 24f;

	// Token: 0x04001F92 RID: 8082
	private static WorldUILayoutParameters hiddenParams = new WorldUILayoutParameters
	{
		includeTapedeck = false,
		includeRecord = false
	};

	// Token: 0x04001F93 RID: 8083
	public static WorldUILayout WorldUILayoutHidden = new WorldUILayout(WorldUILayout.hiddenParams);

	// Token: 0x04001F94 RID: 8084
	[CompilerGenerated]
	private static ViewportWatchdog.ViewportSizeChangedAction f__mg_cache0;
}
