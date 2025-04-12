using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003AE RID: 942
public class BWProfileWorld : BWWorld
{
	// Token: 0x060028F4 RID: 10484 RVA: 0x0012CD75 File Offset: 0x0012B175
	public BWProfileWorld(JObject jsonObj) : base(jsonObj)
	{
	}

	// Token: 0x060028F5 RID: 10485 RVA: 0x0012CD80 File Offset: 0x0012B180
	public BWProfileWorld(BWUser user)
	{
		base.title = user.username;
		base.worldID = "profile_" + user.userID;
		base.authorID = user.userID;
		base.authorUsername = user.username;
		this.profileGender = "unknown";
		base.source = Resources.Load<TextAsset>(Blocksworld.DefaultProfileWorldAssetPath()).text;
	}

	// Token: 0x170001B8 RID: 440
	// (get) Token: 0x060028F6 RID: 10486 RVA: 0x0012CDF2 File Offset: 0x0012B1F2
	// (set) Token: 0x060028F7 RID: 10487 RVA: 0x0012CDFA File Offset: 0x0012B1FA
	public string avatarSourceJsonStr { get; private set; }

	// Token: 0x170001B9 RID: 441
	// (get) Token: 0x060028F8 RID: 10488 RVA: 0x0012CE03 File Offset: 0x0012B203
	// (set) Token: 0x060028F9 RID: 10489 RVA: 0x0012CE0B File Offset: 0x0012B20B
	public string imageUrl { get; private set; }

	// Token: 0x170001BA RID: 442
	// (get) Token: 0x060028FA RID: 10490 RVA: 0x0012CE14 File Offset: 0x0012B214
	// (set) Token: 0x060028FB RID: 10491 RVA: 0x0012CE1C File Offset: 0x0012B21C
	public string profileGender { get; private set; }

	// Token: 0x170001BB RID: 443
	// (get) Token: 0x060028FC RID: 10492 RVA: 0x0012CE25 File Offset: 0x0012B225
	// (set) Token: 0x060028FD RID: 10493 RVA: 0x0012CE2D File Offset: 0x0012B22D
	public new DateTime updatedAt { get; private set; }

	// Token: 0x060028FE RID: 10494 RVA: 0x0012CE38 File Offset: 0x0012B238
	internal override void UpdateFromJson(JObject json)
	{
		base.UpdateFromJson(json);
		this.avatarSourceJsonStr = BWJsonHelpers.PropertyIfExists(this.avatarSourceJsonStr, "avatar_source_json_str", json);
		this.imageUrl = BWJsonHelpers.PropertyIfExists(this.imageUrl, "image_url", json);
		this.profileGender = BWJsonHelpers.PropertyIfExists(this.profileGender, "profile_gender", json);
		this.updatedAt = BWJsonHelpers.PropertyIfExists(this.updatedAt, "updated_at_timestamp", json);
		if (string.IsNullOrEmpty(this.profileGender))
		{
			this.profileGender = "unknown";
		}
	}

	// Token: 0x060028FF RID: 10495 RVA: 0x0012CEC3 File Offset: 0x0012B2C3
	internal void UpdateFromWorldSave(string worldSource, string avatarSource, string profileGender)
	{
		base.source = worldSource;
		this.avatarSourceJsonStr = avatarSource;
		this.profileGender = profileGender;
	}

	// Token: 0x06002900 RID: 10496 RVA: 0x0012CEDC File Offset: 0x0012B2DC
	public Dictionary<string, string> AttrsToSave()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["author_id"] = base.authorID.ToString();
		if (base.source != null)
		{
			dictionary["source_json_str"] = base.source;
		}
		if (this.avatarSourceJsonStr != null)
		{
			dictionary["avatar_source_json_str"] = this.avatarSourceJsonStr;
		}
		if (this.imageUrl != null)
		{
			dictionary["image_url"] = this.imageUrl;
		}
		if (this.profileGender != null)
		{
			dictionary["profile_gender"] = this.profileGender;
		}
		return dictionary;
	}

	// Token: 0x06002901 RID: 10497 RVA: 0x0012CF80 File Offset: 0x0012B380
	public Dictionary<string, string> AttrsToSaveRemote()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (base.source != null)
		{
			dictionary["source_json_str"] = base.source;
		}
		if (this.avatarSourceJsonStr != null)
		{
			dictionary["avatar_source_json_str"] = this.avatarSourceJsonStr;
		}
		if (this.profileGender != null)
		{
			dictionary["profile_gender"] = this.profileGender;
		}
		return dictionary;
	}
}
