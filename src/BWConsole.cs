using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BWConsole : MonoBehaviour
{
	private struct Log
	{
		public string message;

		public string stackTrace;

		public LogType type;
	}

	public static class TextureScaler
	{
		public static Texture2D scaled(Texture2D src, int width, int height)
		{
			Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, mipmap: false);
			Color[] pixels = texture2D.GetPixels(0);
			float num = 1f / (float)width;
			float num2 = 1f / (float)height;
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = src.GetPixelBilinear(num * ((float)i % (float)width), num2 * Mathf.Floor(i / width));
			}
			texture2D.SetPixels(pixels, 0);
			texture2D.Apply();
			return texture2D;
		}
	}

	public KeyCode toggleKey = KeyCode.BackQuote;

	public int messageCapacity = 2500;

	public int displayCapacity = 500;

	public bool ignoreWarnings;

	private List<Log> _logs = new List<Log>();

	private Vector2 _scrollPosition;

	private bool _showConsole;

	private bool _hideDupes;

	private static readonly Dictionary<LogType, Color> logTypeColors;

	private const int margin = 20;

	private Rect _windowRect = new Rect(20f, (float)Screen.height * 0.25f + 20f, (float)Screen.width * 0.85f, (float)Screen.height * 0.75f - 40f);

	private Rect _titleBarRect = new Rect(0f, 0f, 10000f, 20f);

	private GUIContent _eraseLabel = new GUIContent("Erase All", "Clear the contents of the console.");

	private GUIContent _hideDupesLabel = new GUIContent("Hide Duplicates", "Hide repeated messages.");

	private string _command = "";

	private Process consoleProcess;

	private StreamWriter logWriter;

	private Dictionary<string, string> serverUrls;

	private string currentServer;

	public void OnEnable()
	{
		Application.RegisterLogCallback(HandleLog);
	}

	public void OnDisable()
	{
		Application.RegisterLogCallback(null);
	}

	public void Start()
	{
		base.enabled = BWEnvConfig.Flags.ContainsKey("DEBUG_CONSOLE") && BWEnvConfig.Flags["DEBUG_CONSOLE"];
	}

	public void Update()
	{
		if (Input.GetKeyDown(toggleKey))
		{
			_showConsole = !_showConsole;
		}
	}

	public void OnGUI()
	{
		if (_showConsole)
		{
			GUI.backgroundColor = Color.cyan;
			_windowRect = GUILayout.Window(123456, _windowRect, ConsoleWindow, "Blocksworld Console (press " + toggleKey.ToString() + " to hide)");
		}
	}

	public void CommandParse(string cmd)
	{
		string[] array = cmd.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return;
		}
		switch (array[0].ToLower())
		{
		case "help":
			BWLog.Info("[Console] Available commands:");
			BWLog.Info("  help                - Display available commands");
			BWLog.Info("  reload [worlds|world|game] - Reload worlds, current world, or the entire game scene");
			BWLog.Info("  save world          - Save the current world");
			BWLog.Info("  export              - Export selected model");
			BWLog.Info("  skybox <path>       - Set custom skybox from PNG file");
			break;
		case "reload":
		{
			if (array.Length < 2)
			{
				BWLog.Error("[Console] 'reload' takes at least 1 argument.");
				break;
			}
			string text2 = array[1].ToLower();
			switch (text2)
			{
			case "worlds":
				BWUserWorldsDataManager.Instance.LoadWorlds();
				break;
			case "world":
			{
				BWLocalWorld worldWithLocalWorldID = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(WorldSession.current.worldId);
				worldWithLocalWorldID.OverwriteSource(Util.ObfuscateSourceForUser("{}", BWUser.currentUser.userID), sourceHasWinCondition: false);
				BWUserWorldsDataManager.Instance.LoadSourceForLocalWorld(worldWithLocalWorldID, null);
				break;
			}
			case "game":
				BWLog.Info("[Console] Reloading entire game...");
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				break;
			default:
				BWLog.Error("[Console] No such reload kind: " + text2);
				break;
			}
			break;
		}
		case "save":
		{
			if (array.Length < 2)
			{
				BWLog.Error("[Console] 'save' takes at least 1 argument.");
				break;
			}
			string text3 = array[1].ToLower();
			if (text3 == "world")
			{
				BWStandalone.Instance.SaveCurrentWorldSession(null);
				WorldSession.Save();
				WorldSession.FastSave();
			}
			else
			{
				BWLog.Error("[Console] No such save kind: " + text3);
			}
			break;
		}
		case "export":
			if (Blocksworld.selectedBunch == null)
			{
				BWLog.Info("[Console] Please select a model to export.");
			}
			else
			{
				Blocksworld.SaveMeshToFile(Blocksworld.selectedBunch);
			}
			break;
		case "skybox":
		{
			if (array.Length < 2)
			{
				BWLog.Error("[Console] 'skybox' requires a file path argument.");
				break;
			}
			string text = array[1].Trim('"').Trim();
			BWLog.Info("[Console] Processed path: " + text);
			if (!text.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
			{
				BWLog.Error("[Console] Only PNG files are supported for custom skybox.");
				break;
			}
			try
			{
				if (!File.Exists(text))
				{
					BWLog.Error("[Console] File not found: " + text);
					break;
				}
				Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipmap: false);
				byte[] data = File.ReadAllBytes(text);
				if (!texture2D.LoadImage(data))
				{
					BWLog.Error("[Console] Failed to load PNG from path: " + text);
					break;
				}
				BWLog.Info($"[Console] Original Texture Size: {texture2D.width}x{texture2D.height}");
				int num = Mathf.NextPowerOfTwo(Mathf.Max(texture2D.width, texture2D.height));
				Texture2D texture2D2 = TextureScaler.scaled(texture2D, num, num);
				BWLog.Info($"[Console] Resized Texture Size: {texture2D2.width}x{texture2D2.height}");
				Material material = new Material(Shader.Find("Skybox/Cubemap"));
				Cubemap cubemap = new Cubemap(num, TextureFormat.RGBA32, mipmap: false);
				Color[] pixels = texture2D2.GetPixels();
				cubemap.SetPixels(pixels, CubemapFace.PositiveX);
				cubemap.SetPixels(pixels, CubemapFace.NegativeX);
				cubemap.SetPixels(pixels, CubemapFace.PositiveY);
				cubemap.SetPixels(pixels, CubemapFace.NegativeY);
				cubemap.SetPixels(pixels, CubemapFace.PositiveZ);
				cubemap.SetPixels(pixels, CubemapFace.NegativeZ);
				cubemap.Apply();
				material.SetTexture("_Tex", cubemap);
				RenderSettings.skybox = material;
				BWLog.Info("[Console] Custom PNG skybox applied successfully from: " + text);
				break;
			}
			catch (Exception arg)
			{
				BWLog.Error($"[Console] Complete error details: {arg}");
				break;
			}
		}
		}
	}

	public void ConsoleWindow(int windowID)
	{
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
		string text = string.Empty;
		int num = 0;
		for (int num2 = _logs.Count - 1; num2 >= 0; num2--)
		{
			Log log = _logs[num2];
			if (!_hideDupes || !(log.message == text))
			{
				text = log.message;
				GUI.contentColor = logTypeColors[log.type];
				try
				{
					GUILayout.Label(text);
				}
				catch
				{
					GUILayout.Label("Unable to display log message with stack trace:");
				}
				GUILayout.Label(log.stackTrace);
				num++;
				if (num > displayCapacity)
				{
					break;
				}
			}
		}
		GUILayout.EndScrollView();
		GUI.contentColor = Color.white;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(_eraseLabel))
		{
			_logs.Clear();
		}
		_hideDupes = GUILayout.Toggle(_hideDupes, _hideDupesLabel, GUILayout.ExpandWidth(expand: false));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_command = GUILayout.TextField(_command);
		if (GUILayout.Button("Submit"))
		{
			BWLog.Info("[Console] " + _command);
			CommandParse(_command);
			_command = "";
		}
		GUILayout.EndHorizontal();
		GUI.DragWindow(_titleBarRect);
	}

	public void HandleLog(string message, string stackTrace, LogType type)
	{
		if (!ignoreWarnings || type != LogType.Warning)
		{
			if (_logs.Count >= messageCapacity)
			{
				_logs.RemoveAt(0);
			}
			_logs.Add(new Log
			{
				message = message,
				stackTrace = stackTrace,
				type = type
			});
		}
	}

	static BWConsole()
	{
		logTypeColors = new Dictionary<LogType, Color>
		{
			{
				LogType.Assert,
				Color.white
			},
			{
				LogType.Error,
				Color.red
			},
			{
				LogType.Exception,
				Color.red
			},
			{
				LogType.Log,
				Color.yellow
			},
			{
				LogType.Warning,
				Color.white
			}
		};
	}
}
