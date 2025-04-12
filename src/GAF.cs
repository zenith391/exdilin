using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x02000164 RID: 356
public class GAF
{
	// Token: 0x06001548 RID: 5448 RVA: 0x00094B08 File Offset: 0x00092F08
	public GAF(BlockItem blockItem)
	{
		this._blockItemId = blockItem.Id;
		this.Predicate = PredicateRegistry.ByName(blockItem.GafPredicateName, true);
		if (blockItem.GafDefaultArgs == null)
		{
			this.Args = new object[1];
		}
		else
		{
			this.Args = new object[blockItem.GafDefaultArgs.Length];
			for (int i = 0; i < this.Args.Length; i++)
			{
				this.Args[i] = blockItem.GafDefaultArgs[i];
			}
		}
		this._hashCode = this.CalculateHashCode();
	}

	// Token: 0x06001549 RID: 5449 RVA: 0x00094B9D File Offset: 0x00092F9D
	public GAF(string predicateName, params object[] args)
	{
		if (args == null)
		{
			args = new object[1];
		}
		this.Predicate = PredicateRegistry.ByName(predicateName, true);
		this.Args = args;
		this._hashCode = this.CalculateHashCode();
	}

	// Token: 0x0600154A RID: 5450 RVA: 0x00094BD3 File Offset: 0x00092FD3
	public GAF(Predicate predicate, object[] args, bool dummy)
	{
		this.Predicate = predicate;
		if (args == null)
		{
			args = new object[1];
		}
		this.Args = args;
		this._hashCode = this.CalculateHashCode();
	}

	// Token: 0x0600154B RID: 5451 RVA: 0x00094C03 File Offset: 0x00093003
	public GAF(Predicate predicate, params object[] args)
	{
		this.Predicate = predicate;
		if (args == null)
		{
			args = new object[1];
		}
		this.Args = args;
		this._hashCode = this.CalculateHashCode();
	}

	// Token: 0x0600154C RID: 5452 RVA: 0x00094C34 File Offset: 0x00093034
	private int CalculateHashCode()
	{
		int num = (this.Predicate == null) ? 17 : this.Predicate.GetHashCode();
		object[] args = this.Args;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] != null)
			{
				num ^= args[i].GetHashCode() << i;
			}
		}
		if (args.Length > this.Predicate.ArgTypes.Length)
		{
			BWLog.Warning(string.Concat(new object[]
			{
				"Found a GAF with more arguments than its predicate supports. Pred name: '",
				this.Predicate.Name,
				"',  Args.Length: ",
				args.Length,
				", Predicate.ArgTypes.Length: ",
				this.Predicate.ArgTypes.Length
			}));
		}
		return num;
	}

	// Token: 0x0600154D RID: 5453 RVA: 0x00094D04 File Offset: 0x00093104
	public GAF Clone()
	{
		object[] array = new object[this.Args.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = this.Args[i];
		}
		return new GAF(this.Predicate, array)
		{
			_blockItemId = this._blockItemId
		};
	}

	// Token: 0x0600154E RID: 5454 RVA: 0x00094D58 File Offset: 0x00093158
	public GAF ClonePart(int argCount)
	{
		object[] array = new object[argCount];
		for (int i = 0; i < argCount; i++)
		{
			array[i] = this.Args[i];
		}
		return new GAF(this.Predicate, array)
		{
			_blockItemId = this._blockItemId
		};
	}

	// Token: 0x17000060 RID: 96
	// (get) Token: 0x0600154F RID: 5455 RVA: 0x00094DA3 File Offset: 0x000931A3
	public int BlockItemId
	{
		get
		{
			if (this._blockItemId == 0)
			{
				this._blockItemId = GafToBlockItem.Find(this);
			}
			return this._blockItemId;
		}
	}

	// Token: 0x06001550 RID: 5456 RVA: 0x00094DC4 File Offset: 0x000931C4
	public bool HasDynamicLabel()
	{
		if (this.Predicate == Block.predicateSendCustomSignal || this.Predicate == Block.predicateSendCustomSignalModel)
		{
			string stringArgSafe = Util.GetStringArgSafe(this.Args, 0, "*");
			return stringArgSafe != "*";
		}
		if (Block.customVariablePredicates.Contains(this.Predicate))
		{
			string stringArgSafe2 = Util.GetStringArgSafe(this.Args, 0, "*");
			return stringArgSafe2 != "*";
		}
		if (GAF.dynamicLabelPredicates == null)
		{
			GAF.dynamicLabelPredicates = new HashSet<Predicate>
			{
				Block.predicateCreateModel,
				Block.predicateCustomTag
			};
		}
		return GAF.dynamicLabelPredicates.Contains(this.Predicate);
	}

	// Token: 0x06001551 RID: 5457 RVA: 0x00094E80 File Offset: 0x00093280
	public string GetDynamicLabel()
	{
		string text = Util.GetStringArgSafe(this.Args, 0, string.Empty);
		if (Block.blockVariableOperationsOnGlobals.Contains(this.Predicate))
		{
			string stringArgSafe = Util.GetStringArgSafe(this.Args, 1, string.Empty);
			text = Block.GetLabelForPredicate(this.Predicate, text, stringArgSafe);
		}
		else if (Block.blockVariableOperationsOnOtherBlockVars.Contains(this.Predicate))
		{
			string stringArgSafe2 = Util.GetStringArgSafe(this.Args, 1, string.Empty);
			text = Block.GetLabelForPredicate(this.Predicate, text, stringArgSafe2);
		}
		return text;
	}

	// Token: 0x06001552 RID: 5458 RVA: 0x00094F0F File Offset: 0x0009330F
	public TileResultCode RunSensor(Block block, ScriptRowExecutionInfo eInfo)
	{
		return this.Predicate.RunSensor(block, eInfo, this.Args);
	}

	// Token: 0x06001553 RID: 5459 RVA: 0x00094F24 File Offset: 0x00093324
	public TileResultCode RunAction(Block block, ScriptRowExecutionInfo eInfo)
	{
		return this.Predicate.RunAction(block, eInfo, this.Args);
	}

	// Token: 0x06001554 RID: 5460 RVA: 0x00094F39 File Offset: 0x00093339
	public bool UpdatesIconOnArgumentChange()
	{
		return this.Predicate.updatesIconOnArgumentChange;
	}

	// Token: 0x06001555 RID: 5461 RVA: 0x00094F46 File Offset: 0x00093346
	public bool CanBeAction()
	{
		return this.Predicate.Action != null;
	}

	// Token: 0x06001556 RID: 5462 RVA: 0x00094F59 File Offset: 0x00093359
	public bool CanBeCondition()
	{
		return this.Predicate.Sensor != null;
	}

	// Token: 0x06001557 RID: 5463 RVA: 0x00094F6C File Offset: 0x0009336C
	public bool AllowTilesAfter()
	{
		return !Block.noTilesAfterPredicates.Contains(this.Predicate);
	}

	// Token: 0x06001558 RID: 5464 RVA: 0x00094F81 File Offset: 0x00093381
	public bool IsRare()
	{
		return this.Rare;
	}

	// Token: 0x06001559 RID: 5465 RVA: 0x00094F89 File Offset: 0x00093389
	public bool IsCreateModel()
	{
		return this.Predicate == Block.predicateCreateModel;
	}

	// Token: 0x0600155A RID: 5466 RVA: 0x00094F98 File Offset: 0x00093398
	public bool IsButtonInput()
	{
		return this.Predicate.Name == "Block.ButtonInput";
	}

	// Token: 0x0600155B RID: 5467 RVA: 0x00094FB0 File Offset: 0x000933B0
	public bool IsPaint()
	{
		if (GAF.paintPredicates == null)
		{
			GAF.paintPredicates = new HashSet<Predicate>
			{
				Block.predicatePaintTo,
				PredicateRegistry.ByName("Block.PaintSkyTo", true),
				PredicateRegistry.ByName("Master.PaintSkyTo", true),
				PredicateRegistry.ByName("Master.PaintTerrainTo", true)
			};
		}
		return GAF.paintPredicates.Contains(this.Predicate);
	}

	// Token: 0x0600155C RID: 5468 RVA: 0x00095028 File Offset: 0x00093428
	public bool Matches(GAF other)
	{
		if (other.Predicate != this.Predicate)
		{
			return false;
		}
		if (this.Predicate == Block.predicateCreateModel)
		{
			int intArg = Util.GetIntArg(this.Args, 0, -1);
			int intArg2 = Util.GetIntArg(other.Args, 0, -2);
			return intArg == intArg2;
		}
		return other.BlockItemId == this.BlockItemId;
	}

	// Token: 0x0600155D RID: 5469 RVA: 0x00095090 File Offset: 0x00093490
	public bool MatchesAny(HashSet<GAF> gafs)
	{
		if (gafs != null)
		{
			foreach (GAF gaf in gafs)
			{
				if (gaf != null && this.Matches(gaf))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	// Token: 0x0600155E RID: 5470 RVA: 0x00095104 File Offset: 0x00093504
	public override bool Equals(object other)
	{
		if (!(other is GAF))
		{
			return false;
		}
		GAF gaf = (GAF)other;
		if (gaf.Predicate != this.Predicate)
		{
			return false;
		}
		if (this.Args.Length != gaf.Args.Length)
		{
			return false;
		}
		for (int i = 0; i < this.Args.Length; i++)
		{
			if (gaf.Args[i] != this.Args[i] && !gaf.Args[i].Equals(this.Args[i]))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x0600155F RID: 5471 RVA: 0x0009519A File Offset: 0x0009359A
	public bool EqualsInTutorial(GAF g2)
	{
		return Tutorial.GAFsEqualInTutorial(this, g2);
	}

	// Token: 0x06001560 RID: 5472 RVA: 0x000951A3 File Offset: 0x000935A3
	public override int GetHashCode()
	{
		return this._hashCode;
	}

	// Token: 0x06001561 RID: 5473 RVA: 0x000951AC File Offset: 0x000935AC
	public override string ToString()
	{
		string str = this.Predicate.Name + "(";
		for (int i = 0; i < this.Args.Length; i++)
		{
			if (i != 0)
			{
				str += ",";
			}
			object obj = this.Args[i];
			string str2;
			if (obj == null)
			{
				str2 = "*";
			}
			else if (obj is float || obj is int || obj is bool || obj is string || obj is Vector3 || obj is Quaternion)
			{
				str2 = obj.ToString();
			}
			else
			{
				BWLog.Error(string.Format("Don't know how to serialize this object: {0}", obj.GetType().ToString()));
				str2 = obj.ToString();
			}
			str += str2;
		}
		return str + ")";
	}

	// Token: 0x06001562 RID: 5474 RVA: 0x00095298 File Offset: 0x00093698
	public bool FixSemanticallyInvalidness()
	{
		Predicate predicate = this.Predicate;
		if (predicate == Block.predicateTextureTo)
		{
			string key = (string)this.Args[0];
			if (!Materials.textureInfos.ContainsKey(key))
			{
				this.Args[0] = "Plain";
				return true;
			}
		}
		if (predicate == Block.predicatePaintTo)
		{
			string key2 = (string)this.Args[0];
			if (!Blocksworld.colorDefinitions.ContainsKey(key2))
			{
				this.Args[0] = "Yellow";
				return true;
			}
		}
		if (predicate == Block.predicateCreate)
		{
			string item = (string)this.Args[0];
			if (!Blocksworld.existingBlockNames.Contains(item))
			{
				this.Args[0] = "Cube";
				return true;
			}
		}
		if (predicate == Block.predicatePlaySoundDurational)
		{
			string text = (string)this.Args[0];
			if (!Sound.existingSfxs.Contains(text) && !Sound.existingSfxs.Contains("SFX " + text))
			{
				this.Args[0] = "Checkpoint";
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001563 RID: 5475 RVA: 0x000953AC File Offset: 0x000937AC
	public bool IsSemanticallyValid()
	{
		Predicate predicate = this.Predicate;
        if (BWEnvConfig.Flags["SKIP_GAF_VERIFICATION"])
        {
            return true;
        }
        if (predicate == Block.predicateTextureTo)
		{
			string text = (string)this.Args[0];
			if (!Materials.textureInfos.ContainsKey(text))
			{
				BWLog.Info("GAF validation error: Could not find texture '" + text + "'");
				return false;
			}
		}
		else if (predicate == Block.predicatePaintTo)
		{
			string text2 = (string)this.Args[0];
			if (!Blocksworld.colorDefinitions.ContainsKey(text2))
			{
				BWLog.Info("GAF validation error: Could not find paint '" + text2 + "'");
				return false;
			}
		}
		else if (predicate == Block.predicateCreate)
		{
			string text3 = (string)this.Args[0];
			if (!Blocksworld.existingBlockNames.Contains(text3))
			{
				BWLog.Info("GAF validation error: Could not find block '" + text3 + "'");
				return false;
			}
		}
		else if (predicate == Block.predicatePlaySoundDurational)
		{
			string text4 = (string)this.Args[0];
			if (!Sound.existingSfxs.Contains(text4) && !Sound.existingSfxs.Contains("SFX " + text4))
			{
				BWLog.Info("GAF validation error: Could not find SFX '" + text4 + "'");
				return false;
			}
		}
		return true;
	}

	// Token: 0x06001564 RID: 5476 RVA: 0x000954F0 File Offset: 0x000938F0
	public static GAF FromJSON(JObject obj, bool nullOnFailure = false, bool logOnFailure = true)
	{
		if (obj.ObjectValue != null)
		{
			string text = (string)obj["predicate"];
			text = SymbolCompat.RenamePredicate(text);
			Predicate predicate = PredicateRegistry.ByName(text, logOnFailure);
			if (predicate == null)
			{
				if (BW.isUnityEditor && Options.CreateErrorGafs)
				{
					return new GAF("Error", new object[]
					{
						"Predicate not found: " + text
					});
				}
				return null;
			}
			else
			{
				List<JObject> arrayValue = obj["args"].ArrayValue;
				Type[] argTypes = predicate.ArgTypes;
				if (predicate.ArgTypes.Length < arrayValue.Count)
				{
					if (logOnFailure)
					{
						BWLog.Error(string.Concat(new object[]
						{
							"Found more arguments than supported for predicate ",
							predicate.Name,
							" Max supported: ",
							predicate.ArgTypes.Length,
							" Found: ",
							arrayValue.Count
						}));
					}
					return null;
				}
				int num = Mathf.Min(arrayValue.Count, argTypes.Length);
				object[] array = new object[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = GAF.ArgFromJSON(predicate.ArgTypes[i], obj["args"][i]);
				}
				array = predicate.ExtendArguments(array, false);
				return SymbolCompat.RenameGAF(new GAF(predicate, array));
			}
		}
		else
		{
			if (obj.ArrayValue == null)
			{
				if (logOnFailure)
				{
					BWLog.Error("JObject obj is not a valid GAF in GAF.FromJSON()");
				}
				return null;
			}
			string text2 = (string)obj[0];
			text2 = SymbolCompat.RenamePredicate(text2);
			Predicate predicate2 = PredicateRegistry.ByName(text2, logOnFailure);
			if (predicate2 == null)
			{
				if (BW.isUnityEditor && Options.CreateErrorGafs)
				{
					return new GAF("Error", new object[]
					{
						"Predicate not found: " + text2
					});
				}
				return null;
			}
			else
			{
				List<JObject> arrayValue2 = obj.ArrayValue;
				Type[] argTypes2 = predicate2.ArgTypes;
				if (arrayValue2.Count - 1 > argTypes2.Length)
				{
					if (logOnFailure)
					{
						BWLog.Error(string.Concat(new object[]
						{
							"Found more arguments than supported for predicate ",
							predicate2.Name,
							" Max supported: ",
							predicate2.ArgTypes.Length,
							" Found: ",
							arrayValue2.Count - 1
						}));
					}
					return null;
				}
				int num2 = Mathf.Min(arrayValue2.Count - 1, argTypes2.Length);
				object[] array2 = new object[num2];
				for (int j = 1; j < obj.ArrayValue.Count; j++)
				{
					array2[j - 1] = GAF.ArgFromJSON(predicate2.ArgTypes[j - 1], obj[j]);
				}
				array2 = predicate2.ExtendArguments(array2, false);
				return SymbolCompat.RenameGAF(new GAF(predicate2, array2));
			}
		}
	}

	// Token: 0x06001565 RID: 5477 RVA: 0x000957C4 File Offset: 0x00093BC4
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
			encoder.WriteBool(false);
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

	// Token: 0x06001566 RID: 5478 RVA: 0x000958B4 File Offset: 0x00093CB4
	public void ToJSON(JSONStreamEncoder encoder)
	{
		encoder.BeginObject();
		encoder.WriteKey("predicate");
		encoder.WriteString(this.Predicate.Name);
		encoder.WriteKey("args");
		encoder.BeginArray();
		for (int i = 0; i < this.Args.Length; i++)
		{
			object obj = this.Args[i];
			try
			{
				GAF.WriteArgJSON(encoder, obj, false);
			}
			catch
			{
				string text = "GAF_Serialization_Error";
				string text2 = string.Concat(new object[]
				{
					"World:",
					WorldSession.current.worldTitle,
					"Predicate:",
					this.Predicate.Name,
					" argument index:",
					i
				});
				BWLog.Error(text + text2);
				if (i < this.Predicate.ArgTypes.Length)
				{
					this.WriteDefaultArgValue(encoder, this.Predicate.ArgTypes[i], false);
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

	// Token: 0x06001567 RID: 5479 RVA: 0x000959E0 File Offset: 0x00093DE0
	public void ToJSONCompact(JSONStreamEncoder encoder)
	{
		encoder.BeginArray();
		string text = this.Predicate.Name;
		object[] array = this.Args;
		if (Blocksworld.useCompactGafWriteRenamings)
		{
			string text2;
			if (SymbolCompat.predicateInvRenamings.TryGetValue(text, out text2))
			{
				text = text2;
			}
			string text3 = null;
			string text6;
			if (this.Predicate == Block.predicateCreate)
			{
				string text4;
				if (SymbolCompat.blockInvRenamings.TryGetValue((string)this.Args[0], out text4))
				{
					text3 = text4;
				}
			}
			else if (this.Predicate == Block.predicatePaintTo)
			{
				string text5;
				if (SymbolCompat.paintInvRenamings.TryGetValue((string)this.Args[0], out text5))
				{
					text3 = text5;
				}
			}
			else if (this.Predicate == Block.predicateTextureTo && SymbolCompat.textureInvRenamings.TryGetValue((string)this.Args[0], out text6))
			{
				text3 = text6;
			}
			if (text3 != null)
			{
				array = (object[])this.Args.Clone();
				array[0] = text3;
			}
		}
		encoder.WriteString(text);
		for (int i = 0; i < array.Length; i++)
		{
			object obj = array[i];
			try
			{
				GAF.WriteArgJSON(encoder, obj, true);
			}
			catch
			{
				string text7 = "GAF_Serialization_Error";
				string text8 = string.Concat(new object[]
				{
					"World:",
					WorldSession.current.worldTitle,
					"Predicate:",
					this.Predicate.Name,
					" argument index:",
					i
				});
				BWLog.Error(text7 + text8);
				if (i < this.Predicate.ArgTypes.Length)
				{
					this.WriteDefaultArgValue(encoder, this.Predicate.ArgTypes[i], true);
				}
				else
				{
					encoder.WriteNumber(0L);
				}
				BW.Analytics.SendAnalyticsEvent(text7, text8);
			}
		}
		encoder.EndArray();
	}

	// Token: 0x17000061 RID: 97
	// (get) Token: 0x06001568 RID: 5480 RVA: 0x00095BD4 File Offset: 0x00093FD4
	public bool HasBuildPanelLabel
	{
		get
		{
			return (this.Predicate != Block.predicateCreate && this.Predicate != Block.predicateTextureTo && this.Predicate != Block.predicatePaintTo && !Blocksworld.globalGafs.Contains(this)) || this.Predicate == Block.predicateButton || this.Predicate == Block.predicateThen;
		}
	}

	// Token: 0x06001569 RID: 5481 RVA: 0x00095C44 File Offset: 0x00094044
	public static void WriteGAFCountDictionary(JSONStreamEncoder encoder, Dictionary<GAF, int> dict)
	{
		encoder.BeginArray();
		encoder.InsertNewline();
		foreach (KeyValuePair<GAF, int> keyValuePair in dict)
		{
			encoder.BeginArray();
			keyValuePair.Key.ToJSONCompact(encoder);
			if (keyValuePair.Value == -1)
			{
				encoder.WriteString("infinity");
			}
			else
			{
				encoder.WriteNumber((long)keyValuePair.Value);
			}
			encoder.EndArray();
			encoder.InsertNewline();
		}
		encoder.EndArray();
	}

	// Token: 0x0600156A RID: 5482 RVA: 0x00095CF0 File Offset: 0x000940F0
	public static void WriteArgJSON(JSONStreamEncoder encoder, object obj, bool compact = false)
	{
		if (obj is float)
		{
			encoder.WriteNumber((float)obj);
		}
		else if (obj is int)
		{
			encoder.WriteNumber((long)((int)obj));
		}
		else if (obj is bool)
		{
			encoder.WriteBool((bool)obj);
		}
		else if (obj is string)
		{
			encoder.WriteString((string)obj);
		}
		else if (obj is Vector3)
		{
			((Vector3)obj).ToJSON(encoder, !Options.NoSnapSave, compact, Blocksworld.useCompactGafWriteRenamings);
		}
		else if (obj is Quaternion)
		{
			((Quaternion)obj).ToJSON(encoder);
		}
		else
		{
			if (obj == null)
			{
				throw new ArgumentException("obj", "Trying to serialize null object");
			}
			throw new ArgumentException("obj", string.Format("Don't know how to serialize this object: {0}", obj.GetType().ToString()));
		}
	}

	// Token: 0x0600156B RID: 5483 RVA: 0x00095DEC File Offset: 0x000941EC
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

	// Token: 0x0600156C RID: 5484 RVA: 0x00095EAC File Offset: 0x000942AC
	public static bool IsJSONGAFSupported(JObject jgaf)
	{
		string text = (string)jgaf[0];
		if (!PredicateRegistry.ContainsName(text))
		{
			return false;
		}
		if (text != null)
		{
			if (text == "Block.Create" || text == "Block.TextureTo" || text == "Block.PaintTo")
			{
				GAF gaf = GAF.FromJSON(jgaf, false, true);
				if (gaf == null)
				{
					BWLog.Info("GAF.FromJSON() returned null. predName: " + text);
				}
				return TileIconManager.Instance.GAFHasIcon(gaf);
			}
		}
		return true;
	}

	// Token: 0x0600156D RID: 5485 RVA: 0x00095F3C File Offset: 0x0009433C
	public static bool IsJSONGAFListSupported(string jsonGafs)
	{
		JObject jobject = JSONDecoder.Decode(jsonGafs);
		if (!PredicateRegistry.IsInitialized())
		{
			BWLog.Info("Predicate Registry not initialized in IsJSONGAFListSupported(). Returning true.");
			return true;
		}
		foreach (JObject jobject2 in jobject.ArrayValue)
		{
			if (!GAF.IsJSONGAFSupported(jobject2))
			{
				BWLog.Info("Unsupported GAF detected in IsJSONGAFListSupported()");
				foreach (JObject obj in jobject2.ArrayValue)
				{
					BWLog.Info((string)obj);
				}
				return false;
			}
		}
		return true;
	}

	// Token: 0x0600156E RID: 5486 RVA: 0x00096020 File Offset: 0x00094420
	public static List<GAF> GetAllGlobalGAFs()
	{
		List<GAF> list = new List<GAF>();
		list.AddRange(new GAF[]
		{
			new GAF("Meta.TileBackground", new object[0]),
			new GAF("Meta.TileBackgroundWithLabel", new object[0]),
			new GAF("Meta.ButtonBackground", new object[0])
		});
		list.Add(new GAF("Meta.Then", new object[0]));
		foreach (UIInputControl.ControlVariant controlVariant in UIInputControl.allButtonVariants)
		{
			string text = controlVariant.ToString();
			GAF item = new GAF("Control", new object[]
			{
				text
			});
			GAF item2 = new GAF("Control", new object[]
			{
				text + " Pressed"
			});
			list.Add(item);
			list.Add(item2);
			foreach (UIInputControl.ControlType controlType in UIInputControl.allButtonTypes)
			{
				GAF item3 = new GAF("Block.ButtonInput", new object[]
				{
					controlType.ToString() + " " + text
				});
				list.Add(item3);
			}
		}
		return list;
	}

	// Token: 0x040010A9 RID: 4265
	public bool Rare;

	// Token: 0x040010AA RID: 4266
	public readonly Predicate Predicate;

	// Token: 0x040010AB RID: 4267
	public readonly object[] Args;

	// Token: 0x040010AC RID: 4268
	private readonly int _hashCode;

	// Token: 0x040010AD RID: 4269
	private int _blockItemId;

	// Token: 0x040010AE RID: 4270
	private static HashSet<Predicate> dynamicLabelPredicates;

	// Token: 0x040010AF RID: 4271
	private static HashSet<Predicate> paintPredicates;
}
