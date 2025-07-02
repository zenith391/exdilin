using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UIPanelElementMenuText : UIPanelElement
{
	public BWMenuTextEnum textPattern;

	public BWMenuTextEnum noDataText;

	private Text _text;

	public override void Init(UIPanelContents parentPanel)
	{
		_text = GetComponent<Text>();
	}

	public override void Clear()
	{
		base.Clear();
		_text.text = MenuTextDefinitions.GetTextString(textPattern);
		_text.enabled = false;
	}

	public override void Fill(Dictionary<string, string> data)
	{
		string textString = MenuTextDefinitions.GetTextString(textPattern);
		string textString2 = MenuTextDefinitions.GetTextString(noDataText);
		string text = ReplacePlaceholderTextWithData(textString, data, textString2);
		_text.text = text;
		_text.enabled = true;
	}
}
