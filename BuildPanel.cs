using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000261 RID: 609
public class BuildPanel : Panel
{
	// Token: 0x06001C66 RID: 7270 RVA: 0x000C8F7C File Offset: 0x000C737C
	public BuildPanel(string name, int columns) : base(name, 20)
	{
		this.columns = columns;
		this.expanded = 200f;
		this.buttonOverlayEnabled = true;
		this.separatorSize = Mathf.Abs(base.padding);
		this.tileSize = base.size + base.margin;
		this.width = Blocksworld.UI.SidePanel.BuildPanelWidth();
		this.height = (float)NormalizedScreen.height;
		this.CreateExtras();
		this.tabBar = new TabBar(this);
		this.tileObjectPool = new TilePool("BuildPanel", 128, TilePool.TileImageSource.Resources);
		this.useMeshBackground = false;
		ViewportWatchdog.AddListener(new ViewportWatchdog.ViewportSizeChangedAction(this.ViewportSizeDidChange));
		this.tiles = new List<List<Tile>>();
	}

	// Token: 0x06001C67 RID: 7271 RVA: 0x000C910E File Offset: 0x000C750E
	public TabBar GetTabBar()
	{
		return this.tabBar;
	}

	// Token: 0x06001C68 RID: 7272 RVA: 0x000C9118 File Offset: 0x000C7518
	public Vector3 GetTileGridStartPosition()
	{
		float x = -(float)base.padding;
		float y = (float)base.padding;
		return base.position + new Vector3(x, y, 0f);
	}

	// Token: 0x06001C69 RID: 7273 RVA: 0x000C9150 File Offset: 0x000C7550
	public void CreateInventoryTiles(HashSet<GAF> gafs)
	{
		this.inventoryGAFs = gafs;
		this.inventoryBlockItemIds = new HashSet<int>();
		for (int i = 0; i < 10; i++)
		{
			List<Tile> list = new List<Tile>();
			this.tiles.Add(list);
			List<List<int>> tilesInTabAsIDs = PanelSlots.GetTilesInTabAsIDs((TabBarTabId)i);
			if (tilesInTabAsIDs == null)
			{
				Debug.Log("No build panel tab data for tab " + (TabBarTabId)i);
			}
			else
			{
				for (int j = 0; j < tilesInTabAsIDs.Count; j++)
				{
					List<int> list2 = tilesInTabAsIDs[j];
					for (int k = 0; k < list2.Count; k++)
					{
						int num = list2[k];
						BlockItem blockItem = BlockItem.FindByID(num);
						GAF gaf = new GAF(blockItem);
						if (this.inventoryGAFs.Contains(gaf))
						{
							this.inventoryBlockItemIds.Add(num);
							Tile tile = base.CreateTileInPanel(gaf);
							tile.panelSection = j;
							list.Add(tile);
							if (gaf.Predicate == Block.predicateSendSignal)
							{
								this.customSignalSectionIndex = j + 1;
							}
							else if (gaf.Predicate == Block.predicateVariableCustomInt || gaf.Predicate == Block.predicateBlockVariableInt)
							{
								this.customVariableSectionIndex = j;
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06001C6A RID: 7274 RVA: 0x000C929C File Offset: 0x000C769C
	public Tile FindTileMatching(GAF patternGaf)
	{
		if (patternGaf == null || this.tiles == null)
		{
			return null;
		}
		for (int i = 0; i < this.tiles.Count; i++)
		{
			for (int j = 0; j < this.tiles[i].Count; j++)
			{
				Tile tile = this.tiles[i][j];
				if (patternGaf.Matches(tile.gaf))
				{
					return tile;
				}
			}
		}
		return null;
	}

	// Token: 0x06001C6B RID: 7275 RVA: 0x000C9324 File Offset: 0x000C7724
	public Tile FindTile(GAF gaf)
	{
		if (gaf == null || this.tiles == null)
		{
			return null;
		}
		for (int i = 0; i < this.tiles.Count; i++)
		{
			for (int j = 0; j < this.tiles[i].Count; j++)
			{
				Tile tile = this.tiles[i][j];
				if (gaf.Equals(tile.gaf))
				{
					return tile;
				}
			}
		}
		return null;
	}

	// Token: 0x06001C6C RID: 7276 RVA: 0x000C93A9 File Offset: 0x000C77A9
	public Tile GetFirstTileInSection(int section)
	{
		if (section < this.tileSections.Count && this.tileSections[section].Count > 0)
		{
			return this.tileSections[section][0];
		}
		return null;
	}

	// Token: 0x06001C6D RID: 7277 RVA: 0x000C93E7 File Offset: 0x000C77E7
	public void ClearCachedGAFs()
	{
		this.customSignalGAFs.Clear();
		this.customVariableGAFs.Clear();
	}

	// Token: 0x06001C6E RID: 7278 RVA: 0x000C9400 File Offset: 0x000C7800
	public Tile AddTileToBuildPanel(GAF gaf, TabBarTabId tab, int section, int indexInSection = -1)
	{
		Tile tile = base.CreateTileInPanel(gaf);
		tile.panelSection = section;
		if (indexInSection < 0)
		{
			this.tiles[(int)tab].Add(tile);
		}
		else
		{
			List<Tile> list = this.tiles[(int)tab];
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < list.Count; i++)
			{
				Tile tile2 = list[i];
				if (tile2.panelSection == section)
				{
					if (num == indexInSection)
					{
						list.Insert(i, tile);
						return tile;
					}
					num2 = i;
					num++;
				}
			}
			if (num2 == list.Count - 1)
			{
				list.Add(tile);
			}
			else
			{
				list.Insert(num2 + 1, tile);
			}
		}
		return tile;
	}

	// Token: 0x06001C6F RID: 7279 RVA: 0x000C94BD File Offset: 0x000C78BD
	public bool IsGAFInBuildPanel(GAF gaf)
	{
		return this.inventoryGAFs.Contains(gaf) || this.customSignalGAFs.Contains(gaf) || this.customVariableGAFs.Contains(gaf);
	}

	// Token: 0x06001C70 RID: 7280 RVA: 0x000C94F0 File Offset: 0x000C78F0
	public bool IsBlockItemInInventory(int blockItemId)
	{
		return this.inventoryBlockItemIds.Contains(blockItemId);
	}

	// Token: 0x06001C71 RID: 7281 RVA: 0x000C9500 File Offset: 0x000C7900
	public List<Tile> GetTilesInTab(TabBarTabId tab)
	{
		if (tab < (TabBarTabId)this.tiles.Count)
		{
			return this.tiles[(int)tab];
		}
		return null;
	}

	// Token: 0x1700012E RID: 302
	// (get) Token: 0x06001C72 RID: 7282 RVA: 0x000C9530 File Offset: 0x000C7930
	private IEnumerable<Tile> allTiles
	{
		get
		{
			for (int r = 0; r < this.tiles.Count; r++)
			{
				for (int i = 0; i < this.tiles[r].Count; i++)
				{
					yield return this.tiles[r][i];
				}
			}
			yield break;
		}
	}

	// Token: 0x06001C73 RID: 7283 RVA: 0x000C9553 File Offset: 0x000C7953
	protected override float PanelYOffset()
	{
		return -this.height;
	}

	// Token: 0x06001C74 RID: 7284 RVA: 0x000C955C File Offset: 0x000C795C
	private void CreateExtras()
	{
		this.goShopButton = (UnityEngine.Object.Instantiate(Resources.Load("GUI/Prefab Panel Shop Button")) as GameObject);
		this.goShopButton.name = "Panel Shop Button";
		this.goShopButton.transform.parent = this.goParent.transform;
		this.goShopButton.transform.localScale = new Vector3(this.goShopButton.transform.localScale.x * NormalizedScreen.pixelScale, this.goShopButton.transform.localScale.y * NormalizedScreen.pixelScale, 1f);
		this.goShopButton.transform.localPosition = new Vector3(0.565f * this.goShopButton.transform.localScale.x, 0f, 0f);
		this.goShopButton.SetActive(this.showShopButton);
		this.goTabTitleLabel = (UnityEngine.Object.Instantiate(Resources.Load("GUI/Prefab Tab Title Label")) as GameObject);
		this.goTabTitleLabel.name = "Build Panel Title";
		this.goTabTitleLabel.transform.parent = this.goParent.transform;
		this.goTabTitleLabel.transform.localScale = new Vector3(NormalizedScreen.pixelScale, NormalizedScreen.pixelScale, 1f);
		float num = Blocksworld.UI.QuickSelect.GetHeight() * NormalizedScreen.pixelScale * 1.1f;
		this.goTabTitleLabel.transform.localPosition = new Vector3(0.565f * this.goShopButton.transform.localScale.x, -num, 0f);
	}

	// Token: 0x06001C75 RID: 7285 RVA: 0x000C9714 File Offset: 0x000C7B14
	private Vector3 CalculateSnapWithinBounds(Vector3 p)
	{
		float max = Mathf.Max((float)NormalizedScreen.height + 2.1f, this.height - 2.1f);
		p.y = Mathf.Clamp(p.y, (float)NormalizedScreen.height + 2.1f, max);
		return p;
	}

	// Token: 0x06001C76 RID: 7286 RVA: 0x000C9760 File Offset: 0x000C7B60
	public void PositionReset(bool hide = false)
	{
		this.pos = this.GetResetPosition();
		this.UpdatePosition();
	}

	// Token: 0x06001C77 RID: 7287 RVA: 0x000C9774 File Offset: 0x000C7B74
	public Vector3 GetResetPosition()
	{
		Vector3 a = new Vector3((float)NormalizedScreen.width, (float)NormalizedScreen.height, this.depth);
		return a + Blocksworld.UI.SidePanel.GetBuildPanelTopLeftOffset() / NormalizedScreen.scale;
	}

	// Token: 0x06001C78 RID: 7288 RVA: 0x000C97BB File Offset: 0x000C7BBB
	public override void Move(Vector3 delta)
	{
		base.Move(delta);
	}

	// Token: 0x06001C79 RID: 7289 RVA: 0x000C97C4 File Offset: 0x000C7BC4
	public void GhostVisibleTiles(bool ghost)
	{
		foreach (Tile tile in this.allTiles)
		{
			if (tile.IsShowing())
			{
				if (ghost)
				{
					tile.tileObject.Disable();
				}
				else if (tile.IsEnabled())
				{
					tile.tileObject.Enable();
				}
				else
				{
					tile.tileObject.Disable();
				}
			}
		}
	}

	// Token: 0x06001C7A RID: 7290 RVA: 0x000C9864 File Offset: 0x000C7C64
	public void ClearTileSections()
	{
		if (this.tileSections == null || this.tileSections.Count == 0)
		{
			return;
		}
		if (BuildPanel.predicateGAFsToDestroyOnRefresh == null)
		{
			BuildPanel.predicateGAFsToDestroyOnRefresh = new HashSet<Predicate>();
			BuildPanel.predicateGAFsToDestroyOnRefresh.Add(Block.predicateSendCustomSignal);
			BuildPanel.predicateGAFsToDestroyOnRefresh.Add(Block.predicateSendCustomSignalModel);
			BuildPanel.predicateGAFsToDestroyOnRefresh.UnionWith(Block.customVariablePredicates);
		}
		for (int i = 0; i < this.tileSections.Count; i++)
		{
			for (int j = 0; j < this.tileSections[i].Count; j++)
			{
				Tile tile = this.tileSections[i][j];
				tile.Show(false);
				if (BuildPanel.predicateGAFsToDestroyOnRefresh.Contains(tile.gaf.Predicate))
				{
					tile.DestroyPhysical();
				}
			}
		}
		this.tileSections = null;
	}

	// Token: 0x06001C7B RID: 7291 RVA: 0x000C9950 File Offset: 0x000C7D50
	private void RefreshTileSections()
	{
		if (this.tileSections == null || this.tileSections.Count == 0)
		{
			return;
		}
		for (int i = 0; i < this.tileSections.Count; i++)
		{
			for (int j = 0; j < this.tileSections[i].Count; j++)
			{
				Tile tile = this.tileSections[i][j];
				Vector3 pos = this.goParent.transform.position + tile.positionInPanel;
				if (tile.visibleInPanel && this.OnScreenTest(pos))
				{
					if (!tile.IsShowing())
					{
						tile.CreatePhysical();
						tile.tileObject.transform.parent = this.goParent.transform;
						tile.tileObject.SetScale(NormalizedScreen.pixelScale * Vector3.one);
					}
					tile.tileObject.MoveTo(pos);
				}
				else
				{
					tile.DestroyPhysical();
				}
			}
		}
		this.lastRefreshPos = this.pos.y;
		this.lastRefreshTime = Time.realtimeSinceStartup;
	}

	// Token: 0x06001C7C RID: 7292 RVA: 0x000C9A78 File Offset: 0x000C7E78
	private bool OnScreenTest(Vector3 pos)
	{
		return pos.x > -100f && pos.x < (float)(NormalizedScreen.width + 100) && pos.y > -100f && pos.y < (float)(NormalizedScreen.height + 100);
	}

	// Token: 0x06001C7D RID: 7293 RVA: 0x000C9AD4 File Offset: 0x000C7ED4
	private void ViewportSizeDidChange()
	{
		this.separatorSize = Mathf.Abs(base.padding);
		this.tileSize = base.size + base.margin;
		this.width = Blocksworld.UI.SidePanel.BuildPanelWidth();
		this.PositionReset(!this.isPanelVisible);
		if (this.isPanelVisible)
		{
			this.Layout();
		}
		else
		{
			this.doLayoutOnShow = true;
		}
	}

	// Token: 0x06001C7E RID: 7294 RVA: 0x000C9B48 File Offset: 0x000C7F48
	public override void UpdatePosition()
	{
		base.UpdatePosition();
		if (this.isPanelVisible && Mathf.Abs(this.pos.y - this.lastRefreshPos) > 50f && Time.realtimeSinceStartup - this.lastRefreshTime > 0.25f)
		{
			this.RefreshTileSections();
		}
	}

	// Token: 0x06001C7F RID: 7295 RVA: 0x000C9BA4 File Offset: 0x000C7FA4
	public void SnapBackInsideBounds(bool immediately = false)
	{
		if (this.pos.y < (float)(NormalizedScreen.height + 2))
		{
			base.MoveTo(new Vector2(this.pos.x, (float)(NormalizedScreen.height + 2)), immediately, true);
		}
		else if (this.pos.y > this.height - 2f)
		{
			base.MoveTo(new Vector2(this.pos.x, this.height - 2f), immediately, true);
		}
	}

	// Token: 0x06001C80 RID: 7296 RVA: 0x000C9C38 File Offset: 0x000C8038
	public override void EndTrackingTouch()
	{
		base.EndTrackingTouch();
		this.SnapBackInsideBounds(false);
	}

	// Token: 0x06001C81 RID: 7297 RVA: 0x000C9C47 File Offset: 0x000C8047
	public override bool IsWithinBounds()
	{
		return this.pos.y >= (float)(NormalizedScreen.height - 2) && this.pos.y <= this.height + 2f;
	}

	// Token: 0x06001C82 RID: 7298 RVA: 0x000C9C80 File Offset: 0x000C8080
	public bool AtTopLimit()
	{
		return this.pos.y < (float)(NormalizedScreen.height + 8);
	}

	// Token: 0x06001C83 RID: 7299 RVA: 0x000C9C97 File Offset: 0x000C8097
	public bool AtBottomLimit()
	{
		return this.pos.y > this.height - 8f;
	}

	// Token: 0x06001C84 RID: 7300 RVA: 0x000C9CB4 File Offset: 0x000C80B4
	public void UpdateInner()
	{
		if (!this.trackingTouch)
		{
			this.pos.y = this.pos.y + this.smoothSpeed.y;
		}
		if (this.snappingBack)
		{
			this.smoothSpeed.y = this.smoothSpeed.y * this._snapBackMomentum;
		}
		else
		{
			this.smoothSpeed.y = this.smoothSpeed.y * this._momentum;
		}
		if (base.IsShowing())
		{
			this.UpdatePosition();
		}
		if (!this.IsWithinBounds() && this.wasWithinBounds && !this.trackingTouch && !this.snappingBack)
		{
			this.boundsPassingSpeed = this.smoothSpeed.magnitude;
		}
		if (!this.trackingTouch && !this.IsWithinBounds() && !this.snappingBack)
		{
			this.smoothSpeed = this.smoothSpeed / this._momentum * Mathf.Pow(this._snapBackMomentum, 1.5f);
			if (Math.Abs(this.smoothSpeed.magnitude) < this.boundsPassingSpeed / 2f)
			{
				this.SnapBackInsideBounds(false);
			}
		}
	}

	// Token: 0x06001C85 RID: 7301 RVA: 0x000C9DEC File Offset: 0x000C81EC
	public void Update()
	{
		this.wasWithinBounds = this.IsWithinBounds();
		base.StepTutorialIfNecessary();
		float num = Time.deltaTime;
		int num2 = 0;
		while (num > 0f && num2 < 3)
		{
			this.UpdateInner();
			num -= 0.0333333351f;
			num2++;
		}
		this.UpdateMessage();
	}

	// Token: 0x06001C86 RID: 7302 RVA: 0x000C9E44 File Offset: 0x000C8244
	private void UpdateMessage()
	{
		if (this.isPanelVisible && this.panelIsEmpty)
		{
			string messageStr = Blocksworld.UI.TabBar.GetNoDataMessageStr();
			if (this.tabBar.SelectedTab == TabBarTabId.Models)
			{
				BuildPanel.ModelSelectionState modelSelectionState = BuildPanel.ModelSelectionState.None;
				if (TBox.IsShowingModel())
				{
					modelSelectionState = BuildPanel.ModelSelectionState.PotentialModel;
				}
				else if (TBox.IsShowing())
				{
					if (Blocksworld.selectedBlock != null && Blocksworld.selectedBlock.connectionTypes.Count > 0)
					{
						modelSelectionState = BuildPanel.ModelSelectionState.ConnectedSingleBlock;
					}
					else
					{
						modelSelectionState = BuildPanel.ModelSelectionState.SingleBlock;
					}
				}
				messageStr = this.modelHelpMessages[(int)modelSelectionState];
			}
			Blocksworld.UI.SidePanel.ShowPanelMessage(messageStr);
		}
		else
		{
			Blocksworld.UI.SidePanel.HidePanelMessage();
		}
	}

	// Token: 0x06001C87 RID: 7303 RVA: 0x000C9EFC File Offset: 0x000C82FC
	public override void Show(bool show)
	{
		base.Show(show);
		this.goShopButton.SetActive(show && this.showShopButton);
		if (show)
		{
			this.goShopButton.GetComponent<MeshRenderer>().enabled = true;
			this.goTabTitleLabel.GetComponent<MeshRenderer>().enabled = true;
		}
		if (this.ignoreShow)
		{
			this.ignoreShow = false;
			return;
		}
		this.SnapBackInsideBounds(true);
		Vector3 resetPosition = this.GetResetPosition();
		resetPosition.y = this.pos.y;
		base.MoveTo(resetPosition, true, false);
		if (show && this.doLayoutOnShow)
		{
			this.doLayoutOnShow = false;
			this.Layout();
		}
		this.isPanelVisible = show;
	}

	// Token: 0x06001C88 RID: 7304 RVA: 0x000C9FB3 File Offset: 0x000C83B3
	public void SetTutorialMode(bool tutorialActive)
	{
		this.ShowShopButton(!tutorialActive);
		this.tabBar.SetActive(true);
	}

	// Token: 0x06001C89 RID: 7305 RVA: 0x000C9FCB File Offset: 0x000C83CB
	public void ShowShopButton(bool show)
	{
		this.showShopButton = show;
		this.goShopButton.SetActive(this.showShopButton && base.IsShowing());
		this.Layout();
		this.PositionReset(false);
	}

	// Token: 0x06001C8A RID: 7306 RVA: 0x000CA000 File Offset: 0x000C8400
	public void ResetState()
	{
		this.tabBar.Reset();
	}

	// Token: 0x06001C8B RID: 7307 RVA: 0x000CA010 File Offset: 0x000C8410
	public void DisableTilesBut(Tile but)
	{
		foreach (Tile tile in this.allTiles)
		{
			tile.Enable(tile == but);
		}
	}

	// Token: 0x06001C8C RID: 7308 RVA: 0x000CA06C File Offset: 0x000C846C
	public void ClearAllTiles()
	{
		foreach (Tile tile in this.allTiles)
		{
			tile.Show(false);
			tile.DestroyPhysical();
		}
		this.tiles = new List<List<Tile>>();
		this.tileSections = new List<List<Tile>>();
	}

	// Token: 0x06001C8D RID: 7309 RVA: 0x000CA0E4 File Offset: 0x000C84E4
	private void HideAllTiles()
	{
		foreach (Tile tile in this.allTiles)
		{
			tile.Show(false);
		}
	}

	// Token: 0x06001C8E RID: 7310 RVA: 0x000CA140 File Offset: 0x000C8540
	private void ShowAllTiles()
	{
		foreach (Tile tile in this.allTiles)
		{
			tile.Show(true);
		}
	}

	// Token: 0x06001C8F RID: 7311 RVA: 0x000CA19C File Offset: 0x000C859C
	private List<List<Tile>> ComputeTileVisibility(TabBarTabId selectedTab)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		List<Tile> list2 = this.tiles[(int)selectedTab];
		for (int i = 0; i < 10; i++)
		{
			TabBarTabId tabBarTabId = (TabBarTabId)i;
			if (tabBarTabId != selectedTab)
			{
				this.HideTilesInTab(tabBarTabId);
			}
		}
		Blocksworld.UpdateTilesForTab(selectedTab, list2);
		for (int j = 0; j < list2.Count; j++)
		{
			Tile tile = list2[j];
			if (tile.visibleInPanel)
			{
				if (Tutorial.InTutorialOrPuzzle() || !BW.Options.useScarcity() || Scarcity.GetExistsInInventory(tile.gaf))
				{
					while (list.Count <= tile.panelSection)
					{
						list.Add(new List<Tile>());
					}
					List<Tile> list3 = list[tile.panelSection];
					list3.Add(list2[j]);
				}
			}
		}
		if (selectedTab == TabBarTabId.Actions)
		{
			this.InsertCustomSignalTiles(list);
			this.InsertCustomVariableTiles(list);
		}
		for (int k = list.Count - 1; k >= 0; k--)
		{
			if (list[k].Count == 0)
			{
				list.RemoveAt(k);
			}
		}
		return list;
	}

	// Token: 0x06001C90 RID: 7312 RVA: 0x000CA2E0 File Offset: 0x000C86E0
	private void InsertCustomSignalTiles(List<List<Tile>> sections)
	{
		this.customSignalGAFs.Clear();
		bool flag = Blocksworld.GetSelectedScriptBlock() != null;
		flag &= (Blocksworld.customSignals.Count > 0);
		flag &= (sections.Count > this.customSignalSectionIndex);
		if (flag)
		{
			List<Tile> list = new List<Tile>();
			sections.Insert(this.customSignalSectionIndex, list);
			foreach (string text in Blocksworld.customSignals)
			{
				GAF gaf = new GAF(Block.predicateSendCustomSignal, new object[]
				{
					text,
					1f
				});
				GAF item = new GAF(Block.predicateSendCustomSignalModel, new object[]
				{
					text,
					1f
				});
				this.customSignalGAFs.Add(gaf);
				this.customSignalGAFs.Add(item);
				Tile tile = base.CreateTileInPanel(gaf);
				tile.panelSection = this.customSignalSectionIndex;
				list.Add(tile);
				tile.Show(true);
				if (Blocksworld.enabledGAFs != null)
				{
					bool enabled = Blocksworld.enabledGAFs.Contains(tile.gaf);
					tile.Enable(enabled);
				}
			}
		}
	}

	// Token: 0x06001C91 RID: 7313 RVA: 0x000CA434 File Offset: 0x000C8834
	private void InsertCustomVariableTiles(List<List<Tile>> sections)
	{
		this.customVariableGAFs.Clear();
		Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
		bool flag = selectedScriptBlock != null;
		flag &= (sections.Count > this.customVariableSectionIndex);
		if (flag)
		{
			List<Tile> list = new List<Tile>();
			sections.Insert(this.customVariableSectionIndex, list);
			Tile tile = base.CreateTileInPanel(new GAF(Block.predicateVariableCustomInt, new object[]
			{
				"*",
				0
			}));
			tile.panelSection = this.customVariableSectionIndex;
			list.Add(tile);
			tile.Show(true);
			if (Blocksworld.enabledGAFs != null)
			{
				bool enabled = Blocksworld.enabledGAFs.Contains(tile.gaf);
				tile.Enable(enabled);
			}
			Tile tile2 = base.CreateTileInPanel(new GAF(Block.predicateBlockVariableInt, new object[]
			{
				"*",
				0
			}));
			tile2.panelSection = this.customVariableSectionIndex;
			list.Add(tile2);
			tile2.Show(true);
			if (Blocksworld.enabledGAFs != null)
			{
				bool enabled2 = Blocksworld.enabledGAFs.Contains(tile2.gaf);
				tile2.Enable(enabled2);
			}
			List<Tile> list2 = null;
			List<Tile> list3 = null;
			List<Tile> list4 = null;
			List<Tile> list5 = null;
			int num = this.customVariableSectionIndex + 1;
			int num2 = this.customVariableSectionIndex + 1;
			int num3 = this.customVariableSectionIndex + 1;
			int num4 = this.customVariableSectionIndex + 1;
			if (Blocksworld.customIntVariables.Count > 0)
			{
				list2 = new List<Tile>();
				sections.Insert(num, list2);
				num2 = num + 1;
			}
			if (Blocksworld.blockIntVariables.ContainsKey(selectedScriptBlock))
			{
				if (Blocksworld.customIntVariables.Count > 0)
				{
					list4 = new List<Tile>();
					sections.Insert(num2, list4);
					num3 = num2 + 1;
				}
				list3 = new List<Tile>();
				sections.Insert(num3, list3);
				num4 = num3 + 1;
				if (Blocksworld.blockIntVariables[selectedScriptBlock].Count > 0)
				{
					list5 = new List<Tile>();
					sections.Insert(num4, list5);
				}
			}
			foreach (KeyValuePair<string, int> keyValuePair in Blocksworld.customIntVariables)
			{
				this.AddOperatorTile(keyValuePair.Key, Block.predicateVariableCustomInt, keyValuePair.Value, this.customVariableSectionIndex, list);
				foreach (Predicate predicate in Block.globalVariableOperations)
				{
					int arg = Block.variablePredicateParamDefaults[predicate];
					this.AddOperatorTile(keyValuePair.Key, predicate, arg, num, list2);
				}
			}
			if (Blocksworld.blockIntVariables.ContainsKey(selectedScriptBlock))
			{
				foreach (KeyValuePair<string, int> keyValuePair2 in Blocksworld.blockIntVariables[selectedScriptBlock])
				{
					this.AddOperatorTile(keyValuePair2.Key, Block.predicateBlockVariableInt, keyValuePair2.Value, this.customVariableSectionIndex, list);
					foreach (Predicate predicate2 in Block.blockVariableOperations)
					{
						int arg2 = Block.variablePredicateParamDefaults[predicate2];
						this.AddOperatorTile(keyValuePair2.Key, predicate2, arg2, num3, list3);
					}
					foreach (KeyValuePair<string, int> keyValuePair3 in Blocksworld.customIntVariables)
					{
						this.AddVariableTile(keyValuePair2.Key, Block.predicateBlockVariableIntLoadGlobal, keyValuePair3.Key, num2, list4);
					}
					foreach (KeyValuePair<string, int> keyValuePair4 in Blocksworld.blockIntVariables[selectedScriptBlock])
					{
						if (keyValuePair4.Key != keyValuePair2.Key)
						{
							foreach (Predicate pred in Block.blockVariableOperationsOnOtherBlockVars)
							{
								this.AddVariableTile(keyValuePair2.Key, pred, keyValuePair4.Key, num4, list5);
							}
						}
					}
				}
				foreach (KeyValuePair<string, int> keyValuePair5 in Blocksworld.blockIntVariables[selectedScriptBlock])
				{
					foreach (KeyValuePair<string, int> keyValuePair6 in Blocksworld.customIntVariables)
					{
						this.AddVariableTile(keyValuePair5.Key, Block.predicateBlockVariableIntStoreGlobal, keyValuePair6.Key, num2, list4);
					}
				}
			}
		}
	}

	// Token: 0x06001C92 RID: 7314 RVA: 0x000CAA18 File Offset: 0x000C8E18
	private void AddOperatorTile(string variable, Predicate pred, int arg, int sectionIndex, List<Tile> tileSection)
	{
		GAF gaf = new GAF(pred, new object[]
		{
			variable,
			arg
		});
		this.customVariableGAFs.Add(gaf);
		Tile tile = base.CreateTileInPanel(gaf);
		tile.panelSection = sectionIndex;
		tileSection.Add(tile);
		tile.Show(true);
		if (Blocksworld.enabledGAFs != null)
		{
			bool enabled = Blocksworld.enabledGAFs.Contains(tile.gaf);
			tile.Enable(enabled);
		}
	}

	// Token: 0x06001C93 RID: 7315 RVA: 0x000CAA90 File Offset: 0x000C8E90
	private void AddVariableTile(string variable, Predicate pred, string arg, int sectionIndex, List<Tile> tileSection)
	{
		GAF gaf = new GAF(pred, new object[]
		{
			variable,
			arg
		});
		this.customVariableGAFs.Add(gaf);
		Tile tile = base.CreateTileInPanel(gaf);
		tile.panelSection = sectionIndex;
		tileSection.Add(tile);
		tile.Show(true);
		if (Blocksworld.enabledGAFs != null)
		{
			bool enabled = Blocksworld.enabledGAFs.Contains(tile.gaf);
			tile.Enable(enabled);
		}
	}

	// Token: 0x06001C94 RID: 7316 RVA: 0x000CAB00 File Offset: 0x000C8F00
	private void HideTilesInTab(TabBarTabId tab)
	{
		if (tab >= (TabBarTabId)this.tiles.Count)
		{
			return;
		}
		List<Tile> list = this.tiles[(int)tab];
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Show(false);
			list[i].DestroyPhysical();
		}
	}

	// Token: 0x06001C95 RID: 7317 RVA: 0x000CAB5E File Offset: 0x000C8F5E
	private int RowCount(int elements)
	{
		return (elements != 0) ? (1 + (elements - 1) / this.columns) : 0;
	}

	// Token: 0x06001C96 RID: 7318 RVA: 0x000CAB78 File Offset: 0x000C8F78
	private void LayoutTileSection(List<Tile> contiguousTiles, float startingY)
	{
		if (this.goParent == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < contiguousTiles.Count; i++)
		{
			Tile tile = contiguousTiles[i];
			if (tile.visibleInPanel)
			{
				if (tile.gaf.Predicate != Block.predicateThen)
				{
					int num2 = num / this.columns;
					int num3 = num % this.columns;
					float x = ((float)num3 + 0.25f) * (float)this.tileSize;
					float y = startingY - ((float)num2 - 0.5f) * (float)this.tileSize;
					tile.LocalMoveTo(x, y);
					num++;
				}
			}
		}
	}

	// Token: 0x06001C97 RID: 7319 RVA: 0x000CAC2A File Offset: 0x000C902A
	public void ClearLayout()
	{
		this.tileSections = null;
	}

	// Token: 0x06001C98 RID: 7320 RVA: 0x000CAC34 File Offset: 0x000C9034
	public void Layout()
	{
		if (this.tiles == null || this.tiles.Count == 0)
		{
			return;
		}
		this.goShopButton.SetActive(this.showShopButton);
		this.UpdateTabTitleLabel();
		float num = (float)this.tileSize;
		num += Blocksworld.UI.QuickSelect.GetHeight() + (float)this.separatorSize * 1.5f;
		num += this.goTabTitleLabel.GetComponent<Renderer>().bounds.size.y * NormalizedScreen.pixelScale;
		this.tileSectionPositions = null;
		this.ClearTileSections();
		this.ClearCachedGAFs();
		this.tileSections = this.ComputeTileVisibility(this.tabBar.SelectedTab);
		this.visibleTiles.Clear();
		if (this.tileSections.Count == 0)
		{
			num += (float)this.separatorSize;
			this.panelIsEmpty = true;
		}
		else
		{
			this.tileSectionPositions = new float[this.tileSections.Count + 1];
			for (int i = 0; i < this.tileSections.Count; i++)
			{
				this.tileSectionPositions[i] = num - (float)this.tileSize - (float)this.separatorSize;
				num += (float)this.separatorSize;
				this.LayoutTileSection(this.tileSections[i], -num);
				num += (float)(this.RowCount(this.tileSections[i].Count) * this.tileSize);
				this.visibleTiles.AddRange(this.tileSections[i]);
			}
			this.panelIsEmpty = false;
			this.tileSectionPositions[this.tileSections.Count] = num;
		}
		if (this.showShopButton)
		{
			if (num < (float)NormalizedScreen.height - this.goShopButton.transform.localScale.y)
			{
				num = (float)NormalizedScreen.height - this.goShopButton.transform.localScale.y;
			}
			else
			{
				num -= (float)this.separatorSize;
			}
			this.goShopButton.transform.localPosition = new Vector3(this.goShopButton.transform.localPosition.x, -num, this.goShopButton.transform.localPosition.z);
			num += this.goShopButton.transform.localScale.y;
		}
		this.height = Mathf.Max(num, (float)NormalizedScreen.height);
		this.UpdatePosition();
		this.RefreshTileSections();
	}

	// Token: 0x06001C99 RID: 7321 RVA: 0x000CAEC4 File Offset: 0x000C92C4
	private void UpdateTabTitleLabel()
	{
		TextMesh component = this.goTabTitleLabel.GetComponent<TextMesh>();
		component.text = this.tabTitleStrings[(int)this.tabBar.SelectedTab];
	}

	// Token: 0x06001C9A RID: 7322 RVA: 0x000CAEF8 File Offset: 0x000C92F8
	public void SetScrollPosToNextSection()
	{
		if (this.tileSectionPositions != null)
		{
			float num = 0f;
			float num2 = base.GetScrollPos() + Blocksworld.UI.QuickSelect.GetHeight();
			float num3 = num2 - (float)NormalizedScreen.height;
			float num4 = num3 + (float)NormalizedScreen.height;
			for (int i = 1; i < this.tileSectionPositions.Length - 1; i++)
			{
				float num5 = this.tileSectionPositions[i];
				float num6 = this.tileSectionPositions[i + 1];
				if (num3 < num5 && num4 < num6)
				{
					num = num5;
					break;
				}
				if (num4 < num6)
				{
					num = num6;
					break;
				}
			}
			base.SetScrollPos(num + (float)NormalizedScreen.height - Blocksworld.UI.QuickSelect.GetHeight());
			this.SnapBackInsideBounds(true);
		}
	}

	// Token: 0x06001C9B RID: 7323 RVA: 0x000CAFC3 File Offset: 0x000C93C3
	public override bool Hit(Vector3 v)
	{
		return Blocksworld.UI.SidePanel.HitBuildPanel(v) || Blocksworld.UI.TabBar.Hit(v);
	}

	// Token: 0x06001C9C RID: 7324 RVA: 0x000CAFF0 File Offset: 0x000C93F0
	public override Tile HitTile(Vector3 pos, bool allowDisabledTiles = false)
	{
		for (int i = 0; i < this.visibleTiles.Count; i++)
		{
			Tile tile = this.visibleTiles[i];
			if (tile != null && tile.tileObject != null && tile.Hit(pos, allowDisabledTiles))
			{
				return tile;
			}
		}
		return null;
	}

	// Token: 0x06001C9D RID: 7325 RVA: 0x000CB04D File Offset: 0x000C944D
	internal bool IsBlockTabSelected()
	{
		return this.tabBar.SelectedTab == TabBarTabId.Blocks;
	}

	// Token: 0x06001C9E RID: 7326 RVA: 0x000CB05D File Offset: 0x000C945D
	internal bool IsModelTabSelected()
	{
		return this.tabBar.SelectedTab == TabBarTabId.Models;
	}

	// Token: 0x04001743 RID: 5955
	private const float buildTileZOffset = -1f;

	// Token: 0x04001744 RID: 5956
	private const float sizeDragHandle = 32f;

	// Token: 0x04001745 RID: 5957
	private float viewportOffset = 0.041f;

	// Token: 0x04001746 RID: 5958
	private float boundsPassingSpeed;

	// Token: 0x04001747 RID: 5959
	public int columns;

	// Token: 0x04001748 RID: 5960
	public bool showShopButton = true;

	// Token: 0x04001749 RID: 5961
	public GameObject goShopButton;

	// Token: 0x0400174A RID: 5962
	public bool ignoreShow;

	// Token: 0x0400174B RID: 5963
	private TabBar tabBar;

	// Token: 0x0400174C RID: 5964
	public int tileSize;

	// Token: 0x0400174D RID: 5965
	private int separatorSize;

	// Token: 0x0400174E RID: 5966
	public bool isPanelVisible;

	// Token: 0x0400174F RID: 5967
	private bool doLayoutOnShow;

	// Token: 0x04001750 RID: 5968
	private bool panelIsEmpty;

	// Token: 0x04001751 RID: 5969
	private GameObject goTabTitleLabel;

	// Token: 0x04001752 RID: 5970
	private HashSet<GAF> inventoryGAFs = new HashSet<GAF>();

	// Token: 0x04001753 RID: 5971
	private HashSet<GAF> customSignalGAFs = new HashSet<GAF>();

	// Token: 0x04001754 RID: 5972
	private HashSet<GAF> customVariableGAFs = new HashSet<GAF>();

	// Token: 0x04001755 RID: 5973
	private HashSet<int> inventoryBlockItemIds = new HashSet<int>();

	// Token: 0x04001756 RID: 5974
	private static HashSet<Predicate> predicateGAFsToDestroyOnRefresh;

	// Token: 0x04001757 RID: 5975
	private int customSignalSectionIndex;

	// Token: 0x04001758 RID: 5976
	private int customVariableSectionIndex;

	// Token: 0x04001759 RID: 5977
	private string[] tabTitleStrings = new string[]
	{
		"Blocks",
		"Models",
		"Props",
		"Colors",
		"Textures",
		"Blocksters",
		"Gear",
		"Action Blocks",
		"Actions",
		"Sound Effects"
	};

	// Token: 0x0400175A RID: 5978
	private string[] modelHelpMessages = new string[]
	{
		"Select a modified block or model to save it here.",
		"Connect this block to a model to save it here.",
		"Tap the block again to select the model to save it here.",
		"Tap the blue button above to save your model here."
	};

	// Token: 0x0400175B RID: 5979
	private float lastRefreshPos;

	// Token: 0x0400175C RID: 5980
	private float lastRefreshTime;

	// Token: 0x0400175D RID: 5981
	private List<Tile> visibleTiles = new List<Tile>();

	// Token: 0x0400175E RID: 5982
	private List<List<Tile>> tileSections;

	// Token: 0x0400175F RID: 5983
	private float[] tileSectionPositions;

	// Token: 0x02000262 RID: 610
	private enum ModelSelectionState
	{
		// Token: 0x04001761 RID: 5985
		None,
		// Token: 0x04001762 RID: 5986
		SingleBlock,
		// Token: 0x04001763 RID: 5987
		ConnectedSingleBlock,
		// Token: 0x04001764 RID: 5988
		PotentialModel
	}
}
