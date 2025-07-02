using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Deselectable : Selectable
{
	public new Animator animator;

	public string animatorDeselectTrigger = "Deselected";

	private bool pointerOver;

	public override void OnPointerEnter(PointerEventData eventData)
	{
		pointerOver = true;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		pointerOver = false;
	}

	public void Deselect()
	{
		if (!pointerOver && animator != null)
		{
			animator.SetTrigger(animatorDeselectTrigger);
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		Deselect();
	}
}
