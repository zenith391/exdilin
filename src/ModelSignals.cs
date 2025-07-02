using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class ModelSignals
{
	private class ModelSignalsState
	{
		public Block modelBlock;

		public bool[] sending = new bool[26];

		public float[] sendingValues = new float[26];

		public Dictionary<string, float> sendingCustom = new Dictionary<string, float>();
	}

	private class ResetModelSignalsCommand : Command
	{
		public override void Execute()
		{
			base.Execute();
			Clear();
		}
	}

	private static Dictionary<Block, ModelSignalsState> modelSignals = new Dictionary<Block, ModelSignalsState>();

	private static ResetModelSignalsCommand resetSignalsCommand = new ResetModelSignalsCommand();

	public static void Clear()
	{
		modelSignals.Clear();
	}

	private static ModelSignalsState GetModelSignalsState(Block b)
	{
		Block modelBlock = b.modelBlock;
		if (!modelSignals.TryGetValue(modelBlock, out var value))
		{
			value = new ModelSignalsState
			{
				modelBlock = modelBlock
			};
			modelSignals[modelBlock] = value;
		}
		return value;
	}

	public static bool IsSendingSignal(Block b, int index, out float signalStrength)
	{
		ModelSignalsState modelSignalsState = GetModelSignalsState(b);
		signalStrength = modelSignalsState.sendingValues[index];
		return modelSignalsState.sending[index];
	}

	public static void SendSignal(Block b, int index, float signalStrength)
	{
		ModelSignalsState modelSignalsState = GetModelSignalsState(b);
		modelSignalsState.sending[index] = true;
		modelSignalsState.sendingValues[index] = signalStrength;
		Blocksworld.AddResetStateUniqueCommand(resetSignalsCommand);
	}

	public static bool IsSendingCustomSignal(Block b, ScriptRowExecutionInfo eInfo, string signalName, float argStrength)
	{
		ModelSignalsState modelSignalsState = GetModelSignalsState(b);
		if (modelSignalsState.sendingCustom.TryGetValue(signalName, out var value))
		{
			eInfo.floatArg = argStrength * Mathf.Min(eInfo.floatArg, value);
			return true;
		}
		return false;
	}

	public static void SendCustomSignal(Block b, ScriptRowExecutionInfo eInfo, string signalName, float strength)
	{
		ModelSignalsState modelSignalsState = GetModelSignalsState(b);
		if (!Blocksworld.sendingCustom.TryGetValue(signalName, out var value))
		{
			value = 1f;
		}
		modelSignalsState.sendingCustom[signalName] = Mathf.Max(eInfo.floatArg * strength, value);
		Blocksworld.AddResetStateUniqueCommand(resetSignalsCommand);
	}

	public static void Play()
	{
		Clear();
	}

	public static void Stop()
	{
		Clear();
	}
}
