using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200041C RID: 1052
[RequireComponent(typeof(Text))]
public class UIPanelElementMenuText : UIPanelElement
{
	// Token: 0x06002D89 RID: 11657 RVA: 0x00144C21 File Offset: 0x00143021
	public override void Init(UIPanelContents parentPanel)
	{
		this._text = base.GetComponent<Text>();
	}

	// Token: 0x06002D8A RID: 11658 RVA: 0x00144C2F File Offset: 0x0014302F
	public override void Clear()
	{
		base.Clear();
		this._text.text = MenuTextDefinitions.GetTextString(this.textPattern);
		this._text.enabled = false;
	}

	// Token: 0x06002D8B RID: 11659 RVA: 0x00144C5C File Offset: 0x0014305C
	public override void Fill(Dictionary<string, string> data)
	{
		string textString = MenuTextDefinitions.GetTextString(this.textPattern);
		string textString2 = MenuTextDefinitions.GetTextString(this.noDataText);
		string text = base.ReplacePlaceholderTextWithData(textString, data, textString2);
		this._text.text = text;
		this._text.enabled = true;
	}

	// Token: 0x04002618 RID: 9752
	public BWMenuTextEnum textPattern;

	// Token: 0x04002619 RID: 9753
	public BWMenuTextEnum noDataText;

	// Token: 0x0400261A RID: 9754
	private Text _text;
}
