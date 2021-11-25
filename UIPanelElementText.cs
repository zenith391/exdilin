using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

// Token: 0x02000423 RID: 1059
public class UIPanelElementText : UIPanelElement
{
	// Token: 0x06002DAE RID: 11694 RVA: 0x00145389 File Offset: 0x00143789
	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
		this._text = base.GetComponent<Text>();
		this._tmpText = base.GetComponent<TextMeshProUGUI>();
	}

	// Token: 0x06002DAF RID: 11695 RVA: 0x001453AC File Offset: 0x001437AC
	public override void Clear()
	{
		base.Clear();
		if (string.IsNullOrEmpty(this.textPattern))
		{
			return;
		}
		if (this._text == null)
		{
			this._text = base.GetComponent<Text>();
		}
		if (this._text != null)
		{
			this._text.text = this.textPattern;
			this._text.enabled = false;
		}
		if (this._tmpText == null)
		{
			this._tmpText = base.GetComponent<TextMeshProUGUI>();
		}
		if (this._tmpText != null)
		{
			this._tmpText.text = this.textPattern;
		}
	}

	// Token: 0x06002DB0 RID: 11696 RVA: 0x0014545C File Offset: 0x0014385C
	public override void Fill(Dictionary<string, string> data)
	{
		if (string.IsNullOrEmpty(this.textPattern))
		{
			return;
		}
		string text = base.ReplacePlaceholderTextWithData(this.textPattern, data, this.noDataText);
		if (this._text != null)
		{
			this._text.text = text;
			this._text.enabled = true;
		}
		if (this._tmpText != null)
		{
			this._tmpText.text = text;
			this._tmpText.enabled = true;
		}
	}

	// Token: 0x06002DB1 RID: 11697 RVA: 0x001454E0 File Offset: 0x001438E0
	public override void Fill(string dataValue)
	{
		if (this._text != null)
		{
			this._text.text = dataValue;
			this._text.enabled = true;
		}
		if (this._tmpText != null)
		{
			this._tmpText.text = dataValue;
			this._tmpText.enabled = true;
		}
	}

	// Token: 0x04002634 RID: 9780
	public string textPattern;

	// Token: 0x04002635 RID: 9781
	public string noDataText = string.Empty;

	// Token: 0x04002636 RID: 9782
	private Text _text;

	// Token: 0x04002637 RID: 9783
	private TextMeshProUGUI _tmpText;
}
