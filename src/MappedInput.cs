using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x020001A3 RID: 419
public class MappedInput
{
	// Token: 0x1700006C RID: 108
	// (get) Token: 0x0600175D RID: 5981 RVA: 0x000A4E8F File Offset: 0x000A328F
	// (set) Token: 0x0600175E RID: 5982 RVA: 0x000A4E96 File Offset: 0x000A3296
	public static MappableInputMode inputMode { get; private set; }

	// Token: 0x1700006D RID: 109
	// (get) Token: 0x0600175F RID: 5983 RVA: 0x000A4E9E File Offset: 0x000A329E
	// (set) Token: 0x06001760 RID: 5984 RVA: 0x000A4EA5 File Offset: 0x000A32A5
	internal static bool Enabled
	{
		get
		{
			return MappedInput._enabled;
		}
		set
		{
			MappedInput.ClearCurrentValues();
			MappedInput._enabled = value;
		}
	}

	// Token: 0x06001761 RID: 5985 RVA: 0x000A4EB4 File Offset: 0x000A32B4
	public static void Init()
	{
		MappedInput.lastValues = new List<float>();
		MappedInput.currentValues = new List<float>();
		MappedInput.currentInputs = new List<MappedInput.InputType>();
		MappedInput.currentInputTargets = new List<int>();
		for (int i = 0; i < 109; i++)
		{
			MappedInput.lastValues.Add(0f);
			MappedInput.currentValues.Add(0f);
		}
		MappedInput.editorInputs = new Dictionary<MappableInput, List<MappedInput.InputType>>();
		MappedInput.runtimeInputs = new Dictionary<MappableInput, List<MappedInput.InputType>>();
		MappedInput.menuInputs = new Dictionary<MappableInput, List<MappedInput.InputType>>();
		MappedInput.lastMousePos = Input.mousePosition;
		string[] joystickNames = Input.GetJoystickNames();
		for (int j = 0; j < joystickNames.Length; j++)
		{
			BWLog.Info("Found controller: " + joystickNames[j]);
		}
		MappedInput.currentGamepadInputMap = MappedInput.gamePadInputMapGenericBluetooth;
		MappedInput.currentGamepadInputButtonKeyCodeMap = MappedInput.gamePadInputButtonKeyCodeMapGenericBluetooth;
		MappedInput.AddDefaultInputs();
		MappedInput._enabled = true;
	}

	// Token: 0x06001762 RID: 5986 RVA: 0x000A4F90 File Offset: 0x000A3390
	private static void AddDefaultInputs()
	{
		MappedInput.AddKeyInput(MappableInputMode.Build, MappableInput.COPY, KeyCode.C, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Build, MappableInput.PASTE, KeyCode.V, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Build, MappableInput.CUT, KeyCode.X, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Build, MappableInput.CAM_FORWARD, KeyCode.W, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Build, MappableInput.CAM_BACK, KeyCode.S, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Build, MappableInput.CAM_LEFT, KeyCode.A, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Build, MappableInput.CAM_RIGHT, KeyCode.D, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Build, MappableInput.CAM_UP, KeyCode.E, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Build, MappableInput.CAM_DOWN, KeyCode.Q, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Build, MappableInput.PLAY, KeyCode.P, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Play, MappableInput.AXIS1_UP, KeyCode.W, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Play, MappableInput.AXIS1_DOWN, KeyCode.S, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Play, MappableInput.AXIS1_LEFT, KeyCode.A, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Play, MappableInput.AXIS1_RIGHT, KeyCode.D, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Play, MappableInput.PLAY, KeyCode.P, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Play, MappableInput.STOP, KeyCode.B, KeyCode.None);
		MappedInput.AddKeyInput(MappableInputMode.Play, MappableInput.SHOW_CONTROLS, KeyCode.F1, KeyCode.None);
	}

	// Token: 0x06001763 RID: 5987 RVA: 0x000A5058 File Offset: 0x000A3458
	private static void ClearCurrentValues()
	{
		for (int i = 0; i < 109; i++)
		{
			MappedInput.lastValues[i] = 0f;
			MappedInput.currentValues[i] = 0f;
		}
		MappedInput.lastMousePos = Input.mousePosition;
	}

	// Token: 0x06001764 RID: 5988 RVA: 0x000A50A4 File Offset: 0x000A34A4
	public static void SetMode(MappableInputMode mode)
	{
		MappedInput.inputMode = mode;
		MappedInput.lastMousePos = Input.mousePosition;
		MappedInput.currentInputs.Clear();
		MappedInput.currentInputTargets.Clear();
		Dictionary<MappableInput, List<MappedInput.InputType>> dictionary = MappedInput.InputsForMode(mode);
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<MappableInput, List<MappedInput.InputType>> keyValuePair in dictionary)
		{
			foreach (MappedInput.InputType item in keyValuePair.Value)
			{
				MappedInput.currentInputs.Add(item);
				MappedInput.currentInputTargets.Add((int)keyValuePair.Key);
			}
		}
	}

	// Token: 0x06001765 RID: 5989 RVA: 0x000A5188 File Offset: 0x000A3588
	private static Dictionary<MappableInput, List<MappedInput.InputType>> InputsForMode(MappableInputMode mode)
	{
		if (mode == MappableInputMode.Build)
		{
			return MappedInput.editorInputs;
		}
		if (mode == MappableInputMode.Play)
		{
			return MappedInput.runtimeInputs;
		}
		if (mode != MappableInputMode.Menu)
		{
			return null;
		}
		return MappedInput.menuInputs;
	}

	// Token: 0x06001766 RID: 5990 RVA: 0x000A51B8 File Offset: 0x000A35B8
	public static void AddKeyInput(MappableInputMode mode, MappableInput input, KeyCode key, KeyCode modifier = KeyCode.None)
	{
		Dictionary<MappableInput, List<MappedInput.InputType>> dictionary = MappedInput.InputsForMode(mode);
		if (dictionary == null)
		{
			BWLog.Error("No map for key input: " + key);
			return;
		}
		if (!dictionary.ContainsKey(input))
		{
			dictionary[input] = new List<MappedInput.InputType>();
		}
		bool setAllowAnyModifier = mode == MappableInputMode.Play;
		dictionary[input].Add(new MappedInput.KeyInput(key, modifier, setAllowAnyModifier));
	}

	// Token: 0x06001767 RID: 5991 RVA: 0x000A521C File Offset: 0x000A361C
	public static void AddControllerAxisInput(MappableInputMode mode, MappableInput input, string inputName)
	{
		Dictionary<MappableInput, List<MappedInput.InputType>> dictionary = MappedInput.InputsForMode(mode);
		if (dictionary == null)
		{
			BWLog.Error("No map for controller axis input: " + inputName);
			return;
		}
		if (!dictionary.ContainsKey(input))
		{
			dictionary[input] = new List<MappedInput.InputType>();
		}
		string empty = string.Empty;
		if (MappedInput.currentGamepadInputMap.TryGetValue(inputName, out empty))
		{
			dictionary[input].Add(new MappedInput.ControllerAxisInput(empty));
		}
	}

	// Token: 0x06001768 RID: 5992 RVA: 0x000A528C File Offset: 0x000A368C
	public static void AddControllerButtonInput(MappableInputMode mode, MappableInput input, string inputName)
	{
		Dictionary<MappableInput, List<MappedInput.InputType>> dictionary = MappedInput.InputsForMode(mode);
		if (dictionary == null)
		{
			BWLog.Error("No map for controller button input: " + inputName);
			return;
		}
		if (!dictionary.ContainsKey(input))
		{
			dictionary[input] = new List<MappedInput.InputType>();
		}
		if (MappedInput.currentGamepadInputButtonKeyCodeMap != null)
		{
			KeyCode key = KeyCode.None;
			if (MappedInput.currentGamepadInputButtonKeyCodeMap.TryGetValue(inputName, out key))
			{
				dictionary[input].Add(new MappedInput.KeyInput(key, KeyCode.None, true));
				return;
			}
		}
		if (MappedInput.currentGamepadInputMap != null)
		{
			string empty = string.Empty;
			if (MappedInput.currentGamepadInputMap.TryGetValue(inputName, out empty))
			{
				dictionary[input].Add(new MappedInput.ControllerButtonInput(empty));
				return;
			}
		}
	}

	// Token: 0x06001769 RID: 5993 RVA: 0x000A5338 File Offset: 0x000A3738
	public static bool AddInputMap(string jsonMap, MappableInputMode mode)
	{
		if (MappedInput.InputsForMode(mode) == null)
		{
			return false;
		}
		JObject jobject = JSONDecoder.Decode(jsonMap);
		Dictionary<string, JObject> objectValue = jobject.ObjectValue;
		foreach (JObject jobject2 in objectValue["mappedInput"].ArrayValue)
		{
			MappableInput input = (MappableInput)Enum.Parse(typeof(MappableInput), jobject2["input"].StringValue);
			if (jobject2.ContainsKey("key"))
			{
				KeyCode modifier = KeyCode.None;
				bool flag = false;
				if (jobject2.ContainsKey("modifier"))
				{
					if (jobject2["modifier"].StringValue == "AnyAlt")
					{
						modifier = KeyCode.LeftAlt;
						flag = true;
					}
					else if (jobject2["modifier"].StringValue == "AnyApple" || jobject2["modifier"].StringValue == "AnyCommand")
					{
						modifier = KeyCode.LeftCommand;
						flag = true;
					}
					else if (jobject2["modifier"].StringValue == "AnyControl")
					{
						modifier = KeyCode.LeftControl;
						flag = true;
					}
					else if (jobject2["modifier"].StringValue == "AnyShift")
					{
						modifier = KeyCode.LeftShift;
						flag = true;
					}
					else
					{
						modifier = (KeyCode)Enum.Parse(typeof(KeyCode), jobject2["modifier"].StringValue);
					}
				}
				MappedInput.AddKeyInput(mode, input, (KeyCode)Enum.Parse(typeof(KeyCode), jobject2["key"].StringValue), modifier);
				if (flag)
				{
					switch (modifier)
					{
					case KeyCode.LeftShift:
						MappedInput.AddKeyInput(mode, input, (KeyCode)Enum.Parse(typeof(KeyCode), jobject2["key"].StringValue), KeyCode.RightShift);
						break;
					case KeyCode.LeftControl:
						MappedInput.AddKeyInput(mode, input, (KeyCode)Enum.Parse(typeof(KeyCode), jobject2["key"].StringValue), KeyCode.RightControl);
						break;
					case KeyCode.LeftAlt:
						MappedInput.AddKeyInput(mode, input, (KeyCode)Enum.Parse(typeof(KeyCode), jobject2["key"].StringValue), KeyCode.RightAlt);
						break;
					case KeyCode.LeftCommand:
						MappedInput.AddKeyInput(mode, input, (KeyCode)Enum.Parse(typeof(KeyCode), jobject2["key"].StringValue), KeyCode.RightCommand);
						break;
					}
				}
			}
			if (jobject2.ContainsKey("controllerAxis"))
			{
				MappedInput.AddControllerAxisInput(mode, input, jobject2["controllerAxis"].StringValue);
			}
			if (jobject2.ContainsKey("controllerButton"))
			{
				MappedInput.AddControllerButtonInput(mode, input, jobject2["controllerButton"].StringValue);
			}
		}
		return true;
	}

	// Token: 0x0600176A RID: 5994 RVA: 0x000A5698 File Offset: 0x000A3A98
	public static void ClearInputMap(MappableInputMode mode)
	{
		MappedInput.ClearCurrentValues();
		MappedInput.currentInputs.Clear();
		MappedInput.currentInputTargets.Clear();
		Dictionary<MappableInput, List<MappedInput.InputType>> dictionary = MappedInput.InputsForMode(mode);
		if (dictionary == null)
		{
			return;
		}
		dictionary.Clear();
	}

	// Token: 0x0600176B RID: 5995 RVA: 0x000A56D4 File Offset: 0x000A3AD4
	private static void UpdateMappedInputs()
	{
		for (int i = 0; i < Mathf.Min(MappedInput.currentInputs.Count, MappedInput.currentInputTargets.Count); i++)
		{
			List<float> list;
			int index;
			(list = MappedInput.currentValues)[index = MappedInput.currentInputTargets[i]] = list[index] + MappedInput.currentInputs[i].GetValue();
		}
	}

	// Token: 0x0600176C RID: 5996 RVA: 0x000A573C File Offset: 0x000A3B3C
	public static void Update()
	{
		if (!MappedInput._enabled)
		{
			return;
		}
		for (int i = 0; i < 109; i++)
		{
			MappedInput.lastValues[i] = MappedInput.currentValues[i];
			MappedInput.currentValues[i] = 0f;
		}
		MappedInput.UpdateMappedInputs();
		List<float> list;
		(list = MappedInput.currentValues)[2] = list[2] + -Mathf.Min(0f, MappedInput.currentValues[0]);
		(list = MappedInput.currentValues)[3] = list[3] + Mathf.Max(0f, MappedInput.currentValues[0]);
		(list = MappedInput.currentValues)[4] = list[4] + -Mathf.Min(0f, MappedInput.currentValues[1]);
		(list = MappedInput.currentValues)[5] = list[5] + Mathf.Max(0f, MappedInput.currentValues[1]);
		(list = MappedInput.currentValues)[8] = list[8] + -Mathf.Min(0f, MappedInput.currentValues[6]);
		(list = MappedInput.currentValues)[9] = list[9] + Mathf.Max(0f, MappedInput.currentValues[6]);
		(list = MappedInput.currentValues)[10] = list[10] + -Mathf.Min(0f, MappedInput.currentValues[7]);
		(list = MappedInput.currentValues)[11] = list[11] + Mathf.Max(0f, MappedInput.currentValues[7]);
		MappedInput.lastMousePos = Input.mousePosition;
	}

	// Token: 0x0600176D RID: 5997 RVA: 0x000A58F6 File Offset: 0x000A3CF6
	public static float InputAxis(MappableInput input)
	{
		return Mathf.Clamp(MappedInput.currentValues[(int)input], -1f, 1f);
	}

	// Token: 0x0600176E RID: 5998 RVA: 0x000A5912 File Offset: 0x000A3D12
	public static bool InputPressed(MappableInput input)
	{
		return Mathf.Abs(MappedInput.currentValues[(int)input]) > 0.3f;
	}

	// Token: 0x0600176F RID: 5999 RVA: 0x000A592B File Offset: 0x000A3D2B
	public static bool InputDown(MappableInput input)
	{
		return Mathf.Abs(MappedInput.currentValues[(int)input]) > 0.3f && Mathf.Abs(MappedInput.lastValues[(int)input]) <= 0.3f;
	}

	// Token: 0x06001770 RID: 6000 RVA: 0x000A5964 File Offset: 0x000A3D64
	public static bool InputUp(MappableInput input)
	{
		return Mathf.Abs(MappedInput.currentValues[(int)input]) <= 0.3f && Mathf.Abs(MappedInput.lastValues[(int)input]) > 0.3f;
	}

	// Token: 0x06001771 RID: 6001 RVA: 0x000A599C File Offset: 0x000A3D9C
	public static bool AnyMetaDown(bool includeShift = true, bool includeControl = true, bool includeCommand = true, bool includeAlt = true, bool includeApple = true)
	{
		return (includeApple && (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand))) || (includeControl && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) || (includeAlt && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) || (includeShift && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) || (includeCommand && (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)));
	}

	// Token: 0x06001772 RID: 6002 RVA: 0x000A5A64 File Offset: 0x000A3E64
	private static bool IsMeta(KeyCode keyCode)
	{
		return keyCode == KeyCode.LeftCommand || keyCode == KeyCode.RightCommand || keyCode == KeyCode.LeftControl || keyCode == KeyCode.RightControl || keyCode == KeyCode.LeftAlt || keyCode == KeyCode.RightAlt || keyCode == KeyCode.LeftShift || keyCode == KeyCode.RightShift || keyCode == KeyCode.LeftCommand || keyCode == KeyCode.RightCommand;
	}

	// Token: 0x06001773 RID: 6003 RVA: 0x000A5AE0 File Offset: 0x000A3EE0
	public static string GetLabel(MappableInput input, bool firstInputOnly)
	{
		string text = string.Empty;
		Dictionary<MappableInput, List<MappedInput.InputType>> dictionary = MappedInput.InputsForMode(MappedInput.inputMode);
		if (dictionary == null)
		{
			return text;
		}
		List<MappedInput.InputType> list;
		if (dictionary.TryGetValue(input, out list))
		{
			bool flag = true;
			for (int num = 0; num != list.Count; num++)
			{
				string label = list[num].GetLabel();
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

	// Token: 0x0400125D RID: 4701
	private const float AXIS_PUSH_VALUE = 0.3f;

	// Token: 0x0400125E RID: 4702
	private static List<float> lastValues = null;

	// Token: 0x0400125F RID: 4703
	private static List<float> currentValues = null;

	// Token: 0x04001260 RID: 4704
	private static List<MappedInput.InputType> currentInputs;

	// Token: 0x04001261 RID: 4705
	private static List<int> currentInputTargets;

	// Token: 0x04001262 RID: 4706
	private static Dictionary<MappableInput, List<MappedInput.InputType>> editorInputs = null;

	// Token: 0x04001263 RID: 4707
	private static Dictionary<MappableInput, List<MappedInput.InputType>> runtimeInputs = null;

	// Token: 0x04001264 RID: 4708
	private static Dictionary<MappableInput, List<MappedInput.InputType>> menuInputs = null;

	// Token: 0x04001266 RID: 4710
	private static Vector3 lastMousePos;

	// Token: 0x04001267 RID: 4711
	private static Dictionary<string, string> currentGamepadInputMap;

	// Token: 0x04001268 RID: 4712
	private static Dictionary<string, KeyCode> currentGamepadInputButtonKeyCodeMap;

	// Token: 0x04001269 RID: 4713
	private static bool _enabled = false;

	// Token: 0x0400126A RID: 4714
	public static Dictionary<string, string> gamePadInputMapXboxOSX = new Dictionary<string, string>
	{
		{
			"LEFT STICK X",
			"Controller Axis 1"
		},
		{
			"LEFT STICK Y",
			"Controller Axis 2"
		},
		{
			"RIGHT STICK X",
			"Controller Axis 3"
		},
		{
			"RIGHT STICK Y",
			"Controller Axis 4"
		},
		{
			"LEFT TRIGGER",
			"Controller Axis 5"
		},
		{
			"RIGHT TRIGGER",
			"Controller Axis 6"
		},
		{
			"DPAD UP",
			"joystick button 5"
		},
		{
			"DPAD DOWN",
			"joystick button 6"
		},
		{
			"DPAD LEFT",
			"joystick button 7"
		},
		{
			"DPAD RIGHT",
			"joystick button 8"
		},
		{
			"START",
			"joystick button 9"
		},
		{
			"BACK",
			"joystick button 10"
		},
		{
			"LEFT STICK CLICK",
			"joystick button 11"
		},
		{
			"RIGHT STICK CLICK",
			"joystick button 12"
		},
		{
			"LEFT BUMPER",
			"joystick button 13"
		},
		{
			"RIGHT BUMPER",
			"joystick button 14"
		},
		{
			"HOME",
			"joystick button 15"
		},
		{
			"A",
			"joystick button 16"
		},
		{
			"B",
			"joystick button 17"
		},
		{
			"X",
			"joystick button 18"
		},
		{
			"Y",
			"joystick button 19"
		}
	};

	// Token: 0x0400126B RID: 4715
	public static Dictionary<string, string> gamePadInputMapXboxWindows = new Dictionary<string, string>
	{
		{
			"LEFT STICK X",
			"Controller Axis 1"
		},
		{
			"LEFT STICK Y",
			"Controller Axis 2"
		},
		{
			"RIGHT STICK X",
			"Controller Axis 4"
		},
		{
			"RIGHT STICK Y",
			"Controller Axis 5"
		},
		{
			"DPAD X",
			"Controller Axis 6"
		},
		{
			"DPAD Y",
			"Controller Axis 7"
		},
		{
			"LEFT TRIGGER",
			"Controller Axis 9"
		},
		{
			"RIGHT TRIGGER",
			"Controller Axis 10"
		}
	};

	// Token: 0x0400126C RID: 4716
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

	// Token: 0x0400126D RID: 4717
	public static Dictionary<string, string> gamePadInputMapGenericBluetooth = new Dictionary<string, string>
	{
		{
			"LEFT STICK X",
			"Controller Axis 1"
		},
		{
			"LEFT STICK Y",
			"Controller Axis 2"
		},
		{
			"RIGHT STICK X",
			"Controller Axis 3"
		},
		{
			"RIGHT STICK Y",
			"Controller Axis 4"
		}
	};

	// Token: 0x0400126E RID: 4718
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

	// Token: 0x020001A4 RID: 420
	private class InputType
	{
		// Token: 0x06001776 RID: 6006 RVA: 0x000A5F4D File Offset: 0x000A434D
		public virtual float GetValue()
		{
			return 0f;
		}

		// Token: 0x06001777 RID: 6007 RVA: 0x000A5F54 File Offset: 0x000A4354
		public virtual string GetLabel()
		{
			return string.Empty;
		}
	}

	// Token: 0x020001A5 RID: 421
	private class MouseAxisInput : MappedInput.InputType
	{
		// Token: 0x06001778 RID: 6008 RVA: 0x000A5F5B File Offset: 0x000A435B
		public MouseAxisInput(int axis)
		{
			this.axisType = axis;
		}

		// Token: 0x06001779 RID: 6009 RVA: 0x000A5F6C File Offset: 0x000A436C
		public override float GetValue()
		{
			int num = this.axisType;
			if (num == 0)
			{
				return (Input.mousePosition.x - MappedInput.lastMousePos.x) / (float)Screen.width;
			}
			if (num != 1)
			{
				return 0f;
			}
			return (Input.mousePosition.y - MappedInput.lastMousePos.y) / (float)Screen.height;
		}

		// Token: 0x0400126F RID: 4719
		private int axisType;
	}

	// Token: 0x020001A6 RID: 422
	private class MouseButtonInput : MappedInput.InputType
	{
		// Token: 0x0600177A RID: 6010 RVA: 0x000A5FD7 File Offset: 0x000A43D7
		public MouseButtonInput(int button)
		{
			this.buttonType = button;
		}

		// Token: 0x0600177B RID: 6011 RVA: 0x000A5FE6 File Offset: 0x000A43E6
		public override float GetValue()
		{
			return (!Input.GetMouseButton(this.buttonType)) ? 0f : 1f;
		}

		// Token: 0x04001270 RID: 4720
		private int buttonType;
	}

	// Token: 0x020001A7 RID: 423
	private class KeyInput : MappedInput.InputType
	{
		// Token: 0x0600177C RID: 6012 RVA: 0x000A6007 File Offset: 0x000A4407
		public KeyInput(KeyCode key, KeyCode modifier, bool setAllowAnyModifier)
		{
			this.keyCode = key;
			this.modifierCode = modifier;
			this.allowAnyModifier = setAllowAnyModifier;
		}

		// Token: 0x0600177D RID: 6013 RVA: 0x000A6024 File Offset: 0x000A4424
		public override float GetValue()
		{
			if (this.modifierCode != KeyCode.None)
			{
				if (!Input.GetKey(this.modifierCode))
				{
					return 0f;
				}
			}
			else if (!this.allowAnyModifier && !MappedInput.IsMeta(this.keyCode) && MappedInput.AnyMetaDown(true, true, true, true, true))
			{
				return 0f;
			}
			return (!Input.GetKey(this.keyCode)) ? 0f : 1f;
		}

		// Token: 0x0600177E RID: 6014 RVA: 0x000A60A6 File Offset: 0x000A44A6
		public override string GetLabel()
		{
			return this.keyCode.ToString();
		}

		// Token: 0x04001271 RID: 4721
		private KeyCode keyCode;

		// Token: 0x04001272 RID: 4722
		private KeyCode modifierCode;

		// Token: 0x04001273 RID: 4723
		private bool allowAnyModifier;
	}

	// Token: 0x020001A8 RID: 424
	private class ControllerAxisInput : MappedInput.InputType
	{
		// Token: 0x0600177F RID: 6015 RVA: 0x000A60B9 File Offset: 0x000A44B9
		public ControllerAxisInput(string inputAxisName)
		{
			this.inputManagerAxisName = inputAxisName;
		}

		// Token: 0x06001780 RID: 6016 RVA: 0x000A60C8 File Offset: 0x000A44C8
		public override float GetValue()
		{
			return Input.GetAxis(this.inputManagerAxisName);
		}

		// Token: 0x04001274 RID: 4724
		private string inputManagerAxisName;
	}

	// Token: 0x020001A9 RID: 425
	private class ControllerButtonInput : MappedInput.InputType
	{
		// Token: 0x06001781 RID: 6017 RVA: 0x000A60D5 File Offset: 0x000A44D5
		public ControllerButtonInput(string inputButtonName)
		{
			this.inputManagerButtonName = inputButtonName;
		}

		// Token: 0x06001782 RID: 6018 RVA: 0x000A60E4 File Offset: 0x000A44E4
		public override float GetValue()
		{
			return (!Input.GetButton(this.inputManagerButtonName)) ? 0f : 1f;
		}

		// Token: 0x04001275 RID: 4725
		private string inputManagerButtonName;
	}
}
