using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000127 RID: 295
public class SaveModelAnimationCommand : ModelAnimationCommand
{
	// Token: 0x06001409 RID: 5129 RVA: 0x0008BEEC File Offset: 0x0008A2EC
	protected override Vector3 GetTargetPos()
	{
		BuildPanel buildPanel = Blocksworld.buildPanel;
		TabBar tabBar = buildPanel.GetTabBar();
		Vector3 a;
		if (tabBar.SelectedTab == TabBarTabId.Models)
		{
			a = buildPanel.GetTileGridStartPosition();
			List<Tile> tilesInTab = Blocksworld.buildPanel.GetTilesInTab(TabBarTabId.Models);
			if (tilesInTab != null && tilesInTab.Count > 0)
			{
				int num = -tilesInTab.Count / buildPanel.columns;
				int num2 = tilesInTab.Count % buildPanel.columns;
				a += new Vector3((float)(num2 * buildPanel.tileSize), (float)(num * buildPanel.tileSize), 0f);
			}
		}
		else
		{
			a = Blocksworld.UI.TabBar.GetTabBarPosition(TabBarTabId.Models);
		}
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(new Vector3(a.x, a.y, 0f) * NormalizedScreen.scale);
		return base.GetRayHitPos(ray);
	}
}
