using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class BuildPanel : Panel
{
	private enum ModelSelectionState
	{
		None,
		SingleBlock,
		ConnectedSingleBlock,
		PotentialModel
	}

	private const float buildTileZOffset = -1f;

	private const float sizeDragHandle = 32f;

	private float viewportOffset = 0.041f;

	private float boundsPassingSpeed;

	public int columns;

	public bool showShopButton = true;

	public GameObject goShopButton;

	public bool ignoreShow;

	private TabBar tabBar;

	public int tileSize;

	private int separatorSize;

	public bool isPanelVisible;

	private bool doLayoutOnShow;

	private bool panelIsEmpty;

	private GameObject goTabTitleLabel;

	private HashSet<GAF> inventoryGAFs = new HashSet<GAF>();

	private HashSet<GAF> customSignalGAFs = new HashSet<GAF>();

	private HashSet<GAF> customVariableGAFs = new HashSet<GAF>();

	private HashSet<int> inventoryBlockItemIds = new HashSet<int>();

	private static HashSet<Predicate> predicateGAFsToDestroyOnRefresh;

	private int customSignalSectionIndex;

	private int customVariableSectionIndex;

	private string[] tabTitleStrings = new string[10] { "Blocks", "Models", "Props", "Colors", "Textures", "Blocksters", "Gear", "Action Blocks", "Actions", "Sound Effects" };

	private string[] modelHelpMessages = new string[4] { "Select a modified block or model to save it here.", "Connect this block to a model to save it here.", "Tap the block again to select the model to save it here.", "Tap the blue button above to save your model here." };

	private float lastRefreshPos;

	private float lastRefreshTime;

	private List<Tile> visibleTiles = new List<Tile>();

	private List<List<Tile>> tileSections;

	private float[] tileSectionPositions;

	private IEnumerable<Tile> allTiles
	{
		get
		{
			for (int r = 0; r < tiles.Count; r++)
			{
				for (int i = 0; i < tiles[r].Count; i++)
				{
					yield return tiles[r][i];
				}
			}
		}
	}

	public BuildPanel(string name, int columns)
		: base(name, 20)
	{
		this.columns = columns;
		expanded = 200f;
		buttonOverlayEnabled = true;
		separatorSize = Mathf.Abs(base.padding);
		tileSize = base.size + base.margin;
		width = Blocksworld.UI.SidePanel.BuildPanelWidth();
		height = NormalizedScreen.height;
		CreateExtras();
		tabBar = new TabBar(this);
		tileObjectPool = new TilePool("BuildPanel", 128, TilePool.TileImageSource.Resources);
		useMeshBackground = false;
		ViewportWatchdog.AddListener(ViewportSizeDidChange);
		tiles = new List<List<Tile>>();
	}

	public TabBar GetTabBar()
	{
		return tabBar;
	}

	public Vector3 GetTileGridStartPosition()
	{
		float x = 0f - (float)base.padding;
		float y = base.padding;
		return base.position + new Vector3(x, y, 0f);
	}

	public void CreateInventoryTiles(HashSet<GAF> gafs)
	{
		inventoryGAFs = gafs;
		inventoryBlockItemIds = new HashSet<int>();
		for (int i = 0; i < 10; i++)
		{
			List<Tile> list = new List<Tile>();
			tiles.Add(list);
			List<List<int>> tilesInTabAsIDs = PanelSlots.GetTilesInTabAsIDs((TabBarTabId)i);
			if (tilesInTabAsIDs == null)
			{
				TabBarTabId tabBarTabId = (TabBarTabId)i;
				Debug.Log("No build panel tab data for tab " + tabBarTabId);
				continue;
			}
			for (int j = 0; j < tilesInTabAsIDs.Count; j++)
			{
				List<int> list2 = tilesInTabAsIDs[j];
				for (int k = 0; k < list2.Count; k++)
				{
					int num = list2[k];
					BlockItem blockItem = BlockItem.FindByID(num);
					GAF gAF = new GAF(blockItem);
					if (inventoryGAFs.Contains(gAF))
					{
						inventoryBlockItemIds.Add(num);
						Tile tile = CreateTileInPanel(gAF);
						tile.panelSection = j;
						list.Add(tile);
						if (gAF.Predicate == Block.predicateSendSignal)
						{
							customSignalSectionIndex = j + 1;
						}
						else if (gAF.Predicate == Block.predicateVariableCustomInt || gAF.Predicate == Block.predicateBlockVariableInt)
						{
							customVariableSectionIndex = j;
						}
					}
				}
			}
		}
	}

	public Tile FindTileMatching(GAF patternGaf)
	{
		if (patternGaf == null || tiles == null)
		{
			return null;
		}
		for (int i = 0; i < tiles.Count; i++)
		{
			for (int j = 0; j < tiles[i].Count; j++)
			{
				Tile tile = tiles[i][j];
				if (patternGaf.Matches(tile.gaf))
				{
					return tile;
				}
			}
		}
		return null;
	}

	public Tile FindTile(GAF gaf)
	{
		if (gaf == null || tiles == null)
		{
			return null;
		}
		for (int i = 0; i < tiles.Count; i++)
		{
			for (int j = 0; j < tiles[i].Count; j++)
			{
				Tile tile = tiles[i][j];
				if (gaf.Equals(tile.gaf))
				{
					return tile;
				}
			}
		}
		return null;
	}

	public Tile GetFirstTileInSection(int section)
	{
		if (section < tileSections.Count && tileSections[section].Count > 0)
		{
			return tileSections[section][0];
		}
		return null;
	}

	public void ClearCachedGAFs()
	{
		customSignalGAFs.Clear();
		customVariableGAFs.Clear();
	}

	public Tile AddTileToBuildPanel(GAF gaf, TabBarTabId tab, int section, int indexInSection = -1)
	{
		Tile tile = CreateTileInPanel(gaf);
		tile.panelSection = section;
		if (indexInSection < 0)
		{
			tiles[(int)tab].Add(tile);
		}
		else
		{
			List<Tile> list = tiles[(int)tab];
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

	public bool IsGAFInBuildPanel(GAF gaf)
	{
		if (!inventoryGAFs.Contains(gaf) && !customSignalGAFs.Contains(gaf))
		{
			return customVariableGAFs.Contains(gaf);
		}
		return true;
	}

	public bool IsBlockItemInInventory(int blockItemId)
	{
		return inventoryBlockItemIds.Contains(blockItemId);
	}

	public List<Tile> GetTilesInTab(TabBarTabId tab)
	{
		if ((int)tab < tiles.Count)
		{
			return tiles[(int)tab];
		}
		return null;
	}

	protected override float PanelYOffset()
	{
		return 0f - height;
	}

	private void CreateExtras()
	{
		goShopButton = UnityEngine.Object.Instantiate(Resources.Load("GUI/Prefab Panel Shop Button")) as GameObject;
		goShopButton.name = "Panel Shop Button";
		goShopButton.transform.parent = goParent.transform;
		goShopButton.transform.localScale = new Vector3(goShopButton.transform.localScale.x * NormalizedScreen.pixelScale, goShopButton.transform.localScale.y * NormalizedScreen.pixelScale, 1f);
		goShopButton.transform.localPosition = new Vector3(0.565f * goShopButton.transform.localScale.x, 0f, 0f);
		goShopButton.SetActive(showShopButton);
		goTabTitleLabel = UnityEngine.Object.Instantiate(Resources.Load("GUI/Prefab Tab Title Label")) as GameObject;
		goTabTitleLabel.name = "Build Panel Title";
		goTabTitleLabel.transform.parent = goParent.transform;
		goTabTitleLabel.transform.localScale = new Vector3(NormalizedScreen.pixelScale, NormalizedScreen.pixelScale, 1f);
		float num = Blocksworld.UI.QuickSelect.GetHeight() * NormalizedScreen.pixelScale * 1.1f;
		goTabTitleLabel.transform.localPosition = new Vector3(0.565f * goShopButton.transform.localScale.x, 0f - num, 0f);
	}

	private Vector3 CalculateSnapWithinBounds(Vector3 p)
	{
		float max = Mathf.Max((float)NormalizedScreen.height + 2.1f, height - 2.1f);
		p.y = Mathf.Clamp(p.y, (float)NormalizedScreen.height + 2.1f, max);
		return p;
	}

	public void PositionReset(bool hide = false)
	{
		pos = GetResetPosition();
		UpdatePosition();
	}

	public Vector3 GetResetPosition()
	{
		Vector3 vector = new Vector3(NormalizedScreen.width, NormalizedScreen.height, depth);
		return vector + Blocksworld.UI.SidePanel.GetBuildPanelTopLeftOffset() / NormalizedScreen.scale;
	}

	public override void Move(Vector3 delta)
	{
		base.Move(delta);
	}

	public void GhostVisibleTiles(bool ghost)
	{
		foreach (Tile allTile in allTiles)
		{
			if (allTile.IsShowing())
			{
				if (ghost)
				{
					allTile.tileObject.Disable();
				}
				else if (allTile.IsEnabled())
				{
					allTile.tileObject.Enable();
				}
				else
				{
					allTile.tileObject.Disable();
				}
			}
		}
	}

	public void ClearTileSections()
	{
		if (tileSections == null || tileSections.Count == 0)
		{
			return;
		}
		if (predicateGAFsToDestroyOnRefresh == null)
		{
			predicateGAFsToDestroyOnRefresh = new HashSet<Predicate>();
			predicateGAFsToDestroyOnRefresh.Add(Block.predicateSendCustomSignal);
			predicateGAFsToDestroyOnRefresh.Add(Block.predicateSendCustomSignalModel);
			predicateGAFsToDestroyOnRefresh.UnionWith(Block.customVariablePredicates);
		}
		for (int i = 0; i < tileSections.Count; i++)
		{
			for (int j = 0; j < tileSections[i].Count; j++)
			{
				Tile tile = tileSections[i][j];
				tile.Show(show: false);
				if (predicateGAFsToDestroyOnRefresh.Contains(tile.gaf.Predicate))
				{
					tile.DestroyPhysical();
				}
			}
		}
		tileSections = null;
	}

	private void RefreshTileSections()
	{
		if (tileSections == null || tileSections.Count == 0)
		{
			return;
		}
		for (int i = 0; i < tileSections.Count; i++)
		{
			for (int j = 0; j < tileSections[i].Count; j++)
			{
				Tile tile = tileSections[i][j];
				Vector3 vector = goParent.transform.position + tile.positionInPanel;
				if (tile.visibleInPanel && OnScreenTest(vector))
				{
					if (!tile.IsShowing())
					{
						tile.CreatePhysical();
						tile.tileObject.transform.parent = goParent.transform;
						tile.tileObject.SetScale(NormalizedScreen.pixelScale * Vector3.one);
					}
					tile.tileObject.MoveTo(vector);
				}
				else
				{
					tile.DestroyPhysical();
				}
			}
		}
		lastRefreshPos = pos.y;
		lastRefreshTime = Time.realtimeSinceStartup;
	}

	private bool OnScreenTest(Vector3 pos)
	{
		if (pos.x > -100f && pos.x < (float)(NormalizedScreen.width + 100) && pos.y > -100f)
		{
			return pos.y < (float)(NormalizedScreen.height + 100);
		}
		return false;
	}

	private void ViewportSizeDidChange()
	{
		separatorSize = Mathf.Abs(base.padding);
		tileSize = base.size + base.margin;
		width = Blocksworld.UI.SidePanel.BuildPanelWidth();
		PositionReset(!isPanelVisible);
		if (isPanelVisible)
		{
			Layout();
		}
		else
		{
			doLayoutOnShow = true;
		}
	}

	public override void UpdatePosition()
	{
		base.UpdatePosition();
		if (isPanelVisible && Mathf.Abs(pos.y - lastRefreshPos) > 50f && Time.realtimeSinceStartup - lastRefreshTime > 0.25f)
		{
			RefreshTileSections();
		}
	}

	public void SnapBackInsideBounds(bool immediately = false)
	{
		if (pos.y < (float)(NormalizedScreen.height + 2))
		{
			MoveTo(new Vector2(pos.x, NormalizedScreen.height + 2), immediately, snapBack: true);
		}
		else if (pos.y > height - 2f)
		{
			MoveTo(new Vector2(pos.x, height - 2f), immediately, snapBack: true);
		}
	}

	public override void EndTrackingTouch()
	{
		base.EndTrackingTouch();
		SnapBackInsideBounds();
	}

	public override bool IsWithinBounds()
	{
		if (pos.y >= (float)(NormalizedScreen.height - 2))
		{
			return pos.y <= height + 2f;
		}
		return false;
	}

	public bool AtTopLimit()
	{
		return pos.y < (float)(NormalizedScreen.height + 8);
	}

	public bool AtBottomLimit()
	{
		return pos.y > height - 8f;
	}

	public void UpdateInner()
	{
		if (!trackingTouch)
		{
			pos.y += smoothSpeed.y;
		}
		if (snappingBack)
		{
			smoothSpeed.y *= _snapBackMomentum;
		}
		else
		{
			smoothSpeed.y *= _momentum;
		}
		if (IsShowing())
		{
			UpdatePosition();
		}
		if (!IsWithinBounds() && wasWithinBounds && !trackingTouch && !snappingBack)
		{
			boundsPassingSpeed = smoothSpeed.magnitude;
		}
		if (!trackingTouch && !IsWithinBounds() && !snappingBack)
		{
			smoothSpeed = smoothSpeed / _momentum * Mathf.Pow(_snapBackMomentum, 1.5f);
			if (Math.Abs(smoothSpeed.magnitude) < boundsPassingSpeed / 2f)
			{
				SnapBackInsideBounds();
			}
		}
	}

	public void Update()
	{
		wasWithinBounds = IsWithinBounds();
		StepTutorialIfNecessary();
		float num = Time.deltaTime;
		int num2 = 0;
		while (num > 0f && num2 < 3)
		{
			UpdateInner();
			num -= 1f / 30f;
			num2++;
		}
		UpdateMessage();
	}

	private void UpdateMessage()
	{
		if (isPanelVisible && panelIsEmpty)
		{
			string messageStr = Blocksworld.UI.TabBar.GetNoDataMessageStr();
			if (tabBar.SelectedTab == TabBarTabId.Models)
			{
				ModelSelectionState modelSelectionState = ModelSelectionState.None;
				if (TBox.IsShowingModel())
				{
					modelSelectionState = ModelSelectionState.PotentialModel;
				}
				else if (TBox.IsShowing())
				{
					modelSelectionState = ((Blocksworld.selectedBlock == null || Blocksworld.selectedBlock.connectionTypes.Count <= 0) ? ModelSelectionState.SingleBlock : ModelSelectionState.ConnectedSingleBlock);
				}
				messageStr = modelHelpMessages[(int)modelSelectionState];
			}
			Blocksworld.UI.SidePanel.ShowPanelMessage(messageStr);
		}
		else
		{
			Blocksworld.UI.SidePanel.HidePanelMessage();
		}
	}

	public override void Show(bool show)
	{
		base.Show(show);
		goShopButton.SetActive(show && showShopButton);
		if (show)
		{
			goShopButton.GetComponent<MeshRenderer>().enabled = true;
			goTabTitleLabel.GetComponent<MeshRenderer>().enabled = true;
		}
		if (ignoreShow)
		{
			ignoreShow = false;
			return;
		}
		SnapBackInsideBounds(immediately: true);
		Vector3 resetPosition = GetResetPosition();
		resetPosition.y = pos.y;
		MoveTo(resetPosition, immediately: true);
		if (show && doLayoutOnShow)
		{
			doLayoutOnShow = false;
			Layout();
		}
		isPanelVisible = show;
	}

	public void SetTutorialMode(bool tutorialActive)
	{
		ShowShopButton(!tutorialActive);
		tabBar.SetActive(active: true);
	}

	public void ShowShopButton(bool show)
	{
		showShopButton = show;
		goShopButton.SetActive(showShopButton && IsShowing());
		Layout();
		PositionReset();
	}

	public void ResetState()
	{
		tabBar.Reset();
	}

	public void DisableTilesBut(Tile but)
	{
		foreach (Tile allTile in allTiles)
		{
			allTile.Enable(allTile == but);
		}
	}

	public void ClearAllTiles()
	{
		foreach (Tile allTile in allTiles)
		{
			allTile.Show(show: false);
			allTile.DestroyPhysical();
		}
		tiles = new List<List<Tile>>();
		tileSections = new List<List<Tile>>();
	}

	private void HideAllTiles()
	{
		foreach (Tile allTile in allTiles)
		{
			allTile.Show(show: false);
		}
	}

	private void ShowAllTiles()
	{
		foreach (Tile allTile in allTiles)
		{
			allTile.Show(show: true);
		}
	}

	private List<List<Tile>> ComputeTileVisibility(TabBarTabId selectedTab)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		List<Tile> list2 = tiles[(int)selectedTab];
		for (int i = 0; i < 10; i++)
		{
			TabBarTabId tabBarTabId = (TabBarTabId)i;
			if (tabBarTabId != selectedTab)
			{
				HideTilesInTab(tabBarTabId);
			}
		}
		Blocksworld.UpdateTilesForTab(selectedTab, list2);
		for (int j = 0; j < list2.Count; j++)
		{
			Tile tile = list2[j];
			if (tile.visibleInPanel && (Tutorial.InTutorialOrPuzzle() || !BW.Options.useScarcity() || Scarcity.GetExistsInInventory(tile.gaf)))
			{
				while (list.Count <= tile.panelSection)
				{
					list.Add(new List<Tile>());
				}
				List<Tile> list3 = list[tile.panelSection];
				list3.Add(list2[j]);
			}
		}
		if (selectedTab == TabBarTabId.Actions)
		{
			InsertCustomSignalTiles(list);
			InsertCustomVariableTiles(list);
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].Count == 0)
			{
				list.RemoveAt(num);
			}
		}
		return list;
	}

	private void InsertCustomSignalTiles(List<List<Tile>> sections)
	{
		customSignalGAFs.Clear();
		bool flag = Blocksworld.GetSelectedScriptBlock() != null;
		flag &= Blocksworld.customSignals.Count > 0;
		if (!(flag & (sections.Count > customSignalSectionIndex)))
		{
			return;
		}
		List<Tile> list = new List<Tile>();
		sections.Insert(customSignalSectionIndex, list);
		foreach (string customSignal in Blocksworld.customSignals)
		{
			GAF gAF = new GAF(Block.predicateSendCustomSignal, customSignal, 1f);
			GAF item = new GAF(Block.predicateSendCustomSignalModel, customSignal, 1f);
			customSignalGAFs.Add(gAF);
			customSignalGAFs.Add(item);
			Tile tile = CreateTileInPanel(gAF);
			tile.panelSection = customSignalSectionIndex;
			list.Add(tile);
			tile.Show(show: true);
			if (Blocksworld.enabledGAFs != null)
			{
				bool enabled = Blocksworld.enabledGAFs.Contains(tile.gaf);
				tile.Enable(enabled);
			}
		}
	}

	private void InsertCustomVariableTiles(List<List<Tile>> sections)
	{
		customVariableGAFs.Clear();
		Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
		bool flag = selectedScriptBlock != null;
		if (!(flag & (sections.Count > customVariableSectionIndex)))
		{
			return;
		}
		List<Tile> list = new List<Tile>();
		sections.Insert(customVariableSectionIndex, list);
		Tile tile = CreateTileInPanel(new GAF(Block.predicateVariableCustomInt, "*", 0));
		tile.panelSection = customVariableSectionIndex;
		list.Add(tile);
		tile.Show(show: true);
		if (Blocksworld.enabledGAFs != null)
		{
			bool enabled = Blocksworld.enabledGAFs.Contains(tile.gaf);
			tile.Enable(enabled);
		}
		Tile tile2 = CreateTileInPanel(new GAF(Block.predicateBlockVariableInt, "*", 0));
		tile2.panelSection = customVariableSectionIndex;
		list.Add(tile2);
		tile2.Show(show: true);
		if (Blocksworld.enabledGAFs != null)
		{
			bool enabled2 = Blocksworld.enabledGAFs.Contains(tile2.gaf);
			tile2.Enable(enabled2);
		}
		List<Tile> list2 = null;
		List<Tile> list3 = null;
		List<Tile> list4 = null;
		List<Tile> list5 = null;
		int num = customVariableSectionIndex + 1;
		int num2 = customVariableSectionIndex + 1;
		int num3 = customVariableSectionIndex + 1;
		int num4 = customVariableSectionIndex + 1;
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
		foreach (KeyValuePair<string, int> customIntVariable in Blocksworld.customIntVariables)
		{
			AddOperatorTile(customIntVariable.Key, Block.predicateVariableCustomInt, customIntVariable.Value, customVariableSectionIndex, list);
			foreach (Predicate globalVariableOperation in Block.globalVariableOperations)
			{
				int arg = Block.variablePredicateParamDefaults[globalVariableOperation];
				AddOperatorTile(customIntVariable.Key, globalVariableOperation, arg, num, list2);
			}
		}
		if (!Blocksworld.blockIntVariables.ContainsKey(selectedScriptBlock))
		{
			return;
		}
		foreach (KeyValuePair<string, int> item in Blocksworld.blockIntVariables[selectedScriptBlock])
		{
			AddOperatorTile(item.Key, Block.predicateBlockVariableInt, item.Value, customVariableSectionIndex, list);
			foreach (Predicate blockVariableOperation in Block.blockVariableOperations)
			{
				int arg2 = Block.variablePredicateParamDefaults[blockVariableOperation];
				AddOperatorTile(item.Key, blockVariableOperation, arg2, num3, list3);
			}
			foreach (KeyValuePair<string, int> customIntVariable2 in Blocksworld.customIntVariables)
			{
				AddVariableTile(item.Key, Block.predicateBlockVariableIntLoadGlobal, customIntVariable2.Key, num2, list4);
			}
			foreach (KeyValuePair<string, int> item2 in Blocksworld.blockIntVariables[selectedScriptBlock])
			{
				if (!(item2.Key != item.Key))
				{
					continue;
				}
				foreach (Predicate blockVariableOperationsOnOtherBlockVar in Block.blockVariableOperationsOnOtherBlockVars)
				{
					AddVariableTile(item.Key, blockVariableOperationsOnOtherBlockVar, item2.Key, num4, list5);
				}
			}
		}
		foreach (KeyValuePair<string, int> item3 in Blocksworld.blockIntVariables[selectedScriptBlock])
		{
			foreach (KeyValuePair<string, int> customIntVariable3 in Blocksworld.customIntVariables)
			{
				AddVariableTile(item3.Key, Block.predicateBlockVariableIntStoreGlobal, customIntVariable3.Key, num2, list4);
			}
		}
	}

	private void AddOperatorTile(string variable, Predicate pred, int arg, int sectionIndex, List<Tile> tileSection)
	{
		GAF gAF = new GAF(pred, variable, arg);
		customVariableGAFs.Add(gAF);
		Tile tile = CreateTileInPanel(gAF);
		tile.panelSection = sectionIndex;
		tileSection.Add(tile);
		tile.Show(show: true);
		if (Blocksworld.enabledGAFs != null)
		{
			bool enabled = Blocksworld.enabledGAFs.Contains(tile.gaf);
			tile.Enable(enabled);
		}
	}

	private void AddVariableTile(string variable, Predicate pred, string arg, int sectionIndex, List<Tile> tileSection)
	{
		GAF gAF = new GAF(pred, variable, arg);
		customVariableGAFs.Add(gAF);
		Tile tile = CreateTileInPanel(gAF);
		tile.panelSection = sectionIndex;
		tileSection.Add(tile);
		tile.Show(show: true);
		if (Blocksworld.enabledGAFs != null)
		{
			bool enabled = Blocksworld.enabledGAFs.Contains(tile.gaf);
			tile.Enable(enabled);
		}
	}

	private void HideTilesInTab(TabBarTabId tab)
	{
		if ((int)tab < tiles.Count)
		{
			List<Tile> list = tiles[(int)tab];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Show(show: false);
				list[i].DestroyPhysical();
			}
		}
	}

	private int RowCount(int elements)
	{
		if (elements == 0)
		{
			return 0;
		}
		return 1 + (elements - 1) / columns;
	}

	private void LayoutTileSection(List<Tile> contiguousTiles, float startingY)
	{
		if (goParent == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < contiguousTiles.Count; i++)
		{
			Tile tile = contiguousTiles[i];
			if (tile.visibleInPanel && tile.gaf.Predicate != Block.predicateThen)
			{
				int num2 = num / columns;
				int num3 = num % columns;
				float x = ((float)num3 + 0.25f) * (float)tileSize;
				float y = startingY - ((float)num2 - 0.5f) * (float)tileSize;
				tile.LocalMoveTo(x, y);
				num++;
			}
		}
	}

	public void ClearLayout()
	{
		tileSections = null;
	}

	public void Layout()
	{
		if (tiles == null || tiles.Count == 0)
		{
			return;
		}
		goShopButton.SetActive(showShopButton);
		UpdateTabTitleLabel();
		float num = tileSize;
		num += Blocksworld.UI.QuickSelect.GetHeight() + (float)separatorSize * 1.5f;
		num += goTabTitleLabel.GetComponent<Renderer>().bounds.size.y * NormalizedScreen.pixelScale;
		tileSectionPositions = null;
		ClearTileSections();
		ClearCachedGAFs();
		tileSections = ComputeTileVisibility(tabBar.SelectedTab);
		visibleTiles.Clear();
		if (tileSections.Count == 0)
		{
			num += (float)separatorSize;
			panelIsEmpty = true;
		}
		else
		{
			tileSectionPositions = new float[tileSections.Count + 1];
			for (int i = 0; i < tileSections.Count; i++)
			{
				tileSectionPositions[i] = num - (float)tileSize - (float)separatorSize;
				num += (float)separatorSize;
				LayoutTileSection(tileSections[i], 0f - num);
				num += (float)(RowCount(tileSections[i].Count) * tileSize);
				visibleTiles.AddRange(tileSections[i]);
			}
			panelIsEmpty = false;
			tileSectionPositions[tileSections.Count] = num;
		}
		if (showShopButton)
		{
			num = ((!(num < (float)NormalizedScreen.height - goShopButton.transform.localScale.y)) ? (num - (float)separatorSize) : ((float)NormalizedScreen.height - goShopButton.transform.localScale.y));
			goShopButton.transform.localPosition = new Vector3(goShopButton.transform.localPosition.x, 0f - num, goShopButton.transform.localPosition.z);
			num += goShopButton.transform.localScale.y;
		}
		height = Mathf.Max(num, NormalizedScreen.height);
		UpdatePosition();
		RefreshTileSections();
	}

	private void UpdateTabTitleLabel()
	{
		TextMesh component = goTabTitleLabel.GetComponent<TextMesh>();
		component.text = tabTitleStrings[(int)tabBar.SelectedTab];
	}

	public void SetScrollPosToNextSection()
	{
		if (tileSectionPositions == null)
		{
			return;
		}
		float num = 0f;
		float num2 = GetScrollPos() + Blocksworld.UI.QuickSelect.GetHeight();
		float num3 = num2 - (float)NormalizedScreen.height;
		float num4 = num3 + (float)NormalizedScreen.height;
		for (int i = 1; i < tileSectionPositions.Length - 1; i++)
		{
			float num5 = tileSectionPositions[i];
			float num6 = tileSectionPositions[i + 1];
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
		SetScrollPos(num + (float)NormalizedScreen.height - Blocksworld.UI.QuickSelect.GetHeight());
		SnapBackInsideBounds(immediately: true);
	}

	public override bool Hit(Vector3 v)
	{
		if (!Blocksworld.UI.SidePanel.HitBuildPanel(v))
		{
			return Blocksworld.UI.TabBar.Hit(v);
		}
		return true;
	}

	public override Tile HitTile(Vector3 pos, bool allowDisabledTiles = false)
	{
		for (int i = 0; i < visibleTiles.Count; i++)
		{
			Tile tile = visibleTiles[i];
			if (tile != null && tile.tileObject != null && tile.Hit(pos, allowDisabledTiles))
			{
				return tile;
			}
		}
		return null;
	}

	internal bool IsBlockTabSelected()
	{
		return tabBar.SelectedTab == TabBarTabId.Blocks;
	}

	internal bool IsModelTabSelected()
	{
		return tabBar.SelectedTab == TabBarTabId.Models;
	}
}
