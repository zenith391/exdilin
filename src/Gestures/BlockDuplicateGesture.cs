using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures;

public class BlockDuplicateGesture : BaseGesture
{
	private Block _startMouseBlock;

	private Vector2 _startPos;

	private float _startTime;

	private const float DUPLICATE_HOLD_TIME = 0.7f;

	public event BlockDuplicateBeginDelegate DuplicateBegan;

	public event BlockDuplicateGestureDelegate DuplicateEnded;

	public BlockDuplicateGesture(BlockDuplicateBeginDelegate dupeBegan, BlockDuplicateGestureDelegate dupeEnded)
	{
		DuplicateBegan += dupeBegan;
		DuplicateEnded += dupeEnded;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (allTouches.Count != 1 || Tutorial.InTutorialOrPuzzle() || Blocksworld.InModalDialogState())
		{
			EnterState(GestureState.Failed);
			return;
		}
		Vector3 vector = allTouches[0].Position;
		if (TBox.HitMove(vector) || TBox.HitRotate(vector) || TBox.HitScale(vector))
		{
			EnterState(GestureState.Failed);
			return;
		}
		if (CharacterEditor.Instance.InEditMode())
		{
			EnterState(GestureState.Failed);
			return;
		}
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(allTouches[0].Position * NormalizedScreen.scale);
		RaycastHit[] array = Physics.RaycastAll(ray);
		if (array.Length == 0)
		{
			EnterState(GestureState.Failed);
			return;
		}
		Array.Sort(array, new RaycastDistanceComparer(ray.origin));
		RaycastHit[] array2 = array;
		foreach (RaycastHit raycastHit in array2)
		{
			Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject);
			if (block != null && !(block is BlockWater) && (Tutorial.state == TutorialState.None || !block.IsLocked() || block.isTerrain) && (!(block is BlockVolume) || Options.BlockVolumeDisplay))
			{
				if (this.DuplicateBegan != null && !this.DuplicateBegan(this, block))
				{
					EnterState(GestureState.Failed);
					return;
				}
				_startMouseBlock = block;
				break;
			}
		}
		if (_startMouseBlock == null)
		{
			EnterState(GestureState.Failed);
			return;
		}
		_startPos = vector;
		if (_startMouseBlock.isTerrain && (!_startMouseBlock.SelectableTerrain() || _startMouseBlock.DisableBuildModeMove()))
		{
			EnterState(GestureState.Failed);
			return;
		}
		EnterState(GestureState.Active);
		_startTime = Time.time;
	}

	public override void TouchesStationary(List<Touch> allTouches)
	{
		if (allTouches.Count != 1)
		{
			EnterState(GestureState.Cancelled);
		}
		else
		{
			if (!(Time.time - _startTime > 0.7f))
			{
				return;
			}
			EnterState(GestureState.Ended);
			Dictionary<GAF, int> d = Scarcity.CalculateWorldGafUsage();
			if (this.DuplicateEnded != null)
			{
				this.DuplicateEnded(this, _startMouseBlock, allTouches[0]);
			}
			if (Blocksworld.selectedBunch == null && Blocksworld.selectedBlock == null)
			{
				return;
			}
			Blocksworld.tBoxGesture.ActivateDrag(allTouches[0]);
			Dictionary<GAF, int> d2 = Scarcity.CalculateWorldGafUsage();
			Dictionary<GAF, int> dictionary = Scarcity.CompareGafUsages(d, d2);
			if (dictionary.Count <= 0)
			{
				return;
			}
			List<GAF> list = new List<GAF>(dictionary.Keys);
			foreach (GAF item in list)
			{
				Scarcity.inventoryScales[item] = 1.5f;
			}
			TBoxGesture.SetGafHighlights(list);
		}
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		Vector2 position = allTouches[0].Position;
		if ((position - _startPos).sqrMagnitude > 400f)
		{
			EnterState(GestureState.Cancelled);
		}
	}

	public override void Reset()
	{
		EnterState(GestureState.Possible);
		_startMouseBlock = null;
	}

	public override string ToString()
	{
		return $"BlockDuplicate({_startMouseBlock})";
	}

	public Block GetStartMouseBlock()
	{
		return _startMouseBlock;
	}
}
