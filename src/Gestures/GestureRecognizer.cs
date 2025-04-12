using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x0200017D RID: 381
	public class GestureRecognizer
	{
		// Token: 0x060015E4 RID: 5604 RVA: 0x00099F90 File Offset: 0x00098390
		public GestureRecognizer()
		{
			this._touches = new List<Touch>();
			this._gestures = new List<BaseGesture>();
			this._cancellers = new Dictionary<BaseGesture, List<BaseGesture>>();
			this._preventStart = new HashSet<BaseGesture>();
		}

		// Token: 0x060015E5 RID: 5605 RVA: 0x00099FFB File Offset: 0x000983FB
		public void AddGesture(BaseGesture gesture)
		{
			this._gestures.Add(gesture);
		}

		// Token: 0x060015E6 RID: 5606 RVA: 0x0009A009 File Offset: 0x00098409
		public void RemoveGesture(BaseGesture gesture)
		{
			this._gestures.Remove(gesture);
		}

		// Token: 0x060015E7 RID: 5607 RVA: 0x0009A018 File Offset: 0x00098418
		public void Cancels(BaseGesture parent, BaseGesture child)
		{
			if (!this._cancellers.ContainsKey(parent))
			{
				this._cancellers[parent] = new List<BaseGesture>();
			}
			this._cancellers[parent].Add(child);
		}

		// Token: 0x060015E8 RID: 5608 RVA: 0x0009A050 File Offset: 0x00098450
		public void CancelsAll(BaseGesture parent, IEnumerable<BaseGesture> children)
		{
			foreach (BaseGesture child in children)
			{
				this.Cancels(parent, child);
			}
		}

		// Token: 0x060015E9 RID: 5609 RVA: 0x0009A0A8 File Offset: 0x000984A8
		public void AnyCancels(IEnumerable<BaseGesture> parents, BaseGesture child)
		{
			foreach (BaseGesture parent in parents)
			{
				this.Cancels(parent, child);
			}
		}

		// Token: 0x060015EA RID: 5610 RVA: 0x0009A100 File Offset: 0x00098500
		public void AnyCancelsAll(IEnumerable<BaseGesture> parents, IEnumerable<BaseGesture> children)
		{
			foreach (BaseGesture child in children)
			{
				this.AnyCancels(parents, child);
			}
		}

		// Token: 0x060015EB RID: 5611 RVA: 0x0009A158 File Offset: 0x00098558
		public void OnDebugGUI()
		{
			Color color = GUI.color;
			GUI.color = new Color(0f, 0f, 0f, 1f);
			int num = 40;
			int num2 = 10;
			foreach (Touch touch in this._touches)
			{
				GUI.Label(new Rect((float)num, (float)num2, 400f, 22f), string.Format("({0}, {1}) {2}", touch.Position.x, touch.Position.y, touch.Phase));
				float num3 = NormalizedScreen.scale * touch.Position.x;
				float num4 = NormalizedScreen.scale * touch.Position.y;
				GUI.color = color;
				GUI.DrawTexture(new Rect(num3 - 32f, (float)Screen.height - num4 - 32f, 64f, 64f), (Texture)Resources.Load("GUI/Button Red", typeof(Texture)));
				GUI.color = new Color(0f, 0f, 0f, 1f);
				num2 += 20;
			}
			num2 = 40;
			IEnumerator enumerator2 = Enum.GetValues(typeof(GestureState)).GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					object obj = enumerator2.Current;
					GestureState gestureState = (GestureState)obj;
					GUI.color = new Color(0f, 0f, 0f, 1f);
					GUI.Label(new Rect((float)num, (float)num2, 200f, 22f), gestureState.ToString());
					int num5 = num2 + 20;
					foreach (BaseGesture baseGesture in this._gestures)
					{
						if (baseGesture.gestureState == gestureState)
						{
							GUI.color = ((!baseGesture.IsEnabled) ? new Color(0.3f, 0.3f, 0.3f, 1f) : new Color(0f, 0f, 0f, 1f));
							GUI.Label(new Rect((float)num, (float)num5, 800f, 22f), string.Format("{0} {1}", baseGesture, (!this._preventStart.Contains(baseGesture)) ? string.Empty : "(prevented)"));
							num5 += 20;
						}
					}
					num += 200;
					if (num >= 900)
					{
						num2 += 400;
						num = 320;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator2 as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			GUI.color = color;
		}

		// Token: 0x060015EC RID: 5612 RVA: 0x0009A49C File Offset: 0x0009889C
		public void Update()
		{
			this.MatchTouchesToInput();
			this.RunGestures();
		}

		// Token: 0x060015ED RID: 5613 RVA: 0x0009A4AC File Offset: 0x000988AC
		private void MatchTouchesToInput()
		{
			this.RemoveTouchesWithPhase(this._touches, TouchPhase.Ended);
			HashSet<Touch> hashSet = this.CurrentTouchPoints();
			this.currentPointsLeft.Clear();
			foreach (Touch item in hashSet)
			{
				this.currentPointsLeft.Add(item);
			}
			this.touchesLeft.Clear();
			this.touchesLeft.AddRange(this._touches);
			foreach (Touch touch in hashSet)
			{
				if (touch.Phase == TouchPhase.Moved || touch.Phase == TouchPhase.Stationary || touch.Phase == TouchPhase.Ended)
				{
					Touch touch2 = null;
					float num = 1E+08f;
					for (int i = 0; i < this.touchesLeft.Count; i++)
					{
						Touch touch3 = this.touchesLeft[i];
						float sqrMagnitude = (touch.Position - touch3.Position).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							touch2 = touch3;
						}
					}
					if (touch2 != null)
					{
						this.touchesLeft.Remove(touch2);
						this.currentPointsLeft.Remove(touch);
						touch2.Moved(touch.Position);
					}
				}
			}
			for (int j = 0; j < this.touchesLeft.Count; j++)
			{
				Touch touch4 = this.touchesLeft[j];
				touch4.End();
			}
			foreach (Touch item2 in this.currentPointsLeft)
			{
				this._touches.Add(item2);
			}
		}

		// Token: 0x060015EE RID: 5614 RVA: 0x0009A6D4 File Offset: 0x00098AD4
		private void RunGestures()
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			int num = 0;
			for (int i = 0; i < this._touches.Count; i++)
			{
				Touch touch = this._touches[i];
				if (touch.Phase == TouchPhase.Began)
				{
					flag = true;
				}
				if (touch.Phase == TouchPhase.Moved)
				{
					flag2 = true;
					num = touch.moveFrameCount;
				}
				if (touch.Phase == TouchPhase.Stationary)
				{
					flag3 = true;
				}
				if (touch.Phase == TouchPhase.Ended)
				{
					flag4 = true;
				}
			}
			for (int j = 0; j < this._gestures.Count; j++)
			{
				BaseGesture baseGesture = this._gestures[j];
				if (baseGesture.CanReceiveEvents && !this._preventStart.Contains(baseGesture))
				{
					List<Touch> list = null;
					this._hasResetAll = false;
					bool isActive = baseGesture.IsActive;
					if (flag4 && baseGesture.IsStarted)
					{
						list = this.CopyIfNecessary(list, this._touches);
						baseGesture.TouchesEnded(list);
					}
					bool flag5 = false;
					for (int k = 0; k < this._touches.Count; k++)
					{
						if (this._touches[k].Phase == TouchPhase.Ended)
						{
							flag5 = true;
							break;
						}
					}
					if (flag5)
					{
						list = this.CopyIfNecessary(list, this._touches);
						this.RemoveTouchesWithPhase(list, TouchPhase.Ended);
					}
					if (flag)
					{
						list = this.CopyIfNecessary(list, this._touches);
						baseGesture.TouchesBegan(list);
					}
					if (flag2)
					{
						if (baseGesture.IsStarted)
						{
							list = this.CopyIfNecessary(list, this._touches);
							baseGesture.TouchesMoved(list);
						}
						else if ((float)num < baseGesture.touchBeginWindow)
						{
							list = this.CopyIfNecessary(list, this._touches);
							baseGesture.TouchesBegan(list);
						}
					}
					if (flag3 && baseGesture.IsStarted)
					{
						list = this.CopyIfNecessary(list, this._touches);
						baseGesture.TouchesStationary(list);
					}
					if (!isActive && baseGesture.IsActive)
					{
						this.CancelDependents(baseGesture);
					}
				}
			}
			if (!this._hasResetAll && this.AllTouchesEnded())
			{
				this._hasResetAll = true;
				for (int l = 0; l < this._gestures.Count; l++)
				{
					BaseGesture baseGesture2 = this._gestures[l];
					if (!baseGesture2.CanReceiveEvents)
					{
						baseGesture2.Reset();
					}
				}
				this._preventStart.Clear();
			}
		}

		// Token: 0x060015EF RID: 5615 RVA: 0x0009A979 File Offset: 0x00098D79
		private List<Touch> CopyIfNecessary(List<Touch> old, List<Touch> copyFrom)
		{
			if (old == null)
			{
				return new List<Touch>(copyFrom);
			}
			return old;
		}

		// Token: 0x060015F0 RID: 5616 RVA: 0x0009A98C File Offset: 0x00098D8C
		private bool AllTouchesEnded()
		{
			for (int i = 0; i < this._touches.Count; i++)
			{
				Touch touch = this._touches[i];
				if (touch.Phase != TouchPhase.Ended)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060015F1 RID: 5617 RVA: 0x0009A9D4 File Offset: 0x00098DD4
		private void RemoveTouchesWithPhase(List<Touch> touches, TouchPhase phase)
		{
			for (int i = touches.Count - 1; i >= 0; i--)
			{
				if (touches[i].Phase == phase)
				{
					touches.RemoveAt(i);
				}
			}
		}

		// Token: 0x060015F2 RID: 5618 RVA: 0x0009AA14 File Offset: 0x00098E14
		private void CancelDependents(BaseGesture gesture)
		{
			if (!this._cancellers.ContainsKey(gesture))
			{
				return;
			}
			List<BaseGesture> list = this._cancellers[gesture];
			for (int i = 0; i < list.Count; i++)
			{
				BaseGesture baseGesture = list[i];
				if (baseGesture.IsStarted)
				{
					baseGesture.Cancel();
				}
				else
				{
					this._preventStart.Add(baseGesture);
				}
			}
		}

		// Token: 0x060015F3 RID: 5619 RVA: 0x0009AA84 File Offset: 0x00098E84
		private Touch CreateMouseTouch(TouchPhase phase)
		{
			float scale = NormalizedScreen.scale;
			return new Touch(Input.mousePosition / scale)
			{
				Phase = phase
			};
		}

		// Token: 0x060015F4 RID: 5620 RVA: 0x0009AAB8 File Offset: 0x00098EB8
		private Touch CreateMouseZoomTouch(TouchPhase phase)
		{
			float scale = NormalizedScreen.scale;
			Vector2 vector = Input.mousePosition / scale;
			Vector2 a = new Vector2((float)(NormalizedScreen.width / 2), (float)(NormalizedScreen.height / 2));
			return new Touch(vector + 2f * (a - vector))
			{
				Phase = phase
			};
		}

		// Token: 0x060015F5 RID: 5621 RVA: 0x0009AB18 File Offset: 0x00098F18
		private Touch CreateTwoFingerSwipeTouch(TouchPhase phase)
		{
			float scale = NormalizedScreen.scale;
			Vector2 a = Input.mousePosition / scale;
			return new Touch(a + new Vector2(120f, 0f))
			{
				Phase = phase
			};
		}

		// Token: 0x060015F6 RID: 5622 RVA: 0x0009AB60 File Offset: 0x00098F60
		private HashSet<Touch> CurrentTouchPoints()
		{
			float scale = NormalizedScreen.scale;
			GestureRecognizer.currentTouchPointsTouches.Clear();
			this.tempGestureCommands.Clear();
			this.tempGestureCommands.AddRange(this.gestureCommands);
			for (int i = 0; i < this.tempGestureCommands.Count; i++)
			{
				GestureCommand gestureCommand = this.tempGestureCommands[i];
				gestureCommand.Execute(GestureRecognizer.currentTouchPointsTouches);
				if (gestureCommand.done)
				{
					this.gestureCommands.Remove(gestureCommand);
				}
			}
			if (BW.Options.useMouse())
			{
				bool mouseButton = Input.GetMouseButton(0);
				bool flag = false;
				bool flag2 = false;
				if (this.prevMouseDown && mouseButton)
				{
					GestureRecognizer.currentTouchPointsTouches.Add(this.CreateMouseTouch(TouchPhase.Moved));
					flag = MappedInput.InputPressed(MappableInput.SIM_ZOOM);
					if (flag && !this.prevSimulateZoom)
					{
						GestureRecognizer.currentTouchPointsTouches.Add(this.CreateMouseZoomTouch(TouchPhase.Began));
					}
					else if (flag && this.prevSimulateZoom)
					{
						GestureRecognizer.currentTouchPointsTouches.Add(this.CreateMouseZoomTouch(TouchPhase.Moved));
					}
					else if (!flag && this.prevSimulateZoom)
					{
						GestureRecognizer.currentTouchPointsTouches.Add(this.CreateMouseZoomTouch(TouchPhase.Ended));
					}
					else
					{
						flag2 = MappedInput.InputPressed(MappableInput.SIM_TWO_FINGER_SWIPE);
						if (flag2 && !this.prevSimulateTwoFingerSwipe)
						{
							GestureRecognizer.currentTouchPointsTouches.Add(this.CreateTwoFingerSwipeTouch(TouchPhase.Began));
						}
						else if (flag2 && this.prevSimulateTwoFingerSwipe)
						{
							GestureRecognizer.currentTouchPointsTouches.Add(this.CreateTwoFingerSwipeTouch(TouchPhase.Moved));
						}
						else if (!flag2 && this.prevSimulateTwoFingerSwipe)
						{
							GestureRecognizer.currentTouchPointsTouches.Add(this.CreateTwoFingerSwipeTouch(TouchPhase.Ended));
						}
					}
				}
				else if (this.prevMouseDown && !mouseButton)
				{
					GestureRecognizer.currentTouchPointsTouches.Add(this.CreateMouseTouch(TouchPhase.Ended));
					if (this.prevSimulateZoom)
					{
						GestureRecognizer.currentTouchPointsTouches.Add(this.CreateMouseZoomTouch(TouchPhase.Ended));
					}
					if (this.prevSimulateTwoFingerSwipe)
					{
						GestureRecognizer.currentTouchPointsTouches.Add(this.CreateTwoFingerSwipeTouch(TouchPhase.Ended));
					}
				}
				else if (!this.prevMouseDown && mouseButton)
				{
					GestureRecognizer.currentTouchPointsTouches.Add(this.CreateMouseTouch(TouchPhase.Began));
				}
				this.prevMouseDown = mouseButton;
				this.prevSimulateZoom = flag;
				this.prevSimulateTwoFingerSwipe = flag2;
			}
			for (int j = 0; j < Input.touchCount; j++)
			{
				UnityEngine.Touch touch = Input.GetTouch(j);
				Touch touch2 = new Touch(touch.position / scale);
				touch2.Phase = touch.phase;
				GestureRecognizer.currentTouchPointsTouches.Add(touch2);
			}
			return GestureRecognizer.currentTouchPointsTouches;
		}

		// Token: 0x04001111 RID: 4369
		private readonly List<Touch> _touches;

		// Token: 0x04001112 RID: 4370
		private readonly List<BaseGesture> _gestures;

		// Token: 0x04001113 RID: 4371
		private readonly Dictionary<BaseGesture, List<BaseGesture>> _cancellers;

		// Token: 0x04001114 RID: 4372
		private readonly HashSet<BaseGesture> _preventStart;

		// Token: 0x04001115 RID: 4373
		private bool prevMouseDown;

		// Token: 0x04001116 RID: 4374
		private bool prevSimulateZoom;

		// Token: 0x04001117 RID: 4375
		private bool prevSimulateTwoFingerSwipe;

		// Token: 0x04001118 RID: 4376
		private bool _hasResetAll;

		// Token: 0x04001119 RID: 4377
		public List<GestureCommand> gestureCommands = new List<GestureCommand>();

		// Token: 0x0400111A RID: 4378
		private HashSet<Touch> currentPointsLeft = new HashSet<Touch>();

		// Token: 0x0400111B RID: 4379
		private List<Touch> touchesLeft = new List<Touch>();

		// Token: 0x0400111C RID: 4380
		private static HashSet<Touch> currentTouchPointsTouches = new HashSet<Touch>();

		// Token: 0x0400111D RID: 4381
		private List<GestureCommand> tempGestureCommands = new List<GestureCommand>();
	}
}
