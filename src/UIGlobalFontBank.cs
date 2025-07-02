using System.Collections.Generic;
using UnityEngine;

public class UIGlobalFontBank : MonoBehaviour
{
	private static UIGlobalFontBank instance;

	public static Dictionary<FontDef.Style, Font> fonts;

	public static Dictionary<FontDef.Style, float> fontSizes;

	public static Dictionary<FontDef.Style, FontStyle> fontStyles;

	public static bool isInitialized;

	public List<FontDef> customFontDefinitions;

	public static UIGlobalFontBank Instance => instance;

	private void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		instance.Init();
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Init()
	{
		fonts = new Dictionary<FontDef.Style, Font>();
		fontSizes = new Dictionary<FontDef.Style, float>();
		fontStyles = new Dictionary<FontDef.Style, FontStyle>();
		if (customFontDefinitions.Count == 0)
		{
			return;
		}
		FontDef fontDef = customFontDefinitions[0];
		for (int i = 0; i < 13; i++)
		{
			FontDef.Style key = (FontDef.Style)i;
			if (i < customFontDefinitions.Count)
			{
				FontDef fontDef2 = customFontDefinitions[i];
				if (fontDef2.font == null || fontDef2.useDefault)
				{
					fonts.Add(key, fontDef.font);
					fontSizes.Add(key, fontDef.fontSize);
					fontStyles.Add(key, fontDef.fontStyle);
				}
				else
				{
					fonts.Add(key, fontDef2.font);
					fontSizes.Add(key, fontDef2.fontSize);
					fontStyles.Add(key, fontDef2.fontStyle);
				}
			}
			else
			{
				fonts.Add(key, fontDef.font);
				fontSizes.Add(key, fontDef.fontSize);
				fontStyles.Add(key, fontDef.fontStyle);
			}
		}
		isInitialized = true;
	}
}
