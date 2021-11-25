using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020002FB RID: 763
public class UIDebug : MonoBehaviour
{
	// Token: 0x0600227A RID: 8826 RVA: 0x00101204 File Offset: 0x000FF604
	public void SetText(string text)
	{
		base.gameObject.SetActive(!string.IsNullOrEmpty(text));
		this.textField.text = text;
	}

	// Token: 0x04001D81 RID: 7553
	public Text textField;
}
