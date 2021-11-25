using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020003DA RID: 986
[RequireComponent(typeof(Button))]
public class InstantiatePopupButton : MonoBehaviour, IPopupDelegate
{
	// Token: 0x06002BBF RID: 11199 RVA: 0x0013B9C1 File Offset: 0x00139DC1
	private void OnEnable()
	{
		this.button = base.GetComponent<Button>();
		this.button.onClick.AddListener(new UnityAction(this.Spawn));
	}

	// Token: 0x06002BC0 RID: 11200 RVA: 0x0013B9EB File Offset: 0x00139DEB
	private void OnDisable()
	{
		this.button.onClick.RemoveListener(new UnityAction(this.Spawn));
	}

	// Token: 0x06002BC1 RID: 11201 RVA: 0x0013BA09 File Offset: 0x00139E09
	public void ClosePopup()
	{
		if (this.backgroundHideAnimator != null)
		{
			this.backgroundHideAnimator.SetTrigger(this.showBackgroundTrigger);
		}
		UnityEngine.Object.Destroy(this.instantiatedObject);
		this.instantiatedObject = null;
	}

	// Token: 0x06002BC2 RID: 11202 RVA: 0x0013BA40 File Offset: 0x00139E40
	private void Spawn()
	{
		if (this.instantiatedObject != null)
		{
			return;
		}
		if (this.backgroundHideAnimator != null)
		{
			this.backgroundHideAnimator.SetTrigger(this.hideBackgroundTrigger);
		}
		this.instantiatedObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab.gameObject);
		this.popup = this.instantiatedObject.GetComponent<UIPopup>();
		if (this.parentTo != null)
		{
			RectTransform rectTransform = (RectTransform)this.instantiatedObject.transform;
			rectTransform.SetParent(this.parentTo, false);
			rectTransform.SetSiblingIndex(this.siblingIndex);
		}
		this.popup.Show(this);
	}

	// Token: 0x040024FA RID: 9466
	public UIPopup prefab;

	// Token: 0x040024FB RID: 9467
	public RectTransform parentTo;

	// Token: 0x040024FC RID: 9468
	public int siblingIndex;

	// Token: 0x040024FD RID: 9469
	public Animator backgroundHideAnimator;

	// Token: 0x040024FE RID: 9470
	public string hideBackgroundTrigger = "Disable";

	// Token: 0x040024FF RID: 9471
	public string showBackgroundTrigger = "Enable";

	// Token: 0x04002500 RID: 9472
	private Button button;

	// Token: 0x04002501 RID: 9473
	private GameObject instantiatedObject;

	// Token: 0x04002502 RID: 9474
	private UIPopup popup;
}
