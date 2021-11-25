using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x0200017E RID: 382
	public class JoystickControlGesture : BaseGesture
	{
		// Token: 0x060015F8 RID: 5624 RVA: 0x0009AE35 File Offset: 0x00099235
		public JoystickControlGesture(OldSymbol left, OldSymbol right, OldSymbol up, OldSymbol down, Tile joystick, Tile range)
		{
			this._left = left;
			this._right = right;
			this._up = up;
			this._down = down;
			this._joystick = joystick;
			this._range = range;
		}

		// Token: 0x060015F9 RID: 5625 RVA: 0x0009AE6C File Offset: 0x0009926C
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (this._joystick.tileObject == null)
			{
				base.EnterState(GestureState.Failed);
			}
			foreach (Touch touch in allTouches)
			{
				if (touch.Phase == TouchPhase.Began && this._joystick.HitExtended(touch.Position, 40f, 40f, 40f, 40f, false))
				{
					this._touch = touch;
					break;
				}
			}
			if (this._touch != null)
			{
				base.EnterState(GestureState.Active);
				this.UpdateJoystick();
			}
		}

		// Token: 0x060015FA RID: 5626 RVA: 0x0009AF38 File Offset: 0x00099338
		public override void TouchesMoved(List<Touch> allTouches)
		{
			this.UpdateJoystick();
		}

		// Token: 0x060015FB RID: 5627 RVA: 0x0009AF40 File Offset: 0x00099340
		public override void TouchesStationary(List<Touch> allTouches)
		{
			this.UpdateJoystick();
		}

		// Token: 0x060015FC RID: 5628 RVA: 0x0009AF48 File Offset: 0x00099348
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (this._touch != null && this._touch.Phase == TouchPhase.Ended)
			{
				this._joystick.MoveTo(this._range.tileObject.GetPosition(), false);
				this.ClearJoystickValues();
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

		// Token: 0x060015FD RID: 5629 RVA: 0x0009AFF8 File Offset: 0x000993F8
		public override void Reset()
		{
			this._touch = null;
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x060015FE RID: 5630 RVA: 0x0009B008 File Offset: 0x00099408
		public override string ToString()
		{
			return string.Format("JoystickControl({0}, {1})", this._joystick.gaf, this._touch != null);
		}

		// Token: 0x060015FF RID: 5631 RVA: 0x0009B030 File Offset: 0x00099430
		private void UpdateJoystick()
		{
			if (base.gestureState != GestureState.Active || this._touch == null)
			{
				return;
			}
			int num = 80;
			this._joystick.MoveTo(this._touch.Position.x - 0.5f * (float)num, this._touch.Position.y - 0.5f * (float)num);
			Vector2 vector = this._range.tileObject.GetPosition() - this._joystick.tileObject.GetPosition();
			float value = Mathf.Min(1f, 0.02f * Mathf.Abs(vector.x));
			float value2 = Mathf.Min(1f, 0.02f * Mathf.Abs(vector.y));
			this.ClearJoystickValues();
			Blocksworld.joysticks[(vector.x <= 0f) ? this._right : this._left] = value;
			Blocksworld.joysticks[(vector.y <= 0f) ? this._up : this._down] = value2;
		}

		// Token: 0x06001600 RID: 5632 RVA: 0x0009B15C File Offset: 0x0009955C
		private void ClearJoystickValues()
		{
			Blocksworld.joysticks[this._left] = 0f;
			Blocksworld.joysticks[this._right] = 0f;
			Blocksworld.joysticks[this._up] = 0f;
			Blocksworld.joysticks[this._down] = 0f;
		}

		// Token: 0x0400111E RID: 4382
		private const float kSensitivity = 0.02f;

		// Token: 0x0400111F RID: 4383
		private readonly OldSymbol _left;

		// Token: 0x04001120 RID: 4384
		private readonly OldSymbol _right;

		// Token: 0x04001121 RID: 4385
		private readonly OldSymbol _up;

		// Token: 0x04001122 RID: 4386
		private readonly OldSymbol _down;

		// Token: 0x04001123 RID: 4387
		private readonly Tile _joystick;

		// Token: 0x04001124 RID: 4388
		private readonly Tile _range;

		// Token: 0x04001125 RID: 4389
		private Touch _touch;
	}
}
