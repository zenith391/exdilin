using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIGlobalFont : MonoBehaviour
{
	public FontDef.Style fontBankStyle;

	public bool useFontBankSize;

	public bool useFontBankStyle;

	private IEnumerator Start()
	{
		while (!UIGlobalFontBank.isInitialized)
		{
			yield return null;
		}
		UpdateFont();
	}

	public void UpdateFont()
	{
		Text component = GetComponent<Text>();
		if (component != null)
		{
			component.font = UIGlobalFontBank.fonts[fontBankStyle];
			if (useFontBankSize)
			{
				component.fontSize = (int)UIGlobalFontBank.fontSizes[fontBankStyle];
			}
			if (useFontBankStyle)
			{
				component.fontStyle = UIGlobalFontBank.fontStyles[fontBankStyle];
			}
		}
	}
}
