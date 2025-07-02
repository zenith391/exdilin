using UnityEngine;

public class TabBar
{
	private const float defaultWidth = 66f;

	public const float DefaultTabHeight = 76.8f;

	public const float IndicatorWidth = 32f;

	private BuildPanel buildPanel;

	private float[] buildPanelPositions = new float[10];

	private bool allowTabChanges = true;

	private int selectedTabIndex;

	public static float pixelWidth => 66f * NormalizedScreen.pixelScale;

	public TabBarTabId SelectedTab => (TabBarTabId)selectedTabIndex;

	public TabBar(BuildPanel buildPanel)
	{
		this.buildPanel = buildPanel;
		ResetBuildPanelScrollPositions();
		Blocksworld.UI.TabBar.AssignDelegate(this);
	}

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
		default:
			BWLog.Error("Unable to determine panel index for build panel tab: " + panelSlotInternalIdentifier);
			return result;
		}
	}

	public static int GetPanelIndexForTab(string panelSlotInternalIdentifier)
	{
		TabBarTabId tab = tabBarTabIdFromPanelSlotInternalIdentifier(panelSlotInternalIdentifier);
		return GetPanelIndexForTab(tab);
	}

	public static int GetPanelIndexForTab(TabBarTabId tab)
	{
		return (int)tab;
	}

	private float GetDynamicTabHeight()
	{
		if (BW.isIPad)
		{
			return 76.8f;
		}
		float a = 200f;
		return Mathf.Min(a, (float)NormalizedScreen.height / 10f);
	}

	public void SetActive(bool active)
	{
		if (!active)
		{
			Reset();
		}
		allowTabChanges = active;
	}

	public void Reset()
	{
		ResetBuildPanelScrollPositions();
		InitializeSelectedTab(0);
	}

	private void ResetBuildPanelScrollPositions()
	{
		for (int i = 0; i < 10; i++)
		{
			buildPanelPositions[i] = NormalizedScreen.height;
		}
	}

	public void SetSelectedTab(int tabIndex, bool playSound = true)
	{
		if (!allowTabChanges)
		{
			return;
		}
		if (tabIndex != selectedTabIndex)
		{
			if (playSound)
			{
				Sound.PlayOneShotSound("Button Generic");
			}
			InitializeSelectedTab(tabIndex);
		}
		Blocksworld.bw.CancelTileDragGestures();
	}

	public void ScrollToNextSection(bool playSound = true)
	{
		if (allowTabChanges)
		{
			buildPanel.SetScrollPosToNextSection();
		}
	}

	private void InitializeSelectedTab(int tabIndex)
	{
		int num = selectedTabIndex;
		selectedTabIndex = tabIndex;
		if (num != selectedTabIndex)
		{
			buildPanelPositions[num] = buildPanel.GetScrollPos();
		}
		Scarcity.UpdateInventory();
		Blocksworld.UpdateTiles();
		buildPanel.SetScrollPos(buildPanelPositions[selectedTabIndex]);
		buildPanel.SnapBackInsideBounds(immediately: true);
	}
}
