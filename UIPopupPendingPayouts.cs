using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000453 RID: 1107
public class UIPopupPendingPayouts : UIPopup
{
	// Token: 0x06002F0C RID: 12044 RVA: 0x0014DB5A File Offset: 0x0014BF5A
	private void OnEnable()
	{
		this.collectButton.onClick.AddListener(new UnityAction(this.Collect));
		BWPendingPayouts.AddPendingPayoutsCollectedListener(new PendingPayoutsCollectedEventListener(this.OnCollectComplete));
	}

	// Token: 0x06002F0D RID: 12045 RVA: 0x0014DB89 File Offset: 0x0014BF89
	private void OnDisable()
	{
		this.collectButton.onClick.RemoveListener(new UnityAction(this.Collect));
		BWPendingPayouts.RemovePendingPayoutsCollectedListener(new PendingPayoutsCollectedEventListener(this.OnCollectComplete));
	}

	// Token: 0x06002F0E RID: 12046 RVA: 0x0014DBB8 File Offset: 0x0014BFB8
	private void Collect()
	{
		BWPendingPayouts.Collect();
		this.collectButton.interactable = false;
	}

	// Token: 0x06002F0F RID: 12047 RVA: 0x0014DBCB File Offset: 0x0014BFCB
	private void OnCollectComplete(bool success)
	{
		if (success)
		{
			BWStandalone.Overlays.DoCoinAwardAnimation(this.coinAnimSource.position, new UnityAction(base.Hide));
		}
		else
		{
			base.Hide();
		}
	}

	// Token: 0x04002776 RID: 10102
	public Button collectButton;

	// Token: 0x04002777 RID: 10103
	public RectTransform coinAnimSource;
}
