using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public static class BWModelPublishCooldown
{
	private static DateTime lastModelTimestamp;

	private static int modelCountPerCooldown = 3;

	public static int availableSlots { get; private set; }

	private static DateTime CooldownEnd => lastModelTimestamp.AddMinutes(CooldownMinutes());

	private static float CooldownMinutes()
	{
		return BWUser.currentUser.PremiumMembershipTier() switch
		{
			1 => BWAppConfiguration.CooldownMinutesPerModel_Tier1, 
			2 => BWAppConfiguration.CooldownMinutesPerModel_Tier2, 
			3 => BWAppConfiguration.CooldownMinutesPerModel_Tier3, 
			_ => BWAppConfiguration.CooldownMinutesPerModel, 
		};
	}

	public static void UpdateFromJson(JObject json)
	{
		lastModelTimestamp = BWJsonHelpers.PropertyIfExists(lastModelTimestamp, "last_published_model_at", json);
	}

	public static bool CanPublish()
	{
		return DateTime.UtcNow > CooldownEnd;
	}

	public static void UpdateAvailableSlots(List<BWUserModel> modelList)
	{
		availableSlots = modelCountPerCooldown;
		foreach (BWUserModel model in modelList)
		{
			if (model.updatedAt > lastModelTimestamp)
			{
				availableSlots--;
			}
		}
		availableSlots = Mathf.Max(0, availableSlots);
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
			int cooldownMinutesPerCoinModel = BWAppConfiguration.CooldownMinutesPerCoinModel;
			priceToSkip = Mathf.CeilToInt((float)timeSpan.TotalMinutes / (float)cooldownMinutesPerCoinModel) + 2;
		}
	}
}
