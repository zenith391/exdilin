using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISceneBackground : Selectable
{
	private new void OnEnable()
	{
		base.transition = Transition.None;
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (MainUIController.active)
		{
			MainUIController.Instance.SceneBackgroundSelected();
			Selectable defaultSelectable = MainUIController.Instance.loadedSceneController.GetDefaultSelectable();
			base.navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = defaultSelectable,
				selectOnDown = defaultSelectable,
				selectOnLeft = defaultSelectable,
				selectOnRight = defaultSelectable
			};
		}
	}
}
