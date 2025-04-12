using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000416 RID: 1046
public class UIPanelElementConditional : UIPanelElement
{
	// Token: 0x06002D76 RID: 11638 RVA: 0x001444EC File Offset: 0x001428EC
	public override void Fill(Dictionary<string, string> data)
	{
		string text;
		if (data.TryGetValue(this.dataKey, out text))
		{
			string b = (!string.IsNullOrEmpty(this.matchString)) ? this.matchString : "true";
			bool flag = text.ToLowerInvariant() == b;
			if (this.negate)
			{
				flag = !flag;
			}
			if (this.disableTargetSelectables && this.targetSelectables != null)
			{
				foreach (Selectable selectable in this.targetSelectables)
				{
					selectable.interactable = flag;
				}
			}
			if (this.hideTargetObjects)
			{
				if (this.targetObjects != null && this.targetObjects.Length > 0)
				{
					foreach (GameObject gameObject in this.targetObjects)
					{
						gameObject.SetActive(flag);
					}
				}
				else
				{
					base.gameObject.SetActive(flag);
				}
			}
			if (this.disableTargetCanvasGroups && this.targetCanvasGroups != null)
			{
				foreach (CanvasGroup canvasGroup in this.targetCanvasGroups)
				{
					canvasGroup.interactable = flag;
					canvasGroup.alpha = ((!flag) ? this.disabledCanvasAlpha : 1f);
				}
			}
		}
	}

	// Token: 0x040025F8 RID: 9720
	public string dataKey;

	// Token: 0x040025F9 RID: 9721
	public string matchString;

	// Token: 0x040025FA RID: 9722
	public bool negate;

	// Token: 0x040025FB RID: 9723
	public GameObject[] targetObjects;

	// Token: 0x040025FC RID: 9724
	public Selectable[] targetSelectables;

	// Token: 0x040025FD RID: 9725
	public CanvasGroup[] targetCanvasGroups;

	// Token: 0x040025FE RID: 9726
	public bool hideTargetObjects = true;

	// Token: 0x040025FF RID: 9727
	public bool disableTargetSelectables = true;

	// Token: 0x04002600 RID: 9728
	public bool disableTargetCanvasGroups = true;

	// Token: 0x04002601 RID: 9729
	public float disabledCanvasAlpha = 0.35f;
}
