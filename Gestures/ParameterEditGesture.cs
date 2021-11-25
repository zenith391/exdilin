using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000183 RID: 387
	public class ParameterEditGesture : BaseGesture
	{
		// Token: 0x0600161B RID: 5659 RVA: 0x0009B9AF File Offset: 0x00099DAF
		public void StartEditing(NumericHandleTileParameter parameter)
		{
			this.targetParameter = parameter;
			this.IsEnabled = true;
			base.EnterState(GestureState.Possible);
			this.holdingHandle = false;
		}

		// Token: 0x0600161C RID: 5660 RVA: 0x0009B9CD File Offset: 0x00099DCD
		private void StopEditor()
		{
			if (Blocksworld.bw.tileParameterEditor.IsEditing())
			{
				Sound.PlayOneShotSound("Slider Parameter Changed", 1f);
				Blocksworld.bw.tileParameterEditor.StopEditing();
			}
		}

		// Token: 0x0600161D RID: 5661 RVA: 0x0009BA04 File Offset: 0x00099E04
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (this.targetParameter == null || Blocksworld.CurrentState != State.Build)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (allTouches.Count > 1)
			{
				base.EnterState(GestureState.Failed);
				this.StopEditor();
				return;
			}
			Touch touch = allTouches[0];
			if (touch.Phase != TouchPhase.Began)
			{
				return;
			}
			Vector2 position = touch.Position;
			if (this.targetParameter.handle.Hit(position, false))
			{
				base.EnterState(GestureState.Active);
				this.targetParameter.GrabHandle(position);
				this.holdingHandle = true;
			}
			else if (this.targetParameter.tile.Hit(position, false) || this.targetParameter.rightSide.Hit(position, true))
			{
				base.EnterState(GestureState.Active);
			}
			else
			{
				this.StopEditor();
				base.EnterState(GestureState.Cancelled);
			}
		}

		// Token: 0x0600161E RID: 5662 RVA: 0x0009BAEF File Offset: 0x00099EEF
		public override void TouchesMoved(List<Touch> allTouches)
		{
			if (this.targetParameter == null)
			{
				this.StopEditor();
				base.EnterState(GestureState.Failed);
				return;
			}
			if (this.holdingHandle)
			{
				this.targetParameter.HoldHandle(allTouches[0].Position);
			}
		}

		// Token: 0x0600161F RID: 5663 RVA: 0x0009BB2C File Offset: 0x00099F2C
		public override void TouchesStationary(List<Touch> allTouches)
		{
			if (this.targetParameter == null)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (this.holdingHandle)
			{
				this.targetParameter.HoldHandle(allTouches[0].Position);
			}
		}

		// Token: 0x06001620 RID: 5664 RVA: 0x0009BB64 File Offset: 0x00099F64
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (this.targetParameter == null)
			{
				base.EnterState(GestureState.Failed);
				this.StopEditor();
				return;
			}
			base.EnterState(GestureState.Possible);
			Vector2 position = allTouches[0].Position;
			if (this.holdingHandle)
			{
				this.targetParameter.ReleaseHandle();
				this.holdingHandle = false;
			}
			else if (this.targetParameter.tile.Hit(position, false) || this.targetParameter.rightSide.Hit(position, true))
			{
				this.targetParameter.tile.StepSubParameterIndex();
				Blocksworld.BlockPanelTileTapped(this.targetParameter.tile);
			}
			bool flag = Tutorial.state == TutorialState.SetParameter;
			Tutorial.Step();
			if (flag && Tutorial.state != TutorialState.SetParameter && Tutorial.state != TutorialState.TapTile)
			{
				Blocksworld.bw.tileParameterEditor.StopEditing();
				base.EnterState(GestureState.Cancelled);
			}
			TBox.UpdateCopyButtonVisibility();
		}

		// Token: 0x06001621 RID: 5665 RVA: 0x0009BC5F File Offset: 0x0009A05F
		public override void Reset()
		{
			this.targetParameter = null;
			this.holdingHandle = false;
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x04001133 RID: 4403
		private Vector2 startPosition;

		// Token: 0x04001134 RID: 4404
		private NumericHandleTileParameter targetParameter;

		// Token: 0x04001135 RID: 4405
		private bool holdingHandle;
	}
}
