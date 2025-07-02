using System.Collections.Generic;
using SimpleJSON;

internal class BWBlockShopSection
{
	public string sectionTitle;

	public List<BlockItem> blockItems;

	public BWBlockShopSection(JObject json)
	{
		sectionTitle = json["section_title"].StringValue;
		List<JObject> arrayValue = json["block_items"].ArrayValue;
		blockItems = new List<BlockItem>();
		foreach (JObject item in arrayValue)
		{
			string stringValue = item.StringValue;
			if (BlockItem.Exists(stringValue))
			{
				BlockItem blockItem = BlockItem.FindByInternalIdentifier(item.StringValue);
				if (BWBlockItemPricing.AlaCarteIsAvailable(blockItem.Id))
				{
					blockItems.Add(blockItem);
				}
			}
		}
	}
}
