using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003C7 RID: 967
public class BWUserActivityProfile : BWUserActivity
{
	// Token: 0x06002A5F RID: 10847 RVA: 0x00134CBC File Offset: 0x001330BC
	public BWUserActivityProfile(JObject json) : base(json)
	{
		this.userId = BWJsonHelpers.PropertyIfExists(this.userId, "user_id", json);
		this.userStatus = BWJsonHelpers.PropertyIfExists(this.userStatus, "user_status", json);
		this.profileImageUrl = string.Format("{0}/profiles/{1}/approved.png", BWEnvConfig.AWS_S3_BASE_URL, this.userId);
	}

	// Token: 0x06002A60 RID: 10848 RVA: 0x00134D20 File Offset: 0x00133120
	public override Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = base.AttributesForMenuUI();
		dictionary["user_id"] = this.userId.ToString();
		dictionary["user_status"] = this.userStatus.ToString();
		dictionary["image_url"] = this.profileImageUrl;
		return dictionary;
	}

	// Token: 0x04002467 RID: 9319
	public int userId;

	// Token: 0x04002468 RID: 9320
	public int userStatus;

	// Token: 0x04002469 RID: 9321
	public string profileImageUrl;
}
