using System;
using System.Collections.Generic;
using System.Globalization;
using SimpleJSON;

// Token: 0x020003D5 RID: 981
using Exdilin;
public class BWWorld
{
	// Token: 0x06002B71 RID: 11121 RVA: 0x0012B134 File Offset: 0x00129534
	public BWWorld()
	{
		this.Reset();
	}

	// Token: 0x06002B72 RID: 11122 RVA: 0x0012B142 File Offset: 0x00129542
	public BWWorld(JObject json)
	{
		this.LoadFromJson(json);
	}

	// Token: 0x06002B73 RID: 11123 RVA: 0x0012B154 File Offset: 0x00129554
	public BWWorld(BWWorld copyFrom)
	{
		this.appVersion = copyFrom.appVersion;
		this.authorID = copyFrom.authorID;
		this.authorProfileImageURL = copyFrom.authorProfileImageURL;
		this.authorUsername = copyFrom.authorUsername;
		this.authorBlocksworldPremium = copyFrom.authorBlocksworldPremium;
		this.averageStarRating = copyFrom.averageStarRating;
		this.categoryIDs = new List<int>(copyFrom.categoryIDs);
		this.description = copyFrom.description;
		this.hasWinCondition = copyFrom.hasWinCondition;
		this.largeImageURL = copyFrom.largeImageURL;
		this.likesCount = copyFrom.likesCount;
		this.playCount = copyFrom.playCount;
		this.publicationStatus = copyFrom.publicationStatus;
		this.smallImageURL = copyFrom.smallImageURL;
		this.source = copyFrom.source;
		this.title = copyFrom.title;
		this.worldID = copyFrom.worldID;
		this.createdAt = copyFrom.createdAt;
		this.updatedAt = copyFrom.updatedAt;
		this.requiredMods = new List<Dependency>(copyFrom.requiredMods);
	}

	// Token: 0x17000221 RID: 545
	// (get) Token: 0x06002B74 RID: 11124 RVA: 0x0012B250 File Offset: 0x00129650
	// (set) Token: 0x06002B75 RID: 11125 RVA: 0x0012B258 File Offset: 0x00129658
	public string appVersion { get; protected set; }

	// Token: 0x17000222 RID: 546
	// (get) Token: 0x06002B76 RID: 11126 RVA: 0x0012B261 File Offset: 0x00129661
	// (set) Token: 0x06002B77 RID: 11127 RVA: 0x0012B269 File Offset: 0x00129669
	public int authorID { get; protected set; }

	// Token: 0x17000223 RID: 547
	// (get) Token: 0x06002B78 RID: 11128 RVA: 0x0012B272 File Offset: 0x00129672
	// (set) Token: 0x06002B79 RID: 11129 RVA: 0x0012B27A File Offset: 0x0012967A
	public string authorProfileImageURL { get; protected set; }

	// Token: 0x17000224 RID: 548
	// (get) Token: 0x06002B7A RID: 11130 RVA: 0x0012B283 File Offset: 0x00129683
	// (set) Token: 0x06002B7B RID: 11131 RVA: 0x0012B28B File Offset: 0x0012968B
	public string authorUsername { get; protected set; }

	// Token: 0x17000225 RID: 549
	// (get) Token: 0x06002B7C RID: 11132 RVA: 0x0012B294 File Offset: 0x00129694
	// (set) Token: 0x06002B7D RID: 11133 RVA: 0x0012B29C File Offset: 0x0012969C
	public bool authorBlocksworldPremium { get; protected set; }

	// Token: 0x17000226 RID: 550
	// (get) Token: 0x06002B7E RID: 11134 RVA: 0x0012B2A5 File Offset: 0x001296A5
	// (set) Token: 0x06002B7F RID: 11135 RVA: 0x0012B2AD File Offset: 0x001296AD
	public float averageStarRating { get; protected set; }

	// Token: 0x17000227 RID: 551
	// (get) Token: 0x06002B80 RID: 11136 RVA: 0x0012B2B6 File Offset: 0x001296B6
	// (set) Token: 0x06002B81 RID: 11137 RVA: 0x0012B2BE File Offset: 0x001296BE
	public List<int> categoryIDs { get; protected set; }

	// Token: 0x17000228 RID: 552
	// (get) Token: 0x06002B82 RID: 11138 RVA: 0x0012B2C7 File Offset: 0x001296C7
	// (set) Token: 0x06002B83 RID: 11139 RVA: 0x0012B2CF File Offset: 0x001296CF
	public bool createdOnIOS { get; protected set; }

	// Token: 0x17000229 RID: 553
	// (get) Token: 0x06002B84 RID: 11140 RVA: 0x0012B2D8 File Offset: 0x001296D8
	// (set) Token: 0x06002B85 RID: 11141 RVA: 0x0012B2E0 File Offset: 0x001296E0
	public string description { get; protected set; }

	// Token: 0x1700022A RID: 554
	// (get) Token: 0x06002B86 RID: 11142 RVA: 0x0012B2E9 File Offset: 0x001296E9
	// (set) Token: 0x06002B87 RID: 11143 RVA: 0x0012B2F1 File Offset: 0x001296F1
	public bool hasWinCondition { get; protected set; }

	// Token: 0x1700022B RID: 555
	// (get) Token: 0x06002B88 RID: 11144 RVA: 0x0012B2FA File Offset: 0x001296FA
	// (set) Token: 0x06002B89 RID: 11145 RVA: 0x0012B302 File Offset: 0x00129702
	public string largeImageURL { get; protected set; }

	// Token: 0x1700022C RID: 556
	// (get) Token: 0x06002B8A RID: 11146 RVA: 0x0012B30B File Offset: 0x0012970B
	// (set) Token: 0x06002B8B RID: 11147 RVA: 0x0012B313 File Offset: 0x00129713
	public int likesCount { get; protected set; }

	// Token: 0x1700022D RID: 557
	// (get) Token: 0x06002B8C RID: 11148 RVA: 0x0012B31C File Offset: 0x0012971C
	// (set) Token: 0x06002B8D RID: 11149 RVA: 0x0012B324 File Offset: 0x00129724
	public int playCount { get; protected set; }

	// Token: 0x1700022E RID: 558
	// (get) Token: 0x06002B8E RID: 11150 RVA: 0x0012B32D File Offset: 0x0012972D
	// (set) Token: 0x06002B8F RID: 11151 RVA: 0x0012B335 File Offset: 0x00129735
	public BWWorld.PublicationStatus publicationStatus { get; protected set; }

	// Token: 0x1700022F RID: 559
	// (get) Token: 0x06002B90 RID: 11152 RVA: 0x0012B33E File Offset: 0x0012973E
	// (set) Token: 0x06002B91 RID: 11153 RVA: 0x0012B346 File Offset: 0x00129746
	public string smallImageURL { get; protected set; }

	// Token: 0x17000230 RID: 560
	// (get) Token: 0x06002B92 RID: 11154 RVA: 0x0012B34F File Offset: 0x0012974F
	// (set) Token: 0x06002B93 RID: 11155 RVA: 0x0012B357 File Offset: 0x00129757
	public string source { get; set; }

	// Token: 0x17000231 RID: 561
	// (get) Token: 0x06002B94 RID: 11156 RVA: 0x0012B360 File Offset: 0x00129760
	// (set) Token: 0x06002B95 RID: 11157 RVA: 0x0012B368 File Offset: 0x00129768
	public string title { get; protected set; }

	// Token: 0x17000232 RID: 562
	// (get) Token: 0x06002B96 RID: 11158 RVA: 0x0012B371 File Offset: 0x00129771
	// (set) Token: 0x06002B97 RID: 11159 RVA: 0x0012B379 File Offset: 0x00129779
	public string worldID { get; protected set; }

	// Token: 0x17000233 RID: 563
	// (get) Token: 0x06002B98 RID: 11160 RVA: 0x0012B382 File Offset: 0x00129782
	// (set) Token: 0x06002B99 RID: 11161 RVA: 0x0012B38A File Offset: 0x0012978A
	public DateTime createdAt { get; protected set; }

	// Token: 0x17000234 RID: 564
	// (get) Token: 0x06002B9A RID: 11162 RVA: 0x0012B393 File Offset: 0x00129793
	// (set) Token: 0x06002B9B RID: 11163 RVA: 0x0012B39B File Offset: 0x0012979B
	public DateTime updatedAt { get; protected set; }

	public List<Exdilin.Dependency> requiredMods { get; protected set; }

	// Token: 0x17000235 RID: 565
	// (get) Token: 0x06002B9C RID: 11164 RVA: 0x0012B3A4 File Offset: 0x001297A4
	public string obfuscatedSource
	{
		get
		{
			return Util.ObfuscateSourceForUser(this.source, BWUser.currentUser.userID);
		}
	}

	// Token: 0x17000236 RID: 566
	// (get) Token: 0x06002B9D RID: 11165 RVA: 0x0012B3BB File Offset: 0x001297BB
	internal bool isRemote
	{
		get
		{
			return !string.IsNullOrEmpty(this.worldID);
		}
	}

	// Token: 0x06002B9E RID: 11166 RVA: 0x0012B3CC File Offset: 0x001297CC
	internal void Reset()
	{
		this.appVersion = BWEnvConfig.BLOCKSWORLD_VERSION;
		this.authorID = 0;
		this.authorProfileImageURL = string.Empty;
		this.authorUsername = string.Empty;
		this.authorBlocksworldPremium = false;
		this.averageStarRating = 0f;
		this.categoryIDs = new List<int>();
		this.description = string.Empty;
		this.hasWinCondition = false;
		this.largeImageURL = string.Empty;
		this.likesCount = 0;
		this.playCount = 0;
		this.publicationStatus = BWWorld.PublicationStatus.NOT_PUBLISHED;
		this.smallImageURL = string.Empty;
		this.source = string.Empty;
		this.title = string.Empty;
		this.worldID = string.Empty;
		this.requiredMods = new List<Exdilin.Dependency>();
	}

	// Token: 0x06002B9F RID: 11167 RVA: 0x0012B47C File Offset: 0x0012987C
	internal void LoadFromJson(JObject json)
	{
		this.Reset();
		this.UpdateFromJson(json);
	}

	// Token: 0x06002BA0 RID: 11168 RVA: 0x0012B48C File Offset: 0x0012988C
	internal virtual void UpdateFromJson(JObject json)
	{
		this.appVersion = BWJsonHelpers.PropertyIfExists(this.appVersion, "app_version", json);
		this.authorID = BWJsonHelpers.PropertyIfExists(this.authorID, "author_id", json);
		this.authorProfileImageURL = BWJsonHelpers.PropertyIfExists(this.authorProfileImageURL, "author_profile_image_url", json);
		this.authorUsername = BWJsonHelpers.PropertyIfExists(this.authorUsername, "author_username", json);
		this.averageStarRating = BWJsonHelpers.PropertyIfExists(this.averageStarRating, "average_star_rating", json);
		int num = -1;
		num = BWJsonHelpers.PropertyIfExists(num, "author_status", json);
		this.authorBlocksworldPremium = (Util.IsPremiumUserStatus(num) || Util.IsBlocksworldOfficialUser(this.authorID));
		this.createdOnIOS = (Util.IsIOSExclusiveUserStatus(num) && !Util.IsBlocksworldOfficialUser(this.authorID));
		this.categoryIDs = BWJsonHelpers.PropertyIfExists(this.categoryIDs, "category_ids", json);
		this.description = BWJsonHelpers.PropertyIfExists(this.description, "description", json);
		this.hasWinCondition = BWJsonHelpers.PropertyIfExists(this.hasWinCondition, "has_win_condition", json);
		this.likesCount = BWJsonHelpers.PropertyIfExists(this.likesCount, "likes_count", json);
		this.playCount = BWJsonHelpers.PropertyIfExists(this.playCount, "play_count", json);
		this.publicationStatus = (BWWorld.PublicationStatus)BWJsonHelpers.PropertyIfExists((int)this.publicationStatus, "publication_status", json);
		this.source = BWJsonHelpers.PropertyIfExists(this.source, "source_json_str", json);
		this.title = BWJsonHelpers.PropertyIfExists(this.title, "title", json);
		this.worldID = BWJsonHelpers.IDPropertyAsStringIfExists(this.worldID, "id", json);
		this.createdAt = BWJsonHelpers.PropertyIfExists(this.createdAt, "created_at", json);
		this.updatedAt = BWJsonHelpers.PropertyIfExists(this.updatedAt, "updated_at", json);
		this.largeImageURL = BWJsonHelpers.PropertyIfExists(this.largeImageURL, "image_urls_for_sizes", "1024x768", json);
		this.smallImageURL = BWJsonHelpers.PropertyIfExists(this.smallImageURL, "image_urls_for_sizes", "512x384", json);
		BWJsonHelpers.AddForEachInArray(this.categoryIDs, "category_ids", json);
		if (json.ContainsKey("required_mods")) {
			List<JObject> jArray = json["required_mods"].ArrayValue;
			foreach (JObject obj in jArray) {
				Dependency dependency = new Dependency(obj["id"].StringValue, new Exdilin.Version(obj["version"].StringValue));
				this.requiredMods.Add(dependency);
			}
		}
	}

	// Token: 0x06002BA1 RID: 11169 RVA: 0x0012B6A4 File Offset: 0x00129AA4
	internal virtual Dictionary<string, object> MetadataAttributes()
	{
		List<Dictionary<string, string>> modList = new List<Dictionary<string, string>>();
		foreach (Dependency dependency in requiredMods) {
			modList.Add(new Dictionary<string, string>() {
				{
					"id",
					dependency.Id
				},
				{
					"version",
					dependency.MinimumVersion.ToString()
				}
			});
		}
		return new Dictionary<string, object>
		{
			{
				"app_version",
				this.appVersion
			},
			{
				"author_id",
				this.authorID
			},
			{
				"author_profile_image_url",
				this.authorProfileImageURL
			},
			{
				"author_username",
				this.authorUsername
			},
			{
				"author_blocksworld_premium",
				this.authorBlocksworldPremium
			},
			{
				"average_star_rating",
				this.averageStarRating
			},
			{
				"category_ids",
				this.categoryIDs
			},
			{
				"description",
				this.description
			},
			{
				"has_win_condition",
				this.hasWinCondition
			},
			{
				"likes_count",
				this.likesCount
			},
			{
				"play_count",
				this.playCount
			},
			{
				"publication_status",
				(int)this.publicationStatus
			},
			{
				"title",
				this.title
			},
			{
				"id",
				this.worldID
			},
			{
				"created_at",
				this.createdAt.ToString("s", CultureInfo.InvariantCulture)
			},
			{
				"updated_at",
				this.updatedAt.ToString("s", CultureInfo.InvariantCulture)
			},
			{
				"image_urls_for_sizes",
				new Dictionary<string, string>
				{
					{
						"1024x768",
						this.largeImageURL
					},
					{
						"512x384",
						this.smallImageURL
					}
				}
			},
			{
				"required_mods",
				modList
			}
		};
	}

	// Token: 0x06002BA2 RID: 11170 RVA: 0x0012B844 File Offset: 0x00129C44
	internal Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("title", this.title);
		dictionary.Add("description", this.description);
		dictionary.Add("author_id", this.authorID.ToString());
		dictionary.Add("author_username", this.authorUsername);
		dictionary.Add("average_star_rating", this.averageStarRating.ToString());
		dictionary.Add("created_on_ios", this.createdOnIOS.ToString());
		dictionary.Add("thumbnail_url", this.smallImageURL);
		dictionary.Add("screenshot_url", this.largeImageURL);
		dictionary.Add("author_profile_image_url", this.authorProfileImageURL);
		dictionary.Add("world_id", (this.worldID != null) ? this.worldID : string.Empty);
		string empty = string.Empty;
		string empty2 = string.Empty;
		this.GetCategoryListStrings(out empty, out empty2);
		dictionary.Add("category_names", empty);
		dictionary.Add("category_ids", empty2);
		return dictionary;
	}

	// Token: 0x06002BA3 RID: 11171 RVA: 0x0012B974 File Offset: 0x00129D74
	public void GetCategoryListStrings(out string categoryNamesListStr, out string categoryIDsListStr)
	{
		categoryNamesListStr = string.Empty;
		categoryIDsListStr = string.Empty;
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < this.categoryIDs.Count; i++)
		{
			int item = this.categoryIDs[i];
			if (!hashSet.Contains(item))
			{
				hashSet.Add(item);
				string worldCategoryName = BWCategory.GetWorldCategoryName(this.categoryIDs[i]);
				if (!string.IsNullOrEmpty(worldCategoryName))
				{
					if (i > 0)
					{
						categoryNamesListStr += ",";
						categoryIDsListStr += ",";
					}
					categoryIDsListStr += item.ToString();
					categoryNamesListStr += worldCategoryName;
				}
			}
		}
	}

	// Token: 0x06002BA4 RID: 11172 RVA: 0x0012BA3C File Offset: 0x00129E3C
	public bool AuthorIsCurrentUser()
	{
		return this.authorID == BWUser.currentUser.userID;
	}

	// Token: 0x06002BA5 RID: 11173 RVA: 0x0012BA50 File Offset: 0x00129E50
	public bool IsPublic()
	{
		return this.publicationStatus != BWWorld.PublicationStatus.NOT_PUBLISHED && this.publicationStatus != BWWorld.PublicationStatus.DELETED;
	}

	// Token: 0x06002BA6 RID: 11174 RVA: 0x0012BA6D File Offset: 0x00129E6D
	public bool IsRejected()
	{
		return this.publicationStatus == BWWorld.PublicationStatus.COMMUNITY_REJECTED || this.publicationStatus == BWWorld.PublicationStatus.REJECTED || this.publicationStatus == BWWorld.PublicationStatus.REPORT_REJECTED;
	}

	// Token: 0x06002BA7 RID: 11175 RVA: 0x0012BA95 File Offset: 0x00129E95
	public bool IsPendingModeration()
	{
		return this.publicationStatus == BWWorld.PublicationStatus.COMMUNITY_PENDING || this.publicationStatus == BWWorld.PublicationStatus.PENDING;
	}

	// Token: 0x040024E2 RID: 9442
	public static List<string> expectedImageUrlsForUI = new List<string>
	{
		"thumbnail_url",
		"screenshot_url",
		"author_profile_image_url"
	};

	// Token: 0x040024E3 RID: 9443
	public static List<string> expectedDataKeysForUI = new List<string>
	{
		"title",
		"description",
		"author_username",
		"author_id",
		"average_star_rating",
		"category_names",
		"world_id"
	};

	// Token: 0x020003D6 RID: 982
	public enum PublicationStatus
	{
		// Token: 0x040024E5 RID: 9445
		PENDING,
		// Token: 0x040024E6 RID: 9446
		APPROVED,
		// Token: 0x040024E7 RID: 9447
		REJECTED,
		// Token: 0x040024E8 RID: 9448
		DELETED = 4,
		// Token: 0x040024E9 RID: 9449
		NOT_PUBLISHED,
		// Token: 0x040024EA RID: 9450
		REPORT_REJECTED = 9,
		// Token: 0x040024EB RID: 9451
		COMMUNITY_PENDING,
		// Token: 0x040024EC RID: 9452
		COMMUNITY_APPROVED,
		// Token: 0x040024ED RID: 9453
		COMMUNITY_REJECTED
	}
}
