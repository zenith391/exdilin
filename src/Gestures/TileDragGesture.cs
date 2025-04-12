using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures
{
	// Token: 0x0200018B RID: 395
	public class TileDragGesture : BaseGesture
	{
		// Token: 0x06001663 RID: 5731 RVA: 0x0009E2C5 File Offset: 0x0009C6C5
		public TileDragGesture(BuildPanel buildPanel, ScriptPanel scriptPanel)
		{
			this._scriptPanel = scriptPanel;
			this._buildPanel = buildPanel;
			this.touchBeginWindow = 12f;
			this.Reset();
		}

		// Token: 0x06001664 RID: 5732 RVA: 0x0009E304 File Offset: 0x0009C704
		public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
		{
			if (TileDragGesture.highlightGafs.Count > 0)
			{
				if (result == null)
				{
					result = new HashSet<GAF>();
				}
				foreach (GAF item in TileDragGesture.highlightGafs)
				{
					result.Add(item);
				}
			}
			return result;
		}

		// Token: 0x06001665 RID: 5733 RVA: 0x0009E380 File Offset: 0x0009C780
		public static Tile GetOriginalDragTile()
		{
			return TileDragGesture.origDragTile;
		}

		// Token: 0x06001666 RID: 5734 RVA: 0x0009E388 File Offset: 0x0009C788
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (allTouches.Count != 1)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			bool flag = allTouches[0].Phase == TouchPhase.Began;
			bool flag2 = allTouches[0].Phase == TouchPhase.Moved && (float)allTouches[0].moveFrameCount < this.touchBeginWindow;
			if (!flag && !flag2)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			Vector2 position = allTouches[0].Position;
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			bool flag3 = quickSelect != null && quickSelect.Hit(position);
			Tile tile = null;
			Tile tile2 = null;
			Tile tile3 = null;
			this.beginTouchPos = position;
			bool flag4 = Tutorial.InTutorial();
			bool flag5 = Tutorial.InTutorialOrPuzzle();
			bool flag6 = !flag4 || Tutorial.usingQuickSelectColorIcon;
			bool flag7 = !flag4 || Tutorial.usingQuickSelectTextureIcon;
			if (flag3 && (flag6 || flag7))
			{
				if (flag6 && quickSelect.HitPaint(position, false))
				{
					tile = quickSelect.CreatePaintTile();
				}
				else if (flag7 && quickSelect.HitTexture(position, false))
				{
					tile = quickSelect.CreateTextureTile();
					quickSelect.ShowTextureScarcity();
				}
				else if (!flag5 && quickSelect.HitScript(position, false))
				{
					tile = quickSelect.CreateScriptTile();
					quickSelect.ShowScriptScarcity();
				}
				this._selectedBlockOnStart = Blocksworld.selectedBlock;
				if (tile == null)
				{
					this._selectedTile = null;
					base.EnterState(GestureState.Active);
					return;
				}
			}
			else
			{
				tile2 = this._buildPanel.HitTile(position, false);
				tile3 = this._scriptPanel.HitTile(position, false);
			}
			if (tile2 == null && tile3 == null && tile == null)
			{
				base.EnterState(GestureState.Possible);
				return;
			}
			this.originPanelBlock = false;
			TileDragGesture.currentGafUsage = Scarcity.CalculateWorldGafUsage(false, false);
			TileDragGesture.startInventory = Scarcity.GetInventoryCopy();
			Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
			if (this._scriptPanel.Hit(position) && !flag3 && tile3 != null && !tile3.IsCreate() && !tile3.IsThen())
			{
				this._selectedTile = (this._mouseTile = tile3);
				TileDragGesture.origDragTile = this._mouseTile;
				selectedScriptBlock.RemoveTile(this._mouseTile);
				base.EnterState(GestureState.Active);
				this.originPanelBlock = true;
				this.wasOverBuildPanel = false;
				this.wasOverScriptPanel = true;
				TileDragGesture.highlightGafs.Add(this._mouseTile.gaf);
				Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), true, 0.2f, 1f, false);
			}
			else if (!this._scriptPanel.Hit(position) && !flag3 && tile2 != null && !tile2.IsCreate() && !tile2.IsCreateModel() && !tile2.IsThen())
			{
				this._mouseTile = tile2;
				Tile tile4 = null;
				GAF gaf = this._mouseTile.gaf;
				Predicate predicate = gaf.Predicate;
				EditableTileParameter editableParameter = predicate.EditableParameter;
				if (editableParameter != null && editableParameter.settings.overwriteGafArgumentInBuildPanel)
				{
					object[] args = predicate.ExtendArguments(gaf.Args, true);
					tile4 = new Tile(new GAF(gaf.Predicate, args));
				}
				if (tile4 == null)
				{
					this._selectedTile = this._mouseTile.Clone();
				}
				else
				{
					this._selectedTile = tile4;
				}
				this._selectedTile.Enable(true);
				TileDragGesture.origDragTile = this._mouseTile;
				Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), true, 0.2f, 1f, false);
				TileDragGesture.highlightGafs.Add(this._mouseTile.gaf);
				this.wasOverBuildPanel = true;
				this.wasOverScriptPanel = false;
				base.EnterState(GestureState.Active);
			}
			else
			{
				if (tile == null)
				{
					base.EnterState(GestureState.Failed);
					return;
				}
				this._selectedTile = tile;
				this.draggingScriptTile = this._selectedTile.IsScriptGear();
				this._selectedTile.Show(true);
				this._selectedTile.Enable(true);
				TileDragGesture.highlightGafs.Add(tile.gaf);
				Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), true, 0.2f, 1f, false);
				base.EnterState(GestureState.Active);
			}
			this.origSnapPos = this._scriptPanel.SnapTile(position, this._selectedTile);
			this._selectedTile.Show(true);
			this.MoveSelectedTile(position, false);
			bool flag8 = this._selectedTile.IsTexture();
			bool flag9 = this._selectedTile.IsPaint();
			if (!flag8 && !flag9)
			{
				Scarcity.PaintScarcityBadges();
			}
			this._possibleDuplicate = (this._scriptPanel.Hit(position) && tile3 != null);
			this._startTime = Time.time;
			this._appliedSkyBox = false;
		}

		// Token: 0x06001667 RID: 5735 RVA: 0x0009E8B8 File Offset: 0x0009CCB8
		public override void TouchesMoved(List<Touch> allTouches)
		{
			if (this._selectedTile == null)
			{
				return;
			}
			this._possibleDuplicate = false;
			TileIconManager.Instance.ClearNewLoadLimit();
			Vector2 position = allTouches[allTouches.Count - 1].Position;
			this.MoveSelectedTile(position, false);
			bool flag = Blocksworld.mouseBlock != Blocksworld.mouseBlockLast;
			bool flag2 = Blocksworld.mouseBlockNormal != Blocksworld.mouseBlockNormalLast;
			Block mouseBlock = Blocksworld.mouseBlock;
			Block mouseBlockLast = Blocksworld.mouseBlockLast;
			int num = 0;
			if (mouseBlock != null)
			{
				Vector3 vector;
				Vector3 vector2;
				num = mouseBlock.GetMeshIndexForRay(Blocksworld.mainCamera.ScreenPointToRay(new Vector3(position.x, position.y) * NormalizedScreen.scale), !flag && this.oldTexture == null && this._paintLast == null, out vector, out vector2);
			}
			bool flag3 = this.oldMeshIndex != num;
			bool flag4 = this._selectedTile.IsTexture();
			bool flag5 = this._selectedTile.IsPaint();
			bool flag6 = this._selectedTile.IsScriptGear();
			this.draggingScriptTile = flag6;
			bool flag7 = Tutorial.state != TutorialState.None && Tutorial.state != TutorialState.Puzzle;
			bool flag8 = !flag7 || Tutorial.state == TutorialState.Color;
			bool flag9 = !flag7 || Tutorial.state == TutorialState.Texture;
			if (this._selectedTile.IsSkyBox())
			{
				int num2 = (int)this._selectedTile.gaf.Args[0];
				if (mouseBlock == Blocksworld.worldSky)
				{
					if (!this._appliedSkyBox)
					{
						if (WorldEnvironmentManager.SkyBoxIndex() != num2)
						{
							WorldEnvironmentManager.SaveSkyBoxHistory();
						}
						WorldEnvironmentManager.ChangeSkyBoxPermanently(num2);
						this._appliedSkyBox = true;
					}
				}
				else if (mouseBlockLast == Blocksworld.worldSky)
				{
					WorldEnvironmentManager.RevertToPreviousSkyBox();
					this._appliedSkyBox = false;
				}
				else if (this._scriptPanel.Hit(position) && this._selectedBlockOnStart is BlockMaster)
				{
					return;
				}
				History.AddStateIfNecessary();
			}
			else if (mouseBlock != null && mouseBlockLast != null && this._paintLast != null && flag5 && !flag7 && !flag && mouseBlock.ContainsPaintableSubmeshes() && (!mouseBlock.isTerrain || mouseBlock.SelectableTerrain()))
			{
				mouseBlockLast.PaintTo(this._paintLast, true, this._paintMeshIndexLast);
				string paint = (string)this._selectedTile.gaf.Args[0];
				this.PaintBlock(mouseBlock, paint, position, false);
			}
			else if (mouseBlock != null && this.oldTexture != null && flag4 && !flag7 && !flag && flag3 && mouseBlock.ContainsPaintableSubmeshes() && (!mouseBlock.isTerrain || mouseBlock.SelectableTerrain()))
			{
				mouseBlock.TextureTo(this.oldTexture, this.oldTextureNormal, true, this.oldMeshIndex, false);
				this.oldTexture = null;
				this.oldMeshIndex = -1;
				this.oldTextureNormal = Vector3.zero;
				this.TextureBlock(mouseBlock, (string)this._selectedTile.gaf.Args[0], position, Blocksworld.mouseBlockNormal, false);
			}
			else if (flag5 && flag)
			{
				this._paintLastTime = Time.time;
				if (mouseBlockLast != null && this._paintLast != null && !flag7)
				{
					mouseBlockLast.PaintTo(this._paintLast, true, this._paintMeshIndexLast);
					if (mouseBlockLast.IsScaled())
					{
						mouseBlockLast.ScaleTo(mouseBlockLast.Scale(), true, true);
					}
					this._paintLast = null;
					this._paintMeshIndexLast = 0;
				}
				if (mouseBlock != null && flag8 && !this._scriptPanel.Hit(position) && !Blocksworld.locked.Contains(mouseBlock))
				{
					float delay = (!mouseBlock.isTerrain || mouseBlock.SelectableTerrain()) ? 0f : 0.5f;
					Blocksworld.bw.StartCoroutine(this.DelayedPaint(mouseBlock, (string)this._selectedTile.gaf.Args[0], delay, position));
				}
			}
			else if (flag4 && (flag || flag2))
			{
				this._textureLastTime = Time.time;
				if (mouseBlockLast != null && this.oldTexture != null && !flag7)
				{
					mouseBlockLast.TextureTo(Scarcity.GetNormalizedTexture(this.oldTexture), this.oldTextureNormal, true, this.oldMeshIndex, false);
					if (mouseBlockLast.IsScaled())
					{
						mouseBlockLast.ScaleTo(mouseBlockLast.Scale(), true, true);
					}
					this.oldTexture = null;
					this.oldMeshIndex = -1;
					this.oldTextureNormal = Vector3.zero;
				}
				if (mouseBlock != null && flag9 && !this._scriptPanel.Hit(position) && !Blocksworld.locked.Contains(mouseBlock))
				{
					float delay2 = (!mouseBlock.isTerrain || mouseBlock.SelectableTerrain()) ? 0f : 0.5f;
					Blocksworld.bw.StartCoroutine(this.DelayedTexture(mouseBlock, Scarcity.GetNormalizedTexture((string)this._selectedTile.gaf.Args[0]), Blocksworld.mouseBlockNormal, delay2, position));
				}
			}
			bool flag10 = this._buildPanel.Hit(position);
			bool flag11 = this._scriptPanel.Hit(position);
			if (flag6)
			{
				bool flag12 = false;
				bool flag13 = false;
				if (flag11 && !this.wasOverScriptPanel)
				{
					flag12 = true;
				}
				if (this.wasOverScriptPanel && !flag11)
				{
					flag13 = true;
				}
				if (!flag11)
				{
					bool flag14 = mouseBlock == null || (mouseBlock.isTerrain && !mouseBlock.SelectableTerrain()) || Blocksworld.editorSelectionLocked.Contains(mouseBlock) || mouseBlock.HasGroup("locked-model");
					if (this._selectedBlockOnStart == null)
					{
						if (flag14)
						{
							if (Blocksworld.selectedBlock != null)
							{
								Blocksworld.Deselect(true, true);
								flag13 = true;
							}
						}
						else if (mouseBlock != Blocksworld.selectedBlock)
						{
							if (Blocksworld.selectedBlock != null)
							{
								Blocksworld.Deselect(true, true);
							}
							Blocksworld.SelectBlock(mouseBlock, true, true);
							this.HideSelectedButtons();
							flag12 = true;
						}
					}
					else if (flag14 && Blocksworld.selectedBlock != this._selectedBlockOnStart)
					{
						Blocksworld.SelectBlock(this._selectedBlockOnStart, true, true);
						if (mouseBlock == this._selectedBlockOnStart)
						{
							flag12 = true;
						}
						else
						{
							flag13 = true;
						}
					}
					else if (!flag14 && mouseBlock != Blocksworld.selectedBlock)
					{
						if (Blocksworld.selectedBlock != null)
						{
							Blocksworld.Deselect(true, true);
						}
						Blocksworld.SelectBlock(mouseBlock, true, true);
						this.HideSelectedButtons();
						flag12 = true;
					}
					else if (mouseBlockLast != mouseBlock)
					{
						if (mouseBlock == this._selectedBlockOnStart)
						{
							flag12 = true;
						}
						if (mouseBlockLast == this._selectedBlockOnStart)
						{
							flag13 = true;
						}
					}
				}
				if (flag12)
				{
					Sound.PlayOneShotSound("Paste Target Found", 1f);
					this._scriptPanel.Highlight(true);
				}
				else if (flag13)
				{
					Sound.PlayOneShotSound("Paste Target Lost", 1f);
					this._scriptPanel.Highlight(false);
				}
			}
			if ((flag10 && !this.wasOverBuildPanel) || (flag11 && !this.wasOverScriptPanel))
			{
				Scarcity.UpdateScarcityBadges(TileDragGesture.highlightGafs, TileDragGesture.currentGafUsage, TileDragGesture.startInventory);
			}
			this.wasOverBuildPanel = flag10;
			this.wasOverScriptPanel = flag11;
		}

		// Token: 0x06001668 RID: 5736 RVA: 0x0009F038 File Offset: 0x0009D438
		private void HideSelectedButtons()
		{
			TBox.tileButtonMove.Hide();
			TBox.tileButtonRotate.Hide();
			TBox.tileButtonScale.Hide();
			TBox.tileLockedModelIcon.Hide();
			Blocksworld.UI.SidePanel.HideCopyModelButton();
			Blocksworld.UI.SidePanel.HideSaveModelButton();
		}

		// Token: 0x06001669 RID: 5737 RVA: 0x0009F08C File Offset: 0x0009D48C
		private void ToggleButton(Tile buttonTile)
		{
			string text = (string)buttonTile.gaf.Args[0];
			UIInputControl.ControlType controlType;
			UIInputControl.ControlVariant variant;
			if (UIInputControl.controlTypeFromString.TryGetValue(text, out controlType) && UIInputControl.controlVariantFromString.TryGetValue(text, out variant))
			{
				Blocksworld.UI.Controls.MapControlToVariant(controlType, variant);
			}
			foreach (Block block in BWSceneManager.AllBlocks())
			{
				foreach (List<Tile> list in block.tiles)
				{
					foreach (Tile tile in list)
					{
						if (tile.gaf.Predicate == Block.predicateButton)
						{
							string text2 = (string)tile.gaf.Args[0];
							UIInputControl.ControlType controlType2;
							if (UIInputControl.controlTypeFromString.TryGetValue(text2, out controlType2) && controlType2 == controlType && text2 != text)
							{
								tile.gaf = new GAF(Block.predicateButton, new object[]
								{
									text
								});
								if (tile.IsShowing())
								{
									tile.tileObject.Setup(tile.gaf, tile.IsEnabled());
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600166A RID: 5738 RVA: 0x0009F26C File Offset: 0x0009D66C
		public override void TouchesStationary(List<Touch> allTouches)
		{
			Vector2 position = allTouches[0].Position;
			this.MoveSelectedTile(position, false);
			if (this._possibleDuplicate && Time.time - this._startTime > 0.7f)
			{
				this._possibleDuplicate = false;
				Tile tile = this._selectedTile.Clone();
				Vector2 vector = this._scriptPanel.SnapTile(position, tile);
				int x = (int)vector.x;
				int y = (int)vector.y;
				Blocksworld.bw.InsertTile(Blocksworld.GetSelectedScriptBlock(), x, y, tile);
				Sound.PlayCreateSound(tile.gaf, true, null);
				this._scriptPanel.SavePositionForNextLayout();
				this._scriptPanel.AssignUnparentedTiles();
				this._scriptPanel.Layout();
				History.AddStateIfNecessary();
			}
		}

		// Token: 0x0600166B RID: 5739 RVA: 0x0009F330 File Offset: 0x0009D730
		public override void TouchesEnded(List<Touch> allTouches)
		{
			Vector2 position = allTouches[0].Position;
			if (allTouches[0].Phase != TouchPhase.Ended)
			{
				return;
			}
			if (this._selectedTile == null)
			{
				UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
				bool flag = false;
				Dictionary<GAF, int> missing = new Dictionary<GAF, int>();
				List<GAF> list = new List<GAF>();
				string caption = string.Empty;
				string str = "model";
				string str2 = "Missing items:";
				if (quickSelect != null)
				{
					if (quickSelect.HitModel(position, true))
					{
						flag = !Blocksworld.clipboard.HasEnoughScarcityForModel(missing, list);
					}
					else if (quickSelect.HitScript(position, true))
					{
						str = "script";
						flag = !Blocksworld.clipboard.HasEnoughScarcityForScript(missing, list);
					}
					else if (!Tutorial.InTutorialOrPuzzle() && !quickSelect.HitTexture(position, false) && quickSelect.HitTexture(position, true))
					{
						Tile tile = quickSelect.CreatePaintTile();
						Blocksworld.UI.Dialog.ShowZeroInventoryDialog(tile);
					}
					if (list.Count > 0)
					{
						str2 = "A world can only have one of these:";
					}
				}
				if (flag && (position - this.beginTouchPos).magnitude < 10f)
				{
					caption = "\nCould not paste " + str + ".\n\n" + str2;
					Blocksworld.UI.Dialog.ShowPasteFailInfo(missing, list, caption);
				}
				base.EnterState(GestureState.Ended);
				return;
			}
			bool flag2 = this._selectedTile.IsTexture();
			bool flag3 = this._selectedTile.IsPaint();
			bool flag4 = this._selectedTile.IsSfx();
			bool flag5 = this._buildPanel.Hit(position);
			if (flag3)
			{
				string paint = (string)this._selectedTile.gaf.Args[0];
				Blocksworld.clipboard.SetPaintColor(paint, true);
				if (Blocksworld.UI.QuickSelect.HitPaint(position, false))
				{
					Blocksworld.clipboard.autoPaintMode = !Blocksworld.clipboard.autoPaintMode;
				}
			}
			if (flag2)
			{
				string texture = (string)this._selectedTile.gaf.Args[0];
				Blocksworld.clipboard.SetTexture(texture, true);
				if (Blocksworld.UI.QuickSelect.HitTexture(position, false))
				{
					Blocksworld.clipboard.autoTextureMode = !Blocksworld.clipboard.autoTextureMode;
				}
			}
			if (flag4 && flag5 && (this._selectedTile.gaf.Args.Length < 3 || (string)this._selectedTile.gaf.Args[2] != "loop"))
			{
				Sound.PlayOneShotSound((string)this._selectedTile.gaf.Args[0], 1f);
			}
			bool flag6 = this._selectedTile.IsScriptGear();
			bool flag7 = this._selectedTile.IsSkyBox();
			bool flag8 = false;
			Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
			bool flag9 = !flag7 || selectedScriptBlock is BlockMaster;
			bool flag10;
			if (flag6 && selectedScriptBlock != null)
			{
				if (this._scriptPanel.Hit(this._selectedTile.tileObject.GetPosition()) || Blocksworld.mouseBlock == Blocksworld.selectedBlock)
				{
					Blocksworld.bw.PasteScriptFromClipboard(selectedScriptBlock);
					flag8 = true;
				}
				this._selectedTile.Show(false);
				flag10 = true;
				this._scriptPanel.Highlight(false);
			}
			else if (selectedScriptBlock != null && this._scriptPanel.IsShowing() && flag9)
			{
				if (flag7)
				{
					this._selectedTile.gaf = new GAF(BlockMaster.predicateSkyBoxTo, this._selectedTile.gaf.Args);
				}
				Vector2 vector = this._scriptPanel.SnapTile(position, this._selectedTile);
				int x = (int)vector.x;
				int num = (int)vector.y;
				bool flag11 = num != 0;
				if (flag11)
				{
					Tile tile2 = this._selectedTile.Clone();
					tile2.MoveTo(this._selectedTile.GetPosition(), false);
					if (tile2.gaf.Predicate.EditableParameter != null)
					{
						tile2.gaf.Predicate.EditableParameter.ApplyTileParameterUI(tile2);
					}
					tile2.Show(true);
					this._selectedTile.Show(false);
					flag10 = true;
					Blocksworld.selectedTile = tile2;
					if (tile2.gaf.Predicate == Block.predicateSendCustomSignal && (string)tile2.gaf.Args[0] == "*")
					{
						string text = "Signal";
						Predicate predicate = Block.predicateSendCustomSignal;
						if (Tutorial.InTutorial())
						{
							text = Tutorial.GetTargetSignalName();
							predicate = Tutorial.GetTargetSignalPredicate();
						}
						tile2.gaf = new GAF(predicate, new object[]
						{
							text,
							1f
						});
						tile2.Show(false);
						tile2.Show(true);
						this._scriptPanel.UpdateOverlay(tile2);
						if (!Tutorial.InTutorial())
						{
							Blocksworld.bw.tileParameterEditor.StartEditing(Blocksworld.selectedTile, new SignalNameTileParameter(0));
						}
					}
					else if (!this.originPanelBlock && !Tutorial.InTutorial() && Blocksworld.selectedTile.gaf.Predicate.EditableParameter != null && Blocksworld.selectedTile.gaf.Predicate.EditableParameter.settings.autoOpenOnNewTiles)
					{
						Blocksworld.bw.tileParameterEditor.StartEditing(Blocksworld.selectedTile, Blocksworld.selectedTile.gaf.Predicate.EditableParameter);
					}
					else if (!Tutorial.InTutorial())
					{
						if (tile2.gaf.Predicate == Block.predicateVariableCustomInt && (string)tile2.gaf.Args[0] == "*")
						{
							string text2 = "Int";
							tile2.gaf = new GAF(tile2.gaf.Predicate, new object[]
							{
								text2,
								0
							});
							tile2.Show(false);
							tile2.Show(true);
							this._scriptPanel.UpdateOverlay(tile2);
							Blocksworld.bw.tileParameterEditor.StartEditing(Blocksworld.selectedTile, new VariableNameTileParameter(0));
						}
						else if (tile2.gaf.Predicate == Block.predicateBlockVariableInt && (string)tile2.gaf.Args[0] == "*")
						{
							string text3 = Blocksworld.NextAvailableBlockVariableName(selectedScriptBlock);
							tile2.gaf = new GAF(tile2.gaf.Predicate, new object[]
							{
								text3,
								0
							});
							tile2.Show(false);
							tile2.Show(true);
							this._scriptPanel.UpdateOverlay(tile2);
						}
					}
					Blocksworld.bw.InsertTile(selectedScriptBlock, x, num, tile2);
					Sound.PlayCreateSound(tile2.gaf, true, null);
					this._scriptPanel.SavePositionForNextLayout();
					int num2 = selectedScriptBlock.AddOrRemoveEmptyScriptLine();
					this._scriptPanel.AssignUnparentedTiles();
					this._scriptPanel.Layout();
					if (this.wasOverScriptPanel && !this.snappedSomewhereElse && this._scriptPanel.HitTile(position, false) == tile2)
					{
						bool flag12 = !tile2.gaf.Predicate.CanEditTile(tile2) || (Blocksworld.selectedTile != null && Blocksworld.selectedTile.Equals(tile2));
						if (flag12)
						{
							GAF nextUnlocked = TileToggleChain.GetNextUnlocked(tile2.gaf);
							if (nextUnlocked != null && !tile2.gaf.Equals(nextUnlocked))
							{
								tile2.gaf = nextUnlocked;
								tile2.Show(true);
								tile2.tileObject.Setup(nextUnlocked, true);
								if (tile2.gaf.Predicate == Block.predicateButton)
								{
									this.ToggleButton(tile2);
								}
							}
						}
						Blocksworld.BlockPanelTileTapped(tile2);
					}
					this.ReplaceTile(selectedScriptBlock, tile2);
				}
				else
				{
					this._selectedTile.SetParent(null);
					this._selectedTile.Show(false);
					flag10 = true;
					this._scriptPanel.SavePositionForNextLayout();
					int num3 = selectedScriptBlock.AddOrRemoveEmptyScriptLine();
					this._scriptPanel.Layout();
				}
				if (!this.originPanelBlock || !this._scriptPanel.Hit(position))
				{
					Blocksworld.UpdateTiles();
				}
				Sound.PlayCreateSound(this._selectedTile.gaf, true, null);
			}
			else
			{
				this._selectedTile.Show(false);
				flag10 = true;
			}
			this._paintBefore = null;
			Block mouseBlock = Blocksworld.mouseBlock;
			if (flag10 && !this._buildPanel.Hit(position) && (this._scriptPanel.IsShowing() || !this._scriptPanel.Hit(position)) && (!flag3 || (mouseBlock != null && mouseBlock.isTerrain)) && (!flag2 || (mouseBlock != null && mouseBlock.isTerrain)) && !flag8)
			{
				Sound.PlaySound("Tile Drop", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
			}
			if (flag10)
			{
				this._selectedTile.Destroy();
				this._selectedTile = null;
			}
			this.draggingScriptTile = false;
			Tutorial.Step();
			History.AddStateIfNecessary();
			base.EnterState(GestureState.Ended);
			TileDragGesture.highlightGafs.Clear();
			Scarcity.UpdateInventory(true, null);
			Blocksworld.UI.QuickSelect.HideScarcity();
			TBox.UpdateCopyButtonVisibility();
		}

		// Token: 0x0600166C RID: 5740 RVA: 0x0009FCEC File Offset: 0x0009E0EC
		private void ReplaceTile(Block scriptBlock, Tile tile)
		{
			GAF gaf = tile.gaf;
			GAF gaf2 = gaf;
			if (scriptBlock is BlockMaster)
			{
				gaf2 = BlockMaster.ReplaceGaf(gaf);
			}
			if (tile.gaf.Predicate == Block.predicateButton)
			{
				string key = (string)tile.gaf.Args[0];
				UIInputControl.ControlType control;
				if (UIInputControl.controlTypeFromString.TryGetValue(key, out control))
				{
					UIInputControl.ControlVariant controlVariant = Blocksworld.UI.Controls.GetControlVariant(control);
					string text = control.ToString();
					if (controlVariant != UIInputControl.ControlVariant.Default)
					{
						text = text + " " + controlVariant.ToString();
					}
					gaf2 = new GAF(Block.predicateButton, new object[]
					{
						text
					});
				}
			}
			if (gaf2 != gaf)
			{
				tile.gaf = gaf2;
				if (gaf2.Predicate.EditableParameter != null)
				{
					tile.subParameterCount = gaf2.Predicate.EditableParameter.subParameterCount;
				}
				tile.Show(true);
				tile.tileObject.Setup(gaf2, tile.IsEnabled());
			}
		}

		// Token: 0x0600166D RID: 5741 RVA: 0x0009FDF4 File Offset: 0x0009E1F4
		public override void Cancel()
		{
			base.EnterState(GestureState.Cancelled);
			if (this._selectedTile != null)
			{
				this._selectedTile.Show(false);
				this._selectedTile.Destroy();
				this._selectedTile = null;
			}
		}

		// Token: 0x0600166E RID: 5742 RVA: 0x0009FE28 File Offset: 0x0009E228
		public override void Reset()
		{
			base.EnterState(GestureState.Possible);
			this.draggingScriptTile = false;
			if (this._selectedTile != null)
			{
				this._selectedTile.Show(false);
				this._selectedTile.Destroy();
				this._selectedTile = null;
				Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
				if (selectedScriptBlock != null && this._scriptPanel.IsShowing())
				{
					int num = selectedScriptBlock.AddOrRemoveEmptyScriptLine();
					this._scriptPanel.AssignUnparentedTiles();
					this._scriptPanel.Layout();
				}
			}
			this._mouseTile = null;
			this._selectedTile = null;
			this._paintLast = null;
			this._paintLastTime = Time.time;
			this._paintMeshIndexLast = 0;
			this.oldTexture = null;
			this.oldTextureNormal = Vector3.zero;
			this._textureLastTime = Time.time;
			this.oldMeshIndex = -1;
			this.origSnapPos = Vector2.zero;
			this.snappedSomewhereElse = false;
			TileDragGesture.origDragTile = null;
			TileDragGesture.highlightGafs.Clear();
			this._possibleDuplicate = false;
			this._selectedBlockOnStart = null;
			this._scriptPanel.Highlight(false);
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			if (quickSelect != null)
			{
				quickSelect.showScriptScarcityBadge = false;
			}
		}

		// Token: 0x0600166F RID: 5743 RVA: 0x0009FF4A File Offset: 0x0009E34A
		public override string ToString()
		{
			return "TileDrag";
		}

		// Token: 0x06001670 RID: 5744 RVA: 0x0009FF54 File Offset: 0x0009E354
		private Vector2 MoveSelectedTile(Vector2 pos, bool first = false)
		{
			Vector2 vector = Vector2.zero;
			if (this._selectedTile == null)
			{
				return vector;
			}
			Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
			bool flag = !this._selectedTile.IsScriptGear();
			flag &= (!this._selectedTile.IsSkyBox() || selectedScriptBlock is BlockMaster);
			if (this._scriptPanel.IsShowing() && flag)
			{
				vector = this._scriptPanel.SnapTile(pos, this._selectedTile);
				if (!first && (vector - this.origSnapPos).sqrMagnitude > 0.01f)
				{
					this.snappedSomewhereElse = true;
				}
			}
			if (selectedScriptBlock == null || !this._scriptPanel.IsShowing() || vector == Vector2.zero)
			{
				this._selectedTile.MoveTo(pos.x - 40f, pos.y - 40f, -0.5f);
			}
			return vector;
		}

		// Token: 0x06001671 RID: 5745 RVA: 0x000A0054 File Offset: 0x0009E454
		private void PaintBlock(Block block, string paint, Vector2 screenPos, bool first)
		{
			bool flag = Tutorial.InTutorialOrPuzzle();
			GAF gaf = new GAF("Block.PaintTo", new object[]
			{
				paint
			});
			if (Scarcity.GetInventoryCount(gaf, false) == 0 && this.originPanelBlock && !flag)
			{
				Sound.PlaySound("Error", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
				return;
			}
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y) * NormalizedScreen.scale);
			int paintMeshIndexLast = this._paintMeshIndexLast;
			if (Tutorial.state == TutorialState.None)
			{
				Vector3 vector = default(Vector3);
				this._paintMeshIndexLast = block.GetMeshIndexForRay(ray, first, out vector, out vector);
			}
			else
			{
				this._paintMeshIndexLast = Tutorial.forceMeshIndex;
			}
			this._paintLast = block.GetPaint(this._paintMeshIndexLast);
			this._paintBefore = this._paintLast;
			if (this._paintLast == null)
			{
				BWLog.Info("Paint was null in paint block");
			}
			bool flag2 = this._paintMeshIndexLast != paintMeshIndexLast;
			if (first || (this._paintBefore != paint && flag2))
			{
				Sound.PlaySound("Paint", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
			}
			block.PaintTo(paint, true, this._paintMeshIndexLast);
			if (block.IsScaled())
			{
				block.ScaleTo(block.Scale(), true, true);
			}
			if (flag2 || first)
			{
				Scarcity.UpdateInventory(false, null);
				Scarcity.UpdateScarcityBadges(TileDragGesture.highlightGafs, TileDragGesture.currentGafUsage, TileDragGesture.startInventory);
			}
		}

		// Token: 0x06001672 RID: 5746 RVA: 0x000A01E8 File Offset: 0x0009E5E8
		private IEnumerator DelayedPaint(Block block, string paint, float delay, Vector2 screenPos)
		{
			float myTime = Time.time;
			if (delay > 0f)
			{
				yield return new WaitForSeconds(delay);
			}
			if (this._paintLastTime <= myTime)
			{
				this.PaintBlock(block, paint, screenPos, true);
			}
			yield break;
		}

		// Token: 0x06001673 RID: 5747 RVA: 0x000A0220 File Offset: 0x0009E620
		private void TextureBlock(Block block, string texture, Vector2 screenPos, Vector3 normal, bool first)
		{
			if (block is BlockSky)
			{
				if (!BlockSky.IsSkyTexture(texture))
				{
					if (first)
					{
						Sound.PlaySound("Error", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
					}
					return;
				}
			}
			else if (block is BlockWater)
			{
				if (!BlockWater.IsWaterTexture(texture))
				{
					if (first)
					{
						Sound.PlaySound("Error", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
					}
					return;
				}
			}
			else if (block.isTerrain && texture != "Plain" && !MaterialTexture.IsPhysicMaterialTexture(texture))
			{
				if (first)
				{
					Sound.PlaySound("Error", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
					Scarcity.UpdateScarcityBadges(TileDragGesture.highlightGafs, TileDragGesture.currentGafUsage, TileDragGesture.startInventory);
				}
				return;
			}
			bool flag = Tutorial.state != TutorialState.None;
			GAF gaf = new GAF("Block.TextureTo", new object[]
			{
				Scarcity.GetNormalizedTexture(texture),
				Vector3.zero
			});
			if (Scarcity.GetInventoryCount(gaf, false) == 0 && this.originPanelBlock && !flag)
			{
				return;
			}
			Vector3 vector = default(Vector3);
			Vector3 vector2 = normal;
			int num;
			if (Tutorial.state == TutorialState.None)
			{
				Ray ray = Blocksworld.mainCamera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y) * NormalizedScreen.scale);
				num = block.GetMeshIndexForRay(ray, first, out vector, out vector2);
			}
			else
			{
				num = Tutorial.forceMeshIndex;
			}
			if (this.oldTexture == null)
			{
				this.oldTexture = block.GetTexture(num);
				this.oldTextureNormal = block.GetTextureToNormal(num);
			}
			else
			{
				this.oldTexture = texture;
				this.oldTextureNormal = normal;
			}
			TileResultCode tileResultCode = block.TextureTo(texture, normal, true, num, false);
			this.couldTextureBlock = (tileResultCode == TileResultCode.True);
			if (this.couldTextureBlock && Options.ShowTextureToInfo)
			{
				string text = string.Concat(new object[]
				{
					"Textured block. Type: '",
					block.BlockType(),
					"' Mesh index: ",
					num,
					" Normal: ",
					normal,
					" Texture: ",
					texture
				});
				if (num > 0 && num - 1 < block.subMeshGameObjects.Count)
				{
					GameObject gameObject = block.subMeshGameObjects[num - 1];
					text = text + " Submesh name: " + gameObject.name;
				}
				OnScreenLog.AddLogItem(text, 2f, false);
			}
			if (first || this.oldMeshIndex != num)
			{
				if (this.couldTextureBlock)
				{
					Sound.PlayCreateSound(this._selectedTile.gaf, false, null);
				}
				else
				{
					Sound.PlaySound("Error", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
				}
			}
			if (this.couldTextureBlock && block.IsScaled())
			{
				block.ScaleTo(block.Scale(), true, true);
			}
			if (first || this.oldMeshIndex != num)
			{
				Scarcity.UpdateInventory(false, null);
				Scarcity.UpdateScarcityBadges(TileDragGesture.highlightGafs, TileDragGesture.currentGafUsage, TileDragGesture.startInventory);
			}
			this.oldMeshIndex = num;
		}

		// Token: 0x06001674 RID: 5748 RVA: 0x000A055C File Offset: 0x0009E95C
		private IEnumerator DelayedTexture(Block block, string texture, Vector3 normal, float delay, Vector2 screenPos)
		{
			float myTime = Time.time;
			if (delay > 0f)
			{
				yield return new WaitForSeconds(delay);
			}
			if (this._textureLastTime <= myTime)
			{
				this.TextureBlock(block, texture, screenPos, normal, true);
			}
			yield break;
		}

		// Token: 0x0400116F RID: 4463
		private readonly BuildPanel _buildPanel;

		// Token: 0x04001170 RID: 4464
		private readonly ScriptPanel _scriptPanel;

		// Token: 0x04001171 RID: 4465
		private Tile _mouseTile;

		// Token: 0x04001172 RID: 4466
		private Tile _selectedTile;

		// Token: 0x04001173 RID: 4467
		private float _paintLastTime;

		// Token: 0x04001174 RID: 4468
		private string _paintLast;

		// Token: 0x04001175 RID: 4469
		private int _paintMeshIndexLast;

		// Token: 0x04001176 RID: 4470
		private string _paintBefore;

		// Token: 0x04001177 RID: 4471
		private bool _appliedSkyBox;

		// Token: 0x04001178 RID: 4472
		private float _textureLastTime;

		// Token: 0x04001179 RID: 4473
		private string oldTexture;

		// Token: 0x0400117A RID: 4474
		private int oldMeshIndex;

		// Token: 0x0400117B RID: 4475
		private Vector3 oldTextureNormal = Vector3.zero;

		// Token: 0x0400117C RID: 4476
		private Vector2 origSnapPos = Vector2.zero;

		// Token: 0x0400117D RID: 4477
		private bool snappedSomewhereElse;

		// Token: 0x0400117E RID: 4478
		private bool couldTextureBlock;

		// Token: 0x0400117F RID: 4479
		private Block _selectedBlockOnStart;

		// Token: 0x04001180 RID: 4480
		private static Tile origDragTile = null;

		// Token: 0x04001181 RID: 4481
		private bool originPanelBlock;

		// Token: 0x04001182 RID: 4482
		private bool wasOverBuildPanel;

		// Token: 0x04001183 RID: 4483
		private bool wasOverScriptPanel;

		// Token: 0x04001184 RID: 4484
		private Vector2 beginTouchPos;

		// Token: 0x04001185 RID: 4485
		private bool _possibleDuplicate;

		// Token: 0x04001186 RID: 4486
		private float _startTime;

		// Token: 0x04001187 RID: 4487
		private const float DUPLICATE_HOLD_TIME = 0.7f;

		// Token: 0x04001188 RID: 4488
		public bool draggingScriptTile;

		// Token: 0x04001189 RID: 4489
		private static Dictionary<GAF, int> currentGafUsage;

		// Token: 0x0400118A RID: 4490
		private static Dictionary<GAF, int> startInventory;

		// Token: 0x0400118B RID: 4491
		private static HashSet<GAF> highlightGafs = new HashSet<GAF>();
	}
}
