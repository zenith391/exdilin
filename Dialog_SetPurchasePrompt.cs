using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020002F0 RID: 752
public class Dialog_SetPurchasePrompt : UIDialogPanel
{
	// Token: 0x06002220 RID: 8736 RVA: 0x000FF2EF File Offset: 0x000FD6EF
	public override void Init()
	{
		this.mainText.Init();
		this.captionText.Init();
		this.priceText.Init();
		this.tileScrollView.Init();
	}

	// Token: 0x06002221 RID: 8737 RVA: 0x000FF31D File Offset: 0x000FD71D
	protected override void OnHide()
	{
		this.tileScrollView.ClearTiles();
	}

	// Token: 0x06002222 RID: 8738 RVA: 0x000FF32A File Offset: 0x000FD72A
	public void Setup(string dialogCaptionStr, string setTitleStr, int setId, int setPrice)
	{
		this._setId = setId;
		this.captionText.Set(dialogCaptionStr);
		this.mainText.ReplacePlaceholder(setTitleStr, "building set title");
		this.priceText.Set(setPrice.ToString());
	}

	// Token: 0x06002223 RID: 8739 RVA: 0x000FF368 File Offset: 0x000FD768
	public void SetupRewardTiles(Dictionary<GAF, int> rewards)
	{
		this.tileScrollView.ClearTiles();
		foreach (KeyValuePair<GAF, int> keyValuePair in rewards)
		{
			GAF key = keyValuePair.Key;
			if (key.Predicate == Block.predicateCreateModel)
			{
				string stringArgSafe = Util.GetStringArgSafe(key.Args, 0, string.Empty);
				Texture2D texture;
				if (RewardVisualization.GetIconForModel(stringArgSafe, out texture))
				{
					this.tileScrollView.AddTileFromTexture(texture);
				}
				else
				{
					BWLog.Info("No icon for model: " + stringArgSafe);
				}
			}
			else
			{
				this.tileScrollView.AddTilesFromGAF(key, keyValuePair.Value);
			}
		}
	}

	// Token: 0x06002224 RID: 8740 RVA: 0x000FF434 File Offset: 0x000FD834
	public void DidTapPurchase()
	{
		this.doCloseDialog();
		WorldSession.current.PurchaseBuildingSet(this._setId);
	}

	// Token: 0x06002225 RID: 8741 RVA: 0x000FF451 File Offset: 0x000FD851
	public void DidTapQuit()
	{
		this.doCloseDialog();
		WorldSession.Quit();
	}

	// Token: 0x04001D25 RID: 7461
	public UIEditableText captionText;

	// Token: 0x04001D26 RID: 7462
	public UIEditableText mainText;

	// Token: 0x04001D27 RID: 7463
	public UIEditableText priceText;

	// Token: 0x04001D28 RID: 7464
	public UITileScrollView tileScrollView;

	// Token: 0x04001D29 RID: 7465
	private int _setId;
}
