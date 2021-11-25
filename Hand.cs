using System;
using UnityEngine;

// Token: 0x02000192 RID: 402
public class Hand
{
	// Token: 0x06001697 RID: 5783 RVA: 0x000A17E0 File Offset: 0x0009FBE0
	public Hand(bool left = false)
	{
		this.handDefault = Blocksworld.UI.Overlay.CreateHandDefaultOverlayObject();
		this.handTap = Blocksworld.UI.Overlay.CreateHandTapOverlayObject();
		this.state = HandState.None;
		this.Hide();
	}

	// Token: 0x06001698 RID: 5784 RVA: 0x000A1838 File Offset: 0x0009FC38
	public void Show()
	{
		this._isVisible = true;
		if (this._isTapping)
		{
			this.handDefault.SetActive(false);
			this.handTap.SetActive(true);
		}
		else
		{
			this.handDefault.SetActive(true);
			this.handTap.SetActive(false);
		}
	}

	// Token: 0x06001699 RID: 5785 RVA: 0x000A188C File Offset: 0x0009FC8C
	private void Tap(bool tap)
	{
		this._isTapping = tap;
		if (this._isVisible)
		{
			this.Show();
		}
	}

	// Token: 0x17000068 RID: 104
	// (get) Token: 0x0600169A RID: 5786 RVA: 0x000A18A6 File Offset: 0x0009FCA6
	private GameObject activeHand
	{
		get
		{
			return (!this._isTapping) ? this.handDefault : this.handTap;
		}
	}

	// Token: 0x0600169B RID: 5787 RVA: 0x000A18C4 File Offset: 0x0009FCC4
	public void DragArrow(Arrow arrow)
	{
		this.arrow = arrow;
		this.SwitchState(HandState.DragWaiting, 0f);
	}

	// Token: 0x0600169C RID: 5788 RVA: 0x000A18D9 File Offset: 0x0009FCD9
	public void TapTarget(Target target, float tapDuration = 0.25f)
	{
		this.target = target;
		this.tapDuration = tapDuration;
		this.SwitchState(HandState.TapWaiting, 0f);
	}

	// Token: 0x0600169D RID: 5789 RVA: 0x000A18F5 File Offset: 0x0009FCF5
	public void Hide()
	{
		this._isVisible = false;
		this.handDefault.SetActive(false);
		this.handTap.SetActive(false);
		this.state = HandState.None;
	}

	// Token: 0x0600169E RID: 5790 RVA: 0x000A1920 File Offset: 0x0009FD20
	public void Update()
	{
		switch (this.state)
		{
		case HandState.DragWaiting:
			if (!this.StateWaiting(1f))
			{
				this.Show();
				this.Tap(false);
				this.PositionHandOnArrow(0f);
				this.SwitchState(HandState.DragAppearing, 0f);
				this.StateAppearing();
			}
			break;
		case HandState.DragAppearing:
			this.Show();
			if (!this.StateAppearing())
			{
				this.Tap(true);
				this.PositionHandOnArrow(0f);
				this.SwitchState(HandState.DragMoving, 0f);
			}
			break;
		case HandState.DragMoving:
			this.Show();
			if (!this.StateMoving())
			{
				this.PositionHandOnArrow(3.14159274f);
				this.SwitchState(HandState.DragHolding, 0f);
			}
			break;
		case HandState.DragHolding:
		{
			this.Show();
			float time = (Tutorial.manualPaintOrTexture.Count <= 0) ? 0f : 2f;
			if (!this.StateHolding(time))
			{
				this.Tap(false);
				this.PositionHandOnArrow(3.14159274f);
				this.SwitchState(HandState.DragDisappearing, 1.57079637f);
			}
			break;
		}
		case HandState.DragDisappearing:
			this.Show();
			if (!this.StateDisappearing())
			{
				this.SwitchState(HandState.DragWaiting, 0f);
			}
			break;
		case HandState.TapWaiting:
			if (!this.StateWaiting(1f))
			{
				this.Show();
				this.Tap(false);
				this.PositionHandOnTarget();
				this.SwitchState(HandState.TapAppearing, 0f);
				this.StateAppearing();
			}
			break;
		case HandState.TapAppearing:
			this.Show();
			if (!this.StateAppearing())
			{
				this.SwitchState(HandState.TapWaitingBefore, 0f);
			}
			break;
		case HandState.TapWaitingBefore:
			if (!this.StateWaiting(0.25f))
			{
				this.Show();
				this.Tap(true);
				this.PositionHandOnTarget();
				this.SwitchState(HandState.TapTapping, 0f);
			}
			break;
		case HandState.TapTapping:
			this.Show();
			if (!this.StateWaiting(this.tapDuration))
			{
				this.Tap(false);
				this.PositionHandOnTarget();
				this.SwitchState(HandState.TapWaitingAfter, 0f);
			}
			break;
		case HandState.TapWaitingAfter:
			if (!this.StateWaiting(0.25f))
			{
				this.SwitchState(HandState.TapDisappearing, 1.57079637f);
			}
			break;
		case HandState.TapDisappearing:
			this.Show();
			if (!this.StateDisappearing())
			{
				this.SwitchState(HandState.TapWaiting, 0f);
			}
			break;
		}
	}

	// Token: 0x0600169F RID: 5791 RVA: 0x000A1B91 File Offset: 0x0009FF91
	private bool StateWaiting(float time = 1f)
	{
		this.counter += Time.deltaTime;
		return this.counter < time;
	}

	// Token: 0x060016A0 RID: 5792 RVA: 0x000A1BAE File Offset: 0x0009FFAE
	private bool StateHolding(float time = 0f)
	{
		this.counter += Time.deltaTime;
		return this.counter < time;
	}

	// Token: 0x060016A1 RID: 5793 RVA: 0x000A1BCC File Offset: 0x0009FFCC
	private bool StateAppearing()
	{
		this.Show();
		this.counter += 8f * Time.deltaTime;
		float f = Mathf.Clamp(this.counter, 0f, 1.57079637f);
		float scale = Mathf.Sin(f);
		this.SetScale(scale);
		return this.counter < 1.57079637f;
	}

	// Token: 0x060016A2 RID: 5794 RVA: 0x000A1C28 File Offset: 0x000A0028
	private bool StateDisappearing()
	{
		this.counter = Mathf.Max(0f, this.counter - 8f * Time.deltaTime);
		float f = Mathf.Clamp(this.counter, 0f, 1.57079637f);
		float scale = Mathf.Sin(f);
		this.SetScale(scale);
		return this.counter > 0f;
	}

	// Token: 0x060016A3 RID: 5795 RVA: 0x000A1C88 File Offset: 0x000A0088
	private bool StateMoving()
	{
		this.counter += 2f * Time.deltaTime;
		this.PositionHandOnArrow(this.counter);
		return this.counter < 3.14159274f;
	}

	// Token: 0x060016A4 RID: 5796 RVA: 0x000A1CBB File Offset: 0x000A00BB
	private void SwitchState(HandState state, float counter = 0f)
	{
		this.state = state;
		this.counter = counter;
	}

	// Token: 0x060016A5 RID: 5797 RVA: 0x000A1CCC File Offset: 0x000A00CC
	private void PositionHandOnArrow(float counter)
	{
		float d = 0.5f * (-Mathf.Cos(counter) + 1f);
		Vector3 pos = this.arrow.from + d * (this.arrow.to - this.arrow.from) + this.screenOffset;
		this.MoveTo(pos);
	}

	// Token: 0x060016A6 RID: 5798 RVA: 0x000A1D34 File Offset: 0x000A0134
	private void PositionHandOnTarget()
	{
		Vector3 screenPosition = this.target.GetScreenPosition();
		this.MoveTo(screenPosition);
	}

	// Token: 0x060016A7 RID: 5799 RVA: 0x000A1D54 File Offset: 0x000A0154
	private void MoveTo(Vector3 pos)
	{
		Blocksworld.UI.Overlay.SetOverlayObjectPosition(this.activeHand, pos);
	}

	// Token: 0x060016A8 RID: 5800 RVA: 0x000A1D6C File Offset: 0x000A016C
	private void SetScale(float scale)
	{
		Blocksworld.UI.Overlay.SetOverlayObjectScale(this.activeHand, scale);
	}

	// Token: 0x040011A9 RID: 4521
	private float counter;

	// Token: 0x040011AA RID: 4522
	private HandState state;

	// Token: 0x040011AB RID: 4523
	private Arrow arrow;

	// Token: 0x040011AC RID: 4524
	private Target target;

	// Token: 0x040011AD RID: 4525
	private float tapDuration;

	// Token: 0x040011AE RID: 4526
	private GameObject handDefault;

	// Token: 0x040011AF RID: 4527
	private GameObject handTap;

	// Token: 0x040011B0 RID: 4528
	private bool _isVisible;

	// Token: 0x040011B1 RID: 4529
	private bool _isTapping;

	// Token: 0x040011B2 RID: 4530
	public Vector3 screenOffset = Vector3.zero;
}
