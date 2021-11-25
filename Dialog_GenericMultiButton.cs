using System;
using UnityEngine;

// Token: 0x020002EA RID: 746
public class Dialog_GenericMultiButton : UIDialogPanel
{
	// Token: 0x06002204 RID: 8708 RVA: 0x000FEBE4 File Offset: 0x000FCFE4
	public override void Init()
	{
		this.mainText.Init();
		foreach (UIEditableText uieditableText in this.buttonTexts)
		{
			uieditableText.Init();
		}
		if (this.tileScrollView != null)
		{
			this.tileScrollView.Init();
		}
	}

	// Token: 0x06002205 RID: 8709 RVA: 0x000FEC3D File Offset: 0x000FD03D
	protected override void OnHide()
	{
		if (this.tileScrollView != null)
		{
			this.tileScrollView.ClearTiles();
		}
	}

	// Token: 0x06002206 RID: 8710 RVA: 0x000FEC5C File Offset: 0x000FD05C
	public void Setup(string mainTextStr, string[] buttonLabels, Action[] buttonActions)
	{
		this.mainText.Set(mainTextStr);
		this._buttonActions = buttonActions;
		if (buttonLabels.Length != buttonActions.Length)
		{
			BWLog.Error("argument mismatch");
		}
		if (buttonLabels.Length > this.buttonObjects.Length || buttonLabels.Length > this.buttonTexts.Length)
		{
			BWLog.Info("Too many buttons!");
		}
		this._buttonCount = Mathf.Min(buttonLabels.Length, this.buttonObjects.Length);
		for (int i = 0; i < this.buttonObjects.Length; i++)
		{
			if (i < this._buttonCount)
			{
				this.buttonObjects[i].SetActive(true);
				this.buttonTexts[i].Set(buttonLabels[i]);
			}
			else
			{
				this.buttonObjects[i].SetActive(false);
			}
		}
	}

	// Token: 0x06002207 RID: 8711 RVA: 0x000FED28 File Offset: 0x000FD128
	public void DidTapButton(int buttonID)
	{
		this.doCloseDialog();
		if (buttonID >= this._buttonCount)
		{
			BWLog.Error("invalid button");
			return;
		}
		Action action = this._buttonActions[buttonID];
		if (action != null)
		{
			action();
		}
	}

	// Token: 0x04001D0D RID: 7437
	public UIEditableText[] buttonTexts;

	// Token: 0x04001D0E RID: 7438
	public GameObject[] buttonObjects;

	// Token: 0x04001D0F RID: 7439
	public UIEditableText mainText;

	// Token: 0x04001D10 RID: 7440
	public UITileScrollView tileScrollView;

	// Token: 0x04001D11 RID: 7441
	private Action[] _buttonActions;

	// Token: 0x04001D12 RID: 7442
	private int _buttonCount;
}
