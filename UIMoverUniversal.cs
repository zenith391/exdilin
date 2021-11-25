using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000311 RID: 785
public class UIMoverUniversal : Graphic
{
	// Token: 0x06002365 RID: 9061 RVA: 0x00105AEC File Offset: 0x00103EEC
	public void Init(int inputAxis)
	{
		this._moverMask = MoverDirectionMask.NONE;
		this.handle.Init();
		this._alpha = 1f;
		this._maxOffsetMagnitude = this.movementScale * NormalizedScreen.scale;
		this._offset = Vector2.zero;
		if (inputAxis != 2)
		{
			this.SetToInputAxis1();
		}
		else
		{
			this.SetToInputAxis2();
		}
		this._leftKeyIndicatorText = this.leftKeyIndicator.GetComponentsInChildren<Text>();
		this._rightKeyIndicatorText = this.rightKeyIndicator.GetComponentsInChildren<Text>();
		this._upKeyIndicatorText = this.upKeyIndicator.GetComponentsInChildren<Text>();
		this._downKeyIndicatorText = this.downKeyIndicator.GetComponentsInChildren<Text>();
		base.gameObject.SetActive(false);
	}

	// Token: 0x06002366 RID: 9062 RVA: 0x00105B9B File Offset: 0x00103F9B
	private void SetToInputAxis1()
	{
		this._leftInputAxis = MappableInput.AXIS1_LEFT;
		this._rightInputAxis = MappableInput.AXIS1_RIGHT;
		this._upInputAxis = MappableInput.AXIS1_UP;
		this._downInputAxis = MappableInput.AXIS1_DOWN;
	}

	// Token: 0x06002367 RID: 9063 RVA: 0x00105BB9 File Offset: 0x00103FB9
	private void SetToInputAxis2()
	{
		this._leftInputAxis = MappableInput.AXIS2_LEFT;
		this._rightInputAxis = MappableInput.AXIS2_RIGHT;
		this._upInputAxis = MappableInput.AXIS2_UP;
		this._downInputAxis = MappableInput.AXIS2_DOWN;
	}

	// Token: 0x06002368 RID: 9064 RVA: 0x00105BDC File Offset: 0x00103FDC
	public void UpdateAlphaFade(float alpha)
	{
		this._alpha = alpha;
		bool flag = (this._moverMask & MoverDirectionMask.LEFT) != MoverDirectionMask.NONE;
		bool flag2 = (this._moverMask & MoverDirectionMask.RIGHT) != MoverDirectionMask.NONE;
		bool flag3 = (this._moverMask & MoverDirectionMask.UP) != MoverDirectionMask.NONE;
		bool flag4 = (this._moverMask & MoverDirectionMask.DOWN) != MoverDirectionMask.NONE;
		bool flag5 = this._moverMask != MoverDirectionMask.NONE;
		this.dpadLeftBackground.alpha = this._alpha * ((!flag) ? 0.25f : 1f);
		this.dpadRightBackground.alpha = this._alpha * ((!flag2) ? 0.25f : 1f);
		this.dpadUpBackground.alpha = this._alpha * ((!flag3) ? 0.25f : 1f);
		this.dpadDownBackground.alpha = this._alpha * ((!flag4) ? 0.25f : 1f);
		this.handle.SetAlpha(this._alpha * ((!flag5) ? 0.25f : 1f));
	}

	// Token: 0x06002369 RID: 9065 RVA: 0x00105CFE File Offset: 0x001040FE
	public void UpdateDirectionMask(MoverDirectionMask mask)
	{
		this._moverMask |= mask;
		this.UpdateAlphaFade(this._alpha);
	}

	// Token: 0x0600236A RID: 9066 RVA: 0x00105D1A File Offset: 0x0010411A
	public new bool IsActive()
	{
		return this._moverMask != MoverDirectionMask.NONE;
	}

	// Token: 0x0600236B RID: 9067 RVA: 0x00105D28 File Offset: 0x00104128
	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	// Token: 0x0600236C RID: 9068 RVA: 0x00105D36 File Offset: 0x00104136
	public void Hide()
	{
		base.gameObject.SetActive(false);
		this._moverMask = MoverDirectionMask.NONE;
		this._offset = Vector2.zero;
	}

	// Token: 0x0600236D RID: 9069 RVA: 0x00105D58 File Offset: 0x00104158
	public void UpdateMover()
	{
		Vector2 vector = Vector2.zero;
		if (Blocksworld.lockInput)
		{
			this._offset = Vector2.zero;
			this.handle.Press(false);
		}
		else
		{
			float num = ((this._moverMask & MoverDirectionMask.LEFT) == MoverDirectionMask.NONE) ? 0f : MappedInput.InputAxis(this._leftInputAxis);
			float num2 = ((this._moverMask & MoverDirectionMask.RIGHT) == MoverDirectionMask.NONE) ? 0f : MappedInput.InputAxis(this._rightInputAxis);
			float num3 = ((this._moverMask & MoverDirectionMask.UP) == MoverDirectionMask.NONE) ? 0f : MappedInput.InputAxis(this._upInputAxis);
			float num4 = ((this._moverMask & MoverDirectionMask.DOWN) == MoverDirectionMask.NONE) ? 0f : MappedInput.InputAxis(this._downInputAxis);
			bool flag = num > 0f || num2 > 0f || num3 > 0f || num4 > 0f;
			this.handle.Press(flag);
			if (flag)
			{
				vector = new Vector2(num2 - num, num3 - num4);
				if (vector.sqrMagnitude > 1f)
				{
					vector.Normalize();
				}
				vector *= this._maxOffsetMagnitude;
			}
			float num5 = (!flag) ? 16f : 8f;
			Vector2 offset = Vector2.Lerp(this._offset, vector, num5 * Time.deltaTime);
			if (!flag && offset.sqrMagnitude < 0.1f)
			{
				offset = Vector2.zero;
			}
			if (offset.magnitude > this._maxOffsetMagnitude)
			{
				offset = this._maxOffsetMagnitude * offset.normalized;
			}
			this._offset = offset;
		}
		this.handle.MoveTo(this._offset);
	}

	// Token: 0x0600236E RID: 9070 RVA: 0x00105F20 File Offset: 0x00104320
	public Vector2 GetNormalizedOffset()
	{
		return this._offset / this._maxOffsetMagnitude;
	}

	// Token: 0x0600236F RID: 9071 RVA: 0x00105F34 File Offset: 0x00104334
	public Vector3 GetWorldOffset()
	{
		Vector2 normalizedOffset = this.GetNormalizedOffset();
		Vector3 a = Blocksworld.cameraForward * normalizedOffset.y;
		Vector3 b = Blocksworld.cameraRight * normalizedOffset.x;
		return a + b;
	}

	// Token: 0x06002370 RID: 9072 RVA: 0x00105F75 File Offset: 0x00104375
	public bool IsMoving()
	{
		return this._offset.sqrMagnitude > 0f;
	}

	// Token: 0x04001E9D RID: 7837
	public RectTransform moverTransform;

	// Token: 0x04001E9E RID: 7838
	public UIMoverHandle handle;

	// Token: 0x04001E9F RID: 7839
	public CanvasGroup dpadLeftBackground;

	// Token: 0x04001EA0 RID: 7840
	public CanvasGroup dpadRightBackground;

	// Token: 0x04001EA1 RID: 7841
	public CanvasGroup dpadUpBackground;

	// Token: 0x04001EA2 RID: 7842
	public CanvasGroup dpadDownBackground;

	// Token: 0x04001EA3 RID: 7843
	public GameObject leftKeyIndicator;

	// Token: 0x04001EA4 RID: 7844
	public GameObject rightKeyIndicator;

	// Token: 0x04001EA5 RID: 7845
	public GameObject upKeyIndicator;

	// Token: 0x04001EA6 RID: 7846
	public GameObject downKeyIndicator;

	// Token: 0x04001EA7 RID: 7847
	public float movementScale = 64f;

	// Token: 0x04001EA8 RID: 7848
	public int visibilityGroup;

	// Token: 0x04001EA9 RID: 7849
	private MoverDirectionMask _moverMask;

	// Token: 0x04001EAA RID: 7850
	private Text[] _leftKeyIndicatorText;

	// Token: 0x04001EAB RID: 7851
	private Text[] _rightKeyIndicatorText;

	// Token: 0x04001EAC RID: 7852
	private Text[] _upKeyIndicatorText;

	// Token: 0x04001EAD RID: 7853
	private Text[] _downKeyIndicatorText;

	// Token: 0x04001EAE RID: 7854
	private MappableInput _leftInputAxis;

	// Token: 0x04001EAF RID: 7855
	private MappableInput _rightInputAxis;

	// Token: 0x04001EB0 RID: 7856
	private MappableInput _upInputAxis;

	// Token: 0x04001EB1 RID: 7857
	private MappableInput _downInputAxis;

	// Token: 0x04001EB2 RID: 7858
	private float _alpha;

	// Token: 0x04001EB3 RID: 7859
	private float _maxOffsetMagnitude;

	// Token: 0x04001EB4 RID: 7860
	private Vector2 _offset;
}
