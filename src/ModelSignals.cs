using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020001DB RID: 475
public class ModelSignals
{
	// Token: 0x06001890 RID: 6288 RVA: 0x000AC5C1 File Offset: 0x000AA9C1
	public static void Clear()
	{
		ModelSignals.modelSignals.Clear();
	}

	// Token: 0x06001891 RID: 6289 RVA: 0x000AC5D0 File Offset: 0x000AA9D0
	private static ModelSignals.ModelSignalsState GetModelSignalsState(Block b)
	{
		Block modelBlock = b.modelBlock;
		ModelSignals.ModelSignalsState modelSignalsState;
		if (!ModelSignals.modelSignals.TryGetValue(modelBlock, out modelSignalsState))
		{
			modelSignalsState = new ModelSignals.ModelSignalsState
			{
				modelBlock = modelBlock
			};
			ModelSignals.modelSignals[modelBlock] = modelSignalsState;
		}
		return modelSignalsState;
	}

	// Token: 0x06001892 RID: 6290 RVA: 0x000AC614 File Offset: 0x000AAA14
	public static bool IsSendingSignal(Block b, int index, out float signalStrength)
	{
		ModelSignals.ModelSignalsState modelSignalsState = ModelSignals.GetModelSignalsState(b);
		signalStrength = modelSignalsState.sendingValues[index];
		return modelSignalsState.sending[index];
	}

	// Token: 0x06001893 RID: 6291 RVA: 0x000AC63C File Offset: 0x000AAA3C
	public static void SendSignal(Block b, int index, float signalStrength)
	{
		ModelSignals.ModelSignalsState modelSignalsState = ModelSignals.GetModelSignalsState(b);
		modelSignalsState.sending[index] = true;
		modelSignalsState.sendingValues[index] = signalStrength;
		Blocksworld.AddResetStateUniqueCommand(ModelSignals.resetSignalsCommand, true);
	}

	// Token: 0x06001894 RID: 6292 RVA: 0x000AC670 File Offset: 0x000AAA70
	public static bool IsSendingCustomSignal(Block b, ScriptRowExecutionInfo eInfo, string signalName, float argStrength)
	{
		ModelSignals.ModelSignalsState modelSignalsState = ModelSignals.GetModelSignalsState(b);
		float b2;
		if (modelSignalsState.sendingCustom.TryGetValue(signalName, out b2))
		{
			eInfo.floatArg = argStrength * Mathf.Min(eInfo.floatArg, b2);
			return true;
		}
		return false;
	}

	// Token: 0x06001895 RID: 6293 RVA: 0x000AC6B0 File Offset: 0x000AAAB0
	public static void SendCustomSignal(Block b, ScriptRowExecutionInfo eInfo, string signalName, float strength)
	{
		ModelSignals.ModelSignalsState modelSignalsState = ModelSignals.GetModelSignalsState(b);
		float b2;
		if (!Blocksworld.sendingCustom.TryGetValue(signalName, out b2))
		{
			b2 = 1f;
		}
		modelSignalsState.sendingCustom[signalName] = Mathf.Max(eInfo.floatArg * strength, b2);
		Blocksworld.AddResetStateUniqueCommand(ModelSignals.resetSignalsCommand, true);
	}

	// Token: 0x06001896 RID: 6294 RVA: 0x000AC701 File Offset: 0x000AAB01
	public static void Play()
	{
		ModelSignals.Clear();
	}

	// Token: 0x06001897 RID: 6295 RVA: 0x000AC708 File Offset: 0x000AAB08
	public static void Stop()
	{
		ModelSignals.Clear();
	}

	// Token: 0x0400137B RID: 4987
	private static Dictionary<Block, ModelSignals.ModelSignalsState> modelSignals = new Dictionary<Block, ModelSignals.ModelSignalsState>();

	// Token: 0x0400137C RID: 4988
	private static ModelSignals.ResetModelSignalsCommand resetSignalsCommand = new ModelSignals.ResetModelSignalsCommand();

	// Token: 0x020001DC RID: 476
	private class ModelSignalsState
	{
		// Token: 0x0400137D RID: 4989
		public Block modelBlock;

		// Token: 0x0400137E RID: 4990
		public bool[] sending = new bool[26];

		// Token: 0x0400137F RID: 4991
		public float[] sendingValues = new float[26];

		// Token: 0x04001380 RID: 4992
		public Dictionary<string, float> sendingCustom = new Dictionary<string, float>();
	}

	// Token: 0x020001DD RID: 477
	private class ResetModelSignalsCommand : Command
	{
		// Token: 0x0600189B RID: 6299 RVA: 0x000AC75A File Offset: 0x000AAB5A
		public override void Execute()
		{
			base.Execute();
			ModelSignals.Clear();
		}
	}
}
