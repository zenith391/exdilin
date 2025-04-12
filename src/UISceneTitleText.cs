using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000432 RID: 1074
[RequireComponent(typeof(Text))]
public class UISceneTitleText : UISceneInfoDisplay
{
	// Token: 0x06002E41 RID: 11841 RVA: 0x00149A30 File Offset: 0x00147E30
	public void Awake()
	{
		this.text = base.GetComponent<Text>();
		this.text.enabled = false;
	}

	// Token: 0x06002E42 RID: 11842 RVA: 0x00149A4A File Offset: 0x00147E4A
	public override void Setup(UISceneInfo sceneInfo)
	{
		this.text.text = sceneInfo.title;
		this.text.enabled = true;
	}

	// Token: 0x040026BD RID: 9917
	private Text text;
}
