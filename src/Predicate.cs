using System;
using Blocks;

// Token: 0x0200026A RID: 618
public class Predicate
{
	// Token: 0x06001CF3 RID: 7411 RVA: 0x000CC3FC File Offset: 0x000CA7FC
	public Predicate(string name, PredicateSensorConstructorDelegate sensor, PredicateActionConstructorDelegate action, Type[] argTypes, string[] argNames = null, EditableTileParameter editableParameter = null)
	{
		this.Name = name;
		this.Sensor = sensor;
		this.Action = action;
		this.ArgTypes = ((argTypes == null) ? new Type[0] : argTypes);
		this.ArgNames = argNames;
		this.subParameterCounts = new int[this.ArgTypes.Length];
		for (int i = 0; i < this.subParameterCounts.Length; i++)
		{
			this.subParameterCounts[i] = 1;
		}
		this.EditableParameter = editableParameter;
		this.index = Predicate.predicateCount;
		Predicate.predicateCount++;
	}

	// Token: 0x17000134 RID: 308
	// (get) Token: 0x06001CF4 RID: 7412 RVA: 0x000CC49A File Offset: 0x000CA89A
	// (set) Token: 0x06001CF5 RID: 7413 RVA: 0x000CC4A4 File Offset: 0x000CA8A4
	public EditableTileParameter EditableParameter
	{
		get
		{
			return this.editableParameter;
		}
		set
		{
			this.editableParameter = value;
			if (value != null)
			{
				if (value.parameterIndex >= 0 && value.parameterIndex < this.subParameterCounts.Length)
				{
					this.subParameterCounts[value.parameterIndex] = value.subParameterCount;
				}
				else
				{
					BWLog.Info(string.Concat(new object[]
					{
						"Invalid sub parameter count index ",
						value.parameterIndex,
						". Length is ",
						this.subParameterCounts.Length,
						" Predicate is ",
						this.Name
					}));
				}
			}
		}
	}

	// Token: 0x06001CF6 RID: 7414 RVA: 0x000CC544 File Offset: 0x000CA944
	public object[] ExtendArguments(object[] args, bool overwrite)
	{
		if (this.argumentExtender != null)
		{
			return this.argumentExtender(args, overwrite);
		}
		if (overwrite)
		{
			BWLog.Warning("Extend Arguments called with null argumentExtender!");
		}
		return args;
	}

	// Token: 0x06001CF7 RID: 7415 RVA: 0x000CC570 File Offset: 0x000CA970
	public bool CanEditTile(Tile tile)
	{
		if (this.EditableParameter == null)
		{
			return false;
		}
		if (this.EditableParameter.parameterIndex >= tile.gaf.Args.Length)
		{
			BWLog.Warning(string.Concat(new object[]
			{
				"Trying to edit ",
				this.Name,
				" but tile does not have parameter ",
				this.EditableParameter.parameterIndex,
				" ",
				tile.gaf.ToString()
			}));
			return false;
		}
		return true;
	}

	// Token: 0x06001CF8 RID: 7416 RVA: 0x000CC5FC File Offset: 0x000CA9FC
	public TileResultCode RunAction(Block block, ScriptRowExecutionInfo info, object[] args)
	{
		return this.Action(block)(info, args);
	}

	// Token: 0x06001CF9 RID: 7417 RVA: 0x000CC611 File Offset: 0x000CAA11
	public TileResultCode RunSensor(Block block, ScriptRowExecutionInfo info, object[] args)
	{
		return this.Sensor(block)(info, args);
	}

	// Token: 0x06001CFA RID: 7418 RVA: 0x000CC626 File Offset: 0x000CAA26
	public bool CompatibleWith(Block block)
	{
		return PredicateRegistry.CompatibleWith(this, block);
	}

	// Token: 0x0400179A RID: 6042
	public readonly string Name;

	// Token: 0x0400179B RID: 6043
	public readonly PredicateSensorConstructorDelegate Sensor;

	// Token: 0x0400179C RID: 6044
	public readonly PredicateActionConstructorDelegate Action;

	// Token: 0x0400179D RID: 6045
	public readonly Type[] ArgTypes;

	// Token: 0x0400179E RID: 6046
	public readonly string[] ArgNames;

	// Token: 0x0400179F RID: 6047
	public readonly int index;

	// Token: 0x040017A0 RID: 6048
	public static int predicateCount;

	// Token: 0x040017A1 RID: 6049
	private EditableTileParameter editableParameter;

	// Token: 0x040017A2 RID: 6050
	public int[] subParameterCounts;

	// Token: 0x040017A3 RID: 6051
	public bool updatesIconOnArgumentChange;

	// Token: 0x040017A4 RID: 6052
	public Func<object[], bool, object[]> argumentExtender;

	// Token: 0x040017A5 RID: 6053
	public bool canHaveOverlay;
}
