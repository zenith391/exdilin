using Exdilin;
using Exdilin.UI;
using UnityEngine;

namespace ModExdilin
{
    /*
     * The part of Exdilin that adds developer features using its own mod API.
     */
    public class ExdilinMod : Mod
    {
        public override string Name => "Exdilin";

		public override Version Version => VersionStatic;
		public static Version VersionStatic = new Version(0, 7, 0);

		public override string Id => "exdilin";

        public override void Register(RegisterType registerType)
        {
            if (registerType == RegisterType.TILE_PARAMETERS)
            {
                BWLog.Info("Adding tile parameter settings to Gauge..");
                TileParameterSetting set = new TileParameterSetting();
                set.predicateName = "GaugeUI.SetText"; set.type = TileParameterType.StringSingleLine;
                set.parameterIndex = 0;
                set.stringAcceptAny = true; set.stringAcceptAnyHint = "Gauge text";
                TileParameterSettingRegistry.AddTileParameterSetting(set);

                set = new TileParameterSetting();
                set.predicateName = "GaugeUI.Equals"; set.type = TileParameterType.IntSlider;
                set.parameterIndex = 0;
                set.intMinValue = 0; set.intMaxValue = 100; set.intStep = 1;
                set.sliderSensitivity = 50f;
                TileParameterSettingRegistry.AddTileParameterSetting(set);
               
                set = new TileParameterSetting();
                set.predicateName = "GaugeUI.SetMaxValue"; set.type = TileParameterType.IntSlider;
                set.parameterIndex = 0;
                set.intMinValue = 0; set.intMaxValue = 10000; set.intStep = 1;
                set.sliderSensitivity = 50f;
                TileParameterSettingRegistry.AddTileParameterSetting(set);

                set = new TileParameterSetting();
                set.predicateName = "GaugeUI.Increment"; set.type = TileParameterType.IntSlider;
                set.parameterIndex = 0;
                set.intMinValue = -1000; set.intMaxValue = 1000; set.intStep = 1;
                set.sliderSensitivity = 50f;
                set.intOnlyShowPositive = true;
                TileParameterSettingRegistry.AddTileParameterSetting(set);
            } else if (registerType == RegisterType.SETTINGS)
            {
                SettingEntry apiServer = new SettingEntry("apiServer", "API Server", "https://blocksworld-api.lindenlab.com");
                SettingsRegistry.AddSetting(apiServer);
            }
        }

        public override void Init()
        {
        }

        public override void PreInit()
        {
            BWLog.Info("Pre-inited Exdilin Mod succcessfully!");
        }
    }
}
