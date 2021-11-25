using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Exdilin;

// Token: 0x020003BE RID: 958
using System.Reflection.Emit;
public class BWSteamworksInitializer : MonoBehaviour
{

	// added by Exdilin
	public enum LoginMethod {
		Steam,
		AuthToken
	};
	public static LoginMethod Method;
	// end of added by Exdilin

	// Token: 0x060029BA RID: 10682 RVA: 0x00132150 File Offset: 0x00130550
	private void SetVersionInfoWithMessage(string message)
	{
		string text = BWEnvConfig.BLOCKSWORLD_VERSION;
		if (BWEnvConfig.BLOCKSWORLD_ENVIRONMENT != "production")
		{
			text = text + " (with Exdilin " + ModExdilin.ExdilinMod.VersionStatic.ToString() + ")";
		}
		if (!string.IsNullOrEmpty(message))
		{
			text = text + " (" + message + ")";
		}
		if (this.versionText != null)
		{
			for (int i = 0; i < this.versionText.Length; i++)
			{
				this.versionText[i].text = text;
			}
		}
	}

	// Token: 0x060029BB RID: 10683 RVA: 0x001321D8 File Offset: 0x001305D8
	public void UpdateText(string newText)
	{
		if (this.statusText != null)
		{
			for (int i = 0; i < this.statusText.Length; i++)
			{
				this.statusText[i].text = newText;
			}
		}
	}

	// Token: 0x060029BC RID: 10684 RVA: 0x00132218 File Offset: 0x00130618
	private void Awake()
	{
		string versionInfoWithMessage = string.Empty;
		if (BWEnvConfig.Flags.ContainsKey("DEBUG_CONSOLE") && BWEnvConfig.Flags["DEBUG_CONSOLE"])
		{
			versionInfoWithMessage = "with debug console";
		}
		this.SetVersionInfoWithMessage(versionInfoWithMessage);
		this.UpdateText("Loading...");
		this._GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(new Callback<GetAuthSessionTicketResponse_t>.DispatchDelegate(this.OnGetAuthSessionTicketResponse));
		this._HAuthTicket = HAuthTicket.Invalid;
	}

	// Token: 0x060029BD RID: 10685 RVA: 0x0013228D File Offset: 0x0013068D
	private void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback)
	{
		this.steamworksErrorCode = (int)pCallback.m_eResult;
		this.steamworksRequestInProgress = false;
	}

	// Token: 0x060029BE RID: 10686 RVA: 0x001322A3 File Offset: 0x001306A3
	private void Stop()
	{
		Debug.Log("Stopping...");
		if (this._HAuthTicket != HAuthTicket.Invalid)
		{
			SteamUser.CancelAuthTicket(this._HAuthTicket);
			this._HAuthTicket = HAuthTicket.Invalid;
		}
	}

	// Token: 0x060029BF RID: 10687 RVA: 0x001322DA File Offset: 0x001306DA
	private void LaunchGame()
	{
		this.UpdateText("Starting Blocksworld...");
		SceneManager.LoadScene("BlocksworldMain", LoadSceneMode.Additive);
		base.StartCoroutine(this.FadeOutLaunchScreen());
	}

	// Token: 0x060029C0 RID: 10688 RVA: 0x00132300 File Offset: 0x00130700
	private IEnumerator FadeOutLaunchScreen()
	{
		while (BWStandalone.Instance == null || !BWStandalone.Instance.menuLoaded)
		{
			yield return null;
		}
		if (this.launchScreenAnimator != null)
		{
			this.launchScreenAnimator.SetTrigger("Hide");
		}
		yield break;
	}

	// ADDED BY EXDILIN
	// get auth token from an external source (file)
	public string GetExternalAuthToken() {
		string basePath = Application.dataPath + "/..";
		string path = basePath + "/auth_token.txt";
		if (File.Exists(path)) {
			string text = File.ReadAllText(path);
			File.Delete(path);
			return text;
		} else {
			return null;
		}
	}

	// Token: 0x060029C1 RID: 10689 RVA: 0x0013231C File Offset: 0x0013071C
	private IEnumerator Start()
	{
		yield return null;
		// all auth token login related part is added by Exdilin
		string extToken = GetExternalAuthToken();
		bool useSteam = extToken == null;
		Method = useSteam ? LoginMethod.Steam : LoginMethod.AuthToken;

		string nickname = "";
		if (BWEnvConfig.Flags["DEMO_USER"])
		{
			this.UpdateText("Be our guest!");
			yield return new WaitForSeconds(1f);
			BWUser.SetupDemoCurrentUser();
		}
		else if (useSteam)
		{
			while (!SteamManager.Initialized) {
				this.UpdateText("Launch Steam and try again...");
				yield return new WaitForSeconds(5f);
			}
			CSteamID steamIDPlayer = SteamUser.GetSteamID();
			CSteamID steamIDOwner = SteamApps.GetAppOwner();
			if (!steamIDPlayer.IsValid() || !steamIDOwner.IsValid()) {
				this.UpdateText("Invalid Steam ID");
				yield break;
			}
			if (steamIDOwner != steamIDPlayer) {
				for (; ; )
				{
					this.UpdateText("Blocksworld account owner not logged in!");
					yield return new WaitForSeconds(5f);
				}
			}
			string personaName = SteamFriends.GetPersonaName();
			this.UpdateText("Welcome " + personaName + "!");
			yield return new WaitForSeconds(1f);
			this.UpdateText("Starting session...");
			yield return null;
			this.steamworksRequestInProgress = true;
			byte[] ticket = new byte[1024];
			uint pcbTicket = 0u;
			this._HAuthTicket = SteamUser.GetAuthSessionTicket(ticket, 1024, out pcbTicket);
			while (this.steamworksRequestInProgress)
			{
				yield return new WaitForSeconds(0.2f);
			}
			if (this.steamworksErrorCode != 1 || pcbTicket <= 0u)
			{
				this.UpdateText("Unable to start session.  Please try again later.");
				yield break;
			}
			byte[] truncatedTicket = new byte[pcbTicket];
			Array.Copy(ticket, 0L, truncatedTicket, 0L, (long)((ulong)pcbTicket));
			this._HexEncodedTicket = BitConverter.ToString(truncatedTicket).Replace("-", string.Empty);
            BWLog.Info("Generated Steam ticket: " + this._HexEncodedTicket);
            yield return null;
            nickname = SteamFriends.GetPlayerNickname(steamIDPlayer);
			if (string.IsNullOrEmpty(nickname))
			{
				nickname = personaName;
			}
			this.UpdateText("Logging in as " + nickname + "...");
			yield return null;
			this.RequestCurrentUser(steamIDPlayer);
			while (this.apiRequestInProgress)
			{
				yield return new WaitForSeconds(0.5f);
			}
			if (this.apiRequestErrorCode == 404)
			{
				this.UpdateText("Creating Blocksworld account for " + nickname);
				this.RequestCreateUser(steamIDPlayer, personaName, nickname);
				yield return null;
			}
			else
			{
				if (this.apiRequestConnectionRefused || this.apiRequestErrorCode != 0)
				{
					this.UpdateText("Unable to login.  Please try again later.");
					this.SetVersionInfoWithMessage(string.Concat(new object[]
					{
						"error ",
						this.apiRequestErrorCode,
						" ",
						this.apiRequestErrorMsg
					}));
					yield break;
				}
				this.UpdateText("Logged in!");
				yield return null;
				if (BWUser.currentUser.username != nickname) {
					this.RequestUpdateUsername(nickname);
					while (this.apiRequestInProgress) {
						yield return new WaitForSeconds(0.5f);
					}
					if (this.apiRequestConnectionRefused || this.apiRequestErrorCode != 0) {
						this.UpdateText("Unable to change Blocksworld username!");
						yield return new WaitForSeconds(2.0f);
					}
					this.UpdateText("Changed username!");
					yield return null;
				}
			}
			while (this.apiRequestInProgress)
			{
                yield return new WaitForSeconds(0.5f);
			}
			if (this.apiRequestConnectionRefused || this.apiRequestErrorCode != 0)
			{
				this.UpdateText("Unable to connect to server.  Please try again later.");
				this.SetVersionInfoWithMessage(string.Concat(new object[]
				{
					"error ",
					this.apiRequestErrorCode,
					" ",
					this.apiRequestErrorMsg
				}));
				yield break;
			}
		} else { // when not using steam
			this.UpdateText("Starting launcher session...");
			yield return null;
			nickname = "todo";
			this.UpdateText("Logging in with token " + extToken + "...");
			yield return null;
			this.RequestCurrentUserAuth(extToken);
			while (this.apiRequestInProgress) {
				yield return new WaitForSeconds(0.5f);
			}
			if (this.apiRequestErrorCode == 404) {
				this.UpdateText("Invalid authentication token. Try to restart the launcher.");
				yield break;
			} else {
				if (this.apiRequestConnectionRefused || this.apiRequestErrorCode != 0) {
					this.UpdateText("Unable to login. Please try again later.");
					this.SetVersionInfoWithMessage(string.Concat(new object[]
					{
						"error ",
						this.apiRequestErrorCode,
						" ",
						this.apiRequestErrorMsg
					}));
					yield break;
				}
				this.UpdateText("Logged in!");
				yield return null;
			}
		}
		this.UpdateText("Downloading " + nickname + "'s worlds...");
		yield return null;
		this.RequestCurrentUserWorlds();
		while (this.apiRequestInProgress) {
			yield return new WaitForSeconds(0.5f);
		}
		if (this.apiRequestConnectionRefused || this.apiRequestErrorCode != 0) {
			this.UpdateText("Unable to download worlds.  Please try again later.");
			this.SetVersionInfoWithMessage(string.Concat(new object[]
			{
					"error ",
					this.apiRequestErrorCode,
					" ",
					this.apiRequestErrorMsg
			}));
			yield break;
		}
		this.UpdateText("Fetching remote configuration...");
		yield return null;
		this.RequestAppRemoteConfiguration();
		while (this.apiRequestInProgress)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (this.apiRequestConnectionRefused || this.apiRequestErrorCode != 0)
		{
			this.UpdateText("Unable to fetch remote configuration.  Please try again later.");
			this.SetVersionInfoWithMessage(string.Concat(new object[]
			{
				"error ",
				this.apiRequestErrorCode,
				" ",
				this.apiRequestErrorMsg
			}));
			yield break;
		}
		this.UpdateText("Downloading categories...");
		yield return null;
		this.RequestContentCategories();
		while (this.apiRequestInProgress)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (this.apiRequestConnectionRefused || this.apiRequestErrorCode != 0)
		{
			this.UpdateText("Unable to download categories.  Please try again later.");
			this.SetVersionInfoWithMessage(string.Concat(new object[]
			{
				"error ",
				this.apiRequestErrorCode,
				" ",
				this.apiRequestErrorMsg
			}));
			yield break;
		}
		this.UpdateText("Downloading block data...");
		yield return null;
		this.RequestBlockPricing();
		while (this.apiRequestInProgress)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (this.apiRequestConnectionRefused || this.apiRequestErrorCode != 0)
		{
			this.UpdateText("Unable to download block data.  Please try again later.");
			this.SetVersionInfoWithMessage(string.Concat(new object[]
			{
				"error ",
				this.apiRequestErrorCode,
				" ",
				this.apiRequestErrorMsg
			}));
			yield break;
		}
        this.UpdateText("Loading mods..");
        yield return ModLoader.LoadModsCoroutine(delegate(string id, string stage)
		{
			if (stage == "load") {
				UpdateText("Loading mods.. (" + id + ")");
			} else if (stage == "preinit") {
				UpdateText("Pre-initing mods.. (" + id + ")");
			}
		});

		BWAPIRequestBase request = BW.API.CreateRequest("GET", "/api/v2/exdilin/configuration?version=" + ModExdilin.ExdilinMod.VersionStatic.ToString());
		request.onSuccess = delegate (JObject resp)
		{
			Exdilin.Version ver = ModExdilin.ExdilinMod.VersionStatic;
			int verId = (ver.Major << 16) | (ver.Minor << 8) | ver.Patch;
			Debug.Log("Latest version ID: " + resp["latest_version_id"].IntValue + ", current version ID: " + verId);
			if (resp["latest_version_id"].IntValue > verId) {
				ModLoader.OverlaysAvailable += delegate (object sender, EventArgs e)
				{
					BWStandalone.Overlays.ShowConfirmationDialog("Update", "Update Exdilin to " + resp["latest_version"].StringValue + " ?", delegate ()
					{
						System.Diagnostics.Process.Start("https://bwsecondary.ddns.net/mods/0");
					}, "Update!", "No");
				};
			}
		};
		request.onFailure = delegate (BWAPIRequestError error)
		{

		};
		// failure intentionally isn't handled
		request.SendOwnerCoroutine(this);
		this.LaunchGame();
		yield return null;
		yield break;
	}

	// Token: 0x060029C2 RID: 10690 RVA: 0x00132337 File Offset: 0x00130737
	private void ResetAPIRequestVariables()
	{
		this.apiRequestErrorCode = 0;
		this.apiRequestErrorMsg = string.Empty;
		this.apiRequestInProgress = true;
		this.apiRequestConnectionRefused = false;
	}

    // Token: 0x060029C3 RID: 10691 RVA: 0x0013235C File Offset: 0x0013075C
    private void RequestCurrentUser(CSteamID steamIDPlayer)
	{
		this.ResetAPIRequestVariables();
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/steam_current_user");
		bwapirequestBase.AddParam("steam_id", steamIDPlayer.m_SteamID.ToString());
		bwapirequestBase.AddParam("steam_auth_ticket", this._HexEncodedTicket);
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			BWUser.LoadCurrentUser(resp);
			this.apiRequestErrorCode = 0;
			this.apiRequestInProgress = false;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWUser.currentUser = null;
			BWLog.Info(error.ToString());
			this.apiRequestErrorCode = error.httpStatusCode;
			this.apiRequestErrorMsg = error.message;
			this.apiRequestInProgress = false;
			this.apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
        bwapirequestBase.SendOwnerCoroutine(this);
	}

	// by exdilin
	private void RequestCurrentUserAuth(string authToken) {
		this.ResetAPIRequestVariables();
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v2/account/login/auth_token");
		bwapirequestBase.AddParam("auth_token", authToken);
		bwapirequestBase.onSuccess = delegate (JObject resp)
		{
			BWUser.LoadCurrentUser(resp);
			this.apiRequestErrorCode = 0;
			this.apiRequestInProgress = false;
		};
		bwapirequestBase.onFailure = delegate (BWAPIRequestError error)
		{
			BWUser.currentUser = null;
			BWLog.Info(error.ToString());
			this.apiRequestErrorCode = error.httpStatusCode;
			this.apiRequestErrorMsg = error.message;
			this.apiRequestInProgress = false;
			this.apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x060029C4 RID: 10692 RVA: 0x001323E0 File Offset: 0x001307E0
	private void RequestCreateUser(CSteamID steamIDPlayer, string personaName, string nickname)
	{
		this.ResetAPIRequestVariables();
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v1/steam_users");
		bwapirequestBase.AddParam("steam_id", steamIDPlayer.m_SteamID.ToString());
		bwapirequestBase.AddParam("steam_auth_ticket", this._HexEncodedTicket);
		bwapirequestBase.AddParam("steam_persona", personaName);
		bwapirequestBase.AddParam("steam_nickname", nickname);
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			BWUser.LoadCurrentUser(resp);
			this.apiRequestErrorCode = 0;
			this.apiRequestInProgress = false;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWUser.currentUser = null;
			BWLog.Info(error.ToString());
			this.apiRequestErrorCode = error.httpStatusCode;
			this.apiRequestErrorMsg = error.message;
			this.apiRequestInProgress = false;
			this.apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x060029C5 RID: 10693 RVA: 0x0013247C File Offset: 0x0013087C
	private void RequestUpdateUsername(string nickname)
	{
		this.ResetAPIRequestVariables();
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v1/steam_current_user/username");
		bwapirequestBase.AddParam("steam_nickname", nickname);
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			BWUser.UpdateCurrentUser(resp);
			this.apiRequestErrorCode = 0;
			this.apiRequestInProgress = false;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWUser.currentUser = null;
			BWLog.Info(error.ToString());
			this.apiRequestErrorCode = error.httpStatusCode;
			this.apiRequestErrorMsg = error.message;
			this.apiRequestInProgress = false;
			this.apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x060029C6 RID: 10694 RVA: 0x001324DC File Offset: 0x001308DC
	private void RequestAppRemoteConfiguration()
	{
		this.ResetAPIRequestVariables();
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/steam-app-remote-configuration");
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			BWAppConfiguration.LoadRemoteConfiguration(resp);
			this.apiRequestErrorCode = 0;
			this.apiRequestInProgress = false;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.ToString());
			this.apiRequestErrorCode = error.httpStatusCode;
			this.apiRequestErrorMsg = error.message;
			this.apiRequestInProgress = false;
			this.apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x060029C7 RID: 10695 RVA: 0x00132530 File Offset: 0x00130930
	private void RequestContentCategories()
	{
		this.ResetAPIRequestVariables();
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/content-categories-no-ip");
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			BWCategory.LoadContentCategories(resp);
			this.apiRequestErrorCode = 0;
			this.apiRequestInProgress = false;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.ToString());
			this.apiRequestErrorCode = error.httpStatusCode;
			this.apiRequestErrorMsg = error.message;
			this.apiRequestInProgress = false;
			this.apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x060029C8 RID: 10696 RVA: 0x00132584 File Offset: 0x00130984
	private void RequestBlockPricing()
	{
		this.ResetAPIRequestVariables();
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/block_items/pricing");
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			BWBlockItemPricing.LoadBlockPrices(resp);
			this.apiRequestErrorCode = 0;
			this.apiRequestInProgress = false;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.ToString());
			this.apiRequestErrorCode = error.httpStatusCode;
			this.apiRequestErrorMsg = error.message;
			this.apiRequestInProgress = false;
			this.apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x060029C9 RID: 10697 RVA: 0x001325D8 File Offset: 0x001309D8
	private void RequestCurrentUserWorlds()
	{
		this.ResetAPIRequestVariables();
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/worlds");
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			BWUser.UpdateCurrentUser(resp);
			this.apiRequestErrorCode = 0;
			this.apiRequestInProgress = false;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.ToString());
			this.apiRequestErrorCode = error.httpStatusCode;
			this.apiRequestErrorMsg = error.message;
			this.apiRequestInProgress = false;
			this.apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x040023FA RID: 9210
	public Text[] statusText;

	// Token: 0x040023FB RID: 9211
	public Text[] versionText;

	// Token: 0x040023FC RID: 9212
	public Animator launchScreenAnimator;

	// Token: 0x040023FD RID: 9213
	private int apiRequestErrorCode;

	// Token: 0x040023FE RID: 9214
	private string apiRequestErrorMsg = string.Empty;

	// Token: 0x040023FF RID: 9215
	private bool apiRequestInProgress;

	// Token: 0x04002400 RID: 9216
	private bool apiRequestConnectionRefused;

	// Token: 0x04002401 RID: 9217
	private int steamworksErrorCode;

	// Token: 0x04002402 RID: 9218
	private bool steamworksRequestInProgress;

	// Token: 0x04002403 RID: 9219
	protected Callback<GetAuthSessionTicketResponse_t> _GetAuthSessionTicketResponse;

	// Token: 0x04002404 RID: 9220
	private string _HexEncodedTicket;

	// Token: 0x04002405 RID: 9221
	private HAuthTicket _HAuthTicket;
}
