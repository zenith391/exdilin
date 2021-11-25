using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200041D RID: 1053
public class UIPanelElementMessageButton : UIPanelElement
{
	// Token: 0x06002D8D RID: 11661 RVA: 0x00144CAB File Offset: 0x001430AB
	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
		this.messageID = parentPanel.itemId;
	}

	// Token: 0x06002D8E RID: 11662 RVA: 0x00144CC0 File Offset: 0x001430C0
	private void OnEnable()
	{
		Button component = base.GetComponent<Button>();
		if (component != null)
		{
			component.onClick.AddListener(new UnityAction(this.Trigger));
		}
		this.uiSound = base.GetComponent<UISound>();
	}

	// Token: 0x06002D8F RID: 11663 RVA: 0x00144D04 File Offset: 0x00143104
	private void OnDisable()
	{
		Button component = base.GetComponent<Button>();
		if (component != null)
		{
			component.onClick.RemoveListener(new UnityAction(this.Trigger));
		}
	}

	// Token: 0x06002D90 RID: 11664 RVA: 0x00144D3C File Offset: 0x0014313C
	public override void Fill(Dictionary<string, string> data)
	{
		this.buttonMessage = base.ReplacePlaceholderTextWithData(this.buttonMessageKey, data, this.buttonMessageKey);
		if (!string.IsNullOrEmpty(this.messageIDKey))
		{
			this.messageID = base.ReplacePlaceholderTextWithData(this.messageIDKey, data, this.messageID);
		}
	}

	// Token: 0x06002D91 RID: 11665 RVA: 0x00144D8C File Offset: 0x0014318C
	public void Trigger()
	{
		if (BWStandalone.Instance != null && this.parentPanel != null)
		{
			string senderID = (!string.IsNullOrEmpty(this.messageID)) ? this.messageID : this.parentPanel.itemId;
			bool success = BWStandalone.Instance.HandleMenuUIMessage(this.buttonMessage, senderID, this.parentPanel.dataSource.dataType, this.parentPanel.dataSource.dataSubtype);
			if (this.uiSound != null)
			{
				this.uiSound.PlayMessageButtonSound(success);
			}
		}
	}

	// Token: 0x0400261B RID: 9755
	public string buttonMessageKey;

	// Token: 0x0400261C RID: 9756
	public string messageIDKey;

	// Token: 0x0400261D RID: 9757
	private string buttonMessage;

	// Token: 0x0400261E RID: 9758
	private string messageID;

	// Token: 0x0400261F RID: 9759
	private UISound uiSound;
}
