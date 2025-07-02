using System.Collections.Generic;
using Blocks;

public class ScriptRowExecutionInfo
{
	public int rowIndex;

	public int pc;

	public float timer;

	public bool sensorValue;

	public float floatArg;

	public bool beforeThen;

	public bool justFinishedExecutingRow;

	public Predicate[] predicateTiles;

	public PredicateSensorDelegate[] predicateSensorTiles;

	public PredicateActionDelegate[] predicateActionTiles;

	public object[][] predicateArgs;

	public bool canStopSensorEvaluation;

	public bool[] isNegate;

	public bool[] sensorNegated;

	public Block block;

	public ScriptRowExecutionInfo()
	{
	}

	public ScriptRowExecutionInfo(int rowIndex, Block block)
	{
		this.rowIndex = rowIndex;
		this.block = block;
		pc = 0;
		timer = 0f;
		floatArg = 1f;
		sensorValue = true;
		beforeThen = true;
		canStopSensorEvaluation = true;
		List<Tile> list = block.GetRuntimeTiles()[rowIndex];
		isNegate = new bool[list.Count];
		sensorNegated = new bool[list.Count];
		predicateTiles = new Predicate[list.Count];
		predicateArgs = new object[list.Count][];
		predicateSensorTiles = new PredicateSensorDelegate[list.Count];
		predicateActionTiles = new PredicateActionDelegate[list.Count];
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			Predicate predicate = list[i].gaf.Predicate;
			bool flag2 = predicate == Block.predicateNegate;
			isNegate[i] = flag2;
			if (flag2)
			{
				canStopSensorEvaluation = false;
			}
			bool flag3 = predicate == Block.predicateNegateMod;
			if (flag3)
			{
				flag = !flag;
			}
			if (flag3 || flag2)
			{
				sensorNegated[i] = false;
			}
			else
			{
				sensorNegated[i] = flag;
			}
			predicateTiles[i] = predicate;
			predicateArgs[i] = list[i].gaf.Args;
			predicateSensorTiles[i] = ((predicate.Sensor != null) ? predicate.Sensor(block) : null);
			predicateActionTiles[i] = ((predicate.Action != null) ? predicate.Action(block) : null);
		}
	}

	public object[] Args(int rowIndex)
	{
		return predicateArgs[rowIndex];
	}

	public void RunRow()
	{
		timer += Blocksworld.fixedDeltaTime;
		justFinishedExecutingRow = false;
		while (pc < predicateTiles.Length)
		{
			Predicate predicate = predicateTiles[pc];
			object[] args = predicateArgs[pc];
			if (predicate != Block.predicateThen)
			{
				TileResultCode tileResultCode;
				if (beforeThen)
				{
					tileResultCode = predicateSensorTiles[pc](this, args);
					if (sensorNegated[pc])
					{
						bool flag = tileResultCode == TileResultCode.True;
						if (!flag)
						{
							floatArg = 0f;
						}
						tileResultCode = ((!flag) ? TileResultCode.True : TileResultCode.False);
						floatArg = 1f - floatArg;
					}
				}
				else
				{
					tileResultCode = predicateActionTiles[pc](this, args);
				}
				if (canStopSensorEvaluation || !beforeThen)
				{
					switch (tileResultCode)
					{
					case TileResultCode.False:
						pc = 0;
						timer = 0f;
						floatArg = 1f;
						sensorValue = true;
						if (!beforeThen)
						{
							justFinishedExecutingRow = true;
						}
						beforeThen = true;
						return;
					case TileResultCode.Delayed:
						return;
					default:
						timer = 0f;
						pc++;
						break;
					}
					continue;
				}
				if (isNegate[pc])
				{
					sensorValue = !sensorValue;
					floatArg = 1f - floatArg;
				}
				else
				{
					if (tileResultCode == TileResultCode.False)
					{
						floatArg = 0f;
					}
					sensorValue = sensorValue && tileResultCode == TileResultCode.True;
				}
				timer = 0f;
				pc++;
				Predicate predicate2 = predicateTiles[pc];
				if (predicate2 != Block.predicateThen || sensorValue || !(floatArg < 0.001f))
				{
					continue;
				}
				pc = 0;
				timer = 0f;
				floatArg = 1f;
				sensorValue = true;
				beforeThen = true;
				return;
			}
			beforeThen = false;
			timer = 0f - Blocksworld.fixedDeltaTime;
			pc++;
			return;
		}
		pc = 0;
		floatArg = 1f;
		sensorValue = true;
		beforeThen = true;
		justFinishedExecutingRow = true;
	}
}
