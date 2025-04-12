using System;
using UnityEngine;

// Token: 0x0200044F RID: 1103
public class UIPopupDetailPanel : UIPopup, DetailPanelDelegate
{
	// Token: 0x06002EF9 RID: 12025 RVA: 0x0014D3F8 File Offset: 0x0014B7F8
	public override void LoadData(UIDataSource dataSource, ImageManager imageManager, string itemID)
	{
		dataSource.LoadIfNeeded();
		base.LoadData(dataSource, imageManager, itemID);
		this.detailPanel = UnityEngine.Object.Instantiate<DetailPanel>(this.detailPanelPrefab);
		this.detailPanel.Init();
		RectTransform rectTransform = (RectTransform)this.detailPanel.transform;
		rectTransform.SetParent(this.detailPanelParent, false);
		this.detailPanel.LoadContentForID(itemID, dataSource, imageManager);
		this.detailPanel.detailPanelDelegate = this;
		this.detailPanel.Show(true);
	}

	// Token: 0x06002EFA RID: 12026 RVA: 0x0014D474 File Offset: 0x0014B874
	public void DetailPanelClosed(DetailPanel detailPanel)
	{
		if (detailPanel == this.detailPanel)
		{
			base.Hide();
		}
	}

	// Token: 0x04002757 RID: 10071
	public DetailPanel detailPanelPrefab;

	// Token: 0x04002758 RID: 10072
	public RectTransform detailPanelParent;

	// Token: 0x04002759 RID: 10073
	private DetailPanel detailPanel;
}
