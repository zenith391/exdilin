using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

public class BWUserWorldsDataManager : MonoBehaviour
{
	private BWUser user;

	private UIDataManager dataManager;

	private List<BWWorld> deletedWorlds;

	private bool loading;

	private bool fetchedDeletedWorlds;

	private string localWorldsDirectory;

	private static BWUserWorldsDataManager instance;

	internal int userID { get; private set; }

	internal List<BWLocalWorld> localWorlds { get; private set; }

	internal bool localWorldsLoaded { get; private set; }

	public static BWUserWorldsDataManager Instance => instance;

	private event WorldsListChangedEventHandler onWorldListChanged;

	public void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		localWorldsDirectory = BWFilesystem.CurrentUserWorldsFolder;
		localWorlds = new List<BWLocalWorld>();
		instance = this;
	}

	private string LocalWorldFolder(BWLocalWorld localWorld)
	{
		return Path.Combine(localWorldsDirectory, localWorld.localWorldID);
	}

	private string LocalWorldScreenshotPath(BWLocalWorld localWorld)
	{
		return Path.Combine(Path.Combine(localWorldsDirectory, localWorld.localWorldID), "screenshot.png");
	}

	public void AddListener(WorldsListChangedEventHandler listener)
	{
		onWorldListChanged -= listener;
		onWorldListChanged += listener;
	}

	public void RemoveListener(WorldsListChangedEventHandler listener)
	{
		onWorldListChanged -= listener;
	}

	public void ClearListeners()
	{
		this.onWorldListChanged = null;
	}

	private void NotifyListeners()
	{
		if (this.onWorldListChanged != null)
		{
			this.onWorldListChanged();
		}
	}

	public void Remove()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public List<BWLocalWorld> GetPublicWorlds()
	{
		return localWorlds.FindAll((BWLocalWorld world) => world.IsPublic());
	}

	public void LoadWorlds()
	{
		if (!localWorldsLoaded && !loading)
		{
			StartCoroutine(LoadWorldsCoroutine());
		}
	}

	private IEnumerator LoadWorldsCoroutine()
	{
		loading = true;
		if (!fetchedDeletedWorlds || deletedWorlds == null)
		{
			BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/deleted_worlds");
			bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
			{
				deletedWorlds = new List<BWWorld>();
				JObject jObject = responseJson["worlds"];
				foreach (JObject item2 in jObject.ArrayValue)
				{
					BWWorld item = new BWWorld(item2);
					deletedWorlds.Add(item);
				}
				fetchedDeletedWorlds = true;
			};
			bWAPIRequestBase.onFailure = delegate
			{
				fetchedDeletedWorlds = true;
			};
			bWAPIRequestBase.SendOwnerCoroutine(this);
		}
		while (!fetchedDeletedWorlds)
		{
			yield return null;
		}
		yield return StartCoroutine(LoadWorldsFromLocalFiles());
		if (deletedWorlds != null)
		{
			for (int num = localWorlds.Count - 1; num >= 0; num--)
			{
				BWLocalWorld bWLocalWorld = localWorlds[num];
				foreach (BWWorld deletedWorld in deletedWorlds)
				{
					if (bWLocalWorld.isRemote && bWLocalWorld.worldID == deletedWorld.worldID)
					{
						string worldFolder = LocalWorldFolder(bWLocalWorld);
						DeleteLocalWorldFolder(worldFolder);
						localWorlds.RemoveAt(num);
					}
				}
			}
		}
		yield return StartCoroutine(RestoreRemoteWorlds());
		SortWorldList();
		localWorldsLoaded = true;
		loading = false;
		NotifyListeners();
	}

	private void SortWorldList()
	{
		localWorlds.Sort((BWLocalWorld x, BWLocalWorld y) => y.createdAt.CompareTo(x.createdAt));
	}

	public BWLocalWorld GetWorldWithLocalWorldID(string localWorldID)
	{
		if (!localWorldsLoaded)
		{
			BWLog.Error("Local worlds not loaded");
			return null;
		}
		foreach (BWLocalWorld localWorld in localWorlds)
		{
			if (localWorld.localWorldID == localWorldID)
			{
				return localWorld;
			}
		}
		return null;
	}

	public BWLocalWorld GetLocalWorldWithRemoteID(string worldID)
	{
		if (!localWorldsLoaded)
		{
			BWLog.Error("Local worlds not loaded");
			return null;
		}
		foreach (BWLocalWorld localWorld in localWorlds)
		{
			if (localWorld.worldID == worldID)
			{
				return localWorld;
			}
		}
		return null;
	}

	public int PublishedWorldCount()
	{
		int count = 0;
		localWorlds.ForEach(delegate(BWLocalWorld world)
		{
			if (world.IsPublic())
			{
				count++;
			}
		});
		return count;
	}

	public BWLocalWorld CreateNewWorldFromTemplate(BWWorldTemplate template)
	{
		BWLocalWorld bWLocalWorld = new BWLocalWorld(template);
		bWLocalWorld.SetCreatedAtTime();
		localWorlds.Insert(0, bWLocalWorld);
		SaveWorldLocal(bWLocalWorld, null);
		return bWLocalWorld;
	}

	public BWLocalWorld CloneLocalWorld(string localWorldID)
	{
		BWLocalWorld worldWithLocalWorldID = GetWorldWithLocalWorldID(localWorldID);
		if (worldWithLocalWorldID == null)
		{
			BWLog.Error("No local world with id: " + localWorldID);
			return null;
		}
		BWLocalWorld bWLocalWorld = new BWLocalWorld(worldWithLocalWorldID);
		bWLocalWorld.ChangeTitle("Copy of - " + worldWithLocalWorldID.title);
		bWLocalWorld.SetCreatedAtTime();
		if (string.IsNullOrEmpty(bWLocalWorld.source))
		{
			string text = LocalWorldFolder(worldWithLocalWorldID);
			if (!Directory.Exists(text))
			{
				BWLog.Error("World Folder " + text + " not found.");
				return null;
			}
			string path = Path.Combine(text, "source.bw");
			bWLocalWorld.OverwriteSource(File.ReadAllText(path), worldWithLocalWorldID.hasWinCondition);
		}
		SaveWorldLocal(bWLocalWorld, null);
		string text2 = LocalWorldScreenshotPath(worldWithLocalWorldID);
		if (File.Exists(text2))
		{
			string text3 = LocalWorldScreenshotPath(bWLocalWorld);
			File.Copy(text2, text3);
			bWLocalWorld.OverwriteImageURLsWithLocalFilePath(text3);
		}
		localWorlds.Insert(0, bWLocalWorld);
		return bWLocalWorld;
	}

	public void SaveWorldLocal(BWLocalWorld worldInfo, byte[] imageData, bool notifyListeners = true)
	{
		if (!Directory.Exists(localWorldsDirectory))
		{
			Directory.CreateDirectory(localWorldsDirectory);
		}
		if (!Directory.Exists(localWorldsDirectory))
		{
			return;
		}
		if (string.IsNullOrEmpty(worldInfo.localWorldID))
		{
			BWLog.Error("BWLocalWorld needs to be assigned a local world ID before being saved");
			return;
		}
		string text = LocalWorldFolder(worldInfo);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		bool flag = imageData != null;
		string text2 = LocalWorldScreenshotPath(worldInfo);
		if (flag)
		{
			if (!File.Exists(text2))
			{
				File.Create(text2).Dispose();
			}
			File.WriteAllBytes(text2, imageData);
			MainUIController.Instance.imageManager.SetUnloaded(BWFilesystem.FileProtocolPrefixStr + text2);
			worldInfo.OverwriteImageURLsWithLocalFilePath(BWFilesystem.FileProtocolPrefixStr + text2);
			worldInfo.GenerateScreenshotChecksum(imageData);
		}
		Dictionary<string, object> obj = worldInfo.MetadataAttributes();
		string contents = JSONEncoder.Encode(obj);
		string path = Path.Combine(text, "metadata.json");
		if (!File.Exists(path))
		{
			File.Create(path).Dispose();
		}
		File.WriteAllText(path, contents);
		string obfuscatedSource = worldInfo.obfuscatedSource;
		string path2 = Path.Combine(text, "source.bw");
		if (!File.Exists(path2))
		{
			File.Create(path2).Dispose();
		}
		File.WriteAllText(path2, obfuscatedSource);
		if (notifyListeners)
		{
			NotifyListeners();
		}
	}

	public void OverwriteScreenshot(BWLocalWorld localWorld, byte[] imageData)
	{
		string text = LocalWorldScreenshotPath(localWorld);
		if (!File.Exists(text))
		{
			File.Create(text).Dispose();
		}
		File.WriteAllBytes(text, imageData);
		MainUIController.Instance.imageManager.SetUnloaded(BWFilesystem.FileProtocolPrefixStr + text);
		localWorld.OverwriteImageURLsWithLocalFilePath(BWFilesystem.FileProtocolPrefixStr + text);
		localWorld.GenerateScreenshotChecksum(imageData);
		localWorld.screenshotTakenManually = true;
		localWorld.localChangedScreenshot = true;
		SaveWorldMetadataLocal(localWorld);
		if (localWorld.publicationStatus == BWWorld.PublicationStatus.NOT_PUBLISHED)
		{
			UpdateRemoteWorld(localWorld, imageData);
		}
	}

	public void SaveWorldMetadataLocal(BWLocalWorld worldInfo, bool notifyListeners = true)
	{
		if (!Directory.Exists(localWorldsDirectory))
		{
			Directory.CreateDirectory(localWorldsDirectory);
		}
		if (Directory.Exists(localWorldsDirectory))
		{
			string text = LocalWorldFolder(worldInfo);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			Dictionary<string, object> obj = worldInfo.MetadataAttributes();
			string contents = JSONEncoder.Encode(obj);
			string path = Path.Combine(text, "metadata.json");
			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}
			File.WriteAllText(path, contents);
			worldInfo.changedFromLocalSave = false;
			if (notifyListeners)
			{
				NotifyListeners();
			}
		}
	}

	private IEnumerator LoadWorldsFromLocalFiles()
	{
		if (!Directory.Exists(localWorldsDirectory))
		{
			yield break;
		}
		float startTime = Time.time;
		string[] worldFolders = Directory.GetDirectories(localWorldsDirectory);
		HashSet<string> loadedWorldIDs = new HashSet<string>();
		Dictionary<string, WWW> metaDataLoaders = new Dictionary<string, WWW>();
		int num = 0;
		for (int i = 0; i < worldFolders.Length; i++)
		{
			string text = worldFolders[i];
			string text2 = Path.Combine(text, "metadata.json");
			string text3 = Path.Combine(text, "source.bw");
			if (!File.Exists(text2) || !File.Exists(text3))
			{
				BWLog.Error("Invalid world data in folder: " + text);
				continue;
			}
			FileInfo fileInfo = new FileInfo(text3);
			FileInfo fileInfo2 = new FileInfo(text2);
			if (fileInfo.Length == 0L || fileInfo2.Length == 0L)
			{
				BWLog.Error("zero size world data in folder: " + text);
				continue;
			}
			WWW value = new WWW(BWFilesystem.FileProtocolPrefixStr + text2);
			metaDataLoaders.Add(text, value);
			num++;
			if (num >= 128 || i == worldFolders.Length - 1)
			{
				yield return StartCoroutine(ProcessMetaDataLoaders(metaDataLoaders, loadedWorldIDs));
				metaDataLoaders.Clear();
				num = 0;
			}
		}
		float time = Time.time;
		BWLog.Info("Loaded: " + loadedWorldIDs.Count + " worlds in " + (time - startTime).ToString());
	}

	private IEnumerator ProcessMetaDataLoaders(Dictionary<string, WWW> metaDataLoaders, HashSet<string> loadedWorldIDs)
	{
		bool loadComplete = false;
		while (!loadComplete)
		{
			loadComplete = true;
			HashSet<string> hashSet = new HashSet<string>();
			foreach (KeyValuePair<string, WWW> metaDataLoader in metaDataLoaders)
			{
				string key = metaDataLoader.Key;
				if (hashSet.Contains(key))
				{
					continue;
				}
				hashSet.Add(key);
				WWW value = metaDataLoader.Value;
				if (value.isDone)
				{
					string text = value.text;
					if (text.Length == 0)
					{
						BWLog.Error("Invalid world metadata in folder: " + key);
						continue;
					}
					JObject jsonObj = JSONDecoder.Decode(text);
					BWLocalWorld bWLocalWorld = new BWLocalWorld(jsonObj);
					if (string.IsNullOrEmpty(bWLocalWorld.localWorldID))
					{
						BWLog.Error("Invalid world in folder: " + key);
					}
					else if (bWLocalWorld.authorID == BWUser.currentUser.userID && (string.IsNullOrEmpty(bWLocalWorld.worldID) || !loadedWorldIDs.Contains(bWLocalWorld.worldID)))
					{
						string text2 = Path.Combine(key, "screenshot.png");
						if (File.Exists(text2))
						{
							bWLocalWorld.OverwriteImageURLsWithLocalFilePath(BWFilesystem.FileProtocolPrefixStr + text2);
						}
						loadedWorldIDs.Add(bWLocalWorld.worldID);
						localWorlds.Add(bWLocalWorld);
					}
				}
				else
				{
					loadComplete = false;
				}
			}
			yield return null;
		}
	}

	private IEnumerator RestoreRemoteWorlds()
	{
		HashSet<BWLocalWorld> restoredWorlds = new HashSet<BWLocalWorld>();
		foreach (BWWorld world in BWUser.currentUser.worlds)
		{
			BWLocalWorld restoredLocalWorld = null;
			BWLocalWorld localCopy = null;
			foreach (BWLocalWorld localWorld in localWorlds)
			{
				if (world.worldID == localWorld.worldID)
				{
					localCopy = localWorld;
					break;
				}
			}
			if (localCopy != null)
			{
				if (!(localCopy.updatedAt < world.updatedAt))
				{
					continue;
				}
				bool flag = localCopy.HasLocalChanges();
				bool overwrite = !flag;
				if (flag)
				{
					BWStandalone.Overlays.SetUIBusy(busy: false);
					BWStandalone.Overlays.ShowConfirmationDialog("Conflict detected.", "Your world \"" + localCopy.title + "\"\n has been modified both in this device and from another device.\n\nYou need to resolve the conflict by selecting the right version.\n What version do you want to keep?", delegate
					{
						overwrite = true;
					}, "Version on \"other\" device", "Version on this device");
					while (BWStandalone.Overlays.IsShowingPopup())
					{
						yield return null;
					}
					BWStandalone.Overlays.SetUIBusy(busy: true);
				}
				if (overwrite)
				{
					Debug.Log("Overwriting " + localCopy.title);
					restoredLocalWorld = new BWLocalWorld(world, localCopy.localWorldID);
				}
				else
				{
					BWStandalone.Overlays.SetUIBusy(busy: false);
				}
			}
			else
			{
				restoredLocalWorld = new BWLocalWorld(world);
			}
			if (restoredLocalWorld == null)
			{
				continue;
			}
			restoredWorlds.Add(restoredLocalWorld);
			string path = $"/api/v1/worlds/{world.worldID}";
			BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
			bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
			{
				BWStandalone.Overlays.SetUIBusy(busy: false);
				BWLog.Error(error.message);
				restoredWorlds.Remove(restoredLocalWorld);
			};
			bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
			{
				BWStandalone.Overlays.SetUIBusy(busy: false);
				JObject jObject = responseJson["world"];
				if (jObject == null)
				{
					BWLog.Info("Failed to parse response for world " + world.worldID);
					restoredWorlds.Remove(restoredLocalWorld);
				}
				else
				{
					restoredLocalWorld.UpdateFromJson(jObject);
				}
				if (string.IsNullOrEmpty(restoredLocalWorld.source))
				{
					BWLog.Info("Failed to load remote source for world " + world.worldID);
					restoredWorlds.Remove(restoredLocalWorld);
				}
			};
			bWAPIRequestBase.SendOwnerCoroutine(this);
		}
		bool flag2 = false;
		while (!flag2 && restoredWorlds.Count > 0)
		{
			yield return null;
			bool flag3 = true;
			foreach (BWLocalWorld item in restoredWorlds)
			{
				if (string.IsNullOrEmpty(item.source))
				{
					flag3 = false;
					break;
				}
			}
			flag2 = flag3;
		}
		foreach (BWLocalWorld item2 in restoredWorlds)
		{
			for (int num = localWorlds.Count - 1; num >= 0; num--)
			{
				BWLocalWorld bWLocalWorld = localWorlds[num];
				if (bWLocalWorld.localWorldID == item2.localWorldID)
				{
					DeleteLocalWorldFolder(LocalWorldFolder(bWLocalWorld));
					localWorlds.RemoveAt(num);
				}
			}
			BWLog.Info("Restored world with id: " + item2.worldID + ", saving local copy");
			SaveWorldLocal(item2, null, notifyListeners: false);
			localWorlds.Add(item2);
		}
	}

	public void LoadSourceForLocalWorld(BWLocalWorld world, UnityAction completion)
	{
		StartCoroutine(CoroutineLoadSourceForLocalWorld(world, completion));
	}

	private IEnumerator CoroutineLoadSourceForLocalWorld(BWLocalWorld world, UnityAction completion)
	{
		if (world.source != null && world.source.Length > 2)
		{
			completion?.Invoke();
			yield break;
		}
		string text = LocalWorldFolder(world);
		if (!Directory.Exists(text))
		{
			BWLog.Error("World Folder " + text + " not found.");
			yield break;
		}
		string text2 = Path.Combine(text, "source.bw");
		if (!File.Exists(text2))
		{
			LoadRemoteSourceForLocalWorld(world, completion);
			yield break;
		}
		WWW sourceLoader = new WWW(BWFilesystem.FileProtocolPrefixStr + text2);
		while (!sourceLoader.isDone)
		{
			yield return null;
		}
		string text3 = sourceLoader.text;
		if (!world.OverwriteSource(text3, world.hasWinCondition))
		{
			LoadRemoteSourceForLocalWorld(world, completion);
		}
		else
		{
			completion?.Invoke();
		}
	}

	private void LoadRemoteSourceForLocalWorld(BWLocalWorld localWorld, UnityAction completion)
	{
		if (!localWorld.isRemote)
		{
			return;
		}
		string remoteWorldID = localWorld.worldID;
		string path = $"/api/v1/worlds/{remoteWorldID}";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
		};
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			JObject jObject = responseJson["world"];
			if (jObject == null)
			{
				BWLog.Info("Failed to parse response for world " + remoteWorldID);
			}
			else
			{
				BWWorld bWWorld = new BWWorld(jObject);
				localWorld.OverwriteSource(bWWorld.source, bWWorld.hasWinCondition);
				if (completion != null)
				{
					completion();
				}
			}
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void DeleteLocalWorld(string worldID, bool notifyListeners = true)
	{
		BWLocalWorld worldWithLocalWorldID = GetWorldWithLocalWorldID(worldID);
		if (worldWithLocalWorldID != null)
		{
			if (worldWithLocalWorldID.isRemote)
			{
				DeleteWorldRemote(worldWithLocalWorldID.worldID);
			}
			DeleteLocalWorldFolder(LocalWorldFolder(worldWithLocalWorldID));
			localWorlds.Remove(worldWithLocalWorldID);
			if (notifyListeners)
			{
				NotifyListeners();
			}
		}
	}

	private void DeleteLocalWorldFolder(string worldFolder)
	{
		string[] files = Directory.GetFiles(worldFolder);
		string[] array = files;
		foreach (string path in array)
		{
			File.Delete(path);
		}
		Directory.Delete(worldFolder);
	}

	private void DeleteWorldRemote(string worldID)
	{
		string path = $"/api/v1/worlds/{worldID}";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("DELETE", path);
		BWStandalone.Overlays.SetUIBusy(busy: true);
		bWAPIRequestBase.onSuccess = delegate
		{
			BWLog.Info("Deleted world: " + worldID);
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info("Failed to delete world: " + worldID + ", error: " + error.message);
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void DeleteAll()
	{
		if (Directory.Exists(localWorldsDirectory))
		{
			Directory.Delete(localWorldsDirectory, recursive: true);
		}
		localWorlds.Clear();
		NotifyListeners();
	}

	public void CreateRemoteWorld(BWLocalWorld localWorld, byte[] imageData, Action<BWLocalWorld> success = null, Action failure = null)
	{
		if (imageData == null)
		{
			string path = LocalWorldScreenshotPath(localWorld);
			if (File.Exists(path))
			{
				imageData = File.ReadAllBytes(path);
			}
		}
		if (imageData == null)
		{
			BWLog.Error("Must provide screenshot to create new remote world");
			return;
		}
		if (!BWEncript.VerifyChecksum(localWorld.screenshotChecksumStr, imageData))
		{
			BWLog.Error("suspicious screenshot");
			return;
		}
		if (string.IsNullOrEmpty(localWorld.source))
		{
			LoadSourceForLocalWorld(localWorld, delegate
			{
				if (!string.IsNullOrEmpty(localWorld.source))
				{
					CreateRemoteWorld(localWorld, imageData, success, failure);
				}
			});
			return;
		}
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v1/worlds");
		bWAPIRequestBase.AddParams(localWorld.AttrsToCreateRemote());
		bWAPIRequestBase.AddImageData("screenshot_image", imageData, "screenshot.jpg", "image/png");
		BWStandalone.Overlays.SetUIBusy(busy: true);
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			Debug.Log("Create remote success!");
			JObject json = responseJson["world"];
			localWorld.UpdateFromJson(json);
			SaveWorldMetadataLocal(localWorld);
			if (success != null)
			{
				success(localWorld);
			}
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			Debug.Log("Create remote failed: " + error.message);
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void UpdateRemoteWorld(BWLocalWorld localWorld, byte[] imageData = null, Action<BWLocalWorld> success = null, Action failure = null)
	{
		if (!localWorld.isRemote)
		{
			CreateRemoteWorld(localWorld, imageData, success, failure);
			return;
		}
		if (!localWorld.HasLocalChanges())
		{
			if (success != null)
			{
				success(localWorld);
			}
			return;
		}
		if (localWorld.localChangedSource && string.IsNullOrEmpty(localWorld.source))
		{
			LoadSourceForLocalWorld(localWorld, delegate
			{
				if (!string.IsNullOrEmpty(localWorld.source))
				{
					UpdateRemoteWorld(localWorld, imageData, success, failure);
				}
			});
			return;
		}
		string path = $"/api/v1/worlds/{localWorld.worldID}";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("PUT", path);
		bWAPIRequestBase.AddParams(localWorld.AttrsToSaveChangesRemote());
		if (localWorld.localChangedScreenshot)
		{
			if (imageData == null)
			{
				string path2 = LocalWorldScreenshotPath(localWorld);
				if (File.Exists(path2))
				{
					imageData = File.ReadAllBytes(path2);
				}
			}
			if (imageData == null && !localWorld.localChangedMetadata && !localWorld.localChangedSource)
			{
				if (failure != null)
				{
					failure();
				}
				return;
			}
			if (!BWEncript.VerifyChecksum(localWorld.screenshotChecksumStr, imageData))
			{
				BWLog.Info("Invalid screenshot");
				BWStandalone.Overlays.ShowMessage("Sorry, I can't upload the world with this screenshot. Please re-save the world and try again.");
				if (failure != null)
				{
					failure();
				}
				return;
			}
			bWAPIRequestBase.AddImageData("screenshot_image", imageData, "screenshot.jpg", "image/png");
		}
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			JObject jObject = responseJson["world"];
			localWorld.UpdateFromJson(jObject);
			localWorld.StopTrackingLocalChanges();
			SaveWorldMetadataLocal(localWorld, notifyListeners: false);
			BWUser.currentUser.RemoteWorldUpdated(localWorld.worldID, jObject);
			if (success != null)
			{
				success(localWorld);
			}
		};
		bWAPIRequestBase.onFailure = delegate
		{
			if (failure != null)
			{
				failure();
			}
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void RevertLocalChanges(string localWorldID, UnityAction completion)
	{
		BWLocalWorld localWorld = GetWorldWithLocalWorldID(localWorldID);
		if (localWorld == null || !localWorld.isRemote)
		{
			return;
		}
		int worldIndex = localWorlds.IndexOf(localWorld);
		string remoteWorldID = localWorld.worldID;
		string path = $"/api/v1/worlds/{remoteWorldID}";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
		};
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			BWLog.Info("Reverting world with id: " + remoteWorldID);
			JObject jObject = responseJson["world"];
			if (jObject == null)
			{
				BWLog.Info("Failed to parse response for world " + remoteWorldID);
			}
			else
			{
				BWWorld copyFrom = new BWWorld(jObject);
				BWLocalWorld bWLocalWorld = new BWLocalWorld(copyFrom, localWorldID);
				if (bWLocalWorld != null && !string.IsNullOrEmpty(bWLocalWorld.source))
				{
					DeleteLocalWorldFolder(LocalWorldFolder(localWorld));
					localWorlds.Remove(localWorld);
					SaveWorldLocal(bWLocalWorld, null, notifyListeners: false);
					localWorlds.Insert(worldIndex, bWLocalWorld);
					NotifyListeners();
					if (completion != null)
					{
						completion();
					}
				}
			}
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void PublishLocalWorld(string localWorldID)
	{
		BWLog.Info("Publishing local world " + localWorldID);
		BWStandalone.Overlays.SetUIBusy(busy: true);
		BWLocalWorld localWorld = GetWorldWithLocalWorldID(localWorldID);
		if (localWorld == null)
		{
			return;
		}
		BWLog.Info("Publishing world");
		Action<BWLocalWorld> success = delegate(BWLocalWorld world)
		{
			string path = $"/api/v1/worlds/{world.worldID}/published_status";
			BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("PUT", path);
			bWAPIRequestBase.AddParam("is_published", "true");
			bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
			{
				BWLog.Info("Publish success");
				localWorld.UpdateFromJson(responseJson);
				BWUser.currentUser.RemoteWorldUpdated(localWorld.worldID, responseJson);
				SaveWorldMetadataLocal(localWorld, notifyListeners: false);
				NotifyListeners();
				BWStandalone.Overlays.SetUIBusy(busy: false);
				BWStandalone.Overlays.notifications.ShowNotification(localWorld.title + " Published successfully");
				BWWorldPublishCooldown.SetWorldTimestamp(localWorld.updatedAt, localWorld.worldID);
			};
			bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
			{
				if (error.httpStatusCode == 402)
				{
					BWLog.Info("World publication on cooldown!");
					BWStandalone.Overlays.ShowMessage(BWMenuTextEnum.WorldPublishOnCooldownMessage);
				}
				else if (error.message == "profanity_filter_error")
				{
					BWLog.Info("World failed profanity check!");
					BWStandalone.Overlays.ShowMessage(BWMenuTextEnum.PublishFailedProfanityCheckMessage);
				}
				else
				{
					BWLog.Info("Publish fail " + error.httpStatusCode + ": " + error.message);
				}
				BWStandalone.Overlays.SetUIBusy(busy: false);
			};
			bWAPIRequestBase.SendOwnerCoroutine(this);
		};
		Action failure = delegate
		{
			BWLog.Info("Failed to update remote world prior to publishing");
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		UpdateRemoteWorld(localWorld, null, success, failure);
	}

	public void UnpublishLocalWorld(string localWorldID)
	{
		BWLocalWorld localWorld = GetWorldWithLocalWorldID(localWorldID);
		if (localWorld != null)
		{
			BWStandalone.Overlays.SetUIBusy(busy: true);
			string path = $"/api/v1/worlds/{localWorld.worldID}/published_status";
			BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("PUT", path);
			bWAPIRequestBase.AddParam("is_published", "false");
			bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
			{
				localWorld.UpdateFromJson(responseJson);
				SaveWorldMetadataLocal(localWorld);
				NotifyListeners();
				BWStandalone.Overlays.SetUIBusy(busy: false);
				BWStandalone.Overlays.notifications.ShowNotification(localWorld.title + " Unpublished");
			};
			bWAPIRequestBase.onFailure = delegate
			{
				BWStandalone.Overlays.SetUIBusy(busy: false);
			};
			bWAPIRequestBase.SendOwnerCoroutine(this);
		}
	}
}
