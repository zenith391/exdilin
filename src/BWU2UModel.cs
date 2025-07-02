using System;
using System.Collections.Generic;
using SimpleJSON;

public class BWU2UModel
{
	public string modelID { get; private set; }

	public string modelTitle { get; private set; }

	public string modelShortTitle { get; private set; }

	public string modelDescription { get; private set; }

	public string previewTerrain { get; private set; }

	public int categoryId { get; private set; }

	public int authorId { get; private set; }

	public string authorUsername { get; private set; }

	public string authorProfileImageUrl { get; private set; }

	public bool authorIsBlocksworldPremium { get; private set; }

	public int coinsPrice { get; private set; }

	public int coinsPriceMarkup { get; private set; }

	public int coinsPriceBlocksInventory { get; private set; }

	public int coinsPriceBlueprint { get; private set; }

	public bool createdOnIOS { get; private set; }

	public int blocksworldPremiumCoinsPrice { get; private set; }

	public int blocksworldPremiumCoinsPriceBlueprint { get; private set; }

	public bool sourceLocked { get; private set; }

	public string sourceJsonStr { get; private set; }

	public string sourceEqualityChecksum { get; private set; }

	public string blocksInventoryStr { get; private set; }

	public string iconUrl { get; private set; }

	public string imageUrl { get; private set; }

	public DateTime createdAt { get; private set; }

	public DateTime updatedAt { get; private set; }

	public bool isPurchased { get; private set; }

	public BWU2UModel(JObject json)
	{
		UpdateFromJson(json);
	}

	public void UpdateFromJson(JObject json)
	{
		modelID = BWJsonHelpers.IDPropertyAsStringIfExists(modelID, "id", json);
		modelTitle = BWJsonHelpers.PropertyIfExists(modelTitle, "title", json);
		modelShortTitle = BWJsonHelpers.PropertyIfExists(modelShortTitle, "short_title", json);
		modelDescription = BWJsonHelpers.PropertyIfExists(modelDescription, "description", json);
		authorId = BWJsonHelpers.PropertyIfExists(authorId, "author_id", json);
		authorUsername = BWJsonHelpers.PropertyIfExists(authorUsername, "author_username", json);
		authorProfileImageUrl = BWJsonHelpers.PropertyIfExists(authorProfileImageUrl, "author_profile_image_url", json);
		int property = -1;
		property = BWJsonHelpers.PropertyIfExists(property, "author_status", json);
		authorIsBlocksworldPremium = Util.IsPremiumUserStatus(property) || Util.IsBlocksworldOfficialUser(authorId);
		createdOnIOS = Util.IsIOSExclusiveUserStatus(property) && !Util.IsBlocksworldOfficialUser(authorId);
		previewTerrain = BWJsonHelpers.PropertyIfExists(previewTerrain, "preview_terrain", json);
		categoryId = BWJsonHelpers.PropertyIfExists(categoryId, "model_category_id", json);
		sourceJsonStr = BWJsonHelpers.PropertyIfExists(sourceJsonStr, "source_json_str", json);
		blocksInventoryStr = BWJsonHelpers.PropertyIfExists(blocksInventoryStr, "blocks_inventory_str", json);
		sourceEqualityChecksum = BWJsonHelpers.PropertyIfExists(sourceEqualityChecksum, "source_equality_checksum", json);
		sourceLocked = BWJsonHelpers.PropertyIfExists(sourceLocked, "source_locked", json);
		coinsPrice = BWJsonHelpers.PropertyIfExists(coinsPrice, "coins_price", json);
		coinsPriceMarkup = BWJsonHelpers.PropertyIfExists(coinsPriceMarkup, "coins_price_markup", json);
		coinsPriceBlueprint = BWJsonHelpers.PropertyIfExists(coinsPriceBlueprint, "coins_price_blueprint", json);
		coinsPriceBlocksInventory = BWJsonHelpers.PropertyIfExists(coinsPriceBlocksInventory, "coins_price_blocks_inventory", json);
		blocksworldPremiumCoinsPrice = BWJsonHelpers.PropertyIfExists(blocksworldPremiumCoinsPrice, "blocksworld_premium_coins_price", json);
		blocksworldPremiumCoinsPriceBlueprint = BWJsonHelpers.PropertyIfExists(blocksworldPremiumCoinsPriceBlueprint, "blocksworld_premium_coins_price_blueprint", json);
		iconUrl = BWJsonHelpers.PropertyIfExists(iconUrl, "icon_urls_for_sizes", "128x128", json);
		imageUrl = BWJsonHelpers.PropertyIfExists(imageUrl, "image_urls_for_sizes", "768x768", json);
		createdAt = BWJsonHelpers.PropertyIfExists(createdAt, "created_at", json);
		updatedAt = BWJsonHelpers.PropertyIfExists(updatedAt, "updated_at", json);
	}

	public Dictionary<string, string> AttributesForMenuUI()
	{
		return new Dictionary<string, string>
		{
			{ "author_username", authorUsername },
			{
				"author_id",
				authorId.ToString()
			},
			{ "author_profile_image_url", authorProfileImageUrl },
			{
				"category",
				BWCategory.GetModelCategoryName(categoryId)
			},
			{ "title", modelTitle },
			{ "short_title", modelShortTitle },
			{ "description", modelDescription },
			{ "icon_url", iconUrl },
			{ "image_url", imageUrl },
			{
				"coins_price",
				coinsPrice.ToString()
			},
			{
				"coins_price_blueprint",
				coinsPriceBlueprint.ToString()
			},
			{
				"premium_coins_price",
				blocksworldPremiumCoinsPrice.ToString()
			},
			{
				"premium_coins_price_blueprint",
				blocksworldPremiumCoinsPriceBlueprint.ToString()
			},
			{
				"created_by_current_user",
				(BWUser.currentUser.userID == authorId).ToString()
			},
			{
				"model_id",
				(modelID != null) ? modelID : string.Empty
			},
			{
				"source_locked",
				sourceLocked.ToString()
			}
		};
	}
}
