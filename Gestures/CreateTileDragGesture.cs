using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Blocks;
using UnityEngine;

namespace Gestures
{
	// Token: 0x0200017C RID: 380
	public class CreateTileDragGesture : BaseGesture
	{
		// Token: 0x060015CF RID: 5583 RVA: 0x0009860B File Offset: 0x00096A0B
		public CreateTileDragGesture(BuildPanel buildPanel)
		{
			this._buildPanel = buildPanel;
			this.touchBeginWindow = 12f;
		}

		// Token: 0x060015D0 RID: 5584 RVA: 0x0009863B File Offset: 0x00096A3B
		public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
		{
			if (CreateTileDragGesture.origDragTile != null)
			{
				if (result == null)
				{
					result = new HashSet<GAF>();
				}
				result.UnionWith(CreateTileDragGesture.highlights);
				result.Add(CreateTileDragGesture.origDragTile.gaf);
			}
			return result;
		}

		// Token: 0x060015D1 RID: 5585 RVA: 0x00098671 File Offset: 0x00096A71
		public static Tile GetOriginalDragTile()
		{
			return CreateTileDragGesture.origDragTile;
		}

		// Token: 0x060015D2 RID: 5586 RVA: 0x00098678 File Offset: 0x00096A78
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (CharacterEditor.Instance.InEditMode())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (base.gestureState == GestureState.Active)
			{
				return;
			}
			if (!TBox.dragBlockTween.IsFinished())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			bool flag = allTouches[0].Phase == TouchPhase.Began;
			bool flag2 = (allTouches[0].Phase == TouchPhase.Moved || allTouches[0].Phase == TouchPhase.Stationary) && (float)allTouches[0].moveFrameCount < this.touchBeginWindow;
			if (!flag && !flag2)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			Blocksworld.blocksworldCamera.SetCameraStill(true);
			Vector2 position = allTouches[0].Position;
			this.startModelTile = null;
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			bool flag3 = quickSelect != null && quickSelect.Hit(position);
			Tile tile = null;
			if (flag3 && !Tutorial.InTutorialOrPuzzle() && quickSelect.HitModel(position, false))
			{
				tile = quickSelect.CreateModelTile();
				quickSelect.ShowModelScarcity();
				this.startedAsCopyBuffer = true;
			}
			CreateTileDragGesture.highlights.Clear();
			if (tile != null)
			{
				this._dragTile = tile;
				CreateTileDragGesture.origDragTile = this._dragTile;
				this._dragTile.Show(true);
				Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), true, 0.2f, 1f, false);
				this.MoveDragTile(position);
				base.EnterState(GestureState.Active);
			}
			else if (this._buildPanel.Hit(position) && !Blocksworld.scriptPanel.Hit(position))
			{
				Tile tile2 = this._buildPanel.HitTile(position, false);
				if (tile2 != null && (tile2.IsCreate() || tile2.IsCreateModel()))
				{
					CreateTileDragGesture.origDragTile = tile2;
					this.HideDragTile();
					this._dragTile = tile2.Clone();
					Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), true, 0.2f, 1f, false);
					this._dragTile.Show(true);
					this.MoveDragTile(position);
					base.EnterState(GestureState.Active);
					this.startedAsCopyBuffer = tile2.IsCreateModel();
					if (this.startedAsCopyBuffer)
					{
						this.startModelTile = tile2;
					}
				}
				else
				{
					base.EnterState(GestureState.Possible);
				}
			}
			else if (Blocksworld.mouseBlock != null)
			{
				if (Blocksworld.mouseBlock.DisableBuildModeMove())
				{
					base.EnterState(GestureState.Failed);
				}
				else
				{
					this._dragBlock = Blocksworld.mouseBlock;
					base.EnterState(GestureState.Tracking);
					Blocksworld.blocksworldCamera.StoreOrbitPos();
				}
			}
			else
			{
				base.EnterState(GestureState.Failed);
			}
		}

		// Token: 0x060015D3 RID: 5587 RVA: 0x0009894C File Offset: 0x00096D4C
		private void HandleBlockToPanelTransition(Vector2 pos, bool isCopiedModel, bool isTrashedModel, bool isModelCreate, bool overBuildPanel, UIQuickSelect quickSelect)
		{
			if (!isCopiedModel && !isTrashedModel && !isModelCreate && this._dragBlock != null && overBuildPanel && (Blocksworld.selectedBunch != null || Blocksworld.selectedBlock != null) && (this.raycastDragging || Blocksworld.tBoxGesture.raycastDragging))
			{
				this.HideDragTile();
				BlockGrouped blockGrouped = Blocksworld.selectedBlock as BlockGrouped;
				if (Blocksworld.modelCollection != null)
				{
					Blocksworld.modelCollection.RefreshScarcity();
				}
				int num = Blocksworld.clipboard.AvailableCopyPasteBufferCount();
				int num2 = Blocksworld.clipboard.AvailableScriptCount(Blocksworld.clipboard.scriptCopyPasteBuffer, null, null);
				if (blockGrouped == null && Blocksworld.selectedBlock != null && Blocksworld.selectedBlock.HasDefaultTiles())
				{
					this._dragTile = new Tile(new GAF(Block.predicateCreate, new object[]
					{
						Blocksworld.selectedBlock.BlockType()
					}));
					if (this.lastCharacterWasRandomAnim && Blocksworld.selectedBlock.BlockType().StartsWith("Anim Character"))
					{
						this._dragTile.gaf.Args[0] = "Anim Character Random";
					}
					else if (this.lastCharacterWasRandomOrig && Blocksworld.selectedBlock.BlockType().StartsWith("Character"))
					{
						this._dragTile.gaf.Args[0] = "Character Random";
					}
					this.trashedModelIsSimple = true;
				}
				else
				{
					if (blockGrouped != null && blockGrouped.group != null)
					{
						if (blockGrouped.group.GetBlockList().TrueForAll((Block b) => b.HasDefaultTiles()))
						{
							this.trashedModelIsSimple = true;
							this._dragTile = new Tile(blockGrouped.GetIconGaf());
							goto IL_2C0;
						}
					}
					if (Blocksworld.selectedBunch != null)
					{
						if (Blocksworld.selectedBunch.blocks.TrueForAll((Block b) => b is BlockGrouped && ((BlockGrouped)b).group.GetBlocks().Length == Blocksworld.selectedBunch.blocks.Count && b.HasDefaultTiles()))
						{
							this.trashedModelIsSimple = true;
							this._dragTile = new Tile(Blocksworld.selectedBunch.blocks[0].GetIconGaf());
							goto IL_2C0;
						}
					}
					if (this.startModelTile != null)
					{
						this._dragTile = this.startModelTile.Clone();
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
						TileObject tileObjectForIcon = Blocksworld.tilePool.GetTileObjectForIcon(iconName, true);
						tileObjectForIcon.Show();
						this._dragTile = new Tile(tileObjectForIcon);
					}
				}
				IL_2C0:
				Blocksworld.clipboard.SetTrashedModelToSelection();
				if (Blocksworld.selectedBunch != null && !this._dragTile.IsCreateModel())
				{
					ModelData modelData = Blocksworld.modelCollection.FindSimilarModel(Blocksworld.clipboard.modelTrashedBuffer);
					if (modelData != null)
					{
						this._dragTile.Destroy();
						this._dragTile = modelData.tile.Clone();
					}
				}
				this._dragTile.Show(true);
				CreateTileDragGesture.origDragTile = this._dragTile;
				if (Blocksworld.selectedBunch != null)
				{
					Blocksworld.DestroyBunch(Blocksworld.selectedBunch);
					Sound.PlayOneShotSound("Destroy", 1f);
				}
				else
				{
					Sound.PlayDestroySound(new GAF(Block.predicateCreate, new object[]
					{
						Blocksworld.selectedBlock.BlockType()
					}), Blocksworld.selectedBlock);
					Blocksworld.DestroyBlock(Blocksworld.selectedBlock);
				}
				Blocksworld.Deselect(true, false);
				this._refreshTilesOnDragEnd = true;
				Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
				Scarcity.UpdateInventory(false, dictionary);
				foreach (KeyValuePair<GAF, int> keyValuePair in dictionary)
				{
					Scarcity.inventoryScales[keyValuePair.Key] = 1.5f;
					CreateTileDragGesture.highlights.Add(keyValuePair.Key);
				}
				Blocksworld.UI.QuickSelect.UpdateTextureIconScarcity();
				if (Blocksworld.modelCollection != null)
				{
					List<ModelData> list = Blocksworld.modelCollection.RefreshScarcity();
					for (int i = 0; i < list.Count; i++)
					{
						Tile tile = list[i].tile;
						if (tile != null)
						{
							int intArg = Util.GetIntArg(tile.gaf.Args, 0, -1);
							if (intArg >= 0)
							{
								GAF gaf = new GAF(Block.predicateCreateModel, new object[]
								{
									intArg
								});
								Scarcity.inventoryScales[gaf] = 1.5f;
								CreateTileDragGesture.highlights.Add(gaf);
							}
						}
					}
				}
				int num4 = Blocksworld.clipboard.AvailableCopyPasteBufferCount();
				int num5 = Blocksworld.clipboard.AvailableScriptCount(Blocksworld.clipboard.scriptCopyPasteBuffer, null, null);
				quickSelect.showModelScarcityBadge = (num4 != num);
				quickSelect.showScriptScarcityBadge = (num5 != num2);
				this._refreshTilesOnDragEnd = true;
				this._dragBlock = null;
				base.EnterState(GestureState.Active);
				if (!this._buildPanel.Hit(pos))
				{
					this._buildPanel.SnapBackInsideBounds(true);
				}
				Blocksworld.blocksworldCamera.RestoreOrbitPos();
			}
		}

		// Token: 0x060015D4 RID: 5588 RVA: 0x00098EB0 File Offset: 0x000972B0
		private void HandlePanelToModelOrGroupTransition(Vector2 pos, List<Touch> allTouches, bool isCopiedModel, bool isModelCreate, bool isTrashedModel, bool isBlockGroupTemplate, UIQuickSelect quickSelect)
		{
			if (Blocksworld.clipboard == null || Blocksworld.modelCollection == null || quickSelect == null || this._dragTile == null || this._dragTile.tileObject == null)
			{
				BWLog.Warning("Failed precondition for panel to model or group transition");
				return;
			}
			int num = Blocksworld.clipboard.AvailableCopyPasteBufferCount();
			int num2 = Blocksworld.clipboard.AvailableScriptCount(Blocksworld.clipboard.scriptCopyPasteBuffer, null, null);
			Dictionary<GAF, int> d = Scarcity.CalculateWorldGafUsage(false, false);
			List<List<List<Tile>>> list = null;
			BlockGroupTemplate blockGroupTemplate = null;
			if (isCopiedModel)
			{
				list = Blocksworld.clipboard.modelCopyPasteBuffer;
			}
			else if (isModelCreate)
			{
				int num3 = (int)this._dragTile.gaf.Args[0];
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
				blockGroupTemplate = BlockGroups.GetBlockGroupTemplate((string)this._dragTile.gaf.Args[0]);
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
					if (this._dragTile != null)
					{
						this._dragTile.Show(false);
					}
					this._dragTile = null;
					for (int i = 0; i < allTouches.Count; i++)
					{
						allTouches[i].Phase = TouchPhase.Ended;
					}
					base.EnterState(GestureState.Ended);
					Sound.PlayOneShotSound("Destroy", 1f);
					string str = (list2.Count != 0) ? "A world can only have one of these:" : "Missing items:";
					Blocksworld.UI.Dialog.ShowPasteFailInfo(missing, list2, "\nCould not create model.\n\n" + str);
					return;
				}
			}
			Blocksworld.bw.PasteModelToFinger(list, allTouches[0], false, blockGroupTemplate);
			if (isBlockGroupTemplate && Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.blocks.Count > 0)
			{
				BlockGrouped blockGrouped = Blocksworld.selectedBunch.blocks[0] as BlockGrouped;
				if (blockGrouped != null && blockGrouped.group != null && blockGrouped.GroupRotateMainBlockOnPlacement())
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
			}
			TBox.StartMove(pos, TBox.MoveMode.Raycast);
			if (Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.blocks.Count > 0)
			{
				this._dragBlock = Blocksworld.selectedBunch.blocks[0];
			}
			else if (Blocksworld.selectedBlock != null)
			{
				this._dragBlock = Blocksworld.selectedBlock;
			}
			Dictionary<GAF, int> d2 = Scarcity.CalculateWorldGafUsage(false, false);
			Dictionary<GAF, int> dictionary = Scarcity.CompareGafUsages(d, d2, "d1", "d2");
			CreateTileDragGesture.highlights.Clear();
			foreach (GAF gaf in dictionary.Keys)
			{
				Scarcity.inventoryScales[gaf] = 1.5f;
				CreateTileDragGesture.highlights.Add(gaf);
			}
			List<ModelData> list3 = Blocksworld.modelCollection.RefreshScarcity();
			for (int j = 0; j < list3.Count; j++)
			{
				Scarcity.inventoryScales[list3[j].tile.gaf] = 1.5f;
				CreateTileDragGesture.highlights.Add(list3[j].tile.gaf);
			}
			int num4 = Blocksworld.clipboard.AvailableCopyPasteBufferCount();
			int num5 = Blocksworld.clipboard.AvailableScriptCount(Blocksworld.clipboard.scriptCopyPasteBuffer, null, null);
			quickSelect.showModelScarcityBadge = (num4 != num);
			quickSelect.showScriptScarcityBadge = (num5 != num2);
			Scarcity.UpdateInventory(true, null);
			Blocksworld.UI.QuickSelect.UpdateTextureIconScarcity();
			this._dragTile.Show(false);
			this._dragTile = null;
			this.raycastDragging = true;
		}

		// Token: 0x060015D5 RID: 5589 RVA: 0x0009939C File Offset: 0x0009779C
		private void HandlePanelToBlockTransition(Vector2 pos)
		{
			if (Blocksworld.TileEnabled(this._dragTile) || Tutorial.state == TutorialState.None)
			{
				string text = null;
				string text2 = null;
				bool flag = false;
				bool flag2 = false;
				if (Blocksworld.buildPanel.IsBlockTabSelected())
				{
					string text3 = (string)this._dragTile.gaf.Args[0];
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
							flag3 = (MaterialTexture.CanMaterialTextureNonTerrain(activeTexture) && !Materials.IsNormalTerrainTexture(activeTexture));
						}
						if (flag3)
						{
							GAF gaf = new GAF("Block.TextureTo", new object[]
							{
								Scarcity.GetNormalizedTexture(activeTexture),
								Vector3.zero
							});
							if (Scarcity.GetInventoryCount(gaf, Tutorial.state == TutorialState.None && Scarcity.inventory != null) != 0)
							{
								text2 = activeTexture;
							}
						}
					}
				}
				string stringArgSafe = Util.GetStringArgSafe(this._dragTile.gaf.Args, 0, string.Empty);
				this.lastCharacterWasRandomAnim = (this._dragTile.gaf.Predicate == Block.predicateCreate && stringArgSafe == "Anim Character Random");
				this.lastCharacterWasRandomOrig = (this._dragTile.gaf.Predicate == Block.predicateCreate && stringArgSafe == "Character Random");
				if (this.lastCharacterWasRandomAnim)
				{
					this._dragTile.gaf.Args[0] = ((UnityEngine.Random.Range(0, 2) % 2 != 1) ? "Anim Character Female" : "Anim Character Male");
				}
				else if (this.lastCharacterWasRandomOrig)
				{
					this._dragTile.gaf.Args[0] = ((UnityEngine.Random.Range(0, 2) % 2 != 1) ? "Character Female" : "Character Male");
				}
				this._dragBlock = Blocksworld.bw.AddNewBlock(this._dragTile, true, null, true);
				if (this._dragBlock is BlockAnimatedCharacter)
				{
					BlockAnimatedCharacter blockAnimatedCharacter = this._dragBlock as BlockAnimatedCharacter;
					blockAnimatedCharacter.SetLimbsToDefaults();
				}
				if (text != null)
				{
					if (flag)
					{
						this._dragBlock.PaintTo(text, true, 1);
					}
					else
					{
						this._dragBlock.PaintToAllMeshes(text, true);
					}
				}
				if (text2 != null)
				{
					TileResultCode tileResultCode;
					if (flag)
					{
						tileResultCode = this._dragBlock.TextureTo(text2, Vector3.forward, true, 1, false);
					}
					else
					{
						tileResultCode = this._dragBlock.TextureTo(text2, Vector3.forward, true, 0, false);
					}
					flag2 = (TileResultCode.True == tileResultCode);
				}
				if (Tutorial.state == TutorialState.CreateBlock)
				{
					Tutorial.AutoRotateBlock(this._dragBlock);
				}
				if (!Tutorial.InTutorialOrPuzzle())
				{
					BlockCharacter blockCharacter = this._dragBlock as BlockCharacter;
					if (this._dragBlock is BlockCharacter || this._dragBlock is BlockAnimatedCharacter)
					{
						string text4 = this._dragBlock.BlockType();
						bool flag4 = false;
						flag4 |= text4.EndsWith("ale");
						flag4 |= text4.EndsWith("Mini");
						if (flag4)
						{
							List<string> list = new List<string>(Scarcity.FreePaints(text4, 0));
							string text5 = list[UnityEngine.Random.Range(0, list.Count)];
							this._dragBlock.PaintTo(text5, true, 0);
							if (this._dragBlock is BlockCharacter)
							{
								this._dragBlock.PaintTo(text5, true, 4);
								this._dragBlock.PaintTo(text5, true, 5);
							}
							List<string> list2 = new List<string>(Scarcity.FreePaints(text4, 6));
							if (list2.Count > 1)
							{
								list2.Remove(text5);
							}
							if (text5 == this.lastRandomSkinPaint && list2.Count > 1)
							{
								list2.Remove(this.lastRandomHairPaint);
							}
							string paint = list2[UnityEngine.Random.Range(0, list2.Count)];
							this._dragBlock.PaintTo(paint, true, 6);
							this.lastRandomHairPaint = paint;
							this.lastRandomSkinPaint = text5;
						}
					}
				}
				if (Options.LockTileOnNewBlocks)
				{
					List<Tile> list3 = new List<Tile>();
					list3.Add(new Tile(new GAF("Meta.Then", new object[0])));
					list3.Add(new Tile(new GAF("Block.Locked", new object[0])));
					this._dragBlock.tiles.Insert(1, list3);
				}
				Scarcity.inventoryScales[this._dragTile.gaf] = 1.5f;
				Scarcity.UpdateInventory(true, null);
				Sound.PlayCreateSound(this._dragTile.gaf, false, this._dragBlock);
				if (Blocksworld.selectedBunch != null)
				{
					Blocksworld.selectedBunch.Add(this._dragBlock);
				}
				Blocksworld.Select(this._dragBlock, false, false);
				this._refreshTilesOnDragEnd = true;
				Blocksworld.blocksworldCamera.RestoreOrbitDistance();
				TBox.StartMove(pos, TBox.MoveMode.Raycast);
				if (this._dragTile != null)
				{
					this._dragTile.Show(false);
				}
				this._dragTile = null;
				this.raycastDragging = true;
				if (flag2)
				{
					Blocksworld.UI.QuickSelect.UpdateTextureIconScarcity();
				}
			}
			else
			{
				this.RestoreItems();
				Tutorial.Step();
				base.EnterState(GestureState.Ended);
			}
		}

		// Token: 0x060015D6 RID: 5590 RVA: 0x000998F8 File Offset: 0x00097CF8
		private void HandlePanelToBlockOrModelTransition(Vector2 pos, List<Touch> allTouches, bool isCopiedModel, bool isTrashedModel, bool overBuildPanel, bool inQuickSelect, bool isBlockGroupTemplate, bool isModelCreate, UIQuickSelect quickSelect)
		{
			if (this._dragTile != null && !overBuildPanel && !inQuickSelect)
			{
				CreateTileDragGesture.highlights.Clear();
				if (Blocksworld.modelCollection != null)
				{
					Blocksworld.modelCollection.RefreshScarcity();
				}
				RaycastHit[] array = Physics.RaycastAll(Blocksworld.mainCamera.ScreenPointToRay(pos * NormalizedScreen.scale));
				bool flag = false;
				bool flag2 = Tutorial.InTutorial();
				Block block = null;
				foreach (RaycastHit raycastHit in array)
				{
					GameObject gameObject = raycastHit.collider.gameObject;
					if (flag2 && gameObject == Tutorial.placementHelper)
					{
						flag = true;
					}
					Block block2 = BWSceneManager.FindBlock(gameObject, false);
					if (!(block2 is BlockSky) && Tutorial.DistanceOK(raycastHit.point) && Tutorial.RaycastTargetBlockOK(block2))
					{
						block = block2;
						break;
					}
				}
				if (block != null || flag)
				{
					if (this._dragTile != null && (isCopiedModel || isTrashedModel || isBlockGroupTemplate || isModelCreate))
					{
						this.HandlePanelToModelOrGroupTransition(pos, allTouches, isCopiedModel, isModelCreate, isTrashedModel, isBlockGroupTemplate, quickSelect);
					}
					else
					{
						this.HandlePanelToBlockTransition(pos);
					}
				}
			}
		}

		// Token: 0x060015D7 RID: 5591 RVA: 0x00099A44 File Offset: 0x00097E44
		public override void TouchesMoved(List<Touch> allTouches)
		{
			Vector2 position = allTouches[0].Position;
			bool overBuildPanel = this._buildPanel.Hit(position);
			bool isCopiedModel = this._dragTile != null && this._dragTile.IsCopiedModel();
			bool isModelCreate = this._dragTile != null && this._dragTile.IsCreateModel();
			bool isTrashedModel = this._dragTile != null && this._dragTile.IsUIOnly();
			bool isBlockGroupTemplate = this._dragTile != null && BlockGroups.IsBlockGroupCreateGAF(this._dragTile.gaf);
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			bool inQuickSelect = quickSelect != null && quickSelect.Hit(position);
			this.HandleBlockToPanelTransition(position, isCopiedModel, isTrashedModel, isModelCreate, overBuildPanel, quickSelect);
			this.HandlePanelToBlockOrModelTransition(position, allTouches, isCopiedModel, isTrashedModel, overBuildPanel, inQuickSelect, isBlockGroupTemplate, isModelCreate, quickSelect);
			if (base.gestureState == GestureState.Active)
			{
				this.MoveDragBlock(position);
				this.MoveDragTile(position);
			}
		}

		// Token: 0x060015D8 RID: 5592 RVA: 0x00099B45 File Offset: 0x00097F45
		public override void TouchesStationary(List<Touch> allTouches)
		{
			if (base.gestureState == GestureState.Active)
			{
				this.MoveDragBlock(allTouches[0].Position);
				this.MoveDragTile(allTouches[0].Position);
			}
		}

		// Token: 0x060015D9 RID: 5593 RVA: 0x00099B78 File Offset: 0x00097F78
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (base.gestureState == GestureState.Active && allTouches[0].Phase == TouchPhase.Ended)
			{
				History.AddStateIfNecessary();
				if (this._dragBlock != null)
				{
					Blocksworld.ScrollToFirstBlockSpecificTile(this._dragBlock);
				}
				if (this._dragTile != null && this._dragTile.IsUIOnly() && !this.startedAsCopyBuffer && !Tutorial.InTutorialOrPuzzle() && !this.trashedModelIsSimple && Blocksworld.clipboard.modelTrashedBuffer.Count > 1 && (Blocksworld.modelCollection == null || !Blocksworld.modelCollection.ContainsSimilarModel(Blocksworld.clipboard.modelTrashedBuffer)))
				{
					string text = "Do you want to remove this model from your world?";
					UIDialog dialog = Blocksworld.UI.Dialog;
					string mainText = text;
					string buttonAText = "No";
					string buttonBText = "Yes";
					if (CreateTileDragGesture.f__mg_cache0 == null)
					{
						CreateTileDragGesture.f__mg_cache0 = new Action(History.Undo);
					}
					dialog.ShowGenericDialog(mainText, buttonAText, buttonBText, CreateTileDragGesture.f__mg_cache0, null);
				}
				this.RestoreItems();
				Tutorial.Step();
				base.EnterState(GestureState.Ended);
			}
			else if (allTouches[0].Phase == TouchPhase.Ended)
			{
				base.EnterState(GestureState.Cancelled);
			}
			Blocksworld.blocksworldCamera.SetCameraStill(false);
			if (Blocksworld.selectedBlock != null)
			{
				Blocksworld.blocksworldCamera.UpdateOrbitDistance(true, Mathf.Max(20f, 20f + Util.MaxComponent(Blocksworld.selectedBlock.size)));
			}
			if (this._refreshTilesOnDragEnd)
			{
				this._refreshTilesOnDragEnd = false;
				Blocksworld.UpdateTiles();
			}
			Blocksworld.UI.QuickSelect.HideScarcity();
		}

		// Token: 0x060015DA RID: 5594 RVA: 0x00099D05 File Offset: 0x00098105
		private void HideDragTile()
		{
			if (this._dragTile != null)
			{
				this._dragTile.Show(false);
			}
		}

		// Token: 0x060015DB RID: 5595 RVA: 0x00099D1E File Offset: 0x0009811E
		public override void Cancel()
		{
			this.RestoreItems();
			this._refreshTilesOnDragEnd = false;
			Tutorial.Step();
			base.EnterState(GestureState.Cancelled);
		}

		// Token: 0x060015DC RID: 5596 RVA: 0x00099D3C File Offset: 0x0009813C
		public override void Reset()
		{
			this._dragTile = null;
			this._dragBlock = null;
			CreateTileDragGesture.origDragTile = null;
			this.raycastDragging = false;
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			if (quickSelect != null)
			{
				quickSelect.showModelScarcityBadge = false;
			}
			base.EnterState(GestureState.Possible);
			CreateTileDragGesture.highlights.Clear();
			this._refreshTilesOnDragEnd = false;
		}

		// Token: 0x060015DD RID: 5597 RVA: 0x00099D9C File Offset: 0x0009819C
		public override string ToString()
		{
			string text = "none";
			if (this._dragTile != null)
			{
				text = "tile";
			}
			else if (this._dragBlock != null)
			{
				text = "block";
			}
			return string.Concat(new object[]
			{
				"CreateTileDrag(",
				text,
				")(",
				base.gestureState,
				")"
			});
		}

		// Token: 0x060015DE RID: 5598 RVA: 0x00099E0C File Offset: 0x0009820C
		private void MoveDragTile(Vector2 pos)
		{
			if (this._dragTile == null)
			{
				return;
			}
			Vector3 pos2 = pos - NormalizedScreen.pixelScale * 0.5f * 80f * Vector2.one;
			pos2.z = -0.5f;
			this._dragTile.MoveTo(pos2, false);
		}

		// Token: 0x060015DF RID: 5599 RVA: 0x00099E65 File Offset: 0x00098265
		private void MoveDragBlock(Vector2 pos)
		{
			if (this._dragBlock == null)
			{
				return;
			}
			TBox.ContinueMove(pos, false);
		}

		// Token: 0x060015E0 RID: 5600 RVA: 0x00099E7C File Offset: 0x0009827C
		private void RestoreItems()
		{
			if (base.gestureState == GestureState.Active)
			{
				if (this._dragTile != null)
				{
					this._dragTile.Show(false);
				}
				if (this._dragBlock != null && this._dragBlock.go != null)
				{
					TBox.StopMove();
					if (this._dragBlock == Blocksworld.selectedBlock && this._dragBlock.SelectableTerrain())
					{
						Blocksworld.ShowSelectedBlockPanel();
					}
					if (this.startedAsCopyBuffer && this.startModelTile != null && Blocksworld.selectedBunch != null)
					{
						ModelUtils.CheckModelConflictInputGAFs(Blocksworld.selectedBunch.blocks, BWSceneManager.AllBlocks());
					}
				}
				this.trashedModelIsSimple = false;
				this.startedAsCopyBuffer = false;
			}
		}

		// Token: 0x04001100 RID: 4352
		private readonly BuildPanel _buildPanel;

		// Token: 0x04001101 RID: 4353
		private Tile _dragTile;

		// Token: 0x04001102 RID: 4354
		private Block _dragBlock;

		// Token: 0x04001103 RID: 4355
		public bool raycastDragging;

		// Token: 0x04001104 RID: 4356
		private static Tile origDragTile = null;

		// Token: 0x04001105 RID: 4357
		private bool trashedModelIsSimple;

		// Token: 0x04001106 RID: 4358
		private bool startedAsCopyBuffer;

		// Token: 0x04001107 RID: 4359
		private Tile startModelTile;

		// Token: 0x04001108 RID: 4360
		private string lastRandomHairPaint = string.Empty;

		// Token: 0x04001109 RID: 4361
		private string lastRandomSkinPaint = string.Empty;

		// Token: 0x0400110A RID: 4362
		private bool lastCharacterWasRandomOrig;

		// Token: 0x0400110B RID: 4363
		private bool lastCharacterWasRandomAnim;

		// Token: 0x0400110C RID: 4364
		private bool _refreshTilesOnDragEnd;

		// Token: 0x0400110D RID: 4365
		private static HashSet<GAF> highlights = new HashSet<GAF>();

		// Token: 0x04001110 RID: 4368
		[CompilerGenerated]
		private static Action f__mg_cache0;
	}
}
