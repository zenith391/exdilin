using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000312 RID: 786
public class UIOptionsScreen : MonoBehaviour
{
	// Token: 0x06002372 RID: 9074 RVA: 0x00105F94 File Offset: 0x00104394
	public void Init(string titleStr, string usernameStr, string descriptionStr)
	{
		this.titleText.text = titleStr;
		this.usernameText.text = usernameStr;
		this.descriptionText.text = descriptionStr;
		bool active = false;
		this.editorSaveMenuButton.SetActive(active);
		Button[] componentsInChildren = this.shareButtonParent.GetComponentsInChildren<Button>();
		for (int num = 0; num != componentsInChildren.Length; num++)
		{
			componentsInChildren[num].onClick.AddListener(new UnityAction(this.ButtonPressed_ShowSharePopup));
		}
	}

	// Token: 0x06002373 RID: 9075 RVA: 0x0010600C File Offset: 0x0010440C
	public void ButtonPressed_Resume()
	{
		Blocksworld.bw.HideOptionsScreen();
	}

	// Token: 0x06002374 RID: 9076 RVA: 0x00106018 File Offset: 0x00104418
	public void ButtonPressed_Exit()
	{
		Blocksworld.bw.HideOptionsScreen();
		Blocksworld.bw.ButtonExitWorldTapped();
	}

	// Token: 0x06002375 RID: 9077 RVA: 0x0010602E File Offset: 0x0010442E
	public void ButtonPressed_EditorSaveMenu()
	{
	}

	// Token: 0x06002376 RID: 9078 RVA: 0x00106030 File Offset: 0x00104430
	public void ButtonPressed_ShowSharePopup()
	{
		RectTransform component = this.shareButtonParent.GetComponent<RectTransform>();
		Canvas component2 = base.GetComponent<Canvas>();
		Rect shareRect = RectTransformUtility.PixelAdjustRect(component, component2);
		float x = component.anchoredPosition.x;
		float num = 768f - component.anchoredPosition.y - shareRect.height;
		shareRect.center = new Vector2(x, -num);
		WorldSession.platformDelegate.ShowSharingPopup(shareRect);
	}

	// Token: 0x04001EB5 RID: 7861
	public Text titleText;

	// Token: 0x04001EB6 RID: 7862
	public Text usernameText;

	// Token: 0x04001EB7 RID: 7863
	public Text descriptionText;

	// Token: 0x04001EB8 RID: 7864
	public GameObject editorSaveMenuButton;

	// Token: 0x04001EB9 RID: 7865
	public GameObject shareButtonParent;
}
