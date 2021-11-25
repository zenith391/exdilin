using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003C6 RID: 966
public class BWUserActivityModel : BWUserActivity
{
	// Token: 0x06002A5D RID: 10845 RVA: 0x00134B2C File Offset: 0x00132F2C
	public BWUserActivityModel(JObject json) : base(json)
	{
		this.iconUrl = BWJsonHelpers.PropertyIfExists(this.iconUrl, "model_icon_urls_for_sizes", "128x128", json);
		this.modelId = BWJsonHelpers.PropertyIfExists(this.modelId, "model_id", json);
		this.modelPreviewTerrain = BWJsonHelpers.PropertyIfExists(this.modelPreviewTerrain, "model_preview_terrain", json);
		this.modelSalesCount = BWJsonHelpers.PropertyIfExists(this.modelSalesCount, "model_sales_count", json);
		this.modelTitle = BWJsonHelpers.PropertyIfExists(this.modelTitle, "model_title", json);
	}

	// Token: 0x06002A5E RID: 10846 RVA: 0x00134BB8 File Offset: 0x00132FB8
	public override Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = base.AttributesForMenuUI();
		if (this.modelSalesCount > 0)
		{
			dictionary["description"] = "Sold " + this.modelSalesCount + "!";
		}
		else if (!string.IsNullOrEmpty(this.modelTitle))
		{
			dictionary["description"] = this.modelTitle;
		}
		dictionary["model_id"] = this.modelId.ToString();
		dictionary["title"] = this.modelTitle;
		dictionary["image_url"] = this.iconUrl;
		dictionary["preview_terrain"] = this.modelPreviewTerrain;
		dictionary["sales_count"] = this.modelSalesCount.ToString();
		dictionary["button_message"] = "ShowU2UModelDetail";
		dictionary["message_id"] = this.modelId.ToString();
		return dictionary;
	}

	// Token: 0x04002462 RID: 9314
	public string iconUrl;

	// Token: 0x04002463 RID: 9315
	public int modelId;

	// Token: 0x04002464 RID: 9316
	public string modelPreviewTerrain;

	// Token: 0x04002465 RID: 9317
	public int modelSalesCount;

	// Token: 0x04002466 RID: 9318
	public string modelTitle;
}
