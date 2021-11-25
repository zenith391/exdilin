using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000334 RID: 820
public class VariableManager
{
	// Token: 0x06002527 RID: 9511 RVA: 0x0010F182 File Offset: 0x0010D582
	private static void _SetGlobalBooleanVariableValue(string name, bool value)
	{
		VariableManager.globalBooleanVariables[name] = value;
	}

	// Token: 0x06002528 RID: 9512 RVA: 0x0010F190 File Offset: 0x0010D590
	private static bool _GetGlobalBooleanVariableValue(string name)
	{
		bool flag;
		return VariableManager.globalBooleanVariables.TryGetValue(name, out flag) && flag;
	}

	// Token: 0x06002529 RID: 9513 RVA: 0x0010F1B2 File Offset: 0x0010D5B2
	private static void _SetGlobalIntegerVariableValue(string name, int value)
	{
		VariableManager.globalIntegerVariables[name] = value;
	}

	// Token: 0x0600252A RID: 9514 RVA: 0x0010F1C0 File Offset: 0x0010D5C0
	private static int _GetGlobalIntegerVariableValue(string name)
	{
		int result;
		if (VariableManager.globalIntegerVariables.TryGetValue(name, out result))
		{
			return result;
		}
		return 0;
	}

	// Token: 0x0600252B RID: 9515 RVA: 0x0010F1E4 File Offset: 0x0010D5E4
	public static TileResultCode SetGlobalBooleanVariableValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		bool intBooleanArg = Util.GetIntBooleanArg(args, 1, true);
		VariableManager._SetGlobalBooleanVariableValue(stringArg, intBooleanArg);
		return TileResultCode.True;
	}

	// Token: 0x0600252C RID: 9516 RVA: 0x0010F210 File Offset: 0x0010D610
	public static TileResultCode GlobalBooleanVariableValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		bool intBooleanArg = Util.GetIntBooleanArg(args, 1, true);
		return (VariableManager._GetGlobalBooleanVariableValue(stringArg) != intBooleanArg) ? TileResultCode.False : TileResultCode.True;
	}

	// Token: 0x0600252D RID: 9517 RVA: 0x0010F248 File Offset: 0x0010D648
	public static TileResultCode SetGlobalIntegerVariableValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		int intArg = Util.GetIntArg(args, 1, 0);
		VariableManager._SetGlobalIntegerVariableValue(stringArg, intArg);
		return TileResultCode.True;
	}

	// Token: 0x0600252E RID: 9518 RVA: 0x0010F274 File Offset: 0x0010D674
	public static TileResultCode GlobalIntegerVariableValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		int intArg = Util.GetIntArg(args, 1, 0);
		return (VariableManager._GetGlobalIntegerVariableValue(stringArg) != intArg) ? TileResultCode.False : TileResultCode.True;
	}

	// Token: 0x0600252F RID: 9519 RVA: 0x0010F2AC File Offset: 0x0010D6AC
	public static TileResultCode RandomizeGlobalIntegerVariable(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		int intArg = Util.GetIntArg(args, 1, 0);
		int intArg2 = Util.GetIntArg(args, 2, 100);
		VariableManager._SetGlobalIntegerVariableValue(stringArg, UnityEngine.Random.Range(intArg, intArg2));
		return TileResultCode.True;
	}

	// Token: 0x06002530 RID: 9520 RVA: 0x0010F2E8 File Offset: 0x0010D6E8
	public static TileResultCode IncrementIntegerVariable(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		int intArg = Util.GetIntArg(args, 1, 0);
		VariableManager._SetGlobalIntegerVariableValue(stringArg, VariableManager._GetGlobalIntegerVariableValue(stringArg) + intArg);
		return TileResultCode.True;
	}

	// Token: 0x06002531 RID: 9521 RVA: 0x0010F31A File Offset: 0x0010D71A
	public static List<string> GetGlobalBooleanVariableNames()
	{
		return new List<string>(VariableManager.globalBooleanVariables.Keys);
	}

	// Token: 0x06002532 RID: 9522 RVA: 0x0010F32B File Offset: 0x0010D72B
	public static bool GetGlobalBooleanVariableValue(string name)
	{
		return VariableManager._GetGlobalBooleanVariableValue(name);
	}

	// Token: 0x06002533 RID: 9523 RVA: 0x0010F333 File Offset: 0x0010D733
	public static List<string> GetGlobalIntegerVariableNames()
	{
		return new List<string>(VariableManager.globalIntegerVariables.Keys);
	}

	// Token: 0x06002534 RID: 9524 RVA: 0x0010F344 File Offset: 0x0010D744
	public static int GetGlobalIntegerVariableValue(string name)
	{
		return VariableManager._GetGlobalIntegerVariableValue(name);
	}

	// Token: 0x06002535 RID: 9525 RVA: 0x0010F34C File Offset: 0x0010D74C
	public static void Clear()
	{
		VariableManager.globalBooleanVariables.Clear();
	}

	// Token: 0x04001FBF RID: 8127
	private static Dictionary<string, bool> globalBooleanVariables = new Dictionary<string, bool>();

	// Token: 0x04001FC0 RID: 8128
	private static Dictionary<string, int> globalIntegerVariables = new Dictionary<string, int>();
}
