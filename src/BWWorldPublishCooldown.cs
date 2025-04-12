using System;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003D7 RID: 983
public static class BWWorldPublishCooldown
{
	// Token: 0x17000237 RID: 567
	// (get) Token: 0x06002BA9 RID: 11177 RVA: 0x0013B65D File Offset: 0x00139A5D
	private static DateTime CooldownEnd
	{
		get
		{
			return BWWorldPublishCooldown.worldTimestampUTC.AddMinutes((double)BWWorldPublishCooldown.CooldownMinutes());
		}
	}

	// Token: 0x06002BAA RID: 11178 RVA: 0x0013B670 File Offset: 0x00139A70
	private static float CooldownMinutes()
	{
		int num = BWUser.currentUser.PremiumMembershipTier();
		if (num == 1)
		{
			return (float)BWAppConfiguration.CooldownMinutesPerWorld_Tier1;
		}
		if (num == 2)
		{
			return (float)BWAppConfiguration.CooldownMinutesPerWorld_Tier2;
		}
		if (num == 3)
		{
			return (float)BWAppConfiguration.CooldownMinutesPerWorld_Tier3;
		}
		return (float)BWAppConfiguration.CooldownMinutesPerWorld;
	}

	// Token: 0x06002BAB RID: 11179 RVA: 0x0013B6B8 File Offset: 0x00139AB8
	public static void SetWorldTimestamp(DateTime timestampUTC, string worldID)
	{
		BWWorldPublishCooldown.lastPublishedWorldID = worldID;
		BWWorldPublishCooldown.worldTimestampUTC = timestampUTC;
	}

	// Token: 0x06002BAC RID: 11180 RVA: 0x0013B6C6 File Offset: 0x00139AC6
	public static void UpdateFromUserData(JObject userJson)
	{
		BWWorldPublishCooldown.worldTimestampUTC = BWJsonHelpers.PropertyIfExists(BWWorldPublishCooldown.worldTimestampUTC, "last_published_world_at", userJson);
		BWWorldPublishCooldown.lastPublishedWorldID = BWJsonHelpers.IDPropertyAsStringIfExists(BWWorldPublishCooldown.lastPublishedWorldID, "last_published_world_id", userJson);
	}

	// Token: 0x06002BAD RID: 11181 RVA: 0x0013B6F2 File Offset: 0x00139AF2
	public static bool CanPublish()
	{
		return DateTime.UtcNow > BWWorldPublishCooldown.CooldownEnd;
	}

	// Token: 0x06002BAE RID: 11182 RVA: 0x0013B704 File Offset: 0x00139B04
	public static bool CanPublish(string worldID)
	{
		DateTime utcNow = DateTime.UtcNow;
		return utcNow > BWWorldPublishCooldown.CooldownEnd || (worldID == BWWorldPublishCooldown.lastPublishedWorldID && utcNow < BWWorldPublishCooldown.worldTimestampUTC.AddHours(BWWorldPublishCooldown.worldPublishLeewayInHours));
	}

	// Token: 0x06002BAF RID: 11183 RVA: 0x0013B750 File Offset: 0x00139B50
	public static void CooldownRemaining(out int hours, out int minutes, out int seconds, out int priceToSkip)
	{
		DateTime utcNow = DateTime.UtcNow;
		if (utcNow > BWWorldPublishCooldown.CooldownEnd)
		{
			hours = 0;
			minutes = 0;
			seconds = 0;
			priceToSkip = 0;
			return;
		}
		TimeSpan timeSpan = BWWorldPublishCooldown.CooldownEnd.Subtract(utcNow);
		hours = timeSpan.Hours;
		minutes = timeSpan.Minutes;
		seconds = timeSpan.Seconds;
		int cooldownMinutesPerCoinWorld = BWAppConfiguration.CooldownMinutesPerCoinWorld;
		priceToSkip = Mathf.CeilToInt((float)timeSpan.TotalMinutes / (float)cooldownMinutesPerCoinWorld) + 2;
	}

	// Token: 0x040024EE RID: 9454
	private static DateTime worldTimestampUTC;

	// Token: 0x040024EF RID: 9455
	private static string lastPublishedWorldID;

	// Token: 0x040024F0 RID: 9456
	private static double worldPublishLeewayInHours = 1.0;
}
