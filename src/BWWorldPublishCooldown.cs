using System;
using SimpleJSON;
using UnityEngine;

public static class BWWorldPublishCooldown
{
	private static DateTime worldTimestampUTC;

	private static string lastPublishedWorldID;

	private static double worldPublishLeewayInHours = 1.0;

	private static DateTime CooldownEnd => worldTimestampUTC.AddMinutes(CooldownMinutes());

	private static float CooldownMinutes()
	{
		return BWUser.currentUser.PremiumMembershipTier() switch
		{
			1 => BWAppConfiguration.CooldownMinutesPerWorld_Tier1, 
			2 => BWAppConfiguration.CooldownMinutesPerWorld_Tier2, 
			3 => BWAppConfiguration.CooldownMinutesPerWorld_Tier3, 
			_ => BWAppConfiguration.CooldownMinutesPerWorld, 
		};
	}

	public static void SetWorldTimestamp(DateTime timestampUTC, string worldID)
	{
		lastPublishedWorldID = worldID;
		worldTimestampUTC = timestampUTC;
	}

	public static void UpdateFromUserData(JObject userJson)
	{
		worldTimestampUTC = BWJsonHelpers.PropertyIfExists(worldTimestampUTC, "last_published_world_at", userJson);
		lastPublishedWorldID = BWJsonHelpers.IDPropertyAsStringIfExists(lastPublishedWorldID, "last_published_world_id", userJson);
	}

	public static bool CanPublish()
	{
		return DateTime.UtcNow > CooldownEnd;
	}

	public static bool CanPublish(string worldID)
	{
		DateTime utcNow = DateTime.UtcNow;
		if (!(utcNow > CooldownEnd))
		{
			if (worldID == lastPublishedWorldID)
			{
				return utcNow < worldTimestampUTC.AddHours(worldPublishLeewayInHours);
			}
			return false;
		}
		return true;
	}

	public static void CooldownRemaining(out int hours, out int minutes, out int seconds, out int priceToSkip)
	{
		DateTime utcNow = DateTime.UtcNow;
		if (utcNow > CooldownEnd)
		{
			hours = 0;
			minutes = 0;
			seconds = 0;
			priceToSkip = 0;
		}
		else
		{
			TimeSpan timeSpan = CooldownEnd.Subtract(utcNow);
			hours = timeSpan.Hours;
			minutes = timeSpan.Minutes;
			seconds = timeSpan.Seconds;
			int cooldownMinutesPerCoinWorld = BWAppConfiguration.CooldownMinutesPerCoinWorld;
			priceToSkip = Mathf.CeilToInt((float)timeSpan.TotalMinutes / (float)cooldownMinutesPerCoinWorld) + 2;
		}
	}
}
