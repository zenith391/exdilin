using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class Dialog_CurrentUserWorldList : UIDialogPanel
{
	public Dropdown worldDropdown;

	public Button closeButton;

	public UIWorldInfo selectedWorldUI;

	public Action<string> completion;

	public Text titleText;

	public Text noWorldsText;

	private List<WorldInfo> worldInfoOptions;

	private string selectedWorldId;

	public override void Init()
	{
		selectedWorldUI.Init();
		worldDropdown.onValueChanged.AddListener(DropdownSelectedWorld);
		closeButton.onClick.AddListener(CloseDialog);
		titleText.text = "Choose Destination:";
		noWorldsText.text = string.Empty;
		noWorldsText.enabled = false;
		base.Init();
	}

	public void Setup(Action<string> completion, string worldId)
	{
		selectedWorldId = worldId;
		this.completion = completion;
		selectedWorldUI.ClearWorldInfo();
		ShowNoWorldsWarning(show: false);
		if (WorldSession.current == null)
		{
			WorldInfoList worldInfoList = new WorldInfoList();
			foreach (BWWorld world in BWUser.currentUser.worlds)
			{
				worldInfoList.AddInfoForWorld(world.worldID);
			}
			WorldListDidChange(worldInfoList, EventArgs.Empty);
			worldInfoList.WorldListChanged += WorldListDidChange;
		}
		else
		{
			if (WorldSession.current.availableTeleportWorlds == null)
			{
				WorldSession.current.LoadAvailableTeleportWorlds();
				noWorldsText.text = WorldSession.current.availableTeleportWorlds.StatusString();
				ShowNoWorldsWarning(show: true);
			}
			else
			{
				WorldListDidChange(WorldSession.current.availableTeleportWorlds, EventArgs.Empty);
			}
			WorldSession.current.availableTeleportWorlds.WorldListChanged += WorldListDidChange;
		}
	}

	private void WorldListDidChange(object sender, EventArgs e)
	{
		worldDropdown.ClearOptions();
		worldInfoOptions = new List<WorldInfo>();
		WorldInfoList worldInfoList = sender as WorldInfoList;
		WorldInfo worldInfo = null;
		List<string> list = new List<string>();
		List<WorldInfo> sortedWorldInfos = worldInfoList.sortedWorldInfos;
		if (sortedWorldInfos.Count == 0)
		{
			noWorldsText.text = worldInfoList.StatusString();
			ShowNoWorldsWarning(show: true);
			return;
		}
		ShowNoWorldsWarning(show: false);
		foreach (WorldInfo item in sortedWorldInfos)
		{
			list.Add(item.TitleForDropdown());
			worldInfoOptions.Add(item);
			if (item.id == selectedWorldId)
			{
				worldInfo = item;
			}
		}
		worldDropdown.AddOptions(list);
		if (worldInfo != null)
		{
			worldDropdown.captionText.text = worldInfo.TitleForDropdown();
			worldDropdown.value = worldInfoOptions.IndexOf(worldInfo);
			selectedWorldUI.LoadWorldInfo(worldInfo);
		}
		else if (worldInfoOptions.Count > 0)
		{
			selectedWorldId = worldInfoOptions[0].id;
			worldDropdown.value = 0;
			selectedWorldUI.LoadWorldInfo(worldInfoOptions[0]);
		}
		worldDropdown.RefreshShownValue();
	}

	private void DropdownSelectedWorld(int index)
	{
		selectedWorldId = worldInfoOptions[index].id;
		selectedWorldUI.LoadWorldInfo(worldInfoOptions[index]);
	}

	private void ShowNoWorldsWarning(bool show)
	{
		if (show)
		{
			worldDropdown.gameObject.SetActive(value: false);
			selectedWorldUI.Hide();
			noWorldsText.enabled = true;
		}
		else
		{
			worldDropdown.gameObject.SetActive(value: true);
			selectedWorldUI.Show();
			noWorldsText.enabled = false;
		}
	}

	private void CloseDialog()
	{
		if (completion != null)
		{
			completion(selectedWorldId);
		}
		ShowNoWorldsWarning(show: false);
		if (WorldSession.current != null)
		{
			WorldSession.current.availableTeleportWorlds.WorldListChanged -= WorldListDidChange;
		}
		worldInfoOptions = new List<WorldInfo>();
		selectedWorldId = string.Empty;
		selectedWorldUI.ClearWorldInfo();
		doCloseDialog();
	}
}
