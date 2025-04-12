using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003A7 RID: 935
public class BWLocalWorld : BWWorld
{
	// Token: 0x060028AA RID: 10410 RVA: 0x0012BB43 File Offset: 0x00129F43
	public BWLocalWorld(JObject jsonObj) : base(jsonObj)
	{
	}

	// Token: 0x060028AB RID: 10411 RVA: 0x0012BB4C File Offset: 0x00129F4C
	public BWLocalWorld(BWWorldTemplate template)
	{
		base.Reset();
		base.authorID = BWUser.currentUser.userID;
		base.authorUsername = BWUser.currentUser.username;
		base.authorProfileImageURL = BWUser.currentUser.profileImageURL;
		base.source = template.source;
		base.largeImageURL = template.largeImageURL;
		base.smallImageURL = template.smallImageURL;
		this.localWorldID = Guid.NewGuid().ToString();
		base.createdAt = DateTime.UtcNow;
		base.updatedAt = DateTime.UtcNow;
	}

	// Token: 0x060028AC RID: 10412 RVA: 0x0012BBE8 File Offset: 0x00129FE8
	public BWLocalWorld(BWLocalWorld copyFrom) : base(copyFrom)
	{
		this.localWorldID = Guid.NewGuid().ToString();
		base.publicationStatus = BWWorld.PublicationStatus.NOT_PUBLISHED;
		base.worldID = string.Empty;
		base.likesCount = 0;
		base.playCount = 0;
		this.screenshotTakenManually = copyFrom.screenshotTakenManually;
		this.screenshotChecksumStr = copyFrom.screenshotChecksumStr;
	}

	// Token: 0x060028AD RID: 10413 RVA: 0x0012BC4D File Offset: 0x0012A04D
	public BWLocalWorld(BWWorld copyFrom, string withLocalWorldID) : base(copyFrom)
	{
		this.localWorldID = withLocalWorldID;
	}

	// Token: 0x060028AE RID: 10414 RVA: 0x0012BC60 File Offset: 0x0012A060
	public BWLocalWorld(BWWorld copyFrom) : base(copyFrom)
	{
		this.localWorldID = Guid.NewGuid().ToString();
	}

	// Token: 0x170001AF RID: 431
	// (get) Token: 0x060028AF RID: 10415 RVA: 0x0012BC8D File Offset: 0x0012A08D
	// (set) Token: 0x060028B0 RID: 10416 RVA: 0x0012BC95 File Offset: 0x0012A095
	public string localWorldID { get; private set; }

	// Token: 0x060028B1 RID: 10417 RVA: 0x0012BC9E File Offset: 0x0012A09E
	public void SetCreatedAtTime()
	{
		base.createdAt = DateTime.UtcNow;
		base.updatedAt = DateTime.UtcNow;
	}

	// Token: 0x060028B2 RID: 10418 RVA: 0x0012BCB6 File Offset: 0x0012A0B6
	public void SetUpdatedAtTime()
	{
		base.updatedAt = DateTime.UtcNow;
	}

	// Token: 0x060028B3 RID: 10419 RVA: 0x0012BCC4 File Offset: 0x0012A0C4
	internal override void UpdateFromJson(JObject json)
	{
		base.UpdateFromJson(json);
		this.localWorldID = BWJsonHelpers.PropertyIfExists(this.localWorldID, "local_world_id", json);
		this.localChangedSource = BWJsonHelpers.PropertyIfExists(this.localChangedSource, "local_changed_source", json);
		this.localChangedMetadata = BWJsonHelpers.PropertyIfExists(this.localChangedMetadata, "local_changed_metadata", json);
		this.localChangedScreenshot = BWJsonHelpers.PropertyIfExists(this.localChangedScreenshot, "local_changed_screenshot", json);
		this.screenshotTakenManually = BWJsonHelpers.PropertyIfExists(this.screenshotTakenManually, "screenshot_taken_manually", json);
		this.screenshotChecksumStr = BWJsonHelpers.PropertyIfExists(this.screenshotChecksumStr, "screenshot_checksum", json);
	}

	// Token: 0x060028B4 RID: 10420 RVA: 0x0012BD62 File Offset: 0x0012A162
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

	internal void OverwriteSource_Exdilin(List<Exdilin.Dependency> requiredMods) {
		base.requiredMods = requiredMods;
	}

	// Token: 0x060028B5 RID: 10421 RVA: 0x0012BD98 File Offset: 0x0012A198
	internal bool OverwriteMetadata(Dictionary<string, string> data)
	{
		bool flag = false;
		this.changedFromLocalSave = false;
		string title = base.title;
		string description = base.description;
		HashSet<int> hashSet = new HashSet<int>(base.categoryIDs);
		string title2 = base.title;
		string description2 = base.description;
		HashSet<int> hashSet2 = new HashSet<int>(base.categoryIDs);
		if (base.isRemote)
		{
			BWWorld bwworld = BWUser.currentUser.worlds.Find((BWWorld w) => w.worldID == base.worldID);
			if (bwworld != null)
			{
				title2 = bwworld.title;
				description2 = bwworld.description;
				hashSet2 = new HashSet<int>(bwworld.categoryIDs);
			}
		}
		if (data.ContainsKey("title"))
		{
			base.title = data["title"];
			this.changedFromLocalSave |= (base.title != title);
			flag |= (base.isRemote && base.title != title2);
		}
		if (data.ContainsKey("description"))
		{
			base.description = data["description"];
			this.changedFromLocalSave |= (base.description != description);
			flag |= (base.isRemote && base.description != description2);
		}
		if (data.ContainsKey("category_ids"))
		{
			string[] array = data["category_ids"].Split(new char[]
			{
				','
			});
			List<int> list = new List<int>();
			foreach (string s in array)
			{
				int item;
				if (int.TryParse(s, out item) && !list.Contains(item))
				{
					this.changedFromLocalSave |= !hashSet.Contains(item);
					flag |= (base.isRemote && !hashSet2.Contains(item));
					list.Add(item);
				}
			}
			this.changedFromLocalSave |= (list.Count != hashSet.Count);
			flag |= (base.isRemote && list.Count != hashSet2.Count);
			base.categoryIDs = list;
		}
		this.localChangedMetadata = flag;
		return this.changedFromLocalSave;
	}

	// Token: 0x060028B6 RID: 10422 RVA: 0x0012BFF0 File Offset: 0x0012A3F0
	internal void OverwriteImageURLsWithLocalFilePath(string filePath)
	{
		base.smallImageURL = filePath;
		base.largeImageURL = filePath;
	}

	// Token: 0x060028B7 RID: 10423 RVA: 0x0012C00D File Offset: 0x0012A40D
	internal void ChangeTitle(string newTitle)
	{
		base.title = newTitle;
	}

	// Token: 0x060028B8 RID: 10424 RVA: 0x0012C018 File Offset: 0x0012A418
	internal void GenerateScreenshotChecksum(byte[] imageData)
	{
		string checksumStr = BWEncript.GetChecksumStr(imageData);
		this.screenshotChecksumStr = checksumStr;
	}

	// Token: 0x060028B9 RID: 10425 RVA: 0x0012C034 File Offset: 0x0012A434
	internal override Dictionary<string, object> MetadataAttributes()
	{
		Dictionary<string, object> dictionary = base.MetadataAttributes();
		if (!string.IsNullOrEmpty(this.screenshotChecksumStr))
		{
			dictionary["screenshot_checksum"] = this.screenshotChecksumStr;
		}
		if (!string.IsNullOrEmpty(this.localWorldID))
		{
			dictionary["local_world_id"] = this.localWorldID;
			dictionary["local_changed_source"] = ((!this.localChangedSource) ? 0f : 1f);
			dictionary["local_changed_metadata"] = ((!this.localChangedMetadata) ? 0f : 1f);
			dictionary["local_changed_screenshot"] = ((!this.localChangedScreenshot) ? 0f : 1f);
			dictionary["screenshot_taken_manually"] = ((!this.screenshotTakenManually) ? 0f : 1f);
		}
		return dictionary;
	}

	// Token: 0x060028BA RID: 10426 RVA: 0x0012C133 File Offset: 0x0012A533
	public bool HasLocalChanges()
	{
		return this.localChangedSource || this.localChangedMetadata || this.localChangedScreenshot;
	}

	// Token: 0x060028BB RID: 10427 RVA: 0x0012C154 File Offset: 0x0012A554
	public void StopTrackingLocalChanges()
	{
		this.localChangedSource = (this.localChangedMetadata = (this.localChangedScreenshot = false));
	}

	// Token: 0x060028BC RID: 10428 RVA: 0x0012C17C File Offset: 0x0012A57C
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
		BlocksInventory blocksInventory = BlocksInventory.FromString(worldGAFUsageAsBlocksInventory, true);
		int num = blocksInventory.TotalCount();
		return num < worldLowEffortGafCount;
	}

	// Token: 0x060028BD RID: 10429 RVA: 0x0012C1D4 File Offset: 0x0012A5D4
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
		if (base.source != null) {
			dictionary["required_mods_json_str"] = JSONEncoder.Encode(base.MetadataAttributes()["required_mods"]);
		}
		if (base.categoryIDs != null)
		{
			dictionary["category_ids_json_str"] = this.CategoryIdsJsonStr();
		}
		return dictionary;
	}

	// Token: 0x060028BE RID: 10430 RVA: 0x0012C288 File Offset: 0x0012A688
	public Dictionary<string, string> AttrsToSaveChangesRemote()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!this.HasLocalChanges())
		{
			return dictionary;
		}
		if (this.localChangedSource)
		{
			if (base.source != null)
			{
				dictionary["source_json_str"] = base.source;
				dictionary["has_win_condition"] = ((!base.hasWinCondition) ? "false" : "true");
				dictionary["required_mods_json_str"] = JSONEncoder.Encode(base.MetadataAttributes()["required_mods"]);
			}
		}
		if (this.localChangedMetadata)
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
				dictionary["category_ids_json_str"] = this.CategoryIdsJsonStr();
			}
		}
		return dictionary;
	}

	// Token: 0x060028BF RID: 10431 RVA: 0x0012C35F File Offset: 0x0012A75F
	private string CategoryIdsJsonStr()
	{
		if (base.categoryIDs.Count == 0)
		{
			return "[]";
		}
		return JSONEncoder.Encode(base.categoryIDs);
	}

	// Token: 0x0400237F RID: 9087
	public bool useLocalImages;

	// Token: 0x04002381 RID: 9089
	public bool localChangedSource;

	// Token: 0x04002382 RID: 9090
	public bool localChangedMetadata;

	// Token: 0x04002383 RID: 9091
	public bool localChangedScreenshot;

	// Token: 0x04002384 RID: 9092
	public bool screenshotTakenManually;

	// Token: 0x04002385 RID: 9093
	public bool changedFromLocalSave;

	// Token: 0x04002386 RID: 9094
	public string screenshotChecksumStr;
}
