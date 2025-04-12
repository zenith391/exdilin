using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200045A RID: 1114
[RequireComponent(typeof(ScrollRect))]
public class UIScrollControl : MonoBehaviour
{
	// Token: 0x06002F2B RID: 12075 RVA: 0x0014E3C3 File Offset: 0x0014C7C3
	public void OnEnable()
	{
		this.scrollRect = base.GetComponent<ScrollRect>();
	}

	// Token: 0x06002F2C RID: 12076 RVA: 0x0014E3D1 File Offset: 0x0014C7D1
	public void ScrollToTransform(RectTransform rt, float screenPos, float scrollSpeed, float delay)
	{
		base.StopAllCoroutines();
		base.StartCoroutine(this.ScrollToTransformCoroutine(rt, screenPos, scrollSpeed, delay));
	}

	// Token: 0x06002F2D RID: 12077 RVA: 0x0014E3EB File Offset: 0x0014C7EB
	public void SnapToTransform(RectTransform rt, float screenPos)
	{
		base.StopAllCoroutines();
		this.scrollRect.StopMovement();
		base.StartCoroutine(this.SnapToTransformCoroutine(rt, screenPos));
	}

	// Token: 0x06002F2E RID: 12078 RVA: 0x0014E40D File Offset: 0x0014C80D
	public void ClampScroll()
	{
		base.StartCoroutine(this.ClampScrollCoroutine());
	}

	// Token: 0x06002F2F RID: 12079 RVA: 0x0014E41C File Offset: 0x0014C81C
	private IEnumerator ClampScrollCoroutine()
	{
		yield return null;
		this.scrollRect.verticalNormalizedPosition = Mathf.Clamp01(this.scrollRect.verticalNormalizedPosition);
		yield break;
	}

	// Token: 0x06002F30 RID: 12080 RVA: 0x0014E437 File Offset: 0x0014C837
	public void Cancel()
	{
		base.StopAllCoroutines();
	}

	// Token: 0x06002F31 RID: 12081 RVA: 0x0014E440 File Offset: 0x0014C840
	private IEnumerator SnapToTransformCoroutine(RectTransform rectTransform, float screenPos)
	{
		float restoreSensitivity = this.scrollRect.scrollSensitivity;
		this.scrollRect.scrollSensitivity = 0f;
		this.scrollRect.verticalScrollbar.enabled = false;
		int maxIterations = 20;
		int iterations = 0;
		float diff = 100f;
		while (iterations <= maxIterations && Mathf.Abs(diff) > 2f)
		{
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);
			float centerY = 0f;
			for (int i = 0; i < 4; i++)
			{
				centerY += corners[i].y / 4f;
			}
			float target = screenPos * (float)Screen.height;
			diff = target - centerY;
			if (Mathf.Abs(diff) > 2f)
			{
				float num = diff / this.scrollRect.content.sizeDelta.y;
				float verticalNormalizedPosition = Mathf.Clamp01(this.scrollRect.verticalNormalizedPosition - num);
				this.scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
				Canvas.ForceUpdateCanvases();
			}
			if (iterations == 0)
			{
				yield return null;
			}
			iterations++;
		}
		this.scrollRect.scrollSensitivity = restoreSensitivity;
		this.scrollRect.verticalScrollbar.enabled = true;
		yield break;
	}

	// Token: 0x06002F32 RID: 12082 RVA: 0x0014E46C File Offset: 0x0014C86C
	private IEnumerator ScrollToTransformCoroutine(RectTransform rectTransform, float screenPos, float scrollSpeed, float delay)
	{
		this.scrollRect.StopMovement();
		float restoreSensitivity = this.scrollRect.scrollSensitivity;
		this.scrollRect.scrollSensitivity = 0f;
		this.scrollRect.verticalScrollbar.enabled = false;
		Canvas.ForceUpdateCanvases();
		yield return new WaitForSeconds(delay);
		if (this.scrollRect == null || rectTransform == null)
		{
			yield break;
		}
		Vector3[] corners = new Vector3[4];
		float speedScale = (float)Screen.height / this.scrollRect.content.sizeDelta.y;
		float timer = 0f;
		float diff = 100f;
		while (Mathf.Abs(diff) > 2f && timer < 2f)
		{
			if (this.scrollRect == null || rectTransform == null)
			{
				yield break;
			}
			rectTransform.GetWorldCorners(corners);
			float centerY = 0f;
			for (int i = 0; i < 4; i++)
			{
				centerY += corners[i].y / 4f;
			}
			float target = screenPos * (float)Screen.height;
			diff = target - centerY;
			float normalizedDiff = diff / this.scrollRect.content.sizeDelta.y;
			this.scrollRect.verticalNormalizedPosition = Mathf.Clamp01(this.scrollRect.verticalNormalizedPosition - scrollSpeed * normalizedDiff);
			Canvas.ForceUpdateCanvases();
			if (scrollSpeed < 1f)
			{
				yield return null;
			}
			timer += Time.deltaTime;
		}
		if (this.scrollRect == null || rectTransform == null)
		{
			yield break;
		}
		this.scrollRect.verticalScrollbar.enabled = true;
		this.scrollRect.scrollSensitivity = restoreSensitivity;
		yield break;
	}

	// Token: 0x040027A0 RID: 10144
	private ScrollRect scrollRect;
}
