using UnityEngine;

public class ItemPanelStateExpandedReveal : StateMachineBehaviour
{
	private ItemPanel panel;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		panel = animator.GetComponent<ItemPanel>();
		panel.ShowExpandedContents();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		panel.HideExpandedContents();
	}
}
