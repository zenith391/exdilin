using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x0200036B RID: 875
using Exdilin;
public class WorldSession
{
	// Token: 0x17000179 RID: 377
	// (get) Token: 0x060026B9 RID: 9913 RVA: 0x0011D8EB File Offset: 0x0011BCEB
	private BlocksworldWebInterface WebGLInterface
	{
		get
		{
			return BlocksworldWebInterface.Instance;
		}
	}

	// Token: 0x1700017A RID: 378
	// (get) Token: 0x060026BA RID: 9914 RVA: 0x0011D8F2 File Offset: 0x0011BCF2
	public static WorldSessionPlatformDelegate platformDelegate
	{
		get
		{
			if (WorldSession._platformDelegate == null)
			{
				WorldSession._platformDelegate = new WorldSessionPlatformDelegate_Standalone();
			}
			return WorldSession._platformDelegate;
		}
	}

	// Token: 0x060026BB RID: 9915 RVA: 0x0011D910 File Offset: 0x0011BD10
	public static void Start(WorldSessionConfig worldSessionConfig)
	{
		if (worldSessionConfig.worldRequiredMods != null) {
			foreach (Dependency dep in worldSessionConfig.worldRequiredMods) {
				Debug.Log(dep.Id);
			}
		}

		WorldEnvironmentManager.ResetConfiguration();
		WorldSession.current = new WorldSession();
		if (!WorldSession.current.ApplyConfig(worldSessionConfig))
		{
			BWLog.Error("Unable to load world source!  Quitting back to the main menu.");
			WorldSession.platformDelegate.WorldWillQuit("corrupt_world_source");
			ViewportWatchdog.StopWatching();
			Blocksworld.CleanupAndQuitToMenu();
			return;
		}
		WorldSession.current.initialButtonLayout.Apply();
		if (Blocksworld.worldSky == null)
		{
			List<Tile> list = new List<Tile>();
			list.Add(new Tile(new GAF("Block.Create", new object[]
			{
				"Sky"
			})));
			Block.NewBlock(new List<List<Tile>>
			{
				list
			}, false, false);
		}
		WorldSession.current.LaunchWorld();
		BWLog.Info("Starting world session type " + WorldSession.current.config.sessionType.ToString() + ", worldTitle: " + WorldSession.current.config.worldTitle);
	}

	// Token: 0x060026BC RID: 9916 RVA: 0x0011DA01 File Offset: 0x0011BE01
	public static void StartForIOS(string worldSessionConfigJsonStr)
	{
		Blocksworld.bw.StartCoroutine(WorldSession.CoroutineStartForIOS(worldSessionConfigJsonStr));
	}

	// Token: 0x060026BD RID: 9917 RVA: 0x0011DA14 File Offset: 0x0011BE14
	private static IEnumerator CoroutineStartForIOS(string worldSessionConfigJsonStr)
	{
		WorldSession._platformDelegate.WorldDidStartLoading();
		yield return null;
		WorldSessionConfig config = WorldSessionConfig.BuildFromConfigJsonStr(worldSessionConfigJsonStr);
		WorldSession.Start(config);
		yield break;
	}

	// Token: 0x060026BE RID: 9918 RVA: 0x0011DA30 File Offset: 0x0011BE30
	public static void JumpToWorld(WorldInfo worldInfo)
	{
		if (!worldInfo.HasWorldSource())
		{
			BWLog.Error("Trying to jump to world with no source loaded");
			return;
		}
		Blocksworld.worldSessionHadHypderjumpUse = true;
		Sound.PlayOneShotSound("Teleport_Arrival", 1f);
		bool flag = WorldSession.isNormalBuildAndPlaySession();
		bool jumpedFromBuildMode = WorldSession.current.config.jumpedFromBuildMode;
		WorldSessionConfig worldSessionConfig = WorldSession.CreateRestoreConfig();
		Blocksworld.Cleanup(false);
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
		WorldSession.Start(worldSessionConfig2);
		if (WorldSession.jumpRestoreConfig == null)
		{
			WorldSession.jumpRestoreConfig = worldSessionConfig;
		}
		WorldSession.current.isJumpSession = true;
	}

	// Token: 0x060026BF RID: 9919 RVA: 0x0011DB14 File Offset: 0x0011BF14
	private static WorldSessionConfig CreateRestoreConfig()
	{
		if (WorldSession.isNormalBuildAndPlaySession())
		{
			WorldSession.current.worldSourceJsonStr = Blocksworld.bw.Serialize(true);
			WorldSession.current.hasWinCondition = Blocksworld.HasWinCondition();
		}
		WorldSessionConfig original = WorldSession.current.config;
		WorldSessionConfig worldSessionConfig = WorldSessionConfig.Clone(original);
		worldSessionConfig.hasBlocksInventory = false;
		worldSessionConfig.hasModelCollectionInfo = false;
		worldSessionConfig.blocksInventory = null;
		worldSessionConfig.modelCollectionJson = null;
		return worldSessionConfig;
	}

	// Token: 0x060026C0 RID: 9920 RVA: 0x0011DB80 File Offset: 0x0011BF80
	public static void RestoreJumpConfig()
	{
		if (WorldSession.jumpRestoreConfig != null)
		{
			WorldSessionConfig worldSessionConfig = WorldSessionConfig.Clone(WorldSession.jumpRestoreConfig);
			WorldSession.jumpRestoreConfig = null;
			Blocksworld.Cleanup(false);
			WorldSession.Start(worldSessionConfig);
		}
		else
		{
			BWLog.Error("build mode restore config is null");
		}
	}

	// Token: 0x060026C1 RID: 9921 RVA: 0x0011DBC4 File Offset: 0x0011BFC4
	public static void StartForStandaloneWithRemoteWorldId(string id, bool buildMode, string currentUserAvatarSource)
	{
		if (string.IsNullOrEmpty(id))
		{
			BWLog.Error("Invalid worldId.");
			return;
		}
		BWRemoteWorldsDataManager.Instance.LoadWorld(id, delegate(BWWorld world)
		{
			WorldSession.StartForStandaloneWithRemoteWorld(world, buildMode, currentUserAvatarSource);
		});
	}

	// Token: 0x060026C2 RID: 9922 RVA: 0x0011DC14 File Offset: 0x0011C014
	public static void StartForStandaloneWithRemoteWorld(BWWorld world, bool buildMode, string currentAvatarSource)
	{
		if (world == null)
		{
			return;
		}
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
		string path = string.Format("/api/v1/worlds/{0}/plays", world.worldID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", path);
		bwapirequestBase.Send();
		WorldSession.Start(worldSessionConfig);
	}

	// Token: 0x060026C3 RID: 9923 RVA: 0x0011DCCC File Offset: 0x0011C0CC
	private static IEnumerator CoroutineStartForStandaloneWithRemoteWorldId(string id, bool buildMode, string currentUserAvatarSource)
	{
		if (string.IsNullOrEmpty(id))
		{
			BWLog.Error("Invalid worldId.");
			yield break;
		}
		string path = string.Format("/api/v1/worlds/{0}", id);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
		};
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
			WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
			JObject jobject = responseJson["world"];
			string stringValue = jobject["title"].StringValue;
			string worldSourceJsonStr = (!jobject.ContainsKey("source_json_str")) ? jobject["source"].StringValue : jobject["source_json_str"].StringValue;
			worldSessionConfig.sessionType = ((!buildMode) ? BWWorldSessionType.BWWorldSessionCommunityPlay : BWWorldSessionType.BWWorldSessionBuild);
			worldSessionConfig.worldId = id;
			worldSessionConfig.worldTitle = stringValue;
			worldSessionConfig.worldSourceJsonStr = worldSourceJsonStr;
			worldSessionConfig.likedByCurrentUser = false;
			worldSessionConfig.hasBlocksInventory = buildMode;
			worldSessionConfig.hasModelCollectionInfo = false;
			worldSessionConfig.blocksInventory = BWUser.currentUser.blocksInventory;
			worldSessionConfig.currentUserAvatarSource = currentUserAvatarSource;
			string path2 = string.Format("/api/v1/worlds/{0}/plays", id);
			BWAPIRequestBase bwapirequestBase2 = BW.API.CreateRequest("POST", path2);
			bwapirequestBase2.Send();
			WorldSession.Start(worldSessionConfig);
		};
		bwapirequestBase.Send();
		yield break;
	}

	// Token: 0x060026C4 RID: 9924 RVA: 0x0011DCF8 File Offset: 0x0011C0F8
	public static void StartForStandaloneInBuildMode(string worldID, string title, string source, string currentUserAvatarSource, bool screenshotTakenManually)
	{
		WorldSession.Start(new WorldSessionConfig
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

	// Token: 0x060026C5 RID: 9925 RVA: 0x0011DD64 File Offset: 0x0011C164
	public static void StartForStandaloneWorldScreenshotSession(string worldID, string title, string source, string currentUserAvatarSource)
	{
		WorldSession.Start(new WorldSessionConfig
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

	// Token: 0x060026C6 RID: 9926 RVA: 0x0011DDCC File Offset: 0x0011C1CC
	public static void StartForStandaloneWithProfileWorld(string profileGender, string worldID, string title, string source, string avatarSource)
	{
		WorldSession.Start(new WorldSessionConfig
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

	// Token: 0x060026C7 RID: 9927 RVA: 0x0011DE30 File Offset: 0x0011C230
	public static void StartForStandaloneWithUserModel(BWUserModel model)
	{
		string sourceJsonStr = model.sourceJsonStr;
		string previewTerrain = model.previewTerrain;
		WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
		worldSessionConfig.sessionType = BWWorldSessionType.BWWorldSessionUserModelPreview;
		HashSet<string> hashSet = new HashSet<string>
		{
			"land",
			"sky",
			"water"
		};
		string str = (!hashSet.Contains(previewTerrain)) ? "land" : previewTerrain;
		string path = "ModelPreviewWorlds/model_preview_world_source_" + str;
		worldSessionConfig.worldSourceJsonStr = Resources.Load<TextAsset>(path).text;
		worldSessionConfig.previewModelSource = sourceJsonStr;
		worldSessionConfig.previewModelPositionX = 0f;
		worldSessionConfig.previewModelPositionY = ((!(previewTerrain == "sky")) ? 0f : 30f);
		worldSessionConfig.previewModelPositionZ = 0f;
		worldSessionConfig.hasBlocksInventory = false;
		worldSessionConfig.hasModelCollectionInfo = false;
		WorldSession.Start(worldSessionConfig);
	}

	// Token: 0x060026C8 RID: 9928 RVA: 0x0011DF1C File Offset: 0x0011C31C
	public static void StartForStandaloneWithCommunityModel(string remoteModelID)
	{
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", string.Format("/api/v1/u2u_models/{0}", remoteModelID));
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("u2u_model"))
			{
				BWU2UModel bwu2UModel = new BWU2UModel(responseJson["u2u_model"]);
				string sourceJsonStr = bwu2UModel.sourceJsonStr;
				string previewTerrain = bwu2UModel.previewTerrain;
				WorldSessionConfig worldSessionConfig = new WorldSessionConfig();
				worldSessionConfig.sessionType = BWWorldSessionType.BWWorldSessionCommunityModelPreview;
				HashSet<string> hashSet = new HashSet<string>
				{
					"land",
					"sky",
					"water"
				};
				string str = (!hashSet.Contains(previewTerrain)) ? "land" : previewTerrain;
				string path = "ModelPreviewWorlds/model_preview_world_source_" + str;
				worldSessionConfig.worldSourceJsonStr = Resources.Load<TextAsset>(path).text;
				worldSessionConfig.previewModelSource = sourceJsonStr;
				worldSessionConfig.previewModelPositionX = 0f;
				worldSessionConfig.previewModelPositionY = ((!(previewTerrain == "sky")) ? 0f : 30f);
				worldSessionConfig.previewModelPositionZ = 0f;
				worldSessionConfig.hasBlocksInventory = false;
				worldSessionConfig.hasModelCollectionInfo = false;
				worldSessionConfig.currentUserId = BWUser.currentUser.userID;
				worldSessionConfig.currentUserCoins = BWUser.currentUser.coins;
				worldSessionConfig.previewModelAuthorUserId = bwu2UModel.authorId;
				worldSessionConfig.previewModelAuthorUsername = bwu2UModel.authorUsername;
				worldSessionConfig.previewModelLocked = bwu2UModel.sourceLocked;
				worldSessionConfig.previewModelSellingPrice = bwu2UModel.coinsPrice;
				worldSessionConfig.previewModelPurchased = BWU2UModelDataManager.Instance.HasPurchasedModel(remoteModelID);
				WorldSession.Start(worldSessionConfig);
			}
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
		};
		bwapirequestBase.Send();
	}

	// Token: 0x060026C9 RID: 9929 RVA: 0x0011DF94 File Offset: 0x0011C394
	public static void StartForWebGLWithWorldSourceJsonStr(string worldSourceJsonStr)
	{
		if (worldSourceJsonStr == null)
		{
			Debug.LogError("Error: attempting to load world with no source");
			return;
		}
		if (WorldSession.current != null)
		{
			Debug.LogError("Error: attempting to load world before previous world is unloaded");
			return;
		}
		Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
		WorldSessionConfig worldSessionConfig = WorldSessionConfig.BuildForWebGL(worldSourceJsonStr);
		BlocksworldWebInterface.Instance.OnBeforeWorldLoad();
		WorldSession.Start(worldSessionConfig);
	}

	// Token: 0x060026CA RID: 9930 RVA: 0x0011DFE8 File Offset: 0x0011C3E8
	public static void StartForWebGLWithModelSourceJsonStr(string backgroundType, string modelSourceJsonStr)
	{
		WorldSessionConfig worldSessionConfig = WorldSessionConfig.BuildForWebGLWithModelSource(backgroundType, modelSourceJsonStr);
		BlocksworldWebInterface.Instance.OnBeforeWorldLoad();
		WorldSession.Start(worldSessionConfig);
	}

	// Token: 0x060026CB RID: 9931 RVA: 0x0011E010 File Offset: 0x0011C410
	public static void StartForWebGLBuildModeDemo()
	{
		Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
		WorldSessionConfig worldSessionConfig = WorldSessionConfig.BuildForWebGLBuildModeDemo();
		WorldSession.Start(worldSessionConfig);
	}

	// Token: 0x060026CC RID: 9932 RVA: 0x0011E033 File Offset: 0x0011C433
	public static void UpdateModelCollection(string modelCollectionJsonStr, bool includeOfflineModels)
	{
		if (WorldSession.current == null)
		{
			BWLog.Error("Trying to update model collection but no active session");
			return;
		}
		WorldSession.current.config.UpdateModelCollectionFromJson(modelCollectionJsonStr, includeOfflineModels);
	}

	// Token: 0x060026CD RID: 9933 RVA: 0x0011E05B File Offset: 0x0011C45B
	public static void RefreshModelIcons()
	{
		if (WorldSession.current == null)
		{
			return;
		}
		WorldSession.current.scheduledModelRefresh = Time.time + 2f;
	}

	// Token: 0x060026CE RID: 9934 RVA: 0x0011E080 File Offset: 0x0011C480
	public static void BuildingSetPurchaseCallback(string status)
	{
		if (WorldSession.current == null)
		{
			BWLog.Error("Building set purchase callback with no active session");
			return;
		}
		if (WorldSession.isPuzzleBuildSession() || WorldSession.isPuzzlePlaySession())
		{
			if (string.Equals(status, "error"))
			{
				WorldSession.Quit();
				return;
			}
			WorldSession.current.OnPurchaseBuildingSetFromPuzzle();
		}
	}

	// Token: 0x060026CF RID: 9935 RVA: 0x0011E0D8 File Offset: 0x0011C4D8
	public static void ModelPurchaseCallback(string status)
	{
		if (WorldSession.current == null)
		{
			BWLog.Error("Model purchase callback with no active session");
			return;
		}
		if (string.Equals(status, "error"))
		{
			return;
		}
		Blocksworld.VisualizeRewardModel("purchasedModel", WorldSession.current.config.previewModelSource);
	}

	// Token: 0x1700017B RID: 379
	// (get) Token: 0x060026D0 RID: 9936 RVA: 0x0011E124 File Offset: 0x0011C524
	public BWWorldSessionType sessionType
	{
		get
		{
			return this.config.sessionType;
		}
	}

	// Token: 0x1700017C RID: 380
	// (get) Token: 0x060026D1 RID: 9937 RVA: 0x0011E131 File Offset: 0x0011C531
	public string worldId
	{
		get
		{
			return this.config.worldId;
		}
	}

	// Token: 0x1700017D RID: 381
	// (get) Token: 0x060026D2 RID: 9938 RVA: 0x0011E13E File Offset: 0x0011C53E
	public string worldTitle
	{
		get
		{
			return this.config.worldTitle;
		}
	}

	// Token: 0x1700017E RID: 382
	// (get) Token: 0x060026D3 RID: 9939 RVA: 0x0011E14B File Offset: 0x0011C54B
	public bool worldIsPublished
	{
		get
		{
			return this.config.worldIsPublished;
		}
	}

	// Token: 0x1700017F RID: 383
	// (get) Token: 0x060026D4 RID: 9940 RVA: 0x0011E158 File Offset: 0x0011C558
	public bool isWorldAuthorCurrentUser
	{
		get
		{
			return this.config.worldAuthorId == this.config.currentUserId;
		}
	}

	// Token: 0x17000180 RID: 384
	// (get) Token: 0x060026D5 RID: 9941 RVA: 0x0011E172 File Offset: 0x0011C572
	// (set) Token: 0x060026D6 RID: 9942 RVA: 0x0011E17F File Offset: 0x0011C57F
	public string worldSourceJsonStr
	{
		get
		{
			return this.config.worldSourceJsonStr;
		}
		set
		{
			this.config.worldSourceJsonStr = value;
		}
	}

	// Token: 0x17000181 RID: 385
	// (get) Token: 0x060026D7 RID: 9943 RVA: 0x0011E18D File Offset: 0x0011C58D
	// (set) Token: 0x060026D8 RID: 9944 RVA: 0x0011E19A File Offset: 0x0011C59A
	public string profileCharacterGender
	{
		get
		{
			return this.config.profileGender;
		}
		set
		{
			this.config.profileGender = value;
		}
	}

	// Token: 0x17000182 RID: 386
	// (get) Token: 0x060026D9 RID: 9945 RVA: 0x0011E1A8 File Offset: 0x0011C5A8
	// (set) Token: 0x060026DA RID: 9946 RVA: 0x0011E1E4 File Offset: 0x0011C5E4
	public bool hasWinCondition
	{
		get
		{
			if (this.config.worldHasWinCondition == null)
			{
				this.config.worldHasWinCondition = new bool?(Blocksworld.HasWinCondition());
			}
			return this.config.worldHasWinCondition.Value;
		}
		set
		{
			this.config.worldHasWinCondition = new bool?(value);
		}
	}

	// following variable added by exdilin
	public List<Exdilin.Dependency> requiredMods {
		get {
			if (this.config.worldRequiredMods == null) {
				this.config.worldRequiredMods = Blocksworld.GetRequiredMods();
			}
			return this.config.worldRequiredMods;
		}
		set {
			this.config.worldRequiredMods = value;
		}
	}

	public string requiredModsJsonStr {
		get {
			List<Dictionary<string, string>> modList = new List<Dictionary<string, string>>();
			foreach (Exdilin.Dependency dependency in requiredMods) {
				modList.Add(new Dictionary<string, string>() {
				{
					"id",
					dependency.Id
				},
				{
					"version",
					dependency.MinimumVersion.ToString()
				}
				});
			}
			return JSONEncoder.Encode(modList);
		}
	}

	// Token: 0x060026DB RID: 9947 RVA: 0x0011E1F8 File Offset: 0x0011C5F8
	public bool ApplyConfig(WorldSessionConfig worldSessionConfig)
	{
		this.config = worldSessionConfig;
		Blocksworld.bw.ClearWorldState();
		bool flag = Blocksworld.bw.LoadFromString(this.config.worldSourceJsonStr);
		if (!flag)
		{
			if (worldSessionConfig.sessionType != BWWorldSessionType.BWWorldSessionProfileBuild)
			{
				return false;
			}
			this.config.worldSourceJsonStr = Resources.Load<TextAsset>(Blocksworld.DefaultProfileWorldAssetPath()).text;
			flag = Blocksworld.bw.LoadFromString(this.config.worldSourceJsonStr);
		}
		if (!flag)
		{
			BWLog.Error("Failed to load world source");
		}
		this.ResetAutoSaveCounter();
		Blocksworld.bw.ProcessLoadedWorld();
		if (this.config.hasBlocksInventory)
		{
			Blocksworld.UI.SidePanel.Show();
			this.config.ApplyBuildPanelInfo();
		}
		if (this.config.loadStandaloneModelCollection)
		{
			Blocksworld.modelCollection.Clear();
			BWUserModelsDataManager.Instance.AddListener(new ModelsListChangedEventHandler(this.UpdateStandaloneModelCollection));
			Blocksworld.modelCollection.LoadModelCollectionForStandalone();
			Blocksworld.SetupBuildPanel();
		}
		else if (this.config.hasModelCollectionInfo)
		{
			this.config.ApplyModelCollectionInfo();
		}
		switch (this.config.sessionType)
		{
		case BWWorldSessionType.BWWorldSessionBuild:
			this.OnLoadedLocalWorld();
			break;
		case BWWorldSessionType.BWWorldSessionScreenshot:
			this.OnLoadedScreenshotSession();
			break;
		case BWWorldSessionType.BWWorldSessionProfileBuild:
			this.OnLoadedPlayerProfileWorld();
			break;
		case BWWorldSessionType.BWWorldSessionCommunityPlay:
			this.OnLoadedRemoteWorld();
			break;
		case BWWorldSessionType.BWWorldSessionPuzzleBuild:
			this.OnLoadedPuzzleBuildMode();
			break;
		case BWWorldSessionType.BWWorldSessionPuzzlePlay:
			this.OnLoadedPuzzlePlayMode();
			break;
		case BWWorldSessionType.BWWorldSessionUserModelPreview:
			this.OnLoadedUserModelPreview();
			break;
		case BWWorldSessionType.BWWorldSessionBuildingSetModelPreview:
			this.OnLoadedBuildingSetModelPreview();
			break;
		case BWWorldSessionType.BWWorldSessionCommunityModelPreview:
			this.OnLoadedCommunityModelPreview();
			break;
		}
		return true;
	}

	// Token: 0x060026DC RID: 9948 RVA: 0x0011E3C3 File Offset: 0x0011C7C3
	public void UpdateStandaloneModelCollection()
	{
		Blocksworld.modelCollection.UpdateLocalModelCollectionForStandalone();
	}

	// Token: 0x060026DD RID: 9949 RVA: 0x0011E3CF File Offset: 0x0011C7CF
	public bool WorldSourceEquals(string newWorldSourceJsonStr)
	{
		return !string.IsNullOrEmpty(this.worldSourceJsonStr) && string.Equals(newWorldSourceJsonStr, this.worldSourceJsonStr);
	}

	// Token: 0x17000183 RID: 387
	// (get) Token: 0x060026DE RID: 9950 RVA: 0x0011E3EF File Offset: 0x0011C7EF
	public bool IsLiked
	{
		get
		{
			return this.isLiked;
		}
	}

	// Token: 0x060026DF RID: 9951 RVA: 0x0011E3F7 File Offset: 0x0011C7F7
	public static void NotifyFileTooNew()
	{
		WorldSession.platformDelegate.NotifyWorldTooNew();
	}

	// Token: 0x060026E0 RID: 9952 RVA: 0x0011E403 File Offset: 0x0011C803
	public static bool SessionTypeIsPuzzle(BWWorldSessionType sessionType)
	{
		return sessionType == BWWorldSessionType.BWWorldSessionPuzzleBuild || sessionType == BWWorldSessionType.BWWorldSessionPuzzlePlay;
	}

	// Token: 0x060026E1 RID: 9953 RVA: 0x0011E413 File Offset: 0x0011C813
	public static bool isPuzzleBuildSession()
	{
		return WorldSession.current != null && WorldSession.current.config.sessionType == BWWorldSessionType.BWWorldSessionPuzzleBuild;
	}

	// Token: 0x060026E2 RID: 9954 RVA: 0x0011E434 File Offset: 0x0011C834
	public static bool isConstructionChallenge()
	{
		return (WorldSession.isPuzzleBuildSession() || WorldSession.isCommunitySession()) && Scarcity.puzzleInventory != null;
	}

	// Token: 0x060026E3 RID: 9955 RVA: 0x0011E458 File Offset: 0x0011C858
	public static bool isPuzzlePlaySession()
	{
		return WorldSession.current != null && WorldSession.current.config.sessionType == BWWorldSessionType.BWWorldSessionPuzzlePlay;
	}

	// Token: 0x060026E4 RID: 9956 RVA: 0x0011E479 File Offset: 0x0011C879
	public static bool isProfileBuildSession()
	{
		return WorldSession.current != null && WorldSession.current.config.sessionType == BWWorldSessionType.BWWorldSessionProfileBuild;
	}

	// Token: 0x060026E5 RID: 9957 RVA: 0x0011E49A File Offset: 0x0011C89A
	public static bool isNormalBuildAndPlaySession()
	{
		return WorldSession.current != null && WorldSession.current.config.sessionType == BWWorldSessionType.BWWorldSessionBuild;
	}

	// Token: 0x060026E6 RID: 9958 RVA: 0x0011E4BB File Offset: 0x0011C8BB
	public static bool isWorldScreenshotSession()
	{
		return WorldSession.current != null && WorldSession.current.config.sessionType == BWWorldSessionType.BWWorldSessionScreenshot;
	}

	// Token: 0x060026E7 RID: 9959 RVA: 0x0011E4DC File Offset: 0x0011C8DC
	public static bool isUserModelPreviewSession()
	{
		return WorldSession.current != null && WorldSession.current.config.sessionType == BWWorldSessionType.BWWorldSessionUserModelPreview;
	}

	// Token: 0x060026E8 RID: 9960 RVA: 0x0011E4FE File Offset: 0x0011C8FE
	public static bool isBuildingSetModelPreviewSession()
	{
		return WorldSession.current != null && WorldSession.current.config.sessionType == BWWorldSessionType.BWWorldSessionBuildingSetModelPreview;
	}

	// Token: 0x060026E9 RID: 9961 RVA: 0x0011E520 File Offset: 0x0011C920
	public static bool isCommunitySession()
	{
		return WorldSession.current != null && WorldSession.current.config.sessionType == BWWorldSessionType.BWWorldSessionCommunityPlay;
	}

	// Token: 0x060026EA RID: 9962 RVA: 0x0011E541 File Offset: 0x0011C941
	public static bool isCommunityModelPreviewSession()
	{
		return WorldSession.current != null && WorldSession.current.config.sessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview;
	}

	// Token: 0x060026EB RID: 9963 RVA: 0x0011E564 File Offset: 0x0011C964
	public static bool canShowLeaderboard()
	{
		bool flag = WorldSession.current != null && WorldSession.current.hasWinCondition;
		return flag && WorldSession.isCommunitySession();
	}

	// Token: 0x17000184 RID: 388
	// (get) Token: 0x060026EC RID: 9964 RVA: 0x0011E59F File Offset: 0x0011C99F
	public static bool AllowInput
	{
		get
		{
			return WorldSession.current != null && !WorldSession.current.blockInput;
		}
	}

	// Token: 0x060026ED RID: 9965 RVA: 0x0011E5BC File Offset: 0x0011C9BC
	private void LaunchWorld()
	{
		if (!this.config.hasModelCollectionInfo)
		{
			this.CompleteWorldLoad();
		}
		else if (!Blocksworld.modelCollection.modelLoadInProgress)
		{
			this.CompleteWorldLoad();
		}
		else
		{
			BWLog.Info("waiting for models to load");
		}
	}

	// Token: 0x060026EE RID: 9966 RVA: 0x0011E608 File Offset: 0x0011CA08
	public void OnModelLoadComplete()
	{
		this.CompleteWorldLoad();
	}

	// Token: 0x060026EF RID: 9967 RVA: 0x0011E610 File Offset: 0x0011CA10
	private void CompleteWorldLoad()
	{
		if (!this.worldLoadComplete)
		{
			this.worldLoadComplete = true;
			Blocksworld.bw.StartCoroutine(this.CoroutineCompleteLoad());
		}
	}

	// Token: 0x060026F0 RID: 9968 RVA: 0x0011E638 File Offset: 0x0011CA38
	private IEnumerator CoroutineCompleteLoad()
	{
		yield return null;
		if (this.launchAction != null)
		{
			this.launchAction();
		}
		this.launchAction = null;
		MappedInput.SetMode((Blocksworld.CurrentState != State.Play) ? MappableInputMode.Build : MappableInputMode.Play);
		Blocksworld.UI.Tapedeck.RefreshRecordButtonState();
		yield return null;
		if (!this.isJumpSession)
		{
			WorldSession.platformDelegate.WorldDidFinishLoading();
		}
		yield break;
	}

	// Token: 0x060026F1 RID: 9969 RVA: 0x0011E654 File Offset: 0x0011CA54
	private void OnLoadedLocalWorld()
	{
		this.initialButtonLayout = new WorldUILayout(new WorldUILayoutParameters(TILE_BUTTON.PLAY, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT)
		{
			includeUndoRedo = true
		});
		this.statePlayButtonLayout = new WorldUILayout(new WorldUILayoutParameters(TILE_BUTTON.TOOLS, TILE_BUTTON.CAPTURE, TILE_BUTTON.RESTART, TILE_BUTTON.EXIT)
		{
			includeVRCameraToggle = this.config.hasVRCameraButtonInPlayMode
		});
		this.leaderboardStartScreenLayout = new WorldUILayout(TILE_BUTTON.EXIT);
		this.leaderboardWinScreenLayout = new WorldUILayout(TILE_BUTTON.RESTART, TILE_BUTTON.EXIT);
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.EnableWorldSave(true);
		Blocksworld.EnableBuildMode();
		Blocksworld.SetBuildPanelRightSided();
		this.sessionTitle = this.config.worldTitle;
		this.sessionUserName = this.config.worldAuthorUsername;
		this.sessionDescription = this.config.worldDescription;
		this.launchAction = delegate()
		{
			Blocksworld.SetBlocksworldState(State.Build);
			Blocksworld.UI.TabBar.SwitchToTab(TabBarTabId.Blocks);
			Scarcity.UpdateInventory(true, null);
			Blocksworld.UpdateTiles();
		};
	}

	// Token: 0x060026F2 RID: 9970 RVA: 0x0011E734 File Offset: 0x0011CB34
	private void OnLoadedRemoteWorld()
	{
		bool flag = Scarcity.puzzleInventory != null;
		if (flag)
		{
			this.OnLoadedConstructionChallenge();
			return;
		}
		this.initialButtonLayout = new WorldUILayout(new WorldUILayoutParameters
		{
			includeLikeUnlike = false
		});
		WorldUILayoutParameters worldUILayoutParameters = new WorldUILayoutParameters();
		if (this.config.jumpedFromBuildMode)
		{
			worldUILayoutParameters.mainButtons = new TILE_BUTTON[]
			{
				TILE_BUTTON.TOOLS,
				TILE_BUTTON.CAPTURE,
				TILE_BUTTON.RESTART,
				TILE_BUTTON.EXIT
			};
			worldUILayoutParameters.includeVRCameraToggle = this.config.hasVRCameraButtonInPlayMode;
		}
		else
		{
			worldUILayoutParameters.mainButtons = new TILE_BUTTON[]
			{
				TILE_BUTTON.RESTART,
				TILE_BUTTON.CAPTURE,
				TILE_BUTTON.EXIT
			};
			worldUILayoutParameters.includeLikeUnlike = false;
			worldUILayoutParameters.includeVRCameraToggle = this.config.hasVRCameraButtonInPlayMode;
		}
		this.statePlayButtonLayout = new WorldUILayout(worldUILayoutParameters);
		this.leaderboardStartScreenLayout = new WorldUILayout(TILE_BUTTON.EXIT);
		this.leaderboardWinScreenLayout = new WorldUILayout(TILE_BUTTON.RESTART, TILE_BUTTON.EXIT);
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		bool jumpedFromBuildMode = this.config.jumpedFromBuildMode;
		Blocksworld.EnableWorldSave(jumpedFromBuildMode);
		Blocksworld.DisableBuildMode();
		this.isLiked = this.config.likedByCurrentUser;
		this.sessionTitle = this.config.worldTitle;
		this.sessionUserName = this.config.worldAuthorUsername;
		this.sessionDescription = this.config.worldDescription;
		this.launchAction = delegate()
		{
			Blocksworld.bw.Play();
		};
	}

	// Token: 0x060026F3 RID: 9971 RVA: 0x0011E894 File Offset: 0x0011CC94
	private void OnLoadedPlayerProfileWorld()
	{
		this.initialButtonLayout = new WorldUILayout(new WorldUILayoutParameters(TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT, TILE_BUTTON.PROFILE_SELECT)
		{
			includeUndoRedo = true,
			includeRecord = false
		});
		this.stateCaptureButtonLayout = WorldUILayout.WorldUILayoutHidden;
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.EnableWorldSave(true);
		Blocksworld.EnableBuildMode();
		Blocksworld.SetBuildPanelRightSided();
		this.sessionTitle = this.config.worldTitle;
		this.sessionUserName = this.config.worldAuthorUsername;
		this.sessionDescription = this.config.worldDescription;
		this.launchAction = delegate()
		{
			Blocksworld.SetBlocksworldState(State.Build);
			Blocksworld.UI.SidePanel.Show();
			Blocksworld.UpdateTiles();
			Block profileCharacterBlock = ProfileBlocksterUtils.GetProfileCharacterBlock();
			Blocksworld.UI.ShowProfileSelectionScreen();
			if (!this.config.isNewProfile)
			{
				ProfileType profileCharacterType = ProfileBlocksterUtils.GetProfileCharacterType(profileCharacterBlock);
				Blocksworld.UI.ProfileSelection.ScrollToType(profileCharacterType);
			}
			if (profileCharacterBlock is BlockAnimatedCharacter)
			{
				this.profileWorldAnimatedBlockster = (profileCharacterBlock as BlockAnimatedCharacter);
			}
		};
	}

	// Token: 0x060026F4 RID: 9972 RVA: 0x0011E930 File Offset: 0x0011CD30
	private void OnLoadedPuzzleBuildMode()
	{
		bool flag = Scarcity.puzzleInventory != null;
		if (flag)
		{
			this.OnLoadedConstructionChallenge();
			return;
		}
		Blocksworld.clipboard.autoPaintMode = false;
		Blocksworld.clipboard.autoTextureMode = false;
		WorldUILayoutParameters worldUILayoutParameters;
		if (this.config.hideExitButton)
		{
			this.initialButtonLayout = new WorldUILayout(TILE_BUTTON.PLAY);
			worldUILayoutParameters = new WorldUILayoutParameters(TILE_BUTTON.RESTART);
		}
		else
		{
			this.initialButtonLayout = new WorldUILayout(TILE_BUTTON.PLAY, TILE_BUTTON.EXIT);
			worldUILayoutParameters = new WorldUILayoutParameters(TILE_BUTTON.RESTART, TILE_BUTTON.EXIT);
		}
		worldUILayoutParameters.includeVRCameraToggle = this.config.hasVRCameraButtonInPlayMode;
		this.statePlayButtonLayout = new WorldUILayout(worldUILayoutParameters);
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.EnableWorldSave(true);
		Blocksworld.EnableBuildMode();
		Blocksworld.SetBuildPanelRightSided();
		this.config.LoadPuzzleGAFsAndRewards();
		this.launchAction = delegate()
		{
			Blocksworld.UI.Tapedeck.Ghost(true);
			Blocksworld.SetBlocksworldState(State.Build);
			Blocksworld.UI.SidePanel.Show();
			Blocksworld.UpdateTiles();
			Tutorial.Start();
		};
		this.sessionTitle = this.config.worldTitle;
		this.sessionUserName = "Blocksworld";
		this.sessionDescription = this.config.worldDescription;
	}

	// Token: 0x060026F5 RID: 9973 RVA: 0x0011EA40 File Offset: 0x0011CE40
	private void OnLoadedConstructionChallenge()
	{
		this.initialButtonLayout = new WorldUILayout(TILE_BUTTON.PLAY, TILE_BUTTON.EXIT);
		this.statePlayButtonLayout = new WorldUILayout(TILE_BUTTON.TOOLS, TILE_BUTTON.EXIT);
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.EnableWorldSave(false);
		Blocksworld.EnableBuildMode();
		Blocksworld.SetBuildPanelRightSided();
		this.config.LoadPuzzleGAFsAndRewards();
		this.launchAction = delegate()
		{
			Blocksworld.SetBlocksworldState(State.Build);
			Blocksworld.UI.SidePanel.Show();
			Blocksworld.UpdateTiles();
			Tutorial.Start();
		};
		this.sessionTitle = this.config.worldTitle;
		this.sessionUserName = "Blocksworld";
		this.sessionDescription = this.config.worldDescription;
	}

	// Token: 0x060026F6 RID: 9974 RVA: 0x0011EAE0 File Offset: 0x0011CEE0
	private void OnLoadedPuzzlePlayMode()
	{
		this.initialButtonLayout = new WorldUILayout(new WorldUILayoutParameters(TILE_BUTTON.RESTART, TILE_BUTTON.EXIT)
		{
			includeVRCameraToggle = this.config.hasVRCameraButtonInPlayMode
		});
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.EnableWorldSave(false);
		Blocksworld.DisableBuildMode();
		this.config.LoadPuzzleGAFsAndRewards();
		this.launchAction = delegate()
		{
			Tutorial.Start(true);
			Blocksworld.bw.Play();
		};
		this.sessionTitle = this.config.worldTitle;
		this.sessionUserName = "Blocksworld";
		this.sessionDescription = this.config.worldDescription;
	}

	// Token: 0x060026F7 RID: 9975 RVA: 0x0011EB84 File Offset: 0x0011CF84
	private void OnLoadedUserModelPreview()
	{
		WorldUILayoutParameters worldUILayoutParameters = new WorldUILayoutParameters(TILE_BUTTON.PAUSE, TILE_BUTTON.RESTART, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT);
		worldUILayoutParameters.includeTitleBar = true;
		worldUILayoutParameters.includeVRCameraToggle = this.config.hasVRCameraButtonInPlayMode;
		worldUILayoutParameters.includeBuildModelButton = true;
		worldUILayoutParameters.titleBarText = this.config.previewModelTitle;
		worldUILayoutParameters.titleBarSubtitle = this.config.GetFormattedPreviewModelAuthorStr();
		worldUILayoutParameters.titleBarHasCoinBalance = true;
		worldUILayoutParameters.titleBarCoinBalance = this.config.currentUserCoins;
		this.initialButtonLayout = new WorldUILayout(worldUILayoutParameters);
		this.statePauseButtonLayout = new WorldUILayout(new WorldUILayoutParameters(worldUILayoutParameters)
		{
			mainButtons = new TILE_BUTTON[]
			{
				TILE_BUTTON.PLAY,
				TILE_BUTTON.RESTART,
				TILE_BUTTON.CAPTURE,
				TILE_BUTTON.EXIT
			}
		});
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.DisableBuildMode();
		Blocksworld.EnableWorldSave(false);
		this.launchAction = delegate()
		{
			this.InsertPreviewModel();
			if (this.config.previewModel != null)
			{
				Blocksworld.bw.Play();
			}
			else
			{
				WorldSession.platformDelegate.WorldWillQuit("corrupt_model_source");
				Blocksworld.CleanupAndQuitToMenu();
			}
		};
		this.sessionTitle = this.config.previewModelTitle;
		this.sessionUserName = this.config.previewModelAuthorUsername;
		this.sessionDescription = this.config.worldDescription;
	}

	// Token: 0x060026F8 RID: 9976 RVA: 0x0011EC88 File Offset: 0x0011D088
	private void OnLoadedBuildingSetModelPreview()
	{
		WorldUILayoutParameters worldUILayoutParameters = new WorldUILayoutParameters(TILE_BUTTON.PAUSE, TILE_BUTTON.RESTART, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT);
		worldUILayoutParameters.includeTitleBar = true;
		worldUILayoutParameters.includeVRCameraToggle = this.config.hasVRCameraButtonInPlayMode;
		worldUILayoutParameters.includePurchasedBanner = this.config.previewModelPurchased;
		worldUILayoutParameters.titleBarText = this.config.previewModelTitle;
		worldUILayoutParameters.titleBarSubtitle = this.config.GetFormattedPreviewModelAuthorStr();
		worldUILayoutParameters.titleBarHasCoinBalance = true;
		worldUILayoutParameters.titleBarCoinBalance = this.config.currentUserCoins;
		this.initialButtonLayout = new WorldUILayout(worldUILayoutParameters);
		this.statePauseButtonLayout = new WorldUILayout(new WorldUILayoutParameters(worldUILayoutParameters)
		{
			mainButtons = new TILE_BUTTON[]
			{
				TILE_BUTTON.PLAY,
				TILE_BUTTON.RESTART,
				TILE_BUTTON.CAPTURE,
				TILE_BUTTON.EXIT
			}
		});
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.DisableBuildMode();
		Blocksworld.EnableWorldSave(false);
		this.launchAction = delegate()
		{
			this.InsertPreviewModel();
			if (this.config.previewModel != null)
			{
				Blocksworld.bw.Play();
			}
			else
			{
				WorldSession.platformDelegate.WorldWillQuit("corrupt_model_source");
				Blocksworld.CleanupAndQuitToMenu();
			}
		};
		this.sessionTitle = this.config.previewModelTitle;
		this.sessionUserName = this.config.previewModelAuthorUsername;
		this.sessionDescription = this.config.worldDescription;
	}

	// Token: 0x060026F9 RID: 9977 RVA: 0x0011ED94 File Offset: 0x0011D194
	private void OnLoadedCommunityModelPreview()
	{
		WorldUILayoutParameters worldUILayoutParameters = new WorldUILayoutParameters(TILE_BUTTON.PAUSE, TILE_BUTTON.RESTART, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT);
		worldUILayoutParameters.includeTitleBar = true;
		worldUILayoutParameters.includeVRCameraToggle = this.config.hasVRCameraButtonInPlayMode;
		worldUILayoutParameters.includePurchasedBanner = this.config.previewModelPurchased;
		worldUILayoutParameters.includeBuildModelButton = (this.config.previewModelPurchased && !this.config.previewModelLocked);
		worldUILayoutParameters.buyModelPrice = this.config.previewModelSellingPrice;
		worldUILayoutParameters.titleBarText = this.config.previewModelTitle;
		worldUILayoutParameters.titleBarSubtitle = this.config.GetFormattedPreviewModelAuthorStr();
		worldUILayoutParameters.titleBarHasCoinBalance = true;
		worldUILayoutParameters.titleBarCoinBalance = this.config.currentUserCoins;
		this.initialButtonLayout = new WorldUILayout(worldUILayoutParameters);
		this.statePauseButtonLayout = new WorldUILayout(new WorldUILayoutParameters(worldUILayoutParameters)
		{
			mainButtons = new TILE_BUTTON[]
			{
				TILE_BUTTON.PLAY,
				TILE_BUTTON.RESTART,
				TILE_BUTTON.CAPTURE,
				TILE_BUTTON.EXIT
			}
		});
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		Blocksworld.DisableBuildMode();
		Blocksworld.EnableWorldSave(false);
		this.previewModelImage = WorldSession.platformDelegate.GetCurrentlyLoadedPreviewModelImage();
		if (this.previewModelImage == null)
		{
			BWLog.Info("Failed to get image for preview model");
		}
		this.launchAction = delegate()
		{
			this.InsertPreviewModel();
			if (this.config.previewModel != null)
			{
				Blocksworld.bw.Play();
			}
			else
			{
				WorldSession.platformDelegate.WorldWillQuit("corrupt_model_source");
				Blocksworld.CleanupAndQuitToMenu();
			}
		};
		this.sessionTitle = this.config.previewModelTitle;
		this.sessionUserName = this.config.previewModelAuthorUsername;
		this.sessionDescription = this.config.worldDescription;
	}

	// Token: 0x060026FA RID: 9978 RVA: 0x0011EF04 File Offset: 0x0011D304
	private void OnLoadedScreenshotSession()
	{
		this.initialButtonLayout = new WorldUILayout(TILE_BUTTON.RESTART, TILE_BUTTON.CAPTURE_SETUP, TILE_BUTTON.EXIT);
		this.captureSetupButtonLayout = new WorldUILayout(TILE_BUTTON.RESTART, TILE_BUTTON.PLAY, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT);
		this.stateCaptureButtonLayout = WorldUILayout.WorldUILayoutHidden;
		Blocksworld.DisableBuildMode();
		Blocksworld.SetMusicEnabled(Blocksworld.IsMusicEnabledForState());
		this.launchAction = delegate()
		{
			Blocksworld.bw.Play();
		};
		this.sessionTitle = this.config.worldTitle;
		this.sessionUserName = this.config.worldAuthorUsername;
		this.sessionDescription = this.config.worldDescription;
	}

	// Token: 0x060026FB RID: 9979 RVA: 0x0011EFA0 File Offset: 0x0011D3A0
	public bool BlockIsAvailable(string blockItemIdentifier)
	{
		return WorldSession.current.config.blocksInventory == null || WorldSession.current.config.blocksInventory.ContainsBlockItemIdentifier(blockItemIdentifier);
	}

	// Token: 0x060026FC RID: 9980 RVA: 0x0011EFD0 File Offset: 0x0011D3D0
	private void InsertPreviewModel()
	{
		this.config.previewModel = Blocksworld.InsertModel(this.config.previewModelSource, this.config.previewModelPositionX, this.config.previewModelPositionY, this.config.previewModelPositionZ);
		if (this.config.previewModel != null)
		{
			foreach (Block block in this.config.previewModel)
			{
				block.Update();
			}
			Bounds bounds = Util.ComputeBoundsWithSize(this.config.previewModel, true);
			Vector3 normalized = Blocksworld.cameraTransform.position.normalized;
			float d = Mathf.Max(3f * bounds.extents.magnitude, 10f);
			Blocksworld.cameraTransform.position = bounds.center + d * normalized;
			Blocksworld.cameraTransform.LookAt(bounds.center);
			Blocksworld.blocksworldCamera.Follow(this.config.previewModel[0]);
		}
	}

	// Token: 0x060026FD RID: 9981 RVA: 0x0011F110 File Offset: 0x0011D510
	internal void TriggerModelTutorial()
	{
		List<List<List<Tile>>> tilesLists = ModelUtils.ParseModelString(this.config.previewModelSource);
		Blocksworld.SetupTutorialGAFs(tilesLists);
		Blocksworld.EnableBuildMode();
		Blocksworld.SetBuildPanelRightSided();
		Blocksworld.UI.Overlay.HidePurchasedBanner();
		Action completionHandler = delegate()
		{
			Tutorial.Start();
			WorldUILayout worldUILayout = new WorldUILayout(TILE_BUTTON.PLAY, TILE_BUTTON.EXIT);
			worldUILayout.Apply();
			this.inModelTutorial = true;
		};
		Sound.PlayOneShotSound("Button Stop", 1f);
		Blocksworld.bw.Stop(false, true);
		List<Block> modelBlocks = BWSceneManager.NonTerrainBlocks();
		ModelTutorializeUtils.PrepareForStepByStepTutorial(modelBlocks, new ModelTutorializeUtils.StepByStepTutorializeOptions
		{
			waitTimePerBlock = 0f
		}, completionHandler);
	}

	// Token: 0x060026FE RID: 9982 RVA: 0x0011F194 File Offset: 0x0011D594
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
		foreach (Chunk chunk in hashSet2)
		{
			chunk.Destroy(false);
		}
		foreach (Block block2 in hashSet)
		{
			BWSceneManager.RemoveBlock(block2);
			block2.Destroy();
		}
		this.InsertPreviewModel();
	}

	// Token: 0x060026FF RID: 9983 RVA: 0x0011F298 File Offset: 0x0011D698
	internal void EndModelTutorial()
	{
		Tutorial.Stop(false);
		Tutorial.blocks.Clear();
		this.inModelTutorial = false;
		Tutorial.state = TutorialState.None;
		Blocksworld.UI.ShowTitleBar();
		Blocksworld.UI.ShowBuildModelButton();
		if (this.config.previewModelPurchased)
		{
			Blocksworld.UI.Overlay.ShowPurchasedBanner();
		}
		Blocksworld.DisableBuildMode();
	}

	// Token: 0x06002700 RID: 9984 RVA: 0x0011F2FC File Offset: 0x0011D6FC
	public void OnPlay()
	{
		Blocksworld.UI.SidePanel.Hide();
		Blocksworld.UI.Dialog.CloseActiveDialog();
		if (this.inModelTutorial)
		{
			this.EndModelTutorial();
			this.RestorePreviewModel();
		}
		if (this.statePlayButtonLayout != null)
		{
			this.statePlayButtonLayout.Apply();
		}
		else
		{
			this.initialButtonLayout.Apply();
		}
		Blocksworld.UI.SetControlsCanvasVisible(!Blocksworld.IsVRCameraMode());
	}

	// Token: 0x06002701 RID: 9985 RVA: 0x0011F376 File Offset: 0x0011D776
	public void OnRestart()
	{
		Blocksworld.UI.SidePanel.Hide();
		if (this.statePlayButtonLayout != null)
		{
			this.statePlayButtonLayout.Apply();
		}
		else
		{
			this.initialButtonLayout.Apply();
		}
		this.ResetAutoSaveCounter();
	}

	// Token: 0x06002702 RID: 9986 RVA: 0x0011F3B4 File Offset: 0x0011D7B4
	public void OnStop()
	{
		if (!Blocksworld.bw.forcePlayMode)
		{
			Blocksworld.UI.SidePanel.Show();
		}
		if (this.initialButtonLayout != null)
		{
			this.initialButtonLayout.Apply();
		}
		this.ResetAutoSaveCounter();
		this.jumpToWorldTriggered = false;
		this.jumpToWorldInfo = null;
	}

	// Token: 0x06002703 RID: 9987 RVA: 0x0011F409 File Offset: 0x0011D809
	public void OnShare()
	{
		if (this.stateShareButtonLayout != null)
		{
			this.stateShareButtonLayout.Apply();
		}
		else
		{
			this.initialButtonLayout.Apply();
		}
	}

	// Token: 0x06002704 RID: 9988 RVA: 0x0011F431 File Offset: 0x0011D831
	public void OnCapture()
	{
		if (this.stateCaptureButtonLayout != null)
		{
			this.stateCaptureButtonLayout.Apply();
		}
		else
		{
			this.initialButtonLayout.Apply();
		}
	}

	// Token: 0x06002705 RID: 9989 RVA: 0x0011F459 File Offset: 0x0011D859
	public void OnCompleteBuild()
	{
		if (this.config.isModelPreview)
		{
			this.EndModelTutorial();
			Blocksworld.bw.Play();
		}
		else
		{
			this.config.puzzleBuildModeCompleted = true;
			WorldSession.platformDelegate.CompletePuzzleBuild();
		}
	}

	// Token: 0x06002706 RID: 9990 RVA: 0x0011F498 File Offset: 0x0011D898
	public void OnWinGame()
	{
		if (WorldSession.isPuzzleBuildSession() || WorldSession.isPuzzlePlaySession())
		{
			if (!this.config.puzzleHasRewardGAFs && this.config.puzzleIsPurchased)
			{
				WorldSession.platformDelegate.CompletePuzzlePlay();
				return;
			}
			if (!this.config.puzzleIsPurchased)
			{
				Blocksworld.ShowSetPurchasePrompt(this.config.puzzleRewardGAFsJsonStr, this.config.puzzleSetTitle, this.config.puzzleBuildingSetId, this.config.puzzleBuildingSetPrice);
			}
			else if (!this.config.puzzlePlayModeCompleted && this.config.puzzleBuildModeCompleted)
			{
				Blocksworld.VisualizeBlockReward(this.config.puzzleRewardGAFsJsonStr);
			}
			this.config.puzzlePlayModeCompleted = true;
		}
	}

	// Token: 0x06002707 RID: 9991 RVA: 0x0011F566 File Offset: 0x0011D966
	public void OnPurchaseBuildingSetFromPuzzle()
	{
		Blocksworld.winIsWaiting = true;
		Blocksworld.SetBlocksworldState(State.Play);
		Tutorial.ResetState();
		Blocksworld.waitForSetPurchase = false;
		this.config.puzzleIsPurchased = true;
		Blocksworld.VisualizeBlockReward(this.config.puzzleRewardGAFsJsonStr);
	}

	// Token: 0x06002708 RID: 9992 RVA: 0x0011F59B File Offset: 0x0011D99B
	public void OnRewardVisualizationComplete()
	{
		if (this.sessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview)
		{
			WorldSession.Quit();
			return;
		}
		WorldSession.platformDelegate.CompletePuzzlePlay();
	}

	// Token: 0x06002709 RID: 9993 RVA: 0x0011F5BA File Offset: 0x0011D9BA
	public void ReplayKitViewControllerDidAppear()
	{
		this.Pause();
		Blocksworld.musicPlayer.SetVolumeMultiplier(0f);
	}

	// Token: 0x0600270A RID: 9994 RVA: 0x0011F5D1 File Offset: 0x0011D9D1
	public void ReplayKitViewControllerDidDisappear()
	{
		this.Unpause();
		Blocksworld.musicPlayer.SetVolumeMultiplier(1f);
		WorldUILayout.currentLayout.Apply();
		Blocksworld.UI.Tapedeck.RefreshRecordButtonState();
	}

	// Token: 0x0600270B RID: 9995 RVA: 0x0011F601 File Offset: 0x0011DA01
	public void ToggleWorldUpvoted()
	{
		this.isLiked = !this.isLiked;
		WorldUILayout.currentLayout.Apply();
		WorldSession.platformDelegate.SetWorldUpvoted(this.isLiked);
	}

	// Token: 0x0600270C RID: 9996 RVA: 0x0011F62C File Offset: 0x0011DA2C
	public void ConfirmModelPurchase()
	{
		Sound.PlayOneShotSound("Button Generic", 1f);
		Blocksworld.UI.Dialog.ShowModelPurchaseConfirmation(this.config.previewModelTitle, this.config.previewModelSellingPrice, this.previewModelImage);
	}

	// Token: 0x0600270D RID: 9997 RVA: 0x0011F668 File Offset: 0x0011DA68
	public void AddModelToCart()
	{
		WorldSession.QuitWithDeepLink("add_model_to_cart");
	}

	// Token: 0x0600270E RID: 9998 RVA: 0x0011F674 File Offset: 0x0011DA74
	public bool TakeScreenshot()
	{
		bool flag = WorldSession.isWorldScreenshotSession() || WorldSession.isProfileBuildSession();
		if (flag)
		{
			string label = (!WorldSession.isProfileBuildSession()) ? this.config.worldTitle : "Profile Picture";
			byte[] array = Util.RenderScreenshotForCoverImage();
			if (array == null)
			{
				return false;
			}
			WorldSession.platformDelegate.SendScreenShot(array, label);
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
			WorldSession.platformDelegate.ImageWriteToSavedPhotosAlbum(array2);
		}
		return true;
	}

	// Token: 0x0600270F RID: 9999 RVA: 0x0011F714 File Offset: 0x0011DB14
	public void EnterEditorShareMode()
	{
		this.initialButtonLayout = (this.statePlayButtonLayout = new WorldUILayout(TILE_BUTTON.PAUSE, TILE_BUTTON.EXIT));
		this.statePauseButtonLayout = new WorldUILayout(TILE_BUTTON.PLAY, TILE_BUTTON.CAPTURE, TILE_BUTTON.EXIT);
		this.stateShareButtonLayout = new WorldUILayout(TILE_BUTTON.PLAY, TILE_BUTTON.EXIT);
		Blocksworld.UI.SidePanel.Hide();
		this.statePauseButtonLayout.Apply();
	}

	// Token: 0x06002710 RID: 10000 RVA: 0x0011F76F File Offset: 0x0011DB6F
	public void EnterLeaderboardStartScreen()
	{
		WorldSession.current.Pause();
		Blocksworld.lockInput = true;
		if (this.leaderboardStartScreenLayout != null)
		{
			this.leaderboardStartScreenLayout.Apply();
		}
	}

	// Token: 0x06002711 RID: 10001 RVA: 0x0011F797 File Offset: 0x0011DB97
	public void ExitLeaderboardStartScreen()
	{
		WorldSession.current.Unpause();
		Blocksworld.lockInput = false;
		if (this.statePlayButtonLayout != null)
		{
			this.statePlayButtonLayout.Apply();
		}
		else
		{
			this.initialButtonLayout.Apply();
		}
	}

	// Token: 0x06002712 RID: 10002 RVA: 0x0011F7CF File Offset: 0x0011DBCF
	public void EnterLeaderboardWinScreen()
	{
		Blocksworld.lockInput = true;
		if (this.leaderboardWinScreenLayout != null)
		{
			this.leaderboardWinScreenLayout.Apply();
		}
	}

	// Token: 0x06002713 RID: 10003 RVA: 0x0011F7F0 File Offset: 0x0011DBF0
	public void EnterScreenCaptureSetup()
	{
		if (Blocksworld.CurrentState != State.Play)
		{
			BWLog.Warning("trying to enter screen capture setup from wrong state: " + Blocksworld.CurrentState);
			return;
		}
		this.PausePlay();
		Blocksworld.UI.SetControlsCanvasVisible(false);
		if (this.captureSetupButtonLayout != null)
		{
			this.captureSetupButtonLayout.Apply();
		}
		Blocksworld.SetBlocksworldState(State.FrameCapture);
		MappedInput.SetMode(MappableInputMode.Build);
	}

	// Token: 0x06002714 RID: 10004 RVA: 0x0011F858 File Offset: 0x0011DC58
	public void ExitScreenCaptureSetup()
	{
		if (Blocksworld.CurrentState != State.FrameCapture)
		{
			BWLog.Warning("trying to exit screen capture setup from wrong state: " + Blocksworld.CurrentState);
			return;
		}
		this.UnpausePlay();
		Blocksworld.UI.SetControlsCanvasVisible(true);
		Blocksworld.SetBlocksworldState(State.Play);
		MappedInput.SetMode(MappableInputMode.Play);
	}

	// Token: 0x06002715 RID: 10005 RVA: 0x0011F8A7 File Offset: 0x0011DCA7
	public static void PauseCurrentSession()
	{
		if (WorldSession.current == null)
		{
			return;
		}
		WorldSession.current.Pause();
	}

	// Token: 0x06002716 RID: 10006 RVA: 0x0011F8BE File Offset: 0x0011DCBE
	public static void UnpauseCurrentSession()
	{
		if (WorldSession.current == null)
		{
			return;
		}
		WorldSession.current.Unpause();
	}

	// Token: 0x06002717 RID: 10007 RVA: 0x0011F8D5 File Offset: 0x0011DCD5
	public void PauseButtonPressed()
	{
		this.Pause();
		if (this.statePauseButtonLayout != null)
		{
			this.statePauseButtonLayout.Apply();
		}
		else
		{
			this.initialButtonLayout.Apply();
		}
	}

	// Token: 0x06002718 RID: 10008 RVA: 0x0011F904 File Offset: 0x0011DD04
	public void Pause()
	{
		if (Blocksworld.CurrentState == State.Paused || Blocksworld.CurrentState == State.EditTile || Blocksworld.CurrentState == State.WaitForOption)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Play)
		{
			this.PausePlay();
		}
		Blocksworld.UI.SetControlsCanvasVisible(false);
		this.sessionUnpauseState = Blocksworld.CurrentState;
		Blocksworld.SetBlocksworldState(State.Paused);
	}

	// Token: 0x06002719 RID: 10009 RVA: 0x0011F964 File Offset: 0x0011DD64
	public void Unpause()
	{
		if (Blocksworld.CurrentState != State.Paused)
		{
			return;
		}
		if (this.sessionUnpauseState == State.Play)
		{
			this.UnpausePlay();
		}
		Blocksworld.blocksworldCamera.SetCameraStill(false);
		Blocksworld.lockInput = false;
		Blocksworld.UI.SetControlsCanvasVisible(true);
		Blocksworld.SetBlocksworldState(this.sessionUnpauseState);
	}

	// Token: 0x0600271A RID: 10010 RVA: 0x0011F9B7 File Offset: 0x0011DDB7
	private void PausePlay()
	{
		BWSceneManager.PauseBlocks();
		VisualEffect.PauseVfxs();
		Blocksworld.weather.Pause();
	}

	// Token: 0x0600271B RID: 10011 RVA: 0x0011F9CD File Offset: 0x0011DDCD
	private void UnpausePlay()
	{
		BWSceneManager.UnpauseBlocks();
		VisualEffect.ResumeVfxs();
		Blocksworld.weather.Resume();
	}

	// Token: 0x0600271C RID: 10012 RVA: 0x0011F9E3 File Offset: 0x0011DDE3
	public static void Quit()
	{
		WorldSession.QuitWithDeepLink(null);
	}

	// Token: 0x0600271D RID: 10013 RVA: 0x0011F9EC File Offset: 0x0011DDEC
	public static void QuitWithDeepLink(string deepLinkStr)
	{
		if (WorldSession._platformDelegate.ScreenRecordingInProgress())
		{
			WorldSession._platformDelegate.StopRecordingScreen();
		}
		WorldSession.current.Unpause();
		WorldSession.current._deepLinkOnQuit = deepLinkStr;
		Blocksworld.musicPlayer.Unload();
		WorldSession.current.exitTriggered = true;
	}

	// Token: 0x0600271E RID: 10014 RVA: 0x0011FA3C File Offset: 0x0011DE3C
	public bool WorldTeleportHasSource()
	{
		return this.jumpToWorldInfo != null && !this.jumpToWorldInfo.HasWorldSource();
	}

	// Token: 0x0600271F RID: 10015 RVA: 0x0011FA5C File Offset: 0x0011DE5C
	public bool TriggerJumpToWorld(string worldId, float delay = 0f)
	{
		if (this.jumpToWorldTriggered)
		{
			return false;
		}
		if (string.IsNullOrEmpty(worldId))
		{
			return false;
		}
		WorldInfo worldWithId = this.availableTeleportWorlds.GetWorldWithId(worldId);
		if (worldWithId == null)
		{
			BWLog.Error("Trying to jump to unloaded world info");
			return false;
		}
		this.jumpToWorldTriggered = true;
		this.jumpToWorldCountdown = delay;
		this.jumpToWorldInfo = worldWithId;
		this.jumpToWorldInfo.LoadWorldSourceForTeleport();
		return true;
	}

	// Token: 0x06002720 RID: 10016 RVA: 0x0011FAC4 File Offset: 0x0011DEC4
	public static void Save()
	{
		if (!Blocksworld.worldSaveEnabled)
		{
			return;
		}
		if (WorldSession.isProfileBuildSession())
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
			WorldSession.current.worldSourceJsonStr = Blocksworld.bw.Serialize(true);
			string avatarSource = Blocksworld.bw.ExtractProfileWorldAvatarString();
			WorldSession.platformDelegate.SetProfileWorldData(WorldSession.current.worldSourceJsonStr, avatarSource, WorldSession.current.config.profileGender);
		}
		else
		{
			Tutorial.BeforeSave();
			if (WorldSession.current.config.jumpedFromBuildMode)
			{
				if (WorldSession.jumpRestoreConfig == null)
				{
					BWLog.Error("Can't save original build mode world from jump world");
					return;
				}
				WorldSession.current.worldSourceJsonStr = WorldSession.jumpRestoreConfig.worldSourceJsonStr;
				WorldSession.current.hasWinCondition = WorldSession.jumpRestoreConfig.worldHasWinCondition.GetValueOrDefault();
				WorldSession.current.config.worldScreenshotTakenManually = WorldSession.jumpRestoreConfig.worldScreenshotTakenManually;
			}
			else
			{
				WorldSession.current.worldSourceJsonStr = Blocksworld.bw.Serialize(true);
				WorldSession.current.hasWinCondition = Blocksworld.HasWinCondition();
			}
			if (WorldSession.current.config.worldScreenshotTakenManually)
			{
				WorldSession.platformDelegate.SaveCurrentWorldData();
			}
			else
			{
				byte[] screenshotImageData = WorldSession.getScreenshotImageData();
				WorldSession.platformDelegate.SaveCurrentWorldDataWithScreenshot(screenshotImageData);
			}
			WorldSession.current.ResetAutoSaveCounter();
			Tutorial.AfterSave();
		}
	}

	// Token: 0x06002721 RID: 10017 RVA: 0x0011FC3C File Offset: 0x0011E03C
	public static void FastSave()
	{
		string text = Blocksworld.bw.Serialize(true);
		if (WorldSession.current.WorldSourceEquals(text))
		{
			WorldSession.current.ResetAutoSaveCounter();
			return;
		}
		Tutorial.BeforeSave();
		WorldSession.current.worldSourceJsonStr = text;
		WorldSession.current.hasWinCondition = Blocksworld.HasWinCondition();
		WorldSession.current.config.metaData = WorldMetrics.GetMetaData();
		WorldSession.platformDelegate.SaveCurrentWorldData();
		WorldSession.current.ResetAutoSaveCounter();
		Tutorial.AfterSave();
	}

	// Token: 0x06002722 RID: 10018 RVA: 0x0011FCBC File Offset: 0x0011E0BC
	public static void FastSaveAutoUpdate()
	{
		if (WorldSession.current == null)
		{
			return;
		}
		if (WorldSession.current.isJumpSession)
		{
			return;
		}
		if (WorldSession.current.config.autoSaveInterval > 0f && Time.time > WorldSession.current.scheduledAutoSave)
		{
			BWLog.Info("Auto saving...");
			WorldSession.FastSave();
		}
	}

	// Token: 0x06002723 RID: 10019 RVA: 0x0011FD20 File Offset: 0x0011E120
	public static byte[] getScreenshotImageData()
	{
		Camera mainCamera = Blocksworld.mainCamera;
		Vector3 position = Blocksworld.cameraTransform.position;
		Quaternion rotation = Blocksworld.cameraTransform.rotation;
		bool show = TBox.IsShowing();
		TBox.Show(false);
		Blocksworld.blocksworldCamera.SetReticleEnabled(false);
		Blocksworld.bw.StatePlayUpdate();
		byte[] result = Util.RenderScreenshotForCoverImage();
		TBox.Show(show);
		Blocksworld.blocksworldCamera.SetReticleEnabled(true);
		Blocksworld.cameraTransform.position = position;
		Blocksworld.cameraTransform.rotation = rotation;
		return result;
	}

	// Token: 0x06002724 RID: 10020 RVA: 0x0011FD9A File Offset: 0x0011E19A
	public static void LoadClipboard()
	{
		WorldSession.platformDelegate.LoadClipboard();
	}

	// Token: 0x06002725 RID: 10021 RVA: 0x0011FDA6 File Offset: 0x0011E1A6
	public static void SaveClipboard(string clipboardJsonStr)
	{
		WorldSession.platformDelegate.SaveClipboard(clipboardJsonStr);
	}

	// Token: 0x06002726 RID: 10022 RVA: 0x0011FDB3 File Offset: 0x0011E1B3
	public static void OpenStore()
	{
		WorldSession.Save();
		WorldSession.QuitWithDeepLink("store/main");
	}

	// Token: 0x06002727 RID: 10023 RVA: 0x0011FDC4 File Offset: 0x0011E1C4
	public static void OpenStoreWithBlockItemId(TabBarTabId tabId, int blockItemId)
	{
		if (BWStandalone.Instance != null)
		{
			WorldSession.Save();
			BWStandalone.Instance.OpenStoreFromWorldWithBlockItemId(tabId, blockItemId);
			WorldSession.Quit();
		}
	}

	// Token: 0x06002728 RID: 10024 RVA: 0x0011FDEC File Offset: 0x0011E1EC
	public void PurchaseBuildingSet(int setId)
	{
		WorldSession.platformDelegate.PurchaseBuildingSet(setId);
	}

	// Token: 0x06002729 RID: 10025 RVA: 0x0011FDFC File Offset: 0x0011E1FC
	public string GetCurrentUserAvatarSource()
	{
		string text = this.config.currentUserAvatarSource;
		if (string.IsNullOrEmpty(text))
		{
			text = Resources.Load<TextAsset>("default_user_avatar_source").text;
		}
		return text;
	}

	// Token: 0x0600272A RID: 10026 RVA: 0x0011FE31 File Offset: 0x0011E231
	public void LoadAvailableTeleportWorlds()
	{
		if (this.availableTeleportWorlds == null)
		{
			this.availableTeleportWorlds = new WorldInfoList();
			if (WorldSession.isNormalBuildAndPlaySession())
			{
				this.availableTeleportWorlds.LoadCurrentUserWorlds();
			}
		}
	}

	// Token: 0x0600272B RID: 10027 RVA: 0x0011FE5E File Offset: 0x0011E25E
	public void AddToAvailableTeleportWorlds(string worldId)
	{
		if (this.availableTeleportWorlds == null)
		{
			this.LoadAvailableTeleportWorlds();
		}
		this.availableTeleportWorlds.AddInfoForWorld(worldId);
	}

	// Token: 0x0600272C RID: 10028 RVA: 0x0011FE7D File Offset: 0x0011E27D
	public void ClearAvailableTeleportWorlds()
	{
		this.availableTeleportWorlds.Clear();
		this.availableTeleportWorlds = null;
	}

	// Token: 0x0600272D RID: 10029 RVA: 0x0011FE94 File Offset: 0x0011E294
	public void UpdateLoop()
	{
		if ((this.exitTriggered || this.jumpToWorldTriggered) && !Blocksworld.modelCollection.modelSaveInProgress && !WorldSession._platformDelegate.NativeModalActive() && !Blocksworld.bw.ModelAnimationInProgress() && !ScreenshotUtils.IsBusy())
		{
			if (this.exitTriggered && !WorldSession._platformDelegate.ScreenRecordingInProgress())
			{
				if (Blocksworld.worldSessionHadVR)
				{
					WorldSession.platformDelegate.TrackAchievementIncrease("virtualnaut", 1);
				}
				if (this.config.sessionType == BWWorldSessionType.BWWorldSessionBuild || WorldSession.current.config.jumpedFromBuildMode)
				{
					if (Blocksworld.worldSessionHadBlocksterMover)
					{
						WorldSession.platformDelegate.TrackAchievementIncrease("first_steps", 1);
					}
					if (Blocksworld.worldSessionHadBlocksterSpeaker)
					{
						WorldSession.platformDelegate.TrackAchievementIncrease("first_words", 1);
					}
					if (Blocksworld.worldSessionHadHypderjumpUse)
					{
						WorldSession.platformDelegate.TrackAchievementIncrease("interdimensional_builder", 1);
					}
				}
				if (this.config.sessionType == BWWorldSessionType.BWWorldSessionCommunityPlay && Blocksworld.worldSessionCoinsCollected > 0)
				{
					WorldSession.platformDelegate.TrackAchievementIncrease("coin_collector", Blocksworld.worldSessionCoinsCollected);
				}
				if (this.config.sessionType == BWWorldSessionType.BWWorldSessionCommunityModelPreview)
				{
					WorldSession.platformDelegate.TrackAchievementIncrease("window_shopper", 1);
				}
				this.scheduledModelRefresh = 0f;
				this.exitTriggered = false;
				this.jumpToWorldTriggered = false;
				if (BWStandalone.Instance != null)
				{
					BWUserModelsDataManager.Instance.RemoveListener(new ModelsListChangedEventHandler(this.UpdateStandaloneModelCollection));
				}
				WorldSession.platformDelegate.WorldWillQuit(this._deepLinkOnQuit);
				ViewportWatchdog.StopWatching();
				Blocksworld.CleanupAndQuitToMenu();
			}
			else if (this.jumpToWorldTriggered && this.jumpToWorldInfo != null)
			{
				if (Blocksworld.CurrentState == State.Build)
				{
					this.jumpToWorldTriggered = false;
					this.jumpToWorldInfo = null;
				}
				else if (Blocksworld.CurrentState == State.Play && this.jumpToWorldInfo.HasWorldSource() && Blocksworld.TimeInCurrentState() > 1f)
				{
					if (this.jumpToWorldCountdown <= 0f)
					{
						this.exitTriggered = false;
						this.jumpToWorldTriggered = false;
						WorldSession.JumpToWorld(this.jumpToWorldInfo);
					}
					else
					{
						this.jumpToWorldCountdown -= Time.deltaTime;
					}
				}
			}
		}
		if (this.scheduledModelRefresh > 0f && Time.time > this.scheduledModelRefresh)
		{
			TileIconManager.Instance.CheckModelIcons();
			this.scheduledModelRefresh = 0f;
		}
	}

	// Token: 0x0600272E RID: 10030 RVA: 0x00120117 File Offset: 0x0011E517
	public void ResetAutoSaveCounter()
	{
		if (this.config.autoSaveInterval > 0f)
		{
			this.scheduledAutoSave = Time.time + this.config.autoSaveInterval;
		}
	}

	// Token: 0x040021EA RID: 8682
	public static WorldSession current;

	// Token: 0x040021EB RID: 8683
	public static BWWorldSessionType defaultSessionType;

	// Token: 0x040021EC RID: 8684
	public static string worldIdClipboard = string.Empty;

	// Token: 0x040021ED RID: 8685
	public static WorldSessionConfig jumpRestoreConfig;

	// Token: 0x040021EE RID: 8686
	public string sessionTitle = string.Empty;

	// Token: 0x040021EF RID: 8687
	public string sessionUserName = string.Empty;

	// Token: 0x040021F0 RID: 8688
	public string sessionDescription = string.Empty;

	// Token: 0x040021F1 RID: 8689
	public WorldSessionConfig config;

	// Token: 0x040021F2 RID: 8690
	public WorldInfoList availableTeleportWorlds;

	// Token: 0x040021F3 RID: 8691
	public BlockAnimatedCharacter profileWorldAnimatedBlockster;

	// Token: 0x040021F4 RID: 8692
	private WorldUILayout initialButtonLayout;

	// Token: 0x040021F5 RID: 8693
	private WorldUILayout statePlayButtonLayout;

	// Token: 0x040021F6 RID: 8694
	private WorldUILayout statePauseButtonLayout;

	// Token: 0x040021F7 RID: 8695
	private WorldUILayout stateCaptureButtonLayout;

	// Token: 0x040021F8 RID: 8696
	private WorldUILayout stateShareButtonLayout;

	// Token: 0x040021F9 RID: 8697
	private WorldUILayout captureSetupButtonLayout;

	// Token: 0x040021FA RID: 8698
	private WorldUILayout leaderboardStartScreenLayout;

	// Token: 0x040021FB RID: 8699
	private WorldUILayout leaderboardWinScreenLayout;

	// Token: 0x040021FC RID: 8700
	private Action launchAction;

	// Token: 0x040021FD RID: 8701
	private bool exitTriggered;

	// Token: 0x040021FE RID: 8702
	private bool jumpToWorldTriggered;

	// Token: 0x040021FF RID: 8703
	private float jumpToWorldCountdown;

	// Token: 0x04002200 RID: 8704
	private WorldInfo jumpToWorldInfo;

	// Token: 0x04002201 RID: 8705
	public bool isJumpSession;

	// Token: 0x04002202 RID: 8706
	private bool blockInput;

	// Token: 0x04002203 RID: 8707
	public bool worldLoadComplete;

	// Token: 0x04002204 RID: 8708
	private float scheduledModelRefresh;

	// Token: 0x04002205 RID: 8709
	private float scheduledAutoSave;

	// Token: 0x04002206 RID: 8710
	private bool isLiked;

	// Token: 0x04002207 RID: 8711
	private bool worldLoadInProgress;

	// Token: 0x04002208 RID: 8712
	private bool inModelTutorial;

	// Token: 0x04002209 RID: 8713
	private Texture2D previewModelImage;

	// Token: 0x0400220A RID: 8714
	private State sessionUnpauseState;

	// Token: 0x0400220B RID: 8715
	private string _deepLinkOnQuit = string.Empty;

	// Token: 0x0400220C RID: 8716
	public const string PURCHASED_MODEL = "purchasedModel";

	// Token: 0x0400220D RID: 8717
	private static WorldSessionPlatformDelegate _platformDelegate;
}
