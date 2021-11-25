using System;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200044E RID: 1102
public class UIPopupConfirmation : UIPopup
{
	// Token: 0x06002EF6 RID: 12022 RVA: 0x0014D3B8 File Offset: 0x0014B7B8
	public void SetTitleText(string titleTextStr)
	{
		this.titleLayout.enabled = true;
		this.titleText.text = titleTextStr;
	}

	// Token: 0x06002EF7 RID: 12023 RVA: 0x0014D3D2 File Offset: 0x0014B7D2
	public void Confirm()
	{
		base.Hide();
		if (this.yesAction != null)
		{
			this.yesAction();
		}
	}

	// Token: 0x04002752 RID: 10066
	public Text titleText;

	// Token: 0x04002753 RID: 10067
	public LayoutElement titleLayout;

	// Token: 0x04002754 RID: 10068
	public Text yesText;

	// Token: 0x04002755 RID: 10069
	public Text noText;

	// Token: 0x04002756 RID: 10070
	public UnityAction yesAction;
}
