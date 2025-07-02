using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UISceneTitleText : UISceneInfoDisplay
{
	private Text text;

	public void Awake()
	{
		text = GetComponent<Text>();
		text.enabled = false;
	}

	public override void Setup(UISceneInfo sceneInfo)
	{
		text.text = sceneInfo.title;
		text.enabled = true;
	}
}
