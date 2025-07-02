using System;
using UnityEngine;

[Serializable]
public class HudMeshStyle
{
	public string id;

	public Texture2D backgroundTexture;

	public Color textColor;

	public Font font;

	public FontStyle fontStyle;

	public int fontSize;

	public TextAnchor alignment;

	public bool wordWrap;

	public bool stretchWidth;

	public bool stretchHeight;

	public RectOffset border;

	public RectOffset padding;

	public HudMeshStyle Clone()
	{
		return new HudMeshStyle
		{
			id = id,
			backgroundTexture = backgroundTexture,
			textColor = textColor,
			font = font,
			fontStyle = fontStyle,
			fontSize = fontSize,
			alignment = alignment,
			wordWrap = wordWrap,
			stretchWidth = stretchWidth,
			stretchHeight = stretchHeight,
			border = border,
			padding = padding
		};
	}
}
