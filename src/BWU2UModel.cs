using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003C0 RID: 960
public class BWU2UModel
{
	// Token: 0x060029DB RID: 10715 RVA: 0x0013344C File Offset: 0x0013184C
	public BWU2UModel(JObject json)
	{
		this.UpdateFromJson(json);
	}

	// Token: 0x170001CA RID: 458
	// (get) Token: 0x060029DC RID: 10716 RVA: 0x0013345B File Offset: 0x0013185B
	// (set) Token: 0x060029DD RID: 10717 RVA: 0x00133463 File Offset: 0x00131863
	public string modelID { get; private set; }

	// Token: 0x170001CB RID: 459
	// (get) Token: 0x060029DE RID: 10718 RVA: 0x0013346C File Offset: 0x0013186C
	// (set) Token: 0x060029DF RID: 10719 RVA: 0x00133474 File Offset: 0x00131874
	public string modelTitle { get; private set; }

	// Token: 0x170001CC RID: 460
	// (get) Token: 0x060029E0 RID: 10720 RVA: 0x0013347D File Offset: 0x0013187D
	// (set) Token: 0x060029E1 RID: 10721 RVA: 0x00133485 File Offset: 0x00131885
	public string modelShortTitle { get; private set; }

	// Token: 0x170001CD RID: 461
	// (get) Token: 0x060029E2 RID: 10722 RVA: 0x0013348E File Offset: 0x0013188E
	// (set) Token: 0x060029E3 RID: 10723 RVA: 0x00133496 File Offset: 0x00131896
	public string modelDescription { get; private set; }

	// Token: 0x170001CE RID: 462
	// (get) Token: 0x060029E4 RID: 10724 RVA: 0x0013349F File Offset: 0x0013189F
	// (set) Token: 0x060029E5 RID: 10725 RVA: 0x001334A7 File Offset: 0x001318A7
	public string previewTerrain { get; private set; }

	// Token: 0x170001CF RID: 463
	// (get) Token: 0x060029E6 RID: 10726 RVA: 0x001334B0 File Offset: 0x001318B0
	// (set) Token: 0x060029E7 RID: 10727 RVA: 0x001334B8 File Offset: 0x001318B8
	public int categoryId { get; private set; }

	// Token: 0x170001D0 RID: 464
	// (get) Token: 0x060029E8 RID: 10728 RVA: 0x001334C1 File Offset: 0x001318C1
	// (set) Token: 0x060029E9 RID: 10729 RVA: 0x001334C9 File Offset: 0x001318C9
	public int authorId { get; private set; }

	// Token: 0x170001D1 RID: 465
	// (get) Token: 0x060029EA RID: 10730 RVA: 0x001334D2 File Offset: 0x001318D2
	// (set) Token: 0x060029EB RID: 10731 RVA: 0x001334DA File Offset: 0x001318DA
	public string authorUsername { get; private set; }

	// Token: 0x170001D2 RID: 466
	// (get) Token: 0x060029EC RID: 10732 RVA: 0x001334E3 File Offset: 0x001318E3
	// (set) Token: 0x060029ED RID: 10733 RVA: 0x001334EB File Offset: 0x001318EB
	public string authorProfileImageUrl { get; private set; }

	// Token: 0x170001D3 RID: 467
	// (get) Token: 0x060029EE RID: 10734 RVA: 0x001334F4 File Offset: 0x001318F4
	// (set) Token: 0x060029EF RID: 10735 RVA: 0x001334FC File Offset: 0x001318FC
	public bool authorIsBlocksworldPremium { get; private set; }

	// Token: 0x170001D4 RID: 468
	// (get) Token: 0x060029F0 RID: 10736 RVA: 0x00133505 File Offset: 0x00131905
	// (set) Token: 0x060029F1 RID: 10737 RVA: 0x0013350D File Offset: 0x0013190D
	public int coinsPrice { get; private set; }

	// Token: 0x170001D5 RID: 469
	// (get) Token: 0x060029F2 RID: 10738 RVA: 0x00133516 File Offset: 0x00131916
	// (set) Token: 0x060029F3 RID: 10739 RVA: 0x0013351E File Offset: 0x0013191E
	public int coinsPriceMarkup { get; private set; }

	// Token: 0x170001D6 RID: 470
	// (get) Token: 0x060029F4 RID: 10740 RVA: 0x00133527 File Offset: 0x00131927
	// (set) Token: 0x060029F5 RID: 10741 RVA: 0x0013352F File Offset: 0x0013192F
	public int coinsPriceBlocksInventory { get; private set; }

	// Token: 0x170001D7 RID: 471
	// (get) Token: 0x060029F6 RID: 10742 RVA: 0x00133538 File Offset: 0x00131938
	// (set) Token: 0x060029F7 RID: 10743 RVA: 0x00133540 File Offset: 0x00131940
	public int coinsPriceBlueprint { get; private set; }

	// Token: 0x170001D8 RID: 472
	// (get) Token: 0x060029F8 RID: 10744 RVA: 0x00133549 File Offset: 0x00131949
	// (set) Token: 0x060029F9 RID: 10745 RVA: 0x00133551 File Offset: 0x00131951
	public bool createdOnIOS { get; private set; }

	// Token: 0x170001D9 RID: 473
	// (get) Token: 0x060029FA RID: 10746 RVA: 0x0013355A File Offset: 0x0013195A
	// (set) Token: 0x060029FB RID: 10747 RVA: 0x00133562 File Offset: 0x00131962
	public int blocksworldPremiumCoinsPrice { get; private set; }

	// Token: 0x170001DA RID: 474
	// (get) Token: 0x060029FC RID: 10748 RVA: 0x0013356B File Offset: 0x0013196B
	// (set) Token: 0x060029FD RID: 10749 RVA: 0x00133573 File Offset: 0x00131973
	public int blocksworldPremiumCoinsPriceBlueprint { get; private set; }

	// Token: 0x170001DB RID: 475
	// (get) Token: 0x060029FE RID: 10750 RVA: 0x0013357C File Offset: 0x0013197C
	// (set) Token: 0x060029FF RID: 10751 RVA: 0x00133584 File Offset: 0x00131984
	public bool sourceLocked { get; private set; }

	// Token: 0x170001DC RID: 476
	// (get) Token: 0x06002A00 RID: 10752 RVA: 0x0013358D File Offset: 0x0013198D
	// (set) Token: 0x06002A01 RID: 10753 RVA: 0x00133595 File Offset: 0x00131995
	public string sourceJsonStr { get; private set; }

	// Token: 0x170001DD RID: 477
	// (get) Token: 0x06002A02 RID: 10754 RVA: 0x0013359E File Offset: 0x0013199E
	// (set) Token: 0x06002A03 RID: 10755 RVA: 0x001335A6 File Offset: 0x001319A6
	public string sourceEqualityChecksum { get; private set; }

	// Token: 0x170001DE RID: 478
	// (get) Token: 0x06002A04 RID: 10756 RVA: 0x001335AF File Offset: 0x001319AF
	// (set) Token: 0x06002A05 RID: 10757 RVA: 0x001335B7 File Offset: 0x001319B7
	public string blocksInventoryStr { get; private set; }

	// Token: 0x170001DF RID: 479
	// (get) Token: 0x06002A06 RID: 10758 RVA: 0x001335C0 File Offset: 0x001319C0
	// (set) Token: 0x06002A07 RID: 10759 RVA: 0x001335C8 File Offset: 0x001319C8
	public string iconUrl { get; private set; }

	// Token: 0x170001E0 RID: 480
	// (get) Token: 0x06002A08 RID: 10760 RVA: 0x001335D1 File Offset: 0x001319D1
	// (set) Token: 0x06002A09 RID: 10761 RVA: 0x001335D9 File Offset: 0x001319D9
	public string imageUrl { get; private set; }

	// Token: 0x170001E1 RID: 481
	// (get) Token: 0x06002A0A RID: 10762 RVA: 0x001335E2 File Offset: 0x001319E2
	// (set) Token: 0x06002A0B RID: 10763 RVA: 0x001335EA File Offset: 0x001319EA
	public DateTime createdAt { get; private set; }

	// Token: 0x170001E2 RID: 482
	// (get) Token: 0x06002A0C RID: 10764 RVA: 0x001335F3 File Offset: 0x001319F3
	// (set) Token: 0x06002A0D RID: 10765 RVA: 0x001335FB File Offset: 0x001319FB
	public DateTime updatedAt { get; private set; }

	// Token: 0x170001E3 RID: 483
	// (get) Token: 0x06002A0E RID: 10766 RVA: 0x00133604 File Offset: 0x00131A04
	// (set) Token: 0x06002A0F RID: 10767 RVA: 0x0013360C File Offset: 0x00131A0C
	public bool isPurchased { get; private set; }

	// Token: 0x06002A10 RID: 10768 RVA: 0x00133618 File Offset: 0x00131A18
	public void UpdateFromJson(JObject json)
	{
		this.modelID = BWJsonHelpers.IDPropertyAsStringIfExists(this.modelID, "id", json);
		this.modelTitle = BWJsonHelpers.PropertyIfExists(this.modelTitle, "title", json);
		this.modelShortTitle = BWJsonHelpers.PropertyIfExists(this.modelShortTitle, "short_title", json);
		this.modelDescription = BWJsonHelpers.PropertyIfExists(this.modelDescription, "description", json);
		this.authorId = BWJsonHelpers.PropertyIfExists(this.authorId, "author_id", json);
		this.authorUsername = BWJsonHelpers.PropertyIfExists(this.authorUsername, "author_username", json);
		this.authorProfileImageUrl = BWJsonHelpers.PropertyIfExists(this.authorProfileImageUrl, "author_profile_image_url", json);
		int num = -1;
		num = BWJsonHelpers.PropertyIfExists(num, "author_status", json);
		this.authorIsBlocksworldPremium = (Util.IsPremiumUserStatus(num) || Util.IsBlocksworldOfficialUser(this.authorId));
		this.createdOnIOS = (Util.IsIOSExclusiveUserStatus(num) && !Util.IsBlocksworldOfficialUser(this.authorId));
		this.previewTerrain = BWJsonHelpers.PropertyIfExists(this.previewTerrain, "preview_terrain", json);
		this.categoryId = BWJsonHelpers.PropertyIfExists(this.categoryId, "model_category_id", json);
		this.sourceJsonStr = BWJsonHelpers.PropertyIfExists(this.sourceJsonStr, "source_json_str", json);
		this.blocksInventoryStr = BWJsonHelpers.PropertyIfExists(this.blocksInventoryStr, "blocks_inventory_str", json);
		this.sourceEqualityChecksum = BWJsonHelpers.PropertyIfExists(this.sourceEqualityChecksum, "source_equality_checksum", json);
		this.sourceLocked = BWJsonHelpers.PropertyIfExists(this.sourceLocked, "source_locked", json);
		this.coinsPrice = BWJsonHelpers.PropertyIfExists(this.coinsPrice, "coins_price", json);
		this.coinsPriceMarkup = BWJsonHelpers.PropertyIfExists(this.coinsPriceMarkup, "coins_price_markup", json);
		this.coinsPriceBlueprint = BWJsonHelpers.PropertyIfExists(this.coinsPriceBlueprint, "coins_price_blueprint", json);
		this.coinsPriceBlocksInventory = BWJsonHelpers.PropertyIfExists(this.coinsPriceBlocksInventory, "coins_price_blocks_inventory", json);
		this.blocksworldPremiumCoinsPrice = BWJsonHelpers.PropertyIfExists(this.blocksworldPremiumCoinsPrice, "blocksworld_premium_coins_price", json);
		this.blocksworldPremiumCoinsPriceBlueprint = BWJsonHelpers.PropertyIfExists(this.blocksworldPremiumCoinsPriceBlueprint, "blocksworld_premium_coins_price_blueprint", json);
		this.iconUrl = BWJsonHelpers.PropertyIfExists(this.iconUrl, "icon_urls_for_sizes", "128x128", json);
		this.imageUrl = BWJsonHelpers.PropertyIfExists(this.imageUrl, "image_urls_for_sizes", "768x768", json);
		this.createdAt = BWJsonHelpers.PropertyIfExists(this.createdAt, "created_at", json);
		this.updatedAt = BWJsonHelpers.PropertyIfExists(this.updatedAt, "updated_at", json);
	}

	// Token: 0x06002A11 RID: 10769 RVA: 0x00133890 File Offset: 0x00131C90
	public Dictionary<string, string> AttributesForMenuUI()
	{
		return new Dictionary<string, string>
		{
			{
				"author_username",
				this.authorUsername
			},
			{
				"author_id",
				this.authorId.ToString()
			},
			{
				"author_profile_image_url",
				this.authorProfileImageUrl
			},
			{
				"category",
				BWCategory.GetModelCategoryName(this.categoryId)
			},
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
				"icon_url",
				this.iconUrl
			},
			{
				"image_url",
				this.imageUrl
			},
			{
				"coins_price",
				this.coinsPrice.ToString()
			},
			{
				"coins_price_blueprint",
				this.coinsPriceBlueprint.ToString()
			},
			{
				"premium_coins_price",
				this.blocksworldPremiumCoinsPrice.ToString()
			},
			{
				"premium_coins_price_blueprint",
				this.blocksworldPremiumCoinsPriceBlueprint.ToString()
			},
			{
				"created_by_current_user",
				(BWUser.currentUser.userID == this.authorId).ToString()
			},
			{
				"model_id",
				(this.modelID != null) ? this.modelID : string.Empty
			},
			{
				"source_locked",
				this.sourceLocked.ToString()
			}
		};
	}
}
