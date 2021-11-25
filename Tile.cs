using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x020002C9 RID: 713
public class Tile
{
	// Token: 0x06002075 RID: 8309 RVA: 0x000EE2D0 File Offset: 0x000EC6D0
	public Tile(GAF gaf)
	{
		if (gaf == null)
		{
			throw new ArgumentException("GAF can not be null", "gaf");
		}
		this.gaf = gaf;
		if (gaf.Predicate != null && gaf.Predicate.EditableParameter != null)
		{
			this.subParameterCount = gaf.Predicate.EditableParameter.subParameterCount;
		}
	}

	// Token: 0x06002076 RID: 8310 RVA: 0x000EE358 File Offset: 0x000EC758
	public Tile(Predicate pred, params object[] args)
	{
		this.gaf = new GAF(pred, args);
		if (this.gaf.Predicate.EditableParameter != null)
		{
			this.subParameterCount = this.gaf.Predicate.EditableParameter.subParameterCount;
		}
	}

	// Token: 0x06002077 RID: 8311 RVA: 0x000EE3CC File Offset: 0x000EC7CC
	public Tile(TileObject tileObject)
	{
		this.gaf = new GAF(Block.predicateUI, null);
		this.tileObject = tileObject;
	}

	// Token: 0x06002078 RID: 8312 RVA: 0x000EE41B File Offset: 0x000EC81B
	public void AssignToPanel(Panel panel)
	{
		this.parentPanel = panel;
	}

	// Token: 0x17000156 RID: 342
	// (get) Token: 0x06002079 RID: 8313 RVA: 0x000EE424 File Offset: 0x000EC824
	public string UniqueID
	{
		get
		{
			if (this._uniqueID == null)
			{
				this._uniqueID = this.gaf.ToString();
			}
			return this._uniqueID;
		}
	}

	// Token: 0x0600207A RID: 8314 RVA: 0x000EE448 File Offset: 0x000EC848
	public virtual Tile Clone()
	{
		return new Tile(this.gaf.Clone())
		{
			cachedBackgroundColor = this.cachedBackgroundColor,
			cachedForegroundColor = this.cachedForegroundColor
		};
	}

	// Token: 0x0600207B RID: 8315 RVA: 0x000EE47F File Offset: 0x000EC87F
	public string GetLabelText()
	{
		return TileIconManager.Instance.GetLabelStr(this.gaf);
	}

	// Token: 0x0600207C RID: 8316 RVA: 0x000EE494 File Offset: 0x000EC894
	public void ToJSON(JSONStreamEncoder encoder, bool compact = false)
	{
		if (compact)
		{
			this.gaf.ToJSONCompact(encoder);
		}
		else
		{
			encoder.BeginObject();
			encoder.WriteKey("type");
			encoder.WriteString("tile");
			encoder.WriteKey("gaf");
			this.gaf.ToJSON(encoder);
			encoder.EndObject();
		}
	}

	// Token: 0x0600207D RID: 8317 RVA: 0x000EE4F4 File Offset: 0x000EC8F4
	public static Tile FromJSON(JObject obj)
	{
		if (obj.ObjectValue == null)
		{
			GAF gaf = GAF.FromJSON(obj, false, true);
			if (gaf == null)
			{
				return null;
			}
			if (gaf.IsSemanticallyValid())
			{
				return (gaf != null) ? new Tile(gaf) : null;
			}
			if (!BW.isUnityEditor || !Options.CreateErrorGafs)
			{
				return null;
			}
			if (!gaf.FixSemanticallyInvalidness())
			{
				BWLog.Error("Could not fix semantic issue");
				return null;
			}
			return new Tile(gaf);
		}
		else
		{
			if (obj.ObjectValue.ContainsKey("sym"))
			{
				return new Tile(SymbolCompat.ToGaf(obj));
			}
			GAF gaf2 = GAF.FromJSON(obj["gaf"], false, true);
			if (gaf2 == null)
			{
				return null;
			}
			if (gaf2.IsSemanticallyValid())
			{
				return (gaf2 != null) ? new Tile(gaf2) : null;
			}
			if (!BW.isUnityEditor || !Options.CreateErrorGafs)
			{
				return null;
			}
			if (!gaf2.FixSemanticallyInvalidness())
			{
				BWLog.Error("Could not fix semantic issue");
				return null;
			}
			return new Tile(gaf2);
		}
	}

	// Token: 0x0600207E RID: 8318 RVA: 0x000EE5FE File Offset: 0x000EC9FE
	public void Destroy()
	{
		this.DestroyPhysical();
	}

	// Token: 0x0600207F RID: 8319 RVA: 0x000EE606 File Offset: 0x000ECA06
	public override string ToString()
	{
		return this.gaf.ToString();
	}

	// Token: 0x06002080 RID: 8320 RVA: 0x000EE613 File Offset: 0x000ECA13
	public bool IsThen()
	{
		return this.gaf.Predicate == Block.predicateThen;
	}

	// Token: 0x06002081 RID: 8321 RVA: 0x000EE627 File Offset: 0x000ECA27
	public bool IsCreate()
	{
		return this.gaf.Predicate == Block.predicateCreate;
	}

	// Token: 0x06002082 RID: 8322 RVA: 0x000EE63B File Offset: 0x000ECA3B
	public bool IsCreateModel()
	{
		return this.gaf.Predicate == Block.predicateCreateModel;
	}

	// Token: 0x06002083 RID: 8323 RVA: 0x000EE64F File Offset: 0x000ECA4F
	public bool IsPaint()
	{
		return this.gaf.Predicate == Block.predicatePaintTo;
	}

	// Token: 0x06002084 RID: 8324 RVA: 0x000EE663 File Offset: 0x000ECA63
	public bool IsScriptGear()
	{
		return this.tileObject != null && this.tileObject.IconName() == UIQuickSelect.scriptButtonIconName;
	}

	// Token: 0x06002085 RID: 8325 RVA: 0x000EE68E File Offset: 0x000ECA8E
	public bool IsCopiedModel()
	{
		return this.tileObject != null && this.tileObject.IconName() == UIQuickSelect.modelButtonIconName;
	}

	// Token: 0x06002086 RID: 8326 RVA: 0x000EE6B9 File Offset: 0x000ECAB9
	public bool IsUIOnly()
	{
		return this.gaf.Predicate == Block.predicateUI;
	}

	// Token: 0x06002087 RID: 8327 RVA: 0x000EE6CD File Offset: 0x000ECACD
	public bool IsTexture()
	{
		return this.gaf.Predicate == Block.predicateTextureTo;
	}

	// Token: 0x06002088 RID: 8328 RVA: 0x000EE6E1 File Offset: 0x000ECAE1
	public bool IsSfx()
	{
		return this.gaf.Predicate == Block.predicatePlaySoundDurational;
	}

	// Token: 0x06002089 RID: 8329 RVA: 0x000EE6F5 File Offset: 0x000ECAF5
	public bool IsSkyBox()
	{
		return this.gaf.Predicate == BlockSky.predicateSkyBoxTo;
	}

	// Token: 0x0600208A RID: 8330 RVA: 0x000EE709 File Offset: 0x000ECB09
	public bool IsLocked()
	{
		return this.gaf.Predicate == PredicateRegistry.ByName("Block.Locked", true);
	}

	// Token: 0x0600208B RID: 8331 RVA: 0x000EE723 File Offset: 0x000ECB23
	public void MoveTo(float x, float y)
	{
		this.MoveTo(new Vector3(x, y), true);
	}

	// Token: 0x0600208C RID: 8332 RVA: 0x000EE733 File Offset: 0x000ECB33
	public void MoveTo(float x, float y, float z)
	{
		this.MoveTo(new Vector3(x, y, z), false);
	}

	// Token: 0x0600208D RID: 8333 RVA: 0x000EE744 File Offset: 0x000ECB44
	public void MoveTo(Vector3 pos, bool useExistingZ = false)
	{
		if (this.parentPanel != null)
		{
			Vector3 position = this.parentPanel.GetTransform().position;
			this.LocalMoveTo(pos - position, useExistingZ);
			if (this.tileObject != null)
			{
				this.tileObject.SetPosition(pos);
			}
			else
			{
				this.cachedPosition = pos;
			}
			return;
		}
		if (this.tileObjectAssignedParent != null)
		{
			Vector3 position2 = this.tileObjectAssignedParent.position;
			this.LocalMoveTo(pos - position2, useExistingZ);
			if (this.tileObject != null)
			{
				this.tileObject.SetPosition(pos);
			}
			else
			{
				this.cachedPosition = pos;
			}
			return;
		}
		if (this.IsShowing())
		{
			if (useExistingZ)
			{
				pos.z = this.tileObject.GetPosition().z;
			}
			pos = new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), pos.z);
			this.tileObject.SetPosition(pos);
		}
		else
		{
			if (useExistingZ)
			{
				pos.z = this.cachedPosition.z;
			}
			this.cachedPosition = new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), pos.z);
		}
	}

	// Token: 0x0600208E RID: 8334 RVA: 0x000EE8A0 File Offset: 0x000ECCA0
	public void SmoothMoveTo(Vector3 pos)
	{
		if (this.parentPanel != null)
		{
			Vector3 position = this.parentPanel.GetTransform().position;
			this.LocalMoveTo(pos - position, false);
			return;
		}
		bool flag = this.tileObject == null;
		if (flag)
		{
			this.Show(true);
		}
		this.tileObject.SetPosition(pos);
	}

	// Token: 0x0600208F RID: 8335 RVA: 0x000EE8FE File Offset: 0x000ECCFE
	public void LocalMoveTo(float x, float y)
	{
		this.LocalMoveTo(new Vector3(x, y), true);
	}

	// Token: 0x06002090 RID: 8336 RVA: 0x000EE90E File Offset: 0x000ECD0E
	public void LocalMoveTo(float x, float y, float z)
	{
		this.LocalMoveTo(new Vector3(x, y, z), false);
	}

	// Token: 0x06002091 RID: 8337 RVA: 0x000EE920 File Offset: 0x000ECD20
	public void LocalMoveTo(Vector3 pos, bool useExistingZ = false)
	{
		if (this.parentPanel != null)
		{
			this.positionInPanel = new Vector3(pos.x, pos.y, (!useExistingZ) ? pos.z : this.positionInPanel.z);
		}
		else if (this.tileObject != null)
		{
			if (useExistingZ)
			{
				pos.z = this.tileObject.GetLocalPosition().z;
			}
			this.tileObject.SetLocalPosition(new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), pos.z));
		}
		this.cachedLocalPosition = pos;
	}

	// Token: 0x06002092 RID: 8338 RVA: 0x000EE9DC File Offset: 0x000ECDDC
	public Vector3 GetPosition()
	{
		if (this.tileObject != null)
		{
			return this.tileObject.GetPosition();
		}
		if (this.parentPanel != null)
		{
			Vector3 position = this.parentPanel.GetTransform().position;
			return position + this.positionInPanel;
		}
		return this.cachedPosition;
	}

	// Token: 0x06002093 RID: 8339 RVA: 0x000EEA35 File Offset: 0x000ECE35
	public Vector3 GetLocalPosition()
	{
		return this.cachedLocalPosition;
	}

	// Token: 0x06002094 RID: 8340 RVA: 0x000EEA40 File Offset: 0x000ECE40
	public Vector3 GetCenterPosition()
	{
		if (this.tileObject != null)
		{
			return this.tileObject.GetCenterPosition();
		}
		return this.cachedPosition + new Vector3(33f, 33f, 0f) * NormalizedScreen.scale;
	}

	// Token: 0x06002095 RID: 8341 RVA: 0x000EEA93 File Offset: 0x000ECE93
	public Vector3 GetScale()
	{
		if (this.tileObject != null)
		{
			return this.tileObject.GetScale();
		}
		return 80f * Vector3.one;
	}

	// Token: 0x06002096 RID: 8342 RVA: 0x000EEAC6 File Offset: 0x000ECEC6
	public bool HitControlButton(Vector3 v)
	{
		return this.HitControlButton((string)this.gaf.Args[0], v);
	}

	// Token: 0x06002097 RID: 8343 RVA: 0x000EEAE4 File Offset: 0x000ECEE4
	public bool HitControlButton(string symbol, Vector3 v)
	{
		Vector4 vector;
		return Tile.buttonExtensions.TryGetValue(symbol, out vector) && this.HitExtended(v, vector.x, vector.y, vector.z, vector.w, false);
	}

	// Token: 0x06002098 RID: 8344 RVA: 0x000EEB29 File Offset: 0x000ECF29
	public bool Hit(Vector3 v, bool allowDisabledTiles = false)
	{
		return !(this.tileObject == null) && this.tileObject.Hit(v, allowDisabledTiles);
	}

	// Token: 0x06002099 RID: 8345 RVA: 0x000EEB4C File Offset: 0x000ECF4C
	public Bounds GetHitBounds()
	{
		Vector3 vector = Vector3.zero;
		if (this.parentPanel != null)
		{
			vector = this.parentPanel.GetTransform().position + this.positionInPanel;
		}
		else if (this.tileObject != null)
		{
			vector = this.tileObject.GetPosition();
		}
		float num = -7f * NormalizedScreen.pixelScale;
		Vector3 scale = this.GetScale();
		Vector3 b = new Vector3(vector.x - num, vector.y - num, -99999f);
		Vector3 a = new Vector3(vector.x + (float)((!this.doubleWidth) ? 1 : 2) * (scale.x + num), vector.y + scale.y + num, 99999f);
		return new Bounds(0.5f * (a + b), a - b);
	}

	// Token: 0x0600209A RID: 8346 RVA: 0x000EEC3A File Offset: 0x000ED03A
	public bool HitExtended(Vector3 v, float extendXMin, float extendXMax, float extendYMin, float extendYMax, bool allowDisabledTiles = false)
	{
		return !(this.tileObject == null) && this.tileObject.HitExtended(v, extendXMin, extendXMax, extendYMin, extendYMax, allowDisabledTiles);
	}

	// Token: 0x0600209B RID: 8347 RVA: 0x000EEC63 File Offset: 0x000ED063
	public TileResultCode Condition(Block obj, ScriptRowExecutionInfo eInfo)
	{
		return this.gaf.RunSensor(obj, eInfo);
	}

	// Token: 0x0600209C RID: 8348 RVA: 0x000EEC72 File Offset: 0x000ED072
	public TileResultCode Execute(Block obj, ScriptRowExecutionInfo eInfo)
	{
		return this.gaf.RunAction(obj, eInfo);
	}

	// Token: 0x0600209D RID: 8349 RVA: 0x000EEC84 File Offset: 0x000ED084
	public static void CreateAntigravityArgumentConverters()
	{
		Vector3[] array = new Vector3[]
		{
			Vector3.up,
			Vector3.down,
			Vector3.right,
			Vector3.left,
			Vector3.forward,
			Vector3.back
		};
		List<List<string>> list = new List<List<string>>
		{
			new List<string>
			{
				"AntiGravityColumn.IncreaseLocalVelocityChunk",
				"AntiGravity.IncreaseLocalVelocityChunk",
				"FlightYoke.IncreaseLocalVelocityChunk"
			},
			new List<string>
			{
				"AntiGravityColumn.IncreaseLocalTorqueChunk",
				"AntiGravity.IncreaseLocalTorqueChunk",
				"FlightYoke.IncreaseLocalTorqueChunk"
			}
		};
		Dictionary<string, List<GAF>> dictionary = new Dictionary<string, List<GAF>>();
		foreach (List<string> list2 in list)
		{
			foreach (string predicateName in list2)
			{
				foreach (Vector3 vector in array)
				{
					GAF item = new GAF(predicateName, new object[]
					{
						vector
					});
					string pathToIcon = TileIconManager.Instance.GetPathToIcon(item);
					if (pathToIcon != null)
					{
						if (!dictionary.ContainsKey(pathToIcon))
						{
							dictionary[pathToIcon] = new List<GAF>();
						}
						dictionary[pathToIcon].Add(item);
					}
				}
			}
		}
		foreach (KeyValuePair<string, List<GAF>> keyValuePair in dictionary)
		{
			foreach (GAF gaf in keyValuePair.Value)
			{
				foreach (GAF gaf2 in keyValuePair.Value)
				{
					if (gaf != gaf2)
					{
						Vector3 vector2 = (Vector3)gaf.Args[0];
						Vector3 vector3 = (Vector3)gaf2.Args[0];
						if (vector2 != vector3)
						{
							Tile.AddVectorArgConverter(gaf.Predicate, gaf2.Predicate, vector2, vector3);
						}
					}
				}
			}
		}
	}

	// Token: 0x0600209E RID: 8350 RVA: 0x000EEF94 File Offset: 0x000ED394
	private static void AddVectorArgConverter(Predicate p1, Predicate p2, Vector3 v1, Vector3 v2)
	{
		PredicateRegistry.AddEquivalentPredicateArgumentConverter(p1, p2, delegate(object[] args)
		{
			if ((Vector3)args[0] == v1)
			{
				args[0] = v2;
				return true;
			}
			return false;
		});
	}

	// Token: 0x0600209F RID: 8351 RVA: 0x000EEFC8 File Offset: 0x000ED3C8
	public static void UpdateTileParameterSettings()
	{
		TileParameterSettings component = Blocksworld.blocksworldDataContainer.GetComponent<TileParameterSettings>();
		TileParameterEditor tileParameterEditor = (!(Blocksworld.bw != null)) ? null : Blocksworld.bw.tileParameterEditor;

		if (tileParameterEditor != null && tileParameterEditor.enabled)
		{
			Blocksworld.bw.parameterEditGesture.Cancel();
			tileParameterEditor.StopEditing();
		}
		foreach (TileParameterSetting tileParameterSetting in component.settings)
		{
			if (tileParameterSetting.activated)
			{
				foreach (string name in tileParameterSetting.matchingPredicateNames)
				{
					Predicate predicate = PredicateRegistry.ByName(name, true);

					if (predicate != null)
					{
						bool flag = false;
                        switch (tileParameterSetting.type)
						{
						case TileParameterType.IntSlider:
							predicate.EditableParameter = new IntTileParameter(tileParameterSetting.intMinValue, tileParameterSetting.intMaxValue, tileParameterSetting.intStep, tileParameterSetting.sliderSensitivity, tileParameterSetting.parameterIndex, null, false, tileParameterSetting.prefixValueString, tileParameterSetting.postfixValueString);
							break;
						case TileParameterType.IntPresentingFloatSlider:
							predicate.EditableParameter = new IntTileParameter(tileParameterSetting.intMinValue, tileParameterSetting.intMaxValue, tileParameterSetting.intStep, tileParameterSetting.sliderSensitivity, tileParameterSetting.parameterIndex, Tile.GetBidiIntFloatConverter(tileParameterSetting), tileParameterSetting.intOnlyShowPositive, tileParameterSetting.prefixValueString, tileParameterSetting.postfixValueString);
							flag = true;
							break;
						case TileParameterType.FloatSlider:
							predicate.EditableParameter = new FloatTileParameter(tileParameterSetting.floatMinValue, tileParameterSetting.floatMaxValue, tileParameterSetting.floatStep, tileParameterSetting.sliderSensitivity, tileParameterSetting.parameterIndex, tileParameterSetting.floatOnlyShowPositive, tileParameterSetting.prefixValueString, tileParameterSetting.postfixValueString);
							flag = true;
							break;
						case TileParameterType.TimeSlider:
							predicate.EditableParameter = new TimeTileParameter(tileParameterSetting.parameterIndex, tileParameterSetting.sliderSensitivity);
							flag = true;
							break;
						case TileParameterType.StringSingleLine:
							predicate.EditableParameter = new StringTileParameter(tileParameterSetting.parameterIndex, false, tileParameterSetting.stringAcceptAny, tileParameterSetting.stringAcceptAnyHint);
                            break;
						case TileParameterType.StringMultiLine:
							predicate.EditableParameter = new StringTileParameter(tileParameterSetting.parameterIndex, true, tileParameterSetting.stringAcceptAny, tileParameterSetting.stringAcceptAnyHint);
							break;
						case TileParameterType.ColorSlider:
							predicate.EditableParameter = new ColorTileParameter(tileParameterSetting.intMinValue, tileParameterSetting.intMaxValue, tileParameterSetting.intStep, tileParameterSetting.sliderSensitivity, tileParameterSetting.parameterIndex, 2);
							break;
						case TileParameterType.ScoreSlider:
							predicate.EditableParameter = new ScoreTileParameter(tileParameterSetting.parameterIndex);
							break;
						case TileParameterType.WorldId:
							predicate.EditableParameter = new UserWorldTileParameter(tileParameterSetting.parameterIndex);
							break;
						case TileParameterType.EnumerationSlider:
							predicate.EditableParameter = new IntTileParameter(tileParameterSetting.intMinValue, tileParameterSetting.intMaxValue, tileParameterSetting.intStep, tileParameterSetting.sliderSensitivity, tileParameterSetting.parameterIndex, Tile.GetBidiIntStringConverter(tileParameterSetting));
							break;
						}
						if (flag && (tileParameterSetting.setGafArgumentIfNotExists || tileParameterSetting.overwriteGafArgumentInBuildPanel))
						{
							predicate.argumentExtender = Tile.CreateArgumentExtender(tileParameterSetting, predicate);
						}
						if (predicate.EditableParameter != null)
						{
							predicate.EditableParameter.settings = tileParameterSetting;
						}
						predicate.canHaveOverlay = true;
					}
				}
			}
		}
	}

	// Token: 0x060020A0 RID: 8352 RVA: 0x000EF2F4 File Offset: 0x000ED6F4
	private static Func<object[], bool, object[]> CreateArgumentExtender(TileParameterSetting setting, Predicate pred)
	{
		return delegate(object[] args, bool overwrite)
		{
			if (setting.parameterIndex == args.Length || overwrite)
			{
				object[] array = args;
				bool flag = setting.parameterIndex + 1 > args.Length;
				if (overwrite || (flag && setting.setGafArgumentIfNotExists))
				{
					array = new object[Mathf.Max(setting.parameterIndex + 1, args.Length)];
					for (int i = 0; i < args.Length; i++)
					{
						array[i] = args[i];
					}
					object obj = (setting.parameterIndex >= args.Length) ? null : args[setting.parameterIndex];
					switch (setting.type)
					{
					case TileParameterType.IntPresentingFloatSlider:
					{
						IntTileParameter intTileParameter = pred.EditableParameter as IntTileParameter;
						if (intTileParameter != null && intTileParameter.converter != null)
						{
							int num = (!intTileParameter.onlyShowPositive || obj == null) ? 1 : ((int)Mathf.Sign((float)obj));
							array[setting.parameterIndex] = (float)num * intTileParameter.converter.intToFloat(setting.intDefaultValue);
						}
						else
						{
							BWLog.Info("Could not find converter for int-presenting slider");
						}
						break;
					}
					case TileParameterType.FloatSlider:
					{
						FloatTileParameter floatTileParameter = pred.EditableParameter as FloatTileParameter;
						if (floatTileParameter != null)
						{
							float num2 = (!floatTileParameter.onlyShowPositive || obj == null) ? 1f : Mathf.Sign((float)obj);
							array[setting.parameterIndex] = num2 * setting.floatDefaultValue;
						}
						break;
					}
					case TileParameterType.TimeSlider:
					{
						TimeTileParameter timeTileParameter = pred.EditableParameter as TimeTileParameter;
						if (timeTileParameter != null)
						{
							array[setting.parameterIndex] = setting.floatDefaultValue;
						}
						break;
					}
					default:
						BWLog.Info("Argument extension not supported for type " + setting.type);
						break;
					}
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] == null)
					{
						BWLog.Warning(string.Concat(new object[]
						{
							"Argument extension was trying to set an argument to null for predicate ",
							pred.Name,
							" and index ",
							j
						}));
						return args;
					}
				}
				return array;
			}
			return args;
		};
	}

	// Token: 0x060020A1 RID: 8353 RVA: 0x000EF321 File Offset: 0x000ED721
	private static BidiIntStringConverter GetBidiIntStringConverter(TileParameterSetting setting)
	{
		return new BidiIntStringConverter(setting.tableConverterStringValues, setting.intMinValue, setting.intStep);
	}

	// Token: 0x060020A2 RID: 8354 RVA: 0x000EF33C File Offset: 0x000ED73C
	private static BidiIntFloatConverter GetBidiIntFloatConverter(TileParameterSetting setting)
	{
		switch (setting.bidiIntFloatConverterType)
		{
		case BidiIntFloatConverterType.Affine:
			return new AffineBidiIntFloatConverter
			{
				bias = setting.affineConverterBias,
				multiplier = setting.affineConverterMultiplier
			};
		case BidiIntFloatConverterType.Range:
			return AffineBidiIntFloatConverter.FromRange(setting.rangeConverterFrom, setting.rangeConverterTo, setting.intMinValue, setting.intMaxValue);
		case BidiIntFloatConverterType.Table:
			return new TableBidiIntFloatConverter(setting.tableConverterFloatValues, setting.intMinValue, setting.intMaxValue, setting.intOnlyShowPositive);
		case BidiIntFloatConverterType.PiecewiseLinear:
			return new PiecewiseLinearIntFloatConverter(setting.piecewiseLinearConverterIntValues, setting.piecewiseLinearConverterFloatValues, setting.intOnlyShowPositive);
		default:
			return null;
		}
	}

	// Token: 0x060020A3 RID: 8355 RVA: 0x000EF3E0 File Offset: 0x000ED7E0
	public void Enable(bool enabled)
	{
		this.isEnabled = enabled;
		if (this.tileObject != null)
		{
			if (enabled)
			{
				this.tileObject.Enable();
			}
			else
			{
				this.tileObject.Disable();
			}
		}
	}

	// Token: 0x060020A4 RID: 8356 RVA: 0x000EF41B File Offset: 0x000ED81B
	public bool IsEnabled()
	{
		return this.isEnabled;
	}

	// Token: 0x060020A5 RID: 8357 RVA: 0x000EF424 File Offset: 0x000ED824
	public void Show(bool show)
	{
		if (this.gaf.Predicate == Block.predicateUI)
		{
			this.tileObject.Show(show);
			return;
		}
		if (this.parentPanel != null)
		{
			this.visibleInPanel = show;
			return;
		}
		if (show)
		{
			if (this.tileObject == null)
			{
				this.CreatePhysical();
				this.tileObject.SetPosition(this.cachedPosition);
				this.tileObject.SetLocalPosition(this.cachedLocalPosition);
				if (this.isEnabled)
				{
					this.tileObject.Enable();
				}
				else
				{
					this.tileObject.Disable();
				}
			}
		}
		else
		{
			if (this.tileObject != null)
			{
				this.cachedPosition = this.tileObject.GetPosition();
			}
			this.DestroyPhysical();
		}
	}

	// Token: 0x060020A6 RID: 8358 RVA: 0x000EF4F8 File Offset: 0x000ED8F8
	public void CreatePhysical()
	{
		if (this.tileObject == null)
		{
			GAF gaf = (!this.gaf.HasDynamicLabel()) ? Scarcity.GetNormalizedGaf(this.gaf, false) : this.gaf;
			this.UpdateDynamicLabelIfNecessary();
			if (this.IsCreateModel())
			{
				this.tileObject = Blocksworld.modelTilePool.GetTileObject(gaf, this.isEnabled, false);
			}
			else if (this.parentPanel != null)
			{
				this.tileObject = this.parentPanel.tileObjectPool.GetTileObject(gaf, this.isEnabled, false);
			}
			else
			{
				this.tileObject = Blocksworld.tilePool.GetTileObject(gaf, this.isEnabled, false);
				if (this.tileObjectAssignedParent != null)
				{
					this.tileObject.SetParent(this.tileObjectAssignedParent);
				}
			}
			if (this.cachedBackgroundColor != Color.clear)
			{
				this.tileObject.OverrideBackgroundColor(this.cachedBackgroundColor);
			}
			this.tileObject.OverrideForegroundColor(this.cachedForegroundColor);
		}
		this.tileObject.Show();
	}

	// Token: 0x060020A7 RID: 8359 RVA: 0x000EF618 File Offset: 0x000EDA18
	public void DestroyPhysical()
	{
		if (this.tileObject != null)
		{
			if (this.tileObject.obtainedFromPool != null)
			{
				this.tileObject.ReturnToPool();
			}
			else
			{
				UnityEngine.Object.Destroy(this.tileObject.GetGameObject());
			}
			this.tileObject = null;
		}
	}

	// Token: 0x060020A8 RID: 8360 RVA: 0x000EF670 File Offset: 0x000EDA70
	public bool UpdateDynamicLabelIfNecessary()
	{
		if (this.gaf.IsCreateModel() || !this.gaf.HasDynamicLabel())
		{
			return false;
		}
		string dynamicLabel = this.gaf.GetDynamicLabel();
		if (string.IsNullOrEmpty(dynamicLabel))
		{
			return false;
		}
		TileIconManager.Instance.labelAtlas.AddNewLabel(dynamicLabel);
		if (this.tileObject != null)
		{
			this.tileObject.Setup(this.gaf, this.isEnabled);
		}
		return true;
	}

	// Token: 0x060020A9 RID: 8361 RVA: 0x000EF6F1 File Offset: 0x000EDAF1
	public bool IsShowing()
	{
		return this.tileObject != null;
	}

	// Token: 0x060020AA RID: 8362 RVA: 0x000EF6FF File Offset: 0x000EDAFF
	public void SetTileBackgroundColor(Color bgColor)
	{
		if (this.tileObject != null)
		{
			this.tileObject.OverrideBackgroundColor(bgColor);
		}
		this.cachedBackgroundColor = bgColor;
	}

	// Token: 0x060020AB RID: 8363 RVA: 0x000EF725 File Offset: 0x000EDB25
	public void SetTileForegroundColor(Color fgColor)
	{
		if (this.tileObject != null)
		{
			this.tileObject.OverrideForegroundColor(fgColor);
		}
		this.cachedForegroundColor = fgColor;
	}

	// Token: 0x060020AC RID: 8364 RVA: 0x000EF74B File Offset: 0x000EDB4B
	public void SetTileScale(Vector3 scale)
	{
		if (this.tileObject != null)
		{
			this.tileObject.SetScale(scale);
		}
	}

	// Token: 0x060020AD RID: 8365 RVA: 0x000EF76A File Offset: 0x000EDB6A
	public void SetTileScale(float scale)
	{
		if (this.tileObject != null)
		{
			this.tileObject.SetScale(scale * Vector3.one);
		}
	}

	// Token: 0x060020AE RID: 8366 RVA: 0x000EF793 File Offset: 0x000EDB93
	public void SetParent(Transform p)
	{
		if (this.tileObject != null)
		{
			this.tileObject.SetParent(p);
		}
		this.tileObjectAssignedParent = p;
	}

	// Token: 0x060020AF RID: 8367 RVA: 0x000EF7B9 File Offset: 0x000EDBB9
	private static Color[] GradientToColors(Color[] colors)
	{
		return Tile.GradientToColors(colors[0], colors[1]);
	}

	// Token: 0x060020B0 RID: 8368 RVA: 0x000EF7D8 File Offset: 0x000EDBD8
	private static Color[] GradientToColors(Color top, Color bottom)
	{
		return new Color[]
		{
			top,
			bottom,
			bottom,
			top
		};
	}

	// Token: 0x060020B1 RID: 8369 RVA: 0x000EF814 File Offset: 0x000EDC14
	public bool UpdatesIconOnArgumentChange()
	{
		return this.gaf.UpdatesIconOnArgumentChange();
	}

	// Token: 0x060020B2 RID: 8370 RVA: 0x000EF821 File Offset: 0x000EDC21
	public void StepSubParameterIndex()
	{
		this.subParameterIndex = (this.subParameterIndex + 1) % this.subParameterCount;
		if (this.subParameterCount > 1)
		{
			Sound.PlayOneShotSound("Slider Handle Grabbed", 1f);
		}
	}

	// Token: 0x04001BB7 RID: 7095
	public GAF gaf;

	// Token: 0x04001BB8 RID: 7096
	public TileObject tileObject;

	// Token: 0x04001BB9 RID: 7097
	private Transform tileObjectAssignedParent;

	// Token: 0x04001BBA RID: 7098
	public float time;

	// Token: 0x04001BBB RID: 7099
	public bool doubleWidth;

	// Token: 0x04001BBC RID: 7100
	public int subParameterIndex;

	// Token: 0x04001BBD RID: 7101
	public int subParameterCount = 1;

	// Token: 0x04001BBE RID: 7102
	public static string[] tagNames = new string[]
	{
		"Circle",
		"Triangle",
		"Square",
		"Diamond",
		"Heart",
		"Star",
		"Hexagon",
		"X",
		"Target",
		"Hero",
		"Villain"
	};

	// Token: 0x04001BBF RID: 7103
	public static string[] shortTagNames = new string[]
	{
		"0",
		"1",
		"2",
		"3",
		"4",
		"5",
		"6",
		"7",
		"8",
		"A",
		"B"
	};

	// Token: 0x04001BC0 RID: 7104
	public static string iconBasePath = Application.dataPath + "/..";

	// Token: 0x04001BC1 RID: 7105
	public Panel parentPanel;

	// Token: 0x04001BC2 RID: 7106
	public Vector3 positionInPanel;

	// Token: 0x04001BC3 RID: 7107
	public bool visibleInPanel;

	// Token: 0x04001BC4 RID: 7108
	public int panelSection;

	// Token: 0x04001BC5 RID: 7109
	private bool isEnabled = true;

	// Token: 0x04001BC6 RID: 7110
	private Vector3 cachedPosition;

	// Token: 0x04001BC7 RID: 7111
	private Vector3 cachedLocalPosition;

	// Token: 0x04001BC8 RID: 7112
	private Color cachedBackgroundColor = Color.clear;

	// Token: 0x04001BC9 RID: 7113
	private Color cachedForegroundColor = Color.white;

	// Token: 0x04001BCA RID: 7114
	private string _uniqueID;

	// Token: 0x04001BCB RID: 7115
	public static Dictionary<string, Vector4> buttonExtensions = new Dictionary<string, Vector4>
	{
		{
			"L",
			new Vector4(38f, 38f, 18f, 38f)
		},
		{
			"L Pressed",
			new Vector4(38f, 38f, 18f, 38f)
		},
		{
			"Left",
			new Vector4(38f, 18f, 38f, 18f)
		},
		{
			"Left Pressed",
			new Vector4(38f, 18f, 38f, 18f)
		},
		{
			"Right",
			new Vector4(18f, 38f, 38f, 18f)
		},
		{
			"Right Pressed",
			new Vector4(18f, 38f, 38f, 18f)
		},
		{
			"Up",
			new Vector4(18f, 38f, 38f, 18f)
		},
		{
			"Up Pressed",
			new Vector4(18f, 38f, 38f, 18f)
		},
		{
			"Down",
			new Vector4(38f, 18f, 38f, 18f)
		},
		{
			"Down Pressed",
			new Vector4(38f, 18f, 38f, 18f)
		},
		{
			"R",
			new Vector4(38f, 38f, 18f, 38f)
		},
		{
			"R Pressed",
			new Vector4(38f, 38f, 18f, 38f)
		}
	};

	// Token: 0x04001BCC RID: 7116
	public const int hitMargin = -7;
}
