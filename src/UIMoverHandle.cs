using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000310 RID: 784
public class UIMoverHandle : MonoBehaviour
{
	// Token: 0x0600235E RID: 9054 RVA: 0x00105A31 File Offset: 0x00103E31
	public void Init()
	{
		this._image = base.GetComponent<Image>();
		this._rt = (RectTransform)base.transform;
	}

	// Token: 0x0600235F RID: 9055 RVA: 0x00105A50 File Offset: 0x00103E50
	public void MoveTo(Vector2 offset)
	{
		this._rt.anchoredPosition = offset;
	}

	// Token: 0x06002360 RID: 9056 RVA: 0x00105A5E File Offset: 0x00103E5E
	public void SetImage(Sprite sprite)
	{
		this._image.sprite = sprite;
		this.normalSprite = sprite;
		this.pressedSprite = sprite;
	}

	// Token: 0x06002361 RID: 9057 RVA: 0x00105A7A File Offset: 0x00103E7A
	public void Press(bool press)
	{
		this._image.sprite = ((!press) ? this.normalSprite : this.pressedSprite);
	}

	// Token: 0x06002362 RID: 9058 RVA: 0x00105A9E File Offset: 0x00103E9E
	public void SetColor(Color c)
	{
		this._image.color = c;
	}

	// Token: 0x06002363 RID: 9059 RVA: 0x00105AAC File Offset: 0x00103EAC
	public void SetAlpha(float a)
	{
		Color color = this._image.color;
		color.a = a;
		this._image.color = color;
	}

	// Token: 0x04001E99 RID: 7833
	private Image _image;

	// Token: 0x04001E9A RID: 7834
	private RectTransform _rt;

	// Token: 0x04001E9B RID: 7835
	public Sprite normalSprite;

	// Token: 0x04001E9C RID: 7836
	public Sprite pressedSprite;
}
