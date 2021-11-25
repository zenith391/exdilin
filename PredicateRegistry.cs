using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Blocks;

// Token: 0x0200026B RID: 619
public static class PredicateRegistry
{
	// Token: 0x06001CFC RID: 7420 RVA: 0x000CC634 File Offset: 0x000CAA34
	public static Predicate Add<TOwner>(string name, PredicateSensorConstructorDelegate sensor = null, PredicateActionConstructorDelegate action = null, Type[] argTypes = null, string[] argNames = null, EditableTileParameter editableParameter = null)
	{
		Predicate predicate = new Predicate(name, sensor, action, argTypes, argNames, editableParameter);
		PredicateRegistry.Names[name] = predicate;
		Type typeFromHandle = typeof(TOwner);
		if (!PredicateRegistry.Types.ContainsKey(typeFromHandle))
		{
			PredicateRegistry.Types[typeFromHandle] = new List<Predicate>();
			PredicateRegistry.TypeSets[typeFromHandle] = new HashSet<Predicate>();
		}
		PredicateRegistry.Types[typeFromHandle].Add(predicate);
		PredicateRegistry.TypeSets[typeFromHandle].Add(predicate);
		PredicateRegistry.AllTypes.Add(typeFromHandle);
		PredicateRegistry.PredicateTypes[predicate] = typeFromHandle;
		PredicateRegistry.IndexToPredicate[predicate.index] = predicate;
		return predicate;
	}

	/// <summary>
	///  ADDED BY EXDILIN (0.5.1)
	/// </summary>
	/// <returns>The typed.</returns>
	/// <param name="type">Type.</param>
	/// <param name="name">Name.</param>
	/// <param name="sensor">Sensor.</param>
	/// <param name="action">Action.</param>
	/// <param name="argTypes">Argument types.</param>
	/// <param name="argNames">Argument names.</param>
	/// <param name="editableParameter">Editable parameter.</param>
	public static Predicate AddTyped(Type type, string name, PredicateSensorConstructorDelegate sensor = null, PredicateActionConstructorDelegate action = null, Type[] argTypes = null, string[] argNames = null, EditableTileParameter editableParameter = null) {
		Predicate predicate = new Predicate(name, sensor, action, argTypes, argNames, editableParameter);
		PredicateRegistry.Names[name] = predicate;
		if (!PredicateRegistry.Types.ContainsKey(type)) {
			PredicateRegistry.Types[type] = new List<Predicate>();
			PredicateRegistry.TypeSets[type] = new HashSet<Predicate>();
		}
		PredicateRegistry.Types[type].Add(predicate);
		PredicateRegistry.TypeSets[type].Add(predicate);
		PredicateRegistry.AllTypes.Add(type);
		PredicateRegistry.PredicateTypes[predicate] = type;
		PredicateRegistry.IndexToPredicate[predicate.index] = predicate;
		return predicate;
	}

	// Token: 0x06001CFD RID: 7421 RVA: 0x000CC6E4 File Offset: 0x000CAAE4
	public static void UpdateEquivalentPredicates()
	{
		Dictionary<Type, List<string>> dictionary = new Dictionary<Type, List<string>>
		{
			{
				typeof(BlockAbstractWheel),
				new List<string>
				{
					".Drive",
					".Turn",
					".TurnTowardsTag",
					".TurnTowardsTagRaw",
					".IsWheelTowardsTag",
					"IsDPadAlongWheel",
					"TurnAlongDPad",
					"DriveAlongDPad",
					"DriveAlongDPadRaw"
				}
			},
			{
				typeof(BlockAbstractStabilizer),
				new List<string>
				{
					".Stabilize",
					".ControlPosition",
					".Burst"
				}
			},
			{
				typeof(BlockAbstractMotor),
				new List<string>
				{
					".Turn",
					".Step",
					".Return",
					".FreeSpin",
					".TargetAngle"
				}
			},
			{
				typeof(BlockAbstractLegs),
				new List<string>
				{
					".GotoTag",
					".ChaseTag",
					".GotoTap",
					".AnalogStickControl",
					".TurnTowardsTag",
					".TurnTowardsTap",
					".AvoidTag",
					".Idle",
					".Translate",
					".Jump",
					".StartFirstPersonCamera",
					".FreezeRotation"
				}
			},
			{
				typeof(BlockWalkable),
				new List<string>
				{
					".GotoTag",
					".ChaseTag",
					".GotoTap",
					".AnalogStickControl",
					".TurnTowardsTag",
					".TurnTowardsTap",
					".AvoidTag",
					".Idle",
					".Translate",
					".Jump",
					".StartAnimFirstPersonCamera",
					".FreezeRotation"
				}
			},
			{
				typeof(BlockAbstractRocket),
				new List<string>
				{
					".Fire",
					".Smoke"
				}
			},
			{
				typeof(BlockAbstractAntiGravity),
				new List<string>
				{
					".IncreaseModelGravityInfluence",
					".IncreaseChunkGravityInfluence",
					".AlignInGravityFieldChunk",
					".AlignTerrainChunk",
					".BankTurnChunk",
					".PositionInGravityFieldChunk",
					".TurnTowardsTagChunk",
					".AlignAlongDPadChunk",
					".IncreaseLocalVelocityChunk",
					".IncreaseLocalTorqueChunk",
					".HoverInGravityFieldChunk"
				}
			},
			{
				typeof(BlockAbstractLaser),
				new List<string>
				{
					".Beam",
					".Pulse"
				}
			},
			{
				typeof(BlockAbstractTorsionSpring),
				new List<string>
				{
					".Charge",
					".StepCharge",
					".Release",
					".FreeSpin",
					".SetRigidity"
				}
			},
			{
				typeof(BlockAbstractParticles),
				new List<string>
				{
					".ParticleShoot",
					".ParticleSpread",
					".ParticleAngle"
				}
			}
		};
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>
		{
			{
				".AnalogStickControl",
				".DPadControl"
			},
			{
				".StartFirstPersonCamera",
				".StartAnimFirstPersonCamera"
			}
		};
		Dictionary<Type, List<Type>> dictionary3 = new Dictionary<Type, List<Type>>();
		foreach (Type type in PredicateRegistry.AllTypes)
		{
			foreach (KeyValuePair<Type, List<string>> keyValuePair in dictionary)
			{
				if (type.IsSubclassOf(keyValuePair.Key) || (type == typeof(BlockCharacter) && keyValuePair.Key == typeof(BlockWalkable)) || (type == typeof(BlockAnimatedCharacter) && keyValuePair.Key == typeof(BlockAbstractLegs)))
				{
					List<Type> list;
					if (!dictionary3.ContainsKey(keyValuePair.Key))
					{
						list = new List<Type>();
						dictionary3[keyValuePair.Key] = list;
					}
					else
					{
						list = dictionary3[keyValuePair.Key];
					}
					list.Add(type);
				}
			}
		}
		foreach (Type key in dictionary3.Keys)
		{
			List<Type> list2 = dictionary3[key];
			List<string> list3 = dictionary[key];
			foreach (string text in list3)
			{
				HashSet<Predicate> hashSet = new HashSet<Predicate>();
				foreach (Type t in list2)
				{
					List<Predicate> list4 = PredicateRegistry.ForType(t, false);
					foreach (Predicate predicate in list4)
					{
						if (predicate.Name.EndsWith(text) || (dictionary2.ContainsKey(text) && predicate.Name.EndsWith(dictionary2[text])))
						{
							hashSet.Add(predicate);
						}
					}
				}
				if (hashSet.Count > 1)
				{
					foreach (Predicate key2 in hashSet)
					{
						PredicateRegistry.EquivalentPredicates[key2] = hashSet;
					}
				}
			}
		}
	}

	// Token: 0x06001CFE RID: 7422 RVA: 0x000CCE2C File Offset: 0x000CB22C
	public static void AddEquivalentPredicateArgumentConverter(Predicate fromPred, Predicate toPred, Func<object[], bool> converter)
	{
		Dictionary<Predicate, List<Func<object[], bool>>> dictionary;
		if (!PredicateRegistry.argumentConverters.TryGetValue(fromPred, out dictionary))
		{
			dictionary = new Dictionary<Predicate, List<Func<object[], bool>>>();
			PredicateRegistry.argumentConverters[fromPred] = dictionary;
		}
		List<Func<object[], bool>> list;
		if (!dictionary.TryGetValue(toPred, out list))
		{
			list = new List<Func<object[], bool>>();
			dictionary[toPred] = list;
		}
		list.Add(converter);
	}

	// Token: 0x06001CFF RID: 7423 RVA: 0x000CCE80 File Offset: 0x000CB280
	public static object[] ConvertEquivalentPredicateArguments(Predicate fromPred, Predicate toPred, object[] args)
	{
		object[] array = new object[args.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = args[i];
		}
		Dictionary<Predicate, List<Func<object[], bool>>> dictionary;
		List<Func<object[], bool>> list;
		if (PredicateRegistry.argumentConverters.TryGetValue(fromPred, out dictionary) && dictionary.TryGetValue(toPred, out list))
		{
			foreach (Func<object[], bool> func in list)
			{
				if (func(array))
				{
					break;
				}
			}
		}
		return array;
	}

	// Token: 0x06001D00 RID: 7424 RVA: 0x000CCF28 File Offset: 0x000CB328
	public static HashSet<Predicate> GetEquivalentPredicates(Predicate pred)
	{
		HashSet<Predicate> result;
		if (PredicateRegistry.EquivalentPredicates.TryGetValue(pred, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06001D01 RID: 7425 RVA: 0x000CCF4A File Offset: 0x000CB34A
	public static HashSet<Type> GetAllRegisteredTypes()
	{
		return PredicateRegistry.AllTypes;
	}

	// Token: 0x06001D02 RID: 7426 RVA: 0x000CCF54 File Offset: 0x000CB354
	public static List<Predicate> GetMatches(Regex rgx)
	{
		List<Predicate> list = new List<Predicate>();
		foreach (KeyValuePair<string, Predicate> keyValuePair in PredicateRegistry.Names)
		{
			if (rgx.IsMatch(keyValuePair.Key))
			{
				list.Add(keyValuePair.Value);
			}
		}
		return list;
	}

	// Token: 0x06001D03 RID: 7427 RVA: 0x000CCFD0 File Offset: 0x000CB3D0
	public static bool IsInitialized()
	{
		return PredicateRegistry.Names.Count != 0;
	}

	// Token: 0x06001D04 RID: 7428 RVA: 0x000CCFE2 File Offset: 0x000CB3E2
	public static bool ContainsName(string name)
	{
		return PredicateRegistry.Names.ContainsKey(name);
	}

	// Token: 0x06001D05 RID: 7429 RVA: 0x000CCFEF File Offset: 0x000CB3EF
	public static List<string> GetAllPredicateNames()
	{
		return new List<string>(PredicateRegistry.Names.Keys);
	}

	// Token: 0x06001D06 RID: 7430 RVA: 0x000CD000 File Offset: 0x000CB400
	public static Predicate ByName(string name, bool logError = true)
	{
		if (PredicateRegistry.Names.ContainsKey(name))
		{
			return PredicateRegistry.Names[name];
		}
		if (logError)
		{
			if (!Blocksworld.started)
			{
				BWLog.Warning(string.Concat(new object[]
				{
					"Could not find predicate with name '",
					name,
					"' Blocksworld isn't started yet. Registered predicate count: ",
					PredicateRegistry.Names.Count
				}));
			}
			else
			{
				BWLog.Warning("Unknown GAF predicate '" + name + "'. There is a bad block or this world has been created in a newer app version.");
			}
		}
		return null;
	}

	// Token: 0x06001D07 RID: 7431 RVA: 0x000CD08C File Offset: 0x000CB48C
	public static bool CompatibleWith(Predicate pred, Block b)
	{
		for (Type type = b.GetType(); type != null; type = type.BaseType)
		{
			HashSet<Predicate> hashSet;
			if (PredicateRegistry.TypeSets.TryGetValue(type, out hashSet) && hashSet.Contains(pred))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001D08 RID: 7432 RVA: 0x000CD0D4 File Offset: 0x000CB4D4
	public static List<Predicate> ForBlock(Block b, bool includeBaseTypes = true)
	{
		Type type = b.GetType();
		return PredicateRegistry.ForType(type, includeBaseTypes);
	}

	// Token: 0x06001D09 RID: 7433 RVA: 0x000CD0F0 File Offset: 0x000CB4F0
	public static List<Predicate> ForType(Type t, bool includeBaseTypes = true)
	{
		List<Predicate> list = new List<Predicate>();
		while (t != null)
		{
			if (PredicateRegistry.Types.ContainsKey(t))
			{
				list.AddRange(PredicateRegistry.Types[t]);
			}
			if (!includeBaseTypes)
			{
				break;
			}
			t = t.BaseType;
		}
		return list;
	}

	// Token: 0x06001D0A RID: 7434 RVA: 0x000CD143 File Offset: 0x000CB543
	public static Type GetTypeForPredicate(Predicate pred)
	{
		if (PredicateRegistry.PredicateTypes.ContainsKey(pred))
		{
			return PredicateRegistry.PredicateTypes[pred];
		}
		return null;
	}

	// Token: 0x040017A6 RID: 6054
	private static Dictionary<string, Predicate> Names = new Dictionary<string, Predicate>();

	// Token: 0x040017A7 RID: 6055
	private static Dictionary<Type, List<Predicate>> Types = new Dictionary<Type, List<Predicate>>();

	// Token: 0x040017A8 RID: 6056
	private static Dictionary<Type, HashSet<Predicate>> TypeSets = new Dictionary<Type, HashSet<Predicate>>();

	// Token: 0x040017A9 RID: 6057
	private static Dictionary<Predicate, Type> PredicateTypes = new Dictionary<Predicate, Type>();

	// Token: 0x040017AA RID: 6058
	private static HashSet<Type> AllTypes = new HashSet<Type>();

	// Token: 0x040017AB RID: 6059
	private static Dictionary<Predicate, HashSet<Predicate>> EquivalentPredicates = new Dictionary<Predicate, HashSet<Predicate>>();

	// Token: 0x040017AC RID: 6060
	public static Dictionary<int, Predicate> IndexToPredicate = new Dictionary<int, Predicate>();

	// Token: 0x040017AD RID: 6061
	private static Dictionary<Predicate, Dictionary<Predicate, List<Func<object[], bool>>>> argumentConverters = new Dictionary<Predicate, Dictionary<Predicate, List<Func<object[], bool>>>>();
}
