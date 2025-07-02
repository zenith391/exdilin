using UnityEngine;

public interface WorldSessionPlatformDelegate
{
	void WorldDidStartLoading();

	void WorldDidFinishLoading();

	void WorldWillQuit(string deepLinkStr);

	void WorldDidQuit();

	void SaveCurrentWorldData();

	void SaveCurrentWorldDataWithScreenshot(byte[] imageData);

	void SetProfileWorldData(string worldSource, string avatarSource, string profileGender);

	void SendScreenShot(byte[] imageData, string label);

	void ImageWriteToSavedPhotosAlbum(byte[] imageData);

	void NotifyWorldTooNew();

	void SaveAsNewModel(string modelTitle, string modelSource, string blocksInventoryStr, string hash, byte[] iconBytesSD, byte[] iconBytesHD, byte[] imageBytesSD, byte[] imageBytesHD);

	string GetModelSource(string modelType, string modelId);

	Texture2D GetCurrentlyLoadedPreviewModelImage();

	void RequestMissingModelIcon(string modelType, string modelId);

	void PurchaseCurrentlyLoadedModel();

	void PurchaseBuildingSet(int buildingSetId);

	void LoadClipboard();

	void SaveClipboard(string clipboardDataStr);

	void CompletePuzzlePlay();

	void CompletePuzzleBuild();

	void SetWorldUpvoted(bool upvoted);

	bool IsWorldUpvoted();

	bool ScreenRecordingAvailable();

	void StartRecordingScreen();

	void StopRecordingScreen();

	bool ScreenRecordingInProgress();

	bool NativeModalActive();

	void ShowSharingPopup(Rect shareRect);

	void ShowSharingPopupWithMessage(Rect shareRect, string message);

	void TrackAchievementIncrease(string internalIdentifier, int progress);

	void AddItemsToCart(BlocksInventory itemsInventory);
}
