using System;
using System.Collections.Generic;
using UnityEngine.UI;

// Token: 0x0200041B RID: 1051
public class UIPanelElementLoadSceneButton : UIPanelElement
{
	// Token: 0x06002D83 RID: 11651 RVA: 0x00144AE0 File Offset: 0x00142EE0
	public void Awake()
	{
		this.sceneInfo = new UISceneInfo
		{
			path = this.scenePath,
			title = this.sceneTitle,
			userImageUrl = this.sceneUserImageUrl,
			dataType = this.sceneDataType,
			dataSubtype = this.sceneDataSubtype
		};
		Button component = base.GetComponent<Button>();
		if (component != null)
		{
			component.onClick.AddListener(delegate()
			{
				this.TriggerSceneLoad();
			});
		}
	}

	// Token: 0x06002D84 RID: 11652 RVA: 0x00144B60 File Offset: 0x00142F60
	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
	}

	// Token: 0x06002D85 RID: 11653 RVA: 0x00144B69 File Offset: 0x00142F69
	public void TriggerSceneLoad()
	{
		if (string.IsNullOrEmpty(this.sceneInfo.path))
		{
			return;
		}
		if (MainUIController.active)
		{
			MainUIController.Instance.LoadUIScene(this.sceneInfo, false, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
		}
	}

	// Token: 0x06002D86 RID: 11654 RVA: 0x00144BA0 File Offset: 0x00142FA0
	public override void Fill(Dictionary<string, string> data)
	{
		this.sceneInfo.title = base.ReplacePlaceholderTextWithData(this.sceneTitle, data, null);
		this.sceneInfo.dataType = base.ReplacePlaceholderTextWithData(this.sceneDataType, data, null);
		this.sceneInfo.dataSubtype = base.ReplacePlaceholderTextWithData(this.sceneDataSubtype, data, null);
		this.sceneInfo.userImageUrl = base.ReplacePlaceholderTextWithData(this.sceneUserImageUrl, data, null);
	}

	// Token: 0x04002611 RID: 9745
	public string scenePath;

	// Token: 0x04002612 RID: 9746
	public string sceneTitle;

	// Token: 0x04002613 RID: 9747
	public string sceneDataType;

	// Token: 0x04002614 RID: 9748
	public string sceneDataSubtype;

	// Token: 0x04002615 RID: 9749
	public string sceneUserImageUrl;

	// Token: 0x04002616 RID: 9750
	private bool initComplete;

	// Token: 0x04002617 RID: 9751
	private UISceneInfo sceneInfo;
}
