using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIPanelElementToggle : UIPanelElement
{
	public string dataKey;

	public bool negate;

	public bool sendToggleMessages = true;

	public bool informPanel;

	public string toggleOnMessage;

	public string toggleOffMessage;

	private Toggle toggle;

	private bool suppressToggleActions;

	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
		toggle = GetComponent<Toggle>();
		toggle.onValueChanged.AddListener(Toggled);
	}

	public override void Fill(Dictionary<string, string> data)
	{
		base.Fill(data);
		foreach (KeyValuePair<string, string> datum in data)
		{
			if (datum.Key == dataKey)
			{
				suppressToggleActions = true;
				bool flag = datum.Value.ToLowerInvariant() == "true";
				if (negate)
				{
					flag = !flag;
				}
				toggle.isOn = flag;
				suppressToggleActions = false;
			}
		}
	}

	private void Toggled(bool val)
	{
		if (!suppressToggleActions)
		{
			if (informPanel)
			{
				parentPanel.ElementEditedBool(dataKey, val);
			}
			if (sendToggleMessages)
			{
				string message = ((!val) ? toggleOffMessage : toggleOnMessage);
				parentPanel.ButtonPressed(message);
			}
		}
	}
}
