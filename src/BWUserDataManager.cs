using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SimpleJSON;
using UnityEngine;

public class BWUserDataManager : MonoBehaviour
{
	private static BWUserDataManager instance;

	private HashSet<string> bookmarkedWorldIDs;

	private HashSet<string> locallyReportedWorldIDs;

	private HashSet<string> locallyReportedModelIDs;

	public List<BWSocialUser> followedUsers;

	public List<BWSocialUser> followers;

	internal BWProfileWorld currentUserProfileWorld { get; private set; }

	internal bool userProfileSyncComplete { get; private set; }

	public static BWUserDataManager Instance => instance;

	private event ProfileChangedEventHandler onProfileChanged;

	private event CurrentUserDataChangedEventHandler onCurrentUserDataChanged;

	public void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	public void AddListener(ProfileChangedEventHandler listener)
	{
		onProfileChanged -= listener;
		onProfileChanged += listener;
	}

	public void RemoveListener(ProfileChangedEventHandler listener)
	{
		onProfileChanged -= listener;
	}

	public void AddCurrentUserDataChangedListener(CurrentUserDataChangedEventHandler listener)
	{
		onCurrentUserDataChanged -= listener;
		onCurrentUserDataChanged += listener;
	}

	public void RemoveCurrentUserDataChangedListener(CurrentUserDataChangedEventHandler listener)
	{
		onCurrentUserDataChanged -= listener;
	}

	public void ClearListeners()
	{
		this.onProfileChanged = null;
		this.onCurrentUserDataChanged = null;
	}

	public void NotifyListeners()
	{
		if (this.onProfileChanged != null)
		{
			this.onProfileChanged();
		}
	}

	private void NotifyCurrentUserDataListeners()
	{
		if (this.onCurrentUserDataChanged != null)
		{
			this.onCurrentUserDataChanged();
		}
	}

	private string CurrentUserProfileWorldPath()
	{
		return Path.Combine(BWFilesystem.CurrentUserProfileWorldFolder, "profileWorld.bw");
	}

	private string CurrentUserProfilePicturePath()
	{
		return Path.Combine(BWFilesystem.CurrentUserProfileWorldFolder, "profile.png");
	}

	private string CurrentUserDataPath()
	{
		return Path.Combine(BWFilesystem.CurrentUserDataFolder, "userData.json");
	}

	private string CurrentUserShoppingCartPath()
	{
		return Path.Combine(BWFilesystem.CurrentUserDataFolder, "shoppingCart.json");
	}

	public void SyncCurrentUserProfile()
	{
		StartCoroutine(SyncCurrentUserProfileCoroutine());
	}

	private IEnumerator SyncCurrentUserProfileCoroutine()
	{
		currentUserProfileWorld = LoadCurrentProfileWorldLocal();
		userProfileSyncComplete = false;
		bool worldSyncComplete = false;
		bool worldRestoredFromRemote = false;
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/profile_world");
		if (currentUserProfileWorld != null)
		{
			string valueStr = currentUserProfileWorld.updatedAt.ToString("s", CultureInfo.InvariantCulture);
			bWAPIRequestBase.AddParam("updated_at_timestamp", valueStr);
		}
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			worldSyncComplete = true;
			if (responseJson.ContainsKey("profile_world"))
			{
				currentUserProfileWorld = new BWProfileWorld(responseJson["profile_world"]);
				SaveCurrentUserProfileWorld();
				worldRestoredFromRemote = true;
			}
		};
		bWAPIRequestBase.onFailure = delegate
		{
			worldSyncComplete = true;
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
		while (!worldSyncComplete)
		{
			yield return null;
		}
		if (currentUserProfileWorld == null)
		{
			userProfileSyncComplete = true;
			yield break;
		}
		string profilePicturePath = CurrentUserProfilePicturePath();
		if (worldRestoredFromRemote || !File.Exists(profilePicturePath))
		{
			WWW imageDownload = new WWW(BWUser.currentUser.profileImageURL);
			while (!imageDownload.isDone)
			{
				yield return null;
			}
			if (!string.IsNullOrEmpty(imageDownload.error))
			{
				BWLog.Info("user has no remote profile picture");
			}
			else if (imageDownload.texture != null)
			{
				byte[] bytes = imageDownload.texture.EncodeToPNG();
				if (!File.Exists(profilePicturePath))
				{
					File.Create(profilePicturePath).Dispose();
				}
				File.WriteAllBytes(profilePicturePath, bytes);
			}
		}
		userProfileSyncComplete = true;
	}

	private BWProfileWorld CreateNewProfileWorld()
	{
		currentUserProfileWorld = new BWProfileWorld(BWUser.currentUser);
		SaveCurrentUserProfileWorld();
		return currentUserProfileWorld;
	}

	public BWProfileWorld CreateOrLoadCurrentUserProfileWorld()
	{
		if (currentUserProfileWorld == null)
		{
			currentUserProfileWorld = LoadCurrentProfileWorldLocal();
		}
		if (currentUserProfileWorld != null)
		{
			return currentUserProfileWorld;
		}
		BWLog.Info("Unable to load existing profile world, creating a new one instead...");
		return CreateNewProfileWorld();
	}

	private BWProfileWorld LoadCurrentProfileWorldLocal()
	{
		string path = CurrentUserProfileWorldPath();
		if (File.Exists(path))
		{
			try
			{
				string source = File.ReadAllText(path);
				JObject jsonObj = JSONDecoder.Decode(Util.UnobfuscateSourceForUser(source, BWUser.currentUser.userID));
				currentUserProfileWorld = new BWProfileWorld(jsonObj);
				return currentUserProfileWorld;
			}
			catch
			{
				return null;
			}
		}
		return null;
	}

	public void SaveCurrentUserProfileWorld()
	{
		if (!Directory.Exists(BWFilesystem.CurrentUserProfileWorldFolder))
		{
			Directory.CreateDirectory(BWFilesystem.CurrentUserProfileWorldFolder);
		}
		string path = CurrentUserProfileWorldPath();
		if (!File.Exists(path))
		{
			File.Create(path).Dispose();
		}
		Dictionary<string, string> obj = currentUserProfileWorld.AttrsToSave();
		string source = JSONEncoder.Encode(obj);
		File.WriteAllText(path, Util.ObfuscateSourceForUser(source, BWUser.currentUser.userID));
	}

	public void SaveCurrentUserProfilePicture(byte[] imageData)
	{
		if (!Directory.Exists(BWFilesystem.CurrentUserDataFolder))
		{
			Directory.CreateDirectory(BWFilesystem.CurrentUserDataFolder);
		}
		string text = CurrentUserProfilePicturePath();
		if (!File.Exists(text))
		{
			File.Create(text).Dispose();
		}
		File.WriteAllBytes(text, imageData);
		MainUIController.Instance.imageManager.SetUnloaded(BWFilesystem.FileProtocolPrefixStr + text);
		NotifyListeners();
	}

	public void UploadCurrentUserProfile(byte[] imageData)
	{
		string path = "/api/v1/current_user/profile_world";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("PUT", path);
		bWAPIRequestBase.AddParams(currentUserProfileWorld.AttrsToSaveRemote());
		bWAPIRequestBase.AddImageData("profile_image", imageData, "profilepic.jpg", "image/jpeg");
		bWAPIRequestBase.onSuccess = delegate(JObject response)
		{
			BWUser.currentUser.UpdateFromJson(response);
			MainUIController.Instance.imageManager.SetUnloaded(BWUser.currentUser.profileImageURL);
			NotifyListeners();
		};
		bWAPIRequestBase.Send();
	}

	public void LoadUserData()
	{
		string path = CurrentUserDataPath();
		if (File.Exists(path))
		{
			string json = File.ReadAllText(path);
			JObject json2 = JSONDecoder.Decode(json);
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			list = BWJsonHelpers.PropertyIfExists(list, "bookmarked_world_ids", json2);
			list2 = BWJsonHelpers.PropertyIfExists(list2, "reported_world_ids", json2);
			list3 = BWJsonHelpers.PropertyIfExists(list3, "reported_model_ids", json2);
			bookmarkedWorldIDs = new HashSet<string>(list);
			locallyReportedWorldIDs = new HashSet<string>(list2);
			locallyReportedModelIDs = new HashSet<string>(list3);
		}
		else
		{
			bookmarkedWorldIDs = new HashSet<string>();
			locallyReportedWorldIDs = new HashSet<string>();
			locallyReportedModelIDs = new HashSet<string>();
		}
	}

	public void SaveUserData()
	{
		if (!Directory.Exists(BWFilesystem.CurrentUserDataFolder))
		{
			Directory.CreateDirectory(BWFilesystem.CurrentUserDataFolder);
		}
		string contents = JSONEncoder.Encode(new Dictionary<string, object>
		{
			{
				"bookmarked_world_ids",
				BookmarkedWorldIDs()
			},
			{
				"reported_world_ids",
				ReportedWorldIDs()
			},
			{
				"reported_model_ids",
				ReportedModelIDs()
			}
		});
		string path = CurrentUserDataPath();
		if (!File.Exists(path))
		{
			File.Create(path).Dispose();
		}
		File.WriteAllText(path, contents);
	}

	public JObject LoadShoppingCartJSON()
	{
		string path = CurrentUserShoppingCartPath();
		if (File.Exists(path))
		{
			string json = File.ReadAllText(path);
			JObject result = null;
			try
			{
				result = JSONDecoder.Decode(json);
			}
			catch (ParseError parseError)
			{
				BWLog.Error("Error loading the shopping cart: \n" + parseError.Message + "\n" + parseError.StackTrace);
			}
			return result;
		}
		return null;
	}

	public void SaveShoppingCartJSON(string jsonStr)
	{
		if (!Directory.Exists(BWFilesystem.CurrentUserDataFolder))
		{
			Directory.CreateDirectory(BWFilesystem.CurrentUserDataFolder);
		}
		string path = CurrentUserShoppingCartPath();
		if (!File.Exists(path))
		{
			File.Create(path).Dispose();
		}
		File.WriteAllText(path, jsonStr);
	}

	public void UpdateUIWithCurrentUserData()
	{
		NotifyCurrentUserDataListeners();
	}

	public bool HasBookmarkedWorld(string worldID)
	{
		return bookmarkedWorldIDs.Contains(worldID);
	}

	public void BookmarkWorld(string worldID)
	{
		bookmarkedWorldIDs.Add(worldID);
		LikeWorldRemote(worldID);
		SaveUserData();
	}

	public void UnbookmarkWorld(string worldID)
	{
		bookmarkedWorldIDs.Remove(worldID);
		UnlikeWorldRemote(worldID);
		SaveUserData();
	}

	public void ReportWorld(string worldID)
	{
		locallyReportedWorldIDs.Add(worldID);
		SaveUserData();
		NotifyCurrentUserDataListeners();
	}

	public void ReportModel(string modelID)
	{
		locallyReportedModelIDs.Add(modelID);
		SaveUserData();
		NotifyCurrentUserDataListeners();
	}

	public bool HasReportedWorld(string worldID)
	{
		return locallyReportedWorldIDs.Contains(worldID);
	}

	public bool HasReportedModel(string modelID)
	{
		return locallyReportedModelIDs.Contains(modelID);
	}

	public List<string> BookmarkedWorldIDs()
	{
		return new List<string>(bookmarkedWorldIDs);
	}

	public List<string> ReportedWorldIDs()
	{
		return new List<string>(locallyReportedWorldIDs);
	}

	public List<string> ReportedModelIDs()
	{
		return new List<string>(locallyReportedModelIDs);
	}

	public void LikeWorldRemote(string worldId)
	{
		string path = $"/api/v1/worlds/{worldId}/likes";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", path);
		bWAPIRequestBase.AddParam("action", "add_like");
		bWAPIRequestBase.onSuccess = delegate
		{
			NotifyCurrentUserDataListeners();
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void UnlikeWorldRemote(string worldId)
	{
		string path = $"/api/v1/worlds/{worldId}/likes";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", path);
		bWAPIRequestBase.AddParam("action", "remove_like");
		bWAPIRequestBase.onSuccess = delegate
		{
			NotifyCurrentUserDataListeners();
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void LoadFollowers()
	{
		string path = $"/api/v1/user/{BWUser.currentUser.userID}/followers";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			followers = new List<BWSocialUser>();
			List<JObject> arrayValue = responseJson["attrs_for_follow_users"].ArrayValue;
			if (arrayValue != null)
			{
				foreach (JObject item2 in arrayValue)
				{
					BWSocialUser item = new BWSocialUser(item2);
					followers.Add(item);
				}
			}
			NotifyCurrentUserDataListeners();
		};
		bWAPIRequestBase.onFailure = delegate
		{
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void LoadFollowedUsers()
	{
		string path = $"/api/v1/user/{BWUser.currentUser.userID}/followed_users";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			followedUsers = new List<BWSocialUser>();
			List<JObject> arrayValue = responseJson["attrs_for_follow_users"].ArrayValue;
			if (arrayValue != null)
			{
				foreach (JObject item2 in arrayValue)
				{
					BWSocialUser item = new BWSocialUser(item2);
					followedUsers.Add(item);
				}
			}
			NotifyCurrentUserDataListeners();
		};
		bWAPIRequestBase.onFailure = delegate
		{
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void FollowUser(string userIDStr)
	{
		string path = $"/api/v1/user/{userIDStr}/follow_activity";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", path);
		BWStandalone.Overlays.SetUIBusy(busy: true);
		bWAPIRequestBase.onSuccess = delegate
		{
			LoadFollowedUsers();
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		bWAPIRequestBase.onFailure = delegate
		{
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void UnfollowUser(string userIDStr)
	{
		string path = $"/api/v1/user/{userIDStr}/follow_activity";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("DELETE", path);
		BWStandalone.Overlays.SetUIBusy(busy: true);
		bWAPIRequestBase.onSuccess = delegate
		{
			LoadFollowedUsers();
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		bWAPIRequestBase.onFailure = delegate
		{
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public bool CurrentUserIsFollowing(int userID)
	{
		foreach (BWSocialUser followedUser in followedUsers)
		{
			if (userID == followedUser.userID)
			{
				return true;
			}
		}
		return false;
	}
}
