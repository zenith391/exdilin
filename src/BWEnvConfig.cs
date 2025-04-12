using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000039 RID: 57
public static class BWEnvConfig
{
	public static string API_BASE_URL = GetApiBaseUrl();

    // Token: 0x040001DC RID: 476
    public static readonly string AWS_S3_BASE_URL = "https://blocksworld-production.s3.amazonaws.com";

	// Token: 0x040001DD RID: 477
	public static readonly string BLOCKSWORLD_BRANCH = "develop";

	// Token: 0x040001DE RID: 478
	public static readonly string BLOCKSWORLD_COMMIT = "238affebf4";

	// Token: 0x040001DF RID: 479
	public static readonly string BLOCKSWORLD_ENVIRONMENT = "develop";

	// Token: 0x040001E0 RID: 480
	public static string BLOCKSWORLD_VERSION = "1.47.0";

	// Token: 0x040001E1 RID: 481
	public static readonly Dictionary<string, bool> Flags = new Dictionary<string, bool>
	{
		{
			"DEBUG_CONSOLE",
			true
		},
		{
			"DEMO_USER",
			false
		},
		{
			"INCLUDE_NON_PRODUCTION_READY_BLOCK_ITEMS",
			true
		},
        { // added by exdilin
            "SKIP_GAF_VERIFICATION", // necesarry for worlds with non-production ready blocks.
            true
        }
    };

	private static string GetApiBaseUrl() {
		String pref = PlayerPrefs.GetString("exdilin.apiServer", "https://bwsecondary.ddns.net:8080");
		if (pref == "https://blocksworld-api.lindenlab.com") {
			return "https://bwsecondary.ddns.net:8080"; // override, blocksworld-api.lindenlab.com has been unregistered and won't work at all anymore
		} else {
			return pref;
		}
	}

	public static void RevertAPIServer()
    {
		// As of Exdilin 0.7.0, we're only reverting if it isn't the main URL
        if (API_BASE_URL == "https://blocksworld-api.lindenlab.com")
        {

			BWLog.Warning("Reverting " + API_BASE_URL);
			BWLog.Warning("Unfixable error for official server ('infinite logging error'), switching to the 'secondary server'.");
            PlayerPrefs.SetString("exdilin.apiServer", "https://bwsecondary.ddns.net:8080");
        }
        else if (API_BASE_URL != "https://bwsecondary.ddns.net:8080")
        {

            PlayerPrefs.SetString("exdilin.apiServer", "https://bwsecondary.ddns.net:8080");
        }
        API_BASE_URL = PlayerPrefs.GetString("exdilin.apiServer");
    }
}
