using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Token: 0x020003B9 RID: 953
public class BWStandalone : MonoBehaviour
{
	// Token: 0x170001C3 RID: 451
	// (get) Token: 0x0600295D RID: 10589 RVA: 0x0012EB94 File Offset: 0x0012CF94
	// (set) Token: 0x0600295E RID: 10590 RVA: 0x0012EB9C File Offset: 0x0012CF9C
	public bool menuLoaded { get; private set; }

	// Token: 0x170001C4 RID: 452
	// (get) Token: 0x0600295F RID: 10591 RVA: 0x0012EBA5 File Offset: 0x0012CFA5
	// (set) Token: 0x06002960 RID: 10592 RVA: 0x0012EBAD File Offset: 0x0012CFAD
	public bool addingItemToCart { get; private set; }

	// Token: 0x170001C5 RID: 453
	// (get) Token: 0x06002961 RID: 10593 RVA: 0x0012EBB6 File Offset: 0x0012CFB6
	public static MenuOverlays Overlays
	{
		get
		{
			return BWStandalone.Instance.menuOverlays;
		}
	}

	// Token: 0x170001C6 RID: 454
	// (get) Token: 0x06002962 RID: 10594 RVA: 0x0012EBC2 File Offset: 0x0012CFC2
	public static BWShoppingCart ShoppingCart
	{
		get
		{
			return BWStandalone.Instance.shoppingCart;
		}
	}

	// Token: 0x06002963 RID: 10595 RVA: 0x0012EBCE File Offset: 0x0012CFCE
	private void Awake()
	{
		BWStandalone.Instance = this;
	}

	// Token: 0x06002964 RID: 10596 RVA: 0x0012EBD8 File Offset: 0x0012CFD8
	private IEnumerator Start()
	{
		while (BWUserDataManager.Instance == null)
		{
			yield return null;
		}
		BWLog.Info("Loading user profile..");
		BWUserDataManager.Instance.SyncCurrentUserProfile();
		BWLog.Info("Loading iAPs..");
		BWSteamIAPManager.Instance.Init();
		BWLog.Info("Loading social interaction..");
		BWUserActivityDefinition.LoadUserActivityDefinitions();
		BWUserDataManager.Instance.LoadFollowers();
		BWUserDataManager.Instance.LoadFollowedUsers();
		while (!BWSteamIAPManager.Instance.loadComplete)
		{
			yield return null;
		}
		Scene bootScene = SceneManager.GetActiveScene();
		AsyncOperation gameSceneLoader = SceneManager.LoadSceneAsync("BlocksworldGameScene", LoadSceneMode.Additive);
		BWLog.Info("Loading BlocksworldGameScene..");
		while (!gameSceneLoader.isDone)
		{
			yield return null;
		}
		this.gameScene = SceneManager.GetSceneByName("BlocksworldGameScene");
		SceneManager.SetActiveScene(this.gameScene);
		yield return new WaitForSeconds(1f);
		while (Blocksworld.isLoadingScene)
		{
			yield return null;
		}
		Blocksworld.inBackground = true;
		ViewportWatchdog.StopWatching();
		this.shoppingCart = new BWShoppingCart();
		BWUserModelsDataManager.Instance.LoadAllModelsMetadata();
		BWU2UModelDataManager.Instance.LoadCurrentUserPurchasedModels();
		AsyncOperation menuSceneLoader = SceneManager.LoadSceneAsync("MenuRoot", LoadSceneMode.Additive);
		BWLog.Info("Loading menu..");
		while (!menuSceneLoader.isDone)
		{
			yield return null;
		}
		yield return null;
		this.mainMenuScene = SceneManager.GetSceneByName("MenuRoot");
		SceneManager.SetActiveScene(this.mainMenuScene);
		foreach (GameObject gameObject in this.mainMenuScene.GetRootGameObjects())
		{
			this.menuController = gameObject.GetComponentInChildren<MainUIController>();
			if (this.menuController != null)
			{
				this.menuController.sharedUIResources.dataManager.useExampleData = false;
				this.menuController.sharedUIResources.imageManager.useExampleData = false;
				this.menuController.sharedUIResources.dataManager.Init();
				this.menuOverlays = this.menuController.GetComponent<MenuOverlays>();
				break;
			}
		}
		while (!this.menuController.hasLoadedScene)
		{
			yield return null;
		}
		this.menuLoaded = true;
		yield return new WaitForSeconds(3f);
		while (!BWUserDataManager.Instance.userProfileSyncComplete)
		{
			yield return null;
		}
		while (!BWUserModelsDataManager.Instance.localModelsLoaded || !BWU2UModelDataManager.Instance.currentUserPurchasedModelsLoaded)
		{
			yield return null;
		}
		SceneManager.UnloadSceneAsync(bootScene);
		yield break;
	}

	// Token: 0x06002965 RID: 10597 RVA: 0x0012EBF3 File Offset: 0x0012CFF3
	public void Update()
	{
		if (this.menuController != null && !this.menuController.IsLoadingScene())
		{
			MenuInputHandler.HandleInput();
		}
	}

	// Token: 0x06002966 RID: 10598 RVA: 0x0012EC1C File Offset: 0x0012D01C
	public void ReturnToMenu()
	{
		MappedInput.SetMode(MappableInputMode.Menu);
		SceneManager.SetActiveScene(this.lastMenuScene);
		if (this.onReturnToMenuLoadScene != null)
		{
			this.menuController.LoadUIScene(this.onReturnToMenuLoadScene, false, SceneTransitionStyle.None, SceneTransitionStyle.Fade);
		}
		else if (!this.menuController.IsLoadingScene())
		{
			this.menuController.Show();
			BWStandalone.Overlays.ShowLoadingOverlay(false);
			this.menuController.loadedSceneController.OnReturnToScene();
			if (!string.IsNullOrEmpty(this.onReturnToMenuAction))
			{
				this.HandleMenuUIMessage(this.onReturnToMenuAction, this.onReturnToMenuActionID, this.onReturnToMenuActionDataType, this.onReturnToMenuActionDataSubtype);
			}
			if (this.currentWorldInfo is BWLocalWorld)
			{
				this.HighlightMissingFields(this.currentWorldInfo as BWLocalWorld, false);
			}
		}
		BWStandalone.Overlays.enabled = true;
		this.onReturnToMenuLoadScene = null;
		this.onReturnToMenuAction = string.Empty;
		this.onReturnToMenuActionID = string.Empty;
		this.onReturnToMenuActionDataType = string.Empty;
		this.onReturnToMenuActionDataSubtype = string.Empty;
	}

	// Token: 0x06002967 RID: 10599 RVA: 0x0012ED25 File Offset: 0x0012D125
	public void OnUISceneLoad()
	{
		this.addingItemToCart = false;
	}

	// Token: 0x06002968 RID: 10600 RVA: 0x0012ED30 File Offset: 0x0012D130
	public bool HandleMenuUIMessage(string messageStr, string senderID, string senderDataType, string senderDataSubtype)
	{
		bool result = true;
		switch (messageStr)
		{
		case "PlayWorld":
			this.PlayWorldWithId(senderID, senderDataType, senderDataSubtype, false);
			return result;
		case "NewWorldFromTemplate":
			this.CreateNewWorldWithTemplate(senderID);
			return result;
		case "BuildLocalWorld":
			this.StartBuildModeWithLocalWorld(senderID, senderDataType, senderDataSubtype);
			return result;
		case "BuildRemoteWorld":
			this.PlayWorldWithId(senderID, senderDataType, senderDataSubtype, true);
			return result;
		case "LocalWorldScreenshotSession":
			this.StartLocalWorldScreenshotSession(senderID, senderDataType, senderDataSubtype);
			return result;
		case "ReportWorld":
			this.ShowReportWorldDialog(senderID);
			return result;
		case "ReportModel":
			this.ShowReportModelDialog(senderID);
			return result;
		case "BookmarkWorld":
			this.BookmarkWorld(senderID);
			return result;
		case "UnbookmarkWorld":
			this.UnbookmarkWorld(senderID);
			return result;
		case "LoadProfileWorld":
			this.LoadProfileWorld();
			return result;
		case "DeleteLocalWorld":
			this.DeleteLocalWorld(senderID);
			return result;
		case "PublishLocalWorld":
			this.PublishLocalWorld(senderID);
			return result;
		case "UnpublishLocalWorld":
			this.UnpublishLocalWorld(senderID);
			return result;
		case "DeleteAllLocalWorlds":
			BWUserWorldsDataManager.Instance.DeleteAll();
			return result;
		case "UserModelPreview":
			this.LoadUserModelPreview(senderID, senderDataType, senderDataSubtype);
			return result;
		case "CommunityModelPreview":
			this.LoadCommunityModelPreview(senderID, senderDataType, senderDataSubtype);
			return result;
		case "DeleteLocalModel":
			this.DeleteLocalModel(senderID);
			return result;
		case "CloneLocalWorld":
			this.CloneLocalWorld(senderID);
			return result;
		case "RevertLocalWorldChanges":
			this.RevertLocalWorldChanges(senderID, senderDataType, senderDataSubtype);
			return result;
		case "CloneLocalWorldAndPlay":
			this.CloneLocalWorldAndPlay(senderID);
			return result;
		case "SelectItemWithID":
			this.SelectItemWithID(senderID, senderDataType, senderDataSubtype);
			return result;
		case "OpenUserProfile":
			this.OpenUserProfile(senderID);
			return result;
		case "PublishLocalModel":
			this.PublishLocalModel(senderID);
			return result;
		case "UnpublishLocalModel":
			this.UnpublishLocalModel(senderID);
			return result;
		case "LockModelSource":
			this.LockModelSource(senderID);
			return result;
		case "UnlockModelSource":
			this.UnlockModelSource(senderID);
			return result;
		case "EditModelPrice":
			this.EditModelPrice(senderID);
			return result;
		case "AddBlockToCart":
			this.AddBlockItemToCart(senderID, senderDataType, senderDataSubtype, true);
			return result;
		case "DecreaseCartBlockCount":
		case "RemoveBlockFromCart":
			this.RemoveBlockItemFromCart(senderID);
			return result;
		case "IncreaseCartBlockCount":
			this.AddBlockItemToCart(senderID, senderDataType, senderDataSubtype, false);
			return result;
		case "ClearBlockFromCart":
			this.ClearBlockItemFromCart(senderID);
			return result;
		case "AddModelToCart":
			this.AddModelToCart(senderID, false, senderDataType, senderDataSubtype);
			return result;
		case "AddModelBlueprintToCart":
			this.AddModelToCart(senderID, true, senderDataType, senderDataSubtype);
			return result;
		case "ClearModelFromCart":
			this.ClearModelFromCart(senderID);
			return result;
		case "PurchaseCartContents":
			return this.shoppingCart.BuyContents();
		case "ClearCartContents":
			return this.shoppingCart.ClearContents();
		case "HighlightMissingFields":
		{
			BWLocalWorld worldWithLocalWorldID = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(senderID);
			if (worldWithLocalWorldID != null)
			{
				this.HighlightMissingFields(worldWithLocalWorldID, false);
			}
			return result;
		}
		case "LinkToIOSAccount":
			this.ShowLinkToIOSAccountDialog();
			return result;
		case "BuyCoinPack":
			this.BuyCoinPack(senderID);
			return result;
		case "FollowUser":
			BWUserDataManager.Instance.FollowUser(senderID);
			return result;
		case "UnfollowUser":
			BWUserDataManager.Instance.UnfollowUser(senderID);
			return result;
		case "ShowWorldDetail":
			this.ShowWorldDetailPanel(senderID);
			return result;
		case "ShowU2UModelDetail":
			this.ShowU2UModelDetailPanel(senderID);
			return result;
		case "ShowUserModelDetail":
			this.ShowUserModelDetailPanel(senderID);
			return result;
		}
		result = false;
		BWLog.Info("Received ui message: " + messageStr + " id " + senderID);
		return result;
	}

	// Token: 0x06002969 RID: 10601 RVA: 0x0012F2F0 File Offset: 0x0012D6F0
	public void HandleWorldExitMessage(string messageStr)
	{
		if (messageStr == "corrupt_model_source")
		{
			BWStandalone.Overlays.notifications.ShowNotification("Can't load model!");
			return;
		}
		if (messageStr == "corrupt_world_source")
		{
			BWStandalone.Overlays.notifications.ShowNotification("Can't load world!");
			return;
		}
		if (messageStr == "add_model_to_cart")
		{
			if (!string.IsNullOrEmpty(this.currentPreviewModelID))
			{
				this.AddModelToCart(this.currentPreviewModelID, false, null, null);
			}
			return;
		}
		if (messageStr == "store/main")
		{
			UISceneInfo sceneInfo = new UISceneInfo
			{
				path = "ShopMenu_Blocks",
				dataSubtype = "Blocks"
			};
			this.menuController.LoadUIScene(sceneInfo, false, SceneTransitionStyle.None, SceneTransitionStyle.Fade);
			return;
		}
		if (messageStr == "shopping_cart")
		{
			UISceneInfo sceneInfo2 = new UISceneInfo
			{
				path = "ShoppingCart"
			};
			this.menuController.LoadUIScene(sceneInfo2, false, SceneTransitionStyle.None, SceneTransitionStyle.Fade);
			return;
		}
		string[] array = messageStr.Split(new char[]
		{
			'/'
		});
		if (array.Length > 1 && array[0] == "deep-link" && array.Length == 3 && array[1] == "profile")
		{
			string dataSubtype = array[2];
			UISceneInfo sceneInfo3 = new UISceneInfo
			{
				path = "UserProfile",
				dataType = "UserProfile",
				dataSubtype = dataSubtype
			};
			this.menuController.LoadUIScene(sceneInfo3, false, SceneTransitionStyle.None, SceneTransitionStyle.Fade);
		}
	}

	// Token: 0x0600296A RID: 10602 RVA: 0x0012F46C File Offset: 0x0012D86C
	private void PlayWorldWithId(string worldID, string dataType, string dataSubtype, bool buildMode)
	{
		this.PrepareForWorldSessionLaunch();
		this.onReturnToMenuAction = "SelectItemWithID";
		this.onReturnToMenuActionID = worldID;
		this.onReturnToMenuActionDataType = dataType;
		this.onReturnToMenuActionDataSubtype = dataSubtype;
		BWProfileWorld bwprofileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
		string currentUserAvatarSource = (bwprofileWorld != null) ? bwprofileWorld.avatarSourceJsonStr : string.Empty;
		WorldSession.StartForStandaloneWithRemoteWorldId(worldID, buildMode, currentUserAvatarSource);
	}

	// Token: 0x0600296B RID: 10603 RVA: 0x0012F4CC File Offset: 0x0012D8CC
	private void ShowWorldDetailPanel(string worldID)
	{
		UIDataSource dataSource = MainUIController.Instance.dataManager.GetDataSource("RemoteWorld", worldID);
		BWStandalone.Overlays.ShowPopupWorldDetailPanel(worldID, dataSource);
	}

	// Token: 0x0600296C RID: 10604 RVA: 0x0012F4FC File Offset: 0x0012D8FC
	private void ShowU2UModelDetailPanel(string modelID)
	{
		UIDataSource dataSource = MainUIController.Instance.dataManager.GetDataSource("U2UModel", modelID);
		BWStandalone.Overlays.ShowPopupU2UModelDetailPanel(modelID, dataSource);
	}

	// Token: 0x0600296D RID: 10605 RVA: 0x0012F52C File Offset: 0x0012D92C
	private void ShowUserModelDetailPanel(string localModelID)
	{
		BWUserModel modelWithLocalId = BWUserModelsDataManager.Instance.GetModelWithLocalId(localModelID);
		modelWithLocalId.LoadSourceFromDataManager();
		string dataType = (!modelWithLocalId.isPublished) ? "CurrentUserUnpublishedModels" : "CurrentUserPublishedModels";
		UIDataSource dataSource = MainUIController.Instance.dataManager.GetDataSource(dataType, null);
		BWStandalone.Overlays.ShowPopupUserModelDetailPanel(localModelID, dataSource);
	}

	// Token: 0x0600296E RID: 10606 RVA: 0x0012F584 File Offset: 0x0012D984
	private void ShowReportWorldDialog(string worldID)
	{
		BWStandalone.Overlays.ShowPopupReportWorld(worldID);
	}

	// Token: 0x0600296F RID: 10607 RVA: 0x0012F591 File Offset: 0x0012D991
	private void ShowReportModelDialog(string modelID)
	{
		BWStandalone.Overlays.ShowPopupReportModel(modelID);
	}

	// Token: 0x06002970 RID: 10608 RVA: 0x0012F59E File Offset: 0x0012D99E
	private void BookmarkWorld(string worldID)
	{
		BWUserDataManager.Instance.BookmarkWorld(worldID);
		BWStandalone.Overlays.notifications.ShowNotification("Bookmarked World");
	}

	// Token: 0x06002971 RID: 10609 RVA: 0x0012F5C0 File Offset: 0x0012D9C0
	private void UnbookmarkWorld(string worldID)
	{
		BWUserDataManager.Instance.UpdateUIWithCurrentUserData();
		UnityAction yesAction = delegate()
		{
			BWUserDataManager.Instance.UnbookmarkWorld(worldID);
			BWStandalone.Overlays.notifications.ShowNotification("Removed From Bookmarks");
			BWUserDataManager.Instance.UpdateUIWithCurrentUserData();
		};
		BWStandalone.Overlays.ShowConfirmationDialog(BWMenuTextEnum.RemoveFromBookmarkedWorldsConfirmation, yesAction);
	}

	// Token: 0x06002972 RID: 10610 RVA: 0x0012F600 File Offset: 0x0012DA00
	private void OpenUserProfile(string userIDStr)
	{
		if (string.IsNullOrEmpty(userIDStr))
		{
			BWLog.Error("Invalid userID");
			return;
		}
		UISceneInfo uisceneInfo = new UISceneInfo();
		string b = BWUser.currentUser.userID.ToString();
		if (userIDStr == b)
		{
			uisceneInfo.path = "CurrentUserProfile";
		}
		else
		{
			uisceneInfo.path = "UserProfile";
			uisceneInfo.dataType = "UserProfile";
			uisceneInfo.dataSubtype = userIDStr;
		}
		this.menuController.LoadUIScene(uisceneInfo);
	}

	// Token: 0x06002973 RID: 10611 RVA: 0x0012F688 File Offset: 0x0012DA88
	private void StartBuildModeWithLocalWorld(string worldID, string dataType, string dataSubtype)
	{
		BWLocalWorld world = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(worldID);
		if (world == null)
		{
			BWLog.Error("Can't find local world with ID: worldID");
			return;
		}
		UnityAction completion = delegate()
		{
			this.currentWorldInfo = world;
			this.PrepareForWorldSessionLaunch();
			this.onReturnToMenuAction = "SelectItemWithID";
			this.onReturnToMenuActionID = worldID;
			this.onReturnToMenuActionDataType = dataType;
			this.onReturnToMenuActionDataSubtype = dataSubtype;
			BWProfileWorld bwprofileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
			string currentUserAvatarSource = (bwprofileWorld != null) ? bwprofileWorld.avatarSourceJsonStr : string.Empty;
			WorldSession.StartForStandaloneInBuildMode(this.currentWorldInfo.worldID, this.currentWorldInfo.title, this.currentWorldInfo.source, currentUserAvatarSource, world.screenshotTakenManually);
		};
		BWUserWorldsDataManager.Instance.LoadSourceForLocalWorld(world, completion);
	}

	// Token: 0x06002974 RID: 10612 RVA: 0x0012F704 File Offset: 0x0012DB04
	private void StartLocalWorldScreenshotSession(string localWorldID, string dataType, string dataSubtype)
	{
		BWLocalWorld world = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(localWorldID);
		if (world == null)
		{
			BWLog.Error("Can't find local world with ID: worldID");
			return;
		}
		UnityAction completion = delegate()
		{
			this.currentWorldInfo = world;
			this.PrepareForWorldSessionLaunch();
			this.onReturnToMenuAction = "SelectItemWithID";
			this.onReturnToMenuActionID = localWorldID;
			this.onReturnToMenuActionDataType = dataType;
			this.onReturnToMenuActionDataSubtype = dataSubtype;
			BWProfileWorld bwprofileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
			string currentUserAvatarSource = (bwprofileWorld != null) ? bwprofileWorld.avatarSourceJsonStr : string.Empty;
			WorldSession.StartForStandaloneWorldScreenshotSession(this.currentWorldInfo.worldID, this.currentWorldInfo.title, this.currentWorldInfo.source, currentUserAvatarSource);
		};
		BWUserWorldsDataManager.Instance.LoadSourceForLocalWorld(world, completion);
	}

	// Token: 0x06002975 RID: 10613 RVA: 0x0012F780 File Offset: 0x0012DB80
	private void LoadProfileWorld()
	{
		BWProfileWorld bwprofileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
		if (bwprofileWorld != null)
		{
			this.currentWorldInfo = bwprofileWorld;
			this.PrepareForWorldSessionLaunch();
			WorldSession.StartForStandaloneWithProfileWorld(bwprofileWorld.profileGender, bwprofileWorld.worldID, bwprofileWorld.title, bwprofileWorld.source, bwprofileWorld.avatarSourceJsonStr);
		}
	}

	// Token: 0x06002976 RID: 10614 RVA: 0x0012F7D0 File Offset: 0x0012DBD0
	private void LoadUserModelPreview(string localModelId, string dataType, string dataSubtype)
	{
		BWUserModel modelWithLocalId = BWUserModelsDataManager.Instance.GetModelWithLocalId(localModelId);
		if (modelWithLocalId == null)
		{
			BWLog.Error("No local model with id: " + localModelId);
			return;
		}
		modelWithLocalId.LoadSourceFromDataManager();
		if (!string.IsNullOrEmpty(modelWithLocalId.sourceJsonStr))
		{
			this.currentPreviewModelID = localModelId;
			this.currentWorldInfo = null;
			this.PrepareForWorldSessionLaunch();
			this.onReturnToMenuAction = "SelectItemWithID";
			this.onReturnToMenuActionID = localModelId;
			this.onReturnToMenuActionDataType = dataType;
			this.onReturnToMenuActionDataSubtype = dataSubtype;
			WorldSession.StartForStandaloneWithUserModel(modelWithLocalId);
		}
	}

	// Token: 0x06002977 RID: 10615 RVA: 0x0012F850 File Offset: 0x0012DC50
	private void LoadCommunityModelPreview(string remoteModelId, string dataType, string dataSubtype)
	{
		this.currentPreviewModelID = remoteModelId;
		this.currentWorldInfo = null;
		this.PrepareForWorldSessionLaunch();
		this.onReturnToMenuAction = "SelectItemWithID";
		this.onReturnToMenuActionID = remoteModelId;
		this.onReturnToMenuActionDataType = dataType;
		this.onReturnToMenuActionDataSubtype = dataSubtype;
		WorldSession.StartForStandaloneWithCommunityModel(remoteModelId);
	}

	// Token: 0x06002978 RID: 10616 RVA: 0x0012F88C File Offset: 0x0012DC8C
	private void PublishLocalModel(string localModelID)
	{
		if (!BWModelPublishCooldown.CanPublish())
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int priceToSkip = 0;
			BWModelPublishCooldown.CooldownRemaining(out num, out num2, out num3, out priceToSkip);
			BWStandalone.Overlays.ShowPopupSkipPublishCooldown(priceToSkip, delegate
			{
				BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v1/current_user/pay_model_cooldown");
				bwapirequestBase.AddParam("coins", priceToSkip.ToString());
				bwapirequestBase.onSuccess = delegate(JObject respJson)
				{
					if (respJson.ContainsKey("attrs_for_current_user"))
					{
						UISoundPlayer.Instance.PlayClip("shop_purchase", 1f);
						BWUser.UpdateCurrentUserAndNotifyListeners(respJson["attrs_for_current_user"]);
						if (BWModelPublishCooldown.CanPublish())
						{
							this.PublishLocalModel(localModelID);
						}
					}
				};
				bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
				{
				};
				bwapirequestBase.SendOwnerCoroutine(this);
			});
			return;
		}
		BWUserModelsDataManager.Instance.PublishModel(localModelID);
	}

	// Token: 0x06002979 RID: 10617 RVA: 0x0012F910 File Offset: 0x0012DD10
	private void UnpublishLocalModel(string localModelID)
	{
		BWUserModelsDataManager.Instance.UnpublishModel(localModelID);
	}

	// Token: 0x0600297A RID: 10618 RVA: 0x0012F91D File Offset: 0x0012DD1D
	private void DeleteLocalModel(string localModelID)
	{
		BWUserModelsDataManager.Instance.DeleteLocalModel(localModelID);
	}

	// Token: 0x0600297B RID: 10619 RVA: 0x0012F92A File Offset: 0x0012DD2A
	private void LockModelSource(string localModelID)
	{
		BWUserModelsDataManager.Instance.SetModelSourceLocked(localModelID, true);
	}

	// Token: 0x0600297C RID: 10620 RVA: 0x0012F938 File Offset: 0x0012DD38
	private void UnlockModelSource(string localModelID)
	{
		BWUserModelsDataManager.Instance.SetModelSourceLocked(localModelID, false);
	}

	// Token: 0x0600297D RID: 10621 RVA: 0x0012F948 File Offset: 0x0012DD48
	private void EditModelPrice(string localModelID)
	{
		BWUserModel modelWithLocalId = BWUserModelsDataManager.Instance.GetModelWithLocalId(localModelID);
		if (modelWithLocalId == null)
		{
			BWLog.Error("no model with ID: " + localModelID);
			return;
		}
		if (modelWithLocalId.CoinsBasePrice() < 2147483647)
		{
			BWStandalone.Overlays.ShowPopupEditModelPrice(modelWithLocalId);
		}
		else
		{
			BWStandalone.Overlays.ShowMessage("You can't sell this model!");
		}
	}

	// Token: 0x0600297E RID: 10622 RVA: 0x0012F9A8 File Offset: 0x0012DDA8
	private void CreateNewWorldWithTemplate(string templateTitle)
	{
		foreach (BWWorldTemplate bwworldTemplate in BWUser.currentUser.worldTemplates)
		{
			if (bwworldTemplate.title == templateTitle)
			{
				BWLocalWorld bwlocalWorld = BWUserWorldsDataManager.Instance.CreateNewWorldFromTemplate(bwworldTemplate);
				this.currentWorldInfo = bwlocalWorld;
				this.PrepareForWorldSessionLaunch();
				this.onReturnToMenuLoadScene = new UISceneInfo();
				this.onReturnToMenuLoadScene.path = "BuildMenu";
				this.onReturnToMenuLoadScene.parameters = new Dictionary<string, string>
				{
					{
						"LoadAction",
						"SelectItemWithID"
					},
					{
						"LoadActionID",
						bwlocalWorld.localWorldID
					}
				};
				BWProfileWorld bwprofileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
				string currentUserAvatarSource = (bwprofileWorld != null) ? bwprofileWorld.avatarSourceJsonStr : string.Empty;
				WorldSession.StartForStandaloneInBuildMode(this.currentWorldInfo.worldID, this.currentWorldInfo.title, this.currentWorldInfo.source, currentUserAvatarSource, bwlocalWorld.screenshotTakenManually);
				break;
			}
		}
	}

	// Token: 0x0600297F RID: 10623 RVA: 0x0012FAD4 File Offset: 0x0012DED4
	private void DeleteLocalWorld(string worldID)
	{
		BWUserWorldsDataManager.Instance.DeleteLocalWorld(worldID, true);
	}

	// Token: 0x06002980 RID: 10624 RVA: 0x0012FAE4 File Offset: 0x0012DEE4
	private void PublishLocalWorld(string localWorldID)
	{
		BWLocalWorld world = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(localWorldID);
		if (world == null)
		{
			BWLog.Error("No local world with id: " + localWorldID);
			return;
		}
		if (world.IsLowEffortWorld())
		{
			BWStandalone.Overlays.ShowMessage("This world is too simple to publish!");
			return;
		}
		if (!BWWorldPublishCooldown.CanPublish(world.worldID))
		{
			Debug.Log("Showing skip cooldown popup");
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int priceToSkip = 0;
			BWWorldPublishCooldown.CooldownRemaining(out num, out num2, out num3, out priceToSkip);
			BWStandalone.Overlays.ShowPopupSkipPublishCooldown(priceToSkip, delegate
			{
				BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v1/current_user/pay_world_cooldown");
				bwapirequestBase.AddParam("coins", priceToSkip.ToString());
				bwapirequestBase.onSuccess = delegate(JObject respJson)
				{
					if (respJson.ContainsKey("attrs_for_current_user"))
					{
						UISoundPlayer.Instance.PlayClip("shop_purchase", 1f);
						BWUser.UpdateCurrentUserAndNotifyListeners(respJson["attrs_for_current_user"]);
						if (BWWorldPublishCooldown.CanPublish(world.worldID))
						{
							this.PublishLocalWorld(localWorldID);
						}
					}
				};
				bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
				{
				};
				bwapirequestBase.SendOwnerCoroutine(this);
			});
			return;
		}
		if (this.HighlightMissingFields(world, true))
		{
			return;
		}
		this.menuController.loadedSceneController.ResetDetailPanelAnimation(localWorldID);
		if (world.IsPublic())
		{
			BWStandalone.Overlays.ShowConfirmationDialog("Publish changes?", delegate()
			{
				BWUserWorldsDataManager.Instance.PublishLocalWorld(localWorldID);
			});
		}
		else if (BWUserWorldsDataManager.Instance.PublishedWorldCount() == 0)
		{
			string text = MenuTextDefinitions.GetTextString(BWMenuTextEnum.PublishWorldConfirmation);
			text = text.Replace("<World Title>", world.title);
			BWStandalone.Overlays.ShowConfirmationDialog(text, delegate()
			{
				BWUserWorldsDataManager.Instance.PublishLocalWorld(localWorldID);
			});
		}
		else
		{
			BWUserWorldsDataManager.Instance.PublishLocalWorld(localWorldID);
		}
	}

	// Token: 0x06002981 RID: 10625 RVA: 0x0012FC7C File Offset: 0x0012E07C
	private bool HighlightMissingFields(BWLocalWorld world, bool showNotifications = false)
	{
		bool flag = string.IsNullOrEmpty(world.title);
		bool flag2 = string.IsNullOrEmpty(world.description);
		bool flag3 = world.categoryIDs == null || world.categoryIDs.Count == 0;
		if (flag || flag2 || flag3)
		{
			string animTrigger = (!flag) ? "Reset" : "Highlight";
			string animTrigger2 = (!flag2) ? "Reset" : "Highlight";
			string animTrigger3 = (!flag3) ? "Reset" : "Highlight";
			this.menuController.loadedSceneController.TriggerDetailPanelAnimation(world.localWorldID, "title", animTrigger);
			this.menuController.loadedSceneController.TriggerDetailPanelAnimation(world.localWorldID, "description", animTrigger2);
			this.menuController.loadedSceneController.TriggerDetailPanelAnimation(world.localWorldID, "categories", animTrigger3);
			if (showNotifications)
			{
				List<string> list = new List<string>();
				if (flag)
				{
					BWStandalone.Overlays.notifications.ShowNotification(MenuTextDefinitions.GetTextString(BWMenuTextEnum.MissingWorldTitleNotification));
				}
				if (flag2)
				{
					BWStandalone.Overlays.notifications.ShowNotification(MenuTextDefinitions.GetTextString(BWMenuTextEnum.MissingWorldDescriptionNotification));
				}
				if (flag3)
				{
					BWStandalone.Overlays.notifications.ShowNotification(MenuTextDefinitions.GetTextString(BWMenuTextEnum.MissingWorldCategoryNotification));
				}
			}
			return true;
		}
		return false;
	}

	// Token: 0x06002982 RID: 10626 RVA: 0x0012FDD0 File Offset: 0x0012E1D0
	private void SelectItemWithID(string itemID, string dataType, string dataSubtype)
	{
		if (dataType == "RemoteWorld")
		{
			this.ShowWorldDetailPanel(itemID);
		}
		else if (dataType == "U2UModel")
		{
			this.ShowU2UModelDetailPanel(itemID);
		}
		else
		{
			this.menuController.loadedSceneController.SelectItemInSceneElements(itemID, dataType, dataSubtype);
		}
	}

	// Token: 0x06002983 RID: 10627 RVA: 0x0012FE28 File Offset: 0x0012E228
	private void AddBlockItemToCart(string itemID, string dataType, string dataSubtype, bool animate)
	{
		if (this.addingItemToCart)
		{
			return;
		}
		int blockItemID = 0;
		int.TryParse(itemID, out blockItemID);
		if (blockItemID == 0)
		{
			return;
		}
		if (!animate)
		{
			this.shoppingCart.AddBlockItemPack(blockItemID, 1);
			return;
		}
		this.addingItemToCart = true;
		GameObject gameObject = this.CloneItemWithID(itemID, dataType, dataSubtype);
		if (gameObject != null)
		{
			BWStandalone.Overlays.AnimateTransform((RectTransform)gameObject.transform, this.menuController.loadedSceneController.GetShoppingCartPosition(), 0.25f, 2f, true, delegate
			{
				this.shoppingCart.AddBlockItemPack(blockItemID, 1);
				this.addingItemToCart = false;
			});
		}
	}

	// Token: 0x06002984 RID: 10628 RVA: 0x0012FEE4 File Offset: 0x0012E2E4
	private void RemoveBlockItemFromCart(string itemID)
	{
		int num = 0;
		int.TryParse(itemID, out num);
		if (num == 0)
		{
			return;
		}
		this.shoppingCart.RemoveBlockItemPack(num, 1);
	}

	// Token: 0x06002985 RID: 10629 RVA: 0x0012FF10 File Offset: 0x0012E310
	private void ClearBlockItemFromCart(string itemID)
	{
		int num = 0;
		int.TryParse(itemID, out num);
		if (num == 0)
		{
			return;
		}
		this.shoppingCart.ClearBlockItemPacks(num);
	}

	// Token: 0x06002986 RID: 10630 RVA: 0x0012FF3C File Offset: 0x0012E33C
	private void AddModelToCart(string itemID, bool blueprintOnly, string dataType, string dataSubtype)
	{
		if (this.addingItemToCart)
		{
			return;
		}
		if (this.shoppingCart.ContainsU2UModel(itemID))
		{
			BWStandalone.Overlays.ShowConfirmationDialog("This model is already in your shopping cart.", delegate()
			{
				this.menuController.LoadUIScene("ShoppingCart");
			}, "Open Cart", "Ok");
			return;
		}
		BWU2UModel model = BWU2UModelDataManager.Instance.GetCachedModel(itemID);
		if (model == null)
		{
			return;
		}
		if (model.authorId == BWUser.currentUser.userID)
		{
			BWStandalone.Overlays.ShowMessage("You can't buy your own model!");
			return;
		}
		this.addingItemToCart = true;
		GameObject gameObject = this.CloneItemWithID(itemID, dataType, dataSubtype);
		if (gameObject != null)
		{
			BWStandalone.Overlays.AnimateTransform((RectTransform)gameObject.transform, this.menuController.loadedSceneController.GetShoppingCartPosition(), 0.25f, 2f, true, delegate
			{
				UISoundPlayer.Instance.PlayClip("simple_swish_15", 0.6f);
				this.shoppingCart.AddU2UModel(model, blueprintOnly);
				this.addingItemToCart = false;
			});
		}
		else
		{
			UISoundPlayer.Instance.PlayClip("simple_swish_15", 0.6f);
			this.shoppingCart.AddU2UModel(model, blueprintOnly);
			this.addingItemToCart = false;
		}
	}

	// Token: 0x06002987 RID: 10631 RVA: 0x00130078 File Offset: 0x0012E478
	private void ClearModelFromCart(string itemID)
	{
		this.shoppingCart.ClearU2UModel(itemID);
	}

	// Token: 0x06002988 RID: 10632 RVA: 0x00130086 File Offset: 0x0012E486
	private void BuyCoinPack(string coinPackInternalIdentifier)
	{
		BWSteamIAPManager.Instance.BuyCoinPack(coinPackInternalIdentifier);
	}

	// Token: 0x06002989 RID: 10633 RVA: 0x00130093 File Offset: 0x0012E493
	private GameObject CloneItemWithID(string itemID, string dataType, string dataSubtype)
	{
		return this.menuController.loadedSceneController.CloneItemInSceneElements(itemID, dataType, dataSubtype);
	}

	// Token: 0x0600298A RID: 10634 RVA: 0x001300A8 File Offset: 0x0012E4A8
	private void UnpublishLocalWorld(string worldID)
	{
		BWUserWorldsDataManager.Instance.UnpublishLocalWorld(worldID);
	}

	// Token: 0x0600298B RID: 10635 RVA: 0x001300B5 File Offset: 0x0012E4B5
	private void CloneLocalWorld(string localWorldID)
	{
		BWUserWorldsDataManager.Instance.CloneLocalWorld(localWorldID);
	}

	// Token: 0x0600298C RID: 10636 RVA: 0x001300C4 File Offset: 0x0012E4C4
	private void CloneLocalWorldAndPlay(string localWorldID)
	{
		BWLocalWorld bwlocalWorld = BWUserWorldsDataManager.Instance.CloneLocalWorld(localWorldID);
		if (bwlocalWorld == null)
		{
			BWLog.Error("Failed to clone world with ID: " + localWorldID);
			return;
		}
		this.currentWorldInfo = bwlocalWorld;
		this.PrepareForWorldSessionLaunch();
		this.onReturnToMenuLoadScene = new UISceneInfo();
		this.onReturnToMenuLoadScene.path = "BuildMenu";
		this.onReturnToMenuLoadScene.parameters = new Dictionary<string, string>
		{
			{
				"LoadAction",
				"SelectItemWithID"
			},
			{
				"LoadActionID",
				bwlocalWorld.localWorldID
			}
		};
		BWProfileWorld bwprofileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
		string currentUserAvatarSource = (bwprofileWorld != null) ? bwprofileWorld.avatarSourceJsonStr : string.Empty;
		WorldSession.StartForStandaloneInBuildMode(this.currentWorldInfo.worldID, this.currentWorldInfo.title, this.currentWorldInfo.source, currentUserAvatarSource, bwlocalWorld.screenshotTakenManually);
	}

	// Token: 0x0600298D RID: 10637 RVA: 0x001301A0 File Offset: 0x0012E5A0
	private void RevertLocalWorldChanges(string localWorldID, string dataType, string dataSubtype)
	{
		BWStandalone.Overlays.ShowConfirmationDialog("Revert Changes?", delegate()
		{
			BWUserWorldsDataManager.Instance.RevertLocalChanges(localWorldID, delegate
			{
				this.SelectItemWithID(localWorldID, dataType, dataSubtype);
			});
		});
	}

	// Token: 0x0600298E RID: 10638 RVA: 0x001301EC File Offset: 0x0012E5EC
	private void ShowLinkToIOSAccountDialog()
	{
		BWStandalone.Overlays.SetUIBusy(true);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/steam_current_user/account_link");
		bwapirequestBase.onSuccess = delegate(JObject respJson)
		{
			BWStandalone.Overlays.SetUIBusy(false);
			bool flag = false;
			bool flag2 = false;
			flag = BWJsonHelpers.PropertyIfExists(flag, "ios_link_available", respJson);
			flag2 = BWJsonHelpers.PropertyIfExists(flag2, "ios_link_initiated", respJson);
			if (flag2)
			{
				BWStandalone.Overlays.ShowPopupLinkToIOSAccount();
			}
			else if (flag)
			{
				string text = "Account linking enables you to access your worlds, models, and blocks across all platforms on which Blocksworld is available.";
				text += "\n\n";
				text += "If you would like to link your Steam Blocksworld account to your iOS Blocksworld account, please start the process in the iOS version of Blocksworld and then return here!";
				BWStandalone.Overlays.ShowMessage(text);
			}
			else
			{
				BWStandalone.Overlays.ShowMessage("Account linking is not available at this time.");
			}
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWStandalone.Overlays.SetUIBusy(false);
			string messageStr = "The following error has occurred:\n\n[" + error.message + "]\n\nPlease contact support.";
			BWStandalone.Overlays.ShowMessage(messageStr);
		};
		bwapirequestBase.Send();
	}

	// Token: 0x0600298F RID: 10639 RVA: 0x00130265 File Offset: 0x0012E665
	public void PrepareForWorldSessionLaunch()
	{
		this.lastMenuScene = SceneManager.GetActiveScene();
		this.onReturnToMenuLoadScene = null;
		SceneManager.SetActiveScene(this.gameScene);
		BWStandalone.Overlays.ShowLoadingOverlay(true);
		BWStandalone.Overlays.enabled = false;
		this.availableScreenshotIndex = 0;
	}

	// Token: 0x06002990 RID: 10640 RVA: 0x001302A4 File Offset: 0x0012E6A4
	public void SaveCurrentWorldSession(byte[] imageData)
	{
		if (this.currentWorldInfo == null)
		{
			BWLog.Error("No current world info");
			return;
		}
		if (this.currentWorldInfo is BWLocalWorld)
		{
			BWLog.Info("Saving local world " + ((imageData != null) ? "with screenshot" : "without screenshot "));
			BWLocalWorld bwlocalWorld = this.currentWorldInfo as BWLocalWorld;
			if (WorldSession.current.worldSourceJsonStr != bwlocalWorld.source)
			{
				bwlocalWorld.localChangedSource = true;
				bwlocalWorld.OverwriteSource(WorldSession.current.worldSourceJsonStr, WorldSession.current.hasWinCondition);
				bwlocalWorld.OverwriteSource_Exdilin(WorldSession.current.requiredMods);
				bwlocalWorld.SetUpdatedAtTime();
			}
			if (imageData != null)
			{
				bwlocalWorld.localChangedScreenshot = true;
			}
			BWUserWorldsDataManager.Instance.SaveWorldLocal(bwlocalWorld, imageData, true);
			if (bwlocalWorld.publicationStatus == BWWorld.PublicationStatus.NOT_PUBLISHED)
			{
				BWUserWorldsDataManager.Instance.UpdateRemoteWorld(bwlocalWorld, imageData, null, null);
			}
		} else {
			string path = string.Format("/api/v1/worlds/{0}", currentWorldInfo.worldID);
			BWAPIRequestBase request = BW.API.CreateRequest("PUT", path);
			Dictionary<string, string> dict = new Dictionary<string, string>();
			dict["source_json_str"] = WorldSession.current.worldSourceJsonStr;
			dict["has_win_condition"] = WorldSession.current.hasWinCondition.ToString();
			dict["required_mods_json_str"] = WorldSession.current.requiredModsJsonStr; // added by exdilin
			request.AddParams(dict);
			if (imageData != null) {
				request.AddImageData("screenshot_image", imageData, "screenshot.jpg", "image/png");
			}
			request.Send();
			currentWorldInfo.source = WorldSession.current.worldSourceJsonStr;
		}
	}

	// Token: 0x06002991 RID: 10641 RVA: 0x00130380 File Offset: 0x0012E780
	public void SendScreenshot(byte[] imageData, string label)
	{
		if (WorldSession.isProfileBuildSession())
		{
			Blocksworld.UI.Dialog.ShowProfilePictureConfirmation(imageData);
		}
		else if (WorldSession.isWorldScreenshotSession() && this.currentWorldInfo is BWLocalWorld)
		{
			BWLocalWorld localWorld = this.currentWorldInfo as BWLocalWorld;
			BWUserWorldsDataManager.Instance.OverwriteScreenshot(localWorld, imageData);
		}
		else
		{
			string currentUserScreenshotsFolder = BWFilesystem.CurrentUserScreenshotsFolder;
			if (!Directory.Exists(currentUserScreenshotsFolder))
			{
				Directory.CreateDirectory(currentUserScreenshotsFolder);
			}
			string path = string.Empty;
			bool flag = false;
			while (!flag)
			{
				string path2 = label + "_" + this.availableScreenshotIndex.ToString() + ".png";
				path = Path.Combine(currentUserScreenshotsFolder, path2);
				if (File.Exists(path))
				{
					this.availableScreenshotIndex++;
				}
				else
				{
					flag = true;
				}
			}
			File.WriteAllBytes(path, imageData);
		}
	}

	// Token: 0x06002992 RID: 10642 RVA: 0x00130461 File Offset: 0x0012E861
	public void SetProfileWorldData(string worldSource, string avatarSource, string profileGender)
	{
		BWUserDataManager.Instance.currentUserProfileWorld.UpdateFromWorldSave(worldSource, avatarSource, profileGender);
		BWUserDataManager.Instance.SaveCurrentUserProfileWorld();
	}

	// Token: 0x06002993 RID: 10643 RVA: 0x00130480 File Offset: 0x0012E880
	public void SetCurrentUserProfilePicture(byte[] imageData)
	{
		WorldSession.Save();
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.LoadImage(imageData);
		texture2D.Apply();
		int height = texture2D.height;
		int x = Mathf.FloorToInt((float)(texture2D.width - height) * 0.5f);
		Color[] pixels = texture2D.GetPixels(x, 0, height, height);
		Texture2D texture2D2 = new Texture2D(height, height);
		texture2D2.SetPixels(pixels);
		texture2D2.Apply();
		byte[] imageData2 = texture2D2.EncodeToPNG();
		BWUserDataManager.Instance.SaveCurrentUserProfilePicture(imageData2);
		BWUserDataManager.Instance.UploadCurrentUserProfile(imageData2);
		UnityEngine.Object.Destroy(texture2D);
		UnityEngine.Object.Destroy(texture2D2);
	}

	// Token: 0x06002994 RID: 10644 RVA: 0x00130515 File Offset: 0x0012E915
	public void PurchaseCurrentlyLoadedModel()
	{
		if (!string.IsNullOrEmpty(this.currentPreviewModelID))
		{
			BWU2UModelDataManager.Instance.PurchaseU2UModel(this.currentPreviewModelID, delegate
			{
				WorldSession.ModelPurchaseCallback("success");
			});
		}
	}

	// Token: 0x06002995 RID: 10645 RVA: 0x00130554 File Offset: 0x0012E954
	public void OpenStoreFromWorldWithBlockItemId(TabBarTabId tabId, int blockItemId)
	{
		this.onReturnToMenuLoadScene = new UISceneInfo();
		this.onReturnToMenuLoadScene.path = "ShopMenu_Blocks";
		string dataSubtype = string.Empty;
		switch (tabId)
		{
		case TabBarTabId.Blocks:
			dataSubtype = "Blocks";
			break;
		case TabBarTabId.Props:
			dataSubtype = "Props";
			break;
		case TabBarTabId.Textures:
			dataSubtype = "Textures";
			break;
		case TabBarTabId.Blocksters:
			dataSubtype = "Blocksters";
			break;
		case TabBarTabId.Gear:
			dataSubtype = "Gear";
			break;
		case TabBarTabId.ActionBlocks:
			dataSubtype = "Actions";
			break;
		}
		this.onReturnToMenuLoadScene.dataSubtype = dataSubtype;
		this.onReturnToMenuLoadScene.parameters = new Dictionary<string, string>
		{
			{
				"LoadAction",
				"SelectItemWithID"
			},
			{
				"LoadActionID",
				blockItemId.ToString()
			}
		};
	}

	// Token: 0x06002996 RID: 10646 RVA: 0x00130635 File Offset: 0x0012EA35
	public void AddItemsToCartFromWorld(BlocksInventory blocksInventory)
	{
		this.shoppingCart.AddBlocksInventory(blocksInventory);
	}

	// Token: 0x040023C7 RID: 9159
	public static BWStandalone Instance;

	// Token: 0x040023C9 RID: 9161
	private const string gameSceneName = "BlocksworldGameScene";

	// Token: 0x040023CA RID: 9162
	private const string menuSceneName = "MenuRoot";

	// Token: 0x040023CB RID: 9163
	private Scene gameScene;

	// Token: 0x040023CC RID: 9164
	private Scene mainMenuScene;

	// Token: 0x040023CD RID: 9165
	private Scene lastMenuScene;

	// Token: 0x040023CE RID: 9166
	private UISceneInfo onReturnToMenuLoadScene;

	// Token: 0x040023CF RID: 9167
	private string onReturnToMenuAction;

	// Token: 0x040023D0 RID: 9168
	public string onReturnToMenuActionID;

	// Token: 0x040023D1 RID: 9169
	private string onReturnToMenuActionDataType;

	// Token: 0x040023D2 RID: 9170
	private string onReturnToMenuActionDataSubtype;

	// Token: 0x040023D3 RID: 9171
	private Blocksworld bw;

	// Token: 0x040023D4 RID: 9172
	private MainUIController menuController;

	// Token: 0x040023D5 RID: 9173
	private MenuOverlays menuOverlays;

	// Token: 0x040023D6 RID: 9174
	public BWWorld currentWorldInfo;

	// Token: 0x040023D7 RID: 9175
	private string currentPreviewModelID;

	// Token: 0x040023D8 RID: 9176
	private BWShoppingCart shoppingCart;

	// Token: 0x040023D9 RID: 9177
	private int availableScreenshotIndex;
}
