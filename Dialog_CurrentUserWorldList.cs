using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020002E8 RID: 744
public class Dialog_CurrentUserWorldList : UIDialogPanel
{
	// Token: 0x060021FA RID: 8698 RVA: 0x000FE7CC File Offset: 0x000FCBCC
	public override void Init()
	{
		this.selectedWorldUI.Init();
		this.worldDropdown.onValueChanged.AddListener(new UnityAction<int>(this.DropdownSelectedWorld));
		this.closeButton.onClick.AddListener(new UnityAction(this.CloseDialog));
		this.titleText.text = "Choose Destination:";
		this.noWorldsText.text = string.Empty;
		this.noWorldsText.enabled = false;
		base.Init();
	}

	// Token: 0x060021FB RID: 8699 RVA: 0x000FE850 File Offset: 0x000FCC50
	public void Setup(Action<string> completion, string worldId)
	{
		this.selectedWorldId = worldId;
		this.completion = completion;
		this.selectedWorldUI.ClearWorldInfo();
		this.ShowNoWorldsWarning(false);
		if (WorldSession.current == null) {
			WorldInfoList list = new WorldInfoList();
			foreach (BWWorld world in BWUser.currentUser.worlds) {
				list.AddInfoForWorld(world.worldID);
			}
			this.WorldListDidChange(list, EventArgs.Empty);
			list.WorldListChanged += this.WorldListDidChange;
		} else {
			if (WorldSession.current.availableTeleportWorlds == null) {
				WorldSession.current.LoadAvailableTeleportWorlds();
				this.noWorldsText.text = WorldSession.current.availableTeleportWorlds.StatusString();
				this.ShowNoWorldsWarning(true);
			} else {
				this.WorldListDidChange(WorldSession.current.availableTeleportWorlds, EventArgs.Empty);
			}
			WorldSession.current.availableTeleportWorlds.WorldListChanged += this.WorldListDidChange;
		}
	}

	// Token: 0x060021FC RID: 8700 RVA: 0x000FE8EC File Offset: 0x000FCCEC
	private void WorldListDidChange(object sender, EventArgs e)
	{
		this.worldDropdown.ClearOptions();
		this.worldInfoOptions = new List<WorldInfo>();
		WorldInfoList worldInfoList = sender as WorldInfoList;
		WorldInfo worldInfo = null;
		List<string> list = new List<string>();
		List<WorldInfo> sortedWorldInfos = worldInfoList.sortedWorldInfos;
		if (sortedWorldInfos.Count == 0)
		{
			this.noWorldsText.text = worldInfoList.StatusString();
			this.ShowNoWorldsWarning(true);
		}
		else
		{
			this.ShowNoWorldsWarning(false);
			foreach (WorldInfo worldInfo2 in sortedWorldInfos)
			{
				list.Add(worldInfo2.TitleForDropdown());
				this.worldInfoOptions.Add(worldInfo2);
				if (worldInfo2.id == this.selectedWorldId)
				{
					worldInfo = worldInfo2;
				}
			}
			this.worldDropdown.AddOptions(list);
			if (worldInfo != null)
			{
				this.worldDropdown.captionText.text = worldInfo.TitleForDropdown();
				this.worldDropdown.value = this.worldInfoOptions.IndexOf(worldInfo);
				this.selectedWorldUI.LoadWorldInfo(worldInfo);
			}
			else if (this.worldInfoOptions.Count > 0)
			{
				this.selectedWorldId = this.worldInfoOptions[0].id;
				this.worldDropdown.value = 0;
				this.selectedWorldUI.LoadWorldInfo(this.worldInfoOptions[0]);
			}
			this.worldDropdown.RefreshShownValue();
		}
	}

	// Token: 0x060021FD RID: 8701 RVA: 0x000FEA7C File Offset: 0x000FCE7C
	private void DropdownSelectedWorld(int index)
	{
		this.selectedWorldId = this.worldInfoOptions[index].id;
		this.selectedWorldUI.LoadWorldInfo(this.worldInfoOptions[index]);
	}

	// Token: 0x060021FE RID: 8702 RVA: 0x000FEAAC File Offset: 0x000FCEAC
	private void ShowNoWorldsWarning(bool show)
	{
		if (show)
		{
			this.worldDropdown.gameObject.SetActive(false);
			this.selectedWorldUI.Hide();
			this.noWorldsText.enabled = true;
		}
		else
		{
			this.worldDropdown.gameObject.SetActive(true);
			this.selectedWorldUI.Show();
			this.noWorldsText.enabled = false;
		}
	}

	// Token: 0x060021FF RID: 8703 RVA: 0x000FEB14 File Offset: 0x000FCF14
	private void CloseDialog()
	{
		if (this.completion != null)
		{
			this.completion(this.selectedWorldId);
		}
		this.ShowNoWorldsWarning(false);
		if (WorldSession.current != null)
			WorldSession.current.availableTeleportWorlds.WorldListChanged -= this.WorldListDidChange;
		this.worldInfoOptions = new List<WorldInfo>();
		this.selectedWorldId = string.Empty;
		this.selectedWorldUI.ClearWorldInfo();
		this.doCloseDialog();
	}

	// Token: 0x04001D03 RID: 7427
	public Dropdown worldDropdown;

	// Token: 0x04001D04 RID: 7428
	public Button closeButton;

	// Token: 0x04001D05 RID: 7429
	public UIWorldInfo selectedWorldUI;

	// Token: 0x04001D06 RID: 7430
	public Action<string> completion;

	// Token: 0x04001D07 RID: 7431
	public Text titleText;

	// Token: 0x04001D08 RID: 7432
	public Text noWorldsText;

	// Token: 0x04001D09 RID: 7433
	private List<WorldInfo> worldInfoOptions;

	// Token: 0x04001D0A RID: 7434
	private string selectedWorldId;
}
