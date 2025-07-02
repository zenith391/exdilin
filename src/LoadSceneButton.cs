using UnityEngine.UI;

public class LoadSceneButton : Button
{
	public string scenePath;

	public string sceneTitle;

	public string sceneDataType;

	public string sceneDataSubtype;

	protected override void OnEnable()
	{
		base.onClick.RemoveListener(LoadScene);
		base.onClick.AddListener(LoadScene);
	}

	protected override void OnDisable()
	{
		base.onClick.RemoveAllListeners();
	}

	private void LoadScene()
	{
		if (MainUIController.active)
		{
			UISceneInfo sceneInfo = new UISceneInfo
			{
				path = scenePath,
				title = sceneTitle,
				dataType = sceneDataType,
				dataSubtype = sceneDataSubtype
			};
			MainUIController.Instance.LoadUIScene(sceneInfo, back: false, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
		}
	}
}
