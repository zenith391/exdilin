using System.Collections.Generic;

namespace Gestures;

public abstract class BaseGesture
{
	private GestureState _state;

	public bool IsEnabled = true;

	public float touchBeginWindow;

	public GestureState gestureState => _state;

	public bool CanReceiveEvents
	{
		get
		{
			if (IsEnabled)
			{
				if (_state != GestureState.Possible && _state != GestureState.Tracking)
				{
					return _state == GestureState.Active;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsStarted
	{
		get
		{
			if (_state != GestureState.Tracking)
			{
				return _state == GestureState.Active;
			}
			return true;
		}
	}

	public bool IsFailed => _state == GestureState.Failed;

	public bool IsActive => _state == GestureState.Active;

	protected void EnterState(GestureState state)
	{
		_state = state;
	}

	public virtual void TouchesBegan(List<Touch> allTouches)
	{
	}

	public virtual void TouchesMoved(List<Touch> allTouches)
	{
	}

	public virtual void TouchesStationary(List<Touch> allTouches)
	{
	}

	public virtual void TouchesEnded(List<Touch> allTouches)
	{
	}

	public abstract void Reset();

	public virtual void Cancel()
	{
		EnterState(GestureState.Cancelled);
	}
}
