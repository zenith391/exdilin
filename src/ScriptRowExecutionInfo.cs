using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x02000280 RID: 640
public class ScriptRowExecutionInfo
{
	// Token: 0x06001E03 RID: 7683 RVA: 0x000D77A7 File Offset: 0x000D5BA7
	public ScriptRowExecutionInfo()
	{
	}

	// Token: 0x06001E04 RID: 7684 RVA: 0x000D77B0 File Offset: 0x000D5BB0
	public ScriptRowExecutionInfo(int rowIndex, Block block)
	{
		this.rowIndex = rowIndex;
		this.block = block;
		this.pc = 0;
		this.timer = 0f;
		this.floatArg = 1f;
		this.sensorValue = true;
		this.beforeThen = true;
		this.canStopSensorEvaluation = true;
		List<Tile> list = block.GetRuntimeTiles()[rowIndex];
		this.isNegate = new bool[list.Count];
		this.sensorNegated = new bool[list.Count];
		this.predicateTiles = new Predicate[list.Count];
		this.predicateArgs = new object[list.Count][];
		this.predicateSensorTiles = new PredicateSensorDelegate[list.Count];
		this.predicateActionTiles = new PredicateActionDelegate[list.Count];
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			Predicate predicate = list[i].gaf.Predicate;
			bool flag2 = predicate == Block.predicateNegate;
			this.isNegate[i] = flag2;
			if (flag2)
			{
				this.canStopSensorEvaluation = false;
			}
			bool flag3 = predicate == Block.predicateNegateMod;
			if (flag3)
			{
				flag = !flag;
			}
			if (flag3 || flag2)
			{
				this.sensorNegated[i] = false;
			}
			else
			{
				this.sensorNegated[i] = flag;
			}
			this.predicateTiles[i] = predicate;
			this.predicateArgs[i] = list[i].gaf.Args;
			this.predicateSensorTiles[i] = ((predicate.Sensor != null) ? predicate.Sensor(block) : null);
			this.predicateActionTiles[i] = ((predicate.Action != null) ? predicate.Action(block) : null);
		}
	}

	// Token: 0x06001E05 RID: 7685 RVA: 0x000D796A File Offset: 0x000D5D6A
	public object[] Args(int rowIndex)
	{
		return this.predicateArgs[rowIndex];
	}

	// Token: 0x06001E06 RID: 7686 RVA: 0x000D7974 File Offset: 0x000D5D74
	public void RunRow()
	{
		this.timer += Blocksworld.fixedDeltaTime;
		this.justFinishedExecutingRow = false;
		while (this.pc < this.predicateTiles.Length)
		{
			Predicate predicate = this.predicateTiles[this.pc];
			object[] args = this.predicateArgs[this.pc];
			if (predicate != Block.predicateThen)
			{
				TileResultCode tileResultCode;
				if (this.beforeThen)
				{
					tileResultCode = this.predicateSensorTiles[this.pc](this, args);
					if (this.sensorNegated[this.pc])
					{
						bool flag = tileResultCode == TileResultCode.True;
						if (!flag)
						{
							this.floatArg = 0f;
						}
						tileResultCode = ((!flag) ? TileResultCode.True : TileResultCode.False);
						this.floatArg = 1f - this.floatArg;
					}
				}
				else
				{
					tileResultCode = this.predicateActionTiles[this.pc](this, args);
				}
				if (this.canStopSensorEvaluation || !this.beforeThen)
				{
					if (tileResultCode == TileResultCode.False)
					{
						this.pc = 0;
						this.timer = 0f;
						this.floatArg = 1f;
						this.sensorValue = true;
						if (!this.beforeThen)
						{
							this.justFinishedExecutingRow = true;
						}
						this.beforeThen = true;
						return;
					}
					if (tileResultCode == TileResultCode.Delayed)
					{
						return;
					}
					this.timer = 0f;
					this.pc++;
				}
				else
				{
					if (this.isNegate[this.pc])
					{
						this.sensorValue = !this.sensorValue;
						this.floatArg = 1f - this.floatArg;
					}
					else
					{
						if (tileResultCode == TileResultCode.False)
						{
							this.floatArg = 0f;
						}
						this.sensorValue = (this.sensorValue && tileResultCode == TileResultCode.True);
					}
					this.timer = 0f;
					this.pc++;
					Predicate predicate2 = this.predicateTiles[this.pc];
					if (predicate2 == Block.predicateThen && !this.sensorValue && this.floatArg < 0.001f)
					{
						this.pc = 0;
						this.timer = 0f;
						this.floatArg = 1f;
						this.sensorValue = true;
						this.beforeThen = true;
						return;
					}
				}
				continue;
			}
			this.beforeThen = false;
			this.timer = -Blocksworld.fixedDeltaTime;
			this.pc++;
			return;
		}
		this.pc = 0;
		this.floatArg = 1f;
		this.sensorValue = true;
		this.beforeThen = true;
		this.justFinishedExecutingRow = true;
	}

	// Token: 0x0400184A RID: 6218
	public int rowIndex;

	// Token: 0x0400184B RID: 6219
	public int pc;

	// Token: 0x0400184C RID: 6220
	public float timer;

	// Token: 0x0400184D RID: 6221
	public bool sensorValue;

	// Token: 0x0400184E RID: 6222
	public float floatArg;

	// Token: 0x0400184F RID: 6223
	public bool beforeThen;

	// Token: 0x04001850 RID: 6224
	public bool justFinishedExecutingRow;

	// Token: 0x04001851 RID: 6225
	public Predicate[] predicateTiles;

	// Token: 0x04001852 RID: 6226
	public PredicateSensorDelegate[] predicateSensorTiles;

	// Token: 0x04001853 RID: 6227
	public PredicateActionDelegate[] predicateActionTiles;

	// Token: 0x04001854 RID: 6228
	public object[][] predicateArgs;

	// Token: 0x04001855 RID: 6229
	public bool canStopSensorEvaluation;

	// Token: 0x04001856 RID: 6230
	public bool[] isNegate;

	// Token: 0x04001857 RID: 6231
	public bool[] sensorNegated;

	// Token: 0x04001858 RID: 6232
	public Block block;
}
