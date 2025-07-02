using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Exdilin;
using ModExdilin;
using SimpleJSON;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BWSteamworksInitializer : MonoBehaviour
{
	public enum LoginMethod
	{
		Steam,
		AuthToken
	}

	public static LoginMethod Method;

	public Text[] statusText;

	public Text[] versionText;

	public Animator launchScreenAnimator;

	private int apiRequestErrorCode;

	private string apiRequestErrorMsg = string.Empty;

	private bool apiRequestInProgress;

	private bool apiRequestConnectionRefused;

	private int steamworksErrorCode;

	private bool steamworksRequestInProgress;

	protected Callback<GetAuthSessionTicketResponse_t> _GetAuthSessionTicketResponse;

	private string _HexEncodedTicket;

	private HAuthTicket _HAuthTicket;

	private void SetVersionInfoWithMessage(string message)
	{
		string text = BWEnvConfig.BLOCKSWORLD_VERSION;
		if (BWEnvConfig.BLOCKSWORLD_ENVIRONMENT != "production")
		{
			text = text + " (with Exdilin " + ExdilinMod.VersionStatic.ToString() + ")";
		}
		if (!string.IsNullOrEmpty(message))
		{
			text = text + " (" + message + ")";
		}
		if (versionText != null)
		{
			for (int i = 0; i < versionText.Length; i++)
			{
				versionText[i].text = text;
			}
		}
	}

	public void UpdateText(string newText)
	{
		if (statusText != null)
		{
			for (int i = 0; i < statusText.Length; i++)
			{
				statusText[i].text = newText;
			}
		}
	}

	private void Awake()
	{
		string versionInfoWithMessage = string.Empty;
		if (BWEnvConfig.Flags.ContainsKey("DEBUG_CONSOLE") && BWEnvConfig.Flags["DEBUG_CONSOLE"])
		{
			versionInfoWithMessage = "with debug console";
		}
		SetVersionInfoWithMessage(versionInfoWithMessage);
		UpdateText("Loading...");
		_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnGetAuthSessionTicketResponse);
		_HAuthTicket = HAuthTicket.Invalid;
	}

	private void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback)
	{
		steamworksErrorCode = (int)pCallback.m_eResult;
		steamworksRequestInProgress = false;
	}

	private void Stop()
	{
		UnityEngine.Debug.Log("Stopping...");
		if (_HAuthTicket != HAuthTicket.Invalid)
		{
			SteamUser.CancelAuthTicket(_HAuthTicket);
			_HAuthTicket = HAuthTicket.Invalid;
		}
	}

	private void LaunchGame()
	{
		UpdateText("Starting Blocksworld...");
		SceneManager.LoadScene("BlocksworldMain", LoadSceneMode.Additive);
		StartCoroutine(FadeOutLaunchScreen());
	}

	private IEnumerator FadeOutLaunchScreen()
	{
		while (BWStandalone.Instance == null || !BWStandalone.Instance.menuLoaded)
		{
			yield return null;
		}
		if (launchScreenAnimator != null)
		{
			launchScreenAnimator.SetTrigger("Hide");
		}
	}

	public string GetExternalAuthToken()
	{
		string text = Application.dataPath + "/..";
		string path = text + "/auth_token.txt";
		if (File.Exists(path))
		{
			string result = File.ReadAllText(path);
			File.Delete(path);
			return result;
		}
		return null;
	}

	private IEnumerator Start()
	{
		yield return null;
		string extToken = GetExternalAuthToken();
		bool flag = extToken == null;
		Method = ((!flag) ? LoginMethod.AuthToken : LoginMethod.Steam);
		string nickname = "";
		if (BWEnvConfig.Flags["DEMO_USER"])
		{
			UpdateText("Be our guest!");
			yield return new WaitForSeconds(1f);
			BWUser.SetupDemoCurrentUser();
		}
		else if (flag)
		{
			while (!SteamManager.Initialized)
			{
				UpdateText("Launch Steam and try again...");
				yield return new WaitForSeconds(5f);
			}
			CSteamID steamIDPlayer = SteamUser.GetSteamID();
			CSteamID appOwner = SteamApps.GetAppOwner();
			if (!steamIDPlayer.IsValid() || !appOwner.IsValid())
			{
				UpdateText("Invalid Steam ID");
				yield break;
			}
			if (appOwner != steamIDPlayer)
			{
				while (true)
				{
					UpdateText("Blocksworld account owner not logged in!");
					yield return new WaitForSeconds(5f);
				}
			}
			string personaName = SteamFriends.GetPersonaName();
			UpdateText("Welcome " + personaName + "!");
			yield return new WaitForSeconds(1f);
			UpdateText("Starting session...");
			yield return null;
			steamworksRequestInProgress = true;
			byte[] ticket = new byte[1024];
			uint pcbTicket = 0u;
			_HAuthTicket = SteamUser.GetAuthSessionTicket(ticket, 1024, out pcbTicket);
			while (steamworksRequestInProgress)
			{
				yield return new WaitForSeconds(0.2f);
			}
			if (steamworksErrorCode != 1 || pcbTicket == 0)
			{
				UpdateText("Unable to start session.  Please try again later.");
				yield break;
			}
			byte[] array = new byte[pcbTicket];
			Array.Copy(ticket, 0L, array, 0L, pcbTicket);
			_HexEncodedTicket = BitConverter.ToString(array).Replace("-", string.Empty);
			BWLog.Info("Generated Steam ticket: " + _HexEncodedTicket);
			yield return null;
			nickname = SteamFriends.GetPlayerNickname(steamIDPlayer);
			if (string.IsNullOrEmpty(nickname))
			{
				nickname = personaName;
			}
			UpdateText("Logging in as " + nickname + "...");
			yield return null;
			RequestCurrentUser(steamIDPlayer);
			while (apiRequestInProgress)
			{
				yield return new WaitForSeconds(0.5f);
			}
			if (apiRequestErrorCode == 404)
			{
				UpdateText("Creating Blocksworld account for " + nickname);
				RequestCreateUser(steamIDPlayer, personaName, nickname);
				yield return null;
			}
			else
			{
				if (apiRequestConnectionRefused || apiRequestErrorCode != 0)
				{
					UpdateText("Unable to login.  Please try again later.");
					SetVersionInfoWithMessage("error " + apiRequestErrorCode + " " + apiRequestErrorMsg);
					yield break;
				}
				UpdateText("Logged in!");
				yield return null;
				if (BWUser.currentUser.username != nickname)
				{
					RequestUpdateUsername(nickname);
					while (apiRequestInProgress)
					{
						yield return new WaitForSeconds(0.5f);
					}
					if (apiRequestConnectionRefused || apiRequestErrorCode != 0)
					{
						UpdateText("Unable to change Blocksworld username!");
						yield return new WaitForSeconds(2f);
					}
					UpdateText("Changed username!");
					yield return null;
				}
			}
			while (apiRequestInProgress)
			{
				yield return new WaitForSeconds(0.5f);
			}
			if (apiRequestConnectionRefused || apiRequestErrorCode != 0)
			{
				UpdateText("Unable to connect to server.  Please try again later.");
				SetVersionInfoWithMessage("error " + apiRequestErrorCode + " " + apiRequestErrorMsg);
				yield break;
			}
		}
		else
		{
			UpdateText("Starting launcher session...");
			yield return null;
			nickname = "todo";
			UpdateText("Logging in with token " + extToken + "...");
			yield return null;
			RequestCurrentUserAuth(extToken);
			while (apiRequestInProgress)
			{
				yield return new WaitForSeconds(0.5f);
			}
			if (apiRequestErrorCode == 404)
			{
				UpdateText("Invalid authentication token. Try to restart the launcher.");
				yield break;
			}
			if (apiRequestConnectionRefused || apiRequestErrorCode != 0)
			{
				UpdateText("Unable to login. Please try again later.");
				SetVersionInfoWithMessage("error " + apiRequestErrorCode + " " + apiRequestErrorMsg);
				yield break;
			}
			UpdateText("Logged in!");
			yield return null;
		}
		UpdateText("Downloading " + nickname + "'s worlds...");
		yield return null;
		RequestCurrentUserWorlds();
		while (apiRequestInProgress)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (apiRequestConnectionRefused || apiRequestErrorCode != 0)
		{
			UpdateText("Unable to download worlds.  Please try again later.");
			SetVersionInfoWithMessage("error " + apiRequestErrorCode + " " + apiRequestErrorMsg);
			yield break;
		}
		UpdateText("Fetching remote configuration...");
		yield return null;
		RequestAppRemoteConfiguration();
		while (apiRequestInProgress)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (apiRequestConnectionRefused || apiRequestErrorCode != 0)
		{
			UpdateText("Unable to fetch remote configuration.  Please try again later.");
			SetVersionInfoWithMessage("error " + apiRequestErrorCode + " " + apiRequestErrorMsg);
			yield break;
		}
		UpdateText("Downloading categories...");
		yield return null;
		RequestContentCategories();
		while (apiRequestInProgress)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (apiRequestConnectionRefused || apiRequestErrorCode != 0)
		{
			UpdateText("Unable to download categories.  Please try again later.");
			SetVersionInfoWithMessage("error " + apiRequestErrorCode + " " + apiRequestErrorMsg);
			yield break;
		}
		UpdateText("Downloading block data...");
		yield return null;
		RequestBlockPricing();
		while (apiRequestInProgress)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (apiRequestConnectionRefused || apiRequestErrorCode != 0)
		{
			UpdateText("Unable to download block data.  Please try again later.");
			SetVersionInfoWithMessage("error " + apiRequestErrorCode + " " + apiRequestErrorMsg);
			yield break;
		}
		UpdateText("Loading mods..");
		yield return ModLoader.LoadModsCoroutine(delegate(string id, string stage)
		{
			if (stage == "load")
			{
				UpdateText("Loading mods.. (" + id + ")");
			}
			else if (stage == "preinit")
			{
				UpdateText("Pre-initing mods.. (" + id + ")");
			}
		});
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v2/exdilin/configuration?version=" + ExdilinMod.VersionStatic.ToString());
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			Exdilin.Version versionStatic = ExdilinMod.VersionStatic;
			int num = (versionStatic.Major << 16) | (versionStatic.Minor << 8) | versionStatic.Patch;
			UnityEngine.Debug.Log("Latest version ID: " + resp["latest_version_id"].IntValue + ", current version ID: " + num);
			if (resp["latest_version_id"].IntValue > num)
			{
				ModLoader.OverlaysAvailable += delegate
				{
					BWStandalone.Overlays.ShowConfirmationDialog("Update", "Update Exdilin to " + resp["latest_version"].StringValue + " ?", delegate
					{
						Process.Start("https://bwsecondary.ddns.net/mods/0");
					}, "Update!", "No");
				};
			}
		};
		bWAPIRequestBase.onFailure = delegate
		{
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
		LaunchGame();
		yield return null;
	}

	private void ResetAPIRequestVariables()
	{
		apiRequestErrorCode = 0;
		apiRequestErrorMsg = string.Empty;
		apiRequestInProgress = true;
		apiRequestConnectionRefused = false;
	}

	private void RequestCurrentUser(CSteamID steamIDPlayer)
	{
		ResetAPIRequestVariables();
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/steam_current_user");
		bWAPIRequestBase.AddParam("steam_id", steamIDPlayer.m_SteamID.ToString());
		bWAPIRequestBase.AddParam("steam_auth_ticket", _HexEncodedTicket);
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			BWUser.LoadCurrentUser(resp);
			apiRequestErrorCode = 0;
			apiRequestInProgress = false;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWUser.currentUser = null;
			BWLog.Info(error.ToString());
			apiRequestErrorCode = error.httpStatusCode;
			apiRequestErrorMsg = error.message;
			apiRequestInProgress = false;
			apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	private void RequestCurrentUserAuth(string authToken)
	{
		ResetAPIRequestVariables();
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v2/account/login/auth_token");
		bWAPIRequestBase.AddParam("auth_token", authToken);
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			BWUser.LoadCurrentUser(resp);
			apiRequestErrorCode = 0;
			apiRequestInProgress = false;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWUser.currentUser = null;
			BWLog.Info(error.ToString());
			apiRequestErrorCode = error.httpStatusCode;
			apiRequestErrorMsg = error.message;
			apiRequestInProgress = false;
			apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	private void RequestCreateUser(CSteamID steamIDPlayer, string personaName, string nickname)
	{
		ResetAPIRequestVariables();
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v1/steam_users");
		bWAPIRequestBase.AddParam("steam_id", steamIDPlayer.m_SteamID.ToString());
		bWAPIRequestBase.AddParam("steam_auth_ticket", _HexEncodedTicket);
		bWAPIRequestBase.AddParam("steam_persona", personaName);
		bWAPIRequestBase.AddParam("steam_nickname", nickname);
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			BWUser.LoadCurrentUser(resp);
			apiRequestErrorCode = 0;
			apiRequestInProgress = false;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWUser.currentUser = null;
			BWLog.Info(error.ToString());
			apiRequestErrorCode = error.httpStatusCode;
			apiRequestErrorMsg = error.message;
			apiRequestInProgress = false;
			apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	private void RequestUpdateUsername(string nickname)
	{
		ResetAPIRequestVariables();
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v1/steam_current_user/username");
		bWAPIRequestBase.AddParam("steam_nickname", nickname);
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			BWUser.UpdateCurrentUser(resp);
			apiRequestErrorCode = 0;
			apiRequestInProgress = false;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWUser.currentUser = null;
			BWLog.Info(error.ToString());
			apiRequestErrorCode = error.httpStatusCode;
			apiRequestErrorMsg = error.message;
			apiRequestInProgress = false;
			apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	private void RequestAppRemoteConfiguration()
	{
		ResetAPIRequestVariables();
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/steam-app-remote-configuration");
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			BWAppConfiguration.LoadRemoteConfiguration(resp);
			apiRequestErrorCode = 0;
			apiRequestInProgress = false;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.ToString());
			apiRequestErrorCode = error.httpStatusCode;
			apiRequestErrorMsg = error.message;
			apiRequestInProgress = false;
			apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	private void RequestContentCategories()
	{
		ResetAPIRequestVariables();
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/content-categories-no-ip");
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			BWCategory.LoadContentCategories(resp);
			apiRequestErrorCode = 0;
			apiRequestInProgress = false;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.ToString());
			apiRequestErrorCode = error.httpStatusCode;
			apiRequestErrorMsg = error.message;
			apiRequestInProgress = false;
			apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	private void RequestBlockPricing()
	{
		ResetAPIRequestVariables();
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/block_items/pricing");
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			BWBlockItemPricing.LoadBlockPrices(resp);
			apiRequestErrorCode = 0;
			apiRequestInProgress = false;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.ToString());
			apiRequestErrorCode = error.httpStatusCode;
			apiRequestErrorMsg = error.message;
			apiRequestInProgress = false;
			apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	private void RequestCurrentUserWorlds()
	{
		ResetAPIRequestVariables();
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/worlds");
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			BWUser.UpdateCurrentUser(resp);
			apiRequestErrorCode = 0;
			apiRequestInProgress = false;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.ToString());
			apiRequestErrorCode = error.httpStatusCode;
			apiRequestErrorMsg = error.message;
			apiRequestInProgress = false;
			apiRequestConnectionRefused = error.message.StartsWith("Failed to connect");
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}
}
