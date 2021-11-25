using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020002E6 RID: 742
public class CompositeButtonTinter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	// Token: 0x060021EC RID: 8684 RVA: 0x000FE4B0 File Offset: 0x000FC8B0
	private void Awake()
	{
		for (int num = 0; num != base.transform.GetChildCount(); num++)
		{
			this._images.AddRange(base.transform.GetChild(num).GetComponentsInChildren<Image>());
			this._texts.AddRange(base.transform.GetChild(num).GetComponentsInChildren<Text>());
		}
		this._button = base.GetComponent<Button>();
		if (this._button != null)
		{
			this._button.onClick.AddListener(new UnityAction(this.OnButtonClick));
		}
		this.EnterState(CompositeButtonTinter.ButtonState.Normal);
	}

	// Token: 0x1700015D RID: 349
	// (get) Token: 0x060021ED RID: 8685 RVA: 0x000FE551 File Offset: 0x000FC951
	private bool Pressed
	{
		get
		{
			return this._currentState == CompositeButtonTinter.ButtonState.Pressed;
		}
	}

	// Token: 0x1700015E RID: 350
	// (get) Token: 0x060021EE RID: 8686 RVA: 0x000FE55C File Offset: 0x000FC95C
	private bool Ghosted
	{
		get
		{
			return this._currentState == CompositeButtonTinter.ButtonState.Ghosted;
		}
	}

	// Token: 0x060021EF RID: 8687 RVA: 0x000FE568 File Offset: 0x000FC968
	private void EnterState(CompositeButtonTinter.ButtonState state)
	{
		switch (state)
		{
		case CompositeButtonTinter.ButtonState.Normal:
			this.SetChildrenTint(this.normalTint);
			break;
		case CompositeButtonTinter.ButtonState.Highlighted:
			this.SetChildrenTint(this.highlightTint);
			break;
		case CompositeButtonTinter.ButtonState.Pressed:
			this.SetChildrenTint(this.pressedTint);
			break;
		case CompositeButtonTinter.ButtonState.Ghosted:
			this.SetChildrenTint(this.ghostedTint);
			break;
		}
	}

	// Token: 0x060021F0 RID: 8688 RVA: 0x000FE5D4 File Offset: 0x000FC9D4
	private void SetChildrenTint(Color color)
	{
		for (int num = 0; num != this._images.Count; num++)
		{
			this._images[num].color = color;
		}
		for (int num2 = 0; num2 != this._texts.Count; num2++)
		{
			this._texts[num2].color = color;
		}
	}

	// Token: 0x060021F1 RID: 8689 RVA: 0x000FE63D File Offset: 0x000FCA3D
	public void SetGhosted(bool status)
	{
		if (status)
		{
			Debug.Log("Ghost!");
			this.EnterState(CompositeButtonTinter.ButtonState.Ghosted);
		}
		else
		{
			this.EnterState(CompositeButtonTinter.ButtonState.Normal);
		}
	}

	// Token: 0x060021F2 RID: 8690 RVA: 0x000FE662 File Offset: 0x000FCA62
	public void Reset()
	{
		this.EnterState(CompositeButtonTinter.ButtonState.Normal);
		this._pointerOver = false;
	}

	// Token: 0x060021F3 RID: 8691 RVA: 0x000FE672 File Offset: 0x000FCA72
	public void OnDisable()
	{
		this.Reset();
	}

	// Token: 0x060021F4 RID: 8692 RVA: 0x000FE67A File Offset: 0x000FCA7A
	public void OnPointerDown(PointerEventData eventData)
	{
		if (this.Ghosted)
		{
			return;
		}
		if (this.Pressed)
		{
			return;
		}
		this.EnterState(CompositeButtonTinter.ButtonState.Pressed);
	}

	// Token: 0x060021F5 RID: 8693 RVA: 0x000FE69B File Offset: 0x000FCA9B
	private void OnButtonClick()
	{
		this.EnterState(CompositeButtonTinter.ButtonState.Normal);
	}

	// Token: 0x060021F6 RID: 8694 RVA: 0x000FE6A4 File Offset: 0x000FCAA4
	public void OnPointerUp(PointerEventData eventData)
	{
		if (this.Ghosted)
		{
			return;
		}
		if (!this.Pressed)
		{
			return;
		}
		if (this._pointerOver)
		{
			this.EnterState(CompositeButtonTinter.ButtonState.Highlighted);
		}
		else
		{
			this.EnterState(CompositeButtonTinter.ButtonState.Normal);
		}
	}

	// Token: 0x060021F7 RID: 8695 RVA: 0x000FE6DC File Offset: 0x000FCADC
	public void OnPointerEnter(PointerEventData eventData)
	{
		this._pointerOver = true;
		if (!this.Pressed && !this.Ghosted)
		{
			this.EnterState(CompositeButtonTinter.ButtonState.Highlighted);
		}
	}

	// Token: 0x060021F8 RID: 8696 RVA: 0x000FE702 File Offset: 0x000FCB02
	public void OnPointerExit(PointerEventData eventData)
	{
		if (!this.Ghosted)
		{
			this.EnterState(CompositeButtonTinter.ButtonState.Normal);
		}
		this._pointerOver = false;
	}

	// Token: 0x04001CF5 RID: 7413
	public Color normalTint = Color.white;

	// Token: 0x04001CF6 RID: 7414
	public Color highlightTint = new Color(0.9f, 0.9f, 0.9f, 1f);

	// Token: 0x04001CF7 RID: 7415
	public Color pressedTint = new Color(0.7f, 0.7f, 0.8f, 1f);

	// Token: 0x04001CF8 RID: 7416
	public Color ghostedTint = new Color(1f, 1f, 1f, 0.5f);

	// Token: 0x04001CF9 RID: 7417
	private bool _pointerOver;

	// Token: 0x04001CFA RID: 7418
	private CompositeButtonTinter.ButtonState _currentState;

	// Token: 0x04001CFB RID: 7419
	private List<Image> _images = new List<Image>();

	// Token: 0x04001CFC RID: 7420
	private List<Text> _texts = new List<Text>();

	// Token: 0x04001CFD RID: 7421
	private Button _button;

	// Token: 0x020002E7 RID: 743
	private enum ButtonState
	{
		// Token: 0x04001CFF RID: 7423
		Normal,
		// Token: 0x04001D00 RID: 7424
		Highlighted,
		// Token: 0x04001D01 RID: 7425
		Pressed,
		// Token: 0x04001D02 RID: 7426
		Ghosted
	}
}
