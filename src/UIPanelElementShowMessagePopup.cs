using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000421 RID: 1057
[RequireComponent(typeof(Button))]
public class UIPanelElementShowMessagePopup : UIPanelElement
{
	// Token: 0x06002D9F RID: 11679 RVA: 0x00145050 File Offset: 0x00143450
	public void OnEnable()
	{
		this.popupMessageText = ((!this.useMenuTextEnum) ? this.messageText : MenuTextDefinitions.GetTextString(this.menuTextEnum));
		this.button = base.GetComponent<Button>();
		this.button.onClick.AddListener(new UnityAction(this.ShowPopup));
	}

	// Token: 0x06002DA0 RID: 11680 RVA: 0x001450AC File Offset: 0x001434AC
	public void OnDisable()
	{
		if (this.button != null)
		{
			this.button.onClick.RemoveListener(new UnityAction(this.ShowPopup));
		}
	}

	// Token: 0x06002DA1 RID: 11681 RVA: 0x001450DB File Offset: 0x001434DB
	public override void Clear()
	{
		base.Clear();
	}

	// Token: 0x06002DA2 RID: 11682 RVA: 0x001450E4 File Offset: 0x001434E4
	public override void Fill(Dictionary<string, string> data)
	{
		string text = (!this.useMenuTextEnum) ? this.messageText : MenuTextDefinitions.GetTextString(this.menuTextEnum);
		this.popupMessageText = base.ReplacePlaceholderTextWithData(text, data, text);
	}

	// Token: 0x06002DA3 RID: 11683 RVA: 0x00145122 File Offset: 0x00143522
	private void ShowPopup()
	{
		if (!string.IsNullOrEmpty(this.popupMessageText))
		{
			BWStandalone.Overlays.ShowMessage(this.popupMessageText);
		}
	}

	// Token: 0x0400262A RID: 9770
	public string messageText;

	// Token: 0x0400262B RID: 9771
	public bool useMenuTextEnum;

	// Token: 0x0400262C RID: 9772
	public BWMenuTextEnum menuTextEnum;

	// Token: 0x0400262D RID: 9773
	private Button button;

	// Token: 0x0400262E RID: 9774
	private string popupMessageText;
}
