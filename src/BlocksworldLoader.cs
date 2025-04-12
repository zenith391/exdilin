using System;
using System.Collections;
using UnityEngine;

// Token: 0x020001F9 RID: 505
public class BlocksworldLoader : MonoBehaviour
{
	// Token: 0x06001A0F RID: 6671 RVA: 0x000C1021 File Offset: 0x000BF421
	public void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
		base.StartCoroutine(this.LoadBlocksworldScene());
	}

	// Token: 0x06001A10 RID: 6672 RVA: 0x000C1038 File Offset: 0x000BF438
	public IEnumerator LoadBlocksworldScene()
	{
		AsyncOperation async = Application.LoadLevelAsync("Scene");
		yield return async;
		UnityEngine.Object.Destroy(this);
		yield break;
	}
}
