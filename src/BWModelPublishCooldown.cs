using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003A8 RID: 936
public static class BWModelPublishCooldown
{
	// Token: 0x170001B0 RID: 432
	// (get) Token: 0x060028C1 RID: 10433 RVA: 0x0012C395 File Offset: 0x0012A795
	// (set) Token: 0x060028C2 RID: 10434 RVA: 0x0012C39C File Offset: 0x0012A79C
	public static int availableSlots { get; private set; }

	// Token: 0x170001B1 RID: 433
	// (get) Token: 0x060028C3 RID: 10435 RVA: 0x0012C3A4 File Offset: 0x0012A7A4
	private static DateTime CooldownEnd
	{
		get
		{
			return BWModelPublishCooldown.lastModelTimestamp.AddMinutes((double)BWModelPublishCooldown.CooldownMinutes());
		}
	}

	// Token: 0x060028C4 RID: 10436 RVA: 0x0012C3B8 File Offset: 0x0012A7B8
	private static float CooldownMinutes()
	{
		int num = BWUser.currentUser.PremiumMembershipTier();
		if (num == 1)
		{
			return (float)BWAppConfiguration.CooldownMinutesPerModel_Tier1;
		}
		if (num == 2)
		{
			return (float)BWAppConfiguration.CooldownMinutesPerModel_Tier2;
		}
		if (num == 3)
		{
			return (float)BWAppConfiguration.CooldownMinutesPerModel_Tier3;
		}
		return (float)BWAppConfiguration.CooldownMinutesPerModel;
	}

	// Token: 0x060028C5 RID: 10437 RVA: 0x0012C400 File Offset: 0x0012A800
	public static void UpdateFromJson(JObject json)
	{
		BWModelPublishCooldown.lastModelTimestamp = BWJsonHelpers.PropertyIfExists(BWModelPublishCooldown.lastModelTimestamp, "last_published_model_at", json);
	}

	// Token: 0x060028C6 RID: 10438 RVA: 0x0012C417 File Offset: 0x0012A817
	public static bool CanPublish()
	{
		return DateTime.UtcNow > BWModelPublishCooldown.CooldownEnd;
	}

	// Token: 0x060028C7 RID: 10439 RVA: 0x0012C428 File Offset: 0x0012A828
	public static void UpdateAvailableSlots(List<BWUserModel> modelList)
	{
		BWModelPublishCooldown.availableSlots = BWModelPublishCooldown.modelCountPerCooldown;
		foreach (BWUserModel bwuserModel in modelList)
		{
			if (bwuserModel.updatedAt > BWModelPublishCooldown.lastModelTimestamp)
			{
				BWModelPublishCooldown.availableSlots--;
			}
		}
		BWModelPublishCooldown.availableSlots = Mathf.Max(0, BWModelPublishCooldown.availableSlots);
	}

	// Token: 0x060028C8 RID: 10440 RVA: 0x0012C4B4 File Offset: 0x0012A8B4
	public static void CooldownRemaining(out int hours, out int minutes, out int seconds, out int priceToSkip)
	{
		DateTime utcNow = DateTime.UtcNow;
		if (utcNow > BWModelPublishCooldown.CooldownEnd)
		{
			hours = 0;
			minutes = 0;
			seconds = 0;
			priceToSkip = 0;
			return;
		}
		TimeSpan timeSpan = BWModelPublishCooldown.CooldownEnd.Subtract(utcNow);
		hours = timeSpan.Hours;
		minutes = timeSpan.Minutes;
		seconds = timeSpan.Seconds;
		int cooldownMinutesPerCoinModel = BWAppConfiguration.CooldownMinutesPerCoinModel;
		priceToSkip = Mathf.CeilToInt((float)timeSpan.TotalMinutes / (float)cooldownMinutesPerCoinModel) + 2;
	}

	// Token: 0x04002387 RID: 9095
	private static DateTime lastModelTimestamp;

	// Token: 0x04002389 RID: 9097
	private static int modelCountPerCooldown = 3;
}
