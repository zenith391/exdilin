using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

public class BWUserModelsDataManager : MonoBehaviour
{
	public List<BWUserModel> localModels;

	private bool loading;

	private string localModelsDirectory;

	private static BWUserModelsDataManager instance;

	internal bool localModelsLoaded { get; private set; }

	public static BWUserModelsDataManager Instance => instance;

	private event ModelsListChangedEventHandler onModelListChanged;

	public void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		localModelsDirectory = BWFilesystem.CurrentUserModelsFolder;
		localModels = new List<BWUserModel>();
		instance = this;
	}

	public void Start()
	{
	}

	private string LocalModelFolder(BWUserModel model)
	{
		return Path.Combine(localModelsDirectory, model.localID);
	}

	private string LocalModelIconSDPath(BWUserModel model)
	{
		return Path.Combine(LocalModelFolder(model), "iconSD.png");
	}

	private string LocalModelIconHDPath(BWUserModel model)
	{
		return Path.Combine(LocalModelFolder(model), "iconHD.png");
	}

	private string LocalModelImageHDPath(BWUserModel model)
	{
		return Path.Combine(LocalModelFolder(model), "image.png");
	}

	private string LocalModelImageSDPath(BWUserModel model)
	{
		return Path.Combine(LocalModelFolder(model), "imageSD.png");
	}

	public void AddListener(ModelsListChangedEventHandler listener)
	{
		onModelListChanged -= listener;
		onModelListChanged += listener;
	}

	public void RemoveListener(ModelsListChangedEventHandler listener)
	{
		onModelListChanged -= listener;
	}

	public void ClearListeners()
	{
		this.onModelListChanged = null;
	}

	public void NotifyListeners()
	{
		if (this.onModelListChanged != null)
		{
			this.onModelListChanged();
		}
	}

	public BWUserModel GetModelWithLocalId(string localId)
	{
		if (!localModelsLoaded)
		{
			BWLog.Error("Local models not loaded");
			return null;
		}
		foreach (BWUserModel localModel in localModels)
		{
			if (localModel.localID == localId)
			{
				return localModel;
			}
		}
		return null;
	}

	public void SaveModelLocal(BWUserModel model, byte[] iconSDImageData, byte[] iconHDImageData, byte[] imageSDData, byte[] imageHDData)
	{
		if (!Directory.Exists(localModelsDirectory))
		{
			Directory.CreateDirectory(localModelsDirectory);
		}
		if (!Directory.Exists(localModelsDirectory))
		{
			return;
		}
		string text = LocalModelFolder(model);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (iconSDImageData != null && iconHDImageData != null && imageSDData != null && imageHDData != null)
		{
			string text2 = LocalModelIconSDPath(model);
			string text3 = LocalModelIconHDPath(model);
			string text4 = LocalModelImageHDPath(model);
			string text5 = LocalModelImageHDPath(model);
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
		localModels.Insert(0, model);
	}

	public void SaveModelMetadata(BWUserModel model)
	{
		Dictionary<string, object> obj = model.MetadataAttributes();
		string contents = JSONEncoder.Encode(obj);
		string path = LocalModelFolder(model);
		string path2 = Path.Combine(path, "metadata.json");
		if (!File.Exists(path2))
		{
			File.Create(path2).Dispose();
		}
		File.WriteAllText(path2, contents);
	}

	private void CreateRemoteModel(BWUserModel localModel, UnityAction completion)
	{
		CreateRemoteModel(localModel, null, null, null, null, completion);
	}

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
			string path = LocalModelImageHDPath(localModel);
			if (File.Exists(path))
			{
				imageHDData = File.ReadAllBytes(path);
			}
		}
		bool flag = false;
		if (imageSDData == null)
		{
			string path2 = LocalModelImageSDPath(localModel);
			if (File.Exists(path2))
			{
				imageSDData = File.ReadAllBytes(path2);
			}
			else if (imageHDData != null)
			{
				Texture2D texture2D = new Texture2D(ScreenshotUtils.snapshotSizeSD * 2, ScreenshotUtils.snapshotSizeSD * 2, TextureFormat.ARGB32, mipmap: false);
				texture2D.LoadImage(imageHDData);
				Texture2D texture2D2 = new Texture2D(ScreenshotUtils.snapshotSizeSD, ScreenshotUtils.snapshotSizeSD, TextureFormat.ARGB32, mipmap: false);
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
			string path3 = LocalModelIconSDPath(localModel);
			if (File.Exists(path3))
			{
				iconSDImageData = File.ReadAllBytes(path3);
			}
		}
		if (iconHDImageData == null)
		{
			string path4 = LocalModelIconHDPath(localModel);
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
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v1/user_models");
		bWAPIRequestBase.AddParams(localModel.AttrsToCreateRemote());
		bWAPIRequestBase.AddImageData("iconSD", iconSDImageData, "iconSD.png", "image/png");
		bWAPIRequestBase.AddImageData("iconHD", iconHDImageData, "iconHD.png", "image/png");
		bWAPIRequestBase.AddImageData("imageSD", imageSDData, "imageSD.png", "image/png");
		bWAPIRequestBase.AddImageData("imageHD", imageHDData, "imageHD.png", "image/png");
		BWStandalone.Overlays.SetUIBusy(busy: true);
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			BWLog.Info("Create remote success!");
			JObject json = responseJson["user_model"];
			localModel.UpdateFromJson(json);
			SaveModelMetadata(localModel);
			SortModelList();
			BWStandalone.Overlays.SetUIBusy(busy: false);
			NotifyListeners();
			if (completion != null && localModel.isRemote)
			{
				completion();
			}
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			Debug.Log("Create remote failed: " + error.message);
			BWStandalone.Overlays.SetUIBusy(busy: false);
			SortModelList();
			NotifyListeners();
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void UpdateRemoteModel(BWUserModel userModel)
	{
		if (string.IsNullOrEmpty(userModel.modelID))
		{
			CreateRemoteModel(userModel, null);
		}
		else
		{
			SaveModelRemote(userModel, null);
		}
	}

	public void PublishModel(string localModelID)
	{
		BWUserModel modelWithLocalId = GetModelWithLocalId(localModelID);
		if (modelWithLocalId == null)
		{
			BWLog.Error("Failed to find local model: " + localModelID);
		}
		else if (!modelWithLocalId.isRemote)
		{
			CreateRemoteModel(modelWithLocalId, delegate
			{
				PublishModel(localModelID);
			});
		}
		else
		{
			Dictionary<string, string> publishParameters = new Dictionary<string, string> { { "publish", "yes" } };
			SaveModelRemote(modelWithLocalId, publishParameters);
		}
	}

	public void UnpublishModel(string localModelID)
	{
		BWUserModel modelWithLocalId = GetModelWithLocalId(localModelID);
		if (modelWithLocalId == null)
		{
			BWLog.Error("Failed to find local model: " + localModelID);
			return;
		}
		Dictionary<string, string> publishParameters = new Dictionary<string, string> { { "unpublish", "yes" } };
		SaveModelRemote(modelWithLocalId, publishParameters);
	}

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
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("PUT", "/api/v1/user_models/" + userModel.modelID);
		bWAPIRequestBase.AddParams(userModel.AttrsToUpdateRemote());
		if (publishParameters != null)
		{
			bWAPIRequestBase.AddParams(publishParameters);
		}
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
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
				SaveModelMetadata(userModel);
				BWModelPublishCooldown.UpdateAvailableSlots(localModels);
				NotifyListeners();
			}
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error("Failed to update remote " + error.message);
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public string LoadSourceForModel(string modelType, string localModelID)
	{
		if (modelType == "user_model")
		{
			string path = Path.Combine(localModelsDirectory, localModelID);
			string path2 = Path.Combine(path, "source.bw");
			if (File.Exists(path2))
			{
				string source = File.ReadAllText(path2);
				return Util.UnobfuscateSourceForUser(source, BWUser.currentUser.userID);
			}
		}
		return null;
	}

	public void LoadAllModelsMetadata()
	{
		if (!localModelsLoaded && !loading)
		{
			StartCoroutine(LoadAllModelsCoroutine());
		}
	}

	private IEnumerator LoadAllModelsCoroutine()
	{
		loading = true;
		if (Directory.Exists(localModelsDirectory))
		{
			string[] modelFolders = Directory.GetDirectories(localModelsDirectory);
			Dictionary<string, WWW> modelLoaders = new Dictionary<string, WWW>();
			int num = 0;
			for (int i = 0; i < modelFolders.Length; i++)
			{
				string text = modelFolders[i];
				string text2 = Path.Combine(text, "metadata.json");
				string path = Path.Combine(text, "source.bw");
				if (!File.Exists(text2) || !File.Exists(path))
				{
					BWLog.Error("Invalid model data in folder: " + text);
					continue;
				}
				WWW value = new WWW(BWFilesystem.FileProtocolPrefixStr + text2);
				modelLoaders.Add(text, value);
				num++;
				if (num >= 128 || i == modelFolders.Length - 1)
				{
					yield return StartCoroutine(ProcessMetaDataLoaders(modelLoaders));
					modelLoaders.Clear();
					num = 0;
				}
			}
		}
		bool remoteSyncComplete = false;
		HashSet<BWUserModel> remoteModels = new HashSet<BWUserModel>();
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/user_models");
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("user_models"))
			{
				List<JObject> arrayValue = responseJson["user_models"].ArrayValue;
				foreach (JObject item2 in arrayValue)
				{
					BWUserModel item = new BWUserModel(item2);
					remoteModels.Add(item);
				}
			}
			remoteSyncComplete = true;
		};
		bWAPIRequestBase.onFailure = delegate
		{
			BWLog.Error("Failed to sync remote models");
			remoteSyncComplete = true;
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
		while (!remoteSyncComplete)
		{
			yield return null;
		}
		RestoreRemoteModels(remoteModels);
		yield return StartCoroutine(RestoreMissingImages());
		SortModelList();
		BWModelPublishCooldown.UpdateAvailableSlots(localModels);
		localModelsLoaded = true;
		loading = false;
		NotifyListeners();
	}

	private IEnumerator ProcessMetaDataLoaders(Dictionary<string, WWW> metaDataLoaders)
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
						BWLog.Error("Invalid model metadata in folder: " + key);
						continue;
					}
					JObject json = JSONDecoder.Decode(text);
					BWUserModel bWUserModel = new BWUserModel(json);
					if (string.IsNullOrEmpty(bWUserModel.localID))
					{
						BWLog.Error("Invalid model in folder: " + key);
					}
					else if (bWUserModel.authorId != BWUser.currentUser.userID)
					{
						BWLog.Info("Trying to load model created by different user, ignoring");
					}
					else
					{
						localModels.Add(bWUserModel);
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

	private void RestoreRemoteModels(HashSet<BWUserModel> remoteModels)
	{
		HashSet<BWUserModel> hashSet = new HashSet<BWUserModel>();
		foreach (BWUserModel remoteModel in remoteModels)
		{
			BWUserModel bWUserModel = null;
			foreach (BWUserModel localModel in localModels)
			{
				if (remoteModel.modelID == localModel.modelID)
				{
					bWUserModel = localModel;
					break;
				}
			}
			BWUserModel bWUserModel2;
			if (bWUserModel != null)
			{
				if (!(bWUserModel.updatedAt > DateTime.MinValue) || !(bWUserModel.updatedAt < remoteModel.updatedAt))
				{
					if (string.IsNullOrEmpty(bWUserModel.imageUrl))
					{
						bWUserModel.SetRemoteImageUrl(remoteModel.imageUrl);
					}
					if (string.IsNullOrEmpty(bWUserModel.iconUrl))
					{
						bWUserModel.SetRemoteIconUrl(remoteModel.iconUrl);
					}
					continue;
				}
				Debug.Log(string.Concat("Found newer version of model: ", bWUserModel.modelID, " local ", bWUserModel.updatedAt, " remote ", remoteModel.updatedAt));
				bWUserModel2 = new BWUserModel(remoteModel, bWUserModel.localID);
			}
			else
			{
				bWUserModel2 = new BWUserModel(remoteModel);
			}
			if (bWUserModel2 != null)
			{
				hashSet.Add(bWUserModel2);
			}
		}
		foreach (BWUserModel item in hashSet)
		{
			for (int num = localModels.Count - 1; num >= 0; num--)
			{
				BWUserModel bWUserModel3 = localModels[num];
				if (bWUserModel3.localID == item.localID)
				{
					Debug.Log("Deleting old copy of " + bWUserModel3.modelID);
					DeleteLocalModelFolder(LocalModelFolder(bWUserModel3));
					localModels.Remove(bWUserModel3);
				}
			}
		}
		foreach (BWUserModel item2 in hashSet)
		{
			BWLog.Info("Restored model with id: " + item2.modelID + ", saving local copy");
			SaveModelLocal(item2, null, null, null, null);
		}
	}

	private IEnumerator RestoreMissingImages()
	{
		HashSet<BWUserModel> missingModelImageTracker = new HashSet<BWUserModel>();
		foreach (BWUserModel localModel in localModels)
		{
			missingModelImageTracker.Add(localModel);
			StartCoroutine(DownloadMissingImages(localModel, missingModelImageTracker));
		}
		while (missingModelImageTracker.Count > 0)
		{
			yield return null;
		}
	}

	private IEnumerator DownloadMissingImages(BWUserModel model, HashSet<BWUserModel> missingModelImageTracker)
	{
		string imageHDPath = LocalModelImageHDPath(model);
		string iconHDPath = LocalModelIconHDPath(model);
		bool imageExists = File.Exists(imageHDPath);
		bool iconExists = File.Exists(iconHDPath);
		long num = ((!imageExists) ? 0 : new FileInfo(imageHDPath).Length);
		long num2 = ((!iconExists) ? 0 : new FileInfo(iconHDPath).Length);
		if (imageExists && iconExists && num > 200 && num2 > 200)
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
	}

	private void SortModelList()
	{
		localModels.Sort((BWUserModel x, BWUserModel y) => y.createdAt.CompareTo(x.createdAt));
	}

	public void DeleteLocalModel(string modelId)
	{
		BWUserModel modelWithLocalId = GetModelWithLocalId(modelId);
		if (modelWithLocalId != null)
		{
			if (!string.IsNullOrEmpty(modelWithLocalId.modelID))
			{
				DeleteModelRemote(modelWithLocalId.modelID);
			}
			string modelFolder = LocalModelFolder(modelWithLocalId);
			DeleteLocalModelFolder(modelFolder);
			localModels.Remove(modelWithLocalId);
			NotifyListeners();
		}
	}

	private void DeleteModelRemote(string modelID)
	{
		string path = $"/api/v1/user_models/{modelID}";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("DELETE", path);
		BWStandalone.Overlays.SetUIBusy(busy: true);
		bWAPIRequestBase.onSuccess = delegate
		{
			BWLog.Info("Deleted model: " + modelID);
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info("Failed to delete model: " + modelID + ", error: " + error.message);
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	private void DeleteLocalModelFolder(string modelFolder)
	{
		string[] files = Directory.GetFiles(modelFolder);
		string[] array = files;
		foreach (string path in array)
		{
			File.Delete(path);
		}
		Directory.Delete(modelFolder);
	}

	public void SetModelSourceLocked(string localModelID, bool locked)
	{
		BWUserModel modelWithLocalId = GetModelWithLocalId(localModelID);
		if (modelWithLocalId == null)
		{
			BWLog.Error("No local model with id: " + localModelID);
			return;
		}
		modelWithLocalId.SetSourceLocked(locked);
		SaveModelMetadata(modelWithLocalId);
		SaveModelRemote(modelWithLocalId, null);
		NotifyListeners();
	}

	public void ChangeModelPriceMarkup(BWUserModel model, int change)
	{
		model.ChangePriceMarkup(change);
		SaveModelMetadata(model);
		NotifyListeners();
	}

	public void ResetModelPriceMarkup(BWUserModel model)
	{
		model.ResetPriceMarkup();
		SaveModelMetadata(model);
		NotifyListeners();
	}
}
