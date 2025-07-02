using UnityEngine;
using UnityEngine.UI;

public class ItemPanelStatePressed : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Selectable component = animator.GetComponent<Selectable>();
		component.Select();
	}
}
