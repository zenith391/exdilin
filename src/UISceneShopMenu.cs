using UnityEngine;

public class UISceneShopMenu : UISceneBase
{
	public UISceneElementGroup shopSections;

	public RectTransform shoppingCartIcon;

	public UITabGroup tabGroup;

	public string defaultTab;

	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		if (!string.IsNullOrEmpty(sceneInfo.dataSubtype))
		{
			string categoryIDForTitle = BWBlockShopData.GetCategoryIDForTitle(sceneInfo.dataSubtype);
			string childDataType = "BlockShop_" + categoryIDForTitle;
			shopSections.childDataType = childDataType;
			tabGroup.SelectTabWithTag(sceneInfo.dataSubtype);
		}
		else
		{
			tabGroup.SelectTabWithTag(defaultTab);
		}
		if (scrollRect != null)
		{
			scrollRect.verticalNormalizedPosition = 0f;
		}
		base.SceneDidLoad(sceneInfo);
		if (uiController != null)
		{
			uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Shop);
		}
	}

	public override void OnReturnToScene()
	{
		base.OnReturnToScene();
		uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Shop);
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
