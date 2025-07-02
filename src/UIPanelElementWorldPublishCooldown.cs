using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIPublishCooldown))]
public class UIPanelElementWorldPublishCooldown : UIPanelElement
{
	private string worldIDKey = "world_id";

	public override void Clear()
	{
		base.Clear();
		UIPublishCooldown component = GetComponent<UIPublishCooldown>();
		component.StopAutoRefresh();
		component.worldID = null;
	}

	public override void Fill(Dictionary<string, string> data)
	{
		UIPublishCooldown component = GetComponent<UIPublishCooldown>();
		if (data.ContainsKey(worldIDKey))
		{
			component.worldID = data[worldIDKey];
		}
		else
		{
			component.worldID = null;
		}
		component.Refresh();
		component.StartAutoRefresh();
	}
}
