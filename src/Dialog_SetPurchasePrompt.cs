using System.Collections.Generic;
using Blocks;

public class Dialog_SetPurchasePrompt : UIDialogPanel
{
	public UIEditableText captionText;

	public UIEditableText mainText;

	public UIEditableText priceText;

	public UITileScrollView tileScrollView;

	private int _setId;

	public override void Init()
	{
		mainText.Init();
		captionText.Init();
		priceText.Init();
		tileScrollView.Init();
	}

	protected override void OnHide()
	{
		tileScrollView.ClearTiles();
	}

	public void Setup(string dialogCaptionStr, string setTitleStr, int setId, int setPrice)
	{
		_setId = setId;
		captionText.Set(dialogCaptionStr);
		mainText.ReplacePlaceholder(setTitleStr, "building set title");
		priceText.Set(setPrice.ToString());
	}

	public void SetupRewardTiles(Dictionary<GAF, int> rewards)
	{
		tileScrollView.ClearTiles();
		foreach (KeyValuePair<GAF, int> reward in rewards)
		{
			GAF key = reward.Key;
			if (key.Predicate == Block.predicateCreateModel)
			{
				string stringArgSafe = Util.GetStringArgSafe(key.Args, 0, string.Empty);
				if (RewardVisualization.GetIconForModel(stringArgSafe, out var icon))
				{
					tileScrollView.AddTileFromTexture(icon);
				}
				else
				{
					BWLog.Info("No icon for model: " + stringArgSafe);
				}
			}
			else
			{
				tileScrollView.AddTilesFromGAF(key, reward.Value);
			}
		}
	}

	public void DidTapPurchase()
	{
		doCloseDialog();
		WorldSession.current.PurchaseBuildingSet(_setId);
	}

	public void DidTapQuit()
	{
		doCloseDialog();
		WorldSession.Quit();
	}
}
