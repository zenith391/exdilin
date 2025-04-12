using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x02000422 RID: 1058
[RequireComponent(typeof(TMP_InputField))]
public class UIPanelElementTMPTextField : UIPanelElement
{
	// Token: 0x06002DA5 RID: 11685 RVA: 0x0014514C File Offset: 0x0014354C
	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
	}

	// Token: 0x06002DA6 RID: 11686 RVA: 0x00145158 File Offset: 0x00143558
	public void OnEnable()
	{
		this._inputField = base.GetComponent<TMP_InputField>();
		this._text = this._inputField.textComponent;
		this._inputField.onValueChanged.RemoveAllListeners();
		this._inputField.onEndEdit.RemoveAllListeners();
		this._inputField.onValueChanged.AddListener(new UnityAction<string>(this.TextChanged));
		this._inputField.onEndEdit.AddListener(new UnityAction<string>(this.DidEditText));
		this._inputField.onValidateInput = null;
		TMP_InputField inputField = this._inputField;
		inputField.onValidateInput = (TMP_InputField.OnValidateInput)Delegate.Combine(inputField.onValidateInput, new TMP_InputField.OnValidateInput(this.HandleOnValidateInput));
	}

	// Token: 0x06002DA7 RID: 11687 RVA: 0x0014520D File Offset: 0x0014360D
	public void OnDisable()
	{
		this._inputField.onValidateInput = null;
	}

	// Token: 0x06002DA8 RID: 11688 RVA: 0x0014521C File Offset: 0x0014361C
	public override void Clear()
	{
		base.Clear();
		if (this._inputField == null)
		{
			this._inputField = base.GetComponent<TMP_InputField>();
			this._text = this._inputField.textComponent;
		}
		this._inputField.text = string.Empty;
		this._text.text = string.Empty;
		this._inputField.onValidateInput = null;
	}

	// Token: 0x06002DA9 RID: 11689 RVA: 0x0014528C File Offset: 0x0014368C
	public override void Fill(Dictionary<string, string> data)
	{
		string empty = string.Empty;
		if (data.TryGetValue(this.dataKey, out empty))
		{
			this._text.text = empty;
			this._inputField.text = empty;
		}
		this._inputField.onValidateInput = null;
		TMP_InputField inputField = this._inputField;
		inputField.onValidateInput = (TMP_InputField.OnValidateInput)Delegate.Combine(inputField.onValidateInput, new TMP_InputField.OnValidateInput(this.HandleOnValidateInput));
	}

	// Token: 0x06002DAA RID: 11690 RVA: 0x00145300 File Offset: 0x00143700
	private char HandleOnValidateInput(string text, int charIndex, char addedChar)
	{
		if (this.limitToTileWidth && !ModelUtils.IsValidShortTitle(text))
		{
			return '\0';
		}
		return addedChar;
	}

	// Token: 0x06002DAB RID: 11691 RVA: 0x00145328 File Offset: 0x00143728
	private void TextChanged(string textStr)
	{
		if (!Input.GetKey(KeyCode.Escape))
		{
			this.lastTextEntry = textStr;
		}
	}

	// Token: 0x06002DAC RID: 11692 RVA: 0x0014533D File Offset: 0x0014373D
	private void DidEditText(string inputStr)
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			inputStr = this.lastTextEntry;
		}
		if (this.parentPanel != null)
		{
			this.parentPanel.ElementEditedText(this.dataKey, inputStr);
		}
	}

	// Token: 0x0400262F RID: 9775
	public string dataKey;

	// Token: 0x04002630 RID: 9776
	public bool limitToTileWidth;

	// Token: 0x04002631 RID: 9777
	private TMP_Text _text;

	// Token: 0x04002632 RID: 9778
	private TMP_InputField _inputField;

	// Token: 0x04002633 RID: 9779
	private string lastTextEntry;
}
