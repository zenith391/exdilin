using System.Collections.Generic;
using UnityEngine;

public class UIPanelElementList : UIPanelElement
{
	public string dataKey;

	public GameObject template;

	private List<GameObject> elements;

	public override void Clear()
	{
		if (elements == null)
		{
			elements = new List<GameObject>();
		}
		else
		{
			foreach (GameObject element in elements)
			{
				Object.Destroy(element);
			}
			elements.Clear();
		}
		template.gameObject.SetActive(value: false);
	}

	public override void Fill(Dictionary<string, string> data)
	{
		Clear();
		string value = null;
		if (!data.TryGetValue(dataKey, out value))
		{
			return;
		}
		string[] array = value.Split(',');
		RectTransform parent = (RectTransform)base.transform;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!string.IsNullOrEmpty(text))
			{
				GameObject gameObject = Object.Instantiate(template.gameObject);
				RectTransform rectTransform = (RectTransform)gameObject.transform;
				rectTransform.SetParent(parent, worldPositionStays: false);
				gameObject.SetActive(value: true);
				elements.Add(gameObject);
				UIPanelElement[] componentsInChildren = gameObject.GetComponentsInChildren<UIPanelElement>();
				UIPanelElement[] array3 = componentsInChildren;
				foreach (UIPanelElement uIPanelElement in array3)
				{
					uIPanelElement.Init(parentPanel);
					uIPanelElement.Fill(text);
				}
			}
		}
	}
}
