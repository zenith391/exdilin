using System;
using System.Collections.Generic;

public class UIDataSourceLocalWorldList : UIDataSource
{
	public delegate bool WorldFilter(BWLocalWorld world);

	private WorldFilter filter;

	private Func<string> titleContructor;

	public UIDataSourceLocalWorldList(UIDataManager dataManager)
		: base(dataManager)
	{
		BWUserWorldsDataManager.Instance.AddListener(OnWorldListChanged);
	}

	public static UIDataSourceLocalWorldList CurrentUserUnpublishedWorlds(UIDataManager dataManager)
	{
		UIDataSourceLocalWorldList uIDataSourceLocalWorldList = new UIDataSourceLocalWorldList(dataManager);
		uIDataSourceLocalWorldList.filter = (BWLocalWorld world) => world.publicationStatus == BWWorld.PublicationStatus.NOT_PUBLISHED;
		return uIDataSourceLocalWorldList;
	}

	public static UIDataSourceLocalWorldList CurrentUserPublishedWorlds(UIDataManager dataManager)
	{
		UIDataSourceLocalWorldList uIDataSourceLocalWorldList = new UIDataSourceLocalWorldList(dataManager);
		uIDataSourceLocalWorldList.filter = (BWLocalWorld world) => world.IsPublic();
		return uIDataSourceLocalWorldList;
	}

	public override void Refresh()
	{
		base.Refresh();
		if (BWUserWorldsDataManager.Instance.localWorldsLoaded)
		{
			OnWorldListChanged();
			base.loadState = LoadState.Loaded;
		}
		else
		{
			BWUserWorldsDataManager.Instance.LoadWorlds();
		}
	}

	public override void OverwriteData(string id, string dataKey, string newValueStr)
	{
		base.OverwriteData(id, dataKey, newValueStr);
		BWLocalWorld worldWithLocalWorldID = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(id);
		if (worldWithLocalWorldID != null && worldWithLocalWorldID.OverwriteMetadata(base.Data[id]))
		{
			base.Data[id]["has_unpublished_changes"] = "true";
			if (dataKey == "category_ids")
			{
				worldWithLocalWorldID.GetCategoryListStrings(out var categoryNamesListStr, out var categoryIDsListStr);
				base.Data[id]["category_names"] = categoryNamesListStr;
				base.Data[id]["category_ids"] = categoryIDsListStr;
			}
			NotifyListeners();
		}
	}

	public override void BackupData(string id)
	{
		BWLocalWorld worldWithLocalWorldID = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(id);
		if (worldWithLocalWorldID != null && worldWithLocalWorldID.changedFromLocalSave)
		{
			BWUserWorldsDataManager.Instance.SaveWorldMetadataLocal(worldWithLocalWorldID);
			if (worldWithLocalWorldID.isRemote && worldWithLocalWorldID.publicationStatus == BWWorld.PublicationStatus.NOT_PUBLISHED)
			{
				BWUserWorldsDataManager.Instance.UpdateRemoteWorld(worldWithLocalWorldID);
			}
		}
	}

	public override void RequestFullData(string localWorldID)
	{
		BWLocalWorld world = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(localWorldID);
		if (world == null)
		{
			return;
		}
		BWUserWorldsDataManager.Instance.LoadSourceForLocalWorld(world, delegate
		{
			if (!base.Keys.Contains(localWorldID))
			{
				base.Keys.Add(localWorldID);
			}
			base.Data[localWorldID] = UIDataForWorld(world);
		});
	}

	private void OnWorldListChanged()
	{
		ClearData();
		foreach (BWLocalWorld localWorld in BWUserWorldsDataManager.Instance.localWorlds)
		{
			if (string.IsNullOrEmpty(localWorld.localWorldID))
			{
				BWLog.Info("Invalid world data, ignoring");
			}
			else if (filter == null || filter(localWorld))
			{
				base.Keys.Add(localWorld.localWorldID);
				Dictionary<string, string> value = UIDataForWorld(localWorld);
				base.Data.Add(localWorld.localWorldID, value);
			}
		}
		if (titleContructor != null)
		{
			string value2 = titleContructor();
			base.Info.Add("title", value2);
		}
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}

	private Dictionary<string, string> UIDataForWorld(BWLocalWorld worldInfo)
	{
		Dictionary<string, string> dictionary = worldInfo.AttributesForMenuUI();
		bool flag = worldInfo.publicationStatus != BWWorld.PublicationStatus.NOT_PUBLISHED;
		bool flag2 = !flag || worldInfo.HasLocalChanges();
		string value = ((!flag) ? "false" : "true");
		string value2 = ((!flag2) ? "false" : "true");
		string value3 = ((!flag) ? "Publish" : "Publish Changes");
		dictionary.Add("is_published", value);
		dictionary.Add("has_unpublished_changes", value2);
		dictionary.Add("publish_button_title", value3);
		BWWorld bWWorld = BWUser.currentUser.worlds.Find((BWWorld w) => w.worldID == worldInfo.worldID);
		bool flag3 = false;
		bool flag4 = false;
		if (bWWorld != null)
		{
			flag3 = bWWorld.IsRejected();
			flag4 = bWWorld.IsPendingModeration();
		}
		dictionary.Add("is_rejected", flag3.ToString());
		dictionary.Add("is_pending_moderation", flag4.ToString());
		return dictionary;
	}

	public override string GetPlayButtonMessage()
	{
		return "BuildLocalWorld";
	}

	public static List<string> ExpectedImageUrlsForUI()
	{
		return BWWorld.expectedImageUrlsForUI;
	}

	public static List<string> ExpectedDataKeysForUI()
	{
		return new List<string>(BWWorld.expectedDataKeysForUI) { "is_published", "has_unpublished_changes", "publish_button_title" };
	}
}
