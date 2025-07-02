using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TriggerAnimationButton : MonoBehaviour
{
	public Animator animator;

	public string animationTrigger;

	private Button button;

	private void OnEnable()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(Trigger);
	}

	private void OnDisable()
	{
		button.onClick.RemoveListener(Trigger);
	}

	private void Trigger()
	{
		if (!(animator == null))
		{
			animator.SetTrigger(animationTrigger);
		}
	}
}
