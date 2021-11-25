using System;
using System.Collections.Generic;

namespace Exdilin
{
    /*
     * This class is used for adding parameters to tiles and is made for action tiles.
     */
    public static class TileParameterSettingRegistry
    {
        public static void AddTileParameterSetting(TileParameterSetting setting)
        {
            setting.matchingPredicateNames = new string[] { setting.predicateName };
            setting.activated = true;
            TileParameterSettings container = Blocksworld.blocksworldDataContainer.GetComponent<TileParameterSettings>();
            BWLog.Info("Add tile parameter for predicate " + setting.predicateName + " at index " + container.settings.Length);
            TileParameterSetting[] array = new TileParameterSetting[container.settings.Length + 1];
            for (int i = 0; i < container.settings.Length; i++)
            {
                array[i] = container.settings[i];
            }
            array[container.settings.Length] = setting;
            container.settings = array;
        }
    }
}
