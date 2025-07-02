using System;
using System.Collections.Generic;
using SimpleJSON;

public class BWPendingPayouts
{
	private static DateTime pendingPayoutsLastSync;

	public static List<BWPendingPayout> pendingPayouts;

	public static bool isLoadingPayouts { get; private set; }

	private static event PendingPayoutsEventListener pendingPayoutsDidLoad;

	private static event PendingPayoutsCollectedEventListener pendingPayoutsCollected;

	public static void LoadCurrentUserPendingPayouts()
	{
		if (DateTime.UtcNow <= pendingPayoutsLastSync.AddMinutes(10.0))
		{
			return;
		}
		isLoadingPayouts = true;
		pendingPayouts = new List<BWPendingPayout>();
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/pending_payouts");
		bWAPIRequestBase.onSuccess = delegate(JObject respJson)
		{
			foreach (JObject item2 in respJson["pending_payouts"].ArrayValue)
			{
				BWPendingPayout item = new BWPendingPayout(item2);
				pendingPayouts.Add(item);
			}
			pendingPayoutsLastSync = DateTime.UtcNow;
			if (pendingPayouts.Count > 0 && BWPendingPayouts.pendingPayoutsDidLoad != null)
			{
				BWPendingPayouts.pendingPayoutsDidLoad(pendingPayouts);
			}
			isLoadingPayouts = false;
		};
		bWAPIRequestBase.onFailure = delegate
		{
			isLoadingPayouts = false;
		};
		bWAPIRequestBase.Send();
	}

	public static int TotalCoins()
	{
		int total = 0;
		pendingPayouts.ForEach(delegate(BWPendingPayout p)
		{
			total += p.coinGrants;
		});
		return total;
	}

	public static void Collect()
	{
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v1/current_user/collected_payouts");
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (BWPendingPayout pendingPayout in pendingPayouts)
		{
			list.Add(pendingPayout.AttributesForCollection());
		}
		Dictionary<string, object> obj = new Dictionary<string, object> { { "payouts", list } };
		string jsonStr = JSONEncoder.Encode(obj);
		bWAPIRequestBase.AddJsonParameters(jsonStr);
		bWAPIRequestBase.onSuccess = delegate(JObject respJson)
		{
			BWLog.Info("Payout collection success");
			if (respJson.ContainsKey("attrs_for_current_user"))
			{
				BWUser.UpdateCurrentUser(respJson["attrs_for_current_user"]);
			}
			if (BWPendingPayouts.pendingPayoutsCollected != null)
			{
				BWPendingPayouts.pendingPayoutsCollected(success: true);
			}
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error("Payout collection failed: " + error.message);
			BWPendingPayouts.pendingPayoutsCollected(success: false);
		};
		bWAPIRequestBase.Send();
	}

	public static void AddPendingPayoutsListener(PendingPayoutsEventListener listener)
	{
		pendingPayoutsDidLoad -= listener;
		pendingPayoutsDidLoad += listener;
	}

	public static void RemovePendingPayoutsListener(PendingPayoutsEventListener listener)
	{
		pendingPayoutsDidLoad -= listener;
	}

	public static void AddPendingPayoutsCollectedListener(PendingPayoutsCollectedEventListener listener)
	{
		pendingPayoutsCollected -= listener;
		pendingPayoutsCollected += listener;
	}

	public static void RemovePendingPayoutsCollectedListener(PendingPayoutsCollectedEventListener listener)
	{
		pendingPayoutsCollected -= listener;
	}
}
