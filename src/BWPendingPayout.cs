using System.Collections.Generic;
using SimpleJSON;

public class BWPendingPayout
{
	public string payoutType;

	public string refID;

	public int coinGrants;

	public string title;

	public string msg1;

	public string msg2;

	public bool hasGoldBorder;

	public BWPendingPayout(JObject json)
	{
		payoutType = BWJsonHelpers.PropertyIfExists(payoutType, "payout_type", json);
		refID = BWJsonHelpers.IDPropertyAsStringIfExists(refID, "ref_id", json);
		coinGrants = BWJsonHelpers.PropertyIfExists(coinGrants, "coin_grants", json);
		title = BWJsonHelpers.PropertyIfExists(title, "title", json);
		msg1 = BWJsonHelpers.PropertyIfExists(msg1, "msg1", json);
		msg2 = BWJsonHelpers.PropertyIfExists(msg2, "msg2", json);
		hasGoldBorder = BWJsonHelpers.PropertyIfExists(hasGoldBorder, "has_gold_border", json);
	}

	public Dictionary<string, object> AttributesForCollection()
	{
		return new Dictionary<string, object>
		{
			{ "payout_type", payoutType },
			{ "ref_id", refID },
			{
				"coin_grants",
				coinGrants.ToString()
			}
		};
	}

	public Dictionary<string, string> AttributesForMenuUI()
	{
		return new Dictionary<string, string>
		{
			{ "title", title },
			{
				"coins",
				coinGrants.ToString()
			},
			{ "message1", msg1 },
			{ "message2", msg2 },
			{
				"hasGoldBorder",
				hasGoldBorder.ToString()
			}
		};
	}
}
