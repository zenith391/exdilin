using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003B8 RID: 952
public class BWSocialUser
{
	// Token: 0x0600294F RID: 10575 RVA: 0x0012E956 File Offset: 0x0012CD56
	public BWSocialUser(JObject json)
	{
		this.UpdateFromJson(json);
	}

	// Token: 0x170001BE RID: 446
	// (get) Token: 0x06002950 RID: 10576 RVA: 0x0012E965 File Offset: 0x0012CD65
	// (set) Token: 0x06002951 RID: 10577 RVA: 0x0012E96D File Offset: 0x0012CD6D
	public int userID { get; private set; }

	// Token: 0x170001BF RID: 447
	// (get) Token: 0x06002952 RID: 10578 RVA: 0x0012E976 File Offset: 0x0012CD76
	// (set) Token: 0x06002953 RID: 10579 RVA: 0x0012E97E File Offset: 0x0012CD7E
	public string username { get; private set; }

	// Token: 0x170001C0 RID: 448
	// (get) Token: 0x06002954 RID: 10580 RVA: 0x0012E987 File Offset: 0x0012CD87
	// (set) Token: 0x06002955 RID: 10581 RVA: 0x0012E98F File Offset: 0x0012CD8F
	public int userStatus { get; private set; }
	// Token: 0x170001C1 RID: 449
	// (get) Token: 0x06002956 RID: 10582 RVA: 0x0012E998 File Offset: 0x0012CD98
	// (set) Token: 0x06002957 RID: 10583 RVA: 0x0012E9A0 File Offset: 0x0012CDA0
	public DateTime startedFollowingAt { get; private set; }

	// Token: 0x170001C2 RID: 450
	// (get) Token: 0x06002958 RID: 10584 RVA: 0x0012E9A9 File Offset: 0x0012CDA9
	// (set) Token: 0x06002959 RID: 10585 RVA: 0x0012E9B1 File Offset: 0x0012CDB1
	public int relationship { get; private set; }

    //
    public string profileImageUrl { get; private set; }
    //

    // Token: 0x0600295A RID: 10586 RVA: 0x0012E9BC File Offset: 0x0012CDBC
    public void UpdateFromJson(JObject json)
	{
		this.userID = BWJsonHelpers.PropertyIfExists(this.userID, "user_id", json);
		this.username = BWJsonHelpers.PropertyIfExists(this.username, "username", json);
		this.userStatus = BWJsonHelpers.PropertyIfExists(this.userStatus, "user_status", json);
		this.startedFollowingAt = BWJsonHelpers.PropertyIfExists(this.startedFollowingAt, "started_following_at", json);
        this.relationship = BWJsonHelpers.PropertyIfExists(this.relationship, "relationship", json);
        //
        this.profileImageUrl = BWJsonHelpers.PropertyIfExists(this.profileImageUrl, "profile_image_url", json);
        //
	}

	// Token: 0x0600295B RID: 10587 RVA: 0x0012EA3C File Offset: 0x0012CE3C
	public Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["id"] = this.userID.ToString();
		dictionary["username"] = ((!string.IsNullOrEmpty(this.username)) ? this.username : "Unnamed Blockster");
        //
        if (string.IsNullOrEmpty(this.profileImageUrl))
        {
            dictionary["profile_image"] = string.Format("{0}/profiles/{1}/approved.png", BWEnvConfig.AWS_S3_BASE_URL, this.userID.ToString());
        }
        else
        {
            dictionary["profile_image"] = this.profileImageUrl;
        }
        //
        dictionary["is_blocksworld_premium"] = Util.IsPremiumUserStatus(this.userStatus).ToString();
		string value = string.Empty;
		DateTime utcNow = DateTime.UtcNow;
		if (utcNow.Date == this.startedFollowingAt.Date)
		{
			value = "Since today!";
		}
		else if (utcNow.AddDays(-1.0).Date == this.startedFollowingAt.Date)
		{
			value = "Since yesterday";
		}
		else
		{
			value = "Since " + this.startedFollowingAt.ToShortDateString();
		}
		dictionary["following_since"] = value;
		return dictionary;
	}
}
