using System;

// Token: 0x02000442 RID: 1090
public class UISceneBuildMenu : UISceneBase
{
	// Token: 0x06002EB4 RID: 11956 RVA: 0x0014C464 File Offset: 0x0014A864
	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		base.SceneDidLoad(sceneInfo);
		if (this.uiController != null)
		{
			this.uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Build);
		}
	}

	// Token: 0x06002EB5 RID: 11957 RVA: 0x0014C48F File Offset: 0x0014A88F
	public override void OnReturnToScene()
	{
		base.OnReturnToScene();
		this.uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Build);
	}

	// Token: 0x06002EB6 RID: 11958 RVA: 0x0014C4A8 File Offset: 0x0014A8A8
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
