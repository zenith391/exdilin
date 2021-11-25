using System;
using System.Collections.Generic;

namespace Gestures
{
	// Token: 0x02000172 RID: 370
	public abstract class BaseGesture
	{
		// Token: 0x17000063 RID: 99
		// (get) Token: 0x0600158A RID: 5514 RVA: 0x000969A2 File Offset: 0x00094DA2
		public GestureState gestureState
		{
			get
			{
				return this._state;
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x0600158B RID: 5515 RVA: 0x000969AA File Offset: 0x00094DAA
		public bool CanReceiveEvents
		{
			get
			{
				return this.IsEnabled && (this._state == GestureState.Possible || this._state == GestureState.Tracking || this._state == GestureState.Active);
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x0600158C RID: 5516 RVA: 0x000969DD File Offset: 0x00094DDD
		public bool IsStarted
		{
			get
			{
				return this._state == GestureState.Tracking || this._state == GestureState.Active;
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x0600158D RID: 5517 RVA: 0x000969F7 File Offset: 0x00094DF7
		public bool IsFailed
		{
			get
			{
				return this._state == GestureState.Failed;
			}
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x0600158E RID: 5518 RVA: 0x00096A02 File Offset: 0x00094E02
		public bool IsActive
		{
			get
			{
				return this._state == GestureState.Active;
			}
		}

		// Token: 0x0600158F RID: 5519 RVA: 0x00096A0D File Offset: 0x00094E0D
		protected void EnterState(GestureState state)
		{
			this._state = state;
		}

		// Token: 0x06001590 RID: 5520 RVA: 0x00096A16 File Offset: 0x00094E16
		public virtual void TouchesBegan(List<Touch> allTouches)
		{
		}

		// Token: 0x06001591 RID: 5521 RVA: 0x00096A18 File Offset: 0x00094E18
		public virtual void TouchesMoved(List<Touch> allTouches)
		{
		}

		// Token: 0x06001592 RID: 5522 RVA: 0x00096A1A File Offset: 0x00094E1A
		public virtual void TouchesStationary(List<Touch> allTouches)
		{
		}

		// Token: 0x06001593 RID: 5523 RVA: 0x00096A1C File Offset: 0x00094E1C
		public virtual void TouchesEnded(List<Touch> allTouches)
		{
		}

		// Token: 0x06001594 RID: 5524
		public abstract void Reset();

		// Token: 0x06001595 RID: 5525 RVA: 0x00096A1E File Offset: 0x00094E1E
		public virtual void Cancel()
		{
			this.EnterState(GestureState.Cancelled);
		}

		// Token: 0x040010D5 RID: 4309
		private GestureState _state;

		// Token: 0x040010D6 RID: 4310
		public bool IsEnabled = true;

		// Token: 0x040010D7 RID: 4311
		public float touchBeginWindow;
	}
}
