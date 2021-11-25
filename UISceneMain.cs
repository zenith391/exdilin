using System;

// Token: 0x02000444 RID: 1092
public class UISceneMain : UISceneBase
{
	// Token: 0x06002EBC RID: 11964 RVA: 0x0014C641 File Offset: 0x0014AA41
	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		base.SceneDidLoad(sceneInfo);
		if (this.uiController != null)
		{
			this.uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Play);
		}
	}

	// Token: 0x06002EBD RID: 11965 RVA: 0x0014C66C File Offset: 0x0014AA6C
	public override void OnReturnToScene()
	{
		base.OnReturnToScene();
		this.uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Play);
	}

	// Token: 0x06002EBE RID: 11966 RVA: 0x0014C688 File Offset: 0x0014AA88
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
