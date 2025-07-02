using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

public class GAF
{
	public bool Rare;

	public readonly Predicate Predicate;

	public readonly object[] Args;

	private readonly int _hashCode;

	private int _blockItemId;

	private static HashSet<Predicate> dynamicLabelPredicates;

	private static HashSet<Predicate> paintPredicates;

	public int BlockItemId
	{
		get
		{
			if (_blockItemId == 0)
			{
				_blockItemId = GafToBlockItem.Find(this);
			}
			return _blockItemId;
		}
	}

	public bool HasBuildPanelLabel
	{
		get
		{
			if ((Predicate == Block.predicateCreate || Predicate == Block.predicateTextureTo || Predicate == Block.predicatePaintTo || Blocksworld.globalGafs.Contains(this)) && Predicate != Block.predicateButton)
			{
				return Predicate == Block.predicateThen;
			}
			return true;
		}
	}

	public GAF(BlockItem blockItem)
	{
		_blockItemId = blockItem.Id;
		Predicate = PredicateRegistry.ByName(blockItem.GafPredicateName);
		if (blockItem.GafDefaultArgs == null)
		{
			Args = new object[1];
		}
		else
		{
			Args = new object[blockItem.GafDefaultArgs.Length];
			for (int i = 0; i < Args.Length; i++)
			{
				Args[i] = blockItem.GafDefaultArgs[i];
			}
		}
		_hashCode = CalculateHashCode();
	}

	public GAF(string predicateName, params object[] args)
	{
		if (args == null)
		{
			args = new object[1];
		}
		Predicate = PredicateRegistry.ByName(predicateName);
		Args = args;
		_hashCode = CalculateHashCode();
	}

	public GAF(Predicate predicate, object[] args, bool dummy)
	{
		Predicate = predicate;
		if (args == null)
		{
			args = new object[1];
		}
		Args = args;
		_hashCode = CalculateHashCode();
	}

	public GAF(Predicate predicate, params object[] args)
	{
		Predicate = predicate;
		if (args == null)
		{
			args = new object[1];
		}
		Args = args;
		_hashCode = CalculateHashCode();
	}

	private int CalculateHashCode()
	{
		int num = ((Predicate == null) ? 17 : Predicate.GetHashCode());
		object[] args = Args;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] != null)
			{
				num ^= args[i].GetHashCode() << i;
			}
		}
		if (args.Length > Predicate.ArgTypes.Length)
		{
			BWLog.Warning("Found a GAF with more arguments than its predicate supports. Pred name: '" + Predicate.Name + "',  Args.Length: " + args.Length + ", Predicate.ArgTypes.Length: " + Predicate.ArgTypes.Length);
		}
		return num;
	}

	public GAF Clone()
	{
		object[] array = new object[Args.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Args[i];
		}
		return new GAF(Predicate, array)
		{
			_blockItemId = _blockItemId
		};
	}

	public GAF ClonePart(int argCount)
	{
		object[] array = new object[argCount];
		for (int i = 0; i < argCount; i++)
		{
			array[i] = Args[i];
		}
		return new GAF(Predicate, array)
		{
			_blockItemId = _blockItemId
		};
	}

	public bool HasDynamicLabel()
	{
		if (Predicate == Block.predicateSendCustomSignal || Predicate == Block.predicateSendCustomSignalModel)
		{
			string stringArgSafe = Util.GetStringArgSafe(Args, 0, "*");
			return stringArgSafe != "*";
		}
		if (Block.customVariablePredicates.Contains(Predicate))
		{
			string stringArgSafe2 = Util.GetStringArgSafe(Args, 0, "*");
			return stringArgSafe2 != "*";
		}
		if (dynamicLabelPredicates == null)
		{
			dynamicLabelPredicates = new HashSet<Predicate>
			{
				Block.predicateCreateModel,
				Block.predicateCustomTag
			};
		}
		return dynamicLabelPredicates.Contains(Predicate);
	}

	public string GetDynamicLabel()
	{
		string text = Util.GetStringArgSafe(Args, 0, string.Empty);
		if (Block.blockVariableOperationsOnGlobals.Contains(Predicate))
		{
			string stringArgSafe = Util.GetStringArgSafe(Args, 1, string.Empty);
			text = Block.GetLabelForPredicate(Predicate, text, stringArgSafe);
		}
		else if (Block.blockVariableOperationsOnOtherBlockVars.Contains(Predicate))
		{
			string stringArgSafe2 = Util.GetStringArgSafe(Args, 1, string.Empty);
			text = Block.GetLabelForPredicate(Predicate, text, stringArgSafe2);
		}
		return text;
	}

	public TileResultCode RunSensor(Block block, ScriptRowExecutionInfo eInfo)
	{
		return Predicate.RunSensor(block, eInfo, Args);
	}

	public TileResultCode RunAction(Block block, ScriptRowExecutionInfo eInfo)
	{
		return Predicate.RunAction(block, eInfo, Args);
	}

	public bool UpdatesIconOnArgumentChange()
	{
		return Predicate.updatesIconOnArgumentChange;
	}

	public bool CanBeAction()
	{
		return Predicate.Action != null;
	}

	public bool CanBeCondition()
	{
		return Predicate.Sensor != null;
	}

	public bool AllowTilesAfter()
	{
		return !Block.noTilesAfterPredicates.Contains(Predicate);
	}

	public bool IsRare()
	{
		return Rare;
	}

	public bool IsCreateModel()
	{
		return Predicate == Block.predicateCreateModel;
	}

	public bool IsButtonInput()
	{
		return Predicate.Name == "Block.ButtonInput";
	}

	public bool IsPaint()
	{
		if (paintPredicates == null)
		{
			paintPredicates = new HashSet<Predicate>
			{
				Block.predicatePaintTo,
				PredicateRegistry.ByName("Block.PaintSkyTo"),
				PredicateRegistry.ByName("Master.PaintSkyTo"),
				PredicateRegistry.ByName("Master.PaintTerrainTo")
			};
		}
		return paintPredicates.Contains(Predicate);
	}

	public bool Matches(GAF other)
	{
		if (other.Predicate != Predicate)
		{
			return false;
		}
		if (Predicate == Block.predicateCreateModel)
		{
			int intArg = Util.GetIntArg(Args, 0, -1);
			int intArg2 = Util.GetIntArg(other.Args, 0, -2);
			return intArg == intArg2;
		}
		return other.BlockItemId == BlockItemId;
	}

	public bool MatchesAny(HashSet<GAF> gafs)
	{
		if (gafs != null)
		{
			foreach (GAF gaf in gafs)
			{
				if (gaf != null && Matches(gaf))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public override bool Equals(object other)
	{
		if (!(other is GAF))
		{
			return false;
		}
		GAF gAF = (GAF)other;
		if (gAF.Predicate != Predicate)
		{
			return false;
		}
		if (Args.Length != gAF.Args.Length)
		{
			return false;
		}
		for (int i = 0; i < Args.Length; i++)
		{
			if (gAF.Args[i] != Args[i] && !gAF.Args[i].Equals(Args[i]))
			{
				return false;
			}
		}
		return true;
	}

	public bool EqualsInTutorial(GAF g2)
	{
		return Tutorial.GAFsEqualInTutorial(this, g2);
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public override string ToString()
	{
		string text = Predicate.Name + "(";
		for (int i = 0; i < Args.Length; i++)
		{
			if (i != 0)
			{
				text += ",";
			}
			object obj = Args[i];
			string text2;
			if (obj == null)
			{
				text2 = "*";
			}
			else if (obj is float || obj is int || obj is bool || obj is string || obj is Vector3 || obj is Quaternion)
			{
				text2 = obj.ToString();
			}
			else
			{
				BWLog.Error($"Don't know how to serialize this object: {obj.GetType().ToString()}");
				text2 = obj.ToString();
			}
			text += text2;
		}
		return text + ")";
	}

	public bool FixSemanticallyInvalidness()
	{
		Predicate predicate = Predicate;
		if (predicate == Block.predicateTextureTo)
		{
			string key = (string)Args[0];
			if (!Materials.textureInfos.ContainsKey(key))
			{
				Args[0] = "Plain";
				return true;
			}
		}
		if (predicate == Block.predicatePaintTo)
		{
			string key2 = (string)Args[0];
			if (!Blocksworld.colorDefinitions.ContainsKey(key2))
			{
				Args[0] = "Yellow";
				return true;
			}
		}
		if (predicate == Block.predicateCreate)
		{
			string item = (string)Args[0];
			if (!Blocksworld.existingBlockNames.Contains(item))
			{
				Args[0] = "Cube";
				return true;
			}
		}
		if (predicate == Block.predicatePlaySoundDurational)
		{
			string text = (string)Args[0];
			if (!Sound.existingSfxs.Contains(text) && !Sound.existingSfxs.Contains("SFX " + text))
			{
				Args[0] = "Checkpoint";
				return true;
			}
		}
		return false;
	}

	public bool IsSemanticallyValid()
	{
		Predicate predicate = Predicate;
		if (BWEnvConfig.Flags["SKIP_GAF_VERIFICATION"])
		{
			return true;
		}
		if (predicate == Block.predicateTextureTo)
		{
			string text = (string)Args[0];
			if (!Materials.textureInfos.ContainsKey(text))
			{
				BWLog.Info("GAF validation error: Could not find texture '" + text + "'");
				return false;
			}
		}
		else if (predicate == Block.predicatePaintTo)
		{
			string text2 = (string)Args[0];
			if (!Blocksworld.colorDefinitions.ContainsKey(text2))
			{
				BWLog.Info("GAF validation error: Could not find paint '" + text2 + "'");
				return false;
			}
		}
		else if (predicate == Block.predicateCreate)
		{
			string text3 = (string)Args[0];
			if (!Blocksworld.existingBlockNames.Contains(text3))
			{
				BWLog.Info("GAF validation error: Could not find block '" + text3 + "'");
				return false;
			}
		}
		else if (predicate == Block.predicatePlaySoundDurational)
		{
			string text4 = (string)Args[0];
			if (!Sound.existingSfxs.Contains(text4) && !Sound.existingSfxs.Contains("SFX " + text4))
			{
				BWLog.Info("GAF validation error: Could not find SFX '" + text4 + "'");
				return false;
			}
		}
		return true;
	}

	public static GAF FromJSON(JObject obj, bool nullOnFailure = false, bool logOnFailure = true)
	{
		if (obj.ObjectValue != null)
		{
			string name = (string)obj["predicate"];
			name = SymbolCompat.RenamePredicate(name);
			Predicate predicate = PredicateRegistry.ByName(name, logOnFailure);
			if (predicate == null)
			{
				if (BW.isUnityEditor && Options.CreateErrorGafs)
				{
					return new GAF("Error", "Predicate not found: " + name);
				}
				return null;
			}
			List<JObject> arrayValue = obj["args"].ArrayValue;
			Type[] argTypes = predicate.ArgTypes;
			if (predicate.ArgTypes.Length < arrayValue.Count)
			{
				if (logOnFailure)
				{
					BWLog.Error("Found more arguments than supported for predicate " + predicate.Name + " Max supported: " + predicate.ArgTypes.Length + " Found: " + arrayValue.Count);
				}
				return null;
			}
			int num = Mathf.Min(arrayValue.Count, argTypes.Length);
			object[] array = new object[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = ArgFromJSON(predicate.ArgTypes[i], obj["args"][i]);
			}
			array = predicate.ExtendArguments(array, overwrite: false);
			return SymbolCompat.RenameGAF(new GAF(predicate, array));
		}
		if (obj.ArrayValue == null)
		{
			if (logOnFailure)
			{
				BWLog.Error("JObject obj is not a valid GAF in GAF.FromJSON()");
			}
			return null;
		}
		string name2 = (string)obj[0];
		name2 = SymbolCompat.RenamePredicate(name2);
		Predicate predicate2 = PredicateRegistry.ByName(name2, logOnFailure);
		if (predicate2 == null)
		{
			if (BW.isUnityEditor && Options.CreateErrorGafs)
			{
				return new GAF("Error", "Predicate not found: " + name2);
			}
			return null;
		}
		List<JObject> arrayValue2 = obj.ArrayValue;
		Type[] argTypes2 = predicate2.ArgTypes;
		if (arrayValue2.Count - 1 > argTypes2.Length)
		{
			if (logOnFailure)
			{
				BWLog.Error("Found more arguments than supported for predicate " + predicate2.Name + " Max supported: " + predicate2.ArgTypes.Length + " Found: " + (arrayValue2.Count - 1));
			}
			return null;
		}
		int num2 = Mathf.Min(arrayValue2.Count - 1, argTypes2.Length);
		object[] array2 = new object[num2];
		for (int j = 1; j < obj.ArrayValue.Count; j++)
		{
			array2[j - 1] = ArgFromJSON(predicate2.ArgTypes[j - 1], obj[j]);
		}
		array2 = predicate2.ExtendArguments(array2, overwrite: false);
		return SymbolCompat.RenameGAF(new GAF(predicate2, array2));
	}

	private void WriteDefaultArgValue(JSONStreamEncoder encoder, Type argType, bool compact)
	{
		if (argType.Equals(typeof(float)))
		{
			encoder.WriteNumber(0f);
		}
		else if (argType.Equals(typeof(int)))
		{
			encoder.WriteNumber(0L);
		}
		else if (argType.Equals(typeof(bool)))
		{
			encoder.WriteBool(b: false);
		}
		else if (argType.Equals(typeof(string)))
		{
			encoder.WriteString(string.Empty);
		}
		else if (argType.Equals(typeof(Vector3)))
		{
			Vector3.zero.ToJSON(encoder, !Options.NoSnapSave, compact, Blocksworld.useCompactGafWriteRenamings);
		}
		else if (argType.Equals(typeof(Quaternion)))
		{
			Quaternion.identity.ToJSON(encoder);
		}
	}

	public void ToJSON(JSONStreamEncoder encoder)
	{
		encoder.BeginObject();
		encoder.WriteKey("predicate");
		encoder.WriteString(Predicate.Name);
		encoder.WriteKey("args");
		encoder.BeginArray();
		for (int i = 0; i < Args.Length; i++)
		{
			object obj = Args[i];
			try
			{
				WriteArgJSON(encoder, obj);
			}
			catch
			{
				string text = "GAF_Serialization_Error";
				string text2 = "World:" + WorldSession.current.worldTitle + "Predicate:" + Predicate.Name + " argument index:" + i;
				BWLog.Error(text + text2);
				if (i < Predicate.ArgTypes.Length)
				{
					WriteDefaultArgValue(encoder, Predicate.ArgTypes[i], compact: false);
				}
				else
				{
					encoder.WriteNumber(0L);
				}
				BW.Analytics.SendAnalyticsEvent(text, text2);
			}
		}
		encoder.EndArray();
		encoder.EndObject();
	}

	public void ToJSONCompact(JSONStreamEncoder encoder)
	{
		encoder.BeginArray();
		string text = Predicate.Name;
		object[] array = Args;
		if (Blocksworld.useCompactGafWriteRenamings)
		{
			if (SymbolCompat.predicateInvRenamings.TryGetValue(text, out var value))
			{
				text = value;
			}
			string text2 = null;
			string value4;
			if (Predicate == Block.predicateCreate)
			{
				if (SymbolCompat.blockInvRenamings.TryGetValue((string)Args[0], out var value2))
				{
					text2 = value2;
				}
			}
			else if (Predicate == Block.predicatePaintTo)
			{
				if (SymbolCompat.paintInvRenamings.TryGetValue((string)Args[0], out var value3))
				{
					text2 = value3;
				}
			}
			else if (Predicate == Block.predicateTextureTo && SymbolCompat.textureInvRenamings.TryGetValue((string)Args[0], out value4))
			{
				text2 = value4;
			}
			if (text2 != null)
			{
				array = (object[])Args.Clone();
				array[0] = text2;
			}
		}
		encoder.WriteString(text);
		for (int i = 0; i < array.Length; i++)
		{
			object obj = array[i];
			try
			{
				WriteArgJSON(encoder, obj, compact: true);
			}
			catch
			{
				string text3 = "GAF_Serialization_Error";
				string text4 = "World:" + WorldSession.current.worldTitle + "Predicate:" + Predicate.Name + " argument index:" + i;
				BWLog.Error(text3 + text4);
				if (i < Predicate.ArgTypes.Length)
				{
					WriteDefaultArgValue(encoder, Predicate.ArgTypes[i], compact: true);
				}
				else
				{
					encoder.WriteNumber(0L);
				}
				BW.Analytics.SendAnalyticsEvent(text3, text4);
			}
		}
		encoder.EndArray();
	}

	public static void WriteGAFCountDictionary(JSONStreamEncoder encoder, Dictionary<GAF, int> dict)
	{
		encoder.BeginArray();
		encoder.InsertNewline();
		foreach (KeyValuePair<GAF, int> item in dict)
		{
			encoder.BeginArray();
			item.Key.ToJSONCompact(encoder);
			if (item.Value == -1)
			{
				encoder.WriteString("infinity");
			}
			else
			{
				encoder.WriteNumber(item.Value);
			}
			encoder.EndArray();
			encoder.InsertNewline();
		}
		encoder.EndArray();
	}

	public static void WriteArgJSON(JSONStreamEncoder encoder, object obj, bool compact = false)
	{
		if (obj is float)
		{
			encoder.WriteNumber((float)obj);
			return;
		}
		if (obj is int)
		{
			encoder.WriteNumber((int)obj);
			return;
		}
		if (obj is bool)
		{
			encoder.WriteBool((bool)obj);
			return;
		}
		if (obj is string)
		{
			encoder.WriteString((string)obj);
			return;
		}
		if (obj is Vector3)
		{
			((Vector3)obj).ToJSON(encoder, !Options.NoSnapSave, compact, Blocksworld.useCompactGafWriteRenamings);
			return;
		}
		if (obj is Quaternion)
		{
			((Quaternion)obj).ToJSON(encoder);
			return;
		}
		if (obj == null)
		{
			throw new ArgumentException("obj", "Trying to serialize null object");
		}
		throw new ArgumentException("obj", $"Don't know how to serialize this object: {obj.GetType().ToString()}");
	}

	public static object ArgFromJSON(Type type, JObject obj)
	{
		if (type == typeof(float))
		{
			return (float)obj;
		}
		if (type == typeof(int))
		{
			return (int)obj;
		}
		if (type == typeof(bool))
		{
			return (bool)obj;
		}
		if (type == typeof(string))
		{
			return (string)obj;
		}
		if (type == typeof(Vector3))
		{
			return obj.Vector3Value();
		}
		if (type == typeof(Quaternion))
		{
			return obj.QuaternionValue();
		}
		throw new ArgumentException("type", "Don't know how to materialize this type");
	}

	public static bool IsJSONGAFSupported(JObject jgaf)
	{
		string text = (string)jgaf[0];
		if (!PredicateRegistry.ContainsName(text))
		{
			return false;
		}
		switch (text)
		{
		case "Block.Create":
		case "Block.TextureTo":
		case "Block.PaintTo":
		{
			GAF gAF = FromJSON(jgaf);
			if (gAF == null)
			{
				BWLog.Info("GAF.FromJSON() returned null. predName: " + text);
			}
			return TileIconManager.Instance.GAFHasIcon(gAF);
		}
		default:
			return true;
		}
	}

	public static bool IsJSONGAFListSupported(string jsonGafs)
	{
		JObject jObject = JSONDecoder.Decode(jsonGafs);
		if (!PredicateRegistry.IsInitialized())
		{
			BWLog.Info("Predicate Registry not initialized in IsJSONGAFListSupported(). Returning true.");
			return true;
		}
		foreach (JObject item in jObject.ArrayValue)
		{
			if (IsJSONGAFSupported(item))
			{
				continue;
			}
			BWLog.Info("Unsupported GAF detected in IsJSONGAFListSupported()");
			foreach (JObject item2 in item.ArrayValue)
			{
				BWLog.Info((string)item2);
			}
			return false;
		}
		return true;
	}

	public static List<GAF> GetAllGlobalGAFs()
	{
		List<GAF> list = new List<GAF>();
		list.AddRange(new GAF[3]
		{
			new GAF("Meta.TileBackground"),
			new GAF("Meta.TileBackgroundWithLabel"),
			new GAF("Meta.ButtonBackground")
		});
		list.Add(new GAF("Meta.Then"));
		foreach (UIInputControl.ControlVariant allButtonVariant in UIInputControl.allButtonVariants)
		{
			string text = allButtonVariant.ToString();
			GAF item = new GAF("Control", text);
			GAF item2 = new GAF("Control", text + " Pressed");
			list.Add(item);
			list.Add(item2);
			foreach (UIInputControl.ControlType allButtonType in UIInputControl.allButtonTypes)
			{
				GAF item3 = new GAF("Block.ButtonInput", allButtonType.ToString() + " " + text);
				list.Add(item3);
			}
		}
		return list;
	}
}
