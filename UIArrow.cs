using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020002F7 RID: 759
public class UIArrow : MonoBehaviour
{
	// Token: 0x06002244 RID: 8772 RVA: 0x000FF7B8 File Offset: 0x000FDBB8
	public void Init()
	{
		this._rt = (RectTransform)base.transform;
		this._canvasGroup = base.GetComponent<CanvasGroup>();
		this._defaultHeadSize = this.headRT.sizeDelta;
		this._defaultShaftSize = this.shaftRT.sizeDelta;
		this._shaftImage = this.shaftRT.GetComponent<Image>();
		this._headImage = this.headRT.GetComponent<Image>();
	}

	// Token: 0x06002245 RID: 8773 RVA: 0x000FF826 File Offset: 0x000FDC26
	public void Show(bool show)
	{
		base.gameObject.SetActiveRecursively(show);
	}

	// Token: 0x06002246 RID: 8774 RVA: 0x000FF834 File Offset: 0x000FDC34
	public bool IsShowing()
	{
		return base.gameObject.activeSelf;
	}

	// Token: 0x06002247 RID: 8775 RVA: 0x000FF844 File Offset: 0x000FDC44
	public void SetSwipeMode(int mode)
	{
		switch (mode)
		{
		case 0:
			this._shaftImage.sprite = this.defaultShaftSprite;
			this._headImage.sprite = this.defaultHeadSprite;
			this._invisible = false;
			break;
		case 1:
			this._shaftImage.sprite = this.swipeShaftSprite;
			this._headImage.sprite = this.swipeHeadSprite;
			this._invisible = false;
			break;
		case 2:
			this._shaftImage.sprite = this.doubleSwiptShaftSprite;
			this._headImage.sprite = this.swipeHeadSprite;
			this._invisible = false;
			break;
		case 3:
			this._shaftImage.sprite = this.defaultShaftSprite;
			this._headImage.sprite = this.defaultHeadSprite;
			this._invisible = true;
			this._canvasGroup.alpha = 0f;
			break;
		}
	}

	// Token: 0x06002248 RID: 8776 RVA: 0x000FF934 File Offset: 0x000FDD34
	public void SetEndpoints(Vector2 fromPos, Vector2 toPos)
	{
		Vector2 from = toPos - fromPos;
		float magnitude = from.magnitude;
		this._rt.anchoredPosition = fromPos;
		this.headRT.anchoredPosition = magnitude * Vector2.right;
		float num = Mathf.Clamp(magnitude / 80f, 0.75f, 1f);
		float num2 = (magnitude >= 10f) ? 1f : 0f;
		this._canvasGroup.alpha = ((!this._invisible) ? num2 : 0f);
		this.shaftRT.sizeDelta = new Vector2(magnitude - num * 40f, num * this._defaultShaftSize.y);
		this.headRT.sizeDelta = num * this._defaultHeadSize;
		float z = Mathf.Sign(from.y) * Mathf.Abs(Vector2.Angle(from, Vector2.right));
		this._rt.localEulerAngles = new Vector3(0f, 0f, z);
	}

	// Token: 0x04001D3A RID: 7482
	public Vector2 debugFrom = Vector2.zero;

	// Token: 0x04001D3B RID: 7483
	public Vector2 debugTo = new Vector2(-256f, 0f);

	// Token: 0x04001D3C RID: 7484
	public RectTransform shaftRT;

	// Token: 0x04001D3D RID: 7485
	public RectTransform headRT;

	// Token: 0x04001D3E RID: 7486
	public Sprite defaultShaftSprite;

	// Token: 0x04001D3F RID: 7487
	public Sprite swipeShaftSprite;

	// Token: 0x04001D40 RID: 7488
	public Sprite doubleSwiptShaftSprite;

	// Token: 0x04001D41 RID: 7489
	public Sprite defaultHeadSprite;

	// Token: 0x04001D42 RID: 7490
	public Sprite swipeHeadSprite;

	// Token: 0x04001D43 RID: 7491
	private RectTransform _rt;

	// Token: 0x04001D44 RID: 7492
	private CanvasGroup _canvasGroup;

	// Token: 0x04001D45 RID: 7493
	private Vector2 _defaultHeadSize;

	// Token: 0x04001D46 RID: 7494
	private Vector2 _defaultShaftSize;

	// Token: 0x04001D47 RID: 7495
	private Image _shaftImage;

	// Token: 0x04001D48 RID: 7496
	private Image _headImage;

	// Token: 0x04001D49 RID: 7497
	private bool _invisible;

	// Token: 0x04001D4A RID: 7498
	private const float kMinScale = 0.75f;

	// Token: 0x04001D4B RID: 7499
	private const float kLengthForDefaultScale = 80f;

	// Token: 0x04001D4C RID: 7500
	private const float kFadeStartLength = 10f;

	// Token: 0x04001D4D RID: 7501
	private const float kHeadGap = 40f;
}
