using UnityEngine;

public class UIPanelElementAnimator : MonoBehaviour
{
	public string animKey;

	public Animator animator;

	public void TriggerAnimation(string animTrigger)
	{
		animator.ResetTrigger("Reset");
		animator.SetTrigger(animTrigger);
	}

	public void Reset()
	{
		animator.SetTrigger("Reset");
	}
}
