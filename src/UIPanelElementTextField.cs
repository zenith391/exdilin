using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class UIPanelElementTextField : UIPanelElement
{
	public string dataKey;

	public bool limitToTileWidth;

	private Text _text;

	private InputField _inputField;

	private string lastTextEntry;

	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
	}

	public void OnEnable()
	{
		_inputField = GetComponent<InputField>();
		_text = _inputField.textComponent;
		_inputField.onValueChanged.RemoveAllListeners();
		_inputField.onEndEdit.RemoveAllListeners();
		_inputField.onValueChanged.AddListener(TextChanged);
		_inputField.onEndEdit.AddListener(DidEditText);
		_inputField.onValidateInput = null;
		InputField inputField = _inputField;
		inputField.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(inputField.onValidateInput, new InputField.OnValidateInput(HandleOnValidateInput));
	}

	public void OnDisable()
	{
		_inputField.onValidateInput = null;
	}

	public override void Clear()
	{
		base.Clear();
		if (_inputField == null)
		{
			_inputField = GetComponent<InputField>();
			_text = _inputField.textComponent;
		}
		_inputField.text = string.Empty;
		_text.text = string.Empty;
		_inputField.onValidateInput = null;
	}

	public override void Fill(Dictionary<string, string> data)
	{
		string value = string.Empty;
		if (data.TryGetValue(dataKey, out value))
		{
			_text.text = value;
			_inputField.text = value;
		}
		_inputField.onValidateInput = null;
		InputField inputField = _inputField;
		inputField.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(inputField.onValidateInput, new InputField.OnValidateInput(HandleOnValidateInput));
	}

	private char HandleOnValidateInput(string text, int charIndex, char addedChar)
	{
		if (limitToTileWidth && !ModelUtils.IsValidShortTitle(text))
		{
			return '\0';
		}
		return addedChar;
	}

	private void TextChanged(string textStr)
	{
		textStr = Util.FixNonAscii(textStr);
		if (!Input.GetKey(KeyCode.Escape))
		{
			lastTextEntry = textStr;
		}
	}

	private void DidEditText(string inputStr)
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			inputStr = lastTextEntry;
		}
		inputStr = Util.FixNonAscii(inputStr);
		if (parentPanel != null)
		{
			parentPanel.ElementEditedText(dataKey, inputStr);
		}
	}
}
