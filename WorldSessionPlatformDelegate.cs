using System;
using UnityEngine;

// Token: 0x0200036E RID: 878
public interface WorldSessionPlatformDelegate
{
	// Token: 0x06002752 RID: 10066
	void WorldDidStartLoading();

	// Token: 0x06002753 RID: 10067
	void WorldDidFinishLoading();

	// Token: 0x06002754 RID: 10068
	void WorldWillQuit(string deepLinkStr);

	// Token: 0x06002755 RID: 10069
	void WorldDidQuit();

	// Token: 0x06002756 RID: 10070
	void SaveCurrentWorldData();

	// Token: 0x06002757 RID: 10071
	void SaveCurrentWorldDataWithScreenshot(byte[] imageData);

	// Token: 0x06002758 RID: 10072
	void SetProfileWorldData(string worldSource, string avatarSource, string profileGender);

	// Token: 0x06002759 RID: 10073
	void SendScreenShot(byte[] imageData, string label);

	// Token: 0x0600275A RID: 10074
	void ImageWriteToSavedPhotosAlbum(byte[] imageData);

	// Token: 0x0600275B RID: 10075
	void NotifyWorldTooNew();

	// Token: 0x0600275C RID: 10076
	void SaveAsNewModel(string modelTitle, string modelSource, string blocksInventoryStr, string hash, byte[] iconBytesSD, byte[] iconBytesHD, byte[] imageBytesSD, byte[] imageBytesHD);

	// Token: 0x0600275D RID: 10077
	string GetModelSource(string modelType, string modelId);

	// Token: 0x0600275E RID: 10078
	Texture2D GetCurrentlyLoadedPreviewModelImage();

	// Token: 0x0600275F RID: 10079
	void RequestMissingModelIcon(string modelType, string modelId);

	// Token: 0x06002760 RID: 10080
	void PurchaseCurrentlyLoadedModel();

	// Token: 0x06002761 RID: 10081
	void PurchaseBuildingSet(int buildingSetId);

	// Token: 0x06002762 RID: 10082
	void LoadClipboard();

	// Token: 0x06002763 RID: 10083
	void SaveClipboard(string clipboardDataStr);

	// Token: 0x06002764 RID: 10084
	void CompletePuzzlePlay();

	// Token: 0x06002765 RID: 10085
	void CompletePuzzleBuild();

	// Token: 0x06002766 RID: 10086
	void SetWorldUpvoted(bool upvoted);

	// Token: 0x06002767 RID: 10087
	bool IsWorldUpvoted();

	// Token: 0x06002768 RID: 10088
	bool ScreenRecordingAvailable();

	// Token: 0x06002769 RID: 10089
	void StartRecordingScreen();

	// Token: 0x0600276A RID: 10090
	void StopRecordingScreen();

	// Token: 0x0600276B RID: 10091
	bool ScreenRecordingInProgress();

	// Token: 0x0600276C RID: 10092
	bool NativeModalActive();

	// Token: 0x0600276D RID: 10093
	void ShowSharingPopup(Rect shareRect);

	// Token: 0x0600276E RID: 10094
	void ShowSharingPopupWithMessage(Rect shareRect, string message);

	// Token: 0x0600276F RID: 10095
	void TrackAchievementIncrease(string internalIdentifier, int progress);

	// Token: 0x06002770 RID: 10096
	void AddItemsToCart(BlocksInventory itemsInventory);
}
