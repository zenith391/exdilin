using System.Collections.Generic;

public class UIPanelElementModelCategorySelector : UIPanelElementDropdown
{
	public override void OnEnable()
	{
		base.OnEnable();
		dataValues = new List<string>();
		foreach (BWCategory modelCategory in BWCategory.modelCategories)
		{
			dataValues.Add(modelCategory.name);
		}
		dropdown.ClearOptions();
		dropdown.AddOptions(dataValues);
	}
}
