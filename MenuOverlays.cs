using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x02000407 RID: 1031
public class MenuOverlays : MonoBehaviour, IPopupDelegate
{
	// Token: 0x06002D16 RID: 11542 RVA: 0x001424BC File Offset: 0x001408BC
	public void SetUIBusy(bool busy)
	{
		this.uiBusyOverlay.SetActive(busy);
	}

	// Token: 0x06002D17 RID: 11543 RVA: 0x001424CA File Offset: 0x001408CA
	public void ShowMessage(string messageStr)
	{
		this.ShowPopup(this.messagePopupPrefab, null);
		this.activePopup.mainText.text = messageStr;
	}

	// Token: 0x06002D18 RID: 11544 RVA: 0x001424EC File Offset: 0x001408EC
	public void ShowMessage(BWMenuTextEnum message)
	{
		string textString = MenuTextDefinitions.GetTextString(message);
		this.ShowMessage(textString);
	}

	// Token: 0x06002D19 RID: 11545 RVA: 0x00142508 File Offset: 0x00140908
	public void ShowConfirmationDialog(string titleStr, string messageStr, UnityAction yesAction, string yesButtonText, string noButtonText)
	{
		this.ShowPopup(this.confirmationPopupPrefab, null);
		this.activePopup.mainText.text = messageStr;
		UIPopupConfirmation uipopupConfirmation = (UIPopupConfirmation)this.activePopup;
		uipopupConfirmation.SetTitleText(titleStr);
		uipopupConfirmation.yesAction = yesAction;
		uipopupConfirmation.yesText.text = yesButtonText;
		uipopupConfirmation.noText.text = noButtonText;
	}

	// Token: 0x06002D1A RID: 11546 RVA: 0x00142568 File Offset: 0x00140968
	public void ShowConfirmationDialog(string messageStr, UnityAction yesAction)
	{
		this.ShowPopup(this.confirmationPopupPrefab, null);
		this.activePopup.mainText.text = messageStr;
		((UIPopupConfirmation)this.activePopup).yesAction = yesAction;
	}

	// Token: 0x06002D1B RID: 11547 RVA: 0x0014259C File Offset: 0x0014099C
	public void ShowConfirmationDialog(string messageStr, UnityAction yesAction, string yesButtonText, string noButtonText)
	{
		this.ShowPopup(this.confirmationPopupPrefab, null);
		this.activePopup.mainText.text = messageStr;
		UIPopupConfirmation uipopupConfirmation = (UIPopupConfirmation)this.activePopup;
		uipopupConfirmation.yesAction = yesAction;
		uipopupConfirmation.yesText.text = yesButtonText;
		uipopupConfirmation.noText.text = noButtonText;
	}

	// Token: 0x06002D1C RID: 11548 RVA: 0x001425F4 File Offset: 0x001409F4
	public void ShowConfirmationDialog(BWMenuTextEnum message, UnityAction yesAction)
	{
		string textString = MenuTextDefinitions.GetTextString(message);
		this.ShowConfirmationDialog(textString, yesAction);
	}

	// Token: 0x06002D1D RID: 11549 RVA: 0x00142610 File Offset: 0x00140A10
	public void ShowPopupReportWorld(string worldID)
	{
		if (this.reportWorldPopupPrefab != null)
		{
			this.ShowPopup(this.reportWorldPopupPrefab, null);
			UIReportWorldPanel component = this.activePopup.GetComponent<UIReportWorldPanel>();
			if (component != null)
			{
				component.Setup(worldID);
			}
		}
	}

	// Token: 0x06002D1E RID: 11550 RVA: 0x0014265C File Offset: 0x00140A5C
	public void ShowPopupReportModel(string modelID)
	{
		if (this.reportModelPopupPrefab != null)
		{
			this.ShowPopup(this.reportModelPopupPrefab, null);
			UIReportModelPanel component = this.activePopup.GetComponent<UIReportModelPanel>();
			if (component != null)
			{
				component.Setup(modelID);
			}
		}
	}

	// Token: 0x06002D1F RID: 11551 RVA: 0x001426A7 File Offset: 0x00140AA7
	public void ShowPopupEscapeMenu()
	{
		if (this.escapeMenuPopupPrefab != null)
		{
			this.ShowPopup(this.escapeMenuPopupPrefab, null);
		}
	}

	// Token: 0x06002D20 RID: 11552 RVA: 0x001426C8 File Offset: 0x00140AC8
	public void ShowPopupSkipPublishCooldown(int coins, UnityAction skipPublishAction)
	{
		this.ShowConfirmationDialog("Pay " + coins + " coins to publish now?", delegate()
		{
			if (BWUser.currentUser.coins < coins)
			{
				this.ShowPopupInsufficentCoins();
			}
			else
			{
				skipPublishAction();
			}
		});
	}

	// Token: 0x06002D21 RID: 11553 RVA: 0x0014271C File Offset: 0x00140B1C
	public void ShowPopupInsufficentCoins()
	{
		BWStandalone.Overlays.ShowConfirmationDialog("Not Enough Coins!", "You don't have enough coins to buy these items.\n\nWould you like to get more coins?", delegate()
		{
			MainUIController.Instance.LoadUIScene("ShopMenu");
		}, "Yes", "No");
	}

	// Token: 0x06002D22 RID: 11554 RVA: 0x0014275C File Offset: 0x00140B5C
	public void ShowPopupEditModelPrice(BWUserModel model)
	{
		if (this.editModelPricePopupPrefab != null)
		{
			this.ShowPopup(this.editModelPricePopupPrefab, this);
			UIPopupSetModelPrice uipopupSetModelPrice = (UIPopupSetModelPrice)this.activePopup;
			uipopupSetModelPrice.SetupForModel(model);
		}
	}

	// Token: 0x06002D23 RID: 11555 RVA: 0x0014279B File Offset: 0x00140B9B
	public void ShowPopupLinkToIOSAccount()
	{
		if (this.linkToIOSPopupPrefab != null)
		{
			this.ShowPopup(this.linkToIOSPopupPrefab, this);
		}
	}

	// Token: 0x06002D24 RID: 11556 RVA: 0x001427BC File Offset: 0x00140BBC
	public void ShowPopupPendingPayouts()
	{
		if (this.pendingPayoutsPopupPrefab != null)
		{
			this.ShowPopup(this.pendingPayoutsPopupPrefab, this);
		}
	}

	// Token: 0x06002D25 RID: 11557 RVA: 0x001427DD File Offset: 0x00140BDD
	public void ShowPopupWorldDetailPanel(string worldID, UIDataSource dataSource)
	{
		if (this.worldDetailPopupPrefab != null)
		{
			this.ShowPopup(this.worldDetailPopupPrefab, this);
			this.activePopup.LoadData(dataSource, ImageManager.Instance, worldID);
		}
	}

	// Token: 0x06002D26 RID: 11558 RVA: 0x00142810 File Offset: 0x00140C10
	public void ShowPopupU2UModelDetailPanel(string modelID, UIDataSource dataSource)
	{
		if (this.u2uModelDetailPanelPrefab != null)
		{
			this.ShowPopup(this.u2uModelDetailPanelPrefab, this);
			this.activePopup.LoadData(dataSource, ImageManager.Instance, modelID);
		}
	}

	// Token: 0x06002D27 RID: 11559 RVA: 0x00142843 File Offset: 0x00140C43
	public void ShowPopupUserModelDetailPanel(string localModelID, UIDataSource dataSource)
	{
		if (this.userModelDetailPanelPrefab != null)
		{
			this.ShowPopup(this.userModelDetailPanelPrefab, this);
			this.activePopup.LoadData(dataSource, ImageManager.Instance, localModelID);
		}
	}

	// Token: 0x06002D28 RID: 11560 RVA: 0x00142878 File Offset: 0x00140C78
	public UIPopup ShowPopup(UIPopup popupPrefab, IPopupDelegate popupDelegate = null)
	{
		this.popupBackground.gameObject.SetActive(true);
		if (this.activePopup != null)
		{
			UnityEngine.Object.Destroy(this.activePopup.gameObject);
		}
		else
		{
			this.selectionBeforePopup = EventSystem.current.currentSelectedGameObject;
		}
		this.activePopup = UnityEngine.Object.Instantiate<UIPopup>(popupPrefab);
		if (popupDelegate == null)
		{
			popupDelegate = this;
		}
		this.activePopup.Show(popupDelegate);
		RectTransform rectTransform = (RectTransform)this.activePopup.gameObject.transform;
		RectTransform parent = (RectTransform)this.popupBackground.transform;
		rectTransform.SetParent(parent, false);
		if (this.activePopup.defaultSelectable != null)
		{
			EventSystem.current.SetSelectedGameObject(this.activePopup.defaultSelectable.gameObject);
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(this.activePopup.gameObject);
		}
		return this.activePopup;
	}

	// Token: 0x06002D29 RID: 11561 RVA: 0x0014296C File Offset: 0x00140D6C
	public void PopupBackgroundClicked()
	{
		if (this.activePopup == null)
		{
			this.ClosePopup();
		}
		else if (this.activePopup.closeOnBackgroundClick)
		{
			this.activePopup.Hide();
		}
	}

	// Token: 0x06002D2A RID: 11562 RVA: 0x001429A5 File Offset: 0x00140DA5
	public bool IsShowingPopup()
	{
		return this.activePopup != null;
	}

	// Token: 0x06002D2B RID: 11563 RVA: 0x001429B4 File Offset: 0x00140DB4
	public void ClosePopup()
	{
		UISoundPlayer.Instance.PlayClip("DefaultButtonClick", 1f);
		if (this.activePopup != null)
		{
			UnityEngine.Object.Destroy(this.activePopup.gameObject);
		}
		this.activePopup = null;
		this.popupBackground.gameObject.SetActive(false);
		EventSystem.current.SetSelectedGameObject(this.selectionBeforePopup);
	}

	// Token: 0x06002D2C RID: 11564 RVA: 0x00142A1E File Offset: 0x00140E1E
	public void ShowLoadingOverlay(bool show)
	{
		this.loadingOverlayAnimator.SetTrigger((!show) ? "Hide" : "Show");
	}

	// Token: 0x06002D2D RID: 11565 RVA: 0x00142A40 File Offset: 0x00140E40
	public void ParentToCanvas(RectTransform rt)
	{
		rt.SetParent(this.overlayTransform, true);
		rt.localScale = Vector3.one;
	}

	// Token: 0x06002D2E RID: 11566 RVA: 0x00142A5C File Offset: 0x00140E5C
	public void DoCoinAwardAnimation(Vector3 fromPosition, UnityAction completion)
	{
		RectTransform rectTransform = UnityEngine.Object.Instantiate<RectTransform>(this.coinAnimPrefab);
		rectTransform.position = fromPosition;
		this.ParentToCanvas(rectTransform);
		MainUIController.Instance.menuBar.PutCoinsDiplayInOverlay(true);
		Vector3 position = MainUIController.Instance.menuBar.coinsAnimTarget.position;
		base.StartCoroutine(this.CoinAwardAnimCoroutine(rectTransform, position, completion));
	}

	// Token: 0x06002D2F RID: 11567 RVA: 0x00142AB8 File Offset: 0x00140EB8
	private IEnumerator CoinAwardAnimCoroutine(RectTransform coinAnimT, Vector3 targetPos, UnityAction completion)
	{
		UISoundPlayer.Instance.PlayClip("shop_purchase", 1f);
		this.ParentToCanvas(coinAnimT);
		yield return base.StartCoroutine(this.AnimateTransformCoroutine(coinAnimT, targetPos, 0.6f, 1.2f, true, null));
		BWUserDataManager.Instance.NotifyListeners();
		yield return new WaitForSeconds(1f);
		MainUIController.Instance.menuBar.PutCoinsDiplayInOverlay(false);
		if (completion != null)
		{
			completion();
		}
		yield break;
	}

	// Token: 0x06002D30 RID: 11568 RVA: 0x00142AE8 File Offset: 0x00140EE8
	public void AnimateTransform(RectTransform rt, Vector3 endPosition, float endScale, float speed, bool destroyOnCompletion, UnityAction completion)
	{
		this.ParentToCanvas(rt);
		base.StartCoroutine(this.AnimateTransformCoroutine(rt, endPosition, endScale, speed, destroyOnCompletion, completion));
	}

	// Token: 0x06002D31 RID: 11569 RVA: 0x00142B08 File Offset: 0x00140F08
	private IEnumerator AnimateTransformCoroutine(RectTransform rt, Vector3 endPosition, float endScale, float speed, bool destroyOnCompletion, UnityAction completion)
	{
		float timer = 0f;
		Vector3 startPosition = rt.position;
		Vector3 startScale = rt.localScale;
		float dist = (endPosition - startPosition).magnitude / (float)Screen.width;
		float duration = dist / speed;
		while (timer < duration)
		{
			Vector3 pos = Vector3.Lerp(startPosition, endPosition, timer / duration);
			Vector3 scale = Vector3.Lerp(startScale, endScale * Vector3.one, timer / duration);
			rt.position = pos;
			rt.localScale = scale;
			timer += Time.deltaTime;
			yield return null;
		}
		if (destroyOnCompletion)
		{
			UnityEngine.Object.Destroy(rt.gameObject);
		}
		if (completion != null)
		{
			completion();
		}
		yield break;
	}

	// Token: 0x04002596 RID: 9622
	public RectTransform overlayTransform;

	// Token: 0x04002597 RID: 9623
	public GameObject uiBusyOverlay;

	// Token: 0x04002598 RID: 9624
	public Animator loadingOverlayAnimator;

	// Token: 0x04002599 RID: 9625
	public Selectable popupBackground;

	// Token: 0x0400259A RID: 9626
	public UIPopup activePopup;

	// Token: 0x0400259B RID: 9627
	public UIPopup messagePopupPrefab;

	// Token: 0x0400259C RID: 9628
	public UIPopupConfirmation confirmationPopupPrefab;

	// Token: 0x0400259D RID: 9629
	public UIPopup escapeMenuPopupPrefab;

	// Token: 0x0400259E RID: 9630
	public UIPopup escapeMenuInWorldPopupPrefab;

	// Token: 0x0400259F RID: 9631
	public UIPopup reportWorldPopupPrefab;

	// Token: 0x040025A0 RID: 9632
	public UIPopup reportModelPopupPrefab;

	// Token: 0x040025A1 RID: 9633
	public UIPopupSetModelPrice editModelPricePopupPrefab;

	// Token: 0x040025A2 RID: 9634
	public UIPopupLinkToIOS linkToIOSPopupPrefab;

	// Token: 0x040025A3 RID: 9635
	public UIPopupPendingPayouts pendingPayoutsPopupPrefab;

	// Token: 0x040025A4 RID: 9636
	public UIPopupDetailPanel worldDetailPopupPrefab;

	// Token: 0x040025A5 RID: 9637
	public UIPopupDetailPanel u2uModelDetailPanelPrefab;

	// Token: 0x040025A6 RID: 9638
	public UIPopupDetailPanel userModelDetailPanelPrefab;

	// Token: 0x040025A7 RID: 9639
	public UINotificationFeed notifications;

	// Token: 0x040025A8 RID: 9640
	public RectTransform coinAnimPrefab;

	// Token: 0x040025A9 RID: 9641
	private GameObject selectionBeforePopup;
}
