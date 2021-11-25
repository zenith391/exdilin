using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000450 RID: 1104
public class UIPopupEscapeMenu : UIPopup
{
	// Token: 0x06002EFC RID: 12028 RVA: 0x0014D498 File Offset: 0x0014B898
	private void Awake()
	{
		this.exitButton.onClick.AddListener(delegate()
		{
			Application.Quit();
		});
		this.supportButton.onClick.AddListener(delegate()
		{
			Application.OpenURL("https://blocksworld-api.lindenlab.com/support/");
		});
		this.wikiButton.onClick.AddListener(delegate()
		{
			Application.OpenURL("https://blocksworld.gamepedia.com/Blocksworld_Wiki");
		});
		if (this.exitWorldButton != null)
		{
			this.exitWorldButton.onClick.AddListener(delegate()
			{
				if (WorldSession.current != null)
				{
					base.Hide();
					Blocksworld.bw.ButtonExitWorldTapped();
				}
			});
		}
	}

	// Token: 0x0400275A RID: 10074
	public Button exitButton;

	// Token: 0x0400275B RID: 10075
	public Button exitWorldButton;

	// Token: 0x0400275C RID: 10076
	public Button supportButton;

	// Token: 0x0400275D RID: 10077
	public Button wikiButton;
}
