using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class PanelMoveGesture : BaseGesture
{
	private readonly Panel _panel;

	public PanelMoveGesture(Panel panel)
	{
		_panel = panel;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (allTouches.Count != 1)
		{
			EnterState(GestureState.Failed);
		}
		else if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
		{
			EnterState(GestureState.Failed);
		}
		else if (_panel.Hit(allTouches[0].Position))
		{
			_panel.BeginTrackingTouch();
			EnterState(GestureState.Active);
		}
		else
		{
			EnterState(GestureState.Failed);
		}
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		Vector2 vector = allTouches[0].Position - allTouches[0].LastPosition;
		_panel.Move(vector);
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		_panel.EndTrackingTouch();
		Tutorial.Step();
		EnterState(GestureState.Ended);
	}

	public override void Reset()
	{
		EnterState(GestureState.Possible);
	}

	public override string ToString()
	{
		return "PanelMove";
	}
}
