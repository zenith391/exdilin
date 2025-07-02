using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMover : Graphic, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	private enum MovementMode
	{
		Zeroed,
		Direct,
		MoveToTarget,
		Released
	}

	public UIMoverHandle handle;

	public MoverBackgroundDef[] moverBackgrounds;

	public float controlSnap = 8f;

	public float releaseSnap = 3f;

	public bool moveToTouch = true;

	public bool dimWhenNotMoving = true;

	public bool useDirectionalBackgrounds = true;

	public bool useDirectionalHandles = true;

	public RectTransform moverTransform;

	public GameObject leftKeyIndicator;

	public GameObject rightKeyIndicator;

	public GameObject upKeyIndicator;

	public GameObject downKeyIndicator;

	public float movementScale = 100f;

	public int visibilityGroup;

	private bool _pointerControlEnabled = true;

	private MoverDirectionMask activeMask;

	private GameObject currentBackground;

	private Image currentBackgroundImage;

	private bool _active;

	private bool _visible;

	private bool _underPointerControl;

	private int _pointerId = -1;

	private float _distanceFromMoverExtentToCenter = 150f;

	private Vector2 _maxOffsetPos = new Vector2(150f, 300f);

	private Vector2 _moveStartPos;

	private Vector2 _offset;

	private float _maxOffsetMagnitude;

	private MappableInput _leftInputAxis;

	private MappableInput _rightInputAxis;

	private MappableInput _upInputAxis;

	private MappableInput _downInputAxis;

	private Image[] _leftKeyIndicatorImage;

	private Image[] _rightKeyIndicatorImage;

	private Image[] _upKeyIndicatorImage;

	private Image[] _downKeyIndicatorImage;

	private Text[] _leftKeyIndicatorText;

	private Text[] _rightKeyIndicatorText;

	private Text[] _upKeyIndicatorText;

	private Text[] _downKeyIndicatorText;

	private RectTransform _inputTransform;

	private Vector2 _basePosition;

	private Vector2 _basePositionOffset;

	private Vector2 _touchZoneFullScale;

	private const float _dimmedOpacity = 0.5f;

	private MovementMode _movementMode;

	public void Init()
	{
		_inputTransform = (RectTransform)base.transform;
		_touchZoneFullScale = _inputTransform.sizeDelta;
		_basePosition = moverTransform.anchoredPosition;
		_basePositionOffset = Vector2.zero;
		_maxOffsetMagnitude = movementScale * NormalizedScreen.scale;
		_distanceFromMoverExtentToCenter = (206f + movementScale * 0.75f) * 0.5f * NormalizedScreen.scale;
		_maxOffsetPos.x = ((RectTransform)base.transform).rect.width - _distanceFromMoverExtentToCenter;
		_maxOffsetPos.y = ((RectTransform)base.transform).rect.height - _distanceFromMoverExtentToCenter;
		handle.Init();
		if (leftKeyIndicator != null)
		{
			_leftKeyIndicatorImage = leftKeyIndicator.GetComponentsInChildren<Image>();
			_leftKeyIndicatorText = leftKeyIndicator.GetComponentsInChildren<Text>();
		}
		if (rightKeyIndicator != null)
		{
			_rightKeyIndicatorImage = rightKeyIndicator.GetComponentsInChildren<Image>();
			_rightKeyIndicatorText = rightKeyIndicator.GetComponentsInChildren<Text>();
		}
		if (upKeyIndicator != null)
		{
			_upKeyIndicatorImage = upKeyIndicator.GetComponentsInChildren<Image>();
			_upKeyIndicatorText = upKeyIndicator.GetComponentsInChildren<Text>();
		}
		if (downKeyIndicator != null)
		{
			_downKeyIndicatorImage = downKeyIndicator.GetComponentsInChildren<Image>();
			_downKeyIndicatorText = downKeyIndicator.GetComponentsInChildren<Text>();
		}
	}

	public void SetActive(bool active)
	{
		if (active != _active)
		{
			_movementMode = MovementMode.Zeroed;
			_offset = Vector2.zero;
			_underPointerControl = false;
		}
		_active = active;
		base.gameObject.SetActive(active);
		if (dimWhenNotMoving)
		{
			float opacity = ((!IsMoving()) ? 0.5f : 1f);
			SetOpacity(opacity);
		}
		if (!active)
		{
			activeMask = MoverDirectionMask.NONE;
		}
	}

	public void AdjustBasePositionOffset(float minSafeX, float minSafeY)
	{
		_basePositionOffset = new Vector2(minSafeX - _inputTransform.anchoredPosition.x, minSafeY - _inputTransform.anchoredPosition.y);
		if (_visible && !IsMoving())
		{
			Show();
		}
	}

	public void Show()
	{
		if (!_visible || !IsMoving())
		{
			moverTransform.anchoredPosition = _basePosition + _basePositionOffset;
		}
		_visible = true;
		handle.gameObject.SetActive(value: true);
		if (currentBackground != null)
		{
			currentBackground.SetActive(value: true);
		}
		if (dimWhenNotMoving)
		{
			float opacity = ((!IsMoving()) ? 0.5f : 1f);
			SetOpacity(opacity);
		}
	}

	public void Hide()
	{
		_visible = false;
		_underPointerControl = false;
		if (moveToTouch)
		{
			moverTransform.anchoredPosition = _basePosition + _basePositionOffset;
		}
		handle.Press(press: false);
		handle.gameObject.SetActive(value: false);
		if (currentBackground != null)
		{
			currentBackground.SetActive(value: false);
		}
	}

	public void SetPointerControlEnabled(bool enabled)
	{
		_pointerControlEnabled = enabled;
		raycastTarget = enabled;
	}

	private void SetOpacity(float opacity)
	{
		Color color = new Color(1f, 1f, 1f, opacity);
		if (currentBackgroundImage != null)
		{
			currentBackgroundImage.color = color;
		}
		handle.SetColor(color);
	}

	public void SetDirectionMask(MoverDirectionMask mask)
	{
		if (activeMask == mask)
		{
			return;
		}
		activeMask |= mask;
		if (useDirectionalBackgrounds)
		{
			for (int i = 0; i < moverBackgrounds.Length; i++)
			{
				if (moverBackgrounds[i].directionMask == activeMask)
				{
					currentBackground = moverBackgrounds[i].moverBackgroundObj;
					currentBackgroundImage = currentBackground.GetComponent<Image>();
					if (useDirectionalHandles && moverBackgrounds[i].handleSprite != null)
					{
						handle.SetImage(moverBackgrounds[i].handleSprite);
					}
				}
				else
				{
					moverBackgrounds[i].moverBackgroundObj.SetActive(value: false);
				}
			}
		}
		if (_visible)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void SetToInputAxis1()
	{
		_leftInputAxis = MappableInput.AXIS1_LEFT;
		_rightInputAxis = MappableInput.AXIS1_RIGHT;
		_upInputAxis = MappableInput.AXIS1_UP;
		_downInputAxis = MappableInput.AXIS1_DOWN;
	}

	public void SetToInputAxis2()
	{
		_leftInputAxis = MappableInput.AXIS2_LEFT;
		_rightInputAxis = MappableInput.AXIS2_RIGHT;
		_upInputAxis = MappableInput.AXIS2_UP;
		_downInputAxis = MappableInput.AXIS2_DOWN;
	}

	public void UpdateControl()
	{
		Vector2 vector = Vector2.zero;
		if (!_active)
		{
			_offset = Vector2.zero;
			return;
		}
		Show();
		if (Blocksworld.lockInput)
		{
			_movementMode = MovementMode.Zeroed;
		}
		else if (_underPointerControl)
		{
			Vector2 offsetFromPointer = GetOffsetFromPointer();
			vector = offsetFromPointer - _moveStartPos;
			_movementMode = MovementMode.Direct;
		}
		else
		{
			float num = MappedInput.InputAxis(_leftInputAxis);
			float num2 = MappedInput.InputAxis(_rightInputAxis);
			float num3 = MappedInput.InputAxis(_upInputAxis);
			float num4 = MappedInput.InputAxis(_downInputAxis);
			if (num > 0f || num2 > 0f || num3 > 0f || num4 > 0f)
			{
				_movementMode = MovementMode.MoveToTarget;
				vector = new Vector2(num2 - num, num3 - num4);
				if (vector.sqrMagnitude > 1f)
				{
					vector.Normalize();
				}
				vector *= _maxOffsetMagnitude;
			}
			else
			{
				_movementMode = MovementMode.Released;
			}
		}
		switch (_movementMode)
		{
		case MovementMode.Zeroed:
			_offset = Vector2.zero;
			break;
		case MovementMode.Direct:
			_offset = vector;
			break;
		case MovementMode.MoveToTarget:
			_offset = Vector2.Lerp(_offset, vector, controlSnap * Time.deltaTime);
			break;
		case MovementMode.Released:
		{
			float b = releaseSnap * 100f * Time.deltaTime * NormalizedScreen.scale;
			b = Mathf.Min(_offset.magnitude, b);
			_offset -= b * _offset.normalized;
			if (_offset.sqrMagnitude < 0.1f)
			{
				_offset = Vector2.zero;
				_movementMode = MovementMode.Zeroed;
				if (moveToTouch)
				{
					moverTransform.anchoredPosition = _basePosition + _basePositionOffset;
				}
			}
			break;
		}
		}
		_offset = ClampOffset(_offset);
		handle.Press(_movementMode == MovementMode.MoveToTarget);
		handle.MoveTo(_offset);
	}

	public Vector2 GetOffset()
	{
		return _offset;
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
		if (!_underPointerControl)
		{
			return _offset.sqrMagnitude > 0f;
		}
		return true;
	}

	public bool OwnsTouch(int touchId)
	{
		if (!BW.Options.useTouch())
		{
			if (_pointerControlEnabled)
			{
				return _underPointerControl;
			}
			return false;
		}
		if (Input.touchCount <= touchId)
		{
			return false;
		}
		bool flag = Input.GetTouch(touchId).fingerId == _pointerId;
		return _pointerControlEnabled && _underPointerControl && flag;
	}

	public void SetTouchZoneScale(Vector2 scale)
	{
		_inputTransform.sizeDelta = new Vector2(Mathf.Clamp01(scale.x) * _touchZoneFullScale.x, Mathf.Clamp01(scale.y) * _touchZoneFullScale.y);
	}

	private Vector2 GetOffsetFromPointer()
	{
		Vector2 vector = _moveStartPos;
		if (BW.Options.useTouch())
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.fingerId == _pointerId)
				{
					vector = touch.position;
					break;
				}
			}
		}
		else if (BW.Options.useMouse())
		{
			vector = Input.mousePosition;
		}
		vector -= new Vector2(base.transform.position.x, base.transform.position.y);
		return vector / NormalizedScreen.pixelScale;
	}

	private Vector2 ClampOffset(Vector2 offset)
	{
		if (offset.magnitude > _maxOffsetMagnitude)
		{
			offset = _maxOffsetMagnitude * offset.normalized;
		}
		bool flag = (activeMask & MoverDirectionMask.LEFT) != 0;
		bool flag2 = (activeMask & MoverDirectionMask.RIGHT) != 0;
		bool flag3 = (activeMask & MoverDirectionMask.UP) != 0;
		bool flag4 = (activeMask & MoverDirectionMask.DOWN) != 0;
		float num = offset.x;
		float num2 = offset.y;
		if (!flag)
		{
			num = Mathf.Max(0f, num);
		}
		if (!flag2)
		{
			num = Mathf.Min(0f, num);
		}
		if (!flag3)
		{
			num2 = Mathf.Min(0f, num2);
		}
		if (!flag4)
		{
			num2 = Mathf.Max(0f, num2);
		}
		return new Vector2(num, num2);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_active && _pointerControlEnabled)
		{
			Show();
			Tutorial.TurnOffMoverHelp();
			if (BW.Options.useTouch())
			{
				_pointerId = eventData.pointerId;
			}
			_moveStartPos = GetOffsetFromPointer();
			_moveStartPos.x = Mathf.Clamp(_moveStartPos.x, _distanceFromMoverExtentToCenter, _maxOffsetPos.x);
			_moveStartPos.y = Mathf.Clamp(_moveStartPos.y, _distanceFromMoverExtentToCenter, _maxOffsetPos.y);
			if (moveToTouch)
			{
				moverTransform.anchoredPosition = _moveStartPos;
			}
			if (dimWhenNotMoving)
			{
				SetOpacity(1f);
			}
			_underPointerControl = true;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (_active && _pointerControlEnabled)
		{
			if (BW.Options.useTouch() && eventData.pointerId == _pointerId)
			{
				_pointerId = -1;
			}
			_underPointerControl = false;
			if (dimWhenNotMoving)
			{
				SetOpacity(0.5f);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}
}
