using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using Exdilin;
using SimpleJSON;
using UnityEngine;

public class WorldSession
{
	public static WorldSession current;

	public static BWWorldSessionType defaultSessionType;

	public static string worldIdClipboard = string.Empty;

	public static WorldSessionConfig jumpRestoreConfig;

	public string sessionTitle = string.Empty;

	public string sessionUserName = string.Empty;

	public string sessionDescription = string.Empty;

	public WorldSessionConfig config;

	public WorldInfoList availableTeleportWorlds;

	public BlockAnimatedCharacter profileWorldAnimatedBlockster;

	private WorldUILayout initialButtonLayout;

	private WorldUILayout statePlayButtonLayout;

	private WorldUILayout statePauseButtonLayout;

	private WorldUILayout stateCaptureButtonLayout;

	private WorldUILayout stateShareButtonLayout;

	private WorldUILayout captureSetupButtonLayout;

	private WorldUILayout leaderboardStartScreenLayout;

	private WorldUILayout leaderboardWinScreenLayout;

	private Action launchAction;

	private bool exitTriggered;

	private bool jumpToWorldTriggered;

	private float jumpToWorldCountdown;

	private WorldInfo jumpToWorldInfo;

	public bool isJumpSession;

	private bool blockInput;

	public bool worldLoadComplete;

	private float scheduledModelRefresh;

	private float scheduledAutoSave;

	private bool isLiked;

	private bool worldLoadInProgress;

	private bool inModelTutorial;

	private Texture2D previewModelImage;

	private State sessionUnpauseState;

	private string _deepLinkOnQuit = string.Empty;

	public const string PURCHASED_MODEL = "purchasedModel";

	private static WorldSessionPlatformDelegate _platformDelegate;

	private BlocksworldWebInterface WebGLInterface => BlocksworldWebInterface.Instance;

	public static WorldSessionPlatformDelegate platformDelegate
	{
		get
		{
			if (_platformDelegate == null)
			{
				_platformDelegate = new WorldSessionPlatformDelegate_Standalone();
			}
			return _platformDelegate;
		}
	}

	public BWWorldSessionType sessionType => config.sessionType;

	public string worldId => config.worldId;

	public string worldTitle => config.worldTitle;

	public bool worldIsPublished => config.worldIsPublished;

	public bool isWorldAuthorCurrentUser => config.worldAuthorId == config.currentUserId;

	public string worldSourceJsonStr
	{
		get
		{
			return config.worldSourceJsonStr;
		}
		set
		{
			config.worldSourceJsonStr = value;
		}
	}

	public string profileCharacterGender
	{
		get
		{
			return config.profileGender;
		}
		set
		{
			config.profileGender = value;
		}
	}

	public bool hasWinCondition
	{
		get
		{
			if (!config.worldHasWinCondition.HasValue)
			{
				config.worldHasWinCondition = Blocksworld.HasWinCondition();
			}
			return config.worldHasWinCondition.Value;
		}
		set
		{
			config.worldHasWinCondition = value;
		}
	}

	public List<Dependency> requiredMods
	{
		get
		{
			if (config.worldRequiredMods == null)
			{
				config.worldRequiredMods = Blocksworld.GetRequiredMods();
			}
			return config.worldRequiredMods;
		}
		set
		{
			config.worldRequiredMods = value;
		}
	}

	public string requiredModsJsonStr
	{
		get
		{
			List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
			foreach (Dependency requiredMod in requiredMods)
			{
				list.Add(new Dictionary<string, string>
				{
					{ "id", requiredMod.Id },
					{
						"version",
						requiredMod.MinimumVersion.ToString()
					}
				});
			}
			return JSONEncoder.Encode(list);
		}
	}

	public bool IsLiked => isLiked;

	public static bool AllowInput
	{
		get
		{
			if (current != null)
			{
				return !current.blockInput;
			}
			return false;
		}
	}

	public static void Start(WorldSessionConfig worldSessionConfig)
	{
		if (worldSessionConfig.worldRequiredMods != null)
		{
			foreach (Dependency worldRequiredMod in worldSessionConfig.worldRequiredMods)
			{
				Debug.Log(worldRequiredMod.Id);
			}
		}
		WorldEnvironmentManager.ResetConfiguration();
		current = new WorldSession();
		if (!current.ApplyConfig(worldSessionConfig))
		{
			BWLog.Error("Unable to load world source!  Quitting back to the main menu.");
			platformDelegate.WorldWillQuit("corrupt_world_source");
			ViewportWatchdog.StopWatching();
			Blocksworld.CleanupAndQuitToMenu();
			return;
		}
		current.initialButtonLayout.Apply();
		if (Blocksworld.worldSky == null)
		{
			List<Tile> list = new List<Tile>();
			list.Add(new Tile(new GAF("Block.Create", "Sky")));
			Block.NewBlock(new List<List<Tile>> { list });
		}
		current.LaunchWorld();
		BWLog.Info("Starting world session type " + current.config.sessionType.ToString() + ", worldTitle: " + current.config.worldTitle);
	}

	public static void StartForIOS(string worldSessionConfigJsonStr)
	{
		Blocksworld.bw.StartCoroutine(CoroutineStartForIOS(worldSessionConfigJsonStr));
	}

	private static IEnumerator CoroutineStartForIOS(string worldSessionConfigJsonStr)
	{
		_platformDelegate.WorldDidStartLoading();
		yield return null;
		WorldSessionConfig worldSessionConfig = WorldSessionConfig.BuildFromConfigJsonStr(worldSessionConfigJsonStr);
		Start(worldSessionConfig);
	}

	public static void JumpToWorld(WorldInfo worldInfo)
	{
		if (!worldInfo.HasWorldSource())
		{
			BWLog.Error("Trying to jump to world with no source loaded");
			return;
		}
		Blocksworld.worldSessionHadHypderjumpUse = true;
		Sound.PlayOneShotSound("Teleport_Arrival");
		bool flag = isNormalBuildAndPlaySession();
		bool jumpedFromBuildMode = current.config.jumpedFromBuildMode;
		WorldSessionConfig worldSessionConfig = CreateRestoreConfig();
		Blocksworld.Cleanup(quitToMenu: false);
		WorldSessionConfig worldSessionConfig2 = new WorldSessionConfig();
		worldSessionConfig2.sessionType = BWWorldSessionType.BWWorldSessionCommunityPlay;
		worldSessionConfig2.worldId = worldInfo.id;
		worldSessionConfig2.worldTitle = worldInfo.title;
		worldSessionConfig2.worldSourceJsonStr = worldInfo.WorldSource();
		worldSessionConfig2.likedByCurrentUser = false;
		worldSessionConfig2.hasBlocksInventory = false;
		worldSessionConfig2.hasModelCollectionInfo = false;
		worldSessionConfig2.currentUserAvatarSource = worldSessionConfig.currentUserAvatarSource;
		if (flag || jumpedFromBuildMode)
		{
			worldSessionConfig2.jumpedFromBuildMode = true;
		}
		Start(worldSessionConfig2);
		if (jumpRestoreConfig == null)
		{
			jumpRestoreConfig = worldSessionConfig;
		}
		current.isJumpSession = true;
	}

	private static WorldSessionConfig CreateRestoreConfig()
	{
		if (isNormalBuildAndPlaySession())
		{
			current.worldSourceJsonStr = Blocksworld.bw.Serialize();
			current.hasWinCondition = Blocksworld.HasWinCondition();
		}
		WorldSessionConfig original = current.config;
		WorldSessionConfig worldSessionConfig = WorldSessionConfig.Clone(original);
		worldSessionConfig.hasBlocksInventory = false;
		worldSessionConfig.hasModelCollectionInfo = false;
		worldSessionConfig.blocksInventory = null;
		worldSessionConfig.modelCollectionJson = null;
		return worldSessionConfig;
	}

	public static void RestoreJumpConfig()
	{
		if (jumpRestoreConfig != null)
		{
			WorldSessionConfig worldSessionConfig = WorldSessionConfig.Clone(jumpRestoreConfig);
			jumpRestoreConfig = null;
			Blocksworld.Cleanup(quitToMenu: false);
			Start(worldSessionConfig);
		}
		else
		{
			BWLog.Error("build mode restore config is null");
		}
	}

	public static void StartForStandaloneWithRemoteWorldId(string id, bool buildMode, string currentUserAvatarSource)
	{
		if (string.IsNullOrEmpty(id))
		{
			BWLog.Error("Invalid worldId.");
			return;
		}
		BWRemoteWorldsDataManager.Instance.LoadWorld(id, delegate(BWWorld world)
		{
			StartForStandaloneWithRemoteWorld(world, buildMode, currentUserAvatarSource);
		});
	}

	public static void StartForStandaloneWithRemoteWorld(BWWorld world, bool buildMode, string currentAvatarSource)
	{
		if (world != null)
		{
			Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
			WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
			worldSessionConfig.sessionType = ((!buildMode) ? BWWorldSessionType.BWWorldSessionCommunityPlay : BWWorldSessionType.BWWorldSessionBuild);
			worldSessionConfig.worldId = world.worldID;
			worldSessionConfig.worldTitle = world.title;
			worldSessionConfig.worldSourceJsonStr = world.source;
			worldSessionConfig.worldRequiredMods = world.requiredMods;
			worldSessionConfig.likedByCurrentUser = false;
			worldSessionConfig.hasBlocksInventory = buildMode;
			worldSessionConfig.hasModelCollectionInfo = false;
			worldSessionConfig.blocksInventory = BWUser.currentUser.blocksInventory;
			worldSessionConfig.currentUserAvatarSource = currentAvatarSource;
			string path = $"/api/v1/worlds/{world.worldID}/plays";
			BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", path);
			bWAPIRequestBase.Send();
			Start(worldSessionConfig);
		}
	}

	private static IEnumerator CoroutineStartForStandaloneWithRemoteWorldId(string id, bool buildMode, string currentUserAvatarSource)
	{
		if (string.IsNullOrEmpty(id))
		{
			BWLog.Error("Invalid worldId.");
			yield break;
		}
		string path = $"/api/v1/worlds/{id}";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
		};
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
			WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
			JObject jObject = responseJson["world"];
			string stringValue = jObject["title"].StringValue;
			string text = ((!jObject.ContainsKey("source_json_str")) ? jObject["source"].StringValue : jObject["source_json_str"].StringValue);
			worldSessionConfig.sessionType = ((!buildMode) ? BWWorldSessionType.BWWorldSessionCommunityPlay : BWWorldSessionType.BWWorldSessionBuild);
			worldSessionConfig.worldId = id;
			worldSessionConfig.worldTitle = stringValue;
			worldSessionConfig.worldSourceJsonStr = text;
			worldSessionConfig.likedByCurrentUser = false;
			worldSessionConfig.hasBlocksInventory = buildMode;
			worldSessionConfig.hasModelCollectionInfo = false;
			worldSessionConfig.blocksInventory = BWUser.currentUser.blocksInventory;
			worldSessionConfig.currentUserAvatarSource = currentUserAvatarSource;
			string path2 = $"/api/v1/worlds/{id}/plays";
			BWAPIRequestBase bWAPIRequestBase2 = BW.API.CreateRequest("POST", path2);
			bWAPIRequestBase2.Send();
			Start(worldSessionConfig);
		};
		bWAPIRequestBase.Send();
	}

	public static void StartForStandaloneInBuildMode(string worldID, string title, string source, string currentUserAvatarSource, bool screenshotTakenManually)
	{
		Start(new WorldSessionConfig
		{
			sessionType = BWWorldSessionType.BWWorldSessionBuild,
			worldId = worldID,
			worldTitle = title,
			worldSourceJsonStr = source,
			hasBlocksInventory = true,
			hasModelCollectionInfo = false,
			loadStandaloneModelCollection = true,
			blocksInventory = BWUser.currentUser.blocksInventory,
			currentUserAvatarSource = currentUserAvatarSource,
			worldScreenshotTakenManually = screenshotTakenManually
		});
	}

	public static void StartForStandaloneWorldScreenshotSession(string worldID, string title, string source, string currentUserAvatarSource)
	{
		Start(new WorldSessionConfig
		{
			sessionType = BWWorldSessionType.BWWorldSessionScreenshot,
			worldId = worldID,
			worldTitle = title,
			worldSourceJsonStr = source,
			hasBlocksInventory = true,
			hasModelCollectionInfo = false,
			loadStandaloneModelCollection = true,
			blocksInventory = BWUser.currentUser.blocksInventory,
			currentUserAvatarSource = currentUserAvatarSource,
			worldScreenshotTakenManually = true
		});
	}

	public static void StartForStandaloneWithProfileWorld(string profileGender, string worldID, string title, string source, string avatarSource)
	{
		Start(new WorldSessionConfig
		{
			sessionType = BWWorldSessionType.BWWorldSessionProfileBuild,
			profileGender = profileGender,
			worldId = worldID,
			worldTitle = title,
			worldSourceJsonStr = source,
			hasBlocksInventory = true,
			hasModelCollectionInfo = false,
			blocksInventory = BWUser.currentUser.blocksInventory,
			currentUserAvatarSource = avatarSource
		});
	}

	public static void StartForStandaloneWithUserModel(BWUserModel model)
	{
		string sourceJsonStr = model.sourceJsonStr;
		string previewTerrain = model.previewTerrain;
		WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
		worldSessionConfig.sessionType = BWWorldSessionType.BWWorldSessionUserModelPreview;
		HashSet<string> hashSet = new HashSet<string> { "land", "sky", "water" };
		string text = ((!hashSet.Contains(previewTerrain)) ? "land" : previewTerrain);
		string path = "ModelPreviewWorlds/model_preview_world_source_" + text;
		worldSessionConfig.worldSourceJsonStr = Resources.Load<TextAsset>(path).text;
		worldSessionConfig.previewModelSource = sourceJsonStr;
		worldSessionConfig.previewModelPositionX = 0f;
		worldSessionConfig.previewModelPositionY = ((!(previewTerrain == "sky")) ? 0f : 30f);
		worldSessionConfig.previewModelPositionZ = 0f;
		worldSessionConfig.hasBlocksInventory = false;
		worldSessionConfig.hasModelCollectionInfo = false;
		Start(worldSessionConfig);
	}

	public static void StartForStandaloneWithCommunityModel(string remoteModelID)
	{
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", $"/api/v1/u2u_models/{remoteModelID}");
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("u2u_model"))
			{
				BWU2UModel bWU2UModel = new BWU2UModel(responseJson["u2u_model"]);
				string sourceJsonStr = bWU2UModel.sourceJsonStr;
				string previewTerrain = bWU2UModel.previewTerrain;
				WorldSessionConfig worldSessionConfig = new WorldSessionConfig
				{
					sessionType = BWWorldSessionType.BWWorldSessionCommunityModelPreview
				};
				HashSet<string> hashSet = new HashSet<string> { "land", "sky", "water" };
				string text = ((!hashSet.Contains(previewTerrain)) ? "land" : previewTerrain);
				string path = "ModelPreviewWorlds/model_preview_world_source_" + text;
				worldSessionConfig.worldSourceJsonStr = Resources.Load<TextAsset>(path).text;
				worldSessionConfig.previewModelSource = sourceJsonStr;
				worldSessionConfig.previewModelPositionX = 0f;
				worldSessionConfig.previewModelPositionY = ((!(previewTerrain == "sky")) ? 0f : 30f);
				worldSessionConfig.previewModelPositionZ = 0f;
				worldSessionConfig.hasBlocksInventory = false;
				worldSessionConfig.hasModelCollectionInfo = false;
				worldSessionConfig.currentUserId = BWUser.currentUser.userID;
				worldSessionConfig.currentUserCoins = BWUser.currentUser.coins;
				worldSessionConfig.previewModelAuthorUserId = bWU2UModel.authorId;
				worldSessionConfig.previewModelAuthorUsername = bWU2UModel.authorUsername;
				worldSessionConfig.previewModelLocked = bWU2UModel.sourceLocked;
				worldSessionConfig.previewModelSellingPrice = bWU2UModel.coinsPrice;
				worldSessionConfig.previewModelPurchased = BWU2UModelDataManager.Instance.HasPurchasedModel(remoteModelID);
				Start(worldSessionConfig);
			}
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
		};
		bWAPIRequestBase.Send();
	}

	public static void StartForWebGLWithWorldSourceJsonStr(string worldSourceJsonStr)
	{
		if (worldSourceJsonStr == null)
		{
			Debug.LogError("Error: attempting to load world with no source");
			return;
		}
		if (current != null)
		{
			Debug.LogError("Error: attempting to load world before previous world is unloaded");
			return;
		}
		Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
		WorldSessionConfig worldSessionConfig = WorldSessionConfig.BuildForWebGL(worldSourceJsonStr);
		BlocksworldWebInterface.Instance.OnBeforeWorldLoad();
		Start(worldSessionConfig);
	}

	public static void StartForWebGLWithModelSourceJsonStr(string backgroundType, string modelSourceJsonStr)
	{
		WorldSessionConfig worldSessionConfig = WorldSessionConfig.BuildForWebGLWithModelSource(backgroundType, modelSourceJsonStr);
		BlocksworldWebInterface.Instance.OnBeforeWorldLoad();
		Start(worldSessionConfig);
	}

	public static void StartForWebGLBuildModeDemo()
	{
		Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
		WorldSessionConfig worldSessionConfig = WorldSessionConfig.BuildForWebGLBuildModeDemo();
		Start(worldSessionConfig);
	}

	public static void UpdateModelCollection(string modelCollectionJsonStr, bool includeOfflineModels)
	{
		if (current == null)
		{
			BWLog.Error("Trying to update model collection but no active session");
		}
		else
		{
			current.config.UpdateModelCollectionFromJson(modelCollectionJsonStr, includeOfflineModels);
		}
	}

	public static void RefreshModelIcons()
	{
		if (current != null)
		{
			current.scheduledModelRefresh = Time.time + 2f;
		}
	}

	public static void BuildingSetPurchaseCallback(string status)
	{
		if (current == null)
		{
			BWLog.Error("Building set purchase callback with no active session");
		}
		else if (isPuzzleBuildSession() || isPuzzlePlaySession())
		{
			if (string.Equals(status, "error"))
			{
				Quit();
			}
			else
			{
				current.OnPurchaseBuildingSetFromPuzzle();
			}
		}
	}

	public static void ModelPurchaseCallback(string status)
	{
		if (current == null)
		{
			BWLog.Error("Model purchase callback with no active session");
		}
		else if (!string.Equals(status, "error"))
		{
			Blocksworld.VisualizeRewardModel("purchasedModel", current.config.previewModelSource);
		}
	}

	public bool ApplyConfig(WorldSessionConfig worldSessionConfig)
	{
		config = worldSessionConfig;
		Blocksworld.bw.ClearWorldState();
		bool flag = Blocksworld.bw.LoadFromString(config.worldSourceJsonStr);
		if (!flag)
		{
			if (worldSessionConfig.sessionType != BWWorldSessionType.BWWorldSessionProfileBuild)
			{
				return false;
			}
			config.worldSourceJsonStr = Resources.Load<TextAsset>(Blocksworld.DefaultProfileWorldAssetPath()).text;
			flag = Blocksworld.bw.LoadFromString(config.worldSourceJsonStr);
		}
		if (!flag)
		{
			BWLog.Error("Failed to load world source");
		}
		ResetAutoSaveCounter();
		Blocksworld.bw.ProcessLoadedWorld();
		if (config.hasBlocksInventory)
		{
			Blocksworld.UI.SidePanel.Show();
			config.ApplyBuildPanelInfo();
		}
		if (config.loadStandaloneModelCollection)
		{
			Blocksworld.modelCollection.Clear();
			BWUserModelsDataManager.Instance.AddListener(UpdateStandaloneModelCollection);
			Blocksworld.modelCollection.LoadModelCollectionForStandalone();
			Blocksworld.SetupBuildPanel();
		}
		else if (config.hasModelCollectionInfo)
		{
			config.ApplyModelCollectionInfo();
		}
		switch (config.sessionType)
		{
		case BWWorldSessionType.BWWorldSessionBuild:
			OnLoadedLocalWorld();
			break;
		case BWWorldSessionType.BWWorldSessionScreenshot:
			OnLoadedScreenshotSession();
			break;
		case BWWorldSessionType.BWWorldSessionProfileBuild:
			OnLoadedPlayerProfileWorld();
			break;
		case BWWorldSessionType.BWWorldSessionCommunityPlay:
			OnLoadedRemoteWorld();
			break;
		case BWWorldSessionType.BWWorldSessionPuzzleBuild:
			OnLoadedPuzzleBuildMode();
			break;
		case BWWorldSessionType.BWWorldSessionPuzzlePlay:
			OnLoadedPuzzlePlayMode();
			break;
		case BWWorldSessionType.BWWorldSessionUserModelPreview:
			OnLoadedUserModelPreview();
			break;
		case BWWorldSessionType.BWWorldSessionBuildingSetModelPreview:
			OnLoadedBuildingSetModelPreview();
			break;
		case BWWorldSessionType.BWWorldSessionCommunityModelPreview:
			OnLoadedCommunityModelPreview();
			break;
		}
		return true;
	}

	public void UpdateStandaloneModelCollection()
	{
		Blocksworld.modelCollection.UpdateLocalModelCollectionForStandalone();
	}

	public bool WorldSourceEquals(string newWorldSourceJsonStr)
	{
		if (!string.IsNullOrEmpty(worldSourceJsonStr))
		{
			return string.Equals(newWorldSourceJsonStr, worldSourceJsonStr);
		}
		return false;
	}

	public static void NotifyFileTooNew()
	{
		platformDelegate.NotifyWorldTooNew();
	}

	public static bool SessionTypeIsPuzzle(BWWorldSessionType sessionType)
	{
		if (sessionType != BWWorldSessionType.BWWorldSessionPuzzleBuild)
		{
			return sessionType == BWWorldSessionType.BWWorldSessionPuzzlePlay;
		}
		return true;
	}

	public static bool isPuzzleBuildSession()
	{
		if (current != null)
		{
			return current.config.sessionType == BWWorldSessionType.BWWorldSessionPuzzleBuild;
		}
		return false;
	}

	public static bool isConstructionChallenge()
	{
		if (isPuzzleBuildSession() || isCommunitySession())
		{
			return Scarcity.puzzleInventory != null;
		}
		return false;
	}

	public static bool isPuzzlePlaySession()
	{
		if (current != null)
		{
			return current.config.sessionType == BWWorldSessionType.BWWorldSessionPuzzlePlay;
		}
		return false;
	}

	public static bool isProfileBuildSession()
	{
		if (current != null)
		{
			return current.config.sessionType == BWWorldSessionType.BWWorldSessionProfileBuild;
		}
		return false;
	}

	public static bool isNormalBuildAndPlaySession()
	{
		if (current != null)
		{
			return current.config.sessionType == BWWorldSessionType.BWWorldSessionBuild;
		}
		return false;
	}

	public static bool isWorldScreenshotSession()
	{
		if (current != null)
		{
			return current.config.sessionType == BWWorldSessionType.BWWorldSessionScreenshot;
		}
		return false;
	}

	public static bool isUserModelPreviewSession()
	{
		if (current != null)
		{
			return current.config.sessionType == BWWorldSessionType.BWWorldSessionUserModelPreview;
		}
		return false;
	}

	public static bool isBuildingSetModelPreviewSession()
	{
		if (current != null)
		{
			return current.config.sessionType == BWWorldSessionType.BWWorldSessionBuildingSetModelPreview;
		}
		return false;
	}

	public static bool isCommunitySession()
	{
		if (current != null)
		{
			return current.config.sessionType == BWWorldSessionType.BWWorldSessionCommunityPlay;
		}
		return false;
	}

	public static bool isCommunityModelPreviewSession()
	{
		if (current != null)
		{
			return current.config.sessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview;
		}
		return false;
	}

	public static bool canShowLeaderboard()
	{
		if (current != null && current.hasWinCondition)
		{
			return isCommunitySession();
		}
		return false;
	}

	private void LaunchWorld()
	{
		if (!config.hasModelCollectionInfo)
		{
			CompleteWorldLoad();
		}
		else if (!Blocksworld.modelCollection.modelLoadInProgress)
		{
			CompleteWorldLoad();
		}
		else
		{
			BWLog.Info("waiting for models to load");
		}
	}

	public void OnModelLoadComplete()
	{
		CompleteWorldLoad();
	}

	private void CompleteWorldLoad()
	{
		if (!worldLoadComplete)
		{
			worldLoadComplete = true;
			Blocksworld.bw.StartCoroutine(CoroutineCompleteLoad());
		}
	}

	private IEnumerator CoroutineCompleteLoad()
	{
		yield return null;
		if (launchAction != null)
		{
			launchAction();
		}
		launchAction = null;
		MappedInput.SetMode((Blocksworld.CurrentState != State.Play) ? MappableInputMode.Build : MappableInputMode.Play);
		Blocksworld.UI.Tapedeck.RefreshRecordButtonState();
		yield return null;
		if (!isJumpSession)
		{
			platformDelegate.WorldDidFinishLoading();
		}
	}

	private void OnLoadedLocalWorld()
	{
		initialButtonLayout = new WorldUILayout(new WorldUILayoutParameters(TILE_BUTTON.PLAY, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT)
		{
			includeUndoRedo = true
		});
		statePlayButtonLayout = new WorldUILayout(new WorldUILayoutParameters(TILE_BUTTON.TOOLS, TILE_BUTTON.CAPTURE, TILE_BUTTON.RESTART, TILE_BUTTON.EXIT)
		{
			includeVRCameraToggle = config.hasVRCameraButtonInPlayMode
		});
		leaderboardStartScreenLayout = new WorldUILayout(TILE_BUTTON.EXIT);
		leaderboardWinScreenLayout = new WorldUILayout(TILE_BUTTON.RESTART, TILE_BUTTON.EXIT);
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.EnableWorldSave(enabled: true);
		Blocksworld.EnableBuildMode();
		Blocksworld.SetBuildPanelRightSided();
		sessionTitle = config.worldTitle;
		sessionUserName = config.worldAuthorUsername;
		sessionDescription = config.worldDescription;
		launchAction = delegate
		{
			Blocksworld.SetBlocksworldState(State.Build);
			Blocksworld.UI.TabBar.SwitchToTab(TabBarTabId.Blocks);
			Scarcity.UpdateInventory();
			Blocksworld.UpdateTiles();
		};
	}

	private void OnLoadedRemoteWorld()
	{
		if (Scarcity.puzzleInventory != null)
		{
			OnLoadedConstructionChallenge();
			return;
		}
		initialButtonLayout = new WorldUILayout(new WorldUILayoutParameters
		{
			includeLikeUnlike = false
		});
		WorldUILayoutParameters worldUILayoutParameters = new WorldUILayoutParameters();
		if (config.jumpedFromBuildMode)
		{
			worldUILayoutParameters.mainButtons = new TILE_BUTTON[4]
			{
				TILE_BUTTON.TOOLS,
				TILE_BUTTON.CAPTURE,
				TILE_BUTTON.RESTART,
				TILE_BUTTON.EXIT
			};
			worldUILayoutParameters.includeVRCameraToggle = config.hasVRCameraButtonInPlayMode;
		}
		else
		{
			worldUILayoutParameters.mainButtons = new TILE_BUTTON[3]
			{
				TILE_BUTTON.RESTART,
				TILE_BUTTON.CAPTURE,
				TILE_BUTTON.EXIT
			};
			worldUILayoutParameters.includeLikeUnlike = false;
			worldUILayoutParameters.includeVRCameraToggle = config.hasVRCameraButtonInPlayMode;
		}
		statePlayButtonLayout = new WorldUILayout(worldUILayoutParameters);
		leaderboardStartScreenLayout = new WorldUILayout(TILE_BUTTON.EXIT);
		leaderboardWinScreenLayout = new WorldUILayout(TILE_BUTTON.RESTART, TILE_BUTTON.EXIT);
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		bool jumpedFromBuildMode = config.jumpedFromBuildMode;
		Blocksworld.EnableWorldSave(jumpedFromBuildMode);
		Blocksworld.DisableBuildMode();
		isLiked = config.likedByCurrentUser;
		sessionTitle = config.worldTitle;
		sessionUserName = config.worldAuthorUsername;
		sessionDescription = config.worldDescription;
		launchAction = delegate
		{
			Blocksworld.bw.Play();
		};
	}

	private void OnLoadedPlayerProfileWorld()
	{
		initialButtonLayout = new WorldUILayout(new WorldUILayoutParameters(TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT, TILE_BUTTON.PROFILE_SELECT)
		{
			includeUndoRedo = true,
			includeRecord = false
		});
		stateCaptureButtonLayout = WorldUILayout.WorldUILayoutHidden;
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.EnableWorldSave(enabled: true);
		Blocksworld.EnableBuildMode();
		Blocksworld.SetBuildPanelRightSided();
		sessionTitle = config.worldTitle;
		sessionUserName = config.worldAuthorUsername;
		sessionDescription = config.worldDescription;
		launchAction = delegate
		{
			Blocksworld.SetBlocksworldState(State.Build);
			Blocksworld.UI.SidePanel.Show();
			Blocksworld.UpdateTiles();
			Block profileCharacterBlock = ProfileBlocksterUtils.GetProfileCharacterBlock();
			Blocksworld.UI.ShowProfileSelectionScreen();
			if (!config.isNewProfile)
			{
				ProfileType profileCharacterType = ProfileBlocksterUtils.GetProfileCharacterType(profileCharacterBlock);
				Blocksworld.UI.ProfileSelection.ScrollToType(profileCharacterType);
			}
			if (profileCharacterBlock is BlockAnimatedCharacter)
			{
				profileWorldAnimatedBlockster = profileCharacterBlock as BlockAnimatedCharacter;
			}
		};
	}

	private void OnLoadedPuzzleBuildMode()
	{
		if (Scarcity.puzzleInventory != null)
		{
			OnLoadedConstructionChallenge();
			return;
		}
		Blocksworld.clipboard.autoPaintMode = false;
		Blocksworld.clipboard.autoTextureMode = false;
		WorldUILayoutParameters worldUILayoutParameters;
		if (config.hideExitButton)
		{
			initialButtonLayout = new WorldUILayout(TILE_BUTTON.PLAY);
			worldUILayoutParameters = new WorldUILayoutParameters(TILE_BUTTON.RESTART);
		}
		else
		{
			initialButtonLayout = new WorldUILayout(TILE_BUTTON.PLAY, TILE_BUTTON.EXIT);
			worldUILayoutParameters = new WorldUILayoutParameters(TILE_BUTTON.RESTART, TILE_BUTTON.EXIT);
		}
		worldUILayoutParameters.includeVRCameraToggle = config.hasVRCameraButtonInPlayMode;
		statePlayButtonLayout = new WorldUILayout(worldUILayoutParameters);
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.EnableWorldSave(enabled: true);
		Blocksworld.EnableBuildMode();
		Blocksworld.SetBuildPanelRightSided();
		config.LoadPuzzleGAFsAndRewards();
		launchAction = delegate
		{
			Blocksworld.UI.Tapedeck.Ghost(ghost: true);
			Blocksworld.SetBlocksworldState(State.Build);
			Blocksworld.UI.SidePanel.Show();
			Blocksworld.UpdateTiles();
			Tutorial.Start();
		};
		sessionTitle = config.worldTitle;
		sessionUserName = "Blocksworld";
		sessionDescription = config.worldDescription;
	}

	private void OnLoadedConstructionChallenge()
	{
		initialButtonLayout = new WorldUILayout(TILE_BUTTON.PLAY, TILE_BUTTON.EXIT);
		statePlayButtonLayout = new WorldUILayout(TILE_BUTTON.TOOLS, TILE_BUTTON.EXIT);
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.EnableWorldSave(enabled: false);
		Blocksworld.EnableBuildMode();
		Blocksworld.SetBuildPanelRightSided();
		config.LoadPuzzleGAFsAndRewards();
		launchAction = delegate
		{
			Blocksworld.SetBlocksworldState(State.Build);
			Blocksworld.UI.SidePanel.Show();
			Blocksworld.UpdateTiles();
			Tutorial.Start();
		};
		sessionTitle = config.worldTitle;
		sessionUserName = "Blocksworld";
		sessionDescription = config.worldDescription;
	}

	private void OnLoadedPuzzlePlayMode()
	{
		initialButtonLayout = new WorldUILayout(new WorldUILayoutParameters(TILE_BUTTON.RESTART, TILE_BUTTON.EXIT)
		{
			includeVRCameraToggle = config.hasVRCameraButtonInPlayMode
		});
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.EnableWorldSave(enabled: false);
		Blocksworld.DisableBuildMode();
		config.LoadPuzzleGAFsAndRewards();
		launchAction = delegate
		{
			Tutorial.Start(skipBuild: true);
			Blocksworld.bw.Play();
		};
		sessionTitle = config.worldTitle;
		sessionUserName = "Blocksworld";
		sessionDescription = config.worldDescription;
	}

	private void OnLoadedUserModelPreview()
	{
		WorldUILayoutParameters worldUILayoutParameters = new WorldUILayoutParameters(TILE_BUTTON.PAUSE, TILE_BUTTON.RESTART, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT);
		worldUILayoutParameters.includeTitleBar = true;
		worldUILayoutParameters.includeVRCameraToggle = config.hasVRCameraButtonInPlayMode;
		worldUILayoutParameters.includeBuildModelButton = true;
		worldUILayoutParameters.titleBarText = config.previewModelTitle;
		worldUILayoutParameters.titleBarSubtitle = config.GetFormattedPreviewModelAuthorStr();
		worldUILayoutParameters.titleBarHasCoinBalance = true;
		worldUILayoutParameters.titleBarCoinBalance = config.currentUserCoins;
		initialButtonLayout = new WorldUILayout(worldUILayoutParameters);
		statePauseButtonLayout = new WorldUILayout(new WorldUILayoutParameters(worldUILayoutParameters)
		{
			mainButtons = new TILE_BUTTON[4]
			{
				TILE_BUTTON.PLAY,
				TILE_BUTTON.RESTART,
				TILE_BUTTON.CAPTURE,
				TILE_BUTTON.EXIT
			}
		});
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.DisableBuildMode();
		Blocksworld.EnableWorldSave(enabled: false);
		launchAction = delegate
		{
			InsertPreviewModel();
			if (config.previewModel != null)
			{
				Blocksworld.bw.Play();
			}
			else
			{
				platformDelegate.WorldWillQuit("corrupt_model_source");
				Blocksworld.CleanupAndQuitToMenu();
			}
		};
		sessionTitle = config.previewModelTitle;
		sessionUserName = config.previewModelAuthorUsername;
		sessionDescription = config.worldDescription;
	}

	private void OnLoadedBuildingSetModelPreview()
	{
		WorldUILayoutParameters worldUILayoutParameters = new WorldUILayoutParameters(TILE_BUTTON.PAUSE, TILE_BUTTON.RESTART, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT);
		worldUILayoutParameters.includeTitleBar = true;
		worldUILayoutParameters.includeVRCameraToggle = config.hasVRCameraButtonInPlayMode;
		worldUILayoutParameters.includePurchasedBanner = config.previewModelPurchased;
		worldUILayoutParameters.titleBarText = config.previewModelTitle;
		worldUILayoutParameters.titleBarSubtitle = config.GetFormattedPreviewModelAuthorStr();
		worldUILayoutParameters.titleBarHasCoinBalance = true;
		worldUILayoutParameters.titleBarCoinBalance = config.currentUserCoins;
		initialButtonLayout = new WorldUILayout(worldUILayoutParameters);
		statePauseButtonLayout = new WorldUILayout(new WorldUILayoutParameters(worldUILayoutParameters)
		{
			mainButtons = new TILE_BUTTON[4]
			{
				TILE_BUTTON.PLAY,
				TILE_BUTTON.RESTART,
				TILE_BUTTON.CAPTURE,
				TILE_BUTTON.EXIT
			}
		});
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.DisableBuildMode();
		Blocksworld.EnableWorldSave(enabled: false);
		launchAction = delegate
		{
			InsertPreviewModel();
			if (config.previewModel != null)
			{
				Blocksworld.bw.Play();
			}
			else
			{
				platformDelegate.WorldWillQuit("corrupt_model_source");
				Blocksworld.CleanupAndQuitToMenu();
			}
		};
		sessionTitle = config.previewModelTitle;
		sessionUserName = config.previewModelAuthorUsername;
		sessionDescription = config.worldDescription;
	}

	private void OnLoadedCommunityModelPreview()
	{
		WorldUILayoutParameters worldUILayoutParameters = new WorldUILayoutParameters(TILE_BUTTON.PAUSE, TILE_BUTTON.RESTART, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT);
		worldUILayoutParameters.includeTitleBar = true;
		worldUILayoutParameters.includeVRCameraToggle = config.hasVRCameraButtonInPlayMode;
		worldUILayoutParameters.includePurchasedBanner = config.previewModelPurchased;
		worldUILayoutParameters.includeBuildModelButton = config.previewModelPurchased && !config.previewModelLocked;
		worldUILayoutParameters.buyModelPrice = config.previewModelSellingPrice;
		worldUILayoutParameters.titleBarText = config.previewModelTitle;
		worldUILayoutParameters.titleBarSubtitle = config.GetFormattedPreviewModelAuthorStr();
		worldUILayoutParameters.titleBarHasCoinBalance = true;
		worldUILayoutParameters.titleBarCoinBalance = config.currentUserCoins;
		initialButtonLayout = new WorldUILayout(worldUILayoutParameters);
		statePauseButtonLayout = new WorldUILayout(new WorldUILayoutParameters(worldUILayoutParameters)
		{
			mainButtons = new TILE_BUTTON[4]
			{
				TILE_BUTTON.PLAY,
				TILE_BUTTON.RESTART,
				TILE_BUTTON.CAPTURE,
				TILE_BUTTON.EXIT
			}
		});
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.DisableBuildMode();
		Blocksworld.EnableWorldSave(enabled: false);
		previewModelImage = platformDelegate.GetCurrentlyLoadedPreviewModelImage();
		if (previewModelImage == null)
		{
			BWLog.Info("Failed to get image for preview model");
		}
		launchAction = delegate
		{
			InsertPreviewModel();
			if (config.previewModel != null)
			{
				Blocksworld.bw.Play();
			}
			else
			{
				platformDelegate.WorldWillQuit("corrupt_model_source");
				Blocksworld.CleanupAndQuitToMenu();
			}
		};
		sessionTitle = config.previewModelTitle;
		sessionUserName = config.previewModelAuthorUsername;
		sessionDescription = config.worldDescription;
	}

	private void OnLoadedScreenshotSession()
	{
		initialButtonLayout = new WorldUILayout(TILE_BUTTON.RESTART, TILE_BUTTON.CAPTURE_SETUP, TILE_BUTTON.EXIT);
		captureSetupButtonLayout = new WorldUILayout(TILE_BUTTON.RESTART, TILE_BUTTON.PLAY, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT);
		stateCaptureButtonLayout = WorldUILayout.WorldUILayoutHidden;
		Blocksworld.DisableBuildMode();
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		launchAction = delegate
		{
			Blocksworld.bw.Play();
		};
		sessionTitle = config.worldTitle;
		sessionUserName = config.worldAuthorUsername;
		sessionDescription = config.worldDescription;
	}

	public bool BlockIsAvailable(string blockItemIdentifier)
	{
		if (current.config.blocksInventory != null)
		{
			return current.config.blocksInventory.ContainsBlockItemIdentifier(blockItemIdentifier);
		}
		return true;
	}

	private void InsertPreviewModel()
	{
		config.previewModel = Blocksworld.InsertModel(config.previewModelSource, config.previewModelPositionX, config.previewModelPositionY, config.previewModelPositionZ);
		if (config.previewModel == null)
		{
			return;
		}
		foreach (Block item in config.previewModel)
		{
			item.Update();
		}
		Bounds bounds = Util.ComputeBoundsWithSize(config.previewModel);
		Vector3 normalized = Blocksworld.cameraTransform.position.normalized;
		float num = Mathf.Max(3f * bounds.extents.magnitude, 10f);
		Blocksworld.cameraTransform.position = bounds.center + num * normalized;
		Blocksworld.cameraTransform.LookAt(bounds.center);
		Blocksworld.blocksworldCamera.Follow(config.previewModel[0]);
	}

	internal void TriggerModelTutorial()
	{
		List<List<List<Tile>>> tilesLists = ModelUtils.ParseModelString(config.previewModelSource);
		Blocksworld.SetupTutorialGAFs(tilesLists);
		Blocksworld.EnableBuildMode();
		Blocksworld.SetBuildPanelRightSided();
		Blocksworld.UI.Overlay.HidePurchasedBanner();
		Action completionHandler = delegate
		{
			Tutorial.Start();
			WorldUILayout worldUILayout = new WorldUILayout(TILE_BUTTON.PLAY, TILE_BUTTON.EXIT);
			worldUILayout.Apply();
			inModelTutorial = true;
		};
		Sound.PlayOneShotSound("Button Stop");
		Blocksworld.bw.Stop();
		List<Block> modelBlocks = BWSceneManager.NonTerrainBlocks();
		ModelTutorializeUtils.PrepareForStepByStepTutorial(modelBlocks, new ModelTutorializeUtils.StepByStepTutorializeOptions
		{
			waitTimePerBlock = 0f
		}, completionHandler);
	}

	private void RestorePreviewModel()
	{
		List<Block> list = BWSceneManager.NonTerrainBlocks();
		HashSet<Block> hashSet = new HashSet<Block>();
		HashSet<Chunk> hashSet2 = new HashSet<Chunk>();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			hashSet.Add(block);
			if (block.chunk != null)
			{
				hashSet2.Add(block.chunk);
			}
		}
		foreach (Chunk item in hashSet2)
		{
			item.Destroy();
		}
		foreach (Block item2 in hashSet)
		{
			BWSceneManager.RemoveBlock(item2);
			item2.Destroy();
		}
		InsertPreviewModel();
	}

	internal void EndModelTutorial()
	{
		Tutorial.Stop();
		Tutorial.blocks.Clear();
		inModelTutorial = false;
		Tutorial.state = TutorialState.None;
		Blocksworld.UI.ShowTitleBar();
		Blocksworld.UI.ShowBuildModelButton();
		if (config.previewModelPurchased)
		{
			Blocksworld.UI.Overlay.ShowPurchasedBanner();
		}
		Blocksworld.DisableBuildMode();
	}

	public void OnPlay()
	{
		Blocksworld.UI.SidePanel.Hide();
		Blocksworld.UI.Dialog.CloseActiveDialog();
		if (inModelTutorial)
		{
			EndModelTutorial();
			RestorePreviewModel();
		}
		if (statePlayButtonLayout != null)
		{
			statePlayButtonLayout.Apply();
		}
		else
		{
			initialButtonLayout.Apply();
		}
		Blocksworld.UI.SetControlsCanvasVisible(!Blocksworld.IsVRCameraMode());
	}

	public void OnRestart()
	{
		Blocksworld.UI.SidePanel.Hide();
		if (statePlayButtonLayout != null)
		{
			statePlayButtonLayout.Apply();
		}
		else
		{
			initialButtonLayout.Apply();
		}
		ResetAutoSaveCounter();
	}

	public void OnStop()
	{
		if (!Blocksworld.bw.forcePlayMode)
		{
			Blocksworld.UI.SidePanel.Show();
		}
		if (initialButtonLayout != null)
		{
			initialButtonLayout.Apply();
		}
		ResetAutoSaveCounter();
		jumpToWorldTriggered = false;
		jumpToWorldInfo = null;
	}

	public void OnShare()
	{
		if (stateShareButtonLayout != null)
		{
			stateShareButtonLayout.Apply();
		}
		else
		{
			initialButtonLayout.Apply();
		}
	}

	public void OnCapture()
	{
		if (stateCaptureButtonLayout != null)
		{
			stateCaptureButtonLayout.Apply();
		}
		else
		{
			initialButtonLayout.Apply();
		}
	}

	public void OnCompleteBuild()
	{
		if (config.isModelPreview)
		{
			EndModelTutorial();
			Blocksworld.bw.Play();
		}
		else
		{
			config.puzzleBuildModeCompleted = true;
			platformDelegate.CompletePuzzleBuild();
		}
	}

	public void OnWinGame()
	{
		if (!isPuzzleBuildSession() && !isPuzzlePlaySession())
		{
			return;
		}
		if (!config.puzzleHasRewardGAFs && config.puzzleIsPurchased)
		{
			platformDelegate.CompletePuzzlePlay();
			return;
		}
		if (!config.puzzleIsPurchased)
		{
			Blocksworld.ShowSetPurchasePrompt(config.puzzleRewardGAFsJsonStr, config.puzzleSetTitle, config.puzzleBuildingSetId, config.puzzleBuildingSetPrice);
		}
		else if (!config.puzzlePlayModeCompleted && config.puzzleBuildModeCompleted)
		{
			Blocksworld.VisualizeBlockReward(config.puzzleRewardGAFsJsonStr);
		}
		config.puzzlePlayModeCompleted = true;
	}

	public void OnPurchaseBuildingSetFromPuzzle()
	{
		Blocksworld.winIsWaiting = true;
		Blocksworld.SetBlocksworldState(State.Play);
		Tutorial.ResetState();
		Blocksworld.waitForSetPurchase = false;
		config.puzzleIsPurchased = true;
		Blocksworld.VisualizeBlockReward(config.puzzleRewardGAFsJsonStr);
	}

	public void OnRewardVisualizationComplete()
	{
		if (sessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview)
		{
			Quit();
		}
		else
		{
			platformDelegate.CompletePuzzlePlay();
		}
	}

	public void ReplayKitViewControllerDidAppear()
	{
		Pause();
		Blocksworld.musicPlayer.SetVolumeMultiplier(0f);
	}

	public void ReplayKitViewControllerDidDisappear()
	{
		Unpause();
		Blocksworld.musicPlayer.SetVolumeMultiplier(1f);
		WorldUILayout.currentLayout.Apply();
		Blocksworld.UI.Tapedeck.RefreshRecordButtonState();
	}

	public void ToggleWorldUpvoted()
	{
		isLiked = !isLiked;
		WorldUILayout.currentLayout.Apply();
		platformDelegate.SetWorldUpvoted(isLiked);
	}

	public void ConfirmModelPurchase()
	{
		Sound.PlayOneShotSound("Button Generic");
		Blocksworld.UI.Dialog.ShowModelPurchaseConfirmation(config.previewModelTitle, config.previewModelSellingPrice, previewModelImage);
	}

	public void AddModelToCart()
	{
		QuitWithDeepLink("add_model_to_cart");
	}

	public bool TakeScreenshot()
	{
		if (isWorldScreenshotSession() || isProfileBuildSession())
		{
			string label = ((!isProfileBuildSession()) ? config.worldTitle : "Profile Picture");
			byte[] array = Util.RenderScreenshotForCoverImage();
			if (array == null)
			{
				return false;
			}
			platformDelegate.SendScreenShot(array, label);
		}
		else
		{
			Blocksworld.UI.Overlay.ShowScreenshotIdent();
			byte[] array2 = Util.RenderFullScreenScreenshot();
			Blocksworld.UI.Overlay.HideScreenshotIdent();
			if (array2 == null)
			{
				return false;
			}
			platformDelegate.ImageWriteToSavedPhotosAlbum(array2);
		}
		return true;
	}

	public void EnterEditorShareMode()
	{
		initialButtonLayout = (statePlayButtonLayout = new WorldUILayout(TILE_BUTTON.PAUSE, TILE_BUTTON.EXIT));
		statePauseButtonLayout = new WorldUILayout(TILE_BUTTON.PLAY, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT);
		stateShareButtonLayout = new WorldUILayout(TILE_BUTTON.PLAY, TILE_BUTTON.EXIT);
		Blocksworld.UI.SidePanel.Hide();
		statePauseButtonLayout.Apply();
	}

	public void EnterLeaderboardStartScreen()
	{
		current.Pause();
		Blocksworld.lockInput = true;
		if (leaderboardStartScreenLayout != null)
		{
			leaderboardStartScreenLayout.Apply();
		}
	}

	public void ExitLeaderboardStartScreen()
	{
		current.Unpause();
		Blocksworld.lockInput = false;
		if (statePlayButtonLayout != null)
		{
			statePlayButtonLayout.Apply();
		}
		else
		{
			initialButtonLayout.Apply();
		}
	}

	public void EnterLeaderboardWinScreen()
	{
		Blocksworld.lockInput = true;
		if (leaderboardWinScreenLayout != null)
		{
			leaderboardWinScreenLayout.Apply();
		}
	}

	public void EnterScreenCaptureSetup()
	{
		if (Blocksworld.CurrentState != State.Play)
		{
			BWLog.Warning("trying to enter screen capture setup from wrong state: " + Blocksworld.CurrentState);
			return;
		}
		PausePlay();
		Blocksworld.UI.SetControlsCanvasVisible(visible: false);
		if (captureSetupButtonLayout != null)
		{
			captureSetupButtonLayout.Apply();
		}
		Blocksworld.SetBlocksworldState(State.FrameCapture);
		MappedInput.SetMode(MappableInputMode.Build);
	}

	public void ExitScreenCaptureSetup()
	{
		if (Blocksworld.CurrentState != State.FrameCapture)
		{
			BWLog.Warning("trying to exit screen capture setup from wrong state: " + Blocksworld.CurrentState);
			return;
		}
		UnpausePlay();
		Blocksworld.UI.SetControlsCanvasVisible(visible: true);
		Blocksworld.SetBlocksworldState(State.Play);
		MappedInput.SetMode(MappableInputMode.Play);
	}

	public static void PauseCurrentSession()
	{
		if (current != null)
		{
			current.Pause();
		}
	}

	public static void UnpauseCurrentSession()
	{
		if (current != null)
		{
			current.Unpause();
		}
	}

	public void PauseButtonPressed()
	{
		Pause();
		if (statePauseButtonLayout != null)
		{
			statePauseButtonLayout.Apply();
		}
		else
		{
			initialButtonLayout.Apply();
		}
	}

	public void Pause()
	{
		if (Blocksworld.CurrentState != State.Paused && Blocksworld.CurrentState != State.EditTile && Blocksworld.CurrentState != State.WaitForOption)
		{
			if (Blocksworld.CurrentState == State.Play)
			{
				PausePlay();
			}
			Blocksworld.UI.SetControlsCanvasVisible(visible: false);
			sessionUnpauseState = Blocksworld.CurrentState;
			Blocksworld.SetBlocksworldState(State.Paused);
		}
	}

	public void Unpause()
	{
		if (Blocksworld.CurrentState == State.Paused)
		{
			if (sessionUnpauseState == State.Play)
			{
				UnpausePlay();
			}
			Blocksworld.blocksworldCamera.SetCameraStill(still: false);
			Blocksworld.lockInput = false;
			Blocksworld.UI.SetControlsCanvasVisible(visible: true);
			Blocksworld.SetBlocksworldState(sessionUnpauseState);
		}
	}

	private void PausePlay()
	{
		BWSceneManager.PauseBlocks();
		VisualEffect.PauseVfxs();
		Blocksworld.weather.Pause();
	}

	private void UnpausePlay()
	{
		BWSceneManager.UnpauseBlocks();
		VisualEffect.ResumeVfxs();
		Blocksworld.weather.Resume();
	}

	public static void Quit()
	{
		QuitWithDeepLink(null);
	}

	public static void QuitWithDeepLink(string deepLinkStr)
	{
		if (_platformDelegate.ScreenRecordingInProgress())
		{
			_platformDelegate.StopRecordingScreen();
		}
		current.Unpause();
		current._deepLinkOnQuit = deepLinkStr;
		Blocksworld.musicPlayer.Unload();
		current.exitTriggered = true;
	}

	public bool WorldTeleportHasSource()
	{
		if (jumpToWorldInfo != null)
		{
			return !jumpToWorldInfo.HasWorldSource();
		}
		return false;
	}

	public bool TriggerJumpToWorld(string worldId, float delay = 0f)
	{
		if (jumpToWorldTriggered)
		{
			return false;
		}
		if (string.IsNullOrEmpty(worldId))
		{
			return false;
		}
		WorldInfo worldWithId = availableTeleportWorlds.GetWorldWithId(worldId);
		if (worldWithId == null)
		{
			BWLog.Error("Trying to jump to unloaded world info");
			return false;
		}
		jumpToWorldTriggered = true;
		jumpToWorldCountdown = delay;
		jumpToWorldInfo = worldWithId;
		jumpToWorldInfo.LoadWorldSourceForTeleport();
		return true;
	}

	public static void Save()
	{
		if (!Blocksworld.worldSaveEnabled)
		{
			return;
		}
		if (isProfileBuildSession())
		{
			if (BWSceneManager.BlockCount() == 0)
			{
				BWLog.Error("Trying to save invalid profile world");
				return;
			}
			if (ProfileBlocksterUtils.GetProfileCharacterBlock() == null)
			{
				BWLog.Error("Trying to save profile world with no profile character");
				return;
			}
			current.worldSourceJsonStr = Blocksworld.bw.Serialize();
			string avatarSource = Blocksworld.bw.ExtractProfileWorldAvatarString();
			platformDelegate.SetProfileWorldData(current.worldSourceJsonStr, avatarSource, current.config.profileGender);
			return;
		}
		Tutorial.BeforeSave();
		if (current.config.jumpedFromBuildMode)
		{
			if (jumpRestoreConfig == null)
			{
				BWLog.Error("Can't save original build mode world from jump world");
				return;
			}
			current.worldSourceJsonStr = jumpRestoreConfig.worldSourceJsonStr;
			current.hasWinCondition = jumpRestoreConfig.worldHasWinCondition == true;
			current.config.worldScreenshotTakenManually = jumpRestoreConfig.worldScreenshotTakenManually;
		}
		else
		{
			current.worldSourceJsonStr = Blocksworld.bw.Serialize();
			current.hasWinCondition = Blocksworld.HasWinCondition();
		}
		if (current.config.worldScreenshotTakenManually)
		{
			platformDelegate.SaveCurrentWorldData();
		}
		else
		{
			byte[] screenshotImageData = getScreenshotImageData();
			platformDelegate.SaveCurrentWorldDataWithScreenshot(screenshotImageData);
		}
		current.ResetAutoSaveCounter();
		Tutorial.AfterSave();
	}

	public static void FastSave()
	{
		string newWorldSourceJsonStr = Blocksworld.bw.Serialize();
		if (current.WorldSourceEquals(newWorldSourceJsonStr))
		{
			current.ResetAutoSaveCounter();
			return;
		}
		Tutorial.BeforeSave();
		current.worldSourceJsonStr = newWorldSourceJsonStr;
		current.hasWinCondition = Blocksworld.HasWinCondition();
		current.config.metaData = WorldMetrics.GetMetaData();
		platformDelegate.SaveCurrentWorldData();
		current.ResetAutoSaveCounter();
		Tutorial.AfterSave();
	}

	public static void FastSaveAutoUpdate()
	{
		if (current != null && !current.isJumpSession && current.config.autoSaveInterval > 0f && Time.time > current.scheduledAutoSave)
		{
			BWLog.Info("Auto saving...");
			FastSave();
		}
	}

	public static byte[] getScreenshotImageData()
	{
		Camera mainCamera = Blocksworld.mainCamera;
		Vector3 position = Blocksworld.cameraTransform.position;
		Quaternion rotation = Blocksworld.cameraTransform.rotation;
		bool show = TBox.IsShowing();
		TBox.Show(show: false);
		Blocksworld.blocksworldCamera.SetReticleEnabled(enabled: false);
		Blocksworld.bw.StatePlayUpdate();
		byte[] result = Util.RenderScreenshotForCoverImage();
		TBox.Show(show);
		Blocksworld.blocksworldCamera.SetReticleEnabled(enabled: true);
		Blocksworld.cameraTransform.position = position;
		Blocksworld.cameraTransform.rotation = rotation;
		return result;
	}

	public static void LoadClipboard()
	{
		platformDelegate.LoadClipboard();
	}

	public static void SaveClipboard(string clipboardJsonStr)
	{
		platformDelegate.SaveClipboard(clipboardJsonStr);
	}

	public static void OpenStore()
	{
		Save();
		QuitWithDeepLink("store/main");
	}

	public static void OpenStoreWithBlockItemId(TabBarTabId tabId, int blockItemId)
	{
		if (BWStandalone.Instance != null)
		{
			Save();
			BWStandalone.Instance.OpenStoreFromWorldWithBlockItemId(tabId, blockItemId);
			Quit();
		}
	}

	public void PurchaseBuildingSet(int setId)
	{
		platformDelegate.PurchaseBuildingSet(setId);
	}

	public string GetCurrentUserAvatarSource()
	{
		string text = config.currentUserAvatarSource;
		if (string.IsNullOrEmpty(text))
		{
			text = Resources.Load<TextAsset>("default_user_avatar_source").text;
		}
		return text;
	}

	public void LoadAvailableTeleportWorlds()
	{
		if (availableTeleportWorlds == null)
		{
			availableTeleportWorlds = new WorldInfoList();
			if (isNormalBuildAndPlaySession())
			{
				availableTeleportWorlds.LoadCurrentUserWorlds();
			}
		}
	}

	public void AddToAvailableTeleportWorlds(string worldId)
	{
		if (availableTeleportWorlds == null)
		{
			LoadAvailableTeleportWorlds();
		}
		availableTeleportWorlds.AddInfoForWorld(worldId);
	}

	public void ClearAvailableTeleportWorlds()
	{
		availableTeleportWorlds.Clear();
		availableTeleportWorlds = null;
	}

	public void UpdateLoop()
	{
		if ((exitTriggered || jumpToWorldTriggered) && !Blocksworld.modelCollection.modelSaveInProgress && !_platformDelegate.NativeModalActive() && !Blocksworld.bw.ModelAnimationInProgress() && !ScreenshotUtils.IsBusy())
		{
			if (exitTriggered && !_platformDelegate.ScreenRecordingInProgress())
			{
				if (Blocksworld.worldSessionHadVR)
				{
					platformDelegate.TrackAchievementIncrease("virtualnaut", 1);
				}
				if (config.sessionType == BWWorldSessionType.BWWorldSessionBuild || current.config.jumpedFromBuildMode)
				{
					if (Blocksworld.worldSessionHadBlocksterMover)
					{
						platformDelegate.TrackAchievementIncrease("first_steps", 1);
					}
					if (Blocksworld.worldSessionHadBlocksterSpeaker)
					{
						platformDelegate.TrackAchievementIncrease("first_words", 1);
					}
					if (Blocksworld.worldSessionHadHypderjumpUse)
					{
						platformDelegate.TrackAchievementIncrease("interdimensional_builder", 1);
					}
				}
				if (config.sessionType == BWWorldSessionType.BWWorldSessionCommunityPlay && Blocksworld.worldSessionCoinsCollected > 0)
				{
					platformDelegate.TrackAchievementIncrease("coin_collector", Blocksworld.worldSessionCoinsCollected);
				}
				if (config.sessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview)
				{
					platformDelegate.TrackAchievementIncrease("window_shopper", 1);
				}
				scheduledModelRefresh = 0f;
				exitTriggered = false;
				jumpToWorldTriggered = false;
				if (BWStandalone.Instance != null)
				{
					BWUserModelsDataManager.Instance.RemoveListener(UpdateStandaloneModelCollection);
				}
				platformDelegate.WorldWillQuit(_deepLinkOnQuit);
				ViewportWatchdog.StopWatching();
				Blocksworld.CleanupAndQuitToMenu();
			}
			else if (jumpToWorldTriggered && jumpToWorldInfo != null)
			{
				if (Blocksworld.CurrentState == State.Build)
				{
					jumpToWorldTriggered = false;
					jumpToWorldInfo = null;
				}
				else if (Blocksworld.CurrentState == State.Play && jumpToWorldInfo.HasWorldSource() && Blocksworld.TimeInCurrentState() > 1f)
				{
					if (jumpToWorldCountdown <= 0f)
					{
						exitTriggered = false;
						jumpToWorldTriggered = false;
						JumpToWorld(jumpToWorldInfo);
					}
					else
					{
						jumpToWorldCountdown -= Time.deltaTime;
					}
				}
			}
		}
		if (scheduledModelRefresh > 0f && Time.time > scheduledModelRefresh)
		{
			TileIconManager.Instance.CheckModelIcons();
			scheduledModelRefresh = 0f;
		}
	}

	public void ResetAutoSaveCounter()
	{
		if (config.autoSaveInterval > 0f)
		{
			scheduledAutoSave = Time.time + config.autoSaveInterval;
		}
	}
}
