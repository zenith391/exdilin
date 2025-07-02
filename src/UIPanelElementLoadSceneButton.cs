using System.Collections.Generic;
using UnityEngine.UI;

public class UIPanelElementLoadSceneButton : UIPanelElement
{
	public string scenePath;

	public string sceneTitle;

	public string sceneDataType;

	public string sceneDataSubtype;

	public string sceneUserImageUrl;

	private bool initComplete;

	private UISceneInfo sceneInfo;

	public void Awake()
	{
		sceneInfo = new UISceneInfo
		{
			path = scenePath,
			title = sceneTitle,
			userImageUrl = sceneUserImageUrl,
			dataType = sceneDataType,
			dataSubtype = sceneDataSubtype
		};
		Button component = GetComponent<Button>();
		if (component != null)
		{
			component.onClick.AddListener(delegate
			{
				TriggerSceneLoad();
			});
		}
	}

	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
	}

	public void TriggerSceneLoad()
	{
		if (!string.IsNullOrEmpty(sceneInfo.path) && MainUIController.active)
		{
			MainUIController.Instance.LoadUIScene(sceneInfo, back: false, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
		}
	}

	public override void Fill(Dictionary<string, string> data)
	{
		sceneInfo.title = ReplacePlaceholderTextWithData(sceneTitle, data);
		sceneInfo.dataType = ReplacePlaceholderTextWithData(sceneDataType, data);
		sceneInfo.dataSubtype = ReplacePlaceholderTextWithData(sceneDataSubtype, data);
		sceneInfo.userImageUrl = ReplacePlaceholderTextWithData(sceneUserImageUrl, data);
	}
}
