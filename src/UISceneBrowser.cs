public class UISceneBrowser : UISceneBase
{
	public MenuBarButtonEnum menuBarButton;

	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		base.SceneDidLoad(sceneInfo);
		if (uiController != null)
		{
			uiController.menuBar.SelectMenuButton(menuBarButton);
		}
	}

	public override void OnReturnToScene()
	{
		base.OnReturnToScene();
		if (uiController != null)
		{
			uiController.menuBar.SelectMenuButton(menuBarButton);
		}
	}
}
