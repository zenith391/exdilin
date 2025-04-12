using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000467 RID: 1127
public class ItemPanelStateSelected : StateMachineBehaviour
{
	// Token: 0x06002F81 RID: 12161 RVA: 0x0014F898 File Offset: 0x0014DC98
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Selectable component = animator.GetComponent<Selectable>();
		component.Select();
	}
}
