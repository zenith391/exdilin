using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000427 RID: 1063
[RequireComponent(typeof(UIPublishCooldown))]
public class UIPanelElementWorldPublishCooldown : UIPanelElement
{
	// Token: 0x06002DC9 RID: 11721 RVA: 0x00145CAC File Offset: 0x001440AC
	public override void Clear()
	{
		base.Clear();
		UIPublishCooldown component = base.GetComponent<UIPublishCooldown>();
		component.StopAutoRefresh();
		component.worldID = null;
	}

	// Token: 0x06002DCA RID: 11722 RVA: 0x00145CD4 File Offset: 0x001440D4
	public override void Fill(Dictionary<string, string> data)
	{
		UIPublishCooldown component = base.GetComponent<UIPublishCooldown>();
		if (data.ContainsKey(this.worldIDKey))
		{
			component.worldID = data[this.worldIDKey];
		}
		else
		{
			component.worldID = null;
		}
		component.Refresh();
		component.StartAutoRefresh();
	}

	// Token: 0x0400264B RID: 9803
	private string worldIDKey = "world_id";
}
