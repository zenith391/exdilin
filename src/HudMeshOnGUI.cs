using System.Collections.Generic;
using UnityEngine;

public static class HudMeshOnGUI
{
	public static float depth = 1.8f;

	private const float deltaZ = 0.05f;

	private const int maxStep = 8;

	private static Material _templateMaterial;

	private static HudMeshData _dataSource;

	public static Material templateMaterial
	{
		get
		{
			if (_templateMaterial == null)
			{
				InitTemplateMaterial();
			}
			return _templateMaterial;
		}
	}

	public static HudMeshData dataSource
	{
		get
		{
			if (_dataSource == null)
			{
				InitDataSource();
			}
			return _dataSource;
		}
	}

	public static float GetDepth(int step)
	{
		return depth - (float)(step % 8) * 0.05f;
	}

	public static void Init()
	{
		InitDataSource();
		InitTemplateMaterial();
		HudMeshText.Init();
	}

	private static void InitTemplateMaterial()
	{
		_templateMaterial = new Material(Shader.Find("Blocksworld/HudLabel"));
	}

	private static void InitDataSource()
	{
		string path = ((!Blocksworld.hd) ? "GUI/HudMeshDataSourceSD" : "GUI/HudMeshDataSourceHD");
		_dataSource = Object.Instantiate(Resources.Load<HudMeshData>(path));
	}

	public static void Label(ref HudMeshLabel label, Rect rect, string text, HudMeshStyle style = null, float extraContentHeight = 0f)
	{
		if (style == null)
		{
			style = DefaultLabelStyle();
		}
		if (label == null)
		{
			label = HudMeshLabel.Create(rect, text, style, extraContentHeight);
		}
		label.Refresh(rect, text, extraContentHeight);
	}

	public static void Label(ref HudMeshLabel label, Rect rect, string text, Color color, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = DefaultLabelStyle();
		}
		if (label == null)
		{
			label = HudMeshLabel.Create(rect, text, style, color);
		}
		label.Refresh(rect, text);
	}

	public static void Label(ref HudMeshLabel label, Rect rect, Texture texture, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = DefaultLabelStyle();
		}
		if (label == null)
		{
			label = HudMeshLabel.Create(rect, texture, style);
		}
		label.Refresh(rect, string.Empty, texture);
	}

	public static void Label(List<HudMeshLabel> labelList, int index, Rect rect, string text, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = DefaultLabelStyle();
		}
		HudMeshLabel hudMeshLabel = null;
		if (index < labelList.Count)
		{
			hudMeshLabel = labelList[index];
			if (hudMeshLabel == null)
			{
				hudMeshLabel = (labelList[index] = HudMeshLabel.Create(rect, text, style));
				hudMeshLabel.Refresh(rect, text);
			}
			else
			{
				Label(ref hudMeshLabel, rect, text, style);
			}
			return;
		}
		hudMeshLabel = HudMeshLabel.Create(rect, text, style);
		for (int i = labelList.Count; i < index; i++)
		{
			labelList.Add(null);
		}
		labelList.Add(hudMeshLabel);
		hudMeshLabel.Refresh(rect, text);
	}

	public static void Label(List<HudMeshLabel> labelList, int index, Rect rect, Texture texture, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = DefaultLabelStyle();
		}
		HudMeshLabel hudMeshLabel = null;
		if (index < labelList.Count)
		{
			hudMeshLabel = labelList[index];
			if (hudMeshLabel == null)
			{
				hudMeshLabel = (labelList[index] = HudMeshLabel.Create(rect, texture, style));
				hudMeshLabel.Refresh(rect, string.Empty, texture);
			}
			else
			{
				Label(ref hudMeshLabel, rect, texture, style);
			}
			return;
		}
		hudMeshLabel = HudMeshLabel.Create(rect, texture, style);
		for (int i = labelList.Count; i < index; i++)
		{
			labelList.Add(null);
		}
		labelList.Add(hudMeshLabel);
		hudMeshLabel.Refresh(rect, string.Empty, texture);
	}

	private static HudMeshStyle DefaultLabelStyle()
	{
		return dataSource.GetStyle("label");
	}

	public static bool Button(ref HudMeshButton button, Rect rect, string text, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = DefaultButtonStyle();
		}
		if (button == null)
		{
			button = HudMeshButton.Create(rect, text, style);
		}
		button.Refresh(rect, text);
		return button.IsPressed();
	}

	public static bool Button(ref HudMeshButton button, Rect rect, string text, Color color, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = DefaultButtonStyle();
		}
		if (button == null)
		{
			button = HudMeshButton.Create(rect, text, style, color);
		}
		button.Refresh(rect, text);
		return button.IsPressed();
	}

	public static bool Button(ref HudMeshButton button, Rect rect, Texture texture, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = DefaultButtonStyle();
		}
		if (button == null)
		{
			button = HudMeshButton.Create(rect, string.Empty, style);
		}
		button.Refresh(rect, string.Empty, texture);
		return button.IsPressed();
	}

	public static bool Button(List<HudMeshButton> buttonList, int index, Rect rect, string text, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = DefaultButtonStyle();
		}
		HudMeshButton hudMeshButton = null;
		if (index < buttonList.Count)
		{
			hudMeshButton = buttonList[index];
			if (hudMeshButton == null)
			{
				hudMeshButton = (buttonList[index] = HudMeshButton.Create(rect, text, style));
			}
			return Button(ref hudMeshButton, rect, text, style);
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

	public static bool Button(List<HudMeshButton> buttonList, int index, Rect rect, Texture texture, HudMeshStyle style = null)
	{
		if (style == null)
		{
			style = DefaultButtonStyle();
		}
		HudMeshButton hudMeshButton = null;
		if (index < buttonList.Count)
		{
			hudMeshButton = buttonList[index];
			if (hudMeshButton == null)
			{
				hudMeshButton = (buttonList[index] = HudMeshButton.Create(rect, texture, style));
			}
			return Button(ref hudMeshButton, rect, texture, style);
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

	private static HudMeshStyle DefaultButtonStyle()
	{
		return dataSource.GetStyle("button");
	}
}
