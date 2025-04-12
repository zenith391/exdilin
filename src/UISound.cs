using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x0200045C RID: 1116
public class UISound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler
{
	// Token: 0x06002F3D RID: 12093 RVA: 0x0014EC54 File Offset: 0x0014D054
	private void OnEnable()
	{
	}

	// Token: 0x06002F3E RID: 12094 RVA: 0x0014EC56 File Offset: 0x0014D056
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(this.hoverEnter))
		{
			UISoundPlayer.Instance.PlayClip(this.hoverEnter, this.volume);
		}
	}

	// Token: 0x06002F3F RID: 12095 RVA: 0x0014EC7E File Offset: 0x0014D07E
	public void OnPointerExit(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(this.hoverExit))
		{
			UISoundPlayer.Instance.PlayClip(this.hoverExit, this.volume);
		}
	}

	// Token: 0x06002F40 RID: 12096 RVA: 0x0014ECA6 File Offset: 0x0014D0A6
	public void OnPointerDown(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(this.pressDown))
		{
			UISoundPlayer.Instance.PlayClip(this.pressDown, this.volume);
		}
	}

	// Token: 0x06002F41 RID: 12097 RVA: 0x0014ECCE File Offset: 0x0014D0CE
	public void OnPointerUp(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(this.release))
		{
			UISoundPlayer.Instance.PlayClip(this.release, this.volume);
		}
	}

	// Token: 0x06002F42 RID: 12098 RVA: 0x0014ECF8 File Offset: 0x0014D0F8
	public void PlayMessageButtonSound(bool success)
	{
		if (success && !string.IsNullOrEmpty(this.buttonSuccess))
		{
			UISoundPlayer.Instance.PlayClip(this.buttonSuccess, this.volume);
		}
		else if (!success && !string.IsNullOrEmpty(this.buttonFailure))
		{
			UISoundPlayer.Instance.PlayClip(this.buttonFailure, this.volume);
		}
	}

	// Token: 0x040027A4 RID: 10148
	public string hoverEnter;

	// Token: 0x040027A5 RID: 10149
	public string hoverExit;

	// Token: 0x040027A6 RID: 10150
	public string pressDown = "DefaultButtonClick";

	// Token: 0x040027A7 RID: 10151
	public string release;

	// Token: 0x040027A8 RID: 10152
	public string buttonSuccess = "DefaultButtonClick";

	// Token: 0x040027A9 RID: 10153
	public string buttonFailure;

	// Token: 0x040027AA RID: 10154
	public float volume = 1f;
}
