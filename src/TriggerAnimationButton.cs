using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020003DC RID: 988
[RequireComponent(typeof(Button))]
public class TriggerAnimationButton : MonoBehaviour
{
	// Token: 0x06002BC8 RID: 11208 RVA: 0x0013BB99 File Offset: 0x00139F99
	private void OnEnable()
	{
		this.button = base.GetComponent<Button>();
		this.button.onClick.AddListener(new UnityAction(this.Trigger));
	}

	// Token: 0x06002BC9 RID: 11209 RVA: 0x0013BBC3 File Offset: 0x00139FC3
	private void OnDisable()
	{
		this.button.onClick.RemoveListener(new UnityAction(this.Trigger));
	}

	// Token: 0x06002BCA RID: 11210 RVA: 0x0013BBE1 File Offset: 0x00139FE1
	private void Trigger()
	{
		if (this.animator == null)
		{
			return;
		}
		this.animator.SetTrigger(this.animationTrigger);
	}

	// Token: 0x04002507 RID: 9479
	public Animator animator;

	// Token: 0x04002508 RID: 9480
	public string animationTrigger;

	// Token: 0x04002509 RID: 9481
	private Button button;
}
