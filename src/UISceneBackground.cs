using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x02000458 RID: 1112
public class UISceneBackground : Selectable
{
	// Token: 0x06002F25 RID: 12069 RVA: 0x0014E337 File Offset: 0x0014C737
	private new void OnEnable()
	{
		base.transition = Selectable.Transition.None;
	}

	// Token: 0x06002F26 RID: 12070 RVA: 0x0014E340 File Offset: 0x0014C740
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
