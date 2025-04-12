using System;
using System.Collections.Generic;

// Token: 0x020003E9 RID: 1001
public class UIDataSourceLocalWorldList : UIDataSource
{
	// Token: 0x06002C2E RID: 11310 RVA: 0x0013D8EF File Offset: 0x0013BCEF
	public UIDataSourceLocalWorldList(UIDataManager dataManager) : base(dataManager)
	{
		BWUserWorldsDataManager.Instance.AddListener(new WorldsListChangedEventHandler(this.OnWorldListChanged));
	}

	// Token: 0x06002C2F RID: 11311 RVA: 0x0013D910 File Offset: 0x0013BD10
	public static UIDataSourceLocalWorldList CurrentUserUnpublishedWorlds(UIDataManager dataManager)
	{
		UIDataSourceLocalWorldList uidataSourceLocalWorldList = new UIDataSourceLocalWorldList(dataManager);
		uidataSourceLocalWorldList.filter = ((BWLocalWorld world) => world.publicationStatus == BWWorld.PublicationStatus.NOT_PUBLISHED);
		return uidataSourceLocalWorldList;
	}

	// Token: 0x06002C30 RID: 11312 RVA: 0x0013D948 File Offset: 0x0013BD48
	public static UIDataSourceLocalWorldList CurrentUserPublishedWorlds(UIDataManager dataManager)
	{
		UIDataSourceLocalWorldList uidataSourceLocalWorldList = new UIDataSourceLocalWorldList(dataManager);
		uidataSourceLocalWorldList.filter = ((BWLocalWorld world) => world.IsPublic());
		return uidataSourceLocalWorldList;
	}

	// Token: 0x06002C31 RID: 11313 RVA: 0x0013D980 File Offset: 0x0013BD80
	public override void Refresh()
	{
		base.Refresh();
		if (BWUserWorldsDataManager.Instance.localWorldsLoaded)
		{
			this.OnWorldListChanged();
			base.loadState = UIDataSource.LoadState.Loaded;
		}
		else
		{
			BWUserWorldsDataManager.Instance.LoadWorlds();
		}
	}

	// Token: 0x06002C32 RID: 11314 RVA: 0x0013D9B4 File Offset: 0x0013BDB4
	public override void OverwriteData(string id, string dataKey, string newValueStr)
	{
		base.OverwriteData(id, dataKey, newValueStr);
		BWLocalWorld worldWithLocalWorldID = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(id);
		if (worldWithLocalWorldID != null && worldWithLocalWorldID.OverwriteMetadata(base.Data[id]))
		{
			base.Data[id]["has_unpublished_changes"] = "true";
			if (dataKey == "category_ids")
			{
				string value;
				string value2;
				worldWithLocalWorldID.GetCategoryListStrings(out value, out value2);
				base.Data[id]["category_names"] = value;
				base.Data[id]["category_ids"] = value2;
			}
			base.NotifyListeners();
		}
	}

	// Token: 0x06002C33 RID: 11315 RVA: 0x0013DA5C File Offset: 0x0013BE5C
	public override void BackupData(string id)
	{
		BWLocalWorld worldWithLocalWorldID = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(id);
		if (worldWithLocalWorldID == null || !worldWithLocalWorldID.changedFromLocalSave)
		{
			return;
		}
		BWUserWorldsDataManager.Instance.SaveWorldMetadataLocal(worldWithLocalWorldID, true);
		if (worldWithLocalWorldID.isRemote && worldWithLocalWorldID.publicationStatus == BWWorld.PublicationStatus.NOT_PUBLISHED)
		{
			BWUserWorldsDataManager.Instance.UpdateRemoteWorld(worldWithLocalWorldID, null, null, null);
		}
	}

	// Token: 0x06002C34 RID: 11316 RVA: 0x0013DAB8 File Offset: 0x0013BEB8
	public override void RequestFullData(string localWorldID)
	{
		BWLocalWorld world = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(localWorldID);
		if (world == null)
		{
			return;
		}
		BWUserWorldsDataManager.Instance.LoadSourceForLocalWorld(world, delegate
		{
			if (!this.Keys.Contains(localWorldID))
			{
				this.Keys.Add(localWorldID);
			}
			this.Data[localWorldID] = this.UIDataForWorld(world);
		});
	}

	// Token: 0x06002C35 RID: 11317 RVA: 0x0013DB18 File Offset: 0x0013BF18
	private void OnWorldListChanged()
	{
		base.ClearData();
		foreach (BWLocalWorld bwlocalWorld in BWUserWorldsDataManager.Instance.localWorlds)
		{
			if (string.IsNullOrEmpty(bwlocalWorld.localWorldID))
			{
				BWLog.Info("Invalid world data, ignoring");
			}
			else if (this.filter == null || this.filter(bwlocalWorld))
			{
				base.Keys.Add(bwlocalWorld.localWorldID);
				Dictionary<string, string> value = this.UIDataForWorld(bwlocalWorld);
				base.Data.Add(bwlocalWorld.localWorldID, value);
			}
		}
		if (this.titleContructor != null)
		{
			string value2 = this.titleContructor();
			base.Info.Add("title", value2);
		}
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
	}

	// Token: 0x06002C36 RID: 11318 RVA: 0x0013DC14 File Offset: 0x0013C014
	private Dictionary<string, string> UIDataForWorld(BWLocalWorld worldInfo)
	{
		Dictionary<string, string> dictionary = worldInfo.AttributesForMenuUI();
		bool flag = worldInfo.publicationStatus != BWWorld.PublicationStatus.NOT_PUBLISHED;
		bool flag2 = !flag || worldInfo.HasLocalChanges();
		string value = (!flag) ? "false" : "true";
		string value2 = (!flag2) ? "false" : "true";
		string value3 = (!flag) ? "Publish" : "Publish Changes";
		dictionary.Add("is_published", value);
		dictionary.Add("has_unpublished_changes", value2);
		dictionary.Add("publish_button_title", value3);
		BWWorld bwworld = BWUser.currentUser.worlds.Find((BWWorld w) => w.worldID == worldInfo.worldID);
		bool flag3 = false;
		bool flag4 = false;
		if (bwworld != null)
		{
			flag3 = bwworld.IsRejected();
			flag4 = bwworld.IsPendingModeration();
		}
		dictionary.Add("is_rejected", flag3.ToString());
		dictionary.Add("is_pending_moderation", flag4.ToString());
		return dictionary;
	}

	// Token: 0x06002C37 RID: 11319 RVA: 0x0013DD3A File Offset: 0x0013C13A
	public override string GetPlayButtonMessage()
	{
		return "BuildLocalWorld";
	}

	// Token: 0x06002C38 RID: 11320 RVA: 0x0013DD41 File Offset: 0x0013C141
	public static List<string> ExpectedImageUrlsForUI()
	{
		return BWWorld.expectedImageUrlsForUI;
	}

	// Token: 0x06002C39 RID: 11321 RVA: 0x0013DD48 File Offset: 0x0013C148
	public static List<string> ExpectedDataKeysForUI()
	{
		return new List<string>(BWWorld.expectedDataKeysForUI)
		{
			"is_published",
			"has_unpublished_changes",
			"publish_button_title"
		};
	}

	// Token: 0x0400252E RID: 9518
	private UIDataSourceLocalWorldList.WorldFilter filter;

	// Token: 0x0400252F RID: 9519
	private Func<string> titleContructor;

	// Token: 0x020003EA RID: 1002
	// (Invoke) Token: 0x06002C3D RID: 11325
	public delegate bool WorldFilter(BWLocalWorld world);
}
