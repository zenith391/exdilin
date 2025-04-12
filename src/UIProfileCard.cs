using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000314 RID: 788
public class UIProfileCard : MonoBehaviour
{
	// Token: 0x06002391 RID: 9105 RVA: 0x00106520 File Offset: 0x00104920
	public void Setup(UIProfileSelection selectionUI, UIProfileCardInfo cardInfo, bool unlocked)
	{
		this._selectionUI = selectionUI;
		this._profileType = cardInfo.type;
		this._unlocked = unlocked;
		this.cardButton.image.sprite = cardInfo.image;
		this.cardButton.onClick.AddListener(new UnityAction(this.CardTap));
		this.lockedOverlay.enabled = !unlocked;
	}

	// Token: 0x06002392 RID: 9106 RVA: 0x00106588 File Offset: 0x00104988
	public ProfileType ProfileType()
	{
		return this._profileType;
	}

	// Token: 0x06002393 RID: 9107 RVA: 0x00106590 File Offset: 0x00104990
	public void SetSelected(bool selected)
	{
		this.selectionBorder.enabled = selected;
	}

	// Token: 0x06002394 RID: 9108 RVA: 0x0010659E File Offset: 0x0010499E
	private void CardTap()
	{
		if (this._unlocked)
		{
			this._selectionUI.SelectProfileType(this._profileType);
		}
	}

	// Token: 0x04001EC6 RID: 7878
	public Button cardButton;

	// Token: 0x04001EC7 RID: 7879
	public Image selectionBorder;

	// Token: 0x04001EC8 RID: 7880
	public Image lockedOverlay;

	// Token: 0x04001EC9 RID: 7881
	private UIProfileSelection _selectionUI;

	// Token: 0x04001ECA RID: 7882
	private ProfileType _profileType;

	// Token: 0x04001ECB RID: 7883
	private bool _unlocked;
}
