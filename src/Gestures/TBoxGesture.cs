using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000188 RID: 392
	public class TBoxGesture : BaseGesture
	{
		// Token: 0x06001647 RID: 5703 RVA: 0x0009D613 File Offset: 0x0009BA13
		public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
		{
			if (TBoxGesture.highlights != null)
			{
				if (result == null)
				{
					result = new HashSet<GAF>();
				}
				result.UnionWith(TBoxGesture.highlights);
			}
			return result;
		}

		// Token: 0x06001648 RID: 5704 RVA: 0x0009D638 File Offset: 0x0009BA38
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (CharacterEditor.Instance.InEditMode())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (allTouches.Count > 2 || TBox.selected == null)
			{
				if (base.gestureState == GestureState.Active)
				{
					this.Stop();
				}
				base.EnterState(GestureState.Failed);
				return;
			}
			if (allTouches.Count > 1 && this.raycastDragging)
			{
				this.Stop();
				base.EnterState(GestureState.Failed);
				return;
			}
			if (Blocksworld.selectedBlock != null && Blocksworld.selectedBlock.go == null)
			{
				BWLog.Info("TBoxGesture trying to do something with a destroyed block");
				this.Stop();
				base.EnterState(GestureState.Failed);
				return;
			}
			if (Blocksworld.mouseBlock != null && Blocksworld.mouseBlock.go == null)
			{
				BWLog.Info("mouseBlock is a destroyed block");
				this.Stop();
				base.EnterState(GestureState.Failed);
				return;
			}
			if (this.touch == null)
			{
				Vector3 pos = allTouches[0].Position;
				if (TBox.HitRotate(pos))
				{
					this.touch = allTouches[0];
					this.tBox = false;
					this.startTouchPos = this.touch.Position;
					this.startButtonPos = TBox.tileButtonRotate.GetPosition();
					TBox.StartRotate(this.touch.Position, TBox.RotateMode.Button);
					TBox.tileButtonMove.Hide();
					TBox.tileButtonScale.Hide();
					TBox.tileLockedModelIcon.Hide();
					TBox.tileCharacterEditIcon.Hide();
					Blocksworld.UI.SidePanel.HideCopyModelButton();
					Blocksworld.UI.SidePanel.HideSaveModelButton();
					base.EnterState(GestureState.Active);
				}
				else if (TBox.HitMove(pos))
				{
					this.touch = allTouches[0];
					this.tBox = false;
					TBox.StartMove(this.touch.Position, TBox.MoveMode.Plane);
					TBox.tileButtonRotate.Hide();
					TBox.tileButtonScale.Hide();
					TBox.tileLockedModelIcon.Hide();
					TBox.tileCharacterEditIcon.Hide();
					Blocksworld.UI.SidePanel.HideCopyModelButton();
					Blocksworld.UI.SidePanel.HideSaveModelButton();
					base.EnterState(GestureState.Active);
				}
				else if (TBox.HitScale(pos) && Blocksworld.selectedBlock != null)
				{
					this.touch = allTouches[0];
					this.tBox = false;
					TBox.StartScale(this.touch.Position, TBox.ScaleMode.Plane);
					TBox.tileButtonRotate.Hide();
					TBox.tileButtonMove.Hide();
					TBox.tileLockedModelIcon.Hide();
					TBox.tileCharacterEditIcon.Hide();
					Blocksworld.UI.SidePanel.HideCopyModelButton();
					Blocksworld.UI.SidePanel.HideSaveModelButton();
					base.EnterState(GestureState.Active);
				}
				else if (Blocksworld.mouseBlock != null)
				{
					if (Tutorial.state == TutorialState.Rotation || Tutorial.state == TutorialState.Scale)
					{
						base.EnterState(GestureState.Failed);
						return;
					}
					Vector3 vector = Vector3.zero;
					bool flag = false;
					if (Blocksworld.mouseBlock == Blocksworld.selectedBlock)
					{
						flag = true;
						vector = Blocksworld.mouseBlock.GetPosition();
					}
					else if (Blocksworld.selectedBlock == null && Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.ContainsBlock(Blocksworld.mouseBlock))
					{
						flag = true;
						vector = Blocksworld.selectedBunch.GetPosition();
					}
					else if (Blocksworld.GetSelectedScriptBlock() == Blocksworld.mouseBlock && Blocksworld.selectedBlock != null)
					{
						flag = true;
						vector = Blocksworld.selectedBlock.GetPosition();
					}
					if (!flag)
					{
						base.EnterState(GestureState.Failed);
						return;
					}
					this.raycastDragging = true;
					this.touch = allTouches[0];
					this.touchStartPos = this.touch.Position;
					this.startPos = vector;
					TBox.StartMove(this.touch.Position, TBox.MoveMode.Raycast);
					TBox.tileButtonRotate.Hide();
					TBox.tileButtonMove.Hide();
					TBox.tileButtonScale.Hide();
					TBox.tileLockedModelIcon.Hide();
					TBox.tileCharacterEditIcon.Hide();
					Blocksworld.UI.SidePanel.HideCopyModelButton();
					Blocksworld.UI.SidePanel.HideSaveModelButton();
					base.EnterState(GestureState.Active);
					Tutorial.Step();
				}
			}
			else if (allTouches.Count == 2)
			{
				this.tBox = true;
				this.touch = allTouches[1];
				this.RestartGesture();
			}
		}

		// Token: 0x06001649 RID: 5705 RVA: 0x0009DA94 File Offset: 0x0009BE94
		public static void SetGafHighlights(List<GAF> gafs)
		{
			TBoxGesture.highlights = new HashSet<GAF>(gafs);
		}

		// Token: 0x0600164A RID: 5706 RVA: 0x0009DAA4 File Offset: 0x0009BEA4
		public void ActivateDrag(Touch t)
		{
			this.touch = t;
			this.raycastDragging = true;
			if (Blocksworld.mouseBlock == null)
			{
				this.startPos = t.Position;
			}
			else if (Blocksworld.mouseBlock == Blocksworld.selectedBlock)
			{
				this.startPos = Blocksworld.mouseBlock.GetPosition();
			}
			else if (Blocksworld.selectedBunch != null)
			{
				this.startPos = Blocksworld.selectedBunch.GetPosition();
			}
			else
			{
				this.startPos = t.Position;
			}
			TBox.StartMove(this.touch.Position, TBox.MoveMode.Raycast);
			TBox.tileButtonRotate.Hide();
			TBox.tileButtonMove.Hide();
			TBox.tileButtonScale.Hide();
			TBox.tileLockedModelIcon.Hide();
			Blocksworld.UI.SidePanel.HideCopyModelButton();
			Blocksworld.UI.SidePanel.HideSaveModelButton();
			base.EnterState(GestureState.Active);
		}

		// Token: 0x0600164B RID: 5707 RVA: 0x0009DB94 File Offset: 0x0009BF94
		private void RestartGesture()
		{
			if (this.touch == null || TBox.tileButtonRotate == null)
			{
				return;
			}
			if (TBox.tileButtonRotate.IsShowing())
			{
				TBox.StopRotate();
				this.startTouchPos = this.touch.Position;
				TBox.StartRotate(this.touch.Position, TBox.RotateMode.Finger);
			}
			else if (TBox.tileButtonMove.IsShowing())
			{
				TBox.StopMove();
				TBox.StartMove(this.touch.Position, TBox.MoveMode.Up);
			}
			else if (TBox.tileButtonScale.IsShowing())
			{
				TBox.StopScale();
				TBox.StartScale(this.touch.Position, TBox.ScaleMode.Up);
			}
		}

		// Token: 0x0600164C RID: 5708 RVA: 0x0009DC48 File Offset: 0x0009C048
		public override void TouchesMoved(List<Touch> allTouches)
		{
			if (base.gestureState == GestureState.Active)
			{
				if (TBox.selected == null)
				{
					base.EnterState(GestureState.Failed);
				}
				else if (TBox.tileButtonRotate.IsShowing())
				{
					if (!this.tBox)
					{
						if (Mathf.Abs(this.startTouchPos.x - this.touch.Position.x) >= Mathf.Abs(this.startTouchPos.y - this.touch.Position.y))
						{
							TBox.tileButtonRotate.MoveTo(this.touch.Position.x - 40f, this.startButtonPos.y);
						}
						else
						{
							TBox.tileButtonRotate.MoveTo(this.startButtonPos.x, this.touch.Position.y - 40f);
						}
					}
					TBox.ContinueRotate(this.startTouchPos, this.touch.Position);
				}
				else if (TBox.tileButtonMove.IsShowing())
				{
					if (!this.tBox)
					{
						TBox.tileButtonMove.MoveTo(this.touch.Position.x - 40f, this.touch.Position.y - 40f);
					}
					TBox.ContinueMove(this.touch.Position, false);
				}
				else if (TBox.tileButtonScale.IsShowing())
				{
					if (!this.tBox)
					{
						TBox.tileButtonScale.MoveTo(this.touch.Position.x - 40f, this.touch.Position.y - 40f);
					}
					TBox.ContinueScale(this.touch.Position);
				}
				else
				{
					if (Blocksworld.selectedBlock == null || !Blocksworld.selectedBlock.DisableBuildModeMove())
					{
						float magnitude = (this.touchStartPos - this.touch.Position).magnitude;
						if (magnitude > 10f)
						{
							TBox.ContinueMove(this.touch.Position, false);
						}
					}
					if (Blocksworld.selectedBlock != null && Blocksworld.selectedBlock.GetPosition() != this.startPos)
					{
						this.didActuallyMove = true;
					}
					if (Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.GetPosition() != this.startPos)
					{
						this.didActuallyMove = true;
					}
				}
			}
		}

		// Token: 0x0600164D RID: 5709 RVA: 0x0009DEC5 File Offset: 0x0009C2C5
		public override void TouchesStationary(List<Touch> allTouches)
		{
			this.TouchesMoved(allTouches);
		}

		// Token: 0x0600164E RID: 5710 RVA: 0x0009DED0 File Offset: 0x0009C2D0
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (this.touch.Phase == TouchPhase.Ended)
			{
				if (this.tBox && allTouches.Count == 2 && allTouches[0].Phase != TouchPhase.Ended)
				{
					this.touch = allTouches[0];
					this.tBox = false;
					this.RestartGesture();
				}
				else
				{
					this.Stop();
				}
				History.AddStateIfNecessary();
			}
			Blocksworld.UI.QuickSelect.UpdateModelIcon();
		}

		// Token: 0x0600164F RID: 5711 RVA: 0x0009DF54 File Offset: 0x0009C354
		public void Stop()
		{
			bool flag = TBox.tileButtonRotate.IsShowing();
			if (flag)
			{
				TBox.StopRotate();
			}
			else if (TBox.tileButtonScale.IsShowing())
			{
				TBox.StopScale();
			}
			else if (TBox.tileButtonMove.IsShowing())
			{
				TBox.StopMove();
			}
			else
			{
				TBox.StopMove();
				if (!this.didActuallyMove && (Blocksworld.mouseBlock == null || !Blocksworld.mouseBlock.isTerrain))
				{
					if (TBoxGesture.skipOneTap)
					{
						TBoxGesture.skipOneTap = false;
					}
					else
					{
						Blocksworld.Select(Blocksworld.mouseBlock, false, true);
					}
				}
			}
			TBox.Show(true);
			if (Blocksworld.selectedBlock != null)
			{
				if (Blocksworld.selectedBlock.DisableBuildModeMove())
				{
					if (Blocksworld.selectedBlock != Blocksworld.mouseBlock && !flag)
					{
						Blocksworld.Deselect(false, true);
					}
					else
					{
						Blocksworld.ShowSelectedBlockPanel();
					}
				}
				else if (Blocksworld.selectedBlock.SelectableTerrain())
				{
					Blocksworld.ShowSelectedBlockPanel();
				}
			}
			if (Blocksworld.selectedBlock != null && Blocksworld.selectedBunch == null && this.raycastDragging)
			{
				Blocksworld.ScrollToFirstBlockSpecificTile(Blocksworld.selectedBlock);
			}
			base.EnterState(GestureState.Ended);
			Tutorial.Step();
		}

		// Token: 0x06001650 RID: 5712 RVA: 0x0009E08B File Offset: 0x0009C48B
		public override void Reset()
		{
			base.EnterState(GestureState.Possible);
			this.touch = null;
			this.raycastDragging = false;
			this.didActuallyMove = false;
			TBoxGesture.highlights = null;
		}

		// Token: 0x06001651 RID: 5713 RVA: 0x0009E0AF File Offset: 0x0009C4AF
		public override string ToString()
		{
			return "TBoxGesture";
		}

		// Token: 0x0400115F RID: 4447
		public bool tBox;

		// Token: 0x04001160 RID: 4448
		private Touch touch;

		// Token: 0x04001161 RID: 4449
		private Vector2 startTouchPos;

		// Token: 0x04001162 RID: 4450
		private Vector3 startButtonPos;

		// Token: 0x04001163 RID: 4451
		public bool raycastDragging;

		// Token: 0x04001164 RID: 4452
		public bool didActuallyMove;

		// Token: 0x04001165 RID: 4453
		private Vector3 startPos;

		// Token: 0x04001166 RID: 4454
		private Vector2 touchStartPos;

		// Token: 0x04001167 RID: 4455
		public static bool skipOneTap;

		// Token: 0x04001168 RID: 4456
		private static HashSet<GAF> highlights;
	}
}
