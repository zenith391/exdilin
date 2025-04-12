using System;

// Token: 0x02000441 RID: 1089
public class UISceneBrowser : UISceneBase
{
	// Token: 0x06002EB1 RID: 11953 RVA: 0x0014C3FD File Offset: 0x0014A7FD
	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		base.SceneDidLoad(sceneInfo);
		if (this.uiController != null)
		{
			this.uiController.menuBar.SelectMenuButton(this.menuBarButton);
		}
	}

	// Token: 0x06002EB2 RID: 11954 RVA: 0x0014C42D File Offset: 0x0014A82D
	public override void OnReturnToScene()
	{
		base.OnReturnToScene();
		if (this.uiController != null)
		{
			this.uiController.menuBar.SelectMenuButton(this.menuBarButton);
		}
	}

	// Token: 0x04002724 RID: 10020
	public MenuBarButtonEnum menuBarButton;
}
