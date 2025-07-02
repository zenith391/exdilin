using UnityEngine;

public static class Options
{
	private static bool DisplayBlockNames_Set;

	private static bool DisplayBlockNames_Fast;

	private static bool DisplayWheelBlockNames_Set;

	private static bool DisplayWheelBlockNames_Fast;

	private static bool ShowMisalignedBlocks_Set;

	private static bool ShowMisalignedBlocks_Fast;

	private static bool OnScreenActionDebug_Set;

	private static bool OnScreenActionDebug_Fast;

	public static bool LoadFromRemoteApiDevelop
	{
		get
		{
			return GetBool("LoadFromRemoteApiDevelop");
		}
		set
		{
			if (value)
			{
				SetBool("LoadFromRemoteApiDevelop", value: true);
				SetBool("LoadFromRemoteApiStaging", value: false);
				SetBool("LoadFromRemoteApiProduction", value: false);
				SetBool("LoadWorldSessionFromTestConfigFile", value: false);
				SetBool("LoadStarterIslandTemplateFile", value: false);
			}
			else
			{
				SetBool("LoadFromRemoteApiDevelop", value: false);
			}
		}
	}

	public static string LoadFromRemoteApiDevelop_GroupName => "Scene World Loader";

	public static bool LoadFromRemoteApiStaging
	{
		get
		{
			return GetBool("LoadFromRemoteApiStaging");
		}
		set
		{
			if (value)
			{
				SetBool("LoadFromRemoteApiDevelop", value: false);
				SetBool("LoadFromRemoteApiStaging", value: true);
				SetBool("LoadFromRemoteApiProduction", value: false);
				SetBool("LoadWorldSessionFromTestConfigFile", value: false);
				SetBool("LoadStarterIslandTemplateFile", value: false);
			}
			else
			{
				SetBool("LoadFromRemoteApiStaging", value: false);
			}
		}
	}

	public static string LoadFromRemoteApiStaging_GroupName => "Scene World Loader";

	public static bool LoadFromRemoteApiProduction
	{
		get
		{
			return GetBool("LoadFromRemoteApiProduction");
		}
		set
		{
			if (value)
			{
				SetBool("LoadFromRemoteApiDevelop", value: false);
				SetBool("LoadFromRemoteApiStaging", value: false);
				SetBool("LoadFromRemoteApiProduction", value: true);
				SetBool("LoadWorldSessionFromTestConfigFile", value: false);
				SetBool("LoadStarterIslandTemplateFile", value: false);
			}
			else
			{
				SetBool("LoadFromRemoteApiProduction", value: false);
			}
		}
	}

	public static string LoadFromRemoteApiProduction_GroupName => "Scene World Loader";

	public static bool BuildRemote
	{
		get
		{
			return GetBool("BuildRemote");
		}
		set
		{
			SetBool("BuildRemote", value);
		}
	}

	public static string BuildRemote_GroupName => "Scene World Loader";

	public static bool LoadUserProfile
	{
		get
		{
			return GetBool("LoadUserProfile");
		}
		set
		{
			SetBool("LoadUserProfile", value);
		}
	}

	public static string LoadUserProfile_GroupName => "Scene World Loader";

	public static int RemoteApiWorldId
	{
		get
		{
			return GetInt("RemoteApiWorldId");
		}
		set
		{
			SetInt("RemoteApiWorldId", value);
		}
	}

	public static string RemoteApiWorldId_GroupName => "Scene World Loader";

	public static string CurrentUserAuthToken
	{
		get
		{
			return GetString("CurrentUserAuthToken");
		}
		set
		{
			SetString("CurrentUserAuthToken", value);
		}
	}

	public static string CurrentUserAuthToken_GroupName => "Scene World Loader";

	public static bool LoadWorldSessionFromTestConfigFile
	{
		get
		{
			return GetBool("LoadWorldSessionFromTestConfigFile");
		}
		set
		{
			if (value)
			{
				SetBool("LoadFromRemoteApi", value: false);
				SetBool("LoadWorldSessionFromTestConfigFile", value);
				SetBool("LoadStarterIslandTemplateFile", value: false);
			}
		}
	}

	public static string LoadWorldSessionFromTestConfigFile_GroupName => "Scene World Loader";

	public static bool LoadStarterIslandTemplateFile
	{
		get
		{
			return GetBool("LoadStarterIslandTemplateFile");
		}
		set
		{
			if (value)
			{
				SetBool("LoadFromRemoteApi", value: false);
				SetBool("LoadWorldSessionFromTestConfigFile", value: false);
				SetBool("LoadStarterIslandTemplateFile", value);
			}
		}
	}

	public static string LoadStarterIslandTemplateFile_GroupName => "Scene World Loader";

	public static bool LoadLastLoadedWorldFromFile
	{
		get
		{
			if (!GetBool("LoadFromRemoteApi") && !GetBool("LoadWorldSessionFromTestConfigFile"))
			{
				return !GetBool("LoadStarterIslandTemplateFile");
			}
			return false;
		}
		set
		{
			if (value)
			{
				SetBool("LoadFromRemoteApi", value: false);
				SetBool("LoadWorldSessionFromTestConfigFile", value: false);
				SetBool("LoadStarterIslandTemplateFile", value: false);
			}
		}
	}

	public static string LoadLastLoadedWorldFromFile_GroupName => "Scene World Loader";

	public static string EditorProfileAvatar
	{
		get
		{
			return GetString("EditorProfileAvatar");
		}
		set
		{
			SetString("EditorProfileAvatar", value);
		}
	}

	public static bool BackupWorlds
	{
		get
		{
			return GetBool("BackupWorlds");
		}
		set
		{
			SetBool("BackupWorlds", value);
		}
	}

	public static string BackupWorlds_GroupName => "World Backup";

	public static bool DebugSFX
	{
		get
		{
			return GetBool("debug-sfx");
		}
		set
		{
			SetBool("debug-sfx", value);
		}
	}

	public static string DebugSFX_GroupName => "SFX and Music";

	public static bool SFXEnabled
	{
		get
		{
			return GetBool("sfx-enabled");
		}
		set
		{
			SetBool("sfx-enabled", value);
		}
	}

	public static string SFXEnabled_GroupName => "SFX and Music";

	public static bool BuildMusicEnabled
	{
		get
		{
			return GetBool("build-music-enabled");
		}
		set
		{
			SetBool("build-music-enabled", value);
		}
	}

	public static string BuildMusicEnabled_GroupName => "SFX and Music";

	public static bool PlayMusicEnabled
	{
		get
		{
			return GetBool("play-music-enabled");
		}
		set
		{
			SetBool("play-music-enabled", value);
		}
	}

	public static string PlayMusicEnabled_GroupName => "SFX and Music";

	public static bool LockTileOnNewBlocks
	{
		get
		{
			return GetBool("lock-tile-on-new-blocks");
		}
		set
		{
			SetBool("lock-tile-on-new-blocks", value);
		}
	}

	public static bool BlockVolumeDisplay
	{
		get
		{
			return GetBool("block-volume-display");
		}
		set
		{
			SetBool("block-volume-display", value);
		}
	}

	public static string BlockVolumeDisplay_GroupName => "Display";

	public static bool BlockCameraHintDisplay
	{
		get
		{
			return GetBool("block-camera-hint-display");
		}
		set
		{
			SetBool("block-camera-hint-display", value);
		}
	}

	public static string BlockCameraHintDisplay_GroupName => "Display";

	public static bool GlueVolumeDisplay
	{
		get
		{
			return GetBool("glue-volume-display");
		}
		set
		{
			SetBool("glue-volume-display", value);
		}
	}

	public static string GlueVolumeDisplay_GroupName => "Display";

	public static bool ShapeVolumeDisplay
	{
		get
		{
			return GetBool("shape-volume-display");
		}
		set
		{
			SetBool("shape-volume-display", value);
		}
	}

	public static string ShapeVolumeDisplay_GroupName => "Display";

	public static bool JointMeshDisplay
	{
		get
		{
			return GetBool("joint-mesh-display");
		}
		set
		{
			SetBool("joint-mesh-display", value);
		}
	}

	public static string JointMeshDisplay_GroupName => "Display";

	public static bool HideTutorialGraphics
	{
		get
		{
			return GetBool("HideTutorialGraphics");
		}
		set
		{
			SetBool("HideTutorialGraphics", value);
		}
	}

	public static string HideTutorialGraphics_GroupName => "Display";

	public static bool HideInGameUI
	{
		get
		{
			return GetBool("HideInGameUI");
		}
		set
		{
			SetBool("HideInGameUI", value);
		}
	}

	public static string HideInGameUI_GroupName => "Display";

	public static bool HideLeaderboard
	{
		get
		{
			return GetBool("HideLeaderboard");
		}
		set
		{
			SetBool("HideLeaderboard", value);
		}
	}

	public static string HideLeaderboard_GroupName => "Display";

	public static bool ShowForwardUpRightOnSelected
	{
		get
		{
			return GetBool("ShowForwardUpRightOnSelected");
		}
		set
		{
			SetBool("ShowForwardUpRightOnSelected", value);
		}
	}

	public static string ShowForwardUpRightOnSelected_GroupName => "Display";

	public static bool ShowCenterOfMasses
	{
		get
		{
			return GetBool("ShowCenterOfMasses");
		}
		set
		{
			SetBool("ShowCenterOfMasses", value);
		}
	}

	public static string ShowCenterOfMasses_GroupName => "Display";

	public static bool HideMover
	{
		get
		{
			return GetBool("hideMover");
		}
		set
		{
			SetBool("hideMover", value);
		}
	}

	public static string HideMover_GroupName => "Display";

	public static float ManualCameraSmoothness
	{
		get
		{
			return GetFloat("ManualCameraSmoothness");
		}
		set
		{
			SetFloat("ManualCameraSmoothness", value);
		}
	}

	public static string ManualCameraSmoothness_GroupName => "Camera";

	public static float WASDSmoothness
	{
		get
		{
			return GetFloat("WASDSmoothness");
		}
		set
		{
			SetFloat("WASDSmoothness", value);
		}
	}

	public static string WASDSmoothness_GroupName => "Camera";

	public static float WASDMovementSpeedup
	{
		get
		{
			return GetFloat("WASDMovementSpeedup");
		}
		set
		{
			SetFloat("WASDMovementSpeedup", value);
		}
	}

	public static string WASDMovementSpeedup_GroupName => "Camera";

	public static float WASDRotationSpeedup
	{
		get
		{
			return GetFloat("WASDRotationSpeedup");
		}
		set
		{
			SetFloat("WASDRotationSpeedup", value);
		}
	}

	public static string WASDRotationSpeedup_GroupName => "Camera";

	public static float MouseWheelZoomSpeedup
	{
		get
		{
			return GetFloat("mouse-wheel-zoom-speedup");
		}
		set
		{
			SetFloat("mouse-wheel-zoom-speedup", value);
		}
	}

	public static string MouseWheelZoomSpeedup_GroupName => "Camera";

	public static bool RelativeZoom
	{
		get
		{
			return GetBool("relative-zoom");
		}
		set
		{
			SetBool("relative-zoom", value);
		}
	}

	public static string RelativeZoom_GroupName => "Camera";

	public static bool TutorialDisableAutoCamera
	{
		get
		{
			return GetBool("TutorialDisableAutoCamera");
		}
		set
		{
			SetBool("TutorialDisableAutoCamera", value);
		}
	}

	public static string TutorialDisableAutoCamera_GroupName => "Camera";

	public static bool DisableCameraSnapping
	{
		get
		{
			return GetBool("DisableCameraSnapping");
		}
		set
		{
			SetBool("DisableCameraSnapping", value);
		}
	}

	public static string DisableCameraSnapping_GroupName => "Camera";

	public static bool DisableAutoFollow
	{
		get
		{
			return GetBool("DisableAutoFollow");
		}
		set
		{
			SetBool("DisableAutoFollow", value);
		}
	}

	public static string DisableAutoFollow_GroupName => "Camera";

	public static bool DisableGameCamera
	{
		get
		{
			return GetBool("DisableGameCamera");
		}
		set
		{
			SetBool("DisableGameCamera", value);
		}
	}

	public static string DisableGameCamera_GroupName => "Camera";

	public static bool DisableWASD
	{
		get
		{
			return GetBool("disableWASD");
		}
		set
		{
			SetBool("disableWASD", value);
		}
	}

	public static string DisableWASD_GroupName => "Camera";

	public static bool EnableVRGoggles
	{
		get
		{
			return GetBool("enableVRGoggles");
		}
		set
		{
			SetBool("enableVRGoggles", value);
		}
	}

	public static string EnableVRGoggles_GroupName => "Camera";

	public static float MouseWheelScrollSpeedup
	{
		get
		{
			return GetFloat("mouse-wheel-scroll-speedup");
		}
		set
		{
			SetFloat("mouse-wheel-scroll-speedup", value);
		}
	}

	public static string MouseWheelScrollSpeedup_GroupName => "Side Panel";

	public static bool ShowDevTiles
	{
		get
		{
			return GetBool("ShowDevTiles");
		}
		set
		{
			SetBool("ShowDevTiles", value);
		}
	}

	public static string ShowDevTiles_GroupName => "Side Panel";

	public static bool QuickKeyScroll
	{
		get
		{
			return GetBool("QuickKeyScroll");
		}
		set
		{
			SetBool("QuickKeyScroll", value);
		}
	}

	public static string QuickKeyScroll_GroupName => "Side Panel";

	public static bool DisableAutoScrollToScriptTile
	{
		get
		{
			return GetBool("DisableAutoScrollToScriptTile");
		}
		set
		{
			SetBool("DisableAutoScrollToScriptTile", value);
		}
	}

	public static string DisableAutoScrollToScriptTile_GroupName => "Side Panel";

	public static int PanelColumnCount
	{
		get
		{
			return GetInt("PanelColumnCount");
		}
		set
		{
			SetInt("PanelColumnCount", value);
		}
	}

	public static string PanelColumnCount_GroupName => "Side Panel";

	public static bool EnableTerrainSelection
	{
		get
		{
			return GetBool("EnableTerrainSelection");
		}
		set
		{
			SetBool("EnableTerrainSelection", value);
		}
	}

	public static string EnableTerrainSelection_GroupName => "Block Manipulation";

	public static bool ControlInvertsUpMode
	{
		get
		{
			return GetBool("ControlInvertsUpMode");
		}
		set
		{
			SetBool("ControlInvertsUpMode", value);
		}
	}

	public static string ControlInvertsUpMode_GroupName => "Block Manipulation";

	public static bool RaycastMoveSingletonBlocksWithoutSelection
	{
		get
		{
			return GetBool("RaycastMoveSingletonBlocksWithoutSelection");
		}
		set
		{
			SetBool("RaycastMoveSingletonBlocksWithoutSelection", value);
		}
	}

	public static string RaycastMoveSingletonBlocksWithoutSelection_GroupName => "Block Manipulation";

	public static bool RaycastMoveBlocksWithoutSelection
	{
		get
		{
			return GetBool("RaycastMoveBlocksWithoutSelection");
		}
		set
		{
			SetBool("RaycastMoveBlocksWithoutSelection", value);
		}
	}

	public static string RaycastMoveBlocksWithoutSelection_GroupName => "Block Manipulation";

	public static bool AxisLockMoveAndScaleEnabled
	{
		get
		{
			return GetBool("AxisLockMoveAndScaleEnabled");
		}
		set
		{
			SetBool("AxisLockMoveAndScaleEnabled", value);
		}
	}

	public static string AxisLockMoveAndScaleEnabled_GroupName => "Block Manipulation";

	public static bool EnableDevUtils
	{
		get
		{
			return GetBool("EnableDevUtils");
		}
		set
		{
			SetBool("EnableDevUtils", value);
		}
	}

	public static string EnableDevUtils_GroupName => "Block Manipulation";

	public static bool ShowTextureToInfo
	{
		get
		{
			return GetBool("ShowTextureToInfo");
		}
		set
		{
			SetBool("ShowTextureToInfo", value);
		}
	}

	public static string ShowTextureToInfo_GroupName => "Debug";

	public static bool SetRewardModelFromClipboard
	{
		get
		{
			return GetBool("SetRewardModelFromClipboard");
		}
		set
		{
			SetBool("SetRewardModelFromClipboard", value);
		}
	}

	public static string SetRewardModelFromClipboard_GroupName => "Debug";

	public static bool ExportRewardModelFromClipboard
	{
		get
		{
			return GetBool("ExportRewardModelFromClipboard");
		}
		set
		{
			SetBool("ExportRewardModelFromClipboard", value);
		}
	}

	public static string ExportRewardModelFromClipboard_GroupName => "Debug";

	public static bool DisplayBlockNames
	{
		get
		{
			return GetBoolFast("DisplayBlockNames", ref DisplayBlockNames_Set, ref DisplayBlockNames_Fast);
		}
		set
		{
			SetBoolFast("DisplayBlockNames", value, ref DisplayBlockNames_Set, ref DisplayBlockNames_Fast);
		}
	}

	public static string DisplayBlockNames_GroupName => "Debug";

	public static bool DisplayWheelBlockNames
	{
		get
		{
			return GetBoolFast("DisplayWheelBlockNames", ref DisplayWheelBlockNames_Set, ref DisplayWheelBlockNames_Fast);
		}
		set
		{
			SetBoolFast("DisplayWheelBlockNames", value, ref DisplayWheelBlockNames_Set, ref DisplayWheelBlockNames_Fast);
		}
	}

	public static string DisplayWheelBlockNames_GroupName => "Debug";

	public static bool ShowMisalignedBlocks
	{
		get
		{
			return GetBoolFast("ShowMisalignedBlocks", ref ShowMisalignedBlocks_Set, ref ShowMisalignedBlocks_Fast);
		}
		set
		{
			SetBoolFast("ShowMisalignedBlocks", value, ref ShowMisalignedBlocks_Set, ref ShowMisalignedBlocks_Fast);
		}
	}

	public static string ShowMisalignedBlocks_GroupName => "Debug";

	public static bool UseSimpleAutoPlayTrigger
	{
		get
		{
			return GetBool("UseSimpleAutoPlayTrigger");
		}
		set
		{
			SetBool("UseSimpleAutoPlayTrigger", value);
		}
	}

	public static string UseSimpleAutoPlayTrigger_GroupName => "Debug";

	public static bool OnScreenActionDebug
	{
		get
		{
			return GetBoolFast("OnScreenActionDebug", ref OnScreenActionDebug_Set, ref OnScreenActionDebug_Fast);
		}
		set
		{
			SetBoolFast("OnScreenActionDebug", value, ref OnScreenActionDebug_Set, ref OnScreenActionDebug_Fast);
		}
	}

	public static string OnScreenActionDebug_GroupName => "Debug";

	public static bool OnScreenMouseBlockInfo
	{
		get
		{
			return GetBool("OnScreenMouseBlockInfo");
		}
		set
		{
			SetBool("OnScreenMouseBlockInfo", value);
		}
	}

	public static string OnScreenMouseBlockInfo_GroupName => "Debug";

	public static bool DebugIconLoad
	{
		get
		{
			return GetBool("DebugIconLoad");
		}
		set
		{
			SetBool("DebugIconLoad", value);
		}
	}

	public static string DebugIconLoad_GroupName => "Debug";

	public static bool AllowAllStateTransitions
	{
		get
		{
			return GetBool("AllowAllStateTransitions");
		}
		set
		{
			SetBool("AllowAllStateTransitions", value);
		}
	}

	public static string AllowAllStateTransitions_GroupName => "Debug";

	public static bool InstantStateAnimationShift
	{
		get
		{
			return GetBool("InstantStateAnimationShift");
		}
		set
		{
			SetBool("InstantStateAnimationShift", value);
		}
	}

	public static string InstantStateAnimationShift_GroupName => "Debug";

	public static bool DebugGestures
	{
		get
		{
			return GetBool("DebugGestures");
		}
		set
		{
			SetBool("DebugGestures", value);
		}
	}

	public static string DebugGestures_GroupName => "Debug";

	public static string ScreenshotDirectory
	{
		get
		{
			return GetString("ScreenshotDirectory");
		}
		set
		{
			SetString("ScreenshotDirectory", value);
		}
	}

	public static string ScreenshotDirectory_GroupName => "Screenshot";

	public static float ScreenshotSizeMultiplier
	{
		get
		{
			return GetFloat("ScreenshotSizeMultiplier");
		}
		set
		{
			SetFloat("ScreenshotSizeMultiplier", value);
		}
	}

	public static string ScreenshotSizeMultiplier_GroupName => "Screenshot";

	public static bool AntialiasScreenshot
	{
		get
		{
			return GetBool("AntialiasScreenshot");
		}
		set
		{
			SetBool("AntialiasScreenshot", value);
		}
	}

	public static string AntialiasScreenshot_GroupName => "Screenshot";

	public static bool RemoveBackgroundInScreenshot
	{
		get
		{
			return GetBool("RemoveBackgroundInScreenshot");
		}
		set
		{
			SetBool("RemoveBackgroundInScreenshot", value);
		}
	}

	public static string RemoveBackgroundInScreenshot_GroupName => "Screenshot";

	public static bool SaveWorldsAsInIOS
	{
		get
		{
			return GetBool("SaveWorldsAsInIOS");
		}
		set
		{
			SetBool("SaveWorldsAsInIOS", value);
		}
	}

	public static bool AutoSaveEnabled
	{
		get
		{
			return GetBool("AutoSaveEnabled");
		}
		set
		{
			SetBool("AutoSavedEnabled", value);
		}
	}

	public static bool DisableEditorMouseInput
	{
		get
		{
			return GetBool("DisableEditorMouseInput");
		}
		set
		{
			SetBool("DisableEditorMouseInput", value);
		}
	}

	public static bool ShowIOSControlsInEditor
	{
		get
		{
			return GetBool("ShowIOSControlsInEditor");
		}
		set
		{
			SetBool("ShowIOSControlsInEditor", value);
		}
	}

	public static bool UseCompactGafWriteRenamings
	{
		get
		{
			return GetBool("UseCompactGafWriteRenamings");
		}
		set
		{
			SetBool("UseCompactGafWriteRenamings", value);
		}
	}

	public static int WorldBackupMaxCount
	{
		get
		{
			return PlayerPrefs.GetInt("WorldBackupMaxCount");
		}
		set
		{
			PlayerPrefs.SetInt("WorldBackupMaxCount", value);
		}
	}

	public static string WorldBackupMaxCount_GroupName => "World Backup";

	public static bool EnableUnityEditorScarcity
	{
		get
		{
			return GetBool("EnableUnityEditorScarcity");
		}
		set
		{
			SetBool("EnableUnityEditorScarcity", value);
		}
	}

	public static bool UseExampleInventory
	{
		get
		{
			return GetBool("UseExampleInventory");
		}
		set
		{
			SetBool("UseExampleInventory", value);
		}
	}

	public static bool EnableOpaqueParameterBackground
	{
		get
		{
			return GetBool("EnableOpaqueParameterBackground");
		}
		set
		{
			SetBool("EnableOpaqueParameterBackground", value);
		}
	}

	public static bool CreateErrorGafs
	{
		get
		{
			return GetBool("CreateErrorGafs");
		}
		set
		{
			SetBool("CreateErrorGafs", value);
		}
	}

	public static bool InterpolateRigidBodies
	{
		get
		{
			return GetBool("InterpolateRigidBodies");
		}
		set
		{
			SetBool("InterpolateRigidBodies", value);
		}
	}

	public static float TimeScale
	{
		get
		{
			return GetFloat("TimeScale");
		}
		set
		{
			SetFloat("TimeScale", value);
		}
	}

	public static bool EditorIsHD
	{
		get
		{
			return GetBool("EditorIsHD");
		}
		set
		{
			SetBool("EditorIsHD", value);
		}
	}

	public static bool MarketingScreenshots
	{
		get
		{
			return GetBool("MarketingScreenshots");
		}
		set
		{
			SetBool("MarketingScreenshots", value);
		}
	}

	public static bool Cowlorded
	{
		get
		{
			return GetBool("cowlorded");
		}
		set
		{
			SetBool("cowlorded", value);
		}
	}

	public static string EditorUser
	{
		get
		{
			return GetString("EditorUser");
		}
		set
		{
			SetString("EditorUser", value);
		}
	}

	public static bool ReloadCachedMaterialsEveryFrame
	{
		get
		{
			return GetBool("ReloadCachedMaterialsEveryFrame");
		}
		set
		{
			SetBool("ReloadCachedMaterialsEveryFrame", value);
		}
	}

	public static bool NoSnapSave
	{
		get
		{
			return GetBool("NoSnapSave");
		}
		set
		{
			SetBool("NoSnapSave", value);
		}
	}

	public static bool WriteExtendedRenamingsOnLoad
	{
		get
		{
			return GetBool("WriteExtendedRenamingsOnLoad");
		}
		set
		{
			SetBool("WriteExtendedRenamingsOnLoad", value);
		}
	}

	public static bool HighlightTouches => GetBool("highlightTouches");

	public static bool DisableShape
	{
		get
		{
			return GetBool("disableShape");
		}
		set
		{
			SetBool("disableShape", value);
		}
	}

	public static bool ShowScreenRecordButtonsInEditor
	{
		get
		{
			return GetBool("showScreenRecordButtonsInEditor");
		}
		set
		{
			SetBool("showScreenRecordButtonsInEditor", value);
		}
	}

	private static float GetFloat(string prefName)
	{
		return PlayerPrefs.GetFloat(prefName);
	}

	private static void SetFloat(string prefName, float value)
	{
		PlayerPrefs.SetFloat(prefName, value);
	}

	private static int GetInt(string prefName)
	{
		return PlayerPrefs.GetInt(prefName);
	}

	private static void SetInt(string prefName, int value)
	{
		PlayerPrefs.SetInt(prefName, value);
	}

	private static string GetString(string prefName)
	{
		return PlayerPrefs.GetString(prefName);
	}

	private static void SetString(string prefName, string value)
	{
		PlayerPrefs.SetString(prefName, value);
	}

	private static bool GetBool(string prefName)
	{
		return PlayerPrefs.GetInt(prefName) != 0;
	}

	private static void SetBool(string prefName, bool value)
	{
		PlayerPrefs.SetInt(prefName, value ? 1 : 0);
	}

	private static bool GetBoolFast(string prefName, ref bool set, ref bool fast)
	{
		if (!set)
		{
			fast = GetBool(prefName);
			set = true;
		}
		return fast;
	}

	private static void SetBoolFast(string prefName, bool value, ref bool set, ref bool fast)
	{
		SetBool(prefName, value);
		set = true;
		fast = value;
	}
}
