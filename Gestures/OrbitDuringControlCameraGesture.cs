using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000180 RID: 384
	public class OrbitDuringControlCameraGesture : BaseGesture
	{
		// Token: 0x06001606 RID: 5638 RVA: 0x0009B29C File Offset: 0x0009969C
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (this._touch != null)
			{
				return;
			}
			if (Blocksworld.lockInput)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (!Blocksworld.UI.Controls.AnyControlActive())
			{
				base.EnterState(GestureState.Tracking);
				return;
			}
			for (int i = 0; i < allTouches.Count; i++)
			{
				if (!Blocksworld.UI.Controls.DPadOwnsTouch(i))
				{
					bool flag = !Blocksworld.UI.Controls.ControlOwnsTouch(i);
					if (flag)
					{
						this._touch = allTouches[i];
						break;
					}
				}
			}
			if (this._touch == null)
			{
				base.EnterState(GestureState.Possible);
			}
			else
			{
				this._startPos = this._touch.Position;
				base.EnterState(GestureState.Tracking);
			}
		}

		// Token: 0x06001607 RID: 5639 RVA: 0x0009B370 File Offset: 0x00099770
		public override void TouchesMoved(List<Touch> allTouches)
		{
			if (this._touch == null)
			{
				return;
			}
			if (base.gestureState == GestureState.Tracking && !Blocksworld.UI.Controls.AnyControlActive())
			{
				base.EnterState(GestureState.Possible);
				this._touch = null;
				return;
			}
			if (base.gestureState == GestureState.Tracking && (this._touch.Position - this._startPos).sqrMagnitude >= 100f)
			{
				base.EnterState(GestureState.Active);
			}
			if (base.gestureState == GestureState.Active)
			{
				Vector2 posDiff = this._touch.LastPosition - this._touch.Position;
				Blocksworld.blocksworldCamera.OrbitBy(posDiff);
			}
		}

		// Token: 0x06001608 RID: 5640 RVA: 0x0009B428 File Offset: 0x00099828
		public override void TouchesEnded(List<Touch> allTouches)
		{
			foreach (Touch touch in allTouches)
			{
				if (touch == this._touch && touch.Phase == TouchPhase.Ended)
				{
					this._touch = null;
					if (Blocksworld.UI.Controls.AnyControlActive())
					{
						base.EnterState(GestureState.Possible);
					}
					else
					{
						base.EnterState(GestureState.Failed);
					}
					break;
				}
			}
		}

		// Token: 0x06001609 RID: 5641 RVA: 0x0009B4C4 File Offset: 0x000998C4
		public override void Reset()
		{
			this._touch = null;
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x0600160A RID: 5642 RVA: 0x0009B4D4 File Offset: 0x000998D4
		public override string ToString()
		{
			return "OrbitDuringControlCameraGesture";
		}

		// Token: 0x04001127 RID: 4391
		private Vector2 _startPos;

		// Token: 0x04001128 RID: 4392
		private Touch _touch;
	}
}
