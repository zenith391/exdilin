using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SimpleJSON;
using UnityEngine;

[Serializable]
public class BWUserModel
{
	public enum PublicationStatusEnum
	{
		NOT_PUBLISHED,
		MODERATION_PENDING,
		MODERATION_APPROVED,
		MODERATION_REJECTED,
		DELETED_BY_AUTHOR
	}

	public PublicationStatusEnum publicationStatus = PublicationStatusEnum.MODERATION_PENDING;

	private const int MODEL_CATEGORY_UNCATEGORIZED = 1;

	private const int MINIMUM_COINS_PRICE_MARKUP = 5;

	private const int INITIAL_MARKUP_PERCENTAGE = 10;

	private int coinsBasePrice;

	private bool basePriceSet;

	public string modelID { get; protected set; }

	public string localID { get; protected set; }

	public string modelTitle { get; protected set; }

	public string modelShortTitle { get; protected set; }

	public string modelDescription { get; protected set; }

	public string previewTerrain { get; protected set; }

	public string u2uModelId { get; protected set; }

	public int categoryId { get; protected set; }

	public int authorId { get; protected set; }

	public string authorUsername { get; protected set; }

	public string authorProfileImageUrl { get; protected set; }

	public int coinsPriceMarkup { get; private set; }

	public int coinsPriceBlocksInventory { get; private set; }

	public int coinsPriceBlueprint { get; private set; }

	public bool sourceLocked { get; protected set; }

	public string sourceJsonStr { get; protected set; }

	public string sourceEqualityChecksum { get; protected set; }

	public string blocksInventoryStr { get; protected set; }

	public string imageSDChecksumStr { get; protected set; }

	public string imageHDChecksumStr { get; protected set; }

	public string iconSDChecksumStr { get; protected set; }

	public string iconHDChecksumStr { get; protected set; }

	public string iconUrl { get; protected set; }

	public string imageUrl { get; protected set; }

	public DateTime createdAt { get; protected set; }

	public DateTime updatedAt { get; protected set; }

	public bool localChangedMetadata { get; protected set; }

	public string obfuscatedSourceJsonStr => Util.ObfuscateSourceForUser(sourceJsonStr, BWUser.currentUser.userID);

	internal bool isRemote => !string.IsNullOrEmpty(modelID);

	public bool isPublished
	{
		get
		{
			if (!string.IsNullOrEmpty(u2uModelId) && publicationStatus != PublicationStatusEnum.NOT_PUBLISHED)
			{
				return publicationStatus != PublicationStatusEnum.DELETED_BY_AUTHOR;
			}
			return false;
		}
	}

	public bool isSellable => CoinsBasePrice() < int.MaxValue;

	public BWUserModel(JObject json)
	{
		UpdateFromJson(json);
	}

	public BWUserModel(BWUserModel copyFrom)
	{
		Reset();
		localID = Guid.NewGuid().ToString();
		modelID = copyFrom.modelID;
		modelTitle = copyFrom.modelTitle;
		modelShortTitle = copyFrom.modelShortTitle;
		modelDescription = copyFrom.modelDescription;
		authorId = copyFrom.authorId;
		authorUsername = copyFrom.authorUsername;
		authorProfileImageUrl = copyFrom.authorProfileImageUrl;
		previewTerrain = copyFrom.previewTerrain;
		categoryId = copyFrom.categoryId;
		u2uModelId = copyFrom.u2uModelId;
		sourceJsonStr = copyFrom.sourceJsonStr;
		blocksInventoryStr = copyFrom.blocksInventoryStr;
		sourceEqualityChecksum = copyFrom.sourceEqualityChecksum;
		coinsPriceMarkup = copyFrom.coinsPriceMarkup;
		sourceLocked = copyFrom.sourceLocked;
		iconUrl = copyFrom.iconUrl;
		imageUrl = copyFrom.imageUrl;
		createdAt = copyFrom.createdAt;
		updatedAt = copyFrom.updatedAt;
		publicationStatus = copyFrom.publicationStatus;
	}

	public BWUserModel(BWUserModel copyFrom, string withLocalID)
	{
		Reset();
		localID = withLocalID;
		modelID = copyFrom.modelID;
		modelTitle = copyFrom.modelTitle;
		modelShortTitle = copyFrom.modelShortTitle;
		modelDescription = copyFrom.modelDescription;
		authorId = copyFrom.authorId;
		authorUsername = copyFrom.authorUsername;
		authorProfileImageUrl = copyFrom.authorProfileImageUrl;
		previewTerrain = copyFrom.previewTerrain;
		categoryId = copyFrom.categoryId;
		u2uModelId = copyFrom.u2uModelId;
		sourceJsonStr = copyFrom.sourceJsonStr;
		blocksInventoryStr = copyFrom.blocksInventoryStr;
		sourceEqualityChecksum = copyFrom.sourceEqualityChecksum;
		iconUrl = copyFrom.iconUrl;
		imageUrl = copyFrom.imageUrl;
		coinsPriceMarkup = copyFrom.coinsPriceMarkup;
		sourceLocked = copyFrom.sourceLocked;
		createdAt = copyFrom.createdAt;
		updatedAt = copyFrom.updatedAt;
		publicationStatus = copyFrom.publicationStatus;
	}

	public BWUserModel(string modelTitle, string modelSource, string blocksInventoryStr, string sourceEqualityChecksum)
	{
		Reset();
		localID = Guid.NewGuid().ToString();
		this.modelTitle = Util.FixNonAscii(modelTitle);
		modelShortTitle = Util.FixNonAscii(modelTitle);
		sourceJsonStr = modelSource;
		this.blocksInventoryStr = blocksInventoryStr;
		this.sourceEqualityChecksum = sourceEqualityChecksum;
		authorId = BWUser.currentUser.userID;
		authorUsername = BWUser.currentUser.username;
		authorProfileImageUrl = BWUser.currentUser.profileImageURL;
		publicationStatus = PublicationStatusEnum.MODERATION_PENDING;
		ResetPriceMarkup();
	}

	public int CoinsBasePrice()
	{
		if (basePriceSet)
		{
			return coinsBasePrice;
		}
		BlocksInventory inventory = BlocksInventory.FromString(blocksInventoryStr);
		coinsBasePrice = BWBlockItemPricing.CoinsValueOfBlocksInventory(inventory);
		basePriceSet = true;
		return coinsBasePrice;
	}

	public void UpdateFromJson(JObject json)
	{
		modelID = BWJsonHelpers.IDPropertyAsStringIfExists(modelID, "id", json);
		localID = BWJsonHelpers.PropertyIfExists(localID, "local_id", json);
		modelTitle = BWJsonHelpers.PropertyIfExists(modelTitle, "title", json);
		modelShortTitle = BWJsonHelpers.PropertyIfExists(modelShortTitle, "short_title", json);
		modelDescription = BWJsonHelpers.PropertyIfExists(modelDescription, "description", json);
		authorId = BWJsonHelpers.PropertyIfExists(authorId, "author_id", json);
		authorUsername = BWJsonHelpers.PropertyIfExists(authorUsername, "author_username", json);
		authorProfileImageUrl = BWJsonHelpers.PropertyIfExists(authorProfileImageUrl, "author_profile_image_url", json);
		previewTerrain = BWJsonHelpers.PropertyIfExists(previewTerrain, "preview_terrain", json);
		categoryId = BWJsonHelpers.PropertyIfExists(categoryId, "model_category_id", json);
		u2uModelId = BWJsonHelpers.IDPropertyAsStringIfExists(u2uModelId, "u2u_model_id", json);
		sourceJsonStr = BWJsonHelpers.PropertyIfExists(sourceJsonStr, "source_json_str", json);
		sourceLocked = BWJsonHelpers.PropertyIfExists(sourceLocked, "source_locked", json);
		blocksInventoryStr = BWJsonHelpers.PropertyIfExists(blocksInventoryStr, "blocks_inventory_str", json);
		sourceEqualityChecksum = BWJsonHelpers.PropertyIfExists(sourceEqualityChecksum, "source_equality_checksum", json);
		coinsPriceMarkup = BWJsonHelpers.PropertyIfExists(coinsPriceMarkup, "coins_price_markup", json);
		coinsPriceMarkup = Mathf.Max(coinsPriceMarkup, 5);
		imageSDChecksumStr = BWJsonHelpers.PropertyIfExists(imageSDChecksumStr, "imageSD_checksum", json);
		imageHDChecksumStr = BWJsonHelpers.PropertyIfExists(imageHDChecksumStr, "imageHD_checksum", json);
		iconSDChecksumStr = BWJsonHelpers.PropertyIfExists(iconSDChecksumStr, "iconSD_checksum", json);
		iconHDChecksumStr = BWJsonHelpers.PropertyIfExists(iconHDChecksumStr, "iconHD_checksum", json);
		imageUrl = BWJsonHelpers.PropertyIfExists(imageUrl, "image_url", json);
		iconUrl = BWJsonHelpers.PropertyIfExists(iconUrl, "icon_url", json);
		iconUrl = BWJsonHelpers.PropertyIfExists(iconUrl, "icon_urls_for_sizes", "128x128", json);
		imageUrl = BWJsonHelpers.PropertyIfExists(imageUrl, "image_urls_for_sizes", "768x768", json);
		createdAt = BWJsonHelpers.PropertyIfExists(createdAt, "created_at", json);
		updatedAt = BWJsonHelpers.PropertyIfExists(updatedAt, "updated_at", json);
		publicationStatus = (PublicationStatusEnum)BWJsonHelpers.PropertyIfExists((int)publicationStatus, "moderation_status", json);
		publicationStatus = (PublicationStatusEnum)BWJsonHelpers.PropertyIfExists((int)publicationStatus, "publication_status", json);
		localChangedMetadata = BWJsonHelpers.PropertyIfExists(localChangedMetadata, "local_changed_metadata", json);
	}

	public void LoadSourceFromDataManager()
	{
		sourceJsonStr = BWUserModelsDataManager.Instance.LoadSourceForModel("user_model", localID);
	}

	public void GenerateImageChecksums(byte[] iconBytesSD, byte[] iconBytesHD, byte[] imageBytesSD, byte[] imageBytesHD)
	{
		imageSDChecksumStr = BWEncript.GetChecksumStr(imageBytesSD);
		imageHDChecksumStr = BWEncript.GetChecksumStr(imageBytesHD);
		iconSDChecksumStr = BWEncript.GetChecksumStr(iconBytesSD);
		iconHDChecksumStr = BWEncript.GetChecksumStr(iconBytesHD);
	}

	private void Reset()
	{
		modelTitle = string.Empty;
		modelShortTitle = string.Empty;
		modelDescription = string.Empty;
		previewTerrain = "land";
		categoryId = 1;
		coinsPriceMarkup = 5;
	}

	public void DidPublish()
	{
		localChangedMetadata = false;
	}

	public bool OverwriteMetadata(Dictionary<string, string> data)
	{
		bool flag = false;
		if (data.ContainsKey("title") && modelTitle != data["title"])
		{
			modelTitle = data["title"];
			flag = true;
		}
		if (data.ContainsKey("short_title") && modelShortTitle != data["short_title"])
		{
			modelShortTitle = data["short_title"];
			flag = true;
		}
		if (data.ContainsKey("preview_terrain") && previewTerrain != data["preview_terrain"])
		{
			previewTerrain = data["preview_terrain"];
			flag = true;
		}
		if (data.ContainsKey("category"))
		{
			string text = data["category"];
			foreach (BWCategory modelCategory in BWCategory.modelCategories)
			{
				if (modelCategory.name == text)
				{
					if (categoryId != modelCategory.categoryID)
					{
						categoryId = modelCategory.categoryID;
						flag = true;
					}
					break;
				}
			}
		}
		if (data.ContainsKey("description") && !string.IsNullOrEmpty(data["description"]) && modelDescription != data["description"])
		{
			modelDescription = data["description"];
			flag = true;
		}
		localChangedMetadata |= flag;
		return flag;
	}

	public void ChangePriceMarkup(int change)
	{
		coinsPriceMarkup += change;
		coinsPriceMarkup = Mathf.Max(5, coinsPriceMarkup);
		localChangedMetadata = true;
	}

	public void ResetPriceMarkup()
	{
		coinsPriceMarkup = CoinsBasePrice() * 10 / 100;
		coinsPriceMarkup = Mathf.Max(5, coinsPriceMarkup);
		localChangedMetadata = true;
	}

	public void SetSourceLocked(bool locked)
	{
		sourceLocked = locked;
		localChangedMetadata = true;
	}

	public void SetRemoteImageUrl(string url)
	{
		imageUrl = url;
	}

	public void SetRemoteIconUrl(string url)
	{
		iconUrl = url;
	}

	public Dictionary<string, object> MetadataAttributes()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (modelID != null)
		{
			dictionary.Add("id", modelID);
		}
		if (u2uModelId != null)
		{
			dictionary.Add("u2u_model_id", u2uModelId);
		}
		dictionary.Add("local_id", localID);
		dictionary.Add("title", modelTitle);
		dictionary.Add("short_title", modelShortTitle);
		dictionary.Add("description", modelDescription);
		dictionary.Add("author_id", authorId);
		dictionary.Add("author_username", authorUsername);
		dictionary.Add("author_profile_image_url", authorProfileImageUrl);
		dictionary.Add("preview_terrain", previewTerrain);
		dictionary.Add("model_category_id", categoryId);
		dictionary.Add("blocks_inventory_str", blocksInventoryStr);
		dictionary.Add("source_equality_checksum", sourceEqualityChecksum);
		dictionary.Add("coins_price_markup", coinsPriceMarkup);
		dictionary.Add("source_locked", sourceLocked);
		dictionary.Add("imageSD_checksum", imageSDChecksumStr);
		dictionary.Add("imageHD_checksum", imageHDChecksumStr);
		dictionary.Add("iconSD_checksum", iconSDChecksumStr);
		dictionary.Add("iconHD_checksum", iconHDChecksumStr);
		dictionary.Add("icon_url", iconUrl);
		dictionary.Add("image_url", imageUrl);
		dictionary.Add("publication_status", (int)publicationStatus);
		dictionary.Add("local_changed_metadata", localChangedMetadata);
		dictionary.Add("created_at", createdAt.ToString("s", CultureInfo.InvariantCulture));
		dictionary.Add("updated_at", updatedAt.ToString("s", CultureInfo.InvariantCulture));
		return dictionary;
	}

	public Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string path = Path.Combine(BWFilesystem.CurrentUserModelsFolder, localID);
		string value = BWFilesystem.FileProtocolPrefixStr + Path.Combine(path, "iconHD.png");
		string value2 = BWFilesystem.FileProtocolPrefixStr + Path.Combine(path, "image.png");
		dictionary.Add("author_username", authorUsername);
		dictionary.Add("author_id", authorId.ToString());
		dictionary.Add("author_profile_image_url", authorProfileImageUrl);
		dictionary.Add("title", modelTitle);
		dictionary.Add("short_title", modelShortTitle);
		dictionary.Add("description", modelDescription);
		dictionary.Add("icon_url", value);
		dictionary.Add("image_url", value2);
		dictionary.Add("is_published", isPublished.ToString());
		dictionary.Add("coins_price_markup", coinsPriceMarkup.ToString());
		dictionary.Add("coins_price", (CoinsBasePrice() + coinsPriceMarkup).ToString());
		dictionary.Add("preview_terrain", previewTerrain);
		dictionary.Add("source_locked", sourceLocked.ToString());
		dictionary.Add("model_id", u2uModelId);
		dictionary.Add("show_publish", (!isPublished && isSellable).ToString());
		dictionary.Add("show_unpublish", isPublished.ToString());
		dictionary.Add("has_unpublished_changes", localChangedMetadata.ToString());
		dictionary.Add("is_sellable", isSellable.ToString());
		string modelCategoryName = BWCategory.GetModelCategoryName(categoryId);
		dictionary.Add("category", modelCategoryName);
		return dictionary;
	}

	public Dictionary<string, string> AttrsToCreateRemote()
	{
		return new Dictionary<string, string>
		{
			{ "title", modelTitle },
			{ "short_title", modelShortTitle },
			{ "description", modelDescription },
			{ "preview_terrain", previewTerrain },
			{
				"model_category_id",
				categoryId.ToString()
			},
			{ "blocks_inventory_str", blocksInventoryStr },
			{ "source_equality_checksum", sourceEqualityChecksum },
			{ "source_json_str", sourceJsonStr },
			{
				"coins_price_markup",
				coinsPriceMarkup.ToString()
			},
			{
				"source_locked",
				sourceLocked.ToString()
			}
		};
	}

	public Dictionary<string, string> AttrsToUpdateRemote()
	{
		return new Dictionary<string, string>
		{
			{ "title", modelTitle },
			{ "short_title", modelShortTitle },
			{ "description", modelDescription },
			{ "preview_terrain", previewTerrain },
			{
				"model_category_id",
				categoryId.ToString()
			},
			{ "blocks_inventory_str", blocksInventoryStr },
			{ "source_equality_checksum", sourceEqualityChecksum },
			{
				"coins_price_markup",
				coinsPriceMarkup.ToString()
			},
			{
				"source_locked",
				sourceLocked.ToString()
			}
		};
	}
}
