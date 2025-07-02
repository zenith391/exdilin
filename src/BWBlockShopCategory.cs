using System.Collections.Generic;
using SimpleJSON;

internal class BWBlockShopCategory
{
	public string internalIdentifier;

	public string title;

	public List<BWBlockShopSection> sections;

	public BWBlockShopCategory(JObject json)
	{
		internalIdentifier = json["internal_identifier"].StringValue;
		title = json["title"].StringValue;
		sections = new List<BWBlockShopSection>();
		List<JObject> arrayValue = json["sections"].ArrayValue;
		foreach (JObject item in arrayValue)
		{
			sections.Add(new BWBlockShopSection(item));
		}
	}
}
