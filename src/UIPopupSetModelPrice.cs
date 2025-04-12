using System;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000454 RID: 1108
public class UIPopupSetModelPrice : UIPopup
{
	// Token: 0x06002F11 RID: 12049 RVA: 0x0014DC08 File Offset: 0x0014C008
	private void OnEnable()
	{
		this.decreasePriceButton.decrease = true;
		this.increasePriceButton.AddListener(new UIChangePriceButtonHandler(this.ChangePrice));
		this.decreasePriceButton.AddListener(new UIChangePriceButtonHandler(this.ChangePrice));
		this.resetButton.onClick.RemoveAllListeners();
		this.resetButton.onClick.AddListener(new UnityAction(this.Reset));
		this.doneButton.onClick.RemoveAllListeners();
		this.doneButton.onClick.AddListener(new UnityAction(this.Done));
	}

	// Token: 0x06002F12 RID: 12050 RVA: 0x0014DCA7 File Offset: 0x0014C0A7
	private void OnDisable()
	{
		this.increasePriceButton.ClearListeners();
		this.decreasePriceButton.ClearListeners();
	}

	// Token: 0x06002F13 RID: 12051 RVA: 0x0014DCBF File Offset: 0x0014C0BF
	public void SetupForModel(BWUserModel setupModel)
	{
		this.model = setupModel;
		this.UpdatePrice();
	}

	// Token: 0x06002F14 RID: 12052 RVA: 0x0014DCD0 File Offset: 0x0014C0D0
	private void ChangePrice(int change)
	{
		if (this.model == null)
		{
			return;
		}
		int coinsPriceMarkup = this.model.coinsPriceMarkup;
		BWUserModelsDataManager.Instance.ChangeModelPriceMarkup(this.model, change);
		int coinsPriceMarkup2 = this.model.coinsPriceMarkup;
		if (coinsPriceMarkup2 > coinsPriceMarkup)
		{
			UISoundPlayer.Instance.PlayClip("inc-dec02_inc", 1f);
		}
		else if (coinsPriceMarkup > coinsPriceMarkup2)
		{
			UISoundPlayer.Instance.PlayClip("inc-dec02_dec", 1f);
		}
		else if (coinsPriceMarkup == coinsPriceMarkup2)
		{
			UISoundPlayer.Instance.PlayClip("forbidden_01", 1f);
		}
		this.UpdatePrice();
	}

	// Token: 0x06002F15 RID: 12053 RVA: 0x0014DD73 File Offset: 0x0014C173
	private void Reset()
	{
		if (this.model == null)
		{
			return;
		}
		BWUserModelsDataManager.Instance.ResetModelPriceMarkup(this.model);
		this.UpdatePrice();
	}

	// Token: 0x06002F16 RID: 12054 RVA: 0x0014DD97 File Offset: 0x0014C197
	private void Done()
	{
		base.Hide();
	}

	// Token: 0x06002F17 RID: 12055 RVA: 0x0014DDA0 File Offset: 0x0014C1A0
	private void UpdatePrice()
	{
		int num = this.model.CoinsBasePrice();
		int coinsPriceMarkup = this.model.coinsPriceMarkup;
		int num2 = num + coinsPriceMarkup;
		this.blockPriceText.text = num.ToString();
		this.profitText.text = coinsPriceMarkup.ToString();
		this.priceText.text = num2.ToString();
	}

	// Token: 0x04002778 RID: 10104
	public Text priceText;

	// Token: 0x04002779 RID: 10105
	public Text blockPriceText;

	// Token: 0x0400277A RID: 10106
	public Text profitText;

	// Token: 0x0400277B RID: 10107
	public UIChangePriceButton increasePriceButton;

	// Token: 0x0400277C RID: 10108
	public UIChangePriceButton decreasePriceButton;

	// Token: 0x0400277D RID: 10109
	public Button resetButton;

	// Token: 0x0400277E RID: 10110
	public Button doneButton;

	// Token: 0x0400277F RID: 10111
	private BWUserModel model;

	// Token: 0x04002780 RID: 10112
	private int modelResetMarkup;
}
