using System;
using UnityEngine;

// Token: 0x0200036F RID: 879
public class WorldSessionPlatformDelegate_Standalone : WorldSessionPlatformDelegate
{
	// Token: 0x06002772 RID: 10098 RVA: 0x00121538 File Offset: 0x0011F938
	public void WorldDidStartLoading()
	{
	}

	// Token: 0x06002773 RID: 10099 RVA: 0x0012153A File Offset: 0x0011F93A
	public void WorldDidFinishLoading()
	{
		Blocksworld.EnableGameCameras(true);
		if (BWStandalone.Instance != null)
		{
			MenuInputHandler.RequestControl(Blocksworld.UI);
			BWStandalone.Overlays.ShowLoadingOverlay(false);
			MainUIController.Instance.HideAll();
		}
	}

	// Token: 0x06002774 RID: 10100 RVA: 0x00121574 File Offset: 0x0011F974
	public void WorldWillQuit(string deepLinkStr)
	{
		if (BWStandalone.Instance != null)
		{
			MenuInputHandler.Clear();
			if (!string.IsNullOrEmpty(deepLinkStr))
			{
				BWStandalone.Instance.HandleWorldExitMessage(deepLinkStr);
			}
			Blocksworld.EnableGameCameras(false);
			MainUIController.Instance.Show();
			BWStandalone.Overlays.ShowLoadingOverlay(true);
		}
	}

	// Token: 0x06002775 RID: 10101 RVA: 0x001215C7 File Offset: 0x0011F9C7
	public void WorldDidQuit()
	{
		if (BWStandalone.Instance != null)
		{
			BWStandalone.Instance.ReturnToMenu();
		}
	}

	// Token: 0x06002776 RID: 10102 RVA: 0x001215E3 File Offset: 0x0011F9E3
	public void SaveCurrentWorldData()
	{
		BWStandalone.Instance.SaveCurrentWorldSession(null);
	}

	// Token: 0x06002777 RID: 10103 RVA: 0x001215F0 File Offset: 0x0011F9F0
	public void SaveCurrentWorldDataWithScreenshot(byte[] imageData)
	{
		BWStandalone.Instance.SaveCurrentWorldSession(imageData);
	}

	// Token: 0x06002778 RID: 10104 RVA: 0x001215FD File Offset: 0x0011F9FD
	public void SetProfileWorldData(string worldSource, string avatarSource, string profileGender)
	{
		BWStandalone.Instance.SetProfileWorldData(worldSource, avatarSource, profileGender);
	}

	// Token: 0x06002779 RID: 10105 RVA: 0x0012160C File Offset: 0x0011FA0C
	public void SendScreenShot(byte[] imageData, string label)
	{
		BWStandalone.Instance.SendScreenshot(imageData, label);
	}

	// Token: 0x0600277A RID: 10106 RVA: 0x0012161C File Offset: 0x0011FA1C
	public void ImageWriteToSavedPhotosAlbum(byte[] imageData)
	{
		string text = WorldSession.current.config.worldTitle;
		if (string.IsNullOrEmpty(text))
		{
			text = "bwscreenshot";
		}
		this.SendScreenShot(imageData, text);
	}

	// Token: 0x0600277B RID: 10107 RVA: 0x00121652 File Offset: 0x0011FA52
	public void NotifyWorldTooNew()
	{
	}

	// Token: 0x0600277C RID: 10108 RVA: 0x00121654 File Offset: 0x0011FA54
	public void SaveAsNewModel(string modelTitle, string modelSource, string blocksInventoryStr, string hash, byte[] iconBytesSD, byte[] iconBytesHD, byte[] imageBytesSD, byte[] imageBytesHD)
	{
		BWUserModel bwuserModel = new BWUserModel(modelTitle, modelSource, blocksInventoryStr, hash);
		bwuserModel.GenerateImageChecksums(iconBytesSD, iconBytesHD, imageBytesSD, imageBytesHD);
		BWUserModelsDataManager.Instance.SaveModelLocal(bwuserModel, iconBytesSD, iconBytesHD, imageBytesSD, imageBytesHD);
		BWUserModelsDataManager.Instance.CreateRemoteModel(bwuserModel, iconBytesSD, iconBytesHD, imageBytesSD, imageBytesHD, null);
	}

	// Token: 0x0600277D RID: 10109 RVA: 0x001216A1 File Offset: 0x0011FAA1
	public string GetModelSource(string modelType, string modelId)
	{
		return BWUserModelsDataManager.Instance.LoadSourceForModel(modelType, modelId);
	}

	// Token: 0x0600277E RID: 10110 RVA: 0x001216AF File Offset: 0x0011FAAF
	public Texture2D GetCurrentlyLoadedPreviewModelImage()
	{
		return (Texture2D)Resources.Load("GUI/TabBar/Panel Tab Gear HD");
	}

	// Token: 0x0600277F RID: 10111 RVA: 0x001216C0 File Offset: 0x0011FAC0
	public void RequestMissingModelIcon(string modelType, string modelId)
	{
	}

	// Token: 0x06002780 RID: 10112 RVA: 0x001216C2 File Offset: 0x0011FAC2
	public void PurchaseCurrentlyLoadedModel()
	{
		BWStandalone.Instance.PurchaseCurrentlyLoadedModel();
		WorldSession.ModelPurchaseCallback("success");
	}

	// Token: 0x06002781 RID: 10113 RVA: 0x001216D8 File Offset: 0x0011FAD8
	public void PurchaseBuildingSet(int buildingSetId)
	{
	}

	// Token: 0x06002782 RID: 10114 RVA: 0x001216DA File Offset: 0x0011FADA
	public void LoadClipboard()
	{
		Clipboard.LoadCallback(PlayerPrefs.GetString("tempClipboard"));
	}

	// Token: 0x06002783 RID: 10115 RVA: 0x001216EB File Offset: 0x0011FAEB
	public void SaveClipboard(string clipboardDataStr)
	{
		PlayerPrefs.SetString("tempClipboard", clipboardDataStr);
	}

	// Token: 0x06002784 RID: 10116 RVA: 0x001216F8 File Offset: 0x0011FAF8
	public void CompletePuzzlePlay()
	{
	}

	// Token: 0x06002785 RID: 10117 RVA: 0x001216FA File Offset: 0x0011FAFA
	public void CompletePuzzleBuild()
	{
	}

	// Token: 0x06002786 RID: 10118 RVA: 0x001216FC File Offset: 0x0011FAFC
	public void SetWorldUpvoted(bool upvoted)
	{
	}

	// Token: 0x06002787 RID: 10119 RVA: 0x001216FE File Offset: 0x0011FAFE
	public bool IsWorldUpvoted()
	{
		return false;
	}

	// Token: 0x06002788 RID: 10120 RVA: 0x00121701 File Offset: 0x0011FB01
	public bool ScreenRecordingAvailable()
	{
		return Options.ShowScreenRecordButtonsInEditor;
	}

	// Token: 0x06002789 RID: 10121 RVA: 0x00121708 File Offset: 0x0011FB08
	public void StartRecordingScreen()
	{
		this._screenRecordingInProgress = true;
	}

	// Token: 0x0600278A RID: 10122 RVA: 0x00121711 File Offset: 0x0011FB11
	public void StopRecordingScreen()
	{
		this._screenRecordingInProgress = false;
	}

	// Token: 0x0600278B RID: 10123 RVA: 0x0012171A File Offset: 0x0011FB1A
	public bool ScreenRecordingInProgress()
	{
		return this._screenRecordingInProgress;
	}

	// Token: 0x0600278C RID: 10124 RVA: 0x00121722 File Offset: 0x0011FB22
	public bool NativeModalActive()
	{
		return false;
	}

	// Token: 0x0600278D RID: 10125 RVA: 0x00121725 File Offset: 0x0011FB25
	public void ShowSharingPopup(Rect shareRect)
	{
	}

	// Token: 0x0600278E RID: 10126 RVA: 0x00121727 File Offset: 0x0011FB27
	public void ShowSharingPopupWithMessage(Rect shareRect, string message)
	{
	}

	// Token: 0x0600278F RID: 10127 RVA: 0x00121729 File Offset: 0x0011FB29
	public void TrackAchievementIncrease(string internalIdentifier, int progress)
	{
		BWLog.Info(string.Concat(new object[]
		{
			"Tracked achievement increase '",
			internalIdentifier,
			"' progress: ",
			progress
		}));
	}

	// Token: 0x06002790 RID: 10128 RVA: 0x00121758 File Offset: 0x0011FB58
	public void AddItemsToCart(BlocksInventory itemsInventory)
	{
		BWStandalone.Instance.AddItemsToCartFromWorld(itemsInventory);
	}

	// Token: 0x04002257 RID: 8791
	private bool _screenRecordingInProgress;
}
