using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelElementConditional : UIPanelElement
{
	public string dataKey;

	public string matchString;

	public bool negate;

	public GameObject[] targetObjects;

	public Selectable[] targetSelectables;

	public CanvasGroup[] targetCanvasGroups;

	public bool hideTargetObjects = true;

	public bool disableTargetSelectables = true;

	public bool disableTargetCanvasGroups = true;

	public float disabledCanvasAlpha = 0.35f;

	public override void Fill(Dictionary<string, string> data)
	{
		if (!data.TryGetValue(dataKey, out var value))
		{
			return;
		}
		string text = ((!string.IsNullOrEmpty(matchString)) ? matchString : "true");
		bool flag = value.ToLowerInvariant() == text;
		if (negate)
		{
			flag = !flag;
		}
		if (disableTargetSelectables && targetSelectables != null)
		{
			Selectable[] array = targetSelectables;
			foreach (Selectable selectable in array)
			{
				selectable.interactable = flag;
			}
		}
		if (hideTargetObjects)
		{
			if (targetObjects != null && targetObjects.Length != 0)
			{
				GameObject[] array2 = targetObjects;
				foreach (GameObject gameObject in array2)
				{
					gameObject.SetActive(flag);
				}
			}
			else
			{
				base.gameObject.SetActive(flag);
			}
		}
		if (disableTargetCanvasGroups && targetCanvasGroups != null)
		{
			CanvasGroup[] array3 = targetCanvasGroups;
			foreach (CanvasGroup canvasGroup in array3)
			{
				canvasGroup.interactable = flag;
				canvasGroup.alpha = ((!flag) ? disabledCanvasAlpha : 1f);
			}
		}
	}
}
