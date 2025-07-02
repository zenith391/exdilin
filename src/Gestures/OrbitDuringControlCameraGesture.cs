using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class OrbitDuringControlCameraGesture : BaseGesture
{
	private Vector2 _startPos;

	private Touch _touch;

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (_touch != null)
		{
			return;
		}
		if (Blocksworld.lockInput)
		{
			EnterState(GestureState.Failed);
			return;
		}
		if (!Blocksworld.UI.Controls.AnyControlActive())
		{
			EnterState(GestureState.Tracking);
			return;
		}
		for (int i = 0; i < allTouches.Count; i++)
		{
			if (!Blocksworld.UI.Controls.DPadOwnsTouch(i) && !Blocksworld.UI.Controls.ControlOwnsTouch(i))
			{
				_touch = allTouches[i];
				break;
			}
		}
		if (_touch == null)
		{
			EnterState(GestureState.Possible);
			return;
		}
		_startPos = _touch.Position;
		EnterState(GestureState.Tracking);
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		if (_touch == null)
		{
			return;
		}
		if (base.gestureState == GestureState.Tracking && !Blocksworld.UI.Controls.AnyControlActive())
		{
			EnterState(GestureState.Possible);
			_touch = null;
			return;
		}
		if (base.gestureState == GestureState.Tracking && (_touch.Position - _startPos).sqrMagnitude >= 100f)
		{
			EnterState(GestureState.Active);
		}
		if (base.gestureState == GestureState.Active)
		{
			Vector2 posDiff = _touch.LastPosition - _touch.Position;
			Blocksworld.blocksworldCamera.OrbitBy(posDiff);
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		foreach (Touch allTouch in allTouches)
		{
			if (allTouch == _touch && allTouch.Phase == TouchPhase.Ended)
			{
				_touch = null;
				if (Blocksworld.UI.Controls.AnyControlActive())
				{
					EnterState(GestureState.Possible);
				}
				else
				{
					EnterState(GestureState.Failed);
				}
				break;
			}
		}
	}

	public override void Reset()
	{
		_touch = null;
		EnterState(GestureState.Possible);
	}

	public override string ToString()
	{
		return "OrbitDuringControlCameraGesture";
	}
}
