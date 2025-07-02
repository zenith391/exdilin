using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class MappedInput
{
	private class InputType
	{
		public virtual float GetValue()
		{
			return 0f;
		}

		public virtual string GetLabel()
		{
			return string.Empty;
		}
	}

	private class MouseAxisInput : InputType
	{
		private int axisType;

		public MouseAxisInput(int axis)
		{
			axisType = axis;
		}

		public override float GetValue()
		{
			return axisType switch
			{
				0 => (Input.mousePosition.x - lastMousePos.x) / (float)Screen.width, 
				1 => (Input.mousePosition.y - lastMousePos.y) / (float)Screen.height, 
				_ => 0f, 
			};
		}
	}

	private class MouseButtonInput : InputType
	{
		private int buttonType;

		public MouseButtonInput(int button)
		{
			buttonType = button;
		}

		public override float GetValue()
		{
			if (Input.GetMouseButton(buttonType))
			{
				return 1f;
			}
			return 0f;
		}
	}

	private class KeyInput : InputType
	{
		private KeyCode keyCode;

		private KeyCode modifierCode;

		private bool allowAnyModifier;

		public KeyInput(KeyCode key, KeyCode modifier, bool setAllowAnyModifier)
		{
			keyCode = key;
			modifierCode = modifier;
			allowAnyModifier = setAllowAnyModifier;
		}

		public override float GetValue()
		{
			if (modifierCode != KeyCode.None)
			{
				if (!Input.GetKey(modifierCode))
				{
					return 0f;
				}
			}
			else if (!allowAnyModifier && !IsMeta(keyCode) && AnyMetaDown())
			{
				return 0f;
			}
			if (Input.GetKey(keyCode))
			{
				return 1f;
			}
			return 0f;
		}

		public override string GetLabel()
		{
			return keyCode.ToString();
		}
	}

	private class ControllerAxisInput : InputType
	{
		private string inputManagerAxisName;

		public ControllerAxisInput(string inputAxisName)
		{
			inputManagerAxisName = inputAxisName;
		}

		public override float GetValue()
		{
			return Input.GetAxis(inputManagerAxisName);
		}
	}

	private class ControllerButtonInput : InputType
	{
		private string inputManagerButtonName;

		public ControllerButtonInput(string inputButtonName)
		{
			inputManagerButtonName = inputButtonName;
		}

		public override float GetValue()
		{
			if (Input.GetButton(inputManagerButtonName))
			{
				return 1f;
			}
			return 0f;
		}
	}

	private const float AXIS_PUSH_VALUE = 0.3f;

	private static List<float> lastValues = null;

	private static List<float> currentValues = null;

	private static List<InputType> currentInputs;

	private static List<int> currentInputTargets;

	private static Dictionary<MappableInput, List<InputType>> editorInputs = null;

	private static Dictionary<MappableInput, List<InputType>> runtimeInputs = null;

	private static Dictionary<MappableInput, List<InputType>> menuInputs = null;

	private static Vector3 lastMousePos;

	private static Dictionary<string, string> currentGamepadInputMap;

	private static Dictionary<string, KeyCode> currentGamepadInputButtonKeyCodeMap;

	private static bool _enabled = false;

	public static Dictionary<string, string> gamePadInputMapXboxOSX = new Dictionary<string, string>
	{
		{ "LEFT STICK X", "Controller Axis 1" },
		{ "LEFT STICK Y", "Controller Axis 2" },
		{ "RIGHT STICK X", "Controller Axis 3" },
		{ "RIGHT STICK Y", "Controller Axis 4" },
		{ "LEFT TRIGGER", "Controller Axis 5" },
		{ "RIGHT TRIGGER", "Controller Axis 6" },
		{ "DPAD UP", "joystick button 5" },
		{ "DPAD DOWN", "joystick button 6" },
		{ "DPAD LEFT", "joystick button 7" },
		{ "DPAD RIGHT", "joystick button 8" },
		{ "START", "joystick button 9" },
		{ "BACK", "joystick button 10" },
		{ "LEFT STICK CLICK", "joystick button 11" },
		{ "RIGHT STICK CLICK", "joystick button 12" },
		{ "LEFT BUMPER", "joystick button 13" },
		{ "RIGHT BUMPER", "joystick button 14" },
		{ "HOME", "joystick button 15" },
		{ "A", "joystick button 16" },
		{ "B", "joystick button 17" },
		{ "X", "joystick button 18" },
		{ "Y", "joystick button 19" }
	};

	public static Dictionary<string, string> gamePadInputMapXboxWindows = new Dictionary<string, string>
	{
		{ "LEFT STICK X", "Controller Axis 1" },
		{ "LEFT STICK Y", "Controller Axis 2" },
		{ "RIGHT STICK X", "Controller Axis 4" },
		{ "RIGHT STICK Y", "Controller Axis 5" },
		{ "DPAD X", "Controller Axis 6" },
		{ "DPAD Y", "Controller Axis 7" },
		{ "LEFT TRIGGER", "Controller Axis 9" },
		{ "RIGHT TRIGGER", "Controller Axis 10" }
	};

	public static Dictionary<string, KeyCode> gamePadInputButtonKeyCodeMapXboxWindows = new Dictionary<string, KeyCode>
	{
		{
			"A",
			KeyCode.JoystickButton0
		},
		{
			"B",
			KeyCode.JoystickButton1
		},
		{
			"X",
			KeyCode.JoystickButton2
		},
		{
			"Y",
			KeyCode.JoystickButton3
		},
		{
			"LEFT BUMPER",
			KeyCode.JoystickButton4
		},
		{
			"RIGHT BUMPER",
			KeyCode.JoystickButton5
		},
		{
			"BACK",
			KeyCode.JoystickButton6
		},
		{
			"START",
			KeyCode.JoystickButton7
		}
	};

	public static Dictionary<string, string> gamePadInputMapGenericBluetooth = new Dictionary<string, string>
	{
		{ "LEFT STICK X", "Controller Axis 1" },
		{ "LEFT STICK Y", "Controller Axis 2" },
		{ "RIGHT STICK X", "Controller Axis 3" },
		{ "RIGHT STICK Y", "Controller Axis 4" }
	};

	public static Dictionary<string, KeyCode> gamePadInputButtonKeyCodeMapGenericBluetooth = new Dictionary<string, KeyCode>
	{
		{
			"DPAD UP",
			KeyCode.JoystickButton4
		},
		{
			"DPAD RIGHT",
			KeyCode.JoystickButton5
		},
		{
			"DPAD DOWN",
			KeyCode.JoystickButton6
		},
		{
			"DPAD LEFT",
			KeyCode.JoystickButton7
		},
		{
			"START",
			KeyCode.JoystickButton0
		},
		{
			"LEFT BUMPER",
			KeyCode.JoystickButton8
		},
		{
			"RIGHT BUMPER",
			KeyCode.JoystickButton9
		},
		{
			"LEFT TRIGGER",
			KeyCode.JoystickButton10
		},
		{
			"RIGHT TRIGGER",
			KeyCode.JoystickButton11
		},
		{
			"Y",
			KeyCode.JoystickButton12
		},
		{
			"B",
			KeyCode.JoystickButton13
		},
		{
			"A",
			KeyCode.JoystickButton14
		},
		{
			"X",
			KeyCode.JoystickButton15
		}
	};

	public static MappableInputMode inputMode { get; private set; }

	internal static bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			ClearCurrentValues();
			_enabled = value;
		}
	}

	public static void Init()
	{
		lastValues = new List<float>();
		currentValues = new List<float>();
		currentInputs = new List<InputType>();
		currentInputTargets = new List<int>();
		for (int i = 0; i < 109; i++)
		{
			lastValues.Add(0f);
			currentValues.Add(0f);
		}
		editorInputs = new Dictionary<MappableInput, List<InputType>>();
		runtimeInputs = new Dictionary<MappableInput, List<InputType>>();
		menuInputs = new Dictionary<MappableInput, List<InputType>>();
		lastMousePos = Input.mousePosition;
		string[] joystickNames = Input.GetJoystickNames();
		for (int j = 0; j < joystickNames.Length; j++)
		{
			BWLog.Info("Found controller: " + joystickNames[j]);
		}
		currentGamepadInputMap = gamePadInputMapGenericBluetooth;
		currentGamepadInputButtonKeyCodeMap = gamePadInputButtonKeyCodeMapGenericBluetooth;
		AddDefaultInputs();
		_enabled = true;
	}

	private static void AddDefaultInputs()
	{
		AddKeyInput(MappableInputMode.Build, MappableInput.COPY, KeyCode.C);
		AddKeyInput(MappableInputMode.Build, MappableInput.PASTE, KeyCode.V);
		AddKeyInput(MappableInputMode.Build, MappableInput.CUT, KeyCode.X);
		AddKeyInput(MappableInputMode.Build, MappableInput.CAM_FORWARD, KeyCode.W);
		AddKeyInput(MappableInputMode.Build, MappableInput.CAM_BACK, KeyCode.S);
		AddKeyInput(MappableInputMode.Build, MappableInput.CAM_LEFT, KeyCode.A);
		AddKeyInput(MappableInputMode.Build, MappableInput.CAM_RIGHT, KeyCode.D);
		AddKeyInput(MappableInputMode.Build, MappableInput.CAM_UP, KeyCode.E);
		AddKeyInput(MappableInputMode.Build, MappableInput.CAM_DOWN, KeyCode.Q);
		AddKeyInput(MappableInputMode.Build, MappableInput.PLAY, KeyCode.P);
		AddKeyInput(MappableInputMode.Play, MappableInput.AXIS1_UP, KeyCode.W);
		AddKeyInput(MappableInputMode.Play, MappableInput.AXIS1_DOWN, KeyCode.S);
		AddKeyInput(MappableInputMode.Play, MappableInput.AXIS1_LEFT, KeyCode.A);
		AddKeyInput(MappableInputMode.Play, MappableInput.AXIS1_RIGHT, KeyCode.D);
		AddKeyInput(MappableInputMode.Play, MappableInput.PLAY, KeyCode.P);
		AddKeyInput(MappableInputMode.Play, MappableInput.STOP, KeyCode.B);
		AddKeyInput(MappableInputMode.Play, MappableInput.SHOW_CONTROLS, KeyCode.F1);
	}

	private static void ClearCurrentValues()
	{
		for (int i = 0; i < 109; i++)
		{
			lastValues[i] = 0f;
			currentValues[i] = 0f;
		}
		lastMousePos = Input.mousePosition;
	}

	public static void SetMode(MappableInputMode mode)
	{
		inputMode = mode;
		lastMousePos = Input.mousePosition;
		currentInputs.Clear();
		currentInputTargets.Clear();
		Dictionary<MappableInput, List<InputType>> dictionary = InputsForMode(mode);
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<MappableInput, List<InputType>> item in dictionary)
		{
			foreach (InputType item2 in item.Value)
			{
				currentInputs.Add(item2);
				currentInputTargets.Add((int)item.Key);
			}
		}
	}

	private static Dictionary<MappableInput, List<InputType>> InputsForMode(MappableInputMode mode)
	{
		return mode switch
		{
			MappableInputMode.Build => editorInputs, 
			MappableInputMode.Play => runtimeInputs, 
			MappableInputMode.Menu => menuInputs, 
			_ => null, 
		};
	}

	public static void AddKeyInput(MappableInputMode mode, MappableInput input, KeyCode key, KeyCode modifier = KeyCode.None)
	{
		Dictionary<MappableInput, List<InputType>> dictionary = InputsForMode(mode);
		if (dictionary == null)
		{
			BWLog.Error("No map for key input: " + key);
			return;
		}
		if (!dictionary.ContainsKey(input))
		{
			dictionary[input] = new List<InputType>();
		}
		bool setAllowAnyModifier = mode == MappableInputMode.Play;
		dictionary[input].Add(new KeyInput(key, modifier, setAllowAnyModifier));
	}

	public static void AddControllerAxisInput(MappableInputMode mode, MappableInput input, string inputName)
	{
		Dictionary<MappableInput, List<InputType>> dictionary = InputsForMode(mode);
		if (dictionary == null)
		{
			BWLog.Error("No map for controller axis input: " + inputName);
			return;
		}
		if (!dictionary.ContainsKey(input))
		{
			dictionary[input] = new List<InputType>();
		}
		string value = string.Empty;
		if (currentGamepadInputMap.TryGetValue(inputName, out value))
		{
			dictionary[input].Add(new ControllerAxisInput(value));
		}
	}

	public static void AddControllerButtonInput(MappableInputMode mode, MappableInput input, string inputName)
	{
		Dictionary<MappableInput, List<InputType>> dictionary = InputsForMode(mode);
		if (dictionary == null)
		{
			BWLog.Error("No map for controller button input: " + inputName);
			return;
		}
		if (!dictionary.ContainsKey(input))
		{
			dictionary[input] = new List<InputType>();
		}
		if (currentGamepadInputButtonKeyCodeMap != null)
		{
			KeyCode value = KeyCode.None;
			if (currentGamepadInputButtonKeyCodeMap.TryGetValue(inputName, out value))
			{
				dictionary[input].Add(new KeyInput(value, KeyCode.None, setAllowAnyModifier: true));
				return;
			}
		}
		if (currentGamepadInputMap != null)
		{
			string value2 = string.Empty;
			if (currentGamepadInputMap.TryGetValue(inputName, out value2))
			{
				dictionary[input].Add(new ControllerButtonInput(value2));
			}
		}
	}

	public static bool AddInputMap(string jsonMap, MappableInputMode mode)
	{
		if (InputsForMode(mode) == null)
		{
			return false;
		}
		JObject jObject = JSONDecoder.Decode(jsonMap);
		Dictionary<string, JObject> objectValue = jObject.ObjectValue;
		foreach (JObject item in objectValue["mappedInput"].ArrayValue)
		{
			MappableInput input = (MappableInput)Enum.Parse(typeof(MappableInput), item["input"].StringValue);
			if (item.ContainsKey("key"))
			{
				KeyCode keyCode = KeyCode.None;
				bool flag = false;
				if (item.ContainsKey("modifier"))
				{
					if (item["modifier"].StringValue == "AnyAlt")
					{
						keyCode = KeyCode.LeftAlt;
						flag = true;
					}
					else if (item["modifier"].StringValue == "AnyApple" || item["modifier"].StringValue == "AnyCommand")
					{
						keyCode = KeyCode.LeftCommand;
						flag = true;
					}
					else if (item["modifier"].StringValue == "AnyControl")
					{
						keyCode = KeyCode.LeftControl;
						flag = true;
					}
					else if (item["modifier"].StringValue == "AnyShift")
					{
						keyCode = KeyCode.LeftShift;
						flag = true;
					}
					else
					{
						keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), item["modifier"].StringValue);
					}
				}
				AddKeyInput(mode, input, (KeyCode)Enum.Parse(typeof(KeyCode), item["key"].StringValue), keyCode);
				if (flag)
				{
					switch (keyCode)
					{
					case KeyCode.LeftShift:
						AddKeyInput(mode, input, (KeyCode)Enum.Parse(typeof(KeyCode), item["key"].StringValue), KeyCode.RightShift);
						break;
					case KeyCode.LeftControl:
						AddKeyInput(mode, input, (KeyCode)Enum.Parse(typeof(KeyCode), item["key"].StringValue), KeyCode.RightControl);
						break;
					case KeyCode.LeftAlt:
						AddKeyInput(mode, input, (KeyCode)Enum.Parse(typeof(KeyCode), item["key"].StringValue), KeyCode.RightAlt);
						break;
					case KeyCode.LeftCommand:
						AddKeyInput(mode, input, (KeyCode)Enum.Parse(typeof(KeyCode), item["key"].StringValue), KeyCode.RightCommand);
						break;
					}
				}
			}
			if (item.ContainsKey("controllerAxis"))
			{
				AddControllerAxisInput(mode, input, item["controllerAxis"].StringValue);
			}
			if (item.ContainsKey("controllerButton"))
			{
				AddControllerButtonInput(mode, input, item["controllerButton"].StringValue);
			}
		}
		return true;
	}

	public static void ClearInputMap(MappableInputMode mode)
	{
		ClearCurrentValues();
		currentInputs.Clear();
		currentInputTargets.Clear();
		InputsForMode(mode)?.Clear();
	}

	private static void UpdateMappedInputs()
	{
		for (int i = 0; i < Mathf.Min(currentInputs.Count, currentInputTargets.Count); i++)
		{
			currentValues[currentInputTargets[i]] += currentInputs[i].GetValue();
		}
	}

	public static void Update()
	{
		if (_enabled)
		{
			for (int i = 0; i < 109; i++)
			{
				lastValues[i] = currentValues[i];
				currentValues[i] = 0f;
			}
			UpdateMappedInputs();
			currentValues[2] += 0f - Mathf.Min(0f, currentValues[0]);
			currentValues[3] += Mathf.Max(0f, currentValues[0]);
			currentValues[4] += 0f - Mathf.Min(0f, currentValues[1]);
			currentValues[5] += Mathf.Max(0f, currentValues[1]);
			currentValues[8] += 0f - Mathf.Min(0f, currentValues[6]);
			currentValues[9] += Mathf.Max(0f, currentValues[6]);
			currentValues[10] += 0f - Mathf.Min(0f, currentValues[7]);
			currentValues[11] += Mathf.Max(0f, currentValues[7]);
			lastMousePos = Input.mousePosition;
		}
	}

	public static float InputAxis(MappableInput input)
	{
		return Mathf.Clamp(currentValues[(int)input], -1f, 1f);
	}

	public static bool InputPressed(MappableInput input)
	{
		return Mathf.Abs(currentValues[(int)input]) > 0.3f;
	}

	public static bool InputDown(MappableInput input)
	{
		if (Mathf.Abs(currentValues[(int)input]) > 0.3f)
		{
			return Mathf.Abs(lastValues[(int)input]) <= 0.3f;
		}
		return false;
	}

	public static bool InputUp(MappableInput input)
	{
		if (Mathf.Abs(currentValues[(int)input]) <= 0.3f)
		{
			return Mathf.Abs(lastValues[(int)input]) > 0.3f;
		}
		return false;
	}

	public static bool AnyMetaDown(bool includeShift = true, bool includeControl = true, bool includeCommand = true, bool includeAlt = true, bool includeApple = true)
	{
		if ((!includeApple || (!Input.GetKey(KeyCode.LeftCommand) && !Input.GetKey(KeyCode.RightCommand))) && (!includeControl || (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))) && (!includeAlt || (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))) && (!includeShift || (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))))
		{
			if (includeCommand)
			{
				if (!Input.GetKey(KeyCode.LeftCommand))
				{
					return Input.GetKey(KeyCode.RightCommand);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	private static bool IsMeta(KeyCode keyCode)
	{
		if (keyCode != KeyCode.LeftCommand && keyCode != KeyCode.RightCommand && keyCode != KeyCode.LeftControl && keyCode != KeyCode.RightControl && keyCode != KeyCode.LeftAlt && keyCode != KeyCode.RightAlt && keyCode != KeyCode.LeftShift && keyCode != KeyCode.RightShift && keyCode != KeyCode.LeftCommand)
		{
			return keyCode == KeyCode.RightCommand;
		}
		return true;
	}

	public static string GetLabel(MappableInput input, bool firstInputOnly)
	{
		string text = string.Empty;
		Dictionary<MappableInput, List<InputType>> dictionary = InputsForMode(inputMode);
		if (dictionary == null)
		{
			return text;
		}
		if (dictionary.TryGetValue(input, out var value))
		{
			bool flag = true;
			for (int i = 0; i != value.Count; i++)
			{
				string label = value[i].GetLabel();
				if (!string.IsNullOrEmpty(label))
				{
					if (!flag)
					{
						text += ",";
					}
					text += label;
					flag = false;
					if (firstInputOnly)
					{
						break;
					}
				}
			}
		}
		return text;
	}
}
