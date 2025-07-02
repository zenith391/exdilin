using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class TileTapGesture : BaseGesture
{
	private readonly Tile _tile;

	private readonly bool _allowMultiTouch;

	private readonly bool _allowDisabledTiles;

	private Touch _touch;

	private float _extendXMin;

	private float _extendXMax;

	private float _extendYMin;

	private float _extendYMax;

	private bool useExtendedHit;

	public event TileTapGestureDelegate TapEnded;

	public TileTapGesture(Tile tile, TileTapGestureDelegate tapEnded, bool allowMultiTouch = false, bool allowDisabledTiles = false)
	{
		TapEnded += tapEnded;
		_tile = tile;
		_allowMultiTouch = allowMultiTouch;
		_allowDisabledTiles = allowDisabledTiles;
	}

	public void SetExtendedHit(float extendXMin, float extendXMax, float extendYMin, float extendYMax)
	{
		_extendXMin = extendXMin;
		_extendXMax = extendXMax;
		_extendYMin = extendYMin;
		_extendYMax = extendYMax;
		useExtendedHit = true;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (_touch != null)
		{
			return;
		}
		if (!_allowMultiTouch && allTouches.Count != 1)
		{
			EnterState(GestureState.Failed);
			return;
		}
		if (Blocksworld.InModalDialogState())
		{
			EnterState(GestureState.Failed);
			return;
		}
		foreach (Touch allTouch in allTouches)
		{
			if (allTouch.Phase == TouchPhase.Began && Hit(allTouch.Position))
			{
				EnterState(GestureState.Active);
				_touch = allTouch;
			}
		}
		if (_touch == null)
		{
			EnterState(_allowMultiTouch ? GestureState.Tracking : GestureState.Failed);
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (_touch != null && _touch.Phase == TouchPhase.Ended)
		{
			if (Hit(_touch.Position) && this.TapEnded != null)
			{
				this.TapEnded(this, _tile);
			}
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

	private bool Hit(Vector2 pos)
	{
		if (useExtendedHit)
		{
			return _tile.HitExtended(pos, _extendXMin, _extendXMax, _extendYMin, _extendYMax, _allowDisabledTiles);
		}
		return _tile.Hit(pos, _allowDisabledTiles);
	}

	public override void Reset()
	{
		_touch = null;
		EnterState(GestureState.Possible);
	}

	public override string ToString()
	{
		return $"TileTap({_tile.gaf})";
	}
}
