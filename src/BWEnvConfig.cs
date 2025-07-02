using System.Collections.Generic;
using UnityEngine;

public static class BWEnvConfig
{
	public static string API_BASE_URL;

	public static readonly string AWS_S3_BASE_URL;

	public static readonly string BLOCKSWORLD_BRANCH;

	public static readonly string BLOCKSWORLD_COMMIT;

	public static readonly string BLOCKSWORLD_ENVIRONMENT;

	public static string BLOCKSWORLD_VERSION;

	public static readonly Dictionary<string, bool> Flags;

	private static string GetApiBaseUrl()
	{
		string text = PlayerPrefs.GetString("exdilin.apiServer", "https://bwsecondary.ddns.net:8080");
		if (text == "https://blocksworld-api.lindenlab.com")
		{
			return "https://bwsecondary.ddns.net:8080";
		}
		if (text != "https://bwsecondary.ddns.net:8080" && text != "http://localhost:8080")
		{
			return "http://localhost:8080";
		}
		return text;
	}

	public static void RevertAPIServer()
	{
		if (API_BASE_URL == "https://blocksworld-api.lindenlab.com")
		{
			BWLog.Warning("Reverting " + API_BASE_URL);
			BWLog.Warning("Unfixable error for official server ('infinite logging error'), switching to the 'secondary server'.");
			PlayerPrefs.SetString("exdilin.apiServer", "https://bwsecondary.ddns.net:8080");
		}
		else if (API_BASE_URL == "https://bwsecondary.ddns.net:8080")
		{
			BWLog.Warning("Unfixable error for Official nor unofficial Secondary server. Switching to Quaternary Local Server. (BW4)!!1");
			PlayerPrefs.SetString("exdilin.apiServer", "http://localhost:8080");
		}
		else
		{
			PlayerPrefs.SetString("exdilin.apiServer", "https://bwsecondary.ddns.net:8080");
		}
		API_BASE_URL = PlayerPrefs.GetString("exdilin.apiServer");
	}

	static BWEnvConfig()
	{
		API_BASE_URL = GetApiBaseUrl();
		AWS_S3_BASE_URL = "https://blocksworld-production.s3.amazonaws.com";
		BLOCKSWORLD_BRANCH = "develop";
		BLOCKSWORLD_COMMIT = "238affebf4";
		BLOCKSWORLD_ENVIRONMENT = "develop";
		BLOCKSWORLD_VERSION = "1.48.0";
		Flags = new Dictionary<string, bool>
		{
			{ "DEBUG_CONSOLE", true },
			{ "DEMO_USER", false },
			{ "INCLUDE_NON_PRODUCTION_READY_BLOCK_ITEMS", true },
			{ "SKIP_GAF_VERIFICATION", true }
		};
	}
}
