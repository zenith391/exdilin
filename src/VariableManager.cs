using System.Collections.Generic;
using UnityEngine;

public class VariableManager
{
	private static Dictionary<string, bool> globalBooleanVariables = new Dictionary<string, bool>();

	private static Dictionary<string, int> globalIntegerVariables = new Dictionary<string, int>();

	private static void _SetGlobalBooleanVariableValue(string name, bool value)
	{
		globalBooleanVariables[name] = value;
	}

	private static bool _GetGlobalBooleanVariableValue(string name)
	{
		bool value;
		return globalBooleanVariables.TryGetValue(name, out value) && value;
	}

	private static void _SetGlobalIntegerVariableValue(string name, int value)
	{
		globalIntegerVariables[name] = value;
	}

	private static int _GetGlobalIntegerVariableValue(string name)
	{
		if (globalIntegerVariables.TryGetValue(name, out var value))
		{
			return value;
		}
		return 0;
	}

	public static TileResultCode SetGlobalBooleanVariableValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		bool intBooleanArg = Util.GetIntBooleanArg(args, 1, defaultValue: true);
		_SetGlobalBooleanVariableValue(stringArg, intBooleanArg);
		return TileResultCode.True;
	}

	public static TileResultCode GlobalBooleanVariableValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		bool intBooleanArg = Util.GetIntBooleanArg(args, 1, defaultValue: true);
		if (_GetGlobalBooleanVariableValue(stringArg) == intBooleanArg)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public static TileResultCode SetGlobalIntegerVariableValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		int intArg = Util.GetIntArg(args, 1, 0);
		_SetGlobalIntegerVariableValue(stringArg, intArg);
		return TileResultCode.True;
	}

	public static TileResultCode GlobalIntegerVariableValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		int intArg = Util.GetIntArg(args, 1, 0);
		if (_GetGlobalIntegerVariableValue(stringArg) == intArg)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public static TileResultCode RandomizeGlobalIntegerVariable(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		int intArg = Util.GetIntArg(args, 1, 0);
		int intArg2 = Util.GetIntArg(args, 2, 100);
		_SetGlobalIntegerVariableValue(stringArg, Random.Range(intArg, intArg2));
		return TileResultCode.True;
	}

	public static TileResultCode IncrementIntegerVariable(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		int intArg = Util.GetIntArg(args, 1, 0);
		_SetGlobalIntegerVariableValue(stringArg, _GetGlobalIntegerVariableValue(stringArg) + intArg);
		return TileResultCode.True;
	}

	public static List<string> GetGlobalBooleanVariableNames()
	{
		return new List<string>(globalBooleanVariables.Keys);
	}

	public static bool GetGlobalBooleanVariableValue(string name)
	{
		return _GetGlobalBooleanVariableValue(name);
	}

	public static List<string> GetGlobalIntegerVariableNames()
	{
		return new List<string>(globalIntegerVariables.Keys);
	}

	public static int GetGlobalIntegerVariableValue(string name)
	{
		return _GetGlobalIntegerVariableValue(name);
	}

	public static void Clear()
	{
		globalBooleanVariables.Clear();
	}
}
