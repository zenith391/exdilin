using System.Collections.Generic;
using SimpleJSON;

public class BWUserActivityModel : BWUserActivity
{
	public string iconUrl;

	public int modelId;

	public string modelPreviewTerrain;

	public int modelSalesCount;

	public string modelTitle;

	public BWUserActivityModel(JObject json)
		: base(json)
	{
		iconUrl = BWJsonHelpers.PropertyIfExists(iconUrl, "model_icon_urls_for_sizes", "128x128", json);
		modelId = BWJsonHelpers.PropertyIfExists(modelId, "model_id", json);
		modelPreviewTerrain = BWJsonHelpers.PropertyIfExists(modelPreviewTerrain, "model_preview_terrain", json);
		modelSalesCount = BWJsonHelpers.PropertyIfExists(modelSalesCount, "model_sales_count", json);
		modelTitle = BWJsonHelpers.PropertyIfExists(modelTitle, "model_title", json);
	}

	public override Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = base.AttributesForMenuUI();
		if (modelSalesCount > 0)
		{
			dictionary["description"] = "Sold " + modelSalesCount + "!";
		}
		else if (!string.IsNullOrEmpty(modelTitle))
		{
			dictionary["description"] = modelTitle;
		}
		dictionary["model_id"] = modelId.ToString();
		dictionary["title"] = modelTitle;
		dictionary["image_url"] = iconUrl;
		dictionary["preview_terrain"] = modelPreviewTerrain;
		dictionary["sales_count"] = modelSalesCount.ToString();
		dictionary["button_message"] = "ShowU2UModelDetail";
		dictionary["message_id"] = modelId.ToString();
		return dictionary;
	}
}
