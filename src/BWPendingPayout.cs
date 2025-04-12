using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003AD RID: 941
public class BWPendingPayout
{
	// Token: 0x060028F1 RID: 10481 RVA: 0x0012CBEC File Offset: 0x0012AFEC
	public BWPendingPayout(JObject json)
	{
		this.payoutType = BWJsonHelpers.PropertyIfExists(this.payoutType, "payout_type", json);
		this.refID = BWJsonHelpers.IDPropertyAsStringIfExists(this.refID, "ref_id", json);
		this.coinGrants = BWJsonHelpers.PropertyIfExists(this.coinGrants, "coin_grants", json);
		this.title = BWJsonHelpers.PropertyIfExists(this.title, "title", json);
		this.msg1 = BWJsonHelpers.PropertyIfExists(this.msg1, "msg1", json);
		this.msg2 = BWJsonHelpers.PropertyIfExists(this.msg2, "msg2", json);
		this.hasGoldBorder = BWJsonHelpers.PropertyIfExists(this.hasGoldBorder, "has_gold_border", json);
	}

	// Token: 0x060028F2 RID: 10482 RVA: 0x0012CCA0 File Offset: 0x0012B0A0
	public Dictionary<string, object> AttributesForCollection()
	{
		return new Dictionary<string, object>
		{
			{
				"payout_type",
				this.payoutType
			},
			{
				"ref_id",
				this.refID
			},
			{
				"coin_grants",
				this.coinGrants.ToString()
			}
		};
	}

	// Token: 0x060028F3 RID: 10483 RVA: 0x0012CCF4 File Offset: 0x0012B0F4
	public Dictionary<string, string> AttributesForMenuUI()
	{
		return new Dictionary<string, string>
		{
			{
				"title",
				this.title
			},
			{
				"coins",
				this.coinGrants.ToString()
			},
			{
				"message1",
				this.msg1
			},
			{
				"message2",
				this.msg2
			},
			{
				"hasGoldBorder",
				this.hasGoldBorder.ToString()
			}
		};
	}

	// Token: 0x04002399 RID: 9113
	public string payoutType;

	// Token: 0x0400239A RID: 9114
	public string refID;

	// Token: 0x0400239B RID: 9115
	public int coinGrants;

	// Token: 0x0400239C RID: 9116
	public string title;

	// Token: 0x0400239D RID: 9117
	public string msg1;

	// Token: 0x0400239E RID: 9118
	public string msg2;

	// Token: 0x0400239F RID: 9119
	public bool hasGoldBorder;
}
