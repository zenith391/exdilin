using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000466 RID: 1126
public class ItemPanelStatePressed : StateMachineBehaviour
{
	// Token: 0x06002F7F RID: 12159 RVA: 0x0014F874 File Offset: 0x0014DC74
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Selectable component = animator.GetComponent<Selectable>();
		component.Select();
	}
}
