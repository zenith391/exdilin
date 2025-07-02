using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class PanelScrollGesture : BaseGesture
{
	private const float TrackingToleranceYUp = 10000f;

	private const float TrackingToleranceYDown = 10000f;

	private const float TrackingToleranceX = 10000f;

	private readonly BuildPanel _buildPanel;

	private readonly Panel _ignorePanel;

	private Vector2 _startPosition;

	private bool _startedAtTop;

	private bool _startedAtBottom;

	private Tile startTile;

	public PanelScrollGesture(BuildPanel buildPanel, Panel ignorePanel)
	{
		_buildPanel = buildPanel;
		_ignorePanel = ignorePanel;
		Reset();
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
			if (base.IsStarted)
			{
				SnapBackPosition();
			}
			EnterState(GestureState.Failed);
			return;
		}
		Vector2 position = allTouches[0].Position;
		if (!Blocksworld.UI.SidePanel.HitBuildPanel(position) || _ignorePanel.Hit(position))
		{
			EnterState(GestureState.Failed);
			return;
		}
		_buildPanel.trackingTouch = true;
		startTile = _buildPanel.HitTile(position, allowDisabledTiles: true);
		_startPosition = position;
		_startedAtTop = _buildPanel.AtTopLimit();
		_startedAtBottom = _buildPanel.AtBottomLimit();
		if (_startedAtTop && _startedAtBottom)
		{
			if (startTile != null)
			{
				CheckForZeroInventoryTileTap(startTile);
			}
			EnterState(GestureState.Failed);
		}
		else if (_buildPanel.HitTile(position) == null)
		{
			EnterState(GestureState.Active);
		}
		else
		{
			EnterState(GestureState.Tracking);
		}
	}

	public override void TouchesStationary(List<Touch> allTouches)
	{
		TouchesMoved(allTouches);
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		Touch touch = allTouches[0];
		if (base.gestureState == GestureState.Tracking)
		{
			Vector2 vector = touch.Position - _startPosition;
			if (vector.y > 10000f)
			{
				EnterState(GestureState.Cancelled);
				SnapBackPosition();
				return;
			}
			if (Mathf.Abs(vector.x) > 10000f || vector.y < -10000f)
			{
				EnterState(GestureState.Active);
			}
			if (!_buildPanel.Hit(touch.Position) || _ignorePanel.Hit(touch.Position))
			{
				EnterState(GestureState.Failed);
				SnapBackPosition();
				return;
			}
		}
		float num = touch.Position.y - touch.LastPosition.y;
		if ((Blocksworld.tileDragGesture.IsActive || Blocksworld.createTileDragGesture.IsActive) && ((_startedAtTop && _buildPanel.AtTopLimit() && num < 0f) || (_startedAtBottom && _buildPanel.AtBottomLimit() && num > 0f)))
		{
			EnterState(GestureState.Cancelled);
			SnapBackPosition();
		}
		else
		{
			_buildPanel.Move(new Vector3(0f, num, 0f));
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (allTouches.Count > 0 && startTile != null && (_startPosition - allTouches[0].Position).magnitude < 20f)
		{
			CheckForZeroInventoryTileTap(startTile);
		}
		Tutorial.Step();
		_buildPanel.EndTrackingTouch();
		EnterState(GestureState.Ended);
	}

	private void CheckForZeroInventoryTileTap(Tile tile)
	{
		if (Scarcity.inventory != null)
		{
			GAF gaf = tile.gaf;
			if (Scarcity.inventory.ContainsKey(gaf) && Scarcity.inventory[gaf] == 0)
			{
				Blocksworld.ZeroInventoryTileTapped(startTile);
			}
		}
	}

	public override void Reset()
	{
		startTile = null;
		_startedAtBottom = false;
		_startedAtTop = false;
		EnterState(GestureState.Possible);
	}

	public override string ToString()
	{
		return "PanelScroll";
	}

	private void SnapBackPosition()
	{
		_buildPanel.EndTrackingTouch();
	}
}
