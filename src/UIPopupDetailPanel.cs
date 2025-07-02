using UnityEngine;

public class UIPopupDetailPanel : UIPopup, DetailPanelDelegate
{
	public DetailPanel detailPanelPrefab;

	public RectTransform detailPanelParent;

	private DetailPanel detailPanel;

	public override void LoadData(UIDataSource dataSource, ImageManager imageManager, string itemID)
	{
		dataSource.LoadIfNeeded();
		base.LoadData(dataSource, imageManager, itemID);
		detailPanel = Object.Instantiate(detailPanelPrefab);
		detailPanel.Init();
		RectTransform rectTransform = (RectTransform)detailPanel.transform;
		rectTransform.SetParent(detailPanelParent, worldPositionStays: false);
		detailPanel.LoadContentForID(itemID, dataSource, imageManager);
		detailPanel.detailPanelDelegate = this;
		detailPanel.Show(immediate: true);
	}

	public void DetailPanelClosed(DetailPanel detailPanel)
	{
		if (detailPanel == this.detailPanel)
		{
			Hide();
		}
	}
}
