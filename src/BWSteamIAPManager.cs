using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using Steamworks;
using UnityEngine;

public class BWSteamIAPManager : MonoBehaviour
{
	private static BWSteamIAPManager instance;

	private bool itemLoadComplete;

	private bool userLocaleLoadComplete;

	private bool itemLoadFailed;

	private bool userLocaleLoadFailed;

	private ulong userSteamID;

	private string userGameLanguage;

	private string userCountryCode;

	private string userCurrencyCode;

	private Dictionary<string, BWSteamIAPCoinPack> coinPacks;

	protected Callback<MicroTxnAuthorizationResponse_t> CoinPackTransactionResponse;

	public static BWSteamIAPManager Instance => instance;

	public bool loadComplete
	{
		get
		{
			if (itemLoadComplete || itemLoadFailed)
			{
				if (!userLocaleLoadComplete)
				{
					return userLocaleLoadFailed;
				}
				return true;
			}
			return false;
		}
	}

	public void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	public void Init()
	{
		if (BWSteamworksInitializer.Method == BWSteamworksInitializer.LoginMethod.Steam)
		{
			CSteamID steamID = SteamUser.GetSteamID();
			if (!steamID.IsValid())
			{
				BWLog.Error("BWSteamIAPManager: Invalid steam ID");
				return;
			}
			userSteamID = steamID.m_SteamID;
			userGameLanguage = SteamApps.GetCurrentGameLanguage();
		}
		if (string.IsNullOrEmpty(userGameLanguage))
		{
			userGameLanguage = "en";
		}
		else
		{
			userGameLanguage = userGameLanguage.Substring(0, Mathf.Min(2, userGameLanguage.Length));
		}
		coinPacks = new Dictionary<string, BWSteamIAPCoinPack>();
		CoinPackTransactionResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnCoinPackPurchased);
		StartCoroutine(LoadData());
	}

	private IEnumerator LoadData()
	{
		LoadUserLocaleInfo();
		while (!userLocaleLoadComplete && !userLocaleLoadFailed)
		{
			yield return null;
		}
		if (userLocaleLoadFailed)
		{
			itemLoadFailed = true;
		}
		else
		{
			LoadIAPItems();
		}
	}

	private void LoadIAPItems()
	{
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/store/coin_packs");
		bWAPIRequestBase.onSuccess = delegate(JObject respJson)
		{
			List<JObject> arrayValue = respJson["coin_packs"].ArrayValue;
			foreach (JObject item in arrayValue)
			{
				BWSteamIAPCoinPack bWSteamIAPCoinPack = new BWSteamIAPCoinPack(item);
				coinPacks[bWSteamIAPCoinPack.internalIdentifier] = bWSteamIAPCoinPack;
			}
			itemLoadComplete = true;
		};
		bWAPIRequestBase.onFailure = delegate
		{
			itemLoadFailed = true;
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void LoadUserLocaleInfo()
	{
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/steam_current_user/locale");
		bWAPIRequestBase.onSuccess = delegate(JObject respJson)
		{
			userCountryCode = BWJsonHelpers.PropertyIfExists(userCountryCode, "country", respJson);
			userCurrencyCode = BWJsonHelpers.PropertyIfExists(userCurrencyCode, "currency", respJson);
			userLocaleLoadComplete = true;
		};
		bWAPIRequestBase.onFailure = delegate
		{
			userLocaleLoadComplete = true;
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public List<BWSteamIAPCoinPack> GetCoinPacks()
	{
		return new List<BWSteamIAPCoinPack>(coinPacks.Values);
	}

	public void BuyCoinPack(string coinPackInternalIdentifier)
	{
		if (coinPacks.ContainsKey(coinPackInternalIdentifier))
		{
			BWSteamIAPCoinPack bWSteamIAPCoinPack = coinPacks[coinPackInternalIdentifier];
			BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v1/steam_store/purchased_coin_packs");
			bWAPIRequestBase.AddParam("item_id", bWSteamIAPCoinPack.steamIAP_ID.ToString());
			bWAPIRequestBase.AddParam("language", userGameLanguage);
			bWAPIRequestBase.AddParam("currency", userCurrencyCode);
			bWAPIRequestBase.AddParam("item_description", bWSteamIAPCoinPack.label);
			bWAPIRequestBase.onSuccess = delegate
			{
				BWLog.Info("Buy coin pack initiated");
			};
			bWAPIRequestBase.onFailure = delegate
			{
				BWLog.Info("Buy coin pack request failed");
			};
			bWAPIRequestBase.SendOwnerCoroutine(this);
		}
	}

	private void OnCoinPackPurchased(MicroTxnAuthorizationResponse_t pCallback)
	{
		Debug.Log("Coin pack purchase complete");
		string path = ((pCallback.m_bAuthorized == 0) ? "/api/v1/steam_store/purchased_coin_packs/cancel" : "/api/v1/steam_store/purchased_coin_packs/complete");
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", path);
		bWAPIRequestBase.AddParam("order_id", pCallback.m_ulOrderID.ToString());
		bWAPIRequestBase.onSuccess = delegate(JObject respJson)
		{
			if (respJson.ContainsKey("attrs_for_current_user"))
			{
				BWUser.UpdateCurrentUserAndNotifyListeners(respJson["attrs_for_current_user"]);
			}
		};
		bWAPIRequestBase.onFailure = delegate
		{
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}
}
