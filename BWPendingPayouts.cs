using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003AC RID: 940
public class BWPendingPayouts
{
	// Token: 0x170001B7 RID: 439
	// (get) Token: 0x060028E0 RID: 10464 RVA: 0x0012C7D3 File Offset: 0x0012ABD3
	// (set) Token: 0x060028E1 RID: 10465 RVA: 0x0012C7DA File Offset: 0x0012ABDA
	public static bool isLoadingPayouts { get; private set; }

	// Token: 0x14000014 RID: 20
	// (add) Token: 0x060028E2 RID: 10466 RVA: 0x0012C7E4 File Offset: 0x0012ABE4
	// (remove) Token: 0x060028E3 RID: 10467 RVA: 0x0012C818 File Offset: 0x0012AC18
	private static event PendingPayoutsEventListener pendingPayoutsDidLoad;

	// Token: 0x14000015 RID: 21
	// (add) Token: 0x060028E4 RID: 10468 RVA: 0x0012C84C File Offset: 0x0012AC4C
	// (remove) Token: 0x060028E5 RID: 10469 RVA: 0x0012C880 File Offset: 0x0012AC80
	private static event PendingPayoutsCollectedEventListener pendingPayoutsCollected;

	// Token: 0x060028E6 RID: 10470 RVA: 0x0012C8B4 File Offset: 0x0012ACB4
	public static void LoadCurrentUserPendingPayouts()
	{
		if (DateTime.UtcNow <= BWPendingPayouts.pendingPayoutsLastSync.AddMinutes(10.0))
		{
			return;
		}
		BWPendingPayouts.isLoadingPayouts = true;
		BWPendingPayouts.pendingPayouts = new List<BWPendingPayout>();
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/pending_payouts");
		bwapirequestBase.onSuccess = delegate(JObject respJson)
		{
			foreach (JObject json in respJson["pending_payouts"].ArrayValue)
			{
				BWPendingPayout item = new BWPendingPayout(json);
				BWPendingPayouts.pendingPayouts.Add(item);
			}
			BWPendingPayouts.pendingPayoutsLastSync = DateTime.UtcNow;
			if (BWPendingPayouts.pendingPayouts.Count > 0 && BWPendingPayouts.pendingPayoutsDidLoad != null)
			{
				BWPendingPayouts.pendingPayoutsDidLoad(BWPendingPayouts.pendingPayouts);
			}
			BWPendingPayouts.isLoadingPayouts = false;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWPendingPayouts.isLoadingPayouts = false;
		};
		bwapirequestBase.Send();
	}

	// Token: 0x060028E7 RID: 10471 RVA: 0x0012C958 File Offset: 0x0012AD58
	public static int TotalCoins()
	{
		int total = 0;
		BWPendingPayouts.pendingPayouts.ForEach(delegate(BWPendingPayout p)
		{
			total += p.coinGrants;
		});
		return total;
	}

	// Token: 0x060028E8 RID: 10472 RVA: 0x0012C990 File Offset: 0x0012AD90
	public static void Collect()
	{
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v1/current_user/collected_payouts");
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (BWPendingPayout bwpendingPayout in BWPendingPayouts.pendingPayouts)
		{
			list.Add(bwpendingPayout.AttributesForCollection());
		}
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{
				"payouts",
				list
			}
		};
		string jsonStr = JSONEncoder.Encode(obj);
		bwapirequestBase.AddJsonParameters(jsonStr);
		bwapirequestBase.onSuccess = delegate(JObject respJson)
		{
			BWLog.Info("Payout collection success");
			if (respJson.ContainsKey("attrs_for_current_user"))
			{
				BWUser.UpdateCurrentUser(respJson["attrs_for_current_user"]);
			}
			if (BWPendingPayouts.pendingPayoutsCollected != null)
			{
				BWPendingPayouts.pendingPayoutsCollected(true);
			}
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error("Payout collection failed: " + error.message);
			BWPendingPayouts.pendingPayoutsCollected(false);
		};
		bwapirequestBase.Send();
	}

	// Token: 0x060028E9 RID: 10473 RVA: 0x0012CA80 File Offset: 0x0012AE80
	public static void AddPendingPayoutsListener(PendingPayoutsEventListener listener)
	{
		BWPendingPayouts.pendingPayoutsDidLoad -= listener;
		BWPendingPayouts.pendingPayoutsDidLoad += listener;
	}

	// Token: 0x060028EA RID: 10474 RVA: 0x0012CA8E File Offset: 0x0012AE8E
	public static void RemovePendingPayoutsListener(PendingPayoutsEventListener listener)
	{
		BWPendingPayouts.pendingPayoutsDidLoad -= listener;
	}

	// Token: 0x060028EB RID: 10475 RVA: 0x0012CA96 File Offset: 0x0012AE96
	public static void AddPendingPayoutsCollectedListener(PendingPayoutsCollectedEventListener listener)
	{
		BWPendingPayouts.pendingPayoutsCollected -= listener;
		BWPendingPayouts.pendingPayoutsCollected += listener;
	}

	// Token: 0x060028EC RID: 10476 RVA: 0x0012CAA4 File Offset: 0x0012AEA4
	public static void RemovePendingPayoutsCollectedListener(PendingPayoutsCollectedEventListener listener)
	{
		BWPendingPayouts.pendingPayoutsCollected -= listener;
	}

	// Token: 0x04002391 RID: 9105
	private static DateTime pendingPayoutsLastSync;

	// Token: 0x04002392 RID: 9106
	public static List<BWPendingPayout> pendingPayouts;
}
