using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class ParameterEditGesture : BaseGesture
{
	private Vector2 startPosition;

	private NumericHandleTileParameter targetParameter;

	private bool holdingHandle;

	public void StartEditing(NumericHandleTileParameter parameter)
	{
		targetParameter = parameter;
		IsEnabled = true;
		EnterState(GestureState.Possible);
		holdingHandle = false;
	}

	private void StopEditor()
	{
		if (Blocksworld.bw.tileParameterEditor.IsEditing())
		{
			Sound.PlayOneShotSound("Slider Parameter Changed");
			Blocksworld.bw.tileParameterEditor.StopEditing();
		}
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (targetParameter == null || Blocksworld.CurrentState != State.Build)
		{
			EnterState(GestureState.Failed);
			return;
		}
		if (allTouches.Count > 1)
		{
			EnterState(GestureState.Failed);
			StopEditor();
			return;
		}
		Touch touch = allTouches[0];
		if (touch.Phase == TouchPhase.Began)
		{
			Vector2 position = touch.Position;
			if (targetParameter.handle.Hit(position))
			{
				EnterState(GestureState.Active);
				targetParameter.GrabHandle(position);
				holdingHandle = true;
			}
			else if (targetParameter.tile.Hit(position) || targetParameter.rightSide.Hit(position, allowDisabledTiles: true))
			{
				EnterState(GestureState.Active);
			}
			else
			{
				StopEditor();
				EnterState(GestureState.Cancelled);
			}
		}
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		if (targetParameter == null)
		{
			StopEditor();
			EnterState(GestureState.Failed);
		}
		else if (holdingHandle)
		{
			targetParameter.HoldHandle(allTouches[0].Position);
		}
	}

	public override void TouchesStationary(List<Touch> allTouches)
	{
		if (targetParameter == null)
		{
			EnterState(GestureState.Failed);
		}
		else if (holdingHandle)
		{
			targetParameter.HoldHandle(allTouches[0].Position);
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (targetParameter == null)
		{
			EnterState(GestureState.Failed);
			StopEditor();
			return;
		}
		EnterState(GestureState.Possible);
		Vector2 position = allTouches[0].Position;
		if (holdingHandle)
		{
			targetParameter.ReleaseHandle();
			holdingHandle = false;
		}
		else if (targetParameter.tile.Hit(position) || targetParameter.rightSide.Hit(position, allowDisabledTiles: true))
		{
			targetParameter.tile.StepSubParameterIndex();
			Blocksworld.BlockPanelTileTapped(targetParameter.tile);
		}
		bool flag = Tutorial.state == TutorialState.SetParameter;
		Tutorial.Step();
		if (flag && Tutorial.state != TutorialState.SetParameter && Tutorial.state != TutorialState.TapTile)
		{
			Blocksworld.bw.tileParameterEditor.StopEditing();
			EnterState(GestureState.Cancelled);
		}
		TBox.UpdateCopyButtonVisibility();
	}

	public override void Reset()
	{
		targetParameter = null;
		holdingHandle = false;
		EnterState(GestureState.Possible);
	}
}
