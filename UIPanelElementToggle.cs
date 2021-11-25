using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000425 RID: 1061
[RequireComponent(typeof(Toggle))]
public class UIPanelElementToggle : UIPanelElement
{
	// Token: 0x06002DBC RID: 11708 RVA: 0x0014579B File Offset: 0x00143B9B
	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
		this.toggle = base.GetComponent<Toggle>();
		this.toggle.onValueChanged.AddListener(new UnityAction<bool>(this.Toggled));
	}

	// Token: 0x06002DBD RID: 11709 RVA: 0x001457CC File Offset: 0x00143BCC
	public override void Fill(Dictionary<string, string> data)
	{
		base.Fill(data);
		foreach (KeyValuePair<string, string> keyValuePair in data)
		{
			if (keyValuePair.Key == this.dataKey)
			{
				this.suppressToggleActions = true;
				bool flag = keyValuePair.Value.ToLowerInvariant() == "true";
				if (this.negate)
				{
					flag = !flag;
				}
				this.toggle.isOn = flag;
				this.suppressToggleActions = false;
			}
		}
	}

	// Token: 0x06002DBE RID: 11710 RVA: 0x0014587C File Offset: 0x00143C7C
	private void Toggled(bool val)
	{
		if (!this.suppressToggleActions)
		{
			if (this.informPanel)
			{
				this.parentPanel.ElementEditedBool(this.dataKey, val);
			}
			if (this.sendToggleMessages)
			{
				string message = (!val) ? this.toggleOffMessage : this.toggleOnMessage;
				this.parentPanel.ButtonPressed(message);
			}
		}
	}

	// Token: 0x0400263D RID: 9789
	public string dataKey;

	// Token: 0x0400263E RID: 9790
	public bool negate;

	// Token: 0x0400263F RID: 9791
	public bool sendToggleMessages = true;

	// Token: 0x04002640 RID: 9792
	public bool informPanel;

	// Token: 0x04002641 RID: 9793
	public string toggleOnMessage;

	// Token: 0x04002642 RID: 9794
	public string toggleOffMessage;

	// Token: 0x04002643 RID: 9795
	private Toggle toggle;

	// Token: 0x04002644 RID: 9796
	private bool suppressToggleActions;
}
