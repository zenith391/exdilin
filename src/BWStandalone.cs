using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BWStandalone : MonoBehaviour
{
	public static BWStandalone Instance;

	private const string gameSceneName = "BlocksworldGameScene";

	private const string menuSceneName = "MenuRoot";

	private Scene gameScene;

	private Scene mainMenuScene;

	private Scene lastMenuScene;

	private UISceneInfo onReturnToMenuLoadScene;

	private string onReturnToMenuAction;

	public string onReturnToMenuActionID;

	private string onReturnToMenuActionDataType;

	private string onReturnToMenuActionDataSubtype;

	private Blocksworld bw;

	private MainUIController menuController;

	private MenuOverlays menuOverlays;

	public BWWorld currentWorldInfo;

	private string currentPreviewModelID;

	private BWShoppingCart shoppingCart;

	private int availableScreenshotIndex;

	public bool menuLoaded { get; private set; }

	public bool addingItemToCart { get; private set; }

	public static MenuOverlays Overlays => Instance.menuOverlays;

	public static BWShoppingCart ShoppingCart => Instance.shoppingCart;

	private void Awake()
	{
		Instance = this;
	}

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
		gameScene = SceneManager.GetSceneByName("BlocksworldGameScene");
		SceneManager.SetActiveScene(gameScene);
		yield return new WaitForSeconds(1f);
		while (Blocksworld.isLoadingScene)
		{
			yield return null;
		}
		Blocksworld.inBackground = true;
		ViewportWatchdog.StopWatching();
		shoppingCart = new BWShoppingCart();
		BWUserModelsDataManager.Instance.LoadAllModelsMetadata();
		BWU2UModelDataManager.Instance.LoadCurrentUserPurchasedModels();
		AsyncOperation menuSceneLoader = SceneManager.LoadSceneAsync("MenuRoot", LoadSceneMode.Additive);
		BWLog.Info("Loading menu..");
		while (!menuSceneLoader.isDone)
		{
			yield return null;
		}
		yield return null;
		mainMenuScene = SceneManager.GetSceneByName("MenuRoot");
		SceneManager.SetActiveScene(mainMenuScene);
		GameObject[] rootGameObjects = mainMenuScene.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			menuController = gameObject.GetComponentInChildren<MainUIController>();
			if (menuController != null)
			{
				menuController.sharedUIResources.dataManager.useExampleData = false;
				menuController.sharedUIResources.imageManager.useExampleData = false;
				menuController.sharedUIResources.dataManager.Init();
				menuOverlays = menuController.GetComponent<MenuOverlays>();
				break;
			}
		}
		while (!menuController.hasLoadedScene)
		{
			yield return null;
		}
		menuLoaded = true;
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
	}

	public void Update()
	{
		if (menuController != null && !menuController.IsLoadingScene())
		{
			MenuInputHandler.HandleInput();
		}
	}

	public void ReturnToMenu()
	{
		MappedInput.SetMode(MappableInputMode.Menu);
		SceneManager.SetActiveScene(lastMenuScene);
		if (onReturnToMenuLoadScene != null)
		{
			menuController.LoadUIScene(onReturnToMenuLoadScene, back: false, SceneTransitionStyle.None, SceneTransitionStyle.Fade);
		}
		else if (!menuController.IsLoadingScene())
		{
			menuController.Show();
			Overlays.ShowLoadingOverlay(show: false);
			menuController.loadedSceneController.OnReturnToScene();
			if (!string.IsNullOrEmpty(onReturnToMenuAction))
			{
				HandleMenuUIMessage(onReturnToMenuAction, onReturnToMenuActionID, onReturnToMenuActionDataType, onReturnToMenuActionDataSubtype);
			}
			if (currentWorldInfo is BWLocalWorld)
			{
				HighlightMissingFields(currentWorldInfo as BWLocalWorld);
			}
		}
		Overlays.enabled = true;
		onReturnToMenuLoadScene = null;
		onReturnToMenuAction = string.Empty;
		onReturnToMenuActionID = string.Empty;
		onReturnToMenuActionDataType = string.Empty;
		onReturnToMenuActionDataSubtype = string.Empty;
	}

	public void OnUISceneLoad()
	{
		addingItemToCart = false;
	}

	public bool HandleMenuUIMessage(string messageStr, string senderID, string senderDataType, string senderDataSubtype)
	{
		bool result = true;
		switch (messageStr)
		{
		case "PlayWorld":
			PlayWorldWithId(senderID, senderDataType, senderDataSubtype, buildMode: false);
			return result;
		case "NewWorldFromTemplate":
			CreateNewWorldWithTemplate(senderID);
			return result;
		case "BuildLocalWorld":
			StartBuildModeWithLocalWorld(senderID, senderDataType, senderDataSubtype);
			return result;
		case "BuildRemoteWorld":
			PlayWorldWithId(senderID, senderDataType, senderDataSubtype, buildMode: true);
			return result;
		case "LocalWorldScreenshotSession":
			StartLocalWorldScreenshotSession(senderID, senderDataType, senderDataSubtype);
			return result;
		case "ReportWorld":
			ShowReportWorldDialog(senderID);
			return result;
		case "ReportModel":
			ShowReportModelDialog(senderID);
			return result;
		case "BookmarkWorld":
			BookmarkWorld(senderID);
			return result;
		case "UnbookmarkWorld":
			UnbookmarkWorld(senderID);
			return result;
		case "LoadProfileWorld":
			LoadProfileWorld();
			return result;
		case "DeleteLocalWorld":
			DeleteLocalWorld(senderID);
			return result;
		case "PublishLocalWorld":
			PublishLocalWorld(senderID);
			return result;
		case "UnpublishLocalWorld":
			UnpublishLocalWorld(senderID);
			return result;
		case "DeleteAllLocalWorlds":
			BWUserWorldsDataManager.Instance.DeleteAll();
			return result;
		case "UserModelPreview":
			LoadUserModelPreview(senderID, senderDataType, senderDataSubtype);
			return result;
		case "CommunityModelPreview":
			LoadCommunityModelPreview(senderID, senderDataType, senderDataSubtype);
			return result;
		case "DeleteLocalModel":
			DeleteLocalModel(senderID);
			return result;
		case "CloneLocalWorld":
			CloneLocalWorld(senderID);
			return result;
		case "RevertLocalWorldChanges":
			RevertLocalWorldChanges(senderID, senderDataType, senderDataSubtype);
			return result;
		case "CloneLocalWorldAndPlay":
			CloneLocalWorldAndPlay(senderID);
			return result;
		case "SelectItemWithID":
			SelectItemWithID(senderID, senderDataType, senderDataSubtype);
			return result;
		case "OpenUserProfile":
			OpenUserProfile(senderID);
			return result;
		case "PublishLocalModel":
			PublishLocalModel(senderID);
			return result;
		case "UnpublishLocalModel":
			UnpublishLocalModel(senderID);
			return result;
		case "LockModelSource":
			LockModelSource(senderID);
			return result;
		case "UnlockModelSource":
			UnlockModelSource(senderID);
			return result;
		case "EditModelPrice":
			EditModelPrice(senderID);
			return result;
		case "AddBlockToCart":
			AddBlockItemToCart(senderID, senderDataType, senderDataSubtype, animate: true);
			return result;
		case "DecreaseCartBlockCount":
		case "RemoveBlockFromCart":
			RemoveBlockItemFromCart(senderID);
			return result;
		case "IncreaseCartBlockCount":
			AddBlockItemToCart(senderID, senderDataType, senderDataSubtype, animate: false);
			return result;
		case "ClearBlockFromCart":
			ClearBlockItemFromCart(senderID);
			return result;
		case "AddModelToCart":
			AddModelToCart(senderID, blueprintOnly: false, senderDataType, senderDataSubtype);
			return result;
		case "AddModelBlueprintToCart":
			AddModelToCart(senderID, blueprintOnly: true, senderDataType, senderDataSubtype);
			return result;
		case "ClearModelFromCart":
			ClearModelFromCart(senderID);
			return result;
		case "PurchaseCartContents":
			return shoppingCart.BuyContents();
		case "ClearCartContents":
			return shoppingCart.ClearContents();
		case "HighlightMissingFields":
		{
			BWLocalWorld worldWithLocalWorldID = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(senderID);
			if (worldWithLocalWorldID != null)
			{
				HighlightMissingFields(worldWithLocalWorldID);
			}
			return result;
		}
		case "LinkToIOSAccount":
			ShowLinkToIOSAccountDialog();
			return result;
		case "BuyCoinPack":
			BuyCoinPack(senderID);
			return result;
		case "FollowUser":
			BWUserDataManager.Instance.FollowUser(senderID);
			return result;
		case "UnfollowUser":
			BWUserDataManager.Instance.UnfollowUser(senderID);
			return result;
		case "ShowWorldDetail":
			ShowWorldDetailPanel(senderID);
			return result;
		case "ShowU2UModelDetail":
			ShowU2UModelDetailPanel(senderID);
			return result;
		case "ShowUserModelDetail":
			ShowUserModelDetailPanel(senderID);
			return result;
		default:
			result = false;
			BWLog.Info("Received ui message: " + messageStr + " id " + senderID);
			return result;
		}
	}

	public void HandleWorldExitMessage(string messageStr)
	{
		switch (messageStr)
		{
		case "corrupt_model_source":
			Overlays.notifications.ShowNotification("Can't load model!");
			return;
		case "corrupt_world_source":
			Overlays.notifications.ShowNotification("Can't load world!");
			return;
		case "add_model_to_cart":
			if (!string.IsNullOrEmpty(currentPreviewModelID))
			{
				AddModelToCart(currentPreviewModelID, blueprintOnly: false, null, null);
			}
			return;
		case "store/main":
		{
			UISceneInfo sceneInfo = new UISceneInfo
			{
				path = "ShopMenu_Blocks",
				dataSubtype = "Blocks"
			};
			menuController.LoadUIScene(sceneInfo, back: false, SceneTransitionStyle.None, SceneTransitionStyle.Fade);
			return;
		}
		case "shopping_cart":
		{
			UISceneInfo sceneInfo2 = new UISceneInfo
			{
				path = "ShoppingCart"
			};
			menuController.LoadUIScene(sceneInfo2, back: false, SceneTransitionStyle.None, SceneTransitionStyle.Fade);
			return;
		}
		}
		string[] array = messageStr.Split('/');
		if (array.Length > 1 && array[0] == "deep-link" && array.Length == 3 && array[1] == "profile")
		{
			string dataSubtype = array[2];
			UISceneInfo sceneInfo3 = new UISceneInfo
			{
				path = "UserProfile",
				dataType = "UserProfile",
				dataSubtype = dataSubtype
			};
			menuController.LoadUIScene(sceneInfo3, back: false, SceneTransitionStyle.None, SceneTransitionStyle.Fade);
		}
	}

	private void PlayWorldWithId(string worldID, string dataType, string dataSubtype, bool buildMode)
	{
		PrepareForWorldSessionLaunch();
		onReturnToMenuAction = "SelectItemWithID";
		onReturnToMenuActionID = worldID;
		onReturnToMenuActionDataType = dataType;
		onReturnToMenuActionDataSubtype = dataSubtype;
		BWProfileWorld bWProfileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
		string currentUserAvatarSource = ((bWProfileWorld != null) ? bWProfileWorld.avatarSourceJsonStr : string.Empty);
		WorldSession.StartForStandaloneWithRemoteWorldId(worldID, buildMode, currentUserAvatarSource);
	}

	private void ShowWorldDetailPanel(string worldID)
	{
		UIDataSource dataSource = MainUIController.Instance.dataManager.GetDataSource("RemoteWorld", worldID);
		Overlays.ShowPopupWorldDetailPanel(worldID, dataSource);
	}

	private void ShowU2UModelDetailPanel(string modelID)
	{
		UIDataSource dataSource = MainUIController.Instance.dataManager.GetDataSource("U2UModel", modelID);
		Overlays.ShowPopupU2UModelDetailPanel(modelID, dataSource);
	}

	private void ShowUserModelDetailPanel(string localModelID)
	{
		BWUserModel modelWithLocalId = BWUserModelsDataManager.Instance.GetModelWithLocalId(localModelID);
		modelWithLocalId.LoadSourceFromDataManager();
		string dataType = ((!modelWithLocalId.isPublished) ? "CurrentUserUnpublishedModels" : "CurrentUserPublishedModels");
		UIDataSource dataSource = MainUIController.Instance.dataManager.GetDataSource(dataType, null);
		Overlays.ShowPopupUserModelDetailPanel(localModelID, dataSource);
	}

	private void ShowReportWorldDialog(string worldID)
	{
		Overlays.ShowPopupReportWorld(worldID);
	}

	private void ShowReportModelDialog(string modelID)
	{
		Overlays.ShowPopupReportModel(modelID);
	}

	private void BookmarkWorld(string worldID)
	{
		BWUserDataManager.Instance.BookmarkWorld(worldID);
		Overlays.notifications.ShowNotification("Bookmarked World");
	}

	private void UnbookmarkWorld(string worldID)
	{
		BWUserDataManager.Instance.UpdateUIWithCurrentUserData();
		UnityAction yesAction = delegate
		{
			BWUserDataManager.Instance.UnbookmarkWorld(worldID);
			Overlays.notifications.ShowNotification("Removed From Bookmarks");
			BWUserDataManager.Instance.UpdateUIWithCurrentUserData();
		};
		Overlays.ShowConfirmationDialog(BWMenuTextEnum.RemoveFromBookmarkedWorldsConfirmation, yesAction);
	}

	private void OpenUserProfile(string userIDStr)
	{
		if (string.IsNullOrEmpty(userIDStr))
		{
			BWLog.Error("Invalid userID");
			return;
		}
		UISceneInfo uISceneInfo = new UISceneInfo();
		string text = BWUser.currentUser.userID.ToString();
		if (userIDStr == text)
		{
			uISceneInfo.path = "CurrentUserProfile";
		}
		else
		{
			uISceneInfo.path = "UserProfile";
			uISceneInfo.dataType = "UserProfile";
			uISceneInfo.dataSubtype = userIDStr;
		}
		menuController.LoadUIScene(uISceneInfo);
	}

	private void StartBuildModeWithLocalWorld(string worldID, string dataType, string dataSubtype)
	{
		BWLocalWorld world = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(worldID);
		if (world == null)
		{
			BWLog.Error("Can't find local world with ID: worldID");
			return;
		}
		UnityAction completion = delegate
		{
			currentWorldInfo = world;
			PrepareForWorldSessionLaunch();
			onReturnToMenuAction = "SelectItemWithID";
			onReturnToMenuActionID = worldID;
			onReturnToMenuActionDataType = dataType;
			onReturnToMenuActionDataSubtype = dataSubtype;
			BWProfileWorld bWProfileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
			string currentUserAvatarSource = ((bWProfileWorld != null) ? bWProfileWorld.avatarSourceJsonStr : string.Empty);
			WorldSession.StartForStandaloneInBuildMode(currentWorldInfo.worldID, currentWorldInfo.title, currentWorldInfo.source, currentUserAvatarSource, world.screenshotTakenManually);
		};
		BWUserWorldsDataManager.Instance.LoadSourceForLocalWorld(world, completion);
	}

	private void StartLocalWorldScreenshotSession(string localWorldID, string dataType, string dataSubtype)
	{
		BWLocalWorld world = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(localWorldID);
		if (world == null)
		{
			BWLog.Error("Can't find local world with ID: worldID");
			return;
		}
		UnityAction completion = delegate
		{
			currentWorldInfo = world;
			PrepareForWorldSessionLaunch();
			onReturnToMenuAction = "SelectItemWithID";
			onReturnToMenuActionID = localWorldID;
			onReturnToMenuActionDataType = dataType;
			onReturnToMenuActionDataSubtype = dataSubtype;
			BWProfileWorld bWProfileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
			string currentUserAvatarSource = ((bWProfileWorld != null) ? bWProfileWorld.avatarSourceJsonStr : string.Empty);
			WorldSession.StartForStandaloneWorldScreenshotSession(currentWorldInfo.worldID, currentWorldInfo.title, currentWorldInfo.source, currentUserAvatarSource);
		};
		BWUserWorldsDataManager.Instance.LoadSourceForLocalWorld(world, completion);
	}

	private void LoadProfileWorld()
	{
		BWProfileWorld bWProfileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
		if (bWProfileWorld != null)
		{
			currentWorldInfo = bWProfileWorld;
			PrepareForWorldSessionLaunch();
			WorldSession.StartForStandaloneWithProfileWorld(bWProfileWorld.profileGender, bWProfileWorld.worldID, bWProfileWorld.title, bWProfileWorld.source, bWProfileWorld.avatarSourceJsonStr);
		}
	}

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
			currentPreviewModelID = localModelId;
			currentWorldInfo = null;
			PrepareForWorldSessionLaunch();
			onReturnToMenuAction = "SelectItemWithID";
			onReturnToMenuActionID = localModelId;
			onReturnToMenuActionDataType = dataType;
			onReturnToMenuActionDataSubtype = dataSubtype;
			WorldSession.StartForStandaloneWithUserModel(modelWithLocalId);
		}
	}

	private void LoadCommunityModelPreview(string remoteModelId, string dataType, string dataSubtype)
	{
		currentPreviewModelID = remoteModelId;
		currentWorldInfo = null;
		PrepareForWorldSessionLaunch();
		onReturnToMenuAction = "SelectItemWithID";
		onReturnToMenuActionID = remoteModelId;
		onReturnToMenuActionDataType = dataType;
		onReturnToMenuActionDataSubtype = dataSubtype;
		WorldSession.StartForStandaloneWithCommunityModel(remoteModelId);
	}

	private void PublishLocalModel(string localModelID)
	{
		if (!BWModelPublishCooldown.CanPublish())
		{
			int hours = 0;
			int minutes = 0;
			int seconds = 0;
			int priceToSkip = 0;
			BWModelPublishCooldown.CooldownRemaining(out hours, out minutes, out seconds, out priceToSkip);
			Overlays.ShowPopupSkipPublishCooldown(priceToSkip, delegate
			{
				BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v1/current_user/pay_model_cooldown");
				bWAPIRequestBase.AddParam("coins", priceToSkip.ToString());
				bWAPIRequestBase.onSuccess = delegate(JObject respJson)
				{
					if (respJson.ContainsKey("attrs_for_current_user"))
					{
						UISoundPlayer.Instance.PlayClip("shop_purchase");
						BWUser.UpdateCurrentUserAndNotifyListeners(respJson["attrs_for_current_user"]);
						if (BWModelPublishCooldown.CanPublish())
						{
							PublishLocalModel(localModelID);
						}
					}
				};
				bWAPIRequestBase.onFailure = delegate
				{
				};
				bWAPIRequestBase.SendOwnerCoroutine(this);
			});
		}
		else
		{
			BWUserModelsDataManager.Instance.PublishModel(localModelID);
		}
	}

	private void UnpublishLocalModel(string localModelID)
	{
		BWUserModelsDataManager.Instance.UnpublishModel(localModelID);
	}

	private void DeleteLocalModel(string localModelID)
	{
		BWUserModelsDataManager.Instance.DeleteLocalModel(localModelID);
	}

	private void LockModelSource(string localModelID)
	{
		BWUserModelsDataManager.Instance.SetModelSourceLocked(localModelID, locked: true);
	}

	private void UnlockModelSource(string localModelID)
	{
		BWUserModelsDataManager.Instance.SetModelSourceLocked(localModelID, locked: false);
	}

	private void EditModelPrice(string localModelID)
	{
		BWUserModel modelWithLocalId = BWUserModelsDataManager.Instance.GetModelWithLocalId(localModelID);
		if (modelWithLocalId == null)
		{
			BWLog.Error("no model with ID: " + localModelID);
		}
		else if (modelWithLocalId.CoinsBasePrice() < int.MaxValue)
		{
			Overlays.ShowPopupEditModelPrice(modelWithLocalId);
		}
		else
		{
			Overlays.ShowMessage("You can't sell this model!");
		}
	}

	private void CreateNewWorldWithTemplate(string templateTitle)
	{
		foreach (BWWorldTemplate worldTemplate in BWUser.currentUser.worldTemplates)
		{
			if (worldTemplate.title == templateTitle)
			{
				BWLocalWorld bWLocalWorld = (BWLocalWorld)(currentWorldInfo = BWUserWorldsDataManager.Instance.CreateNewWorldFromTemplate(worldTemplate));
				PrepareForWorldSessionLaunch();
				onReturnToMenuLoadScene = new UISceneInfo();
				onReturnToMenuLoadScene.path = "BuildMenu";
				onReturnToMenuLoadScene.parameters = new Dictionary<string, string>
				{
					{ "LoadAction", "SelectItemWithID" },
					{ "LoadActionID", bWLocalWorld.localWorldID }
				};
				BWProfileWorld bWProfileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
				string currentUserAvatarSource = ((bWProfileWorld != null) ? bWProfileWorld.avatarSourceJsonStr : string.Empty);
				WorldSession.StartForStandaloneInBuildMode(currentWorldInfo.worldID, currentWorldInfo.title, currentWorldInfo.source, currentUserAvatarSource, bWLocalWorld.screenshotTakenManually);
				break;
			}
		}
	}

	private void DeleteLocalWorld(string worldID)
	{
		BWUserWorldsDataManager.Instance.DeleteLocalWorld(worldID);
	}

	private void PublishLocalWorld(string localWorldID)
	{
		BWLocalWorld world = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(localWorldID);
		if (world == null)
		{
			BWLog.Error("No local world with id: " + localWorldID);
		}
		else if (world.IsLowEffortWorld())
		{
			Overlays.ShowMessage("This world is too simple to publish!");
		}
		else if (!BWWorldPublishCooldown.CanPublish(world.worldID))
		{
			Debug.Log("Showing skip cooldown popup");
			int hours = 0;
			int minutes = 0;
			int seconds = 0;
			int priceToSkip = 0;
			BWWorldPublishCooldown.CooldownRemaining(out hours, out minutes, out seconds, out priceToSkip);
			Overlays.ShowPopupSkipPublishCooldown(priceToSkip, delegate
			{
				BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v1/current_user/pay_world_cooldown");
				bWAPIRequestBase.AddParam("coins", priceToSkip.ToString());
				bWAPIRequestBase.onSuccess = delegate(JObject respJson)
				{
					if (respJson.ContainsKey("attrs_for_current_user"))
					{
						UISoundPlayer.Instance.PlayClip("shop_purchase");
						BWUser.UpdateCurrentUserAndNotifyListeners(respJson["attrs_for_current_user"]);
						if (BWWorldPublishCooldown.CanPublish(world.worldID))
						{
							PublishLocalWorld(localWorldID);
						}
					}
				};
				bWAPIRequestBase.onFailure = delegate
				{
				};
				bWAPIRequestBase.SendOwnerCoroutine(this);
			});
		}
		else
		{
			if (HighlightMissingFields(world, showNotifications: true))
			{
				return;
			}
			menuController.loadedSceneController.ResetDetailPanelAnimation(localWorldID);
			if (world.IsPublic())
			{
				Overlays.ShowConfirmationDialog("Publish changes?", delegate
				{
					BWUserWorldsDataManager.Instance.PublishLocalWorld(localWorldID);
				});
			}
			else if (BWUserWorldsDataManager.Instance.PublishedWorldCount() == 0)
			{
				string textString = MenuTextDefinitions.GetTextString(BWMenuTextEnum.PublishWorldConfirmation);
				textString = textString.Replace("<World Title>", world.title);
				Overlays.ShowConfirmationDialog(textString, delegate
				{
					BWUserWorldsDataManager.Instance.PublishLocalWorld(localWorldID);
				});
			}
			else
			{
				BWUserWorldsDataManager.Instance.PublishLocalWorld(localWorldID);
			}
		}
	}

	private bool HighlightMissingFields(BWLocalWorld world, bool showNotifications = false)
	{
		bool flag = string.IsNullOrEmpty(world.title);
		bool flag2 = string.IsNullOrEmpty(world.description);
		bool flag3 = world.categoryIDs == null || world.categoryIDs.Count == 0;
		if (flag || flag2 || flag3)
		{
			string animTrigger = ((!flag) ? "Reset" : "Highlight");
			string animTrigger2 = ((!flag2) ? "Reset" : "Highlight");
			string animTrigger3 = ((!flag3) ? "Reset" : "Highlight");
			menuController.loadedSceneController.TriggerDetailPanelAnimation(world.localWorldID, "title", animTrigger);
			menuController.loadedSceneController.TriggerDetailPanelAnimation(world.localWorldID, "description", animTrigger2);
			menuController.loadedSceneController.TriggerDetailPanelAnimation(world.localWorldID, "categories", animTrigger3);
			if (showNotifications)
			{
				List<string> list = new List<string>();
				if (flag)
				{
					Overlays.notifications.ShowNotification(MenuTextDefinitions.GetTextString(BWMenuTextEnum.MissingWorldTitleNotification));
				}
				if (flag2)
				{
					Overlays.notifications.ShowNotification(MenuTextDefinitions.GetTextString(BWMenuTextEnum.MissingWorldDescriptionNotification));
				}
				if (flag3)
				{
					Overlays.notifications.ShowNotification(MenuTextDefinitions.GetTextString(BWMenuTextEnum.MissingWorldCategoryNotification));
				}
			}
			return true;
		}
		return false;
	}

	private void SelectItemWithID(string itemID, string dataType, string dataSubtype)
	{
		if (dataType == "RemoteWorld")
		{
			ShowWorldDetailPanel(itemID);
		}
		else if (dataType == "U2UModel")
		{
			ShowU2UModelDetailPanel(itemID);
		}
		else
		{
			menuController.loadedSceneController.SelectItemInSceneElements(itemID, dataType, dataSubtype);
		}
	}

	private void AddBlockItemToCart(string itemID, string dataType, string dataSubtype, bool animate)
	{
		if (addingItemToCart)
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
			shoppingCart.AddBlockItemPack(blockItemID, 1);
			return;
		}
		addingItemToCart = true;
		GameObject gameObject = CloneItemWithID(itemID, dataType, dataSubtype);
		if (gameObject != null)
		{
			Overlays.AnimateTransform((RectTransform)gameObject.transform, menuController.loadedSceneController.GetShoppingCartPosition(), 0.25f, 2f, destroyOnCompletion: true, delegate
			{
				shoppingCart.AddBlockItemPack(blockItemID, 1);
				addingItemToCart = false;
			});
		}
	}

	private void RemoveBlockItemFromCart(string itemID)
	{
		int result = 0;
		int.TryParse(itemID, out result);
		if (result != 0)
		{
			shoppingCart.RemoveBlockItemPack(result, 1);
		}
	}

	private void ClearBlockItemFromCart(string itemID)
	{
		int result = 0;
		int.TryParse(itemID, out result);
		if (result != 0)
		{
			shoppingCart.ClearBlockItemPacks(result);
		}
	}

	private void AddModelToCart(string itemID, bool blueprintOnly, string dataType, string dataSubtype)
	{
		if (addingItemToCart)
		{
			return;
		}
		if (shoppingCart.ContainsU2UModel(itemID))
		{
			Overlays.ShowConfirmationDialog("This model is already in your shopping cart.", delegate
			{
				menuController.LoadUIScene("ShoppingCart");
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
			Overlays.ShowMessage("You can't buy your own model!");
			return;
		}
		addingItemToCart = true;
		GameObject gameObject = CloneItemWithID(itemID, dataType, dataSubtype);
		if (gameObject != null)
		{
			Overlays.AnimateTransform((RectTransform)gameObject.transform, menuController.loadedSceneController.GetShoppingCartPosition(), 0.25f, 2f, destroyOnCompletion: true, delegate
			{
				UISoundPlayer.Instance.PlayClip("simple_swish_15", 0.6f);
				shoppingCart.AddU2UModel(model, blueprintOnly);
				addingItemToCart = false;
			});
		}
		else
		{
			UISoundPlayer.Instance.PlayClip("simple_swish_15", 0.6f);
			shoppingCart.AddU2UModel(model, blueprintOnly);
			addingItemToCart = false;
		}
	}

	private void ClearModelFromCart(string itemID)
	{
		shoppingCart.ClearU2UModel(itemID);
	}

	private void BuyCoinPack(string coinPackInternalIdentifier)
	{
		BWSteamIAPManager.Instance.BuyCoinPack(coinPackInternalIdentifier);
	}

	private GameObject CloneItemWithID(string itemID, string dataType, string dataSubtype)
	{
		return menuController.loadedSceneController.CloneItemInSceneElements(itemID, dataType, dataSubtype);
	}

	private void UnpublishLocalWorld(string worldID)
	{
		BWUserWorldsDataManager.Instance.UnpublishLocalWorld(worldID);
	}

	private void CloneLocalWorld(string localWorldID)
	{
		BWUserWorldsDataManager.Instance.CloneLocalWorld(localWorldID);
	}

	private void CloneLocalWorldAndPlay(string localWorldID)
	{
		BWLocalWorld bWLocalWorld = BWUserWorldsDataManager.Instance.CloneLocalWorld(localWorldID);
		if (bWLocalWorld == null)
		{
			BWLog.Error("Failed to clone world with ID: " + localWorldID);
			return;
		}
		currentWorldInfo = bWLocalWorld;
		PrepareForWorldSessionLaunch();
		onReturnToMenuLoadScene = new UISceneInfo();
		onReturnToMenuLoadScene.path = "BuildMenu";
		onReturnToMenuLoadScene.parameters = new Dictionary<string, string>
		{
			{ "LoadAction", "SelectItemWithID" },
			{ "LoadActionID", bWLocalWorld.localWorldID }
		};
		BWProfileWorld bWProfileWorld = BWUserDataManager.Instance.CreateOrLoadCurrentUserProfileWorld();
		string currentUserAvatarSource = ((bWProfileWorld != null) ? bWProfileWorld.avatarSourceJsonStr : string.Empty);
		WorldSession.StartForStandaloneInBuildMode(currentWorldInfo.worldID, currentWorldInfo.title, currentWorldInfo.source, currentUserAvatarSource, bWLocalWorld.screenshotTakenManually);
	}

	private void RevertLocalWorldChanges(string localWorldID, string dataType, string dataSubtype)
	{
		Overlays.ShowConfirmationDialog("Revert Changes?", delegate
		{
			BWUserWorldsDataManager.Instance.RevertLocalChanges(localWorldID, delegate
			{
				SelectItemWithID(localWorldID, dataType, dataSubtype);
			});
		});
	}

	private void ShowLinkToIOSAccountDialog()
	{
		Overlays.SetUIBusy(busy: true);
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/steam_current_user/account_link");
		bWAPIRequestBase.onSuccess = delegate(JObject respJson)
		{
			Overlays.SetUIBusy(busy: false);
			bool property = false;
			bool property2 = false;
			property = BWJsonHelpers.PropertyIfExists(property, "ios_link_available", respJson);
			if (BWJsonHelpers.PropertyIfExists(property2, "ios_link_initiated", respJson))
			{
				Overlays.ShowPopupLinkToIOSAccount();
			}
			else if (property)
			{
				string text = "Account linking enables you to access your worlds, models, and blocks across all platforms on which Blocksworld is available.";
				text += "\n\n";
				text += "If you would like to link your Steam Blocksworld account to your iOS Blocksworld account, please start the process in the iOS version of Blocksworld and then return here!";
				Overlays.ShowMessage(text);
			}
			else
			{
				Overlays.ShowMessage("Account linking is not available at this time.");
			}
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			Overlays.SetUIBusy(busy: false);
			string messageStr = "The following error has occurred:\n\n[" + error.message + "]\n\nPlease contact support.";
			Overlays.ShowMessage(messageStr);
		};
		bWAPIRequestBase.Send();
	}

	public void PrepareForWorldSessionLaunch()
	{
		lastMenuScene = SceneManager.GetActiveScene();
		onReturnToMenuLoadScene = null;
		SceneManager.SetActiveScene(gameScene);
		Overlays.ShowLoadingOverlay(show: true);
		Overlays.enabled = false;
		availableScreenshotIndex = 0;
	}

	public void SaveCurrentWorldSession(byte[] imageData)
	{
		if (currentWorldInfo == null)
		{
			BWLog.Error("No current world info");
		}
		else if (currentWorldInfo is BWLocalWorld)
		{
			BWLog.Info("Saving local world " + ((imageData != null) ? "with screenshot" : "without screenshot "));
			BWLocalWorld bWLocalWorld = currentWorldInfo as BWLocalWorld;
			if (WorldSession.current.worldSourceJsonStr != bWLocalWorld.source)
			{
				bWLocalWorld.localChangedSource = true;
				bWLocalWorld.OverwriteSource(WorldSession.current.worldSourceJsonStr, WorldSession.current.hasWinCondition);
				bWLocalWorld.OverwriteSource_Exdilin(WorldSession.current.requiredMods);
				bWLocalWorld.SetUpdatedAtTime();
			}
			if (imageData != null)
			{
				bWLocalWorld.localChangedScreenshot = true;
			}
			BWUserWorldsDataManager.Instance.SaveWorldLocal(bWLocalWorld, imageData);
			if (bWLocalWorld.publicationStatus == BWWorld.PublicationStatus.NOT_PUBLISHED)
			{
				BWUserWorldsDataManager.Instance.UpdateRemoteWorld(bWLocalWorld, imageData);
			}
		}
		else
		{
			string path = $"/api/v1/worlds/{currentWorldInfo.worldID}";
			BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("PUT", path);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["source_json_str"] = WorldSession.current.worldSourceJsonStr;
			dictionary["has_win_condition"] = WorldSession.current.hasWinCondition.ToString();
			dictionary["required_mods_json_str"] = WorldSession.current.requiredModsJsonStr;
			bWAPIRequestBase.AddParams(dictionary);
			if (imageData != null)
			{
				bWAPIRequestBase.AddImageData("screenshot_image", imageData, "screenshot.jpg", "image/png");
			}
			bWAPIRequestBase.Send();
			currentWorldInfo.source = WorldSession.current.worldSourceJsonStr;
		}
	}

	public void SendScreenshot(byte[] imageData, string label)
	{
		if (WorldSession.isProfileBuildSession())
		{
			Blocksworld.UI.Dialog.ShowProfilePictureConfirmation(imageData);
			return;
		}
		if (WorldSession.isWorldScreenshotSession() && currentWorldInfo is BWLocalWorld)
		{
			BWLocalWorld localWorld = currentWorldInfo as BWLocalWorld;
			BWUserWorldsDataManager.Instance.OverwriteScreenshot(localWorld, imageData);
			return;
		}
		string currentUserScreenshotsFolder = BWFilesystem.CurrentUserScreenshotsFolder;
		if (!Directory.Exists(currentUserScreenshotsFolder))
		{
			Directory.CreateDirectory(currentUserScreenshotsFolder);
		}
		string path = string.Empty;
		bool flag = false;
		while (!flag)
		{
			string path2 = label + "_" + availableScreenshotIndex + ".png";
			path = Path.Combine(currentUserScreenshotsFolder, path2);
			if (File.Exists(path))
			{
				availableScreenshotIndex++;
			}
			else
			{
				flag = true;
			}
		}
		File.WriteAllBytes(path, imageData);
	}

	public void SetProfileWorldData(string worldSource, string avatarSource, string profileGender)
	{
		BWUserDataManager.Instance.currentUserProfileWorld.UpdateFromWorldSave(worldSource, avatarSource, profileGender);
		BWUserDataManager.Instance.SaveCurrentUserProfileWorld();
	}

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
		Object.Destroy(texture2D);
		Object.Destroy(texture2D2);
	}

	public void PurchaseCurrentlyLoadedModel()
	{
		if (!string.IsNullOrEmpty(currentPreviewModelID))
		{
			BWU2UModelDataManager.Instance.PurchaseU2UModel(currentPreviewModelID, delegate
			{
				WorldSession.ModelPurchaseCallback("success");
			});
		}
	}

	public void OpenStoreFromWorldWithBlockItemId(TabBarTabId tabId, int blockItemId)
	{
		onReturnToMenuLoadScene = new UISceneInfo();
		onReturnToMenuLoadScene.path = "ShopMenu_Blocks";
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
		onReturnToMenuLoadScene.dataSubtype = dataSubtype;
		onReturnToMenuLoadScene.parameters = new Dictionary<string, string>
		{
			{ "LoadAction", "SelectItemWithID" },
			{
				"LoadActionID",
				blockItemId.ToString()
			}
		};
	}

	public void AddItemsToCartFromWorld(BlocksInventory blocksInventory)
	{
		shoppingCart.AddBlocksInventory(blocksInventory);
	}
}
