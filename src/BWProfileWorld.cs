using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class BWProfileWorld : BWWorld
{
	public string avatarSourceJsonStr { get; private set; }

	public string imageUrl { get; private set; }

	public string profileGender { get; private set; }

	public new DateTime updatedAt { get; private set; }

	public BWProfileWorld(JObject jsonObj)
		: base(jsonObj)
	{
	}

	public BWProfileWorld(BWUser user)
	{
		base.title = user.username;
		base.worldID = "profile_" + user.userID;
		base.authorID = user.userID;
		base.authorUsername = user.username;
		profileGender = "unknown";
		base.source = Resources.Load<TextAsset>(Blocksworld.DefaultProfileWorldAssetPath()).text;
	}

	internal override void UpdateFromJson(JObject json)
	{
		base.UpdateFromJson(json);
		avatarSourceJsonStr = BWJsonHelpers.PropertyIfExists(avatarSourceJsonStr, "avatar_source_json_str", json);
		imageUrl = BWJsonHelpers.PropertyIfExists(imageUrl, "image_url", json);
		profileGender = BWJsonHelpers.PropertyIfExists(profileGender, "profile_gender", json);
		updatedAt = BWJsonHelpers.PropertyIfExists(updatedAt, "updated_at_timestamp", json);
		if (string.IsNullOrEmpty(profileGender))
		{
			profileGender = "unknown";
		}
	}

	internal void UpdateFromWorldSave(string worldSource, string avatarSource, string profileGender)
	{
		base.source = worldSource;
		avatarSourceJsonStr = avatarSource;
		this.profileGender = profileGender;
	}

	public Dictionary<string, string> AttrsToSave()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["author_id"] = base.authorID.ToString();
		if (base.source != null)
		{
			dictionary["source_json_str"] = base.source;
		}
		if (avatarSourceJsonStr != null)
		{
			dictionary["avatar_source_json_str"] = avatarSourceJsonStr;
		}
		if (imageUrl != null)
		{
			dictionary["image_url"] = imageUrl;
		}
		if (profileGender != null)
		{
			dictionary["profile_gender"] = profileGender;
		}
		return dictionary;
	}

	public Dictionary<string, string> AttrsToSaveRemote()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (base.source != null)
		{
			dictionary["source_json_str"] = base.source;
		}
		if (avatarSourceJsonStr != null)
		{
			dictionary["avatar_source_json_str"] = avatarSourceJsonStr;
		}
		if (profileGender != null)
		{
			dictionary["profile_gender"] = profileGender;
		}
		return dictionary;
	}
}
