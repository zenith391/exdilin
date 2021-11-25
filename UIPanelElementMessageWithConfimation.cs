using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200041E RID: 1054
public class UIPanelElementMessageWithConfimation : UIPanelElement
{
	// Token: 0x06002D93 RID: 11667 RVA: 0x00144E3F File Offset: 0x0014323F
	private void OnEnable()
	{
		this.button = base.GetComponent<Button>();
		this.button.onClick.AddListener(new UnityAction(this.Spawn));
	}

	// Token: 0x06002D94 RID: 11668 RVA: 0x00144E69 File Offset: 0x00143269
	private void OnDisable()
	{
		this.button.onClick.RemoveListener(new UnityAction(this.Spawn));
	}

	// Token: 0x06002D95 RID: 11669 RVA: 0x00144E87 File Offset: 0x00143287
	public override void Fill(Dictionary<string, string> data)
	{
		this.confirmationMessage = base.ReplacePlaceholderMenuTextWithData(this.confirmationMessagePattern, data, this.confirmationMessageNoData);
	}

	// Token: 0x06002D96 RID: 11670 RVA: 0x00144EA4 File Offset: 0x001432A4
	private void Spawn()
	{
		if (!this.showConfirmation)
		{
			this.parentPanel.ButtonPressed(this.buttonMessage);
			return;
		}
		UnityAction yesAction = delegate()
		{
			this.parentPanel.ButtonPressed(this.buttonMessage);
		};
		BWStandalone.Overlays.ShowConfirmationDialog(this.confirmationMessage, yesAction);
	}

	// Token: 0x04002620 RID: 9760
	public string buttonMessage;

	// Token: 0x04002621 RID: 9761
	public bool showConfirmation = true;

	// Token: 0x04002622 RID: 9762
	public BWMenuTextEnum confirmationMessagePattern;

	// Token: 0x04002623 RID: 9763
	public BWMenuTextEnum confirmationMessageNoData;

	// Token: 0x04002624 RID: 9764
	private string confirmationMessage;

	// Token: 0x04002625 RID: 9765
	private Button button;
}
