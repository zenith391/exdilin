using System.Collections.Generic;
using Blocks;
using Exdilin;
using SimpleJSON;
using UnityEngine;

public class WorldSessionConfig
{
	public BWWorldSessionType sessionType;

	public int currentUserId;

	public string currentUserUsername = "Blockster";

	public string currentUserAuthToken;

	public int currentUserCoins = 102450;

	public string currentUserAvatarSource;

	public bool isNewProfile;

	public string profileGender;

	public bool hasBlocksInventory;

	public bool hasModelCollectionInfo;

	public bool loadStandaloneModelCollection;

	public BlocksInventory blocksInventory;

	public JObject modelCollectionJson;

	public string worldId;

	public string worldTitle = string.Empty;

	public string worldSourceJsonStr;

	public string worldDescription = string.Empty;

	public int worldAuthorId;

	public string worldAuthorUsername;

	public bool? worldHasWinCondition;

	public bool worldScreenshotTakenManually;

	public bool worldIsPublished;

	public bool likedByCurrentUser;

	public bool jumpedFromBuildMode;

	public string puzzleTitle;

	public string puzzleSetTitle;

	public bool puzzleIsPurchased;

	public bool puzzlePlayModeCompleted;

	public bool puzzleBuildModeCompleted;

	public bool puzzleHasRewardGAFs;

	public bool puzzleHasRewardModels;

	public string puzzleRewardGAFsJsonStr;

	public bool hideExitButton;

	public int puzzleBuildingSetId;

	public int puzzleBuildingSetPrice;

	private Dictionary<string, string> modelRewardData;

	public string previewModelSource;

	public float previewModelPositionX;

	public float previewModelPositionY;

	public float previewModelPositionZ;

	public string previewModelTitle;

	public int previewModelAuthorUserId;

	public string previewModelAuthorUsername;

	public int previewModelSellingPrice;

	public bool previewModelPurchased;

	public bool previewModelLocked;

	public List<Block> previewModel;

	public bool hasVRCameraButtonInPlayMode;

	public string metaData;

	public float autoSaveInterval;

	private static string _templateStarterIslandJsonStr;

	private static string _modelPreviewBackgroundLandJsonStr;

	private static string _modelPreviewBackgroundSkyJsonStr;

	private static string _modelPreviewBackgroundWaterJsonStr;

	private static Dictionary<BWWorldSessionType, string> niceSessionNames = new Dictionary<BWWorldSessionType, string>
	{
		{
			BWWorldSessionType.BWWorldSessionUndefined,
			"Default(Build)"
		},
		{
			BWWorldSessionType.BWWorldSessionBuild,
			"Build"
		},
		{
			BWWorldSessionType.BWWorldSessionScreenshot,
			"Screenshot"
		},
		{
			BWWorldSessionType.BWWorldSessionProfileBuild,
			"Profile"
		},
		{
			BWWorldSessionType.BWWorldSessionCommunityPlay,
			"Community Play"
		},
		{
			BWWorldSessionType.BWWorldSessionPuzzleBuild,
			"Puzzle Build"
		},
		{
			BWWorldSessionType.BWWorldSessionPuzzlePlay,
			"Puzzle Play"
		},
		{
			BWWorldSessionType.BWWorldSessionUserModelPreview,
			"User Model Preview"
		},
		{
			BWWorldSessionType.BWWorldSessionBuildingSetModelPreview,
			"Building Set Model Preview"
		},
		{
			BWWorldSessionType.BWWorldSessionCommunityModelPreview,
			"U2U Model Preview"
		}
	};

	public List<Dependency> worldRequiredMods;

	internal bool isModelPreview
	{
		get
		{
			if (sessionType != BWWorldSessionType.BWWorldSessionUserModelPreview && sessionType != BWWorldSessionType.BWWorldSessionBuildingSetModelPreview)
			{
				return sessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview;
			}
			return true;
		}
	}

	public WorldSessionConfig()
	{
		if (BWUser.currentUser != null)
		{
			BWUser currentUser = BWUser.currentUser;
			currentUserCoins = currentUser.coins;
			currentUserUsername = currentUser.username;
			currentUserId = currentUser.userID;
			currentUserAuthToken = currentUser.authToken;
		}
	}

	public static WorldSessionConfig Clone(WorldSessionConfig original)
	{
		return new WorldSessionConfig
		{
			sessionType = original.sessionType,
			worldSourceJsonStr = original.worldSourceJsonStr,
			worldId = original.worldId,
			worldDescription = original.worldDescription,
			worldTitle = original.worldTitle,
			worldAuthorId = original.worldAuthorId,
			worldAuthorUsername = original.worldAuthorUsername,
			worldIsPublished = original.worldIsPublished,
			worldHasWinCondition = original.worldHasWinCondition,
			worldRequiredMods = original.worldRequiredMods,
			worldScreenshotTakenManually = original.worldScreenshotTakenManually,
			currentUserId = original.currentUserId,
			currentUserUsername = original.currentUserUsername,
			currentUserAuthToken = original.currentUserAuthToken,
			currentUserCoins = original.currentUserCoins,
			currentUserAvatarSource = original.currentUserAvatarSource,
			hasBlocksInventory = original.hasBlocksInventory,
			blocksInventory = original.blocksInventory,
			hasModelCollectionInfo = original.hasModelCollectionInfo,
			modelCollectionJson = original.modelCollectionJson,
			likedByCurrentUser = original.likedByCurrentUser
		};
	}

	private static void addBuildPanelInfoForWebGL(WorldSessionConfig config)
	{
		config.blocksInventory = BlocksInventory.CreateUnlimited();
	}

	public static WorldSessionConfig BuildForWebGL(string worldSourceJsonStr)
	{
		return new WorldSessionConfig
		{
			sessionType = BWWorldSessionType.BWWorldSessionCommunityPlay,
			worldId = string.Empty,
			worldTitle = "WebGL",
			worldSourceJsonStr = worldSourceJsonStr,
			hasBlocksInventory = false,
			hasModelCollectionInfo = false,
			hasVRCameraButtonInPlayMode = false
		};
	}

	public static WorldSessionConfig BuildForWebGLWithModelSource(string backgroundType, string modelSourceJsonStr)
	{
		WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
		worldSessionConfig.sessionType = BWWorldSessionType.BWWorldSessionUserModelPreview;
		worldSessionConfig.worldId = string.Empty;
		worldSessionConfig.worldTitle = "WebGL";
		worldSessionConfig.previewModelTitle = string.Empty;
		worldSessionConfig.previewModelAuthorUsername = string.Empty;
		worldSessionConfig.previewModelSource = modelSourceJsonStr;
		switch (backgroundType.ToLowerInvariant())
		{
		case "sky":
			worldSessionConfig.worldSourceJsonStr = ModelPreviewBackgroundSkyJsonStr();
			worldSessionConfig.previewModelPositionY = 30f;
			return worldSessionConfig;
		case "water":
			worldSessionConfig.worldSourceJsonStr = ModelPreviewBackgroundWaterJsonStr();
			return worldSessionConfig;
		default:
			worldSessionConfig.worldSourceJsonStr = ModelPreviewBackgroundLandJsonStr();
			return worldSessionConfig;
		}
	}

	public static WorldSessionConfig BuildForWebGLBuildModeDemo()
	{
		WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
		worldSessionConfig.sessionType = BWWorldSessionType.BWWorldSessionBuild;
		worldSessionConfig.worldId = string.Empty;
		worldSessionConfig.worldTitle = "WebGLBuildDemo";
		worldSessionConfig.worldSourceJsonStr = TemplateStarterIslandJsonStr();
		worldSessionConfig.hasBlocksInventory = true;
		addBuildPanelInfoForWebGL(worldSessionConfig);
		return worldSessionConfig;
	}

	private static string TemplateStarterIslandJsonStr()
	{
		if (string.IsNullOrEmpty(_templateStarterIslandJsonStr))
		{
			_templateStarterIslandJsonStr = (Resources.Load("TemplateWorlds/Starter Island") as TextAsset).text;
		}
		return _templateStarterIslandJsonStr;
	}

	private static string ModelPreviewBackgroundLandJsonStr()
	{
		if (string.IsNullOrEmpty(_modelPreviewBackgroundLandJsonStr))
		{
			_modelPreviewBackgroundLandJsonStr = (Resources.Load("ModelPreviewBackgroundWorlds/ModelPreviewBackground_Land") as TextAsset).text;
		}
		return _modelPreviewBackgroundLandJsonStr;
	}

	private static string ModelPreviewBackgroundSkyJsonStr()
	{
		if (string.IsNullOrEmpty(_modelPreviewBackgroundSkyJsonStr))
		{
			_modelPreviewBackgroundSkyJsonStr = (Resources.Load("ModelPreviewBackgroundWorlds/ModelPreviewBackground_Sky") as TextAsset).text;
		}
		return _modelPreviewBackgroundSkyJsonStr;
	}

	private static string ModelPreviewBackgroundWaterJsonStr()
	{
		if (string.IsNullOrEmpty(_modelPreviewBackgroundWaterJsonStr))
		{
			_modelPreviewBackgroundWaterJsonStr = (Resources.Load("ModelPreviewBackgroundWorlds/ModelPreviewBackground_Water") as TextAsset).text;
		}
		return _modelPreviewBackgroundWaterJsonStr;
	}

	public static WorldSessionConfig BuildFromConfigJsonStr(string worldSessionConfigJsonStr)
	{
		JObject jObject = JSONDecoder.Decode(worldSessionConfigJsonStr);
		if (jObject == null)
		{
			Debug.LogError("Failed to parse worldSessionConfigJsonStr");
			return null;
		}
		return BuildFromConfigJson(jObject);
	}

	public static WorldSessionConfig BuildFromConfigJson(JObject json)
	{
		WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
		worldSessionConfig.sessionType = (BWWorldSessionType)json["session-type"].IntValue;
		if (json.ContainsKey("current-user-id"))
		{
			worldSessionConfig.currentUserId = json["current-user-id"].IntValue;
		}
		if (json.ContainsKey("current-user-username"))
		{
			worldSessionConfig.currentUserUsername = json["current-user-username"].StringValue;
		}
		if (json.ContainsKey("current-user-auth-token"))
		{
			worldSessionConfig.currentUserAuthToken = json["current-user-auth-token"].StringValue;
		}
		if (json.ContainsKey("current-user-coins"))
		{
			worldSessionConfig.currentUserCoins = json["current-user-coins"].IntValue;
		}
		if (json.ContainsKey("current-user-avatar-source-json-str"))
		{
			worldSessionConfig.currentUserAvatarSource = json["current-user-avatar-source-json-str"].StringValue;
		}
		if (worldSessionConfig.sessionType == BWWorldSessionType.BWWorldSessionProfileBuild)
		{
			if (json.ContainsKey("is-new-profile"))
			{
				worldSessionConfig.isNewProfile = json["is-new-profile"].BooleanValue;
			}
			if (json.ContainsKey("profile-gender"))
			{
				worldSessionConfig.profileGender = json["profile-gender"].StringValue;
			}
		}
		worldSessionConfig.hasBlocksInventory = json["has-blocks-inventory"].BooleanValue;
		if (worldSessionConfig.hasBlocksInventory)
		{
			string stringValue = json["blocks-inventory"].StringValue;
			worldSessionConfig.blocksInventory = BlocksInventory.FromString(stringValue);
		}
		worldSessionConfig.hasModelCollectionInfo = json["has-model-collection-info"].BooleanValue;
		if (worldSessionConfig.hasModelCollectionInfo)
		{
			worldSessionConfig.modelCollectionJson = json["model-collection-info"];
		}
		if (json.ContainsKey("world-id"))
		{
			worldSessionConfig.worldId = json["world-id"].StringValue;
		}
		worldSessionConfig.worldTitle = json["world-title"].StringValue;
		worldSessionConfig.worldSourceJsonStr = json["world-source-json-str"].StringValue;
		worldSessionConfig.worldDescription = json["world-description"].StringValue;
		if (json.ContainsKey("world-author-id"))
		{
			worldSessionConfig.worldAuthorId = json["world-author-id"].IntValue;
		}
		if (json.ContainsKey("world-author-username"))
		{
			worldSessionConfig.worldAuthorUsername = json["world-author-username"].StringValue;
		}
		worldSessionConfig.worldHasWinCondition = json["has-win-condition"].BooleanValue;
		if (worldSessionConfig.sessionType == BWWorldSessionType.BWWorldSessionBuild)
		{
			worldSessionConfig.worldScreenshotTakenManually = json["world-screenshot-taken-manually"].BooleanValue;
			worldSessionConfig.worldIsPublished = json["world-is-published"].BooleanValue;
			worldSessionConfig.autoSaveInterval = 30f;
		}
		if (worldSessionConfig.sessionType == BWWorldSessionType.BWWorldSessionCommunityPlay)
		{
			worldSessionConfig.likedByCurrentUser = json["world-liked-by-current-user"].BooleanValue;
		}
		if (WorldSession.SessionTypeIsPuzzle(worldSessionConfig.sessionType))
		{
			worldSessionConfig.puzzleTitle = json["puzzle-title"].StringValue;
			worldSessionConfig.puzzleSetTitle = json["puzzle-set-title"].StringValue;
			worldSessionConfig.puzzleBuildingSetId = json["puzzle-set-id"].IntValue;
			worldSessionConfig.puzzleBuildingSetPrice = json["puzzle-set-price"].IntValue;
			worldSessionConfig.puzzleIsPurchased = json["puzzle-is-purchased"].BooleanValue;
			worldSessionConfig.puzzleBuildModeCompleted = json["puzzle-build-mode-completed"].BooleanValue;
			worldSessionConfig.puzzlePlayModeCompleted = json["puzzle-play-mode-completed"].BooleanValue;
			worldSessionConfig.puzzleHasRewardGAFs = json["puzzle-has-reward-gafs"].BooleanValue;
			worldSessionConfig.puzzleHasRewardModels = json["puzzle-has-reward-models"].BooleanValue;
			worldSessionConfig.puzzleRewardGAFsJsonStr = ((!worldSessionConfig.puzzleHasRewardGAFs) ? string.Empty : json["puzzle-reward-gafs-jsonStr"].StringValue);
			if (json.ContainsKey("hide-exit-button"))
			{
				worldSessionConfig.hideExitButton = json["hide-exit-button"].BooleanValue;
			}
			if (worldSessionConfig.puzzleHasRewardModels)
			{
				string stringValue2 = json["puzzle-reward-models-jsonStr"].StringValue;
				if (string.IsNullOrEmpty(stringValue2))
				{
					worldSessionConfig.puzzleHasRewardModels = false;
				}
				else
				{
					worldSessionConfig.modelRewardData = LoadModelRewardData(json["puzzle-reward-models-jsonStr"].StringValue);
				}
			}
		}
		if (worldSessionConfig.isModelPreview)
		{
			worldSessionConfig.previewModelSource = json["preview-model-source"].StringValue;
			worldSessionConfig.previewModelPositionX = json["preview-model-position-x"].FloatValue;
			worldSessionConfig.previewModelPositionY = json["preview-model-position-y"].FloatValue;
			worldSessionConfig.previewModelPositionZ = json["preview-model-position-z"].FloatValue;
			worldSessionConfig.previewModelTitle = json["preview-model-title"].StringValue;
			if (string.IsNullOrEmpty(worldSessionConfig.previewModelTitle))
			{
				worldSessionConfig.previewModelTitle = "Untitled Model";
			}
			worldSessionConfig.previewModelAuthorUsername = json["preview-model-author-username"].StringValue;
			worldSessionConfig.previewModelLocked = json["preview-model-locked"].BooleanValue;
			if (json.ContainsKey("preview-model-author-id"))
			{
				worldSessionConfig.previewModelAuthorUserId = json["preview-model-author-id"].IntValue;
			}
			else
			{
				worldSessionConfig.previewModelAuthorUserId = worldSessionConfig.currentUserId;
			}
		}
		if (worldSessionConfig.sessionType == BWWorldSessionType.BWWorldSessionBuildingSetModelPreview)
		{
			worldSessionConfig.previewModelPurchased = json["preview-model-purchased"].BooleanValue;
		}
		if (worldSessionConfig.sessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview)
		{
			worldSessionConfig.previewModelPurchased = json["preview-model-purchased"].BooleanValue;
			worldSessionConfig.previewModelSellingPrice = json["preview-model-selling-price"].IntValue;
		}
		worldSessionConfig.hasVRCameraButtonInPlayMode = json.ContainsKey("has-play-mode-vr-camera-button") && json["has-play-mode-vr-camera-button"].BooleanValue;
		worldSessionConfig.hasVRCameraButtonInPlayMode &= IsVRSessionType(worldSessionConfig.sessionType);
		return worldSessionConfig;
	}

	private static bool IsVRSessionType(BWWorldSessionType currentSessionType)
	{
		bool flag = false;
		flag = flag || currentSessionType == BWWorldSessionType.BWWorldSessionBuild;
		flag = flag || currentSessionType == BWWorldSessionType.BWWorldSessionCommunityPlay;
		flag = flag || currentSessionType == BWWorldSessionType.BWWorldSessionPuzzleBuild;
		flag = flag || currentSessionType == BWWorldSessionType.BWWorldSessionPuzzlePlay;
		flag = flag || currentSessionType == BWWorldSessionType.BWWorldSessionUserModelPreview;
		flag = flag || currentSessionType == BWWorldSessionType.BWWorldSessionBuildingSetModelPreview;
		return flag || currentSessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview;
	}

	public string GetFormattedPreviewModelAuthorStr()
	{
		if (!string.IsNullOrEmpty(previewModelAuthorUsername))
		{
			return " by " + previewModelAuthorUsername;
		}
		return string.Empty;
	}

	public void UpdateModelCollectionFromJson(string modelCollectionJsonStr, bool includeOfflineModels)
	{
		modelCollectionJson = JSONDecoder.Decode(modelCollectionJsonStr);
		Blocksworld.modelCollection.LoadFromJSON(modelCollectionJson, includeOfflineModels);
	}

	public void ApplyBuildPanelInfo()
	{
		blocksInventory.AddAutomaticallyIncludedItems();
		Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
		Blocksworld.LoadGAFUnlockData(blocksInventory);
		Blocksworld.SetupBuildPanel();
		if (BW.Options.useScarcity())
		{
			bool addImplicitGafs = sessionType == BWWorldSessionType.BWWorldSessionPuzzleBuild;
			Scarcity.RefreshScarcity(blocksInventory, addImplicitGafs);
		}
	}

	public void ApplyModelCollectionInfo()
	{
		Blocksworld.modelCollection.Clear();
		Blocksworld.modelCollection.LoadFromJSON(modelCollectionJson, includeOfflineModels: true);
	}

	public void LoadPuzzleGAFsAndRewards()
	{
		Blocksworld.UnlockTutorialGAFs(worldSourceJsonStr);
		Scarcity.globalInventory = new Dictionary<GAF, int>();
		if (!puzzleHasRewardModels)
		{
			return;
		}
		foreach (KeyValuePair<string, string> modelRewardDatum in modelRewardData)
		{
			Blocksworld.DefineRewardModel(modelRewardDatum.Key, modelRewardDatum.Value);
			if (!puzzleIsPurchased)
			{
				Blocksworld.DefineRewardModelIcon(modelRewardDatum.Key, modelRewardDatum.Value);
			}
		}
	}

	private static Dictionary<string, string> LoadModelRewardData(string modelRewardJsonStr)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		JObject jObject = JSONDecoder.Decode(modelRewardJsonStr);
		foreach (string key in jObject.ObjectValue.Keys)
		{
			JObject jObject2 = jObject[key];
			if (jObject2 == null)
			{
				BWLog.Error("no model for : " + key);
			}
			else
			{
				dictionary[key] = jObject2.StringValue;
			}
		}
		return dictionary;
	}

	public static string NiceSessionName(BWWorldSessionType type)
	{
		return niceSessionNames[type];
	}
}
