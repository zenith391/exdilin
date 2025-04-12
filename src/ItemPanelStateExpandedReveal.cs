using System;
using UnityEngine;

// Token: 0x02000465 RID: 1125
public class ItemPanelStateExpandedReveal : StateMachineBehaviour
{
	// Token: 0x06002F7C RID: 12156 RVA: 0x0014F845 File Offset: 0x0014DC45
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		this.panel = animator.GetComponent<ItemPanel>();
		this.panel.ShowExpandedContents();
	}

	// Token: 0x06002F7D RID: 12157 RVA: 0x0014F85E File Offset: 0x0014DC5E
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		this.panel.HideExpandedContents();
	}

	// Token: 0x040027CC RID: 10188
	private ItemPanel panel;
}
