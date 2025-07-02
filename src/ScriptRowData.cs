using System.Collections.Generic;

public class ScriptRowData : ObjectData
{
	private static Dictionary<ScriptRowExecutionInfo, ScriptRowData> datas = new Dictionary<ScriptRowExecutionInfo, ScriptRowData>();

	public static ScriptRowData GetScriptRowData(ScriptRowExecutionInfo r)
	{
		if (!datas.TryGetValue(r, out var value))
		{
			value = new ScriptRowData();
			datas[r] = value;
		}
		return value;
	}

	public static void Clear()
	{
		datas.Clear();
	}
}
