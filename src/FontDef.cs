using System;
using UnityEngine;

[Serializable]
public class FontDef
{
	public enum Style
	{
		Default,
		DescNorm,
		DescBold,
		Title,
		MunuBar,
		TestFontStyle1,
		TestFontStyle2,
		TestFontStyle3,
		TestFontStyle4,
		TestFontStyle5,
		TestFontStyle6,
		TestFontStyle7,
		TestFontStyle8,
		Count
	}

	public bool useDefault = true;

	public Font font;

	public float fontSize = 20f;

	public FontStyle fontStyle;
}
