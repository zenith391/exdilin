using System;
using System.Collections.Generic;

// Token: 0x02000041 RID: 65
public class ScriptRowData : ObjectData
{
	// Token: 0x06000226 RID: 550 RVA: 0x0000C7EC File Offset: 0x0000ABEC
	public static ScriptRowData GetScriptRowData(ScriptRowExecutionInfo r)
	{
		ScriptRowData scriptRowData;
		if (!ScriptRowData.datas.TryGetValue(r, out scriptRowData))
		{
			scriptRowData = new ScriptRowData();
			ScriptRowData.datas[r] = scriptRowData;
		}
		return scriptRowData;
	}

	// Token: 0x06000227 RID: 551 RVA: 0x0000C81E File Offset: 0x0000AC1E
	public static void Clear()
	{
		ScriptRowData.datas.Clear();
	}

	// Token: 0x04000208 RID: 520
	private static Dictionary<ScriptRowExecutionInfo, ScriptRowData> datas = new Dictionary<ScriptRowExecutionInfo, ScriptRowData>();
}
