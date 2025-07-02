using UnityEngine;

public class UISceneShopMenuModels : UISceneBase
{
	public RectTransform shoppingCartIcon;

	public UITabGroup tabGroup;

	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		base.SceneDidLoad(sceneInfo);
		if (uiController != null)
		{
			uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Shop);
		}
		if (tabGroup != null)
		{
			tabGroup.SelectTabWithTag("Models");
		}
	}

	public override void OnReturnToScene()
	{
		base.OnReturnToScene();
		uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Shop);
		if (tabGroup != null)
		{
			tabGroup.SelectTabWithTag("Models");
		}
	}

	public override Vector3 GetShoppingCartPosition()
	{
		if (shoppingCartIcon != null)
		{
			return shoppingCartIcon.position;
		}
		return base.GetShoppingCartPosition();
	}

	public override void HandleMenuInputEvents()
	{
		if (MappedInput.InputDown(MappableInput.MENU_CANCEL))
		{
			BWStandalone.Overlays.ShowPopupEscapeMenu();
		}
		else if (MappedInput.InputDown(MappableInput.MENU_HOME_PAGE) && MainUIController.active)
		{
			MainUIController.Instance.LoadUIScene("PlayMenu");
		}
		else if (MappedInput.InputDown(MappableInput.MENU_BUILD_PAGE) && MainUIController.active)
		{
			MainUIController.Instance.LoadUIScene("BuildMenu");
		}
		else if (MappedInput.InputDown(MappableInput.MENU_PROFILE_PAGE) && MainUIController.active)
		{
			MainUIController.Instance.LoadUIScene("CurrentUserProfile");
		}
	}
}
