using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003CF RID: 975
[Serializable]
public class BWUserModel
{
	// Token: 0x06002ABD RID: 10941 RVA: 0x00136314 File Offset: 0x00134714
	public BWUserModel(JObject json)
	{
		this.UpdateFromJson(json);
	}

	// Token: 0x06002ABE RID: 10942 RVA: 0x0013632C File Offset: 0x0013472C
	public BWUserModel(BWUserModel copyFrom)
	{
		this.Reset();
		this.localID = Guid.NewGuid().ToString();
		this.modelID = copyFrom.modelID;
		this.modelTitle = copyFrom.modelTitle;
		this.modelShortTitle = copyFrom.modelShortTitle;
		this.modelDescription = copyFrom.modelDescription;
		this.authorId = copyFrom.authorId;
		this.authorUsername = copyFrom.authorUsername;
		this.authorProfileImageUrl = copyFrom.authorProfileImageUrl;
		this.previewTerrain = copyFrom.previewTerrain;
		this.categoryId = copyFrom.categoryId;
		this.u2uModelId = copyFrom.u2uModelId;
		this.sourceJsonStr = copyFrom.sourceJsonStr;
		this.blocksInventoryStr = copyFrom.blocksInventoryStr;
		this.sourceEqualityChecksum = copyFrom.sourceEqualityChecksum;
		this.coinsPriceMarkup = copyFrom.coinsPriceMarkup;
		this.sourceLocked = copyFrom.sourceLocked;
		this.iconUrl = copyFrom.iconUrl;
		this.imageUrl = copyFrom.imageUrl;
		this.createdAt = copyFrom.createdAt;
		this.updatedAt = copyFrom.updatedAt;
		this.publicationStatus = copyFrom.publicationStatus;
	}

	// Token: 0x06002ABF RID: 10943 RVA: 0x00136458 File Offset: 0x00134858
	public BWUserModel(BWUserModel copyFrom, string withLocalID)
	{
		this.Reset();
		this.localID = withLocalID;
		this.modelID = copyFrom.modelID;
		this.modelTitle = copyFrom.modelTitle;
		this.modelShortTitle = copyFrom.modelShortTitle;
		this.modelDescription = copyFrom.modelDescription;
		this.authorId = copyFrom.authorId;
		this.authorUsername = copyFrom.authorUsername;
		this.authorProfileImageUrl = copyFrom.authorProfileImageUrl;
		this.previewTerrain = copyFrom.previewTerrain;
		this.categoryId = copyFrom.categoryId;
		this.u2uModelId = copyFrom.u2uModelId;
		this.sourceJsonStr = copyFrom.sourceJsonStr;
		this.blocksInventoryStr = copyFrom.blocksInventoryStr;
		this.sourceEqualityChecksum = copyFrom.sourceEqualityChecksum;
		this.iconUrl = copyFrom.iconUrl;
		this.imageUrl = copyFrom.imageUrl;
		this.coinsPriceMarkup = copyFrom.coinsPriceMarkup;
		this.sourceLocked = copyFrom.sourceLocked;
		this.createdAt = copyFrom.createdAt;
		this.updatedAt = copyFrom.updatedAt;
		this.publicationStatus = copyFrom.publicationStatus;
	}

	// Token: 0x06002AC0 RID: 10944 RVA: 0x00136570 File Offset: 0x00134970
	public BWUserModel(string modelTitle, string modelSource, string blocksInventoryStr, string sourceEqualityChecksum)
	{
		this.Reset();
		this.localID = Guid.NewGuid().ToString();
		this.modelTitle = Util.FixNonAscii(modelTitle);
		this.modelShortTitle = Util.FixNonAscii(modelTitle);
		this.sourceJsonStr = modelSource;
		this.blocksInventoryStr = blocksInventoryStr;
		this.sourceEqualityChecksum = sourceEqualityChecksum;
		this.authorId = BWUser.currentUser.userID;
		this.authorUsername = BWUser.currentUser.username;
		this.authorProfileImageUrl = BWUser.currentUser.profileImageURL;
		this.publicationStatus = BWUserModel.PublicationStatusEnum.MODERATION_PENDING;
		this.ResetPriceMarkup();
	}

	// Token: 0x170001FC RID: 508
	// (get) Token: 0x06002AC1 RID: 10945 RVA: 0x00136614 File Offset: 0x00134A14
	// (set) Token: 0x06002AC2 RID: 10946 RVA: 0x0013661C File Offset: 0x00134A1C
	public string modelID { get; protected set; }

	// Token: 0x170001FD RID: 509
	// (get) Token: 0x06002AC3 RID: 10947 RVA: 0x00136625 File Offset: 0x00134A25
	// (set) Token: 0x06002AC4 RID: 10948 RVA: 0x0013662D File Offset: 0x00134A2D
	public string localID { get; protected set; }

	// Token: 0x170001FE RID: 510
	// (get) Token: 0x06002AC5 RID: 10949 RVA: 0x00136636 File Offset: 0x00134A36
	// (set) Token: 0x06002AC6 RID: 10950 RVA: 0x0013663E File Offset: 0x00134A3E
	public string modelTitle { get; protected set; }

	// Token: 0x170001FF RID: 511
	// (get) Token: 0x06002AC7 RID: 10951 RVA: 0x00136647 File Offset: 0x00134A47
	// (set) Token: 0x06002AC8 RID: 10952 RVA: 0x0013664F File Offset: 0x00134A4F
	public string modelShortTitle { get; protected set; }

	// Token: 0x17000200 RID: 512
	// (get) Token: 0x06002AC9 RID: 10953 RVA: 0x00136658 File Offset: 0x00134A58
	// (set) Token: 0x06002ACA RID: 10954 RVA: 0x00136660 File Offset: 0x00134A60
	public string modelDescription { get; protected set; }

	// Token: 0x17000201 RID: 513
	// (get) Token: 0x06002ACB RID: 10955 RVA: 0x00136669 File Offset: 0x00134A69
	// (set) Token: 0x06002ACC RID: 10956 RVA: 0x00136671 File Offset: 0x00134A71
	public string previewTerrain { get; protected set; }

	// Token: 0x17000202 RID: 514
	// (get) Token: 0x06002ACD RID: 10957 RVA: 0x0013667A File Offset: 0x00134A7A
	// (set) Token: 0x06002ACE RID: 10958 RVA: 0x00136682 File Offset: 0x00134A82
	public string u2uModelId { get; protected set; }

	// Token: 0x17000203 RID: 515
	// (get) Token: 0x06002ACF RID: 10959 RVA: 0x0013668B File Offset: 0x00134A8B
	// (set) Token: 0x06002AD0 RID: 10960 RVA: 0x00136693 File Offset: 0x00134A93
	public int categoryId { get; protected set; }

	// Token: 0x17000204 RID: 516
	// (get) Token: 0x06002AD1 RID: 10961 RVA: 0x0013669C File Offset: 0x00134A9C
	// (set) Token: 0x06002AD2 RID: 10962 RVA: 0x001366A4 File Offset: 0x00134AA4
	public int authorId { get; protected set; }

	// Token: 0x17000205 RID: 517
	// (get) Token: 0x06002AD3 RID: 10963 RVA: 0x001366AD File Offset: 0x00134AAD
	// (set) Token: 0x06002AD4 RID: 10964 RVA: 0x001366B5 File Offset: 0x00134AB5
	public string authorUsername { get; protected set; }

	// Token: 0x17000206 RID: 518
	// (get) Token: 0x06002AD5 RID: 10965 RVA: 0x001366BE File Offset: 0x00134ABE
	// (set) Token: 0x06002AD6 RID: 10966 RVA: 0x001366C6 File Offset: 0x00134AC6
	public string authorProfileImageUrl { get; protected set; }

	// Token: 0x17000207 RID: 519
	// (get) Token: 0x06002AD7 RID: 10967 RVA: 0x001366CF File Offset: 0x00134ACF
	// (set) Token: 0x06002AD8 RID: 10968 RVA: 0x001366D7 File Offset: 0x00134AD7
	public int coinsPriceMarkup { get; private set; }

	// Token: 0x17000208 RID: 520
	// (get) Token: 0x06002AD9 RID: 10969 RVA: 0x001366E0 File Offset: 0x00134AE0
	// (set) Token: 0x06002ADA RID: 10970 RVA: 0x001366E8 File Offset: 0x00134AE8
	public int coinsPriceBlocksInventory { get; private set; }

	// Token: 0x17000209 RID: 521
	// (get) Token: 0x06002ADB RID: 10971 RVA: 0x001366F1 File Offset: 0x00134AF1
	// (set) Token: 0x06002ADC RID: 10972 RVA: 0x001366F9 File Offset: 0x00134AF9
	public int coinsPriceBlueprint { get; private set; }

	// Token: 0x1700020A RID: 522
	// (get) Token: 0x06002ADD RID: 10973 RVA: 0x00136702 File Offset: 0x00134B02
	// (set) Token: 0x06002ADE RID: 10974 RVA: 0x0013670A File Offset: 0x00134B0A
	public bool sourceLocked { get; protected set; }

	// Token: 0x1700020B RID: 523
	// (get) Token: 0x06002ADF RID: 10975 RVA: 0x00136713 File Offset: 0x00134B13
	// (set) Token: 0x06002AE0 RID: 10976 RVA: 0x0013671B File Offset: 0x00134B1B
	public string sourceJsonStr { get; protected set; }

	// Token: 0x1700020C RID: 524
	// (get) Token: 0x06002AE1 RID: 10977 RVA: 0x00136724 File Offset: 0x00134B24
	// (set) Token: 0x06002AE2 RID: 10978 RVA: 0x0013672C File Offset: 0x00134B2C
	public string sourceEqualityChecksum { get; protected set; }

	// Token: 0x1700020D RID: 525
	// (get) Token: 0x06002AE3 RID: 10979 RVA: 0x00136735 File Offset: 0x00134B35
	// (set) Token: 0x06002AE4 RID: 10980 RVA: 0x0013673D File Offset: 0x00134B3D
	public string blocksInventoryStr { get; protected set; }

	// Token: 0x1700020E RID: 526
	// (get) Token: 0x06002AE5 RID: 10981 RVA: 0x00136746 File Offset: 0x00134B46
	// (set) Token: 0x06002AE6 RID: 10982 RVA: 0x0013674E File Offset: 0x00134B4E
	public string imageSDChecksumStr { get; protected set; }

	// Token: 0x1700020F RID: 527
	// (get) Token: 0x06002AE7 RID: 10983 RVA: 0x00136757 File Offset: 0x00134B57
	// (set) Token: 0x06002AE8 RID: 10984 RVA: 0x0013675F File Offset: 0x00134B5F
	public string imageHDChecksumStr { get; protected set; }

	// Token: 0x17000210 RID: 528
	// (get) Token: 0x06002AE9 RID: 10985 RVA: 0x00136768 File Offset: 0x00134B68
	// (set) Token: 0x06002AEA RID: 10986 RVA: 0x00136770 File Offset: 0x00134B70
	public string iconSDChecksumStr { get; protected set; }

	// Token: 0x17000211 RID: 529
	// (get) Token: 0x06002AEB RID: 10987 RVA: 0x00136779 File Offset: 0x00134B79
	// (set) Token: 0x06002AEC RID: 10988 RVA: 0x00136781 File Offset: 0x00134B81
	public string iconHDChecksumStr { get; protected set; }

	// Token: 0x17000212 RID: 530
	// (get) Token: 0x06002AED RID: 10989 RVA: 0x0013678A File Offset: 0x00134B8A
	// (set) Token: 0x06002AEE RID: 10990 RVA: 0x00136792 File Offset: 0x00134B92
	public string iconUrl { get; protected set; }

	// Token: 0x17000213 RID: 531
	// (get) Token: 0x06002AEF RID: 10991 RVA: 0x0013679B File Offset: 0x00134B9B
	// (set) Token: 0x06002AF0 RID: 10992 RVA: 0x001367A3 File Offset: 0x00134BA3
	public string imageUrl { get; protected set; }

	// Token: 0x17000214 RID: 532
	// (get) Token: 0x06002AF1 RID: 10993 RVA: 0x001367AC File Offset: 0x00134BAC
	// (set) Token: 0x06002AF2 RID: 10994 RVA: 0x001367B4 File Offset: 0x00134BB4
	public DateTime createdAt { get; protected set; }

	// Token: 0x17000215 RID: 533
	// (get) Token: 0x06002AF3 RID: 10995 RVA: 0x001367BD File Offset: 0x00134BBD
	// (set) Token: 0x06002AF4 RID: 10996 RVA: 0x001367C5 File Offset: 0x00134BC5
	public DateTime updatedAt { get; protected set; }

	// Token: 0x17000216 RID: 534
	// (get) Token: 0x06002AF5 RID: 10997 RVA: 0x001367CE File Offset: 0x00134BCE
	// (set) Token: 0x06002AF6 RID: 10998 RVA: 0x001367D6 File Offset: 0x00134BD6
	public bool localChangedMetadata { get; protected set; }

	// Token: 0x17000217 RID: 535
	// (get) Token: 0x06002AF7 RID: 10999 RVA: 0x001367DF File Offset: 0x00134BDF
	public string obfuscatedSourceJsonStr
	{
		get
		{
			return Util.ObfuscateSourceForUser(this.sourceJsonStr, BWUser.currentUser.userID);
		}
	}

	// Token: 0x17000218 RID: 536
	// (get) Token: 0x06002AF8 RID: 11000 RVA: 0x001367F6 File Offset: 0x00134BF6
	internal bool isRemote
	{
		get
		{
			return !string.IsNullOrEmpty(this.modelID);
		}
	}

	// Token: 0x17000219 RID: 537
	// (get) Token: 0x06002AF9 RID: 11001 RVA: 0x00136806 File Offset: 0x00134C06
	public bool isPublished
	{
		get
		{
			return !string.IsNullOrEmpty(this.u2uModelId) && this.publicationStatus != BWUserModel.PublicationStatusEnum.NOT_PUBLISHED && this.publicationStatus != BWUserModel.PublicationStatusEnum.DELETED_BY_AUTHOR;
		}
	}

	// Token: 0x1700021A RID: 538
	// (get) Token: 0x06002AFA RID: 11002 RVA: 0x00136832 File Offset: 0x00134C32
	public bool isSellable
	{
		get
		{
			return this.CoinsBasePrice() < int.MaxValue;
		}
	}

	// Token: 0x06002AFB RID: 11003 RVA: 0x00136844 File Offset: 0x00134C44
	public int CoinsBasePrice()
	{
		if (this.basePriceSet)
		{
			return this.coinsBasePrice;
		}
		BlocksInventory inventory = BlocksInventory.FromString(this.blocksInventoryStr, true);
		this.coinsBasePrice = BWBlockItemPricing.CoinsValueOfBlocksInventory(inventory);
		this.basePriceSet = true;
		return this.coinsBasePrice;
	}

	// Token: 0x06002AFC RID: 11004 RVA: 0x0013688C File Offset: 0x00134C8C
	public void UpdateFromJson(JObject json)
	{
		this.modelID = BWJsonHelpers.IDPropertyAsStringIfExists(this.modelID, "id", json);
		this.localID = BWJsonHelpers.PropertyIfExists(this.localID, "local_id", json);
		this.modelTitle = BWJsonHelpers.PropertyIfExists(this.modelTitle, "title", json);
		this.modelShortTitle = BWJsonHelpers.PropertyIfExists(this.modelShortTitle, "short_title", json);
		this.modelDescription = BWJsonHelpers.PropertyIfExists(this.modelDescription, "description", json);
		this.authorId = BWJsonHelpers.PropertyIfExists(this.authorId, "author_id", json);
		this.authorUsername = BWJsonHelpers.PropertyIfExists(this.authorUsername, "author_username", json);
		this.authorProfileImageUrl = BWJsonHelpers.PropertyIfExists(this.authorProfileImageUrl, "author_profile_image_url", json);
		this.previewTerrain = BWJsonHelpers.PropertyIfExists(this.previewTerrain, "preview_terrain", json);
		this.categoryId = BWJsonHelpers.PropertyIfExists(this.categoryId, "model_category_id", json);
		this.u2uModelId = BWJsonHelpers.IDPropertyAsStringIfExists(this.u2uModelId, "u2u_model_id", json);
		this.sourceJsonStr = BWJsonHelpers.PropertyIfExists(this.sourceJsonStr, "source_json_str", json);
		this.sourceLocked = BWJsonHelpers.PropertyIfExists(this.sourceLocked, "source_locked", json);
		this.blocksInventoryStr = BWJsonHelpers.PropertyIfExists(this.blocksInventoryStr, "blocks_inventory_str", json);
		this.sourceEqualityChecksum = BWJsonHelpers.PropertyIfExists(this.sourceEqualityChecksum, "source_equality_checksum", json);
		this.coinsPriceMarkup = BWJsonHelpers.PropertyIfExists(this.coinsPriceMarkup, "coins_price_markup", json);
		this.coinsPriceMarkup = Mathf.Max(this.coinsPriceMarkup, 5);
		this.imageSDChecksumStr = BWJsonHelpers.PropertyIfExists(this.imageSDChecksumStr, "imageSD_checksum", json);
		this.imageHDChecksumStr = BWJsonHelpers.PropertyIfExists(this.imageHDChecksumStr, "imageHD_checksum", json);
		this.iconSDChecksumStr = BWJsonHelpers.PropertyIfExists(this.iconSDChecksumStr, "iconSD_checksum", json);
		this.iconHDChecksumStr = BWJsonHelpers.PropertyIfExists(this.iconHDChecksumStr, "iconHD_checksum", json);
		this.imageUrl = BWJsonHelpers.PropertyIfExists(this.imageUrl, "image_url", json);
		this.iconUrl = BWJsonHelpers.PropertyIfExists(this.iconUrl, "icon_url", json);
		this.iconUrl = BWJsonHelpers.PropertyIfExists(this.iconUrl, "icon_urls_for_sizes", "128x128", json);
		this.imageUrl = BWJsonHelpers.PropertyIfExists(this.imageUrl, "image_urls_for_sizes", "768x768", json);
		this.createdAt = BWJsonHelpers.PropertyIfExists(this.createdAt, "created_at", json);
		this.updatedAt = BWJsonHelpers.PropertyIfExists(this.updatedAt, "updated_at", json);
		this.publicationStatus = (BWUserModel.PublicationStatusEnum)BWJsonHelpers.PropertyIfExists((int)this.publicationStatus, "moderation_status", json);
		this.publicationStatus = (BWUserModel.PublicationStatusEnum)BWJsonHelpers.PropertyIfExists((int)this.publicationStatus, "publication_status", json);
		this.localChangedMetadata = BWJsonHelpers.PropertyIfExists(this.localChangedMetadata, "local_changed_metadata", json);
	}

	// Token: 0x06002AFD RID: 11005 RVA: 0x00136B50 File Offset: 0x00134F50
	public void LoadSourceFromDataManager()
	{
		this.sourceJsonStr = BWUserModelsDataManager.Instance.LoadSourceForModel("user_model", this.localID);
	}

	// Token: 0x06002AFE RID: 11006 RVA: 0x00136B6D File Offset: 0x00134F6D
	public void GenerateImageChecksums(byte[] iconBytesSD, byte[] iconBytesHD, byte[] imageBytesSD, byte[] imageBytesHD)
	{
		this.imageSDChecksumStr = BWEncript.GetChecksumStr(imageBytesSD);
		this.imageHDChecksumStr = BWEncript.GetChecksumStr(imageBytesHD);
		this.iconSDChecksumStr = BWEncript.GetChecksumStr(iconBytesSD);
		this.iconHDChecksumStr = BWEncript.GetChecksumStr(iconBytesHD);
	}

	// Token: 0x06002AFF RID: 11007 RVA: 0x00136BA0 File Offset: 0x00134FA0
	private void Reset()
	{
		this.modelTitle = string.Empty;
		this.modelShortTitle = string.Empty;
		this.modelDescription = string.Empty;
		this.previewTerrain = "land";
		this.categoryId = 1;
		this.coinsPriceMarkup = 5;
	}

	// Token: 0x06002B00 RID: 11008 RVA: 0x00136BDC File Offset: 0x00134FDC
	public void DidPublish()
	{
		this.localChangedMetadata = false;
	}

	// Token: 0x06002B01 RID: 11009 RVA: 0x00136BE8 File Offset: 0x00134FE8
	public bool OverwriteMetadata(Dictionary<string, string> data)
	{
		bool flag = false;
		if (data.ContainsKey("title"))
		{
			bool flag2 = this.modelTitle != data["title"];
			if (flag2)
			{
				this.modelTitle = data["title"];
				flag = true;
			}
		}
		if (data.ContainsKey("short_title"))
		{
			bool flag3 = this.modelShortTitle != data["short_title"];
			if (flag3)
			{
				this.modelShortTitle = data["short_title"];
				flag = true;
			}
		}
		if (data.ContainsKey("preview_terrain"))
		{
			bool flag4 = this.previewTerrain != data["preview_terrain"];
			if (flag4)
			{
				this.previewTerrain = data["preview_terrain"];
				flag = true;
			}
		}
		if (data.ContainsKey("category"))
		{
			string b = data["category"];
			foreach (BWCategory bwcategory in BWCategory.modelCategories)
			{
				if (bwcategory.name == b)
				{
					bool flag5 = this.categoryId != bwcategory.categoryID;
					if (flag5)
					{
						this.categoryId = bwcategory.categoryID;
						flag = true;
					}
					break;
				}
			}
		}
		if (data.ContainsKey("description") && !string.IsNullOrEmpty(data["description"]))
		{
			bool flag6 = this.modelDescription != data["description"];
			if (flag6)
			{
				this.modelDescription = data["description"];
				flag = true;
			}
		}
		this.localChangedMetadata = (this.localChangedMetadata || flag);
		return flag;
	}

	// Token: 0x06002B02 RID: 11010 RVA: 0x00136DC8 File Offset: 0x001351C8
	public void ChangePriceMarkup(int change)
	{
		this.coinsPriceMarkup += change;
		this.coinsPriceMarkup = Mathf.Max(5, this.coinsPriceMarkup);
		this.localChangedMetadata = true;
	}

	// Token: 0x06002B03 RID: 11011 RVA: 0x00136DF1 File Offset: 0x001351F1
	public void ResetPriceMarkup()
	{
		this.coinsPriceMarkup = this.CoinsBasePrice() * 10 / 100;
		this.coinsPriceMarkup = Mathf.Max(5, this.coinsPriceMarkup);
		this.localChangedMetadata = true;
	}

	// Token: 0x06002B04 RID: 11012 RVA: 0x00136E1E File Offset: 0x0013521E
	public void SetSourceLocked(bool locked)
	{
		this.sourceLocked = locked;
		this.localChangedMetadata = true;
	}

	// Token: 0x06002B05 RID: 11013 RVA: 0x00136E2E File Offset: 0x0013522E
	public void SetRemoteImageUrl(string url)
	{
		this.imageUrl = url;
	}

	// Token: 0x06002B06 RID: 11014 RVA: 0x00136E37 File Offset: 0x00135237
	public void SetRemoteIconUrl(string url)
	{
		this.iconUrl = url;
	}

	// Token: 0x06002B07 RID: 11015 RVA: 0x00136E40 File Offset: 0x00135240
	public Dictionary<string, object> MetadataAttributes()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (this.modelID != null)
		{
			dictionary.Add("id", this.modelID);
		}
		if (this.u2uModelId != null)
		{
			dictionary.Add("u2u_model_id", this.u2uModelId);
		}
		dictionary.Add("local_id", this.localID);
		dictionary.Add("title", this.modelTitle);
		dictionary.Add("short_title", this.modelShortTitle);
		dictionary.Add("description", this.modelDescription);
		dictionary.Add("author_id", this.authorId);
		dictionary.Add("author_username", this.authorUsername);
		dictionary.Add("author_profile_image_url", this.authorProfileImageUrl);
		dictionary.Add("preview_terrain", this.previewTerrain);
		dictionary.Add("model_category_id", this.categoryId);
		dictionary.Add("blocks_inventory_str", this.blocksInventoryStr);
		dictionary.Add("source_equality_checksum", this.sourceEqualityChecksum);
		dictionary.Add("coins_price_markup", this.coinsPriceMarkup);
		dictionary.Add("source_locked", this.sourceLocked);
		dictionary.Add("imageSD_checksum", this.imageSDChecksumStr);
		dictionary.Add("imageHD_checksum", this.imageHDChecksumStr);
		dictionary.Add("iconSD_checksum", this.iconSDChecksumStr);
		dictionary.Add("iconHD_checksum", this.iconHDChecksumStr);
		dictionary.Add("icon_url", this.iconUrl);
		dictionary.Add("image_url", this.imageUrl);
		dictionary.Add("publication_status", (int)this.publicationStatus);
		dictionary.Add("local_changed_metadata", this.localChangedMetadata);
		dictionary.Add("created_at", this.createdAt.ToString("s", CultureInfo.InvariantCulture));
		dictionary.Add("updated_at", this.updatedAt.ToString("s", CultureInfo.InvariantCulture));
		return dictionary;
	}

	// Token: 0x06002B08 RID: 11016 RVA: 0x00137058 File Offset: 0x00135458
	public Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string path = Path.Combine(BWFilesystem.CurrentUserModelsFolder, this.localID);
		string value = BWFilesystem.FileProtocolPrefixStr + Path.Combine(path, "iconHD.png");
		string value2 = BWFilesystem.FileProtocolPrefixStr + Path.Combine(path, "image.png");
		dictionary.Add("author_username", this.authorUsername);
		dictionary.Add("author_id", this.authorId.ToString());
		dictionary.Add("author_profile_image_url", this.authorProfileImageUrl);
		dictionary.Add("title", this.modelTitle);
		dictionary.Add("short_title", this.modelShortTitle);
		dictionary.Add("description", this.modelDescription);
		dictionary.Add("icon_url", value);
		dictionary.Add("image_url", value2);
		dictionary.Add("is_published", this.isPublished.ToString());
		dictionary.Add("coins_price_markup", this.coinsPriceMarkup.ToString());
		dictionary.Add("coins_price", (this.CoinsBasePrice() + this.coinsPriceMarkup).ToString());
		dictionary.Add("preview_terrain", this.previewTerrain);
		dictionary.Add("source_locked", this.sourceLocked.ToString());
		dictionary.Add("model_id", this.u2uModelId);
		dictionary.Add("show_publish", (!this.isPublished && this.isSellable).ToString());
		dictionary.Add("show_unpublish", this.isPublished.ToString());
		dictionary.Add("has_unpublished_changes", this.localChangedMetadata.ToString());
		dictionary.Add("is_sellable", this.isSellable.ToString());
		string modelCategoryName = BWCategory.GetModelCategoryName(this.categoryId);
		dictionary.Add("category", modelCategoryName);
		return dictionary;
	}

	// Token: 0x06002B09 RID: 11017 RVA: 0x00137288 File Offset: 0x00135688
	public Dictionary<string, string> AttrsToCreateRemote()
	{
		return new Dictionary<string, string>
		{
			{
				"title",
				this.modelTitle
			},
			{
				"short_title",
				this.modelShortTitle
			},
			{
				"description",
				this.modelDescription
			},
			{
				"preview_terrain",
				this.previewTerrain
			},
			{
				"model_category_id",
				this.categoryId.ToString()
			},
			{
				"blocks_inventory_str",
				this.blocksInventoryStr
			},
			{
				"source_equality_checksum",
				this.sourceEqualityChecksum
			},
			{
				"source_json_str",
				this.sourceJsonStr
			},
			{
				"coins_price_markup",
				this.coinsPriceMarkup.ToString()
			},
			{
				"source_locked",
				this.sourceLocked.ToString()
			}
		};
	}

	// Token: 0x06002B0A RID: 11018 RVA: 0x00137370 File Offset: 0x00135770
	public Dictionary<string, string> AttrsToUpdateRemote()
	{
		return new Dictionary<string, string>
		{
			{
				"title",
				this.modelTitle
			},
			{
				"short_title",
				this.modelShortTitle
			},
			{
				"description",
				this.modelDescription
			},
			{
				"preview_terrain",
				this.previewTerrain
			},
			{
				"model_category_id",
				this.categoryId.ToString()
			},
			{
				"blocks_inventory_str",
				this.blocksInventoryStr
			},
			{
				"source_equality_checksum",
				this.sourceEqualityChecksum
			},
			{
				"coins_price_markup",
				this.coinsPriceMarkup.ToString()
			},
			{
				"source_locked",
				this.sourceLocked.ToString()
			}
		};
	}

	// Token: 0x040024A7 RID: 9383
	public BWUserModel.PublicationStatusEnum publicationStatus = BWUserModel.PublicationStatusEnum.MODERATION_PENDING;

	// Token: 0x040024A9 RID: 9385
	private const int MODEL_CATEGORY_UNCATEGORIZED = 1;

	// Token: 0x040024AA RID: 9386
	private const int MINIMUM_COINS_PRICE_MARKUP = 5;

	// Token: 0x040024AB RID: 9387
	private const int INITIAL_MARKUP_PERCENTAGE = 10;

	// Token: 0x040024AC RID: 9388
	private int coinsBasePrice;

	// Token: 0x040024AD RID: 9389
	private bool basePriceSet;

	// Token: 0x020003D0 RID: 976
	public enum PublicationStatusEnum
	{
		// Token: 0x040024AF RID: 9391
		NOT_PUBLISHED,
		// Token: 0x040024B0 RID: 9392
		MODERATION_PENDING,
		// Token: 0x040024B1 RID: 9393
		MODERATION_APPROVED,
		// Token: 0x040024B2 RID: 9394
		MODERATION_REJECTED,
		// Token: 0x040024B3 RID: 9395
		DELETED_BY_AUTHOR
	}
}
