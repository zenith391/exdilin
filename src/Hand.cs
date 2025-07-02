using System;
using UnityEngine;

public class Hand
{
	private float counter;

	private HandState state;

	private Arrow arrow;

	private Target target;

	private float tapDuration;

	private GameObject handDefault;

	private GameObject handTap;

	private bool _isVisible;

	private bool _isTapping;

	public Vector3 screenOffset = Vector3.zero;

	private GameObject activeHand
	{
		get
		{
			if (_isTapping)
			{
				return handTap;
			}
			return handDefault;
		}
	}

	public Hand(bool left = false)
	{
		handDefault = Blocksworld.UI.Overlay.CreateHandDefaultOverlayObject();
		handTap = Blocksworld.UI.Overlay.CreateHandTapOverlayObject();
		state = HandState.None;
		Hide();
	}

	public void Show()
	{
		_isVisible = true;
		if (_isTapping)
		{
			handDefault.SetActive(value: false);
			handTap.SetActive(value: true);
		}
		else
		{
			handDefault.SetActive(value: true);
			handTap.SetActive(value: false);
		}
	}

	private void Tap(bool tap)
	{
		_isTapping = tap;
		if (_isVisible)
		{
			Show();
		}
	}

	public void DragArrow(Arrow arrow)
	{
		this.arrow = arrow;
		SwitchState(HandState.DragWaiting);
	}

	public void TapTarget(Target target, float tapDuration = 0.25f)
	{
		this.target = target;
		this.tapDuration = tapDuration;
		SwitchState(HandState.TapWaiting);
	}

	public void Hide()
	{
		_isVisible = false;
		handDefault.SetActive(value: false);
		handTap.SetActive(value: false);
		state = HandState.None;
	}

	public void Update()
	{
		switch (state)
		{
		case HandState.DragWaiting:
			if (!StateWaiting())
			{
				Show();
				Tap(tap: false);
				PositionHandOnArrow(0f);
				SwitchState(HandState.DragAppearing);
				StateAppearing();
			}
			break;
		case HandState.DragAppearing:
			Show();
			if (!StateAppearing())
			{
				Tap(tap: true);
				PositionHandOnArrow(0f);
				SwitchState(HandState.DragMoving);
			}
			break;
		case HandState.DragMoving:
			Show();
			if (!StateMoving())
			{
				PositionHandOnArrow((float)Math.PI);
				SwitchState(HandState.DragHolding);
			}
			break;
		case HandState.DragHolding:
		{
			Show();
			float time = ((Tutorial.manualPaintOrTexture.Count <= 0) ? 0f : 2f);
			if (!StateHolding(time))
			{
				Tap(tap: false);
				PositionHandOnArrow((float)Math.PI);
				SwitchState(HandState.DragDisappearing, (float)Math.PI / 2f);
			}
			break;
		}
		case HandState.DragDisappearing:
			Show();
			if (!StateDisappearing())
			{
				SwitchState(HandState.DragWaiting);
			}
			break;
		case HandState.TapWaiting:
			if (!StateWaiting())
			{
				Show();
				Tap(tap: false);
				PositionHandOnTarget();
				SwitchState(HandState.TapAppearing);
				StateAppearing();
			}
			break;
		case HandState.TapAppearing:
			Show();
			if (!StateAppearing())
			{
				SwitchState(HandState.TapWaitingBefore);
			}
			break;
		case HandState.TapWaitingBefore:
			if (!StateWaiting(0.25f))
			{
				Show();
				Tap(tap: true);
				PositionHandOnTarget();
				SwitchState(HandState.TapTapping);
			}
			break;
		case HandState.TapTapping:
			Show();
			if (!StateWaiting(tapDuration))
			{
				Tap(tap: false);
				PositionHandOnTarget();
				SwitchState(HandState.TapWaitingAfter);
			}
			break;
		case HandState.TapWaitingAfter:
			if (!StateWaiting(0.25f))
			{
				SwitchState(HandState.TapDisappearing, (float)Math.PI / 2f);
			}
			break;
		case HandState.TapDisappearing:
			Show();
			if (!StateDisappearing())
			{
				SwitchState(HandState.TapWaiting);
			}
			break;
		}
	}

	private bool StateWaiting(float time = 1f)
	{
		counter += Time.deltaTime;
		return counter < time;
	}

	private bool StateHolding(float time = 0f)
	{
		counter += Time.deltaTime;
		return counter < time;
	}

	private bool StateAppearing()
	{
		Show();
		counter += 8f * Time.deltaTime;
		float f = Mathf.Clamp(counter, 0f, (float)Math.PI / 2f);
		float scale = Mathf.Sin(f);
		SetScale(scale);
		return counter < (float)Math.PI / 2f;
	}

	private bool StateDisappearing()
	{
		counter = Mathf.Max(0f, counter - 8f * Time.deltaTime);
		float f = Mathf.Clamp(counter, 0f, (float)Math.PI / 2f);
		float scale = Mathf.Sin(f);
		SetScale(scale);
		return counter > 0f;
	}

	private bool StateMoving()
	{
		counter += 2f * Time.deltaTime;
		PositionHandOnArrow(counter);
		return counter < (float)Math.PI;
	}

	private void SwitchState(HandState state, float counter = 0f)
	{
		this.state = state;
		this.counter = counter;
	}

	private void PositionHandOnArrow(float counter)
	{
		float num = 0.5f * (0f - Mathf.Cos(counter) + 1f);
		Vector3 pos = arrow.from + num * (arrow.to - arrow.from) + screenOffset;
		MoveTo(pos);
	}

	private void PositionHandOnTarget()
	{
		Vector3 screenPosition = target.GetScreenPosition();
		MoveTo(screenPosition);
	}

	private void MoveTo(Vector3 pos)
	{
		Blocksworld.UI.Overlay.SetOverlayObjectPosition(activeHand, pos);
	}

	private void SetScale(float scale)
	{
		Blocksworld.UI.Overlay.SetOverlayObjectScale(activeHand, scale);
	}
}
