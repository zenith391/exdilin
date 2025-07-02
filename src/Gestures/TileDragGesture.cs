using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures;

public class TileDragGesture : BaseGesture
{
	private readonly BuildPanel _buildPanel;

	private readonly ScriptPanel _scriptPanel;

	private Tile _mouseTile;

	private Tile _selectedTile;

	private float _paintLastTime;

	private string _paintLast;

	private int _paintMeshIndexLast;

	private string _paintBefore;

	private bool _appliedSkyBox;

	private float _textureLastTime;

	private string oldTexture;

	private int oldMeshIndex;

	private Vector3 oldTextureNormal = Vector3.zero;

	private Vector2 origSnapPos = Vector2.zero;

	private bool snappedSomewhereElse;

	private bool couldTextureBlock;

	private Block _selectedBlockOnStart;

	private static Tile origDragTile = null;

	private bool originPanelBlock;

	private bool wasOverBuildPanel;

	private bool wasOverScriptPanel;

	private Vector2 beginTouchPos;

	private bool _possibleDuplicate;

	private float _startTime;

	private const float DUPLICATE_HOLD_TIME = 0.7f;

	public bool draggingScriptTile;

	private static Dictionary<GAF, int> currentGafUsage;

	private static Dictionary<GAF, int> startInventory;

	private static HashSet<GAF> highlightGafs = new HashSet<GAF>();

	public TileDragGesture(BuildPanel buildPanel, ScriptPanel scriptPanel)
	{
		_scriptPanel = scriptPanel;
		_buildPanel = buildPanel;
		touchBeginWindow = 12f;
		Reset();
	}

	public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
	{
		if (highlightGafs.Count > 0)
		{
			if (result == null)
			{
				result = new HashSet<GAF>();
			}
			foreach (GAF highlightGaf in highlightGafs)
			{
				result.Add(highlightGaf);
			}
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
			return;
		}
		if (allTouches.Count != 1)
		{
			EnterState(GestureState.Failed);
			return;
		}
		bool flag = allTouches[0].Phase == TouchPhase.Began;
		bool flag2 = allTouches[0].Phase == TouchPhase.Moved && (float)allTouches[0].moveFrameCount < touchBeginWindow;
		if (!flag && !flag2)
		{
			EnterState(GestureState.Failed);
			return;
		}
		Vector2 position = allTouches[0].Position;
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		bool flag3 = quickSelect != null && quickSelect.Hit(position);
		Tile tile = null;
		Tile tile2 = null;
		Tile tile3 = null;
		beginTouchPos = position;
		bool flag4 = Tutorial.InTutorial();
		bool flag5 = Tutorial.InTutorialOrPuzzle();
		bool flag6 = !flag4 || Tutorial.usingQuickSelectColorIcon;
		bool flag7 = !flag4 || Tutorial.usingQuickSelectTextureIcon;
		if (flag3 && (flag6 || flag7))
		{
			if (flag6 && quickSelect.HitPaint(position))
			{
				tile = quickSelect.CreatePaintTile();
			}
			else if (flag7 && quickSelect.HitTexture(position))
			{
				tile = quickSelect.CreateTextureTile();
				quickSelect.ShowTextureScarcity();
			}
			else if (!flag5 && quickSelect.HitScript(position))
			{
				tile = quickSelect.CreateScriptTile();
				quickSelect.ShowScriptScarcity();
			}
			_selectedBlockOnStart = Blocksworld.selectedBlock;
			if (tile == null)
			{
				_selectedTile = null;
				EnterState(GestureState.Active);
				return;
			}
		}
		else
		{
			tile2 = _buildPanel.HitTile(position);
			tile3 = _scriptPanel.HitTile(position);
		}
		if (tile2 == null && tile3 == null && tile == null)
		{
			EnterState(GestureState.Possible);
			return;
		}
		originPanelBlock = false;
		currentGafUsage = Scarcity.CalculateWorldGafUsage();
		startInventory = Scarcity.GetInventoryCopy();
		Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
		if (_scriptPanel.Hit(position) && !flag3 && tile3 != null && !tile3.IsCreate() && !tile3.IsThen())
		{
			_selectedTile = (_mouseTile = tile3);
			origDragTile = _mouseTile;
			selectedScriptBlock.RemoveTile(_mouseTile);
			EnterState(GestureState.Active);
			originPanelBlock = true;
			wasOverBuildPanel = false;
			wasOverScriptPanel = true;
			highlightGafs.Add(_mouseTile.gaf);
			Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), oneShot: true, 0.2f);
		}
		else if (!_scriptPanel.Hit(position) && !flag3 && tile2 != null && !tile2.IsCreate() && !tile2.IsCreateModel() && !tile2.IsThen())
		{
			_mouseTile = tile2;
			Tile tile4 = null;
			GAF gaf = _mouseTile.gaf;
			Predicate predicate = gaf.Predicate;
			EditableTileParameter editableParameter = predicate.EditableParameter;
			if (editableParameter != null && editableParameter.settings.overwriteGafArgumentInBuildPanel)
			{
				object[] args = predicate.ExtendArguments(gaf.Args, overwrite: true);
				tile4 = new Tile(new GAF(gaf.Predicate, args));
			}
			if (tile4 == null)
			{
				_selectedTile = _mouseTile.Clone();
			}
			else
			{
				_selectedTile = tile4;
			}
			_selectedTile.Enable(enabled: true);
			origDragTile = _mouseTile;
			Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), oneShot: true, 0.2f);
			highlightGafs.Add(_mouseTile.gaf);
			wasOverBuildPanel = true;
			wasOverScriptPanel = false;
			EnterState(GestureState.Active);
		}
		else
		{
			if (tile == null)
			{
				EnterState(GestureState.Failed);
				return;
			}
			_selectedTile = tile;
			draggingScriptTile = _selectedTile.IsScriptGear();
			_selectedTile.Show(show: true);
			_selectedTile.Enable(enabled: true);
			highlightGafs.Add(tile.gaf);
			Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), oneShot: true, 0.2f);
			EnterState(GestureState.Active);
		}
		origSnapPos = _scriptPanel.SnapTile(position, _selectedTile);
		_selectedTile.Show(show: true);
		MoveSelectedTile(position);
		bool flag8 = _selectedTile.IsTexture();
		bool flag9 = _selectedTile.IsPaint();
		if (!flag8 && !flag9)
		{
			Scarcity.PaintScarcityBadges();
		}
		_possibleDuplicate = _scriptPanel.Hit(position) && tile3 != null;
		_startTime = Time.time;
		_appliedSkyBox = false;
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		if (_selectedTile == null)
		{
			return;
		}
		_possibleDuplicate = false;
		TileIconManager.Instance.ClearNewLoadLimit();
		Vector2 position = allTouches[allTouches.Count - 1].Position;
		MoveSelectedTile(position);
		bool flag = Blocksworld.mouseBlock != Blocksworld.mouseBlockLast;
		bool flag2 = Blocksworld.mouseBlockNormal != Blocksworld.mouseBlockNormalLast;
		Block mouseBlock = Blocksworld.mouseBlock;
		Block mouseBlockLast = Blocksworld.mouseBlockLast;
		int num = 0;
		if (mouseBlock != null)
		{
			num = mouseBlock.GetMeshIndexForRay(Blocksworld.mainCamera.ScreenPointToRay(new Vector3(position.x, position.y) * NormalizedScreen.scale), !flag && oldTexture == null && _paintLast == null, out var _, out var _);
		}
		bool flag3 = oldMeshIndex != num;
		bool flag4 = _selectedTile.IsTexture();
		bool flag5 = _selectedTile.IsPaint();
		bool flag6 = (draggingScriptTile = _selectedTile.IsScriptGear());
		bool flag7 = Tutorial.state != TutorialState.None && Tutorial.state != TutorialState.Puzzle;
		bool flag8 = !flag7 || Tutorial.state == TutorialState.Color;
		bool flag9 = !flag7 || Tutorial.state == TutorialState.Texture;
		if (_selectedTile.IsSkyBox())
		{
			int num2 = (int)_selectedTile.gaf.Args[0];
			if (mouseBlock == Blocksworld.worldSky)
			{
				if (!_appliedSkyBox)
				{
					if (WorldEnvironmentManager.SkyBoxIndex() != num2)
					{
						WorldEnvironmentManager.SaveSkyBoxHistory();
					}
					WorldEnvironmentManager.ChangeSkyBoxPermanently(num2);
					_appliedSkyBox = true;
				}
			}
			else if (mouseBlockLast == Blocksworld.worldSky)
			{
				WorldEnvironmentManager.RevertToPreviousSkyBox();
				_appliedSkyBox = false;
			}
			else if (_scriptPanel.Hit(position) && _selectedBlockOnStart is BlockMaster)
			{
				return;
			}
			History.AddStateIfNecessary();
		}
		else if (mouseBlock != null && mouseBlockLast != null && _paintLast != null && flag5 && !flag7 && !flag && mouseBlock.ContainsPaintableSubmeshes() && (!mouseBlock.isTerrain || mouseBlock.SelectableTerrain()))
		{
			mouseBlockLast.PaintTo(_paintLast, permanent: true, _paintMeshIndexLast);
			string paint = (string)_selectedTile.gaf.Args[0];
			PaintBlock(mouseBlock, paint, position, first: false);
		}
		else if (mouseBlock != null && oldTexture != null && flag4 && !flag7 && !flag && flag3 && mouseBlock.ContainsPaintableSubmeshes() && (!mouseBlock.isTerrain || mouseBlock.SelectableTerrain()))
		{
			mouseBlock.TextureTo(oldTexture, oldTextureNormal, permanent: true, oldMeshIndex);
			oldTexture = null;
			oldMeshIndex = -1;
			oldTextureNormal = Vector3.zero;
			TextureBlock(mouseBlock, (string)_selectedTile.gaf.Args[0], position, Blocksworld.mouseBlockNormal, first: false);
		}
		else if (flag5 && flag)
		{
			_paintLastTime = Time.time;
			if (mouseBlockLast != null && _paintLast != null && !flag7)
			{
				mouseBlockLast.PaintTo(_paintLast, permanent: true, _paintMeshIndexLast);
				if (mouseBlockLast.IsScaled())
				{
					mouseBlockLast.ScaleTo(mouseBlockLast.Scale(), recalculateCollider: true, forceRescale: true);
				}
				_paintLast = null;
				_paintMeshIndexLast = 0;
			}
			if (mouseBlock != null && flag8 && !_scriptPanel.Hit(position) && !Blocksworld.locked.Contains(mouseBlock))
			{
				float delay = ((!mouseBlock.isTerrain || mouseBlock.SelectableTerrain()) ? 0f : 0.5f);
				Blocksworld.bw.StartCoroutine(DelayedPaint(mouseBlock, (string)_selectedTile.gaf.Args[0], delay, position));
			}
		}
		else if (flag4 && (flag || flag2))
		{
			_textureLastTime = Time.time;
			if (mouseBlockLast != null && oldTexture != null && !flag7)
			{
				mouseBlockLast.TextureTo(Scarcity.GetNormalizedTexture(oldTexture), oldTextureNormal, permanent: true, oldMeshIndex);
				if (mouseBlockLast.IsScaled())
				{
					mouseBlockLast.ScaleTo(mouseBlockLast.Scale(), recalculateCollider: true, forceRescale: true);
				}
				oldTexture = null;
				oldMeshIndex = -1;
				oldTextureNormal = Vector3.zero;
			}
			if (mouseBlock != null && flag9 && !_scriptPanel.Hit(position) && !Blocksworld.locked.Contains(mouseBlock))
			{
				float delay2 = ((!mouseBlock.isTerrain || mouseBlock.SelectableTerrain()) ? 0f : 0.5f);
				Blocksworld.bw.StartCoroutine(DelayedTexture(mouseBlock, Scarcity.GetNormalizedTexture((string)_selectedTile.gaf.Args[0]), Blocksworld.mouseBlockNormal, delay2, position));
			}
		}
		bool flag10 = _buildPanel.Hit(position);
		bool flag11 = _scriptPanel.Hit(position);
		if (flag6)
		{
			bool flag12 = false;
			bool flag13 = false;
			if (flag11 && !wasOverScriptPanel)
			{
				flag12 = true;
			}
			if (wasOverScriptPanel && !flag11)
			{
				flag13 = true;
			}
			if (!flag11)
			{
				bool flag14 = mouseBlock == null || (mouseBlock.isTerrain && !mouseBlock.SelectableTerrain()) || Blocksworld.editorSelectionLocked.Contains(mouseBlock) || mouseBlock.HasGroup("locked-model");
				if (_selectedBlockOnStart == null)
				{
					if (flag14)
					{
						if (Blocksworld.selectedBlock != null)
						{
							Blocksworld.Deselect(silent: true);
							flag13 = true;
						}
					}
					else if (mouseBlock != Blocksworld.selectedBlock)
					{
						if (Blocksworld.selectedBlock != null)
						{
							Blocksworld.Deselect(silent: true);
						}
						Blocksworld.SelectBlock(mouseBlock, silent: true);
						HideSelectedButtons();
						flag12 = true;
					}
				}
				else if (flag14 && Blocksworld.selectedBlock != _selectedBlockOnStart)
				{
					Blocksworld.SelectBlock(_selectedBlockOnStart, silent: true);
					if (mouseBlock == _selectedBlockOnStart)
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
						Blocksworld.Deselect(silent: true);
					}
					Blocksworld.SelectBlock(mouseBlock, silent: true);
					HideSelectedButtons();
					flag12 = true;
				}
				else if (mouseBlockLast != mouseBlock)
				{
					if (mouseBlock == _selectedBlockOnStart)
					{
						flag12 = true;
					}
					if (mouseBlockLast == _selectedBlockOnStart)
					{
						flag13 = true;
					}
				}
			}
			if (flag12)
			{
				Sound.PlayOneShotSound("Paste Target Found");
				_scriptPanel.Highlight(h: true);
			}
			else if (flag13)
			{
				Sound.PlayOneShotSound("Paste Target Lost");
				_scriptPanel.Highlight(h: false);
			}
		}
		if ((flag10 && !wasOverBuildPanel) || (flag11 && !wasOverScriptPanel))
		{
			Scarcity.UpdateScarcityBadges(highlightGafs, currentGafUsage, startInventory);
		}
		wasOverBuildPanel = flag10;
		wasOverScriptPanel = flag11;
	}

	private void HideSelectedButtons()
	{
		TBox.tileButtonMove.Hide();
		TBox.tileButtonRotate.Hide();
		TBox.tileButtonScale.Hide();
		TBox.tileLockedModelIcon.Hide();
		Blocksworld.UI.SidePanel.HideCopyModelButton();
		Blocksworld.UI.SidePanel.HideSaveModelButton();
	}

	private void ToggleButton(Tile buttonTile)
	{
		string text = (string)buttonTile.gaf.Args[0];
		if (UIInputControl.controlTypeFromString.TryGetValue(text, out var value) && UIInputControl.controlVariantFromString.TryGetValue(text, out var value2))
		{
			Blocksworld.UI.Controls.MapControlToVariant(value, value2);
		}
		foreach (Block item in BWSceneManager.AllBlocks())
		{
			foreach (List<Tile> tile in item.tiles)
			{
				foreach (Tile item2 in tile)
				{
					if (item2.gaf.Predicate != Block.predicateButton)
					{
						continue;
					}
					string text2 = (string)item2.gaf.Args[0];
					if (UIInputControl.controlTypeFromString.TryGetValue(text2, out var value3) && value3 == value && text2 != text)
					{
						item2.gaf = new GAF(Block.predicateButton, text);
						if (item2.IsShowing())
						{
							item2.tileObject.Setup(item2.gaf, item2.IsEnabled());
						}
					}
				}
			}
		}
	}

	public override void TouchesStationary(List<Touch> allTouches)
	{
		Vector2 position = allTouches[0].Position;
		MoveSelectedTile(position);
		if (_possibleDuplicate && Time.time - _startTime > 0.7f)
		{
			_possibleDuplicate = false;
			Tile tile = _selectedTile.Clone();
			Vector2 vector = _scriptPanel.SnapTile(position, tile);
			int x = (int)vector.x;
			int y = (int)vector.y;
			Blocksworld.bw.InsertTile(Blocksworld.GetSelectedScriptBlock(), x, y, tile);
			Sound.PlayCreateSound(tile.gaf, script: true);
			_scriptPanel.SavePositionForNextLayout();
			_scriptPanel.AssignUnparentedTiles();
			_scriptPanel.Layout();
			History.AddStateIfNecessary();
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		Vector2 position = allTouches[0].Position;
		if (allTouches[0].Phase != TouchPhase.Ended)
		{
			return;
		}
		if (_selectedTile == null)
		{
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			bool flag = false;
			Dictionary<GAF, int> missing = new Dictionary<GAF, int>();
			List<GAF> list = new List<GAF>();
			string empty = string.Empty;
			string text = "model";
			string text2 = "Missing items:";
			if (quickSelect != null)
			{
				if (quickSelect.HitModel(position, ignoreAvailable: true))
				{
					flag = !Blocksworld.clipboard.HasEnoughScarcityForModel(missing, list);
				}
				else if (quickSelect.HitScript(position, ignoreAvailable: true))
				{
					text = "script";
					flag = !Blocksworld.clipboard.HasEnoughScarcityForScript(missing, list);
				}
				else if (!Tutorial.InTutorialOrPuzzle() && !quickSelect.HitTexture(position) && quickSelect.HitTexture(position, ignoreAvailable: true))
				{
					Tile tile = quickSelect.CreatePaintTile();
					Blocksworld.UI.Dialog.ShowZeroInventoryDialog(tile);
				}
				if (list.Count > 0)
				{
					text2 = "A world can only have one of these:";
				}
			}
			if (flag && (position - beginTouchPos).magnitude < 10f)
			{
				empty = "\nCould not paste " + text + ".\n\n" + text2;
				Blocksworld.UI.Dialog.ShowPasteFailInfo(missing, list, empty);
			}
			EnterState(GestureState.Ended);
			return;
		}
		bool flag2 = _selectedTile.IsTexture();
		bool flag3 = _selectedTile.IsPaint();
		bool flag4 = _selectedTile.IsSfx();
		bool flag5 = _buildPanel.Hit(position);
		if (flag3)
		{
			string paint = (string)_selectedTile.gaf.Args[0];
			Blocksworld.clipboard.SetPaintColor(paint);
			if (Blocksworld.UI.QuickSelect.HitPaint(position))
			{
				Blocksworld.clipboard.autoPaintMode = !Blocksworld.clipboard.autoPaintMode;
			}
		}
		if (flag2)
		{
			string texture = (string)_selectedTile.gaf.Args[0];
			Blocksworld.clipboard.SetTexture(texture);
			if (Blocksworld.UI.QuickSelect.HitTexture(position))
			{
				Blocksworld.clipboard.autoTextureMode = !Blocksworld.clipboard.autoTextureMode;
			}
		}
		if (flag4 && flag5 && (_selectedTile.gaf.Args.Length < 3 || (string)_selectedTile.gaf.Args[2] != "loop"))
		{
			Sound.PlayOneShotSound((string)_selectedTile.gaf.Args[0]);
		}
		bool flag6 = _selectedTile.IsScriptGear();
		bool flag7 = _selectedTile.IsSkyBox();
		bool flag8 = false;
		Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
		bool flag9 = !flag7 || selectedScriptBlock is BlockMaster;
		bool flag10;
		if (flag6 && selectedScriptBlock != null)
		{
			if (_scriptPanel.Hit(_selectedTile.tileObject.GetPosition()) || Blocksworld.mouseBlock == Blocksworld.selectedBlock)
			{
				Blocksworld.bw.PasteScriptFromClipboard(selectedScriptBlock);
				flag8 = true;
			}
			_selectedTile.Show(show: false);
			flag10 = true;
			_scriptPanel.Highlight(h: false);
		}
		else if (selectedScriptBlock != null && _scriptPanel.IsShowing() && flag9)
		{
			if (flag7)
			{
				_selectedTile.gaf = new GAF(BlockMaster.predicateSkyBoxTo, _selectedTile.gaf.Args);
			}
			Vector2 vector = _scriptPanel.SnapTile(position, _selectedTile);
			int x = (int)vector.x;
			int num = (int)vector.y;
			if (num != 0)
			{
				Tile tile2 = _selectedTile.Clone();
				tile2.MoveTo(_selectedTile.GetPosition());
				if (tile2.gaf.Predicate.EditableParameter != null)
				{
					tile2.gaf.Predicate.EditableParameter.ApplyTileParameterUI(tile2);
				}
				tile2.Show(show: true);
				_selectedTile.Show(show: false);
				flag10 = true;
				Blocksworld.selectedTile = tile2;
				if (tile2.gaf.Predicate == Block.predicateSendCustomSignal && (string)tile2.gaf.Args[0] == "*")
				{
					string text3 = "Signal";
					Predicate predicate = Block.predicateSendCustomSignal;
					if (Tutorial.InTutorial())
					{
						text3 = Tutorial.GetTargetSignalName();
						predicate = Tutorial.GetTargetSignalPredicate();
					}
					tile2.gaf = new GAF(predicate, text3, 1f);
					tile2.Show(show: false);
					tile2.Show(show: true);
					_scriptPanel.UpdateOverlay(tile2);
					if (!Tutorial.InTutorial())
					{
						Blocksworld.bw.tileParameterEditor.StartEditing(Blocksworld.selectedTile, new SignalNameTileParameter(0));
					}
				}
				else if (!originPanelBlock && !Tutorial.InTutorial() && Blocksworld.selectedTile.gaf.Predicate.EditableParameter != null && Blocksworld.selectedTile.gaf.Predicate.EditableParameter.settings.autoOpenOnNewTiles)
				{
					Blocksworld.bw.tileParameterEditor.StartEditing(Blocksworld.selectedTile, Blocksworld.selectedTile.gaf.Predicate.EditableParameter);
				}
				else if (!Tutorial.InTutorial())
				{
					if (tile2.gaf.Predicate == Block.predicateVariableCustomInt && (string)tile2.gaf.Args[0] == "*")
					{
						string text4 = "Int";
						tile2.gaf = new GAF(tile2.gaf.Predicate, text4, 0);
						tile2.Show(show: false);
						tile2.Show(show: true);
						_scriptPanel.UpdateOverlay(tile2);
						Blocksworld.bw.tileParameterEditor.StartEditing(Blocksworld.selectedTile, new VariableNameTileParameter(0));
					}
					else if (tile2.gaf.Predicate == Block.predicateBlockVariableInt && (string)tile2.gaf.Args[0] == "*")
					{
						string text5 = Blocksworld.NextAvailableBlockVariableName(selectedScriptBlock);
						tile2.gaf = new GAF(tile2.gaf.Predicate, text5, 0);
						tile2.Show(show: false);
						tile2.Show(show: true);
						_scriptPanel.UpdateOverlay(tile2);
					}
				}
				Blocksworld.bw.InsertTile(selectedScriptBlock, x, num, tile2);
				Sound.PlayCreateSound(tile2.gaf, script: true);
				_scriptPanel.SavePositionForNextLayout();
				int num2 = selectedScriptBlock.AddOrRemoveEmptyScriptLine();
				_scriptPanel.AssignUnparentedTiles();
				_scriptPanel.Layout();
				if (wasOverScriptPanel && !snappedSomewhereElse && _scriptPanel.HitTile(position) == tile2)
				{
					if (!tile2.gaf.Predicate.CanEditTile(tile2) || (Blocksworld.selectedTile != null && Blocksworld.selectedTile.Equals(tile2)))
					{
						GAF nextUnlocked = TileToggleChain.GetNextUnlocked(tile2.gaf);
						if (nextUnlocked != null && !tile2.gaf.Equals(nextUnlocked))
						{
							tile2.gaf = nextUnlocked;
							tile2.Show(show: true);
							tile2.tileObject.Setup(nextUnlocked, enabled: true);
							if (tile2.gaf.Predicate == Block.predicateButton)
							{
								ToggleButton(tile2);
							}
						}
					}
					Blocksworld.BlockPanelTileTapped(tile2);
				}
				ReplaceTile(selectedScriptBlock, tile2);
			}
			else
			{
				_selectedTile.SetParent(null);
				_selectedTile.Show(show: false);
				flag10 = true;
				_scriptPanel.SavePositionForNextLayout();
				int num3 = selectedScriptBlock.AddOrRemoveEmptyScriptLine();
				_scriptPanel.Layout();
			}
			if (!originPanelBlock || !_scriptPanel.Hit(position))
			{
				Blocksworld.UpdateTiles();
			}
			Sound.PlayCreateSound(_selectedTile.gaf, script: true);
		}
		else
		{
			_selectedTile.Show(show: false);
			flag10 = true;
		}
		_paintBefore = null;
		Block mouseBlock = Blocksworld.mouseBlock;
		if (flag10 && !_buildPanel.Hit(position) && (_scriptPanel.IsShowing() || !_scriptPanel.Hit(position)) && (!flag3 || (mouseBlock != null && mouseBlock.isTerrain)) && (!flag2 || (mouseBlock != null && mouseBlock.isTerrain)) && !flag8)
		{
			Sound.PlaySound("Tile Drop", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
		}
		if (flag10)
		{
			_selectedTile.Destroy();
			_selectedTile = null;
		}
		draggingScriptTile = false;
		Tutorial.Step();
		History.AddStateIfNecessary();
		EnterState(GestureState.Ended);
		highlightGafs.Clear();
		Scarcity.UpdateInventory();
		Blocksworld.UI.QuickSelect.HideScarcity();
		TBox.UpdateCopyButtonVisibility();
	}

	private void ReplaceTile(Block scriptBlock, Tile tile)
	{
		GAF gaf = tile.gaf;
		GAF gAF = gaf;
		if (scriptBlock is BlockMaster)
		{
			gAF = BlockMaster.ReplaceGaf(gaf);
		}
		if (tile.gaf.Predicate == Block.predicateButton)
		{
			string key = (string)tile.gaf.Args[0];
			if (UIInputControl.controlTypeFromString.TryGetValue(key, out var value))
			{
				UIInputControl.ControlVariant controlVariant = Blocksworld.UI.Controls.GetControlVariant(value);
				string text = value.ToString();
				if (controlVariant != UIInputControl.ControlVariant.Default)
				{
					text = text + " " + controlVariant;
				}
				gAF = new GAF(Block.predicateButton, text);
			}
		}
		if (gAF != gaf)
		{
			tile.gaf = gAF;
			if (gAF.Predicate.EditableParameter != null)
			{
				tile.subParameterCount = gAF.Predicate.EditableParameter.subParameterCount;
			}
			tile.Show(show: true);
			tile.tileObject.Setup(gAF, tile.IsEnabled());
		}
	}

	public override void Cancel()
	{
		EnterState(GestureState.Cancelled);
		if (_selectedTile != null)
		{
			_selectedTile.Show(show: false);
			_selectedTile.Destroy();
			_selectedTile = null;
		}
	}

	public override void Reset()
	{
		EnterState(GestureState.Possible);
		draggingScriptTile = false;
		if (_selectedTile != null)
		{
			_selectedTile.Show(show: false);
			_selectedTile.Destroy();
			_selectedTile = null;
			Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
			if (selectedScriptBlock != null && _scriptPanel.IsShowing())
			{
				int num = selectedScriptBlock.AddOrRemoveEmptyScriptLine();
				_scriptPanel.AssignUnparentedTiles();
				_scriptPanel.Layout();
			}
		}
		_mouseTile = null;
		_selectedTile = null;
		_paintLast = null;
		_paintLastTime = Time.time;
		_paintMeshIndexLast = 0;
		oldTexture = null;
		oldTextureNormal = Vector3.zero;
		_textureLastTime = Time.time;
		oldMeshIndex = -1;
		origSnapPos = Vector2.zero;
		snappedSomewhereElse = false;
		origDragTile = null;
		highlightGafs.Clear();
		_possibleDuplicate = false;
		_selectedBlockOnStart = null;
		_scriptPanel.Highlight(h: false);
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		if (quickSelect != null)
		{
			quickSelect.showScriptScarcityBadge = false;
		}
	}

	public override string ToString()
	{
		return "TileDrag";
	}

	private Vector2 MoveSelectedTile(Vector2 pos, bool first = false)
	{
		Vector2 vector = Vector2.zero;
		if (_selectedTile == null)
		{
			return vector;
		}
		Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
		bool flag = !_selectedTile.IsScriptGear();
		flag &= !_selectedTile.IsSkyBox() || selectedScriptBlock is BlockMaster;
		if (_scriptPanel.IsShowing() && flag)
		{
			vector = _scriptPanel.SnapTile(pos, _selectedTile);
			if (!first && (vector - origSnapPos).sqrMagnitude > 0.01f)
			{
				snappedSomewhereElse = true;
			}
		}
		if (selectedScriptBlock == null || !_scriptPanel.IsShowing() || vector == Vector2.zero)
		{
			_selectedTile.MoveTo(pos.x - 40f, pos.y - 40f, -0.5f);
		}
		return vector;
	}

	private void PaintBlock(Block block, string paint, Vector2 screenPos, bool first)
	{
		bool flag = Tutorial.InTutorialOrPuzzle();
		GAF gaf = new GAF("Block.PaintTo", paint);
		if (Scarcity.GetInventoryCount(gaf) == 0 && originPanelBlock && !flag)
		{
			Sound.PlaySound("Error", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
			return;
		}
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y) * NormalizedScreen.scale);
		int paintMeshIndexLast = _paintMeshIndexLast;
		if (Tutorial.state == TutorialState.None)
		{
			Vector3 point = default(Vector3);
			_paintMeshIndexLast = block.GetMeshIndexForRay(ray, first, out point, out point);
		}
		else
		{
			_paintMeshIndexLast = Tutorial.forceMeshIndex;
		}
		_paintLast = block.GetPaint(_paintMeshIndexLast);
		_paintBefore = _paintLast;
		if (_paintLast == null)
		{
			BWLog.Info("Paint was null in paint block");
		}
		bool flag2 = _paintMeshIndexLast != paintMeshIndexLast;
		if (first || (_paintBefore != paint && flag2))
		{
			Sound.PlaySound("Paint", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
		}
		block.PaintTo(paint, permanent: true, _paintMeshIndexLast);
		if (block.IsScaled())
		{
			block.ScaleTo(block.Scale(), recalculateCollider: true, forceRescale: true);
		}
		if (flag2 || first)
		{
			Scarcity.UpdateInventory(updateTiles: false);
			Scarcity.UpdateScarcityBadges(highlightGafs, currentGafUsage, startInventory);
		}
	}

	private IEnumerator DelayedPaint(Block block, string paint, float delay, Vector2 screenPos)
	{
		float myTime = Time.time;
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		if (_paintLastTime <= myTime)
		{
			PaintBlock(block, paint, screenPos, first: true);
		}
	}

	private void TextureBlock(Block block, string texture, Vector2 screenPos, Vector3 normal, bool first)
	{
		if (block is BlockSky)
		{
			if (!BlockSky.IsSkyTexture(texture))
			{
				if (first)
				{
					Sound.PlaySound("Error", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
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
					Sound.PlaySound("Error", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
				}
				return;
			}
		}
		else if (block.isTerrain && texture != "Plain" && !MaterialTexture.IsPhysicMaterialTexture(texture))
		{
			if (first)
			{
				Sound.PlaySound("Error", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
				Scarcity.UpdateScarcityBadges(highlightGafs, currentGafUsage, startInventory);
			}
			return;
		}
		bool flag = Tutorial.state != TutorialState.None;
		GAF gaf = new GAF("Block.TextureTo", Scarcity.GetNormalizedTexture(texture), Vector3.zero);
		if (Scarcity.GetInventoryCount(gaf) == 0 && originPanelBlock && !flag)
		{
			return;
		}
		Vector3 point = default(Vector3);
		Vector3 normal2 = normal;
		int num;
		if (Tutorial.state == TutorialState.None)
		{
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y) * NormalizedScreen.scale);
			num = block.GetMeshIndexForRay(ray, first, out point, out normal2);
		}
		else
		{
			num = Tutorial.forceMeshIndex;
		}
		if (oldTexture == null)
		{
			oldTexture = block.GetTexture(num);
			oldTextureNormal = block.GetTextureToNormal(num);
		}
		else
		{
			oldTexture = texture;
			oldTextureNormal = normal;
		}
		TileResultCode tileResultCode = block.TextureTo(texture, normal, permanent: true, num);
		couldTextureBlock = tileResultCode == TileResultCode.True;
		if (couldTextureBlock && Options.ShowTextureToInfo)
		{
			string text = string.Concat("Textured block. Type: '", block.BlockType(), "' Mesh index: ", num, " Normal: ", normal, " Texture: ", texture);
			if (num > 0 && num - 1 < block.subMeshGameObjects.Count)
			{
				GameObject gameObject = block.subMeshGameObjects[num - 1];
				text = text + " Submesh name: " + gameObject.name;
			}
			OnScreenLog.AddLogItem(text, 2f);
		}
		if (first || oldMeshIndex != num)
		{
			if (couldTextureBlock)
			{
				Sound.PlayCreateSound(_selectedTile.gaf);
			}
			else
			{
				Sound.PlaySound("Error", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
			}
		}
		if (couldTextureBlock && block.IsScaled())
		{
			block.ScaleTo(block.Scale(), recalculateCollider: true, forceRescale: true);
		}
		if (first || oldMeshIndex != num)
		{
			Scarcity.UpdateInventory(updateTiles: false);
			Scarcity.UpdateScarcityBadges(highlightGafs, currentGafUsage, startInventory);
		}
		oldMeshIndex = num;
	}

	private IEnumerator DelayedTexture(Block block, string texture, Vector3 normal, float delay, Vector2 screenPos)
	{
		float myTime = Time.time;
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		if (_textureLastTime <= myTime)
		{
			TextureBlock(block, texture, screenPos, normal, first: true);
		}
	}
}
