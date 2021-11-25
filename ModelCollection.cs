using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x020001BC RID: 444
public class ModelCollection
{
	// Token: 0x060017ED RID: 6125 RVA: 0x000A952D File Offset: 0x000A792D
	public ModelCollection()
	{
		this.useScarcity = BW.Options.useScarcity();
	}

	// Token: 0x060017EE RID: 6126 RVA: 0x000A9558 File Offset: 0x000A7958
	public ModelData FindSimilarModel(List<Block> blocks)
	{
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (Block block in blocks)
		{
			list.Add(Blocksworld.CloneBlockTiles(block, false, true));
		}
		return this.FindSimilarModel(list);
	}

	// Token: 0x060017EF RID: 6127 RVA: 0x000A95C4 File Offset: 0x000A79C4
	public ModelData FindSimilarModel(List<List<List<Tile>>> model)
	{
		string a = ModelUtils.GenerateHashString(model);
		foreach (ModelData modelData in this.models)
		{
			if (string.Equals(a, modelData.hash))
			{
				return modelData;
			}
		}
		return null;
	}

	// Token: 0x060017F0 RID: 6128 RVA: 0x000A963C File Offset: 0x000A7A3C
	public bool ContainsSimilarModel(List<List<List<Tile>>> model)
	{
		return this.FindSimilarModel(model) != null;
	}

	// Token: 0x17000071 RID: 113
	// (get) Token: 0x060017F1 RID: 6129 RVA: 0x000A964B File Offset: 0x000A7A4B
	public static int tileSize
	{
		get
		{
			return (int)((float)ScreenshotUtils.iconSizeSD * NormalizedScreen.scale);
		}
	}

	// Token: 0x060017F2 RID: 6130 RVA: 0x000A965C File Offset: 0x000A7A5C
	public void ClearTempTextures()
	{
		UnityEngine.Object.Destroy(this.tempIconSD);
		UnityEngine.Object.Destroy(this.tempIconHD);
		UnityEngine.Object.Destroy(this.tempSnapshotHD);
		UnityEngine.Object.Destroy(this.tempSnapshotSD);
		this.tempIconSD = null;
		this.tempIconHD = null;
		this.tempSnapshotSD = null;
		this.tempSnapshotHD = null;
	}

	// Token: 0x060017F3 RID: 6131 RVA: 0x000A96B1 File Offset: 0x000A7AB1
	public void SetTempIcon(Texture2D tex, bool hd)
	{
		if (hd)
		{
			this.tempIconHD = tex;
		}
		else
		{
			this.tempIconSD = tex;
		}
	}

	// Token: 0x060017F4 RID: 6132 RVA: 0x000A96CC File Offset: 0x000A7ACC
	public void SetTempSnapshot(Texture2D tex, bool hd)
	{
		if (hd)
		{
			this.tempSnapshotHD = tex;
		}
		else
		{
			this.tempSnapshotSD = tex;
		}
	}

	// Token: 0x17000072 RID: 114
	// (get) Token: 0x060017F5 RID: 6133 RVA: 0x000A96E7 File Offset: 0x000A7AE7
	public bool CanSaveModels
	{
		get
		{
			return this.models.Count < 1024;
		}
	}

	// Token: 0x060017F6 RID: 6134 RVA: 0x000A96FB File Offset: 0x000A7AFB
	public void SetTempName(string name)
	{
		this.tempName = name;
	}

	// Token: 0x060017F7 RID: 6135 RVA: 0x000A9704 File Offset: 0x000A7B04
	public void SaveToModelCollection(List<List<List<Tile>>> blockList, Dictionary<GAF, int> gafUsage)
	{
		if (this.models.Count >= 1024)
		{
			Blocksworld.UI.Dialog.ShowMaximumModelsDialog();
			return;
		}
		this.modelSaveInProgress = true;
		Action callback = delegate()
		{
			byte[] iconBytesSD = (!(this.tempIconSD != null)) ? null : this.tempIconSD.EncodeToPNG();
			byte[] iconBytesHD = (!(this.tempIconHD != null)) ? null : this.tempIconHD.EncodeToPNG();
			byte[] imageBytesSD = (!(this.tempSnapshotSD != null)) ? null : this.tempSnapshotSD.EncodeToPNG();
			byte[] imageBytesHD = (!(this.tempSnapshotHD != null)) ? null : this.tempSnapshotHD.EncodeToPNG();
			string jsonforModel = ModelUtils.GetJSONForModel(blockList);
			HashSet<Predicate> infinitePredicates = new HashSet<Predicate>
			{
				Block.predicatePlaySoundDurational
			};
			string blockIDInventoryString = Scarcity.GetBlockIDInventoryString(gafUsage, infinitePredicates);
			string hash = ModelUtils.GenerateHashString(blockList);
			WorldSession.platformDelegate.SaveAsNewModel(this.tempName, jsonforModel, blockIDInventoryString, hash, iconBytesSD, iconBytesHD, imageBytesSD, imageBytesHD);
			this.ClearTempTextures();
			this.tempName = string.Empty;
			this.modelSaveInProgress = false;
		};
		Blocksworld.bw.StartCoroutine(this.RenderMissingIcons(blockList, callback));
	}

	// Token: 0x060017F8 RID: 6136 RVA: 0x000A9780 File Offset: 0x000A7B80
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
					if (ModelCollection.IsDisallowedInModel(gaf))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	// Token: 0x060017F9 RID: 6137 RVA: 0x000A9808 File Offset: 0x000A7C08
	public static bool IsDisallowedInModel(GAF gaf)
	{
		bool flag = gaf.Predicate == Block.predicateCreate;
		if (flag)
		{
			string stringArg = Util.GetStringArg(gaf.Args, 0, string.Empty);
			if (ProfileBlocksterUtils.IsProfileBlockType(stringArg))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060017FA RID: 6138 RVA: 0x000A984C File Offset: 0x000A7C4C
	private IEnumerator RenderMissingIcons(List<List<List<Tile>>> model, Action callback)
	{
		if (this.tempIconSD == null)
		{
			ScreenshotUtils.GenerateModelIconTexture(model, false, delegate(Texture2D t)
			{
				this.tempIconSD = t;
			});
		}
		while (this.tempIconSD == null)
		{
			yield return null;
		}
		if (this.tempIconHD == null)
		{
			ScreenshotUtils.GenerateModelIconTexture(model, true, delegate(Texture2D t)
			{
				this.tempIconHD = t;
			});
		}
		while (this.tempIconHD == null)
		{
			yield return null;
		}
		if (this.tempSnapshotSD == null)
		{
			ScreenshotUtils.GenerateModelSnapshotTexture(model, false, delegate(Texture2D t)
			{
				this.tempSnapshotSD = t;
			});
		}
		while (this.tempSnapshotSD == null)
		{
			yield return null;
		}
		if (this.tempSnapshotHD == null)
		{
			ScreenshotUtils.GenerateModelSnapshotTexture(model, true, delegate(Texture2D t)
			{
				this.tempSnapshotHD = t;
			});
		}
		while (this.tempSnapshotHD == null)
		{
			yield return null;
		}
		callback();
		yield break;
	}

	// Token: 0x060017FB RID: 6139 RVA: 0x000A9878 File Offset: 0x000A7C78
	public List<ModelData> RefreshScarcity()
	{
		List<ModelData> list = new List<ModelData>();
		if (!this.useScarcity)
		{
			return list;
		}
		for (int i = 0; i < this.models.Count; i++)
		{
			int num = Blocksworld.clipboard.AvailableModelCount(this.models[i], null, null, true);
			GAF key = new GAF(Block.predicateCreateModel, new object[]
			{
				i
			});
			bool flag;
			if (Scarcity.inventory.ContainsKey(key))
			{
				int num2 = Scarcity.inventory[key];
				flag = (num != num2);
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				Scarcity.inventory[key] = num;
				list.Add(this.models[i]);
			}
		}
		return list;
	}

	// Token: 0x060017FC RID: 6140 RVA: 0x000A9940 File Offset: 0x000A7D40
	public string GetPathToIcon(GAF modelGAF)
	{
		int num = (int)modelGAF.Args[0];
		if (num < Blocksworld.modelCollection.models.Count)
		{
			return this.models[num].pathToIconFile;
		}
		return null;
	}

	// Token: 0x060017FD RID: 6141 RVA: 0x000A9984 File Offset: 0x000A7D84
	public string GetModelID(GAF modelGAF)
	{
		int num = (int)modelGAF.Args[0];
		if (num < Blocksworld.modelCollection.models.Count)
		{
			return this.models[num].id;
		}
		return null;
	}

	// Token: 0x060017FE RID: 6142 RVA: 0x000A99C8 File Offset: 0x000A7DC8
	public string GetModelType(GAF modelGAF)
	{
		int num = (int)modelGAF.Args[0];
		if (num < Blocksworld.modelCollection.models.Count)
		{
			return this.models[num].type;
		}
		return null;
	}

	// Token: 0x060017FF RID: 6143 RVA: 0x000A9A0C File Offset: 0x000A7E0C
	public string GetModelLabel(GAF modelGAF)
	{
		int num = (int)modelGAF.Args[0];
		if (num < Blocksworld.modelCollection.models.Count)
		{
			return this.models[num].name;
		}
		return null;
	}

	// Token: 0x06001800 RID: 6144 RVA: 0x000A9A50 File Offset: 0x000A7E50
	public bool IsSourceLocked(GAF modelGAF)
	{
		int num = (int)modelGAF.Args[0];
		return num < Blocksworld.modelCollection.models.Count && this.models[num].sourceLocked;
	}

	// Token: 0x06001801 RID: 6145 RVA: 0x000A9A94 File Offset: 0x000A7E94
	public void Clear()
	{
		for (int i = 0; i < this.models.Count; i++)
		{
			if (this.models[i].tile != null)
			{
				this.models[i].tile.Destroy();
			}
		}
		this.models = new List<ModelData>();
		this.ClearTempTextures();
		GC.Collect();
	}

	// Token: 0x06001802 RID: 6146 RVA: 0x000A9B00 File Offset: 0x000A7F00
	public void LoadFromJSON(JObject jobj, bool includeOfflineModels)
	{
		if (this.models == null)
		{
			this.models = new List<ModelData>();
		}
		if (Blocksworld.clipboard == null)
		{
			Blocksworld.SetupClipboard();
		}
		List<JObject> arrayValue = jobj.ArrayValue;
		if (arrayValue == null)
		{
			BWLog.Error("Could not parse model array");
			this.modelLoadInProgress = false;
			WorldSession.current.OnModelLoadComplete();
			return;
		}
		this.modelLoadInProgress = true;
		List<ModelData> list = new List<ModelData>();
		for (int i = 0; i < arrayValue.Count; i++)
		{
			if (this.models.Count >= 1024)
			{
				BWLog.Info("Maximum model count (" + 1024 + ") hit");
				break;
			}
			Dictionary<string, JObject> objectValue = arrayValue[i].ObjectValue;
			JObject jobject;
			JObject jobject2;
			JObject jobject3;
			JObject jobject4;
			if (objectValue.TryGetValue("modelId", out jobject) && objectValue.TryGetValue("modelType", out jobject2) && objectValue.TryGetValue("modelName", out jobject3) && objectValue.TryGetValue("iconFilePath", out jobject4))
			{
				ModelData modelData = new ModelData();
				modelData.id = jobject.StringValue;
				modelData.type = jobject2.StringValue;
				modelData.name = jobject3.StringValue;
				modelData.pathToIconFile = jobject4.StringValue;
				if (includeOfflineModels || !modelData.id.StartsWith("offline"))
				{
					if (objectValue.ContainsKey("prevOfflineModelId"))
					{
						string stringValue = objectValue["prevOfflineModelId"].StringValue;
						for (int j = 0; j < this.models.Count; j++)
						{
							if (this.models[j].id == stringValue && !this.models[j].hidden)
							{
								this.models[j].hidden = true;
							}
						}
					}
					if (objectValue.ContainsKey("modelSourceLocked"))
					{
						modelData.sourceLocked = objectValue["modelSourceLocked"].BooleanValue;
					}
					bool flag = false;
					for (int k = 0; k < this.models.Count; k++)
					{
						if (this.models[k].type == modelData.type && this.models[k].id == modelData.id)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						JObject jobject5;
						JObject jobject6;
						bool flag2;
						if (objectValue.TryGetValue("blocks_inventory_str", out jobject5) && objectValue.TryGetValue("source_equality_checksum_str", out jobject6))
						{
							string stringValue2 = jobject5.StringValue;
							if (string.IsNullOrEmpty(stringValue2) || string.IsNullOrEmpty(jobject6.StringValue))
							{
								flag2 = true;
							}
							else
							{
								modelData.gafUsage = Scarcity.ReadBlockIDInventoryString(stringValue2);
								modelData.uniqueBlockNames = Scarcity.GetUniqueBlockNames(modelData.gafUsage);
								modelData.hash = jobject6.StringValue;
								flag2 = !modelData.hash.StartsWith(ModelUtils.ChecksumVersion);
							}
						}
						else
						{
							flag2 = true;
						}
						if (!flag2 || modelData.Setup())
						{
							this.models.Add(modelData);
							list.Add(modelData);
						}
					}
				}
			}
			else
			{
				BWLog.Error("Bad model data in slot " + i);
			}
		}
		BWLog.Info("Loaded " + list.Count + " models");
		this.AddModelTiles(list);
		this.RefreshScarcity();
		Blocksworld.buildPanel.Layout();
		this.modelLoadInProgress = false;
		WorldSession.current.OnModelLoadComplete();
	}

	// Token: 0x06001803 RID: 6147 RVA: 0x000A9EDA File Offset: 0x000A82DA
	public void LoadModelCollectionForStandalone()
	{
		this.Clear();
		this.LoadLocalModelsFromDataManager();
		this.LoadPurchasedU2UModels();
		this.RefreshScarcity();
		Blocksworld.buildPanel.Layout();
		this.modelLoadInProgress = false;
		WorldSession.current.OnModelLoadComplete();
	}

	// Token: 0x06001804 RID: 6148 RVA: 0x000A9F10 File Offset: 0x000A8310
	public void UpdateLocalModelCollectionForStandalone()
	{
		this.LoadLocalModelsFromDataManager();
		this.RefreshScarcity();
		Blocksworld.buildPanel.Layout();
		this.modelLoadInProgress = false;
	}

	// Token: 0x06001805 RID: 6149 RVA: 0x000A9F30 File Offset: 0x000A8330
	public void LoadLocalModelsFromDataManager()
	{
		if (!BWUserModelsDataManager.Instance.localModelsLoaded)
		{
			BWLog.Error("BWUserModelsDataManager not loaded");
			return;
		}
		if (this.models == null)
		{
			this.models = new List<ModelData>();
		}
		if (Blocksworld.clipboard == null)
		{
			Blocksworld.SetupClipboard();
		}
		List<ModelData> list = new List<ModelData>();
		foreach (BWUserModel bwuserModel in BWUserModelsDataManager.Instance.localModels)
		{
			bool flag = false;
			for (int i = 0; i < this.models.Count; i++)
			{
				if (this.models[i].type == "user_model" && this.models[i].id == bwuserModel.localID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				ModelData modelData = new ModelData();
				modelData.id = bwuserModel.localID;
				modelData.type = "user_model";
				modelData.name = Util.FixNonAscii(bwuserModel.modelShortTitle);
				modelData.pathToIconFile = BWFilesystem.FileProtocolPrefixStr + Path.Combine(Path.Combine(BWFilesystem.CurrentUserModelsFolder, bwuserModel.localID), "iconHD.png");
				if (string.IsNullOrEmpty(bwuserModel.blocksInventoryStr) || string.IsNullOrEmpty(bwuserModel.sourceEqualityChecksum))
				{
					if (!modelData.Setup())
					{
						continue;
					}
				}
				else if (!bwuserModel.sourceEqualityChecksum.StartsWith(ModelUtils.ChecksumVersion))
				{
					if (!modelData.Setup())
					{
						continue;
					}
				}
				else
				{
					modelData.gafUsage = Scarcity.ReadBlockIDInventoryString(bwuserModel.blocksInventoryStr);
					modelData.uniqueBlockNames = Scarcity.GetUniqueBlockNames(modelData.gafUsage);
					modelData.hash = bwuserModel.sourceEqualityChecksum;
				}
				this.models.Add(modelData);
				list.Add(modelData);
			}
		}
		this.AddModelTiles(list);
	}

	// Token: 0x06001806 RID: 6150 RVA: 0x000AA160 File Offset: 0x000A8560
	private void LoadPurchasedU2UModels()
	{
		List<ModelData> list = new List<ModelData>();
		foreach (BWU2UModel bwu2UModel in BWU2UModelDataManager.Instance.PurchasedU2UModels())
		{
			ModelData modelData = new ModelData();
			modelData.id = bwu2UModel.modelID;
			modelData.type = "u2u_model";
			modelData.name = Util.FixNonAscii(bwu2UModel.modelShortTitle);
			modelData.pathToIconFile = bwu2UModel.iconUrl;
			modelData.source = bwu2UModel.sourceJsonStr;
			modelData.sourceLocked = bwu2UModel.sourceLocked;
			if (string.IsNullOrEmpty(bwu2UModel.blocksInventoryStr))
			{
				if (!modelData.Setup())
				{
					continue;
				}
			}
			else
			{
				modelData.gafUsage = Scarcity.ReadBlockIDInventoryString(bwu2UModel.blocksInventoryStr);
				modelData.uniqueBlockNames = Scarcity.GetUniqueBlockNames(modelData.gafUsage);
			}
			if (!string.IsNullOrEmpty(modelData.source) && modelData.sourceLocked)
			{
				modelData.source = Blocksworld.LockModelJSON(modelData.source);
			}
			this.models.Add(modelData);
			list.Add(modelData);
		}
		this.AddModelTiles(list);
	}

	// Token: 0x06001807 RID: 6151 RVA: 0x000AA2A0 File Offset: 0x000A86A0
	private void AddModelTiles(List<ModelData> modelList)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < modelList.Count; i++)
		{
			ModelData modelData = modelList[i];
			if (!this.models.Contains(modelData))
			{
				BWLog.Error("trying to create tile for model not in model collection");
			}
			else
			{
				int num = this.models.IndexOf(modelList[i]);
				GAF gaf = new GAF("Model.Create", new object[]
				{
					num
				});
				int section = 0;
				if (modelData.type == "u2u_model")
				{
					section = 1;
				}
				else if (modelData.type == "building_set_model")
				{
					section = 2;
				}
				modelData.tile = Blocksworld.buildPanel.AddTileToBuildPanel(gaf, TabBarTabId.Models, section, -1);
				list.Add(Util.FixNonAscii(modelData.name));
			}
		}
		TileIconManager.Instance.labelAtlas.AddNewLabels(list);
	}

	// Token: 0x06001808 RID: 6152 RVA: 0x000AA390 File Offset: 0x000A8790
	public static string GenerateMetadataForModelSource(string modelSourceJsonStr)
	{
		List<List<List<Tile>>> list = ModelUtils.ParseModelString(modelSourceJsonStr);
		string value = string.Empty;
		string value2 = string.Empty;
		if (list != null)
		{
			Dictionary<GAF, int> normalizedInventoryUse = Scarcity.GetNormalizedInventoryUse(list, WorldType.User, true);
			value = Scarcity.GetBlockIDInventoryString(normalizedInventoryUse, ModelCollection.infiniteQuantityPredicates);
			value2 = ModelUtils.GenerateHashString(list);
		}
		Dictionary<string, string> obj = new Dictionary<string, string>
		{
			{
				"blocks_inventory_str",
				value
			},
			{
				"source_equality_checksum_str",
				value2
			}
		};
		return JSONEncoder.Encode(obj);
	}

	// Token: 0x06001809 RID: 6153 RVA: 0x000AA400 File Offset: 0x000A8800
	public static string GenerateBlocksInventoryForModelSource(string modelSourceJsonStr)
	{
		List<List<List<Tile>>> list = ModelUtils.ParseModelString(modelSourceJsonStr);
		string result = string.Empty;
		if (list != null)
		{
			Dictionary<GAF, int> normalizedInventoryUse = Scarcity.GetNormalizedInventoryUse(list, WorldType.User, true);
			result = Scarcity.GetBlockIDInventoryString(normalizedInventoryUse, ModelCollection.infiniteQuantityPredicates);
		}
		return result;
	}

	// Token: 0x0600180A RID: 6154 RVA: 0x000AA438 File Offset: 0x000A8838
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

	// Token: 0x040012EC RID: 4844
	public List<ModelData> models = new List<ModelData>();

	// Token: 0x040012ED RID: 4845
	public bool iconLoadInProgress;

	// Token: 0x040012EE RID: 4846
	public bool modelSaveInProgress;

	// Token: 0x040012EF RID: 4847
	public bool modelLoadInProgress;

	// Token: 0x040012F0 RID: 4848
	private Texture2D tempIconSD;

	// Token: 0x040012F1 RID: 4849
	private Texture2D tempSnapshotSD;

	// Token: 0x040012F2 RID: 4850
	private Texture2D tempIconHD;

	// Token: 0x040012F3 RID: 4851
	private Texture2D tempSnapshotHD;

	// Token: 0x040012F4 RID: 4852
	private string tempName;

	// Token: 0x040012F5 RID: 4853
	private bool useScarcity = true;

	// Token: 0x040012F6 RID: 4854
	private const int maxModelCount = 1024;

	// Token: 0x040012F7 RID: 4855
	private static HashSet<Predicate> infiniteQuantityPredicates = new HashSet<Predicate>
	{
		Block.predicatePlaySoundDurational
	};
}
