using System;
using UnityEngine;

// Token: 0x02000464 RID: 1124
public class ItemPanelStateExpanded : StateMachineBehaviour
{
	// Token: 0x06002F7A RID: 12154 RVA: 0x0014F824 File Offset: 0x0014DC24
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		this.panel = animator.GetComponent<ItemPanel>();
		this.panel.LoadExpandedContents();
	}

	// Token: 0x040027CB RID: 10187
	private ItemPanel panel;
}
