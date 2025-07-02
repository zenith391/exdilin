using UnityEngine;
using UnityEngine.UI;

public class Dialog_ModelPurchaseConfirmation : UIDialogPanel
{
	public UIEditableText mainText;

	public UIEditableText priceText;

	public RawImage previewImage;

	public override void Init()
	{
		mainText.Init();
		priceText.Init();
	}

	protected override void OnShow()
	{
	}

	protected override void OnHide()
	{
		previewImage.texture = null;
	}

	public void SetPrice(int price)
	{
		priceText.Set(price.ToString());
	}

	public void SetImage(Texture2D image)
	{
		if (!(image == null))
		{
			previewImage.texture = image;
			previewImage.enabled = true;
			BWLog.Info("Showing model preview image");
		}
	}

	public void DidTapPurchaseButton()
	{
		Blocksworld.UI.HideAll();
		doCloseDialog();
		WorldSession.platformDelegate.PurchaseCurrentlyLoadedModel();
	}
}
