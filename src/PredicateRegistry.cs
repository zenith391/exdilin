using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Blocks;

public static class PredicateRegistry
{
	private static Dictionary<string, Predicate> Names = new Dictionary<string, Predicate>();

	private static Dictionary<Type, List<Predicate>> Types = new Dictionary<Type, List<Predicate>>();

	private static Dictionary<Type, HashSet<Predicate>> TypeSets = new Dictionary<Type, HashSet<Predicate>>();

	private static Dictionary<Predicate, Type> PredicateTypes = new Dictionary<Predicate, Type>();

	private static HashSet<Type> AllTypes = new HashSet<Type>();

	private static Dictionary<Predicate, HashSet<Predicate>> EquivalentPredicates = new Dictionary<Predicate, HashSet<Predicate>>();

	public static Dictionary<int, Predicate> IndexToPredicate = new Dictionary<int, Predicate>();

	private static Dictionary<Predicate, Dictionary<Predicate, List<Func<object[], bool>>>> argumentConverters = new Dictionary<Predicate, Dictionary<Predicate, List<Func<object[], bool>>>>();

	public static Predicate Add<TOwner>(string name, PredicateSensorConstructorDelegate sensor = null, PredicateActionConstructorDelegate action = null, Type[] argTypes = null, string[] argNames = null, EditableTileParameter editableParameter = null)
	{
		Predicate predicate = new Predicate(name, sensor, action, argTypes, argNames, editableParameter);
		Names[name] = predicate;
		Type typeFromHandle = typeof(TOwner);
		if (!Types.ContainsKey(typeFromHandle))
		{
			Types[typeFromHandle] = new List<Predicate>();
			TypeSets[typeFromHandle] = new HashSet<Predicate>();
		}
		Types[typeFromHandle].Add(predicate);
		TypeSets[typeFromHandle].Add(predicate);
		AllTypes.Add(typeFromHandle);
		PredicateTypes[predicate] = typeFromHandle;
		IndexToPredicate[predicate.index] = predicate;
		return predicate;
	}

	public static Predicate AddTyped(Type type, string name, PredicateSensorConstructorDelegate sensor = null, PredicateActionConstructorDelegate action = null, Type[] argTypes = null, string[] argNames = null, EditableTileParameter editableParameter = null)
	{
		Predicate predicate = new Predicate(name, sensor, action, argTypes, argNames, editableParameter);
		Names[name] = predicate;
		if (!Types.ContainsKey(type))
		{
			Types[type] = new List<Predicate>();
			TypeSets[type] = new HashSet<Predicate>();
		}
		Types[type].Add(predicate);
		TypeSets[type].Add(predicate);
		AllTypes.Add(type);
		PredicateTypes[predicate] = type;
		IndexToPredicate[predicate.index] = predicate;
		return predicate;
	}

	public static void UpdateEquivalentPredicates()
	{
		Dictionary<Type, List<string>> dictionary = new Dictionary<Type, List<string>>();
		dictionary.Add(typeof(BlockAbstractWheel), new List<string> { ".Drive", ".Turn", ".TurnTowardsTag", ".TurnTowardsTagRaw", ".IsWheelTowardsTag", "IsDPadAlongWheel", "TurnAlongDPad", "DriveAlongDPad", "DriveAlongDPadRaw" });
		dictionary.Add(typeof(BlockAbstractStabilizer), new List<string> { ".Stabilize", ".ControlPosition", ".Burst" });
		dictionary.Add(typeof(BlockAbstractMotor), new List<string> { ".Turn", ".Step", ".Return", ".FreeSpin", ".TargetAngle" });
		dictionary.Add(typeof(BlockAbstractLegs), new List<string>
		{
			".GotoTag", ".ChaseTag", ".GotoTap", ".AnalogStickControl", ".TurnTowardsTag", ".TurnTowardsTap", ".AvoidTag", ".Idle", ".Translate", ".Jump",
			".StartFirstPersonCamera", ".FreezeRotation"
		});
		dictionary.Add(typeof(BlockWalkable), new List<string>
		{
			".GotoTag", ".ChaseTag", ".GotoTap", ".AnalogStickControl", ".TurnTowardsTag", ".TurnTowardsTap", ".AvoidTag", ".Idle", ".Translate", ".Jump",
			".StartAnimFirstPersonCamera", ".FreezeRotation"
		});
		dictionary.Add(typeof(BlockAbstractRocket), new List<string> { ".Fire", ".Smoke" });
		dictionary.Add(typeof(BlockAbstractAntiGravity), new List<string>
		{
			".IncreaseModelGravityInfluence", ".IncreaseChunkGravityInfluence", ".AlignInGravityFieldChunk", ".AlignTerrainChunk", ".BankTurnChunk", ".PositionInGravityFieldChunk", ".TurnTowardsTagChunk", ".AlignAlongDPadChunk", ".IncreaseLocalVelocityChunk", ".IncreaseLocalTorqueChunk",
			".HoverInGravityFieldChunk"
		});
		dictionary.Add(typeof(BlockAbstractLaser), new List<string> { ".Beam", ".Pulse" });
		dictionary.Add(typeof(BlockAbstractTorsionSpring), new List<string> { ".Charge", ".StepCharge", ".Release", ".FreeSpin", ".SetRigidity" });
		dictionary.Add(typeof(BlockAbstractParticles), new List<string> { ".ParticleShoot", ".ParticleSpread", ".ParticleAngle" });
		Dictionary<Type, List<string>> dictionary2 = dictionary;
		Dictionary<string, string> dictionary3 = new Dictionary<string, string>
		{
			{ ".AnalogStickControl", ".DPadControl" },
			{ ".StartFirstPersonCamera", ".StartAnimFirstPersonCamera" }
		};
		Dictionary<Type, List<Type>> dictionary4 = new Dictionary<Type, List<Type>>();
		foreach (Type allType in AllTypes)
		{
			foreach (KeyValuePair<Type, List<string>> item in dictionary2)
			{
				if (allType.IsSubclassOf(item.Key) || (allType == typeof(BlockCharacter) && item.Key == typeof(BlockWalkable)) || (allType == typeof(BlockAnimatedCharacter) && item.Key == typeof(BlockAbstractLegs)))
				{
					List<Type> list;
					if (!dictionary4.ContainsKey(item.Key))
					{
						list = new List<Type>();
						dictionary4[item.Key] = list;
					}
					else
					{
						list = dictionary4[item.Key];
					}
					list.Add(allType);
				}
			}
		}
		foreach (Type key in dictionary4.Keys)
		{
			List<Type> list2 = dictionary4[key];
			List<string> list3 = dictionary2[key];
			foreach (string item2 in list3)
			{
				HashSet<Predicate> hashSet = new HashSet<Predicate>();
				foreach (Type item3 in list2)
				{
					List<Predicate> list4 = ForType(item3, includeBaseTypes: false);
					foreach (Predicate item4 in list4)
					{
						if (item4.Name.EndsWith(item2) || (dictionary3.ContainsKey(item2) && item4.Name.EndsWith(dictionary3[item2])))
						{
							hashSet.Add(item4);
						}
					}
				}
				if (hashSet.Count <= 1)
				{
					continue;
				}
				foreach (Predicate item5 in hashSet)
				{
					EquivalentPredicates[item5] = hashSet;
				}
			}
		}
	}

	public static void AddEquivalentPredicateArgumentConverter(Predicate fromPred, Predicate toPred, Func<object[], bool> converter)
	{
		if (!argumentConverters.TryGetValue(fromPred, out var value))
		{
			value = new Dictionary<Predicate, List<Func<object[], bool>>>();
			argumentConverters[fromPred] = value;
		}
		if (!value.TryGetValue(toPred, out var value2))
		{
			value2 = (value[toPred] = new List<Func<object[], bool>>());
		}
		value2.Add(converter);
	}

	public static object[] ConvertEquivalentPredicateArguments(Predicate fromPred, Predicate toPred, object[] args)
	{
		object[] array = new object[args.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = args[i];
		}
		if (argumentConverters.TryGetValue(fromPred, out var value) && value.TryGetValue(toPred, out var value2))
		{
			foreach (Func<object[], bool> item in value2)
			{
				if (item(array))
				{
					break;
				}
			}
		}
		return array;
	}

	public static HashSet<Predicate> GetEquivalentPredicates(Predicate pred)
	{
		if (EquivalentPredicates.TryGetValue(pred, out var value))
		{
			return value;
		}
		return null;
	}

	public static HashSet<Type> GetAllRegisteredTypes()
	{
		return AllTypes;
	}

	public static List<Predicate> GetMatches(Regex rgx)
	{
		List<Predicate> list = new List<Predicate>();
		foreach (KeyValuePair<string, Predicate> name in Names)
		{
			if (rgx.IsMatch(name.Key))
			{
				list.Add(name.Value);
			}
		}
		return list;
	}

	public static bool IsInitialized()
	{
		return Names.Count != 0;
	}

	public static bool ContainsName(string name)
	{
		return Names.ContainsKey(name);
	}

	public static List<string> GetAllPredicateNames()
	{
		return new List<string>(Names.Keys);
	}

	public static Predicate ByName(string name, bool logError = true)
	{
		if (Names.ContainsKey(name))
		{
			return Names[name];
		}
		if (logError)
		{
			if (!Blocksworld.started)
			{
				BWLog.Warning("Could not find predicate with name '" + name + "' Blocksworld isn't started yet. Registered predicate count: " + Names.Count);
			}
			else
			{
				BWLog.Warning("Unknown GAF predicate '" + name + "'. There is a bad block or this world has been created in a newer app version.");
			}
		}
		return null;
	}

	public static bool CompatibleWith(Predicate pred, Block b)
	{
		for (Type type = b.GetType(); type != null; type = type.BaseType)
		{
			if (TypeSets.TryGetValue(type, out var value) && value.Contains(pred))
			{
				return true;
			}
		}
		return false;
	}

	public static List<Predicate> ForBlock(Block b, bool includeBaseTypes = true)
	{
		Type type = b.GetType();
		return ForType(type, includeBaseTypes);
	}

	public static List<Predicate> ForType(Type t, bool includeBaseTypes = true)
	{
		List<Predicate> list = new List<Predicate>();
		while (t != null)
		{
			if (Types.ContainsKey(t))
			{
				list.AddRange(Types[t]);
			}
			if (!includeBaseTypes)
			{
				break;
			}
			t = t.BaseType;
		}
		return list;
	}

	public static Type GetTypeForPredicate(Predicate pred)
	{
		if (PredicateTypes.ContainsKey(pred))
		{
			return PredicateTypes[pred];
		}
		return null;
	}
}
