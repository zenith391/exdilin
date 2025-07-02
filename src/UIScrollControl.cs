using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class UIScrollControl : MonoBehaviour
{
	private ScrollRect scrollRect;

	public void OnEnable()
	{
		scrollRect = GetComponent<ScrollRect>();
	}

	public void ScrollToTransform(RectTransform rt, float screenPos, float scrollSpeed, float delay)
	{
		StopAllCoroutines();
		StartCoroutine(ScrollToTransformCoroutine(rt, screenPos, scrollSpeed, delay));
	}

	public void SnapToTransform(RectTransform rt, float screenPos)
	{
		StopAllCoroutines();
		scrollRect.StopMovement();
		StartCoroutine(SnapToTransformCoroutine(rt, screenPos));
	}

	public void ClampScroll()
	{
		StartCoroutine(ClampScrollCoroutine());
	}

	private IEnumerator ClampScrollCoroutine()
	{
		yield return null;
		scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
	}

	public void Cancel()
	{
		StopAllCoroutines();
	}

	private IEnumerator SnapToTransformCoroutine(RectTransform rectTransform, float screenPos)
	{
		float restoreSensitivity = scrollRect.scrollSensitivity;
		scrollRect.scrollSensitivity = 0f;
		scrollRect.verticalScrollbar.enabled = false;
		int maxIterations = 20;
		int iterations = 0;
		float diff = 100f;
		for (; iterations <= maxIterations; iterations++)
		{
			if (!(Mathf.Abs(diff) > 2f))
			{
				break;
			}
			Vector3[] array = new Vector3[4];
			rectTransform.GetWorldCorners(array);
			float num = 0f;
			for (int i = 0; i < 4; i++)
			{
				num += array[i].y / 4f;
			}
			float num2 = screenPos * (float)Screen.height;
			diff = num2 - num;
			if (Mathf.Abs(diff) > 2f)
			{
				float num3 = diff / scrollRect.content.sizeDelta.y;
				float verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition - num3);
				scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
				Canvas.ForceUpdateCanvases();
			}
			if (iterations == 0)
			{
				yield return null;
			}
		}
		scrollRect.scrollSensitivity = restoreSensitivity;
		scrollRect.verticalScrollbar.enabled = true;
	}

	private IEnumerator ScrollToTransformCoroutine(RectTransform rectTransform, float screenPos, float scrollSpeed, float delay)
	{
		scrollRect.StopMovement();
		float restoreSensitivity = scrollRect.scrollSensitivity;
		scrollRect.scrollSensitivity = 0f;
		scrollRect.verticalScrollbar.enabled = false;
		Canvas.ForceUpdateCanvases();
		yield return new WaitForSeconds(delay);
		if (scrollRect == null || rectTransform == null)
		{
			yield break;
		}
		Vector3[] corners = new Vector3[4];
		_ = (float)Screen.height / scrollRect.content.sizeDelta.y;
		float timer = 0f;
		float diff = 100f;
		while (Mathf.Abs(diff) > 2f && timer < 2f)
		{
			if (scrollRect == null || rectTransform == null)
			{
				yield break;
			}
			rectTransform.GetWorldCorners(corners);
			float num = 0f;
			for (int i = 0; i < 4; i++)
			{
				num += corners[i].y / 4f;
			}
			float num2 = screenPos * (float)Screen.height;
			diff = num2 - num;
			float num3 = diff / scrollRect.content.sizeDelta.y;
			scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition - scrollSpeed * num3);
			Canvas.ForceUpdateCanvases();
			if (scrollSpeed < 1f)
			{
				yield return null;
			}
			timer += Time.deltaTime;
		}
		if (!(scrollRect == null) && !(rectTransform == null))
		{
			scrollRect.verticalScrollbar.enabled = true;
			scrollRect.scrollSensitivity = restoreSensitivity;
		}
	}
}
