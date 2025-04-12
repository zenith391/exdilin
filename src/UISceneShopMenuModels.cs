using System;
using UnityEngine;

// Token: 0x02000448 RID: 1096
public class UISceneShopMenuModels : UISceneBase
{
	// Token: 0x06002EDD RID: 11997 RVA: 0x0014CF00 File Offset: 0x0014B300
	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		base.SceneDidLoad(sceneInfo);
		if (this.uiController != null)
		{
			this.uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Shop);
		}
		if (this.tabGroup != null)
		{
			this.tabGroup.SelectTabWithTag("Models");
		}
	}

	// Token: 0x06002EDE RID: 11998 RVA: 0x0014CF57 File Offset: 0x0014B357
	public override void OnReturnToScene()
	{
		base.OnReturnToScene();
		this.uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Shop);
		if (this.tabGroup != null)
		{
			this.tabGroup.SelectTabWithTag("Models");
		}
	}

	// Token: 0x06002EDF RID: 11999 RVA: 0x0014CF91 File Offset: 0x0014B391
	public override Vector3 GetShoppingCartPosition()
	{
		if (this.shoppingCartIcon != null)
		{
			return this.shoppingCartIcon.position;
		}
		return base.GetShoppingCartPosition();
	}

	// Token: 0x06002EE0 RID: 12000 RVA: 0x0014CFB8 File Offset: 0x0014B3B8
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

	// Token: 0x0400273F RID: 10047
	public RectTransform shoppingCartIcon;

	// Token: 0x04002740 RID: 10048
	public UITabGroup tabGroup;
}
