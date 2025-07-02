using System.Collections.Generic;
using UnityEngine.UI;

public class UIPanelElementMessageButton : UIPanelElement
{
	public string buttonMessageKey;

	public string messageIDKey;

	private string buttonMessage;

	private string messageID;

	private UISound uiSound;

	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
		messageID = parentPanel.itemId;
	}

	private void OnEnable()
	{
		Button component = GetComponent<Button>();
		if (component != null)
		{
			component.onClick.AddListener(Trigger);
		}
		uiSound = GetComponent<UISound>();
	}

	private void OnDisable()
	{
		Button component = GetComponent<Button>();
		if (component != null)
		{
			component.onClick.RemoveListener(Trigger);
		}
	}

	public override void Fill(Dictionary<string, string> data)
	{
		buttonMessage = ReplacePlaceholderTextWithData(buttonMessageKey, data, buttonMessageKey);
		if (!string.IsNullOrEmpty(messageIDKey))
		{
			messageID = ReplacePlaceholderTextWithData(messageIDKey, data, messageID);
		}
	}

	public void Trigger()
	{
		if (BWStandalone.Instance != null && parentPanel != null)
		{
			string senderID = ((!string.IsNullOrEmpty(messageID)) ? messageID : parentPanel.itemId);
			bool success = BWStandalone.Instance.HandleMenuUIMessage(buttonMessage, senderID, parentPanel.dataSource.dataType, parentPanel.dataSource.dataSubtype);
			if (uiSound != null)
			{
				uiSound.PlayMessageButtonSound(success);
			}
		}
	}
}
