using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000181 RID: 385
	public class PanelMoveGesture : BaseGesture
	{
		// Token: 0x0600160B RID: 5643 RVA: 0x0009B4DB File Offset: 0x000998DB
		public PanelMoveGesture(Panel panel)
		{
			this._panel = panel;
		}

		// Token: 0x0600160C RID: 5644 RVA: 0x0009B4EC File Offset: 0x000998EC
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (allTouches.Count != 1)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (this._panel.Hit(allTouches[0].Position))
			{
				this._panel.BeginTrackingTouch();
				base.EnterState(GestureState.Active);
			}
			else
			{
				base.EnterState(GestureState.Failed);
			}
		}

		// Token: 0x0600160D RID: 5645 RVA: 0x0009B56C File Offset: 0x0009996C
		public override void TouchesMoved(List<Touch> allTouches)
		{
			Vector2 v = allTouches[0].Position - allTouches[0].LastPosition;
			this._panel.Move(v);
		}

		// Token: 0x0600160E RID: 5646 RVA: 0x0009B5A8 File Offset: 0x000999A8
		public override void TouchesEnded(List<Touch> allTouches)
		{
			this._panel.EndTrackingTouch();
			Tutorial.Step();
			base.EnterState(GestureState.Ended);
		}

		// Token: 0x0600160F RID: 5647 RVA: 0x0009B5C1 File Offset: 0x000999C1
		public override void Reset()
		{
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x06001610 RID: 5648 RVA: 0x0009B5CA File Offset: 0x000999CA
		public override string ToString()
		{
			return "PanelMove";
		}

		// Token: 0x04001129 RID: 4393
		private readonly Panel _panel;
	}
}
