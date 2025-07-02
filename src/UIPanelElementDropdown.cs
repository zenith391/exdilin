using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class UIPanelElementDropdown : UIPanelElement
{
	public string dataKey;

	public List<string> dataValues;

	protected Dropdown dropdown;

	public virtual void OnEnable()
	{
		dropdown = GetComponent<Dropdown>();
		dropdown.onValueChanged.AddListener(DropdownSelect);
	}

	public override void Fill(Dictionary<string, string> data)
	{
		string value = null;
		int num = dropdown.value;
		if (data.TryGetValue(dataKey, out value))
		{
			for (int i = 0; i < dataValues.Count; i++)
			{
				if (dataValues[i] == value)
				{
					num = i;
					break;
				}
			}
		}
		if (dropdown.value != num)
		{
			dropdown.value = num;
		}
	}

	private void DropdownSelect(int dropdownIndex)
	{
		if (dropdownIndex < dataValues.Count)
		{
			parentPanel.ElementEditedText(dataKey, dataValues[dropdownIndex]);
		}
	}
}
