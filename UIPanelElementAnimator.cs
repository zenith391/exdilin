using System;
using UnityEngine;

// Token: 0x02000413 RID: 1043
public class UIPanelElementAnimator : MonoBehaviour
{
	// Token: 0x06002D67 RID: 11623 RVA: 0x00143E05 File Offset: 0x00142205
	public void TriggerAnimation(string animTrigger)
	{
		this.animator.ResetTrigger("Reset");
		this.animator.SetTrigger(animTrigger);
	}

	// Token: 0x06002D68 RID: 11624 RVA: 0x00143E23 File Offset: 0x00142223
	public void Reset()
	{
		this.animator.SetTrigger("Reset");
	}

	// Token: 0x040025EB RID: 9707
	public string animKey;

	// Token: 0x040025EC RID: 9708
	public Animator animator;
}
