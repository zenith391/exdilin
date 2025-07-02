using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Blocks;
using UnityEngine;

namespace Gestures;

public class CreateTileDragGesture : BaseGesture
{
	private readonly BuildPanel _buildPanel;

	private Tile _dragTile;

	private Block _dragBlock;

	public bool raycastDragging;

	private static Tile origDragTile = null;

	private bool trashedModelIsSimple;

	private bool startedAsCopyBuffer;

	private Tile startModelTile;

	private string lastRandomHairPaint = string.Empty;

	private string lastRandomSkinPaint = string.Empty;

	private bool lastCharacterWasRandomOrig;

	private bool lastCharacterWasRandomAnim;

	private bool _refreshTilesOnDragEnd;

	private static HashSet<GAF> highlights = new HashSet<GAF>();

	[CompilerGenerated]
	private static Action f__mg_cache0;

	public CreateTileDragGesture(BuildPanel buildPanel)
	{
		_buildPanel = buildPanel;
		touchBeginWindow = 12f;
	}

	public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
	{
		if (origDragTile != null)
		{
			if (result == null)
			{
				result = new HashSet<GAF>();
			}
			result.UnionWith(highlights);
			result.Add(origDragTile.gaf);
		}
		return result;
	}

	public static Tile GetOriginalDragTile()
	{
		return origDragTile;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
		{
			EnterState(GestureState.Failed);
		}
		else if (CharacterEditor.Instance.InEditMode())
		{
			EnterState(GestureState.Failed);
		}
		else
		{
			if (base.gestureState == GestureState.Active)
			{
				return;
			}
			if (!TBox.dragBlockTween.IsFinished())
			{
				EnterState(GestureState.Failed);
				return;
			}
			bool flag = allTouches[0].Phase == TouchPhase.Began;
			bool flag2 = (allTouches[0].Phase == TouchPhase.Moved || allTouches[0].Phase == TouchPhase.Stationary) && (float)allTouches[0].moveFrameCount < touchBeginWindow;
			if (!flag && !flag2)
			{
				EnterState(GestureState.Failed);
				return;
			}
			Blocksworld.blocksworldCamera.SetCameraStill(still: true);
			Vector2 position = allTouches[0].Position;
			startModelTile = null;
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			bool flag3 = quickSelect != null && quickSelect.Hit(position);
			Tile tile = null;
			if (flag3 && !Tutorial.InTutorialOrPuzzle() && quickSelect.HitModel(position))
			{
				tile = quickSelect.CreateModelTile();
				quickSelect.ShowModelScarcity();
				startedAsCopyBuffer = true;
			}
			highlights.Clear();
			if (tile != null)
			{
				_dragTile = tile;
				origDragTile = _dragTile;
				_dragTile.Show(show: true);
				Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), oneShot: true, 0.2f);
				MoveDragTile(position);
				EnterState(GestureState.Active);
			}
			else if (_buildPanel.Hit(position) && !Blocksworld.scriptPanel.Hit(position))
			{
				Tile tile2 = _buildPanel.HitTile(position);
				if (tile2 != null && (tile2.IsCreate() || tile2.IsCreateModel()))
				{
					origDragTile = tile2;
					HideDragTile();
					_dragTile = tile2.Clone();
					Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), oneShot: true, 0.2f);
					_dragTile.Show(show: true);
					MoveDragTile(position);
					EnterState(GestureState.Active);
					startedAsCopyBuffer = tile2.IsCreateModel();
					if (startedAsCopyBuffer)
					{
						startModelTile = tile2;
					}
				}
				else
				{
					EnterState(GestureState.Possible);
				}
			}
			else if (Blocksworld.mouseBlock != null)
			{
				if (Blocksworld.mouseBlock.DisableBuildModeMove())
				{
					EnterState(GestureState.Failed);
					return;
				}
				_dragBlock = Blocksworld.mouseBlock;
				EnterState(GestureState.Tracking);
				Blocksworld.blocksworldCamera.StoreOrbitPos();
			}
			else
			{
				EnterState(GestureState.Failed);
			}
		}
	}

	private void HandleBlockToPanelTransition(Vector2 pos, bool isCopiedModel, bool isTrashedModel, bool isModelCreate, bool overBuildPanel, UIQuickSelect quickSelect)
	{
		if (!(!isCopiedModel && !isTrashedModel && !isModelCreate && _dragBlock != null && overBuildPanel) || (Blocksworld.selectedBunch == null && Blocksworld.selectedBlock == null) || (!raycastDragging && !Blocksworld.tBoxGesture.raycastDragging))
		{
			return;
		}
		HideDragTile();
		BlockGrouped blockGrouped = Blocksworld.selectedBlock as BlockGrouped;
		if (Blocksworld.modelCollection != null)
		{
			Blocksworld.modelCollection.RefreshScarcity();
		}
		int num = Blocksworld.clipboard.AvailableCopyPasteBufferCount();
		int num2 = Blocksworld.clipboard.AvailableScriptCount(Blocksworld.clipboard.scriptCopyPasteBuffer);
		if (blockGrouped == null && Blocksworld.selectedBlock != null && Blocksworld.selectedBlock.HasDefaultTiles())
		{
			_dragTile = new Tile(new GAF(Block.predicateCreate, Blocksworld.selectedBlock.BlockType()));
			if (lastCharacterWasRandomAnim && Blocksworld.selectedBlock.BlockType().StartsWith("Anim Character"))
			{
				_dragTile.gaf.Args[0] = "Anim Character Random";
			}
			else if (lastCharacterWasRandomOrig && Blocksworld.selectedBlock.BlockType().StartsWith("Character"))
			{
				_dragTile.gaf.Args[0] = "Character Random";
			}
			trashedModelIsSimple = true;
		}
		else if (blockGrouped != null && blockGrouped.group != null && blockGrouped.group.GetBlockList().TrueForAll((Block b) => b.HasDefaultTiles()))
		{
			trashedModelIsSimple = true;
			_dragTile = new Tile(blockGrouped.GetIconGaf());
		}
		else if (Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.blocks.TrueForAll((Block b) => b is BlockGrouped && ((BlockGrouped)b).group.GetBlocks().Length == Blocksworld.selectedBunch.blocks.Count && b.HasDefaultTiles()))
		{
			trashedModelIsSimple = true;
			_dragTile = new Tile(Blocksworld.selectedBunch.blocks[0].GetIconGaf());
		}
		else if (startModelTile != null)
		{
			_dragTile = startModelTile.Clone();
		}
		else
		{
			string iconName;
			if (Blocksworld.selectedBlock != null)
			{
				int num3 = Blocksworld.selectedBlock.BlockItemId();
				if (num3 > 0)
				{
					BlockItem blockItem = BlockItem.FindByID(Blocksworld.selectedBlock.BlockItemId());
					iconName = blockItem.IconName;
				}
				else
				{
					iconName = "Buttons/Trashed_Model";
				}
			}
			else
			{
				iconName = "Buttons/Trashed_Model";
			}
			TileObject tileObjectForIcon = Blocksworld.tilePool.GetTileObjectForIcon(iconName, enabled: true);
			tileObjectForIcon.Show();
			_dragTile = new Tile(tileObjectForIcon);
		}
		Blocksworld.clipboard.SetTrashedModelToSelection();
		if (Blocksworld.selectedBunch != null && !_dragTile.IsCreateModel())
		{
			ModelData modelData = Blocksworld.modelCollection.FindSimilarModel(Blocksworld.clipboard.modelTrashedBuffer);
			if (modelData != null)
			{
				_dragTile.Destroy();
				_dragTile = modelData.tile.Clone();
			}
		}
		_dragTile.Show(show: true);
		origDragTile = _dragTile;
		if (Blocksworld.selectedBunch != null)
		{
			Blocksworld.DestroyBunch(Blocksworld.selectedBunch);
			Sound.PlayOneShotSound("Destroy");
		}
		else
		{
			Sound.PlayDestroySound(new GAF(Block.predicateCreate, Blocksworld.selectedBlock.BlockType()), Blocksworld.selectedBlock);
			Blocksworld.DestroyBlock(Blocksworld.selectedBlock);
		}
		Blocksworld.Deselect(silent: true, updateTiles: false);
		_refreshTilesOnDragEnd = true;
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		Scarcity.UpdateInventory(updateTiles: false, dictionary);
		foreach (KeyValuePair<GAF, int> item in dictionary)
		{
			Scarcity.inventoryScales[item.Key] = 1.5f;
			highlights.Add(item.Key);
		}
		Blocksworld.UI.QuickSelect.UpdateTextureIconScarcity();
		if (Blocksworld.modelCollection != null)
		{
			List<ModelData> list = Blocksworld.modelCollection.RefreshScarcity();
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				Tile tile = list[num4].tile;
				if (tile != null)
				{
					int intArg = Util.GetIntArg(tile.gaf.Args, 0, -1);
					if (intArg >= 0)
					{
						GAF gAF = new GAF(Block.predicateCreateModel, intArg);
						Scarcity.inventoryScales[gAF] = 1.5f;
						highlights.Add(gAF);
					}
				}
			}
		}
		int num5 = Blocksworld.clipboard.AvailableCopyPasteBufferCount();
		int num6 = Blocksworld.clipboard.AvailableScriptCount(Blocksworld.clipboard.scriptCopyPasteBuffer);
		quickSelect.showModelScarcityBadge = num5 != num;
		quickSelect.showScriptScarcityBadge = num6 != num2;
		_refreshTilesOnDragEnd = true;
		_dragBlock = null;
		EnterState(GestureState.Active);
		if (!_buildPanel.Hit(pos))
		{
			_buildPanel.SnapBackInsideBounds(immediately: true);
		}
		Blocksworld.blocksworldCamera.RestoreOrbitPos();
	}

	private void HandlePanelToModelOrGroupTransition(Vector2 pos, List<Touch> allTouches, bool isCopiedModel, bool isModelCreate, bool isTrashedModel, bool isBlockGroupTemplate, UIQuickSelect quickSelect)
	{
		if (Blocksworld.clipboard == null || Blocksworld.modelCollection == null || quickSelect == null || _dragTile == null || _dragTile.tileObject == null)
		{
			BWLog.Warning("Failed precondition for panel to model or group transition");
			return;
		}
		int num = Blocksworld.clipboard.AvailableCopyPasteBufferCount();
		int num2 = Blocksworld.clipboard.AvailableScriptCount(Blocksworld.clipboard.scriptCopyPasteBuffer);
		Dictionary<GAF, int> d = Scarcity.CalculateWorldGafUsage();
		List<List<List<Tile>>> list = null;
		BlockGroupTemplate blockGroupTemplate = null;
		if (isCopiedModel)
		{
			list = Blocksworld.clipboard.modelCopyPasteBuffer;
		}
		else if (isModelCreate)
		{
			int num3 = (int)_dragTile.gaf.Args[0];
			if (num3 < Blocksworld.modelCollection.models.Count)
			{
				list = Blocksworld.modelCollection.models[num3].CreateModel();
			}
		}
		else if (isTrashedModel)
		{
			list = Blocksworld.clipboard.modelTrashedBuffer;
		}
		else if (isBlockGroupTemplate)
		{
			blockGroupTemplate = BlockGroups.GetBlockGroupTemplate((string)_dragTile.gaf.Args[0]);
			list = blockGroupTemplate.blockInfos;
		}
		if (list == null || list.Count == 0)
		{
			BWLog.Warning("Could not find a valid model to paste");
			return;
		}
		if (BW.Options.useScarcity())
		{
			Dictionary<GAF, int> missing = new Dictionary<GAF, int>();
			List<GAF> list2 = new List<GAF>();
			if (Blocksworld.clipboard.AvailablityCountForBlockList(list, missing, list2) == 0)
			{
				if (_dragTile != null)
				{
					_dragTile.Show(show: false);
				}
				_dragTile = null;
				for (int i = 0; i < allTouches.Count; i++)
				{
					allTouches[i].Phase = TouchPhase.Ended;
				}
				EnterState(GestureState.Ended);
				Sound.PlayOneShotSound("Destroy");
				string text = ((list2.Count != 0) ? "A world can only have one of these:" : "Missing items:");
				Blocksworld.UI.Dialog.ShowPasteFailInfo(missing, list2, "\nCould not create model.\n\n" + text);
				return;
			}
		}
		Blocksworld.bw.PasteModelToFinger(list, allTouches[0], addHistoryState: false, blockGroupTemplate);
		if (isBlockGroupTemplate && Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.blocks.Count > 0 && Blocksworld.selectedBunch.blocks[0] is BlockGrouped { group: not null } blockGrouped && blockGrouped.GroupRotateMainBlockOnPlacement())
		{
			Block mainBlockInGroup = blockGrouped.GetMainBlockInGroup();
			if (mainBlockInGroup == null || mainBlockInGroup.go == null)
			{
				BWLog.Warning("Main block in group was destroyed after pasting block template");
				return;
			}
			mainBlockInGroup.TBoxStartRotate();
			mainBlockInGroup.TBoxRotateTo(Quaternion.Euler(0f, 180f + 90f * Mathf.Floor(Blocksworld.cameraTransform.eulerAngles.y / 90f), 0f) * mainBlockInGroup.go.transform.rotation);
			mainBlockInGroup.TBoxStopRotate();
			TBox.FitToSelected();
		}
		TBox.StartMove(pos, TBox.MoveMode.Raycast);
		if (Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.blocks.Count > 0)
		{
			_dragBlock = Blocksworld.selectedBunch.blocks[0];
		}
		else if (Blocksworld.selectedBlock != null)
		{
			_dragBlock = Blocksworld.selectedBlock;
		}
		Dictionary<GAF, int> d2 = Scarcity.CalculateWorldGafUsage();
		Dictionary<GAF, int> dictionary = Scarcity.CompareGafUsages(d, d2);
		highlights.Clear();
		foreach (GAF key in dictionary.Keys)
		{
			Scarcity.inventoryScales[key] = 1.5f;
			highlights.Add(key);
		}
		List<ModelData> list3 = Blocksworld.modelCollection.RefreshScarcity();
		for (int j = 0; j < list3.Count; j++)
		{
			Scarcity.inventoryScales[list3[j].tile.gaf] = 1.5f;
			highlights.Add(list3[j].tile.gaf);
		}
		int num4 = Blocksworld.clipboard.AvailableCopyPasteBufferCount();
		int num5 = Blocksworld.clipboard.AvailableScriptCount(Blocksworld.clipboard.scriptCopyPasteBuffer);
		quickSelect.showModelScarcityBadge = num4 != num;
		quickSelect.showScriptScarcityBadge = num5 != num2;
		Scarcity.UpdateInventory();
		Blocksworld.UI.QuickSelect.UpdateTextureIconScarcity();
		_dragTile.Show(show: false);
		_dragTile = null;
		raycastDragging = true;
	}

	private void HandlePanelToBlockTransition(Vector2 pos)
	{
		if (Blocksworld.TileEnabled(_dragTile) || Tutorial.state == TutorialState.None)
		{
			string text = null;
			string text2 = null;
			bool flag = false;
			bool flag2 = false;
			if (Blocksworld.buildPanel.IsBlockTabSelected())
			{
				string text3 = (string)_dragTile.gaf.Args[0];
				flag = text3.EndsWith("Glue");
				if (Blocksworld.clipboard.autoPaintMode)
				{
					text = Blocksworld.clipboard.activePaint;
				}
				if (Blocksworld.clipboard.autoTextureMode)
				{
					string activeTexture = Blocksworld.clipboard.activeTexture;
					bool flag3 = true;
					if (MaterialTexture.IsPhysicMaterialTexture(activeTexture))
					{
						flag3 = MaterialTexture.CanMaterialTextureNonTerrain(activeTexture) && !Materials.IsNormalTerrainTexture(activeTexture);
					}
					if (flag3)
					{
						GAF gaf = new GAF("Block.TextureTo", Scarcity.GetNormalizedTexture(activeTexture), Vector3.zero);
						if (Scarcity.GetInventoryCount(gaf, Tutorial.state == TutorialState.None && Scarcity.inventory != null) != 0)
						{
							text2 = activeTexture;
						}
					}
				}
			}
			string stringArgSafe = Util.GetStringArgSafe(_dragTile.gaf.Args, 0, string.Empty);
			lastCharacterWasRandomAnim = _dragTile.gaf.Predicate == Block.predicateCreate && stringArgSafe == "Anim Character Random";
			lastCharacterWasRandomOrig = _dragTile.gaf.Predicate == Block.predicateCreate && stringArgSafe == "Character Random";
			if (lastCharacterWasRandomAnim)
			{
				_dragTile.gaf.Args[0] = ((UnityEngine.Random.Range(0, 2) % 2 != 1) ? "Anim Character Female" : "Anim Character Male");
			}
			else if (lastCharacterWasRandomOrig)
			{
				_dragTile.gaf.Args[0] = ((UnityEngine.Random.Range(0, 2) % 2 != 1) ? "Character Female" : "Character Male");
			}
			_dragBlock = Blocksworld.bw.AddNewBlock(_dragTile);
			if (_dragBlock is BlockAnimatedCharacter)
			{
				BlockAnimatedCharacter blockAnimatedCharacter = _dragBlock as BlockAnimatedCharacter;
				blockAnimatedCharacter.SetLimbsToDefaults();
			}
			if (text != null)
			{
				if (flag)
				{
					_dragBlock.PaintTo(text, permanent: true, 1);
				}
				else
				{
					_dragBlock.PaintToAllMeshes(text, permanent: true);
				}
			}
			if (text2 != null)
			{
				TileResultCode tileResultCode = ((!flag) ? _dragBlock.TextureTo(text2, Vector3.forward, permanent: true) : _dragBlock.TextureTo(text2, Vector3.forward, permanent: true, 1));
				flag2 = TileResultCode.True == tileResultCode;
			}
			if (Tutorial.state == TutorialState.CreateBlock)
			{
				Tutorial.AutoRotateBlock(_dragBlock);
			}
			if (!Tutorial.InTutorialOrPuzzle())
			{
				BlockCharacter blockCharacter = _dragBlock as BlockCharacter;
				if (_dragBlock is BlockCharacter || _dragBlock is BlockAnimatedCharacter)
				{
					string text4 = _dragBlock.BlockType();
					bool flag4 = false;
					flag4 |= text4.EndsWith("ale");
					if (flag4 | text4.EndsWith("Mini"))
					{
						List<string> list = new List<string>(Scarcity.FreePaints(text4, 0));
						string text5 = list[UnityEngine.Random.Range(0, list.Count)];
						_dragBlock.PaintTo(text5, permanent: true);
						if (_dragBlock is BlockCharacter)
						{
							_dragBlock.PaintTo(text5, permanent: true, 4);
							_dragBlock.PaintTo(text5, permanent: true, 5);
						}
						List<string> list2 = new List<string>(Scarcity.FreePaints(text4, 6));
						if (list2.Count > 1)
						{
							list2.Remove(text5);
						}
						if (text5 == lastRandomSkinPaint && list2.Count > 1)
						{
							list2.Remove(lastRandomHairPaint);
						}
						string paint = list2[UnityEngine.Random.Range(0, list2.Count)];
						_dragBlock.PaintTo(paint, permanent: true, 6);
						lastRandomHairPaint = paint;
						lastRandomSkinPaint = text5;
					}
				}
			}
			if (Options.LockTileOnNewBlocks)
			{
				List<Tile> list3 = new List<Tile>();
				list3.Add(new Tile(new GAF("Meta.Then")));
				list3.Add(new Tile(new GAF("Block.Locked")));
				_dragBlock.tiles.Insert(1, list3);
			}
			Scarcity.inventoryScales[_dragTile.gaf] = 1.5f;
			Scarcity.UpdateInventory();
			Sound.PlayCreateSound(_dragTile.gaf, script: false, _dragBlock);
			if (Blocksworld.selectedBunch != null)
			{
				Blocksworld.selectedBunch.Add(_dragBlock);
			}
			Blocksworld.Select(_dragBlock, silent: false, updateTiles: false);
			_refreshTilesOnDragEnd = true;
			Blocksworld.blocksworldCamera.RestoreOrbitDistance();
			TBox.StartMove(pos, TBox.MoveMode.Raycast);
			if (_dragTile != null)
			{
				_dragTile.Show(show: false);
			}
			_dragTile = null;
			raycastDragging = true;
			if (flag2)
			{
				Blocksworld.UI.QuickSelect.UpdateTextureIconScarcity();
			}
		}
		else
		{
			RestoreItems();
			Tutorial.Step();
			EnterState(GestureState.Ended);
		}
	}

	private void HandlePanelToBlockOrModelTransition(Vector2 pos, List<Touch> allTouches, bool isCopiedModel, bool isTrashedModel, bool overBuildPanel, bool inQuickSelect, bool isBlockGroupTemplate, bool isModelCreate, UIQuickSelect quickSelect)
	{
		if (_dragTile == null || overBuildPanel || inQuickSelect)
		{
			return;
		}
		highlights.Clear();
		if (Blocksworld.modelCollection != null)
		{
			Blocksworld.modelCollection.RefreshScarcity();
		}
		RaycastHit[] array = Physics.RaycastAll(Blocksworld.mainCamera.ScreenPointToRay(pos * NormalizedScreen.scale));
		bool flag = false;
		bool flag2 = Tutorial.InTutorial();
		Block block = null;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			GameObject gameObject = raycastHit.collider.gameObject;
			if (flag2 && gameObject == Tutorial.placementHelper)
			{
				flag = true;
			}
			Block block2 = BWSceneManager.FindBlock(gameObject);
			if (!(block2 is BlockSky) && Tutorial.DistanceOK(raycastHit.point) && Tutorial.RaycastTargetBlockOK(block2))
			{
				block = block2;
				break;
			}
		}
		if (block != null || flag)
		{
			if (_dragTile != null && (isCopiedModel || isTrashedModel || isBlockGroupTemplate || isModelCreate))
			{
				HandlePanelToModelOrGroupTransition(pos, allTouches, isCopiedModel, isModelCreate, isTrashedModel, isBlockGroupTemplate, quickSelect);
			}
			else
			{
				HandlePanelToBlockTransition(pos);
			}
		}
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		Vector2 position = allTouches[0].Position;
		bool overBuildPanel = _buildPanel.Hit(position);
		bool isCopiedModel = _dragTile != null && _dragTile.IsCopiedModel();
		bool isModelCreate = _dragTile != null && _dragTile.IsCreateModel();
		bool isTrashedModel = _dragTile != null && _dragTile.IsUIOnly();
		bool isBlockGroupTemplate = _dragTile != null && BlockGroups.IsBlockGroupCreateGAF(_dragTile.gaf);
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		bool inQuickSelect = quickSelect != null && quickSelect.Hit(position);
		HandleBlockToPanelTransition(position, isCopiedModel, isTrashedModel, isModelCreate, overBuildPanel, quickSelect);
		HandlePanelToBlockOrModelTransition(position, allTouches, isCopiedModel, isTrashedModel, overBuildPanel, inQuickSelect, isBlockGroupTemplate, isModelCreate, quickSelect);
		if (base.gestureState == GestureState.Active)
		{
			MoveDragBlock(position);
			MoveDragTile(position);
		}
	}

	public override void TouchesStationary(List<Touch> allTouches)
	{
		if (base.gestureState == GestureState.Active)
		{
			MoveDragBlock(allTouches[0].Position);
			MoveDragTile(allTouches[0].Position);
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (base.gestureState == GestureState.Active && allTouches[0].Phase == TouchPhase.Ended)
		{
			History.AddStateIfNecessary();
			if (_dragBlock != null)
			{
				Blocksworld.ScrollToFirstBlockSpecificTile(_dragBlock);
			}
			if (_dragTile != null && _dragTile.IsUIOnly() && !startedAsCopyBuffer && !Tutorial.InTutorialOrPuzzle() && !trashedModelIsSimple && Blocksworld.clipboard.modelTrashedBuffer.Count > 1 && (Blocksworld.modelCollection == null || !Blocksworld.modelCollection.ContainsSimilarModel(Blocksworld.clipboard.modelTrashedBuffer)))
			{
				string text = "Do you want to remove this model from your world?";
				UIDialog dialog = Blocksworld.UI.Dialog;
				string mainText = text;
				string buttonAText = "No";
				string buttonBText = "Yes";
				dialog.ShowGenericDialog(mainText, buttonAText, buttonBText, History.Undo, null);
			}
			RestoreItems();
			Tutorial.Step();
			EnterState(GestureState.Ended);
		}
		else if (allTouches[0].Phase == TouchPhase.Ended)
		{
			EnterState(GestureState.Cancelled);
		}
		Blocksworld.blocksworldCamera.SetCameraStill(still: false);
		if (Blocksworld.selectedBlock != null)
		{
			Blocksworld.blocksworldCamera.UpdateOrbitDistance(useMaxDist: true, Mathf.Max(20f, 20f + Util.MaxComponent(Blocksworld.selectedBlock.size)));
		}
		if (_refreshTilesOnDragEnd)
		{
			_refreshTilesOnDragEnd = false;
			Blocksworld.UpdateTiles();
		}
		Blocksworld.UI.QuickSelect.HideScarcity();
	}

	private void HideDragTile()
	{
		if (_dragTile != null)
		{
			_dragTile.Show(show: false);
		}
	}

	public override void Cancel()
	{
		RestoreItems();
		_refreshTilesOnDragEnd = false;
		Tutorial.Step();
		EnterState(GestureState.Cancelled);
	}

	public override void Reset()
	{
		_dragTile = null;
		_dragBlock = null;
		origDragTile = null;
		raycastDragging = false;
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		if (quickSelect != null)
		{
			quickSelect.showModelScarcityBadge = false;
		}
		EnterState(GestureState.Possible);
		highlights.Clear();
		_refreshTilesOnDragEnd = false;
	}

	public override string ToString()
	{
		string text = "none";
		if (_dragTile != null)
		{
			text = "tile";
		}
		else if (_dragBlock != null)
		{
			text = "block";
		}
		return string.Concat("CreateTileDrag(", text, ")(", base.gestureState, ")");
	}

	private void MoveDragTile(Vector2 pos)
	{
		if (_dragTile != null)
		{
			Vector3 pos2 = pos - NormalizedScreen.pixelScale * 0.5f * 80f * Vector2.one;
			pos2.z = -0.5f;
			_dragTile.MoveTo(pos2);
		}
	}

	private void MoveDragBlock(Vector2 pos)
	{
		if (_dragBlock != null)
		{
			TBox.ContinueMove(pos);
		}
	}

	private void RestoreItems()
	{
		if (base.gestureState != GestureState.Active)
		{
			return;
		}
		if (_dragTile != null)
		{
			_dragTile.Show(show: false);
		}
		if (_dragBlock != null && _dragBlock.go != null)
		{
			TBox.StopMove();
			if (_dragBlock == Blocksworld.selectedBlock && _dragBlock.SelectableTerrain())
			{
				Blocksworld.ShowSelectedBlockPanel();
			}
			if (startedAsCopyBuffer && startModelTile != null && Blocksworld.selectedBunch != null)
			{
				ModelUtils.CheckModelConflictInputGAFs(Blocksworld.selectedBunch.blocks, BWSceneManager.AllBlocks());
			}
		}
		trashedModelIsSimple = false;
		startedAsCopyBuffer = false;
	}
}
