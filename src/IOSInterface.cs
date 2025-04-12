using System;
using System.Runtime.InteropServices;
using UnityEngine;

// Token: 0x0200019E RID: 414
public static class IOSInterface
{
	// Token: 0x060016F7 RID: 5879 RVA: 0x000A495B File Offset: 0x000A2D5B
	public static string GetCachePath()
	{
		return IOSInterface.blocksworld_cache_path();
	}

	// Token: 0x060016F8 RID: 5880 RVA: 0x000A4962 File Offset: 0x000A2D62
	public static void SetProfileWorldData(string serializedData, string avatar, string profileGenderStr)
	{
		IOSInterface.blocksworld_set_profile_world_data(serializedData, avatar, profileGenderStr);
	}

	// Token: 0x060016F9 RID: 5881 RVA: 0x000A496C File Offset: 0x000A2D6C
	public static void SaveCurrentWorldData(string sourceJsonStr, bool hasWinCondition, byte[] screenshotImageData = null)
	{
		int imageDataLength = (screenshotImageData == null) ? 0 : screenshotImageData.Length;
		IOSInterface.blocksworld_set_current_world_data(sourceJsonStr, hasWinCondition, screenshotImageData, imageDataLength);
	}

	// Token: 0x060016FA RID: 5882 RVA: 0x000A4992 File Offset: 0x000A2D92
	public static void SendScreenshot(byte[] data)
	{
		IOSInterface.blocksworld_send_screenshot(data, (data == null) ? 0 : data.Length);
	}

	// Token: 0x060016FB RID: 5883 RVA: 0x000A49A9 File Offset: 0x000A2DA9
	public static void ImageWriteToSavedPhotosAlbum(byte[] data)
	{
		IOSInterface.blocksworld_image_write_to_saved_photos_album(data, (data == null) ? 0 : data.Length);
	}

	// Token: 0x060016FC RID: 5884 RVA: 0x000A49C0 File Offset: 0x000A2DC0
	public static void BlocksworldSceneLoaded()
	{
		IOSInterface.blocksworld_scene_loaded();
	}

	// Token: 0x060016FD RID: 5885 RVA: 0x000A49C7 File Offset: 0x000A2DC7
	public static void WorldDidStartLoading()
	{
		IOSInterface.blocksworld_world_did_start_loading();
	}

	// Token: 0x060016FE RID: 5886 RVA: 0x000A49CE File Offset: 0x000A2DCE
	public static void WorldDidFinishLoading()
	{
		IOSInterface.blocksworld_world_did_finish_loading();
	}

	// Token: 0x060016FF RID: 5887 RVA: 0x000A49D5 File Offset: 0x000A2DD5
	public static void WorldWillQuit(string deepLinkStr)
	{
		IOSInterface.blocksworld_world_will_quit((deepLinkStr != null) ? deepLinkStr : string.Empty);
	}

	// Token: 0x06001700 RID: 5888 RVA: 0x000A49ED File Offset: 0x000A2DED
	public static void WorldDidQuit()
	{
		IOSInterface.blocksworld_world_did_quit();
	}

	// Token: 0x06001701 RID: 5889 RVA: 0x000A49F4 File Offset: 0x000A2DF4
	public static void NotifyFileTooNew()
	{
		IOSInterface.blocksworld_notify_file_too_new();
	}

	// Token: 0x06001702 RID: 5890 RVA: 0x000A49FB File Offset: 0x000A2DFB
	public static float ScreenScale()
	{
		return IOSInterface.blocksworld_screen_scale();
	}

	// Token: 0x06001703 RID: 5891 RVA: 0x000A4A02 File Offset: 0x000A2E02
	public static void SendPlayEvent()
	{
		IOSInterface.blocksworld_send_play_event();
	}

	// Token: 0x06001704 RID: 5892 RVA: 0x000A4A09 File Offset: 0x000A2E09
	public static void SendStopEvent()
	{
		IOSInterface.blocksworld_send_stop_event();
	}

	// Token: 0x06001705 RID: 5893 RVA: 0x000A4A10 File Offset: 0x000A2E10
	public static void SendTutorialStepEvent(int step)
	{
		IOSInterface.blocksworld_send_tutorial_step_event(step);
	}

	// Token: 0x06001706 RID: 5894 RVA: 0x000A4A18 File Offset: 0x000A2E18
	public static void SendAnalyticsEvent(string name, string extra)
	{
		IOSInterface.blocksworld_send_analytics_event(name, extra);
	}

	// Token: 0x06001707 RID: 5895 RVA: 0x000A4A21 File Offset: 0x000A2E21
	public static void SendAnalyticsEvent(string name)
	{
		IOSInterface.blocksworld_send_analytics_event(name, null);
	}

	// Token: 0x06001708 RID: 5896 RVA: 0x000A4A2A File Offset: 0x000A2E2A
	public static void TrackAchievementIncrease(string internalIdentifier, int progress)
	{
		IOSInterface.blocksworld_track_achievement_increase(internalIdentifier, progress);
	}

	// Token: 0x06001709 RID: 5897 RVA: 0x000A4A33 File Offset: 0x000A2E33
	public static void CompletePuzzleBuild()
	{
		IOSInterface.blocksworld_complete_puzzle_build();
	}

	// Token: 0x0600170A RID: 5898 RVA: 0x000A4A3A File Offset: 0x000A2E3A
	public static void CompletePuzzlePlay()
	{
		IOSInterface.blocksworld_complete_puzzle_play();
	}

	// Token: 0x0600170B RID: 5899 RVA: 0x000A4A41 File Offset: 0x000A2E41
	public static void SetWorldUpvoted(bool upvoted)
	{
		IOSInterface.blocksworld_set_world_upvoted((!upvoted) ? 0 : 1);
	}

	// Token: 0x0600170C RID: 5900 RVA: 0x000A4A55 File Offset: 0x000A2E55
	public static void LoadClipboard()
	{
		IOSInterface.blocksworld_load_clipboard();
	}

	// Token: 0x0600170D RID: 5901 RVA: 0x000A4A5C File Offset: 0x000A2E5C
	public static void SaveClipboard(string clipboard)
	{
		IOSInterface.blocksworld_save_clipboard(clipboard);
	}

	// Token: 0x0600170E RID: 5902 RVA: 0x000A4A64 File Offset: 0x000A2E64
	public static void SaveAsNewModel(string name, string modelData, string gafUsage, string hash, byte[] iconBytesSD, byte[] iconBytesHD, byte[] snapshotBytesSD, byte[] snapshotBytesHD)
	{
		if (iconBytesSD == null || iconBytesSD.Length == 0)
		{
			BWLog.Error("SD icon missing");
			return;
		}
		if (iconBytesHD == null || iconBytesHD.Length == 0)
		{
			BWLog.Error("HD icon missing");
			return;
		}
		if (snapshotBytesSD == null || snapshotBytesSD.Length == 0)
		{
			BWLog.Error("SD snapshot missing");
			return;
		}
		if (snapshotBytesHD == null || snapshotBytesHD.Length == 0)
		{
			BWLog.Error("HD snapshot missing");
			return;
		}
		IOSInterface.blocksworld_save_model(name, modelData, gafUsage, hash, iconBytesSD, iconBytesHD, snapshotBytesSD, snapshotBytesHD, iconBytesSD.Length, iconBytesHD.Length, snapshotBytesSD.Length, snapshotBytesHD.Length);
	}

	// Token: 0x0600170F RID: 5903 RVA: 0x000A4AFE File Offset: 0x000A2EFE
	public static void PurchaseBuildingSet(int buildingSetID)
	{
		IOSInterface.blocksworld_purchase_building_set(buildingSetID);
	}

	// Token: 0x06001710 RID: 5904 RVA: 0x000A4B06 File Offset: 0x000A2F06
	public static string GetModelSource(string modelType, string modelId)
	{
		return IOSInterface.blocksworld_get_model_source(modelType, modelId);
	}

	// Token: 0x06001711 RID: 5905 RVA: 0x000A4B0F File Offset: 0x000A2F0F
	public static void RequestMissingModelIcon(string modelType, string modelId)
	{
		IOSInterface.blocksworld_request_missing_model_icon(modelType, modelId);
	}

	// Token: 0x06001712 RID: 5906 RVA: 0x000A4B18 File Offset: 0x000A2F18
	public static void PurchaseCurrentlyLoadedModel()
	{
		IOSInterface.blocksworld_purchase_currently_loaded_model();
	}

	// Token: 0x06001713 RID: 5907 RVA: 0x000A4B1F File Offset: 0x000A2F1F
	public static string GetCurrentlyLoadedPreviewModelImageData()
	{
		return IOSInterface.blocksworld_get_currently_loaded_preview_model_image_data();
	}

	// Token: 0x06001714 RID: 5908 RVA: 0x000A4B26 File Offset: 0x000A2F26
	public static bool ScreenRecordingAvailable()
	{
		return IOSInterface.blocksworld_screen_recording_available() != 0;
	}

	// Token: 0x06001715 RID: 5909 RVA: 0x000A4B33 File Offset: 0x000A2F33
	public static void StartRecordingScreen()
	{
		IOSInterface.blocksworld_start_recording_screen();
	}

	// Token: 0x06001716 RID: 5910 RVA: 0x000A4B3A File Offset: 0x000A2F3A
	public static void StopRecordingScreen()
	{
		IOSInterface.blocksworld_stop_recording_screen();
	}

	// Token: 0x06001717 RID: 5911 RVA: 0x000A4B41 File Offset: 0x000A2F41
	public static bool IsStartingRecording()
	{
		return IOSInterface.blocksworld_is_starting_recording() != 0;
	}

	// Token: 0x06001718 RID: 5912 RVA: 0x000A4B4E File Offset: 0x000A2F4E
	public static bool ScreenRecordingInProgress()
	{
		return IOSInterface.blocksworld_screen_recording_in_progress() != 0;
	}

	// Token: 0x06001719 RID: 5913 RVA: 0x000A4B5B File Offset: 0x000A2F5B
	public static bool NativeModalActive()
	{
		return IOSInterface.blocksworld_native_modal_active() != 0;
	}

	// Token: 0x0600171A RID: 5914 RVA: 0x000A4B68 File Offset: 0x000A2F68
	public static void ShowSharingPopup(Rect sharedRect)
	{
		IOSInterface.blocksworld_show_sharing_popup(sharedRect.x, sharedRect.y, sharedRect.width, sharedRect.height);
	}

	// Token: 0x0600171B RID: 5915 RVA: 0x000A4B8B File Offset: 0x000A2F8B
	public static void ShowSharingPopupWithMessage(Rect shareRect, string message)
	{
		IOSInterface.blocksworld_show_sharing_popup_message(shareRect.x, shareRect.y, shareRect.width, shareRect.height, message);
	}

	// Token: 0x0600171C RID: 5916 RVA: 0x000A4BAF File Offset: 0x000A2FAF
	public static void ApiGet(int requestId, string url, string paramsJsonString)
	{
		IOSInterface.blocksworld_api_get(requestId, url, paramsJsonString);
	}

	// Token: 0x0600171D RID: 5917 RVA: 0x000A4BB9 File Offset: 0x000A2FB9
	public static void ApiPost(int requestId, string url, string paramsJsonString)
	{
		IOSInterface.blocksworld_api_post(requestId, url, paramsJsonString);
	}

	// Token: 0x0600171E RID: 5918 RVA: 0x000A4BC3 File Offset: 0x000A2FC3
	public static void ApiPut(int requestId, string url, string paramsJsonString)
	{
		IOSInterface.blocksworld_api_put(requestId, url, paramsJsonString);
	}

	// Token: 0x0600171F RID: 5919 RVA: 0x000A4BCD File Offset: 0x000A2FCD
	public static void ApiDelete(int requestId, string url)
	{
		IOSInterface.blocksworld_api_delete(requestId, url);
	}

	// Token: 0x06001720 RID: 5920 RVA: 0x000A4BD6 File Offset: 0x000A2FD6
	public static void AddItemsToCart(string itemsInventoryStr)
	{
		IOSInterface.blocksworld_add_items_to_cart(itemsInventoryStr);
	}

	// Token: 0x06001721 RID: 5921
	[DllImport("__Internal")]
	private static extern string blocksworld_cache_path();

	// Token: 0x06001722 RID: 5922
	[DllImport("__Internal")]
	private static extern void blocksworld_set_profile_world_data(string serializedData, string avatar, string profileGender);

	// Token: 0x06001723 RID: 5923
	[DllImport("__Internal")]
	private static extern void blocksworld_set_current_world_data(string serializedData, bool hasWinCondition, byte[] screenshotImageData, int imageDataLength);

	// Token: 0x06001724 RID: 5924
	[DllImport("__Internal")]
	private static extern void blocksworld_send_screenshot(byte[] screenshotBytes, int screenshotBytesSize);

	// Token: 0x06001725 RID: 5925
	[DllImport("__Internal")]
	private static extern void blocksworld_image_write_to_saved_photos_album(byte[] imageData, int imageDataLength);

	// Token: 0x06001726 RID: 5926
	[DllImport("__Internal")]
	private static extern void blocksworld_scene_loaded();

	// Token: 0x06001727 RID: 5927
	[DllImport("__Internal")]
	private static extern void blocksworld_world_did_start_loading();

	// Token: 0x06001728 RID: 5928
	[DllImport("__Internal")]
	private static extern void blocksworld_world_did_finish_loading();

	// Token: 0x06001729 RID: 5929
	[DllImport("__Internal")]
	private static extern void blocksworld_world_will_quit(string deepLinkStr);

	// Token: 0x0600172A RID: 5930
	[DllImport("__Internal")]
	private static extern void blocksworld_world_did_quit();

	// Token: 0x0600172B RID: 5931
	[DllImport("__Internal")]
	private static extern void blocksworld_notify_file_too_new();

	// Token: 0x0600172C RID: 5932
	[DllImport("__Internal")]
	private static extern float blocksworld_screen_scale();

	// Token: 0x0600172D RID: 5933
	[DllImport("__Internal")]
	private static extern void blocksworld_send_play_event();

	// Token: 0x0600172E RID: 5934
	[DllImport("__Internal")]
	private static extern void blocksworld_send_stop_event();

	// Token: 0x0600172F RID: 5935
	[DllImport("__Internal")]
	private static extern void blocksworld_send_analytics_event(string name, string extra);

	// Token: 0x06001730 RID: 5936
	[DllImport("__Internal")]
	private static extern void blocksworld_track_achievement_increase(string internalIdentifier, int progress);

	// Token: 0x06001731 RID: 5937
	[DllImport("__Internal")]
	private static extern void blocksworld_complete_puzzle_build();

	// Token: 0x06001732 RID: 5938
	[DllImport("__Internal")]
	private static extern void blocksworld_complete_puzzle_play();

	// Token: 0x06001733 RID: 5939
	[DllImport("__Internal")]
	private static extern void blocksworld_send_tutorial_step_event(int step);

	// Token: 0x06001734 RID: 5940
	[DllImport("__Internal")]
	private static extern void blocksworld_set_world_upvoted(int upvoted);

	// Token: 0x06001735 RID: 5941
	[DllImport("__Internal")]
	private static extern void blocksworld_load_clipboard();

	// Token: 0x06001736 RID: 5942
	[DllImport("__Internal")]
	private static extern void blocksworld_save_clipboard(string clipboard);

	// Token: 0x06001737 RID: 5943
	[DllImport("__Internal")]
	private static extern void blocksworld_save_model(string name, string modelData, string gafUsage, string hash, byte[] iconBytesSD, byte[] iconBytesHD, byte[] snapshotBytesSD, byte[] snapshotBytesHD, int iconBytesSDSize, int iconBytesHDSize, int snapshotBytesSDSize, int snapshotBytesHDSize);

	// Token: 0x06001738 RID: 5944
	[DllImport("__Internal")]
	private static extern string blocksworld_get_model_source(string modelType, string modelId);

	// Token: 0x06001739 RID: 5945
	[DllImport("__Internal")]
	private static extern void blocksworld_request_missing_model_icon(string modelType, string modelId);

	// Token: 0x0600173A RID: 5946
	[DllImport("__Internal")]
	private static extern void blocksworld_purchase_currently_loaded_model();

	// Token: 0x0600173B RID: 5947
	[DllImport("__Internal")]
	private static extern void blocksworld_purchase_building_set(int buildingSetID);

	// Token: 0x0600173C RID: 5948
	[DllImport("__Internal")]
	private static extern string blocksworld_get_currently_loaded_preview_model_image_data();

	// Token: 0x0600173D RID: 5949
	[DllImport("__Internal")]
	private static extern int blocksworld_screen_recording_available();

	// Token: 0x0600173E RID: 5950
	[DllImport("__Internal")]
	private static extern void blocksworld_start_recording_screen();

	// Token: 0x0600173F RID: 5951
	[DllImport("__Internal")]
	private static extern void blocksworld_stop_recording_screen();

	// Token: 0x06001740 RID: 5952
	[DllImport("__Internal")]
	private static extern int blocksworld_is_starting_recording();

	// Token: 0x06001741 RID: 5953
	[DllImport("__Internal")]
	private static extern int blocksworld_screen_recording_in_progress();

	// Token: 0x06001742 RID: 5954
	[DllImport("__Internal")]
	private static extern int blocksworld_native_modal_active();

	// Token: 0x06001743 RID: 5955
	[DllImport("__Internal")]
	private static extern void blocksworld_show_sharing_popup(float x, float y, float width, float height);

	// Token: 0x06001744 RID: 5956
	[DllImport("__Internal")]
	private static extern void blocksworld_show_sharing_popup_message(float x, float y, float width, float height, string message);

	// Token: 0x06001745 RID: 5957
	[DllImport("__Internal")]
	private static extern void blocksworld_api_get(int requestId, string url, string paramsJsonString);

	// Token: 0x06001746 RID: 5958
	[DllImport("__Internal")]
	private static extern void blocksworld_api_post(int requestId, string url, string paramsJsonString);

	// Token: 0x06001747 RID: 5959
	[DllImport("__Internal")]
	private static extern void blocksworld_api_put(int requestId, string url, string paramsJsonString);

	// Token: 0x06001748 RID: 5960
	[DllImport("__Internal")]
	private static extern void blocksworld_api_delete(int requestId, string url);

	// Token: 0x06001749 RID: 5961
	[DllImport("__Internal")]
	private static extern void blocksworld_add_items_to_cart(string itemsInventoryStr);
}
