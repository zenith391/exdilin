using System;
using System.Collections.Generic;

// Token: 0x0200041F RID: 1055
public class UIPanelElementModelCategorySelector : UIPanelElementDropdown
{
	// Token: 0x06002D99 RID: 11673 RVA: 0x00144F08 File Offset: 0x00143308
	public override void OnEnable()
	{
		base.OnEnable();
		this.dataValues = new List<string>();
		foreach (BWCategory bwcategory in BWCategory.modelCategories)
		{
			this.dataValues.Add(bwcategory.name);
		}
		this.dropdown.ClearOptions();
		this.dropdown.AddOptions(this.dataValues);
	}
}
