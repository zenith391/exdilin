using System;
using SimpleJSON;

// Token: 0x0200039B RID: 923
internal static class BWAppConfiguration
{
	// Token: 0x1700018A RID: 394
	// (get) Token: 0x0600282E RID: 10286 RVA: 0x0012988B File Offset: 0x00127C8B
	// (set) Token: 0x0600282F RID: 10287 RVA: 0x00129892 File Offset: 0x00127C92
	internal static int CooldownMinutesPerCoinModel { get; private set; }

	// Token: 0x1700018B RID: 395
	// (get) Token: 0x06002830 RID: 10288 RVA: 0x0012989A File Offset: 0x00127C9A
	// (set) Token: 0x06002831 RID: 10289 RVA: 0x001298A1 File Offset: 0x00127CA1
	internal static int CooldownMinutesPerCoinWorld { get; private set; }

	// Token: 0x1700018C RID: 396
	// (get) Token: 0x06002832 RID: 10290 RVA: 0x001298A9 File Offset: 0x00127CA9
	// (set) Token: 0x06002833 RID: 10291 RVA: 0x001298B0 File Offset: 0x00127CB0
	internal static int CooldownMinutesPerModel { get; private set; }

	// Token: 0x1700018D RID: 397
	// (get) Token: 0x06002834 RID: 10292 RVA: 0x001298B8 File Offset: 0x00127CB8
	// (set) Token: 0x06002835 RID: 10293 RVA: 0x001298BF File Offset: 0x00127CBF
	internal static int CooldownMinutesPerModel_Tier1 { get; private set; }

	// Token: 0x1700018E RID: 398
	// (get) Token: 0x06002836 RID: 10294 RVA: 0x001298C7 File Offset: 0x00127CC7
	// (set) Token: 0x06002837 RID: 10295 RVA: 0x001298CE File Offset: 0x00127CCE
	internal static int CooldownMinutesPerModel_Tier2 { get; private set; }

	// Token: 0x1700018F RID: 399
	// (get) Token: 0x06002838 RID: 10296 RVA: 0x001298D6 File Offset: 0x00127CD6
	// (set) Token: 0x06002839 RID: 10297 RVA: 0x001298DD File Offset: 0x00127CDD
	internal static int CooldownMinutesPerModel_Tier3 { get; private set; }

	// Token: 0x17000190 RID: 400
	// (get) Token: 0x0600283A RID: 10298 RVA: 0x001298E5 File Offset: 0x00127CE5
	// (set) Token: 0x0600283B RID: 10299 RVA: 0x001298EC File Offset: 0x00127CEC
	internal static int CooldownMinutesPerWorld { get; private set; }

	// Token: 0x17000191 RID: 401
	// (get) Token: 0x0600283C RID: 10300 RVA: 0x001298F4 File Offset: 0x00127CF4
	// (set) Token: 0x0600283D RID: 10301 RVA: 0x001298FB File Offset: 0x00127CFB
	internal static int CooldownMinutesPerWorld_Tier1 { get; private set; }

	// Token: 0x17000192 RID: 402
	// (get) Token: 0x0600283E RID: 10302 RVA: 0x00129903 File Offset: 0x00127D03
	// (set) Token: 0x0600283F RID: 10303 RVA: 0x0012990A File Offset: 0x00127D0A
	internal static int CooldownMinutesPerWorld_Tier2 { get; private set; }

	// Token: 0x17000193 RID: 403
	// (get) Token: 0x06002840 RID: 10304 RVA: 0x00129912 File Offset: 0x00127D12
	// (set) Token: 0x06002841 RID: 10305 RVA: 0x00129919 File Offset: 0x00127D19
	internal static int CooldownMinutesPerWorld_Tier3 { get; private set; }

	// Token: 0x17000194 RID: 404
	// (get) Token: 0x06002842 RID: 10306 RVA: 0x00129921 File Offset: 0x00127D21
	// (set) Token: 0x06002843 RID: 10307 RVA: 0x00129928 File Offset: 0x00127D28
	internal static bool PublicationCooldowns { get; private set; }

	// Token: 0x17000195 RID: 405
	// (get) Token: 0x06002844 RID: 10308 RVA: 0x00129930 File Offset: 0x00127D30
	// (set) Token: 0x06002845 RID: 10309 RVA: 0x00129937 File Offset: 0x00127D37
	internal static string RedemptionExchangeCoins { get; private set; }

	// Token: 0x17000196 RID: 406
	// (get) Token: 0x06002846 RID: 10310 RVA: 0x0012993F File Offset: 0x00127D3F
	// (set) Token: 0x06002847 RID: 10311 RVA: 0x00129946 File Offset: 0x00127D46
	internal static string RedemptionExchangeUSD { get; private set; }

	// Token: 0x17000197 RID: 407
	// (get) Token: 0x06002848 RID: 10312 RVA: 0x0012994E File Offset: 0x00127D4E
	// (set) Token: 0x06002849 RID: 10313 RVA: 0x00129955 File Offset: 0x00127D55
	internal static int WorldLowEffortGafCount { get; private set; }

	// Token: 0x0600284A RID: 10314 RVA: 0x00129960 File Offset: 0x00127D60
	internal static void LoadRemoteConfiguration(JObject rcJson)
	{
		BWAppConfiguration.CooldownMinutesPerCoinModel = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.CooldownMinutesPerCoinModel, "cooldown_minutes_per_coin_model", rcJson);
		BWAppConfiguration.CooldownMinutesPerCoinWorld = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.CooldownMinutesPerCoinWorld, "cooldown_minutes_per_coin_world", rcJson);
		BWAppConfiguration.CooldownMinutesPerModel = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.CooldownMinutesPerModel, "cooldown_minutes_per_model", rcJson);
		BWAppConfiguration.CooldownMinutesPerModel_Tier1 = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.CooldownMinutesPerModel_Tier1, "cooldown_minutes_per_model_tier1", rcJson);
		BWAppConfiguration.CooldownMinutesPerModel_Tier2 = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.CooldownMinutesPerModel_Tier2, "cooldown_minutes_per_model_tier2", rcJson);
		BWAppConfiguration.CooldownMinutesPerModel_Tier3 = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.CooldownMinutesPerModel_Tier3, "cooldown_minutes_per_model_tier3", rcJson);
		BWAppConfiguration.CooldownMinutesPerWorld = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.CooldownMinutesPerWorld, "cooldown_minutes_per_world", rcJson);
		BWAppConfiguration.CooldownMinutesPerWorld_Tier1 = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.CooldownMinutesPerWorld_Tier1, "cooldown_minutes_per_world_tier1", rcJson);
		BWAppConfiguration.CooldownMinutesPerWorld_Tier2 = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.CooldownMinutesPerWorld_Tier2, "cooldown_minutes_per_world_tier2", rcJson);
		BWAppConfiguration.CooldownMinutesPerWorld_Tier3 = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.CooldownMinutesPerWorld_Tier3, "cooldown_minutes_per_world_tier3", rcJson);
		BWAppConfiguration.PublicationCooldowns = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.PublicationCooldowns, "publication_cooldowns", rcJson);
		BWAppConfiguration.RedemptionExchangeCoins = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.RedemptionExchangeCoins, "redemption_exchange_coins", rcJson);
		BWAppConfiguration.RedemptionExchangeUSD = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.RedemptionExchangeUSD, "redemption_exchange_usd", rcJson);
		BWAppConfiguration.WorldLowEffortGafCount = BWJsonHelpers.PropertyIfExists(BWAppConfiguration.WorldLowEffortGafCount, "world_low_effort_gaf_count", rcJson);
	}
}
