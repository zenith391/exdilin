using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003CE RID: 974
public class BWUserDataManager : MonoBehaviour
{
	// Token: 0x170001F9 RID: 505
	// (get) Token: 0x06002A7F RID: 10879 RVA: 0x001354D7 File Offset: 0x001338D7
	// (set) Token: 0x06002A80 RID: 10880 RVA: 0x001354DF File Offset: 0x001338DF
	internal BWProfileWorld currentUserProfileWorld { get; private set; }

	// Token: 0x170001FA RID: 506
	// (get) Token: 0x06002A81 RID: 10881 RVA: 0x001354E8 File Offset: 0x001338E8
	// (set) Token: 0x06002A82 RID: 10882 RVA: 0x001354F0 File Offset: 0x001338F0
	internal bool userProfileSyncComplete { get; private set; }

	// Token: 0x1400001B RID: 27
	// (add) Token: 0x06002A83 RID: 10883 RVA: 0x001354FC File Offset: 0x001338FC
	// (remove) Token: 0x06002A84 RID: 10884 RVA: 0x00135534 File Offset: 0x00133934
	private event ProfileChangedEventHandler onProfileChanged;

	// Token: 0x1400001C RID: 28
	// (add) Token: 0x06002A85 RID: 10885 RVA: 0x0013556C File Offset: 0x0013396C
	// (remove) Token: 0x06002A86 RID: 10886 RVA: 0x001355A4 File Offset: 0x001339A4
	private event CurrentUserDataChangedEventHandler onCurrentUserDataChanged;

	// Token: 0x170001FB RID: 507
	// (get) Token: 0x06002A87 RID: 10887 RVA: 0x001355DA File Offset: 0x001339DA
	public static BWUserDataManager Instance
	{
		get
		{
			return BWUserDataManager.instance;
		}
	}

	// Token: 0x06002A88 RID: 10888 RVA: 0x001355E1 File Offset: 0x001339E1
	public void Awake()
	{
		if (BWUserDataManager.instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		BWUserDataManager.instance = this;
	}

	// Token: 0x06002A89 RID: 10889 RVA: 0x00135605 File Offset: 0x00133A05
	public void AddListener(ProfileChangedEventHandler listener)
	{
		this.onProfileChanged -= listener;
		this.onProfileChanged += listener;
	}

	// Token: 0x06002A8A RID: 10890 RVA: 0x00135615 File Offset: 0x00133A15
	public void RemoveListener(ProfileChangedEventHandler listener)
	{
		this.onProfileChanged -= listener;
	}

	// Token: 0x06002A8B RID: 10891 RVA: 0x0013561E File Offset: 0x00133A1E
	public void AddCurrentUserDataChangedListener(CurrentUserDataChangedEventHandler listener)
	{
		this.onCurrentUserDataChanged -= listener;
		this.onCurrentUserDataChanged += listener;
	}

	// Token: 0x06002A8C RID: 10892 RVA: 0x0013562E File Offset: 0x00133A2E
	public void RemoveCurrentUserDataChangedListener(CurrentUserDataChangedEventHandler listener)
	{
		this.onCurrentUserDataChanged -= listener;
	}

	// Token: 0x06002A8D RID: 10893 RVA: 0x00135637 File Offset: 0x00133A37
	public void ClearListeners()
	{
		this.onProfileChanged = null;
		this.onCurrentUserDataChanged = null;
	}

	// Token: 0x06002A8E RID: 10894 RVA: 0x00135647 File Offset: 0x00133A47
	public void NotifyListeners()
	{
		if (this.onProfileChanged != null)
		{
			this.onProfileChanged();
		}
	}

	// Token: 0x06002A8F RID: 10895 RVA: 0x0013565F File Offset: 0x00133A5F
	private void NotifyCurrentUserDataListeners()
	{
		if (this.onCurrentUserDataChanged != null)
		{
			this.onCurrentUserDataChanged();
		}
	}

	// Token: 0x06002A90 RID: 10896 RVA: 0x00135677 File Offset: 0x00133A77
	private string CurrentUserProfileWorldPath()
	{
		return Path.Combine(BWFilesystem.CurrentUserProfileWorldFolder, "profileWorld.bw");
	}

	// Token: 0x06002A91 RID: 10897 RVA: 0x00135688 File Offset: 0x00133A88
	private string CurrentUserProfilePicturePath()
	{
		return Path.Combine(BWFilesystem.CurrentUserProfileWorldFolder, "profile.png");
	}

	// Token: 0x06002A92 RID: 10898 RVA: 0x00135699 File Offset: 0x00133A99
	private string CurrentUserDataPath()
	{
		return Path.Combine(BWFilesystem.CurrentUserDataFolder, "userData.json");
	}

	// Token: 0x06002A93 RID: 10899 RVA: 0x001356AA File Offset: 0x00133AAA
	private string CurrentUserShoppingCartPath()
	{
		return Path.Combine(BWFilesystem.CurrentUserDataFolder, "shoppingCart.json");
	}

	// Token: 0x06002A94 RID: 10900 RVA: 0x001356BB File Offset: 0x00133ABB
	public void SyncCurrentUserProfile()
	{
		base.StartCoroutine(this.SyncCurrentUserProfileCoroutine());
	}

	// Token: 0x06002A95 RID: 10901 RVA: 0x001356CC File Offset: 0x00133ACC
	private IEnumerator SyncCurrentUserProfileCoroutine()
	{
		this.currentUserProfileWorld = this.LoadCurrentProfileWorldLocal();
		this.userProfileSyncComplete = false;
		bool worldSyncComplete = false;
		bool worldRestoredFromRemote = false;
		BWAPIRequestBase request = BW.API.CreateRequest("GET", "/api/v1/current_user/profile_world");
		if (this.currentUserProfileWorld != null)
		{
			string valueStr = this.currentUserProfileWorld.updatedAt.ToString("s", CultureInfo.InvariantCulture);
			request.AddParam("updated_at_timestamp", valueStr);
		}
		request.onSuccess = delegate(JObject responseJson)
		{
			worldSyncComplete = true;
			if (responseJson.ContainsKey("profile_world"))
			{
				this.currentUserProfileWorld = new BWProfileWorld(responseJson["profile_world"]);
				this.SaveCurrentUserProfileWorld();
				worldRestoredFromRemote = true;
			}
		};
		request.onFailure = delegate(BWAPIRequestError error)
		{
			worldSyncComplete = true;
		};
		request.SendOwnerCoroutine(this);
		while (!worldSyncComplete)
		{
			yield return null;
		}
		if (this.currentUserProfileWorld == null)
		{
			this.userProfileSyncComplete = true;
			yield break;
		}
		string profilePicturePath = this.CurrentUserProfilePicturePath();
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
		this.userProfileSyncComplete = true;
		yield break;
	}

	// Token: 0x06002A96 RID: 10902 RVA: 0x001356E7 File Offset: 0x00133AE7
	private BWProfileWorld CreateNewProfileWorld()
	{
		this.currentUserProfileWorld = new BWProfileWorld(BWUser.currentUser);
		this.SaveCurrentUserProfileWorld();
		return this.currentUserProfileWorld;
	}

	// Token: 0x06002A97 RID: 10903 RVA: 0x00135705 File Offset: 0x00133B05
	public BWProfileWorld CreateOrLoadCurrentUserProfileWorld()
	{
		if (this.currentUserProfileWorld == null)
		{
			this.currentUserProfileWorld = this.LoadCurrentProfileWorldLocal();
		}
		if (this.currentUserProfileWorld != null)
		{
			return this.currentUserProfileWorld;
		}
		BWLog.Info("Unable to load existing profile world, creating a new one instead...");
		return this.CreateNewProfileWorld();
	}

	// Token: 0x06002A98 RID: 10904 RVA: 0x00135740 File Offset: 0x00133B40
	private BWProfileWorld LoadCurrentProfileWorldLocal()
	{
		string path = this.CurrentUserProfileWorldPath();
		if (File.Exists(path))
		{
			try
			{
				string source = File.ReadAllText(path);
				JObject jsonObj = JSONDecoder.Decode(Util.UnobfuscateSourceForUser(source, BWUser.currentUser.userID));
				this.currentUserProfileWorld = new BWProfileWorld(jsonObj);
				return this.currentUserProfileWorld;
			}
			catch
			{
				return null;
			}
		}
		return null;
	}

	// Token: 0x06002A99 RID: 10905 RVA: 0x001357B0 File Offset: 0x00133BB0
	public void SaveCurrentUserProfileWorld()
	{
		if (!Directory.Exists(BWFilesystem.CurrentUserProfileWorldFolder))
		{
			Directory.CreateDirectory(BWFilesystem.CurrentUserProfileWorldFolder);
		}
		string path = this.CurrentUserProfileWorldPath();
		if (!File.Exists(path))
		{
			File.Create(path).Dispose();
		}
		Dictionary<string, string> obj = this.currentUserProfileWorld.AttrsToSave();
		string source = JSONEncoder.Encode(obj);
		File.WriteAllText(path, Util.ObfuscateSourceForUser(source, BWUser.currentUser.userID));
	}

	// Token: 0x06002A9A RID: 10906 RVA: 0x00135820 File Offset: 0x00133C20
	public void SaveCurrentUserProfilePicture(byte[] imageData)
	{
		if (!Directory.Exists(BWFilesystem.CurrentUserDataFolder))
		{
			Directory.CreateDirectory(BWFilesystem.CurrentUserDataFolder);
		}
		string text = this.CurrentUserProfilePicturePath();
		if (!File.Exists(text))
		{
			File.Create(text).Dispose();
		}
		File.WriteAllBytes(text, imageData);
		MainUIController.Instance.imageManager.SetUnloaded(BWFilesystem.FileProtocolPrefixStr + text);
		this.NotifyListeners();
	}

	// Token: 0x06002A9B RID: 10907 RVA: 0x0013588C File Offset: 0x00133C8C
	public void UploadCurrentUserProfile(byte[] imageData)
	{
		string path = "/api/v1/current_user/profile_world";
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("PUT", path);
		bwapirequestBase.AddParams(this.currentUserProfileWorld.AttrsToSaveRemote());
		bwapirequestBase.AddImageData("profile_image", imageData, "profilepic.jpg", "image/jpeg");
		bwapirequestBase.onSuccess = delegate(JObject response)
		{
			BWUser.currentUser.UpdateFromJson(response);
			MainUIController.Instance.imageManager.SetUnloaded(BWUser.currentUser.profileImageURL);
			this.NotifyListeners();
		};
		bwapirequestBase.Send();
	}

	// Token: 0x06002A9C RID: 10908 RVA: 0x001358F0 File Offset: 0x00133CF0
	public void LoadUserData()
	{
		string path = this.CurrentUserDataPath();
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
			this.bookmarkedWorldIDs = new HashSet<string>(list);
			this.locallyReportedWorldIDs = new HashSet<string>(list2);
			this.locallyReportedModelIDs = new HashSet<string>(list3);
		}
		else
		{
			this.bookmarkedWorldIDs = new HashSet<string>();
			this.locallyReportedWorldIDs = new HashSet<string>();
			this.locallyReportedModelIDs = new HashSet<string>();
		}
	}

	// Token: 0x06002A9D RID: 10909 RVA: 0x001359A8 File Offset: 0x00133DA8
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
				this.BookmarkedWorldIDs()
			},
			{
				"reported_world_ids",
				this.ReportedWorldIDs()
			},
			{
				"reported_model_ids",
				this.ReportedModelIDs()
			}
		});
		string path = this.CurrentUserDataPath();
		if (!File.Exists(path))
		{
			File.Create(path).Dispose();
		}
		File.WriteAllText(path, contents);
	}

	// Token: 0x06002A9E RID: 10910 RVA: 0x00135A34 File Offset: 0x00133E34
	public JObject LoadShoppingCartJSON()
	{
		string path = this.CurrentUserShoppingCartPath();
		if (File.Exists(path))
		{
			string json = File.ReadAllText(path);
			JObject jObject = null;
			// error handling added by Exdilin
			try {
				jObject = JSONDecoder.Decode(json);
			} catch (ParseError e) {
				BWLog.Error("Error loading the shopping cart: \n" + e.Message + "\n" + e.StackTrace);
			}
			return jObject;
		}
		return null;
	}

	// Token: 0x06002A9F RID: 10911 RVA: 0x00135A64 File Offset: 0x00133E64
	public void SaveShoppingCartJSON(string jsonStr)
	{
		if (!Directory.Exists(BWFilesystem.CurrentUserDataFolder))
		{
			Directory.CreateDirectory(BWFilesystem.CurrentUserDataFolder);
		}
		string path = this.CurrentUserShoppingCartPath();
		if (!File.Exists(path))
		{
			File.Create(path).Dispose();
		}
		File.WriteAllText(path, jsonStr);
	}

	// Token: 0x06002AA0 RID: 10912 RVA: 0x00135AAF File Offset: 0x00133EAF
	public void UpdateUIWithCurrentUserData()
	{
		this.NotifyCurrentUserDataListeners();
	}

	// Token: 0x06002AA1 RID: 10913 RVA: 0x00135AB7 File Offset: 0x00133EB7
	public bool HasBookmarkedWorld(string worldID)
	{
		return this.bookmarkedWorldIDs.Contains(worldID);
	}

	// Token: 0x06002AA2 RID: 10914 RVA: 0x00135AC5 File Offset: 0x00133EC5
	public void BookmarkWorld(string worldID)
	{
		this.bookmarkedWorldIDs.Add(worldID);
		this.LikeWorldRemote(worldID);
		this.SaveUserData();
	}

	// Token: 0x06002AA3 RID: 10915 RVA: 0x00135AE1 File Offset: 0x00133EE1
	public void UnbookmarkWorld(string worldID)
	{
		this.bookmarkedWorldIDs.Remove(worldID);
		this.UnlikeWorldRemote(worldID);
		this.SaveUserData();
	}

	// Token: 0x06002AA4 RID: 10916 RVA: 0x00135AFD File Offset: 0x00133EFD
	public void ReportWorld(string worldID)
	{
		this.locallyReportedWorldIDs.Add(worldID);
		this.SaveUserData();
		this.NotifyCurrentUserDataListeners();
	}

	// Token: 0x06002AA5 RID: 10917 RVA: 0x00135B18 File Offset: 0x00133F18
	public void ReportModel(string modelID)
	{
		this.locallyReportedModelIDs.Add(modelID);
		this.SaveUserData();
		this.NotifyCurrentUserDataListeners();
	}

	// Token: 0x06002AA6 RID: 10918 RVA: 0x00135B33 File Offset: 0x00133F33
	public bool HasReportedWorld(string worldID)
	{
		return this.locallyReportedWorldIDs.Contains(worldID);
	}

	// Token: 0x06002AA7 RID: 10919 RVA: 0x00135B41 File Offset: 0x00133F41
	public bool HasReportedModel(string modelID)
	{
		return this.locallyReportedModelIDs.Contains(modelID);
	}

	// Token: 0x06002AA8 RID: 10920 RVA: 0x00135B4F File Offset: 0x00133F4F
	public List<string> BookmarkedWorldIDs()
	{
		return new List<string>(this.bookmarkedWorldIDs);
	}

	// Token: 0x06002AA9 RID: 10921 RVA: 0x00135B5C File Offset: 0x00133F5C
	public List<string> ReportedWorldIDs()
	{
		return new List<string>(this.locallyReportedWorldIDs);
	}

	// Token: 0x06002AAA RID: 10922 RVA: 0x00135B69 File Offset: 0x00133F69
	public List<string> ReportedModelIDs()
	{
		return new List<string>(this.locallyReportedModelIDs);
	}

	// Token: 0x06002AAB RID: 10923 RVA: 0x00135B78 File Offset: 0x00133F78
	public void LikeWorldRemote(string worldId)
	{
		string path = string.Format("/api/v1/worlds/{0}/likes", worldId);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", path);
		bwapirequestBase.AddParam("action", "add_like");
		bwapirequestBase.onSuccess = delegate(JObject response)
		{
			this.NotifyCurrentUserDataListeners();
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002AAC RID: 10924 RVA: 0x00135BCC File Offset: 0x00133FCC
	public void UnlikeWorldRemote(string worldId)
	{
		string path = string.Format("/api/v1/worlds/{0}/likes", worldId);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", path);
		bwapirequestBase.AddParam("action", "remove_like");
		bwapirequestBase.onSuccess = delegate(JObject response)
		{
			this.NotifyCurrentUserDataListeners();
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002AAD RID: 10925 RVA: 0x00135C20 File Offset: 0x00134020
	public void LoadFollowers()
	{
		string path = string.Format("/api/v1/user/{0}/followers", BWUser.currentUser.userID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			this.followers = new List<BWSocialUser>();
			List<JObject> arrayValue = responseJson["attrs_for_follow_users"].ArrayValue;
			if (arrayValue != null)
			{
				foreach (JObject json in arrayValue)
				{
					BWSocialUser item = new BWSocialUser(json);
					this.followers.Add(item);
				}
			}
			this.NotifyCurrentUserDataListeners();
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002AAE RID: 10926 RVA: 0x00135C94 File Offset: 0x00134094
	public void LoadFollowedUsers()
	{
		string path = string.Format("/api/v1/user/{0}/followed_users", BWUser.currentUser.userID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			this.followedUsers = new List<BWSocialUser>();
			List<JObject> arrayValue = responseJson["attrs_for_follow_users"].ArrayValue;
			if (arrayValue != null)
			{
				foreach (JObject json in arrayValue)
				{
					BWSocialUser item = new BWSocialUser(json);
					this.followedUsers.Add(item);
				}
			}
			this.NotifyCurrentUserDataListeners();
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002AAF RID: 10927 RVA: 0x00135D08 File Offset: 0x00134108
	public void FollowUser(string userIDStr)
	{
		string path = string.Format("/api/v1/user/{0}/follow_activity", userIDStr);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", path);
		BWStandalone.Overlays.SetUIBusy(true);
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			this.LoadFollowedUsers();
			BWStandalone.Overlays.SetUIBusy(false);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWStandalone.Overlays.SetUIBusy(false);
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002AB0 RID: 10928 RVA: 0x00135D7C File Offset: 0x0013417C
	public void UnfollowUser(string userIDStr)
	{
		string path = string.Format("/api/v1/user/{0}/follow_activity", userIDStr);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("DELETE", path);
		BWStandalone.Overlays.SetUIBusy(true);
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			this.LoadFollowedUsers();
			BWStandalone.Overlays.SetUIBusy(false);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWStandalone.Overlays.SetUIBusy(false);
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002AB1 RID: 10929 RVA: 0x00135DF0 File Offset: 0x001341F0
	public bool CurrentUserIsFollowing(int userID)
	{
		foreach (BWSocialUser bwsocialUser in this.followedUsers)
		{
			if (userID == bwsocialUser.userID)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x04002483 RID: 9347
	private static BWUserDataManager instance;

	// Token: 0x04002484 RID: 9348
	private HashSet<string> bookmarkedWorldIDs;

	// Token: 0x04002485 RID: 9349
	private HashSet<string> locallyReportedWorldIDs;

	// Token: 0x04002486 RID: 9350
	private HashSet<string> locallyReportedModelIDs;

	// Token: 0x04002487 RID: 9351
	public List<BWSocialUser> followedUsers;

	// Token: 0x04002488 RID: 9352
	public List<BWSocialUser> followers;
}
