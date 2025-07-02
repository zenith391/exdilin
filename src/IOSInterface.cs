using System.Runtime.InteropServices;
using UnityEngine;

public static class IOSInterface
{
	public static string GetCachePath()
	{
		return blocksworld_cache_path();
	}

	public static void SetProfileWorldData(string serializedData, string avatar, string profileGenderStr)
	{
		blocksworld_set_profile_world_data(serializedData, avatar, profileGenderStr);
	}

	public static void SaveCurrentWorldData(string sourceJsonStr, bool hasWinCondition, byte[] screenshotImageData = null)
	{
		int imageDataLength = ((screenshotImageData != null) ? screenshotImageData.Length : 0);
		blocksworld_set_current_world_data(sourceJsonStr, hasWinCondition, screenshotImageData, imageDataLength);
	}

	public static void SendScreenshot(byte[] data)
	{
		blocksworld_send_screenshot(data, (data != null) ? data.Length : 0);
	}

	public static void ImageWriteToSavedPhotosAlbum(byte[] data)
	{
		blocksworld_image_write_to_saved_photos_album(data, (data != null) ? data.Length : 0);
	}

	public static void BlocksworldSceneLoaded()
	{
		blocksworld_scene_loaded();
	}

	public static void WorldDidStartLoading()
	{
		blocksworld_world_did_start_loading();
	}

	public static void WorldDidFinishLoading()
	{
		blocksworld_world_did_finish_loading();
	}

	public static void WorldWillQuit(string deepLinkStr)
	{
		blocksworld_world_will_quit((deepLinkStr != null) ? deepLinkStr : string.Empty);
	}

	public static void WorldDidQuit()
	{
		blocksworld_world_did_quit();
	}

	public static void NotifyFileTooNew()
	{
		blocksworld_notify_file_too_new();
	}

	public static float ScreenScale()
	{
		return blocksworld_screen_scale();
	}

	public static void SendPlayEvent()
	{
		blocksworld_send_play_event();
	}

	public static void SendStopEvent()
	{
		blocksworld_send_stop_event();
	}

	public static void SendTutorialStepEvent(int step)
	{
		blocksworld_send_tutorial_step_event(step);
	}

	public static void SendAnalyticsEvent(string name, string extra)
	{
		blocksworld_send_analytics_event(name, extra);
	}

	public static void SendAnalyticsEvent(string name)
	{
		blocksworld_send_analytics_event(name, null);
	}

	public static void TrackAchievementIncrease(string internalIdentifier, int progress)
	{
		blocksworld_track_achievement_increase(internalIdentifier, progress);
	}

	public static void CompletePuzzleBuild()
	{
		blocksworld_complete_puzzle_build();
	}

	public static void CompletePuzzlePlay()
	{
		blocksworld_complete_puzzle_play();
	}

	public static void SetWorldUpvoted(bool upvoted)
	{
		blocksworld_set_world_upvoted(upvoted ? 1 : 0);
	}

	public static void LoadClipboard()
	{
		blocksworld_load_clipboard();
	}

	public static void SaveClipboard(string clipboard)
	{
		blocksworld_save_clipboard(clipboard);
	}

	public static void SaveAsNewModel(string name, string modelData, string gafUsage, string hash, byte[] iconBytesSD, byte[] iconBytesHD, byte[] snapshotBytesSD, byte[] snapshotBytesHD)
	{
		if (iconBytesSD == null || iconBytesSD.Length == 0)
		{
			BWLog.Error("SD icon missing");
		}
		else if (iconBytesHD == null || iconBytesHD.Length == 0)
		{
			BWLog.Error("HD icon missing");
		}
		else if (snapshotBytesSD == null || snapshotBytesSD.Length == 0)
		{
			BWLog.Error("SD snapshot missing");
		}
		else if (snapshotBytesHD == null || snapshotBytesHD.Length == 0)
		{
			BWLog.Error("HD snapshot missing");
		}
		else
		{
			blocksworld_save_model(name, modelData, gafUsage, hash, iconBytesSD, iconBytesHD, snapshotBytesSD, snapshotBytesHD, iconBytesSD.Length, iconBytesHD.Length, snapshotBytesSD.Length, snapshotBytesHD.Length);
		}
	}

	public static void PurchaseBuildingSet(int buildingSetID)
	{
		blocksworld_purchase_building_set(buildingSetID);
	}

	public static string GetModelSource(string modelType, string modelId)
	{
		return blocksworld_get_model_source(modelType, modelId);
	}

	public static void RequestMissingModelIcon(string modelType, string modelId)
	{
		blocksworld_request_missing_model_icon(modelType, modelId);
	}

	public static void PurchaseCurrentlyLoadedModel()
	{
		blocksworld_purchase_currently_loaded_model();
	}

	public static string GetCurrentlyLoadedPreviewModelImageData()
	{
		return blocksworld_get_currently_loaded_preview_model_image_data();
	}

	public static bool ScreenRecordingAvailable()
	{
		return blocksworld_screen_recording_available() != 0;
	}

	public static void StartRecordingScreen()
	{
		blocksworld_start_recording_screen();
	}

	public static void StopRecordingScreen()
	{
		blocksworld_stop_recording_screen();
	}

	public static bool IsStartingRecording()
	{
		return blocksworld_is_starting_recording() != 0;
	}

	public static bool ScreenRecordingInProgress()
	{
		return blocksworld_screen_recording_in_progress() != 0;
	}

	public static bool NativeModalActive()
	{
		return blocksworld_native_modal_active() != 0;
	}

	public static void ShowSharingPopup(Rect sharedRect)
	{
		blocksworld_show_sharing_popup(sharedRect.x, sharedRect.y, sharedRect.width, sharedRect.height);
	}

	public static void ShowSharingPopupWithMessage(Rect shareRect, string message)
	{
		blocksworld_show_sharing_popup_message(shareRect.x, shareRect.y, shareRect.width, shareRect.height, message);
	}

	public static void ApiGet(int requestId, string url, string paramsJsonString)
	{
		blocksworld_api_get(requestId, url, paramsJsonString);
	}

	public static void ApiPost(int requestId, string url, string paramsJsonString)
	{
		blocksworld_api_post(requestId, url, paramsJsonString);
	}

	public static void ApiPut(int requestId, string url, string paramsJsonString)
	{
		blocksworld_api_put(requestId, url, paramsJsonString);
	}

	public static void ApiDelete(int requestId, string url)
	{
		blocksworld_api_delete(requestId, url);
	}

	public static void AddItemsToCart(string itemsInventoryStr)
	{
		blocksworld_add_items_to_cart(itemsInventoryStr);
	}

	[DllImport("__Internal")]
	private static extern string blocksworld_cache_path();

	[DllImport("__Internal")]
	private static extern void blocksworld_set_profile_world_data(string serializedData, string avatar, string profileGender);

	[DllImport("__Internal")]
	private static extern void blocksworld_set_current_world_data(string serializedData, bool hasWinCondition, byte[] screenshotImageData, int imageDataLength);

	[DllImport("__Internal")]
	private static extern void blocksworld_send_screenshot(byte[] screenshotBytes, int screenshotBytesSize);

	[DllImport("__Internal")]
	private static extern void blocksworld_image_write_to_saved_photos_album(byte[] imageData, int imageDataLength);

	[DllImport("__Internal")]
	private static extern void blocksworld_scene_loaded();

	[DllImport("__Internal")]
	private static extern void blocksworld_world_did_start_loading();

	[DllImport("__Internal")]
	private static extern void blocksworld_world_did_finish_loading();

	[DllImport("__Internal")]
	private static extern void blocksworld_world_will_quit(string deepLinkStr);

	[DllImport("__Internal")]
	private static extern void blocksworld_world_did_quit();

	[DllImport("__Internal")]
	private static extern void blocksworld_notify_file_too_new();

	[DllImport("__Internal")]
	private static extern float blocksworld_screen_scale();

	[DllImport("__Internal")]
	private static extern void blocksworld_send_play_event();

	[DllImport("__Internal")]
	private static extern void blocksworld_send_stop_event();

	[DllImport("__Internal")]
	private static extern void blocksworld_send_analytics_event(string name, string extra);

	[DllImport("__Internal")]
	private static extern void blocksworld_track_achievement_increase(string internalIdentifier, int progress);

	[DllImport("__Internal")]
	private static extern void blocksworld_complete_puzzle_build();

	[DllImport("__Internal")]
	private static extern void blocksworld_complete_puzzle_play();

	[DllImport("__Internal")]
	private static extern void blocksworld_send_tutorial_step_event(int step);

	[DllImport("__Internal")]
	private static extern void blocksworld_set_world_upvoted(int upvoted);

	[DllImport("__Internal")]
	private static extern void blocksworld_load_clipboard();

	[DllImport("__Internal")]
	private static extern void blocksworld_save_clipboard(string clipboard);

	[DllImport("__Internal")]
	private static extern void blocksworld_save_model(string name, string modelData, string gafUsage, string hash, byte[] iconBytesSD, byte[] iconBytesHD, byte[] snapshotBytesSD, byte[] snapshotBytesHD, int iconBytesSDSize, int iconBytesHDSize, int snapshotBytesSDSize, int snapshotBytesHDSize);

	[DllImport("__Internal")]
	private static extern string blocksworld_get_model_source(string modelType, string modelId);

	[DllImport("__Internal")]
	private static extern void blocksworld_request_missing_model_icon(string modelType, string modelId);

	[DllImport("__Internal")]
	private static extern void blocksworld_purchase_currently_loaded_model();

	[DllImport("__Internal")]
	private static extern void blocksworld_purchase_building_set(int buildingSetID);

	[DllImport("__Internal")]
	private static extern string blocksworld_get_currently_loaded_preview_model_image_data();

	[DllImport("__Internal")]
	private static extern int blocksworld_screen_recording_available();

	[DllImport("__Internal")]
	private static extern void blocksworld_start_recording_screen();

	[DllImport("__Internal")]
	private static extern void blocksworld_stop_recording_screen();

	[DllImport("__Internal")]
	private static extern int blocksworld_is_starting_recording();

	[DllImport("__Internal")]
	private static extern int blocksworld_screen_recording_in_progress();

	[DllImport("__Internal")]
	private static extern int blocksworld_native_modal_active();

	[DllImport("__Internal")]
	private static extern void blocksworld_show_sharing_popup(float x, float y, float width, float height);

	[DllImport("__Internal")]
	private static extern void blocksworld_show_sharing_popup_message(float x, float y, float width, float height, string message);

	[DllImport("__Internal")]
	private static extern void blocksworld_api_get(int requestId, string url, string paramsJsonString);

	[DllImport("__Internal")]
	private static extern void blocksworld_api_post(int requestId, string url, string paramsJsonString);

	[DllImport("__Internal")]
	private static extern void blocksworld_api_put(int requestId, string url, string paramsJsonString);

	[DllImport("__Internal")]
	private static extern void blocksworld_api_delete(int requestId, string url);

	[DllImport("__Internal")]
	private static extern void blocksworld_add_items_to_cart(string itemsInventoryStr);
}
