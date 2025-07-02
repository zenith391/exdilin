using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UIPanelElementText : UIPanelElement
{
	public string textPattern;

	public string noDataText = string.Empty;

	private Text _text;

	private TextMeshProUGUI _tmpText;

	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
		_text = GetComponent<Text>();
		_tmpText = GetComponent<TextMeshProUGUI>();
	}

	public override void Clear()
	{
		base.Clear();
		if (!string.IsNullOrEmpty(textPattern))
		{
			if (_text == null)
			{
				_text = GetComponent<Text>();
			}
			if (_text != null)
			{
				_text.text = textPattern;
				_text.enabled = false;
			}
			if (_tmpText == null)
			{
				_tmpText = GetComponent<TextMeshProUGUI>();
			}
			if (_tmpText != null)
			{
				_tmpText.text = textPattern;
			}
		}
	}

	public override void Fill(Dictionary<string, string> data)
	{
		if (!string.IsNullOrEmpty(textPattern))
		{
			string text = ReplacePlaceholderTextWithData(textPattern, data, noDataText);
			if (_text != null)
			{
				_text.text = text;
				_text.enabled = true;
			}
			if (_tmpText != null)
			{
				_tmpText.text = text;
				_tmpText.enabled = true;
			}
		}
	}

	public override void Fill(string dataValue)
	{
		if (_text != null)
		{
			_text.text = dataValue;
			_text.enabled = true;
		}
		if (_tmpText != null)
		{
			_tmpText.text = dataValue;
			_tmpText.enabled = true;
		}
	}
}
