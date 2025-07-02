using UnityEngine;
using UnityEngine.EventSystems;

public class UIMenuBarButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public MenuBarButtonEnum menuBarButton;

	public bool deselectSiblings = true;

	public string selectAnimatorTrigger = "Select";

	public string deselectAnimatorTrigger = "Deselect";

	private Animator animator;

	private void OnEnable()
	{
		animator = GetComponent<Animator>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Select();
	}

	public void Select()
	{
		animator.SetTrigger(selectAnimatorTrigger);
		if (deselectSiblings)
		{
			DeselectSiblings();
		}
	}

	public void Deselect()
	{
		animator.SetTrigger(deselectAnimatorTrigger);
	}

	private void DeselectSiblings()
	{
		MenuBar componentInParent = GetComponentInParent<MenuBar>();
		componentInParent.DeselectCustomButton();
		if (!(componentInParent != null))
		{
			return;
		}
		UIMenuBarButton[] componentsInChildren = componentInParent.GetComponentsInChildren<UIMenuBarButton>();
		UIMenuBarButton[] array = componentsInChildren;
		foreach (UIMenuBarButton uIMenuBarButton in array)
		{
			if (uIMenuBarButton != this)
			{
				uIMenuBarButton.Deselect();
			}
		}
	}
}
