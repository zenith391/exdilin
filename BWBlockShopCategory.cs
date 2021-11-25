using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x0200039F RID: 927
internal class BWBlockShopCategory
{
	// Token: 0x06002867 RID: 10343 RVA: 0x0012A0A4 File Offset: 0x001284A4
	public BWBlockShopCategory(JObject json)
	{
		this.internalIdentifier = json["internal_identifier"].StringValue;
		this.title = json["title"].StringValue;
		this.sections = new List<BWBlockShopSection>();
		List<JObject> arrayValue = json["sections"].ArrayValue;
		foreach (JObject json2 in arrayValue)
		{
			this.sections.Add(new BWBlockShopSection(json2));
		}
	}

	// Token: 0x04002352 RID: 9042
	public string internalIdentifier;

	// Token: 0x04002353 RID: 9043
	public string title;

	// Token: 0x04002354 RID: 9044
	public List<BWBlockShopSection> sections;
}
