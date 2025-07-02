using System;
using Blocks;

public class Predicate
{
	public readonly string Name;

	public readonly PredicateSensorConstructorDelegate Sensor;

	public readonly PredicateActionConstructorDelegate Action;

	public readonly Type[] ArgTypes;

	public readonly string[] ArgNames;

	public readonly int index;

	public static int predicateCount;

	private EditableTileParameter editableParameter;

	public int[] subParameterCounts;

	public bool updatesIconOnArgumentChange;

	public Func<object[], bool, object[]> argumentExtender;

	public bool canHaveOverlay;

	public EditableTileParameter EditableParameter
	{
		get
		{
			return editableParameter;
		}
		set
		{
			editableParameter = value;
			if (value != null)
			{
				if (value.parameterIndex >= 0 && value.parameterIndex < subParameterCounts.Length)
				{
					subParameterCounts[value.parameterIndex] = value.subParameterCount;
					return;
				}
				BWLog.Info("Invalid sub parameter count index " + value.parameterIndex + ". Length is " + subParameterCounts.Length + " Predicate is " + Name);
			}
		}
	}

	public Predicate(string name, PredicateSensorConstructorDelegate sensor, PredicateActionConstructorDelegate action, Type[] argTypes, string[] argNames = null, EditableTileParameter editableParameter = null)
	{
		Name = name;
		Sensor = sensor;
		Action = action;
		ArgTypes = ((argTypes == null) ? new Type[0] : argTypes);
		ArgNames = argNames;
		subParameterCounts = new int[ArgTypes.Length];
		for (int i = 0; i < subParameterCounts.Length; i++)
		{
			subParameterCounts[i] = 1;
		}
		EditableParameter = editableParameter;
		index = predicateCount;
		predicateCount++;
	}

	public object[] ExtendArguments(object[] args, bool overwrite)
	{
		if (argumentExtender != null)
		{
			return argumentExtender(args, overwrite);
		}
		if (overwrite)
		{
			BWLog.Warning("Extend Arguments called with null argumentExtender!");
		}
		return args;
	}

	public bool CanEditTile(Tile tile)
	{
		if (EditableParameter == null)
		{
			return false;
		}
		if (EditableParameter.parameterIndex >= tile.gaf.Args.Length)
		{
			BWLog.Warning("Trying to edit " + Name + " but tile does not have parameter " + EditableParameter.parameterIndex + " " + tile.gaf.ToString());
			return false;
		}
		return true;
	}

	public TileResultCode RunAction(Block block, ScriptRowExecutionInfo info, object[] args)
	{
		return Action(block)(info, args);
	}

	public TileResultCode RunSensor(Block block, ScriptRowExecutionInfo info, object[] args)
	{
		return Sensor(block)(info, args);
	}

	public bool CompatibleWith(Block block)
	{
		return PredicateRegistry.CompatibleWith(this, block);
	}
}
