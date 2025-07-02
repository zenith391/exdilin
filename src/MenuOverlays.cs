using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuOverlays : MonoBehaviour, IPopupDelegate
{
	public RectTransform overlayTransform;

	public GameObject uiBusyOverlay;

	public Animator loadingOverlayAnimator;

	public Selectable popupBackground;

	public UIPopup activePopup;

	public UIPopup messagePopupPrefab;

	public UIPopupConfirmation confirmationPopupPrefab;

	public UIPopup escapeMenuPopupPrefab;

	public UIPopup escapeMenuInWorldPopupPrefab;

	public UIPopup reportWorldPopupPrefab;

	public UIPopup reportModelPopupPrefab;

	public UIPopupSetModelPrice editModelPricePopupPrefab;

	public UIPopupLinkToIOS linkToIOSPopupPrefab;

	public UIPopupPendingPayouts pendingPayoutsPopupPrefab;

	public UIPopupDetailPanel worldDetailPopupPrefab;

	public UIPopupDetailPanel u2uModelDetailPanelPrefab;

	public UIPopupDetailPanel userModelDetailPanelPrefab;

	public UINotificationFeed notifications;

	public RectTransform coinAnimPrefab;

	private GameObject selectionBeforePopup;

	public void SetUIBusy(bool busy)
	{
		uiBusyOverlay.SetActive(busy);
	}

	public void ShowMessage(string messageStr)
	{
		ShowPopup(messagePopupPrefab);
		activePopup.mainText.text = messageStr;
	}

	public void ShowMessage(BWMenuTextEnum message)
	{
		string textString = MenuTextDefinitions.GetTextString(message);
		ShowMessage(textString);
	}

	public void ShowConfirmationDialog(string titleStr, string messageStr, UnityAction yesAction, string yesButtonText, string noButtonText)
	{
		ShowPopup(confirmationPopupPrefab);
		activePopup.mainText.text = messageStr;
		UIPopupConfirmation uIPopupConfirmation = (UIPopupConfirmation)activePopup;
		uIPopupConfirmation.SetTitleText(titleStr);
		uIPopupConfirmation.yesAction = yesAction;
		uIPopupConfirmation.yesText.text = yesButtonText;
		uIPopupConfirmation.noText.text = noButtonText;
	}

	public void ShowConfirmationDialog(string messageStr, UnityAction yesAction)
	{
		ShowPopup(confirmationPopupPrefab);
		activePopup.mainText.text = messageStr;
		((UIPopupConfirmation)activePopup).yesAction = yesAction;
	}

	public void ShowConfirmationDialog(string messageStr, UnityAction yesAction, string yesButtonText, string noButtonText)
	{
		ShowPopup(confirmationPopupPrefab);
		activePopup.mainText.text = messageStr;
		UIPopupConfirmation uIPopupConfirmation = (UIPopupConfirmation)activePopup;
		uIPopupConfirmation.yesAction = yesAction;
		uIPopupConfirmation.yesText.text = yesButtonText;
		uIPopupConfirmation.noText.text = noButtonText;
	}

	public void ShowConfirmationDialog(BWMenuTextEnum message, UnityAction yesAction)
	{
		string textString = MenuTextDefinitions.GetTextString(message);
		ShowConfirmationDialog(textString, yesAction);
	}

	public void ShowPopupReportWorld(string worldID)
	{
		if (reportWorldPopupPrefab != null)
		{
			ShowPopup(reportWorldPopupPrefab);
			UIReportWorldPanel component = activePopup.GetComponent<UIReportWorldPanel>();
			if (component != null)
			{
				component.Setup(worldID);
			}
		}
	}

	public void ShowPopupReportModel(string modelID)
	{
		if (reportModelPopupPrefab != null)
		{
			ShowPopup(reportModelPopupPrefab);
			UIReportModelPanel component = activePopup.GetComponent<UIReportModelPanel>();
			if (component != null)
			{
				component.Setup(modelID);
			}
		}
	}

	public void ShowPopupEscapeMenu()
	{
		if (escapeMenuPopupPrefab != null)
		{
			ShowPopup(escapeMenuPopupPrefab);
		}
	}

	public void ShowPopupSkipPublishCooldown(int coins, UnityAction skipPublishAction)
	{
		ShowConfirmationDialog("Pay " + coins + " coins to publish now?", delegate
		{
			if (BWUser.currentUser.coins < coins)
			{
				ShowPopupInsufficentCoins();
			}
			else
			{
				skipPublishAction();
			}
		});
	}

	public void ShowPopupInsufficentCoins()
	{
		BWStandalone.Overlays.ShowConfirmationDialog("Not Enough Coins!", "You don't have enough coins to buy these items.\n\nWould you like to get more coins?", delegate
		{
			MainUIController.Instance.LoadUIScene("ShopMenu");
		}, "Yes", "No");
	}

	public void ShowPopupEditModelPrice(BWUserModel model)
	{
		if (editModelPricePopupPrefab != null)
		{
			ShowPopup(editModelPricePopupPrefab, this);
			UIPopupSetModelPrice uIPopupSetModelPrice = (UIPopupSetModelPrice)activePopup;
			uIPopupSetModelPrice.SetupForModel(model);
		}
	}

	public void ShowPopupLinkToIOSAccount()
	{
		if (linkToIOSPopupPrefab != null)
		{
			ShowPopup(linkToIOSPopupPrefab, this);
		}
	}

	public void ShowPopupPendingPayouts()
	{
		if (pendingPayoutsPopupPrefab != null)
		{
			ShowPopup(pendingPayoutsPopupPrefab, this);
		}
	}

	public void ShowPopupWorldDetailPanel(string worldID, UIDataSource dataSource)
	{
		if (worldDetailPopupPrefab != null)
		{
			ShowPopup(worldDetailPopupPrefab, this);
			activePopup.LoadData(dataSource, ImageManager.Instance, worldID);
		}
	}

	public void ShowPopupU2UModelDetailPanel(string modelID, UIDataSource dataSource)
	{
		if (u2uModelDetailPanelPrefab != null)
		{
			ShowPopup(u2uModelDetailPanelPrefab, this);
			activePopup.LoadData(dataSource, ImageManager.Instance, modelID);
		}
	}

	public void ShowPopupUserModelDetailPanel(string localModelID, UIDataSource dataSource)
	{
		if (userModelDetailPanelPrefab != null)
		{
			ShowPopup(userModelDetailPanelPrefab, this);
			activePopup.LoadData(dataSource, ImageManager.Instance, localModelID);
		}
	}

	public UIPopup ShowPopup(UIPopup popupPrefab, IPopupDelegate popupDelegate = null)
	{
		popupBackground.gameObject.SetActive(value: true);
		if (activePopup != null)
		{
			Object.Destroy(activePopup.gameObject);
		}
		else
		{
			selectionBeforePopup = EventSystem.current.currentSelectedGameObject;
		}
		activePopup = Object.Instantiate(popupPrefab);
		if (popupDelegate == null)
		{
			popupDelegate = this;
		}
		activePopup.Show(popupDelegate);
		RectTransform rectTransform = (RectTransform)activePopup.gameObject.transform;
		RectTransform parent = (RectTransform)popupBackground.transform;
		rectTransform.SetParent(parent, worldPositionStays: false);
		if (activePopup.defaultSelectable != null)
		{
			EventSystem.current.SetSelectedGameObject(activePopup.defaultSelectable.gameObject);
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(activePopup.gameObject);
		}
		return activePopup;
	}

	public void PopupBackgroundClicked()
	{
		if (activePopup == null)
		{
			ClosePopup();
		}
		else if (activePopup.closeOnBackgroundClick)
		{
			activePopup.Hide();
		}
	}

	public bool IsShowingPopup()
	{
		return activePopup != null;
	}

	public void ClosePopup()
	{
		UISoundPlayer.Instance.PlayClip("DefaultButtonClick");
		if (activePopup != null)
		{
			Object.Destroy(activePopup.gameObject);
		}
		activePopup = null;
		popupBackground.gameObject.SetActive(value: false);
		EventSystem.current.SetSelectedGameObject(selectionBeforePopup);
	}

	public void ShowLoadingOverlay(bool show)
	{
		loadingOverlayAnimator.SetTrigger((!show) ? "Hide" : "Show");
	}

	public void ParentToCanvas(RectTransform rt)
	{
		rt.SetParent(overlayTransform, worldPositionStays: true);
		rt.localScale = Vector3.one;
	}

	public void DoCoinAwardAnimation(Vector3 fromPosition, UnityAction completion)
	{
		RectTransform rectTransform = Object.Instantiate(coinAnimPrefab);
		rectTransform.position = fromPosition;
		ParentToCanvas(rectTransform);
		MainUIController.Instance.menuBar.PutCoinsDiplayInOverlay(inOverlay: true);
		Vector3 position = MainUIController.Instance.menuBar.coinsAnimTarget.position;
		StartCoroutine(CoinAwardAnimCoroutine(rectTransform, position, completion));
	}

	private IEnumerator CoinAwardAnimCoroutine(RectTransform coinAnimT, Vector3 targetPos, UnityAction completion)
	{
		UISoundPlayer.Instance.PlayClip("shop_purchase");
		ParentToCanvas(coinAnimT);
		yield return StartCoroutine(AnimateTransformCoroutine(coinAnimT, targetPos, 0.6f, 1.2f, destroyOnCompletion: true, null));
		BWUserDataManager.Instance.NotifyListeners();
		yield return new WaitForSeconds(1f);
		MainUIController.Instance.menuBar.PutCoinsDiplayInOverlay(inOverlay: false);
		completion?.Invoke();
	}

	public void AnimateTransform(RectTransform rt, Vector3 endPosition, float endScale, float speed, bool destroyOnCompletion, UnityAction completion)
	{
		ParentToCanvas(rt);
		StartCoroutine(AnimateTransformCoroutine(rt, endPosition, endScale, speed, destroyOnCompletion, completion));
	}

	private IEnumerator AnimateTransformCoroutine(RectTransform rt, Vector3 endPosition, float endScale, float speed, bool destroyOnCompletion, UnityAction completion)
	{
		float timer = 0f;
		Vector3 startPosition = rt.position;
		Vector3 startScale = rt.localScale;
		float num = (endPosition - startPosition).magnitude / (float)Screen.width;
		float duration = num / speed;
		while (timer < duration)
		{
			Vector3 position = Vector3.Lerp(startPosition, endPosition, timer / duration);
			Vector3 localScale = Vector3.Lerp(startScale, endScale * Vector3.one, timer / duration);
			rt.position = position;
			rt.localScale = localScale;
			timer += Time.deltaTime;
			yield return null;
		}
		if (destroyOnCompletion)
		{
			Object.Destroy(rt.gameObject);
		}
		completion?.Invoke();
	}
}
