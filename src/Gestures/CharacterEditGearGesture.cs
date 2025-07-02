using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures;

public class CharacterEditGearGesture : BaseGesture
{
	private BuildPanel _buildPanel;

	private BlockAnimatedCharacter _targetCharacter;

	private Tile _dragTile;

	private Block _dragBlock;

	private BlocksterGearType _gearType;

	private List<CharacterEditor.SnapTarget> _snapTargets;

	private List<CharacterEditor.SnapTarget> _rotationTargets;

	private CharacterEditor.SnapTarget _snappedTo;

	private CharacterEditor.SnapTarget _rotateSnappedTo;

	private bool _partIsSnapped;

	private bool _overBuildPanel;

	private bool _enableRotationDrag;

	private Vector2 _dragOffset;

	private Quaternion _nonSnappedRotation;

	private static GAF _buildPanelTileGAF;

	private static HashSet<GAF> _displacedBlockGAFs;

	public CharacterEditGearGesture(BuildPanel buildPanel)
	{
		_buildPanel = buildPanel;
		touchBeginWindow = 12f;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
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
			if (!CharacterEditor.Instance.InEditMode())
			{
				EnterState(GestureState.Failed);
				return;
			}
			_targetCharacter = CharacterEditor.Instance.CharacterBlock();
			bool flag = allTouches[0].Phase == TouchPhase.Began;
			bool flag2 = (allTouches[0].Phase == TouchPhase.Moved || allTouches[0].Phase == TouchPhase.Stationary) && (float)allTouches[0].moveFrameCount < touchBeginWindow;
			if (!flag && !flag2)
			{
				EnterState(GestureState.Failed);
				return;
			}
			Blocksworld.blocksworldCamera.SetCameraStill(still: true);
			Vector2 position = allTouches[0].Position;
			_dragBlock = null;
			Block gearBlock;
			CharacterEditor.SnapTarget snapTarget;
			if (_buildPanel.Hit(position) && !Blocksworld.scriptPanel.Hit(position))
			{
				Tile tile = _buildPanel.HitTile(position);
				if (tile != null && tile.IsCreate())
				{
					_buildPanelTileGAF = tile.gaf;
					_dragTile = tile.Clone();
					_dragOffset = Vector2.zero;
					_dragBlock = Blocksworld.bw.AddNewBlock(_dragTile, addToBlocks: false);
					_snappedTo = CharacterEditor.SnapTarget.None;
					_overBuildPanel = true;
					_gearType = CharacterEditor.GetBlocksterGearType(_dragBlock);
					if (_gearType == BlocksterGearType.None)
					{
						BWLog.Info(_dragBlock?.ToString() + " has no gear type, section " + tile.panelSection);
						EnterState(GestureState.Failed);
						CharacterEditor.Instance.Exit();
						return;
					}
					_enableRotationDrag = false;
					_nonSnappedRotation = _dragBlock.GetRotation();
				}
			}
			else if (CharacterEditor.Instance.GetGearUnderScreenPosition(position, out gearBlock, out snapTarget))
			{
				int num = gearBlock.BlockItemId();
				BlockItem blockItem = ((num <= 0) ? null : BlockItem.FindByID(num));
				if (blockItem != null)
				{
					_dragBlock = gearBlock;
					_buildPanelTileGAF = new GAF(blockItem);
					_dragTile = new Tile(_buildPanelTileGAF);
					_snappedTo = snapTarget;
					_overBuildPanel = false;
					_gearType = CharacterEditor.GetBlocksterGearType(_dragBlock);
					if (_gearType == BlocksterGearType.None)
					{
						EnterState(GestureState.Failed);
						return;
					}
					_enableRotationDrag = true;
					Vector2 vector = Blocksworld.mainCamera.WorldToScreenPoint(_dragBlock.GetPosition());
					_dragOffset = vector - position * NormalizedScreen.scale;
				}
			}
			if (_dragBlock != null)
			{
				BWLog.Info(string.Concat("Dragging gear ", _dragBlock, " gearType ", _gearType));
				_snapTargets = new List<CharacterEditor.SnapTarget>();
				_rotationTargets = new List<CharacterEditor.SnapTarget>();
				switch (_gearType)
				{
				case BlocksterGearType.Head:
				case BlocksterGearType.HeadTop:
					_snapTargets.Add(CharacterEditor.SnapTarget.Head);
					_rotationTargets.Add(CharacterEditor.SnapTarget.HeadFront);
					_rotationTargets.Add(CharacterEditor.SnapTarget.HeadLeft);
					_rotationTargets.Add(CharacterEditor.SnapTarget.HeadBack);
					_rotationTargets.Add(CharacterEditor.SnapTarget.HeadRight);
					break;
				case BlocksterGearType.HeadSide:
					_snapTargets.Add(CharacterEditor.SnapTarget.HeadLeft);
					_snapTargets.Add(CharacterEditor.SnapTarget.HeadRight);
					_snapTargets.Add(CharacterEditor.SnapTarget.HeadBack);
					_snapTargets.Add(CharacterEditor.SnapTarget.HeadFront);
					break;
				case BlocksterGearType.Body:
				case BlocksterGearType.Back:
					_snapTargets.Add(CharacterEditor.SnapTarget.Body);
					break;
				case BlocksterGearType.RightHand:
					_snapTargets.Add(CharacterEditor.SnapTarget.RightHand);
					break;
				case BlocksterGearType.LeftHand:
					_snapTargets.Add(CharacterEditor.SnapTarget.LeftHand);
					break;
				case BlocksterGearType.EitherHand:
					_snapTargets.Add(CharacterEditor.SnapTarget.RightHand);
					_snapTargets.Add(CharacterEditor.SnapTarget.LeftHand);
					break;
				}
				Drag(position);
				_displacedBlockGAFs = new HashSet<GAF>();
				Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), oneShot: true, 0.2f);
				EnterState(GestureState.Active);
			}
			else
			{
				EnterState(GestureState.Possible);
			}
		}
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		Vector2 position = allTouches[0].Position;
		Drag(position);
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (base.gestureState == GestureState.Active && allTouches[0].Phase == TouchPhase.Ended)
		{
			History.AddStateIfNecessary();
			Reset();
		}
	}

	public override void Reset()
	{
		_buildPanelTileGAF = null;
		if (_displacedBlockGAFs != null)
		{
			_displacedBlockGAFs.Clear();
		}
		CharacterEditor.Instance.RemoveAllDisplacedBlocks();
		if (!_partIsSnapped && _dragBlock != null)
		{
			CharacterEditor.Instance.RemoveAttachment(_dragBlock);
			BWSceneManager.RemoveBlock(_dragBlock);
			_dragBlock.Destroy();
			Scarcity.UpdateInventory();
		}
		if (_dragTile != null)
		{
			_dragTile.Destroy();
		}
		_partIsSnapped = false;
		_snappedTo = CharacterEditor.SnapTarget.None;
		_rotateSnappedTo = CharacterEditor.SnapTarget.None;
		_dragTile = null;
		_dragBlock = null;
		_gearType = BlocksterGearType.None;
		_targetCharacter = null;
		EnterState(GestureState.Possible);
	}

	private void Drag(Vector2 pos)
	{
		Vector3 pos2 = pos - NormalizedScreen.pixelScale * 0.5f * 80f * Vector2.one;
		pos2.z = -0.5f;
		_dragTile.MoveTo(pos2);
		Vector3 vector = Blocksworld.mainCamera.transform.InverseTransformPoint(_targetCharacter.goT.position);
		Vector2 vector2 = pos * NormalizedScreen.scale + _dragOffset;
		Plane plane = new Plane(Vector3.up, _targetCharacter.goT.position - Vector3.up);
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(vector2);
		float num = vector.z;
		foreach (CharacterEditor.SnapTarget snapTarget in _snapTargets)
		{
			num = Mathf.Min(Blocksworld.mainCamera.transform.InverseTransformPoint(CharacterEditor.Instance.GetSnapTargetPosition(snapTarget, _gearType)).z, num);
		}
		if (plane.Raycast(ray, out var enter))
		{
			num = Mathf.Min(enter, num);
		}
		num -= 0.05f;
		Vector3 position = new Vector3(vector2.x, vector2.y, num);
		Vector3 pos3 = Blocksworld.mainCamera.ScreenToWorldPoint(position);
		_dragBlock.MoveTo(pos3);
		Vector3 pos4 = Vector3.zero;
		Quaternion rot = _dragBlock.GetRotation();
		CharacterEditor.SnapTarget snappedTo = _snappedTo;
		_snappedTo = CharacterEditor.Instance.GetSnapTargetUnderScreenPosition(pos, _gearType, _snapTargets);
		if (_snappedTo != CharacterEditor.SnapTarget.None)
		{
			pos4 = CharacterEditor.Instance.GetSnapTargetPosition(_snappedTo, _gearType);
			Vector3 vector3 = CharacterEditor.Instance.BlockSnapOffset(_dragBlock, _snappedTo);
			pos4 += vector3;
			if (_snappedTo != snappedTo)
			{
				rot = CharacterEditor.Instance.GetSnapTargetRotation(_snappedTo) * CharacterEditor.Instance.GetGearCharacterEditorOrientation(_dragBlock);
			}
			else if (_enableRotationDrag && _rotationTargets.Count > 0)
			{
				CharacterEditor.SnapTarget rotateSnappedTo = _rotateSnappedTo;
				_rotateSnappedTo = CharacterEditor.Instance.GetSnapTargetUnderScreenPosition(pos, _gearType, _rotationTargets);
				Quaternion quaternion = Quaternion.LookRotation(Vector3.right);
				Quaternion quaternion2 = Quaternion.LookRotation(Vector3.left);
				if ((rotateSnappedTo == CharacterEditor.SnapTarget.HeadFront && _rotateSnappedTo == CharacterEditor.SnapTarget.HeadLeft) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadLeft && _rotateSnappedTo == CharacterEditor.SnapTarget.HeadBack) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadBack && _rotateSnappedTo == CharacterEditor.SnapTarget.HeadRight) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadRight && _rotateSnappedTo == CharacterEditor.SnapTarget.HeadFront))
				{
					rot *= quaternion2;
				}
				else if ((rotateSnappedTo == CharacterEditor.SnapTarget.HeadFront && _rotateSnappedTo == CharacterEditor.SnapTarget.HeadRight) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadRight && _rotateSnappedTo == CharacterEditor.SnapTarget.HeadBack) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadBack && _rotateSnappedTo == CharacterEditor.SnapTarget.HeadLeft) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadLeft && _rotateSnappedTo == CharacterEditor.SnapTarget.HeadFront))
				{
					rot *= quaternion;
				}
			}
		}
		if (_snappedTo == CharacterEditor.SnapTarget.None && _enableRotationDrag)
		{
			_enableRotationDrag = false;
		}
		_partIsSnapped = _snappedTo != CharacterEditor.SnapTarget.None;
		if (_partIsSnapped)
		{
			_dragBlock.MoveTo(pos4);
			_dragBlock.RotateTo(rot);
		}
		else
		{
			_dragBlock.RotateTo(_nonSnappedRotation);
		}
		if (snappedTo != _snappedTo)
		{
			bool flag = false;
			if (snappedTo != CharacterEditor.SnapTarget.None)
			{
				CharacterEditor.Instance.RestoreAllDisplacedBlocks();
				CharacterEditor.Instance.RemoveAttachment(_dragBlock);
				flag = true;
			}
			if (_partIsSnapped)
			{
				CharacterEditor.Instance.AddAttachment(_dragBlock);
				List<Block> displacedBlocks = CharacterEditor.Instance.GetDisplacedBlocks();
				foreach (Block item in displacedBlocks)
				{
					_displacedBlockGAFs.Add(item.BlockCreateGAF());
				}
				flag = true;
			}
			if (flag)
			{
				Scarcity.inventoryScales[_dragTile.gaf] = 1.5f;
				Scarcity.UpdateInventory();
			}
		}
		bool overBuildPanel = _overBuildPanel;
		_overBuildPanel = _buildPanel.Hit(pos);
		if (_overBuildPanel)
		{
			_dragBlock.Deactivate();
			_dragTile.Show(show: true);
			_dragTile.MoveTo(pos2);
		}
		else
		{
			_dragBlock.Activate();
			_dragTile.Show(show: false);
		}
	}

	public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
	{
		if (_buildPanelTileGAF != null)
		{
			if (result == null)
			{
				result = new HashSet<GAF>();
			}
			result.Add(_buildPanelTileGAF);
			if (_displacedBlockGAFs != null)
			{
				result.UnionWith(_displacedBlockGAFs);
			}
		}
		return result;
	}
}
