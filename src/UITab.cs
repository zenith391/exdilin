using UnityEngine;
using UnityEngine.EventSystems;

public class UITab : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public new string tag;

	public bool selectOnClick;

	public string selectAnimatorTrigger = "Select";

	public string deselectAnimatorTrigger = "Deselect";

	private Animator animator;

	private UITabGroup tabGroup;

	private void OnEnable()
	{
		animator = GetComponent<Animator>();
		tabGroup = GetComponentInParent<UITabGroup>();
		if (tabGroup == null)
		{
			BWLog.Error("UITab outside of UITabGroup");
			base.enabled = false;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (selectOnClick)
		{
			tabGroup.SelectTab(this);
		}
	}

	public void Select(bool select)
	{
		if (select)
		{
			Select();
		}
		else
		{
			Deselect();
		}
	}

	public void Select()
	{
		animator.SetTrigger(selectAnimatorTrigger);
	}

	public void Deselect()
	{
		animator.SetTrigger(deselectAnimatorTrigger);
	}
}
