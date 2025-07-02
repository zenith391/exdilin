using System;
using System.IO;
using Steamworks;

public static class BWFilesystem
{
	private static string UserGeneratedContentRoot
	{
		get
		{
			string bLOCKSWORLD_ENVIRONMENT = BWEnvConfig.BLOCKSWORLD_ENVIRONMENT;
			string path;
			string folderPath;
			switch (bLOCKSWORLD_ENVIRONMENT)
			{
			case "prod_demo":
			case "production":
				path = "Blocksworld";
				goto IL_0068;
			case "staging":
				path = "blocksworld_staging";
				goto IL_0068;
			case "develop":
				path = "blocksworld_develop";
				goto IL_0068;
			case "local":
				path = "blocksworld_local";
				goto IL_0068;
			default:
				{
					BWLog.Error("Filesystem root folder not configured for this environment: " + bLOCKSWORLD_ENVIRONMENT);
					return null;
				}
				IL_0068:
				folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				return Path.Combine(folderPath, path);
			}
		}
	}

	private static string CurrentUserRoot
	{
		get
		{
			if (BWEnvConfig.Flags["DEMO_USER"])
			{
				return Path.Combine(UserGeneratedContentRoot, "guest");
			}
			if (BWUser.currentUser == null)
			{
				BWLog.Error("Trying to access current user folder with no current user");
				return null;
			}
			string text = null;
			if (BWSteamworksInitializer.Method == BWSteamworksInitializer.LoginMethod.Steam)
			{
				text = "user_" + SteamUser.GetSteamID().ToString();
				return Path.Combine(UserGeneratedContentRoot, text);
			}
			string[] array = (Directory.Exists(UserGeneratedContentRoot) ? Directory.GetDirectories(UserGeneratedContentRoot) : new string[0]);
			if (array.Length != 0)
			{
				return array[0];
			}
			return Path.Combine(UserGeneratedContentRoot, "user_1000");
		}
	}

	public static string CurrentUserWorldsFolder => Path.Combine(CurrentUserRoot, "worlds");

	public static string CurrentUserScreenshotsFolder => Path.Combine(CurrentUserRoot, "screenshots");

	public static string CurrentUserModelsFolder => Path.Combine(CurrentUserRoot, "models");

	public static string CurrentUserProfileWorldFolder => Path.Combine(CurrentUserRoot, "profile");

	public static string CurrentUserDataFolder => Path.Combine(CurrentUserRoot, "user");

	public static string FileProtocolPrefixStr => "file:///";

	public static bool FileExists(string path)
	{
		if (path.StartsWith(FileProtocolPrefixStr))
		{
			path = path.Substring(FileProtocolPrefixStr.Length);
		}
		return File.Exists(path);
	}
}
