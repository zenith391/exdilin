using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using Steamworks;
using UnityEngine;

// Token: 0x020003BC RID: 956
public class BWSteamIAPManager : MonoBehaviour
{
	// Token: 0x170001C8 RID: 456
	// (get) Token: 0x060029A5 RID: 10661 RVA: 0x00131AD7 File Offset: 0x0012FED7
	public static BWSteamIAPManager Instance
	{
		get
		{
			return BWSteamIAPManager.instance;
		}
	}

	// Token: 0x060029A6 RID: 10662 RVA: 0x00131ADE File Offset: 0x0012FEDE
	public void Awake()
	{
		if (BWSteamIAPManager.instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		BWSteamIAPManager.instance = this;
	}

	// Token: 0x170001C9 RID: 457
	// (get) Token: 0x060029A7 RID: 10663 RVA: 0x00131B02 File Offset: 0x0012FF02
	public bool loadComplete
	{
		get
		{
			return (this.itemLoadComplete || this.itemLoadFailed) && (this.userLocaleLoadComplete || this.userLocaleLoadFailed);
		}
	}

	// Token: 0x060029A8 RID: 10664 RVA: 0x00131B34 File Offset: 0x0012FF34
	public void Init()
	{
		if (BWSteamworksInitializer.Method == BWSteamworksInitializer.LoginMethod.Steam) {
			CSteamID steamID = SteamUser.GetSteamID();
			if (!steamID.IsValid()) {
				BWLog.Error("BWSteamIAPManager: Invalid steam ID");
				return;
			}
			this.userSteamID = steamID.m_SteamID;
			this.userGameLanguage = SteamApps.GetCurrentGameLanguage();
		}
		if (string.IsNullOrEmpty(this.userGameLanguage))
		{
			this.userGameLanguage = "en";
		}
		else
		{
			this.userGameLanguage = this.userGameLanguage.Substring(0, Mathf.Min(2, this.userGameLanguage.Length));
		}
		this.coinPacks = new Dictionary<string, BWSteamIAPCoinPack>();
		this.CoinPackTransactionResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(new Callback<MicroTxnAuthorizationResponse_t>.DispatchDelegate(this.OnCoinPackPurchased));
		base.StartCoroutine(this.LoadData());
	}

	// Token: 0x060029A9 RID: 10665 RVA: 0x00131BE8 File Offset: 0x0012FFE8
	private IEnumerator LoadData()
	{
		this.LoadUserLocaleInfo();
		while (!this.userLocaleLoadComplete && !this.userLocaleLoadFailed)
		{
			yield return null;
		}
		if (this.userLocaleLoadFailed)
		{
			this.itemLoadFailed = true;
			yield break;
		}
		this.LoadIAPItems();
		yield break;
	}

	// Token: 0x060029AA RID: 10666 RVA: 0x00131C04 File Offset: 0x00130004
	private void LoadIAPItems()
	{
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/store/coin_packs");
		bwapirequestBase.onSuccess = delegate(JObject respJson)
		{
			List<JObject> arrayValue = respJson["coin_packs"].ArrayValue;
			foreach (JObject json in arrayValue)
			{
				BWSteamIAPCoinPack bwsteamIAPCoinPack = new BWSteamIAPCoinPack(json);
				this.coinPacks[bwsteamIAPCoinPack.internalIdentifier] = bwsteamIAPCoinPack;
			}
			this.itemLoadComplete = true;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			this.itemLoadFailed = true;
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x060029AB RID: 10667 RVA: 0x00131C54 File Offset: 0x00130054
	public void LoadUserLocaleInfo()
	{
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/steam_current_user/locale");
		bwapirequestBase.onSuccess = delegate(JObject respJson)
		{
			this.userCountryCode = BWJsonHelpers.PropertyIfExists(this.userCountryCode, "country", respJson);
			this.userCurrencyCode = BWJsonHelpers.PropertyIfExists(this.userCurrencyCode, "currency", respJson);
			this.userLocaleLoadComplete = true;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			this.userLocaleLoadComplete = true;
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x060029AC RID: 10668 RVA: 0x00131CA1 File Offset: 0x001300A1
	public List<BWSteamIAPCoinPack> GetCoinPacks()
	{
		return new List<BWSteamIAPCoinPack>(this.coinPacks.Values);
	}

	// Token: 0x060029AD RID: 10669 RVA: 0x00131CB4 File Offset: 0x001300B4
	public void BuyCoinPack(string coinPackInternalIdentifier)
	{
		if (this.coinPacks.ContainsKey(coinPackInternalIdentifier))
		{
			BWSteamIAPCoinPack bwsteamIAPCoinPack = this.coinPacks[coinPackInternalIdentifier];
			BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v1/steam_store/purchased_coin_packs");
			bwapirequestBase.AddParam("item_id", bwsteamIAPCoinPack.steamIAP_ID.ToString());
			bwapirequestBase.AddParam("language", this.userGameLanguage);
			bwapirequestBase.AddParam("currency", this.userCurrencyCode);
			bwapirequestBase.AddParam("item_description", bwsteamIAPCoinPack.label);
			bwapirequestBase.onSuccess = delegate(JObject resp)
			{
				BWLog.Info("Buy coin pack initiated");
			};
			bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
			{
				BWLog.Info("Buy coin pack request failed");
			};
			bwapirequestBase.SendOwnerCoroutine(this);
		}
	}

	// Token: 0x060029AE RID: 10670 RVA: 0x00131D90 File Offset: 0x00130190
	private void OnCoinPackPurchased(MicroTxnAuthorizationResponse_t pCallback)
	{
		Debug.Log("Coin pack purchase complete");
		string path = (pCallback.m_bAuthorized == 0) ? "/api/v1/steam_store/purchased_coin_packs/cancel" : "/api/v1/steam_store/purchased_coin_packs/complete";
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", path);
		bwapirequestBase.AddParam("order_id", pCallback.m_ulOrderID.ToString());
		bwapirequestBase.onSuccess = delegate(JObject respJson)
		{
			if (respJson.ContainsKey("attrs_for_current_user"))
			{
				BWUser.UpdateCurrentUserAndNotifyListeners(respJson["attrs_for_current_user"]);
			}
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x040023E4 RID: 9188
	private static BWSteamIAPManager instance;

	// Token: 0x040023E5 RID: 9189
	private bool itemLoadComplete;

	// Token: 0x040023E6 RID: 9190
	private bool userLocaleLoadComplete;

	// Token: 0x040023E7 RID: 9191
	private bool itemLoadFailed;

	// Token: 0x040023E8 RID: 9192
	private bool userLocaleLoadFailed;

	// Token: 0x040023E9 RID: 9193
	private ulong userSteamID;

	// Token: 0x040023EA RID: 9194
	private string userGameLanguage;

	// Token: 0x040023EB RID: 9195
	private string userCountryCode;

	// Token: 0x040023EC RID: 9196
	private string userCurrencyCode;

	// Token: 0x040023ED RID: 9197
	private Dictionary<string, BWSteamIAPCoinPack> coinPacks;

	// Token: 0x040023EE RID: 9198
	protected Callback<MicroTxnAuthorizationResponse_t> CoinPackTransactionResponse;
}
