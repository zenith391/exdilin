using SimpleJSON;

internal static class BWAppConfiguration
{
	internal static int CooldownMinutesPerCoinModel { get; private set; }

	internal static int CooldownMinutesPerCoinWorld { get; private set; }

	internal static int CooldownMinutesPerModel { get; private set; }

	internal static int CooldownMinutesPerModel_Tier1 { get; private set; }

	internal static int CooldownMinutesPerModel_Tier2 { get; private set; }

	internal static int CooldownMinutesPerModel_Tier3 { get; private set; }

	internal static int CooldownMinutesPerWorld { get; private set; }

	internal static int CooldownMinutesPerWorld_Tier1 { get; private set; }

	internal static int CooldownMinutesPerWorld_Tier2 { get; private set; }

	internal static int CooldownMinutesPerWorld_Tier3 { get; private set; }

	internal static bool PublicationCooldowns { get; private set; }

	internal static string RedemptionExchangeCoins { get; private set; }

	internal static string RedemptionExchangeUSD { get; private set; }

	internal static int WorldLowEffortGafCount { get; private set; }

	internal static void LoadRemoteConfiguration(JObject rcJson)
	{
		CooldownMinutesPerCoinModel = BWJsonHelpers.PropertyIfExists(CooldownMinutesPerCoinModel, "cooldown_minutes_per_coin_model", rcJson);
		CooldownMinutesPerCoinWorld = BWJsonHelpers.PropertyIfExists(CooldownMinutesPerCoinWorld, "cooldown_minutes_per_coin_world", rcJson);
		CooldownMinutesPerModel = BWJsonHelpers.PropertyIfExists(CooldownMinutesPerModel, "cooldown_minutes_per_model", rcJson);
		CooldownMinutesPerModel_Tier1 = BWJsonHelpers.PropertyIfExists(CooldownMinutesPerModel_Tier1, "cooldown_minutes_per_model_tier1", rcJson);
		CooldownMinutesPerModel_Tier2 = BWJsonHelpers.PropertyIfExists(CooldownMinutesPerModel_Tier2, "cooldown_minutes_per_model_tier2", rcJson);
		CooldownMinutesPerModel_Tier3 = BWJsonHelpers.PropertyIfExists(CooldownMinutesPerModel_Tier3, "cooldown_minutes_per_model_tier3", rcJson);
		CooldownMinutesPerWorld = BWJsonHelpers.PropertyIfExists(CooldownMinutesPerWorld, "cooldown_minutes_per_world", rcJson);
		CooldownMinutesPerWorld_Tier1 = BWJsonHelpers.PropertyIfExists(CooldownMinutesPerWorld_Tier1, "cooldown_minutes_per_world_tier1", rcJson);
		CooldownMinutesPerWorld_Tier2 = BWJsonHelpers.PropertyIfExists(CooldownMinutesPerWorld_Tier2, "cooldown_minutes_per_world_tier2", rcJson);
		CooldownMinutesPerWorld_Tier3 = BWJsonHelpers.PropertyIfExists(CooldownMinutesPerWorld_Tier3, "cooldown_minutes_per_world_tier3", rcJson);
		PublicationCooldowns = BWJsonHelpers.PropertyIfExists(PublicationCooldowns, "publication_cooldowns", rcJson);
		RedemptionExchangeCoins = BWJsonHelpers.PropertyIfExists(RedemptionExchangeCoins, "redemption_exchange_coins", rcJson);
		RedemptionExchangeUSD = BWJsonHelpers.PropertyIfExists(RedemptionExchangeUSD, "redemption_exchange_usd", rcJson);
		WorldLowEffortGafCount = BWJsonHelpers.PropertyIfExists(WorldLowEffortGafCount, "world_low_effort_gaf_count", rcJson);
	}
}
