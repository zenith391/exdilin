using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x020003D2 RID: 978
public class BWUserModelsDataManager : MonoBehaviour
{
	// Token: 0x1700021B RID: 539
	// (get) Token: 0x06002B10 RID: 11024 RVA: 0x0013744F File Offset: 0x0013584F
	// (set) Token: 0x06002B11 RID: 11025 RVA: 0x00137457 File Offset: 0x00135857
	internal bool localModelsLoaded { get; private set; }

	// Token: 0x1400001D RID: 29
	// (add) Token: 0x06002B12 RID: 11026 RVA: 0x00137460 File Offset: 0x00135860
	// (remove) Token: 0x06002B13 RID: 11027 RVA: 0x00137498 File Offset: 0x00135898
	private event ModelsListChangedEventHandler onModelListChanged;

	// Token: 0x1700021C RID: 540
	// (get) Token: 0x06002B14 RID: 11028 RVA: 0x001374CE File Offset: 0x001358CE
	public static BWUserModelsDataManager Instance
	{
		get
		{
			return BWUserModelsDataManager.instance;
		}
	}

	// Token: 0x06002B15 RID: 11029 RVA: 0x001374D5 File Offset: 0x001358D5
	public void Awake()
	{
		if (BWUserModelsDataManager.instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		this.localModelsDirectory = BWFilesystem.CurrentUserModelsFolder;
		this.localModels = new List<BWUserModel>();
		BWUserModelsDataManager.instance = this;
	}

	// Token: 0x06002B16 RID: 11030 RVA: 0x0013750F File Offset: 0x0013590F
	public void Start()
	{
	}

	// Token: 0x06002B17 RID: 11031 RVA: 0x00137511 File Offset: 0x00135911
	private string LocalModelFolder(BWUserModel model)
	{
		return Path.Combine(this.localModelsDirectory, model.localID);
	}

	// Token: 0x06002B18 RID: 11032 RVA: 0x00137524 File Offset: 0x00135924
	private string LocalModelIconSDPath(BWUserModel model)
	{
		return Path.Combine(this.LocalModelFolder(model), "iconSD.png");
	}

	// Token: 0x06002B19 RID: 11033 RVA: 0x00137537 File Offset: 0x00135937
	private string LocalModelIconHDPath(BWUserModel model)
	{
		return Path.Combine(this.LocalModelFolder(model), "iconHD.png");
	}

	// Token: 0x06002B1A RID: 11034 RVA: 0x0013754A File Offset: 0x0013594A
	private string LocalModelImageHDPath(BWUserModel model)
	{
		return Path.Combine(this.LocalModelFolder(model), "image.png");
	}

	// Token: 0x06002B1B RID: 11035 RVA: 0x0013755D File Offset: 0x0013595D
	private string LocalModelImageSDPath(BWUserModel model)
	{
		return Path.Combine(this.LocalModelFolder(model), "imageSD.png");
	}

	// Token: 0x06002B1C RID: 11036 RVA: 0x00137570 File Offset: 0x00135970
	public void AddListener(ModelsListChangedEventHandler listener)
	{
		this.onModelListChanged -= listener;
		this.onModelListChanged += listener;
	}

	// Token: 0x06002B1D RID: 11037 RVA: 0x00137580 File Offset: 0x00135980
	public void RemoveListener(ModelsListChangedEventHandler listener)
	{
		this.onModelListChanged -= listener;
	}

	// Token: 0x06002B1E RID: 11038 RVA: 0x00137589 File Offset: 0x00135989
	public void ClearListeners()
	{
		this.onModelListChanged = null;
	}

	// Token: 0x06002B1F RID: 11039 RVA: 0x00137592 File Offset: 0x00135992
	public void NotifyListeners()
	{
		if (this.onModelListChanged != null)
		{
			this.onModelListChanged();
		}
	}

	// Token: 0x06002B20 RID: 11040 RVA: 0x001375AC File Offset: 0x001359AC
	public BWUserModel GetModelWithLocalId(string localId)
	{
		if (!this.localModelsLoaded)
		{
			BWLog.Error("Local models not loaded");
			return null;
		}
		foreach (BWUserModel bwuserModel in this.localModels)
		{
			if (bwuserModel.localID == localId)
			{
				return bwuserModel;
			}
		}
		return null;
	}

	// Token: 0x06002B21 RID: 11041 RVA: 0x00137634 File Offset: 0x00135A34
	public void SaveModelLocal(BWUserModel model, byte[] iconSDImageData, byte[] iconHDImageData, byte[] imageSDData, byte[] imageHDData)
	{
		if (!Directory.Exists(this.localModelsDirectory))
		{
			Directory.CreateDirectory(this.localModelsDirectory);
		}
		if (!Directory.Exists(this.localModelsDirectory))
		{
			return;
		}
		string text = this.LocalModelFolder(model);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (iconSDImageData != null && iconHDImageData != null && imageSDData != null && imageHDData != null)
		{
			string text2 = this.LocalModelIconSDPath(model);
			string text3 = this.LocalModelIconHDPath(model);
			string text4 = this.LocalModelImageHDPath(model);
			string text5 = this.LocalModelImageHDPath(model);
			if (!File.Exists(text2))
			{
				File.Create(text2).Dispose();
			}
			if (!File.Exists(text3))
			{
				File.Create(text3).Dispose();
			}
			File.WriteAllBytes(text2, iconSDImageData);
			File.WriteAllBytes(text3, iconHDImageData);
			MainUIController.Instance.imageManager.SetUnloaded(BWFilesystem.FileProtocolPrefixStr + text2);
			MainUIController.Instance.imageManager.SetUnloaded(BWFilesystem.FileProtocolPrefixStr + text3);
			if (!File.Exists(text4))
			{
				File.Create(text4).Dispose();
			}
			File.WriteAllBytes(text4, imageSDData);
			if (!File.Exists(text5))
			{
				File.Create(text5).Dispose();
			}
			File.WriteAllBytes(text5, imageHDData);
			MainUIController.Instance.imageManager.SetUnloaded(BWFilesystem.FileProtocolPrefixStr + text4);
			MainUIController.Instance.imageManager.SetUnloaded(BWFilesystem.FileProtocolPrefixStr + text5);
		}
		Dictionary<string, object> obj = model.MetadataAttributes();
		string contents = JSONEncoder.Encode(obj);
		string path = Path.Combine(text, "metadata.json");
		if (!File.Exists(path))
		{
			File.Create(path).Dispose();
		}
		File.WriteAllText(path, contents);
		string path2 = Path.Combine(text, "source.bw");
		if (!File.Exists(path2))
		{
			File.Create(path2).Dispose();
		}
		File.WriteAllText(path2, model.obfuscatedSourceJsonStr);
		this.localModels.Insert(0, model);
	}

	// Token: 0x06002B22 RID: 11042 RVA: 0x00137824 File Offset: 0x00135C24
	public void SaveModelMetadata(BWUserModel model)
	{
		Dictionary<string, object> obj = model.MetadataAttributes();
		string contents = JSONEncoder.Encode(obj);
		string path = this.LocalModelFolder(model);
		string path2 = Path.Combine(path, "metadata.json");
		if (!File.Exists(path2))
		{
			File.Create(path2).Dispose();
		}
		File.WriteAllText(path2, contents);
	}

	// Token: 0x06002B23 RID: 11043 RVA: 0x00137870 File Offset: 0x00135C70
	private void CreateRemoteModel(BWUserModel localModel, UnityAction completion)
	{
		this.CreateRemoteModel(localModel, null, null, null, null, completion);
	}

	// Token: 0x06002B24 RID: 11044 RVA: 0x00137880 File Offset: 0x00135C80
	public void CreateRemoteModel(BWUserModel localModel, byte[] iconSDImageData, byte[] iconHDImageData, byte[] imageSDData, byte[] imageHDData, UnityAction completion)
	{
		if (string.IsNullOrEmpty(localModel.sourceJsonStr))
		{
			localModel.LoadSourceFromDataManager();
			if (string.IsNullOrEmpty(localModel.sourceJsonStr))
			{
				BWLog.Error("Missing source for model");
				return;
			}
		}
		if (imageHDData == null)
		{
			string path = this.LocalModelImageHDPath(localModel);
			if (File.Exists(path))
			{
				imageHDData = File.ReadAllBytes(path);
			}
		}
		bool flag = false;
		if (imageSDData == null)
		{
			string path2 = this.LocalModelImageSDPath(localModel);
			if (File.Exists(path2))
			{
				imageSDData = File.ReadAllBytes(path2);
			}
			else if (imageHDData != null)
			{
				Texture2D texture2D = new Texture2D(ScreenshotUtils.snapshotSizeSD * 2, ScreenshotUtils.snapshotSizeSD * 2, TextureFormat.ARGB32, false);
				texture2D.LoadImage(imageHDData);
				Texture2D texture2D2 = new Texture2D(ScreenshotUtils.snapshotSizeSD, ScreenshotUtils.snapshotSizeSD, TextureFormat.ARGB32, false);
				for (int i = 0; i < texture2D2.width; i++)
				{
					for (int j = 0; j < texture2D2.height; j++)
					{
						texture2D2.SetPixel(i, j, texture2D.GetPixel(i * 2, j * 2));
					}
				}
				imageSDData = texture2D2.EncodeToPNG();
				flag = true;
				UnityEngine.Object.Destroy(texture2D);
				UnityEngine.Object.Destroy(texture2D2);
			}
		}
		if (iconSDImageData == null)
		{
			string path3 = this.LocalModelIconSDPath(localModel);
			if (File.Exists(path3))
			{
				iconSDImageData = File.ReadAllBytes(path3);
			}
		}
		if (iconHDImageData == null)
		{
			string path4 = this.LocalModelIconHDPath(localModel);
			if (File.Exists(path4))
			{
				iconHDImageData = File.ReadAllBytes(path4);
			}
		}
		if (imageSDData == null || imageHDData == null || iconSDImageData == null || iconHDImageData == null)
		{
			BWLog.Error("Must provide images to create new remote model");
			return;
		}
		if ((!flag && !BWEncript.VerifyChecksum(localModel.imageSDChecksumStr, imageSDData)) || !BWEncript.VerifyChecksum(localModel.imageHDChecksumStr, imageHDData) || !BWEncript.VerifyChecksum(localModel.iconSDChecksumStr, iconSDImageData) || !BWEncript.VerifyChecksum(localModel.iconHDChecksumStr, iconHDImageData))
		{
			BWLog.Error("suspicious image");
			return;
		}
		if (string.IsNullOrEmpty(localModel.sourceJsonStr))
		{
			localModel.LoadSourceFromDataManager();
			if (string.IsNullOrEmpty(localModel.sourceJsonStr))
			{
				BWLog.Error("model has no source");
				return;
			}
		}
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v1/user_models");
		bwapirequestBase.AddParams(localModel.AttrsToCreateRemote());
		bwapirequestBase.AddImageData("iconSD", iconSDImageData, "iconSD.png", "image/png");
		bwapirequestBase.AddImageData("iconHD", iconHDImageData, "iconHD.png", "image/png");
		bwapirequestBase.AddImageData("imageSD", imageSDData, "imageSD.png", "image/png");
		bwapirequestBase.AddImageData("imageHD", imageHDData, "imageHD.png", "image/png");
		BWStandalone.Overlays.SetUIBusy(true);
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			BWLog.Info("Create remote success!");
			JObject json = responseJson["user_model"];
			localModel.UpdateFromJson(json);
			this.SaveModelMetadata(localModel);
			this.SortModelList();
			BWStandalone.Overlays.SetUIBusy(false);
			this.NotifyListeners();
			if (completion != null && localModel.isRemote)
			{
				completion();
			}
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			Debug.Log("Create remote failed: " + error.message);
			BWStandalone.Overlays.SetUIBusy(false);
			this.SortModelList();
			this.NotifyListeners();
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002B25 RID: 11045 RVA: 0x00137BC5 File Offset: 0x00135FC5
	public void UpdateRemoteModel(BWUserModel userModel)
	{
		if (string.IsNullOrEmpty(userModel.modelID))
		{
			this.CreateRemoteModel(userModel, null);
			return;
		}
		this.SaveModelRemote(userModel, null);
	}

	// Token: 0x06002B26 RID: 11046 RVA: 0x00137BE8 File Offset: 0x00135FE8
	public void PublishModel(string localModelID)
	{
		BWUserModel modelWithLocalId = this.GetModelWithLocalId(localModelID);
		if (modelWithLocalId == null)
		{
			BWLog.Error("Failed to find local model: " + localModelID);
			return;
		}
		if (!modelWithLocalId.isRemote)
		{
			this.CreateRemoteModel(modelWithLocalId, delegate()
			{
				this.PublishModel(localModelID);
			});
			return;
		}
		Dictionary<string, string> publishParameters = new Dictionary<string, string>
		{
			{
				"publish",
				"yes"
			}
		};
		this.SaveModelRemote(modelWithLocalId, publishParameters);
	}

	// Token: 0x06002B27 RID: 11047 RVA: 0x00137C74 File Offset: 0x00136074
	public void UnpublishModel(string localModelID)
	{
		BWUserModel modelWithLocalId = this.GetModelWithLocalId(localModelID);
		if (modelWithLocalId == null)
		{
			BWLog.Error("Failed to find local model: " + localModelID);
			return;
		}
		Dictionary<string, string> publishParameters = new Dictionary<string, string>
		{
			{
				"unpublish",
				"yes"
			}
		};
		this.SaveModelRemote(modelWithLocalId, publishParameters);
	}

	// Token: 0x06002B28 RID: 11048 RVA: 0x00137CC0 File Offset: 0x001360C0
	private void SaveModelRemote(BWUserModel userModel, Dictionary<string, string> publishParameters)
	{
		if (string.IsNullOrEmpty(userModel.sourceJsonStr))
		{
			userModel.LoadSourceFromDataManager();
			if (string.IsNullOrEmpty(userModel.sourceJsonStr))
			{
				BWLog.Error("Missing source for model");
				return;
			}
		}
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("PUT", "/api/v1/user_models/" + userModel.modelID);
		bwapirequestBase.AddParams(userModel.AttrsToUpdateRemote());
		if (publishParameters != null)
		{
			bwapirequestBase.AddParams(publishParameters);
		}
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("last_published_model_at"))
			{
				BWModelPublishCooldown.UpdateFromJson(responseJson);
			}
			if (responseJson.ContainsKey("user_model"))
			{
				userModel.UpdateFromJson(responseJson["user_model"]);
				if (publishParameters != null)
				{
					userModel.DidPublish();
				}
				this.SaveModelMetadata(userModel);
				BWModelPublishCooldown.UpdateAvailableSlots(this.localModels);
				this.NotifyListeners();
			}
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error("Failed to update remote " + error.message);
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002B29 RID: 11049 RVA: 0x00137DB4 File Offset: 0x001361B4
	public string LoadSourceForModel(string modelType, string localModelID)
	{
		if (modelType == "user_model")
		{
			string path = Path.Combine(this.localModelsDirectory, localModelID);
			string path2 = Path.Combine(path, "source.bw");
			if (File.Exists(path2))
			{
				string source = File.ReadAllText(path2);
				return Util.UnobfuscateSourceForUser(source, BWUser.currentUser.userID);
			}
		}
		return null;
	}

	// Token: 0x06002B2A RID: 11050 RVA: 0x00137E0E File Offset: 0x0013620E
	public void LoadAllModelsMetadata()
	{
		if (!this.localModelsLoaded && !this.loading)
		{
			base.StartCoroutine(this.LoadAllModelsCoroutine());
		}
	}

	// Token: 0x06002B2B RID: 11051 RVA: 0x00137E34 File Offset: 0x00136234
	private IEnumerator LoadAllModelsCoroutine()
	{
		this.loading = true;
		if (Directory.Exists(this.localModelsDirectory))
		{
			string[] modelFolders = Directory.GetDirectories(this.localModelsDirectory);
			Dictionary<string, WWW> modelLoaders = new Dictionary<string, WWW>();
			int currentBatchCount = 0;
			for (int i = 0; i < modelFolders.Length; i++)
			{
				string modelFolder = modelFolders[i];
				string metadataPath = Path.Combine(modelFolder, "metadata.json");
				string sourcePath = Path.Combine(modelFolder, "source.bw");
				if (!File.Exists(metadataPath) || !File.Exists(sourcePath))
				{
					BWLog.Error("Invalid model data in folder: " + modelFolder);
				}
				else
				{
					WWW metadataLoader = new WWW(BWFilesystem.FileProtocolPrefixStr + metadataPath);
					modelLoaders.Add(modelFolder, metadataLoader);
					currentBatchCount++;
					if (currentBatchCount >= 128 || i == modelFolders.Length - 1)
					{
						yield return base.StartCoroutine(this.ProcessMetaDataLoaders(modelLoaders));
						modelLoaders.Clear();
						currentBatchCount = 0;
					}
				}
			}
		}
		bool remoteSyncComplete = false;
		HashSet<BWUserModel> remoteModels = new HashSet<BWUserModel>();
		BWAPIRequestBase request = BW.API.CreateRequest("GET", "/api/v1/user_models");
		request.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("user_models"))
			{
				List<JObject> arrayValue = responseJson["user_models"].ArrayValue;
				foreach (JObject json in arrayValue)
				{
					BWUserModel item = new BWUserModel(json);
					remoteModels.Add(item);
				}
			}
			remoteSyncComplete = true;
		};
		request.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error("Failed to sync remote models");
			remoteSyncComplete = true;
		};
		request.SendOwnerCoroutine(this);
		while (!remoteSyncComplete)
		{
			yield return null;
		}
		this.RestoreRemoteModels(remoteModels);
		yield return base.StartCoroutine(this.RestoreMissingImages());
		this.SortModelList();
		BWModelPublishCooldown.UpdateAvailableSlots(this.localModels);
		this.localModelsLoaded = true;
		this.loading = false;
		this.NotifyListeners();
		yield break;
	}

	// Token: 0x06002B2C RID: 11052 RVA: 0x00137E50 File Offset: 0x00136250
	private IEnumerator ProcessMetaDataLoaders(Dictionary<string, WWW> metaDataLoaders)
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
							BWLog.Error("Invalid model metadata in folder: " + key);
						}
						else
						{
							JObject json = JSONDecoder.Decode(text);
							BWUserModel bwuserModel = new BWUserModel(json);
							if (string.IsNullOrEmpty(bwuserModel.localID))
							{
								BWLog.Error("Invalid model in folder: " + key);
							}
							else if (bwuserModel.authorId != BWUser.currentUser.userID)
							{
								BWLog.Info("Trying to load model created by different user, ignoring");
							}
							else
							{
								this.localModels.Add(bwuserModel);
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

	// Token: 0x06002B2D RID: 11053 RVA: 0x00137E74 File Offset: 0x00136274
	private void RestoreRemoteModels(HashSet<BWUserModel> remoteModels)
	{
		HashSet<BWUserModel> hashSet = new HashSet<BWUserModel>();
		foreach (BWUserModel bwuserModel in remoteModels)
		{
			BWUserModel bwuserModel2 = null;
			foreach (BWUserModel bwuserModel3 in this.localModels)
			{
				if (bwuserModel.modelID == bwuserModel3.modelID)
				{
					bwuserModel2 = bwuserModel3;
					break;
				}
			}
			BWUserModel bwuserModel4;
			if (bwuserModel2 != null)
			{
				if (!(bwuserModel2.updatedAt > DateTime.MinValue) || !(bwuserModel2.updatedAt < bwuserModel.updatedAt))
				{
					if (string.IsNullOrEmpty(bwuserModel2.imageUrl))
					{
						bwuserModel2.SetRemoteImageUrl(bwuserModel.imageUrl);
					}
					if (string.IsNullOrEmpty(bwuserModel2.iconUrl))
					{
						bwuserModel2.SetRemoteIconUrl(bwuserModel.iconUrl);
					}
					continue;
				}
				Debug.Log(string.Concat(new object[]
				{
					"Found newer version of model: ",
					bwuserModel2.modelID,
					" local ",
					bwuserModel2.updatedAt,
					" remote ",
					bwuserModel.updatedAt
				}));
				bwuserModel4 = new BWUserModel(bwuserModel, bwuserModel2.localID);
			}
			else
			{
				bwuserModel4 = new BWUserModel(bwuserModel);
			}
			if (bwuserModel4 != null)
			{
				hashSet.Add(bwuserModel4);
			}
		}
		foreach (BWUserModel bwuserModel5 in hashSet)
		{
			for (int i = this.localModels.Count - 1; i >= 0; i--)
			{
				BWUserModel bwuserModel6 = this.localModels[i];
				if (bwuserModel6.localID == bwuserModel5.localID)
				{
					Debug.Log("Deleting old copy of " + bwuserModel6.modelID);
					this.DeleteLocalModelFolder(this.LocalModelFolder(bwuserModel6));
					this.localModels.Remove(bwuserModel6);
				}
			}
		}
		foreach (BWUserModel bwuserModel7 in hashSet)
		{
			BWLog.Info("Restored model with id: " + bwuserModel7.modelID + ", saving local copy");
			this.SaveModelLocal(bwuserModel7, null, null, null, null);
		}
	}

	// Token: 0x06002B2E RID: 11054 RVA: 0x00138180 File Offset: 0x00136580
	private IEnumerator RestoreMissingImages()
	{
		HashSet<BWUserModel> missingModelImageTracker = new HashSet<BWUserModel>();
		foreach (BWUserModel bwuserModel in this.localModels)
		{
			missingModelImageTracker.Add(bwuserModel);
			base.StartCoroutine(this.DownloadMissingImages(bwuserModel, missingModelImageTracker));
		}
		while (missingModelImageTracker.Count > 0)
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x06002B2F RID: 11055 RVA: 0x0013819C File Offset: 0x0013659C
	private IEnumerator DownloadMissingImages(BWUserModel model, HashSet<BWUserModel> missingModelImageTracker)
	{
		string imageHDPath = this.LocalModelImageHDPath(model);
		string iconHDPath = this.LocalModelIconHDPath(model);
		bool imageExists = File.Exists(imageHDPath);
		bool iconExists = File.Exists(iconHDPath);
		long imageSize = (!imageExists) ? 0L : new FileInfo(imageHDPath).Length;
		long iconSize = (!iconExists) ? 0L : new FileInfo(iconHDPath).Length;
		if (imageExists && iconExists && imageSize > 200L && iconSize > 200L)
		{
			missingModelImageTracker.Remove(model);
			yield break;
		}
		WWW imageDownload = new WWW(model.imageUrl);
		WWW iconDownload = new WWW(model.iconUrl);
		while (!imageDownload.isDone || !iconDownload.isDone)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(imageDownload.error))
		{
			BWLog.Error("Failed to download remote model image " + model.imageUrl);
		}
		else if (imageDownload.texture != null)
		{
			if (imageExists)
			{
				File.Delete(imageHDPath);
			}
			File.WriteAllBytes(imageHDPath, imageDownload.texture.EncodeToPNG());
		}
		if (!string.IsNullOrEmpty(iconDownload.error))
		{
			BWLog.Error("Failed to download remote model icon " + model.iconUrl);
		}
		if (iconDownload.texture != null)
		{
			if (iconExists)
			{
				File.Delete(iconHDPath);
			}
			File.WriteAllBytes(iconHDPath, iconDownload.texture.EncodeToPNG());
		}
		imageDownload.Dispose();
		iconDownload.Dispose();
		missingModelImageTracker.Remove(model);
		yield break;
	}

	// Token: 0x06002B30 RID: 11056 RVA: 0x001381C5 File Offset: 0x001365C5
	private void SortModelList()
	{
		this.localModels.Sort((BWUserModel x, BWUserModel y) => y.createdAt.CompareTo(x.createdAt));
	}

	// Token: 0x06002B31 RID: 11057 RVA: 0x001381F0 File Offset: 0x001365F0
	public void DeleteLocalModel(string modelId)
	{
		BWUserModel modelWithLocalId = this.GetModelWithLocalId(modelId);
		if (modelWithLocalId != null)
		{
			if (!string.IsNullOrEmpty(modelWithLocalId.modelID))
			{
				this.DeleteModelRemote(modelWithLocalId.modelID);
			}
			string modelFolder = this.LocalModelFolder(modelWithLocalId);
			this.DeleteLocalModelFolder(modelFolder);
			this.localModels.Remove(modelWithLocalId);
			this.NotifyListeners();
		}
	}

	// Token: 0x06002B32 RID: 11058 RVA: 0x0013824C File Offset: 0x0013664C
	private void DeleteModelRemote(string modelID)
	{
		string path = string.Format("/api/v1/user_models/{0}", modelID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("DELETE", path);
		BWStandalone.Overlays.SetUIBusy(true);
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			BWLog.Info("Deleted model: " + modelID);
			BWStandalone.Overlays.SetUIBusy(false);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info("Failed to delete model: " + modelID + ", error: " + error.message);
			BWStandalone.Overlays.SetUIBusy(false);
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002B33 RID: 11059 RVA: 0x001382C0 File Offset: 0x001366C0
	private void DeleteLocalModelFolder(string modelFolder)
	{
		string[] files = Directory.GetFiles(modelFolder);
		foreach (string path in files)
		{
			File.Delete(path);
		}
		Directory.Delete(modelFolder);
	}

	// Token: 0x06002B34 RID: 11060 RVA: 0x001382FC File Offset: 0x001366FC
	public void SetModelSourceLocked(string localModelID, bool locked)
	{
		BWUserModel modelWithLocalId = this.GetModelWithLocalId(localModelID);
		if (modelWithLocalId == null)
		{
			BWLog.Error("No local model with id: " + localModelID);
			return;
		}
		modelWithLocalId.SetSourceLocked(locked);
		this.SaveModelMetadata(modelWithLocalId);
		this.SaveModelRemote(modelWithLocalId, null);
		this.NotifyListeners();
	}

	// Token: 0x06002B35 RID: 11061 RVA: 0x00138344 File Offset: 0x00136744
	public void ChangeModelPriceMarkup(BWUserModel model, int change)
	{
		model.ChangePriceMarkup(change);
		this.SaveModelMetadata(model);
		this.NotifyListeners();
	}

	// Token: 0x06002B36 RID: 11062 RVA: 0x0013835A File Offset: 0x0013675A
	public void ResetModelPriceMarkup(BWUserModel model)
	{
		model.ResetPriceMarkup();
		this.SaveModelMetadata(model);
		this.NotifyListeners();
	}

	// Token: 0x040024B4 RID: 9396
	public List<BWUserModel> localModels;

	// Token: 0x040024B6 RID: 9398
	private bool loading;

	// Token: 0x040024B8 RID: 9400
	private string localModelsDirectory;

	// Token: 0x040024B9 RID: 9401
	private static BWUserModelsDataManager instance;
}
