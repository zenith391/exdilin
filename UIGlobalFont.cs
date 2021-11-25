using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000408 RID: 1032
public class UIGlobalFont : MonoBehaviour
{
	// Token: 0x06002D34 RID: 11572 RVA: 0x00142E80 File Offset: 0x00141280
	private IEnumerator Start()
	{
		while (!UIGlobalFontBank.isInitialized)
		{
			yield return null;
		}
		this.UpdateFont();
		yield break;
	}

	// Token: 0x06002D35 RID: 11573 RVA: 0x00142E9C File Offset: 0x0014129C
	public void UpdateFont()
	{
		Text component = base.GetComponent<Text>();
		if (component != null)
		{
			component.font = UIGlobalFontBank.fonts[this.fontBankStyle];
			if (this.useFontBankSize)
			{
				component.fontSize = (int)UIGlobalFontBank.fontSizes[this.fontBankStyle];
			}
			if (this.useFontBankStyle)
			{
				component.fontStyle = UIGlobalFontBank.fontStyles[this.fontBankStyle];
			}
		}
	}

	// Token: 0x040025AB RID: 9643
	public FontDef.Style fontBankStyle;

	// Token: 0x040025AC RID: 9644
	public bool useFontBankSize;

	// Token: 0x040025AD RID: 9645
	public bool useFontBankStyle;
}
