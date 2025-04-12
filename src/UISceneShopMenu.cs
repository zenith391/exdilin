using System;
using UnityEngine;

// Token: 0x02000447 RID: 1095
public class UISceneShopMenu : UISceneBase
{
	// Token: 0x06002ED8 RID: 11992 RVA: 0x0014CD64 File Offset: 0x0014B164
	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		if (!string.IsNullOrEmpty(sceneInfo.dataSubtype))
		{
			string categoryIDForTitle = BWBlockShopData.GetCategoryIDForTitle(sceneInfo.dataSubtype);
			string childDataType = "BlockShop_" + categoryIDForTitle;
			this.shopSections.childDataType = childDataType;
			this.tabGroup.SelectTabWithTag(sceneInfo.dataSubtype);
		}
		else
		{
			this.tabGroup.SelectTabWithTag(this.defaultTab);
		}
		if (this.scrollRect != null)
		{
			this.scrollRect.verticalNormalizedPosition = 0f;
		}
		base.SceneDidLoad(sceneInfo);
		if (this.uiController != null)
		{
			this.uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Shop);
		}
	}

	// Token: 0x06002ED9 RID: 11993 RVA: 0x0014CE16 File Offset: 0x0014B216
	public override void OnReturnToScene()
	{
		base.OnReturnToScene();
		this.uiController.menuBar.SelectMenuButton(MenuBarButtonEnum.Shop);
	}

	// Token: 0x06002EDA RID: 11994 RVA: 0x0014CE2F File Offset: 0x0014B22F
	public override Vector3 GetShoppingCartPosition()
	{
		if (this.shoppingCartIcon != null)
		{
			return this.shoppingCartIcon.position;
		}
		return base.GetShoppingCartPosition();
	}

	// Token: 0x06002EDB RID: 11995 RVA: 0x0014CE54 File Offset: 0x0014B254
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

	// Token: 0x0400273B RID: 10043
	public UISceneElementGroup shopSections;

	// Token: 0x0400273C RID: 10044
	public RectTransform shoppingCartIcon;

	// Token: 0x0400273D RID: 10045
	public UITabGroup tabGroup;

	// Token: 0x0400273E RID: 10046
	public string defaultTab;
}
