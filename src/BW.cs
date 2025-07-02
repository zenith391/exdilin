public static class BW
{
	private static IPlatformOptions _optionsInterface;

	private static IAnalytics _analyticsInterface;

	private static IBackend _backendInterface;

	public static bool isUnityEditor => false;

	public static bool isIPad => false;

	public static bool isWebGL => false;

	public static IPlatformOptions Options
	{
		get
		{
			if (_optionsInterface == null)
			{
				_optionsInterface = OptionsInterfaceForPlatform();
			}
			return _optionsInterface;
		}
	}

	public static IAnalytics Analytics
	{
		get
		{
			if (_analyticsInterface == null)
			{
				_analyticsInterface = AnalyticsInterfaceForPlatform();
			}
			return _analyticsInterface;
		}
	}

	public static IBackend API
	{
		get
		{
			if (_backendInterface == null)
			{
				_backendInterface = BackendInterfaceForPlatform();
			}
			return _backendInterface;
		}
	}

	private static IPlatformOptions OptionsInterfaceForPlatform()
	{
		return new PlatformOptions_Standalone();
	}

	private static IAnalytics AnalyticsInterfaceForPlatform()
	{
		return new AnalyticsInterface();
	}

	private static IBackend BackendInterfaceForPlatform()
	{
		return new BWAPI_Standalone();
	}
}
