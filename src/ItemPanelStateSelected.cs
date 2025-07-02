using UnityEngine;
using UnityEngine.UI;

public class ItemPanelStateSelected : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Selectable component = animator.GetComponent<Selectable>();
		component.Select();
	}
}
