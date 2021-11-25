using System;

// Token: 0x0200024E RID: 590
public static class BW
{
	// Token: 0x1700007D RID: 125
	// (get) Token: 0x06001B0F RID: 6927 RVA: 0x000C65C6 File Offset: 0x000C49C6
	public static bool isUnityEditor
	{
		get
		{
			return false;
		}
	}

	// Token: 0x1700007E RID: 126
	// (get) Token: 0x06001B10 RID: 6928 RVA: 0x000C65C9 File Offset: 0x000C49C9
	public static bool isIPad
	{
		get
		{
			return false;
		}
	}

	// Token: 0x1700007F RID: 127
	// (get) Token: 0x06001B11 RID: 6929 RVA: 0x000C65CC File Offset: 0x000C49CC
	public static bool isWebGL
	{
		get
		{
			return false;
		}
	}

	// Token: 0x17000080 RID: 128
	// (get) Token: 0x06001B12 RID: 6930 RVA: 0x000C65CF File Offset: 0x000C49CF
	public static IPlatformOptions Options
	{
		get
		{
			if (BW._optionsInterface == null)
			{
				BW._optionsInterface = BW.OptionsInterfaceForPlatform();
			}
			return BW._optionsInterface;
		}
	}

	// Token: 0x17000081 RID: 129
	// (get) Token: 0x06001B13 RID: 6931 RVA: 0x000C65EA File Offset: 0x000C49EA
	public static IAnalytics Analytics
	{
		get
		{
			if (BW._analyticsInterface == null)
			{
				BW._analyticsInterface = BW.AnalyticsInterfaceForPlatform();
			}
			return BW._analyticsInterface;
		}
	}

	// Token: 0x17000082 RID: 130
	// (get) Token: 0x06001B14 RID: 6932 RVA: 0x000C6605 File Offset: 0x000C4A05
	public static IBackend API
	{
		get
		{
			if (BW._backendInterface == null)
			{
				BW._backendInterface = BW.BackendInterfaceForPlatform();
			}
			return BW._backendInterface;
		}
	}

	// Token: 0x06001B15 RID: 6933 RVA: 0x000C6620 File Offset: 0x000C4A20
	private static IPlatformOptions OptionsInterfaceForPlatform()
	{
		return new PlatformOptions_Standalone();
	}

	// Token: 0x06001B16 RID: 6934 RVA: 0x000C6627 File Offset: 0x000C4A27
	private static IAnalytics AnalyticsInterfaceForPlatform()
	{
		return new AnalyticsInterface();
	}

	// Token: 0x06001B17 RID: 6935 RVA: 0x000C662E File Offset: 0x000C4A2E
	private static IBackend BackendInterfaceForPlatform()
	{
		return new BWAPI_Standalone();
	}

	// Token: 0x04001727 RID: 5927
	private static IPlatformOptions _optionsInterface;

	// Token: 0x04001728 RID: 5928
	private static IAnalytics _analyticsInterface;

	// Token: 0x04001729 RID: 5929
	private static IBackend _backendInterface;
}
