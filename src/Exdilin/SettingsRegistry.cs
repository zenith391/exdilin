using System.Collections.Generic;

namespace Exdilin;

public static class SettingsRegistry
{
	private static List<SettingEntry> entries = new List<SettingEntry>();

	public static SettingEntry[] GetSettings()
	{
		return entries.ToArray();
	}

	public static void AddSetting(SettingEntry entry)
	{
		entry.mod = Mod.ExecutionMod;
		entries.Add(entry);
	}
}
