using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000182 RID: 386
	public class PanelScrollGesture : BaseGesture
	{
		// Token: 0x06001611 RID: 5649 RVA: 0x0009B5D1 File Offset: 0x000999D1
		public PanelScrollGesture(BuildPanel buildPanel, Panel ignorePanel)
		{
			this._buildPanel = buildPanel;
			this._ignorePanel = ignorePanel;
			this.Reset();
		}

		// Token: 0x06001612 RID: 5650 RVA: 0x0009B5F0 File Offset: 0x000999F0
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (allTouches.Count != 1)
			{
				if (base.IsStarted)
				{
					this.SnapBackPosition();
				}
				base.EnterState(GestureState.Failed);
				return;
			}
			Vector2 position = allTouches[0].Position;
			bool flag = Blocksworld.UI.SidePanel.HitBuildPanel(position);
			if (!flag || this._ignorePanel.Hit(position))
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			this._buildPanel.trackingTouch = true;
			this.startTile = this._buildPanel.HitTile(position, true);
			this._startPosition = position;
			this._startedAtTop = this._buildPanel.AtTopLimit();
			this._startedAtBottom = this._buildPanel.AtBottomLimit();
			if (this._startedAtTop && this._startedAtBottom)
			{
				if (this.startTile != null)
				{
					this.CheckForZeroInventoryTileTap(this.startTile);
				}
				base.EnterState(GestureState.Failed);
				return;
			}
			if (this._buildPanel.HitTile(position, false) == null)
			{
				base.EnterState(GestureState.Active);
			}
			else
			{
				base.EnterState(GestureState.Tracking);
			}
		}

		// Token: 0x06001613 RID: 5651 RVA: 0x0009B732 File Offset: 0x00099B32
		public override void TouchesStationary(List<Touch> allTouches)
		{
			this.TouchesMoved(allTouches);
		}

		// Token: 0x06001614 RID: 5652 RVA: 0x0009B73C File Offset: 0x00099B3C
		public override void TouchesMoved(List<Touch> allTouches)
		{
			Touch touch = allTouches[0];
			if (base.gestureState == GestureState.Tracking)
			{
				Vector2 vector = touch.Position - this._startPosition;
				if (vector.y > 10000f)
				{
					base.EnterState(GestureState.Cancelled);
					this.SnapBackPosition();
					return;
				}
				if (Mathf.Abs(vector.x) > 10000f || vector.y < -10000f)
				{
					base.EnterState(GestureState.Active);
				}
				if (!this._buildPanel.Hit(touch.Position) || this._ignorePanel.Hit(touch.Position))
				{
					base.EnterState(GestureState.Failed);
					this.SnapBackPosition();
					return;
				}
			}
			float num = touch.Position.y - touch.LastPosition.y;
			bool flag = Blocksworld.tileDragGesture.IsActive || Blocksworld.createTileDragGesture.IsActive;
			if (flag)
			{
				bool flag2 = (this._startedAtTop && this._buildPanel.AtTopLimit() && num < 0f) || (this._startedAtBottom && this._buildPanel.AtBottomLimit() && num > 0f);
				if (flag2)
				{
					base.EnterState(GestureState.Cancelled);
					this.SnapBackPosition();
					return;
				}
			}
			this._buildPanel.Move(new Vector3(0f, num, 0f));
		}

		// Token: 0x06001615 RID: 5653 RVA: 0x0009B8BC File Offset: 0x00099CBC
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (allTouches.Count > 0 && this.startTile != null && (this._startPosition - allTouches[0].Position).magnitude < 20f)
			{
				this.CheckForZeroInventoryTileTap(this.startTile);
			}
			Tutorial.Step();
			this._buildPanel.EndTrackingTouch();
			base.EnterState(GestureState.Ended);
		}

		// Token: 0x06001616 RID: 5654 RVA: 0x0009B92C File Offset: 0x00099D2C
		private void CheckForZeroInventoryTileTap(Tile tile)
		{
			if (Scarcity.inventory != null)
			{
				GAF gaf = tile.gaf;
				if (Scarcity.inventory.ContainsKey(gaf) && Scarcity.inventory[gaf] == 0)
				{
					Blocksworld.ZeroInventoryTileTapped(this.startTile);
				}
			}
		}

		// Token: 0x06001617 RID: 5655 RVA: 0x0009B975 File Offset: 0x00099D75
		public override void Reset()
		{
			this.startTile = null;
			this._startedAtBottom = false;
			this._startedAtTop = false;
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x06001618 RID: 5656 RVA: 0x0009B993 File Offset: 0x00099D93
		public override string ToString()
		{
			return "PanelScroll";
		}

		// Token: 0x06001619 RID: 5657 RVA: 0x0009B99A File Offset: 0x00099D9A
		private void SnapBackPosition()
		{
			this._buildPanel.EndTrackingTouch();
		}

		// Token: 0x0400112A RID: 4394
		private const float TrackingToleranceYUp = 10000f;

		// Token: 0x0400112B RID: 4395
		private const float TrackingToleranceYDown = 10000f;

		// Token: 0x0400112C RID: 4396
		private const float TrackingToleranceX = 10000f;

		// Token: 0x0400112D RID: 4397
		private readonly BuildPanel _buildPanel;

		// Token: 0x0400112E RID: 4398
		private readonly Panel _ignorePanel;

		// Token: 0x0400112F RID: 4399
		private Vector2 _startPosition;

		// Token: 0x04001130 RID: 4400
		private bool _startedAtTop;

		// Token: 0x04001131 RID: 4401
		private bool _startedAtBottom;

		// Token: 0x04001132 RID: 4402
		private Tile startTile;
	}
}
