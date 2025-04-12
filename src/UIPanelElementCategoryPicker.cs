using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000415 RID: 1045
public class UIPanelElementCategoryPicker : UIPanelElement
{
	// Token: 0x06002D71 RID: 11633 RVA: 0x00144088 File Offset: 0x00142488
	public override void Clear()
	{
		if (this.elements == null)
		{
			this.elements = new List<GameObject>();
			this.toggles = new List<Toggle>();
		}
		else
		{
			foreach (GameObject obj in this.elements)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.elements.Clear();
			this.toggles.Clear();
		}
		this.selectedCategories = new List<int>();
		this.rowTemplate.SetActive(false);
	}

	// Token: 0x06002D72 RID: 11634 RVA: 0x00144138 File Offset: 0x00142538
	public override void Fill(Dictionary<string, string> data)
	{
		this.Clear();
		string text = null;
		this.selectedCategories = new List<int>();
		if (data.TryGetValue(this.categoryIDsKey, out text))
		{
			string[] array = text.Split(new char[]
			{
				','
			});
			foreach (string text2 in array)
			{
				if (!string.IsNullOrEmpty(text2))
				{
					int item;
					if (int.TryParse(text2, out item))
					{
						this.selectedCategories.Add(item);
					}
				}
			}
		}
		if (BWCategory.visibleWorldCategories == null)
		{
			return;
		}
		RectTransform parent = (RectTransform)base.transform;
		using (List<BWCategory>.Enumerator enumerator = BWCategory.visibleWorldCategories.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				BWCategory category = enumerator.Current;
				if (!string.IsNullOrEmpty(category.name))
				{
					if (category != BWCategory.blocksworldOfficialCategory && category != BWCategory.featuredCategory && category != BWCategory.leaderboardCategory)
					{
						bool isOn = this.selectedCategories.Contains(category.categoryID);
						GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.rowTemplate);
						RectTransform rectTransform = (RectTransform)gameObject.transform;
						rectTransform.SetParent(parent, false);
						gameObject.SetActive(true);
						this.elements.Add(gameObject);
						Text componentInChildren = gameObject.GetComponentInChildren<Text>();
						if (componentInChildren != null)
						{
							componentInChildren.text = category.name;
						}
						Toggle componentInChildren2 = gameObject.GetComponentInChildren<Toggle>();
						if (componentInChildren2 != null)
						{
							this.toggles.Add(componentInChildren2);
							componentInChildren2.isOn = isOn;
							componentInChildren2.onValueChanged.AddListener(delegate(bool toggleOn)
							{
								this.SetCategorySelected(category.categoryID, toggleOn);
							});
						}
					}
				}
			}
		}
		this.RefreshInteractablity();
	}

	// Token: 0x06002D73 RID: 11635 RVA: 0x00144364 File Offset: 0x00142764
	private void SetCategorySelected(int categoryID, bool selected)
	{
		if (selected && !this.selectedCategories.Contains(categoryID))
		{
			this.selectedCategories.Add(categoryID);
		}
		if (!selected)
		{
			this.selectedCategories.Remove(categoryID);
		}
		string text = string.Empty;
		for (int i = 0; i < this.selectedCategories.Count; i++)
		{
			if (i > 0)
			{
				text += ",";
			}
			text += this.selectedCategories[i].ToString();
		}
		this.parentPanel.ElementEditedText(this.categoryIDsKey, text);
		this.RefreshInteractablity();
	}

	// Token: 0x06002D74 RID: 11636 RVA: 0x00144418 File Offset: 0x00142818
	private void RefreshInteractablity()
	{
		bool flag = this.selectedCategories.Count >= 3;
		foreach (Toggle toggle in this.toggles)
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

	// Token: 0x040025F2 RID: 9714
	public string categoryIDsKey = "category_ids";

	// Token: 0x040025F3 RID: 9715
	public GameObject rowTemplate;

	// Token: 0x040025F4 RID: 9716
	private List<GameObject> elements;

	// Token: 0x040025F5 RID: 9717
	private List<Toggle> toggles;

	// Token: 0x040025F6 RID: 9718
	private List<int> selectedCategories;

	// Token: 0x040025F7 RID: 9719
	private const int maxSelected = 3;
}
