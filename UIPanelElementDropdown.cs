using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000417 RID: 1047
[RequireComponent(typeof(Dropdown))]
public class UIPanelElementDropdown : UIPanelElement
{
	// Token: 0x06002D78 RID: 11640 RVA: 0x0014465E File Offset: 0x00142A5E
	public virtual void OnEnable()
	{
		this.dropdown = base.GetComponent<Dropdown>();
		this.dropdown.onValueChanged.AddListener(new UnityAction<int>(this.DropdownSelect));
	}

	// Token: 0x06002D79 RID: 11641 RVA: 0x00144688 File Offset: 0x00142A88
	public override void Fill(Dictionary<string, string> data)
	{
		string b = null;
		int num = this.dropdown.value;
		if (data.TryGetValue(this.dataKey, out b))
		{
			for (int i = 0; i < this.dataValues.Count; i++)
			{
				if (this.dataValues[i] == b)
				{
					num = i;
					break;
				}
			}
		}
		if (this.dropdown.value != num)
		{
			this.dropdown.value = num;
		}
	}

	// Token: 0x06002D7A RID: 11642 RVA: 0x0014470D File Offset: 0x00142B0D
	private void DropdownSelect(int dropdownIndex)
	{
		if (dropdownIndex < this.dataValues.Count)
		{
			this.parentPanel.ElementEditedText(this.dataKey, this.dataValues[dropdownIndex]);
		}
	}

	// Token: 0x04002602 RID: 9730
	public string dataKey;

	// Token: 0x04002603 RID: 9731
	public List<string> dataValues;

	// Token: 0x04002604 RID: 9732
	protected Dropdown dropdown;
}
