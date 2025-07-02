using UnityEngine;

public class ItemPanelStateExpanded : StateMachineBehaviour
{
	private ItemPanel panel;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		panel = animator.GetComponent<ItemPanel>();
		panel.LoadExpandedContents();
	}
}
