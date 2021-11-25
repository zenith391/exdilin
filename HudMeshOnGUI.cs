using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000198 RID: 408
public static class HudMeshOnGUI
{
	// Token: 0x060016D4 RID: 5844 RVA: 0x000A410A File Offset: 0x000A250A
	public static float GetDepth(int step)
	{
		return HudMeshOnGUI.depth - (float)(step % 8) * 0.05f;
	}

	// Token: 0x1700006A RID: 106
	// (get) Token: 0x060016D5 RID: 5845 RVA: 0x000A411C File Offset: 0x000A251C
	public static Material templateMaterial
	{
		get
		{
			if (HudMeshOnGUI._templateMaterial == null)
			{
				HudMeshOnGUI.InitTemplateMaterial();
			}
			return HudMeshOnGUI._templateMaterial;
		}
	}

	// Token: 0x1700006B RID: 107
	// (get) Token: 0x060016D6 RID: 5846 RVA: 0x000A4138 File Offset: 0x000A2538
	public static HudMeshData dataSource
	{
		get
		{
			if (HudMeshOnGUI._dataSource == null)
			{
				HudMeshOnGUI.InitDataSource();
			}
			return HudMeshOnGUI._dataSource;
		}
	}

	// Token: 0x060016D7 RID: 5847 RVA: 0x000A4154 File Offset: 0x000A2554
	public static void Init()
	{
		HudMeshOnGUI.InitDataSource();
		HudMeshOnGUI.InitTemplateMaterial();
		HudMeshText.Init();
	}

	// Token: 0x060016D8 RID: 5848 RVA: 0x000A4165 File Offset: 0x000A2565
	private static void InitTemplateMaterial()
	{
		HudMeshOnGUI._templateMaterial = new Material(Shader.Find("Blocksworld/HudLabel"));
	}

	// Token: 0x060016D9 RID: 5849 RVA: 0x000A417C File Offset: 0x000A257C
	private static void InitDataSource()
	{
		string path = (!Blocksworld.hd) ? "GUI/HudMeshDataSourceSD" : "GUI/HudMeshDataSourceHD";
		HudMeshOnGUI._dataSource = UnityEngine.Object.Instantiate<HudMeshData>(Resources.Load<HudMeshData>(path));
	}

	// Token: 0x060016DA RID: 5850 RVA: 0x000A41B3 File Offset: 0x000A25B3
	public static void Label(ref HudMeshLabel label, Rect rect, string text, HudMeshStyle style = null, float extraContentHeight = 0f)
	{
		if (style == null)
		{
			style = HudMeshOnGUI.DefaultLabelStyle();
		}
		if (label == null)
		{
			label = HudMeshLabel.Create(rect, text, style, extraContentHeight);
		}
		label.Refresh(rect, text, extraContentHeight);
	}

	// Token: 0x060016DB RID: 5851 RVA: 0x000A41E6 File Offset: 0x000A25E6
	public static void Label(ref HudMeshLabel label, Rect rect, string text, Color color, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = HudMeshOnGUI.DefaultLabelStyle();
		}
		if (label == null)
		{
			label = HudMeshLabel.Create(rect, text, style, color);
		}
		label.Refresh(rect, text, 0f);
	}

	// Token: 0x060016DC RID: 5852 RVA: 0x000A421D File Offset: 0x000A261D
	public static void Label(ref HudMeshLabel label, Rect rect, Texture texture, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = HudMeshOnGUI.DefaultLabelStyle();
		}
		if (label == null)
		{
			label = HudMeshLabel.Create(rect, texture, style);
		}
		label.Refresh(rect, string.Empty, texture);
	}

	// Token: 0x060016DD RID: 5853 RVA: 0x000A4254 File Offset: 0x000A2654
	public static void Label(List<HudMeshLabel> labelList, int index, Rect rect, string text, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = HudMeshOnGUI.DefaultLabelStyle();
		}
		HudMeshLabel hudMeshLabel = null;
		if (index < labelList.Count)
		{
			hudMeshLabel = labelList[index];
			if (hudMeshLabel == null)
			{
				hudMeshLabel = HudMeshLabel.Create(rect, text, style, 0f);
				labelList[index] = hudMeshLabel;
				hudMeshLabel.Refresh(rect, text, 0f);
				return;
			}
			HudMeshOnGUI.Label(ref hudMeshLabel, rect, text, style, 0f);
		}
		else
		{
			hudMeshLabel = HudMeshLabel.Create(rect, text, style, 0f);
			for (int i = labelList.Count; i < index; i++)
			{
				labelList.Add(null);
			}
			labelList.Add(hudMeshLabel);
			hudMeshLabel.Refresh(rect, text, 0f);
		}
	}

	// Token: 0x060016DE RID: 5854 RVA: 0x000A430C File Offset: 0x000A270C
	public static void Label(List<HudMeshLabel> labelList, int index, Rect rect, Texture texture, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = HudMeshOnGUI.DefaultLabelStyle();
		}
		HudMeshLabel hudMeshLabel = null;
		if (index < labelList.Count)
		{
			hudMeshLabel = labelList[index];
			if (hudMeshLabel == null)
			{
				hudMeshLabel = HudMeshLabel.Create(rect, texture, style);
				labelList[index] = hudMeshLabel;
				hudMeshLabel.Refresh(rect, string.Empty, texture);
				return;
			}
			HudMeshOnGUI.Label(ref hudMeshLabel, rect, texture, style);
		}
		else
		{
			hudMeshLabel = HudMeshLabel.Create(rect, texture, style);
			for (int i = labelList.Count; i < index; i++)
			{
				labelList.Add(null);
			}
			labelList.Add(hudMeshLabel);
			hudMeshLabel.Refresh(rect, string.Empty, texture);
		}
	}

	// Token: 0x060016DF RID: 5855 RVA: 0x000A43B5 File Offset: 0x000A27B5
	private static HudMeshStyle DefaultLabelStyle()
	{
		return HudMeshOnGUI.dataSource.GetStyle("label");
	}

	// Token: 0x060016E0 RID: 5856 RVA: 0x000A43C6 File Offset: 0x000A27C6
	public static bool Button(ref HudMeshButton button, Rect rect, string text, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = HudMeshOnGUI.DefaultButtonStyle();
		}
		if (button == null)
		{
			button = HudMeshButton.Create(rect, text, style);
		}
		button.Refresh(rect, text);
		return button.IsPressed();
	}

	// Token: 0x060016E1 RID: 5857 RVA: 0x000A43FC File Offset: 0x000A27FC
	public static bool Button(ref HudMeshButton button, Rect rect, string text, Color color, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = HudMeshOnGUI.DefaultButtonStyle();
		}
		if (button == null)
		{
			button = HudMeshButton.Create(rect, text, style, color);
		}
		button.Refresh(rect, text);
		return button.IsPressed();
	}

	// Token: 0x060016E2 RID: 5858 RVA: 0x000A4435 File Offset: 0x000A2835
	public static bool Button(ref HudMeshButton button, Rect rect, Texture texture, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = HudMeshOnGUI.DefaultButtonStyle();
		}
		if (button == null)
		{
			button = HudMeshButton.Create(rect, string.Empty, style);
		}
		button.Refresh(rect, string.Empty, texture);
		return button.IsPressed();
	}

	// Token: 0x060016E3 RID: 5859 RVA: 0x000A4474 File Offset: 0x000A2874
	public static bool Button(List<HudMeshButton> buttonList, int index, Rect rect, string text, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = HudMeshOnGUI.DefaultButtonStyle();
		}
		HudMeshButton hudMeshButton = null;
		if (index < buttonList.Count)
		{
			hudMeshButton = buttonList[index];
			if (hudMeshButton == null)
			{
				hudMeshButton = HudMeshButton.Create(rect, text, style);
				buttonList[index] = hudMeshButton;
			}
			return HudMeshOnGUI.Button(ref hudMeshButton, rect, text, style);
		}
		hudMeshButton = HudMeshButton.Create(rect, text, style);
		for (int i = buttonList.Count; i < index; i++)
		{
			buttonList.Add(null);
		}
		if (hudMeshButton == null)
		{
			BWLog.Error("Failed to create button");
			return false;
		}
		buttonList.Add(hudMeshButton);
		hudMeshButton.Refresh(rect, text);
		return hudMeshButton.IsPressed();
	}

	// Token: 0x060016E4 RID: 5860 RVA: 0x000A4524 File Offset: 0x000A2924
	public static bool Button(List<HudMeshButton> buttonList, int index, Rect rect, Texture texture, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = HudMeshOnGUI.DefaultButtonStyle();
		}
		HudMeshButton hudMeshButton = null;
		if (index < buttonList.Count)
		{
			hudMeshButton = buttonList[index];
			if (hudMeshButton == null)
			{
				hudMeshButton = HudMeshButton.Create(rect, texture, style);
				buttonList[index] = hudMeshButton;
			}
			return HudMeshOnGUI.Button(ref hudMeshButton, rect, texture, style);
		}
		hudMeshButton = HudMeshButton.Create(rect, texture, style);
		for (int i = buttonList.Count; i < index; i++)
		{
			buttonList.Add(null);
		}
		buttonList.Add(hudMeshButton);
		hudMeshButton.Refresh(rect, string.Empty, texture);
		return hudMeshButton.IsPressed();
	}

	// Token: 0x060016E5 RID: 5861 RVA: 0x000A45C1 File Offset: 0x000A29C1
	private static HudMeshStyle DefaultButtonStyle()
	{
		return HudMeshOnGUI.dataSource.GetStyle("button");
	}

	// Token: 0x040011DE RID: 4574
	public static float depth = 1.8f;

	// Token: 0x040011DF RID: 4575
	private const float deltaZ = 0.05f;

	// Token: 0x040011E0 RID: 4576
	private const int maxStep = 8;

	// Token: 0x040011E1 RID: 4577
	private static Material _templateMaterial;

	// Token: 0x040011E2 RID: 4578
	private static HudMeshData _dataSource;
}
