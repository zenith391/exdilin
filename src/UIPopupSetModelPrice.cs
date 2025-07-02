using UnityEngine.UI;

public class UIPopupSetModelPrice : UIPopup
{
	public Text priceText;

	public Text blockPriceText;

	public Text profitText;

	public UIChangePriceButton increasePriceButton;

	public UIChangePriceButton decreasePriceButton;

	public Button resetButton;

	public Button doneButton;

	private BWUserModel model;

	private int modelResetMarkup;

	private void OnEnable()
	{
		decreasePriceButton.decrease = true;
		increasePriceButton.AddListener(ChangePrice);
		decreasePriceButton.AddListener(ChangePrice);
		resetButton.onClick.RemoveAllListeners();
		resetButton.onClick.AddListener(Reset);
		doneButton.onClick.RemoveAllListeners();
		doneButton.onClick.AddListener(Done);
	}

	private void OnDisable()
	{
		increasePriceButton.ClearListeners();
		decreasePriceButton.ClearListeners();
	}

	public void SetupForModel(BWUserModel setupModel)
	{
		model = setupModel;
		UpdatePrice();
	}

	private void ChangePrice(int change)
	{
		if (model != null)
		{
			int coinsPriceMarkup = model.coinsPriceMarkup;
			BWUserModelsDataManager.Instance.ChangeModelPriceMarkup(model, change);
			int coinsPriceMarkup2 = model.coinsPriceMarkup;
			if (coinsPriceMarkup2 > coinsPriceMarkup)
			{
				UISoundPlayer.Instance.PlayClip("inc-dec02_inc");
			}
			else if (coinsPriceMarkup > coinsPriceMarkup2)
			{
				UISoundPlayer.Instance.PlayClip("inc-dec02_dec");
			}
			else if (coinsPriceMarkup == coinsPriceMarkup2)
			{
				UISoundPlayer.Instance.PlayClip("forbidden_01");
			}
			UpdatePrice();
		}
	}

	private void Reset()
	{
		if (model != null)
		{
			BWUserModelsDataManager.Instance.ResetModelPriceMarkup(model);
			UpdatePrice();
		}
	}

	private void Done()
	{
		Hide();
	}

	private void UpdatePrice()
	{
		int num = model.CoinsBasePrice();
		int coinsPriceMarkup = model.coinsPriceMarkup;
		int num2 = num + coinsPriceMarkup;
		blockPriceText.text = num.ToString();
		profitText.text = coinsPriceMarkup.ToString();
		priceText.text = num2.ToString();
	}
}
