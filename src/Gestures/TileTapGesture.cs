using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x0200018D RID: 397
	public class TileTapGesture : BaseGesture
	{
		// Token: 0x0600167A RID: 5754 RVA: 0x000A076D File Offset: 0x0009EB6D
		public TileTapGesture(Tile tile, TileTapGestureDelegate tapEnded, bool allowMultiTouch = false, bool allowDisabledTiles = false)
		{
			this.TapEnded += tapEnded;
			this._tile = tile;
			this._allowMultiTouch = allowMultiTouch;
			this._allowDisabledTiles = allowDisabledTiles;
		}

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x0600167B RID: 5755 RVA: 0x000A0794 File Offset: 0x0009EB94
		// (remove) Token: 0x0600167C RID: 5756 RVA: 0x000A07CC File Offset: 0x0009EBCC
		public event TileTapGestureDelegate TapEnded;

		// Token: 0x0600167D RID: 5757 RVA: 0x000A0802 File Offset: 0x0009EC02
		public void SetExtendedHit(float extendXMin, float extendXMax, float extendYMin, float extendYMax)
		{
			this._extendXMin = extendXMin;
			this._extendXMax = extendXMax;
			this._extendYMin = extendYMin;
			this._extendYMax = extendYMax;
			this.useExtendedHit = true;
		}

		// Token: 0x0600167E RID: 5758 RVA: 0x000A0828 File Offset: 0x0009EC28
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (this._touch != null)
			{
				return;
			}
			if (!this._allowMultiTouch && allTouches.Count != 1)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			foreach (Touch touch in allTouches)
			{
				if (touch.Phase == TouchPhase.Began && this.Hit(touch.Position))
				{
					base.EnterState(GestureState.Active);
					this._touch = touch;
				}
			}
			if (this._touch == null)
			{
				base.EnterState((!this._allowMultiTouch) ? GestureState.Failed : GestureState.Tracking);
			}
		}

		// Token: 0x0600167F RID: 5759 RVA: 0x000A0904 File Offset: 0x0009ED04
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (this._touch != null && this._touch.Phase == TouchPhase.Ended)
			{
				if (this.Hit(this._touch.Position) && this.TapEnded != null)
				{
					this.TapEnded(this, this._tile);
				}
				this._touch = null;
			}
			foreach (Touch touch in allTouches)
			{
				if (touch.Phase != TouchPhase.Ended)
				{
					return;
				}
			}
			base.EnterState(GestureState.Ended);
		}

		// Token: 0x06001680 RID: 5760 RVA: 0x000A09C4 File Offset: 0x0009EDC4
		private bool Hit(Vector2 pos)
		{
			if (this.useExtendedHit)
			{
				return this._tile.HitExtended(pos, this._extendXMin, this._extendXMax, this._extendYMin, this._extendYMax, this._allowDisabledTiles);
			}
			return this._tile.Hit(pos, this._allowDisabledTiles);
		}

		// Token: 0x06001681 RID: 5761 RVA: 0x000A0A23 File Offset: 0x0009EE23
		public override void Reset()
		{
			this._touch = null;
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x06001682 RID: 5762 RVA: 0x000A0A33 File Offset: 0x0009EE33
		public override string ToString()
		{
			return string.Format("TileTap({0})", this._tile.gaf);
		}

		// Token: 0x0400118D RID: 4493
		private readonly Tile _tile;

		// Token: 0x0400118E RID: 4494
		private readonly bool _allowMultiTouch;

		// Token: 0x0400118F RID: 4495
		private readonly bool _allowDisabledTiles;

		// Token: 0x04001190 RID: 4496
		private Touch _touch;

		// Token: 0x04001191 RID: 4497
		private float _extendXMin;

		// Token: 0x04001192 RID: 4498
		private float _extendXMax;

		// Token: 0x04001193 RID: 4499
		private float _extendYMin;

		// Token: 0x04001194 RID: 4500
		private float _extendYMax;

		// Token: 0x04001195 RID: 4501
		private bool useExtendedHit;
	}
}
