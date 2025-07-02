public class UISceneBuildMenu : UISceneBase
{
	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		base.SceneDidLoad(sceneInfo);
		if (uiController != null)
		{
			uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Build);
		}
	}

	public override void OnReturnToScene()
	{
		base.OnReturnToScene();
		uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Build);
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
