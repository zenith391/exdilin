using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x020003D4 RID: 980
public class BWUserWorldsDataManager : MonoBehaviour
{
	// Token: 0x1700021D RID: 541
	// (get) Token: 0x06002B3E RID: 11070 RVA: 0x00138F82 File Offset: 0x00137382
	// (set) Token: 0x06002B3F RID: 11071 RVA: 0x00138F8A File Offset: 0x0013738A
	internal int userID { get; private set; }

	// Token: 0x1700021E RID: 542
	// (get) Token: 0x06002B40 RID: 11072 RVA: 0x00138F93 File Offset: 0x00137393
	// (set) Token: 0x06002B41 RID: 11073 RVA: 0x00138F9B File Offset: 0x0013739B
	internal List<BWLocalWorld> localWorlds { get; private set; }

	// Token: 0x1700021F RID: 543
	// (get) Token: 0x06002B42 RID: 11074 RVA: 0x00138FA4 File Offset: 0x001373A4
	// (set) Token: 0x06002B43 RID: 11075 RVA: 0x00138FAC File Offset: 0x001373AC
	internal bool localWorldsLoaded { get; private set; }

	// Token: 0x1400001E RID: 30
	// (add) Token: 0x06002B44 RID: 11076 RVA: 0x00138FB8 File Offset: 0x001373B8
	// (remove) Token: 0x06002B45 RID: 11077 RVA: 0x00138FF0 File Offset: 0x001373F0
	private event WorldsListChangedEventHandler onWorldListChanged;

	// Token: 0x17000220 RID: 544
	// (get) Token: 0x06002B46 RID: 11078 RVA: 0x00139026 File Offset: 0x00137426
	public static BWUserWorldsDataManager Instance
	{
		get
		{
			return BWUserWorldsDataManager.instance;
		}
	}

	// Token: 0x06002B47 RID: 11079 RVA: 0x0013902D File Offset: 0x0013742D
	public void Awake()
	{
		if (BWUserWorldsDataManager.instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		this.localWorldsDirectory = BWFilesystem.CurrentUserWorldsFolder;
		this.localWorlds = new List<BWLocalWorld>();
		BWUserWorldsDataManager.instance = this;
	}

	// Token: 0x06002B48 RID: 11080 RVA: 0x00139067 File Offset: 0x00137467
	private string LocalWorldFolder(BWLocalWorld localWorld)
	{
		return Path.Combine(this.localWorldsDirectory, localWorld.localWorldID);
	}

	// Token: 0x06002B49 RID: 11081 RVA: 0x0013907A File Offset: 0x0013747A
	private string LocalWorldScreenshotPath(BWLocalWorld localWorld)
	{
		return Path.Combine(Path.Combine(this.localWorldsDirectory, localWorld.localWorldID), "screenshot.png");
	}

	// Token: 0x06002B4A RID: 11082 RVA: 0x00139097 File Offset: 0x00137497
	public void AddListener(WorldsListChangedEventHandler listener)
	{
		this.onWorldListChanged -= listener;
		this.onWorldListChanged += listener;
	}

	// Token: 0x06002B4B RID: 11083 RVA: 0x001390A7 File Offset: 0x001374A7
	public void RemoveListener(WorldsListChangedEventHandler listener)
	{
		this.onWorldListChanged -= listener;
	}

	// Token: 0x06002B4C RID: 11084 RVA: 0x001390B0 File Offset: 0x001374B0
	public void ClearListeners()
	{
		this.onWorldListChanged = null;
	}

	// Token: 0x06002B4D RID: 11085 RVA: 0x001390B9 File Offset: 0x001374B9
	private void NotifyListeners()
	{
		if (this.onWorldListChanged != null)
		{
			this.onWorldListChanged();
		}
	}

	// Token: 0x06002B4E RID: 11086 RVA: 0x001390D1 File Offset: 0x001374D1
	public void Remove()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x06002B4F RID: 11087 RVA: 0x001390DE File Offset: 0x001374DE
	public List<BWLocalWorld> GetPublicWorlds()
	{
		return this.localWorlds.FindAll((BWLocalWorld world) => world.IsPublic());
	}

	// Token: 0x06002B50 RID: 11088 RVA: 0x00139108 File Offset: 0x00137508
	public void LoadWorlds()
	{
		if (!this.localWorldsLoaded && !this.loading)
		{
			base.StartCoroutine(this.LoadWorldsCoroutine());
		}
	}

	// Token: 0x06002B51 RID: 11089 RVA: 0x00139130 File Offset: 0x00137530
	private IEnumerator LoadWorldsCoroutine()
	{
		this.loading = true;
		if (!this.fetchedDeletedWorlds || this.deletedWorlds == null)
		{
			BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/deleted_worlds");
			bwapirequestBase.onSuccess = delegate(JObject responseJson)
			{
				this.deletedWorlds = new List<BWWorld>();
				JObject jobject = responseJson["worlds"];
				foreach (JObject json in jobject.ArrayValue)
				{
					BWWorld item = new BWWorld(json);
					this.deletedWorlds.Add(item);
				}
				this.fetchedDeletedWorlds = true;
			};
			bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
			{
				this.fetchedDeletedWorlds = true;
			};
			bwapirequestBase.SendOwnerCoroutine(this);
		}
		while (!this.fetchedDeletedWorlds)
		{
			yield return null;
		}
		yield return base.StartCoroutine(this.LoadWorldsFromLocalFiles());
		if (this.deletedWorlds != null)
		{
			for (int i = this.localWorlds.Count - 1; i >= 0; i--)
			{
				BWLocalWorld bwlocalWorld = this.localWorlds[i];
				foreach (BWWorld bwworld in this.deletedWorlds)
				{
					if (bwlocalWorld.isRemote && bwlocalWorld.worldID == bwworld.worldID)
					{
						string worldFolder = this.LocalWorldFolder(bwlocalWorld);
						this.DeleteLocalWorldFolder(worldFolder);
						this.localWorlds.RemoveAt(i);
					}
				}
			}
		}
		yield return base.StartCoroutine(this.RestoreRemoteWorlds());
		this.SortWorldList();
		this.localWorldsLoaded = true;
		this.loading = false;
		this.NotifyListeners();
		yield break;
	}

	// Token: 0x06002B52 RID: 11090 RVA: 0x0013914B File Offset: 0x0013754B
	private void SortWorldList()
	{
		this.localWorlds.Sort((BWLocalWorld x, BWLocalWorld y) => y.createdAt.CompareTo(x.createdAt));
	}

	// Token: 0x06002B53 RID: 11091 RVA: 0x00139178 File Offset: 0x00137578
	public BWLocalWorld GetWorldWithLocalWorldID(string localWorldID)
	{
		if (!this.localWorldsLoaded)
		{
			BWLog.Error("Local worlds not loaded");
			return null;
		}
		foreach (BWLocalWorld bwlocalWorld in this.localWorlds)
		{
			if (bwlocalWorld.localWorldID == localWorldID)
			{
				return bwlocalWorld;
			}
		}
		return null;
	}

	// Token: 0x06002B54 RID: 11092 RVA: 0x00139200 File Offset: 0x00137600
	public BWLocalWorld GetLocalWorldWithRemoteID(string worldID)
	{
		if (!this.localWorldsLoaded)
		{
			BWLog.Error("Local worlds not loaded");
			return null;
		}
		foreach (BWLocalWorld bwlocalWorld in this.localWorlds)
		{
			if (bwlocalWorld.worldID == worldID)
			{
				return bwlocalWorld;
			}
		}
		return null;
	}

	// Token: 0x06002B55 RID: 11093 RVA: 0x00139288 File Offset: 0x00137688
	public int PublishedWorldCount()
	{
		int count = 0;
		this.localWorlds.ForEach(delegate(BWLocalWorld world)
		{
			if (world.IsPublic())
			{
				count++;
			}
		});
		return count;
	}

	// Token: 0x06002B56 RID: 11094 RVA: 0x001392C0 File Offset: 0x001376C0
	public BWLocalWorld CreateNewWorldFromTemplate(BWWorldTemplate template)
	{
		BWLocalWorld bwlocalWorld = new BWLocalWorld(template);
		bwlocalWorld.SetCreatedAtTime();
		this.localWorlds.Insert(0, bwlocalWorld);
		this.SaveWorldLocal(bwlocalWorld, null, true);
		return bwlocalWorld;
	}

	// Token: 0x06002B57 RID: 11095 RVA: 0x001392F4 File Offset: 0x001376F4
	public BWLocalWorld CloneLocalWorld(string localWorldID)
	{
		BWLocalWorld worldWithLocalWorldID = this.GetWorldWithLocalWorldID(localWorldID);
		if (worldWithLocalWorldID == null)
		{
			BWLog.Error("No local world with id: " + localWorldID);
			return null;
		}
		BWLocalWorld bwlocalWorld = new BWLocalWorld(worldWithLocalWorldID);
		bwlocalWorld.ChangeTitle("Copy of - " + worldWithLocalWorldID.title);
		bwlocalWorld.SetCreatedAtTime();
		if (string.IsNullOrEmpty(bwlocalWorld.source))
		{
			string text = this.LocalWorldFolder(worldWithLocalWorldID);
			if (!Directory.Exists(text))
			{
				BWLog.Error("World Folder " + text + " not found.");
				return null;
			}
			string path = Path.Combine(text, "source.bw");
			bwlocalWorld.OverwriteSource(File.ReadAllText(path), worldWithLocalWorldID.hasWinCondition);
		}
		this.SaveWorldLocal(bwlocalWorld, null, true);
		string text2 = this.LocalWorldScreenshotPath(worldWithLocalWorldID);
		if (File.Exists(text2))
		{
			string text3 = this.LocalWorldScreenshotPath(bwlocalWorld);
			File.Copy(text2, text3);
			bwlocalWorld.OverwriteImageURLsWithLocalFilePath(text3);
		}
		this.localWorlds.Insert(0, bwlocalWorld);
		return bwlocalWorld;
	}

	// Token: 0x06002B58 RID: 11096 RVA: 0x001393E4 File Offset: 0x001377E4
	public void SaveWorldLocal(BWLocalWorld worldInfo, byte[] imageData, bool notifyListeners = true)
	{
		if (!Directory.Exists(this.localWorldsDirectory))
		{
			Directory.CreateDirectory(this.localWorldsDirectory);
		}
		if (!Directory.Exists(this.localWorldsDirectory))
		{
			return;
		}
		if (string.IsNullOrEmpty(worldInfo.localWorldID))
		{
			BWLog.Error("BWLocalWorld needs to be assigned a local world ID before being saved");
			return;
		}
		string text = this.LocalWorldFolder(worldInfo);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		bool flag = imageData != null;
		string text2 = this.LocalWorldScreenshotPath(worldInfo);
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
			this.NotifyListeners();
		}
	}

	// Token: 0x06002B59 RID: 11097 RVA: 0x00139538 File Offset: 0x00137938
	public void OverwriteScreenshot(BWLocalWorld localWorld, byte[] imageData)
	{
		string text = this.LocalWorldScreenshotPath(localWorld);
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
		this.SaveWorldMetadataLocal(localWorld, true);
		if (localWorld.publicationStatus == BWWorld.PublicationStatus.NOT_PUBLISHED)
		{
			this.UpdateRemoteWorld(localWorld, imageData, null, null);
		}
	}

	// Token: 0x06002B5A RID: 11098 RVA: 0x001395C8 File Offset: 0x001379C8
	public void SaveWorldMetadataLocal(BWLocalWorld worldInfo, bool notifyListeners = true)
	{
		if (!Directory.Exists(this.localWorldsDirectory))
		{
			Directory.CreateDirectory(this.localWorldsDirectory);
		}
		if (!Directory.Exists(this.localWorldsDirectory))
		{
			return;
		}
		string text = this.LocalWorldFolder(worldInfo);
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
			this.NotifyListeners();
		}
	}

	// Token: 0x06002B5B RID: 11099 RVA: 0x00139668 File Offset: 0x00137A68
	private IEnumerator LoadWorldsFromLocalFiles()
	{
		if (Directory.Exists(this.localWorldsDirectory))
		{
			float startTime = Time.time;
			string[] worldFolders = Directory.GetDirectories(this.localWorldsDirectory);
			HashSet<string> loadedWorldIDs = new HashSet<string>();
			Dictionary<string, WWW> metaDataLoaders = new Dictionary<string, WWW>();
			int currentBatchCount = 0;
			for (int i = 0; i < worldFolders.Length; i++)
			{
				string worldFolder = worldFolders[i];
				string metadataPath = Path.Combine(worldFolder, "metadata.json");
				string sourcePath = Path.Combine(worldFolder, "source.bw");
				if (!File.Exists(metadataPath) || !File.Exists(sourcePath))
				{
					BWLog.Error("Invalid world data in folder: " + worldFolder);
				}
				else
				{
					FileInfo sourceFileInfo = new FileInfo(sourcePath);
					FileInfo metadataFileInfo = new FileInfo(metadataPath);
					if (sourceFileInfo.Length == 0L || metadataFileInfo.Length == 0L)
					{
						BWLog.Error("zero size world data in folder: " + worldFolder);
					}
					else
					{
						WWW metadataLoader = new WWW(BWFilesystem.FileProtocolPrefixStr + metadataPath);
						metaDataLoaders.Add(worldFolder, metadataLoader);
						currentBatchCount++;
						if (currentBatchCount >= 128 || i == worldFolders.Length - 1)
						{
							yield return base.StartCoroutine(this.ProcessMetaDataLoaders(metaDataLoaders, loadedWorldIDs));
							metaDataLoaders.Clear();
							currentBatchCount = 0;
						}
					}
				}
			}
			float endTime = Time.time;
			BWLog.Info(string.Concat(new object[]
			{
				"Loaded: ",
				loadedWorldIDs.Count,
				" worlds in ",
				(endTime - startTime).ToString()
			}));
		}
		yield break;
	}

	// Token: 0x06002B5C RID: 11100 RVA: 0x00139684 File Offset: 0x00137A84
	private IEnumerator ProcessMetaDataLoaders(Dictionary<string, WWW> metaDataLoaders, HashSet<string> loadedWorldIDs)
	{
		bool loadComplete = false;
		while (!loadComplete)
		{
			loadComplete = true;
			HashSet<string> loadedFolders = new HashSet<string>();
			foreach (KeyValuePair<string, WWW> keyValuePair in metaDataLoaders)
			{
				string key = keyValuePair.Key;
				if (!loadedFolders.Contains(key))
				{
					loadedFolders.Add(key);
					WWW value = keyValuePair.Value;
					if (value.isDone)
					{
						string text = value.text;
						if (text.Length == 0)
						{
							BWLog.Error("Invalid world metadata in folder: " + key);
						}
						else
						{
							JObject jsonObj = JSONDecoder.Decode(text);
							BWLocalWorld bwlocalWorld = new BWLocalWorld(jsonObj);
							if (string.IsNullOrEmpty(bwlocalWorld.localWorldID))
							{
								BWLog.Error("Invalid world in folder: " + key);
							}
							else if (bwlocalWorld.authorID == BWUser.currentUser.userID)
							{
								if (string.IsNullOrEmpty(bwlocalWorld.worldID) || !loadedWorldIDs.Contains(bwlocalWorld.worldID))
								{
									string text2 = Path.Combine(key, "screenshot.png");
									if (File.Exists(text2))
									{
										bwlocalWorld.OverwriteImageURLsWithLocalFilePath(BWFilesystem.FileProtocolPrefixStr + text2);
									}
									loadedWorldIDs.Add(bwlocalWorld.worldID);
									this.localWorlds.Add(bwlocalWorld);
								}
							}
						}
					}
					else
					{
						loadComplete = false;
					}
				}
			}
			yield return null;
		}
		yield break;
	}

	// Token: 0x06002B5D RID: 11101 RVA: 0x001396B0 File Offset: 0x00137AB0
	private IEnumerator RestoreRemoteWorlds()
	{
		HashSet<BWLocalWorld> restoredWorlds = new HashSet<BWLocalWorld>();
		using (List<BWWorld>.Enumerator enumerator = BWUser.currentUser.worlds.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				//BWUserWorldsDataManager.<RestoreRemoteWorlds>c__Iterator3.<RestoreRemoteWorlds>c__AnonStorey9 <RestoreRemoteWorlds>c__AnonStorey2 = new BWUserWorldsDataManager.<RestoreRemoteWorlds>c__Iterator3.<RestoreRemoteWorlds>c__AnonStorey9();
				BWWorld world = enumerator.Current;
				BWLocalWorld restoredLocalWorld = null;
				BWLocalWorld localCopy = null;
				foreach (BWLocalWorld bwlocalWorld in this.localWorlds)
				{
					if (world.worldID == bwlocalWorld.worldID)
					{
						localCopy = bwlocalWorld;
						break;
					}
				}
				if (localCopy != null)
				{
					if (!(localCopy.updatedAt < world.updatedAt))
					{
						continue;
					}
					bool hasLocalChanges = localCopy.HasLocalChanges();
					bool overwrite = !hasLocalChanges;
					if (hasLocalChanges)
					{
						BWStandalone.Overlays.SetUIBusy(false);
						BWStandalone.Overlays.ShowConfirmationDialog("Conflict detected.", "Your world \"" + localCopy.title + "\"\n has been modified both in this device and from another device.\n\nYou need to resolve the conflict by selecting the right version.\n What version do you want to keep?", delegate()
						{
							overwrite = true;
						}, "Version on \"other\" device", "Version on this device");
						while (BWStandalone.Overlays.IsShowingPopup())
						{
							yield return null;
						}
						BWStandalone.Overlays.SetUIBusy(true);
					}
					if (overwrite)
					{
						Debug.Log("Overwriting " + localCopy.title);
						restoredLocalWorld = new BWLocalWorld(world, localCopy.localWorldID);
					} else {
						BWStandalone.Overlays.SetUIBusy(false);
					}
				}
				else
				{
					restoredLocalWorld = new BWLocalWorld(world);
				}
				if (restoredLocalWorld != null)
				{
					restoredWorlds.Add(restoredLocalWorld);
					string worldApiPath = string.Format("/api/v1/worlds/{0}", world.worldID);
					BWAPIRequestBase request = BW.API.CreateRequest("GET", worldApiPath);
					request.onFailure = delegate(BWAPIRequestError error)
					{
						BWStandalone.Overlays.SetUIBusy(false);
						BWLog.Error(error.message);
						restoredWorlds.Remove(restoredLocalWorld);
					};
					request.onSuccess = delegate(JObject responseJson)
					{
						BWStandalone.Overlays.SetUIBusy(false); // added by exdilin
						JObject jobject = responseJson["world"];
						if (jobject == null)
						{
							BWLog.Info("Failed to parse response for world " + world.worldID);
							restoredWorlds.Remove(restoredLocalWorld);
						}
						else
						{
							restoredLocalWorld.UpdateFromJson(jobject);
						}
						if (string.IsNullOrEmpty(restoredLocalWorld.source))
						{
							BWLog.Info("Failed to load remote source for world " + world.worldID);
							restoredWorlds.Remove(restoredLocalWorld);
						}
					};
					request.SendOwnerCoroutine(this);
				}
			}
		}
		bool sourceLoaded = false;
		while (!sourceLoaded && restoredWorlds.Count > 0)
		{
			yield return null;
			bool check = true;
			foreach (BWLocalWorld bwlocalWorld2 in restoredWorlds)
			{
				if (string.IsNullOrEmpty(bwlocalWorld2.source))
				{
					check = false;
					break;
				}
			}
			sourceLoaded = check;
		}
		foreach (BWLocalWorld bwlocalWorld3 in restoredWorlds)
		{
			for (int i = this.localWorlds.Count - 1; i >= 0; i--)
			{
				BWLocalWorld bwlocalWorld4 = this.localWorlds[i];
				if (bwlocalWorld4.localWorldID == bwlocalWorld3.localWorldID)
				{
					this.DeleteLocalWorldFolder(this.LocalWorldFolder(bwlocalWorld4));
					this.localWorlds.RemoveAt(i);
				}
			}
			BWLog.Info("Restored world with id: " + bwlocalWorld3.worldID + ", saving local copy");
			this.SaveWorldLocal(bwlocalWorld3, null, false);
			this.localWorlds.Add(bwlocalWorld3);
		}
		yield break;
	}

	// Token: 0x06002B5E RID: 11102 RVA: 0x001396CB File Offset: 0x00137ACB
	public void LoadSourceForLocalWorld(BWLocalWorld world, UnityAction completion)
	{
		base.StartCoroutine(this.CoroutineLoadSourceForLocalWorld(world, completion));
	}

	// Token: 0x06002B5F RID: 11103 RVA: 0x001396DC File Offset: 0x00137ADC
	private IEnumerator CoroutineLoadSourceForLocalWorld(BWLocalWorld world, UnityAction completion)
	{
		if (world.source != null && world.source.Length > 2)
		{
			if (completion != null)
			{
				completion();
			}
			yield break;
		}
		string worldFolder = this.LocalWorldFolder(world);
		if (!Directory.Exists(worldFolder))
		{
			BWLog.Error("World Folder " + worldFolder + " not found.");
			yield break;
		}
		string sourcePath = Path.Combine(worldFolder, "source.bw");
		if (!File.Exists(sourcePath))
		{
			this.LoadRemoteSourceForLocalWorld(world, completion);
			yield break;
		}
		WWW sourceLoader = new WWW(BWFilesystem.FileProtocolPrefixStr + sourcePath);
		while (!sourceLoader.isDone)
		{
			yield return null;
		}
		string sourceJsonStr = sourceLoader.text;
		if (!world.OverwriteSource(sourceJsonStr, world.hasWinCondition))
		{
			this.LoadRemoteSourceForLocalWorld(world, completion);
			yield break;
		}
		if (completion != null)
		{
			completion();
		}
		yield break;
	}

	// Token: 0x06002B60 RID: 11104 RVA: 0x00139708 File Offset: 0x00137B08
	private void LoadRemoteSourceForLocalWorld(BWLocalWorld localWorld, UnityAction completion)
	{
		if (!localWorld.isRemote)
		{
			return;
		}
		string remoteWorldID = localWorld.worldID;
		string path = string.Format("/api/v1/worlds/{0}", remoteWorldID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
		};
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			JObject jobject = responseJson["world"];
			if (jobject == null)
			{
				BWLog.Info("Failed to parse response for world " + remoteWorldID);
			}
			else
			{
				BWWorld bwworld = new BWWorld(jobject);
				localWorld.OverwriteSource(bwworld.source, bwworld.hasWinCondition);
				if (completion != null)
				{
					completion();
				}
			}
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002B61 RID: 11105 RVA: 0x001397AC File Offset: 0x00137BAC
	public void DeleteLocalWorld(string worldID, bool notifyListeners = true)
	{
		BWLocalWorld worldWithLocalWorldID = this.GetWorldWithLocalWorldID(worldID);
		if (worldWithLocalWorldID != null)
		{
			if (worldWithLocalWorldID.isRemote)
			{
				this.DeleteWorldRemote(worldWithLocalWorldID.worldID);
			}
			this.DeleteLocalWorldFolder(this.LocalWorldFolder(worldWithLocalWorldID));
			this.localWorlds.Remove(worldWithLocalWorldID);
			if (notifyListeners)
			{
				this.NotifyListeners();
			}
		}
	}

	// Token: 0x06002B62 RID: 11106 RVA: 0x00139804 File Offset: 0x00137C04
	private void DeleteLocalWorldFolder(string worldFolder)
	{
		string[] files = Directory.GetFiles(worldFolder);
		foreach (string path in files)
		{
			File.Delete(path);
		}
		Directory.Delete(worldFolder);
	}

	// Token: 0x06002B63 RID: 11107 RVA: 0x00139840 File Offset: 0x00137C40
	private void DeleteWorldRemote(string worldID)
	{
		string path = string.Format("/api/v1/worlds/{0}", worldID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("DELETE", path);
		BWStandalone.Overlays.SetUIBusy(true);
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			BWLog.Info("Deleted world: " + worldID);
			BWStandalone.Overlays.SetUIBusy(false);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info("Failed to delete world: " + worldID + ", error: " + error.message);
			BWStandalone.Overlays.SetUIBusy(false);
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002B64 RID: 11108 RVA: 0x001398B2 File Offset: 0x00137CB2
	public void DeleteAll()
	{
		if (Directory.Exists(this.localWorldsDirectory))
		{
			Directory.Delete(this.localWorldsDirectory, true);
		}
		this.localWorlds.Clear();
		this.NotifyListeners();
	}

	// Token: 0x06002B65 RID: 11109 RVA: 0x001398E4 File Offset: 0x00137CE4
	public void CreateRemoteWorld(BWLocalWorld localWorld, byte[] imageData, Action<BWLocalWorld> success = null, Action failure = null)
	{
		if (imageData == null)
		{
			string path = this.LocalWorldScreenshotPath(localWorld);
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
			this.LoadSourceForLocalWorld(localWorld, delegate
			{
				if (!string.IsNullOrEmpty(localWorld.source))
				{
					this.CreateRemoteWorld(localWorld, imageData, success, failure);
				}
			});
			return;
		}
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v1/worlds");
		bwapirequestBase.AddParams(localWorld.AttrsToCreateRemote());
		bwapirequestBase.AddImageData("screenshot_image", imageData, "screenshot.jpg", "image/png");
		BWStandalone.Overlays.SetUIBusy(true);
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			Debug.Log("Create remote success!");
			JObject json = responseJson["world"];
			localWorld.UpdateFromJson(json);
			this.SaveWorldMetadataLocal(localWorld, true);
			if (success != null)
			{
				success(localWorld);
			}
			BWStandalone.Overlays.SetUIBusy(false);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			Debug.Log("Create remote failed: " + error.message);
			BWStandalone.Overlays.SetUIBusy(false);
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002B66 RID: 11110 RVA: 0x00139A3C File Offset: 0x00137E3C
	public void UpdateRemoteWorld(BWLocalWorld localWorld, byte[] imageData = null, Action<BWLocalWorld> success = null, Action failure = null)
	{
		if (!localWorld.isRemote)
		{
			this.CreateRemoteWorld(localWorld, imageData, success, failure);
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
			this.LoadSourceForLocalWorld(localWorld, delegate
			{
				if (!string.IsNullOrEmpty(localWorld.source))
				{
					this.UpdateRemoteWorld(localWorld, imageData, success, failure);
				}
			});
			return;
		}
		string path = string.Format("/api/v1/worlds/{0}", localWorld.worldID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("PUT", path);
		bwapirequestBase.AddParams(localWorld.AttrsToSaveChangesRemote());
		if (localWorld.localChangedScreenshot)
		{
			if (imageData == null)
			{
				string path2 = this.LocalWorldScreenshotPath(localWorld);
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
			bwapirequestBase.AddImageData("screenshot_image", imageData, "screenshot.jpg", "image/png");
		}
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			JObject jobject = responseJson["world"];
			localWorld.UpdateFromJson(jobject);
			localWorld.StopTrackingLocalChanges();
			this.SaveWorldMetadataLocal(localWorld, false);
			BWUser.currentUser.RemoteWorldUpdated(localWorld.worldID, jobject);
			if (success != null)
			{
				success(localWorld);
			}
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			if (failure != null)
			{
				failure();
			}
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002B67 RID: 11111 RVA: 0x00139C58 File Offset: 0x00138058
	public void RevertLocalChanges(string localWorldID, UnityAction completion)
	{
		BWLocalWorld localWorld = this.GetWorldWithLocalWorldID(localWorldID);
		if (localWorld == null)
		{
			return;
		}
		if (!localWorld.isRemote)
		{
			return;
		}
		int worldIndex = this.localWorlds.IndexOf(localWorld);
		string remoteWorldID = localWorld.worldID;
		string path = string.Format("/api/v1/worlds/{0}", remoteWorldID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
		};
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			BWLog.Info("Reverting world with id: " + remoteWorldID);
			JObject jobject = responseJson["world"];
			if (jobject == null)
			{
				BWLog.Info("Failed to parse response for world " + remoteWorldID);
			}
			else
			{
				BWWorld copyFrom = new BWWorld(jobject);
				BWLocalWorld bwlocalWorld = new BWLocalWorld(copyFrom, localWorldID);
				if (bwlocalWorld != null && !string.IsNullOrEmpty(bwlocalWorld.source))
				{
					this.DeleteLocalWorldFolder(this.LocalWorldFolder(localWorld));
					this.localWorlds.Remove(localWorld);
					this.SaveWorldLocal(bwlocalWorld, null, false);
					this.localWorlds.Insert(worldIndex, bwlocalWorld);
					this.NotifyListeners();
					if (completion != null)
					{
						completion();
					}
				}
			}
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002B68 RID: 11112 RVA: 0x00139D38 File Offset: 0x00138138
	public void PublishLocalWorld(string localWorldID)
	{
		BWLog.Info("Publishing local world " + localWorldID);
		BWStandalone.Overlays.SetUIBusy(true);
		BWLocalWorld localWorld = this.GetWorldWithLocalWorldID(localWorldID);
		if (localWorld != null)
		{
			BWLog.Info("Publishing world");
			Action<BWLocalWorld> success = delegate(BWLocalWorld world)
			{
				string path = string.Format("/api/v1/worlds/{0}/published_status", world.worldID);
				BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("PUT", path);
				bwapirequestBase.AddParam("is_published", "true");
				bwapirequestBase.onSuccess = delegate(JObject responseJson)
				{
					BWLog.Info("Publish success");
					localWorld.UpdateFromJson(responseJson);
					BWUser.currentUser.RemoteWorldUpdated(localWorld.worldID, responseJson);
					this.SaveWorldMetadataLocal(localWorld, false);
					this.NotifyListeners();
					BWStandalone.Overlays.SetUIBusy(false);
					BWStandalone.Overlays.notifications.ShowNotification(localWorld.title + " Published successfully");
					BWWorldPublishCooldown.SetWorldTimestamp(localWorld.updatedAt, localWorld.worldID);
				};
				bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
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
						BWLog.Info(string.Concat(new object[]
						{
							"Publish fail ",
							error.httpStatusCode,
							": ",
							error.message
						}));
					}
					BWStandalone.Overlays.SetUIBusy(false);
				};
				bwapirequestBase.SendOwnerCoroutine(this);
			};
			Action failure = delegate()
			{
				BWLog.Info("Failed to update remote world prior to publishing");
				BWStandalone.Overlays.SetUIBusy(false);
			};
			this.UpdateRemoteWorld(localWorld, null, success, failure);
		}
	}

	// Token: 0x06002B69 RID: 11113 RVA: 0x00139DCC File Offset: 0x001381CC
	public void UnpublishLocalWorld(string localWorldID)
	{
		BWLocalWorld localWorld = this.GetWorldWithLocalWorldID(localWorldID);
		if (localWorld != null)
		{
			BWStandalone.Overlays.SetUIBusy(true);
			string path = string.Format("/api/v1/worlds/{0}/published_status", localWorld.worldID);
			BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("PUT", path);
			bwapirequestBase.AddParam("is_published", "false");
			bwapirequestBase.onSuccess = delegate(JObject responseJson)
			{
				localWorld.UpdateFromJson(responseJson);
				this.SaveWorldMetadataLocal(localWorld, true);
				this.NotifyListeners();
				BWStandalone.Overlays.SetUIBusy(false);
				BWStandalone.Overlays.notifications.ShowNotification(localWorld.title + " Unpublished");
			};
			bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
			{
				BWStandalone.Overlays.SetUIBusy(false);
			};
			bwapirequestBase.SendOwnerCoroutine(this);
		}
	}

	// Token: 0x040024BC RID: 9404
	private BWUser user;

	// Token: 0x040024BD RID: 9405
	private UIDataManager dataManager;

	// Token: 0x040024C0 RID: 9408
	private List<BWWorld> deletedWorlds;

	// Token: 0x040024C2 RID: 9410
	private bool loading;

	// Token: 0x040024C3 RID: 9411
	private bool fetchedDeletedWorlds;

	// Token: 0x040024C5 RID: 9413
	private string localWorldsDirectory;

	// Token: 0x040024C6 RID: 9414
	private static BWUserWorldsDataManager instance;
}
