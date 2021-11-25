using System;
using UnityEngine;

// Token: 0x02000449 RID: 1097
public class UISceneSocial : UISceneBase
{
	// Token: 0x06002EE2 RID: 12002 RVA: 0x0014D061 File Offset: 0x0014B461
	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		base.SceneDidLoad(sceneInfo);
		if (this.uiController != null)
		{
			this.uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Social);
		}
	}

	// Token: 0x06002EE3 RID: 12003 RVA: 0x0014D08C File Offset: 0x0014B48C
	public override void OnReturnToScene()
	{
		base.OnReturnToScene();
		this.uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Social);
	}

	// Token: 0x06002EE4 RID: 12004 RVA: 0x0014D0A8 File Offset: 0x0014B4A8
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
