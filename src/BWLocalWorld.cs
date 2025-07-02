using System;
using System.Collections.Generic;
using Exdilin;
using SimpleJSON;

public class BWLocalWorld : BWWorld
{
	public bool useLocalImages;

	public bool localChangedSource;

	public bool localChangedMetadata;

	public bool localChangedScreenshot;

	public bool screenshotTakenManually;

	public bool changedFromLocalSave;

	public string screenshotChecksumStr;

	public string localWorldID { get; private set; }

	public BWLocalWorld(JObject jsonObj)
		: base(jsonObj)
	{
	}

	public BWLocalWorld(BWWorldTemplate template)
	{
		Reset();
		base.authorID = BWUser.currentUser.userID;
		base.authorUsername = BWUser.currentUser.username;
		base.authorProfileImageURL = BWUser.currentUser.profileImageURL;
		base.source = template.source;
		base.largeImageURL = template.largeImageURL;
		base.smallImageURL = template.smallImageURL;
		localWorldID = Guid.NewGuid().ToString();
		base.createdAt = DateTime.UtcNow;
		base.updatedAt = DateTime.UtcNow;
	}

	public BWLocalWorld(BWLocalWorld copyFrom)
		: base(copyFrom)
	{
		localWorldID = Guid.NewGuid().ToString();
		base.publicationStatus = PublicationStatus.NOT_PUBLISHED;
		base.worldID = string.Empty;
		base.likesCount = 0;
		base.playCount = 0;
		screenshotTakenManually = copyFrom.screenshotTakenManually;
		screenshotChecksumStr = copyFrom.screenshotChecksumStr;
	}

	public BWLocalWorld(BWWorld copyFrom, string withLocalWorldID)
		: base(copyFrom)
	{
		localWorldID = withLocalWorldID;
	}

	public BWLocalWorld(BWWorld copyFrom)
		: base(copyFrom)
	{
		localWorldID = Guid.NewGuid().ToString();
	}

	public void SetCreatedAtTime()
	{
		base.createdAt = DateTime.UtcNow;
		base.updatedAt = DateTime.UtcNow;
	}

	public void SetUpdatedAtTime()
	{
		base.updatedAt = DateTime.UtcNow;
	}

	internal override void UpdateFromJson(JObject json)
	{
		base.UpdateFromJson(json);
		localWorldID = BWJsonHelpers.PropertyIfExists(localWorldID, "local_world_id", json);
		localChangedSource = BWJsonHelpers.PropertyIfExists(localChangedSource, "local_changed_source", json);
		localChangedMetadata = BWJsonHelpers.PropertyIfExists(localChangedMetadata, "local_changed_metadata", json);
		localChangedScreenshot = BWJsonHelpers.PropertyIfExists(localChangedScreenshot, "local_changed_screenshot", json);
		screenshotTakenManually = BWJsonHelpers.PropertyIfExists(screenshotTakenManually, "screenshot_taken_manually", json);
		screenshotChecksumStr = BWJsonHelpers.PropertyIfExists(screenshotChecksumStr, "screenshot_checksum", json);
	}

	internal bool OverwriteSource(string sourceJsonStr, bool sourceHasWinCondition)
	{
		if (sourceJsonStr == null || sourceJsonStr.Length < 3)
		{
			return false;
		}
		base.hasWinCondition = sourceHasWinCondition;
		base.source = Util.UnobfuscateSourceForUser(sourceJsonStr, BWUser.currentUser.userID);
		return true;
	}

	internal void OverwriteSource_Exdilin(List<Dependency> requiredMods)
	{
		base.requiredMods = requiredMods;
	}

	internal bool OverwriteMetadata(Dictionary<string, string> data)
	{
		bool flag = false;
		changedFromLocalSave = false;
		string text = base.title;
		string text2 = base.description;
		HashSet<int> hashSet = new HashSet<int>(base.categoryIDs);
		string text3 = base.title;
		string text4 = base.description;
		HashSet<int> hashSet2 = new HashSet<int>(base.categoryIDs);
		if (base.isRemote)
		{
			BWWorld bWWorld = BWUser.currentUser.worlds.Find((BWWorld w) => w.worldID == base.worldID);
			if (bWWorld != null)
			{
				text3 = bWWorld.title;
				text4 = bWWorld.description;
				hashSet2 = new HashSet<int>(bWWorld.categoryIDs);
			}
		}
		if (data.ContainsKey("title"))
		{
			base.title = data["title"];
			changedFromLocalSave |= base.title != text;
			flag |= base.isRemote && base.title != text3;
		}
		if (data.ContainsKey("description"))
		{
			base.description = data["description"];
			changedFromLocalSave |= base.description != text2;
			flag |= base.isRemote && base.description != text4;
		}
		if (data.ContainsKey("category_ids"))
		{
			string[] array = data["category_ids"].Split(',');
			List<int> list = new List<int>();
			string[] array2 = array;
			foreach (string s in array2)
			{
				if (int.TryParse(s, out var result) && !list.Contains(result))
				{
					changedFromLocalSave |= !hashSet.Contains(result);
					flag |= base.isRemote && !hashSet2.Contains(result);
					list.Add(result);
				}
			}
			changedFromLocalSave |= list.Count != hashSet.Count;
			flag |= base.isRemote && list.Count != hashSet2.Count;
			base.categoryIDs = list;
		}
		localChangedMetadata = flag;
		return changedFromLocalSave;
	}

	internal void OverwriteImageURLsWithLocalFilePath(string filePath)
	{
		base.smallImageURL = filePath;
		base.largeImageURL = filePath;
	}

	internal void ChangeTitle(string newTitle)
	{
		base.title = newTitle;
	}

	internal void GenerateScreenshotChecksum(byte[] imageData)
	{
		string checksumStr = BWEncript.GetChecksumStr(imageData);
		screenshotChecksumStr = checksumStr;
	}

	internal override Dictionary<string, object> MetadataAttributes()
	{
		Dictionary<string, object> dictionary = base.MetadataAttributes();
		if (!string.IsNullOrEmpty(screenshotChecksumStr))
		{
			dictionary["screenshot_checksum"] = screenshotChecksumStr;
		}
		if (!string.IsNullOrEmpty(localWorldID))
		{
			dictionary["local_world_id"] = localWorldID;
			dictionary["local_changed_source"] = ((!localChangedSource) ? 0f : 1f);
			dictionary["local_changed_metadata"] = ((!localChangedMetadata) ? 0f : 1f);
			dictionary["local_changed_screenshot"] = ((!localChangedScreenshot) ? 0f : 1f);
			dictionary["screenshot_taken_manually"] = ((!screenshotTakenManually) ? 0f : 1f);
		}
		return dictionary;
	}

	public bool HasLocalChanges()
	{
		if (!localChangedSource && !localChangedMetadata)
		{
			return localChangedScreenshot;
		}
		return true;
	}

	public void StopTrackingLocalChanges()
	{
		localChangedSource = (localChangedMetadata = (localChangedScreenshot = false));
	}

	public bool IsLowEffortWorld()
	{
		int worldLowEffortGafCount = BWAppConfiguration.WorldLowEffortGafCount;
		if (worldLowEffortGafCount == 0)
		{
			return false;
		}
		if (string.IsNullOrEmpty(base.source))
		{
			BWLog.Error("Source not loaded");
			return false;
		}
		string worldGAFUsageAsBlocksInventory = Blocksworld.GetWorldGAFUsageAsBlocksInventory(base.source);
		BlocksInventory blocksInventory = BlocksInventory.FromString(worldGAFUsageAsBlocksInventory);
		int num = blocksInventory.TotalCount();
		return num < worldLowEffortGafCount;
	}

	public Dictionary<string, string> AttrsToCreateRemote()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (base.title != null)
		{
			dictionary["title"] = base.title;
		}
		if (base.description != null)
		{
			dictionary["description"] = base.description;
		}
		if (base.source != null)
		{
			dictionary["source_json_str"] = base.source;
		}
		if (base.source != null)
		{
			dictionary["has_win_condition"] = ((!base.hasWinCondition) ? "false" : "true");
		}
		if (base.source != null)
		{
			dictionary["required_mods_json_str"] = JSONEncoder.Encode(base.MetadataAttributes()["required_mods"]);
		}
		if (base.categoryIDs != null)
		{
			dictionary["category_ids_json_str"] = CategoryIdsJsonStr();
		}
		return dictionary;
	}

	public Dictionary<string, string> AttrsToSaveChangesRemote()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!HasLocalChanges())
		{
			return dictionary;
		}
		if (localChangedSource && base.source != null)
		{
			dictionary["source_json_str"] = base.source;
			dictionary["has_win_condition"] = ((!base.hasWinCondition) ? "false" : "true");
			dictionary["required_mods_json_str"] = JSONEncoder.Encode(base.MetadataAttributes()["required_mods"]);
		}
		if (localChangedMetadata)
		{
			if (base.title != null)
			{
				dictionary["title"] = base.title;
			}
			if (base.description != null)
			{
				dictionary["description"] = base.description;
			}
			if (base.categoryIDs != null)
			{
				dictionary["category_ids_json_str"] = CategoryIdsJsonStr();
			}
		}
		return dictionary;
	}

	private string CategoryIdsJsonStr()
	{
		if (base.categoryIDs.Count == 0)
		{
			return "[]";
		}
		return JSONEncoder.Encode(base.categoryIDs);
	}
}
