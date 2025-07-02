using UnityEngine;
using UnityEngine.UI;

public class UIMoverUniversal : Graphic
{
	public RectTransform moverTransform;

	public UIMoverHandle handle;

	public CanvasGroup dpadLeftBackground;

	public CanvasGroup dpadRightBackground;

	public CanvasGroup dpadUpBackground;

	public CanvasGroup dpadDownBackground;

	public GameObject leftKeyIndicator;

	public GameObject rightKeyIndicator;

	public GameObject upKeyIndicator;

	public GameObject downKeyIndicator;

	public float movementScale = 64f;

	public int visibilityGroup;

	private MoverDirectionMask _moverMask;

	private Text[] _leftKeyIndicatorText;

	private Text[] _rightKeyIndicatorText;

	private Text[] _upKeyIndicatorText;

	private Text[] _downKeyIndicatorText;

	private MappableInput _leftInputAxis;

	private MappableInput _rightInputAxis;

	private MappableInput _upInputAxis;

	private MappableInput _downInputAxis;

	private float _alpha;

	private float _maxOffsetMagnitude;

	private Vector2 _offset;

	public void Init(int inputAxis)
	{
		_moverMask = MoverDirectionMask.NONE;
		handle.Init();
		_alpha = 1f;
		_maxOffsetMagnitude = movementScale * NormalizedScreen.scale;
		_offset = Vector2.zero;
		if (inputAxis != 2)
		{
			SetToInputAxis1();
		}
		else
		{
			SetToInputAxis2();
		}
		_leftKeyIndicatorText = leftKeyIndicator.GetComponentsInChildren<Text>();
		_rightKeyIndicatorText = rightKeyIndicator.GetComponentsInChildren<Text>();
		_upKeyIndicatorText = upKeyIndicator.GetComponentsInChildren<Text>();
		_downKeyIndicatorText = downKeyIndicator.GetComponentsInChildren<Text>();
		base.gameObject.SetActive(value: false);
	}

	private void SetToInputAxis1()
	{
		_leftInputAxis = MappableInput.AXIS1_LEFT;
		_rightInputAxis = MappableInput.AXIS1_RIGHT;
		_upInputAxis = MappableInput.AXIS1_UP;
		_downInputAxis = MappableInput.AXIS1_DOWN;
	}

	private void SetToInputAxis2()
	{
		_leftInputAxis = MappableInput.AXIS2_LEFT;
		_rightInputAxis = MappableInput.AXIS2_RIGHT;
		_upInputAxis = MappableInput.AXIS2_UP;
		_downInputAxis = MappableInput.AXIS2_DOWN;
	}

	public void UpdateAlphaFade(float alpha)
	{
		_alpha = alpha;
		bool flag = (_moverMask & MoverDirectionMask.LEFT) != 0;
		bool flag2 = (_moverMask & MoverDirectionMask.RIGHT) != 0;
		bool flag3 = (_moverMask & MoverDirectionMask.UP) != 0;
		bool flag4 = (_moverMask & MoverDirectionMask.DOWN) != 0;
		bool flag5 = _moverMask != MoverDirectionMask.NONE;
		dpadLeftBackground.alpha = _alpha * ((!flag) ? 0.25f : 1f);
		dpadRightBackground.alpha = _alpha * ((!flag2) ? 0.25f : 1f);
		dpadUpBackground.alpha = _alpha * ((!flag3) ? 0.25f : 1f);
		dpadDownBackground.alpha = _alpha * ((!flag4) ? 0.25f : 1f);
		handle.SetAlpha(_alpha * ((!flag5) ? 0.25f : 1f));
	}

	public void UpdateDirectionMask(MoverDirectionMask mask)
	{
		_moverMask |= mask;
		UpdateAlphaFade(_alpha);
	}

	public new bool IsActive()
	{
		return _moverMask != MoverDirectionMask.NONE;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		_moverMask = MoverDirectionMask.NONE;
		_offset = Vector2.zero;
	}

	public void UpdateMover()
	{
		Vector2 b = Vector2.zero;
		if (Blocksworld.lockInput)
		{
			_offset = Vector2.zero;
			handle.Press(press: false);
		}
		else
		{
			float num = (((_moverMask & MoverDirectionMask.LEFT) == 0) ? 0f : MappedInput.InputAxis(_leftInputAxis));
			float num2 = (((_moverMask & MoverDirectionMask.RIGHT) == 0) ? 0f : MappedInput.InputAxis(_rightInputAxis));
			float num3 = (((_moverMask & MoverDirectionMask.UP) == 0) ? 0f : MappedInput.InputAxis(_upInputAxis));
			float num4 = (((_moverMask & MoverDirectionMask.DOWN) == 0) ? 0f : MappedInput.InputAxis(_downInputAxis));
			bool flag = num > 0f || num2 > 0f || num3 > 0f || num4 > 0f;
			handle.Press(flag);
			if (flag)
			{
				b = new Vector2(num2 - num, num3 - num4);
				if (b.sqrMagnitude > 1f)
				{
					b.Normalize();
				}
				b *= _maxOffsetMagnitude;
			}
			float num5 = ((!flag) ? 16f : 8f);
			Vector2 offset = Vector2.Lerp(_offset, b, num5 * Time.deltaTime);
			if (!flag && offset.sqrMagnitude < 0.1f)
			{
				offset = Vector2.zero;
			}
			if (offset.magnitude > _maxOffsetMagnitude)
			{
				offset = _maxOffsetMagnitude * offset.normalized;
			}
			_offset = offset;
		}
		handle.MoveTo(_offset);
	}

	public Vector2 GetNormalizedOffset()
	{
		return _offset / _maxOffsetMagnitude;
	}

	public Vector3 GetWorldOffset()
	{
		Vector2 normalizedOffset = GetNormalizedOffset();
		Vector3 vector = Blocksworld.cameraForward * normalizedOffset.y;
		Vector3 vector2 = Blocksworld.cameraRight * normalizedOffset.x;
		return vector + vector2;
	}

	public bool IsMoving()
	{
		return _offset.sqrMagnitude > 0f;
	}
}
