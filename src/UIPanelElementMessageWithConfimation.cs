using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPanelElementMessageWithConfimation : UIPanelElement
{
	public string buttonMessage;

	public bool showConfirmation = true;

	public BWMenuTextEnum confirmationMessagePattern;

	public BWMenuTextEnum confirmationMessageNoData;

	private string confirmationMessage;

	private Button button;

	private void OnEnable()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(Spawn);
	}

	private void OnDisable()
	{
		button.onClick.RemoveListener(Spawn);
	}

	public override void Fill(Dictionary<string, string> data)
	{
		confirmationMessage = ReplacePlaceholderMenuTextWithData(confirmationMessagePattern, data, confirmationMessageNoData);
	}

	private void Spawn()
	{
		if (!showConfirmation)
		{
			parentPanel.ButtonPressed(buttonMessage);
			return;
		}
		UnityAction yesAction = delegate
		{
			parentPanel.ButtonPressed(buttonMessage);
		};
		BWStandalone.Overlays.ShowConfirmationDialog(confirmationMessage, yesAction);
	}
}
