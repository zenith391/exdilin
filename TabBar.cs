using System;
using UnityEngine;

// Token: 0x020002BC RID: 700
public class TabBar
{
	// Token: 0x0600202A RID: 8234 RVA: 0x000ECD3F File Offset: 0x000EB13F
	public TabBar(BuildPanel buildPanel)
	{
		this.buildPanel = buildPanel;
		this.ResetBuildPanelScrollPositions();
		Blocksworld.UI.TabBar.AssignDelegate(this);
	}

	// Token: 0x0600202B RID: 8235 RVA: 0x000ECD78 File Offset: 0x000EB178
	public static TabBarTabId tabBarTabIdFromPanelSlotInternalIdentifier(string panelSlotInternalIdentifier)
	{
		TabBarTabId result = TabBarTabId.Count;
		switch (panelSlotInternalIdentifier)
		{
		case "Blocks":
			return TabBarTabId.Blocks;
		case "Models":
			return TabBarTabId.Models;
		case "Props":
			return TabBarTabId.Props;
		case "Colors":
			return TabBarTabId.Colors;
		case "Textures":
			return TabBarTabId.Textures;
		case "Blocksters":
			return TabBarTabId.Blocksters;
		case "Gear":
			return TabBarTabId.Gear;
		case "Action Blocks":
			return TabBarTabId.ActionBlocks;
		case "Actions & Scripting":
			return TabBarTabId.Actions;
		case "Sound Effects":
			return TabBarTabId.Sounds;
		}
		BWLog.Error("Unable to determine panel index for build panel tab: " + panelSlotInternalIdentifier);
		return result;
	}

	// Token: 0x0600202C RID: 8236 RVA: 0x000ECEC8 File Offset: 0x000EB2C8
	public static int GetPanelIndexForTab(string panelSlotInternalIdentifier)
	{
		TabBarTabId tab = TabBar.tabBarTabIdFromPanelSlotInternalIdentifier(panelSlotInternalIdentifier);
		return TabBar.GetPanelIndexForTab(tab);
	}

	// Token: 0x0600202D RID: 8237 RVA: 0x000ECEE4 File Offset: 0x000EB2E4
	public static int GetPanelIndexForTab(TabBarTabId tab)
	{
		return (int)tab;
	}

	// Token: 0x17000154 RID: 340
	// (get) Token: 0x0600202E RID: 8238 RVA: 0x000ECEF4 File Offset: 0x000EB2F4
	public static float pixelWidth
	{
		get
		{
			return 66f * NormalizedScreen.pixelScale;
		}
	}

	// Token: 0x0600202F RID: 8239 RVA: 0x000ECF04 File Offset: 0x000EB304
	private float GetDynamicTabHeight()
	{
		if (BW.isIPad)
		{
			return 76.8f;
		}
		float a = 200f;
		return Mathf.Min(a, (float)NormalizedScreen.height / 10f);
	}

	// Token: 0x06002030 RID: 8240 RVA: 0x000ECF39 File Offset: 0x000EB339
	public void SetActive(bool active)
	{
		if (!active)
		{
			this.Reset();
		}
		this.allowTabChanges = active;
	}

	// Token: 0x06002031 RID: 8241 RVA: 0x000ECF4E File Offset: 0x000EB34E
	public void Reset()
	{
		this.ResetBuildPanelScrollPositions();
		this.InitializeSelectedTab(0);
	}

	// Token: 0x06002032 RID: 8242 RVA: 0x000ECF60 File Offset: 0x000EB360
	private void ResetBuildPanelScrollPositions()
	{
		for (int i = 0; i < 10; i++)
		{
			this.buildPanelPositions[i] = (float)NormalizedScreen.height;
		}
	}

	// Token: 0x17000155 RID: 341
	// (get) Token: 0x06002033 RID: 8243 RVA: 0x000ECF8E File Offset: 0x000EB38E
	public TabBarTabId SelectedTab
	{
		get
		{
			return (TabBarTabId)this.selectedTabIndex;
		}
	}

	// Token: 0x06002034 RID: 8244 RVA: 0x000ECF96 File Offset: 0x000EB396
	public void SetSelectedTab(int tabIndex, bool playSound = true)
	{
		if (this.allowTabChanges)
		{
			if (tabIndex != this.selectedTabIndex)
			{
				if (playSound)
				{
					Sound.PlayOneShotSound("Button Generic", 1f);
				}
				this.InitializeSelectedTab(tabIndex);
			}
			Blocksworld.bw.CancelTileDragGestures();
		}
	}

	// Token: 0x06002035 RID: 8245 RVA: 0x000ECFD5 File Offset: 0x000EB3D5
	public void ScrollToNextSection(bool playSound = true)
	{
		if (this.allowTabChanges)
		{
			this.buildPanel.SetScrollPosToNextSection();
		}
	}

	// Token: 0x06002036 RID: 8246 RVA: 0x000ECFF0 File Offset: 0x000EB3F0
	private void InitializeSelectedTab(int tabIndex)
	{
		int num = this.selectedTabIndex;
		this.selectedTabIndex = tabIndex;
		if (num != this.selectedTabIndex)
		{
			this.buildPanelPositions[num] = this.buildPanel.GetScrollPos();
		}
		Scarcity.UpdateInventory(true, null);
		Blocksworld.UpdateTiles();
		this.buildPanel.SetScrollPos(this.buildPanelPositions[this.selectedTabIndex]);
		this.buildPanel.SnapBackInsideBounds(true);
	}

	// Token: 0x04001B71 RID: 7025
	private const float defaultWidth = 66f;

	// Token: 0x04001B72 RID: 7026
	public const float DefaultTabHeight = 76.8f;

	// Token: 0x04001B73 RID: 7027
	public const float IndicatorWidth = 32f;

	// Token: 0x04001B74 RID: 7028
	private BuildPanel buildPanel;

	// Token: 0x04001B75 RID: 7029
	private float[] buildPanelPositions = new float[10];

	// Token: 0x04001B76 RID: 7030
	private bool allowTabChanges = true;

	// Token: 0x04001B77 RID: 7031
	private int selectedTabIndex;
}
