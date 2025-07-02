using UnityEngine;
using UnityEngine.UI;

public class UIOptionsScreen : MonoBehaviour
{
	public Text titleText;

	public Text usernameText;

	public Text descriptionText;

	public GameObject editorSaveMenuButton;

	public GameObject shareButtonParent;

	public void Init(string titleStr, string usernameStr, string descriptionStr)
	{
		titleText.text = titleStr;
		usernameText.text = usernameStr;
		descriptionText.text = descriptionStr;
		bool active = false;
		editorSaveMenuButton.SetActive(active);
		Button[] componentsInChildren = shareButtonParent.GetComponentsInChildren<Button>();
		for (int i = 0; i != componentsInChildren.Length; i++)
		{
			componentsInChildren[i].onClick.AddListener(ButtonPressed_ShowSharePopup);
		}
	}

	public void ButtonPressed_Resume()
	{
		Blocksworld.bw.HideOptionsScreen();
	}

	public void ButtonPressed_Exit()
	{
		Blocksworld.bw.HideOptionsScreen();
		Blocksworld.bw.ButtonExitWorldTapped();
	}

	public void ButtonPressed_EditorSaveMenu()
	{
	}

	public void ButtonPressed_ShowSharePopup()
	{
		RectTransform component = shareButtonParent.GetComponent<RectTransform>();
		Canvas component2 = GetComponent<Canvas>();
		Rect shareRect = RectTransformUtility.PixelAdjustRect(component, component2);
		float x = component.anchoredPosition.x;
		float num = 768f - component.anchoredPosition.y - shareRect.height;
		shareRect.center = new Vector2(x, 0f - num);
		WorldSession.platformDelegate.ShowSharingPopup(shareRect);
	}
}
