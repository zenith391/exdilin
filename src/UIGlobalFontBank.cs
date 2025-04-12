using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000409 RID: 1033
public class UIGlobalFontBank : MonoBehaviour
{
	// Token: 0x17000247 RID: 583
	// (get) Token: 0x06002D37 RID: 11575 RVA: 0x00142FBC File Offset: 0x001413BC
	public static UIGlobalFontBank Instance
	{
		get
		{
			return UIGlobalFontBank.instance;
		}
	}

	// Token: 0x06002D38 RID: 11576 RVA: 0x00142FC3 File Offset: 0x001413C3
	private void Awake()
	{
		if (UIGlobalFontBank.instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		UIGlobalFontBank.instance = this;
		UIGlobalFontBank.instance.Init();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	// Token: 0x06002D39 RID: 11577 RVA: 0x00142FFC File Offset: 0x001413FC
	public void Init()
	{
		UIGlobalFontBank.fonts = new Dictionary<FontDef.Style, Font>();
		UIGlobalFontBank.fontSizes = new Dictionary<FontDef.Style, float>();
		UIGlobalFontBank.fontStyles = new Dictionary<FontDef.Style, FontStyle>();
		if (this.customFontDefinitions.Count == 0)
		{
			return;
		}
		FontDef fontDef = this.customFontDefinitions[0];
		for (int i = 0; i < 13; i++)
		{
			FontDef.Style key = (FontDef.Style)i;
			if (i < this.customFontDefinitions.Count)
			{
				FontDef fontDef2 = this.customFontDefinitions[i];
				if (fontDef2.font == null || fontDef2.useDefault)
				{
					UIGlobalFontBank.fonts.Add(key, fontDef.font);
					UIGlobalFontBank.fontSizes.Add(key, fontDef.fontSize);
					UIGlobalFontBank.fontStyles.Add(key, fontDef.fontStyle);
				}
				else
				{
					UIGlobalFontBank.fonts.Add(key, fontDef2.font);
					UIGlobalFontBank.fontSizes.Add(key, fontDef2.fontSize);
					UIGlobalFontBank.fontStyles.Add(key, fontDef2.fontStyle);
				}
			}
			else
			{
				UIGlobalFontBank.fonts.Add(key, fontDef.font);
				UIGlobalFontBank.fontSizes.Add(key, fontDef.fontSize);
				UIGlobalFontBank.fontStyles.Add(key, fontDef.fontStyle);
			}
		}
		UIGlobalFontBank.isInitialized = true;
	}

	// Token: 0x040025AE RID: 9646
	private static UIGlobalFontBank instance;

	// Token: 0x040025AF RID: 9647
	public static Dictionary<FontDef.Style, Font> fonts;

	// Token: 0x040025B0 RID: 9648
	public static Dictionary<FontDef.Style, float> fontSizes;

	// Token: 0x040025B1 RID: 9649
	public static Dictionary<FontDef.Style, FontStyle> fontStyles;

	// Token: 0x040025B2 RID: 9650
	public static bool isInitialized;

	// Token: 0x040025B3 RID: 9651
	public List<FontDef> customFontDefinitions;
}
