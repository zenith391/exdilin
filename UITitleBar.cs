using System;
using UnityEngine;

// Token: 0x02000323 RID: 803
public class UITitleBar : MonoBehaviour
{
	// Token: 0x06002467 RID: 9319 RVA: 0x0010A98C File Offset: 0x00108D8C
	public void Init()
	{
		this._rectTransform = (RectTransform)base.transform.GetChild(0);
		this.title.Init();
		this.subtitle.Init();
		this.coinBalanceValueText.Init();
		ViewportWatchdog.AddListener(new ViewportWatchdog.ViewportSizeChangedAction(this.ViewportSizeDidChange));
	}

	// Token: 0x06002468 RID: 9320 RVA: 0x0010A9E2 File Offset: 0x00108DE2
	public void Show()
	{
		base.gameObject.SetActive(true);
		this.Layout();
	}

	// Token: 0x06002469 RID: 9321 RVA: 0x0010A9F6 File Offset: 0x00108DF6
	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x0600246A RID: 9322 RVA: 0x0010AA04 File Offset: 0x00108E04
	public void SetTitleText(string text)
	{
		this.title.Set(text);
	}

	// Token: 0x0600246B RID: 9323 RVA: 0x0010AA12 File Offset: 0x00108E12
	public void SetSubtitleText(string text)
	{
		this.subtitle.Set(text);
	}

	// Token: 0x0600246C RID: 9324 RVA: 0x0010AA20 File Offset: 0x00108E20
	public void SetSubtext(string str)
	{
		this.SetSubtitleText(str);
		if (string.IsNullOrEmpty(str))
		{
			this.HideSubtitle();
		}
		else
		{
			this.ShowSubtitle();
		}
	}

	// Token: 0x0600246D RID: 9325 RVA: 0x0010AA45 File Offset: 0x00108E45
	public void ShowTitle()
	{
		this.title.Show();
	}

	// Token: 0x0600246E RID: 9326 RVA: 0x0010AA52 File Offset: 0x00108E52
	public void HideTitle()
	{
		this.title.Hide();
	}

	// Token: 0x0600246F RID: 9327 RVA: 0x0010AA5F File Offset: 0x00108E5F
	public void ShowSubtitle()
	{
		this.subtitle.Show();
	}

	// Token: 0x06002470 RID: 9328 RVA: 0x0010AA6C File Offset: 0x00108E6C
	public void HideSubtitle()
	{
		this.subtitle.Hide();
	}

	// Token: 0x06002471 RID: 9329 RVA: 0x0010AA79 File Offset: 0x00108E79
	public void ShowCoinBalance()
	{
		this.coinBalance.SetActive(true);
	}

	// Token: 0x06002472 RID: 9330 RVA: 0x0010AA87 File Offset: 0x00108E87
	public void HideCoinBalance()
	{
		this.coinBalance.SetActive(false);
	}

	// Token: 0x06002473 RID: 9331 RVA: 0x0010AA98 File Offset: 0x00108E98
	public void SetCoinBalance(int coins)
	{
		string str = coins.ToString("N0");
		this.coinBalanceValueText.Set(str);
	}

	// Token: 0x17000173 RID: 371
	// (get) Token: 0x06002474 RID: 9332 RVA: 0x0010AAC0 File Offset: 0x00108EC0
	public float Height
	{
		get
		{
			return (!base.gameObject.activeInHierarchy) ? 0f : this._rectTransform.sizeDelta.y;
		}
	}

	// Token: 0x06002475 RID: 9333 RVA: 0x0010AAFA File Offset: 0x00108EFA
	private void Layout()
	{
	}

	// Token: 0x06002476 RID: 9334 RVA: 0x0010AAFC File Offset: 0x00108EFC
	private void ViewportSizeDidChange()
	{
		if (base.gameObject.activeInHierarchy)
		{
			this.Layout();
		}
	}

	// Token: 0x04001F54 RID: 8020
	public UIEditableText title;

	// Token: 0x04001F55 RID: 8021
	public UIEditableText subtitle;

	// Token: 0x04001F56 RID: 8022
	public GameObject coinBalance;

	// Token: 0x04001F57 RID: 8023
	public UIEditableText coinBalanceValueText;

	// Token: 0x04001F58 RID: 8024
	private RectTransform _rectTransform;
}
