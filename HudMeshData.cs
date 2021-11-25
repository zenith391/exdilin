using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000195 RID: 405
public class HudMeshData : MonoBehaviour
{
	// Token: 0x060016C3 RID: 5827 RVA: 0x000A2D90 File Offset: 0x000A1190
	public void Awake()
	{
		this.styleLookup = new Dictionary<string, HudMeshStyle>();
		for (int i = 0; i < this.styles.Length; i++)
		{
			HudMeshStyle hudMeshStyle = this.styles[i];
			this.styleLookup.Add(hudMeshStyle.id, hudMeshStyle);
			if (hudMeshStyle.id == "Counter")
			{
				this.defaultStyle = hudMeshStyle;
			}
			if (hudMeshStyle.id == "Int Slider Value")
			{
				this.intParamStyle = hudMeshStyle;
			}
			if (hudMeshStyle.id == "Time Slider Value")
			{
				this.timeParamStyle = hudMeshStyle;
			}
			if (hudMeshStyle.id == "Parameter Descriptor")
			{
				this.paramDescStyle = hudMeshStyle;
			}
			if (hudMeshStyle.id == "Inventory")
			{
				this.inventoryStyle = hudMeshStyle;
			}
		}
	}

	// Token: 0x060016C4 RID: 5828 RVA: 0x000A2E68 File Offset: 0x000A1268
	public HudMeshStyle GetStyle(string id)
	{
		HudMeshStyle result;
		if (this.styleLookup.TryGetValue(id, out result))
		{
			return result;
		}
		BWLog.Info("Style: " + id + " not found, using default");
		return this.defaultStyle;
	}

	// Token: 0x040011BF RID: 4543
	public HudMeshStyle[] styles;

	// Token: 0x040011C0 RID: 4544
	private Dictionary<string, HudMeshStyle> styleLookup;

	// Token: 0x040011C1 RID: 4545
	[HideInInspector]
	public HudMeshStyle defaultStyle;

	// Token: 0x040011C2 RID: 4546
	[HideInInspector]
	public HudMeshStyle timeParamStyle;

	// Token: 0x040011C3 RID: 4547
	[HideInInspector]
	public HudMeshStyle intParamStyle;

	// Token: 0x040011C4 RID: 4548
	[HideInInspector]
	public HudMeshStyle paramDescStyle;

	// Token: 0x040011C5 RID: 4549
	[HideInInspector]
	public HudMeshStyle inventoryStyle;
}
