using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIPanelElementShowMessagePopup : UIPanelElement
{
	public string messageText;

	public bool useMenuTextEnum;

	public BWMenuTextEnum menuTextEnum;

	private Button button;

	private string popupMessageText;

	public void OnEnable()
	{
		popupMessageText = ((!useMenuTextEnum) ? messageText : MenuTextDefinitions.GetTextString(menuTextEnum));
		button = GetComponent<Button>();
		button.onClick.AddListener(ShowPopup);
	}

	public void OnDisable()
	{
		if (button != null)
		{
			button.onClick.RemoveListener(ShowPopup);
		}
	}

	public override void Clear()
	{
		base.Clear();
	}

	public override void Fill(Dictionary<string, string> data)
	{
		string text = ((!useMenuTextEnum) ? messageText : MenuTextDefinitions.GetTextString(menuTextEnum));
		popupMessageText = ReplacePlaceholderTextWithData(text, data, text);
	}

	private void ShowPopup()
	{
		if (!string.IsNullOrEmpty(popupMessageText))
		{
			BWStandalone.Overlays.ShowMessage(popupMessageText);
		}
	}
}
