using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000300 RID: 768
public class UIEditableText : MonoBehaviour
{
	// Token: 0x060022A8 RID: 8872 RVA: 0x00101F5C File Offset: 0x0010035C
	public void Init()
	{
		if (this.texts == null || this.texts.Length == 0)
		{
			Debug.Log("editiable text not setup right ", base.gameObject);
			return;
		}
		this.placeholderText = this.texts[0].text;
		for (int i = 0; i < this.texts.Length; i++)
		{
			this.texts[i].raycastTarget = false;
		}
	}

	// Token: 0x060022A9 RID: 8873 RVA: 0x00101FCC File Offset: 0x001003CC
	public void Set(string str)
	{
		this.currentText = str;
		if (this.texts == null)
		{
			Debug.Log("editiable text not setup right ", base.gameObject);
			return;
		}
		for (int i = 0; i < this.texts.Length; i++)
		{
			this.texts[i].text = str;
		}
	}

	// Token: 0x060022AA RID: 8874 RVA: 0x00102023 File Offset: 0x00100423
	public string Get()
	{
		return this.currentText;
	}

	// Token: 0x17000161 RID: 353
	// (get) Token: 0x060022AB RID: 8875 RVA: 0x0010202B File Offset: 0x0010042B
	public float PreferredWidth
	{
		get
		{
			return this.texts[0].preferredWidth;
		}
	}

	// Token: 0x060022AC RID: 8876 RVA: 0x0010203A File Offset: 0x0010043A
	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	// Token: 0x060022AD RID: 8877 RVA: 0x00102048 File Offset: 0x00100448
	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x060022AE RID: 8878 RVA: 0x00102058 File Offset: 0x00100458
	public void ReplacePlaceholder(string str, string placeholderStr = "placeholder")
	{
		placeholderStr = "{" + placeholderStr + "}";
		string str2 = this.placeholderText.Replace(placeholderStr, str);
		this.Set(str2);
	}

	// Token: 0x04001DA8 RID: 7592
	public Text[] texts;

	// Token: 0x04001DA9 RID: 7593
	private string[] placeHolders;

	// Token: 0x04001DAA RID: 7594
	private string[] staticParts;

	// Token: 0x04001DAB RID: 7595
	private string currentText;

	// Token: 0x04001DAC RID: 7596
	private string placeholderText;
}
