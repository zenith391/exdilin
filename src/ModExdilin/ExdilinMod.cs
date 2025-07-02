using Exdilin;

namespace ModExdilin;

public class ExdilinMod : Mod
{
	public static Version VersionStatic = new Version(0, 7, 0);

	public override string Name => "Exdilin";

	public override Version Version => VersionStatic;

	public override string Id => "exdilin";

	public override void Register(RegisterType registerType)
	{
		switch (registerType)
		{
		case RegisterType.TILE_PARAMETERS:
		{
			BWLog.Info("Adding tile parameter settings to Gauge..");
			TileParameterSetting tileParameterSetting = new TileParameterSetting();
			tileParameterSetting.predicateName = "GaugeUI.SetText";
			tileParameterSetting.type = TileParameterType.StringSingleLine;
			tileParameterSetting.parameterIndex = 0;
			tileParameterSetting.stringAcceptAny = true;
			tileParameterSetting.stringAcceptAnyHint = "Gauge text";
			TileParameterSettingRegistry.AddTileParameterSetting(tileParameterSetting);
			tileParameterSetting = new TileParameterSetting();
			tileParameterSetting.predicateName = "GaugeUI.Equals";
			tileParameterSetting.type = TileParameterType.IntSlider;
			tileParameterSetting.parameterIndex = 0;
			tileParameterSetting.intMinValue = 0;
			tileParameterSetting.intMaxValue = 100;
			tileParameterSetting.intStep = 1;
			tileParameterSetting.sliderSensitivity = 50f;
			TileParameterSettingRegistry.AddTileParameterSetting(tileParameterSetting);
			tileParameterSetting = new TileParameterSetting();
			tileParameterSetting.predicateName = "GaugeUI.SetMaxValue";
			tileParameterSetting.type = TileParameterType.IntSlider;
			tileParameterSetting.parameterIndex = 0;
			tileParameterSetting.intMinValue = 0;
			tileParameterSetting.intMaxValue = 10000;
			tileParameterSetting.intStep = 1;
			tileParameterSetting.sliderSensitivity = 50f;
			TileParameterSettingRegistry.AddTileParameterSetting(tileParameterSetting);
			tileParameterSetting = new TileParameterSetting();
			tileParameterSetting.predicateName = "GaugeUI.Increment";
			tileParameterSetting.type = TileParameterType.IntSlider;
			tileParameterSetting.parameterIndex = 0;
			tileParameterSetting.intMinValue = -1000;
			tileParameterSetting.intMaxValue = 1000;
			tileParameterSetting.intStep = 1;
			tileParameterSetting.sliderSensitivity = 50f;
			tileParameterSetting.intOnlyShowPositive = true;
			TileParameterSettingRegistry.AddTileParameterSetting(tileParameterSetting);
			break;
		}
		case RegisterType.SETTINGS:
		{
			SettingEntry entry = new SettingEntry("apiServer", "API Server", "https://blocksworld-api.lindenlab.com");
			SettingsRegistry.AddSetting(entry);
			break;
		}
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
