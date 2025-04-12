using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200044A RID: 1098
public class UINavDebug : MonoBehaviour
{
	// Token: 0x06002EE6 RID: 12006 RVA: 0x0014D151 File Offset: 0x0014B551
	private void Awake()
	{
	}

	// Token: 0x06002EE7 RID: 12007 RVA: 0x0014D154 File Offset: 0x0014B554
	public void UpdateHistory(Stack<UISceneInfo> history, Stack<UISceneInfo> future)
	{
		this.historyArray = history.ToArray();
		this.futureArray = future.ToArray();
		this.historyT.text = string.Empty;
		foreach (UISceneInfo uisceneInfo in this.historyArray)
		{
			Text text = this.historyT;
			text.text = text.text + uisceneInfo.path + "\n";
		}
		foreach (UISceneInfo uisceneInfo2 in this.futureArray)
		{
			Text text2 = this.futureT;
			text2.text = text2.text + uisceneInfo2.path + "\n";
		}
	}

	// Token: 0x04002741 RID: 10049
	public Text historyT;

	// Token: 0x04002742 RID: 10050
	public Text futureT;

	// Token: 0x04002743 RID: 10051
	private UISceneInfo[] historyArray;

	// Token: 0x04002744 RID: 10052
	private UISceneInfo[] futureArray;
}
