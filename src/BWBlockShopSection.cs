using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003A0 RID: 928
internal class BWBlockShopSection
{
	// Token: 0x06002868 RID: 10344 RVA: 0x0012A154 File Offset: 0x00128554
	public BWBlockShopSection(JObject json)
	{
		this.sectionTitle = json["section_title"].StringValue;
		List<JObject> arrayValue = json["block_items"].ArrayValue;
		this.blockItems = new List<BlockItem>();
		foreach (JObject jobject in arrayValue)
		{
			string stringValue = jobject.StringValue;
			if (BlockItem.Exists(stringValue))
			{
				BlockItem blockItem = BlockItem.FindByInternalIdentifier(jobject.StringValue);
				if (BWBlockItemPricing.AlaCarteIsAvailable(blockItem.Id))
				{
					this.blockItems.Add(blockItem);
				}
			}
		}
	}

	// Token: 0x04002355 RID: 9045
	public string sectionTitle;

	// Token: 0x04002356 RID: 9046
	public List<BlockItem> blockItems;
}
