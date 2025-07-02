using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class JoystickControlGesture : BaseGesture
{
	private const float kSensitivity = 0.02f;

	private readonly OldSymbol _left;

	private readonly OldSymbol _right;

	private readonly OldSymbol _up;

	private readonly OldSymbol _down;

	private readonly Tile _joystick;

	private readonly Tile _range;

	private Touch _touch;

	public JoystickControlGesture(OldSymbol left, OldSymbol right, OldSymbol up, OldSymbol down, Tile joystick, Tile range)
	{
		_left = left;
		_right = right;
		_up = up;
		_down = down;
		_joystick = joystick;
		_range = range;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (_joystick.tileObject == null)
		{
			EnterState(GestureState.Failed);
		}
		foreach (Touch allTouch in allTouches)
		{
			if (allTouch.Phase == TouchPhase.Began && _joystick.HitExtended(allTouch.Position, 40f, 40f, 40f, 40f))
			{
				_touch = allTouch;
				break;
			}
		}
		if (_touch != null)
		{
			EnterState(GestureState.Active);
			UpdateJoystick();
		}
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		UpdateJoystick();
	}

	public override void TouchesStationary(List<Touch> allTouches)
	{
		UpdateJoystick();
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (_touch != null && _touch.Phase == TouchPhase.Ended)
		{
			_joystick.MoveTo(_range.tileObject.GetPosition());
			ClearJoystickValues();
			_touch = null;
		}
		foreach (Touch allTouch in allTouches)
		{
			if (allTouch.Phase != TouchPhase.Ended)
			{
				return;
			}
		}
		EnterState(GestureState.Ended);
	}

	public override void Reset()
	{
		_touch = null;
		EnterState(GestureState.Possible);
	}

	public override string ToString()
	{
		return $"JoystickControl({_joystick.gaf}, {_touch != null})";
	}

	private void UpdateJoystick()
	{
		if (base.gestureState == GestureState.Active && _touch != null)
		{
			int num = 80;
			_joystick.MoveTo(_touch.Position.x - 0.5f * (float)num, _touch.Position.y - 0.5f * (float)num);
			Vector2 vector = _range.tileObject.GetPosition() - _joystick.tileObject.GetPosition();
			float value = Mathf.Min(1f, 0.02f * Mathf.Abs(vector.x));
			float value2 = Mathf.Min(1f, 0.02f * Mathf.Abs(vector.y));
			ClearJoystickValues();
			Blocksworld.joysticks[(vector.x <= 0f) ? _right : _left] = value;
			Blocksworld.joysticks[(vector.y <= 0f) ? _up : _down] = value2;
		}
	}

	private void ClearJoystickValues()
	{
		Blocksworld.joysticks[_left] = 0f;
		Blocksworld.joysticks[_right] = 0f;
		Blocksworld.joysticks[_up] = 0f;
		Blocksworld.joysticks[_down] = 0f;
	}
}
