using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020002F8 RID: 760
public class UIButton : MonoBehaviour
{
	// Token: 0x0600224A RID: 8778 RVA: 0x000FFA74 File Offset: 0x000FDE74
	public void Init(bool alphaMode = false)
	{
		this._rt = (RectTransform)base.transform;
		if (this.buttonText != null)
		{
			this.buttonText.Init();
		}
		this._defaultWidth = this._rt.sizeDelta.x;
		this._defaultHeight = this._rt.sizeDelta.y;
		this._preferredWidth = this._defaultWidth;
		this._layoutElement = base.GetComponent<LayoutElement>();
		this._alphaMode = alphaMode;
		this._tapStartComponent = this._rt.GetComponent<TapStartButton>();
		if (this._tapStartComponent != null)
		{
			this._tapStartComponent.tapAction = delegate()
			{
				this.DidClick();
			};
		}
		else
		{
			this._buttonComponent = this._rt.GetComponent<Button>();
		}
		if (this._alphaMode && !base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
	}

	// Token: 0x0600224B RID: 8779 RVA: 0x000FFB78 File Offset: 0x000FDF78
	public void Show()
	{
		if (this._alphaMode)
		{
			if (this._buttonComponent != null)
			{
				this._buttonComponent.interactable = true;
			}
			else if (this._tapStartComponent != null)
			{
				this._tapStartComponent.interactable = true;
			}
		}
		else
		{
			base.gameObject.SetActive(true);
		}
	}

	// Token: 0x0600224C RID: 8780 RVA: 0x000FFBE0 File Offset: 0x000FDFE0
	public void Hide()
	{
		if (this._alphaMode)
		{
			if (this._buttonComponent != null)
			{
				this._buttonComponent.interactable = false;
			}
			else if (this._tapStartComponent != null)
			{
				this._tapStartComponent.interactable = false;
			}
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	// Token: 0x0600224D RID: 8781 RVA: 0x000FFC48 File Offset: 0x000FE048
	public bool Hit(Vector3 pos)
	{
		return base.gameObject.activeInHierarchy && Util.RectTransformContains(this._rt, pos);
	}

	// Token: 0x0600224E RID: 8782 RVA: 0x000FFC6C File Offset: 0x000FE06C
	public void SetText(string text)
	{
		this._preferredWidth = this._defaultWidth;
		Vector2 sizeDelta = new Vector2(this._defaultWidth, this._defaultHeight);
		if (this.buttonText != null)
		{
			this.buttonText.Set(text);
			this._preferredWidth = Mathf.Max(this._defaultWidth, this.buttonText.PreferredWidth + 30f * NormalizedScreen.scale);
			sizeDelta = new Vector2(this._preferredWidth, this._defaultHeight);
		}
		((RectTransform)base.transform).sizeDelta = sizeDelta;
		if (this._layoutElement != null)
		{
			this._layoutElement.preferredWidth = this._preferredWidth;
		}
	}

	// Token: 0x1700015F RID: 351
	// (get) Token: 0x0600224F RID: 8783 RVA: 0x000FFD23 File Offset: 0x000FE123
	public float DefaultWidth
	{
		get
		{
			return this._defaultWidth;
		}
	}

	// Token: 0x17000160 RID: 352
	// (get) Token: 0x06002250 RID: 8784 RVA: 0x000FFD2B File Offset: 0x000FE12B
	public float PreferredWidth
	{
		get
		{
			return this._preferredWidth;
		}
	}

	// Token: 0x06002251 RID: 8785 RVA: 0x000FFD33 File Offset: 0x000FE133
	public void DidClick()
	{
		if (this.clickAction != null)
		{
			this.clickAction();
		}
	}

	// Token: 0x04001D4E RID: 7502
	public UnityAction clickAction;

	// Token: 0x04001D4F RID: 7503
	public UIEditableText buttonText;

	// Token: 0x04001D50 RID: 7504
	public float maxWidth = 220f;

	// Token: 0x04001D51 RID: 7505
	private float _defaultWidth = 155f;

	// Token: 0x04001D52 RID: 7506
	private float _defaultHeight = 55f;

	// Token: 0x04001D53 RID: 7507
	private float _preferredWidth = 155f;

	// Token: 0x04001D54 RID: 7508
	private RectTransform _rt;

	// Token: 0x04001D55 RID: 7509
	private LayoutElement _layoutElement;

	// Token: 0x04001D56 RID: 7510
	private bool _alphaMode;

	// Token: 0x04001D57 RID: 7511
	private Button _buttonComponent;

	// Token: 0x04001D58 RID: 7512
	private TapStartButton _tapStartComponent;
}
