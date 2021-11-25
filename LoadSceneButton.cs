using System;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020003DB RID: 987
public class LoadSceneButton : Button
{
	// Token: 0x06002BC4 RID: 11204 RVA: 0x0013BAF7 File Offset: 0x00139EF7
	protected override void OnEnable()
	{
		base.onClick.RemoveListener(new UnityAction(this.LoadScene));
		base.onClick.AddListener(new UnityAction(this.LoadScene));
	}

	// Token: 0x06002BC5 RID: 11205 RVA: 0x0013BB27 File Offset: 0x00139F27
	protected override void OnDisable()
	{
		base.onClick.RemoveAllListeners();
	}

	// Token: 0x06002BC6 RID: 11206 RVA: 0x0013BB34 File Offset: 0x00139F34
	private void LoadScene()
	{
		if (MainUIController.active)
		{
			UISceneInfo sceneInfo = new UISceneInfo
			{
				path = this.scenePath,
				title = this.sceneTitle,
				dataType = this.sceneDataType,
				dataSubtype = this.sceneDataSubtype
			};
			MainUIController.Instance.LoadUIScene(sceneInfo, false, SceneTransitionStyle.Fade, SceneTransitionStyle.Fade);
		}
	}

	// Token: 0x04002503 RID: 9475
	public string scenePath;

	// Token: 0x04002504 RID: 9476
	public string sceneTitle;

	// Token: 0x04002505 RID: 9477
	public string sceneDataType;

	// Token: 0x04002506 RID: 9478
	public string sceneDataSubtype;
}
