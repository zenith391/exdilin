using System;
using UnityEngine;

// Token: 0x0200031B RID: 795
public class UISpeedometer : MonoBehaviour
{
	// Token: 0x0600240A RID: 9226 RVA: 0x00108EF6 File Offset: 0x001072F6
	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	// Token: 0x0600240B RID: 9227 RVA: 0x00108F04 File Offset: 0x00107304
	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x0600240C RID: 9228 RVA: 0x00108F12 File Offset: 0x00107312
	public void SetSpeed(float speed)
	{
        if (this.text == null)
        {
            this.text = base.GetComponent<UIEditableText>();
        }
		this.text.Set(speed.ToString("0") + " B P S");
	}

	// Token: 0x04001F12 RID: 7954
	private UIEditableText text;
}
