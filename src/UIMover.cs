using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x0200030C RID: 780
public class UIMover : Graphic, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	// Token: 0x06002345 RID: 9029 RVA: 0x00104E58 File Offset: 0x00103258
	public void Init()
	{
		this._inputTransform = (RectTransform)base.transform;
		this._touchZoneFullScale = this._inputTransform.sizeDelta;
		this._basePosition = this.moverTransform.anchoredPosition;
		this._basePositionOffset = Vector2.zero;
		this._maxOffsetMagnitude = this.movementScale * NormalizedScreen.scale;
		this._distanceFromMoverExtentToCenter = (206f + this.movementScale * 0.75f) * 0.5f * NormalizedScreen.scale;
		this._maxOffsetPos.x = ((RectTransform)base.transform).rect.width - this._distanceFromMoverExtentToCenter;
		this._maxOffsetPos.y = ((RectTransform)base.transform).rect.height - this._distanceFromMoverExtentToCenter;
		this.handle.Init();
		if (this.leftKeyIndicator != null)
		{
			this._leftKeyIndicatorImage = this.leftKeyIndicator.GetComponentsInChildren<Image>();
			this._leftKeyIndicatorText = this.leftKeyIndicator.GetComponentsInChildren<Text>();
		}
		if (this.rightKeyIndicator != null)
		{
			this._rightKeyIndicatorImage = this.rightKeyIndicator.GetComponentsInChildren<Image>();
			this._rightKeyIndicatorText = this.rightKeyIndicator.GetComponentsInChildren<Text>();
		}
		if (this.upKeyIndicator != null)
		{
			this._upKeyIndicatorImage = this.upKeyIndicator.GetComponentsInChildren<Image>();
			this._upKeyIndicatorText = this.upKeyIndicator.GetComponentsInChildren<Text>();
		}
		if (this.downKeyIndicator != null)
		{
			this._downKeyIndicatorImage = this.downKeyIndicator.GetComponentsInChildren<Image>();
			this._downKeyIndicatorText = this.downKeyIndicator.GetComponentsInChildren<Text>();
		}
	}

	// Token: 0x06002346 RID: 9030 RVA: 0x00105004 File Offset: 0x00103404
	public void SetActive(bool active)
	{
		if (active != this._active)
		{
			this._movementMode = UIMover.MovementMode.Zeroed;
			this._offset = Vector2.zero;
			this._underPointerControl = false;
		}
		this._active = active;
		base.gameObject.SetActive(active);
		if (this.dimWhenNotMoving)
		{
			float opacity = (!this.IsMoving()) ? 0.5f : 1f;
			this.SetOpacity(opacity);
		}
		if (!active)
		{
			this.activeMask = MoverDirectionMask.NONE;
		}
	}

	// Token: 0x06002347 RID: 9031 RVA: 0x00105084 File Offset: 0x00103484
	public void AdjustBasePositionOffset(float minSafeX, float minSafeY)
	{
		this._basePositionOffset = new Vector2(minSafeX - this._inputTransform.anchoredPosition.x, minSafeY - this._inputTransform.anchoredPosition.y);
		if (this._visible && !this.IsMoving())
		{
			this.Show();
		}
	}

	// Token: 0x06002348 RID: 9032 RVA: 0x001050E4 File Offset: 0x001034E4
	public void Show()
	{
		if (!this._visible || !this.IsMoving())
		{
			this.moverTransform.anchoredPosition = this._basePosition + this._basePositionOffset;
		}
		this._visible = true;
		this.handle.gameObject.SetActive(true);
		if (this.currentBackground != null)
		{
			this.currentBackground.SetActive(true);
		}
		if (this.dimWhenNotMoving)
		{
			float opacity = (!this.IsMoving()) ? 0.5f : 1f;
			this.SetOpacity(opacity);
		}
	}

	// Token: 0x06002349 RID: 9033 RVA: 0x00105188 File Offset: 0x00103588
	public void Hide()
	{
		this._visible = false;
		this._underPointerControl = false;
		if (this.moveToTouch)
		{
			this.moverTransform.anchoredPosition = this._basePosition + this._basePositionOffset;
		}
		this.handle.Press(false);
		this.handle.gameObject.SetActive(false);
		if (this.currentBackground != null)
		{
			this.currentBackground.SetActive(false);
		}
	}

	// Token: 0x0600234A RID: 9034 RVA: 0x00105204 File Offset: 0x00103604
	public void SetPointerControlEnabled(bool enabled)
	{
		this._pointerControlEnabled = enabled;
		this.raycastTarget = enabled;
	}

	// Token: 0x0600234B RID: 9035 RVA: 0x00105214 File Offset: 0x00103614
	private void SetOpacity(float opacity)
	{
		Color color = new Color(1f, 1f, 1f, opacity);
		if (this.currentBackgroundImage != null)
		{
			this.currentBackgroundImage.color = color;
		}
		this.handle.SetColor(color);
	}

	// Token: 0x0600234C RID: 9036 RVA: 0x00105264 File Offset: 0x00103664
	public void SetDirectionMask(MoverDirectionMask mask)
	{
		if (this.activeMask == mask)
		{
			return;
		}
		this.activeMask |= mask;
		if (this.useDirectionalBackgrounds)
		{
			for (int i = 0; i < this.moverBackgrounds.Length; i++)
			{
				if (this.moverBackgrounds[i].directionMask == this.activeMask)
				{
					this.currentBackground = this.moverBackgrounds[i].moverBackgroundObj;
					this.currentBackgroundImage = this.currentBackground.GetComponent<Image>();
					if (this.useDirectionalHandles && this.moverBackgrounds[i].handleSprite != null)
					{
						this.handle.SetImage(this.moverBackgrounds[i].handleSprite);
					}
				}
				else
				{
					this.moverBackgrounds[i].moverBackgroundObj.SetActive(false);
				}
			}
		}
		if (this._visible)
		{
			this.Show();
		}
		else
		{
			this.Hide();
		}
	}

	// Token: 0x0600234D RID: 9037 RVA: 0x0010535B File Offset: 0x0010375B
	public void SetToInputAxis1()
	{
		this._leftInputAxis = MappableInput.AXIS1_LEFT;
		this._rightInputAxis = MappableInput.AXIS1_RIGHT;
		this._upInputAxis = MappableInput.AXIS1_UP;
		this._downInputAxis = MappableInput.AXIS1_DOWN;
	}

	// Token: 0x0600234E RID: 9038 RVA: 0x00105379 File Offset: 0x00103779
	public void SetToInputAxis2()
	{
		this._leftInputAxis = MappableInput.AXIS2_LEFT;
		this._rightInputAxis = MappableInput.AXIS2_RIGHT;
		this._upInputAxis = MappableInput.AXIS2_UP;
		this._downInputAxis = MappableInput.AXIS2_DOWN;
	}

	// Token: 0x0600234F RID: 9039 RVA: 0x0010539C File Offset: 0x0010379C
	public void UpdateControl()
	{
		Vector2 vector = Vector2.zero;
		if (!this._active)
		{
			this._offset = Vector2.zero;
			return;
		}
		this.Show();
		if (Blocksworld.lockInput)
		{
			this._movementMode = UIMover.MovementMode.Zeroed;
		}
		else if (this._underPointerControl)
		{
			Vector2 offsetFromPointer = this.GetOffsetFromPointer();
			vector = offsetFromPointer - this._moveStartPos;
			this._movementMode = UIMover.MovementMode.Direct;
		}
		else
		{
			float num = MappedInput.InputAxis(this._leftInputAxis);
			float num2 = MappedInput.InputAxis(this._rightInputAxis);
			float num3 = MappedInput.InputAxis(this._upInputAxis);
			float num4 = MappedInput.InputAxis(this._downInputAxis);
			if (num > 0f || num2 > 0f || num3 > 0f || num4 > 0f)
			{
				this._movementMode = UIMover.MovementMode.MoveToTarget;
				vector = new Vector2(num2 - num, num3 - num4);
				if (vector.sqrMagnitude > 1f)
				{
					vector.Normalize();
				}
				vector *= this._maxOffsetMagnitude;
			}
			else
			{
				this._movementMode = UIMover.MovementMode.Released;
			}
		}
		switch (this._movementMode)
		{
		case UIMover.MovementMode.Zeroed:
			this._offset = Vector2.zero;
			break;
		case UIMover.MovementMode.Direct:
			this._offset = vector;
			break;
		case UIMover.MovementMode.MoveToTarget:
			this._offset = Vector2.Lerp(this._offset, vector, this.controlSnap * Time.deltaTime);
			break;
		case UIMover.MovementMode.Released:
		{
			float num5 = this.releaseSnap * 100f * Time.deltaTime * NormalizedScreen.scale;
			num5 = Mathf.Min(this._offset.magnitude, num5);
			this._offset -= num5 * this._offset.normalized;
			if (this._offset.sqrMagnitude < 0.1f)
			{
				this._offset = Vector2.zero;
				this._movementMode = UIMover.MovementMode.Zeroed;
				if (this.moveToTouch)
				{
					this.moverTransform.anchoredPosition = this._basePosition + this._basePositionOffset;
				}
			}
			break;
		}
		}
		this._offset = this.ClampOffset(this._offset);
		this.handle.Press(this._movementMode == UIMover.MovementMode.MoveToTarget);
		this.handle.MoveTo(this._offset);
	}

	// Token: 0x06002350 RID: 9040 RVA: 0x001055F2 File Offset: 0x001039F2
	public Vector2 GetOffset()
	{
		return this._offset;
	}

	// Token: 0x06002351 RID: 9041 RVA: 0x001055FA File Offset: 0x001039FA
	public Vector2 GetNormalizedOffset()
	{
		return this._offset / this._maxOffsetMagnitude;
	}

	// Token: 0x06002352 RID: 9042 RVA: 0x00105610 File Offset: 0x00103A10
	public Vector3 GetWorldOffset()
	{
		Vector2 normalizedOffset = this.GetNormalizedOffset();
		Vector3 a = Blocksworld.cameraForward * normalizedOffset.y;
		Vector3 b = Blocksworld.cameraRight * normalizedOffset.x;
		return a + b;
	}

	// Token: 0x06002353 RID: 9043 RVA: 0x00105651 File Offset: 0x00103A51
	public bool IsMoving()
	{
		return this._underPointerControl || this._offset.sqrMagnitude > 0f;
	}

	// Token: 0x06002354 RID: 9044 RVA: 0x00105674 File Offset: 0x00103A74
	public bool OwnsTouch(int touchId)
	{
		if (!BW.Options.useTouch())
		{
			return this._pointerControlEnabled && this._underPointerControl;
		}
		if (Input.touchCount <= touchId)
		{
			return false;
		}
		bool flag = Input.GetTouch(touchId).fingerId == this._pointerId;
		return this._pointerControlEnabled && this._underPointerControl && flag;
	}

	// Token: 0x06002355 RID: 9045 RVA: 0x001056E4 File Offset: 0x00103AE4
	public void SetTouchZoneScale(Vector2 scale)
	{
		this._inputTransform.sizeDelta = new Vector2(Mathf.Clamp01(scale.x) * this._touchZoneFullScale.x, Mathf.Clamp01(scale.y) * this._touchZoneFullScale.y);
	}

	// Token: 0x06002356 RID: 9046 RVA: 0x00105734 File Offset: 0x00103B34
	private Vector2 GetOffsetFromPointer()
	{
		Vector2 a = this._moveStartPos;
		if (BW.Options.useTouch())
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.fingerId == this._pointerId)
				{
					a = touch.position;
					break;
				}
			}
		}
		else if (BW.Options.useMouse())
		{
			a = Input.mousePosition;
		}
		a -= new Vector2(base.transform.position.x, base.transform.position.y);
		return a / NormalizedScreen.pixelScale;
	}

	// Token: 0x06002357 RID: 9047 RVA: 0x001057F0 File Offset: 0x00103BF0
	private Vector2 ClampOffset(Vector2 offset)
	{
		if (offset.magnitude > this._maxOffsetMagnitude)
		{
			offset = this._maxOffsetMagnitude * offset.normalized;
		}
		bool flag = (this.activeMask & MoverDirectionMask.LEFT) != MoverDirectionMask.NONE;
		bool flag2 = (this.activeMask & MoverDirectionMask.RIGHT) != MoverDirectionMask.NONE;
		bool flag3 = (this.activeMask & MoverDirectionMask.UP) != MoverDirectionMask.NONE;
		bool flag4 = (this.activeMask & MoverDirectionMask.DOWN) != MoverDirectionMask.NONE;
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

	// Token: 0x06002358 RID: 9048 RVA: 0x001058CC File Offset: 0x00103CCC
	public void OnPointerDown(PointerEventData eventData)
	{
		if (!this._active || !this._pointerControlEnabled)
		{
			return;
		}
		this.Show();
		Tutorial.TurnOffMoverHelp();
		if (BW.Options.useTouch())
		{
			this._pointerId = eventData.pointerId;
		}
		this._moveStartPos = this.GetOffsetFromPointer();
		this._moveStartPos.x = Mathf.Clamp(this._moveStartPos.x, this._distanceFromMoverExtentToCenter, this._maxOffsetPos.x);
		this._moveStartPos.y = Mathf.Clamp(this._moveStartPos.y, this._distanceFromMoverExtentToCenter, this._maxOffsetPos.y);
		if (this.moveToTouch)
		{
			this.moverTransform.anchoredPosition = this._moveStartPos;
		}
		if (this.dimWhenNotMoving)
		{
			this.SetOpacity(1f);
		}
		this._underPointerControl = true;
	}

	// Token: 0x06002359 RID: 9049 RVA: 0x001059B4 File Offset: 0x00103DB4
	public void OnPointerUp(PointerEventData eventData)
	{
		if (!this._active)
		{
			return;
		}
		if (!this._pointerControlEnabled)
		{
			return;
		}
		if (BW.Options.useTouch() && eventData.pointerId == this._pointerId)
		{
			this._pointerId = -1;
		}
		this._underPointerControl = false;
		if (this.dimWhenNotMoving)
		{
			this.SetOpacity(0.5f);
		}
	}

	// Token: 0x0600235A RID: 9050 RVA: 0x00105A1D File Offset: 0x00103E1D
	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	// Token: 0x0600235B RID: 9051 RVA: 0x00105A1F File Offset: 0x00103E1F
	public void OnPointerExit(PointerEventData eventData)
	{
	}

	// Token: 0x04001E5C RID: 7772
	public UIMoverHandle handle;

	// Token: 0x04001E5D RID: 7773
	public MoverBackgroundDef[] moverBackgrounds;

	// Token: 0x04001E5E RID: 7774
	public float controlSnap = 8f;

	// Token: 0x04001E5F RID: 7775
	public float releaseSnap = 3f;

	// Token: 0x04001E60 RID: 7776
	public bool moveToTouch = true;

	// Token: 0x04001E61 RID: 7777
	public bool dimWhenNotMoving = true;

	// Token: 0x04001E62 RID: 7778
	public bool useDirectionalBackgrounds = true;

	// Token: 0x04001E63 RID: 7779
	public bool useDirectionalHandles = true;

	// Token: 0x04001E64 RID: 7780
	public RectTransform moverTransform;

	// Token: 0x04001E65 RID: 7781
	public GameObject leftKeyIndicator;

	// Token: 0x04001E66 RID: 7782
	public GameObject rightKeyIndicator;

	// Token: 0x04001E67 RID: 7783
	public GameObject upKeyIndicator;

	// Token: 0x04001E68 RID: 7784
	public GameObject downKeyIndicator;

	// Token: 0x04001E69 RID: 7785
	public float movementScale = 100f;

	// Token: 0x04001E6A RID: 7786
	public int visibilityGroup;

	// Token: 0x04001E6B RID: 7787
	private bool _pointerControlEnabled = true;

	// Token: 0x04001E6C RID: 7788
	private MoverDirectionMask activeMask;

	// Token: 0x04001E6D RID: 7789
	private GameObject currentBackground;

	// Token: 0x04001E6E RID: 7790
	private Image currentBackgroundImage;

	// Token: 0x04001E6F RID: 7791
	private bool _active;

	// Token: 0x04001E70 RID: 7792
	private bool _visible;

	// Token: 0x04001E71 RID: 7793
	private bool _underPointerControl;

	// Token: 0x04001E72 RID: 7794
	private int _pointerId = -1;

	// Token: 0x04001E73 RID: 7795
	private float _distanceFromMoverExtentToCenter = 150f;

	// Token: 0x04001E74 RID: 7796
	private Vector2 _maxOffsetPos = new Vector2(150f, 300f);

	// Token: 0x04001E75 RID: 7797
	private Vector2 _moveStartPos;

	// Token: 0x04001E76 RID: 7798
	private Vector2 _offset;

	// Token: 0x04001E77 RID: 7799
	private float _maxOffsetMagnitude;

	// Token: 0x04001E78 RID: 7800
	private MappableInput _leftInputAxis;

	// Token: 0x04001E79 RID: 7801
	private MappableInput _rightInputAxis;

	// Token: 0x04001E7A RID: 7802
	private MappableInput _upInputAxis;

	// Token: 0x04001E7B RID: 7803
	private MappableInput _downInputAxis;

	// Token: 0x04001E7C RID: 7804
	private Image[] _leftKeyIndicatorImage;

	// Token: 0x04001E7D RID: 7805
	private Image[] _rightKeyIndicatorImage;

	// Token: 0x04001E7E RID: 7806
	private Image[] _upKeyIndicatorImage;

	// Token: 0x04001E7F RID: 7807
	private Image[] _downKeyIndicatorImage;

	// Token: 0x04001E80 RID: 7808
	private Text[] _leftKeyIndicatorText;

	// Token: 0x04001E81 RID: 7809
	private Text[] _rightKeyIndicatorText;

	// Token: 0x04001E82 RID: 7810
	private Text[] _upKeyIndicatorText;

	// Token: 0x04001E83 RID: 7811
	private Text[] _downKeyIndicatorText;

	// Token: 0x04001E84 RID: 7812
	private RectTransform _inputTransform;

	// Token: 0x04001E85 RID: 7813
	private Vector2 _basePosition;

	// Token: 0x04001E86 RID: 7814
	private Vector2 _basePositionOffset;

	// Token: 0x04001E87 RID: 7815
	private Vector2 _touchZoneFullScale;

	// Token: 0x04001E88 RID: 7816
	private const float _dimmedOpacity = 0.5f;

	// Token: 0x04001E89 RID: 7817
	private UIMover.MovementMode _movementMode;

	// Token: 0x0200030D RID: 781
	private enum MovementMode
	{
		// Token: 0x04001E8B RID: 7819
		Zeroed,
		// Token: 0x04001E8C RID: 7820
		Direct,
		// Token: 0x04001E8D RID: 7821
		MoveToTarget,
		// Token: 0x04001E8E RID: 7822
		Released
	}
}
