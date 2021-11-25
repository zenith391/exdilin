using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000313 RID: 787
public class UIOverlays : MonoBehaviour
{
	// Token: 0x06002378 RID: 9080 RVA: 0x001060B8 File Offset: 0x001044B8
	public void Init()
	{
		this._canvas = base.GetComponent<Canvas>();
		this._canvas.worldCamera = Blocksworld.mainCamera;
		this._canvas.planeDistance = 1f;
		this.purchasedBanner.SetActive(false);
		this.onScreenMessageText = this.onScreenMessage.GetComponentInChildren<UIEditableText>();
		this.onScreenMessage.SetActive(false);
		this.targetTemplate.SetActive(false);
		this.handTemplateDefault.SetActive(false);
		this.handTemplateTap.SetActive(false);
		this.screenshotIdent.SetActive(false);
		this.arrowTemplate.SetActive(false);
		this.speedometerTemplate.SetActive(false);
	}

	// Token: 0x06002379 RID: 9081 RVA: 0x00106162 File Offset: 0x00104562
	public void ShowPurchasedBanner()
	{
		this.purchasedBanner.SetActive(true);
	}

	// Token: 0x0600237A RID: 9082 RVA: 0x00106170 File Offset: 0x00104570
	public void HidePurchasedBanner()
	{
		this.purchasedBanner.SetActive(false);
	}

	// Token: 0x0600237B RID: 9083 RVA: 0x0010617E File Offset: 0x0010457E
	public void ShowOnScreenMessage(string messageStr)
	{
		this.onScreenMessageText.Set(messageStr);
		this.onScreenMessage.SetActive(true);
		this._fadeoutTimer = 0f;
	}

	// Token: 0x0600237C RID: 9084 RVA: 0x001061A3 File Offset: 0x001045A3
	public void ShowTimedOnScreenMessage(string messageStr, float fadeoutTime)
	{
		this.onScreenMessageText.Set(messageStr);
		this.onScreenMessage.SetActive(true);
		this._fadeoutTimer = fadeoutTime;
	}

	// Token: 0x0600237D RID: 9085 RVA: 0x001061C4 File Offset: 0x001045C4
	public void HideOnScreenMessage()
	{
		this.onScreenMessage.SetActive(false);
		this._fadeoutTimer = 0f;
	}

	// Token: 0x0600237E RID: 9086 RVA: 0x001061DD File Offset: 0x001045DD
	public void ShowScreenshotIdent()
	{
		this._canvas.renderMode = RenderMode.ScreenSpaceCamera;
		this.screenshotIdent.SetActive(true);
	}

	// Token: 0x0600237F RID: 9087 RVA: 0x001061F7 File Offset: 0x001045F7
	public void HideScreenshotIdent()
	{
		this.screenshotIdent.SetActive(false);
		this._canvas.renderMode = RenderMode.ScreenSpaceOverlay;
	}

	// Token: 0x06002380 RID: 9088 RVA: 0x00106214 File Offset: 0x00104614
	public UISpeedometer CreateSpeedometer()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.speedometerTemplate);
		((RectTransform)gameObject.transform).SetParent((RectTransform)this.speedometerTemplate.transform.parent, false);
		return gameObject.GetComponent<UISpeedometer>();
	}

	// Token: 0x06002381 RID: 9089 RVA: 0x00106259 File Offset: 0x00104659
	public void RemoveSpeedometer(UISpeedometer speedometer)
	{
		UnityEngine.Object.Destroy(speedometer.gameObject);
	}

	// Token: 0x06002382 RID: 9090 RVA: 0x00106268 File Offset: 0x00104668
	public UIArrow CreateArrow()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.arrowTemplate);
		((RectTransform)gameObject.transform).SetParent(base.transform, false);
		UIArrow component = gameObject.GetComponent<UIArrow>();
		component.Init();
		return component;
	}

	// Token: 0x06002383 RID: 9091 RVA: 0x001062A6 File Offset: 0x001046A6
	public void SetArrowEndpoints(UIArrow arrow, Vector3 fromPos, Vector3 toPos)
	{
		arrow.SetEndpoints(this.TileSpaceToCanvasSpace(fromPos), this.TileSpaceToCanvasSpace(toPos));
	}

	// Token: 0x06002384 RID: 9092 RVA: 0x001062BC File Offset: 0x001046BC
	public GameObject CreateTargetOverlayObject()
	{
		return this.CreateOverlayObjectFromTemplate(this.targetTemplate);
	}

	// Token: 0x06002385 RID: 9093 RVA: 0x001062CA File Offset: 0x001046CA
	public GameObject CreateHandDefaultOverlayObject()
	{
		return this.CreateOverlayObjectFromTemplate(this.handTemplateDefault);
	}

	// Token: 0x06002386 RID: 9094 RVA: 0x001062D8 File Offset: 0x001046D8
	public GameObject CreateHandTapOverlayObject()
	{
		return this.CreateOverlayObjectFromTemplate(this.handTemplateTap);
	}

	// Token: 0x06002387 RID: 9095 RVA: 0x001062E8 File Offset: 0x001046E8
	private GameObject CreateOverlayObjectFromTemplate(GameObject template)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(template);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(base.transform, false);
		this._activeOverlayObjects.Add(gameObject);
		return gameObject;
	}

	// Token: 0x06002388 RID: 9096 RVA: 0x00106323 File Offset: 0x00104723
	public void RemoveOverlayObject(GameObject overlay)
	{
		if (this._activeOverlayObjects.Contains(overlay))
		{
			this._activeOverlayObjects.Remove(overlay);
		}
		else
		{
			BWLog.Warning("Not a overlay object");
		}
		UnityEngine.Object.Destroy(overlay);
	}

	// Token: 0x06002389 RID: 9097 RVA: 0x00106358 File Offset: 0x00104758
	public Vector3 GetOverlayObjectPosition(GameObject overlay)
	{
		if (!this._activeOverlayObjects.Contains(overlay))
		{
			BWLog.Warning("Not a overlay object");
			return Vector3.zero;
		}
		RectTransform rectTransform = overlay.transform as RectTransform;
		return this.CanvasSpaceToTileSpace(rectTransform.anchoredPosition);
	}

	// Token: 0x0600238A RID: 9098 RVA: 0x001063A4 File Offset: 0x001047A4
	public void SetOverlayObjectPosition(GameObject overlay, Vector3 pos)
	{
		if (!this._activeOverlayObjects.Contains(overlay))
		{
			BWLog.Warning("Not a overlay object");
			return;
		}
		Vector2 anchoredPosition = this.TileSpaceToCanvasSpace(pos);
		RectTransform rectTransform = overlay.transform as RectTransform;
		rectTransform.anchoredPosition = anchoredPosition;
	}

	// Token: 0x0600238B RID: 9099 RVA: 0x001063E8 File Offset: 0x001047E8
	public void SetOverlayObjectSize(GameObject overlay, float size)
	{
		if (!this._activeOverlayObjects.Contains(overlay))
		{
			BWLog.Warning("Not a overlay object");
			return;
		}
		RectTransform rectTransform = (RectTransform)overlay.transform;
		rectTransform.sizeDelta = size * Vector2.one;
	}

	// Token: 0x0600238C RID: 9100 RVA: 0x00106430 File Offset: 0x00104830
	public void SetOverlayObjectScale(GameObject overlay, float scale)
	{
		if (!this._activeOverlayObjects.Contains(overlay))
		{
			BWLog.Warning("Not a overlay object");
			return;
		}
		RectTransform rectTransform = (RectTransform)overlay.transform;
		rectTransform.localScale = scale * Vector3.one;
	}

	// Token: 0x0600238D RID: 9101 RVA: 0x00106478 File Offset: 0x00104878
	private Vector2 TileSpaceToCanvasSpace(Vector3 pos)
	{
		float num = Mathf.Max(1f, (float)Screen.width);
		float d = NormalizedScreen.referenceResolution.x / num;
		return pos * d;
	}

	// Token: 0x0600238E RID: 9102 RVA: 0x001064B4 File Offset: 0x001048B4
	private Vector3 CanvasSpaceToTileSpace(Vector3 pos)
	{
		float d = (float)Screen.width / NormalizedScreen.referenceResolution.x;
		return pos * d;
	}

	// Token: 0x0600238F RID: 9103 RVA: 0x001064DD File Offset: 0x001048DD
	private void Update()
	{
		if (this._fadeoutTimer > 0f)
		{
			this._fadeoutTimer -= Time.deltaTime;
			if (this._fadeoutTimer <= 0f)
			{
				this.HideOnScreenMessage();
			}
		}
	}

	// Token: 0x04001EBA RID: 7866
	public GameObject purchasedBanner;

	// Token: 0x04001EBB RID: 7867
	public GameObject onScreenMessage;

	// Token: 0x04001EBC RID: 7868
	public GameObject targetTemplate;

	// Token: 0x04001EBD RID: 7869
	public GameObject handTemplateDefault;

	// Token: 0x04001EBE RID: 7870
	public GameObject handTemplateTap;

	// Token: 0x04001EBF RID: 7871
	public GameObject arrowTemplate;

	// Token: 0x04001EC0 RID: 7872
	public GameObject speedometerTemplate;

	// Token: 0x04001EC1 RID: 7873
	public GameObject screenshotIdent;

	// Token: 0x04001EC2 RID: 7874
	private UIEditableText onScreenMessageText;

	// Token: 0x04001EC3 RID: 7875
	private HashSet<GameObject> _activeOverlayObjects = new HashSet<GameObject>();

	// Token: 0x04001EC4 RID: 7876
	private Canvas _canvas;

	// Token: 0x04001EC5 RID: 7877
	private float _fadeoutTimer;
}
