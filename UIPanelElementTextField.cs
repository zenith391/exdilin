using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000424 RID: 1060
[RequireComponent(typeof(InputField))]
public class UIPanelElementTextField : UIPanelElement
{
	// Token: 0x06002DB3 RID: 11699 RVA: 0x00145547 File Offset: 0x00143947
	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
	}

	// Token: 0x06002DB4 RID: 11700 RVA: 0x00145550 File Offset: 0x00143950
	public void OnEnable()
	{
		this._inputField = base.GetComponent<InputField>();
		this._text = this._inputField.textComponent;
		this._inputField.onValueChanged.RemoveAllListeners();
		this._inputField.onEndEdit.RemoveAllListeners();
		this._inputField.onValueChanged.AddListener(new UnityAction<string>(this.TextChanged));
		this._inputField.onEndEdit.AddListener(new UnityAction<string>(this.DidEditText));
		this._inputField.onValidateInput = null;
		InputField inputField = this._inputField;
		inputField.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(inputField.onValidateInput, new InputField.OnValidateInput(this.HandleOnValidateInput));
	}

	// Token: 0x06002DB5 RID: 11701 RVA: 0x00145605 File Offset: 0x00143A05
	public void OnDisable()
	{
		this._inputField.onValidateInput = null;
	}

	// Token: 0x06002DB6 RID: 11702 RVA: 0x00145614 File Offset: 0x00143A14
	public override void Clear()
	{
		base.Clear();
		if (this._inputField == null)
		{
			this._inputField = base.GetComponent<InputField>();
			this._text = this._inputField.textComponent;
		}
		this._inputField.text = string.Empty;
		this._text.text = string.Empty;
		this._inputField.onValidateInput = null;
	}

	// Token: 0x06002DB7 RID: 11703 RVA: 0x00145684 File Offset: 0x00143A84
	public override void Fill(Dictionary<string, string> data)
	{
		string empty = string.Empty;
		if (data.TryGetValue(this.dataKey, out empty))
		{
			this._text.text = empty;
			this._inputField.text = empty;
		}
		this._inputField.onValidateInput = null;
		InputField inputField = this._inputField;
		inputField.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(inputField.onValidateInput, new InputField.OnValidateInput(this.HandleOnValidateInput));
	}

	// Token: 0x06002DB8 RID: 11704 RVA: 0x001456F8 File Offset: 0x00143AF8
	private char HandleOnValidateInput(string text, int charIndex, char addedChar)
	{
		if (this.limitToTileWidth && !ModelUtils.IsValidShortTitle(text))
		{
			return '\0';
		}
		return addedChar;
	}

	// Token: 0x06002DB9 RID: 11705 RVA: 0x00145720 File Offset: 0x00143B20
	private void TextChanged(string textStr)
	{
		textStr = Util.FixNonAscii(textStr);
		if (!Input.GetKey(KeyCode.Escape))
		{
			this.lastTextEntry = textStr;
		}
	}

	// Token: 0x06002DBA RID: 11706 RVA: 0x00145740 File Offset: 0x00143B40
	private void DidEditText(string inputStr)
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			inputStr = this.lastTextEntry;
		}
		inputStr = Util.FixNonAscii(inputStr);
		if (this.parentPanel != null)
		{
			this.parentPanel.ElementEditedText(this.dataKey, inputStr);
		}
	}

	// Token: 0x04002638 RID: 9784
	public string dataKey;

	// Token: 0x04002639 RID: 9785
	public bool limitToTileWidth;

	// Token: 0x0400263A RID: 9786
	private Text _text;

	// Token: 0x0400263B RID: 9787
	private InputField _inputField;

	// Token: 0x0400263C RID: 9788
	private string lastTextEntry;
}
