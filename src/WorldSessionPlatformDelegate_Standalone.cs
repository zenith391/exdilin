using UnityEngine;

public class WorldSessionPlatformDelegate_Standalone : WorldSessionPlatformDelegate
{
	private bool _screenRecordingInProgress;

	public void WorldDidStartLoading()
	{
	}

	public void WorldDidFinishLoading()
	{
		Blocksworld.EnableGameCameras(enableCams: true);
		if (BWStandalone.Instance != null)
		{
			MenuInputHandler.RequestControl(Blocksworld.UI);
			BWStandalone.Overlays.ShowLoadingOverlay(show: false);
			MainUIController.Instance.HideAll();
		}
	}

	public void WorldWillQuit(string deepLinkStr)
	{
		if (BWStandalone.Instance != null)
		{
			MenuInputHandler.Clear();
			if (!string.IsNullOrEmpty(deepLinkStr))
			{
				BWStandalone.Instance.HandleWorldExitMessage(deepLinkStr);
			}
			Blocksworld.EnableGameCameras(enableCams: false);
			MainUIController.Instance.Show();
			BWStandalone.Overlays.ShowLoadingOverlay(show: true);
		}
	}

	public void WorldDidQuit()
	{
		if (BWStandalone.Instance != null)
		{
			BWStandalone.Instance.ReturnToMenu();
		}
	}

	public void SaveCurrentWorldData()
	{
		BWStandalone.Instance.SaveCurrentWorldSession(null);
	}

	public void SaveCurrentWorldDataWithScreenshot(byte[] imageData)
	{
		BWStandalone.Instance.SaveCurrentWorldSession(imageData);
	}

	public void SetProfileWorldData(string worldSource, string avatarSource, string profileGender)
	{
		BWStandalone.Instance.SetProfileWorldData(worldSource, avatarSource, profileGender);
	}

	public void SendScreenShot(byte[] imageData, string label)
	{
		BWStandalone.Instance.SendScreenshot(imageData, label);
	}

	public void ImageWriteToSavedPhotosAlbum(byte[] imageData)
	{
		string text = WorldSession.current.config.worldTitle;
		if (string.IsNullOrEmpty(text))
		{
			text = "bwscreenshot";
		}
		SendScreenShot(imageData, text);
	}

	public void NotifyWorldTooNew()
	{
	}

	public void SaveAsNewModel(string modelTitle, string modelSource, string blocksInventoryStr, string hash, byte[] iconBytesSD, byte[] iconBytesHD, byte[] imageBytesSD, byte[] imageBytesHD)
	{
		BWUserModel bWUserModel = new BWUserModel(modelTitle, modelSource, blocksInventoryStr, hash);
		bWUserModel.GenerateImageChecksums(iconBytesSD, iconBytesHD, imageBytesSD, imageBytesHD);
		BWUserModelsDataManager.Instance.SaveModelLocal(bWUserModel, iconBytesSD, iconBytesHD, imageBytesSD, imageBytesHD);
		BWUserModelsDataManager.Instance.CreateRemoteModel(bWUserModel, iconBytesSD, iconBytesHD, imageBytesSD, imageBytesHD, null);
	}

	public string GetModelSource(string modelType, string modelId)
	{
		return BWUserModelsDataManager.Instance.LoadSourceForModel(modelType, modelId);
	}

	public Texture2D GetCurrentlyLoadedPreviewModelImage()
	{
		return (Texture2D)Resources.Load("GUI/TabBar/Panel Tab Gear HD");
	}

	public void RequestMissingModelIcon(string modelType, string modelId)
	{
	}

	public void PurchaseCurrentlyLoadedModel()
	{
		BWStandalone.Instance.PurchaseCurrentlyLoadedModel();
		WorldSession.ModelPurchaseCallback("success");
	}

	public void PurchaseBuildingSet(int buildingSetId)
	{
	}

	public void LoadClipboard()
	{
		Clipboard.LoadCallback(PlayerPrefs.GetString("tempClipboard"));
	}

	public void SaveClipboard(string clipboardDataStr)
	{
		PlayerPrefs.SetString("tempClipboard", clipboardDataStr);
	}

	public void CompletePuzzlePlay()
	{
	}

	public void CompletePuzzleBuild()
	{
	}

	public void SetWorldUpvoted(bool upvoted)
	{
	}

	public bool IsWorldUpvoted()
	{
		return false;
	}

	public bool ScreenRecordingAvailable()
	{
		return Options.ShowScreenRecordButtonsInEditor;
	}

	public void StartRecordingScreen()
	{
		_screenRecordingInProgress = true;
	}

	public void StopRecordingScreen()
	{
		_screenRecordingInProgress = false;
	}

	public bool ScreenRecordingInProgress()
	{
		return _screenRecordingInProgress;
	}

	public bool NativeModalActive()
	{
		return false;
	}

	public void ShowSharingPopup(Rect shareRect)
	{
	}

	public void ShowSharingPopupWithMessage(Rect shareRect, string message)
	{
	}

	public void TrackAchievementIncrease(string internalIdentifier, int progress)
	{
		BWLog.Info("Tracked achievement increase '" + internalIdentifier + "' progress: " + progress);
	}

	public void AddItemsToCart(BlocksInventory itemsInventory)
	{
		BWStandalone.Instance.AddItemsToCartFromWorld(itemsInventory);
	}
}
