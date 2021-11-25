using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020002EE RID: 750
public class Dialog_ModelPurchaseConfirmation : UIDialogPanel
{
	// Token: 0x06002217 RID: 8727 RVA: 0x000FEF5B File Offset: 0x000FD35B
	public override void Init()
	{
		this.mainText.Init();
		this.priceText.Init();
	}

	// Token: 0x06002218 RID: 8728 RVA: 0x000FEF73 File Offset: 0x000FD373
	protected override void OnShow()
	{
	}

	// Token: 0x06002219 RID: 8729 RVA: 0x000FEF75 File Offset: 0x000FD375
	protected override void OnHide()
	{
		this.previewImage.texture = null;
	}

	// Token: 0x0600221A RID: 8730 RVA: 0x000FEF83 File Offset: 0x000FD383
	public void SetPrice(int price)
	{
		this.priceText.Set(price.ToString());
	}

	// Token: 0x0600221B RID: 8731 RVA: 0x000FEF9D File Offset: 0x000FD39D
	public void SetImage(Texture2D image)
	{
		if (image == null)
		{
			return;
		}
		this.previewImage.texture = image;
		this.previewImage.enabled = true;
		BWLog.Info("Showing model preview image");
	}

	// Token: 0x0600221C RID: 8732 RVA: 0x000FEFCE File Offset: 0x000FD3CE
	public void DidTapPurchaseButton()
	{
		Blocksworld.UI.HideAll();
		this.doCloseDialog();
		WorldSession.platformDelegate.PurchaseCurrentlyLoadedModel();
	}

	// Token: 0x04001D21 RID: 7457
	public UIEditableText mainText;

	// Token: 0x04001D22 RID: 7458
	public UIEditableText priceText;

	// Token: 0x04001D23 RID: 7459
	public RawImage previewImage;
}
