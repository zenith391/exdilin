using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

public class Tile
{
	public GAF gaf;

	public TileObject tileObject;

	private Transform tileObjectAssignedParent;

	public float time;

	public bool doubleWidth;

	public int subParameterIndex;

	public int subParameterCount = 1;

	public static string[] tagNames;

	public static string[] shortTagNames;

	public static string iconBasePath;

	public Panel parentPanel;

	public Vector3 positionInPanel;

	public bool visibleInPanel;

	public int panelSection;

	private bool isEnabled = true;

	private Vector3 cachedPosition;

	private Vector3 cachedLocalPosition;

	private Color cachedBackgroundColor = Color.clear;

	private Color cachedForegroundColor = Color.white;

	private string _uniqueID;

	public static Dictionary<string, Vector4> buttonExtensions;

	public const int hitMargin = -7;

	public string UniqueID
	{
		get
		{
			if (_uniqueID == null)
			{
				_uniqueID = gaf.ToString();
			}
			return _uniqueID;
		}
	}

	public Tile(GAF gaf)
	{
		if (gaf == null)
		{
			throw new ArgumentException("GAF can not be null", "gaf");
		}
		this.gaf = gaf;
		if (gaf.Predicate != null && gaf.Predicate.EditableParameter != null)
		{
			subParameterCount = gaf.Predicate.EditableParameter.subParameterCount;
		}
	}

	public Tile(Predicate pred, params object[] args)
	{
		gaf = new GAF(pred, args);
		if (gaf.Predicate.EditableParameter != null)
		{
			subParameterCount = gaf.Predicate.EditableParameter.subParameterCount;
		}
	}

	public Tile(TileObject tileObject)
	{
		gaf = new GAF(Block.predicateUI, null);
		this.tileObject = tileObject;
	}

	public void AssignToPanel(Panel panel)
	{
		parentPanel = panel;
	}

	public virtual Tile Clone()
	{
		return new Tile(gaf.Clone())
		{
			cachedBackgroundColor = cachedBackgroundColor,
			cachedForegroundColor = cachedForegroundColor
		};
	}

	public string GetLabelText()
	{
		return TileIconManager.Instance.GetLabelStr(gaf);
	}

	public void ToJSON(JSONStreamEncoder encoder, bool compact = false)
	{
		if (compact)
		{
			gaf.ToJSONCompact(encoder);
			return;
		}
		encoder.BeginObject();
		encoder.WriteKey("type");
		encoder.WriteString("tile");
		encoder.WriteKey("gaf");
		gaf.ToJSON(encoder);
		encoder.EndObject();
	}

	public static Tile FromJSON(JObject obj)
	{
		if (obj.ObjectValue == null)
		{
			GAF gAF = GAF.FromJSON(obj);
			if (gAF == null)
			{
				return null;
			}
			if (gAF.IsSemanticallyValid())
			{
				if (gAF == null)
				{
					return null;
				}
				return new Tile(gAF);
			}
			if (!BW.isUnityEditor || !Options.CreateErrorGafs)
			{
				return null;
			}
			if (!gAF.FixSemanticallyInvalidness())
			{
				BWLog.Error("Could not fix semantic issue");
				return null;
			}
			return new Tile(gAF);
		}
		if (obj.ObjectValue.ContainsKey("sym"))
		{
			return new Tile(SymbolCompat.ToGaf(obj));
		}
		GAF gAF2 = GAF.FromJSON(obj["gaf"]);
		if (gAF2 == null)
		{
			return null;
		}
		if (gAF2.IsSemanticallyValid())
		{
			if (gAF2 == null)
			{
				return null;
			}
			return new Tile(gAF2);
		}
		if (!BW.isUnityEditor || !Options.CreateErrorGafs)
		{
			return null;
		}
		if (!gAF2.FixSemanticallyInvalidness())
		{
			BWLog.Error("Could not fix semantic issue");
			return null;
		}
		return new Tile(gAF2);
	}

	public void Destroy()
	{
		DestroyPhysical();
	}

	public override string ToString()
	{
		return gaf.ToString();
	}

	public bool IsThen()
	{
		return gaf.Predicate == Block.predicateThen;
	}

	public bool IsCreate()
	{
		return gaf.Predicate == Block.predicateCreate;
	}

	public bool IsCreateModel()
	{
		return gaf.Predicate == Block.predicateCreateModel;
	}

	public bool IsPaint()
	{
		return gaf.Predicate == Block.predicatePaintTo;
	}

	public bool IsScriptGear()
	{
		if (tileObject != null)
		{
			return tileObject.IconName() == UIQuickSelect.scriptButtonIconName;
		}
		return false;
	}

	public bool IsCopiedModel()
	{
		if (tileObject != null)
		{
			return tileObject.IconName() == UIQuickSelect.modelButtonIconName;
		}
		return false;
	}

	public bool IsUIOnly()
	{
		return gaf.Predicate == Block.predicateUI;
	}

	public bool IsTexture()
	{
		return gaf.Predicate == Block.predicateTextureTo;
	}

	public bool IsSfx()
	{
		return gaf.Predicate == Block.predicatePlaySoundDurational;
	}

	public bool IsSkyBox()
	{
		return gaf.Predicate == BlockSky.predicateSkyBoxTo;
	}

	public bool IsLocked()
	{
		return gaf.Predicate == PredicateRegistry.ByName("Block.Locked");
	}

	public void MoveTo(float x, float y)
	{
		MoveTo(new Vector3(x, y), useExistingZ: true);
	}

	public void MoveTo(float x, float y, float z)
	{
		MoveTo(new Vector3(x, y, z));
	}

	public void MoveTo(Vector3 pos, bool useExistingZ = false)
	{
		if (parentPanel != null)
		{
			Vector3 position = parentPanel.GetTransform().position;
			LocalMoveTo(pos - position, useExistingZ);
			if (tileObject != null)
			{
				tileObject.SetPosition(pos);
			}
			else
			{
				cachedPosition = pos;
			}
		}
		else if (tileObjectAssignedParent != null)
		{
			Vector3 position2 = tileObjectAssignedParent.position;
			LocalMoveTo(pos - position2, useExistingZ);
			if (tileObject != null)
			{
				tileObject.SetPosition(pos);
			}
			else
			{
				cachedPosition = pos;
			}
		}
		else if (IsShowing())
		{
			if (useExistingZ)
			{
				pos.z = tileObject.GetPosition().z;
			}
			pos = new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), pos.z);
			tileObject.SetPosition(pos);
		}
		else
		{
			if (useExistingZ)
			{
				pos.z = cachedPosition.z;
			}
			cachedPosition = new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), pos.z);
		}
	}

	public void SmoothMoveTo(Vector3 pos)
	{
		if (parentPanel != null)
		{
			Vector3 position = parentPanel.GetTransform().position;
			LocalMoveTo(pos - position);
			return;
		}
		if (tileObject == null)
		{
			Show(show: true);
		}
		tileObject.SetPosition(pos);
	}

	public void LocalMoveTo(float x, float y)
	{
		LocalMoveTo(new Vector3(x, y), useExistingZ: true);
	}

	public void LocalMoveTo(float x, float y, float z)
	{
		LocalMoveTo(new Vector3(x, y, z));
	}

	public void LocalMoveTo(Vector3 pos, bool useExistingZ = false)
	{
		if (parentPanel != null)
		{
			positionInPanel = new Vector3(pos.x, pos.y, (!useExistingZ) ? pos.z : positionInPanel.z);
		}
		else if (tileObject != null)
		{
			if (useExistingZ)
			{
				pos.z = tileObject.GetLocalPosition().z;
			}
			tileObject.SetLocalPosition(new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), pos.z));
		}
		cachedLocalPosition = pos;
	}

	public Vector3 GetPosition()
	{
		if (tileObject != null)
		{
			return tileObject.GetPosition();
		}
		if (parentPanel != null)
		{
			Vector3 position = parentPanel.GetTransform().position;
			return position + positionInPanel;
		}
		return cachedPosition;
	}

	public Vector3 GetLocalPosition()
	{
		return cachedLocalPosition;
	}

	public Vector3 GetCenterPosition()
	{
		if (tileObject != null)
		{
			return tileObject.GetCenterPosition();
		}
		return cachedPosition + new Vector3(33f, 33f, 0f) * NormalizedScreen.scale;
	}

	public Vector3 GetScale()
	{
		if (tileObject != null)
		{
			return tileObject.GetScale();
		}
		return 80f * Vector3.one;
	}

	public bool HitControlButton(Vector3 v)
	{
		return HitControlButton((string)gaf.Args[0], v);
	}

	public bool HitControlButton(string symbol, Vector3 v)
	{
		if (buttonExtensions.TryGetValue(symbol, out var value))
		{
			return HitExtended(v, value.x, value.y, value.z, value.w);
		}
		return false;
	}

	public bool Hit(Vector3 v, bool allowDisabledTiles = false)
	{
		if (!(tileObject == null))
		{
			return tileObject.Hit(v, allowDisabledTiles);
		}
		return false;
	}

	public Bounds GetHitBounds()
	{
		Vector3 vector = Vector3.zero;
		if (parentPanel != null)
		{
			vector = parentPanel.GetTransform().position + positionInPanel;
		}
		else if (tileObject != null)
		{
			vector = tileObject.GetPosition();
		}
		float num = -7f * NormalizedScreen.pixelScale;
		Vector3 scale = GetScale();
		Vector3 vector2 = new Vector3(vector.x - num, vector.y - num, -99999f);
		Vector3 vector3 = new Vector3(vector.x + (float)((!doubleWidth) ? 1 : 2) * (scale.x + num), vector.y + scale.y + num, 99999f);
		return new Bounds(0.5f * (vector3 + vector2), vector3 - vector2);
	}

	public bool HitExtended(Vector3 v, float extendXMin, float extendXMax, float extendYMin, float extendYMax, bool allowDisabledTiles = false)
	{
		if (!(tileObject == null))
		{
			return tileObject.HitExtended(v, extendXMin, extendXMax, extendYMin, extendYMax, allowDisabledTiles);
		}
		return false;
	}

	public TileResultCode Condition(Block obj, ScriptRowExecutionInfo eInfo)
	{
		return gaf.RunSensor(obj, eInfo);
	}

	public TileResultCode Execute(Block obj, ScriptRowExecutionInfo eInfo)
	{
		return gaf.RunAction(obj, eInfo);
	}

	public static void CreateAntigravityArgumentConverters()
	{
		Vector3[] array = new Vector3[6]
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
			new List<string> { "AntiGravityColumn.IncreaseLocalVelocityChunk", "AntiGravity.IncreaseLocalVelocityChunk", "FlightYoke.IncreaseLocalVelocityChunk" },
			new List<string> { "AntiGravityColumn.IncreaseLocalTorqueChunk", "AntiGravity.IncreaseLocalTorqueChunk", "FlightYoke.IncreaseLocalTorqueChunk" }
		};
		Dictionary<string, List<GAF>> dictionary = new Dictionary<string, List<GAF>>();
		foreach (List<string> item2 in list)
		{
			foreach (string item3 in item2)
			{
				Vector3[] array2 = array;
				foreach (Vector3 vector in array2)
				{
					GAF item = new GAF(item3, vector);
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
		foreach (KeyValuePair<string, List<GAF>> item4 in dictionary)
		{
			foreach (GAF item5 in item4.Value)
			{
				foreach (GAF item6 in item4.Value)
				{
					if (item5 != item6)
					{
						Vector3 vector2 = (Vector3)item5.Args[0];
						Vector3 vector3 = (Vector3)item6.Args[0];
						if (vector2 != vector3)
						{
							AddVectorArgConverter(item5.Predicate, item6.Predicate, vector2, vector3);
						}
					}
				}
			}
		}
	}

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

	public static void UpdateTileParameterSettings()
	{
		TileParameterSettings component = Blocksworld.blocksworldDataContainer.GetComponent<TileParameterSettings>();
		TileParameterEditor tileParameterEditor = ((!(Blocksworld.bw != null)) ? null : Blocksworld.bw.tileParameterEditor);
		if (tileParameterEditor != null && tileParameterEditor.enabled)
		{
			Blocksworld.bw.parameterEditGesture.Cancel();
			tileParameterEditor.StopEditing();
		}
		TileParameterSetting[] settings = component.settings;
		foreach (TileParameterSetting tileParameterSetting in settings)
		{
			if (!tileParameterSetting.activated)
			{
				continue;
			}
			string[] matchingPredicateNames = tileParameterSetting.matchingPredicateNames;
			foreach (string name in matchingPredicateNames)
			{
				Predicate predicate = PredicateRegistry.ByName(name);
				if (predicate != null)
				{
					bool flag = false;
					switch (tileParameterSetting.type)
					{
					case TileParameterType.IntSlider:
						predicate.EditableParameter = new IntTileParameter(tileParameterSetting.intMinValue, tileParameterSetting.intMaxValue, tileParameterSetting.intStep, tileParameterSetting.sliderSensitivity, tileParameterSetting.parameterIndex, null, onlyShowPositive: false, tileParameterSetting.prefixValueString, tileParameterSetting.postfixValueString);
						break;
					case TileParameterType.IntPresentingFloatSlider:
						predicate.EditableParameter = new IntTileParameter(tileParameterSetting.intMinValue, tileParameterSetting.intMaxValue, tileParameterSetting.intStep, tileParameterSetting.sliderSensitivity, tileParameterSetting.parameterIndex, GetBidiIntFloatConverter(tileParameterSetting), tileParameterSetting.intOnlyShowPositive, tileParameterSetting.prefixValueString, tileParameterSetting.postfixValueString);
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
						predicate.EditableParameter = new StringTileParameter(tileParameterSetting.parameterIndex, multiline: false, tileParameterSetting.stringAcceptAny, tileParameterSetting.stringAcceptAnyHint);
						break;
					case TileParameterType.StringMultiLine:
						predicate.EditableParameter = new StringTileParameter(tileParameterSetting.parameterIndex, multiline: true, tileParameterSetting.stringAcceptAny, tileParameterSetting.stringAcceptAnyHint);
						break;
					case TileParameterType.ColorSlider:
						predicate.EditableParameter = new ColorTileParameter(tileParameterSetting.intMinValue, tileParameterSetting.intMaxValue, tileParameterSetting.intStep, tileParameterSetting.sliderSensitivity, tileParameterSetting.parameterIndex);
						break;
					case TileParameterType.ScoreSlider:
						predicate.EditableParameter = new ScoreTileParameter(tileParameterSetting.parameterIndex);
						break;
					case TileParameterType.WorldId:
						predicate.EditableParameter = new UserWorldTileParameter(tileParameterSetting.parameterIndex);
						break;
					case TileParameterType.EnumerationSlider:
						predicate.EditableParameter = new IntTileParameter(tileParameterSetting.intMinValue, tileParameterSetting.intMaxValue, tileParameterSetting.intStep, tileParameterSetting.sliderSensitivity, tileParameterSetting.parameterIndex, GetBidiIntStringConverter(tileParameterSetting));
						break;
					}
					if (flag && (tileParameterSetting.setGafArgumentIfNotExists || tileParameterSetting.overwriteGafArgumentInBuildPanel))
					{
						predicate.argumentExtender = CreateArgumentExtender(tileParameterSetting, predicate);
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
					object obj = ((setting.parameterIndex >= args.Length) ? null : args[setting.parameterIndex]);
					switch (setting.type)
					{
					case TileParameterType.IntPresentingFloatSlider:
						if (pred.EditableParameter is IntTileParameter { converter: not null } intTileParameter)
						{
							int num = ((!intTileParameter.onlyShowPositive || obj == null) ? 1 : ((int)Mathf.Sign((float)obj)));
							array[setting.parameterIndex] = (float)num * intTileParameter.converter.intToFloat(setting.intDefaultValue);
						}
						else
						{
							BWLog.Info("Could not find converter for int-presenting slider");
						}
						break;
					case TileParameterType.FloatSlider:
						if (pred.EditableParameter is FloatTileParameter floatTileParameter)
						{
							float num2 = ((!floatTileParameter.onlyShowPositive || obj == null) ? 1f : Mathf.Sign((float)obj));
							array[setting.parameterIndex] = num2 * setting.floatDefaultValue;
						}
						break;
					case TileParameterType.TimeSlider:
						if (pred.EditableParameter is TimeTileParameter)
						{
							array[setting.parameterIndex] = setting.floatDefaultValue;
						}
						break;
					default:
						BWLog.Info("Argument extension not supported for type " + setting.type);
						break;
					}
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] == null)
					{
						BWLog.Warning("Argument extension was trying to set an argument to null for predicate " + pred.Name + " and index " + j);
						return args;
					}
				}
				return array;
			}
			return args;
		};
	}

	private static BidiIntStringConverter GetBidiIntStringConverter(TileParameterSetting setting)
	{
		return new BidiIntStringConverter(setting.tableConverterStringValues, setting.intMinValue, setting.intStep);
	}

	private static BidiIntFloatConverter GetBidiIntFloatConverter(TileParameterSetting setting)
	{
		return setting.bidiIntFloatConverterType switch
		{
			BidiIntFloatConverterType.Affine => new AffineBidiIntFloatConverter
			{
				bias = setting.affineConverterBias,
				multiplier = setting.affineConverterMultiplier
			}, 
			BidiIntFloatConverterType.Range => AffineBidiIntFloatConverter.FromRange(setting.rangeConverterFrom, setting.rangeConverterTo, setting.intMinValue, setting.intMaxValue), 
			BidiIntFloatConverterType.Table => new TableBidiIntFloatConverter(setting.tableConverterFloatValues, setting.intMinValue, setting.intMaxValue, setting.intOnlyShowPositive), 
			BidiIntFloatConverterType.PiecewiseLinear => new PiecewiseLinearIntFloatConverter(setting.piecewiseLinearConverterIntValues, setting.piecewiseLinearConverterFloatValues, setting.intOnlyShowPositive), 
			_ => null, 
		};
	}

	public void Enable(bool enabled)
	{
		isEnabled = enabled;
		if (tileObject != null)
		{
			if (enabled)
			{
				tileObject.Enable();
			}
			else
			{
				tileObject.Disable();
			}
		}
	}

	public bool IsEnabled()
	{
		return isEnabled;
	}

	public void Show(bool show)
	{
		if (gaf.Predicate == Block.predicateUI)
		{
			tileObject.Show(show);
		}
		else if (parentPanel != null)
		{
			visibleInPanel = show;
		}
		else if (show)
		{
			if (tileObject == null)
			{
				CreatePhysical();
				tileObject.SetPosition(cachedPosition);
				tileObject.SetLocalPosition(cachedLocalPosition);
				if (isEnabled)
				{
					tileObject.Enable();
				}
				else
				{
					tileObject.Disable();
				}
			}
		}
		else
		{
			if (tileObject != null)
			{
				cachedPosition = tileObject.GetPosition();
			}
			DestroyPhysical();
		}
	}

	public void CreatePhysical()
	{
		if (tileObject == null)
		{
			GAF gAF = ((!gaf.HasDynamicLabel()) ? Scarcity.GetNormalizedGaf(gaf) : gaf);
			UpdateDynamicLabelIfNecessary();
			if (IsCreateModel())
			{
				tileObject = Blocksworld.modelTilePool.GetTileObject(gAF, isEnabled);
			}
			else if (parentPanel != null)
			{
				tileObject = parentPanel.tileObjectPool.GetTileObject(gAF, isEnabled);
			}
			else
			{
				tileObject = Blocksworld.tilePool.GetTileObject(gAF, isEnabled);
				if (tileObjectAssignedParent != null)
				{
					tileObject.SetParent(tileObjectAssignedParent);
				}
			}
			if (cachedBackgroundColor != Color.clear)
			{
				tileObject.OverrideBackgroundColor(cachedBackgroundColor);
			}
			tileObject.OverrideForegroundColor(cachedForegroundColor);
		}
		tileObject.Show();
	}

	public void DestroyPhysical()
	{
		if (tileObject != null)
		{
			if (tileObject.obtainedFromPool != null)
			{
				tileObject.ReturnToPool();
			}
			else
			{
				UnityEngine.Object.Destroy(tileObject.GetGameObject());
			}
			tileObject = null;
		}
	}

	public bool UpdateDynamicLabelIfNecessary()
	{
		if (gaf.IsCreateModel() || !gaf.HasDynamicLabel())
		{
			return false;
		}
		string dynamicLabel = gaf.GetDynamicLabel();
		if (string.IsNullOrEmpty(dynamicLabel))
		{
			return false;
		}
		TileIconManager.Instance.labelAtlas.AddNewLabel(dynamicLabel);
		if (tileObject != null)
		{
			tileObject.Setup(gaf, isEnabled);
		}
		return true;
	}

	public bool IsShowing()
	{
		return tileObject != null;
	}

	public void SetTileBackgroundColor(Color bgColor)
	{
		if (tileObject != null)
		{
			tileObject.OverrideBackgroundColor(bgColor);
		}
		cachedBackgroundColor = bgColor;
	}

	public void SetTileForegroundColor(Color fgColor)
	{
		if (tileObject != null)
		{
			tileObject.OverrideForegroundColor(fgColor);
		}
		cachedForegroundColor = fgColor;
	}

	public void SetTileScale(Vector3 scale)
	{
		if (tileObject != null)
		{
			tileObject.SetScale(scale);
		}
	}

	public void SetTileScale(float scale)
	{
		if (tileObject != null)
		{
			tileObject.SetScale(scale * Vector3.one);
		}
	}

	public void SetParent(Transform p)
	{
		if (tileObject != null)
		{
			tileObject.SetParent(p);
		}
		tileObjectAssignedParent = p;
	}

	private static Color[] GradientToColors(Color[] colors)
	{
		return GradientToColors(colors[0], colors[1]);
	}

	private static Color[] GradientToColors(Color top, Color bottom)
	{
		return new Color[4] { top, bottom, bottom, top };
	}

	public bool UpdatesIconOnArgumentChange()
	{
		return gaf.UpdatesIconOnArgumentChange();
	}

	public void StepSubParameterIndex()
	{
		subParameterIndex = (subParameterIndex + 1) % subParameterCount;
		if (subParameterCount > 1)
		{
			Sound.PlayOneShotSound("Slider Handle Grabbed");
		}
	}

	static Tile()
	{
		tagNames = new string[12]
		{
			"Circle", "Triangle", "Square", "Diamond", "Heart", "Star", "Hexagon", "X", "Target", "Hero",
			"Player", "Villain"
		};
		shortTagNames = new string[12]
		{
			"0", "1", "2", "3", "4", "5", "6", "7", "8", "C",
			"A", "B"
		};
		iconBasePath = Application.dataPath + "/..";
		buttonExtensions = new Dictionary<string, Vector4>
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
	}
}
