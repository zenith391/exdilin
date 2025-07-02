using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelElementCategoryPicker : UIPanelElement
{
	public string categoryIDsKey = "category_ids";

	public GameObject rowTemplate;

	private List<GameObject> elements;

	private List<Toggle> toggles;

	private List<int> selectedCategories;

	private const int maxSelected = 3;

	public override void Clear()
	{
		if (elements == null)
		{
			elements = new List<GameObject>();
			toggles = new List<Toggle>();
		}
		else
		{
			foreach (GameObject element in elements)
			{
				Object.Destroy(element);
			}
			elements.Clear();
			toggles.Clear();
		}
		selectedCategories = new List<int>();
		rowTemplate.SetActive(value: false);
	}

	public override void Fill(Dictionary<string, string> data)
	{
		Clear();
		string value = null;
		selectedCategories = new List<int>();
		if (data.TryGetValue(categoryIDsKey, out value))
		{
			string[] array = value.Split(',');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!string.IsNullOrEmpty(text) && int.TryParse(text, out var result))
				{
					selectedCategories.Add(result);
				}
			}
		}
		if (BWCategory.visibleWorldCategories == null)
		{
			return;
		}
		RectTransform parent = (RectTransform)base.transform;
		foreach (BWCategory category in BWCategory.visibleWorldCategories)
		{
			if (string.IsNullOrEmpty(category.name) || category == BWCategory.blocksworldOfficialCategory || category == BWCategory.featuredCategory || category == BWCategory.leaderboardCategory)
			{
				continue;
			}
			bool isOn = selectedCategories.Contains(category.categoryID);
			GameObject gameObject = Object.Instantiate(rowTemplate);
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			rectTransform.SetParent(parent, worldPositionStays: false);
			gameObject.SetActive(value: true);
			elements.Add(gameObject);
			Text componentInChildren = gameObject.GetComponentInChildren<Text>();
			if (componentInChildren != null)
			{
				componentInChildren.text = category.name;
			}
			Toggle componentInChildren2 = gameObject.GetComponentInChildren<Toggle>();
			if (componentInChildren2 != null)
			{
				toggles.Add(componentInChildren2);
				componentInChildren2.isOn = isOn;
				componentInChildren2.onValueChanged.AddListener(delegate(bool toggleOn)
				{
					SetCategorySelected(category.categoryID, toggleOn);
				});
			}
		}
		RefreshInteractablity();
	}

	private void SetCategorySelected(int categoryID, bool selected)
	{
		if (selected && !selectedCategories.Contains(categoryID))
		{
			selectedCategories.Add(categoryID);
		}
		if (!selected)
		{
			selectedCategories.Remove(categoryID);
		}
		string text = string.Empty;
		for (int i = 0; i < selectedCategories.Count; i++)
		{
			if (i > 0)
			{
				text += ",";
			}
			text += selectedCategories[i];
		}
		parentPanel.ElementEditedText(categoryIDsKey, text);
		RefreshInteractablity();
	}

	private void RefreshInteractablity()
	{
		bool flag = selectedCategories.Count >= 3;
		foreach (Toggle toggle in toggles)
		{
			if (toggle.isOn)
			{
				toggle.interactable = true;
			}
			else
			{
				toggle.interactable = !flag;
			}
		}
	}
}
