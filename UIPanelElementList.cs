using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200041A RID: 1050
public class UIPanelElementList : UIPanelElement
{
	// Token: 0x06002D80 RID: 11648 RVA: 0x0014494C File Offset: 0x00142D4C
	public override void Clear()
	{
		if (this.elements == null)
		{
			this.elements = new List<GameObject>();
		}
		else
		{
			foreach (GameObject obj in this.elements)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.elements.Clear();
		}
		this.template.gameObject.SetActive(false);
	}

	// Token: 0x06002D81 RID: 11649 RVA: 0x001449E0 File Offset: 0x00142DE0
	public override void Fill(Dictionary<string, string> data)
	{
		this.Clear();
		string text = null;
		if (data.TryGetValue(this.dataKey, out text))
		{
			string[] array = text.Split(new char[]
			{
				','
			});
			RectTransform parent = (RectTransform)base.transform;
			foreach (string text2 in array)
			{
				if (!string.IsNullOrEmpty(text2))
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.template.gameObject);
					RectTransform rectTransform = (RectTransform)gameObject.transform;
					rectTransform.SetParent(parent, false);
					gameObject.SetActive(true);
					this.elements.Add(gameObject);
					UIPanelElement[] componentsInChildren = gameObject.GetComponentsInChildren<UIPanelElement>();
					foreach (UIPanelElement uipanelElement in componentsInChildren)
					{
						uipanelElement.Init(this.parentPanel);
						uipanelElement.Fill(text2);
					}
				}
			}
		}
	}

	// Token: 0x0400260E RID: 9742
	public string dataKey;

	// Token: 0x0400260F RID: 9743
	public GameObject template;

	// Token: 0x04002610 RID: 9744
	private List<GameObject> elements;
}
