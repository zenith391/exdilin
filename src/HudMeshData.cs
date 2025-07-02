using System.Collections.Generic;
using UnityEngine;

public class HudMeshData : MonoBehaviour
{
	public HudMeshStyle[] styles;

	private Dictionary<string, HudMeshStyle> styleLookup;

	[HideInInspector]
	public HudMeshStyle defaultStyle;

	[HideInInspector]
	public HudMeshStyle timeParamStyle;

	[HideInInspector]
	public HudMeshStyle intParamStyle;

	[HideInInspector]
	public HudMeshStyle paramDescStyle;

	[HideInInspector]
	public HudMeshStyle inventoryStyle;

	public void Awake()
	{
		styleLookup = new Dictionary<string, HudMeshStyle>();
		for (int i = 0; i < styles.Length; i++)
		{
			HudMeshStyle hudMeshStyle = styles[i];
			styleLookup.Add(hudMeshStyle.id, hudMeshStyle);
			if (hudMeshStyle.id == "Counter")
			{
				defaultStyle = hudMeshStyle;
			}
			if (hudMeshStyle.id == "Int Slider Value")
			{
				intParamStyle = hudMeshStyle;
			}
			if (hudMeshStyle.id == "Time Slider Value")
			{
				timeParamStyle = hudMeshStyle;
			}
			if (hudMeshStyle.id == "Parameter Descriptor")
			{
				paramDescStyle = hudMeshStyle;
			}
			if (hudMeshStyle.id == "Inventory")
			{
				inventoryStyle = hudMeshStyle;
			}
		}
	}

	public HudMeshStyle GetStyle(string id)
	{
		if (styleLookup.TryGetValue(id, out var value))
		{
			return value;
		}
		BWLog.Info("Style: " + id + " not found, using default");
		return defaultStyle;
	}
}
