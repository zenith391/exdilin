using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x0200017A RID: 378
	public class CWidgetGesture : BaseGesture
	{
		// Token: 0x060015C3 RID: 5571 RVA: 0x00097570 File Offset: 0x00095970
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (allTouches.Count > 2 || allTouches.Count == 0 || Blocksworld.lockInput || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (Blocksworld.UI.SidePanel.Hit(allTouches[0].Position))
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (allTouches.Count == 1)
			{
				this.touch1 = allTouches[0];
				this.posStart = this.touch1.Position;
				if (Blocksworld.CurrentState == State.Play)
				{
					Ray ray = Blocksworld.mainCamera.ScreenPointToRay(allTouches[0].Position * NormalizedScreen.scale);
					RaycastHit raycastHit;
					if (Physics.Raycast(ray, out raycastHit, 10000f, 234011))
					{
						Rigidbody rigidbody = null;
						if (raycastHit.rigidbody != null)
						{
							rigidbody = raycastHit.rigidbody;
						}
						if (raycastHit.transform.parent != null && raycastHit.transform.parent.GetComponent<Rigidbody>() != null)
						{
							rigidbody = raycastHit.transform.parent.GetComponent<Rigidbody>();
						}
						if (rigidbody != null && !rigidbody.isKinematic)
						{
							base.EnterState(GestureState.Tracking);
							return;
						}
					}
				}
				else if (!CharacterEditor.Instance.InEditMode() && TBox.selected != null)
				{
					if (Blocksworld.tBoxGesture.IsActive && (TBox.tileButtonMove.IsShowing() || TBox.tileButtonRotate.IsShowing() || TBox.tileButtonScale.IsShowing()))
					{
						base.EnterState(GestureState.Failed);
						return;
					}
					if (Blocksworld.selectedBlock == null || !Blocksworld.selectedBlock.DisableBuildModeMove())
					{
						if (TBox.selected.ContainsBlock(Blocksworld.mouseBlock))
						{
							base.EnterState(GestureState.Tracking);
							return;
						}
					}
				}
			}
			else
			{
				this.touch1 = allTouches[0];
				this.touch2 = allTouches[1];
				this.dragDist = 64f;
				if (TBox.IsShowing())
				{
					this.tBoxWasShowing = true;
					TBox.Show(false);
				}
				this.posStart = (this.touch1.Position + this.touch2.Position) / 2f;
				this.distStart = (this.touch1.Position - this.touch2.Position).magnitude;
			}
			this.posLast = this.posStart;
			this.distLast = this.distStart;
			if (CharacterEditor.Instance.InEditMode())
			{
				TBox.tileCharacterEditExitIcon.Show(false);
			}
			base.EnterState(GestureState.Active);
		}

		// Token: 0x060015C4 RID: 5572 RVA: 0x0009783C File Offset: 0x00095C3C
		public override void TouchesMoved(List<Touch> allTouches)
		{
			if (Blocksworld.lockInput || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (base.gestureState == GestureState.Tracking)
			{
				return;
			}
			Vector2 vector = this.touch1.Position;
			if (allTouches.Count == 2)
			{
				if (this.touch2 == null)
				{
					this.touch2 = allTouches[1];
				}
				vector = (this.touch1.Position + this.touch2.Position) / 2f;
			}
			Vector2 vector2 = this.posLast - vector;
			if (this.dragDist < 64f)
			{
				this.dragDist = (vector - this.posStart).sqrMagnitude;
				if (this.dragDist < 64f)
				{
					return;
				}
				this.tBoxWasShowing = TBox.IsShowing();
				if (this.tBoxWasShowing)
				{
					TBox.Show(false);
				}
			}
			bool flag = Blocksworld.tBoxGesture.gestureState == GestureState.Active;
			bool flag2 = Blocksworld.selectedBlock != null && Blocksworld.selectedBlock.DisableBuildModeMove();
			if (!flag || flag2)
			{
				if (allTouches.Count == 2)
				{
					Blocksworld.blocksworldCamera.SetCameraStill(false);
					float magnitude = (this.touch1.Position - this.touch2.Position).magnitude;
					float diff = magnitude - this.distLast;
					Blocksworld.blocksworldCamera.ZoomBy(diff, vector2.magnitude);
					this.distLast = magnitude;
					Blocksworld.blocksworldCamera.PanBy(vector2);
				}
				if (allTouches.Count == 1)
				{
					Blocksworld.blocksworldCamera.SetCameraStill(false);
					Blocksworld.blocksworldCamera.OrbitBy(vector2);
				}
			}
			this.posLast = vector;
		}

		// Token: 0x060015C5 RID: 5573 RVA: 0x000979FC File Offset: 0x00095DFC
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (allTouches.Count == 2 && base.IsActive)
			{
				if (allTouches[0].Phase == TouchPhase.Ended && allTouches[1].Phase == TouchPhase.Ended)
				{
					base.EnterState(GestureState.Ended);
				}
				else
				{
					if (allTouches[0].Phase == TouchPhase.Ended)
					{
						this.touch1 = allTouches[1];
					}
					this.posStart = this.touch1.Position;
					this.posLast = this.posStart;
				}
			}
			if (allTouches.Count == 1)
			{
				base.EnterState(GestureState.Ended);
			}
		}

		// Token: 0x060015C6 RID: 5574 RVA: 0x00097AA0 File Offset: 0x00095EA0
		public override void Reset()
		{
			base.EnterState(GestureState.Possible);
			this.touch1 = (this.touch2 = null);
			this.dragDist = 0f;
			if (CharacterEditor.Instance.InEditMode())
			{
				TBox.tileCharacterEditExitIcon.Show(true);
			}
			else if (this.tBoxWasShowing && TBox.selected != null)
			{
				TBox.Show(true);
			}
			this.tBoxWasShowing = false;
			Tutorial.Step();
		}

		// Token: 0x060015C7 RID: 5575 RVA: 0x00097B15 File Offset: 0x00095F15
		public override string ToString()
		{
			return "CWidget";
		}

		// Token: 0x040010E6 RID: 4326
		private const float sqrDistDrag = 64f;

		// Token: 0x040010E7 RID: 4327
		private Touch touch1;

		// Token: 0x040010E8 RID: 4328
		private Touch touch2;

		// Token: 0x040010E9 RID: 4329
		private Vector2 posStart;

		// Token: 0x040010EA RID: 4330
		private Vector2 posLast;

		// Token: 0x040010EB RID: 4331
		private float distStart;

		// Token: 0x040010EC RID: 4332
		private float distLast;

		// Token: 0x040010ED RID: 4333
		private float dragDist;

		// Token: 0x040010EE RID: 4334
		private const int LAYER_MASK = 234011;

		// Token: 0x040010EF RID: 4335
		private bool tBoxWasShowing;
	}
}
