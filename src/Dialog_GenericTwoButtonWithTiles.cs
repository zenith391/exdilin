using System;
using UnityEngine;

// Token: 0x020002EC RID: 748
public class Dialog_GenericTwoButtonWithTiles : UIDialogPanel
{
	// Token: 0x0600220E RID: 8718 RVA: 0x000FEE1B File Offset: 0x000FD21B
	public override void Init()
	{
		this.mainText.Init();
		this.buttonAText.Init();
		this.buttonBText.Init();
		this.tileScrollView.Init();
	}

	// Token: 0x0600220F RID: 8719 RVA: 0x000FEE49 File Offset: 0x000FD249
	protected override void OnHide()
	{
		this.tileScrollView.ClearTiles();
	}

	// Token: 0x06002210 RID: 8720 RVA: 0x000FEE58 File Offset: 0x000FD258
	public void Setup(string mainTextStr, string buttonATextStr, string buttonBTextStr, Action buttonAAction, Action buttonBAction)
	{
		this.mainText.Set(mainTextStr);
		this.buttonA.SetActive(true);
		this.buttonAText.Set(buttonATextStr);
		this.buttonBText.Set(buttonBTextStr);
		this.onButtonA = buttonAAction;
		this.onButtonB = buttonBAction;
	}

	// Token: 0x06002211 RID: 8721 RVA: 0x000FEEA5 File Offset: 0x000FD2A5
	public void Setup(string mainTextStr, string buttonTextStr, Action buttonAction)
	{
		this.mainText.Set(mainTextStr);
		this.buttonA.SetActive(false);
		this.buttonBText.Set(buttonTextStr);
		this.onButtonB = buttonAction;
		this.onButtonA = null;
	}

	// Token: 0x06002212 RID: 8722 RVA: 0x000FEED9 File Offset: 0x000FD2D9
	public void DidTapButtonA()
	{
		this.doCloseDialog();
		if (this.onButtonA != null)
		{
			this.onButtonA();
		}
	}

	// Token: 0x06002213 RID: 8723 RVA: 0x000FEEFC File Offset: 0x000FD2FC
	public void DidTapButtonB()
	{
		this.doCloseDialog();
		if (this.onButtonB != null)
		{
			this.onButtonB();
		}
	}

	// Token: 0x04001D18 RID: 7448
	public Action onButtonA;

	// Token: 0x04001D19 RID: 7449
	public Action onButtonB;

	// Token: 0x04001D1A RID: 7450
	public UIEditableText mainText;

	// Token: 0x04001D1B RID: 7451
	public UIEditableText buttonAText;

	// Token: 0x04001D1C RID: 7452
	public UIEditableText buttonBText;

	// Token: 0x04001D1D RID: 7453
	public UITileScrollView tileScrollView;

	// Token: 0x04001D1E RID: 7454
	public GameObject buttonA;

	// Token: 0x04001D1F RID: 7455
	public GameObject buttonB;
}
