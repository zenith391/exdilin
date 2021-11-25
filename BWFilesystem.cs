using System;
using System.IO;
using Steamworks;

// Token: 0x020003A5 RID: 933
public static class BWFilesystem
{
	// Token: 0x170001A7 RID: 423
	// (get) Token: 0x06002895 RID: 10389 RVA: 0x0012AAFC File Offset: 0x00128EFC
	private static string UserGeneratedContentRoot
	{
		get
		{
			string blocksworld_ENVIRONMENT = BWEnvConfig.BLOCKSWORLD_ENVIRONMENT;
			if (blocksworld_ENVIRONMENT != null)
			{
				string path;
				if (!(blocksworld_ENVIRONMENT == "local"))
				{
					if (!(blocksworld_ENVIRONMENT == "develop"))
					{
						if (!(blocksworld_ENVIRONMENT == "staging"))
						{
							if (!(blocksworld_ENVIRONMENT == "prod_demo") && !(blocksworld_ENVIRONMENT == "production"))
							{
								goto IL_8F;
							}
							path = "Blocksworld";
						}
						else
						{
							path = "blocksworld_staging";
						}
					}
					else
					{
						path = "blocksworld_develop";
					}
				}
				else
				{
					path = "blocksworld_local";
				}
				string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				return Path.Combine(folderPath, path);
			}
			IL_8F:
			BWLog.Error("Filesystem root folder not configured for this environment: " + blocksworld_ENVIRONMENT);
			return null;
		}
	}

	// Token: 0x170001A8 RID: 424
	// (get) Token: 0x06002896 RID: 10390 RVA: 0x0012ABBC File Offset: 0x00128FBC
	private static string CurrentUserRoot
	{
		get
		{
			if (BWEnvConfig.Flags["DEMO_USER"])
			{
				return Path.Combine(BWFilesystem.UserGeneratedContentRoot, "guest");
			}
			if (BWUser.currentUser == null)
			{
				BWLog.Error("Trying to access current user folder with no current user");
				return null;
			}
			string path = null;
			if (BWSteamworksInitializer.Method == BWSteamworksInitializer.LoginMethod.Steam) {
				path = "user_" + SteamUser.GetSteamID().ToString();
			} else { // added by Exdilin
				string[] pathes = Directory.Exists(BWFilesystem.UserGeneratedContentRoot) ?
					Directory.GetDirectories(BWFilesystem.UserGeneratedContentRoot) : new string[0];
				if (pathes.Length > 0) {
					return pathes[0];
				} else {
					return Path.Combine(BWFilesystem.UserGeneratedContentRoot, "user_1000");
				}
			}
			return Path.Combine(BWFilesystem.UserGeneratedContentRoot, path);
		}
	}

	// Token: 0x170001A9 RID: 425
	// (get) Token: 0x06002897 RID: 10391 RVA: 0x0012AC2C File Offset: 0x0012902C
	public static string CurrentUserWorldsFolder
	{
		get
		{
			return Path.Combine(BWFilesystem.CurrentUserRoot, "worlds");
		}
	}

	// Token: 0x170001AA RID: 426
	// (get) Token: 0x06002898 RID: 10392 RVA: 0x0012AC3D File Offset: 0x0012903D
	public static string CurrentUserScreenshotsFolder
	{
		get
		{
			return Path.Combine(BWFilesystem.CurrentUserRoot, "screenshots");
		}
	}

	// Token: 0x170001AB RID: 427
	// (get) Token: 0x06002899 RID: 10393 RVA: 0x0012AC4E File Offset: 0x0012904E
	public static string CurrentUserModelsFolder
	{
		get
		{
			return Path.Combine(BWFilesystem.CurrentUserRoot, "models");
		}
	}

	// Token: 0x170001AC RID: 428
	// (get) Token: 0x0600289A RID: 10394 RVA: 0x0012AC5F File Offset: 0x0012905F
	public static string CurrentUserProfileWorldFolder
	{
		get
		{
			return Path.Combine(BWFilesystem.CurrentUserRoot, "profile");
		}
	}

	// Token: 0x170001AD RID: 429
	// (get) Token: 0x0600289B RID: 10395 RVA: 0x0012AC70 File Offset: 0x00129070
	public static string CurrentUserDataFolder
	{
		get
		{
			return Path.Combine(BWFilesystem.CurrentUserRoot, "user");
		}
	}

	// Token: 0x170001AE RID: 430
	// (get) Token: 0x0600289C RID: 10396 RVA: 0x0012AC81 File Offset: 0x00129081
	public static string FileProtocolPrefixStr
	{
		get
		{
			return "file:///";
		}
	}

	// Token: 0x0600289D RID: 10397 RVA: 0x0012AC88 File Offset: 0x00129088
	public static bool FileExists(string path)
	{
		if (path.StartsWith(BWFilesystem.FileProtocolPrefixStr))
		{
			path = path.Substring(BWFilesystem.FileProtocolPrefixStr.Length);
		}
		return File.Exists(path);
	}
}
