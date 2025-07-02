using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures;

public class BlockTapGesture : BaseGesture
{
	private Block _startMouseBlock;

	private Vector2 _startPos;

	private Vector3 _startCameraForward;

	public event BlockTapBeginDelegate TapBegan;

	public event BlockTapGestureDelegate TapEnded;

	public BlockTapGesture(BlockTapBeginDelegate tapBegan, BlockTapGestureDelegate tapEnded)
	{
		TapBegan += tapBegan;
		TapEnded += tapEnded;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (allTouches.Count != 1 || Blocksworld.InModalDialogState())
		{
			EnterState(GestureState.Failed);
			return;
		}
		Vector2 position = allTouches[0].Position;
		if (Blocksworld.UI.SidePanel.Hit(position))
		{
			EnterState(GestureState.Failed);
			return;
		}
		if (CharacterEditor.Instance.InEditMode())
		{
			EnterState(GestureState.Failed);
			return;
		}
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(position * NormalizedScreen.scale);
		RaycastHit[] array = Physics.RaycastAll(ray);
		if (array.Length == 0)
		{
			EnterState(GestureState.Failed);
			return;
		}
		float num = float.MaxValue;
		Block block = null;
		Array.Sort(array, new RaycastDistanceComparer(ray.origin));
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			Block block2 = BWSceneManager.FindBlock(raycastHit.collider.gameObject);
			float num2 = raycastHit.distance;
			if (block2 != null && block2 == Blocksworld.mouseBlock)
			{
				num2 -= 0.1f;
			}
			if (block2 is BlockWater { isSolid: false })
			{
				num2 += 50f;
			}
			if (num2 <= num && block2 != null && (Tutorial.state == TutorialState.None || !block2.IsLocked() || block2.isTerrain) && (!(block2 is BlockVolume) || Options.BlockVolumeDisplay))
			{
				block = block2;
				num = num2;
			}
		}
		if (block != null)
		{
			if (this.TapBegan != null && !this.TapBegan(this, block))
			{
				Blocksworld.mouseBlock = block;
				EnterState(GestureState.Failed);
				return;
			}
			_startMouseBlock = block;
		}
		if (_startMouseBlock == null)
		{
			EnterState(GestureState.Failed);
			return;
		}
		_startPos = allTouches[0].Position;
		if (_startMouseBlock.isTerrain)
		{
			EnterState(GestureState.Tracking);
		}
		else
		{
			EnterState(GestureState.Active);
		}
		_startCameraForward = Blocksworld.cameraTransform.forward;
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		if (base.gestureState == GestureState.Tracking)
		{
			Vector2 position = allTouches[0].Position;
			if ((position - _startPos).sqrMagnitude > 400f)
			{
				EnterState(GestureState.Cancelled);
				return;
			}
		}
		if (base.gestureState == GestureState.Active)
		{
			Vector3 forward = Blocksworld.cameraTransform.forward;
			if (Vector3.Dot(_startCameraForward, forward) < 0.99f)
			{
				EnterState(GestureState.Cancelled);
			}
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		EnterState(GestureState.Ended);
		if (this.TapEnded != null)
		{
			Blocksworld.mouseBlock = _startMouseBlock;
			this.TapEnded(this, _startMouseBlock);
		}
	}

	public override void Reset()
	{
		EnterState(GestureState.Possible);
		_startMouseBlock = null;
	}

	public override string ToString()
	{
		return $"BlockTap({_startMouseBlock})";
	}

	public Block GetStartMouseBlock()
	{
		return _startMouseBlock;
	}
}
