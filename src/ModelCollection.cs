using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Blocks;
using SimpleJSON;
using UnityEngine;

public class ModelCollection
{
	public List<ModelData> models = new List<ModelData>();

	public bool iconLoadInProgress;

	public bool modelSaveInProgress;

	public bool modelLoadInProgress;

	private Texture2D tempIconSD;

	private Texture2D tempSnapshotSD;

	private Texture2D tempIconHD;

	private Texture2D tempSnapshotHD;

	private string tempName;

	private bool useScarcity = true;

	private const int maxModelCount = 1024;

	private static HashSet<Predicate> infiniteQuantityPredicates = new HashSet<Predicate> { Block.predicatePlaySoundDurational };

	public static int tileSize => (int)((float)ScreenshotUtils.iconSizeSD * NormalizedScreen.scale);

	public bool CanSaveModels => models.Count < 1024;

	public ModelCollection()
	{
		useScarcity = BW.Options.useScarcity();
	}

	public ModelData FindSimilarModel(List<Block> blocks)
	{
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (Block block in blocks)
		{
			list.Add(Blocksworld.CloneBlockTiles(block, excludeFirstRow: false, ignoreLockedGroupTiles: true));
		}
		return FindSimilarModel(list);
	}

	public ModelData FindSimilarModel(List<List<List<Tile>>> model)
	{
		string a = ModelUtils.GenerateHashString(model);
		foreach (ModelData model2 in models)
		{
			if (string.Equals(a, model2.hash))
			{
				return model2;
			}
		}
		return null;
	}

	public bool ContainsSimilarModel(List<List<List<Tile>>> model)
	{
		return FindSimilarModel(model) != null;
	}

	public void ClearTempTextures()
	{
		UnityEngine.Object.Destroy(tempIconSD);
		UnityEngine.Object.Destroy(tempIconHD);
		UnityEngine.Object.Destroy(tempSnapshotHD);
		UnityEngine.Object.Destroy(tempSnapshotSD);
		tempIconSD = null;
		tempIconHD = null;
		tempSnapshotSD = null;
		tempSnapshotHD = null;
	}

	public void SetTempIcon(Texture2D tex, bool hd)
	{
		if (hd)
		{
			tempIconHD = tex;
		}
		else
		{
			tempIconSD = tex;
		}
	}

	public void SetTempSnapshot(Texture2D tex, bool hd)
	{
		if (hd)
		{
			tempSnapshotHD = tex;
		}
		else
		{
			tempSnapshotSD = tex;
		}
	}

	public void SetTempName(string name)
	{
		tempName = name;
	}

	public void SaveToModelCollection(List<List<List<Tile>>> blockList, Dictionary<GAF, int> gafUsage)
	{
		if (models.Count >= 1024)
		{
			Blocksworld.UI.Dialog.ShowMaximumModelsDialog();
			return;
		}
		modelSaveInProgress = true;
		Action callback = delegate
		{
			byte[] iconBytesSD = ((!(tempIconSD != null)) ? null : tempIconSD.EncodeToPNG());
			byte[] iconBytesHD = ((!(tempIconHD != null)) ? null : tempIconHD.EncodeToPNG());
			byte[] imageBytesSD = ((!(tempSnapshotSD != null)) ? null : tempSnapshotSD.EncodeToPNG());
			byte[] imageBytesHD = ((!(tempSnapshotHD != null)) ? null : tempSnapshotHD.EncodeToPNG());
			string jSONForModel = ModelUtils.GetJSONForModel(blockList);
			HashSet<Predicate> infinitePredicates = new HashSet<Predicate> { Block.predicatePlaySoundDurational };
			string blockIDInventoryString = Scarcity.GetBlockIDInventoryString(gafUsage, infinitePredicates);
			string hash = ModelUtils.GenerateHashString(blockList);
			WorldSession.platformDelegate.SaveAsNewModel(tempName, jSONForModel, blockIDInventoryString, hash, iconBytesSD, iconBytesHD, imageBytesSD, imageBytesHD);
			ClearTempTextures();
			tempName = string.Empty;
			modelSaveInProgress = false;
		};
		Blocksworld.bw.StartCoroutine(RenderMissingIcons(blockList, callback));
	}

	public static bool ModelContainsDisallowedTile(List<List<List<Tile>>> model)
	{
		for (int i = 0; i < model.Count; i++)
		{
			List<List<Tile>> list = model[i];
			for (int j = 0; j < list.Count; j++)
			{
				List<Tile> list2 = list[j];
				for (int k = 0; k < list2.Count; k++)
				{
					Tile tile = list2[k];
					GAF gaf = tile.gaf;
					if (IsDisallowedInModel(gaf))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool IsDisallowedInModel(GAF gaf)
	{
		if (gaf.Predicate == Block.predicateCreate)
		{
			string stringArg = Util.GetStringArg(gaf.Args, 0, string.Empty);
			if (ProfileBlocksterUtils.IsProfileBlockType(stringArg))
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator RenderMissingIcons(List<List<List<Tile>>> model, Action callback)
	{
		if (tempIconSD == null)
		{
			ScreenshotUtils.GenerateModelIconTexture(model, hd: false, delegate(Texture2D t)
			{
				tempIconSD = t;
			});
		}
		while (tempIconSD == null)
		{
			yield return null;
		}
		if (tempIconHD == null)
		{
			ScreenshotUtils.GenerateModelIconTexture(model, hd: true, delegate(Texture2D t)
			{
				tempIconHD = t;
			});
		}
		while (tempIconHD == null)
		{
			yield return null;
		}
		if (tempSnapshotSD == null)
		{
			ScreenshotUtils.GenerateModelSnapshotTexture(model, hd: false, delegate(Texture2D t)
			{
				tempSnapshotSD = t;
			});
		}
		while (tempSnapshotSD == null)
		{
			yield return null;
		}
		if (tempSnapshotHD == null)
		{
			ScreenshotUtils.GenerateModelSnapshotTexture(model, hd: true, delegate(Texture2D t)
			{
				tempSnapshotHD = t;
			});
		}
		while (tempSnapshotHD == null)
		{
			yield return null;
		}
		callback();
	}

	public List<ModelData> RefreshScarcity()
	{
		List<ModelData> list = new List<ModelData>();
		if (!useScarcity)
		{
			return list;
		}
		for (int i = 0; i < models.Count; i++)
		{
			int num = Blocksworld.clipboard.AvailableModelCount(models[i]);
			GAF key = new GAF(Block.predicateCreateModel, i);
			bool flag;
			if (Scarcity.inventory.ContainsKey(key))
			{
				int num2 = Scarcity.inventory[key];
				flag = num != num2;
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				Scarcity.inventory[key] = num;
				list.Add(models[i]);
			}
		}
		return list;
	}

	public string GetPathToIcon(GAF modelGAF)
	{
		int num = (int)modelGAF.Args[0];
		if (num < Blocksworld.modelCollection.models.Count)
		{
			return models[num].pathToIconFile;
		}
		return null;
	}

	public string GetModelID(GAF modelGAF)
	{
		int num = (int)modelGAF.Args[0];
		if (num < Blocksworld.modelCollection.models.Count)
		{
			return models[num].id;
		}
		return null;
	}

	public string GetModelType(GAF modelGAF)
	{
		int num = (int)modelGAF.Args[0];
		if (num < Blocksworld.modelCollection.models.Count)
		{
			return models[num].type;
		}
		return null;
	}

	public string GetModelLabel(GAF modelGAF)
	{
		int num = (int)modelGAF.Args[0];
		if (num < Blocksworld.modelCollection.models.Count)
		{
			return models[num].name;
		}
		return null;
	}

	public bool IsSourceLocked(GAF modelGAF)
	{
		int num = (int)modelGAF.Args[0];
		if (num < Blocksworld.modelCollection.models.Count)
		{
			return models[num].sourceLocked;
		}
		return false;
	}

	public void Clear()
	{
		for (int i = 0; i < models.Count; i++)
		{
			if (models[i].tile != null)
			{
				models[i].tile.Destroy();
			}
		}
		models = new List<ModelData>();
		ClearTempTextures();
		GC.Collect();
	}

	public void LoadFromJSON(JObject jobj, bool includeOfflineModels)
	{
		if (models == null)
		{
			models = new List<ModelData>();
		}
		if (Blocksworld.clipboard == null)
		{
			Blocksworld.SetupClipboard();
		}
		List<JObject> arrayValue = jobj.ArrayValue;
		if (arrayValue == null)
		{
			BWLog.Error("Could not parse model array");
			modelLoadInProgress = false;
			WorldSession.current.OnModelLoadComplete();
			return;
		}
		modelLoadInProgress = true;
		List<ModelData> list = new List<ModelData>();
		for (int i = 0; i < arrayValue.Count; i++)
		{
			if (models.Count >= 1024)
			{
				BWLog.Info("Maximum model count (" + 1024 + ") hit");
				break;
			}
			Dictionary<string, JObject> objectValue = arrayValue[i].ObjectValue;
			if (objectValue.TryGetValue("modelId", out var value) && objectValue.TryGetValue("modelType", out var value2) && objectValue.TryGetValue("modelName", out var value3) && objectValue.TryGetValue("iconFilePath", out var value4))
			{
				ModelData modelData = new ModelData();
				modelData.id = value.StringValue;
				modelData.type = value2.StringValue;
				modelData.name = value3.StringValue;
				modelData.pathToIconFile = value4.StringValue;
				if (!includeOfflineModels && modelData.id.StartsWith("offline"))
				{
					continue;
				}
				if (objectValue.ContainsKey("prevOfflineModelId"))
				{
					string stringValue = objectValue["prevOfflineModelId"].StringValue;
					for (int j = 0; j < models.Count; j++)
					{
						if (models[j].id == stringValue && !models[j].hidden)
						{
							models[j].hidden = true;
						}
					}
				}
				if (objectValue.ContainsKey("modelSourceLocked"))
				{
					modelData.sourceLocked = objectValue["modelSourceLocked"].BooleanValue;
				}
				bool flag = false;
				for (int k = 0; k < models.Count; k++)
				{
					if (models[k].type == modelData.type && models[k].id == modelData.id)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
				bool flag2;
				if (objectValue.TryGetValue("blocks_inventory_str", out var value5) && objectValue.TryGetValue("source_equality_checksum_str", out var value6))
				{
					string stringValue2 = value5.StringValue;
					if (string.IsNullOrEmpty(stringValue2) || string.IsNullOrEmpty(value6.StringValue))
					{
						flag2 = true;
					}
					else
					{
						modelData.gafUsage = Scarcity.ReadBlockIDInventoryString(stringValue2);
						modelData.uniqueBlockNames = Scarcity.GetUniqueBlockNames(modelData.gafUsage);
						modelData.hash = value6.StringValue;
						flag2 = !modelData.hash.StartsWith(ModelUtils.ChecksumVersion);
					}
				}
				else
				{
					flag2 = true;
				}
				if (!flag2 || modelData.Setup())
				{
					models.Add(modelData);
					list.Add(modelData);
				}
			}
			else
			{
				BWLog.Error("Bad model data in slot " + i);
			}
		}
		BWLog.Info("Loaded " + list.Count + " models");
		AddModelTiles(list);
		RefreshScarcity();
		Blocksworld.buildPanel.Layout();
		modelLoadInProgress = false;
		WorldSession.current.OnModelLoadComplete();
	}

	public void LoadModelCollectionForStandalone()
	{
		Clear();
		LoadLocalModelsFromDataManager();
		LoadPurchasedU2UModels();
		RefreshScarcity();
		Blocksworld.buildPanel.Layout();
		modelLoadInProgress = false;
		WorldSession.current.OnModelLoadComplete();
	}

	public void UpdateLocalModelCollectionForStandalone()
	{
		LoadLocalModelsFromDataManager();
		RefreshScarcity();
		Blocksworld.buildPanel.Layout();
		modelLoadInProgress = false;
	}

	public void LoadLocalModelsFromDataManager()
	{
		if (!BWUserModelsDataManager.Instance.localModelsLoaded)
		{
			BWLog.Error("BWUserModelsDataManager not loaded");
			return;
		}
		if (models == null)
		{
			models = new List<ModelData>();
		}
		if (Blocksworld.clipboard == null)
		{
			Blocksworld.SetupClipboard();
		}
		List<ModelData> list = new List<ModelData>();
		foreach (BWUserModel localModel in BWUserModelsDataManager.Instance.localModels)
		{
			bool flag = false;
			for (int i = 0; i < models.Count; i++)
			{
				if (models[i].type == "user_model" && models[i].id == localModel.localID)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			ModelData modelData = new ModelData();
			modelData.id = localModel.localID;
			modelData.type = "user_model";
			modelData.name = Util.FixNonAscii(localModel.modelShortTitle);
			modelData.pathToIconFile = BWFilesystem.FileProtocolPrefixStr + Path.Combine(Path.Combine(BWFilesystem.CurrentUserModelsFolder, localModel.localID), "iconHD.png");
			if (string.IsNullOrEmpty(localModel.blocksInventoryStr) || string.IsNullOrEmpty(localModel.sourceEqualityChecksum))
			{
				if (!modelData.Setup())
				{
					continue;
				}
			}
			else if (!localModel.sourceEqualityChecksum.StartsWith(ModelUtils.ChecksumVersion))
			{
				if (!modelData.Setup())
				{
					continue;
				}
			}
			else
			{
				modelData.gafUsage = Scarcity.ReadBlockIDInventoryString(localModel.blocksInventoryStr);
				modelData.uniqueBlockNames = Scarcity.GetUniqueBlockNames(modelData.gafUsage);
				modelData.hash = localModel.sourceEqualityChecksum;
			}
			models.Add(modelData);
			list.Add(modelData);
		}
		AddModelTiles(list);
	}

	private void LoadPurchasedU2UModels()
	{
		List<ModelData> list = new List<ModelData>();
		foreach (BWU2UModel item in BWU2UModelDataManager.Instance.PurchasedU2UModels())
		{
			ModelData modelData = new ModelData();
			modelData.id = item.modelID;
			modelData.type = "u2u_model";
			modelData.name = Util.FixNonAscii(item.modelShortTitle);
			modelData.pathToIconFile = item.iconUrl;
			modelData.source = item.sourceJsonStr;
			modelData.sourceLocked = item.sourceLocked;
			if (string.IsNullOrEmpty(item.blocksInventoryStr))
			{
				if (!modelData.Setup())
				{
					continue;
				}
			}
			else
			{
				modelData.gafUsage = Scarcity.ReadBlockIDInventoryString(item.blocksInventoryStr);
				modelData.uniqueBlockNames = Scarcity.GetUniqueBlockNames(modelData.gafUsage);
			}
			if (!string.IsNullOrEmpty(modelData.source) && modelData.sourceLocked)
			{
				modelData.source = Blocksworld.LockModelJSON(modelData.source);
			}
			models.Add(modelData);
			list.Add(modelData);
		}
		AddModelTiles(list);
	}

	private void AddModelTiles(List<ModelData> modelList)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < modelList.Count; i++)
		{
			ModelData modelData = modelList[i];
			if (!models.Contains(modelData))
			{
				BWLog.Error("trying to create tile for model not in model collection");
				continue;
			}
			int num = models.IndexOf(modelList[i]);
			GAF gaf = new GAF("Model.Create", num);
			int section = 0;
			if (modelData.type == "u2u_model")
			{
				section = 1;
			}
			else if (modelData.type == "building_set_model")
			{
				section = 2;
			}
			modelData.tile = Blocksworld.buildPanel.AddTileToBuildPanel(gaf, TabBarTabId.Models, section);
			list.Add(Util.FixNonAscii(modelData.name));
		}
		TileIconManager.Instance.labelAtlas.AddNewLabels(list);
	}

	public static string GenerateMetadataForModelSource(string modelSourceJsonStr)
	{
		List<List<List<Tile>>> list = ModelUtils.ParseModelString(modelSourceJsonStr);
		string value = string.Empty;
		string value2 = string.Empty;
		if (list != null)
		{
			Dictionary<GAF, int> normalizedInventoryUse = Scarcity.GetNormalizedInventoryUse(list, WorldType.User, includeLocked: true);
			value = Scarcity.GetBlockIDInventoryString(normalizedInventoryUse, infiniteQuantityPredicates);
			value2 = ModelUtils.GenerateHashString(list);
		}
		Dictionary<string, string> obj = new Dictionary<string, string>
		{
			{ "blocks_inventory_str", value },
			{ "source_equality_checksum_str", value2 }
		};
		return JSONEncoder.Encode(obj);
	}

	public static string GenerateBlocksInventoryForModelSource(string modelSourceJsonStr)
	{
		List<List<List<Tile>>> list = ModelUtils.ParseModelString(modelSourceJsonStr);
		string result = string.Empty;
		if (list != null)
		{
			Dictionary<GAF, int> normalizedInventoryUse = Scarcity.GetNormalizedInventoryUse(list, WorldType.User, includeLocked: true);
			result = Scarcity.GetBlockIDInventoryString(normalizedInventoryUse, infiniteQuantityPredicates);
		}
		return result;
	}

	public static string GenerateSourceEqualityChecksumForModelSource(string modelSourceJsonStr)
	{
		List<List<List<Tile>>> list = ModelUtils.ParseModelString(modelSourceJsonStr);
		string result = string.Empty;
		if (list != null)
		{
			result = ModelUtils.GenerateHashString(list);
		}
		return result;
	}
}
