using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x0200036D RID: 877
using Exdilin;
public class WorldSessionConfig
{

    // Exdilin fix to /name/ always returning "Blockster"
    public WorldSessionConfig()
    {
        if (BWUser.currentUser != null)
        {
            BWUser user = BWUser.currentUser;
            currentUserCoins = user.coins;
            currentUserUsername = user.username;
            currentUserId = user.userID;
            currentUserAuthToken = user.authToken;
        }
    }

    // Token: 0x0600273D RID: 10045 RVA: 0x001208DC File Offset: 0x0011ECDC
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

	// Token: 0x17000185 RID: 389
	// (get) Token: 0x0600273E RID: 10046 RVA: 0x001209E0 File Offset: 0x0011EDE0
	internal bool isModelPreview
	{
		get
		{
			return this.sessionType == BWWorldSessionType.BWWorldSessionUserModelPreview || this.sessionType == BWWorldSessionType.BWWorldSessionBuildingSetModelPreview || this.sessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview;
		}
	}

	// Token: 0x0600273F RID: 10047 RVA: 0x00120A09 File Offset: 0x0011EE09
	private static void addBuildPanelInfoForWebGL(WorldSessionConfig config)
	{
		config.blocksInventory = BlocksInventory.CreateUnlimited();
	}

	// Token: 0x06002740 RID: 10048 RVA: 0x00120A18 File Offset: 0x0011EE18
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

	// Token: 0x06002741 RID: 10049 RVA: 0x00120A68 File Offset: 0x0011EE68
	public static WorldSessionConfig BuildForWebGLWithModelSource(string backgroundType, string modelSourceJsonStr)
	{
		WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
		worldSessionConfig.sessionType = BWWorldSessionType.BWWorldSessionUserModelPreview;
		worldSessionConfig.worldId = string.Empty;
		worldSessionConfig.worldTitle = "WebGL";
		worldSessionConfig.previewModelTitle = string.Empty;
		worldSessionConfig.previewModelAuthorUsername = string.Empty;
		worldSessionConfig.previewModelSource = modelSourceJsonStr;
		string text = backgroundType.ToLowerInvariant();
		if (text != null)
		{
			if (text == "sky")
			{
				worldSessionConfig.worldSourceJsonStr = WorldSessionConfig.ModelPreviewBackgroundSkyJsonStr();
				worldSessionConfig.previewModelPositionY = 30f;
				return worldSessionConfig;
			}
			if (text == "water")
			{
				worldSessionConfig.worldSourceJsonStr = WorldSessionConfig.ModelPreviewBackgroundWaterJsonStr();
				return worldSessionConfig;
			}
		}
		worldSessionConfig.worldSourceJsonStr = WorldSessionConfig.ModelPreviewBackgroundLandJsonStr();
		return worldSessionConfig;
	}

	// Token: 0x06002742 RID: 10050 RVA: 0x00120B24 File Offset: 0x0011EF24
	public static WorldSessionConfig BuildForWebGLBuildModeDemo()
	{
		WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
		worldSessionConfig.sessionType = BWWorldSessionType.BWWorldSessionBuild;
		worldSessionConfig.worldId = string.Empty;
		worldSessionConfig.worldTitle = "WebGLBuildDemo";
		worldSessionConfig.worldSourceJsonStr = WorldSessionConfig.TemplateStarterIslandJsonStr();
		worldSessionConfig.hasBlocksInventory = true;
		WorldSessionConfig.addBuildPanelInfoForWebGL(worldSessionConfig);
		return worldSessionConfig;
	}

	// Token: 0x06002743 RID: 10051 RVA: 0x00120B6D File Offset: 0x0011EF6D
	private static string TemplateStarterIslandJsonStr()
	{
		if (string.IsNullOrEmpty(WorldSessionConfig._templateStarterIslandJsonStr))
		{
			WorldSessionConfig._templateStarterIslandJsonStr = (Resources.Load("TemplateWorlds/Starter Island") as TextAsset).text;
		}
		return WorldSessionConfig._templateStarterIslandJsonStr;
	}

	// Token: 0x06002744 RID: 10052 RVA: 0x00120B9C File Offset: 0x0011EF9C
	private static string ModelPreviewBackgroundLandJsonStr()
	{
		if (string.IsNullOrEmpty(WorldSessionConfig._modelPreviewBackgroundLandJsonStr))
		{
			WorldSessionConfig._modelPreviewBackgroundLandJsonStr = (Resources.Load("ModelPreviewBackgroundWorlds/ModelPreviewBackground_Land") as TextAsset).text;
		}
		return WorldSessionConfig._modelPreviewBackgroundLandJsonStr;
	}

	// Token: 0x06002745 RID: 10053 RVA: 0x00120BCB File Offset: 0x0011EFCB
	private static string ModelPreviewBackgroundSkyJsonStr()
	{
		if (string.IsNullOrEmpty(WorldSessionConfig._modelPreviewBackgroundSkyJsonStr))
		{
			WorldSessionConfig._modelPreviewBackgroundSkyJsonStr = (Resources.Load("ModelPreviewBackgroundWorlds/ModelPreviewBackground_Sky") as TextAsset).text;
		}
		return WorldSessionConfig._modelPreviewBackgroundSkyJsonStr;
	}

	// Token: 0x06002746 RID: 10054 RVA: 0x00120BFA File Offset: 0x0011EFFA
	private static string ModelPreviewBackgroundWaterJsonStr()
	{
		if (string.IsNullOrEmpty(WorldSessionConfig._modelPreviewBackgroundWaterJsonStr))
		{
			WorldSessionConfig._modelPreviewBackgroundWaterJsonStr = (Resources.Load("ModelPreviewBackgroundWorlds/ModelPreviewBackground_Water") as TextAsset).text;
		}
		return WorldSessionConfig._modelPreviewBackgroundWaterJsonStr;
	}

	// Token: 0x06002747 RID: 10055 RVA: 0x00120C2C File Offset: 0x0011F02C
	public static WorldSessionConfig BuildFromConfigJsonStr(string worldSessionConfigJsonStr)
	{
		JObject jobject = JSONDecoder.Decode(worldSessionConfigJsonStr);
		if (jobject == null)
		{
			Debug.LogError("Failed to parse worldSessionConfigJsonStr");
			return null;
		}
		return WorldSessionConfig.BuildFromConfigJson(jobject);
	}

	// Token: 0x06002748 RID: 10056 RVA: 0x00120C58 File Offset: 0x0011F058
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
			worldSessionConfig.blocksInventory = BlocksInventory.FromString(stringValue, true);
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
		worldSessionConfig.worldHasWinCondition = new bool?(json["has-win-condition"].BooleanValue);
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
					worldSessionConfig.modelRewardData = WorldSessionConfig.LoadModelRewardData(json["puzzle-reward-models-jsonStr"].StringValue);
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
		worldSessionConfig.hasVRCameraButtonInPlayMode = (json.ContainsKey("has-play-mode-vr-camera-button") && json["has-play-mode-vr-camera-button"].BooleanValue);
		worldSessionConfig.hasVRCameraButtonInPlayMode &= WorldSessionConfig.IsVRSessionType(worldSessionConfig.sessionType);
		return worldSessionConfig;
	}

	// Token: 0x06002749 RID: 10057 RVA: 0x00121248 File Offset: 0x0011F648
	private static bool IsVRSessionType(BWWorldSessionType currentSessionType)
	{
		bool flag = false;
		flag |= (currentSessionType == BWWorldSessionType.BWWorldSessionBuild);
		flag |= (currentSessionType == BWWorldSessionType.BWWorldSessionCommunityPlay);
		flag |= (currentSessionType == BWWorldSessionType.BWWorldSessionPuzzleBuild);
		flag |= (currentSessionType == BWWorldSessionType.BWWorldSessionPuzzlePlay);
		flag |= (currentSessionType == BWWorldSessionType.BWWorldSessionUserModelPreview);
		flag |= (currentSessionType == BWWorldSessionType.BWWorldSessionBuildingSetModelPreview);
		return flag | currentSessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview;
	}

	// Token: 0x0600274A RID: 10058 RVA: 0x0012128C File Offset: 0x0011F68C
	public string GetFormattedPreviewModelAuthorStr()
	{
		if (!string.IsNullOrEmpty(this.previewModelAuthorUsername))
		{
			return " by " + this.previewModelAuthorUsername;
		}
		return string.Empty;
	}

	// Token: 0x0600274B RID: 10059 RVA: 0x001212B4 File Offset: 0x0011F6B4
	public void UpdateModelCollectionFromJson(string modelCollectionJsonStr, bool includeOfflineModels)
	{
		this.modelCollectionJson = JSONDecoder.Decode(modelCollectionJsonStr);
		Blocksworld.modelCollection.LoadFromJSON(this.modelCollectionJson, includeOfflineModels);
	}

	// Token: 0x0600274C RID: 10060 RVA: 0x001212D4 File Offset: 0x0011F6D4
	public void ApplyBuildPanelInfo()
	{
		this.blocksInventory.AddAutomaticallyIncludedItems();
		Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
		Blocksworld.LoadGAFUnlockData(this.blocksInventory);
		Blocksworld.SetupBuildPanel();
		if (BW.Options.useScarcity())
		{
			bool addImplicitGafs = this.sessionType == BWWorldSessionType.BWWorldSessionPuzzleBuild;
			Scarcity.RefreshScarcity(this.blocksInventory, addImplicitGafs);
		}
	}

	// Token: 0x0600274D RID: 10061 RVA: 0x0012132B File Offset: 0x0011F72B
	public void ApplyModelCollectionInfo()
	{
		Blocksworld.modelCollection.Clear();
		Blocksworld.modelCollection.LoadFromJSON(this.modelCollectionJson, true);
	}

	// Token: 0x0600274E RID: 10062 RVA: 0x00121348 File Offset: 0x0011F748
	public void LoadPuzzleGAFsAndRewards()
	{
		Blocksworld.UnlockTutorialGAFs(this.worldSourceJsonStr);
		Scarcity.globalInventory = new Dictionary<GAF, int>();
		if (this.puzzleHasRewardModels)
		{
			foreach (KeyValuePair<string, string> keyValuePair in this.modelRewardData)
			{
				Blocksworld.DefineRewardModel(keyValuePair.Key, keyValuePair.Value);
				if (!this.puzzleIsPurchased)
				{
					Blocksworld.DefineRewardModelIcon(keyValuePair.Key, keyValuePair.Value);
				}
			}
		}
	}

	// Token: 0x0600274F RID: 10063 RVA: 0x001213F0 File Offset: 0x0011F7F0
	private static Dictionary<string, string> LoadModelRewardData(string modelRewardJsonStr)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		JObject jobject = JSONDecoder.Decode(modelRewardJsonStr);
		foreach (string text in jobject.ObjectValue.Keys)
		{
			JObject jobject2 = jobject[text];
			if (jobject2 == null)
			{
				BWLog.Error("no model for : " + text);
			}
			else
			{
				dictionary[text] = jobject2.StringValue;
			}
		}
		return dictionary;
	}

	// Token: 0x06002750 RID: 10064 RVA: 0x0012148C File Offset: 0x0011F88C
	public static string NiceSessionName(BWWorldSessionType type)
	{
		return WorldSessionConfig.niceSessionNames[type];
	}

	// Token: 0x04002220 RID: 8736
	public BWWorldSessionType sessionType;

	// Token: 0x04002221 RID: 8737
	public int currentUserId;

	// Token: 0x04002222 RID: 8738
	public string currentUserUsername = "Blockster";

	// Token: 0x04002223 RID: 8739
	public string currentUserAuthToken;

	// Token: 0x04002224 RID: 8740
	public int currentUserCoins = 102450; // note by exdilin: why did developers do that?

	// Token: 0x04002225 RID: 8741
	public string currentUserAvatarSource;

	// Token: 0x04002226 RID: 8742
	public bool isNewProfile;

	// Token: 0x04002227 RID: 8743
	public string profileGender;

	// Token: 0x04002228 RID: 8744
	public bool hasBlocksInventory;

	// Token: 0x04002229 RID: 8745
	public bool hasModelCollectionInfo;

	// Token: 0x0400222A RID: 8746
	public bool loadStandaloneModelCollection;

	// Token: 0x0400222B RID: 8747
	public BlocksInventory blocksInventory;

	// Token: 0x0400222C RID: 8748
	public JObject modelCollectionJson;

	// Token: 0x0400222D RID: 8749
	public string worldId;

	// Token: 0x0400222E RID: 8750
	public string worldTitle = string.Empty;

	// Token: 0x0400222F RID: 8751
	public string worldSourceJsonStr;

	// Token: 0x04002230 RID: 8752
	public string worldDescription = string.Empty;

	// Token: 0x04002231 RID: 8753
	public int worldAuthorId;

	// Token: 0x04002232 RID: 8754
	public string worldAuthorUsername;

	// Token: 0x04002233 RID: 8755
	public bool? worldHasWinCondition;

	// Token: 0x04002234 RID: 8756
	public bool worldScreenshotTakenManually;

	// Token: 0x04002235 RID: 8757
	public bool worldIsPublished;

	// Token: 0x04002236 RID: 8758
	public bool likedByCurrentUser;

	// Token: 0x04002237 RID: 8759
	public bool jumpedFromBuildMode;

	// Token: 0x04002238 RID: 8760
	public string puzzleTitle;

	// Token: 0x04002239 RID: 8761
	public string puzzleSetTitle;

	// Token: 0x0400223A RID: 8762
	public bool puzzleIsPurchased;

	// Token: 0x0400223B RID: 8763
	public bool puzzlePlayModeCompleted;

	// Token: 0x0400223C RID: 8764
	public bool puzzleBuildModeCompleted;

	// Token: 0x0400223D RID: 8765
	public bool puzzleHasRewardGAFs;

	// Token: 0x0400223E RID: 8766
	public bool puzzleHasRewardModels;

	// Token: 0x0400223F RID: 8767
	public string puzzleRewardGAFsJsonStr;

	// Token: 0x04002240 RID: 8768
	public bool hideExitButton;

	// Token: 0x04002241 RID: 8769
	public int puzzleBuildingSetId;

	// Token: 0x04002242 RID: 8770
	public int puzzleBuildingSetPrice;

	// Token: 0x04002243 RID: 8771
	private Dictionary<string, string> modelRewardData;

	// Token: 0x04002244 RID: 8772
	public string previewModelSource;

	// Token: 0x04002245 RID: 8773
	public float previewModelPositionX;

	// Token: 0x04002246 RID: 8774
	public float previewModelPositionY;

	// Token: 0x04002247 RID: 8775
	public float previewModelPositionZ;

	// Token: 0x04002248 RID: 8776
	public string previewModelTitle;

	// Token: 0x04002249 RID: 8777
	public int previewModelAuthorUserId;

	// Token: 0x0400224A RID: 8778
	public string previewModelAuthorUsername;

	// Token: 0x0400224B RID: 8779
	public int previewModelSellingPrice;

	// Token: 0x0400224C RID: 8780
	public bool previewModelPurchased;

	// Token: 0x0400224D RID: 8781
	public bool previewModelLocked;

	// Token: 0x0400224E RID: 8782
	public List<Block> previewModel;

	// Token: 0x0400224F RID: 8783
	public bool hasVRCameraButtonInPlayMode;

	// Token: 0x04002250 RID: 8784
	public string metaData;

	// Token: 0x04002251 RID: 8785
	public float autoSaveInterval;

	// Token: 0x04002252 RID: 8786
	private static string _templateStarterIslandJsonStr;

	// Token: 0x04002253 RID: 8787
	private static string _modelPreviewBackgroundLandJsonStr;

	// Token: 0x04002254 RID: 8788
	private static string _modelPreviewBackgroundSkyJsonStr;

	// Token: 0x04002255 RID: 8789
	private static string _modelPreviewBackgroundWaterJsonStr;

	// Token: 0x04002256 RID: 8790
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

	// added by exdilin
	public List<Dependency> worldRequiredMods;
}
