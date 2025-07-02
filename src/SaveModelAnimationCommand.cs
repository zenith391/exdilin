using System.Collections.Generic;
using UnityEngine;

public class SaveModelAnimationCommand : ModelAnimationCommand
{
	protected override Vector3 GetTargetPos()
	{
		BuildPanel buildPanel = Blocksworld.buildPanel;
		TabBar tabBar = buildPanel.GetTabBar();
		Vector3 vector;
		if (tabBar.SelectedTab == TabBarTabId.Models)
		{
			vector = buildPanel.GetTileGridStartPosition();
			List<Tile> tilesInTab = Blocksworld.buildPanel.GetTilesInTab(TabBarTabId.Models);
			if (tilesInTab != null && tilesInTab.Count > 0)
			{
				int num = -tilesInTab.Count / buildPanel.columns;
				int num2 = tilesInTab.Count % buildPanel.columns;
				vector += new Vector3(num2 * buildPanel.tileSize, num * buildPanel.tileSize, 0f);
			}
		}
		else
		{
			vector = Blocksworld.UI.TabBar.GetTabBarPosition(TabBarTabId.Models);
		}
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(new Vector3(vector.x, vector.y, 0f) * NormalizedScreen.scale);
		return GetRayHitPos(ray);
	}
}
