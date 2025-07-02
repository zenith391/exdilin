namespace Exdilin;

public static class TileParameterSettingRegistry
{
	public static void AddTileParameterSetting(TileParameterSetting setting)
	{
		setting.matchingPredicateNames = new string[1] { setting.predicateName };
		setting.activated = true;
		TileParameterSettings component = Blocksworld.blocksworldDataContainer.GetComponent<TileParameterSettings>();
		BWLog.Info("Add tile parameter for predicate " + setting.predicateName + " at index " + component.settings.Length);
		TileParameterSetting[] array = new TileParameterSetting[component.settings.Length + 1];
		for (int i = 0; i < component.settings.Length; i++)
		{
			array[i] = component.settings[i];
		}
		array[component.settings.Length] = setting;
		component.settings = array;
	}
}
