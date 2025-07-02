using System;
using System.Collections.Generic;
using System.Globalization;
using Exdilin;
using SimpleJSON;

public class BWWorld
{
	public enum PublicationStatus
	{
		PENDING = 0,
		APPROVED = 1,
		REJECTED = 2,
		DELETED = 4,
		NOT_PUBLISHED = 5,
		REPORT_REJECTED = 9,
		COMMUNITY_PENDING = 10,
		COMMUNITY_APPROVED = 11,
		COMMUNITY_REJECTED = 12
	}

	public static List<string> expectedImageUrlsForUI = new List<string> { "thumbnail_url", "screenshot_url", "author_profile_image_url" };

	public static List<string> expectedDataKeysForUI = new List<string> { "title", "description", "author_username", "author_id", "average_star_rating", "category_names", "world_id" };

	public string appVersion { get; protected set; }

	public int authorID { get; protected set; }

	public string authorProfileImageURL { get; protected set; }

	public string authorUsername { get; protected set; }

	public bool authorBlocksworldPremium { get; protected set; }

	public float averageStarRating { get; protected set; }

	public List<int> categoryIDs { get; protected set; }

	public bool createdOnIOS { get; protected set; }

	public string description { get; protected set; }

	public bool hasWinCondition { get; protected set; }

	public string largeImageURL { get; protected set; }

	public int likesCount { get; protected set; }

	public int playCount { get; protected set; }

	public PublicationStatus publicationStatus { get; protected set; }

	public string smallImageURL { get; protected set; }

	public string source { get; set; }

	public string title { get; protected set; }

	public string worldID { get; protected set; }

	public DateTime createdAt { get; protected set; }

	public DateTime updatedAt { get; protected set; }

	public List<Dependency> requiredMods { get; protected set; }

	public string obfuscatedSource => Util.ObfuscateSourceForUser(source, BWUser.currentUser.userID);

	internal bool isRemote => !string.IsNullOrEmpty(worldID);

	public BWWorld()
	{
		Reset();
	}

	public BWWorld(JObject json)
	{
		LoadFromJson(json);
	}

	public BWWorld(BWWorld copyFrom)
	{
		appVersion = copyFrom.appVersion;
		authorID = copyFrom.authorID;
		authorProfileImageURL = copyFrom.authorProfileImageURL;
		authorUsername = copyFrom.authorUsername;
		authorBlocksworldPremium = copyFrom.authorBlocksworldPremium;
		averageStarRating = copyFrom.averageStarRating;
		categoryIDs = new List<int>(copyFrom.categoryIDs);
		description = copyFrom.description;
		hasWinCondition = copyFrom.hasWinCondition;
		largeImageURL = copyFrom.largeImageURL;
		likesCount = copyFrom.likesCount;
		playCount = copyFrom.playCount;
		publicationStatus = copyFrom.publicationStatus;
		smallImageURL = copyFrom.smallImageURL;
		source = copyFrom.source;
		title = copyFrom.title;
		worldID = copyFrom.worldID;
		createdAt = copyFrom.createdAt;
		updatedAt = copyFrom.updatedAt;
		requiredMods = new List<Dependency>(copyFrom.requiredMods);
	}

	internal void Reset()
	{
		appVersion = BWEnvConfig.BLOCKSWORLD_VERSION;
		authorID = 0;
		authorProfileImageURL = string.Empty;
		authorUsername = string.Empty;
		authorBlocksworldPremium = false;
		averageStarRating = 0f;
		categoryIDs = new List<int>();
		description = string.Empty;
		hasWinCondition = false;
		largeImageURL = string.Empty;
		likesCount = 0;
		playCount = 0;
		publicationStatus = PublicationStatus.NOT_PUBLISHED;
		smallImageURL = string.Empty;
		source = string.Empty;
		title = string.Empty;
		worldID = string.Empty;
		requiredMods = new List<Dependency>();
	}

	internal void LoadFromJson(JObject json)
	{
		Reset();
		UpdateFromJson(json);
	}

	internal virtual void UpdateFromJson(JObject json)
	{
		appVersion = BWJsonHelpers.PropertyIfExists(appVersion, "app_version", json);
		authorID = BWJsonHelpers.PropertyIfExists(authorID, "author_id", json);
		authorProfileImageURL = BWJsonHelpers.PropertyIfExists(authorProfileImageURL, "author_profile_image_url", json);
		authorUsername = BWJsonHelpers.PropertyIfExists(authorUsername, "author_username", json);
		averageStarRating = BWJsonHelpers.PropertyIfExists(averageStarRating, "average_star_rating", json);
		int property = -1;
		property = BWJsonHelpers.PropertyIfExists(property, "author_status", json);
		authorBlocksworldPremium = Util.IsPremiumUserStatus(property) || Util.IsBlocksworldOfficialUser(authorID);
		createdOnIOS = Util.IsIOSExclusiveUserStatus(property) && !Util.IsBlocksworldOfficialUser(authorID);
		categoryIDs = BWJsonHelpers.PropertyIfExists(categoryIDs, "category_ids", json);
		description = BWJsonHelpers.PropertyIfExists(description, "description", json);
		hasWinCondition = BWJsonHelpers.PropertyIfExists(hasWinCondition, "has_win_condition", json);
		likesCount = BWJsonHelpers.PropertyIfExists(likesCount, "likes_count", json);
		playCount = BWJsonHelpers.PropertyIfExists(playCount, "play_count", json);
		publicationStatus = (PublicationStatus)BWJsonHelpers.PropertyIfExists((int)publicationStatus, "publication_status", json);
		source = BWJsonHelpers.PropertyIfExists(source, "source_json_str", json);
		title = BWJsonHelpers.PropertyIfExists(title, "title", json);
		worldID = BWJsonHelpers.IDPropertyAsStringIfExists(worldID, "id", json);
		createdAt = BWJsonHelpers.PropertyIfExists(createdAt, "created_at", json);
		updatedAt = BWJsonHelpers.PropertyIfExists(updatedAt, "updated_at", json);
		largeImageURL = BWJsonHelpers.PropertyIfExists(largeImageURL, "image_urls_for_sizes", "1024x768", json);
		smallImageURL = BWJsonHelpers.PropertyIfExists(smallImageURL, "image_urls_for_sizes", "512x384", json);
		BWJsonHelpers.AddForEachInArray(categoryIDs, "category_ids", json);
		if (!json.ContainsKey("required_mods"))
		{
			return;
		}
		List<JObject> arrayValue = json["required_mods"].ArrayValue;
		foreach (JObject item2 in arrayValue)
		{
			Dependency item = new Dependency(item2["id"].StringValue, new Exdilin.Version(item2["version"].StringValue));
			requiredMods.Add(item);
		}
	}

	internal virtual Dictionary<string, object> MetadataAttributes()
	{
		List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
		foreach (Dependency requiredMod in requiredMods)
		{
			list.Add(new Dictionary<string, string>
			{
				{ "id", requiredMod.Id },
				{
					"version",
					requiredMod.MinimumVersion.ToString()
				}
			});
		}
		return new Dictionary<string, object>
		{
			{ "app_version", appVersion },
			{ "author_id", authorID },
			{ "author_profile_image_url", authorProfileImageURL },
			{ "author_username", authorUsername },
			{ "author_blocksworld_premium", authorBlocksworldPremium },
			{ "average_star_rating", averageStarRating },
			{ "category_ids", categoryIDs },
			{ "description", description },
			{ "has_win_condition", hasWinCondition },
			{ "likes_count", likesCount },
			{ "play_count", playCount },
			{
				"publication_status",
				(int)publicationStatus
			},
			{ "title", title },
			{ "id", worldID },
			{
				"created_at",
				createdAt.ToString("s", CultureInfo.InvariantCulture)
			},
			{
				"updated_at",
				updatedAt.ToString("s", CultureInfo.InvariantCulture)
			},
			{
				"image_urls_for_sizes",
				new Dictionary<string, string>
				{
					{ "1024x768", largeImageURL },
					{ "512x384", smallImageURL }
				}
			},
			{ "required_mods", list }
		};
	}

	internal Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("title", title);
		dictionary.Add("description", description);
		dictionary.Add("author_id", authorID.ToString());
		dictionary.Add("author_username", authorUsername);
		dictionary.Add("average_star_rating", averageStarRating.ToString());
		dictionary.Add("created_on_ios", createdOnIOS.ToString());
		dictionary.Add("thumbnail_url", smallImageURL);
		dictionary.Add("screenshot_url", largeImageURL);
		dictionary.Add("author_profile_image_url", authorProfileImageURL);
		dictionary.Add("world_id", (worldID != null) ? worldID : string.Empty);
		string categoryNamesListStr = string.Empty;
		string categoryIDsListStr = string.Empty;
		GetCategoryListStrings(out categoryNamesListStr, out categoryIDsListStr);
		dictionary.Add("category_names", categoryNamesListStr);
		dictionary.Add("category_ids", categoryIDsListStr);
		return dictionary;
	}

	public void GetCategoryListStrings(out string categoryNamesListStr, out string categoryIDsListStr)
	{
		categoryNamesListStr = string.Empty;
		categoryIDsListStr = string.Empty;
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < categoryIDs.Count; i++)
		{
			int item = categoryIDs[i];
			if (hashSet.Contains(item))
			{
				continue;
			}
			hashSet.Add(item);
			string worldCategoryName = BWCategory.GetWorldCategoryName(categoryIDs[i]);
			if (!string.IsNullOrEmpty(worldCategoryName))
			{
				if (i > 0)
				{
					categoryNamesListStr += ",";
					categoryIDsListStr += ",";
				}
				categoryIDsListStr += item;
				categoryNamesListStr += worldCategoryName;
			}
		}
	}

	public bool AuthorIsCurrentUser()
	{
		return authorID == BWUser.currentUser.userID;
	}

	public bool IsPublic()
	{
		if (publicationStatus != PublicationStatus.NOT_PUBLISHED)
		{
			return publicationStatus != PublicationStatus.DELETED;
		}
		return false;
	}

	public bool IsRejected()
	{
		if (publicationStatus != PublicationStatus.COMMUNITY_REJECTED && publicationStatus != PublicationStatus.REJECTED)
		{
			return publicationStatus == PublicationStatus.REPORT_REJECTED;
		}
		return true;
	}

	public bool IsPendingModeration()
	{
		if (publicationStatus != PublicationStatus.COMMUNITY_PENDING)
		{
			return publicationStatus == PublicationStatus.PENDING;
		}
		return true;
	}
}
