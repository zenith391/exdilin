using System;
using Blocks;

namespace Exdilin;

public class PredicateEntry
{
	public string id;

	public PredicateSensorConstructorDelegate sensorConstructor;

	public PredicateActionConstructorDelegate actionConstructor;

	public PredicateType predicateType;

	public Type blockType = typeof(Block);

	public Type[] argTypes;

	public string[] argNames;

	public int variableDefault;

	public string variableLabel;

	public PredicateEntry(string id)
	{
		this.id = id;
	}

	public PredicateEntry(string id, PredicateSensorDelegate sensor)
	{
		this.id = id;
		SetSensorDelegate(sensor);
	}

	public PredicateEntry(string id, PredicateActionDelegate action)
	{
		this.id = id;
		SetActionDelegate(action);
	}

	public PredicateEntry(string id, PredicateSensorDelegate sensor, PredicateActionDelegate action)
	{
		this.id = id;
		SetSensorDelegate(sensor);
		SetActionDelegate(action);
	}

	public void SetActionDelegate(PredicateActionDelegate action)
	{
		actionConstructor = (Block b) => action;
	}

	public void SetSensorDelegate(PredicateSensorDelegate sensor)
	{
		sensorConstructor = (Block b) => sensor;
	}

	public void register()
	{
		PredicateEntryRegistry.AddPredicate(this);
	}
}
