using System.Collections.Generic;
using UnityEngine;

public class UIOverlays : MonoBehaviour
{
	public GameObject purchasedBanner;

	public GameObject onScreenMessage;

	public GameObject targetTemplate;

	public GameObject handTemplateDefault;

	public GameObject handTemplateTap;

	public GameObject arrowTemplate;

	public GameObject speedometerTemplate;

	public GameObject screenshotIdent;

	private UIEditableText onScreenMessageText;

	private HashSet<GameObject> _activeOverlayObjects = new HashSet<GameObject>();

	private Canvas _canvas;

	private float _fadeoutTimer;

	public void Init()
	{
		_canvas = GetComponent<Canvas>();
		_canvas.worldCamera = Blocksworld.mainCamera;
		_canvas.planeDistance = 1f;
		purchasedBanner.SetActive(value: false);
		onScreenMessageText = onScreenMessage.GetComponentInChildren<UIEditableText>();
		onScreenMessage.SetActive(value: false);
		targetTemplate.SetActive(value: false);
		handTemplateDefault.SetActive(value: false);
		handTemplateTap.SetActive(value: false);
		screenshotIdent.SetActive(value: false);
		arrowTemplate.SetActive(value: false);
		speedometerTemplate.SetActive(value: false);
	}

	public void ShowPurchasedBanner()
	{
		purchasedBanner.SetActive(value: true);
	}

	public void HidePurchasedBanner()
	{
		purchasedBanner.SetActive(value: false);
	}

	public void ShowOnScreenMessage(string messageStr)
	{
		onScreenMessageText.Set(messageStr);
		onScreenMessage.SetActive(value: true);
		_fadeoutTimer = 0f;
	}

	public void ShowTimedOnScreenMessage(string messageStr, float fadeoutTime)
	{
		onScreenMessageText.Set(messageStr);
		onScreenMessage.SetActive(value: true);
		_fadeoutTimer = fadeoutTime;
	}

	public void HideOnScreenMessage()
	{
		onScreenMessage.SetActive(value: false);
		_fadeoutTimer = 0f;
	}

	public void ShowScreenshotIdent()
	{
		_canvas.renderMode = RenderMode.ScreenSpaceCamera;
		screenshotIdent.SetActive(value: true);
	}

	public void HideScreenshotIdent()
	{
		screenshotIdent.SetActive(value: false);
		_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
	}

	public UISpeedometer CreateSpeedometer()
	{
		GameObject gameObject = Object.Instantiate(speedometerTemplate);
		((RectTransform)gameObject.transform).SetParent((RectTransform)speedometerTemplate.transform.parent, worldPositionStays: false);
		return gameObject.GetComponent<UISpeedometer>();
	}

	public void RemoveSpeedometer(UISpeedometer speedometer)
	{
		Object.Destroy(speedometer.gameObject);
	}

	public UIArrow CreateArrow()
	{
		GameObject gameObject = Object.Instantiate(arrowTemplate);
		((RectTransform)gameObject.transform).SetParent(base.transform, worldPositionStays: false);
		UIArrow component = gameObject.GetComponent<UIArrow>();
		component.Init();
		return component;
	}

	public void SetArrowEndpoints(UIArrow arrow, Vector3 fromPos, Vector3 toPos)
	{
		arrow.SetEndpoints(TileSpaceToCanvasSpace(fromPos), TileSpaceToCanvasSpace(toPos));
	}

	public GameObject CreateTargetOverlayObject()
	{
		return CreateOverlayObjectFromTemplate(targetTemplate);
	}

	public GameObject CreateHandDefaultOverlayObject()
	{
		return CreateOverlayObjectFromTemplate(handTemplateDefault);
	}

	public GameObject CreateHandTapOverlayObject()
	{
		return CreateOverlayObjectFromTemplate(handTemplateTap);
	}

	private GameObject CreateOverlayObjectFromTemplate(GameObject template)
	{
		GameObject gameObject = Object.Instantiate(template);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(base.transform, worldPositionStays: false);
		_activeOverlayObjects.Add(gameObject);
		return gameObject;
	}

	public void RemoveOverlayObject(GameObject overlay)
	{
		if (_activeOverlayObjects.Contains(overlay))
		{
			_activeOverlayObjects.Remove(overlay);
		}
		else
		{
			BWLog.Warning("Not a overlay object");
		}
		Object.Destroy(overlay);
	}

	public Vector3 GetOverlayObjectPosition(GameObject overlay)
	{
		if (!_activeOverlayObjects.Contains(overlay))
		{
			BWLog.Warning("Not a overlay object");
			return Vector3.zero;
		}
		RectTransform rectTransform = overlay.transform as RectTransform;
		return CanvasSpaceToTileSpace(rectTransform.anchoredPosition);
	}

	public void SetOverlayObjectPosition(GameObject overlay, Vector3 pos)
	{
		if (!_activeOverlayObjects.Contains(overlay))
		{
			BWLog.Warning("Not a overlay object");
			return;
		}
		Vector2 anchoredPosition = TileSpaceToCanvasSpace(pos);
		RectTransform rectTransform = overlay.transform as RectTransform;
		rectTransform.anchoredPosition = anchoredPosition;
	}

	public void SetOverlayObjectSize(GameObject overlay, float size)
	{
		if (!_activeOverlayObjects.Contains(overlay))
		{
			BWLog.Warning("Not a overlay object");
			return;
		}
		RectTransform rectTransform = (RectTransform)overlay.transform;
		rectTransform.sizeDelta = size * Vector2.one;
	}

	public void SetOverlayObjectScale(GameObject overlay, float scale)
	{
		if (!_activeOverlayObjects.Contains(overlay))
		{
			BWLog.Warning("Not a overlay object");
			return;
		}
		RectTransform rectTransform = (RectTransform)overlay.transform;
		rectTransform.localScale = scale * Vector3.one;
	}

	private Vector2 TileSpaceToCanvasSpace(Vector3 pos)
	{
		float num = Mathf.Max(1f, Screen.width);
		float num2 = NormalizedScreen.referenceResolution.x / num;
		return pos * num2;
	}

	private Vector3 CanvasSpaceToTileSpace(Vector3 pos)
	{
		float num = (float)Screen.width / NormalizedScreen.referenceResolution.x;
		return pos * num;
	}

	private void Update()
	{
		if (_fadeoutTimer > 0f)
		{
			_fadeoutTimer -= Time.deltaTime;
			if (_fadeoutTimer <= 0f)
			{
				HideOnScreenMessage();
			}
		}
	}
}
